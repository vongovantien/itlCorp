using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class GeneralExportShipmentOverviewFCLResult : GeneralExportShipmentOverviewResult
    {
        public decimal? TotalSellTerminal { get; set; }
        public decimal? TotalSellBillFee{ get; set; }
        public decimal? TotalSellContainerSealFee { get; set; }
        public decimal? TotalSellTelexRelease { get; set; }
        public decimal? TotalSellAutomated { get; set; }
        public decimal? TotalSellVGM { get; set; }
        public decimal? TotalSellBookingFee { get; set; }
        public decimal? TotalBuyTerminal { get; set; }
        public decimal? TotalBuyBillFee { get; set; }
        public decimal? TotalBuyContainerSealFee { get; set; }
        public decimal? TotalBuyTelexRelease { get; set; }
        public decimal? TotalBuyAutomated { get; set; }
        public decimal? TotalBuyVGM { get; set; }
        public decimal? TotalBuyBookingFee { get; set; }
        public string FinalDestination { get; set; }
    }
}
