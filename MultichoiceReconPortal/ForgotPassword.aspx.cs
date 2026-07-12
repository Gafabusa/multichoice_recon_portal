using ClassLibrary.ControlObjects;
using System;
using System.Web.UI;

namespace MultichoiceReconPortal
{
    public partial class ForgotPassword : Page
    {
        private readonly BusinessLogic bll = new BusinessLogic();

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSendCode_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                ShowMessage("Please enter your email address.", "alert-danger");
                return;
            }

            try
            {
                bll.StartPasswordReset(email);
            }
            catch (Exception)
            {
                // Swallow so we never reveal whether the email exists or the mail failed.
            }

            // Always advance to step 2 with the same message (don't leak whether the
            // account exists).
            hfEmail.Value = email;
            pnlRequest.Visible = false;
            pnlReset.Visible = true;
            ShowMessage("If that email belongs to an account, a 6-digit code has been sent to it. Enter it below with your new password.", "alert-success");
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            string email = hfEmail.Value;
            string otp = txtOtp.Text.Trim();
            string newPass = txtNew.Text;
            string confirm = txtConfirm.Text;

            pnlRequest.Visible = false;
            pnlReset.Visible = true;

            if (string.IsNullOrEmpty(otp) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirm))
            {
                ShowMessage("Please enter the code and your new password.", "alert-danger");
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

            string error;
            try
            {
                error = bll.CompletePasswordReset(email, otp, newPass);
            }
            catch (Exception)
            {
                ShowMessage("We could not reset your password right now. Please try again.", "alert-danger");
                return;
            }

            if (error != null)
            {
                ShowMessage(error, "alert-danger");
                return;
            }

            Response.Redirect("~/Default.aspx?changed=1");
        }

        private void ShowMessage(string message, string cssClass)
        {
            lblMsg.Text = message;
            pnlMsg.CssClass = "alert py-2 js-autohide " + cssClass;
            pnlMsg.Visible = true;
        }
    }
}
