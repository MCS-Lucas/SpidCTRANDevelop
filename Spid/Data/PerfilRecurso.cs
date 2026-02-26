namespace Spid.Data;

public class PerfilRecurso
{
    public int Id { get; set; }

    // Perfil: "Admin", "Gestor Principal" ou "Gestor Secundário"
    public string Perfil { get; set; } = null!;

    public int RecursoId { get; set; }
    public Recurso Recurso { get; set; } = null!;
}
