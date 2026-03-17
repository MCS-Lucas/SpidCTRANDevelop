using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Spid.Services;

public class EmailService
{
    private readonly EmailOptions _options;

    public EmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task EnviarRecuperacaoSenhaAsync(string destino, string nomeUsuario, string linkRedefinicao)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) ||
            string.IsNullOrWhiteSpace(_options.FromEmail) ||
            string.IsNullOrWhiteSpace(_options.User))
        {
            throw new InvalidOperationException("As configurações de e-mail não foram preenchidas.");
        }

        using var mensagem = new MailMessage();
        mensagem.From = new MailAddress(_options.FromEmail, _options.FromName);
        mensagem.To.Add(destino);
        mensagem.Subject = "Recuperação de senha - SPID";
        mensagem.IsBodyHtml = true;
        mensagem.Body = $@"
<html>
  <body style='font-family:Arial,Helvetica,sans-serif;background:#f4f7fb;color:#1f2937;padding:24px;'>
    <div style='max-width:560px;margin:0 auto;background:#ffffff;border-radius:16px;padding:32px;box-shadow:0 10px 30px rgba(0,0,0,.08);'>
      <div style='text-align:center;margin-bottom:24px;'>
        <img src='cid:logo-spid' alt='SPID' style='max-width:180px;width:100%;' />
      </div>
      <h2 style='margin:0 0 12px 0;color:#1a1a2e;'>Recuperação de senha</h2>
      <p>Olá, {WebUtility.HtmlEncode(nomeUsuario)}.</p>
      <p>Recebemos uma solicitação para redefinir a sua senha no sistema SPID.</p>
      <p>Para continuar, clique no botão abaixo:</p>
      <p style='margin:24px 0;'>
        <a href='{WebUtility.HtmlEncode(linkRedefinicao)}' style='display:inline-block;background:linear-gradient(135deg,#0f3460,#1a1a2e);color:#ffffff;text-decoration:none;padding:14px 22px;border-radius:10px;font-weight:600;'>Alterar senha</a>
      </p>
      <p>Este link é único, associado exclusivamente ao seu usuário e expira em breve.</p>
      <p>Se você não solicitou esta alteração, ignore este e-mail.</p>
    </div>
  </body>
</html>";

        var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "spid_web.png");
        if (File.Exists(logoPath))
        {
            var htmlView = AlternateView.CreateAlternateViewFromString(mensagem.Body, null, "text/html");
            var logo = new LinkedResource(logoPath) { ContentId = "logo-spid" };
            htmlView.LinkedResources.Add(logo);
            mensagem.AlternateViews.Add(htmlView);
        }

        using var cliente = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.User, _options.Password)
        };

        await cliente.SendMailAsync(mensagem);
    }
}
