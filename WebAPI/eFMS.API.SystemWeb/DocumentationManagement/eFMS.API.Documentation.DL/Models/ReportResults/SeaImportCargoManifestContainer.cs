using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class SeaImportCargoManifestContainer
    {
        public int? Qty { get; set; }
        public string ContType { get; set; }
        public string ContainerNo { get; set; }
        public string SealNo { get; set; }
        public int? TotalPackages { get; set; }
        public string UnitPack { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? CBM { get; set; }
        public int DecimalNo { get; set; }
    }
}
