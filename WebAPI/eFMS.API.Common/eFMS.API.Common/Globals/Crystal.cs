﻿using eFMS.API.Common.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace eFMS.API.Common.Globals
{
    public class Crystal
    {
        public Crystal()
        {
            DataSource = new DataTable();
            SubReports = new List<SubReport>();
            Parameters = new Dictionary<string, object>();
        }
        public Crystal(DataTable dataSource, List<SubReport> subReports, Dictionary<string, object> parameter)
        {
            this.DataSource = dataSource;
            this.SubReports = subReports;
            this.Parameters = parameter;
        }
        public void AddDataSource<T>(List<T> lst)
        {
            DataSource = lst.ToDataTable();
        }
        public void AddSubReport<T>(string name, IList<T> lst)
        {
            SubReports.Add(new SubReport(name, lst.ToDataTable()));
        }
        public void SetParameter<T>(T obj)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                Parameters.Add(prop.Name, obj.GetValueBy(prop.Name));
            }
        }
        public string ReportFile { get; set; }
        public string ReportName { get; set; }
        public ExportFormatType FormatType { get; set; }
        public DataTable DataSource { get; set; }
        public List<SubReport> SubReports { get; set; }
        public Dictionary<string, object> Parameters;
        public bool AllowPrint { get; set; }
        public bool AllowExport { get; set; }
        public bool IsLandscape { get; set; }
        public string PathReportGenerate { get; set; }
    }
    public class SubReport
    {
        public SubReport()
        {
            DataSource = new DataTable();
        }

        public SubReport(string name, DataTable dataSource)
        {
            this.Name = name;
            this.DataSource = dataSource;
        }

        public string Name { get; set; }
        public DataTable DataSource { get; set; }
    }
}
