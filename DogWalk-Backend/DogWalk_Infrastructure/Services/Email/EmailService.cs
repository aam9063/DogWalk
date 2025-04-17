using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace DogWalk_Infrastructure.Services.Email
{
    public class EmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Envía un email
        /// </summary>
        /// <param name="to">Destinatario</param>
        /// <param name="subject">Asunto</param>
        /// <param name="body">Cuerpo del mensaje</param>
        /// <param name="isHtml">Indica si el cuerpo es HTML</param>
        /// <returns>True si el envío fue exitoso</returns>
        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            return await SendEmailAsync(new[] { to }, subject, body, isHtml);
        }

        /// <summary>
        /// Envía un email a múltiples destinatarios
        /// </summary>
        /// <param name="to">Lista de destinatarios</param>
        /// <param name="subject">Asunto</param>
        /// <param name="body">Cuerpo del mensaje</param>
        /// <param name="isHtml">Indica si el cuerpo es HTML</param>
        /// <returns>True si el envío fue exitoso</returns>
        public async Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_options.FromEmail, _options.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                foreach (var recipient in to)
                {
                    message.To.Add(recipient);
                }

                using (var client = new SmtpClient(_options.SmtpServer, _options.SmtpPort))
                {
                    client.EnableSsl = _options.EnableSsl;
                    client.Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);

                    await client.SendMailAsync(message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email a {Recipients}. Asunto: {Subject}", string.Join(", ", to), subject);
                return false;
            }
        }

        /// <summary>
        /// Envía un email con adjuntos
        /// </summary>
        /// <param name="to">Destinatario</param>
        /// <param name="subject">Asunto</param>
        /// <param name="body">Cuerpo del mensaje</param>
        /// <param name="attachments">Lista de adjuntos</param>
        /// <param name="isHtml">Indica si el cuerpo es HTML</param>
        /// <returns>True si el envío fue exitoso</returns>
        public async Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string body, IEnumerable<Attachment> attachments, bool isHtml = true)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_options.FromEmail, _options.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                foreach (var attachment in attachments)
                {
                    message.Attachments.Add(attachment);
                }

                using (var client = new SmtpClient(_options.SmtpServer, _options.SmtpPort))
                {
                    client.EnableSsl = _options.EnableSsl;
                    client.Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);

                    await client.SendMailAsync(message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email con adjuntos a {Recipient}. Asunto: {Subject}", to, subject);
                return false;
            }
        }

        /// <summary>
        /// Envía un email de confirmación de cuenta
        /// </summary>
        /// <param name="to">Email del destinatario</param>
        /// <param name="nombre">Nombre del destinatario</param>
        /// <param name="confirmationLink">Enlace de confirmación</param>
        /// <returns>True si el envío fue exitoso</returns>
        public async Task<bool> SendAccountConfirmationEmail(string to, string nombre, string confirmationLink)
        {
            string subject = "Confirma tu cuenta en DogWalk App";
            string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .button {{ background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }}
                        .footer {{ font-size: 12px; text-align: center; margin-top: 20px; color: #888; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>DogWalk App</h1>
                        </div>
                        <div class='content'>
                            <h2>¡Hola {nombre}!</h2>
                            <p>Gracias por registrarte en DogWalk App. Por favor, confirma tu dirección de correo electrónico haciendo clic en el siguiente enlace:</p>
                            <p style='text-align: center;'>
                                <a href='{confirmationLink}' class='button'>Confirmar mi cuenta</a>
                            </p>
                            <p>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                            <p>{confirmationLink}</p>
                            <p>Si no has creado esta cuenta, puedes ignorar este mensaje.</p>
                        </div>
                        <div class='footer'>
                            <p>© {DateTime.Now.Year} DogWalk App. Todos los derechos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(to, subject, body);
        }

        /// <summary>
        /// Envía un email de reserva confirmada al usuario
        /// </summary>
        /// <param name="to">Email del destinatario</param>
        /// <param name="nombreUsuario">Nombre del usuario</param>
        /// <param name="nombrePaseador">Nombre del paseador</param>
        /// <param name="nombreServicio">Nombre del servicio</param>
        /// <param name="fechaServicio">Fecha del servicio</param>
        /// <param name="precio">Precio del servicio</param>
        /// <param name="reservaId">ID de la reserva</param>
        /// <returns>True si el envío fue exitoso</returns>
        public async Task<bool> SendReservaConfirmadaEmail(string to, string nombreUsuario, string nombrePaseador, string nombreServicio, DateTime fechaServicio, decimal precio, Guid reservaId)
        {
            string subject = "Confirmación de Reserva - DogWalk App";
            string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ font-size: 12px; text-align: center; margin-top: 20px; color: #888; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>DogWalk App</h1>
                        </div>
                        <div class='content'>
                            <h2>¡Reserva Confirmada!</h2>
                            <p>Hola {nombreUsuario},</p>
                            <p>Tu reserva ha sido confirmada exitosamente. Aquí tienes los detalles:</p>
                            
                            <div class='details'>
                                <p><strong>Servicio:</strong> {nombreServicio}</p>
                                <p><strong>Paseador:</strong> {nombrePaseador}</p>
                                <p><strong>Fecha:</strong> {fechaServicio:dd/MM/yyyy HH:mm}</p>
                                <p><strong>Precio:</strong> {precio:C}</p>
                                <p><strong>Reserva ID:</strong> {reservaId}</p>
                            </div>
                            
                            <p>Si tienes alguna pregunta o necesitas hacer cambios, por favor contáctanos respondiendo a este correo o a través de la aplicación.</p>
                            <p>¡Gracias por confiar en DogWalk App!</p>
                        </div>
                        <div class='footer'>
                            <p>© {DateTime.Now.Year} DogWalk App. Todos los derechos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(to, subject, body);
        }

        /// <summary>
        /// Envía un email de factura generada
        /// </summary>
        /// <param name="to">Email del destinatario</param>
        /// <param name="nombreUsuario">Nombre del usuario</param>
        /// <param name="facturaId">ID de la factura</param>
        /// <param name="fechaFactura">Fecha de la factura</param>
        /// <param name="total">Total de la factura</param>
        /// <param name="pdfAttachment">Adjunto con la factura en PDF</param>
        /// <returns>True si el envío fue exitoso</returns>
        public async Task<bool> SendFacturaEmail(string to, string nombreUsuario, Guid facturaId, DateTime fechaFactura, decimal total, Attachment pdfAttachment)
        {
            string subject = "Factura DogWalk App";
            string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ font-size: 12px; text-align: center; margin-top: 20px; color: #888; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>DogWalk App</h1>
                        </div>
                        <div class='content'>
                            <h2>Factura</h2>
                            <p>Hola {nombreUsuario},</p>
                            <p>Adjunto encontrarás la factura correspondiente a tu compra en DogWalk App.</p>
                            
                            <div class='details'>
                                <p><strong>Factura Nº:</strong> {facturaId}</p>
                                <p><strong>Fecha:</strong> {fechaFactura:dd/MM/yyyy}</p>
                                <p><strong>Total:</strong> {total:C}</p>
                            </div>
                            
                            <p>Si tienes alguna pregunta sobre esta factura, no dudes en contactarnos.</p>
                            <p>¡Gracias por confiar en DogWalk App!</p>
                        </div>
                        <div class='footer'>
                            <p>© {DateTime.Now.Year} DogWalk App. Todos los derechos reservados.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailWithAttachmentsAsync(to, subject, body, new[] { pdfAttachment });
        }
    }
}