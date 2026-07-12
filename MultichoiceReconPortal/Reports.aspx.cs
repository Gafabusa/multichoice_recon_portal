using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MultichoiceReconPortal
{
    public partial class Reports : Page
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

            if (!IsPostBack)
            {
                txtFrom.Text = DateTime.Today.ToString("yyyy-MM-dd");
                txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
                BindChannels();
                LoadReport();
            }
        }

        private void BindChannels()
        {
            ddlChannel.Items.Clear();
            ddlChannel.Items.Add(new ListItem("All", ""));

            PortalUser user = Session["User"] as PortalUser;
            System.Collections.Generic.IEnumerable<string> channels =
                (user != null && user.SeesAllData)
                    ? (System.Collections.Generic.IEnumerable<string>)bll.GetChannels()
                    : bll.GetAssignedPartnerCodes(user.UserId);

            foreach (string ch in channels)
            {
                ddlChannel.Items.Add(new ListItem(ch.Trim(), ch.Trim()));
            }
        }

        protected void btnRun_Click(object sender, EventArgs e)
        {
            gvReport.PageIndex = 0;
            LoadReport();
        }

        protected void gvReport_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReport.PageIndex = e.NewPageIndex;
            LoadReport();
        }

        private void GetRange(out DateTime from, out DateTime to)
        {
            if (!DateTime.TryParse(txtFrom.Text, out from)) from = DateTime.Today;
            if (!DateTime.TryParse(txtTo.Text, out to)) to = DateTime.Today;
        }

        private void LoadReport()
        {
            DateTime from, to;
            GetRange(out from, out to);
            string bank = ddlChannel.SelectedValue;

            string scope = bll.GetViewScopeCsv(Session["User"] as PortalUser);

            long total = 0, recon = 0, failed = 0, unrecon = 0;
            DataTable stats = bll.GetDashboardStats(from, to, scope);
            if (stats.Rows.Count > 0)
            {
                DataRow r = stats.Rows[0];
                total = ToLong(r["TotalTxns"]);
                recon = ToLong(r["ReconciledCount"]);
                failed = ToLong(r["FailedCount"]);
                unrecon = ToLong(r["UnreconciledCount"]);
            }
            litTotal.Text = total.ToString("N0");
            litRecon.Text = recon.ToString("N0");
            litFailed.Text = failed.ToString("N0");
            litUnrecon.Text = unrecon.ToString("N0");
            long processed = recon + failed + unrecon;
            litRate.Text = (processed > 0 ? (recon * 100m / processed) : 0m).ToString("0.#") + "%";

            // transactions - the grid shows the SP's Status (RECONCILED/FAILED/PENDING).
            DataTable dt = bll.SearchTransactions(from, to, bank, "", scope);
            gvReport.DataSource = dt;
            gvReport.DataBind();
        }

        protected void btnExportSuccess_Click(object sender, EventArgs e)
        {
            Export("RECONCILED", "SUCCESS");
        }

        protected void btnExportFailed_Click(object sender, EventArgs e)
        {
            Export("FAILED", "FAILED");
        }

        protected void btnExportUnrecon_Click(object sender, EventArgs e)
        {
            Export("UNRECONCILED", "UNRECONCILED");
        }

        // Fetches every transaction for the range, then keeps only the rows whose
        // Status matches (RECONCILED for SUCCESS.csv, FAILED for FAILED.csv) - so
        // PENDING rows are never exported as failures.
        private void Export(string status, string label)
        {
            DateTime from, to;
            GetRange(out from, out to);
            string bank = ddlChannel.SelectedValue;
            string scope = bll.GetViewScopeCsv(Session["User"] as PortalUser);

            DataTable all = bll.SearchTransactions(from, to, bank, "", scope);
            DataTable dt = all.Clone();
            foreach (DataRow row in all.Rows)
            {
                string rowStatus = row.Table.Columns.Contains("Status") ? (row["Status"] ?? "").ToString() : "";
                if (string.Equals(rowStatus, status, StringComparison.OrdinalIgnoreCase)) dt.ImportRow(row);
            }

            string csv = BusinessLogic.ToCsv(dt);
            string fileName = label + "_" + from.ToString("yyyyMMdd") + "_" + to.ToString("yyyyMMdd") + ".csv";

            bll.LogAudit(Session["User"] as PortalUser, "Download report", fileName);

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.Write(csv);
            Response.Flush();
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
        }

        private static long ToLong(object o)
        {
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt64(o);
        }

        protected string StatusClass(object status)
        {
            string s = (status ?? "").ToString().ToUpper();
            if (s == "RECONCILED") return "bg-success";
            if (s == "FAILED") return "bg-danger";
            return "bg-warning text-dark";
        }
    }
}
