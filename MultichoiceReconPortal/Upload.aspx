<%@ Page Title="Upload Statement" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Upload.aspx.cs" Inherits="MultichoiceReconPortal.Upload" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head d-flex justify-content-between align-items-center">
        <div>
            <h1>Upload Statement</h1>
            <p>Upload a channel statement that didn't arrive by email. It is queued for the reconciliation engine and reconciled under your name.</p>
        </div>
        <div class="d-flex gap-2">
            <asp:Panel ID="pnlMultichoiceBtn" runat="server" Visible="false">
                <button type="button" class="btn btn-outline-secondary btn-lg" onclick="mcOpenModal('multichoiceModal')">+ Upload MultiChoice Records</button>
            </asp:Panel>
            <button type="button" class="btn btn-mc btn-lg" onclick="mcOpenModal('uploadModal')">+ Upload Statement</button>
        </div>
    </div>

    <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Label ID="lblMsg" runat="server" />
    </asp:Panel>

    <div class="mc-card">
        <div class="card-header py-3">Recent uploads &amp; statements</div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="gvRecent" runat="server" CssClass="table table-hover align-middle mb-0"
                    AutoGenerateColumns="false" GridLines="None">
                    <Columns>
                        <asp:TemplateField HeaderText="#">
                            <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Partner" HeaderText="Partner" />
                        <asp:BoundField DataField="Product" HeaderText="Product" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='badge <%# StatusClass(Eval("Status")) %>'><%# Eval("Status") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ProcessedBy" HeaderText="Uploaded by" />
                        <asp:TemplateField HeaderText="Received">
                            <ItemTemplate>
                                <%# Eval("RecordDate") == null || Eval("RecordDate") == System.DBNull.Value ? "" : Convert.ToDateTime(Eval("RecordDate")).ToString("dd MMM yyyy HH:mm") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="text-center text-muted py-4">No statements yet.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>

    <!-- Upload modal (overlay; does not move the table) -->
    <div class="modal fade" id="uploadModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <asp:Panel ID="pnlForm" runat="server" DefaultButton="btnUpload">
                    <div class="modal-header">
                        <h5 class="modal-title">New upload</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body p-4 mc-form">
                        <asp:Panel ID="pnlModalMsg" runat="server" Visible="false" CssClass="alert alert-danger py-2 js-modal-alert">
                            <asp:Label ID="lblModalMsg" runat="server" />
                        </asp:Panel>
                        <div class="mb-3">
                            <label class="form-label">Channel</label>
                            <asp:DropDownList ID="ddlChannel" runat="server" CssClass="form-select" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Statement file</label>
                            <asp:FileUpload ID="fuStatement" runat="server" CssClass="form-control" />
                            <div class="form-text">Upload the original file exactly as received (.csv, .xls, .zip). Do not open/resave it in Excel &mdash; that can corrupt long transaction IDs.</div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-outline-secondary btn-lg" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnUpload" runat="server" CssClass="btn btn-mc btn-lg" Text="Upload statement" OnClick="btnUpload_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>

    <!-- MultiChoice records upload modal (Head Accounts only) -->
    <div class="modal fade" id="multichoiceModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <asp:Panel ID="pnlMcForm" runat="server" DefaultButton="btnUploadMc">
                    <div class="modal-header">
                        <h5 class="modal-title">Upload MultiChoice records</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body p-4 mc-form">
                        <asp:Panel ID="pnlMcModalMsg" runat="server" Visible="false" CssClass="alert alert-danger py-2 js-modal-alert">
                            <asp:Label ID="lblMcModalMsg" runat="server" />
                        </asp:Panel>
                        <div class="mb-3">
                            <label class="form-label">MultiChoice file</label>
                            <asp:FileUpload ID="fuMultichoice" runat="server" CssClass="form-control" />
                            <div class="form-text">Upload the MultiChoice records file exactly as received (.csv).</div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-outline-secondary btn-lg" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnUploadMc" runat="server" CssClass="btn btn-mc btn-lg" Text="Upload records" OnClick="btnUploadMc_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>
</asp:Content>
