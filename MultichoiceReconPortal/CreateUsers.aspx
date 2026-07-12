<%@ Page Title="Users" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CreateUsers.aspx.cs" Inherits="MultichoiceReconPortal.CreateUsers" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head d-flex justify-content-between align-items-center">
        <div>
            <h1>Users</h1>
            <p>Create portal users and manage their access. New users get a temporary password by email and set their own on first sign in.</p>
        </div>
        <button type="button" class="btn btn-mc btn-lg" onclick="mcOpenModal('addUserModal')">+ Add User</button>
    </div>

    <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="alert py-2">
        <asp:Label ID="lblMsg" runat="server" />
    </asp:Panel>

    <div class="mc-card">
        <div class="card-header py-3">Existing users</div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="gvUsers" runat="server" CssClass="table table-hover align-middle mb-0"
                    AutoGenerateColumns="false" GridLines="None"
                    DataKeyNames="UserId,Email,FullName,IsActive,RoleId"
                    OnRowCommand="gvUsers_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="#">
                            <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="FullName" HeaderText="Name" />
                        <asp:BoundField DataField="Email" HeaderText="Email" />
                        <asp:BoundField DataField="RoleName" HeaderText="Role" />
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='badge <%# Convert.ToBoolean(Eval("IsActive")) ? "bg-success" : "bg-secondary" %>'>
                                    <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Disabled" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Last login">
                            <ItemTemplate>
                                <%# Eval("LastLoginDate") == null || Eval("LastLoginDate") == System.DBNull.Value ? "Never" : Convert.ToDateTime(Eval("LastLoginDate")).ToString("dd MMM yyyy HH:mm") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="d-flex flex-wrap gap-1">
                                    <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-primary"
                                        CommandName="EditUser" CommandArgument="<%# Container.DataItemIndex %>" Text="Edit" />
                                    <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-secondary"
                                        CommandName="ResetPwd" CommandArgument="<%# Container.DataItemIndex %>" Text="Reset password"
                                        OnClientClick="return confirm('Reset this user\'s password and email them a new temporary one?');" />
                                    <asp:LinkButton runat="server" CssClass='<%# "btn btn-sm " + (Convert.ToBoolean(Eval("IsActive")) ? "btn-outline-danger" : "btn-outline-success") %>'
                                        CommandName="ToggleActive" CommandArgument="<%# Container.DataItemIndex %>"
                                        Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Disable" : "Enable" %>' />
                                    <asp:LinkButton runat="server" CssClass="btn btn-sm btn-outline-danger"
                                        CommandName="DeleteUser" CommandArgument="<%# Container.DataItemIndex %>" Text="Delete"
                                        OnClientClick="return confirm('Delete this user permanently? This cannot be undone.');" />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="text-center text-muted py-4">No users yet.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>

    <!-- Add User modal (overlay; does not move the table) -->
    <div class="modal fade" id="addUserModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <asp:Panel ID="pnlForm" runat="server" DefaultButton="btnCreate">
                    <div class="modal-header">
                        <h5 class="modal-title">Add a user</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body p-4 mc-form">
                        <asp:Panel ID="pnlModalMsg" runat="server" Visible="false" CssClass="alert alert-danger py-2 js-modal-alert">
                            <asp:Label ID="lblModalMsg" runat="server" />
                        </asp:Panel>
                        <div class="mb-3">
                            <label class="form-label">Full name</label>
                            <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Email</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Role</label>
                            <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-outline-secondary btn-lg" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnCreate" runat="server" CssClass="btn btn-mc btn-lg" Text="Create User" OnClick="btnCreate_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>

    <!-- Edit User modal -->
    <div class="modal fade" id="editUserModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content">
                <asp:Panel ID="pnlEditForm" runat="server" DefaultButton="btnUpdate">
                    <div class="modal-header">
                        <h5 class="modal-title">Edit user</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body p-4 mc-form">
                        <asp:HiddenField ID="hfEditUserId" runat="server" />
                        <asp:Panel ID="pnlEditMsg" runat="server" Visible="false" CssClass="alert alert-danger py-2 js-modal-alert">
                            <asp:Label ID="lblEditMsg" runat="server" />
                        </asp:Panel>
                        <div class="mb-3">
                            <label class="form-label">Full name</label>
                            <asp:TextBox ID="txtEditFullName" runat="server" CssClass="form-control" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Email</label>
                            <asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" TextMode="Email" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Role</label>
                            <asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-select" />
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-outline-secondary btn-lg" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnUpdate" runat="server" CssClass="btn btn-mc btn-lg" Text="Save changes" OnClick="btnUpdate_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>
</asp:Content>
