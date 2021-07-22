<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ReportPerview.Default" validateRequest="false" %>
<%@Register Assembly="CrystalDecisions.Web, Version=10.5.3700.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="Style.css" rel="stylesheet" />
</head>
<body>
   <form id="form1" runat="server">
        <asp:Panel ID="pReport" Width="100%" Height="100%" runat="server">
            <CR:CrystalReportViewer ID="rptViewer" runat="server" DisplayGroupTree="True" HasCrystalLogo="true" BestFitPage="False" Width="800px"/>
        </asp:Panel>
    </form>
</body>
</html>
