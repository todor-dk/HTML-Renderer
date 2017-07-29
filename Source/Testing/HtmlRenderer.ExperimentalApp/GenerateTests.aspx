<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GenerateTests.aspx.cs" Inherits="HtmlRenderer.ExperimentalApp.GenerateTests" %>
<%
    TestsCode.Text = HtmlRenderer.ExperimentalApp.Controllers.DocumentsController.GenerateTestCode();
%>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <pre>
                <asp:Label runat="server" id="TestsCode"></asp:Label>
            </pre>
        </div>
    </form>
</body>
</html>
