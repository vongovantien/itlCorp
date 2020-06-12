using eFMS.API.Common.Models;
using eFMS.API.Documentation.Service.Models;
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
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public string POLCode { get; set; }
        public string PODCode { get; set; }
        public string ColoaderCode { get; set; }
        public WarehouseData WarehousePOD { get; set; }
        public WarehouseData WarehousePOL { get; set; }
        public string POLCountryNameEn { get; set; }
        public string POLCountryNameVn { get; set; }
        public string POLCountryCode { get; set; }
        public AgentData AgentData { get; set; }
        public OfficeData CreatorOffice { get; set; }
        public string GroupEmail { get; set; }
        public string RoundUpMethod { get; set; }
        public string ApplyDim { get; set; }
    }

    public class AgentData
    {
        public string NameEn { get; set; }
        public string NameVn { get; set; }
        public string Address { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
    }
    public class OfficeData
    {
        public string NameEn { get; set; }
        public string NameVn { get; set; }
        public string Location { get; set; }
        public string AddressEn { get; set; }
        public string Tel { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }

    }

    public class WarehouseData
    {
        public string NameEn { get; set; }
        public string NameVn { get; set; }
        public string NameAbbr { get; set; }
        public string Location { get; set; }

    }
}
