using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Newtonsoft.Json;
using ReportPerview.Common;
using ReportPerview.Models;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace ReportPerview
{
    public partial class ExportCrystal : Page
    {
        protected void Page_Load(object sender, EventArgs e)
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
                    Utility.WriteToFile("Log Export Crystall fail" , ex.ToString());

                    crystal = null;
                    Response.Redirect("~/NotFound.aspx");
                }
                if (crystal.DataSource.Columns.Count == 0)
                {
                    crystal = null;
                    throw new Exception("Resource not found");
                }
                ReportDocument rpt = ShowReport(crystal);
                //Format Export: PDF, WORD, EXCEL
                ExportCrystalReport(rpt, "PDF", crystal.PathReportGenerate);
            }
        }

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

        void ExportCrystalReport(ReportDocument cryRpt, string formatExport, string pathReportGenerate)
        {
            try
            {
                if (string.IsNullOrEmpty(pathReportGenerate))
                {
                    throw new Exception("Path report generate not found");
                }

                if (cryRpt == null)
                {
                    throw new Exception("Resource not found");
                }

                ExportFormatType exportFormatType = ExportFormatType.NoFormat;
                string extensionFile = Path.GetExtension(pathReportGenerate);
                object formatOption = new PdfRtfWordFormatOptions();
                switch (formatExport)
                {
                    case "WORD":
                        exportFormatType = ExportFormatType.WordForWindows;
                        extensionFile = ".doc";
                        break;
                    case "PDF":
                        exportFormatType = ExportFormatType.PortableDocFormat;
                        break;
                    case "EXCEL":
                        exportFormatType = ExportFormatType.Excel;
                        extensionFile = ".xls";
                        formatOption = new ExcelFormatOptions();
                        break;
                }

                var downloadReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/DownloadReports/");
                //Check exist folder DownloadReports
                if (!Directory.Exists(downloadReportPath))
                {
                    Directory.CreateDirectory(downloadReportPath);
                }

                DiskFileDestinationOptions CrDiskFileDestinationOptions = new DiskFileDestinationOptions();
                // edit: export file with path combine disk path and file name
                CrDiskFileDestinationOptions.DiskFileName = Path.Combine(downloadReportPath, Path.GetFileName(pathReportGenerate));
                ExportOptions CrExportOptions = cryRpt.ExportOptions;
                {
                    CrExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                    CrExportOptions.ExportFormatType = exportFormatType;
                    CrExportOptions.DestinationOptions = CrDiskFileDestinationOptions;
                    CrExportOptions.FormatOptions = formatOption;
                }
                Utility.WriteToFile("Log Export Crystall OPtion", JsonConvert.SerializeObject(CrExportOptions).ToString());

                cryRpt.Export();
            }
            catch (Exception ex)
            {
                Utility.WriteToFile("Log Export Crystall fail", ex.ToString());
            }
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