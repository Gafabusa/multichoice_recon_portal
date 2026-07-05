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
    }
}
