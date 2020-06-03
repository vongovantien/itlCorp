using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccountingManagementService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingManagementService
    {
        private readonly ICurrentUser currentUser;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepo;
        private readonly IContextBase<CatCurrencyExchange> currencyExchangeRepo;
        private readonly IContextBase<OpsTransaction> opsTransactionRepo;
        private readonly IContextBase<CsTransaction> csTransactionRepo;
        private readonly IContextBase<CsTransactionDetail> csTransactionDetailRepo;
        private readonly IContextBase<CatCharge> chargeRepo;
        private readonly IContextBase<CatChargeDefaultAccount> chargeDefaultRepo;
        private readonly IContextBase<CatUnit> unitRepo;
        private readonly IContextBase<CustomsDeclaration> customsDeclarationRepo;
        private readonly IContextBase<AcctCdnote> cdNoteRepo;
        private readonly IContextBase<CatPartner> partnerRepo;
        private readonly IContextBase<SysUser> userRepo;
        private readonly IContextBase<CatPlace> placeRepo;
        private readonly IContextBase<AcctSettlementPayment> settlementPaymentRepo;
        public AccountingManagementService(IContextBase<AccAccountingManagement> repository,
            IMapper mapper,
            ICurrentUser cUser,
            ICurrencyExchangeService exchangeService,
            IContextBase<CsShipmentSurcharge> surcharge,
            IContextBase<CatCurrencyExchange> currencyExchange,
            IContextBase<OpsTransaction> opsTransaction,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CsTransactionDetail> csTransactionDetail,
            IContextBase<CatCharge> charge,
            IContextBase<CatChargeDefaultAccount> chargeDefault,
            IContextBase<CatUnit> unit,
            IContextBase<CustomsDeclaration> customsDeclaration,
            IContextBase<AcctCdnote> cdNote,
            IContextBase<CatPartner> partner,
            IContextBase<SysUser> user,
            IContextBase<CatPlace> place,
            IContextBase<AcctSettlementPayment> settlementPayment) : base(repository, mapper)
        {
            currentUser = cUser;
            currencyExchangeService = exchangeService;
            surchargeRepo = surcharge;
            currencyExchangeRepo = currencyExchange;
            opsTransactionRepo = opsTransaction;
            csTransactionRepo = csTransaction;
            csTransactionDetailRepo = csTransactionDetail;
            chargeRepo = charge;
            chargeDefaultRepo = chargeDefault;
            unitRepo = unit;
            customsDeclarationRepo = customsDeclaration;
            cdNoteRepo = cdNote;
            partnerRepo = partner;
            userRepo = user;
            placeRepo = place;
            settlementPaymentRepo = settlementPayment;
        }

        public HandleState Delete(Guid id)
        {
            var data = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (data != null)
            {
                if (data.Type == AccountingConstants.TypeAcctManagement_Voucher)
                {

                }
            }
            return new HandleState();
        }

        #region --- INVOICE ---
        private List<ChargeOfAccountingManagementModel> GetChargeSellForInvoice(Expression<Func<ChargeOfAccountingManagementModel, bool>> query)
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true) có type là SELL (DEBIT)
            var surcharges = surchargeRepo.Get(x => x.IsFromShipment == true && x.Type == AccountingConstants.TYPE_CHARGE_SELL);
            var opsts = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDes = csTransactionDetailRepo.Get();
            var charges = chargeRepo.Get();
            // Chỉ lấy những charge có Type là Công Nợ
            var chargesDefault = chargeDefaultRepo.Get(x => x.Type == "Công Nợ");
            var units = unitRepo.Get();
            var partners = partnerRepo.Get();
            var querySellOperation = from sur in surcharges
                                     join ops in opsts on sur.Hblid equals ops.Hblid
                                     join chg in charges on sur.ChargeId equals chg.Id into chg2
                                     from chg in chg2.DefaultIfEmpty()
                                     join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                     from chgDef in chgDef2.DefaultIfEmpty()
                                     join uni in units on sur.UnitId equals uni.Id into uni2
                                     from uni in uni2.DefaultIfEmpty()
                                     join pat in partners on sur.PaymentObjectId equals pat.Id into pat2
                                     from pat in pat2.DefaultIfEmpty()
                                     select new ChargeOfAccountingManagementModel
                                     {
                                         SurchargeId = sur.Id,
                                         ChargeId = sur.ChargeId,
                                         ChargeCode = chg.Code,
                                         ChargeName = chg.ChargeNameVn,
                                         JobNo = ops.JobNo,
                                         Hbl = ops.Hwbno,
                                         ContraAccount = chgDef.DebitAccountNo,
                                         OrgAmount = sur.Quantity * sur.UnitPrice,
                                         Vat = sur.Vatrate,
                                         OrgVatAmount = sur.Total,
                                         VatAccount = chgDef.DebitVat,
                                         Currency = sur.CurrencyId,
                                         ExchangeDate = sur.ExchangeDate,
                                         FinalExchangeRate = sur.FinalExchangeRate,
                                         ExchangeRate = 0, //Tính toán bên dưới
                                         AmountVnd = 0, //Tính toán bên dưới
                                         VatAmountVnd = 0, //Tính toán bên dưới
                                         VatPartnerId = sur.PaymentObjectId,
                                         VatPartnerCode = pat.AccountNo,
                                         VatPartnerName = pat.PartnerNameVn,
                                         VatPartnerAddress = pat.AddressVn,
                                         ObhPartner = null,
                                         InvoiceNo = sur.InvoiceNo,
                                         Serie = sur.SeriesNo,
                                         InvoiceDate = sur.InvoiceDate,
                                         CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                         Qty = sur.Quantity,
                                         UnitName = uni.UnitNameVn,
                                         UnitPrice = sur.UnitPrice,
                                         Mbl = ops.Mblno,
                                         SoaNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : string.Empty,
                                         SettlementCode = null,
                                         AcctManagementId = sur.AcctManagementId
                                     };
            querySellOperation = querySellOperation.Where(query);
            var querySellDocumentation = from sur in surcharges
                                         join cstd in csTransDes on sur.Hblid equals cstd.Id
                                         join cst in csTrans on cstd.JobId equals cst.Id
                                         join chg in charges on sur.ChargeId equals chg.Id into chg2
                                         from chg in chg2.DefaultIfEmpty()
                                         join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                         from chgDef in chgDef2.DefaultIfEmpty()
                                         join uni in units on sur.UnitId equals uni.Id into uni2
                                         from uni in uni2.DefaultIfEmpty()
                                         join pat in partners on sur.PaymentObjectId equals pat.Id into pat2
                                         from pat in pat2.DefaultIfEmpty()
                                         select new ChargeOfAccountingManagementModel
                                         {
                                             SurchargeId = sur.Id,
                                             ChargeId = sur.ChargeId,
                                             ChargeCode = chg.Code,
                                             ChargeName = chg.ChargeNameVn,
                                             JobNo = cst.JobNo,
                                             Hbl = cstd.Hwbno,
                                             ContraAccount = chgDef.DebitAccountNo,
                                             OrgAmount = sur.Quantity * sur.UnitPrice,
                                             Vat = sur.Vatrate,
                                             OrgVatAmount = sur.Total,
                                             VatAccount = chgDef.DebitVat,
                                             Currency = sur.CurrencyId,
                                             ExchangeDate = sur.ExchangeDate,
                                             FinalExchangeRate = sur.FinalExchangeRate,
                                             ExchangeRate = 0, //Tính toán bên dưới
                                             AmountVnd = 0, //Tính toán bên dưới
                                             VatAmountVnd = 0, //Tính toán bên dưới
                                             VatPartnerId = sur.PaymentObjectId,
                                             VatPartnerCode = pat.AccountNo,
                                             VatPartnerName = pat.PartnerNameVn,
                                             VatPartnerAddress = pat.AddressVn,
                                             ObhPartner = null,
                                             InvoiceNo = sur.InvoiceNo,
                                             Serie = sur.SeriesNo,
                                             InvoiceDate = sur.InvoiceDate,
                                             CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                             Qty = sur.Quantity,
                                             UnitName = uni.UnitNameVn,
                                             UnitPrice = sur.UnitPrice,
                                             Mbl = cst.Mawb,
                                             SoaNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : string.Empty,
                                             SettlementCode = null,
                                             AcctManagementId = sur.AcctManagementId
                                         };
            querySellDocumentation = querySellDocumentation.Where(query);
            var mergeSell = querySellOperation.Union(querySellDocumentation);
            var dataSell = mergeSell.ToList();
            dataSell.ForEach(fe =>
            {
                fe.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(fe.FinalExchangeRate, fe.ExchangeDate, fe.Currency, AccountingConstants.CURRENCY_LOCAL);
                fe.AmountVnd = fe.OrgAmount * fe.ExchangeRate;
                fe.VatAmountVnd = fe.OrgVatAmount * fe.ExchangeRate;
            });
            return dataSell;
        }

        public List<PartnerOfAcctManagementResult> GetChargeSellForInvoiceByCriteria(PartnerOfAcctManagementCriteria criteria)
        {
            //Chỉ lấy ra những charge chưa issue Invoice hoặc Voucher
            Expression<Func<ChargeOfAccountingManagementModel, bool>> query = chg => (chg.AcctManagementId == Guid.Empty || chg.AcctManagementId == null);


            if (criteria.CdNotes != null && criteria.CdNotes.Count > 0)
            {
                query = query.And(x => criteria.CdNotes.Contains(x.CdNoteNo));
            }

            if (criteria.SoaNos != null && criteria.SoaNos.Count > 0)
            {
                query = query.And(x => criteria.SoaNos.Contains(x.SoaNo));
            }

            if (criteria.JobNos != null && criteria.JobNos.Count > 0)
            {
                query = query.And(x => criteria.JobNos.Contains(x.JobNo));
            }

            if (criteria.Hbls != null && criteria.Hbls.Count > 0)
            {
                query = query.And(x => criteria.Hbls.Contains(x.Hbl));
            }

            if (criteria.Mbls != null && criteria.Mbls.Count > 0)
            {
                query = query.And(x => criteria.Mbls.Contains(x.Mbl));
            }

            //SELLING (DEBIT)
            var charges = GetChargeSellForInvoice(query);

            // Group by theo Partner
            var chargeGroupByPartner = charges.GroupBy(g => new { PartnerId = g.VatPartnerId }).Select(s =>
                new PartnerOfAcctManagementResult
                {
                    PartnerId = s.Key.PartnerId,
                    PartnerName = s.FirstOrDefault()?.VatPartnerName,
                    PartnerAddress = s.FirstOrDefault()?.VatPartnerAddress,
                    SettlementRequester = null,
                    InputRefNo = string.Empty,
                    Charges = s.ToList()
                }
                ).ToList();

            string _inputRefNoHadFoundCharge = string.Empty;
            chargeGroupByPartner.ForEach(fe =>
            {
                if (criteria.CdNotes != null && criteria.CdNotes.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.CdNotes.Contains(x.CdNoteNo)).Select(s => s.CdNoteNo).Distinct());
                }

                if (criteria.SoaNos != null && criteria.SoaNos.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.SoaNos.Contains(x.SoaNo)).Select(s => s.SoaNo).Distinct());
                }

                if (criteria.JobNos != null && criteria.JobNos.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.JobNos.Contains(x.JobNo)).Select(s => s.JobNo).Distinct());
                }

                if (criteria.Hbls != null && criteria.Hbls.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.Hbls.Contains(x.Hbl)).Select(s => s.Hbl).Distinct());
                }
                fe.InputRefNo = _inputRefNoHadFoundCharge;
            });
            return chargeGroupByPartner;
        }
        #endregion --- INVOICE ---

        #region --- VOUCHER ---
        private List<ChargeOfAccountingManagementModel> GetChargeBuySellForVoucher(Expression<Func<ChargeOfAccountingManagementModel, bool>> query)
        {
            //Chỉ lấy những phí có type là SELL (DEBIT) OR BUY (CREDIT)
            var surcharges = surchargeRepo.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL || x.Type == AccountingConstants.TYPE_CHARGE_BUY);
            var opsts = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDes = csTransactionDetailRepo.Get();
            var charges = chargeRepo.Get();
            // Chỉ lấy những charge có Type là Công Nợ
            var chargesDefault = chargeDefaultRepo.Get(x => x.Type == "Công Nợ");
            var units = unitRepo.Get();
            var partners = partnerRepo.Get();
            var settlements = settlementPaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);
            var queryBuySellOperation = from sur in surcharges
                                        join ops in opsts on sur.Hblid equals ops.Hblid
                                        join chg in charges on sur.ChargeId equals chg.Id into chg2
                                        from chg in chg2.DefaultIfEmpty()
                                        join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                        from chgDef in chgDef2.DefaultIfEmpty()
                                        join uni in units on sur.UnitId equals uni.Id into uni2
                                        from uni in uni2.DefaultIfEmpty()
                                        join pat in partners on sur.PaymentObjectId equals pat.Id into pat2
                                        from pat in pat2.DefaultIfEmpty()
                                        join sett in settlements on sur.SettlementCode equals sett.SettlementNo into sett2
                                        from sett in sett2.DefaultIfEmpty()
                                        select new ChargeOfAccountingManagementModel
                                        {
                                            SurchargeId = sur.Id,
                                            ChargeId = sur.ChargeId,
                                            ChargeCode = chg.Code,
                                            ChargeName = chg.ChargeNameVn,
                                            JobNo = ops.JobNo,
                                            Hbl = ops.Hwbno,
                                            ContraAccount = chgDef.DebitAccountNo,
                                            OrgAmount = sur.Quantity * sur.UnitPrice,
                                            Vat = sur.Vatrate,
                                            OrgVatAmount = sur.Total,
                                            VatAccount = chgDef.DebitVat,
                                            Currency = sur.CurrencyId,
                                            ExchangeDate = sur.ExchangeDate,
                                            FinalExchangeRate = sur.FinalExchangeRate,
                                            ExchangeRate = 0, //Tính toán bên dưới
                                            AmountVnd = 0, //Tính toán bên dưới
                                            VatAmountVnd = 0, //Tính toán bên dưới
                                            VatPartnerId = sur.PaymentObjectId,
                                            VatPartnerCode = pat.AccountNo,
                                            VatPartnerName = pat.PartnerNameVn,
                                            VatPartnerAddress = pat.AddressVn,
                                            ObhPartner = null,
                                            InvoiceNo = sur.InvoiceNo,
                                            Serie = sur.SeriesNo,
                                            InvoiceDate = sur.InvoiceDate,
                                            CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                            Qty = sur.Quantity,
                                            UnitName = uni.UnitNameVn,
                                            UnitPrice = sur.UnitPrice,
                                            Mbl = ops.Mblno,
                                            SoaNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : sur.PaySoano,
                                            SettlementCode = sett.SettlementNo,
                                            AcctManagementId = sur.AcctManagementId
                                        };
            queryBuySellOperation = queryBuySellOperation.Where(query);
            var queryBuySellDocumentation = from sur in surcharges
                                            join cstd in csTransDes on sur.Hblid equals cstd.Id
                                            join cst in csTrans on cstd.JobId equals cst.Id
                                            join chg in charges on sur.ChargeId equals chg.Id into chg2
                                            from chg in chg2.DefaultIfEmpty()
                                            join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                            from chgDef in chgDef2.DefaultIfEmpty()
                                            join uni in units on sur.UnitId equals uni.Id into uni2
                                            from uni in uni2.DefaultIfEmpty()
                                            join pat in partners on sur.PaymentObjectId equals pat.Id into pat2
                                            from pat in pat2.DefaultIfEmpty()
                                            join sett in settlements on sur.SettlementCode equals sett.SettlementNo into sett2
                                            from sett in sett2.DefaultIfEmpty()
                                            select new ChargeOfAccountingManagementModel
                                            {
                                                SurchargeId = sur.Id,
                                                ChargeId = sur.ChargeId,
                                                ChargeCode = chg.Code,
                                                ChargeName = chg.ChargeNameVn,
                                                JobNo = cst.JobNo,
                                                Hbl = cstd.Hwbno,
                                                ContraAccount = chgDef.DebitAccountNo,
                                                OrgAmount = sur.Quantity * sur.UnitPrice,
                                                Vat = sur.Vatrate,
                                                OrgVatAmount = sur.Total,
                                                VatAccount = chgDef.DebitVat,
                                                Currency = sur.CurrencyId,
                                                ExchangeDate = sur.ExchangeDate,
                                                FinalExchangeRate = sur.FinalExchangeRate,
                                                ExchangeRate = 0, //Tính toán bên dưới
                                                AmountVnd = 0, //Tính toán bên dưới
                                                VatAmountVnd = 0, //Tính toán bên dưới
                                                VatPartnerId = sur.PaymentObjectId,
                                                VatPartnerCode = pat.AccountNo,
                                                VatPartnerName = pat.PartnerNameVn,
                                                VatPartnerAddress = pat.AddressVn,
                                                ObhPartner = null,
                                                InvoiceNo = sur.InvoiceNo,
                                                Serie = sur.SeriesNo,
                                                InvoiceDate = sur.InvoiceDate,
                                                CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                                Qty = sur.Quantity,
                                                UnitName = uni.UnitNameVn,
                                                UnitPrice = sur.UnitPrice,
                                                Mbl = cst.Mawb,
                                                SoaNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : sur.PaySoano,
                                                SettlementCode = sett.SettlementNo,
                                                AcctManagementId = sur.AcctManagementId
                                            };
            queryBuySellDocumentation = queryBuySellDocumentation.Where(query);
            var mergeBuySell = queryBuySellOperation.Union(queryBuySellDocumentation);
            var dataBuySell = mergeBuySell.ToList();
            dataBuySell.ForEach(fe =>
            {
                fe.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(fe.FinalExchangeRate, fe.ExchangeDate, fe.Currency, AccountingConstants.CURRENCY_LOCAL);
                fe.AmountVnd = fe.OrgAmount * fe.ExchangeRate;
                fe.VatAmountVnd = fe.OrgVatAmount * fe.ExchangeRate;
            });
            return dataBuySell;
        }
       
        private List<ChargeOfAccountingManagementModel> GetChargeObhBuyForVoucher(Expression<Func<ChargeOfAccountingManagementModel, bool>> query)
        {
            //Chỉ lấy những phí có type là OBH
            var surcharges = surchargeRepo.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH);
            var opsts = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDes = csTransactionDetailRepo.Get();
            var charges = chargeRepo.Get();
            // Chỉ lấy những charge có Type là Công Nợ
            var chargesDefault = chargeDefaultRepo.Get(x => x.Type == "Công Nợ");
            var units = unitRepo.Get();
            var partners = partnerRepo.Get();
            var obhPartners = partnerRepo.Get();
            var settlements = settlementPaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);
            var queryObhBuyOperation = from sur in surcharges
                                       join ops in opsts on sur.Hblid equals ops.Hblid
                                       join chg in charges on sur.ChargeId equals chg.Id into chg2
                                       from chg in chg2.DefaultIfEmpty()
                                       join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                       from chgDef in chgDef2.DefaultIfEmpty()
                                       join uni in units on sur.UnitId equals uni.Id into uni2
                                       from uni in uni2.DefaultIfEmpty()
                                       join pat in partners on sur.PayerId equals pat.Id into pat2
                                       from pat in pat2.DefaultIfEmpty()
                                       join obhPat in obhPartners on sur.PaymentObjectId equals obhPat.Id into obhPat2
                                       from obhPat in obhPat2.DefaultIfEmpty()
                                       join sett in settlements on sur.SettlementCode equals sett.SettlementNo into sett2
                                       from sett in sett2.DefaultIfEmpty()
                                       select new ChargeOfAccountingManagementModel
                                       {
                                           SurchargeId = sur.Id,
                                           ChargeId = sur.ChargeId,
                                           ChargeCode = chg.Code,
                                           ChargeName = chg.ChargeNameVn,
                                           JobNo = ops.JobNo,
                                           Hbl = ops.Hwbno,
                                           ContraAccount = chgDef.DebitAccountNo,
                                           OrgAmount = sur.Quantity * sur.UnitPrice,
                                           Vat = sur.Vatrate,
                                           OrgVatAmount = sur.Total,
                                           VatAccount = chgDef.DebitVat,
                                           Currency = sur.CurrencyId,
                                           ExchangeDate = sur.ExchangeDate,
                                           FinalExchangeRate = sur.FinalExchangeRate,
                                           ExchangeRate = 0, //Tính toán bên dưới
                                           AmountVnd = 0, //Tính toán bên dưới
                                           VatAmountVnd = 0, //Tính toán bên dưới
                                           VatPartnerId = sur.PayerId,
                                           VatPartnerCode = pat.AccountNo,
                                           VatPartnerName = pat.PartnerNameVn,
                                           VatPartnerAddress = pat.AddressVn,
                                           ObhPartner = obhPat.PartnerNameVn,
                                           InvoiceNo = sur.InvoiceNo,
                                           Serie = sur.SeriesNo,
                                           InvoiceDate = sur.InvoiceDate,
                                           CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                           Qty = sur.Quantity,
                                           UnitName = uni.UnitNameVn,
                                           UnitPrice = sur.UnitPrice,
                                           Mbl = ops.Mblno,
                                           SoaNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : sur.PaySoano,
                                           SettlementCode = sett.SettlementNo,
                                           AcctManagementId = sur.AcctManagementId
                                       };
            queryObhBuyOperation = queryObhBuyOperation.Where(query);
            var queryObhBuyDocumentation = from sur in surcharges
                                           join cstd in csTransDes on sur.Hblid equals cstd.Id
                                           join cst in csTrans on cstd.JobId equals cst.Id
                                           join chg in charges on sur.ChargeId equals chg.Id into chg2
                                           from chg in chg2.DefaultIfEmpty()
                                           join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                           from chgDef in chgDef2.DefaultIfEmpty()
                                           join uni in units on sur.UnitId equals uni.Id into uni2
                                           from uni in uni2.DefaultIfEmpty()
                                           join pat in partners on sur.PayerId equals pat.Id into pat2
                                           from pat in pat2.DefaultIfEmpty()
                                           join obhPat in obhPartners on sur.PaymentObjectId equals obhPat.Id into obhPat2
                                           from obhPat in obhPat2.DefaultIfEmpty()
                                           join sett in settlements on sur.SettlementCode equals sett.SettlementNo into sett2
                                           from sett in sett2.DefaultIfEmpty()
                                           select new ChargeOfAccountingManagementModel
                                           {
                                               SurchargeId = sur.Id,
                                               ChargeId = sur.ChargeId,
                                               ChargeCode = chg.Code,
                                               ChargeName = chg.ChargeNameVn,
                                               JobNo = cst.JobNo,
                                               Hbl = cstd.Hwbno,
                                               ContraAccount = chgDef.DebitAccountNo,
                                               OrgAmount = sur.Quantity * sur.UnitPrice,
                                               Vat = sur.Vatrate,
                                               OrgVatAmount = sur.Total,
                                               VatAccount = chgDef.DebitVat,
                                               Currency = sur.CurrencyId,
                                               ExchangeDate = sur.ExchangeDate,
                                               FinalExchangeRate = sur.FinalExchangeRate,
                                               ExchangeRate = 0, //Tính toán bên dưới
                                               AmountVnd = 0, //Tính toán bên dưới
                                               VatAmountVnd = 0, //Tính toán bên dưới
                                               VatPartnerId = sur.PayerId,
                                               VatPartnerCode = pat.AccountNo,
                                               VatPartnerName = pat.PartnerNameVn,
                                               VatPartnerAddress = pat.AddressVn,
                                               ObhPartner = obhPat.PartnerNameVn,
                                               InvoiceNo = sur.InvoiceNo,
                                               Serie = sur.SeriesNo,
                                               InvoiceDate = sur.InvoiceDate,
                                               CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                               Qty = sur.Quantity,
                                               UnitName = uni.UnitNameVn,
                                               UnitPrice = sur.UnitPrice,
                                               Mbl = cst.Mawb,
                                               SoaNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : sur.PaySoano,
                                               SettlementCode = sett.SettlementNo,
                                               AcctManagementId = sur.AcctManagementId
                                           };
            queryObhBuyDocumentation = queryObhBuyDocumentation.Where(query);
            var mergeObhBuy = queryObhBuyOperation.Union(queryObhBuyDocumentation);
            var dataObhBuy = mergeObhBuy.ToList();
            dataObhBuy.ForEach(fe =>
            {
                fe.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(fe.FinalExchangeRate, fe.ExchangeDate, fe.Currency, AccountingConstants.CURRENCY_LOCAL);
                fe.AmountVnd = fe.OrgAmount * fe.ExchangeRate;
                fe.VatAmountVnd = fe.OrgVatAmount * fe.ExchangeRate;
            });
            return dataObhBuy;
        }
        
        public List<ChargeOfAccountingManagementModel> GetChargeForVoucher(Expression<Func<ChargeOfAccountingManagementModel, bool>> query)
        {
            //BUY & SELL
            var queryBuySell = GetChargeBuySellForVoucher(query);
            //OBH Payee (BUY - Credit)
            var queryObhBuy = GetChargeObhBuyForVoucher(query);
            //Merge data
            var dataMerge = queryBuySell.Union(queryObhBuy);
            return dataMerge.ToList();
        }

        public List<PartnerOfAcctManagementResult> GetChargeForVoucherByCriteria(PartnerOfAcctManagementCriteria criteria)
        {
            //Chỉ lấy ra những charge chưa issue Invoice hoặc Voucher
            Expression<Func<ChargeOfAccountingManagementModel, bool>> query = chg => (chg.AcctManagementId == Guid.Empty || chg.AcctManagementId == null);

            if (criteria.CdNotes != null && criteria.CdNotes.Count > 0)
            {
                query = query.And(x => criteria.CdNotes.Contains(x.CdNoteNo));
            }

            if (criteria.SoaNos != null && criteria.SoaNos.Count > 0)
            {
                query = query.And(x => criteria.SoaNos.Contains(x.SoaNo));
            }

            if (criteria.JobNos != null && criteria.JobNos.Count > 0)
            {
                query = query.And(x => criteria.JobNos.Contains(x.JobNo));
            }

            if (criteria.Hbls != null && criteria.Hbls.Count > 0)
            {
                query = query.And(x => criteria.Hbls.Contains(x.Hbl));
            }

            if (criteria.Mbls != null && criteria.Mbls.Count > 0)
            {
                query = query.And(x => criteria.Mbls.Contains(x.Mbl));
            }

            if (criteria.SettlementCodes != null && criteria.SettlementCodes.Count > 0)
            {
                query = query.And(x => criteria.SettlementCodes.Contains(x.SettlementCode));
            }

            var charges = GetChargeForVoucher(query);

            // Group by theo Partner
            var chargeGroupByPartner = charges.GroupBy(g => new { PartnerId = g.VatPartnerId }).Select(s =>
                new PartnerOfAcctManagementResult
                {
                    PartnerId = s.Key.PartnerId,
                    PartnerName = s.FirstOrDefault()?.VatPartnerName,
                    PartnerAddress = s.FirstOrDefault()?.VatPartnerAddress,
                    SettlementRequester = null, //Tính toán bên dưới
                    InputRefNo = string.Empty, //Tính toán bên dưới
                    Charges = s.ToList()
                }
                ).ToList();

            string _inputRefNoHadFoundCharge = string.Empty;
            chargeGroupByPartner.ForEach(fe =>
            {
                if (criteria.CdNotes != null && criteria.CdNotes.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.CdNotes.Contains(x.CdNoteNo)).Select(s => s.CdNoteNo).Distinct());
                }

                if (criteria.SoaNos != null && criteria.SoaNos.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.SoaNos.Contains(x.SoaNo)).Select(s => s.SoaNo).Distinct());
                }

                if (criteria.JobNos != null && criteria.JobNos.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.JobNos.Contains(x.JobNo)).Select(s => s.JobNo).Distinct());
                }

                if (criteria.Hbls != null && criteria.Hbls.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.Hbls.Contains(x.Hbl)).Select(s => s.Hbl).Distinct());
                }

                if (criteria.SettlementCodes != null && criteria.SettlementCodes.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.SettlementCodes.Contains(x.SettlementCode)).Select(s => s.SettlementCode).Distinct());
                }

                fe.InputRefNo = _inputRefNoHadFoundCharge;
                var userIdRequester = settlementPaymentRepo.Get(x => x.SettlementNo == fe.Charges[0].SettlementCode).FirstOrDefault()?.Requester;
                fe.SettlementRequester = userRepo.Get(x => x.Id == userIdRequester).FirstOrDefault()?.Username;
            });
            return chargeGroupByPartner;
        }
        #endregion --- VOUCHER ---

    }
}
