using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using Spid.Components;
using Spid.Data; // aqui vai ficar o AppDbContext
using Spid.Services;


var builder = WebApplication.CreateBuilder(args);

// ✅ Habilita Static Web Assets também no ambiente "Docker" (dev)
if (builder.Environment.IsEnvironment("Docker"))
{
    StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
}

// DbContext com SQL Server (instância scoped — usada por navmenu, analisar-viagens, etc.)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Factory — usada pelo Dashboard para criar contextos curtos e isolados, evitando concorrência
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<ImportacaoService>();

// Ativa acesso ao HttpContext durante a renderização (necessário para ler o cookie)
builder.Services.AddHttpContextAccessor();

// Configuração do Sistema de Autenticação por Cookie do ASP.NET Core
builder.Services.AddAuthentication("SpidCookie")
    .AddCookie("SpidCookie", options =>
    {
        options.Cookie.Name = "SpidAuthToken";
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;

        // Segurança do Cookie
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

var app = builder.Build();

// ── Seed de dados (cria usuários e recursos na primeira execução) ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed de recursos e permissões
    if (!db.Recursos.Any())
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
        db.SaveChanges();

        db.PerfisRecurso.AddRange(
            new PerfilRecurso { Perfil = "Admin", RecursoId = importar.Id },
            new PerfilRecurso { Perfil = "Gestor Principal", RecursoId = analisar.Id },
            new PerfilRecurso { Perfil = "Gestor Secundário", RecursoId = analisar.Id }
        );
        db.SaveChanges();
    }

    // Migração de dados: renomear "Gestor" → "Gestor Principal" e adicionar permissão "Gestor Secundário"
    var gestoresAntigos = db.Usuarios.Where(u => u.Perfil == "Gestor").ToList();
    if (gestoresAntigos.Any())
    {
        foreach (var u in gestoresAntigos) u.Perfil = "Gestor Principal";
        db.SaveChanges();
    }

    var perfisAntigos = db.PerfisRecurso.Where(pr => pr.Perfil == "Gestor").ToList();
    if (perfisAntigos.Any())
    {
        foreach (var pr in perfisAntigos) pr.Perfil = "Gestor Principal";
        db.SaveChanges();
    }

    // Adicionar permissão AnalisarViagens para Gestor Secundário, se ainda não existir
    var analisarRecurso = db.Recursos.FirstOrDefault(r => r.Chave == "AnalisarViagens");
    if (analisarRecurso is not null)
    {
        var jaTemSecundario = db.PerfisRecurso
            .Any(pr => pr.Perfil == "Gestor Secundário" && pr.RecursoId == analisarRecurso.Id);
        if (!jaTemSecundario)
        {
            db.PerfisRecurso.Add(new PerfilRecurso { Perfil = "Gestor Secundário", RecursoId = analisarRecurso.Id });
            db.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Endpoint HTTP para logout — SignOutAsync requer HttpContext real (não funciona dentro do WebSocket Blazor)
app.MapGet("/logout-handler", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync("SpidCookie");
    return Results.Redirect("/login");
});

// Endpoint HTTP para login — SignInAsync (gravar cookie) também requer HttpContext real
// O form do Login.razor faz POST aqui; se bem-sucedido emite o cookie e redireciona
app.MapPost("/do-login", async (
    HttpContext ctx,
    AppDbContext db) =>
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

    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Spid.Data.Usuario>();
    var result = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
    if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
        return Results.Redirect("/login?erro=senha");

    var claims = new List<System.Security.Claims.Claim>
    {
        new("UserId", usuario.Id.ToString()),
        new(System.Security.Claims.ClaimTypes.Name, usuario.Nome)
    };
    var identity = new System.Security.Claims.ClaimsIdentity(claims, "SpidCookie");
    var principal = new System.Security.Claims.ClaimsPrincipal(identity);

    await ctx.SignInAsync("SpidCookie", principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
    });

    // Se for o primeiro acesso, redireciona para a tela de recuperar/redefinir senha
    if (usuario.ContadorAcessos == 0)
    {
        return Results.Redirect("/primeiro-acesso");
    }

    return Results.Redirect("/");
}).DisableAntiforgery();

app.Run();