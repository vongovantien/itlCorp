using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Report.DL.Models
{
    public class EDocReportResult
    {
        public string jobNo { get; set; }
        public string codeCus { get; set; }
        public string taxCode { get; set; }
        public string customer { get; set; }
        public string MBL { get; set; }
        public string HBL { get; set; }
        public string customNo { get; set; }
        public DateTime? createDate { get; set; }
        public string creator { get; set; }
        public string aliasName { get; set; }
        public string realFileName { get; set; }
        public string documentType { get; set; }
        public bool? require { get; set; }
        public DateTime? attachTime { get; set; }
        public string attachPerson { get; set; }
        public string userExport { get; set; }
    }
}
