using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class CustomsDeclaration
    {
        public string ClearanceNo { get; set; }
        public string Type { get; set; }
        public string GatewayName { get; set; }
        public string CustomerName { get; set; }
        public string ImportCountryName { get; set; }
        public string ExportCountryName { get; set; }
        public string JobNo { get; set; }
        public string ClearanceDate { get; set; }
        public string Status { get; set; }
    }
}
