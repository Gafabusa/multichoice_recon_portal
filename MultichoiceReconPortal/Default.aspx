<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MultichoiceReconPortal._Default" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Sign In &middot; MultiChoice Reconciliation Portal</title>
    <link href="<%: ResolveUrl("~/Content/bootstrap.min.css") %>" rel="stylesheet" />
    <style>
        :root {
            --mc-blue: #0033a1;
            --mc-blue-dark: #001f66;
            --mc-accent: #e2001a;
        }
        html, body { height: 100%; }
        body {
            margin: 0;
            font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            background: radial-gradient(circle at 20% 20%, #1a54c4 0%, rgba(26,84,196,0) 45%),
                        linear-gradient(135deg, var(--mc-blue-dark) 0%, var(--mc-blue) 55%, #0a2e8c 100%);
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .login-wrapper { width: 100%; max-width: 760px; padding: 24px; }
        .login-card {
            background: #fff;
            border-radius: 20px;
            padding: 60px 80px 52px;
            box-shadow: 0 30px 60px -15px rgba(0,0,0,.45);
            width: 100%;
        }
        .login-brand { text-align: center; margin-bottom: 10px; }
        .login-brand img { max-width: 190px; height: auto; border-radius: 12px; }
        .login-title {
            text-align: center; font-size: 1.7rem; font-weight: 700;
            color: var(--mc-blue-dark); margin: 22px 0 4px;
        }
        .login-sub { text-align: center; color: #7a869a; font-size: 1rem; margin-bottom: 30px; }
        .login-card .form-label { font-weight: 600; color: #33415c; font-size: .95rem; margin-bottom: 6px; }
        .login-card .form-control {
            padding: 1rem 1.2rem; border-radius: 12px; border: 1px solid #d7dde8; font-size: 1.1rem; width: 100%;
        }
        .login-card .form-control:focus {
            border-color: var(--mc-blue); box-shadow: 0 0 0 .2rem rgba(0,51,161,.15);
        }
        .login-card .mb-3 { margin-bottom: 1.15rem !important; }
        .login-btn {
            background: var(--mc-blue); border: none; padding: .9rem;
            border-radius: 12px; font-weight: 600; letter-spacing: .3px; font-size: 1.1rem; margin-top: 10px;
        }
        .login-btn:hover { background: var(--mc-blue-dark); }
        .login-foot { text-align: center; color: #9aa4b6; font-size: .82rem; margin: 28px 0 0; }
        .alert { border-radius: 10px; font-size: .92rem; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrapper">
            <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnLogin" CssClass="login-card">
                <div class="login-brand">
                    <img src="<%: ResolveUrl("~/Content/images/multichoicelogo.jpg") %>" alt="MultiChoice" />
                </div>
                <h1 class="login-title">Reconciliation Portal</h1>
                <p class="login-sub">Sign in with your email to continue</p>

                <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-danger py-2">
                    <asp:Label ID="lblError" runat="server" />
                </asp:Panel>
                <asp:Panel ID="pnlInfo" runat="server" Visible="false" CssClass="alert alert-success py-2">
                    <asp:Label ID="lblInfo" runat="server" />
                </asp:Panel>

                <div class="mb-3">
                    <label class="form-label" for="<%= txtEmail.ClientID %>">Email address</label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"
                        placeholder="you@pegasus.co.ug" autocomplete="username" />
                </div>
                <div class="mb-3">
                    <label class="form-label" for="<%= txtPassword.ClientID %>">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password"
                        placeholder="Enter your password" autocomplete="current-password" />
                </div>

                <asp:Button ID="btnLogin" runat="server" CssClass="btn btn-primary w-100 login-btn"
                    Text="Sign In" OnClick="btnLogin_Click" />

                <p class="login-foot">MultiChoice Reconciliation &copy; <%: DateTime.Now.Year %></p>
            </asp:Panel>
        </div>
    </form>
</body>
</html>
