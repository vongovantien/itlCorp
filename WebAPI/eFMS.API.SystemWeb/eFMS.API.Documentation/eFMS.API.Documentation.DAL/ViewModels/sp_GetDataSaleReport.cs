using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class sp_GetDataSaleReport
    {
        public int? DepartmentId { get; set; }
        public string TransactionType { get; set; }
        public string JobNo { get; set; }
        public string ShipmentType { get; set; }
        public Guid? Pod { get; set; }
        public Guid? Pol { get; set; }
        public string ColoaderId { get; set; }
        public string AgentId { get; set; }
        public string CustomerId { get; set; }
        public string NominationParty { get; set; }
        public Guid HblId { get; set; }
        public string HwbNo { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? ChargeWeight { get; set; }
        public decimal? CBM { get; set; }
        public string ShipperDescription { get; set; }
        public string ConsigneeDescription { get; set; }
        public string SalemanId { get; set; }
        public DateTime? Eta { get; set; }
        public DateTime? Etd { get; set; }
        public string TypeOfService { get; set; }
        public string ShipperId { get; set; }
        public string ConsigneeId { get; set; }
        public int? Cont40HC { get; set; }
        public int? Qty20 { get; set; }
        public int? Qty40 { get; set; }
        public string SalesDepartmentId { get; set; }
        public string DepartSaleManager { get; set; }
        public string DepartmentSale { get; set; }
        public decimal? Cbm { get; set; }
        public decimal? NetWeight { get; set; }

    }
}
