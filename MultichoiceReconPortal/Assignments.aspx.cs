using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MultichoiceReconPortal
{
    public partial class Assignments : Page
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
            if (!user.CanAssignPartners)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindUsers();
            }
        }

        // Head Accounts assigns partners to accountants and to themselves.
        private void BindUsers()
        {
            ddlUser.Items.Clear();
            ddlUser.Items.Add(new ListItem("-- select a user --", ""));
            foreach (PortalUser u in bll.GetUsers())
            {
                if (u.IsAccountant || u.IsHeadAccounts)
                {
                    ddlUser.Items.Add(new ListItem(u.FullName + " (" + u.RoleName + ")", u.UserId.ToString()));
                }
            }
        }

        protected void ddlUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindPartners();
        }

        private void BindPartners()
        {
            int userId;
            if (!int.TryParse(ddlUser.SelectedValue, out userId))
            {
                pnlPartners.Visible = false;
                return;
            }

            cblPartners.DataSource = bll.GetPartners();
            cblPartners.DataBind();

            HashSet<string> assigned = new HashSet<string>();
            foreach (DataRow dr in bll.GetUserPartners(userId).Rows)
            {
                assigned.Add(dr["PartnerId"].ToString());
            }
            foreach (ListItem item in cblPartners.Items)
            {
                item.Selected = assigned.Contains(item.Value);
            }

            pnlPartners.Visible = true;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            PortalUser admin = Session["User"] as PortalUser;
            if (admin == null || !admin.CanAssignPartners)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            int userId;
            if (!int.TryParse(ddlUser.SelectedValue, out userId))
            {
                ShowMessage("Please select a user first.", "alert-danger");
                return;
            }

            List<int> partnerIds = new List<int>();
            foreach (ListItem item in cblPartners.Items)
            {
                int pid;
                if (item.Selected && int.TryParse(item.Value, out pid)) partnerIds.Add(pid);
            }

            try
            {
                bll.SaveUserPartners(userId, partnerIds, admin.Email);
                ShowMessage("Assignments saved (" + partnerIds.Count + " partner(s)).", "alert-success");
            }
            catch (Exception)
            {
                ShowMessage("We could not save the assignments right now. Please try again.", "alert-danger");
            }
        }

        private void ShowMessage(string message, string cssClass)
        {
            lblMsg.Text = message;
            pnlMsg.CssClass = "alert py-2 js-autohide " + cssClass;
            pnlMsg.Visible = true;
        }
    }
}
