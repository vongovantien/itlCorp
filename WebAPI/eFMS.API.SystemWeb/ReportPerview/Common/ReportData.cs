using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportPerview.Common
{
    public class ReportData
    {
        public string MainData { get; set; }
        public Dictionary<string, string> SubData { get; set; }
        public bool AllowPrint { get; set; }
        public bool AllowExport { get; set; }

        public ReportData()
        {
            SubData = new Dictionary<string, string>();
        }
    }
}