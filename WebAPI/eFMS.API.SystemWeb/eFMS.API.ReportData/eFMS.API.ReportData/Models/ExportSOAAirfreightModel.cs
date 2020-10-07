using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class ExportSOAAirfreightModel
    {
        public List<HawbAirFrieghtModel> HawbAirFrieghts { get; set; }

        public string PartnerNameEn { get; set; }
        public string PartnerBillingAddress { get; set; }
        public string PartnerTaxCode { get; set; }

        public string OfficeEn { get; set; }
        public string BankAccountVND { get; set; }
        public string BankAccountUsd { get; set; }
        public string BankNameEn { get; set; }
        public string AddressEn { get; set; }
        public string SwiftCode { get; set; }

        public string SoaNo { get; set; }
        public DateTime? DateSOA { get; set; }
        public DateTime? SoaFromDate { get; set; }

        public string IssuedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string Account { get; set; }
 
    }
}
