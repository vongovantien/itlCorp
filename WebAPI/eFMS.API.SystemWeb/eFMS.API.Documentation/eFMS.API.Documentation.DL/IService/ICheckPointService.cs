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
        HandleState ValidateCheckPointPartnerSurcharge(CheckPoint criteria);
        HandleState ValidateCheckPointMultiplePartnerSurcharge(CheckPointCriteria criteria);
        HandleState ValidateCheckPointPartnerSOA(string partnerId, AcctSoa soa);
        HandleState ValidateCheckPointPartnerConvertClearance(string accountNo, string partnerTaxCode, out CatContract customerContract);
        bool ValidateCheckPointCashContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode, CHECK_POINT_TYPE checkPointType);
        bool ValidateCheckPointOfficialTrialContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode, CHECK_POINT_TYPE checkPointType);
        List<CheckPointPartnerHBLDataGroup> GetPartnerForCheckPointInShipment(Guid Id, string transactionType);
        bool AllowCheckNoProfitShipment(string jobNo, bool? isCheked);
        bool AllowCheckNoProfitShipmentDuplicate(string jobNo, bool? isCheked, bool isReplicate, out string shipmentInvalid);
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
        [Description("UPDATE_HBL")]
        UPDATE_HBL = 8,
        [Description("SurchargeOBH")]
        SURCHARGE_OBH = 9,
    }
}
