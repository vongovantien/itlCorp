using CrystalDecisions.CrystalReports.Engine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
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
                crystal = null;
                Response.Redirect("~/NotFound.aspx");
            }
            var reportPath = Server.MapPath("~/Reports/rptManifest.rpt");
            if (File.Exists(reportPath))
            {
                ReportDocument rpt = new ReportDocument();
                rpt.Load(reportPath);
                rpt.SetDataSource(crystal.DataSource);
                foreach (SubReport sub in crystal.SubReports)
                {
                    if (sub.DataSource.Rows.Count > 0)
                        rpt.Subreports[sub.Name].SetDataSource(sub.DataSource);
                }
                rptViewer.HasPrintButton = false;
                rptViewer.HasExportButton = false;
                rptViewer.ReportSource = rpt;
            }
        }
    }
}