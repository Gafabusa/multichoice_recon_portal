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
            if (!user.CanUpload)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            pnlMultichoiceBtn.Visible = user.CanUploadMultichoice;

            if (!IsPostBack)
            {
                BindChannels();
                BindRecent();
            }
        }

        private void BindChannels()
        {
            // Uploaders (Head Accounts and Accountants) upload only for the partners
            // assigned to them.
            PortalUser user = Session["User"] as PortalUser;
            ddlChannel.DataSource = bll.GetAssignedPartnerCodes(user.UserId);
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
            if (!user.CanUpload)
            {
                Response.Redirect("~/Dashboard.aspx");
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

            // Guard: don't accept a partner statement until MultiChoice's own
            // records for that partner have been uploaded (nothing to reconcile against).
            if (!bll.HasMultichoiceRecordsForPartner(channel))
            {
                ShowModalError("This file was not uploaded. There are no MultiChoice records for " + channel +
                    " yet. Please ask Head Accounts to upload the MultiChoice records for " + channel +
                    " first, then try again.");
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
                bll.LogAudit(user, "Upload statement", channel + ": " + fuStatement.FileName);
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

        protected void btnUploadMc_Click(object sender, EventArgs e)
        {
            PortalUser user = Session["User"] as PortalUser;
            if (user == null)
            {
                Response.Redirect("~/Default.aspx");
                return;
            }
            if (!user.CanUploadMultichoice)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            if (!fuMultichoice.HasFile)
            {
                ShowMcModalError("Please choose a MultiChoice records file to upload.");
                return;
            }

            UploadResult result;
            try
            {
                result = bll.SaveMultichoiceUpload(fuMultichoice.FileName, fuMultichoice.FileBytes, user.FullName, user.Email);
            }
            catch (Exception ex)
            {
                ShowMcModalError("Upload failed: " + ex.Message);
                return;
            }

            if (result.Success)
            {
                bll.LogAudit(user, "Upload MultiChoice records", fuMultichoice.FileName);
                ShowPageMessage(result.Message, "alert-success");
            }
            else
            {
                ShowMcModalError(result.Message);
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

        private void ShowMcModalError(string message)
        {
            lblMcModalMsg.Text = message;
            pnlMcModalMsg.CssClass = "alert alert-danger py-2 js-modal-alert";
            pnlMcModalMsg.Visible = true;
            ClientScript.RegisterStartupScript(GetType(), "reopenMc",
                "var m=new bootstrap.Modal(document.getElementById('multichoiceModal'));m.show();", true);
        }
    }
}
