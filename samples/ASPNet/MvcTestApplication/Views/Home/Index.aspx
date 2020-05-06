<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: ViewData["Message"] %></h2>
    <p>
        <%: ViewData["ApplicationMessage"] %>
    </p>
    <p>
        <%: ViewData["RequestMessage"] %>
    </p>
    <p>.. big pause ..</p>
    <p>
        <%: ViewData["RequestMessage2"] %>
    </p>
    <h2>
        <%: ViewData["ResultMessage"]%>
    </h2>

</asp:Content>
