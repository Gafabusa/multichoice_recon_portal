using ClassLibrary.ControlObjects;
using ClassLibrary.EntityObjects;
using System;
using System.Web.UI;

namespace MultichoiceReconPortal
{
    public partial class Upload : Page
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
                BindChannels();
                BindRecent();
            }
        }

        private void BindChannels()
        {
            ddlChannel.DataSource = bll.GetChannels();
            ddlChannel.DataBind();
        }

        private void BindRecent()
        {
            gvRecent.DataSource = bll.GetRecentUploads(20);
            gvRecent.DataBind();
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            PortalUser user = Session["User"] as PortalUser;
            if (user == null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }

            if (!fuStatement.HasFile)
            {
                ShowMessage("Please choose a statement file to upload.", "alert-danger");
                return;
            }

            string channel = ddlChannel.SelectedValue;
            if (string.IsNullOrEmpty(channel))
            {
                ShowMessage("Please select a channel.", "alert-danger");
                return;
            }

            UploadResult result;
            try
            {
                result = bll.SaveUpload(channel, fuStatement.FileName, fuStatement.FileBytes, user.FullName, user.Email);
            }
            catch (Exception ex)
            {
                ShowMessage("Upload failed: " + ex.Message, "alert-danger");
                return;
            }

            ShowMessage(result.Message, result.Success ? "alert-success" : "alert-danger");
            if (result.Success)
            {
                BindRecent();
            }
        }

        /// <summary>Bootstrap badge class for a statement status (used by the grid).</summary>
        protected string StatusClass(object status)
        {
            string s = (status ?? "").ToString().ToUpper();
            if (s.Contains("FAIL") || s.Contains("ERROR")) return "bg-danger";
            if (s.Contains("PEND")) return "bg-warning text-dark";
            if (s.Contains("PROCESS") || s.Contains("DONE") || s.Contains("COMPLETE")) return "bg-success";
            return "bg-secondary";
        }

        private void ShowMessage(string message, string cssClass)
        {
            lblMsg.Text = message;
            pnlMsg.CssClass = "alert py-2 " + cssClass;
            pnlMsg.Visible = true;
        }
    }
}
