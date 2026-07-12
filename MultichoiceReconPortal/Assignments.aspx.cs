using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
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
            DataTable dt = new DataTable();
            dt.Columns.Add("UserId", typeof(int));
            dt.Columns.Add("FullName");
            dt.Columns.Add("RoleName");
            dt.Columns.Add("Partners");

            foreach (PortalUser u in bll.GetUsers())
            {
                if (!(u.IsAccountant || u.IsHeadAccounts)) continue;

                List<string> names = new List<string>();
                foreach (DataRow pr in bll.GetUserPartners(u.UserId).Rows) names.Add(pr["PartnerName"].ToString());
                dt.Rows.Add(u.UserId, u.FullName, u.RoleName, string.Join(", ", names));
            }

            gvUsers.DataSource = dt;
            gvUsers.DataBind();
        }

        protected string BuildTags(object partners)
        {
            string value = partners == null ? "" : partners.ToString();
            if (string.IsNullOrWhiteSpace(value)) return "<span class='text-muted'>None</span>";

            StringBuilder sb = new StringBuilder();
            foreach (string name in value.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
            {
                sb.Append("<span class='badge'>").Append(HttpUtility.HtmlEncode(name)).Append("</span>");
            }
            return sb.ToString();
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Assign") return;

            int index;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out index)) return;

            DataKey key = gvUsers.DataKeys[index];
            int userId = Convert.ToInt32(key.Values["UserId"]);
            string fullName = Convert.ToString(key.Values["FullName"]);

            hfUserId.Value = userId.ToString();
            litAssignUser.Text = HttpUtility.HtmlEncode(fullName);

            cblPartners.DataSource = bll.GetPartners();
            cblPartners.DataBind();

            HashSet<string> assigned = new HashSet<string>();
            foreach (DataRow pr in bll.GetUserPartners(userId).Rows) assigned.Add(pr["PartnerId"].ToString());
            foreach (ListItem item in cblPartners.Items) item.Selected = assigned.Contains(item.Value);

            OpenModal();
        }

        protected void btnSaveAssign_Click(object sender, EventArgs e)
        {
            PortalUser admin = Session["User"] as PortalUser;
            if (admin == null || !admin.CanAssignPartners)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            int userId;
            if (!int.TryParse(hfUserId.Value, out userId))
            {
                ShowMessage("We could not identify the user. Please try again.", "alert-danger");
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

            // Rebind the table; the modal is NOT reopened, so it closes cleanly.
            BindUsers();
        }

        private void OpenModal()
        {
            // RegisterStartupScript renders only for this response, so the modal does
            // not reopen on the next postback.
            ClientScript.RegisterStartupScript(GetType(), "openAssign",
                "var m=new bootstrap.Modal(document.getElementById('assignModal'));m.show();", true);
        }

        private void ShowMessage(string message, string cssClass)
        {
            lblMsg.Text = message;
            pnlMsg.CssClass = "alert py-2 js-autohide " + cssClass;
            pnlMsg.Visible = true;
        }
    }
}
