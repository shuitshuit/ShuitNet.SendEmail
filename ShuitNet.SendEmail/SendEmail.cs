using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace ShuitNet.SendEmail
{
    public class SendEmail(ILogger<SendEmail> logger, Settings settings)
    {
        private readonly ILogger<SendEmail> _logger = logger;

        private readonly Settings _settings = settings;

        public async Task SendEmailAsync(string? to, string? subject,
            string? body, string? smtpServer, string? smtpUsername,
            string? smtpPassword, string from, string? fromName,
            string? cc, string? bcc, string? attachment, bool? smtpSsl,
            int smtpPort = 0)
        {
            using var client = new SmtpClient();
            if (string.IsNullOrEmpty(smtpServer))
                smtpServer = _settings.SmtpServer;
            if (string.IsNullOrEmpty(smtpUsername))
                smtpUsername = _settings.SmtpUsername;
            if (string.IsNullOrEmpty(smtpPassword))
                smtpPassword = _settings.SmtpPassword;
            if (smtpPort == 0)
                smtpPort = _settings.SmtpPort;
            // smtpSslがnullの場合は_settings.SmtpSslを使用
            smtpSsl ??= _settings.SmtpSsl;
            await client.ConnectAsync(smtpServer, smtpPort, (bool)smtpSsl);
            await client.AuthenticateAsync(smtpUsername, smtpPassword);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, from));
            if (!string.IsNullOrEmpty(to))
                message.To.Add(new MailboxAddress(to, to));
            if (!string.IsNullOrEmpty(cc))
                message.Cc.Add(new MailboxAddress(cc, cc));
            if (!string.IsNullOrEmpty(bcc))
                message.Bcc.Add(new MailboxAddress(bcc, bcc));

            if (!string.IsNullOrEmpty(subject))
                message.Subject = subject;
            if (!string.IsNullOrEmpty(body))
            {
                var textPart = new TextPart(TextFormat.Html);
                textPart.Text = body;
                message.Body = textPart;
            }
            if (!string.IsNullOrEmpty(attachment))
            {
                var attachmentPart = new MimePart("application", "octet-stream")
                {
                    Content = new MimeContent(File.OpenRead(attachment)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = Path.GetFileName(attachment)
                };
                var multipart = new Multipart("mixed");
                multipart.Add(attachmentPart);
                multipart.Add(message.Body);
                message.Body = multipart;
            }

            try
            {
                var result = await client.SendAsync(message);
                _logger.LogInformation("Email sent to {0}\nresult: {1}", to, result);
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, "Error sending email");
                throw new SendException(ex.Message, SendState.ClientError);
            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError(ex, "Error sending email");
                throw new SendException(ex.Message, SendState.ServerError);
            }
            catch (ServiceNotConnectedException ex)
            {
                _logger.LogError(ex, "Error sending email");
                throw new SendException(ex.Message, SendState.ConnectionError);
            }
            catch (ServiceNotAuthenticatedException ex)
            {
                _logger.LogError(ex, "Error sending email");
                throw new SendException(ex.Message, SendState.NotAuthenticated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                throw new SendException(ex.Message, SendState.Error);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
