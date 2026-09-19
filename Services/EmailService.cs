using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using PiscinerosAPI.Models;
using System.Net.Http;

namespace PiscinerosAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task EnviarReciboAsync(Visita visita)
        {
            if (visita.Cliente == null)
            {
                throw new ArgumentException("La visita debe tener un cliente asociado para enviar el recibo.");
            }

            var correoDestino = visita.Cliente.Correo;
            if (string.IsNullOrEmpty(correoDestino))
            {
                // Si el cliente no tiene correo, no enviamos nada.
                return;
            }

            var emailMessage = new MimeMessage();

            // Configurar remitente (desde appsettings o env vars)
            var remitenteNombre = _configuration["Smtp:SenderName"] ?? "Piscineros";
            var remitenteEmail = _configuration["Smtp:SenderEmail"] ?? "no-reply@piscineros.cl";
            emailMessage.From.Add(new MailboxAddress(remitenteNombre, remitenteEmail));
            
            // Configurar destinatario
            emailMessage.To.Add(new MailboxAddress(visita.Cliente.Nombre, correoDestino));
            
            emailMessage.Subject = $"Recibo de Visita - {visita.FechaVisita:dd/MM/yyyy} - Piscineros";

            var builder = new BodyBuilder();

            // Aquí podemos definir la URL del logo en el futuro
            string logoUrl = _configuration["Piscineros:LogoUrl"] ?? "";
            string logoHtml = !string.IsNullOrEmpty(logoUrl) ? $"<img src=\"{logoUrl}\" alt=\"Piscineros Logo\" style=\"max-width: 200px; margin-bottom: 20px;\" /><br/>" : "";

            // Cuerpo del correo en HTML (Sin el precio/monto según indicaciones)
            builder.HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    {logoHtml}
                    <h2>Recibo de Visita Finalizada</h2>
                    <p>Hola <strong>{visita.Cliente.Nombre}</strong>,</p>
                    <p>Te informamos que hemos finalizado exitosamente el trabajo en tu piscina.</p>
                    <hr/>
                    <h3>Detalles del Trabajo</h3>
                    <ul>
                        <li><strong>Fecha:</strong> {visita.FechaVisita:dd/MM/yyyy HH:mm}</li>
                        <li><strong>Dirección:</strong> {visita.Cliente.Direccion}, {visita.Cliente.Comuna}</li>
                        <li><strong>Observaciones:</strong> {(string.IsNullOrWhiteSpace(visita.Observaciones) ? "Sin Observaciones" : visita.Observaciones)}</li>
                    </ul>
                    <hr/>
                    <p>Adjunto a este correo encontrarás una fotografía del estado de tu piscina al finalizar el trabajo.</p>
                    <p>Gracias por preferir a Piscineros.</p>
                </div>
            ";

            // Descargar y adjuntar la foto si existe
            if (!string.IsNullOrEmpty(visita.FotoUrl))
            {
                try
                {
                    var imageBytes = await _httpClient.GetByteArrayAsync(visita.FotoUrl);
                    builder.Attachments.Add("Piscina_Finalizada.jpg", imageBytes, ContentType.Parse("image/jpeg"));
                }
                catch (Exception ex)
                {
                    // Si falla la descarga de la imagen, registramos el error pero podríamos seguir enviando el correo sin adjunto (o lanzar excepción)
                    Console.WriteLine($"Error al descargar la foto para adjuntar: {ex.Message}");
                }
            }

            emailMessage.Body = builder.ToMessageBody();

            // Enviar el correo usando SMTP
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPortString = _configuration["Smtp:Port"];
            var smtpUser = _configuration["Smtp:User"];
            var smtpPass = _configuration["Smtp:Password"];

            // Si no hay configuración SMTP, simplemente no hacemos el envío real (útil para dev local si no han seteado variables)
            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                Console.WriteLine("Advertencia: No hay configuración SMTP. El correo no fue enviado realmente.");
                return;
            }

            int smtpPort = int.TryParse(smtpPortString, out var port) ? port : 587;

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}
