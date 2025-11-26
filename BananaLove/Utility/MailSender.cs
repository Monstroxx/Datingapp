using System;
using System.Net;
using System.Net.Mail;

namespace BananaLove.Utility
{
    public static class MailSender
    {
        /// <summary>
        /// Sendet eine Passwort-Zurücksetzen-Mail an den angegebenen Empfänger.
        /// SMTP-Konfiguration wird aus Umgebungsvariablen gelesen:
        /// SmtpHost, SmtpPort, SmtpUser, SmtpPass, optional SmtpUseSsl.
        /// </summary>
        public static void SendPasswordResetMail(string toEmail, string newPassword)
        {
            try
            {
                string host = Environment.GetEnvironmentVariable("SmtpHost");
                string portStr = Environment.GetEnvironmentVariable("SmtpPort");
                string user = Environment.GetEnvironmentVariable("SmtpUser");
                string pass = Environment.GetEnvironmentVariable("SmtpPass");
                string useSslStr = Environment.GetEnvironmentVariable("SmtpUseSsl");

                if (string.IsNullOrWhiteSpace(host) ||
                    string.IsNullOrWhiteSpace(portStr) ||
                    string.IsNullOrWhiteSpace(user) ||
                    string.IsNullOrWhiteSpace(pass))
                {
                    DebugHandler.LogError("SMTP-Konfiguration unvollständig. Bitte SmtpHost, SmtpPort, SmtpUser und SmtpPass setzen.");
                    return;
                }

                int port = int.TryParse(portStr, out var p) ? p : 465;
                bool useSsl = string.IsNullOrWhiteSpace(useSslStr)
                    ? true
                    : bool.TryParse(useSslStr, out var b) ? b : true;

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = useSsl;
                    client.Credentials = new NetworkCredential(user, pass);

                    var message = new MailMessage
                    {
                        From = new MailAddress(user, "BananaLove"),
                        Subject = "Dein neues BananaLove Passwort",
                        Body = $"Hallo,\n\n" +
                               $"du hast ein neues Passwort für deinen BananaLove-Account angefordert.\n\n" +
                               $"Dein neues Passwort lautet:\n\n" +
                               $"{newPassword}\n\n" +
                               $"Viele Grüße\n" +
                               $"Dein BananaLove Team",
                        IsBodyHtml = false
                    };

                    message.To.Add(new MailAddress(toEmail));

                    client.Send(message);
                    DebugHandler.Log($"Passwort-Reset-E-Mail an {toEmail} gesendet.");
                }
            }
             catch (Exception ex)
             {
                 DebugHandler.LogError($"Fehler beim Versenden der Passwort-Reset-E-Mail: {ex}");
             }
        }
    }
}


