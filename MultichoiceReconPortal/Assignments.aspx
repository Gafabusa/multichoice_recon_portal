<%@ Page Title="Partner Assignments" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Assignments.aspx.cs" Inherits="MultichoiceReconPortal.Assignments" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head">
        <h1>Partner Assignments</h1>
        <p>Assign partners to accountants (and to yourself). A user can only upload, view and report on the partners assigned to them.</p>
    </div>

    <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Label ID="lblMsg" runat="server" />
    </asp:Panel>

    <div class="mc-card">
        <div class="card-body">
            <div class="row g-3 align-items-end mb-3">
                <div class="col-md-5">
                    <label class="form-label small mb-1">User</label>
                    <asp:DropDownList ID="ddlUser" runat="server" CssClass="form-select" AutoPostBack="true"
                        OnSelectedIndexChanged="ddlUser_SelectedIndexChanged" />
                </div>
            </div>

            <asp:Panel ID="pnlPartners" runat="server" Visible="false">
                <label class="form-label small mb-2">Partners</label>
                <asp:CheckBoxList ID="cblPartners" runat="server" CssClass="mc-checkgrid"
                    RepeatColumns="3" RepeatDirection="Horizontal" DataTextField="PartnerName" DataValueField="PartnerId" />
                <div class="mt-3">
                    <asp:Button ID="btnSave" runat="server" CssClass="btn btn-mc" Text="Save assignments" OnClick="btnSave_Click" />
                </div>
            </asp:Panel>
        </div>
    </div>
</asp:Content>
