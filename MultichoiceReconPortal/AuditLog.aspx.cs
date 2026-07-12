using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MultichoiceReconPortal
{
    public partial class AuditLog : Page
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
            if (!user.CanViewAudit)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Default view: the current day and the previous day.
                txtFrom.Text = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
                txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
                LoadLogs();
            }
        }

        protected void btnRun_Click(object sender, EventArgs e)
        {
            gvAudit.PageIndex = 0;
            LoadLogs();
        }

        protected void gvAudit_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAudit.PageIndex = e.NewPageIndex;
            LoadLogs();
        }

        private void LoadLogs()
        {
            DateTime from, to;
            if (!DateTime.TryParse(txtFrom.Text, out from)) from = DateTime.Today.AddDays(-1);
            if (!DateTime.TryParse(txtTo.Text, out to)) to = DateTime.Today;

            gvAudit.DataSource = bll.GetAuditLogs(from, to);
            gvAudit.DataBind();
        }
    }
}
