namespace eFMS.API.Report.DL.Models
{
    class SaleKickBackReportParameter
    {
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public string Contact { get; set; }
        public string Currency { get; set; }
        public decimal TotalIncomes { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal TotalProfit { get; set; }
    }
}
