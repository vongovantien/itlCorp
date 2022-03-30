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

        }

        public bool ValidateCheckPointCashContractPartner(string partnerId, Guid HblId, string transactionType)
        {
            bool valid = true;
            IQueryable<CsShipmentSurcharge> surchargeToCheck = Enumerable.Empty<CsShipmentSurcharge>().AsQueryable();
            // Những lô hàng # saleman
            var opsHblids = new List<Guid>();
            string salemanCurrent = null;
            string salemanBOD = sysUserRepository.Get(x => x.Username == DocumentConstants.ITL_BOD)?.FirstOrDefault()?.Id;

            if (HblId == Guid.Empty) // Mở một lô mới
            {
                surchargeToCheck = csSurchargeRepository.Get(x => x.PaymentObjectId == partnerId);
            } else
            {
                if (transactionType == "CL")
                {
                    salemanCurrent = opsTransactionRepository.Get(x => x.Hblid == HblId)?.FirstOrDefault()?.SalemanId;
                    if (salemanCurrent == salemanBOD)
                    {
                        return valid;
                    }
                    opsHblids = opsTransactionRepository.Get(x => x.Hblid != HblId && x.SalemanId == salemanCurrent && x.SalemanId != salemanBOD)
                     .Select(x => x.Hblid)
                     .ToList();
                }
                else
                {
                    salemanCurrent = csDetailSurchargeRepository.Get(x => x.Id == HblId)?.FirstOrDefault()?.SaleManId;
                    if (salemanCurrent == salemanBOD)
                    {
                        return valid;
                    }
                    opsHblids = csDetailSurchargeRepository.Get(x => x.Id != HblId && x.SaleManId == salemanCurrent && x.SaleManId != salemanBOD)
                        .Select(x => x.Id)
                        .ToList();
                }
                if (opsHblids.Count > 0)
                {
                    surchargeToCheck = csSurchargeRepository.Get(x => x.PaymentObjectId == partnerId
                    && opsHblids.Contains(x.Hblid)
                    && x.Hblid != HblId);
                }
                else
                {
                    surchargeToCheck = csSurchargeRepository.Get(x => x.PaymentObjectId == partnerId
                    && x.Hblid != HblId);
                }
            }
           

            if (surchargeToCheck.Count() == 0)
            {
                return valid;
            }

            var surchargeSellOBH = surchargeToCheck.Where(x =>
                (x.Type == DocumentConstants.CHARGE_SELL_TYPE && x.AcctManagementId == null)
             || (x.Type == DocumentConstants.CHARGE_OBH_TYPE && x.AcctManagementId == null)
                );

            if (surchargeSellOBH.Count() > 0)
            {
                valid = false;
            }
            else
            {
                var accMngt = accAccountMngtRepository.Get(x => x.PartnerId == partnerId
                && (x.PaymentStatus == DocumentConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART 
                || x.PaymentStatus == DocumentConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID)
                );
                // Không có hoá đơn treo
                if (accMngt.Count() == 0)
                {
                    return valid;
                }
                var surchargeWithInvoice = surchargeToCheck.Where(x => x.AcctManagementId != null);
                var qInvoiceInvalid = from sur in surchargeWithInvoice
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

        public bool ValidateCheckPointOfficialTrialContractPartner(string partnerId, Guid HblId, string transactionType)
        {
            throw new NotImplementedException();
        }

        public HandleState ValidateCheckPointPartner(string partnerId, Guid HblId, string transactionType)
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
                    isValid = ValidateCheckPointCashContractPartner(partnerId, HblId, transactionType);
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
