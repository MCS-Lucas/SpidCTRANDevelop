namespace Spid.Data;

public class PerfilRecurso
{
    public int Id { get; set; }

    // Perfil: "Admin", "Gestor Primário", "Gestor Secundário", "Gestor Central Padrão" ou "Gestor Central Ateste"
    public string Perfil { get; set; } = null!;

    public int RecursoId { get; set; }
    public Recurso Recurso { get; set; } = null!;
}
