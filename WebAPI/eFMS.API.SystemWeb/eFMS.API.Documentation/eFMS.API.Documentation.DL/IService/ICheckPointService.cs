
using ITL.NetCore.Common;
using System;
using System.ComponentModel;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICheckPointService
    {
        HandleState ValidateCheckPointPartnerCd(string partnerId, Guid HblId, string transactionType, CHECK_POINT_TYPE type = CHECK_POINT_TYPE.DEBIT_NOTE);
        HandleState ValidateCheckPointPartnerSurcharge(string partnerId, Guid HblId, string transactionType, string settlementCode = "", CHECK_POINT_TYPE type = CHECK_POINT_TYPE.SURCHARGE);
        HandleState ValidateCheckPointPartnerSOA(string partnerId, Guid HblId, string transactionType, CHECK_POINT_TYPE type = CHECK_POINT_TYPE.SOA);

        bool ValidateCheckPointCashContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode);
        bool ValidateCheckPointOfficialTrialContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode);
    }

    public enum CHECK_POINT_TYPE
    {
        [Description("Shipment")]
        SHIPMENT = 1,  
        [Description("SOA")]
        SOA = 2, // ràng khi issue SOA DEBIT
        [Description("Debit")]
        DEBIT_NOTE = 3, // ràng khi issue DEBIT NOTE
        [Description("Credit")]
        CREDIT_NOTE = 4,
        [Description("Surcharge")]
        SURCHARGE = 5 // ràng khi nhập phí
    }
}
