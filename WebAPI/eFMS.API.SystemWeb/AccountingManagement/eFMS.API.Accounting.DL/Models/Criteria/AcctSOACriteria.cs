using System;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class AcctSOACriteria
    {
        public string StrCodes { get; set; }
        public string CustomerID { get; set; }
        public DateTime? SoaFromDateCreate { get; set; }
        public DateTime? SoaToDateCreate { get; set; }
        public string SoaStatus { get; set; }
        public string SoaCurrency { get; set; }
        public string SoaUserCreate { get; set; }
        public string CurrencyLocal { get; set; }
    }
}
