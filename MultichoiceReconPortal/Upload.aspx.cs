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
                ShowModalError("Please choose a statement file to upload.");
                return;
            }

            string channel = ddlChannel.SelectedValue;
            if (string.IsNullOrEmpty(channel))
            {
                ShowModalError("Please select a channel.");
                return;
            }

            UploadResult result;
            try
            {
                result = bll.SaveUpload(channel, fuStatement.FileName, fuStatement.FileBytes, user.FullName, user.Email);
            }
            catch (Exception ex)
            {
                ShowModalError("Upload failed: " + ex.Message);
                return;
            }

            if (result.Success)
            {
                ShowPageMessage(result.Message, "alert-success");
                BindRecent();
            }
            else
            {
                ShowModalError(result.Message);
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

        private void ShowPageMessage(string message, string cssClass)
        {
            lblMsg.Text = message;
            pnlMsg.CssClass = "alert py-2 " + cssClass;
            pnlMsg.Visible = true;
        }

        private void ShowModalError(string message)
        {
            lblModalMsg.Text = message;
            pnlModalMsg.Visible = true;
            ClientScript.RegisterStartupScript(GetType(), "reopenUpload",
                "var m=new bootstrap.Modal(document.getElementById('uploadModal'));m.show();", true);
        }
    }
}
