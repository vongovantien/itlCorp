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
                var _unpaidAmount = acct.UnpaidAmount;
                if (acct.UnpaidAmount == 0 || acct.UnpaidAmount == null)
                {
                    var paymentsInvoice = accountingPaymentRepo.Get(x => x.RefId == acct.Id.ToString());
                    _unpaidAmount = paymentsInvoice.Sum(s => s.Balance);// Unpaid sẽ sum theo Balance
                }
                _unpaidAmount = (_unpaidAmount * _exchangeRate) / qtyService;
                billingUnpaid += _unpaidAmount;
            }

            return billingUnpaid;
        }

        private decimal? CalculatorPaidAmount(AccAccountReceivableModel model)
        {
            decimal? paidAmount = 0;
            //Lấy ra VAT Invoice có payment status là Paid A Part & có status là Updated Invoice
            var acctMngts = accountingManagementRepo.Get(x => x.Type == "Invoice" && x.PaymentStatus == "Paid A Part" && x.Status == "Updated Invoice");
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
            foreach (var acct in acctMngtMerge)
            {
                var qtyService = !string.IsNullOrEmpty(acct.ServiceType) ? acct.ServiceType.Split(';').Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == acct.DatetimeCreated.Value.Date).ToList();
                var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, acct.Currency, model.ContractCurrency);
                // Chia đều cho số lượng service có trong VAT Invoice
                var _paidAmount = acct.PaidAmount;
                if (acct.PaidAmount == 0 || acct.PaidAmount == null)
                {
                    var paymentsInvoice = accountingPaymentRepo.Get(x => x.RefId == acct.Id.ToString());
                    _paidAmount = paymentsInvoice.Sum(s => s.PaymentAmount); //Paid sẽ sum theo PaymentAmount
                }
                _paidAmount = (_paidAmount * _exchangeRate) / qtyService;
                paidAmount += _paidAmount;
            }
            return paidAmount;
        }

        private decimal? CalculatorObhAmount(AccAccountReceivableModel model)
        {
            decimal? obhAmount = 0;
            //Lấy ra các phí OBH theo đối tượng OBH Partner (PaymentObjectId)
            var surcharges = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == "OBH");
            var soas = soaRepo.Get(x => x.PaymentStatus != "Paid");
            var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
            var chargeOperation = from surcharge in surcharges
                                  join soa in soas on surcharge.Soano equals soa.Soano into soa2
                                  from soa in soa2.DefaultIfEmpty()
                                  join operation in operations on surcharge.Hblid equals operation.Hblid
                                  select surcharge;
            var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
            var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
            var chargeDocumentation = from surcharge in surcharges
                                      join soa in soas on surcharge.Soano equals soa.Soano into soa2
                                      from soa in soa2.DefaultIfEmpty()
                                      join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                                      join trans in transactions on transDetail.JobId equals trans.Id
                                      select surcharge;
            var chargeMerge = chargeOperation.Union(chargeDocumentation);
            foreach (var charge in chargeMerge)
            {
                var _exchangeRateToCurrencyContract = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, model.ContractCurrency);
                obhAmount += _exchangeRateToCurrencyContract * charge.Total;
            }
            return obhAmount;
        }

        private decimal? CalculatorObhUnpaid(AccAccountReceivableModel model)
        {
            decimal? obhUnpaid = 0;
            var surchargesPO = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == "OBH");
            var surchargesPR = surchargeRepo.Get(x => x.PayerId == model.PartnerId && x.Type == "OBH");
            var soas = soaRepo.Get(x => x.PaymentStatus != "Paid");
            
            if (model.Service == "CL")
            {
                #region --- Operation ---
                var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
                var chargeOperationPO = from surcharge in surchargesPO
                                        join soa in soas on surcharge.Soano equals soa.Soano
                                        join operation in operations on surcharge.Hblid equals operation.Hblid
                                        select new { soaId = soa.Id, soaNo = soa.Soano, soaCreatedDate = soa.DatetimeCreated, soaCurrency = soa.Currency, serviceType = surcharge.TransactionType };
                var chargeOperationPR = from surcharge in surchargesPR
                                        join soa in soas on surcharge.PaySoano equals soa.Soano
                                        join operation in operations on surcharge.Hblid equals operation.Hblid
                                        select new { soaId = soa.Id, soaNo = soa.Soano, soaCreatedDate = soa.DatetimeCreated, soaCurrency = soa.Currency, serviceType = surcharge.TransactionType };
                var chargeOperation = chargeOperationPO.Union(chargeOperationPR);

                var dataGroupSoa = chargeOperation.ToList().GroupBy(g => new { soaId = g.soaId, soaNo = g.soaNo, soaCreatedDate = g.soaCreatedDate, soaCurrency = g.soaCurrency }).Select(s => new
                {
                    soaId = s.Key.soaId,
                    soaNo = s.Key.soaNo,
                    soaCreatedDate = s.Key.soaCreatedDate,
                    soaCurrency = s.Key.soaCurrency,
                    services = s.Select(se => se.serviceType).ToList()
                });

                foreach (var soa in dataGroupSoa)
                {
                    var qtyService = soa.services.Count() > 0 ? soa.services.Count() : 1;
                    var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == soa.soaCreatedDate.Value.Date).ToList();
                    var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, soa.soaCurrency, model.ContractCurrency);
                    // Chia đều cho số lượng service có trong SOA
                    decimal? _obhUnpaid = 0;
                    var paymentsObh = accountingPaymentRepo.Get(x => x.RefId == soa.soaId.ToString());
                    _obhUnpaid = paymentsObh.Sum(s => s.Balance);// Unpaid sẽ sum theo Balance
                    _obhUnpaid = (_obhUnpaid * _exchangeRate) / qtyService;
                    obhUnpaid += _obhUnpaid;
                }
                #endregion --- Operation ---
            }
            else
            {
                #region --- Documentation ---
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
                var chargeDocumentationPO = from surcharge in surchargesPO
                                            join soa in soas on surcharge.Soano equals soa.Soano
                                            join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                                            join trans in transactions on transDetail.JobId equals trans.Id
                                            select new { soaId = soa.Id, soaNo = soa.Soano, soaCreatedDate = soa.DatetimeCreated, soaCurrency = soa.Currency, serviceType = surcharge.TransactionType };
                var chargeDocumentationPR = from surcharge in surchargesPR
                                            join soa in soas on surcharge.PaySoano equals soa.Soano
                                            join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                                            join trans in transactions on transDetail.JobId equals trans.Id
                                            select new { soaId = soa.Id, soaNo = soa.Soano, soaCreatedDate = soa.DatetimeCreated, soaCurrency = soa.Currency, serviceType = surcharge.TransactionType };
                var chargeDocumentation = chargeDocumentationPO.Union(chargeDocumentationPR);

                var dataGroupSoa = chargeDocumentation.ToList().GroupBy(g => new { soaId = g.soaId, soaNo = g.soaNo, soaCreatedDate = g.soaCreatedDate, soaCurrency = g.soaCurrency }).Select(s => new
                {
                    soaId = s.Key.soaId,
                    soaNo = s.Key.soaNo,
                    soaCreatedDate = s.Key.soaCreatedDate,
                    soaCurrency = s.Key.soaCurrency,
                    services = s.Select(se => se.serviceType).ToList()
                });

                foreach (var soa in dataGroupSoa)
                {
                    var qtyService = soa.services.Count() > 0 ? soa.services.Count() : 1;
                    var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == soa.soaCreatedDate.Value.Date).ToList();
                    var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, soa.soaCurrency, model.ContractCurrency);
                    // Chia đều cho số lượng service có trong SOA
                    decimal? _obhUnpaid = 0;
                    var paymentsObh = accountingPaymentRepo.Get(x => x.RefId == soa.soaId.ToString());
                    _obhUnpaid = paymentsObh.Sum(s => s.Balance);// Unpaid sẽ sum theo Balance
                    _obhUnpaid = (_obhUnpaid * _exchangeRate) / qtyService;
                    obhUnpaid += _obhUnpaid;
                }
                #endregion --- Documentation ---
            }                        
            return obhUnpaid;
        }

        private decimal? CalculatorAdvanceAmount(AccAccountReceivableModel model)
        {
            decimal? advanceAmount = 0;
            return advanceAmount;
        }

        private decimal? CalculatorCreditAmount(AccAccountReceivableModel model)
        {
            decimal? creditAmount = 0;
            return creditAmount;
        }

        private decimal? CalculatorSellingNoVat(AccAccountReceivableModel model)
        {
            decimal? sellingNoVat = 0;
            return sellingNoVat;
        }

        private decimal? CalculatorOver1To15Day(AccAccountReceivableModel model)
        {
            decimal? over1To15Day = 0;
            return over1To15Day;
        }

        private decimal? CalculatorOver16To30Day(AccAccountReceivableModel model)
        {
            decimal? over16To30Day = 0;
            return over16To30Day;
        }

        private decimal? CalculatorOver30Day(AccAccountReceivableModel model)
        {
            decimal? over30Day = 0;
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

                model.AcRef = partner.ParentId;

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
