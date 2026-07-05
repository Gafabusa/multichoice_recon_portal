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

        public void ChangePassword(int userId, string passwordHash)
        {
            ExecuteNonQuery("ChangeUserPasswordMultichoice", userId, passwordHash);
        }

        public void ResetPassword(int userId, string passwordHash)
        {
            ExecuteNonQuery("ResetUserPasswordMultichoice", userId, passwordHash);
        }

        // ---- recon reporting (read-only) -------------------------------------

        public DataTable GetDashboardStats(DateTime fromDate, DateTime toDate)
        {
            return ExecuteDataSet("GetReconDashboardStatsMultichoice", fromDate, toDate).Tables[0];
        }

        public DataTable GetDailyTrend(DateTime fromDate, DateTime toDate)
        {
            return ExecuteDataSet("GetReconDailyTrendMultichoice", fromDate, toDate).Tables[0];
        }

        public DataTable GetByChannel(DateTime fromDate, DateTime toDate)
        {
            return ExecuteDataSet("GetReconByChannelMultichoice", fromDate, toDate).Tables[0];
        }

        public DataTable GetFailureReasons(DateTime fromDate, DateTime toDate)
        {
            return ExecuteDataSet("GetReconFailureReasonsMultichoice", fromDate, toDate).Tables[0];
        }

        public DataTable SearchTransactions(DateTime fromDate, DateTime toDate, string bank, string status, string search)
        {
            return ExecuteDataSet("SearchReconTransactionsMultichoice", fromDate, toDate,
                (object)bank ?? DBNull.Value, (object)status ?? DBNull.Value, (object)search ?? DBNull.Value).Tables[0];
        }

        public DataTable GetRecentStatements(int top)
        {
            return ExecuteDataSet("GetRecentStatementsMultichoice", top).Tables[0];
        }

        // ---- email ------------------------------------------------------------

        /// <summary>
        /// From address for portal emails. Reuses the service's EmailDetails
        /// record (id "4"); falls back to the SMTP_FROM app setting.
        /// </summary>
        public string GetEmailFromAddress()
        {
            try
            {
                DataTable dt = ExecuteDataSet("GetEmailDetails2Multichoice", "4").Tables[0];
                if (dt.Rows.Count > 0 && !string.IsNullOrEmpty(dt.Rows[0]["SMTPUsername"].ToString()))
                {
                    return dt.Rows[0]["SMTPUsername"].ToString();
                }
            }
            catch
            {
                // fall through to config default
            }
            return CommonLogic.ReadAppSetting("SMTP_FROM", "noreply@pegasus.co.ug");
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
