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
}