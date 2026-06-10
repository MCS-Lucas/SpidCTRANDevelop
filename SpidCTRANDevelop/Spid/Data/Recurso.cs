namespace Spid.Data;

public class Recurso
{
    public int Id { get; set; }

    // Identificador único usado no código (ex: "ImportarViagens")
    public string Chave { get; set; } = null!;

    // Nome amigável (ex: "Importar Viagens")
    public string Nome { get; set; } = null!;

    // Descrição da funcionalidade
    public string? Descricao { get; set; }

    public ICollection<PerfilRecurso> PerfisRecurso { get; set; } = new List<PerfilRecurso>();
}
