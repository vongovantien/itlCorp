
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using System;
using System.ComponentModel;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICheckPointService
    {
        HandleState ValidateCheckPointPartnerDebitNote(string partnerId, Guid HblId, string transactionType);
        HandleState ValidateCheckPointPartnerSurcharge(string partnerId, Guid HblId, string transactionType, CHECK_POINT_TYPE checkPointType, string settlementCode);
        HandleState ValidateCheckPointPartnerSOA(string partnerId, AcctSoa soa);

        bool ValidateCheckPointCashContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode, CHECK_POINT_TYPE checkPointType);
        bool ValidateCheckPointOfficialTrialContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode, CHECK_POINT_TYPE checkPointType);
    }

    public enum CHECK_POINT_TYPE
    {
        [Description("Shipment")]
        SHIPMENT = 1,  
        [Description("SOA")]
        SOA = 2,
        [Description("Debit")]
        DEBIT_NOTE = 3, 
        [Description("Credit")]
        CREDIT_NOTE = 4,
        [Description("Surcharge")]
        SURCHARGE = 5,
        [Description("HBL")]
        HBL = 6,
        [Description("PREVIEW_HBL")]
        PREVIEW_HBL = 7,
    }
}
