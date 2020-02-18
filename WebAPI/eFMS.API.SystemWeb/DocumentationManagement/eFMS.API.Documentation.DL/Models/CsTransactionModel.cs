using eFMS.API.Documentation.Service.Models;
using eFMS.API.Infrastructure.Models;
using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsTransactionModel: CsTransaction
    {
        public string SupplierName { get; set; }
        public string AgentName { get; set; }
        public string HWBNo { get; set; }
        public string CustomerId { get; set; }
        public string NotifyPartyId { get; set; }
        public string SaleManId { get; set; }
        public string PODName { get; set; }
        public string POLName { get; set; }
        public string CreatorName { get; set; }
        public int? SumCont { get; set; }
        public int? SumPackage { get; set; }
        public Guid? HblId { get; set; }
        public string PlaceDeliveryName { get; set; }
        public PermissionAllowBase Permission { get; set; }
    }
}
