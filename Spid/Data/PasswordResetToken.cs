namespace Spid.Data;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public string TokenHash { get; set; } = null!;
    public DateTime ExpiraEmUtc { get; set; }
    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UtilizadoEmUtc { get; set; }
    public bool Utilizado { get; set; }

    public bool EstaValido => !Utilizado && ExpiraEmUtc > DateTime.UtcNow;
}
