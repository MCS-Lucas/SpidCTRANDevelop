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

        // SQL Server não permite múltiplos caminhos de cascade delete.
        // Caminho 1: Setores → Colaboradores (CASCADE) → Viagens (CASCADE)
        // Caminho 2: Setores → Viagens (direto - deve ser RESTRICT)
        modelBuilder.Entity<Viagem>()
            .HasOne(v => v.Setor)
            .WithMany(s => s.Viagens)
            .HasForeignKey(v => v.SetorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Idem: ConferenciaMensal referencia Setor e Usuario (que também referencia Setor)
        modelBuilder.Entity<ConferenciaMensal>()
            .HasOne(c => c.Setor)
            .WithMany()
            .HasForeignKey(c => c.SetorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConferenciaMensal>()
            .HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Precisão explícita para campos monetários (evita truncamento no SQL Server)
        modelBuilder.Entity<Viagem>()
            .Property(v => v.ValorCotado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Viagem>()
            .Property(v => v.ValorFinal)
            .HasPrecision(18, 2);

        // Precisão time(0) para horários — armazena apenas HH:MM:SS, sem frações
        modelBuilder.Entity<Viagem>()
            .Property(v => v.HoraInicio)
            .HasColumnType("time(0)");

        modelBuilder.Entity<Viagem>()
            .Property(v => v.HoraFim)
            .HasColumnType("time(0)");

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