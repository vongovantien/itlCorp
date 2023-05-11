using eFMS.API.Common.Models;
using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsWorkOrderModel: CsWorkOrder
    {
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public string PartnerName { get; set; }
        public string TransactionTypeName { get; set; }
    }

    public class CsWorkOrderViewModel
    {
        public Guid Id { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public string PartnerName { get; set; }
        public string SalesmanName { get; set; }
        public string PodCode { get; set; }
        public string PolCode { get; set; }
        public string ApprovedStatus { get; set; }
        public string Code { get; set; }
        public DateTime DatetimeCreated { get; set; }
        public DateTime DatetimeModified { get; set; }
        public string Service { get; set; }
        public bool Active { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public string UserCreated { get; set; }
    }

    public class CsWorkOrderViewUpdateModel: CsWorkOrderModel
    {
        public PermissionAllowBase Permission { get; set; }
        public string PolName { get; set; }
        public string PodName { get; set; }
        public string SalesmanName { get; set; }
        public string ShipperName { get; set; }
        public string AgentName { get; set; }
        public string ConsigneeName { get; set; }
        public List<CsWorkOrderPriceModel> ListPrice { get; set; }
    }
}
