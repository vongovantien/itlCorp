using CrystalDecisions.CrystalReports.Engine;
//using ITL.WebCrystalReport;
using ReportPerview.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportPerview.Common
{
    public static class CrystalHelper
    {
        public static ReportDocument SetParameter(this ReportDocument rpt, Dictionary<string, object> data)
        {

            foreach (var item in data)
            {
                rpt.SetParameterValue(item.Key, item.Value);
            }
            return rpt;
        }
        public static ReportDocument Init(this ReportDocument rpt, Crystal crystal)
        {
            if (crystal.DataSource.Rows.Count > 0)
                rpt.SetDataSource(crystal.DataSource);
            foreach (SubReport sub in crystal.SubReports)
            {
                if (sub.DataSource.Rows.Count > 0)
                    rpt.Subreports[sub.Name].SetDataSource(sub.DataSource);
            }
            rpt.SetParameter(crystal.Parameters);
            return rpt;
        }
    }
}