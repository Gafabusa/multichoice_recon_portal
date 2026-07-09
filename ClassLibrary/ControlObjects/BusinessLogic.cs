using ClassLibrary.EntityObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace ClassLibrary.ControlObjects
{
    /// <summary>
    /// Portal business logic. The .aspx code-behind calls into here; all DB
    /// work goes through DatabaseHandler. Mirrors the service's layering.
    /// </summary>
    public class BusinessLogic
    {
        private readonly DatabaseHandler db = new DatabaseHandler();

        // ---- authentication --------------------------------------------------

        /// <summary>
        /// Validates email + password. Returns the user on success, otherwise
        /// null (unknown email, inactive account, or wrong password).
        /// </summary>
        public PortalUser Login(string email, string password)
        {
            PortalUser user = db.GetUserByEmail(email);
            if (user == null || !user.IsActive)
            {
                return null;
            }

            string hash = CommonLogic.Md5Hash(password);
            if (!string.Equals(user.Password, hash, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            db.UpdateLastLogin(user.UserId);
            return user;
        }

        public PortalUser GetUserByEmail(string email)
        {
            return db.GetUserByEmail(email);
        }

        /// <summary>User changes their own password (clears the force-change flag).</summary>
        public void ChangePassword(int userId, string newPassword)
        {
            db.ChangePassword(userId, CommonLogic.Md5Hash(newPassword));
        }

        // ---- user management (admin) -----------------------------------------

        /// <summary>
        /// Creates a user with a system-generated temporary password. The plain
        /// temp password is returned (via out) so it can be emailed to the user;
        /// only its MD5 hash is stored. Returns -1 if the email already exists.
        /// </summary>
        public int CreateUser(string fullName, string email, int roleId, string createdBy, out string tempPassword)
        {
            tempPassword = CommonLogic.GeneratePassword(10);
            return db.InsertUser(fullName, email, CommonLogic.Md5Hash(tempPassword), roleId, createdBy);
        }

        /// <summary>Admin resets a user's password; returns the new temp password to email out.</summary>
        public string ResetPassword(int userId)
        {
            string tempPassword = CommonLogic.GeneratePassword(10);
            db.ResetPassword(userId, CommonLogic.Md5Hash(tempPassword));
            return tempPassword;
        }

        /// <summary>
        /// Emails a user their temporary password (new account or admin reset).
        /// Uses the same SMTP relay as the recon service.
        /// </summary>
        public void SendCredentialsEmail(string toEmail, string fullName, string tempPassword, bool isReset)
        {
            string from = db.GetEmailFromAddress();
            string server = CommonLogic.ReadAppSetting("SMTP_SERVER", "smtp-relay.gmail.com");
            int port;
            if (!int.TryParse(CommonLogic.ReadAppSetting("SMTP_PORT", "587"), out port))
            {
                port = 587;
            }

            string subject = isReset
                ? "Your MultiChoice Reconciliation Portal password was reset"
                : "Your MultiChoice Reconciliation Portal account";

            string intro = isReset
                ? "Your password has been reset by an administrator."
                : "An account has been created for you on the MultiChoice Reconciliation Portal.";

            string body =
                "<div style='font-family:Segoe UI,Arial,sans-serif;color:#1f2a44;font-size:14px'>" +
                "<h2 style='color:#0033a1;margin:0 0 12px'>MultiChoice Reconciliation Portal</h2>" +
                "<p>Hello " + fullName + ",</p>" +
                "<p>" + intro + "</p>" +
                "<p style='background:#eef2f8;border-radius:8px;padding:12px 14px'>" +
                "<b>Login email:</b> " + toEmail + "<br/>" +
                "<b>Temporary password:</b> " + tempPassword + "</p>" +
                "<p>For your security, please sign in and change this password immediately.</p>" +
                "<p style='color:#7a869a;font-size:12px'>This is an automated message, please do not reply.</p>" +
                "</div>";

            CommonLogic.SendEmail(server, port, from, toEmail, subject, body, true);
        }

        public List<PortalUser> GetUsers()
        {
            return db.GetUsers();
        }

        public DataTable GetRoles()
        {
            return db.GetRoles();
        }

        public void SetUserActive(int userId, bool isActive)
        {
            db.SetUserActive(userId, isActive);
        }

        // ---- manual upload ---------------------------------------------------

        /// <summary>Partners the portal accepts manual uploads for (from the Partners table).</summary>
        public string[] GetChannels()
        {
            return db.GetPartnerCodes();
        }

        /// <summary>
        /// Drops an uploaded statement into the service's raw\&lt;CHANNEL&gt; folder
        /// (exactly where the email downloader puts them) plus a sidecar
        /// "&lt;file&gt;.meta" carrying the uploader's name/email. The service formatter
        /// reads the sidecar and stamps ProcessedBy (=> ReconciledBy) with the
        /// uploader instead of the AutoReconService constant.
        /// </summary>
        public UploadResult SaveUpload(string channel, string fileName, byte[] content, string uploaderName, string uploaderEmail)
        {
            UploadResult result = new UploadResult();
            try
            {
                string rawRoot = CommonLogic.ReadAppSetting("RAW_FOLDER_ROOT", @"D:\MultichoiceReconFiles\raw");
                string dir = Path.Combine(rawRoot, channel);
                Directory.CreateDirectory(dir);

                string safeName = Path.GetFileName(fileName);
                string dest = Path.Combine(dir, safeName);

                // Don't clobber an existing raw file waiting to be formatted.
                if (File.Exists(dest))
                {
                    string baseName = Path.GetFileNameWithoutExtension(safeName);
                    string ext = Path.GetExtension(safeName);
                    safeName = baseName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ext;
                    dest = Path.Combine(dir, safeName);
                }

                File.WriteAllBytes(dest, content);

                string meta =
                    "uploadedby=" + uploaderName + "\r\n" +
                    "email=" + uploaderEmail + "\r\n" +
                    "source=MANUAL\r\n" +
                    "uploadedon=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                File.WriteAllText(dest + ".meta", meta);

                result.Success = true;
                result.SavedFilePath = dest;
                result.Message = "Statement uploaded successfully. It will be picked up and reconciled shortly.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Upload failed: " + ex.Message;
            }
            return result;
        }

        public DataTable GetRecentUploads(int top)
        {
            return db.GetRecentStatements(top);
        }

        // ---- reporting (read-only) -------------------------------------------

        public DataTable GetDashboardStats(DateTime fromDate, DateTime toDate)
        {
            return db.GetDashboardStats(fromDate, toDate);
        }

        public DataTable GetDailyTrend(DateTime fromDate, DateTime toDate)
        {
            return db.GetDailyTrend(fromDate, toDate);
        }

        public DataTable GetByChannel(DateTime fromDate, DateTime toDate)
        {
            return db.GetByChannel(fromDate, toDate);
        }

        public DataTable SearchTransactions(DateTime fromDate, DateTime toDate, string partner, string search)
        {
            return db.SearchTransactions(fromDate, toDate, partner, search);
        }

        /// <summary>Renders a DataTable to CSV (for the SUCCESS/FAILED excel downloads).</summary>
        public static string ToCsv(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(CsvField(dt.Columns[i].ColumnName));
            }
            sb.AppendLine();

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(CsvField(row[i] == null ? string.Empty : row[i].ToString()));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string CsvField(string value)
        {
            if (value == null) return string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}
