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

            // Admin-only page.
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
                ShowMessage("Please enter the user's full name and email.", "alert-danger");
                return;
            }

            if (!int.TryParse(ddlRole.SelectedValue, out roleId))
            {
                ShowMessage("Please choose a role.", "alert-danger");
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
                ShowMessage("We could not create the user right now. Please try again.", "alert-danger");
                return;
            }

            if (newId == -1)
            {
                ShowMessage("A user with that email already exists.", "alert-danger");
                return;
            }

            // Email the temporary password. If mail fails, still report the account was created.
            try
            {
                bll.SendCredentialsEmail(email, fullName, tempPassword, false);
                ShowMessage("User created. A temporary password has been emailed to " + email + ".", "alert-success");
            }
            catch (Exception)
            {
                ShowMessage("User created, but the email could not be sent. Temporary password: " + tempPassword, "alert-warning");
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
            string email = Convert.ToString(key.Values["Email"]);
            string fullName = Convert.ToString(key.Values["FullName"]);
            bool isActive = Convert.ToBoolean(key.Values["IsActive"]);

            if (e.CommandName == "ToggleActive")
            {
                try
                {
                    bll.SetUserActive(userId, !isActive);
                    ShowMessage("User " + (isActive ? "disabled" : "enabled") + ".", "alert-success");
                }
                catch (Exception)
                {
                    ShowMessage("We could not update the user right now. Please try again.", "alert-danger");
                }
            }

            BindUsers();
        }

        private void ShowMessage(string message, string cssClass)
        {
            lblMsg.Text = message;
            pnlMsg.CssClass = "alert py-2 " + cssClass;
            pnlMsg.Visible = true;
        }
    }
}
