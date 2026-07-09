using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MultichoiceReconPortal
{
    public partial class Search : Page
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
                LoadResults();
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

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvResults.PageIndex = 0;
            LoadResults();
        }

        protected void gvResults_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvResults.PageIndex = e.NewPageIndex;
            LoadResults();
        }

        private void LoadResults()
        {
            DateTime from, to;
            if (!DateTime.TryParse(txtFrom.Text, out from)) from = DateTime.Today;
            if (!DateTime.TryParse(txtTo.Text, out to)) to = DateTime.Today;

            string partner = ddlChannel.SelectedValue;
            string status = ddlStatus.SelectedValue;
            string search = txtSearch.Text.Trim();

            DataTable dt = bll.SearchTransactions(from, to, partner, search);

            // Status (RECONCILED / FAILED / PENDING) is computed in the SP; filter on it.
            if (!string.IsNullOrEmpty(status))
            {
                DataTable filtered = dt.Clone();
                foreach (DataRow row in dt.Rows)
                {
                    string rowStatus = row.Table.Columns.Contains("Status") ? (row["Status"] ?? "").ToString() : "";
                    if (string.Equals(rowStatus, status, StringComparison.OrdinalIgnoreCase)) filtered.ImportRow(row);
                }
                dt = filtered;
            }

            gvResults.DataSource = dt;
            gvResults.DataBind();

            litCount.Text = dt.Rows.Count + " transaction(s)";
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
