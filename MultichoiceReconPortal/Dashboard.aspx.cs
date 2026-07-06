using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;

namespace MultichoiceReconPortal
{
    public partial class Dashboard : Page
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
                txtFrom.Text = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString("yyyy-MM-dd");
                txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
                LoadDashboard();
            }
        }

        protected void btnApply_Click(object sender, EventArgs e)
        {
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            DateTime from, to;
            if (!DateTime.TryParse(txtFrom.Text, out from)) from = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            if (!DateTime.TryParse(txtTo.Text, out to)) to = DateTime.Today;

            // KPIs. A transaction is either RECONCILED (Reconciled = 1) or FAILED -
            // there is no pending bucket, so failed = total - reconciled.
            long total = 0, recon = 0, failed = 0;
            decimal totalAmt = 0, reconAmt = 0, failedAmt = 0;
            DataTable stats = bll.GetDashboardStats(from, to);
            if (stats.Rows.Count > 0)
            {
                DataRow r = stats.Rows[0];
                total = ToLong(r["TotalTxns"]);
                recon = ToLong(r["ReconciledCount"]);
                failed = total - recon;
                totalAmt = ToDec(r["TotalAmount"]);
                reconAmt = ToDec(r["ReconciledAmount"]);
                failedAmt = totalAmt - reconAmt;
            }

            litTotal.Text = total.ToString("N0");
            litRecon.Text = recon.ToString("N0");
            litFailed.Text = failed.ToString("N0");
            litRate.Text = (total > 0 ? (recon * 100m / total) : 0m).ToString("0.#") + "%";

            // Only admins see the money figures; uploaders see counts only.
            PortalUser current = Session["User"] as PortalUser;
            bool isAdmin = current != null && current.IsAdmin;
            litTotalAmt.Text = isAdmin ? "UGX " + totalAmt.ToString("N0") : "";
            litReconAmt.Text = isAdmin ? "UGX " + reconAmt.ToString("N0") : "";
            litFailedAmt.Text = isAdmin ? "UGX " + failedAmt.ToString("N0") : "";

            // charts
            DataTable trend = bll.GetDailyTrend(from, to);
            DataTable channel = bll.GetByChannel(from, to);

            // Anchor the trend at zero on the left so the line always starts from 0
            // and then rises/falls to each day's actual count (never a lone dot).
            string trendLabels = "''," + JsLabels(trend, "Day", true);
            string trendRecon = "0," + JsNumbers(trend, "Reconciled");
            string trendFailed = "0," + JsNumbers(trend, "Failed");
            string chLabels = JsLabels(channel, "Bank", false);
            string chRecon = JsNumbers(channel, "Reconciled");
            string chFailed = JsNumbers(channel, "Failed");

            // Charts share these greens; maintainAspectRatio:false lets the fixed-height
            // .mc-chartbox control the height so everything fits without scrolling.
            StringBuilder s = new StringBuilder();
            s.Append("<script>(function(){");
            s.Append("if(typeof Chart==='undefined'){return;}");
            s.Append("var base={responsive:true,maintainAspectRatio:false,plugins:{legend:{position:'bottom'}}};");

            s.Append("new Chart(document.getElementById('chartStatus'),{type:'doughnut',data:{labels:['Reconciled','Failed'],datasets:[{data:[")
             .Append(recon).Append(',').Append(failed)
             .Append("],backgroundColor:['#10664a','#e2001a']}]},options:base});");

            // Daily trend: per-day counts for the selected range as a clean line
            // (no area fill, no dots), rising or falling with the actual numbers.
            // The y-axis is pinned to 0 and the series is anchored at 0 on the left.
            s.Append("new Chart(document.getElementById('chartTrend'),{type:'line',data:{labels:[").Append(trendLabels)
             .Append("],datasets:[{label:'Reconciled',data:[").Append(trendRecon)
             .Append("],borderColor:'#10664a',tension:.25,fill:false,borderWidth:3,pointRadius:0,pointHoverRadius:5},{label:'Failed',data:[").Append(trendFailed)
             .Append("],borderColor:'#e2001a',tension:.25,fill:false,borderWidth:3,pointRadius:0,pointHoverRadius:5}]},")
             .Append("options:{responsive:true,maintainAspectRatio:false,plugins:{legend:{position:'bottom'}},scales:{y:{beginAtZero:true,min:0,ticks:{precision:0}}}}});");

            // By channel: grouped bars, side by side - a green Reconciled bar then a
            // red Failed bar under each channel label, both growing from zero. A small
            // categoryPercentage keeps the pair together and the bars slim (not huge)
            // even when there is only one channel. NOT stacked - the datasets are not
            // stacked on the x/y scales, so they sit next to each other.
            s.Append("new Chart(document.getElementById('chartChannel'),{type:'bar',data:{labels:[").Append(chLabels)
             .Append("],datasets:[{label:'Reconciled',data:[").Append(chRecon)
             .Append("],backgroundColor:'#10664a',categoryPercentage:.35,barPercentage:.7},{label:'Failed',data:[").Append(chFailed)
             .Append("],backgroundColor:'#e2001a',categoryPercentage:.35,barPercentage:.7}]},")
             .Append("options:{responsive:true,maintainAspectRatio:false,plugins:{legend:{position:'bottom'}},scales:{x:{grid:{display:false},ticks:{autoSkip:false,maxRotation:0,minRotation:0}},y:{beginAtZero:true,min:0,ticks:{precision:0}}}}});");
            s.Append("})();</script>");
            litChartScript.Text = s.ToString();
        }

        private static long ToLong(object o)
        {
            return o == null || o == DBNull.Value ? 0 : Convert.ToInt64(o);
        }

        private static decimal ToDec(object o)
        {
            return o == null || o == DBNull.Value ? 0 : Convert.ToDecimal(o);
        }

        private static string JsNumbers(DataTable dt, string col)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(ToLong(dt.Rows[i][col]));
            }
            return sb.ToString();
        }

        private static string JsLabels(DataTable dt, string col, bool isDate)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i > 0) sb.Append(',');
                string label;
                if (isDate && dt.Rows[i][col] != DBNull.Value)
                {
                    label = Convert.ToDateTime(dt.Rows[i][col]).ToString("dd MMM", CultureInfo.InvariantCulture);
                }
                else
                {
                    label = dt.Rows[i][col] == DBNull.Value ? "" : dt.Rows[i][col].ToString();
                }
                sb.Append('\'').Append(label.Replace("'", "")).Append('\'');
            }
            return sb.ToString();
        }
    }
}
