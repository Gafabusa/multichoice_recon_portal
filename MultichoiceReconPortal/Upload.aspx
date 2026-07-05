<%@ Page Title="Upload Statement" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Upload.aspx.cs" Inherits="MultichoiceReconPortal.Upload" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head">
        <h1>Upload Statement</h1>
        <p>Upload a channel statement that didn't arrive by email. It is queued for the reconciliation engine and processed automatically &mdash; reconciled under your name.</p>
    </div>

    <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Label ID="lblMsg" runat="server" />
    </asp:Panel>

    <div class="row g-4">
        <div class="col-lg-5">
            <div class="mc-card">
                <div class="card-header py-3">New upload</div>
                <div class="card-body p-4">
                    <div class="mb-3">
                        <label class="form-label fw-semibold">Channel</label>
                        <asp:DropDownList ID="ddlChannel" runat="server" CssClass="form-select" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-semibold">Statement file</label>
                        <asp:FileUpload ID="fuStatement" runat="server" CssClass="form-control" />
                        <div class="form-text">Upload the original file exactly as received (.csv, .xls, .zip). Do not open/resave it in Excel &mdash; that can corrupt long transaction IDs.</div>
                    </div>
                    <asp:Button ID="btnUpload" runat="server" CssClass="btn btn-mc px-4" Text="Upload &amp; Queue" OnClick="btnUpload_Click" />
                </div>
            </div>
        </div>

        <div class="col-lg-7">
            <div class="mc-card">
                <div class="card-header py-3">Recent uploads &amp; statements</div>
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <asp:GridView ID="gvRecent" runat="server" CssClass="table table-hover align-middle mb-0"
                            AutoGenerateColumns="false" GridLines="None">
                            <Columns>
                                <asp:BoundField DataField="Bank" HeaderText="Channel" />
                                <asp:BoundField DataField="Account" HeaderText="Account" />
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <span class='badge <%# StatusClass(Eval("Status")) %>'><%# Eval("Status") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="ProcessedBy" HeaderText="Processed by" />
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
        </div>
    </div>
</asp:Content>
