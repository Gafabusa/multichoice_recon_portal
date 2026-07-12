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

            byte[] bytes = fuStatement.FileBytes;

            int totalRefs;
            System.Collections.Generic.List<string> alreadyReconciled = bll.FindAlreadyReconciled(channel, bytes, out totalRefs);

            if (alreadyReconciled.Count > 0 && alreadyReconciled.Count >= totalRefs)
            {
                ShowModalError("This file was not uploaded. All " + alreadyReconciled.Count +
                    " transaction(s) in it were already reconciled.");
                return;
            }

            UploadResult result;
            try
            {
                result = bll.SaveUpload(channel, fuStatement.FileName, bytes, user.FullName, user.Email);
            }
            catch (Exception ex)
            {
                ShowModalError("Upload failed: " + ex.Message);
                return;
            }

            if (result.Success)
            {
                if (alreadyReconciled.Count > 0)
                {
                    int sentForRecon = totalRefs - alreadyReconciled.Count;
                    ShowPageMessage(result.Message + " Note: " + alreadyReconciled.Count +
                        " transaction(s) were already reconciled and will be skipped; " + sentForRecon +
                        " transaction(s) have been sent for reconciliation.", "alert-warning");
                }
                else
                {
                    ShowPageMessage(result.Message, "alert-success");
                }
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
            pnlMsg.CssClass = "alert py-2 js-autohide " + cssClass;
            pnlMsg.Visible = true;
        }

        private void ShowModalError(string message)
        {
            lblModalMsg.Text = message;
            pnlModalMsg.CssClass = "alert alert-danger py-2 js-modal-alert";
            pnlModalMsg.Visible = true;
            ClientScript.RegisterStartupScript(GetType(), "reopenUpload",
                "var m=new bootstrap.Modal(document.getElementById('uploadModal'));m.show();", true);
        }
    }
}
