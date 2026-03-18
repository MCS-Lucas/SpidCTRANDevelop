namespace Spid.Services;

using Microsoft.AspNetCore.Identity;
using Spid.Data;

public class UserSession
{
    private static readonly PasswordHasher<Usuario> _hasher = new();

    public Usuario? UsuarioLogado { get; private set; }

    public bool IsAuthenticated => UsuarioLogado is not null;

    /// <summary>
    /// Tenta autenticar o usuário verificando a senha informada contra o hash armazenado.
    /// Retorna true se o login for bem-sucedido.
    /// </summary>
    public bool Login(Usuario usuario, string senha)
    {
        var result = _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);

        if (result == PasswordVerificationResult.Failed)
            return false;

        UsuarioLogado = usuario;
        return true;
    }

    public void Logout()
    {
        UsuarioLogado = null;
    }

    /// <summary>
    /// Gera o hash de uma senha em texto plano (usar ao cadastrar/alterar senha).
    /// </summary>
    public static string HashSenha(Usuario usuario, string senhaTextoPlano)
    {
        return _hasher.HashPassword(usuario, senhaTextoPlano);
    }

    /// <summary>
    /// Restaura a sessão lendo os claims injetados pelo cookie auth no HttpContext
    /// </summary>
    public async Task TryRestoreFromClaimsAsync(System.Security.Claims.ClaimsPrincipal user, AppDbContext db)
    {
        if (UsuarioLogado is not null) return; // Sessão já ativa na memória
        
        var userIdClaim = user.FindFirst("UserId")?.Value;
        if (int.TryParse(userIdClaim, out int userId))
        {
            var usuario = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                db.Usuarios, u => u.Id == userId && u.Ativo);

            if (usuario is not null)
            {
                UsuarioLogado = usuario;
            }
        }
    }
}