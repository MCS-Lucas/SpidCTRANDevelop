using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Spid.Components;
using Spid.Data;
using Spid.Services;


var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();
var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
var useHttpsRedirection = builder.Configuration.GetValue("IIS:UseHttpsRedirection", false);
var httpsPort = builder.Configuration.GetValue<int?>("IIS:HttpsPort");

// Static Web Assets no ambiente Docker
if (builder.Environment.IsEnvironment("Docker"))
{
    StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
}

var connectionString = Environment.GetEnvironmentVariable("SPID_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A connection string 'DefaultConnection' não foi configurada. Defina via variável de ambiente SPID_CONNECTION_STRING ou em appsettings.");

// DbContext principal
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Factory para contextos curtos
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped);

// Componentes Razor / Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ⭐ CONFIGURAÇÃO DE SIGNALR - CRÍTICO PARA PRODUÇÃO
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.MaximumParallelInvocationsPerClient = 1;
});

builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<ImportacaoService>();

var sessionTimeoutMinutes = builder.Configuration.GetValue<int>("SessionTimeoutMinutes", 45);

builder.Services.AddAuthentication("SpidCookie")
    .AddCookie("SpidCookie", options =>
    {
        options.Cookie.Name = "SpidAuthToken";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(sessionTimeoutMinutes);
        options.SlidingExpiration = false; // Rigid expiration so we can manage manual extension

        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = useHttpsRedirection
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Persistir chaves do Cookie no HD para não invalidar a sessão em caso de Load Balancer / Web Garden
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .SetApplicationName("Spid")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// ⭐ CORS - Restritivo em produção, aberto apenas em desenvolvimento
var allowedOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("SpidCors", policy =>
    {
        if (isDevelopment || allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithExposedHeaders("Content-Disposition");
        }
    });
});

// Health check para monitoramento
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "sqlserver");

// Forwarded headers para Nginx/Reverse Proxies (opcional)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;

    options.RequireHeaderSymmetry = false;
    options.ForwardLimit = null;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// HTTPS opcional por configuração
if (useHttpsRedirection && httpsPort.HasValue)
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = httpsPort.Value;
    });
}

var app = builder.Build();

// Força a formatação de números, moedas e datas para o padrão brasileiro
var cultureInfo = new System.Globalization.CultureInfo("pt-BR");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(cultureInfo),
    SupportedCultures = new[] { cultureInfo },
    SupportedUICultures = new[] { cultureInfo }
});

// Sobe o pipeline primeiro
app.UseForwardedHeaders();

app.Use(async (ctx, next) =>
{
    var forwardedProto = ctx.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
    if (!string.IsNullOrWhiteSpace(forwardedProto))
    {
        ctx.Request.Scheme = forwardedProto;
    }
    else
    {
        var httpsServerVariables = new[]
        {
            ctx.Request.Headers["X-Forwarded-Ssl"].FirstOrDefault(),
            ctx.Request.Headers["Front-End-Https"].FirstOrDefault(),
            ctx.Request.Headers["X-ARR-SSL"].FirstOrDefault()
        };

        if (httpsServerVariables.Any(value =>
                string.Equals(value, "on", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "https", StringComparison.OrdinalIgnoreCase)))
        {
            ctx.Request.Scheme = "https";
        }
    }

    var forwardedHost = ctx.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
    if (!string.IsNullOrWhiteSpace(forwardedHost))
    {
        ctx.Request.Host = HostString.FromUriComponent(forwardedHost);
    }

    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // Nota: UseHsts removido — não usar HSTS sem HTTPS configurado no IIS,
    // pois força o navegador a rejeitar requisições HTTP por até 1 ano.
}

if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// ⭐ WEBSOCKET - NECESSÁRIO PARA BLAZOR SERVER
app.UseWebSockets();

app.UseRouting();

// ⭐ CORS - IMPORTANTE para APIs e requisições cross-origin
app.UseCors("SpidCors");

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/logout-handler", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync("SpidCookie");
    return Results.Redirect("/login");
});

app.MapPost("/do-login", async (HttpContext ctx, AppDbContext db) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var ponto = form["ponto"].ToString().Trim();
    var senha = form["senha"].ToString();

    if (string.IsNullOrWhiteSpace(ponto) || string.IsNullOrWhiteSpace(senha))
        return Results.Redirect("/login?erro=campos");

    var usuario = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Ponto == ponto && u.Ativo);

    if (usuario is null)
        return Results.Redirect("/login?erro=usuario");

    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Usuario>();
    var result = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);

    if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
        return Results.Redirect("/login?erro=senha");

    var sessionTimeoutMinutes = ctx.RequestServices.GetRequiredService<IConfiguration>().GetValue<int>("SessionTimeoutMinutes", 45);
    var expiresUtc = DateTimeOffset.UtcNow.AddMinutes(sessionTimeoutMinutes);

    var claims = new List<System.Security.Claims.Claim>
    {
        new("UserId", usuario.Id.ToString()),
        new(System.Security.Claims.ClaimTypes.Name, usuario.Nome),
        new("SessionExpiresUtc", expiresUtc.ToString("o"))
    };

    var identity = new System.Security.Claims.ClaimsIdentity(claims, "SpidCookie");
    var principal = new System.Security.Claims.ClaimsPrincipal(identity);

    await ctx.SignInAsync("SpidCookie", principal, new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = expiresUtc
    });

    if (usuario.ContadorAcessos == 0)
        return Results.Redirect("/primeiro-acesso");

    return Results.Redirect("/");
}).DisableAntiforgery();

app.MapPost("/extend-session", async (HttpContext ctx) =>
{
    if (ctx.User.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();

    var sessionTimeoutMinutes = ctx.RequestServices.GetRequiredService<IConfiguration>().GetValue<int>("SessionTimeoutMinutes", 45);

    var newExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(sessionTimeoutMinutes);

    var identity = new System.Security.Claims.ClaimsIdentity(ctx.User.Identity);
    var existingClaim = identity.FindFirst("SessionExpiresUtc");
    if (existingClaim != null) identity.RemoveClaim(existingClaim);
    identity.AddClaim(new System.Security.Claims.Claim("SessionExpiresUtc", newExpiresUtc.ToString("o")));

    await ctx.SignInAsync("SpidCookie", new System.Security.Claims.ClaimsPrincipal(identity), new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = newExpiresUtc
    });

    return Results.Ok(new { ExpiresUtc = newExpiresUtc });
}).DisableAntiforgery();

app.MapHealthChecks("/health");

var runDatabaseSetupOnBoot = builder.Configuration.GetValue("StartupTasks:RunDatabaseSetupOnBoot", false);

if (runDatabaseSetupOnBoot)
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        _ = Task.Run(async () =>
        {
            var logger = app.Services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("StartupDatabaseTasks");

            try
            {
                await RunDatabaseSetupSafelyAsync(app.Services, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha não tratada nas tarefas de banco executadas após o startup.");
            }
        });
    });
}

await app.RunAsync();

static async Task RunDatabaseSetupSafelyAsync(IServiceProvider rootServices, ILogger logger)
{
    await using var scope = rootServices.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Iniciando migration/seed pós-startup do Spid.");

        db.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));

        await db.Database.MigrateAsync();

        if (!await db.Recursos.AnyAsync())
        {
            var importar = new Recurso
            {
                Chave = "ImportarViagens",
                Nome = "Importar Viagens",
                Descricao = "Permite importar planilhas Excel com dados de viagens"
            };

            var analisar = new Recurso
            {
                Chave = "AnalisarViagens",
                Nome = "Analisar Viagens",
                Descricao = "Permite conferir e aprovar/contestar viagens"
            };

            db.Recursos.AddRange(importar, analisar);
            await db.SaveChangesAsync();

            db.PerfisRecurso.AddRange(
                new PerfilRecurso { Perfil = "Admin", RecursoId = importar.Id },
                new PerfilRecurso { Perfil = "Gestor Principal", RecursoId = analisar.Id },
                new PerfilRecurso { Perfil = "Gestor Secundário", RecursoId = analisar.Id }
            );

            await db.SaveChangesAsync();
        }

        var gestoresAntigos = await db.Usuarios
            .Where(u => u.Perfil == "Gestor")
            .ToListAsync();

        if (gestoresAntigos.Count > 0)
        {
            foreach (var u in gestoresAntigos)
                u.Perfil = "Gestor Principal";

            await db.SaveChangesAsync();
        }

        var perfisAntigos = await db.PerfisRecurso
            .Where(pr => pr.Perfil == "Gestor")
            .ToListAsync();

        if (perfisAntigos.Count > 0)
        {
            foreach (var pr in perfisAntigos)
                pr.Perfil = "Gestor Principal";

            await db.SaveChangesAsync();
        }

        var analisarRecurso = await db.Recursos
            .FirstOrDefaultAsync(r => r.Chave == "AnalisarViagens");

        if (analisarRecurso is not null)
        {
            var jaTemSecundario = await db.PerfisRecurso.AnyAsync(pr =>
                pr.Perfil == "Gestor Secundário" &&
                pr.RecursoId == analisarRecurso.Id);

            if (!jaTemSecundario)
            {
                db.PerfisRecurso.Add(new PerfilRecurso
                {
                    Perfil = "Gestor Secundário",
                    RecursoId = analisarRecurso.Id
                });

                await db.SaveChangesAsync();
            }
        }

        logger.LogInformation("Migration/seed pós-startup finalizado com sucesso.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Falha ao executar migration/seed pós-startup. A aplicação continuará ativa.");
    }
    }
}
