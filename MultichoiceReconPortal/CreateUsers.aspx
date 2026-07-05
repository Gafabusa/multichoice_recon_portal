<%@ Page Title="Users" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CreateUsers.aspx.cs" Inherits="MultichoiceReconPortal.CreateUsers" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="mc-page-head text-center">
        <h1>Users</h1>
        <p>Create portal users and manage their access. New users receive a temporary password by email and set their own password on first sign in.</p>
    </div>

    <div class="row justify-content-center">
        <div class="col-xl-6 col-lg-8">
            <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="alert py-2">
                <asp:Label ID="lblMsg" runat="server" />
            </asp:Panel>

            <div class="mc-card mb-4">
                <div class="card-header py-3">Add a user</div>
                <div class="card-body p-4">
                    <asp:Panel ID="pnlForm" runat="server" DefaultButton="btnCreate">
                        <div class="mb-3">
                            <label class="form-label fw-semibold">Full name</label>
                            <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control form-control-lg" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-semibold">Email</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control form-control-lg" TextMode="Email" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-semibold">Role</label>
                            <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select form-select-lg" />
                        </div>
                        <asp:Button ID="btnCreate" runat="server" CssClass="btn btn-mc btn-lg w-100" Text="Create User" OnClick="btnCreate_Click" />
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>

    <div class="mc-card">
        <div class="card-header py-3">Existing users</div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="gvUsers" runat="server" CssClass="table table-hover align-middle mb-0"
                    AutoGenerateColumns="false" GridLines="None"
                    DataKeyNames="UserId,Email,FullName,IsActive"
                    OnRowCommand="gvUsers_RowCommand">
                    <Columns>
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
                                <asp:LinkButton runat="server" CssClass='<%# "btn btn-sm " + (Convert.ToBoolean(Eval("IsActive")) ? "btn-outline-danger" : "btn-outline-success") %>'
                                    CommandName="ToggleActive" CommandArgument="<%# Container.DataItemIndex %>"
                                    Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Disable" : "Enable" %>' />
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
</asp:Content>
