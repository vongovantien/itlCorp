﻿using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        private readonly IContextBase<SysEmployee> employeeRepo;
        private readonly IStringLocalizer stringLocalizer;

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
            IContextBase<AcctSettlementPayment> settlementPayment,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<SysEmployee> employee) : base(repository, mapper)
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
            employeeRepo = employee;
            stringLocalizer = localizer;
        }

        #region --- DELETE ---
        public HandleState Delete(Guid id)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var data = DataContext.Get(x => x.Id == id).FirstOrDefault();
                    var hs = DataContext.Delete(x => x.Id == id);
                    if (hs.Success)
                    {
                        var charges = surchargeRepo.Get(x => x.AcctManagementId == id);
                        foreach (var item in charges)
                        {
                            item.AcctManagementId = null;
                            if (data.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                            {
                                item.VoucherId = null;
                                item.VoucherIddate = null;
                            }
                            if (data.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                            {
                                item.InvoiceNo = null;
                                item.InvoiceDate = null;
                            }
                            item.DatetimeModified = DateTime.Now;
                            item.UserModified = currentUser.UserID;
                            surchargeRepo.Update(item, x => x.Id == item.Id, false);
                        }
                        surchargeRepo.SubmitChanges();
                        DataContext.SubmitChanges();
                        trans.Commit();
                    }
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public int CheckDeletePermission(Guid id)
        {
            return 1;
        }
        #endregion --- DELETE ---

        #region --- LIST PAGING ---
        public List<AccAccountingManagementResult> Paging(AccAccountingManagementCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetData(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            //Phân trang
            var _totalItem = data.Select(s => s.Id).Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

            return data.ToList();
        }

        private Expression<Func<AccAccountingManagement, bool>> ExpressionQuery(AccAccountingManagementCriteria criteria)
        {
            Expression<Func<AccAccountingManagement, bool>> query = q => true;
            if (!string.IsNullOrEmpty(criteria.TypeOfAcctManagement))
            {
                query = query.And(x => x.Type == criteria.TypeOfAcctManagement);
            }

            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                query = query.And(x => criteria.ReferenceNos.Contains(x.VoucherId) || criteria.ReferenceNos.Contains(x.InvoiceNoReal) || criteria.ReferenceNos.Contains(x.InvoiceNoTempt));
            }

            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(x => x.PartnerId == criteria.PartnerId);
            }

            if (criteria.IssuedDate != null)
            {
                query = query.And(x => x.DatetimeCreated.Value.Date == criteria.IssuedDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(criteria.CreatorId))
            {
                query = query.And(x => x.UserCreated == criteria.CreatorId);
            }

            if (!string.IsNullOrEmpty(criteria.InvoiceStatus))
            {
                query = query.And(x => x.Status == criteria.InvoiceStatus);
            }

            if (!string.IsNullOrEmpty(criteria.VoucherType))
            {
                query = query.And(x => x.VoucherType == criteria.VoucherType);
            }
            return query;
        }

        private IQueryable<AccAccountingManagementResult> GetData(AccAccountingManagementCriteria criteria)
        {
            var query = ExpressionQuery(criteria);
            var partners = partnerRepo.Get();
            var users = userRepo.Get();
            var acct = DataContext.Get(query);
            var data = from acc in acct
                       join pat in partners on acc.PartnerId equals pat.Id into pat2
                       from pat in pat2.DefaultIfEmpty()
                       join user in users on acc.UserCreated equals user.Id into user2
                       from user in user2.DefaultIfEmpty()
                       select new AccAccountingManagementResult
                       {
                           Id = acc.Id,
                           VoucherId = acc.VoucherId,
                           InvoiceNoTempt = acc.InvoiceNoTempt,
                           InvoiceNoReal = acc.InvoiceNoReal,
                           PartnerName = pat.ShortName,
                           TotalAmount = acc.TotalAmount,
                           Currency = acc.Currency,
                           Date = acc.Date, //Invoice Date or Voucher Date
                           DatetimeCreated = acc.DatetimeCreated, //Issue Date
                           CreatorName = user.Username,
                           Status = acc.Status, //Status Invoice,
                           Serie = acc.Serie
                       };
            return data.ToArray().OrderByDescending(o => o.DatetimeModified).AsQueryable();
        }
        #endregion --- LIST PAGING ---

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
                                         ContraAccount = chgDef.CreditAccountNo,
                                         OrgAmount = sur.Quantity * sur.UnitPrice,
                                         Vat = sur.Vatrate,
                                         OrgVatAmount = 0, //Tính toán bên dưới
                                         VatAccount = chgDef.DebitVat,
                                         Currency = sur.CurrencyId,
                                         ExchangeDate = sur.ExchangeDate,
                                         FinalExchangeRate = sur.FinalExchangeRate,
                                         ExchangeRate = 0, //Tính toán bên dưới
                                         AmountVnd = 0, //Tính toán bên dưới
                                         VatAmountVnd = 0, //Tính toán bên dưới
                                         VatPartnerId = sur.PaymentObjectId,
                                         VatPartnerCode = pat.TaxCode, //Tax code
                                         VatPartnerName = pat.PartnerNameVn,
                                         VatPartnerAddress = pat.AddressVn,
                                         ObhPartnerCode = null,
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
                                             ContraAccount = chgDef.CreditAccountNo,
                                             OrgAmount = sur.Quantity * sur.UnitPrice,
                                             Vat = sur.Vatrate,
                                             OrgVatAmount = 0, //Tính toán bên dưới
                                             VatAccount = chgDef.DebitVat,
                                             Currency = sur.CurrencyId,
                                             ExchangeDate = sur.ExchangeDate,
                                             FinalExchangeRate = sur.FinalExchangeRate,
                                             ExchangeRate = 0, //Tính toán bên dưới
                                             AmountVnd = 0, //Tính toán bên dưới
                                             VatAmountVnd = 0, //Tính toán bên dưới
                                             VatPartnerId = sur.PaymentObjectId,
                                             VatPartnerCode = pat.TaxCode, //Tax code
                                             VatPartnerName = pat.PartnerNameVn,
                                             VatPartnerAddress = pat.AddressVn,
                                             ObhPartnerCode = null,
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
                fe.OrgVatAmount = (fe.Vat < 101 & fe.Vat > 0) ? ((fe.OrgAmount * fe.Vat) / 100) : (fe.OrgAmount + Math.Abs(fe.Vat ?? 0));
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
                query = query.And(x => criteria.CdNotes.Where(w => !string.IsNullOrEmpty(w)).Contains(x.CdNoteNo));
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
                                            ContraAccount = chgDef.CreditAccountNo,
                                            OrgAmount = sur.Quantity * sur.UnitPrice,
                                            Vat = sur.Vatrate,
                                            OrgVatAmount = 0, //Tính toán bên dưới
                                            VatAccount = chgDef.DebitVat,
                                            Currency = sur.CurrencyId,
                                            ExchangeDate = sur.ExchangeDate,
                                            FinalExchangeRate = sur.FinalExchangeRate,
                                            ExchangeRate = 0, //Tính toán bên dưới
                                            AmountVnd = 0, //Tính toán bên dưới
                                            VatAmountVnd = 0, //Tính toán bên dưới
                                            VatPartnerId = sur.PaymentObjectId,
                                            VatPartnerCode = pat.TaxCode, //Tax code
                                            VatPartnerName = pat.PartnerNameVn,
                                            VatPartnerAddress = pat.AddressVn,
                                            ObhPartnerCode = null,
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
                                                ContraAccount = chgDef.CreditAccountNo,
                                                OrgAmount = sur.Quantity * sur.UnitPrice,
                                                Vat = sur.Vatrate,
                                                OrgVatAmount = 0, //Tính toán bên dưới
                                                VatAccount = chgDef.DebitVat,
                                                Currency = sur.CurrencyId,
                                                ExchangeDate = sur.ExchangeDate,
                                                FinalExchangeRate = sur.FinalExchangeRate,
                                                ExchangeRate = 0, //Tính toán bên dưới
                                                AmountVnd = 0, //Tính toán bên dưới
                                                VatAmountVnd = 0, //Tính toán bên dưới
                                                VatPartnerId = sur.PaymentObjectId,
                                                VatPartnerCode = pat.TaxCode, //Tax code
                                                VatPartnerName = pat.PartnerNameVn,
                                                VatPartnerAddress = pat.AddressVn,
                                                ObhPartnerCode = null,
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
                fe.OrgVatAmount = (fe.Vat < 101 & fe.Vat > 0) ? ((fe.OrgAmount * fe.Vat) / 100) : (fe.OrgAmount + Math.Abs(fe.Vat ?? 0));
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
                                           ContraAccount = chgDef.CreditAccountNo,
                                           OrgAmount = sur.Quantity * sur.UnitPrice,
                                           Vat = sur.Vatrate,
                                           OrgVatAmount = 0, //Tính toán bên dưới
                                           VatAccount = chgDef.DebitVat,
                                           Currency = sur.CurrencyId,
                                           ExchangeDate = sur.ExchangeDate,
                                           FinalExchangeRate = sur.FinalExchangeRate,
                                           ExchangeRate = 0, //Tính toán bên dưới
                                           AmountVnd = 0, //Tính toán bên dưới
                                           VatAmountVnd = 0, //Tính toán bên dưới
                                           VatPartnerId = sur.PayerId,
                                           VatPartnerCode = pat.TaxCode, //Tax code
                                           VatPartnerName = pat.PartnerNameVn,
                                           VatPartnerAddress = pat.AddressVn,
                                           ObhPartnerCode = obhPat.TaxCode, //Tax code
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
                                               ContraAccount = chgDef.CreditAccountNo,
                                               OrgAmount = sur.Quantity * sur.UnitPrice,
                                               Vat = sur.Vatrate,
                                               OrgVatAmount = 0, //Tính toán bên dưới
                                               VatAccount = chgDef.DebitVat,
                                               Currency = sur.CurrencyId,
                                               ExchangeDate = sur.ExchangeDate,
                                               FinalExchangeRate = sur.FinalExchangeRate,
                                               ExchangeRate = 0, //Tính toán bên dưới
                                               AmountVnd = 0, //Tính toán bên dưới
                                               VatAmountVnd = 0, //Tính toán bên dưới
                                               VatPartnerId = sur.PayerId,
                                               VatPartnerCode = pat.TaxCode, //Tax code
                                               VatPartnerName = pat.PartnerNameVn,
                                               VatPartnerAddress = pat.AddressVn,
                                               ObhPartnerCode = obhPat.TaxCode, //Tax code
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
                fe.OrgVatAmount = (fe.Vat < 101 & fe.Vat > 0) ? ((fe.OrgAmount * fe.Vat) / 100) : (fe.OrgAmount + Math.Abs(fe.Vat ?? 0));
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
                query = query.And(x => criteria.CdNotes.Where(w => !string.IsNullOrEmpty(w)).Contains(x.CdNoteNo));
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

            if (criteria.SettlementCodes != null && criteria.SettlementCodes.Count > 0)
            {
                query = query.And(x => criteria.SettlementCodes.Where(w => !string.IsNullOrEmpty(w)).Contains(x.SettlementCode));
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
                var employeeIdRequester = userRepo.Get(x => x.Id == userIdRequester).FirstOrDefault()?.EmployeeId;
                fe.SettlementRequester = employeeRepo.Get(x => x.Id == employeeIdRequester).FirstOrDefault()?.EmployeeNameVn;
            });
            return chargeGroupByPartner;
        }
        #endregion --- VOUCHER ---

        #region --- CREATE/UPDATE ---
        public HandleState AddAcctMgnt(AccAccountingManagementModel model)
        {
            try
            {
                model.Id = Guid.NewGuid();
                model.Status = "New";
                //Tính toán total amount theo currency
                model.TotalAmount = CaculatorTotalAmount(model);
                model.UserCreated = model.UserModified = currentUser.UserID;
                model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                model.GroupId = currentUser.GroupId;
                model.DepartmentId = currentUser.DepartmentId;
                model.OfficeId = currentUser.OfficeID;
                model.CompanyId = currentUser.CompanyID;
                var accounting = mapper.Map<AccAccountingManagement>(model);

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Add(accounting);
                        if (hs.Success)
                        {
                            var chargesOfAcct = model.Charges;
                            foreach (var chargeOfAcct in chargesOfAcct)
                            {
                                var charge = surchargeRepo.Get(x => x.Id == chargeOfAcct.SurchargeId).FirstOrDefault();
                                charge.AcctManagementId = accounting.Id;
                                charge.FinalExchangeRate = chargeOfAcct.ExchangeRate; //Cập nhật lại Final Exchange Rate
                                if (accounting.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                                {
                                    charge.VoucherId = accounting.VoucherId;
                                    charge.VoucherIddate = accounting.Date;
                                }
                                if (accounting.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                                {
                                    charge.InvoiceNo = accounting.InvoiceNoReal;
                                    charge.InvoiceDate = accounting.Date;
                                }
                                charge.DatetimeModified = DateTime.Now;
                                charge.UserModified = currentUser.UserID;
                                surchargeRepo.Update(charge, x => x.Id == charge.Id, false);
                            }
                            surchargeRepo.SubmitChanges();
                            DataContext.SubmitChanges();
                            trans.Commit();
                        }
                        return hs;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public HandleState UpdateAcctMngt(AccAccountingManagementModel model)
        {
            try
            {
                var accounting = mapper.Map<AccAccountingManagement>(model);
                var acctCurrent = DataContext.Get(x => x.Id == accounting.Id).FirstOrDefault();
                var accountingType = (acctCurrent.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE ? "Vat Invoice" : "Voucher");
                if (acctCurrent == null) return new HandleState("Not found " + accountingType);
                //Tính toán total amount theo currency
                accounting.TotalAmount = CaculatorTotalAmount(model);
                accounting.UserModified = currentUser.UserID;
                accounting.DatetimeModified = DateTime.Now;
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Update(accounting, x => x.Id == accounting.Id);
                        if (hs.Success)
                        {
                            // Gỡ bỏ hết charge có tham chiếu tới Id Accounting
                            // Gỡ bỏ luôn các Invoice No, Invoice Date, Voucher No, Voucher Date
                            var surchargeOfAcctCurrent = surchargeRepo.Get(x => x.AcctManagementId == accounting.Id);
                            foreach (var surchargeOfAcct in surchargeOfAcctCurrent)
                            {
                                surchargeOfAcct.AcctManagementId = null;
                                surchargeOfAcct.FinalExchangeRate = null;
                                if (accounting.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                                {
                                    surchargeOfAcct.VoucherId = null;
                                    surchargeOfAcct.VoucherIddate = null;
                                }
                                if (accounting.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                                {
                                    surchargeOfAcct.InvoiceNo = null;
                                    surchargeOfAcct.InvoiceDate = null;
                                }
                                surchargeOfAcct.DatetimeModified = DateTime.Now;
                                surchargeOfAcct.UserModified = currentUser.UserID;
                                surchargeRepo.Update(surchargeOfAcct, x => x.Id == surchargeOfAcct.Id, false);
                            }

                            //Update lại
                            var chargesOfAcctUpdate = model.Charges;
                            foreach (var chargeOfAcct in chargesOfAcctUpdate)
                            {
                                var charge = surchargeRepo.Get(x => x.Id == chargeOfAcct.SurchargeId).FirstOrDefault();
                                charge.AcctManagementId = accounting.Id;
                                charge.FinalExchangeRate = chargeOfAcct.ExchangeRate; //Cập nhật lại Final Exchange Rate
                                if (accounting.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                                {
                                    charge.VoucherId = accounting.VoucherId;
                                    charge.VoucherIddate = accounting.Date;
                                }
                                if (accounting.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                                {
                                    charge.InvoiceNo = accounting.InvoiceNoReal;
                                    charge.InvoiceDate = accounting.Date;
                                }
                                charge.DatetimeModified = DateTime.Now;
                                charge.UserModified = currentUser.UserID;
                                surchargeRepo.Update(charge, x => x.Id == charge.Id, false);
                            }
                            surchargeRepo.SubmitChanges();
                            DataContext.SubmitChanges();
                            trans.Commit();
                        }
                        return hs;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        private decimal CaculatorTotalAmount(AccAccountingManagementModel model)
        {
            decimal total = 0;
            if (!string.IsNullOrEmpty(model.Currency))
            {
                model.Charges.ForEach(fe =>
                {
                    var exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(fe.FinalExchangeRate, fe.ExchangeDate, fe.Currency, model.Currency);
                    total += exchangeRate * fe.OrgVatAmount ?? 0;
                });
            }
            return total;
        }

        public string GenerateVoucherId()
        {
            int monthCurrent = DateTime.Now.Month;
            string year = DateTime.Now.Year.ToString();
            string month = (monthCurrent < 10 ? "0" : string.Empty) + monthCurrent.ToString();
            string no = "001";

            var voucherNewests = Get(x => x.VoucherId.Substring(0, 4) == year && x.VoucherId.Substring(11, 2) == month).OrderByDescending(o => o.VoucherId).Select(s => s.VoucherId);
            var voucherNewest = voucherNewests.FirstOrDefault();
            if (!string.IsNullOrEmpty(voucherNewest))
            {
                var _noNewest = voucherNewest.Substring(7, 3);
                if (_noNewest != "999")
                {
                    no = (Convert.ToInt32(_noNewest) + 1).ToString();
                    no = no.PadLeft(3, '0');
                }
            }
            var voucher = year + "FDT" + no + "/" + month;
            return voucher;
        }

        public string GenerateInvoiceNoTemp()
        {
            string no = "0000001";
            int outNum = 0;
            // var invoiceNos = Get(x => int.TryParse(x.InvoiceNoTempt, out outNum)).OrderByDescending(o => o.InvoiceNoTempt).Select(s => s.InvoiceNoTempt);
            var invoiceNos = Get(x => int.TryParse(x.InvoiceNoTempt, out outNum)).Select(s => s.InvoiceNoTempt.PadLeft(7, '0')).OrderByDescending(o => o);
            var invoiceNoNewest = invoiceNos.FirstOrDefault();
            if (!string.IsNullOrEmpty(invoiceNoNewest))
            {
                no = (Convert.ToInt32(invoiceNoNewest) + 1).ToString();
                no = no.PadLeft(7, '0');
            }
            return no;
        }
        #endregion -- CREATE/UPDATE ---

        #region --- DETAIL ---
        public AccAccountingManagementModel GetById(Guid id)
        {
            var accounting = DataContext.Get(x => x.Id == id).FirstOrDefault();
            var result = mapper.Map<AccAccountingManagementModel>(accounting);
            if (accounting != null)
            {
                Expression<Func<ChargeOfAccountingManagementModel, bool>> expressionQuery = chg => chg.AcctManagementId == id;
                if (accounting.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                {
                    result.Charges = GetChargeSellForInvoice(expressionQuery);
                }
                else
                {
                    result.Charges = GetChargeForVoucher(expressionQuery);
                }

                result.UserNameCreated = userRepo.Where(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
                result.UserNameModified = userRepo.Where(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
            }
            return result;
        }
        #endregion --- DETAIL ---

        #region --- EXPORT ---
        private string GetCustomNoOldOfShipment(string jobNo)
        {
            var customNos = customsDeclarationRepo.Get(x => x.JobNo == jobNo).OrderBy(o => o.DatetimeModified).Select(s => s.ClearanceNo);
            return customNos.FirstOrDefault();
        }

        public List<AccountingManagementExport> GetDataAcctMngtExport(string typeOfAcctMngt)
        {
            var accountings = DataContext.Get(x => x.Type == typeOfAcctMngt);            
            var partners = partnerRepo.Get();
            var data = new List<AccountingManagementExport>();
            foreach(var acct in accountings)
            {
                Expression<Func<ChargeOfAccountingManagementModel, bool>> expressionQuery = chg => chg.AcctManagementId == acct.Id;
                var charges = new List<ChargeOfAccountingManagementModel>();
                if (acct.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                {
                    charges = GetChargeSellForInvoice(expressionQuery);
                }
                else
                {
                    charges = GetChargeForVoucher(expressionQuery); ;
                }
                foreach(var charge in charges)
                {
                    string _deptCode = string.Empty;
                    if (!string.IsNullOrEmpty(charge.JobNo))
                    {
                        if (charge.JobNo.Contains("LOG"))
                        {
                            _deptCode = "OPS";
                        }
                        else if (charge.JobNo.Contains("A"))
                        {
                            _deptCode = "AIR";
                        }
                        else if (charge.JobNo.Contains("S"))
                        {
                            _deptCode = "SEA";
                        }
                    }
                    string _paymentMethod = string.Empty;
                    if (!string.IsNullOrEmpty(acct.PaymentMethod))
                    {
                        if (acct.PaymentMethod.Contains("Cash"))
                        {
                            _paymentMethod = "TM";
                        }
                        else if (acct.PaymentMethod.Contains("Bank"))
                        {
                            _paymentMethod = "CK";
                        }
                    }
                    string _statusInvoice = string.Empty;
                    if(acct.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                    {
                        if (acct.Status != "New")
                        {
                            _statusInvoice = "Đã phát hành";
                        }
                    }
                    var vatPartner = partners.Where(x => x.Id == charge.VatPartnerId).FirstOrDefault();
                    var partnerAcct = partners.Where(x => x.Id == acct.PartnerId).FirstOrDefault();
                    var item = new AccountingManagementExport();
                    item = mapper.Map<AccountingManagementExport>(charge);
                    item.Date = acct.Date; //Date trên VAT Invoice Or Voucher
                    item.VoucherId = acct.VoucherId; //VoucherId trên VAT Invoice Or Voucher
                    item.PartnerId = partnerAcct.AccountNo; //Partner ID trên VAT Invoice Or Voucher
                    item.AccountNo = acct.AccountNo; //Account No trên VAT Invoice Or Voucher
                    item.VatPartnerNameEn = vatPartner?.PartnerNameEn; //Partner Name En của Charge
                    item.Description = acct.Description;
                    item.IsTick = true; //Default is True
                    item.PaymentTerm = 0; //Default is 0                    
                    item.DepartmentCode =  _deptCode;
                    item.CustomNo = GetCustomNoOldOfShipment(charge.JobNo);                   
                    item.PaymentMethod = _paymentMethod;
                    item.StatusInvoice = _statusInvoice; //Tình trạng hóa đơn (Dùng cho Invoice)
                    item.VatPartnerEmail = vatPartner?.Email; //Email Partner của charge
                    item.ReleaseDateEInvoice = null;

                    data.Add(item);
                }
            }
            
            return data;
        }


        #endregion --- EXPORT ---

        public List<AcctMngtVatInvoiceImportModel> CheckVatInvoiceImport(List<AcctMngtVatInvoiceImportModel> list)
        {
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.VoucherId))
                {
                    item.VoucherId = stringLocalizer[AccountingLanguageSub.MSG_VOUCHER_ID_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    // Danh sách có 2 dòng Voucher ID giống nhau
                    if (list.Count(x => x.VoucherId?.ToLower() == item.VoucherId?.ToLower()) > 1)
                    {
                        item.VoucherId = string.Format(stringLocalizer[AccountingLanguageSub.MSG_VOUCHER_ID_DUPLICATE], item.VoucherId);
                        item.IsValid = false;
                    }
                    else
                    {
                        // Không tìm thấy voucher ID
                        if (!DataContext.Any(x => x.VoucherId == item.VoucherId && x.Type == "Invoice"))
                        {
                            item.VoucherId = stringLocalizer[AccountingLanguageSub.MSG_VOUCHER_ID_NOT_EXIST, item.VoucherId];
                            item.IsValid = false;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(item.RealInvoiceNo))
                            {
                                item.RealInvoiceNo = stringLocalizer[AccountingLanguageSub.MSG_INVOICE_NO_NOT_EMPTY];
                                item.IsValid = false;
                            }
                            if (string.IsNullOrEmpty(item.SerieNo))
                            {
                                item.RealInvoiceNo = stringLocalizer[AccountingLanguageSub.MSG_SERIE_NO_NOT_EMPTY];
                                item.IsValid = false;
                            }
                            //else
                            //{
                            //    // Trùng Invoice, Serie, VoucherID #
                            //    if (DataContext.Any(x => x.InvoiceNoReal == item.RealInvoiceNo && x.Serie == item.SerieNo))
                            //    {
                            //        item.RealInvoiceNo = stringLocalizer[AccountingLanguageSub.MSG_INVOICE_DUPLICATE];
                            //        item.SerieNo = stringLocalizer[AccountingLanguageSub.MSG_SERIE_NO_DUPLICATE];
                            //        item.VoucherId = stringLocalizer[AccountingLanguageSub.MSG_VOUCHER_ID_DUPLICATE];
                            //        item.IsValid = false;
                            //    }
                            //}
                        }
                    }
                }
            });
            return list;
        }

        public ResultHandle ImportVatInvoice(List<AcctMngtVatInvoiceImportModel> list)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in list)
                    {
                        var vatInvoice = DataContext.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE && x.VoucherId == item.VoucherId)?.FirstOrDefault();
                        vatInvoice.InvoiceNoTempt = vatInvoice.InvoiceNoReal = item.RealInvoiceNo;
                        vatInvoice.Date = item.InvoiceDate;
                        vatInvoice.Status = AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED;
                        vatInvoice.UserModified = currentUser.UserID;
                        vatInvoice.DatetimeModified = DateTime.Now;

                        IQueryable<CsShipmentSurcharge> surchargeOfAcctCurrent = surchargeRepo.Get(x => x.AcctManagementId == vatInvoice.Id);

                        if(surchargeOfAcctCurrent != null)
                        {
                            foreach (var surcharge in surchargeOfAcctCurrent)
                            {

                                surcharge.InvoiceNo = item.RealInvoiceNo;
                                surcharge.InvoiceDate = item.InvoiceDate;
                                surcharge.SeriesNo = item.SerieNo;

                                surcharge.DatetimeModified = DateTime.Now;
                                surcharge.UserModified = currentUser.UserID;

                                surchargeRepo.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }
                        }
                        
                        DataContext.Update(vatInvoice, x => x.VoucherId == item.VoucherId);

                    }
                    surchargeRepo.SubmitChanges();
                    DataContext.SubmitChanges();
                    trans.Commit();
                    return new ResultHandle { Status = true, Message = "Import Vat Invoice successfully" };

                }
                catch (Exception ex)
                {
                    var result = new HandleState(ex.Message);
                    return new ResultHandle { Data = new object { }, Message = ex.Message, Status = true };
                }
                finally
                {
                    trans.Dispose();
                }
            }
               
        }
    }
}
