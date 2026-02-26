namespace Spid.Data;

public class ParceiroViagem
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!; // Uber, Wappa...

    public ICollection<Viagem> Viagens { get; set; } = new List<Viagem>();
}