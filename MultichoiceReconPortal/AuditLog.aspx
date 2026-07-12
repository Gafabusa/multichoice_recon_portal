<%@ Page Title="Audit Log" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AuditLog.aspx.cs" Inherits="MultichoiceReconPortal.AuditLog" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head">
        <h1>Audit Log</h1>
        <p>Who did what and when: logins, logouts, uploads and report downloads.</p>
    </div>

    <div class="mc-card mb-4">
        <div class="card-body">
            <asp:Panel ID="pnlFilter" runat="server" DefaultButton="btnRun" CssClass="row g-3 align-items-end">
                <div class="col-md-3">
                    <label class="form-label small mb-1">From</label>
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-3">
                    <label class="form-label small mb-1">To</label>
                    <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-2">
                    <asp:Button ID="btnRun" runat="server" CssClass="btn btn-mc w-100" Text="Apply" OnClick="btnRun_Click" />
                </div>
            </asp:Panel>
        </div>
    </div>

    <div class="mc-card">
        <div class="card-header py-3">Activity</div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="gvAudit" runat="server" CssClass="table table-hover align-middle mb-0"
                    AutoGenerateColumns="false" GridLines="None" AllowPaging="true" PageSize="30"
                    OnPageIndexChanging="gvAudit_PageIndexChanging">
                    <Columns>
                        <asp:TemplateField HeaderText="When">
                            <ItemTemplate>
                                <%# Eval("LoggedAt") == null || Eval("LoggedAt") == System.DBNull.Value ? "" : Convert.ToDateTime(Eval("LoggedAt")).ToString("dd MMM yyyy HH:mm:ss") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="UserName" HeaderText="User" />
                        <asp:BoundField DataField="Action" HeaderText="Action" />
                        <asp:BoundField DataField="Details" HeaderText="Details" />
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="text-center text-muted py-4">No activity for this period.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
