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

        // Fixed key for encrypting the password-reset OTP at rest (never stored in
        // plain text). A 256-bit AES key is derived from this passphrase.
        private const string OtpKey = "Umeme2501PegPay";

        /// <summary>A random 6-digit one-time password.</summary>
        public static string GenerateOtp()
        {
            byte[] data = new byte[4];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
            }
            uint value = BitConverter.ToUInt32(data, 0) % 1000000u;
            return value.ToString("D6");
        }

        /// <summary>AES-encrypts a value with the OTP key; returns base64(IV + cipher).</summary>
        public static string EncryptOtp(string plainText)
        {
            byte[] plain = Encoding.UTF8.GetBytes(plainText ?? string.Empty);
            using (Aes aes = Aes.Create())
            {
                aes.Key = OtpKeyBytes();
                aes.GenerateIV();
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);
                    byte[] result = new byte[aes.IV.Length + cipher.Length];
                    Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                    Buffer.BlockCopy(cipher, 0, result, aes.IV.Length, cipher.Length);
                    return Convert.ToBase64String(result);
                }
            }
        }

        /// <summary>Reverses <see cref="EncryptOtp"/>; returns "" if the value can't be decrypted.</summary>
        public static string DecryptOtp(string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted)) return "";
            try
            {
                byte[] all = Convert.FromBase64String(encrypted);
                using (Aes aes = Aes.Create())
                {
                    aes.Key = OtpKeyBytes();
                    byte[] iv = new byte[16];
                    Buffer.BlockCopy(all, 0, iv, 0, iv.Length);
                    aes.IV = iv;
                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        byte[] cipher = new byte[all.Length - iv.Length];
                        Buffer.BlockCopy(all, iv.Length, cipher, 0, cipher.Length);
                        byte[] plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
                        return Encoding.UTF8.GetString(plain);
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        private static byte[] OtpKeyBytes()
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(OtpKey));
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
