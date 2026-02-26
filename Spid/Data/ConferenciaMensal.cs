namespace Spid.Data;

/// <summary>
/// Registro de confirmação mensal de um gestor.
/// Indica que ele verificou todas as viagens do seu setor naquele mês/ano.
/// </summary>
public class ConferenciaMensal
{
    public int Id { get; set; }

    public int Ano { get; set; }
    public int Mes { get; set; }

    // Setor cujas viagens foram conferidas
    public int SetorId { get; set; }
    public Setor Setor { get; set; } = null!;

    // Gestor que confirmou
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTime DataConfirmacao { get; set; }
}
