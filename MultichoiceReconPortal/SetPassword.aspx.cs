using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Web.Security;
using System.Web.UI;

namespace MultichoiceReconPortal
{
    public partial class SetPassword : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PortalUser user = Session["User"] as PortalUser;
            if (user == null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }
            // Only reachable while a password change is still pending.
            if (!user.MustChangePassword)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
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
                ShowMessage("Please fill in all three fields.");
                return;
            }
            if (!string.Equals(CommonLogic.Md5Hash(current), user.Password, StringComparison.OrdinalIgnoreCase))
            {
                ShowMessage("Your current password is incorrect.");
                return;
            }
            if (newPass.Length < 6)
            {
                ShowMessage("The new password must be at least 6 characters.");
                return;
            }
            if (newPass != confirm)
            {
                ShowMessage("The new password and confirmation do not match.");
                return;
            }
            if (string.Equals(CommonLogic.Md5Hash(newPass), user.Password, StringComparison.OrdinalIgnoreCase))
            {
                ShowMessage("The new password must be different from the temporary one.");
                return;
            }

            try
            {
                new BusinessLogic().ChangePassword(user.UserId, newPass);
            }
            catch (Exception)
            {
                ShowMessage("We could not set your password right now. Please try again.");
                return;
            }

            // Sign out and make them log in again with the new password before they
            // can reach the dashboard.
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Default.aspx?changed=1");
        }

        private void ShowMessage(string message)
        {
            lblMsg.Text = message;
            pnlMsg.CssClass = "alert alert-danger py-2 js-autohide";
            pnlMsg.Visible = true;
        }
    }
}
