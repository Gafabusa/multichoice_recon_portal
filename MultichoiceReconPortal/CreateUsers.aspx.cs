using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MultichoiceReconPortal
{
    public partial class CreateUsers : Page
    {
        private readonly BusinessLogic bll = new BusinessLogic();

        protected void Page_Load(object sender, EventArgs e)
        {
            PortalUser user = Session["User"] as PortalUser;
            if (user == null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }
            if (!user.IsAdmin)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindRoles();
                BindUsers();
            }
        }

        private void BindRoles()
        {
            DataTable roles = bll.GetRoles();
            ddlRole.DataSource = roles;
            ddlRole.DataTextField = "RoleName";
            ddlRole.DataValueField = "RoleId";
            ddlRole.DataBind();
        }

        private void BindUsers()
        {
            gvUsers.DataSource = bll.GetUsers();
            gvUsers.DataBind();
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            int roleId;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
            {
                ShowModalError("Please enter the user's full name and email.");
                return;
            }
            if (!int.TryParse(ddlRole.SelectedValue, out roleId))
            {
                ShowModalError("Please choose a role.");
                return;
            }

            PortalUser admin = Session["User"] as PortalUser;
            string tempPassword;
            int newId;
            try
            {
                newId = bll.CreateUser(fullName, email, roleId, admin != null ? admin.Email : "SYSTEM", out tempPassword);
            }
            catch (Exception)
            {
                ShowModalError("We could not create the user right now. Please try again.");
                return;
            }

            if (newId == -1)
            {
                ShowModalError("A user with that email already exists.");
                return;
            }

            try
            {
                bll.SendCredentialsEmail(email, fullName, tempPassword, false);
                ShowPageMessage("User created. A temporary password has been emailed to " + email + ".", "alert-success");
            }
            catch (Exception)
            {
                ShowPageMessage("User created, but the email could not be sent. Temporary password: " + tempPassword, "alert-warning");
            }

            txtFullName.Text = "";
            txtEmail.Text = "";
            BindUsers();
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out index))
            {
                return;
            }

            DataKey key = gvUsers.DataKeys[index];
            int userId = Convert.ToInt32(key.Values["UserId"]);
            bool isActive = Convert.ToBoolean(key.Values["IsActive"]);

            if (e.CommandName == "ToggleActive")
            {
                try
                {
                    bll.SetUserActive(userId, !isActive);
                    ShowPageMessage("User " + (isActive ? "disabled" : "enabled") + ".", "alert-success");
                }
                catch (Exception)
                {
                    ShowPageMessage("We could not update the user right now. Please try again.", "alert-danger");
                }
            }

            BindUsers();
        }

        private void ShowPageMessage(string message, string cssClass)
        {
            lblMsg.Text = message;
            pnlMsg.CssClass = "alert py-2 js-autohide " + cssClass;
            pnlMsg.Visible = true;
        }

        private void ShowModalError(string message)
        {
            lblModalMsg.Text = message;
            pnlModalMsg.CssClass = "alert alert-danger py-2 js-modal-alert";
            pnlModalMsg.Visible = true;
            // Re-open the modal after the postback so the user sees the error
            // (the shared script then auto-hides it and closes the popup at 10s).
            ClientScript.RegisterStartupScript(GetType(), "reopenAddUser",
                "var m=new bootstrap.Modal(document.getElementById('addUserModal'));m.show();", true);
        }
    }
}
