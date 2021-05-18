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
        /// <summary>Origin Container Freight Station Fee</summary>
        public decimal? TotalSellCFSFee { get; set; }
        /// <summary>Security EBS</summary>
        public decimal? TotalSellEBSFee { get; set; }
        /// <summary>Total Pick up charge + Customs fee</summary>
        public decimal? TotalSellCustomFee { get; set; }

        /// <summary>BUYING</summary>
        public decimal? TotalBuyTerminal { get; set; }
        public decimal? TotalBuyBillFee { get; set; }
        public decimal? TotalBuyContainerSealFee { get; set; }
        public decimal? TotalBuyTelexRelease { get; set; }
        public decimal? TotalBuyAutomated { get; set; }
        public decimal? TotalBuyVGM { get; set; }
        public decimal? TotalBuyBookingFee { get; set; }
        /// <summary>Origin Container Freight Station Fee</summary>
        public decimal? TotalBuyCFSFee { get; set; }
        /// <summary>Security EBS</summary>
        public decimal? TotalBuyEBSFee { get; set; }
        /// <summary>Total Pick up charge + Customs fee</summary>
        public decimal? TotalBuyCustomFee { get; set; }

        public string FinalDestination { get; set; }
        public string NotifyParty { get; set; }
        /// <summary>Incoterm</summary>
        public string Incoterm { get; set; }
    }
}
