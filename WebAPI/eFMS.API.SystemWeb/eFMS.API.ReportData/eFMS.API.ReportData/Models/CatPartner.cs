using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class CatPartner
    {
        public string AccountNo { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string AddressVN { get; set; }
        public string TaxCode { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string UserCreatedName { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }

    }
}
