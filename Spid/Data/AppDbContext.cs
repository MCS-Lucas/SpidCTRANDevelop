using Microsoft.EntityFrameworkCore;

namespace Spid.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Setor> Setores => Set<Setor>();
    public DbSet<Colaborador> Colaboradores => Set<Colaborador>();
    public DbSet<ParceiroViagem> Parceiros => Set<ParceiroViagem>();
    public DbSet<Viagem> Viagens => Set<Viagem>();
    public DbSet<Recurso> Recursos => Set<Recurso>();
    public DbSet<PerfilRecurso> PerfisRecurso => Set<PerfilRecurso>();
    public DbSet<ConferenciaMensal> ConferenciasMensais => Set<ConferenciaMensal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Índice único para o Ponto: usado como login
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Ponto)
            .IsUnique();

        // Relacionamento: 1 Setor -> N Usuários (gestores)
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Setor)
            .WithMany(s => s.Usuarios)
            .HasForeignKey(u => u.SetorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Índice único no IdViagemParceiro para deduplicação na importação
        modelBuilder.Entity<Viagem>()
            .HasIndex(v => v.IdViagemParceiro)
            .IsUnique();

        // Índice único na Chave do recurso
        modelBuilder.Entity<Recurso>()
            .HasIndex(r => r.Chave)
            .IsUnique();

        // Índice composto único: um perfil só pode ter cada recurso uma vez
        modelBuilder.Entity<PerfilRecurso>()
            .HasIndex(pr => new { pr.Perfil, pr.RecursoId })
            .IsUnique();

        // Índice único: só uma confirmação por setor/mês/ano
        modelBuilder.Entity<ConferenciaMensal>()
            .HasIndex(c => new { c.SetorId, c.Ano, c.Mes })
            .IsUnique();
    }
}