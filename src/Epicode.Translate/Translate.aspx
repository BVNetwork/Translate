<%@ Page Language="c#" Codebehind="Translate.aspx.cs" AutoEventWireup="True" Inherits="Epicode.Translate.Translate" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Register TagPrefix="EPiServerUI" Namespace="EPiServer.UI.WebControls" Assembly="EPiServer.UI" %>
<%@ Register TagPrefix="EPiServerUI" Namespace="EPiServer.UI.WebControls.ContentDataSource" Assembly="EPiServer.UI" %>


<asp:Content ContentPlaceHolderID="HeaderContentRegion" runat="server">

    <script type="text/javascript">
        // <![CDATA[
            
        function PageTreeViewInit(treeView) {
            treeView.OnNodeSelected = OnTreeNavigation;
        }

        // Called when the the user selects a page in the tree
        function OnTreeNavigation(itemDataPath) {
            document.getElementById('<%= SelectedContentHiddenField.ClientID %>').value = itemDataPath;
        }
    
// ]]> 
    </script>

</asp:Content>


<asp:Content ContentPlaceHolderID="MainRegion" runat="server">
    <div class="epi-formArea">
        <div class="epi-size10">
            <div>
                <asp:Label AssociatedControlId="SourceLanguageDropDownList" Text="From language:" runat="server" />                        
                <asp:DropDownList ID="SourceLanguageDropDownList" runat="server" />
            </div>
            <div>
                <asp:Label AssociatedControlId="TargetLanguageDropDownList" Text="To language:" runat="server" />
                <asp:DropDownList ID="TargetLanguageDropDownList" runat="server" />
            </div>
        </div>

        <div class="epi-size20 epi-paddingVertical-small">
            
            <asp:Label runat="server" AssociatedControlID="PageRoot" Text="Start node:" />        
            <div class="episcroll episerver-pagetree-selfcontained">
                <EPiServerUI:PageTreeView ID="PageRoot" 
                                          DataSourceID="ContentDataSource" 
                                          ClientInitializationCallback="PageTreeViewInit" 
                                          ExpandDepth="1" 
                                          DataTextField="Name" 
                                          ExpandOnSelect="false"
                                          SelectedNodeViewPath="<%# CurrentContent.ContentLink.ToString() %>"
                                          DataNavigateUrlField="ContentLink" 
                                          CssClass="episerver-pagetreeview" 
                                          EnableViewState="false"
                                          runat="server" >
                    <TreeNodeTemplate>
                        <a href="<%# Server.HtmlEncode(((PageTreeNode)Container.DataItem).NavigateUrl) %>"><%# Server.HtmlEncode(((PageTreeNode)Container.DataItem).Text) %></a>
                    </TreeNodeTemplate>
                </EPiServerUI:PageTreeView>
                <asp:HiddenField ID="SelectedContentHiddenField" runat="server"/>
            </div>
        </div>
        
        <div class="epi-size10">
            <div>                                     
                <asp:CheckBox ID="AutoPublishCheckBox" Checked="True" runat="server"/>
                <asp:Label AssociatedControlId="AutoPublishCheckBox" Text="Auto publish content" runat="server" />   
            </div>
         </div>
        <div class="epi-size10">
            <div>                                     
                <asp:CheckBox ID="TranslateDescendentsCheckBox" runat="server"/>
                <asp:Label AssociatedControlId="TranslateDescendentsCheckBox" Text="Translate descendents" runat="server" />   
            </div>
        </div>
        <div class="epi-size10">
            <div>                                                     
                <asp:CheckBox ID="CreateProjectCheckBox" runat="server"/>
                <asp:Label AssociatedControlId="CreateProjectCheckBox" Text="Create project" runat="server" />   
            </div>
        </div>
        <div class="epi-size10">
            <div>                                     
                
                <asp:Label AssociatedControlId="ProjectNameTextBox" Text="Project name" runat="server" />   
                <asp:TextBox ID="ProjectNameTextBox" CssClass="episize240" runat="server"></asp:TextBox>
            </div>
        </div>
    </div>

    <div class="epi-buttonContainer">
        <span class="epi-cmsButton">
            <asp:Button OnClick="OnClick_Translate" Text="Translate" CssClass="epi-cmsButton-text epi-cmsButton-tools epi-cmsButton-Copy" runat="server"/>
        </span>
    </div>
    
    <asp:PlaceHolder ID="ResultPlaceHolder" Visible="false" runat="server">
        <pre style="overflow: scroll; height: 10em"><asp:Literal ID="ResultLiteral" runat="server"></asp:Literal></pre>
    </asp:PlaceHolder>

    
    <EPiServerUI:ContentDataSource 
        ID="ContentDataSource" 
        UseFallbackLanguage="true" 
        AccessLevel="NoAccess"         
        IncludeRootItem="true" 
        ContentLink="<%# ContentReference.RootPage %>" 
        EvaluateHasChildren="<%# !EPiServer.Configuration.Settings.Instance.UIOptimizeTreeForSpeed %>" 
        runat="server"  />

</asp:Content>