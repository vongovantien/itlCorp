using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Web;
//using ITL.COMMON;
//using ITL.Connection.OLEDB;
//using ITL.WebCrystalReport;
using Newtonsoft.Json;
using ReportPerview.Common;
using ReportPerview.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ReportPerview
{
    public partial class Default : System.Web.UI.Page
    {
        void InitializeCompnent()
        {
            // Fix bug: crystal report viewer next page not working >> Move the contents of Page_Load to Page_Init()
            this.Init += Page_Init; 
        }
        
        private void Page_Init(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                Crystal crystal = new Crystal();
                try
                {
                    crystal = JsonConvert.DeserializeObject<Crystal>(Request.Form.GetValues(0)[0]);
                }
                catch (Exception ex)
                {
                    crystal = null;
                    Response.Redirect("~/NotFound.aspx");
                }
                if (crystal.DataSource.Columns.Count == 0)
                {
                    crystal = null;
                    throw new Exception("Resource not found");
                }
                ReportDocument rpt = ShowReport(crystal);
                if (rpt != null)
                {
                    rptViewer.HasPrintButton = crystal.AllowPrint;
                    rptViewer.HasExportButton = crystal.AllowExport;
                    rptViewer.DisplayGroupTree = false;
                    Session["report"] = rpt;
                    Session["allowPrint"] = crystal.AllowPrint;
                    Session["allowExport"] = crystal.AllowExport;
                    rptViewer.ReportSource = rpt;
                    if (rpt.PrintOptions.PaperOrientation == CrystalDecisions.Shared.PaperOrientation.Landscape)
                    {
                        rptViewer.Width = 1320;
                    }
                    else
                    {
                        rptViewer.Width = 790;
                    }
                }
                else
                {
                    throw new Exception("Resource not found");
                }
            }
            else
            {
                ReportDocument rpt = (ReportDocument)Session["report"];
                bool allowPrint = (bool)Session["allowPrint"];
                bool allowExport = (bool)Session["allowExport"];
                if (rpt != null)
                {
                    rptViewer.HasPrintButton = allowPrint;
                    rptViewer.HasExportButton = allowExport;
                    rptViewer.ReportSource = rpt;
                }
            }
        }

        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    if (!Page.IsPostBack)
        //    {
        //        Crystal crystal = new Crystal();
        //        try
        //        {
        //            crystal = JsonConvert.DeserializeObject<Crystal>(Request.Form.GetValues(0)[0]);
        //        }
        //        catch (Exception ex)
        //        {
        //            crystal = null;
        //            Response.Redirect("~/NotFound.aspx");
        //        }
        //        if(crystal.DataSource.Columns.Count == 0)
        //        {
        //            crystal = null;
        //            throw new Exception("Resource not found");
        //        }
        //        ReportDocument rpt = ShowReport(crystal);
        //        if (rpt != null)
        //        {
        //            rptViewer.HasPrintButton = crystal.AllowPrint;
        //            rptViewer.HasExportButton = crystal.AllowExport;
        //            rptViewer.DisplayGroupTree = false;
        //            Session["report"] = rpt;
        //            Session["allowPrint"] = crystal.AllowPrint;
        //            Session["allowExport"] = crystal.AllowExport;
        //            rptViewer.ReportSource = rpt;
        //            if (rpt.PrintOptions.PaperOrientation == CrystalDecisions.Shared.PaperOrientation.Landscape)
        //            {
        //                rptViewer.Width = 1320;
        //            }
        //            else
        //            {
        //                rptViewer.Width = 790;
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Resource not found");
        //        }
        //    }
        //    else
        //    {
        //        ReportDocument rpt = (ReportDocument)Session["report"];
        //        bool allowPrint = (bool)Session["allowPrint"];
        //        bool allowExport = (bool)Session["allowExport"];
        //        if (rpt != null)
        //        {
        //            rptViewer.HasPrintButton = allowPrint;
        //            rptViewer.HasExportButton = allowExport;
        //            rptViewer.ReportSource = rpt;
        //        }
        //    }
        //}

        /// <summary>
        /// Binding data string to report rpt and show on report viewer
        /// </summary>
        private ReportDocument ShowReport(Crystal data)
        {
            var reportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/");
            string filePath = reportPath + data.ReportName;
            if (File.Exists(filePath))
            {
                ReportDocument rpt = new ReportDocument();
                rpt.Load(reportPath + data.ReportName);
                rpt.Init(data);
                return rpt;
            }
            return null;
        }
        /// <summary>
        /// Override Render method to replace file path when web application is deployed in sub
        /// </summary>
        /// <param name="writer"></param>
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

                        pageHTML = Regex.Replace(pageHTML, pattern, newURL, RegexOptions.IgnoreCase);

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