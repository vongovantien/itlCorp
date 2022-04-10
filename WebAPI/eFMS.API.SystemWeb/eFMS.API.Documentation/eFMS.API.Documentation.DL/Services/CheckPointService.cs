using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CheckPointService : ICheckPointService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IContextBase<AccAccountingManagement> accAccountMngtRepository;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<SysOffice> sysOfficeRepository;
        private readonly IContextBase<CatContract> contractRepository;
        private readonly IContextBase<OpsTransaction> opsTransactionRepository;
        private readonly IContextBase<CsTransaction> csTransactionRepository;
        private readonly IContextBase<CsShipmentSurcharge> csSurchargeRepository;
        private readonly IContextBase<CsTransactionDetail> csDetailSurchargeRepository;

        string salemanBOD = string.Empty;
        IQueryable<CsTransaction> csTransactions;
        IQueryable<OpsTransaction> opsTransactions;

        public CheckPointService(ICurrentUser currUser,
            IContextBase<SysUser> sysUserRepository,
            IContextBase<AccAccountingManagement> accAccountMngtRepository,
            IContextBase<CatPartner> catPartnerRepository,
            IContextBase<SysOffice> sysOfficeRepository,
            IContextBase<CatContract> contractRepo,
            IContextBase<OpsTransaction> opsTransactionRepo,
            IContextBase<CsTransaction> csTransactionRepo,
            IContextBase<CsShipmentSurcharge> csSurcharge,
            IContextBase<CsTransactionDetail> csDetailSurchargeRepo
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
            this.csDetailSurchargeRepository = csDetailSurchargeRepo;

            salemanBOD = sysUserRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault()?.Id;

            csTransactions = csTransactionRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);
            opsTransactions = opsTransactionRepository.Get(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED);
        }

        public bool ValidateCheckPointCashContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode, CHECK_POINT_TYPE checkPointType)
        {
            bool valid = true;
            IQueryable<CsShipmentSurcharge> surchargeToCheck = Enumerable.Empty<CsShipmentSurcharge>().AsQueryable();
            // Những lô hàng # saleman
            var hblIds = new List<Guid>();
            string salemanCurrent = null;
            string salemanBOD = sysUserRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault()?.Id;

            if (HblId == Guid.Empty) // Mở một lô mới, bỏ qua những lô saleman ITL BOD
            {
                if (transactionType == "CL")
                {
                    hblIds = opsTransactionRepository.Get(x => x.SalemanId != salemanBOD
                            && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED)
                             .Select(x => x.Hblid)
                             .ToList();
                }
                else
                {
                    hblIds = csDetailSurchargeRepository.Get(x => x.SaleManId != salemanBOD)
                            .Select(x => x.Id)
                            .ToList();
                }

                if (hblIds.Count > 0)
                {
                    surchargeToCheck = csSurchargeRepository.Get(x => x.PaymentObjectId == partnerId && hblIds.Contains(x.Hblid));
                }
                else
                {
                    surchargeToCheck = csSurchargeRepository.Get(x => x.PaymentObjectId == partnerId);
                }


            }
            else // Check theo từng lô
            {
                if (transactionType == "CL")
                {
                    salemanCurrent = opsTransactionRepository.Get(x => x.Hblid == HblId)?.FirstOrDefault()?.SalemanId;
                    if (salemanCurrent == salemanBOD)
                    {
                        return valid;
                    }
                }
                else
                {
                    salemanCurrent = csDetailSurchargeRepository.Get(x => x.Id == HblId)?.FirstOrDefault()?.SaleManId;
                    if (salemanCurrent == salemanBOD)
                    {
                        return valid;
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

            return valid;
        }

        private List<Guid> GetHblIdShipmentSameSaleman(Guid hblId, string salemanHBLCurrent)
        {
            List<Guid> hblIds = new List<Guid>();

            hblIds = csDetailSurchargeRepository.Get(x => x.SaleManId == salemanHBLCurrent
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


        public bool ValidateCheckPointOfficialTrialContractPartner(string partnerId, Guid HblId, string transactionType, string settlementCode)
        {
            throw new NotImplementedException();
        }


        public HandleState ValidateCheckPointPartnerDebitNote(string partnerId, Guid HblId, string transactionType)
        {
            HandleState result = new HandleState();
            bool isValid = false;

            string salemanBOD = sysUserRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault()?.Id;

            CatContract contract = contractRepository.Get(x => x.PartnerId == partnerId
            && x.Active == true
            /// && x.SaleManId != salemanBOD
            && (x.IsExpired == false || x.IsExpired == null))
            .OrderBy(x => x.ContractType)
            // .ThenBy(c => c.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || c.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL)
            .FirstOrDefault();

            CatPartner partner = catPartnerRepository.Get(x => x.Id == partnerId)?.FirstOrDefault();
            if (contract == null)
            {
                return new HandleState((object)string.Format(@"{0} doesn't have any agreement  please you check again", partner.ShortName));
            }

            if (contract.SaleManId == salemanBOD) return result;

            switch (contract.ContractType)
            {
                case "Cash":
                    isValid = ValidateCheckPointCashContractPartner(partnerId, HblId, transactionType, string.Empty, CHECK_POINT_TYPE.DEBIT_NOTE);
                    break;
                //case "Official":
                //case "Trial":
                // isValid = ValidateCheckPointOfficialTrialContractPartner(Id, HblId);
                // break;
                default:
                    isValid = true;
                    break;
            }
            string messError = null;
            if (isValid == false)
            {
                SysUser saleman = sysUserRepository.Get(x => x.Id == contract.SaleManId)?.FirstOrDefault();

                messError = string.Format(@"{0} - {1} cash agreement of {2} have shipment that not paid yet, please you check it again!",
                    partner?.TaxCode, partner?.ShortName, saleman.Username);

                return new HandleState((object)messError);
            }
            return result;
        }

        public HandleState ValidateCheckPointPartnerSOA(string partnerId, Guid HblId, string transactionType)
        {
            throw new NotImplementedException();
        }

        public HandleState ValidateCheckPointPartnerSurcharge(string partnerId, Guid HblId, string transactionType, string settlementCode)
        {
            HandleState result = new HandleState();
            bool isValid = false;

            string salemanBOD = sysUserRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault()?.Id;

            CatContract contract = contractRepository.Get(x => x.PartnerId == partnerId
            && x.Active == true
            /// && x.SaleManId != salemanBOD
            && (x.IsExpired == false || x.IsExpired == null))
            .OrderBy(x => x.ContractType)
            // .ThenBy(c => c.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || c.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL)
            .FirstOrDefault();

            CatPartner partner = catPartnerRepository.Get(x => x.Id == partnerId)?.FirstOrDefault();
            if (contract == null)
            {
                return new HandleState((object)string.Format(@"{0} doesn't have any agreement  please you check again", partner.ShortName));
            }

            if (contract.SaleManId == salemanBOD) return result;

            switch (contract.ContractType)
            {
                case "Cash":
                    isValid = ValidateCheckPointCashContractPartner(partnerId, HblId, transactionType, settlementCode, CHECK_POINT_TYPE.SURCHARGE);
                    break;
                //case "Official":
                //case "Trial":
                // isValid = ValidateCheckPointOfficialTrialContractPartner(Id, HblId);
                // break;
                default:
                    isValid = true;
                    break;
            }
            string messError = null;
            if (isValid == false)
            {
                SysUser saleman = sysUserRepository.Get(x => x.Id == contract.SaleManId)?.FirstOrDefault();

                messError = string.Format(@"{0} - {1} cash agreement of {2} have shipment that not paid yet, please you check it again!",
                    partner?.TaxCode, partner?.ShortName, saleman.Username);

                return new HandleState((object)messError);
            }
            return result;
        }

    }
}
