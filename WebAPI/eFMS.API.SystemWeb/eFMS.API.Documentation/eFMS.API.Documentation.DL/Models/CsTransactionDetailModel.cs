using eFMS.API.Common.Models;
using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsTransactionDetailModel: CsTransactionDetail
    {
        public List<CsMawbcontainerModel> CsMawbcontainers { get; set; }
        public List<CsDimensionDetailModel> DimensionDetails { get; set; }
        public List<CsShipmentOtherChargeModel> OtherCharges { get; set; }

        public string CustomerName { get; set; }
        public string SaleManName { get; set; }
        public string ShipperName { get; set; }
        public string ConsigneeName { get; set; }
        public string CustomerNameVn { get; set; }
        public string SaleManNameVn { get; set; }
        public string ForwardingAgentName { get; set; }
        public string NotifyParty { get; set; }
        public string POLCode { get; set; }
        public string PODCode { get; set; }
        public string PODName { get; set; }
        public string POLName { get; set; }
        public string ContainerNames { get; set; }
        public string PackageTypes { get; set; }
        public decimal? CBM { get; set; }
        public decimal? CW { get; set; }
        public decimal? GW { get; set; }
        public string Packages { get; set; }
        public string Containers { get; set; }
        public DateTime? ShipmentEta { get; set; }
        public string TransactionType { get; set; }
        public DateTime? ShipmentEtd { get; set; }
        public string ShipmentMawb { get; set; }
        public string PackageTypeName { get; set; }
        public string ShipmentPIC { get; set; }

        public PermissionAllowBase Permission { get; set; }
    }
}
