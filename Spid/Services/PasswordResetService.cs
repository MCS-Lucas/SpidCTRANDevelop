using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spid.Data;

namespace Spid.Services;

public class PasswordResetService
{
    private readonly AppDbContext _db;
    private readonly ResetSenhaOptions _options;

    public PasswordResetService(AppDbContext db, IOptions<ResetSenhaOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<string> CriarTokenAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        var tokenPlano = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", string.Empty);

        var token = new PasswordResetToken
        {
            UsuarioId = usuario.Id,
            TokenHash = GerarHash(tokenPlano),
            CriadoEmUtc = DateTime.UtcNow,
            ExpiraEmUtc = DateTime.UtcNow.AddMinutes(_options.ExpiracaoMinutos),
            Utilizado = false
        };

        _db.PasswordResetTokens.Add(token);
        await _db.SaveChangesAsync(cancellationToken);

        return tokenPlano;
    }

    public async Task<Usuario?> ObterUsuarioPorTokenValidoAsync(string tokenPlano, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenPlano))
            return null;

        var tokenHash = GerarHash(tokenPlano);

        var registro = await _db.PasswordResetTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (registro is null || registro.Utilizado || registro.ExpiraEmUtc <= DateTime.UtcNow)
            return null;

        return registro.Usuario;
    }

    public async Task<bool> RedefinirSenhaAsync(string tokenPlano, string novaSenha, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenPlano))
            return false;

        var tokenHash = GerarHash(tokenPlano);

        var registro = await _db.PasswordResetTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (registro is null || registro.Utilizado || registro.ExpiraEmUtc <= DateTime.UtcNow)
            return false;

        registro.Usuario.SenhaHash = UserSession.HashSenha(registro.Usuario, novaSenha);
        registro.Utilizado = true;
        registro.UtilizadoEmUtc = DateTime.UtcNow;

        var outrosTokens = await _db.PasswordResetTokens
            .Where(t => t.UsuarioId == registro.UsuarioId && !t.Utilizado && t.Id != registro.Id)
            .ToListAsync(cancellationToken);

        foreach (var item in outrosTokens)
        {
            item.Utilizado = true;
            item.UtilizadoEmUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string GerarHash(string valor)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(valor));
        return Convert.ToHexString(bytes);
    }
}
