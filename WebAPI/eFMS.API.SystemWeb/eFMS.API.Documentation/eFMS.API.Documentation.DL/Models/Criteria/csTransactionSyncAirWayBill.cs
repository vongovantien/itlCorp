using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class csTransactionSyncAirWayBill
    {
        public string FlightNo { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public DateTime? FlightDate { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public string IssuedBy { get; set; }
        public Guid? WarehouseId { get; set; }
        public string Mawb { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? ChargeWeight { get; set; }
        public decimal? Hw { get; set; }
        public decimal? Cbm { get; set; }
        public int? PackageQty { get; set; }
        public List<CsDimensionDetailModel> DimensionDetails { get; set; }
    }
}