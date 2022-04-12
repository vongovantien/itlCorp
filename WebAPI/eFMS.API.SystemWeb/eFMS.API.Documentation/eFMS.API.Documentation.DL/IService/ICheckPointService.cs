
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using System;
using System.ComponentModel;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICheckPointService
    {
        HandleState ValidateCheckPointPartnerDebitNote(string partnerId, Guid HblId, string transactionType);
        HandleState ValidateCheckPointPartnerSurcharge(string partnerId, Guid HblId, string transactionType, string settlementCode = "");
        HandleState ValidateCheckPointPartnerSOA(string partnerId, AcctSoa soa);

        bool ValidateCheckPointCashContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode, CHECK_POINT_TYPE checkPointType);
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
