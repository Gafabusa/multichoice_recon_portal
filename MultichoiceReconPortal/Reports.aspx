<%@ Page Title="Reports" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="MultichoiceReconPortal.Reports" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head">
        <h1>Reconciliation Reports</h1>
        <p>Daily reconciliation results with downloadable SUCCESS and FAILED reports.</p>
    </div>

    <div class="mc-card mb-4">
        <div class="card-body">
            <asp:Panel ID="pnlFilter" runat="server" DefaultButton="btnRun" CssClass="row g-3 align-items-end">
                <div class="col-md-2">
                    <label class="form-label small mb-1">From</label>
                    <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-2">
                    <label class="form-label small mb-1">To</label>
                    <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-3">
                    <label class="form-label small mb-1">Channel</label>
                    <asp:DropDownList ID="ddlChannel" runat="server" CssClass="form-select" />
                </div>
                <div class="col-md-2">
                    <asp:Button ID="btnRun" runat="server" CssClass="btn btn-mc w-100" Text="Run report" OnClick="btnRun_Click" />
                </div>
                <div class="col-md-3 text-md-end">
                    <asp:Button ID="btnExportSuccess" runat="server" CssClass="btn btn-outline-success btn-sm" Text="SUCCESS.csv" OnClick="btnExportSuccess_Click" />
                    <asp:Button ID="btnExportFailed" runat="server" CssClass="btn btn-outline-danger btn-sm" Text="FAILED.csv" OnClick="btnExportFailed_Click" />
                    <asp:Button ID="btnExportUnrecon" runat="server" CssClass="btn btn-outline-warning btn-sm" Text="UNRECONCILED.csv" OnClick="btnExportUnrecon_Click" />
                </div>
            </asp:Panel>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-md-3 col-6">
            <div class="mc-stat">
                <div class="label">Total</div>
                <div class="value"><asp:Literal ID="litTotal" runat="server" Text="0" /></div>
            </div>
        </div>
        <div class="col-md-3 col-6">
            <div class="mc-stat">
                <div class="label">Reconciled</div>
                <div class="value"><asp:Literal ID="litRecon" runat="server" Text="0" /></div>
            </div>
        </div>
        <div class="col-md-3 col-6">
            <div class="mc-stat">
                <div class="label">Failed</div>
                <div class="value"><asp:Literal ID="litFailed" runat="server" Text="0" /></div>
            </div>
        </div>
        <div class="col-md-3 col-6">
            <div class="mc-stat">
                <div class="label">Unreconciled</div>
                <div class="value"><asp:Literal ID="litUnrecon" runat="server" Text="0" /></div>
                <div class="sub"><asp:Literal ID="litRate" runat="server" Text="0%" /> match rate</div>
            </div>
        </div>
    </div>

    <div class="mc-card">
        <div class="card-header py-3">Transactions</div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="gvReport" runat="server" CssClass="table table-hover align-middle mb-0"
                    AutoGenerateColumns="false" GridLines="None" AllowPaging="true" PageSize="25"
                    OnPageIndexChanging="gvReport_PageIndexChanging">
                    <Columns>
                        <asp:TemplateField HeaderText="Date">
                            <ItemTemplate>
                                <%# Eval("ConnectionDate") == null || Eval("ConnectionDate") == System.DBNull.Value ? "" : Convert.ToDateTime(Eval("ConnectionDate")).ToString("dd MMM yyyy") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Partner" HeaderText="Partner" />
                        <asp:BoundField DataField="PartnerTxnRef" HeaderText="Reference" />
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
                        <div class="text-center text-muted py-4">No transactions for this period.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
