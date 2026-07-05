using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace ClassLibrary.ControlObjects
{
    /// <summary>
    /// Small shared helpers for the portal (config access, password hashing).
    /// Mirrors the CommonLogic in the recon service.
    /// </summary>
    public class CommonLogic
    {
        public static string ReadAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static string ReadAppSetting(string key, string fallback)
        {
            string value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? fallback : value;
        }

        /// <summary>
        /// MD5 hash as a 32-char uppercase hex string. Matches the seeded
        /// Password values in UsersMultichoice (e.g. admin123 -> 0192023A7BBD73250516F069DF18B500).
        /// </summary>
        public static string Md5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Sends an email through the SMTP relay (same setup as the recon
        /// service: TLS 1.2, no stored credentials — the relay is IP-authorized).
        /// </summary>
        public static void SendEmail(string smtpServer, int smtpPort, string from, string to, string subject, string body, bool isHtml = true)
        {
            using (System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage())
            {
                message.From = new System.Net.Mail.MailAddress(from);
                message.To.Add(new System.Net.Mail.MailAddress(to));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Timeout = 200000;
                    client.Send(message);
                }
            }
        }

        /// <summary>
        /// Generates a readable random password for new/reset users.
        /// Excludes ambiguous characters (0/O, 1/l/I).
        /// </summary>
        public static string GeneratePassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            byte[] data = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
            }
            StringBuilder sb = new StringBuilder(length);
            foreach (byte b in data)
            {
                sb.Append(chars[b % chars.Length]);
            }
            return sb.ToString();
        }
    }
}
