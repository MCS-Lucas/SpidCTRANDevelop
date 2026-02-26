namespace Spid.Data;

public class Colaborador
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Cpf { get; set; } = null!;

    public int SetorId { get; set; }
    public Setor Setor { get; set; } = null!;

    public ICollection<Viagem> Viagens { get; set; } = new List<Viagem>();
}