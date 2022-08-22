using eFMS.API.Documentation.DL.Common;
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

            /*
            // Tạm thời pending check issue debit
            if (valid == false && checkPointType == CHECK_POINT_TYPE.DEBIT_NOTE)
            {
                IQueryable<object> groupHblIdCs = Enumerable.Empty<object>().AsQueryable();
                IQueryable<object> groupHblIdOps = Enumerable.Empty<object>().AsQueryable();

                var hblidShipments = GetHblIdShipmentSameSaleman(HblId, salemanCurrent);
                // K check cùng service, có thể dính lô khác service
                groupHblIdOps = csSurchargeRepository.Get(x => x.PaymentObjectId == partnerId && hblidShipments.Contains(x.Hblid))
                   .Where(x => x.TransactionType == DocumentConstants.LG_SHIPMENT)
                   .GroupBy(x => new { x.JobNo, x.Hblid, x.TransactionType })
                   .SelectMany(j => j, (j, r) => new CsShipmentSurcharge() { JobNo = r.JobNo, Hblid = r.Hblid });

                groupHblIdCs = csSurchargeRepository.Get(x => x.PaymentObjectId == partnerId && hblidShipments.Contains(x.Hblid))
                    .Where(x => x.TransactionType != DocumentConstants.LG_SHIPMENT)
                    .GroupBy(x => new { x.JobNo, x.Hblid, x.TransactionType })
                    .SelectMany(x => x, (x, y) => new CsShipmentSurcharge() { JobNo = y.JobNo, Hblid = y.Hblid });

                var qGrServiceDateShipmentOps = Enumerable.Empty<object>().AsQueryable();
                var qGrServiceDateShipmentCs = Enumerable.Empty<object>().AsQueryable();
                object oldestShipment = null;

                if (groupHblIdCs.Count() > 0)
                {
                    qGrServiceDateShipmentCs = from sur in groupHblIdCs
                                               join cs in csTransactions on ObjectUtility.GetValue(sur, "JobNo") equals cs.JobNo // make sur jobNo k dup, đúng vs hblId
                                               orderby cs.ServiceDate ascending
                                               select new { Hblid = ObjectUtility.GetValue(sur, "Hblid"), cs.ServiceDate };

                    if (qGrServiceDateShipmentCs.Count() > 0)
                    {
                        oldestShipment = qGrServiceDateShipmentCs.FirstOrDefault();

                        if (HblId.ToString() == ObjectUtility.GetValue(oldestShipment, "Hblid").ToString())
                        {
                            valid = true;
                        }
                    }
                }
                else if (groupHblIdOps.Count() > 0)
                {
                    qGrServiceDateShipmentOps = from sur in groupHblIdOps
                                                join ops in opsTransactions on ObjectUtility.GetValue(sur, "Hblid") equals ops.Hblid
                                                orderby ops.ServiceDate ascending
                                                select new { Hblid = ObjectUtility.GetValue(sur, "Hblid"), ops.ServiceDate };

                    if (qGrServiceDateShipmentOps.Count() > 0)
                    {
                        oldestShipment = qGrServiceDateShipmentOps.FirstOrDefault();

                        if (HblId.ToString() == ObjectUtility.GetValue(oldestShipment, "Hblid").ToString())
                        {
                            valid = true;
                        }
                    }
                }
            }
            */
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
    }
}
