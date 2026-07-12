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

            pnlAddPartnerBtn.Visible = user.CanManagePartners;

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

        protected void btnAddPartner_Click(object sender, EventArgs e)
        {
            PortalUser user = Session["User"] as PortalUser;
            if (user == null || !user.CanManagePartners)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            string code = txtPartnerCode.Text.Trim();
            string name = txtPartnerName.Text.Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            {
                ShowPartnerModalError("Please enter the partner code and name.");
                return;
            }

            int result;
            try
            {
                result = bll.AddPartner(code, name);
            }
            catch (Exception)
            {
                ShowPartnerModalError("We could not add the partner right now. Please try again.");
                return;
            }

            if (result == -1)
            {
                ShowPartnerModalError("A partner with that code already exists.");
                return;
            }

            txtPartnerCode.Text = "";
            txtPartnerName.Text = "";
            ShowPartnerMessage("Partner '" + name + "' added.", "alert-success");
            LoadDashboard();
        }

        private void ShowPartnerMessage(string message, string cssClass)
        {
            lblPartnerMsg.Text = message;
            pnlPartnerMsg.CssClass = "alert py-2 js-autohide " + cssClass;
            pnlPartnerMsg.Visible = true;
        }

        private void ShowPartnerModalError(string message)
        {
            lblPartnerModalMsg.Text = message;
            pnlPartnerModalMsg.CssClass = "alert alert-danger py-2 js-modal-alert";
            pnlPartnerModalMsg.Visible = true;
            ClientScript.RegisterStartupScript(GetType(), "reopenAddPartner",
                "var m=new bootstrap.Modal(document.getElementById('addPartnerModal'));m.show();", true);
        }

        private void LoadDashboard()
        {
            DateTime from, to;
            if (!DateTime.TryParse(txtFrom.Text, out from)) from = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            if (!DateTime.TryParse(txtTo.Text, out to)) to = DateTime.Today;

            string scope = bll.GetViewScopeCsv(Session["User"] as PortalUser);

            long total = 0, recon = 0, failed = 0, unrecon = 0, pending = 0;
            decimal totalAmt = 0, reconAmt = 0;
            DataTable stats = bll.GetDashboardStats(from, to, scope);
            if (stats.Rows.Count > 0)
            {
                DataRow r = stats.Rows[0];
                total = ToLong(r["TotalTxns"]);
                recon = ToLong(r["ReconciledCount"]);
                failed = ToLong(r["FailedCount"]);
                unrecon = ToLong(r["UnreconciledCount"]);
                pending = ToLong(r["PendingCount"]);
                totalAmt = ToDec(r["TotalAmount"]);
                reconAmt = ToDec(r["ReconciledAmount"]);
            }

            litTotal.Text = total.ToString("N0");
            litRecon.Text = recon.ToString("N0");
            litFailed.Text = failed.ToString("N0");
            litUnrecon.Text = unrecon.ToString("N0");
            litPending.Text = pending.ToString("N0");
            long processed = recon + failed + unrecon;
            litRate.Text = (processed > 0 ? (recon * 100m / processed) : 0m).ToString("0.#") + "%";

            PortalUser current = Session["User"] as PortalUser;
            bool isAdmin = current != null && current.IsAdmin;
            litTotalAmt.Text = isAdmin ? "UGX " + totalAmt.ToString("N0") : "";
            litReconAmt.Text = isAdmin ? "UGX " + reconAmt.ToString("N0") : "";

            DataTable trend = bll.GetDailyTrend(from, to, scope);
            DataTable channel = bll.GetByChannel(from, to, scope);

            string trendLabels = "''," + JsLabels(trend, "Day", true);
            string trendRecon = "0," + JsNumbers(trend, "Reconciled");
            string trendFailed = "0," + JsNumbers(trend, "Failed");
            string trendUnrecon = "0," + JsNumbers(trend, "Unreconciled");
            string chLabels = JsLabels(channel, "Partner", false);
            string chRecon = JsNumbers(channel, "Reconciled");
            string chFailed = JsNumbers(channel, "Failed");
            string chUnrecon = JsNumbers(channel, "Unreconciled");

            int realPartners = channel.Rows.Count;
            int channelSlots = realPartners < 8 ? 8 : realPartners;
            for (int i = realPartners; i < channelSlots; i++)
            {
                chLabels += (chLabels.Length > 0 ? "," : "") + "''";
                chRecon += (chRecon.Length > 0 ? "," : "") + "null";
                chFailed += (chFailed.Length > 0 ? "," : "") + "null";
                chUnrecon += (chUnrecon.Length > 0 ? "," : "") + "null";
            }

            StringBuilder s = new StringBuilder();
            s.Append("<script>(function(){");
            s.Append("if(typeof Chart==='undefined'){return;}");

            s.Append("new Chart(document.getElementById('chartStatus'),{type:'doughnut',data:{labels:['Reconciled','Failed','Unreconciled'],datasets:[{data:[")
             .Append(recon).Append(',').Append(failed).Append(',').Append(unrecon)
             .Append("],backgroundColor:['#10664a','#e2001a','#f0ad4e']}]},options:{responsive:true,maintainAspectRatio:false,plugins:{legend:{position:'bottom'}}}});");

            s.Append("new Chart(document.getElementById('chartTrend'),{type:'line',data:{labels:[").Append(trendLabels)
             .Append("],datasets:[{label:'Reconciled',data:[").Append(trendRecon)
             .Append("],borderColor:'#10664a',tension:.25,fill:false,borderWidth:3,pointRadius:0,pointHoverRadius:5},{label:'Failed',data:[").Append(trendFailed)
             .Append("],borderColor:'#e2001a',tension:.25,fill:false,borderWidth:3,pointRadius:0,pointHoverRadius:5},{label:'Unreconciled',data:[").Append(trendUnrecon)
             .Append("],borderColor:'#f0ad4e',tension:.25,fill:false,borderWidth:3,pointRadius:0,pointHoverRadius:5}]},")
             .Append("options:{responsive:true,maintainAspectRatio:false,interaction:{mode:'index',intersect:false},layout:{padding:{right:32,top:16}},plugins:{legend:{position:'bottom'},tooltip:{enabled:true}},scales:{x:{offset:false,grid:{display:false}},y:{beginAtZero:true,min:0,grace:'18%',ticks:{precision:0}}}}});");

            if (channelSlots > 14)
                s.Append("var chBox=document.getElementById('chartChannelBox');if(chBox){chBox.style.width='").Append(channelSlots * 110).Append("px';}");
            else
                s.Append("var chBox=document.getElementById('chartChannelBox');if(chBox){chBox.style.width='100%';}");
            s.Append("new Chart(document.getElementById('chartChannel'),{type:'bar',data:{labels:[").Append(chLabels)
             .Append("],datasets:[{label:'Reconciled',data:[").Append(chRecon)
             .Append("],backgroundColor:'#10664a',barThickness:16},{label:'Failed',data:[").Append(chFailed)
             .Append("],backgroundColor:'#e2001a',barThickness:16},{label:'Unreconciled',data:[").Append(chUnrecon)
             .Append("],backgroundColor:'#f0ad4e',barThickness:16}]},")
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
