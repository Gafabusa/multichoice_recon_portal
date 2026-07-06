using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Web.Security;
using System.Web.UI;

namespace MultichoiceReconPortal
{
    public partial class SiteMaster : MasterPage
    {
        /// <summary>The signed-in user, available to any content page via Master.</summary>
        public PortalUser CurrentUser { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            CurrentUser = Session["User"] as PortalUser;

            // Session may have expired while the auth cookie is still valid;
            // rebuild the user from the identity if so.
            if (CurrentUser == null && Context.User.Identity.IsAuthenticated)
            {
                try { CurrentUser = new BusinessLogic().GetUserByEmail(Context.User.Identity.Name); }
                catch { CurrentUser = null; }
                Session["User"] = CurrentUser;
            }

            if (CurrentUser == null)
            {
                FormsAuthentication.SignOut();
                Response.Redirect("~/Default.aspx");
                return;
            }

            // First login (or admin reset): the user MUST change their password
            // before they can reach any other page.
            string currentFile = System.IO.Path.GetFileName(Request.Path ?? "").ToLower();
            if (CurrentUser.MustChangePassword && !currentFile.Contains("changepassword"))
            {
                Response.Redirect("~/ChangePassword.aspx");
                return;
            }

            litUserName.Text = CurrentUser.FullName;
            litTopName.Text = CurrentUser.FullName;
            litRole.Text = CurrentUser.RoleName;
            litInitials.Text = GetInitials(CurrentUser.FullName);

            // Only the admin sees user management.
            liUsers.Visible = CurrentUser.IsAdmin;
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Default.aspx");
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            string[] parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }
    }
}
