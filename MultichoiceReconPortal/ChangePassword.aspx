<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="MultichoiceReconPortal.ChangePassword" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Change Password &middot; MultiChoice Reconciliation Portal</title>
    <link href="<%: ResolveUrl("~/Content/bootstrap.min.css") %>" rel="stylesheet" />
    <style>
        :root { --mc-blue: #0033a1; --mc-blue-dark: #001f66; }
        html, body { height: 100%; }
        body {
            margin: 0; font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            background: radial-gradient(circle at 20% 20%, #1a54c4 0%, rgba(26,84,196,0) 45%),
                        linear-gradient(135deg, var(--mc-blue-dark) 0%, var(--mc-blue) 55%, #0a2e8c 100%);
            display: flex; align-items: center; justify-content: center;
        }
        .login-wrapper { width: 100%; max-width: 720px; padding: 24px; }
        .login-card { background: #fff; border-radius: 20px; padding: 56px 72px 44px; box-shadow: 0 30px 60px -15px rgba(0,0,0,.45); width: 100%; }
        .login-brand { text-align: center; margin-bottom: 8px; }
        .login-brand img { max-width: 160px; height: auto; border-radius: 12px; }
        .login-title { text-align: center; font-size: 1.6rem; font-weight: 700; color: var(--mc-blue-dark); margin: 18px 0 4px; }
        .login-sub { text-align: center; color: #7a869a; font-size: 1rem; margin-bottom: 28px; }
        .login-card .form-label { font-weight: 600; color: #33415c; font-size: .95rem; margin-bottom: 6px; }
        .login-card .form-control { padding: 1rem 1.2rem; border-radius: 12px; border: 1px solid #d7dde8; font-size: 1.1rem; width: 100%; }
        .login-card .form-control:focus { border-color: var(--mc-blue); box-shadow: 0 0 0 .2rem rgba(0,51,161,.15); }
        .login-card .mb-3 { margin-bottom: 1.15rem !important; }
        .login-btn { background: var(--mc-blue); border: none; padding: .9rem; border-radius: 12px; font-weight: 600; font-size: 1.1rem; margin-top: 10px; }
        .login-btn:hover { background: var(--mc-blue-dark); }
        .alert { border-radius: 10px; font-size: .95rem; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrapper">
            <asp:Panel ID="pnlForm" runat="server" DefaultButton="btnSave" CssClass="login-card">
                <div class="login-brand">
                    <img src="<%: ResolveUrl("~/Content/images/multichoicelogo.jpg") %>" alt="MultiChoice" />
                </div>
                <h1 class="login-title">Change Password</h1>
                <p class="login-sub">Set a new password to continue.</p>

                <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="alert py-2">
                    <asp:Label ID="lblMsg" runat="server" />
                </asp:Panel>

                <div class="mb-3">
                    <label class="form-label">Current password</label>
                    <asp:TextBox ID="txtCurrent" runat="server" CssClass="form-control" TextMode="Password" />
                </div>
                <div class="mb-3">
                    <label class="form-label">New password</label>
                    <asp:TextBox ID="txtNew" runat="server" CssClass="form-control" TextMode="Password" />
                    <div class="form-text">At least 6 characters.</div>
                </div>
                <div class="mb-3">
                    <label class="form-label">Confirm new password</label>
                    <asp:TextBox ID="txtConfirm" runat="server" CssClass="form-control" TextMode="Password" />
                </div>
                <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary w-100 login-btn" Text="Update Password" OnClick="btnSave_Click" />
            </asp:Panel>
        </div>
    </form>
</body>
</html>
