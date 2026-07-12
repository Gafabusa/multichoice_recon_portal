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
            <asp:Panel ID="pnlAddPartnerBtn" runat="server" Visible="false">
                <button type="button" class="btn btn-outline-secondary btn-sm" onclick="mcOpenModal('addPartnerModal')">+ Add Partner</button>
            </asp:Panel>
        </div>
    </div>

    <asp:Panel ID="pnlPartnerMsg" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Label ID="lblPartnerMsg" runat="server" />
    </asp:Panel>

    <div class="row g-3 mb-4">
        <div class="col-md-3 col-sm-6">
            <div class="mc-stat">
                <div class="label">Total transactions</div>
                <div class="value"><asp:Literal ID="litTotal" runat="server" Text="0" /></div>
                <div class="sub"><asp:Literal ID="litTotalAmt" runat="server" /></div>
                <div class="sub"><asp:Literal ID="litPending" runat="server" Text="0" /> pending recon</div>
            </div>
        </div>
        <div class="col-md-3 col-sm-6">
            <div class="mc-stat">
                <div class="label">Reconciled</div>
                <div class="value"><asp:Literal ID="litRecon" runat="server" Text="0" /></div>
                <div class="sub"><asp:Literal ID="litReconAmt" runat="server" /></div>
                <div class="sub"><asp:Literal ID="litRate" runat="server" Text="0%" /> match rate</div>
            </div>
        </div>
        <div class="col-md-3 col-sm-6">
            <div class="mc-stat">
                <div class="label">Failed</div>
                <div class="value"><asp:Literal ID="litFailed" runat="server" Text="0" /></div>
            </div>
        </div>
        <div class="col-md-3 col-sm-6">
            <div class="mc-stat">
                <div class="label">Unreconciled</div>
                <div class="value"><asp:Literal ID="litUnrecon" runat="server" Text="0" /></div>
            </div>
        </div>
    </div>

    <!-- charts -->
    <div class="row g-3">
        <div class="col-lg-4">
            <div class="mc-card h-100">
                <div class="card-header py-2">Reconciliation status</div>
                <div class="card-body"><div class="mc-chartbox"><canvas id="chartStatus"></canvas></div></div>
            </div>
        </div>
        <div class="col-lg-8">
            <div class="mc-card h-100">
                <div class="card-header py-2">Daily trend</div>
                <div class="card-body"><div class="mc-chartbox"><canvas id="chartTrend"></canvas></div></div>
            </div>
        </div>
        <div class="col-lg-12">
            <div class="mc-card">
                <div class="card-header py-2">By channel</div>
                <div class="card-body">
                    <div style="overflow-x:auto;">
                        <div id="chartChannelBox" class="mc-chartbox tall"><canvas id="chartChannel"></canvas></div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Add Partner modal (System Admin) -->
    <div class="modal fade" id="addPartnerModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <asp:Panel ID="pnlPartnerForm" runat="server" DefaultButton="btnAddPartner">
                    <div class="modal-header">
                        <h5 class="modal-title">Add a partner</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body p-4 mc-form">
                        <asp:Panel ID="pnlPartnerModalMsg" runat="server" Visible="false" CssClass="alert alert-danger py-2 js-modal-alert">
                            <asp:Label ID="lblPartnerModalMsg" runat="server" />
                        </asp:Panel>
                        <div class="mb-3">
                            <label class="form-label">Partner code</label>
                            <asp:TextBox ID="txtPartnerCode" runat="server" CssClass="form-control" MaxLength="30" />
                            <div class="text-muted small mt-1">Short code used on files and folders (e.g. MTN, AIRTEL, STANBIC).</div>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Partner name</label>
                            <asp:TextBox ID="txtPartnerName" runat="server" CssClass="form-control" MaxLength="100" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-outline-secondary btn-lg" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnAddPartner" runat="server" CssClass="btn btn-mc btn-lg" Text="Add Partner" OnClick="btnAddPartner_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ScriptsContent" runat="server">
    <script src="<%: ResolveUrl("~/Scripts/chart.min.js") %>"></script>
    <asp:Literal ID="litChartScript" runat="server" />
</asp:Content>
