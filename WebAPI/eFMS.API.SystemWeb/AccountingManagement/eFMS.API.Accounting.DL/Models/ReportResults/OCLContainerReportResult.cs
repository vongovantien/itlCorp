using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.ReportResults
{
    public class OCLContainerReportResult
    {
        public int STT { get; set; }
        public string ContainerNo { get; set; }
        public string SealNo { get; set; }
        public string CELLLOG { get; set; }
        public string ST { get; set; }
        public string SZTY { get; set; }	
        public decimal? GW { get; set; }
        public decimal CBM { get; set; }

    }
}
