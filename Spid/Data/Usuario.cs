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

    // Perfil: "Admin", "Gestor Principal" ou "Gestor Secundário"
    public string Perfil { get; set; } = null!;

    // Hash da senha (gerado via PasswordHasher<Usuario>)
    public string SenhaHash { get; set; } = null!;

    public string? Cpf { get; set; }
    public bool Ativo { get; set; } = true;

    // Setor ao qual o usuário pertence (gestores veem viagens do seu setor)
    public int? SetorId { get; set; }
    public Setor? Setor { get; set; }
}