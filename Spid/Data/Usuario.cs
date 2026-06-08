namespace Spid.Data;

public class Usuario
{
    public int Id { get; set; }

    // Nome completo do usuário (gestor ou admin)
    public string Nome { get; set; } = null!;

    // E-mail pode continuar existindo, mas não será a credencial principal de login
    public string Email { get; set; } = null!;

    // Ponto do usuário, usado no login
    public string Ponto { get; set; } = null!;

    // Perfil: "Admin", "Gestor Titular", "Gestor Substituto", "Gestor Central Padrão" ou "Gestor Central Ateste"
    public string Perfil { get; set; } = null!;

    // Hash da senha (gerado via PasswordHasher<Usuario>)
    public string SenhaHash { get; set; } = null!;

    public string? Cpf { get; set; }
    public bool Ativo { get; set; } = true;

    // Contador de acessos: 0 = nunca acessou (primeiro acesso pendente), 1 = já completou primeiro acesso
    public int ContadorAcessos { get; set; } = 0;

    // Centro de Custo ao qual o usuário pertence (gestores veem viagens do seu centro de custo)
    public int? CentroCustoId { get; set; }
    public CentroCusto? CentroCusto { get; set; }
}
