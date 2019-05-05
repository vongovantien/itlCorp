using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Web;
using ITL.Connection.OLEDB;
using ITL.WebCrystalReport;
using ReportPerview.Common;

namespace ReportPerview
{
    public partial class Home : System.Web.UI.Page
    {
        private string key = "Adghdhsge5234634HDFGh#454!gdgđfdfhdh";
        protected void Page_Load(object sender, EventArgs e)
        {
            ////test
            //WebReportBase rptTest = new WebReportBase(
            //          new ConnectionSQLServer(ConfigurationManager.ConnectionStrings["eTMSTestConnectionString"].ConnectionString)
            //          , Server.MapPath("~/Reports"));
            //DataTable main = rptTest.SetDataByView(string.Format(App_GlobalResources.SQLReport.rptLCLQuotation, 27));//27
            //DataTable detail = rptTest.SetDataSubByView("rptLCLQuotationRateCardDetail", string.Format(App_GlobalResources.SQLReport.rptLCLQuotationRateCardDetail, 27, 5));//5
            //DataTable overweight = rptTest.SetDataSubByView("rptLCLQuotationOverWeight", string.Format(App_GlobalResources.SQLReport.rptLCLQuotationOverWeight, 27, 5));

            //JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            //ReportData reportData = new ReportData();
            //reportData.AllowExport = true;
            //reportData.AllowPrint = false;
            //reportData.MainData = Utility.ConvertDataTableToJson(main);
            //reportData.SubData.Add("rptLCLQuotationRateCardDetail", Utility.ConvertDataTableToJson(detail));
            //reportData.SubData.Add("rptLCLQuotationOverWeight", Utility.ConvertDataTableToJson(overweight));
            ////reportData.MainData = Utility.ConvertDataTableToXML(main);
            ////reportData.SubData.Add("rptLCLQuotationRateCardDetail", Utility.ConvertDataTableToXML(detail));
            ////reportData.SubData.Add("rptLCLQuotationOverWeight", Utility.ConvertDataTableToXML(overweight));

            //string data = json_serializer.Serialize(reportData);           
            //string type = "LCLRateCard";
            //string pkey = "Adghdhsge5234634HDFGh#454!gdgđfdfhdh";
            //string iden = Request.QueryString["identity"];

            //string ptext = string.Format("{0}{1}{2}", type, data, pkey);
            //string key = ITL.COMMON.Utility.GetMD5(ptext).ToLower();

            //Response.Redirect("~/Default.aspx?identity=" + HttpUtility.UrlEncode(key) + "&type=" + HttpUtility.UrlEncode(type)
            //    + "&data=" + HttpUtility.UrlEncode(data));
            //return;

            ////------------------

            //if (!Page.IsPostBack)
            //{
            //    string identity = Request.QueryString["identity"];
            //    if (identity == null || identity != key)
            //    {
            //        Response.Redirect("~/NotFound.aspx");
            //        return;
            //    }

            //    WebReportBase rpt = new WebReportBase(
            //           new ConnectionSQLServer(ConfigurationManager.ConnectionStrings["eTMSTestConnectionString"].ConnectionString)
            //           , Server.MapPath("~/Reports"));

            //    string sType = Request.QueryString["type"];
            //    if (sType != null)
            //    {
            //        rptViewer.DisplayGroupTree = true;
            //        rptViewer.HasCrystalLogo = false;
            //        rptViewer.HasPrintButton = true;
            //        rptViewer.HasExportButton = true;

            //        ReportType reportType;
            //        Enum.TryParse(sType, out reportType);
            //        ShowReport(rpt, reportType, Request);
            //    }
            //    //rpt.ExportHttpRequest(Response, ExportCRFormatType.PortableDocFormat );
            //    Session["report"] = rpt;
            //}
            //else
            //{
            //    WebReportBase rpt = (WebReportBase)Session["report"];
            //    rpt.View(ref rptViewer);
            //    //rptViewer.RefreshReport();
            //    //rptViewer.DataBind();
            //}
        }

        private void ShowReport(WebReportBase rpt, ReportType reportType, HttpRequest httpRequest)
        {
            switch (reportType)
            {
                case ReportType.LCLWaybill:                    
                    break;
                case ReportType.FCLWaybill:
                    break;
                case ReportType.DNTWaybill:
                    break;
                case ReportType.LCLRateCard:
                    {
                        int rateCardId = 0, serviceType = 0;

                        string sID = httpRequest.QueryString["id"];
                        if (sID != null)
                            int.TryParse(sID, out rateCardId);

                        string sService = httpRequest.QueryString["service"];
                        if (sService != null)
                            int.TryParse(sService, out serviceType);

                        rpt.SetDataByView(string.Format(App_GlobalResources.SQLReport.rptLCLQuotation, rateCardId));//27
                        rpt.SetDataSubByView("rptLCLQuotationRateCardDetail", string.Format(App_GlobalResources.SQLReport.rptLCLQuotationRateCardDetail, rateCardId, serviceType));//5
                        rpt.SetDataSubByView("rptLCLQuotationOverWeight", string.Format(App_GlobalResources.SQLReport.rptLCLQuotationOverWeight, rateCardId, serviceType));
                        rpt.View("rptLCLQuotation-En", ref rptViewer);
                    }
                    break;
                case ReportType.FCLQuotation:
                    break;
                default:
                    break;
            }
        }        

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            //if (!Page.IsPostBack)
            //{
            //    WebReportBase rpt = new WebReportBase(
            //        new ConnectionSQLServer(ConfigurationManager.ConnectionStrings["eTMSTestConnectionString"].ConnectionString)
            //        , Server.MapPath("~/Reports"));
            //    rpt.SetDataByView(string.Format(App_GlobalResources.SQLReport.rptLCLQuotation, 27));
            //    rpt.SetDataSubByView("rptLCLQuotationRateCardDetail", string.Format(App_GlobalResources.SQLReport.rptLCLQuotationRateCardDetail, 27, 5));
            //    rpt.SetDataSubByView("rptLCLQuotationOverWeight", string.Format(App_GlobalResources.SQLReport.rptLCLQuotationOverWeight, 27, 5));
            //    rpt.View("rptLCLQuotation-En", true, true, ref pReport);

            //    Session["report"] = rpt;
            //    //rpt.ExportHttpRequest(Response, "rptLCLQuotation-En", ExportCRFormatType.PortableDocFormat);
            //}
            //else
            //{
            //    WebReportBase rpt = (WebReportBase)Session["report"];
            //    rpt.View(ref pReport);
            //}
        }

        protected override void Render(HtmlTextWriter writer)
        {
            string appPath = HttpContext.Current.Request.ApplicationPath;
            if (appPath != @"/")
            {
                // variables
                string Content = string.Empty;

                // get the fully rendered content of this page so we can do some additional translations if necessary  
                using (StringWriter sw = new StringWriter())
                {
                    using (HtmlTextWriter htmlWriter = new HtmlTextWriter(sw))
                    {
                        // render current page content to temp writer  
                        base.Render(htmlWriter);
                        //this.RenderChildren(hw);

                        // close writer  
                        htmlWriter.Close();

                        // get content  
                        Content = sw.ToString();

                        // DO SOMETHING WITH THE CONTENT
                        string pattern = @"/CrystalImageHandler.aspx";
                        string newURL = appPath + @"/CrystalImageHandler.aspx";

                        string pageHTML = Regex.Replace(Content, pattern, newURL, RegexOptions.IgnoreCase);

                        pattern = @"/aspnet_client/system_web/";
                        newURL = appPath + @"/aspnet_client/system_web/";

                        pageHTML= Regex.Replace(pageHTML, pattern, newURL, RegexOptions.IgnoreCase);

                        Content = pageHTML;
                    }
                }
                // render content
                writer.Write(Content);
            }
            else
            {
                base.Render(writer);
            }
        }
    }
}