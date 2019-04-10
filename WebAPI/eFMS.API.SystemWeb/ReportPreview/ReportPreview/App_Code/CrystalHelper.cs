using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CrystalHelper
/// </summary>
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
    public static ReportDocument Init(ReportDocument rpt, Crystal crystal)
    {
        rpt.SetParameter(crystal.parameter);
        if (crystal.DataSource.Rows.Count > 0)
            rpt.SetDataSource(crystal.DataSource);
        foreach (SubReport sub in crystal.SubReports)
        {
            if (sub.DataSource.Rows.Count > 0)
                rpt.Subreports[sub.Name].SetDataSource(sub.DataSource);
        }
        return rpt;
    }
}