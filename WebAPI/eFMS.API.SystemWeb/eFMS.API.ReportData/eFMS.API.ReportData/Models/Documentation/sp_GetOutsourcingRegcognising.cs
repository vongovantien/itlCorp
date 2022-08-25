using System;

namespace eFMS.API.ReportData.Models.Documentation
{
    public class sp_GetOutsourcingRegcognising
    {
        public Guid ChargeId { get; set; }
        public string PartnerName { get; set; }
        public string PartnerCode { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string DebitNo { get; set; }
        public string SOA { get; set; }
        public decimal NETAmount { get; set; }
        public decimal VATAmount { get; set; }
        public string LinkChargeId { get; set; }
        public string Creator { get; set; }
        public string CustomNo { get; set; }
        public string Customer { get; set; }
        public DateTime DateService { get; set; }
        public string HBL { get; set; }
        public string JobId { get; set; }
        public string ProductService { get; set; }
        public string ChargeType { get; set; }
    }
}
