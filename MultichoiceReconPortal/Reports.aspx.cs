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
            foreach (string ch in bll.GetChannels())
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

            // summary
            long total = 0, recon = 0, failed = 0;
            DataTable stats = bll.GetDashboardStats(from, to);
            if (stats.Rows.Count > 0)
            {
                DataRow r = stats.Rows[0];
                total = ToLong(r["TotalTxns"]);
                recon = ToLong(r["ReconciledCount"]);
                failed = ToLong(r["FailedCount"]);
            }
            litTotal.Text = total.ToString("N0");
            litRecon.Text = recon.ToString("N0");
            litFailed.Text = failed.ToString("N0");
            litRate.Text = (total > 0 ? (recon * 100m / total) : 0m).ToString("0.#") + "%";

            // transactions
            DataTable dt = bll.SearchTransactions(from, to, bank, "", "");
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

        protected void btnExportAll_Click(object sender, EventArgs e)
        {
            Export("", "ALL");
        }

        private void Export(string status, string label)
        {
            DateTime from, to;
            GetRange(out from, out to);
            string bank = ddlChannel.SelectedValue;

            DataTable dt = bll.SearchTransactions(from, to, bank, status, "");
            string csv = BusinessLogic.ToCsv(dt);
            string fileName = label + "_" + from.ToString("yyyyMMdd") + "_" + to.ToString("yyyyMMdd") + ".csv";

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
