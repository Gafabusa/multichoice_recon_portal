using ClassLibrary.EntityObjects;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace ClassLibrary.ControlObjects
{
    /// <summary>
    /// Portal data access. Same design as the recon service's DatabaseHandler:
    /// Enterprise Library DatabaseProviderFactory + stored procedures suffixed
    /// with "Multichoice". The portal has its own data layer (it does not
    /// reference the service assembly).
    /// </summary>
    public class DatabaseHandler
    {
        private readonly string connStringType = CommonLogic.ReadAppSetting("CONSTR_TYPE");
        private readonly string connString;

        private Database DB;
        private DbCommand procommand;

        public DatabaseHandler()
        {
            connString = string.IsNullOrEmpty(connStringType) ? "LIVE" : connStringType;
            DB = new DatabaseProviderFactory().Create(connString);
        }

        public DataSet ExecuteDataSet(string procedure, params object[] parameters)
        {
            procommand = DB.GetStoredProcCommand(procedure, parameters);
            procommand.CommandTimeout = 300;
            return DB.ExecuteDataSet(procommand);
        }

        public void ExecuteNonQuery(string procedure, params object[] parameters)
        {
            procommand = DB.GetStoredProcCommand(procedure, parameters);
            DB.ExecuteNonQuery(procommand);
        }

        // ---- authentication / users ------------------------------------------

        public PortalUser GetUserByEmail(string email)
        {
            DataTable dt = ExecuteDataSet("GetUserByEmailMultichoice", email).Tables[0];
            return dt.Rows.Count == 0 ? null : MapUser(dt.Rows[0]);
        }

        public void UpdateLastLogin(int userId)
        {
            ExecuteNonQuery("UpdateUserLastLoginMultichoice", userId);
        }

        /// <summary>Inserts a user. Returns -1 if the email already exists.</summary>
        public int InsertUser(string fullName, string email, string passwordHash, int roleId, string createdBy)
        {
            DataTable dt = ExecuteDataSet("InsertUserMultichoice", fullName, email, passwordHash, roleId, createdBy).Tables[0];
            return Convert.ToInt32(dt.Rows[0]["UserId"]);
        }

        public List<PortalUser> GetUsers()
        {
            List<PortalUser> users = new List<PortalUser>();
            DataTable dt = ExecuteDataSet("GetUsersMultichoice").Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                users.Add(MapUser(dr));
            }
            return users;
        }

        public DataTable GetRoles()
        {
            return ExecuteDataSet("GetRolesMultichoice").Tables[0];
        }

        public void SetUserActive(int userId, bool isActive)
        {
            ExecuteNonQuery("SetUserActiveMultichoice", userId, isActive);
        }

        /// <summary>Updates a user's name, email and role. Returns -1 if the email is taken by another user.</summary>
        public int UpdateUser(int userId, string fullName, string email, int roleId)
        {
            DataTable dt = ExecuteDataSet("UpdateUserMultichoice", userId, fullName, email, roleId).Tables[0];
            return Convert.ToInt32(dt.Rows[0]["UserId"]);
        }

        public void DeleteUser(int userId)
        {
            ExecuteNonQuery("DeleteUserMultichoice", userId);
        }

        public void ChangePassword(int userId, string passwordHash)
        {
            ExecuteNonQuery("ChangeUserPasswordMultichoice", userId, passwordHash);
        }

        // ---- password reset by OTP -------------------------------------------

        /// <summary>Stores the encrypted OTP + expiry on the user. Returns the UserId, or 0 if no active user has that email.</summary>
        public int SetPasswordResetOtp(string email, string otpEncrypted, DateTime expiry)
        {
            DataTable dt = ExecuteDataSet("SetPasswordResetOtpMultichoice", email, otpEncrypted, expiry).Tables[0];
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["UserId"]) : 0;
        }

        public DataTable GetPasswordResetInfo(string email)
        {
            return ExecuteDataSet("GetPasswordResetOtpMultichoice", email).Tables[0];
        }

        public void CompletePasswordReset(int userId, string passwordHash)
        {
            ExecuteNonQuery("CompletePasswordResetMultichoice", userId, passwordHash);
        }

        public void ResetPassword(int userId, string passwordHash)
        {
            ExecuteNonQuery("ResetUserPasswordMultichoice", userId, passwordHash);
        }

        // ---- partners (the catalog of partners we deal with) ------------------

        /// <summary>The partner codes we deal with, from the Partners table.</summary>
        public string[] GetPartnerCodes()
        {
            List<string> codes = new List<string>();
            DataTable dt = ExecuteDataSet("GetPartners2").Tables[0];
            foreach (DataRow dr in dt.Rows) codes.Add(dr["PartnerCode"].ToString());
            return codes.ToArray();
        }

        public DataTable GetPartners()
        {
            return ExecuteDataSet("GetPartners2").Tables[0];
        }

        // ---- user <-> partner assignments ------------------------------------

        public DataTable GetUserPartners(int userId)
        {
            return ExecuteDataSet("GetUserPartnersMultichoice", userId).Tables[0];
        }

        public void ClearUserPartners(int userId)
        {
            ExecuteNonQuery("ClearUserPartnersMultichoice", userId);
        }

        public void AssignUserPartner(int userId, int partnerId, string assignedBy)
        {
            ExecuteNonQuery("AssignUserPartnerMultichoice", userId, partnerId, assignedBy);
        }

        /// <summary>True if MultiChoice source-of-truth records exist for this partner.</summary>
        public bool HasMultichoiceRecordsForPartner(string partner)
        {
            DataTable dt = ExecuteDataSet("HasMultichoiceRecordsForPartner2", partner).Tables[0];
            return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        /// <summary>Adds a partner to the catalog. Returns -1 if the partner code already exists.</summary>
        public int InsertPartner(string partnerCode, string partnerName)
        {
            DataTable dt = ExecuteDataSet("InsertPartner2", partnerCode, partnerName).Tables[0];
            return Convert.ToInt32(dt.Rows[0]["PartnerId"]);
        }

        public HashSet<string> GetReconciledRefs(string partner)
        {
            HashSet<string> refs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DataTable dt = ExecuteDataSet("GetReconciledRefs2", partner).Tables[0];
            foreach (DataRow dr in dt.Rows) refs.Add(dr["TransactionId"].ToString());
            return refs;
        }

        // ---- audit log --------------------------------------------------------

        public void InsertAuditLog(int? userId, string userName, string action, string details)
        {
            ExecuteNonQuery("InsertAuditLog2",
                (object)userId ?? DBNull.Value,
                (object)userName ?? DBNull.Value,
                action,
                (object)details ?? DBNull.Value);
        }

        public DataTable GetAuditLogs(DateTime fromDate, DateTime toDate, bool includeAdmin)
        {
            return ExecuteDataSet("GetAuditLogs2", fromDate, toDate, includeAdmin).Tables[0];
        }

        // ---- recon reporting (read-only) -------------------------------------

        public DataTable GetDashboardStats(DateTime fromDate, DateTime toDate, string partnersCsv)
        {
            return ExecuteDataSet("GetReconDashboardStats2", fromDate, toDate,
                (object)partnersCsv ?? DBNull.Value).Tables[0];
        }

        public DataTable GetDailyTrend(DateTime fromDate, DateTime toDate, string partnersCsv)
        {
            return ExecuteDataSet("GetReconDailyTrend2", fromDate, toDate,
                (object)partnersCsv ?? DBNull.Value).Tables[0];
        }

        public DataTable GetByChannel(DateTime fromDate, DateTime toDate, string partnersCsv)
        {
            return ExecuteDataSet("GetReconByChannel2", fromDate, toDate,
                (object)partnersCsv ?? DBNull.Value).Tables[0];
        }

        public DataTable SearchTransactions(DateTime fromDate, DateTime toDate, string partner, string search, string partnersCsv)
        {
            return ExecuteDataSet("SearchReconTransactions2", fromDate, toDate,
                (object)partner ?? DBNull.Value, (object)search ?? DBNull.Value,
                (object)partnersCsv ?? DBNull.Value).Tables[0];
        }

        public DataTable GetRecentStatements(int top)
        {
            return ExecuteDataSet("GetRecentStatements2", top).Tables[0];
        }

        // ---- email ------------------------------------------------------------

        /// <summary>
        /// From address for portal emails - taken from the service's EmailDetails
        /// record (id "4"). No email address is hardcoded anywhere.
        /// </summary>
        public string GetEmailFromAddress()
        {
            DataTable dt = ExecuteDataSet("GetEmailDetails2Multichoice", "4").Tables[0];
            if (dt.Rows.Count > 0) return dt.Rows[0]["SMTPUsername"].ToString();
            return "";
        }

        /// <summary>
        /// Maps a user row. Defensive about columns because the login proc,
        /// the list proc and the insert proc each project a different subset.
        /// </summary>
        private PortalUser MapUser(DataRow dr)
        {
            PortalUser u = new PortalUser();
            DataColumnCollection cols = dr.Table.Columns;

            if (cols.Contains("UserId")) u.UserId = Convert.ToInt32(dr["UserId"]);
            if (cols.Contains("FullName")) u.FullName = dr["FullName"].ToString();
            if (cols.Contains("Email")) u.Email = dr["Email"].ToString();
            if (cols.Contains("Password")) u.Password = dr["Password"].ToString();
            if (cols.Contains("RoleId") && dr["RoleId"] != DBNull.Value) u.RoleId = Convert.ToInt32(dr["RoleId"]);
            if (cols.Contains("RoleName")) u.RoleName = dr["RoleName"].ToString();
            if (cols.Contains("IsActive") && dr["IsActive"] != DBNull.Value) u.IsActive = Convert.ToBoolean(dr["IsActive"]);
            if (cols.Contains("MustChangePassword") && dr["MustChangePassword"] != DBNull.Value) u.MustChangePassword = Convert.ToBoolean(dr["MustChangePassword"]);
            if (cols.Contains("CreatedBy")) u.CreatedBy = dr["CreatedBy"].ToString();
            if (cols.Contains("CreatedDate") && dr["CreatedDate"] != DBNull.Value) u.CreatedDate = Convert.ToDateTime(dr["CreatedDate"]);
            if (cols.Contains("LastLoginDate") && dr["LastLoginDate"] != DBNull.Value) u.LastLoginDate = Convert.ToDateTime(dr["LastLoginDate"]);

            return u;
        }
    }
}
