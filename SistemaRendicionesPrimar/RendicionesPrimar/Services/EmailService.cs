using MailKit.Net.Smtp;
using MimeKit;

namespace RendicionesPrimar.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        
        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }
        
        public async Task EnviarNotificacionAsync(string destinatario, string asunto, string mensaje)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Sistema Rendiciones Primar", _config["Email:From"]));
                email.To.Add(new MailboxAddress("", destinatario));
                email.Subject = asunto;
                
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='background-color: #4a6fa5; color: white; padding: 20px; text-align: center;'>
                            <h2>Sistema de Rendiciones Primar</h2>
                        </div>
                        <div style='padding: 20px; background-color: #f9f9f9;'>
                            <p>{mensaje}</p>
                            <p>Ingresa al sistema para ver más detalles.</p>
                            <hr>
                            <small style='color: #666;'>Este es un mensaje automático del Sistema de Rendiciones Primar.</small>
                        </div>
                    </div>";
                
                email.Body = bodyBuilder.ToMessageBody();
                
                // Solo intentar enviar si está configurado
                var host = _config["Email:Host"];
                if (!string.IsNullOrEmpty(host) && host != "smtp.gmail.com")
                {
                    using var smtp = new SmtpClient();
                    await smtp.ConnectAsync(host, int.Parse(_config["Email:Port"] ?? "587"), false);
                    await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                    _logger.LogInformation("Email enviado a {destinatario}: {asunto}", destinatario, asunto);
                }
                else
                {
                    // En desarrollo, solo logear el email
                    _logger.LogInformation("EMAIL SIMULADO - Para: {destinatario}, Asunto: {asunto}, Mensaje: {mensaje}", destinatario, asunto, mensaje);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {destinatario}: {message}", destinatario, ex.Message);
            }
        }
    }
}