using eFMS.API.ForPartner.Service.Models;
using eFMS.API.ForPartner.DL.Models;
using ITL.NetCore.Connection.BL;
using eFMS.API.ForPartner.DL.IService;
using System;
using ITL.NetCore.Connection.EF;
using eFMS.IdentityServer.DL.UserManager;
using AutoMapper;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using eFMS.API.ForPartner.DL.Models.Criteria;
using System.Linq.Expressions;
using ITL.NetCore.Common;
using System.Collections.Generic;
using eFMS.API.ForPartner.DL.Common;

namespace eFMS.API.ForPartner.DL.Service
{
    public class AccAccountingManagementService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingManagementService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysPartnerApi> sysPartnerApiRepository;


        private readonly IHostingEnvironment environment;
        private readonly IContextBase<CatUnit> catUnitRepository;
        private readonly IContextBase<CatCharge> catChargeRepository;
        private readonly IContextBase<CatChargeDefaultAccount> catChargeDefaultRepository;
        private readonly IContextBase<CatPartner> catPartnerRepository;

        private readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        private readonly IContextBase<CustomsDeclaration> customDeclarationRepository;
        private readonly IContextBase<OpsTransaction> opsTransactionRepository;
        private readonly IContextBase<CsTransaction> csTransactionRepository;
        private readonly IContextBase<CsTransactionDetail> csTransactionDetailRepository;

        public AccAccountingManagementService(
            IContextBase<CatChargeDefaultAccount> catChargeDefaultRepo,
            IContextBase<CatUnit> catUnitRepo,
            IContextBase<CatCharge> catChargeRepo,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<AccAccountingManagement> repository,
            IContextBase<SysPartnerApi> sysPartnerApiRep,
            IContextBase<CsTransaction> csTransactionRepo,
            IContextBase<OpsTransaction> opsTransactionRepo,
            IContextBase<CustomsDeclaration> customDeclarationRepo,
            IContextBase<CsTransactionDetail> csTransactionDetailRepo,
            IHostingEnvironment env,
            IMapper mapper,
            ICurrentUser cUser
            ) : base(repository, mapper)
        {
            currentUser = cUser;
            sysPartnerApiRepository = sysPartnerApiRep;
            environment = env;
            catUnitRepository = catUnitRepo;
            catChargeRepository = catChargeRepo;
            catPartnerRepository = catPartnerRepo;
            surchargeRepository = surchargeRepo;
            csTransactionDetailRepository = csTransactionDetailRepo;
            csTransactionRepository = csTransactionRepo;
            opsTransactionRepository = opsTransactionRepo;
            customDeclarationRepository = customDeclarationRepo;



        }
        public AccAccountingManagementModel GetById(Guid id)
        {
            AccAccountingManagement accounting = DataContext.Get(x => x.Id == id).FirstOrDefault();
            AccAccountingManagementModel result = mapper.Map<AccAccountingManagementModel>(accounting);
            return result;
        }
        public bool ValidateApiKey(string apiKey)
        {
            bool isValid = false;
            SysPartnerApi partnerAPiInfo = sysPartnerApiRepository.Get(x => x.ApiKey == apiKey).FirstOrDefault();
            if (partnerAPiInfo != null && partnerAPiInfo.Active == true)
            {
                isValid = true;
            }
            return isValid;
        }
        public List<ChargeOfAcctMngtResult> GetChargeInvoice(SearchAccMngtCriteria criteria)
        {
            //Chỉ lấy ra những charge chưa issue Invoice hoặc Voucher
            Expression<Func<ChargeOfAcctMngtResult, bool>> query = null;


            if (criteria.CdNotes != null && criteria.CdNotes.Count > 0)
            {
                query = query.And(x => criteria.CdNotes.Where(w => !string.IsNullOrEmpty(w)).Contains(x.DebitNo ?? ""));
            }

            if (criteria.SoaNos != null && criteria.SoaNos.Count > 0)
            {
                query = query.And(x => criteria.SoaNos.Where(w => !string.IsNullOrEmpty(w)).Contains(x.SoaNo));
            }

            if (criteria.JobNos != null && criteria.JobNos.Count > 0)
            {
                query = query.And(x => criteria.JobNos.Where(w => !string.IsNullOrEmpty(w)).Contains(x.JobNo));
            }

            if (criteria.Hbls != null && criteria.Hbls.Count > 0)
            {
                query = query.And(x => criteria.Hbls.Where(w => !string.IsNullOrEmpty(w)).Contains(x.Hbl));
            }

            if (criteria.Mbls != null && criteria.Mbls.Count > 0)
            {
                query = query.And(x => criteria.Mbls.Where(w => !string.IsNullOrEmpty(w)).Contains(x.Mbl));
            }

            //SELLING (DEBIT)
            List<ChargeOfAcctMngtResult> charges = GetChargeSellForInvoice(query);
            return charges;
        }

        List<ChargeOfAcctMngtResult> GetChargeSellForInvoice(Expression<Func<ChargeOfAcctMngtResult, bool>> query)
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true) có type là SELL (DEBIT)
            IQueryable<CsShipmentSurcharge> surcharges = surchargeRepository.Get(x => x.IsFromShipment == true && x.Type == AccountingConstants.TYPE_CHARGE_SELL);
            IQueryable<OpsTransaction> opsts = opsTransactionRepository.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != TermData.Canceled);
            IQueryable<CsTransaction> csTrans = csTransactionRepository.Get(x => x.CurrentStatus != TermData.Canceled);

            IQueryable<CsTransactionDetail> csTransDes = csTransactionDetailRepository.Get();
            IQueryable<CatCharge> catCharges = catChargeRepository.Get();
            IQueryable<CatChargeDefaultAccount> chargesDefault = catChargeDefaultRepository.Get(x => x.Type == "Công Nợ");
            IQueryable<CatUnit> catUnits = catUnitRepository.Get();
            IQueryable<CatPartner> catPartners = catPartnerRepository.Get();
            IQueryable<CustomsDeclaration> cds = customDeclarationRepository.Get();

            List<ChargeOfAcctMngtResult> listChargeResults = new List<ChargeOfAcctMngtResult>();

            IQueryable<ChargeOfAcctMngtResult> querySellOps = from surcharge in surcharges
                                                              join ops in opsts on surcharge.Hblid equals ops.Hblid
                                                              join cd in cds on surcharge.Hblid.ToString() equals cd.Hblid
                                                              join catCharge in catCharges on surcharge.ChargeId equals catCharge.Id into catChargeGrps
                                                              from catChargeGrp in catChargeGrps.DefaultIfEmpty()
                                                              join chgDef in chargesDefault on catChargeGrp.Id equals chgDef.ChargeId into chgDefGroups
                                                              from chgDefGroup in chgDefGroups.DefaultIfEmpty()
                                                              join unit in catUnits on surcharge.UnitId equals unit.Id into unitGrps
                                                              from unitGrp in unitGrps.DefaultIfEmpty()
                                                              join partner in catPartners on surcharge.PaymentObjectId equals partner.Id into partnerGrps
                                                              from partnerGrp in partnerGrps.DefaultIfEmpty()
                                                              select new ChargeOfAcctMngtResult
                                                              {
                                                                  ChargeCode = catChargeGrp.Code,
                                                                  ChargeName = catChargeGrp.ChargeNameVn,
                                                                  OrgAmount = surcharge.Quantity * surcharge.UnitPrice,

                                                                  CustomNo = cd.ClearanceNo,

                                                                  JobNo = surcharge.JobNo,
                                                                  Mbl = surcharge.Mblno,
                                                                  Hbl = surcharge.Hblno,
                                                                  Qty = surcharge.Quantity,
                                                                  Vat = surcharge.Vatrate,
                                                                  UnitPrice = surcharge.UnitPrice,
                                                                  UnitName = unitGrp.UnitNameVn,
                                                                  Currency = surcharge.CurrencyId,
                                                                  FinalExchangeRate = surcharge.FinalExchangeRate,
                                                                  InvoiceDate = surcharge.InvoiceDate,
                                                                  InvoiceNo = surcharge.InvoiceNo,
                                                                  Serie = surcharge.SeriesNo,
                                                                  DebitNo = surcharge.DebitNo,
                                                                  ExchangeDate = surcharge.ExchangeDate,

                                                                  PartnerLocation = partnerGrp.PartnerLocation,
                                                                  PartnerMode = partnerGrp.PartnerMode,
                                                                  PartnerReference = partnerGrp.InternalReferenceNo,

                                                                  SoaNo = (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH_SELL ? surcharge.Soano : null),
                                                                  VatPartnerId = surcharge.PaymentObjectId,
                                                                  VatPartnerName = partnerGrp.ShortName,

                                                              };
            listChargeResults = querySellOps.ToList();
            //listChargeResults.ForEach(fe =>
            //{
            //    fe.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(fe.FinalExchangeRate, fe.ExchangeDate, fe.Currency, AccountingConstants.CURRENCY_LOCAL);
            //    fe.OrgVatAmount = (fe.Vat != null) ? (fe.Vat < 101 & fe.Vat >= 0) ? ((fe.OrgAmount * fe.Vat) / 100) : (fe.OrgAmount + Math.Abs(fe.Vat ?? 0)) : 0;
            //});
            return listChargeResults;

        }
    }
}
