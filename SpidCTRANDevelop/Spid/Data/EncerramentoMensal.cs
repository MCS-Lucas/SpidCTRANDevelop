namespace Spid.Data;

/// <summary>
/// Controle de encerramento/liberação mensal.
/// Quando encerrado, gestores comuns (Primário/Secundário) não podem atestar viagens daquele mês.
/// Admins e Gestores Centrais podem operar normalmente mesmo com o mês encerrado.
/// </summary>
public class EncerramentoMensal
{
    public int Id { get; set; }

    public int Ano { get; set; }
    public int Mes { get; set; }

    /// <summary>Se o mês está atualmente encerrado (bloqueado).</summary>
    public bool Encerrado { get; set; }

    // Último encerramento
    public int? EncerradoPorId { get; set; }
    public Usuario? EncerradoPor { get; set; }
    public DateTime? DataEncerramento { get; set; }

    // Última liberação
    public int? LiberadoPorId { get; set; }
    public Usuario? LiberadoPor { get; set; }
    public DateTime? DataLiberacao { get; set; }
}
