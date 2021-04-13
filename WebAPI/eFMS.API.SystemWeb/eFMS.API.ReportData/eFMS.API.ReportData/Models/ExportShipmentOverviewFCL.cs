using eFMS.API.ReportData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class ExportShipmentOverviewFCL : ExportShipmentOverview
    {
        // SELLING
        public decimal? TotalSellTerminal { get; set; }
        public decimal? TotalSellBillFee{ get; set; }
        public decimal? TotalSellContainerSealFee { get; set; }
        public decimal? TotalSellTelexRelease { get; set; }
        public decimal? TotalSellAutomated { get; set; }
        public decimal? TotalSellVGM { get; set; }
        public decimal? TotalSellBookingFee { get; set; }
        // Origin Container Freight Station Fee
        public decimal? TotalSellCFSFee { get; set; }
        // Security EBS
        public decimal? TotalSellEBSFee { get; set; }
        // BUYING
        public decimal? TotalBuyTerminal { get; set; }
        public decimal? TotalBuyBillFee { get; set; }
        public decimal? TotalBuyContainerSealFee { get; set; }
        public decimal? TotalBuyTelexRelease { get; set; }
        public decimal? TotalBuyAutomated { get; set; }
        public decimal? TotalBuyVGM { get; set; }
        public decimal? TotalBuyBookingFee { get; set; }
        // Origin Container Freight Station Fee
        public decimal? TotalBuyCFSFee { get; set; }
        // Security EBS
        public decimal? TotalBuyEBSFee { get; set; }
        public string FinalDestination { get; set; }
        public string NotifyParty { get; set; }
    }
}
