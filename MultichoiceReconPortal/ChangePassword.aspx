<%@ Page Title="Change Password" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="MultichoiceReconPortal.ChangePassword" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head text-center">
        <h1>Change Password</h1>
        <p>Update the password you use to sign in to the portal.</p>
    </div>

    <div class="mc-card mc-formcard">
        <div class="card-body p-5 mc-form">
            <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="alert py-2">
                <asp:Label ID="lblMsg" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlForm" runat="server" DefaultButton="btnSave">
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
                <asp:Button ID="btnSave" runat="server" CssClass="btn btn-mc btn-lg w-100" Text="Update Password" OnClick="btnSave_Click" />
            </asp:Panel>
        </div>
    </div>
</asp:Content>
