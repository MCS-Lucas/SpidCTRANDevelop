using Microsoft.EntityFrameworkCore;
using Spid.Components;
using Spid.Data; // aqui vai ficar o AppDbContext
using Spid.Services;



var builder = WebApplication.CreateBuilder(args);

// DbContext com SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<ImportacaoService>();

var app = builder.Build();

// ── Seed de dados (cria usuários e recursos na primeira execução) ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Seed de usuários
    if (!db.Usuarios.Any())
    {
        var admin = new Usuario
        {
            Nome = "Admin Teste",
            Email = "admin@spid.com",
            Ponto = "0001",
            Perfil = "Admin",
            Ativo = true
        };
        admin.SenhaHash = UserSession.HashSenha(admin, "admin123");

        var gestor = new Usuario
        {
            Nome = "Gestor Teste",
            Email = "gestor@spid.com",
            Ponto = "6845",
            Perfil = "Gestor Principal",
            Ativo = true
        };
        gestor.SenhaHash = UserSession.HashSenha(gestor, "gestor123");

        db.Usuarios.AddRange(admin, gestor);
        db.SaveChanges();
    }

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

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();