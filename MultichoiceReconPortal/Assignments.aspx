<%@ Page Title="Partner Assignments" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Assignments.aspx.cs" Inherits="MultichoiceReconPortal.Assignments" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .mc-partner-tags .badge { background: #eef4f1 !important; color: var(--mc-blue-dark) !important; font-weight: 600; margin: 0 4px 4px 0; }

        /* checkbox list styled as a clean two-column list of toggles */
        .mc-assign-list { width: 100%; }
        .mc-assign-list td { padding: 8px 14px 8px 6px; }
        .mc-assign-list label { font-size: .95rem; color: var(--mc-ink); margin-left: 6px; cursor: pointer; }
        .mc-assign-list input[type=checkbox] { width: 18px; height: 18px; cursor: pointer; accent-color: var(--mc-blue); vertical-align: middle; }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head">
        <h1>Partner Assignments</h1>
        <p>Assign partners to accountants (and to yourself). A user can only upload, view and report on the partners assigned to them.</p>
    </div>

    <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Label ID="lblMsg" runat="server" />
    </asp:Panel>

    <div class="mc-card">
        <div class="card-header py-3">Users</div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="gvUsers" runat="server" CssClass="table table-hover align-middle mb-0"
                    AutoGenerateColumns="false" GridLines="None"
                    DataKeyNames="UserId,FullName"
                    OnRowCommand="gvUsers_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="#">
                            <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="FullName" HeaderText="Name" />
                        <asp:BoundField DataField="RoleName" HeaderText="Role" />
                        <asp:TemplateField HeaderText="Assigned partners">
                            <ItemTemplate>
                                <span class="mc-partner-tags"><%# BuildTags(Eval("Partners")) %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:LinkButton runat="server" CssClass="btn btn-sm btn-mc"
                                    CommandName="Assign" CommandArgument="<%# Container.DataItemIndex %>" Text="Assign partners" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="text-center text-muted py-4">No accountants or head-accounts users yet.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>

    <!-- Assign partners modal -->
    <div class="modal fade" id="assignModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <asp:Panel ID="pnlAssign" runat="server" DefaultButton="btnSaveAssign">
                    <div class="modal-header">
                        <h5 class="modal-title">Assign partners &mdash; <asp:Literal ID="litAssignUser" runat="server" /></h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body p-4">
                        <asp:HiddenField ID="hfUserId" runat="server" />
                        <p class="text-muted small mb-3">Tick the partners this user should handle, then save.</p>
                        <asp:CheckBoxList ID="cblPartners" runat="server" CssClass="mc-assign-list"
                            RepeatColumns="2" RepeatDirection="Horizontal"
                            DataTextField="PartnerName" DataValueField="PartnerId" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-outline-secondary btn-lg" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnSaveAssign" runat="server" CssClass="btn btn-mc btn-lg" Text="Save assignments" OnClick="btnSaveAssign_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>
</asp:Content>
