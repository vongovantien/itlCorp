using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccAccountReceivableService : RepositoryBase<AccAccountReceivable, AccAccountReceivableModel>, IAccAccountReceivableService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> userRepo;
        private readonly IContextBase<CatPartner> partnerRepo;
        private readonly IContextBase<CatContract> contractPartnerRepo;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepo;
        private readonly IContextBase<OpsTransaction> opsRepo;
        private readonly IContextBase<CsTransaction> transactionRepo;
        private readonly IContextBase<CsTransactionDetail> transactionDetailRepo;
        private readonly IContextBase<AcctSoa> soaRepo;
        private readonly IContextBase<AcctAdvancePayment> advancePaymentRepo;
        private readonly IContextBase<AcctSettlementPayment> settlementPaymentRepo;
        private readonly IContextBase<AccAccountingManagement> accountingManagementRepo;
        private readonly IContextBase<AccAccountingPayment> accountingPaymentRepo;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<CatCurrencyExchange> currencyExchangeRepo;

        public AccAccountReceivableService(IContextBase<AccAccountReceivable> repository,
            IMapper mapper,
            ICurrentUser currUser,
            IContextBase<SysUser> user,
            IContextBase<CatPartner> partner,
            IContextBase<CatContract> contractPartner,
            IContextBase<CsShipmentSurcharge> surcharge,
            IContextBase<OpsTransaction> ops,
            IContextBase<CsTransaction> transaction,
            IContextBase<CsTransactionDetail> transactionDetail,
            IContextBase<AcctSoa> soa,
            IContextBase<AcctAdvancePayment> advancePayment,
            IContextBase<AcctSettlementPayment> settlementPayment,
            IContextBase<AccAccountingManagement> accountingManagement,
            IContextBase<AccAccountingPayment> accountingPayment,
            ICurrencyExchangeService currencyExchange,
            IContextBase<CatCurrencyExchange> catCurrencyExchange) : base(repository, mapper)
        {
            currentUser = currUser;
            userRepo = user;
            partnerRepo = partner;
            contractPartnerRepo = contractPartner;
            surchargeRepo = surcharge;
            opsRepo = ops;
            transactionRepo = transaction;
            transactionDetailRepo = transactionDetail;
            soaRepo = soa;
            advancePaymentRepo = advancePayment;
            settlementPaymentRepo = settlementPayment;
            accountingManagementRepo = accountingManagement;
            accountingPaymentRepo = accountingPayment;
            currencyExchangeService = currencyExchange;
            currencyExchangeRepo = catCurrencyExchange;
        }

        #region --- CALCULATOR VALUE ---
        private decimal? CalculatorBillingAmount(AccAccountReceivableModel model)
        {
            decimal? billingAmount = 0;
            //Lấy ra VAT Invoice có payment status # Paid
            var acctMngts = accountingManagementRepo.Get(x => x.Type == "Invoice" && x.PaymentStatus != "Paid");
            //Lấy ra các phí thu (SELLING)
            var surcharges = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == "SELL" && x.AcctManagementId != null);

            //Service là Custom Logistic
            if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
                var chargeOperation = from surcharge in surcharges
                                      join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                                      join operation in operations on surcharge.Hblid equals operation.Hblid
                                      select surcharge;
                foreach (var charge in chargeOperation)
                {
                    var _exchangeRateToCurrencyContract = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, model.ContractCurrency);
                    billingAmount += _exchangeRateToCurrencyContract * charge.Total;
                }
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
                var chargeDocumentation = from surcharge in surcharges
                                          join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                                          join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                                          join trans in transactions on transDetail.JobId equals trans.Id
                                          select surcharge;
                foreach (var charge in chargeDocumentation)
                {
                    var _exchangeRateToCurrencyContract = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, model.ContractCurrency);
                    billingAmount += _exchangeRateToCurrencyContract * charge.Total;
                }
            }
            return billingAmount;
        }

        private decimal? CalculatorBillingUnpaid(AccAccountReceivableModel model)
        {
            decimal? billingUnpaid = 0;
            //Lấy ra VAT Invoice có payment status # Paid
            var acctMngts = accountingManagementRepo.Get(x => x.Type == "Invoice" && x.PaymentStatus != "Paid");
            //Lấy ra các phí thu (SELLING)
            var surcharges = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == "SELL" && x.AcctManagementId != null);
            var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
            var acctMngtOperation = from surcharge in surcharges
                                    join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                                    join operation in operations on surcharge.Hblid equals operation.Hblid
                                    select acctMngt;
            var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
            var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
            var acctMngtDocumentation = from surcharge in surcharges
                                        join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                                        join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                                        join trans in transactions on transDetail.JobId equals trans.Id
                                        select acctMngt;
            var acctMngtMerge = acctMngtOperation.Union(acctMngtDocumentation);
            foreach(var acct in acctMngtMerge)
            {                
                var qtyService = !string.IsNullOrEmpty(acct.ServiceType) ? acct.ServiceType.Split(';').Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == acct.DatetimeCreated.Value.Date).ToList();
                var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, acct.Currency, model.ContractCurrency);
                // Chia đều cho số lượng service có trong VAT Invoice
                var unpaidAmount = acct.UnpaidAmount;
                if (acct.UnpaidAmount == 0 || acct.UnpaidAmount == null)
                {
                    var paymentsInvoice = accountingPaymentRepo.Get(x => x.RefId == acct.Id.ToString());
                    unpaidAmount = paymentsInvoice.Sum(s => s.Balance);
                }
                var _unpaidAmount = (unpaidAmount * _exchangeRate) / qtyService;
                billingUnpaid += _unpaidAmount;
            }

            return billingUnpaid;
        }

        private decimal CalculatorPaidAmount(AccAccountReceivableModel model)
        {
            var paidAmount = 0;
            return paidAmount;
        }

        private decimal CalculatorObhAmount(AccAccountReceivableModel model)
        {
            var obhAmount = 0;
            return obhAmount;
        }

        private decimal CalculatorObhUnpaid(AccAccountReceivableModel model)
        {
            var obhUnpaid = 0;
            return obhUnpaid;
        }

        private decimal CalculatorAdvanceAmount(AccAccountReceivableModel model)
        {
            var advanceAmount = 0;
            return advanceAmount;
        }

        private decimal CalculatorCreditAmount(AccAccountReceivableModel model)
        {
            var creditAmount = 0;
            return creditAmount;
        }

        private decimal CalculatorSellingNoVat(AccAccountReceivableModel model)
        {
            var sellingNoVat = 0;
            return sellingNoVat;
        }

        private decimal CalculatorOver1To15Day(AccAccountReceivableModel model)
        {
            var over1To15Day = 0;
            return over1To15Day;
        }

        private decimal CalculatorOver16To30Day(AccAccountReceivableModel model)
        {
            var over16To30Day = 0;
            return over16To30Day;
        }

        private decimal CalculatorOver30Day(AccAccountReceivableModel model)
        {
            var over30Day = 0;
            return over30Day;
        }

        #endregion --- CALCULATOR VALUE ---

        #region --- CRUD ---
        public HandleState AddReceivable(AccAccountReceivableModel model)
        {
            try
            {
                model.Id = Guid.NewGuid();
                var partner = partnerRepo.Get(x => x.Id == model.PartnerId).FirstOrDefault();
                if (partner == null) return new HandleState("Not found partner");

                var contractPartner = contractPartnerRepo.Get(x => x.Active == true
                                                                && x.PartnerId == model.PartnerId
                                                                && x.OfficeId == model.Office.ToString()
                                                                && x.SaleService.Contains(model.Service)
                                                                && x.SaleManId == model.SaleMan).FirstOrDefault();

                if (contractPartner == null)
                {
                    // Lấy currency local và use created of partner gán cho Receivable
                    model.ContractId = null;
                    model.ContractCurrency = AccountingConstants.CURRENCY_LOCAL;
                    model.SaleMan = null;
                    model.UserCreated = partner.UserCreated;
                    model.UserModified = partner.UserCreated;
                    model.GroupId = partner.GroupId;
                    model.DepartmentId = partner.DepartmentId;
                    model.OfficeId = partner.OfficeId;
                    model.CompanyId = partner.CompanyId;
                }
                else
                {
                    // Lấy currency của contract & use created of contract gán cho Receivable
                    model.ContractId = contractPartner.Id;
                    model.ContractCurrency = contractPartner.CurrencyId;
                    model.SaleMan = contractPartner.SaleManId;
                    model.UserCreated = contractPartner.UserCreated;
                    model.UserModified = contractPartner.UserCreated;
                    model.GroupId = null;
                    model.DepartmentId = null;
                    model.OfficeId = Guid.Parse(contractPartner.OfficeId);
                    model.CompanyId = contractPartner.CompanyId;
                }
                model.DatetimeCreated = model.DatetimeModified = DateTime.Now;

                var _billingAmount = CalculatorBillingAmount(model);
                var _billingUnpaid = CalculatorBillingUnpaid(model);
                var _paidAmount = CalculatorPaidAmount(model);
                var _obhAmount = CalculatorObhAmount(model);
                var _obhUnpaid = CalculatorObhUnpaid(model);
                var _advanceAmount = CalculatorAdvanceAmount(model);
                var _creditAmount = CalculatorCreditAmount(model);
                var _sellingNoVat = CalculatorSellingNoVat(model);
                var _over1To15Day = CalculatorOver1To15Day(model);
                var _over16To30Day = CalculatorOver16To30Day(model);
                var _over30Day = CalculatorOver30Day(model);
                model.BillingAmount = _billingAmount;
                model.BillingUnpaid = _billingUnpaid;
                model.PaidAmount = _paidAmount;
                model.ObhAmount = _obhAmount;
                model.ObhUnpaid = _obhUnpaid;
                model.AdvanceAmount = _advanceAmount;
                model.CreditAmount = _creditAmount;
                model.SellingNoVat = _sellingNoVat;
                model.Over1To15Day = _over1To15Day;
                model.Over16To30Day = _over16To30Day;
                model.Over30Day = _over30Day;

                AccAccountReceivable receivable = mapper.Map<AccAccountReceivable>(model);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Add(receivable, false);
                        if (hs.Success)
                        {
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
                return new HandleState(ex.Message);
            }
        }

        public HandleState UpdateReceivable()
        {
            return new HandleState();
        }
        #endregion --- CRUD ---

        #region --- LIST & PAGING ---
        #endregion --- LIST & PAGING ---

        #region --- DETAIL ---
        #endregion --- DETAIL ---
    }
}
