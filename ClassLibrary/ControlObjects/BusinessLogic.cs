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

        // ---- forgot password (OTP) -------------------------------------------

        /// <summary>
        /// Starts a password reset: generates a 6-digit OTP, stores it encrypted with
        /// a 5-minute expiry, and emails it. Returns false if no active user has that
        /// email (the page shows the same message either way, to avoid leaking accounts).
        /// </summary>
        public bool StartPasswordReset(string email)
        {
            PortalUser user = db.GetUserByEmail(email);
            if (user == null || !user.IsActive) return false;

            string otp = CommonLogic.GenerateOtp();
            string encrypted = CommonLogic.EncryptOtp(otp);
            db.SetPasswordResetOtp(email, encrypted, DateTime.Now.AddMinutes(5));

            SendOtpEmail(email, user.FullName, otp);
            return true;
        }

        /// <summary>
        /// Verifies the OTP and sets the new password. Returns null on success, or a
        /// user-facing error message on failure.
        /// </summary>
        public string CompletePasswordReset(string email, string otp, string newPassword)
        {
            DataTable dt = db.GetPasswordResetInfo(email);
            if (dt.Rows.Count == 0) return "We could not find an active account for that email.";

            DataRow r = dt.Rows[0];
            string storedEnc = r["ResetOtp"] == DBNull.Value ? "" : r["ResetOtp"].ToString();
            if (string.IsNullOrEmpty(storedEnc))
                return "No reset request was found. Please request a new code.";

            if (r["ResetOtpExpiry"] == DBNull.Value || Convert.ToDateTime(r["ResetOtpExpiry"]) < DateTime.Now)
                return "The code has expired. Please request a new one.";

            string storedOtp = CommonLogic.DecryptOtp(storedEnc);
            if (string.IsNullOrEmpty(storedOtp) || storedOtp != (otp ?? "").Trim())
                return "The code you entered is incorrect.";

            db.CompletePasswordReset(Convert.ToInt32(r["UserId"]), CommonLogic.Md5Hash(newPassword));
            return null;
        }

        private void SendOtpEmail(string toEmail, string fullName, string otp)
        {
            string from = db.GetEmailFromAddress();
            string server = CommonLogic.ReadAppSetting("SMTP_SERVER", "smtp-relay.gmail.com");
            int port;
            if (!int.TryParse(CommonLogic.ReadAppSetting("SMTP_PORT", "587"), out port)) port = 587;

            string body =
                "<div style='font-family:Segoe UI,Arial,sans-serif;color:#1f2a44;font-size:14px'>" +
                "<h2 style='color:#10664a;margin:0 0 12px'>MultiChoice Reconciliation Portal</h2>" +
                "<p>Hello " + fullName + ",</p>" +
                "<p>Use the one-time code below to reset your password. It expires in 5 minutes.</p>" +
                "<p style='font-size:26px;font-weight:700;letter-spacing:4px;background:#eef4f1;border-radius:8px;padding:12px 14px;text-align:center'>" + otp + "</p>" +
                "<p>If you did not request this, you can ignore this email.</p>" +
                "<p style='color:#7a869a;font-size:12px'>This is an automated message, please do not reply.</p>" +
                "</div>";

            CommonLogic.SendEmail(server, port, from, toEmail, "Your password reset code", body, true);
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

        /// <summary>Updates a user's name, email and role. Returns -1 if the email belongs to another user.</summary>
        public int UpdateUser(int userId, string fullName, string email, int roleId)
        {
            return db.UpdateUser(userId, fullName, email, roleId);
        }

        public void DeleteUser(int userId)
        {
            db.DeleteUser(userId);
        }

        // ---- partner catalog (system admin) ----------------------------------

        public DataTable GetPartners()
        {
            return db.GetPartners();
        }

        /// <summary>Adds a partner. Returns -1 if the partner code already exists.</summary>
        public int AddPartner(string partnerCode, string partnerName)
        {
            return db.InsertPartner(partnerCode, partnerName);
        }

        // ---- audit log --------------------------------------------------------

        /// <summary>Records an action in the audit log. Never throws (auditing must not break the action).</summary>
        public void LogAudit(PortalUser user, string action, string details)
        {
            try
            {
                db.InsertAuditLog(user != null ? (int?)user.UserId : null,
                    user != null ? user.FullName : null, action, details);
            }
            catch
            {
                // auditing is best-effort; never surface an audit failure to the user
            }
        }

        /// <summary>
        /// Audit entries for a date range. System Admin sees everything; anyone else
        /// (Head Accounts) sees only Accountant and Head Accounts activity - never the
        /// System Admin's.
        /// </summary>
        public DataTable GetAuditLogs(DateTime fromDate, DateTime toDate, bool includeAdmin)
        {
            return db.GetAuditLogs(fromDate, toDate, includeAdmin);
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
                string rawRoot = CommonLogic.ReadAppSetting("RAW_FOLDER_ROOT");
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

        /// <summary>
        /// Drops a MultiChoice source-of-truth file into MULTICHOICE_RAW_FOLDER with a
        /// sidecar carrying the uploader's name/email. The service's MultiChoice thread
        /// imports it into ReceivedTransactionMultichoice.
        /// </summary>
        public UploadResult SaveMultichoiceUpload(string fileName, byte[] content, string uploaderName, string uploaderEmail)
        {
            UploadResult result = new UploadResult();
            try
            {
                string dir = CommonLogic.ReadAppSetting("MULTICHOICE_RAW_FOLDER");
                Directory.CreateDirectory(dir);

                string safeName = Path.GetFileName(fileName);
                string dest = Path.Combine(dir, safeName);
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
                    "source=MULTICHOICE\r\n" +
                    "uploadedon=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                File.WriteAllText(dest + ".meta", meta);

                result.Success = true;
                result.SavedFilePath = dest;
                result.Message = "MultiChoice records uploaded successfully. They will be imported shortly.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Upload failed: " + ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Returns the references in the uploaded file that were already reconciled
        /// for this partner. totalRefs returns the count of distinct references found.
        /// </summary>
        public List<string> FindAlreadyReconciled(string partner, byte[] content, out int totalRefs)
        {
            List<string> duplicates = new List<string>();
            HashSet<string> distinct = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            totalRefs = 0;
            if (content == null || content.Length == 0) return duplicates;

            HashSet<string> reconciled = db.GetReconciledRefs(partner);

            using (StreamReader reader = new StreamReader(new MemoryStream(content)))
            {
                string line;
                bool first = true;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0) continue;
                    string reference = line.Split(',')[0].Trim();
                    if (first && (reference.Equals("TransactionId", StringComparison.OrdinalIgnoreCase) ||
                                  reference.Equals("PartnerTxnRef", StringComparison.OrdinalIgnoreCase))) { first = false; continue; }
                    first = false;
                    if (reference.Length == 0 || !distinct.Add(reference)) continue;
                    if (reconciled.Contains(reference)) duplicates.Add(reference);
                }
            }
            totalRefs = distinct.Count;
            return duplicates;
        }

        // ---- reporting (read-only) -------------------------------------------

        public DataTable GetDashboardStats(DateTime fromDate, DateTime toDate, string partnersCsv)
        {
            return db.GetDashboardStats(fromDate, toDate, partnersCsv);
        }

        public DataTable GetDailyTrend(DateTime fromDate, DateTime toDate, string partnersCsv)
        {
            return db.GetDailyTrend(fromDate, toDate, partnersCsv);
        }

        public DataTable GetByChannel(DateTime fromDate, DateTime toDate, string partnersCsv)
        {
            return db.GetByChannel(fromDate, toDate, partnersCsv);
        }

        public DataTable SearchTransactions(DateTime fromDate, DateTime toDate, string partner, string search, string partnersCsv)
        {
            return db.SearchTransactions(fromDate, toDate, partner, search, partnersCsv);
        }

        // ---- partner assignment + scoping ------------------------------------

        public DataTable GetUserPartners(int userId)
        {
            return db.GetUserPartners(userId);
        }

        /// <summary>The partner codes assigned to a user.</summary>
        public List<string> GetAssignedPartnerCodes(int userId)
        {
            List<string> codes = new List<string>();
            DataTable dt = db.GetUserPartners(userId);
            foreach (DataRow dr in dt.Rows) codes.Add(dr["PartnerCode"].ToString());
            return codes;
        }

        // A partner code that can never exist, used to scope an accountant who has no
        // assignments to zero rows (the reporting procs treat NULL/'' as "all", so an
        // empty scope would otherwise leak everything).
        private const string NoPartnersSentinel = "__NONE__";

        /// <summary>
        /// The partner-scope CSV to pass to the reporting procs for this user:
        /// null for users who see everything (System Admin, Head Accounts); the
        /// accountant's assigned codes otherwise; and a sentinel that matches nothing
        /// when an accountant has no assigned partners.
        /// </summary>
        public string GetViewScopeCsv(PortalUser user)
        {
            if (user == null || user.SeesAllData) return null;
            List<string> codes = GetAssignedPartnerCodes(user.UserId);
            return codes.Count == 0 ? NoPartnersSentinel : string.Join(",", codes);
        }

        /// <summary>Replaces a user's partner assignments with the given partner ids.</summary>
        public void SaveUserPartners(int userId, IEnumerable<int> partnerIds, string assignedBy)
        {
            db.ClearUserPartners(userId);
            if (partnerIds == null) return;
            foreach (int pid in partnerIds) db.AssignUserPartner(userId, pid, assignedBy);
        }

        public bool HasMultichoiceRecordsForPartner(string partner)
        {
            return db.HasMultichoiceRecordsForPartner(partner);
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
