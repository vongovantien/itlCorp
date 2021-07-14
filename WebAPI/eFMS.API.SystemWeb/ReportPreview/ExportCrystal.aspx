<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExportCrystal.aspx.cs" Inherits="ReportPerview.ExportCrystal" validateRequest="false"%>
<%@Register Assembly="CrystalDecisions.Web, Version=12.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Panel ID="pExportCrystal" Width="100%" Height="100%" runat="server">
            <CR:CrystalReportViewer ID="rptExportViewer" runat="server" HasCrystalLogo="true" BestFitPage="False" Width="800px" />
        </asp:Panel>
    </form>
</body>
</html>
