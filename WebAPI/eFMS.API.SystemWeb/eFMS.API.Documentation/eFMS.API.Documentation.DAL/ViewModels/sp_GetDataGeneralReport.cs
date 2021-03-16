using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class sp_GetDataGeneralReport
    {
        public string JobNo { get; set; }
        public string Mawb { get; set; }
        public Guid? HblId { get; set; }
        public string HwbNo { get; set; }
        public string CustomerId { get; set; }
        public string ColoaderId { get; set; }
        public string AgentId { get; set; }
        public int? PackageQty { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string PersonInCharge { get; set; }
        public string SalemanId { get; set; }
        public string TransactionType { get; set; }
        public decimal? ChargeWeight { get; set; }
        public string UserCreated { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public Guid OfficeId { get; set; }
        public int DepartmentId { get; set; }
        public int GroupId { get; set; }
        public string Shipper { get; set; }
        public string ConsigneeId { get; set; }
        public string ShipperDescription { get; set; }
        public string ConsigneeDescription { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? Cbm { get; set; }
        public string Pono { get; set; }
        public string Commodity { get; set; }
        public string PaymentTerm { get; set; }
        public string Notes { get; set; }
        public string FlightNo { get; set; }
        public string ShipmentType { get; set; }
        public int? Cont20 { get; set; }
        public int? Cont40 { get; set; }
        public int? Cont40HC { get; set; }
        public int? Cont45 { get; set; }
        public string ProductService { get; set; }
    }
}
