namespace eFMS.API.Report.DL.Models
{
    public class GeneralExportShipmentOverviewFCLResult: GeneralExportShipmentOverviewResult
    {
        public decimal? TotalSellTerminal { get; set; }
        public decimal? TotalSellBillFee { get; set; }
        public decimal? TotalSellContainerSealFee { get; set; }
        public decimal? TotalSellTelexRelease { get; set; }
        public decimal? TotalSellAutomated { get; set; }
        public decimal? TotalSellVGM { get; set; }
        public decimal? TotalSellBookingFee { get; set; }
        public decimal? TotalSellCFSFee { get; set; }
        public decimal? TotalSellEBSFee { get; set; }
        public decimal? TotalSellCustomFee { get; set; }
        public decimal? TotalBuyTerminal { get; set; }
        public decimal? TotalBuyBillFee { get; set; }
        public decimal? TotalBuyContainerSealFee { get; set; }
        public decimal? TotalBuyTelexRelease { get; set; }
        public decimal? TotalBuyAutomated { get; set; }
        public decimal? TotalBuyVGM { get; set; }
        public decimal? TotalBuyBookingFee { get; set; }
        public decimal? TotalBuyCFSFee { get; set; }
        public decimal? TotalBuyEBSFee { get; set; }
        public decimal? TotalBuyCustomFee { get; set; }
        public string FinalDestination { get; set; }
        public string NotifyParty { get; set; }
        public string Incoterm { get; set; }
    }
}
