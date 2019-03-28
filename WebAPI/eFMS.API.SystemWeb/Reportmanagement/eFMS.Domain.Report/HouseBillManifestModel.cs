using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.Domain.Report
{
    public class HouseBillManifestModel
    {
        public string Hwbno { get; set; }
        public string Packages { get; set; }
        public decimal Weight { get; set; }
        public decimal Volumn { get; set; }
        public string Shipper { get; set; }
        public string NotifyParty { get; set; }
        public string ShippingMark { get; set; }
        public string Description { get; set; }
    }
}
