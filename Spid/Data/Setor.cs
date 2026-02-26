namespace Spid.Data;

public class Setor
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!; // ex.: "CTRAN/SEMOV"

    public ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
    public ICollection<Viagem> Viagens { get; set; } = new List<Viagem>();
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}