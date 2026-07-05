using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Web.Security;
using System.Web.UI;

namespace MultichoiceReconPortal
{
    public partial class ChangePassword : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PortalUser user = Session["User"] as PortalUser;
            if (user == null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (!IsPostBack && user.MustChangePassword)
            {
                ShowMessage("This is your first sign in. Please set a new password to continue.", "alert-warning");
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            PortalUser user = Session["User"] as PortalUser;
            if (user == null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            string current = txtCurrent.Text;
            string newPass = txtNew.Text;
            string confirm = txtConfirm.Text;

            if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirm))
            {
                ShowMessage("Please fill in all three fields.", "alert-danger");
                return;
            }

            if (!string.Equals(CommonLogic.Md5Hash(current), user.Password, StringComparison.OrdinalIgnoreCase))
            {
                ShowMessage("Your current password is incorrect.", "alert-danger");
                return;
            }

            if (newPass.Length < 6)
            {
                ShowMessage("The new password must be at least 6 characters.", "alert-danger");
                return;
            }

            if (newPass != confirm)
            {
                ShowMessage("The new password and confirmation do not match.", "alert-danger");
                return;
            }

            if (string.Equals(CommonLogic.Md5Hash(newPass), user.Password, StringComparison.OrdinalIgnoreCase))
            {
                ShowMessage("The new password must be different from the current one.", "alert-danger");
                return;
            }

            bool wasForced = user.MustChangePassword;

            try
            {
                new BusinessLogic().ChangePassword(user.UserId, newPass);
            }
            catch (Exception)
            {
                ShowMessage("We could not update your password right now. Please try again.", "alert-danger");
                return;
            }

            if (wasForced)
            {
                // First-login change: sign the user out and make them log in again
                // with the new password before they can reach the dashboard.
                FormsAuthentication.SignOut();
                Session.Clear();
                Session.Abandon();
                Response.Redirect("~/Default.aspx?changed=1");
                return;
            }

            // Voluntary change: refresh the cached user and return to the dashboard.
            user.Password = CommonLogic.Md5Hash(newPass);
            user.MustChangePassword = false;
            Session["User"] = user;
            Response.Redirect("~/Dashboard.aspx");
        }

        private void ShowMessage(string message, string cssClass)
        {
            lblMsg.Text = message;
            pnlMsg.CssClass = "alert py-2 " + cssClass;
            pnlMsg.Visible = true;
        }
    }
}
