﻿using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Documentation.DL.Services
{
    public class CheckPointService : ICheckPointService
    {
        private eFMSDataContextDefault DC => (eFMSDataContextDefault)csSurchargeRepository.DC;

        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IContextBase<AccAccountingManagement> accAccountMngtRepository;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<SysOffice> sysOfficeRepository;
        private readonly IContextBase<CatContract> contractRepository;
        private readonly IContextBase<OpsTransaction> opsTransactionRepository;
        private readonly IContextBase<CsTransaction> csTransactionRepository;
        private readonly IContextBase<CsShipmentSurcharge> csSurchargeRepository;
        private readonly IContextBase<CsTransactionDetail> csTransactionDetail;
        private readonly IContextBase<SysSettingFlow> sysSettingFlowRepository;
        private readonly IContextBase<AcctAdvanceRequest> advanceRequestRepository;
        private readonly IContextBase<AcctAdvancePayment> advancePaymentRepository;
        private readonly IContextBase<AcctSettlementPayment> settlementPaymentRepository;
        private readonly IContextBase<CsLinkCharge> csLinkChargeRepository;

        readonly string salemanBOD = string.Empty;
        readonly IQueryable<CsTransaction> csTransactions;
        readonly IQueryable<OpsTransaction> opsTransactions;
        readonly Guid? HM = Guid.Empty;
        readonly Guid? BH = Guid.Empty;

        public CheckPointService(ICurrentUser currUser,
            IContextBase<SysUser> sysUserRepository,
            IContextBase<AccAccountingManagement> accAccountMngtRepository,
            IContextBase<CatPartner> catPartnerRepository,
            IContextBase<SysOffice> sysOfficeRepository,
            IContextBase<CatContract> contractRepo,
            IContextBase<OpsTransaction> opsTransactionRepo,
            IContextBase<CsTransaction> csTransactionRepo,
            IContextBase<CsShipmentSurcharge> csSurcharge,
            IContextBase<CsTransactionDetail> csTransactionDetailRepo,
            IContextBase<AcctAdvanceRequest> advanceRequest,
            IContextBase<AcctAdvancePayment> advancePayment,
            IContextBase<AcctSettlementPayment> settlementPayment,
            IContextBase<CsLinkCharge> csLinkCharge,
            IContextBase<SysSettingFlow> sysSettingFlow
            )
        {
            this.currentUser = currUser;
            this.sysUserRepository = sysUserRepository;
            this.accAccountMngtRepository = accAccountMngtRepository;
            this.catPartnerRepository = catPartnerRepository;
            this.sysOfficeRepository = sysOfficeRepository;
            this.contractRepository = contractRepo;
            this.opsTransactionRepository = opsTransactionRepo;
            this.csTransactionRepository = csTransactionRepo;
            this.csSurchargeRepository = csSurcharge;
            this.csTransactionDetail = csTransactionDetailRepo;
            sysSettingFlowRepository = sysSettingFlow;
            advanceRequestRepository = advanceRequest;
            advancePaymentRepository = advancePayment;
            settlementPaymentRepository = settlementPayment;
            csLinkChargeRepository = csLinkCharge;

            salemanBOD = sysUserRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault()?.Id;

            csTransactions = csTransactionRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);
            opsTransactions = opsTransactionRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);

            HM = sysOfficeRepository.Get(x => x.Code == DocumentConstants.OFFICE_HM)?.FirstOrDefault()?.Id;
            BH = sysOfficeRepository.Get(x => x.Code == DocumentConstants.OFFICE_BH)?.FirstOrDefault()?.Id;
        }

        public bool ValidateCheckPointCashContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode, CHECK_POINT_TYPE checkPointType)
        {
            bool valid = true;
            IQueryable<CsShipmentSurcharge> surchargeToCheck = Enumerable.Empty<CsShipmentSurcharge>().AsQueryable();
            // Những lô hàng # saleman
            var hblIds = new List<Guid>();
            string salemanCurrent = null;
            if (transactionType == "CL")
            {
                var hbl = opsTransactionRepository.Get(x => x.Hblid == HblId)?.FirstOrDefault();
                salemanCurrent = hbl?.SalemanId;
                if (hbl?.SalemanId == salemanBOD || hbl?.ShipmentType == DocumentConstants.SHIPMENT_TYPE_NOMINATED)
                {
                    return valid;
                }
            }
            else
            {
                var hbl = csTransactionDetail.Get(x => x.Id == HblId)?.FirstOrDefault();
                salemanCurrent = hbl?.SaleManId;

                if (hbl?.SaleManId == salemanBOD)
                {
                    return valid;
                }

                if (!string.IsNullOrEmpty(hbl?.ShipmentType))
                {
                    if (hbl?.ShipmentType == DocumentConstants.SHIPMENT_TYPE_NOMINATED)
                    {
                        return valid;
                    }
                }
                else
                {
                    var jobcs = DC.CsTransaction.FirstOrDefault(x => x.Id == hbl.JobId);
                    if (jobcs?.ShipmentType == DocumentConstants.SHIPMENT_TYPE_NOMINATED)
                    {
                        return valid;
                    }
                }
            }

            hblIds = GetHblIdShipmentSameSaleman(HblId, salemanCurrent);

            if (hblIds.Count > 0)
            {
                surchargeToCheck = csSurchargeRepository.Get(x => x.PaymentObjectId == partnerId
                && hblIds.Contains(x.Hblid)
                && x.Hblid != HblId);
            }
            else
            {
                surchargeToCheck = csSurchargeRepository.Get(x => x.PaymentObjectId == partnerId
                && x.Hblid != HblId);
            }

            // Trường hợp nhập charge trong SM
            if (!string.IsNullOrEmpty(settlementCode))
            {
                surchargeToCheck = surchargeToCheck.Where(x => x.SettlementCode != settlementCode);
            }

            if (surchargeToCheck.Count() == 0)
            {
                return valid;
            }

            IQueryable<CsShipmentSurcharge> surchargeSellOBH = surchargeToCheck.Where(x =>
                (x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.AcctManagementId == null)
             || (x.Type == DocumentConstants.CHARGE_OBH_TYPE && x.AcctManagementId == null)
                );

            if (surchargeSellOBH.Count() > 0)
            {
                valid = false;
            }
            else
            {
                IQueryable<AccAccountingManagement> accMngt = accAccountMngtRepository.Get(x => x.PartnerId == partnerId
                && (x.PaymentStatus == DocumentConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART
                || x.PaymentStatus == DocumentConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID)
                );
                // Không có hoá đơn treo
                if (accMngt.Count() == 0)
                {
                    return valid;
                }
                IQueryable<CsShipmentSurcharge> surchargeWithInvoice = surchargeToCheck.Where(x => x.AcctManagementId != null);
                IQueryable<Guid> qInvoiceInvalid = from sur in surchargeWithInvoice
                                                   join invoice in accMngt on sur.AcctManagementId equals invoice.Id into invoiceGrps
                                                   from invoiceGrp in invoiceGrps.DefaultIfEmpty()
                                                   select invoiceGrp.Id;

                if (qInvoiceInvalid.Count() > 0)
                {
                    valid = false;
                }
            }
           
            return valid;
        }

        private bool ValidateCheckPointPrepaidContractPartner(Guid HblId, string partnerId, string transactionType)
        {
            bool valid = true;
            var surcharges = csSurchargeRepository.Get(x => (x.Type == DocumentConstants.CHARGE_SELL_TYPE || x.Type == DocumentConstants.CHARGE_OBH_TYPE) && x.Hblid == HblId);
            if (surcharges.Count() == 0)
            {
                return false;
            }

            var hasIssuedDebit = surcharges.Any(x => string.IsNullOrEmpty(x.DebitNo));
            if (hasIssuedDebit)
            {
                return false;
            }
            var debitCodes = surcharges.GroupBy(x => x.DebitNo).Select(x => x.FirstOrDefault().DebitNo).ToList();
            var debitNotes = DC.AcctCdnote.Where(x => debitCodes.Contains(x.Code)
                            && (x.Type == DocumentConstants.CDNOTE_TYPE_DEBIT || x.Type == DocumentConstants.CDNOTE_TYPE_INVOICE));
            var hasConfirm = debitNotes.Any(x => x.Status != DocumentConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
            if (hasConfirm)
            {
                valid = false;
            }

            return valid;
        }

        public bool ValidateCheckPointOfficialTrialContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode, CHECK_POINT_TYPE checkPointType)
        {
            // Hết hạn, vượt hạn mức, treo công nợ
            bool valid = true;


            return valid;
        }

        public HandleState ValidateCheckPointPartnerDebitNote(string partnerId, Guid HblId, string transactionType)
        {
            throw new NotImplementedException();
        }

        public HandleState ValidateCheckPointPartnerSOA(string partnerId, AcctSoa soa)
        {

            HandleState result = new HandleState();
            bool isValid = false;

            if (soa.Type != DocumentConstants.SOA_TYPE_DEBIT)
            {
                return result;
            }

            CatContract contract = GetContractByPartnerId(partnerId);
            CatPartner partner = catPartnerRepository.Get(x => x.Id == partnerId)?.FirstOrDefault();
            if (contract == null)
            {
                return new HandleState((object)string.Format(@"{0} doesn't have any agreement or agreement have expired", partner.ShortName));
            }
            if (contract.SaleManId == salemanBOD) return result;

            switch (contract.ContractType)
            {
                case "Cash":
                    if (IsSettingFlowApplyContract(contract.ContractType, currentUser.OfficeID, partner.PartnerType))
                    {
                        isValid = false;
                    }
                    else
                        isValid = true; // K cho gôm SOA debit hđ CASH
                    break;
                case "Trial":
                case "Official":
                    if (IsSettingFlowApplyContract(contract.ContractType, currentUser.OfficeID, partner.PartnerType))
                    {
                        isValid = true;
                    }
                    else
                        isValid = true;
                    break;
                default:
                    isValid = true;
                    break;
            }

            return result;
        }

        public HandleState ValidateCheckPointMultiplePartnerSurcharge(CheckPointCriteria criteria)
        {
            HandleState result = new HandleState();
            if (criteria.Data.Count == 0) return result;
            foreach (var partner in criteria.Data)
            {
                var isValid = ValidateCheckPointPartnerSurcharge(partner.PartnerId, partner.HblId ?? Guid.Empty, criteria.TransactionType, criteria.Type, criteria.SettlementCode);
                if (!isValid.Success)
                {
                    return isValid;
                }
            }
            return result;
        }

        public HandleState ValidateCheckPointPartnerSurcharge(string partnerId, Guid HblId, string transactionType, CHECK_POINT_TYPE checkPointType, string settlementCode)
        {
            HandleState result = new HandleState();
            bool isValid = false;
            string currentSaleman = string.Empty;
            CatPartner partner = catPartnerRepository.First(x => x.Id == partnerId);
            CatContract contract;
            if (partner.PartnerMode == "Internal")
            {
                return result;
            }
            if (HblId == Guid.Empty)
            {
                contract = GetContractByPartnerId(partnerId);
                currentSaleman = contract?.SaleManId;
            }
            else
            {
                if (transactionType == "CL")
                {
                    currentSaleman = opsTransactionRepository.First(x => x.Hblid == HblId)?.SalemanId;
                    contract = GetContractByPartnerId(partnerId, currentSaleman);
                }
                else
                {
                    currentSaleman = csTransactionDetail.First(x => x.Id == HblId)?.SaleManId;
                    contract = GetContractByPartnerId(partnerId, currentSaleman);
                }
            }

            if (currentSaleman == salemanBOD)
            {
                isValid = true;
                return result;
            }

            if (contract == null)
            {
                return new HandleState((object)string.Format(@"{0} doesn't have any agreement please you check again", partner?.ShortName));
            }

            int errorCode = -1;
            switch (contract.ContractType)
            {
                case "Cash":
                    if (checkPointType == CHECK_POINT_TYPE.DEBIT_NOTE || checkPointType == CHECK_POINT_TYPE.HBL || checkPointType == CHECK_POINT_TYPE.SHIPMENT)
                    {
                        isValid = true;
                        break;
                    }
                    if (IsSettingFlowApplyContract(contract.ContractType, currentUser.OfficeID, partner.PartnerType))
                    {
                        isValid = ValidateCheckPointCashContractPartner(partnerId, HblId, transactionType, settlementCode, CHECK_POINT_TYPE.SURCHARGE);
                    }
                    else isValid = true;
                    if (!isValid) errorCode = 1;

                    break;
                case "Trial":
                case "Official":
                case "Guarantee":
                    if (checkPointType == CHECK_POINT_TYPE.PREVIEW_HBL)
                    {
                        isValid = true;
                        break;
                    }
                    if (IsSettingFlowApplyContract(contract.ContractType, currentUser.OfficeID, partner.PartnerType, "overdue"))
                    {
                        if (checkPointType == CHECK_POINT_TYPE.DEBIT_NOTE) // hđ quá hạn vẫn cho issue DEBIT.
                        {
                            isValid = true;
                        }
                        else if (contract.IsOverDue == true)
                        {
                            isValid = false;
                        }
                        else isValid = true;
                        if (!isValid)
                        {
                            errorCode = 2;
                            break;
                        }
                    }
                    if (IsSettingFlowApplyContract(contract.ContractType, currentUser.OfficeID, partner.PartnerType, "credit"))
                    {
                        if (checkPointType == CHECK_POINT_TYPE.DEBIT_NOTE) //vẫn cho issue debit nếu vượt hạn mức
                        {
                            isValid = true;
                        }
                        else if (contract.IsOverLimit == true)
                        {
                            isValid = false;
                        }
                        else isValid = true;
                        if (!isValid)
                        {
                            errorCode = 4;
                            break;
                        }
                    }
                    if (IsSettingFlowApplyContract(contract.ContractType, currentUser.OfficeID, partner.PartnerType, "expired"))
                    {
                        if (checkPointType == CHECK_POINT_TYPE.DEBIT_NOTE)
                        {
                            if (contract.IsExpired == true)
                            {
                                isValid = false;
                            }
                            else isValid = true;
                        }
                        else if (contract.IsExpired == true)
                        {
                            isValid = false;
                        }
                        else isValid = true;
                        if (!isValid)
                        {
                            errorCode = 3;
                            break;
                        }
                    }
                    else isValid = true;
                    break;
                case "Prepaid":
                    if (checkPointType == CHECK_POINT_TYPE.PREVIEW_HBL)
                    {
                        isValid = ValidateCheckPointPrepaidContractPartner(HblId, partnerId, transactionType);
                    } else
                    {
                        isValid = true;
                    }
                    if (!isValid) errorCode = 5;

                    break;
                default:
                    isValid = true;
                    break;
            }
            string messError = null;
            if (isValid == false)
            {
                SysUser saleman = sysUserRepository.Get(x => x.Id == contract.SaleManId)?.FirstOrDefault();
                switch (errorCode)
                {
                    case 1:
                        messError = string.Format(@"{0} - {1} {2} agreement of {3} have shipment that not paid yet, please you check it again!",
                  partner?.TaxCode, partner?.ShortName, contract.ContractType, saleman.Username);
                        break;
                    case 2:
                        messError = string.Format(@"{0} - {1} {2} agreement of {3} have Over Due, please you check it again!",
                  partner?.TaxCode, partner?.ShortName, contract.ContractType, saleman.Username);
                        break;
                    case 3:
                        messError = string.Format(@"{0} - {1} {2} agreement of {3} is Expired, please you check it again!",
                  partner?.TaxCode, partner?.ShortName, contract.ContractType, saleman.Username);
                        break;
                    case 4:
                        messError = string.Format(@"{0} - {1} {2} agreement of {3} is Over Credit Limit {4}%, please you check it again!",
                  partner?.TaxCode, partner?.ShortName, contract.ContractType, saleman.Username, contract.CreditRate);
                        break;
                    case 5:
                        messError = string.Format(@"Contract of {0} is Prepaid. Please issue Prepaid Debit and wait AR confirm",
                            partner?.ShortName);
                        break;
                    default:
                        break;
                }
                return new HandleState((object)messError);
            }
            return result;
        }

        private List<Guid> GetHblIdShipmentSameSaleman(Guid hblId, string salemanHBLCurrent)
        {
            List<Guid> hblIds = new List<Guid>();

            hblIds = csTransactionDetail.Get(x => x.SaleManId == salemanHBLCurrent
               && x.SaleManId != salemanBOD)
               .Select(x => x.Id)
               .ToList();

            var opsHblids = opsTransactionRepository.Get(x => x.SalemanId == salemanHBLCurrent
                  && x.SalemanId != salemanBOD
                  && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED)
                   .Select(x => x.Hblid)
                   .ToList();

            hblIds.AddRange(opsHblids);

            return hblIds;
        }

        private CatContract GetContractByPartnerId(string partnerId, string saleman = "")
        {
            CatContract contract = null;
            if (string.IsNullOrEmpty(saleman))
            {
                contract = contractRepository.Get(x => x.PartnerId == partnerId
                                       && x.Active == true).OrderBy(x => x.ContractType)
                                       .FirstOrDefault();
            }
            else
            {
                var contracts = contractRepository.Get(x => x.PartnerId == partnerId && x.Active == true);
                if (contracts.Count() > 1)
                {
                    contract = contracts.FirstOrDefault(x => x.SaleManId == saleman);
                }
                else
                {
                    contract = contracts.FirstOrDefault();
                }
            }
            return contract;
        }

        private bool IsSettingFlowApplyContract(string ContractType, Guid officeId, string partnerType, string typeCheckPoint = null)
        {
            bool IsApplySetting = false;
            var settingFlow = sysSettingFlowRepository.First(x => x.OfficeId == officeId && x.Type == "AccountReceivable"
            && x.ApplyType != DocumentConstants.SETTING_FLOW_APPLY_TYPE_NONE
            && x.ApplyPartner != DocumentConstants.SETTING_FLOW_APPLY_TYPE_NONE);
            if (settingFlow == null) return IsApplySetting;
            switch (ContractType)
            {
                case "Cash":
                    IsApplySetting = IsApplySettingFlowContractCash(settingFlow.ApplyType, settingFlow.ApplyPartner, settingFlow.IsApplyContract, partnerType);
                    break;
                case "Trial":
                case "Official":
                case "Guarantee":
                    IsApplySetting = IsApplySettingFlowContractTrialOfficial(settingFlow, partnerType, typeCheckPoint);
                    break;
                default:
                    break;
            }

            return IsApplySetting;
        }

        private bool IsApplySettingFlowContractCash(string applyType, string applyPartnerType, bool? isApplyContract, string partnerType)
        {
            bool isApply = false;
            isApply = applyType == DocumentConstants.SETTING_FLOW_APPLY_TYPE_CHECK_POINT
                && isApplyContract == true
                && (applyPartnerType == partnerType || applyPartnerType == DocumentConstants.SETTING_FLOW_APPLY_PARTNER_TYPE_BOTH);

            return isApply;
        }

        private bool IsApplySettingFlowContractTrialOfficial(SysSettingFlow setting, string partnerType, string typeCheckPoint)
        {
            bool isApply = false;
            isApply = setting.ApplyType == DocumentConstants.SETTING_FLOW_APPLY_TYPE_CHECK_POINT
                && setting.ApplyType != DocumentConstants.SETTING_FLOW_APPLY_TYPE_NONE
                && (setting.ApplyPartner == partnerType || setting.ApplyPartner == DocumentConstants.SETTING_FLOW_APPLY_PARTNER_TYPE_BOTH);
            if (typeCheckPoint == "credit")
            {
                isApply = isApply && setting.CreditLimit == true;
            }
            if (typeCheckPoint == "overdue")
            {
                isApply = isApply && setting.OverPaymentTerm == true;
            }
            if (typeCheckPoint == "expired")
            {
                isApply = isApply && setting.ExpiredAgreement == true;
            }
            return isApply;
        }

        public List<CheckPointPartnerHBLDataGroup> GetPartnerForCheckPointInShipment(Guid Id, string transactionType)
        {
            List<CheckPointPartnerHBLDataGroup> partners = new List<CheckPointPartnerHBLDataGroup>();
            IQueryable<CsShipmentSurcharge> surcharges = Enumerable.Empty<CsShipmentSurcharge>().AsQueryable();
            Expression<Func<CsShipmentSurcharge, bool>> querySurcharge = x => x.OfficeId != HM
                && x.OfficeId != BH
                && (x.Type == DocumentConstants.CHARGE_OBH_TYPE || x.Type == DocumentConstants.CHARGE_SELL_TYPE);
            querySurcharge = querySurcharge.And(x => x.IsRefundFee != true);

            var hbls = Enumerable.Empty<CsTransactionDetail>().AsQueryable();
            if (transactionType == "CL")
            {
                querySurcharge = querySurcharge.And(x => x.Hblid == Id);
            }
            else
            {
                hbls = csTransactionDetail.Get(x => x.JobId == Id);
                var hblIds = hbls.Select(x => x.Id).ToList();
                querySurcharge = querySurcharge.And(x => hblIds.Contains(x.Hblid));
            }
            surcharges = csSurchargeRepository.Get(querySurcharge);

            if (surcharges.Count() > 0)
            {
                partners = surcharges.GroupBy(x => new { x.PaymentObjectId }).Select(x => new CheckPointPartnerHBLDataGroup { PartnerId = x.Key.PaymentObjectId, HblId = x.FirstOrDefault().Hblid }).ToList();
            }
            else
            {
                if (transactionType == "CL")
                {
                    var opsJob = opsTransactionRepository.First(x => x.Hblid == Id);
                    if (opsJob != null)
                    {
                        partners.Add(new CheckPointPartnerHBLDataGroup { HblId = opsJob.Hblid, PartnerId = opsJob.CustomerId });

                        return partners;
                    }
                }
                if (hbls.Count() == 0)
                {
                    return partners;
                }
                partners = hbls.Select(x => new CheckPointPartnerHBLDataGroup { HblId = x.Id, PartnerId = x.CustomerId }).ToList();
            }

            return partners;
        }

        /// <summary>
        /// Check if shipment allow check no profit
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool AllowCheckNoProfitShipment(string jobNo, bool? isCheked)
        {
            if (isCheked == true)
            {
                var surcharges = csSurchargeRepository.Get(x => x.Type != "OBH" && x.JobNo == jobNo);
                if (surcharges.Count() <= 0)
                {
                    return true;
                }
                var transaction = surcharges.Select(x => x.TransactionType).FirstOrDefault();
                var shipmentNoProfit = false;
                if (transaction == "CL")
                {
                    shipmentNoProfit = opsTransactionRepository.Get(x => x.JobNo == jobNo).FirstOrDefault()?.NoProfit ?? false;
                }
                else
                {
                    shipmentNoProfit = csTransactionRepository.Get(x => x.JobNo == jobNo).FirstOrDefault()?.NoProfit ?? false;
                }

                if (surcharges.Count() > 0 && !shipmentNoProfit)
                {
                    // [CR:09/05/2022]: so sánh profit trên tổng của lô hàng
                    var buyAmount = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.JobNo == jobNo).Sum(x => x.AmountVnd ?? 0);
                    var sellAmount = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.JobNo == jobNo).Sum(x => x.AmountVnd ?? 0);
                    if (sellAmount - buyAmount > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///  Check if duplicate shipment allow check no profit
        /// </summary>
        /// <param name="jobNo"></param>
        /// <param name="isCheked"></param>
        /// <param name="isReplicate">shipment has Job Replicate</param>
        /// <param name="shipmentInvalid"></param>
        /// <returns></returns>
        public bool AllowCheckNoProfitShipmentDuplicate(string jobNo, bool? isCheked, bool isReplicate, out string shipmentInvalid)
        {
            shipmentInvalid = string.Empty;
            if (isCheked == true)
            {
                var surcharges = csSurchargeRepository.Get(x => x.Type != "OBH" && x.IsFromShipment == true  && x.JobNo == jobNo);
                if(surcharges.Count() <= 0)
                {
                    return true;
                }
                var transaction = surcharges.Select(x => x.TransactionType).FirstOrDefault();
                var shipmentNoProfit = false;
                if (transaction == "CL")
                {
                    var opstraction = opsTransactionRepository.Get(x => x.JobNo == jobNo).FirstOrDefault();
                    shipmentNoProfit = opstraction?.NoProfit ?? false;
                    surcharges = surcharges.Where(x => string.IsNullOrEmpty(x.LinkChargeId));
                    if (isReplicate && !shipmentNoProfit)
                    {
                        // Không lấy phí đã AutoRate
                        var jobRepNo = opsTransactionRepository.Get(x => x.Id == opstraction.ReplicatedId).FirstOrDefault()?.JobNo;
                        var surchargesRep = csSurchargeRepository.Get(x => x.Type != "OBH" && x.IsFromShipment == true && string.IsNullOrEmpty(x.LinkChargeId) && x.JobNo == jobRepNo);
                        List<CsLinkCharge> csLinkFee = csLinkChargeRepository.Get(x => x.JobNoLink == jobRepNo && x.LinkChargeType == DocumentConstants.LINK_CHARGE_TYPE_AUTO_RATE).ToList();
                        if (csLinkFee.Count() > 0)
                        {
                            List<string> listChargeExisted = csLinkFee.Select(x => x.ChargeLinkId).ToList();
                            surchargesRep = surchargesRep.Where(x => !listChargeExisted.Contains(x.Id.ToString()));
                            if (surchargesRep.Count() > 0)
                            {
                                surcharges = surcharges.Union(surchargesRep);
                            }
                        }
                    }
                }
                else
                {
                    var cstransaction = csTransactionRepository.Get(x => x.JobNo == jobNo).FirstOrDefault();
                    shipmentNoProfit = cstransaction?.NoProfit ?? false;
                    if (!shipmentNoProfit)
                    {
                        //[01/03/2022] Không lấy những phí đã linkFee
                        List<CsLinkCharge> csLinkFee = csLinkChargeRepository.Get(x => x.JobNoLink == cstransaction.JobNo && x.LinkChargeType == DocumentConstants.LINK_CHARGE_TYPE_LINK_FEE).ToList();
                        if (csLinkFee.Count > 0)
                        {
                            List<string> listChargeExisted = csLinkFee.Select(x => x.ChargeLinkId).ToList();
                            surcharges = surcharges.Where(x => !listChargeExisted.Contains(x.Id.ToString()));
                        }
                    }
                }

                if (surcharges.Count() > 0 && !shipmentNoProfit)
                {
                    // [CR:09/05/2022]: so sánh profit trên tổng của lô hàng
                    var jobNos = surcharges.Select(x => x.JobNo).Distinct().ToList();
                    foreach (var shipmentNo in jobNos)
                    {
                        var buyAmount = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_BUY_TYPE && x.JobNo == shipmentNo).Sum(x => x.AmountVnd ?? 0);
                        var sellAmount = surcharges.Where(x => x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.JobNo == shipmentNo).Sum(x => x.AmountVnd ?? 0);
                        if (sellAmount - buyAmount > 0)
                        {
                            shipmentInvalid = surcharges.Where(x => x.JobNo == shipmentNo).FirstOrDefault().JobNo;
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Check if can uncheck no profit shipment
        /// </summary>
        /// <param name="jobNo"></param>
        /// <param name="isCheked"></param>
        /// <returns></returns>
        public bool AllowUnCheckNoProfitShipment(string jobNo, bool? isCheked)
        {
            if (isCheked == false)
            {
                var surcharges = csSurchargeRepository.Get(x => x.Type != "OBH" && x.JobNo == jobNo);
                if (surcharges.Count() <= 0)
                {
                    return true;
                }
                var transaction = surcharges.Select(x => x.TransactionType).FirstOrDefault();
                var shipmentNoProfit = false;
                if(transaction == "CL")
                {
                    shipmentNoProfit = opsTransactionRepository.Get(x => x.JobNo == jobNo).FirstOrDefault()?.NoProfit ?? false;
                }
                else
                {
                    shipmentNoProfit = csTransactionRepository.Get(x => x.JobNo == jobNo).FirstOrDefault()?.NoProfit ?? false;
                }

                if (shipmentNoProfit) // Shipment with checked no profit
                {
                    // Check Advance
                    var advanceNos = advanceRequestRepository.Get(x => x.JobId == jobNo).Select(x => x.AdvanceNo).Distinct().ToList();
                    if (advanceNos.Count > 0)
                    {
                        var payees = advancePaymentRepository.Get(x => !string.IsNullOrEmpty(x.Payee) && advanceNos.Any(z => z == x.AdvanceNo)).Select(x => x.Payee);
                        foreach (var payee in payees)
                        {
                            if (catPartnerRepository.Any(x => x.Id == payee && !x.PartnerGroup.ToLower().Contains("staff")))
                            {
                                return false;
                            }
                        }
                    }

                    // Check Settlement
                    var settleNos = surcharges.Where(x => !string.IsNullOrEmpty(x.SettlementCode)).Select(x => x.SettlementCode).Distinct().ToList();
                    if (settleNos.Count > 0)
                    {
                        var payees = settlementPaymentRepository.Get(x => !string.IsNullOrEmpty(x.Payee) && settleNos.Any(z => z == x.SettlementNo)).Select(x => x.Payee);
                        foreach (var payee in payees)
                        {
                            if (catPartnerRepository.Any(x => x.Id == payee && !x.PartnerGroup.ToLower().Contains("staff")))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
