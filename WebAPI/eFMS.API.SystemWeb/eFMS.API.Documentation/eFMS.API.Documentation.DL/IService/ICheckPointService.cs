﻿
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICheckPointService
    {
        HandleState ValidateCheckPointPartnerDebitNote(string partnerId, Guid HblId, string transactionType);
        HandleState ValidateCheckPointPartnerSurcharge(string partnerId, Guid HblId, string transactionType, CHECK_POINT_TYPE checkPointType, string settlementCode);
        HandleState ValidateCheckPointMultiplePartnerSurcharge(CheckPointCriteria criteria);
        HandleState ValidateCheckPointPartnerSOA(string partnerId, AcctSoa soa);
        bool ValidateCheckPointCashContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode, CHECK_POINT_TYPE checkPointType);
        bool ValidateCheckPointOfficialTrialContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode, CHECK_POINT_TYPE checkPointType);
        List<CheckPointPartnerHBLDataGroup> GetPartnerForCheckPointInShipment(Guid Id, string transactionType);
        bool AllowCheckNoProfitShipment(string jobNo, bool? isCheked);
        bool AllowUnCheckNoProfitShipment(string jobNo, bool? isCheked);
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
