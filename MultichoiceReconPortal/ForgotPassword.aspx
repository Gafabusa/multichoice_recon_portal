<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="MultichoiceReconPortal.ForgotPassword" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Forgot Password &middot; MultiChoice Reconciliation Portal</title>
    <link href="<%: ResolveUrl("~/Content/bootstrap.min.css") %>" rel="stylesheet" />
    <style>
        :root { --mc-blue: #10664a; --mc-blue-dark: #0b3d2e; --mc-accent: #e2001a; }
        html, body { height: 100%; }
        body {
            margin: 0; font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            background: radial-gradient(circle at 20% 20%, #1e8a63 0%, rgba(30,138,99,0) 45%),
                        linear-gradient(135deg, var(--mc-blue-dark) 0%, var(--mc-blue) 55%, #0a4f39 100%);
            display: flex; align-items: center; justify-content: center;
        }
        .login-wrapper { width: 520px; max-width: 100%; padding: 20px; }
        .login-card { background: #fff; border-radius: 20px; padding: 44px 44px 32px; box-shadow: 0 30px 60px -15px rgba(0,0,0,.45); width: 100%; }
        @media (max-width: 520px) { .login-card { padding: 30px 22px 24px; border-radius: 16px; } }
        .login-brand { text-align: center; margin-bottom: 10px; }
        .login-brand img { max-width: 190px; height: auto; border-radius: 12px; }
        .login-title { text-align: center; font-size: 1.6rem; font-weight: 700; color: var(--mc-blue-dark); margin: 20px 0 4px; }
        .login-sub { text-align: center; color: #7a869a; font-size: 1rem; margin-bottom: 26px; }
        .login-card .form-label { font-weight: 600; color: #33415c; font-size: .95rem; margin-bottom: 6px; }
        .login-card .form-control { padding: .85rem 1.1rem; border-radius: 12px; border: 1px solid #d7dde8; font-size: 1.05rem; width: 100%; min-height: 54px; }
        .login-card .form-control:focus { border-color: var(--mc-blue); box-shadow: 0 0 0 .2rem rgba(16,102,74,.15); }
        .login-card .mb-3 { margin-bottom: 1.15rem !important; }
        .login-btn { background: var(--mc-blue); border: none; padding: .9rem; border-radius: 12px; font-weight: 600; letter-spacing: .3px; font-size: 1.1rem; margin-top: 6px; }
        .login-btn:hover { background: var(--mc-blue-dark); }
        .login-links { text-align: center; margin-top: 18px; }
        .login-links a { color: var(--mc-blue); text-decoration: none; font-weight: 600; font-size: .92rem; }
        .login-links a:hover { text-decoration: underline; }
        .login-foot { text-align: center; color: #9aa4b6; font-size: .82rem; margin: 20px 0 0; }
        .alert { border-radius: 10px; font-size: .92rem; }
        .form-text { color: #7a869a; font-size: .82rem; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrapper">
            <div class="login-card">
                <div class="login-brand">
                    <img src="<%: ResolveUrl("~/Content/images/multichoicelogo.jpg") %>" alt="MultiChoice" />
                </div>
                <h1 class="login-title">Forgot Password</h1>
                <p class="login-sub">Reset your password with a code sent to your email.</p>

                <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="alert py-2 js-autohide">
                    <asp:Label ID="lblMsg" runat="server" />
                </asp:Panel>

                <%-- Step 1: request a code --%>
                <asp:Panel ID="pnlRequest" runat="server" DefaultButton="btnSendCode">
                    <div class="mb-3">
                        <label class="form-label">Email address</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="example@gmail.com" />
                    </div>
                    <asp:Button ID="btnSendCode" runat="server" CssClass="btn btn-primary w-100 login-btn" Text="Send code" OnClick="btnSendCode_Click" />
                </asp:Panel>

                <%-- Step 2: enter the code and the new password --%>
                <asp:Panel ID="pnlReset" runat="server" Visible="false" DefaultButton="btnReset">
                    <asp:HiddenField ID="hfEmail" runat="server" />
                    <div class="mb-3">
                        <label class="form-label">6-digit code</label>
                        <asp:TextBox ID="txtOtp" runat="server" CssClass="form-control" MaxLength="6" placeholder="Enter the code from your email" />
                        <div class="form-text">The code expires 5 minutes after it was sent.</div>
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
                    <asp:Button ID="btnReset" runat="server" CssClass="btn btn-primary w-100 login-btn" Text="Reset password" OnClick="btnReset_Click" />
                </asp:Panel>

                <div class="login-links">
                    <a href="<%: ResolveUrl("~/Default.aspx") %>">Back to sign in</a>
                </div>

                <p class="login-foot">MultiChoice Reconciliation &copy; <%: DateTime.Now.Year %></p>
            </div>
        </div>
    </form>
    <script>
        (function () {
            var alerts = document.querySelectorAll('.js-autohide');
            for (var i = 0; i < alerts.length; i++) {
                (function (a) { setTimeout(function () { a.style.display = 'none'; }, 10000); })(alerts[i]);
            }
        })();
    </script>
</body>
</html>
