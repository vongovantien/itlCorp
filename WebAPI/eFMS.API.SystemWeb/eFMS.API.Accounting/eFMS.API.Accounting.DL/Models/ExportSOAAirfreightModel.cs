using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class ExportSOAAirfreightModel
    {
        public List<HawbAirFrieghtModel> HawbAirFrieghts { get; set; }

        public string PartnerNameEn { get; set; }
        public string PartnerBillingAddress { get; set; }
        public string PartnerTaxCode { get; set; }

        public string OfficeEn { get; set; }
        public string BankAccountVND { get; set; }
        public string BankAccountUSD { get; set; }
        public string BankNameEn { get; set; }
        public string AddressEn { get; set; }
        public string SwiftCode { get; set; }

        public string SoaNo { get; set; }
        public DateTime? DateSOA { get; set; }

        public string IssuedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string Account { get; set; }


    

    }
}
