namespace Spid.Data;

public class ImportacaoLog
{
    public int Id { get; set; }
    public DateTime DataImportacao { get; set; }
    
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int QuantidadeImportada { get; set; }
}
