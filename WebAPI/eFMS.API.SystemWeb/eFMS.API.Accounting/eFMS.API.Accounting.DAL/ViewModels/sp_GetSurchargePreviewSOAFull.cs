namespace eFMS.API.Accounting.Service.ViewModels
{
    public class sp_GetSurchargePreviewSOAFull
    {
        public string PartnerID { get; set; }
        public string Docs { get; set; }
        public string PartnerName { get; set; }
        public string PersonalContact { get; set; }
        public string Address { get; set; }
        public string Workphone { get; set; }
        public string MAWB { get; set; }
        public string JobNo { get; set; }
        public string HWBNO { get; set; }
        public string CustomNo { get; set; }
        public string InvID { get; set; }
        public string Curr { get; set; }
        public bool Dpt { get; set; }
        public string CdCode { get; set; }
        public decimal Amount { get; set; }
        public decimal ROBH { get; set; }
    }
}
