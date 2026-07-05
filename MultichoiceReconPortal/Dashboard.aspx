<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="MultichoiceReconPortal.Dashboard" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head d-flex flex-wrap justify-content-between align-items-end gap-3">
        <div>
            <h1>Reconciliation Dashboard</h1>
            <p>Reconciliation performance for the selected period.</p>
        </div>
        <div class="d-flex align-items-end gap-2">
            <div>
                <label class="form-label small mb-1">From</label>
                <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control form-control-sm" TextMode="Date" />
            </div>
            <div>
                <label class="form-label small mb-1">To</label>
                <asp:TextBox ID="txtTo" runat="server" CssClass="form-control form-control-sm" TextMode="Date" />
            </div>
            <asp:Button ID="btnApply" runat="server" CssClass="btn btn-mc btn-sm" Text="Apply" OnClick="btnApply_Click" />
        </div>
    </div>

    <!-- KPI cards -->
    <div class="row g-3 mb-4">
        <div class="col-md-3 col-sm-6">
            <div class="mc-stat" style="background:linear-gradient(135deg,#0033a1,#0a2e8c)">
                <div class="label">Total transactions</div>
                <div class="value"><asp:Literal ID="litTotal" runat="server" Text="0" /></div>
                <div class="sub"><asp:Literal ID="litTotalAmt" runat="server" /></div>
            </div>
        </div>
        <div class="col-md-3 col-sm-6">
            <div class="mc-stat" style="background:linear-gradient(135deg,#137a48,#1e9e64)">
                <div class="label">Reconciled</div>
                <div class="value"><asp:Literal ID="litRecon" runat="server" Text="0" /></div>
                <div class="sub"><asp:Literal ID="litReconAmt" runat="server" /></div>
            </div>
        </div>
        <div class="col-md-3 col-sm-6">
            <div class="mc-stat" style="background:linear-gradient(135deg,#a3121f,#e2001a)">
                <div class="label">Failed</div>
                <div class="value"><asp:Literal ID="litFailed" runat="server" Text="0" /></div>
                <div class="sub"><asp:Literal ID="litFailedAmt" runat="server" /></div>
            </div>
        </div>
        <div class="col-md-3 col-sm-6">
            <div class="mc-stat" style="background:linear-gradient(135deg,#5b21b6,#7c3aed)">
                <div class="label">Match rate</div>
                <div class="value"><asp:Literal ID="litRate" runat="server" Text="0%" /></div>
                <div class="sub"><asp:Literal ID="litPending" runat="server" Text="0" /> pending</div>
            </div>
        </div>
    </div>

    <!-- charts -->
    <div class="row g-4">
        <div class="col-lg-4">
            <div class="mc-card h-100">
                <div class="card-header py-3">Reconciliation status</div>
                <div class="card-body"><canvas id="chartStatus" height="240"></canvas></div>
            </div>
        </div>
        <div class="col-lg-8">
            <div class="mc-card h-100">
                <div class="card-header py-3">Daily trend</div>
                <div class="card-body"><canvas id="chartTrend" height="240"></canvas></div>
            </div>
        </div>
        <div class="col-lg-8">
            <div class="mc-card h-100">
                <div class="card-header py-3">By channel</div>
                <div class="card-body"><canvas id="chartChannel" height="240"></canvas></div>
            </div>
        </div>
        <div class="col-lg-4">
            <div class="mc-card h-100">
                <div class="card-header py-3">Top failure reasons</div>
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <asp:GridView ID="gvReasons" runat="server" CssClass="table table-sm align-middle mb-0"
                            AutoGenerateColumns="false" GridLines="None">
                            <Columns>
                                <asp:BoundField DataField="Reason" HeaderText="Reason" />
                                <asp:BoundField DataField="Cnt" HeaderText="Count" ItemStyle-CssClass="text-end" />
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="text-center text-muted py-4">No failures in this period.</div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%: ResolveUrl("~/Scripts/chart.min.js") %>"></script>
    <asp:Literal ID="litChartScript" runat="server" />
</asp:Content>
