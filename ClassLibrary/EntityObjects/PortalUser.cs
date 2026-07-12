using System;

namespace ClassLibrary.EntityObjects
{
    /// <summary>
    /// A portal user (row in UsersMultichoice). Login is by Email; Password is
    /// an MD5 hash. Role drives what the user can see/do in the portal.
    /// </summary>
    public class PortalUser
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }   // MD5 hash (32 hex chars)
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public bool MustChangePassword { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public bool IsAdmin
        {
            get { return string.Equals(RoleName, "SystemAdmin", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsHeadAccounts
        {
            get { return string.Equals(RoleName, "HeadAccounts", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsAccountant
        {
            get { return string.Equals(RoleName, "Accountant", StringComparison.OrdinalIgnoreCase); }
        }

        // ---- what this user is allowed to do (drives nav + page guards) -------

        /// <summary>Head Accounts and Accountants upload; System Admin never uploads.</summary>
        public bool CanUpload { get { return IsHeadAccounts || IsAccountant; } }

        /// <summary>Only the System Admin creates/edits/deletes users and adds partners.</summary>
        public bool CanManageUsers { get { return IsAdmin; } }

        /// <summary>Only the System Admin adds partners to the catalog.</summary>
        public bool CanManagePartners { get { return IsAdmin; } }

        /// <summary>Only Head Accounts assigns partners to accountants (and self).</summary>
        public bool CanAssignPartners { get { return IsHeadAccounts; } }

        /// <summary>Head Accounts assigns/uploads the MultiChoice source-of-truth file.</summary>
        public bool CanUploadMultichoice { get { return IsHeadAccounts; } }

        /// <summary>System Admin and Head Accounts see everything; Accountants are scoped to their partners.</summary>
        public bool SeesAllData { get { return IsAdmin || IsHeadAccounts; } }

        /// <summary>System Admin and Head Accounts can view the audit log.</summary>
        public bool CanViewAudit { get { return IsAdmin || IsHeadAccounts; } }
    }
}
