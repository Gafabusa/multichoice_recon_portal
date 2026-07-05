using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Web.Security;
using System.Web.UI;

namespace MultichoiceReconPortal
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Already signed in? Skip the login screen.
            if (!IsPostBack && Request.IsAuthenticated)
            {
                Response.Redirect("~/Dashboard.aspx");
            }

            if (!IsPostBack && Request.QueryString["changed"] == "1")
            {
                lblInfo.Text = "Your password has been changed. Please sign in with your new password.";
                pnlInfo.Visible = true;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your email and password.");
                return;
            }

            PortalUser user;
            try
            {
                user = new BusinessLogic().Login(email, password);
            }
            catch (Exception)
            {
                ShowError("We could not sign you in right now. Please try again in a moment.");
                return;
            }

            if (user == null)
            {
                ShowError("Invalid email or password.");
                return;
            }

            Session["User"] = user;
            FormsAuthentication.SetAuthCookie(user.Email, false);

            if (user.MustChangePassword)
            {
                Response.Redirect("~/ChangePassword.aspx");
            }
            else
            {
                Response.Redirect("~/Dashboard.aspx");
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            pnlError.Visible = true;
        }
    }
}
