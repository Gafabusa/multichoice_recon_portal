<%@ Page Title="Search" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="MultichoiceReconPortal.Search" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head">
        <h1>Search Transactions</h1>
        <p>Find any transaction and see exactly what happened during reconciliation.</p>
    </div>

    <div class="mc-card mb-4">
        <div class="card-body">
            <asp:Panel ID="pnlFilter" runat="server" DefaultButton="btnSearch" CssClass="row g-3 align-items-end">
                <div class="col-md-2">
                    <label class="form-label small mb-1">From</label>
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-2">
                    <label class="form-label small mb-1">To</label>
                    <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-2">
                    <label class="form-label small mb-1">Partner</label>
                    <asp:DropDownList ID="ddlChannel" runat="server" CssClass="form-select" />
                </div>
                <div class="col-md-2">
                    <label class="form-label small mb-1">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Value="">All</asp:ListItem>
                        <asp:ListItem Value="RECONCILED">Reconciled</asp:ListItem>
                        <asp:ListItem Value="FAILED">Failed</asp:ListItem>
                        <asp:ListItem Value="UNRECONCILED">Unreconciled</asp:ListItem>
                        <asp:ListItem Value="PENDING">Pending</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label class="form-label small mb-1">Reference / Smart card</label>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" placeholder="optional" />
                </div>
                <div class="col-md-2">
                    <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-mc w-100" Text="Search" OnClick="btnSearch_Click" />
                </div>
            </asp:Panel>
        </div>
    </div>

    <div class="mc-card">
        <div class="card-header py-3 d-flex justify-content-between align-items-center">
            <span>Results</span>
            <span class="text-muted small"><asp:Literal ID="litCount" runat="server" /></span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="gvResults" runat="server" CssClass="table table-hover align-middle mb-0"
                    AutoGenerateColumns="false" GridLines="None" AllowPaging="true" PageSize="25"
                    OnPageIndexChanging="gvResults_PageIndexChanging">
                    <Columns>
                        <asp:TemplateField HeaderText="Date">
                            <ItemTemplate>
                                <%# Eval("ConnectionDate") == null || Eval("ConnectionDate") == System.DBNull.Value ? "" : Convert.ToDateTime(Eval("ConnectionDate")).ToString("dd MMM yyyy") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Partner" HeaderText="Partner" />
                        <asp:BoundField DataField="TransactionId" HeaderText="Reference" />
                        <asp:BoundField DataField="SmartCardNumber" HeaderText="Smart card" />
                        <asp:BoundField DataField="CustomerName" HeaderText="Customer" />
                        <asp:BoundField DataField="Package" HeaderText="Package" />
                        <asp:BoundField DataField="Amount" HeaderText="Amount" DataFormatString="{0:N0}" ItemStyle-CssClass="text-end" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='badge <%# StatusClass(Eval("Status")) %>'><%# Eval("Status") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ReconciledBy" HeaderText="Reconciled by" />
                        <asp:BoundField DataField="Reason" HeaderText="Reason" />
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="text-center text-muted py-4">No transactions match your filters.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
