using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountReceivable;
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
        private readonly IContextBase<AcctAdvanceRequest> advanceRequestRepo;
        private readonly IContextBase<SysEmployee> employeeRepo;
        private readonly IContextBase<SysOffice> officeRepo;

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
            IContextBase<CatCurrencyExchange> catCurrencyExchange,
            IContextBase<AcctAdvanceRequest> advanceRequest,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<SysOffice> sysOffice) : base(repository, mapper)
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
            advanceRequestRepo = advanceRequest;
            employeeRepo = sysEmployee;
            officeRepo = sysOffice;
        }

        #region --- CALCULATOR VALUE ---
        private decimal? CalculatorBillingAmount(AccAccountReceivableModel model)
        {
            decimal? billingAmount = 0;
            //Get VAT Invoice have payment status # Paid
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
            //Get Debit charge (SELLING)
            var surcharges = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == AccountingConstants.TYPE_CHARGE_SELL && x.AcctManagementId != null);

            IQueryable<CsShipmentSurcharge> charges = null;

            //Service là Custom Logistic
            if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
                charges = from surcharge in surcharges
                          join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                          join operation in operations on surcharge.Hblid equals operation.Hblid
                          select surcharge;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
                charges = from surcharge in surcharges
                          join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                          join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                          join trans in transactions on transDetail.JobId equals trans.Id
                          select surcharge;

            }
            foreach (var charge in charges)
            {
                var _exchangeRateToCurrencyContract = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, model.ContractCurrency);
                billingAmount += _exchangeRateToCurrencyContract * charge.Total;
            }
            return billingAmount;
        }

        private decimal? CalculatorBillingUnpaid(AccAccountReceivableModel model)
        {
            decimal? billingUnpaid = 0;
            //Get VAT Invoice have payment status # Paid
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
            billingUnpaid = UnpaidAmountVatInvoice(model, acctMngts);
            return billingUnpaid;
        }

        private decimal? CalculatorPaidAmount(AccAccountReceivableModel model)
        {
            decimal? paidAmount = 0;
            //Get VAT Invoice have payment status is Paid A Part & status is Updated Invoice
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE && x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED);
            //Get Debit charge (SELLING)
            var surcharges = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == AccountingConstants.TYPE_CHARGE_SELL && x.AcctManagementId != null);

            IQueryable<AccAccountingManagement> accountants = null;
            if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
                accountants = from surcharge in surcharges
                              join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                              join operation in operations on surcharge.Hblid equals operation.Hblid
                              select acctMngt;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
                accountants = from surcharge in surcharges
                              join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                              join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                              join trans in transactions on transDetail.JobId equals trans.Id
                              select acctMngt;
            }
            foreach (var acct in accountants)
            {
                var qtyService = !string.IsNullOrEmpty(acct.ServiceType) ? acct.ServiceType.Split(';').Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == acct.DatetimeCreated.Value.Date).ToList();
                if (currencyExchange.Count == 0)
                {
                    var maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                    currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                }
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
            //Get OBH charge by OBH Partner (PaymentObjectId)
            var surcharges = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == AccountingConstants.TYPE_CHARGE_OBH);
            var soas = soaRepo.Get(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);

            IQueryable<CsShipmentSurcharge> charges = null;
            if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
                charges = from surcharge in surcharges
                          join soa in soas on surcharge.Soano equals soa.Soano into soa2
                          from soa in soa2.DefaultIfEmpty()
                          join operation in operations on surcharge.Hblid equals operation.Hblid
                          select surcharge;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
                charges = from surcharge in surcharges
                          join soa in soas on surcharge.Soano equals soa.Soano into soa2
                          from soa in soa2.DefaultIfEmpty()
                          join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                          join trans in transactions on transDetail.JobId equals trans.Id
                          select surcharge;
            }
            foreach (var charge in charges)
            {
                var _exchangeRateToCurrencyContract = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, model.ContractCurrency);
                obhAmount += _exchangeRateToCurrencyContract * charge.Total;
            }
            return obhAmount;
        }

        private decimal? CalculatorObhUnpaid(AccAccountReceivableModel model)
        {
            decimal? obhUnpaid = 0;
            // Get SOA have payment status # Paid
            var soas = soaRepo.Get(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
            obhUnpaid = ObhUnpaidAmountSoa(model, soas);
            return obhUnpaid;
        }

        private decimal? CalculatorAdvanceAmount(AccAccountReceivableModel model)
        {
            decimal? advanceAmount = 0;
            var surcharges = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId || x.PayerId == model.PartnerId);
            IQueryable<CsShipmentSurcharge> charges = null;
            if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
                charges = from surcharge in surcharges
                          join operation in operations on surcharge.Hblid equals operation.Hblid
                          select surcharge;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
                charges = from surcharge in surcharges
                          join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                          join trans in transactions on transDetail.JobId equals trans.Id
                          select surcharge;
            }

            //Get settlement have Payment Status # Done
            var settlements = settlementPaymentRepo.Get(x => x.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE);
            var advanceRequests = advanceRequestRepo.Get();
            var dataAdvanceRequests = (from advRequest in advanceRequests
                                       join charge in charges on advRequest.Hblid equals charge.Hblid
                                       join settlement in settlements on charge.SettlementCode equals settlement.SettlementNo into settlement2
                                       from settlement in settlement2.DefaultIfEmpty()
                                       select advRequest).Distinct();
            foreach (var advanceRequest in dataAdvanceRequests)
            {
                var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == advanceRequest.DatetimeCreated.Value.Date).ToList();
                if (currencyExchange.Count == 0)
                {
                    var maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                    currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                }
                var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, advanceRequest.RequestCurrency, model.ContractCurrency);
                advanceAmount += _exchangeRate * advanceRequest.Amount;
            }
            return advanceAmount;
        }

        private decimal? CalculatorCreditAmount(AccAccountReceivableModel model)
        {
            decimal? creditAmount = 0;
            //Lấy ra các phí chi (BUYING) chưa có Voucher
            var surcharges = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == AccountingConstants.TYPE_CHARGE_BUY && string.IsNullOrEmpty(x.VoucherId));

            IQueryable<CsShipmentSurcharge> charges = null;
            //Service là Custom Logistic
            if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
                charges = from surcharge in surcharges
                          join operation in operations on surcharge.Hblid equals operation.Hblid
                          select surcharge;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
                charges = from surcharge in surcharges
                          join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                          join trans in transactions on transDetail.JobId equals trans.Id
                          select surcharge;
            }
            foreach (var charge in charges)
            {
                var _exchangeRateToCurrencyContract = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, model.ContractCurrency);
                creditAmount += _exchangeRateToCurrencyContract * charge.Total;
            }
            return creditAmount;
        }

        private decimal? CalculatorSellingNoVat(AccAccountReceivableModel model)
        {
            decimal? sellingNoVat = 0;
            //Lấy ra các phí thu (SELLING) chưa có Invoice
            var surcharges = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == AccountingConstants.TYPE_CHARGE_SELL && string.IsNullOrEmpty(x.InvoiceNo));

            IQueryable<CsShipmentSurcharge> charges = null;
            //Service là Custom Logistic
            if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
                charges = from surcharge in surcharges
                          join operation in operations on surcharge.Hblid equals operation.Hblid
                          select surcharge;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
                charges = from surcharge in surcharges
                          join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                          join trans in transactions on transDetail.JobId equals trans.Id
                          select surcharge;
            }
            foreach (var charge in charges)
            {
                var _exchangeRateToCurrencyContract = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.CurrencyId, model.ContractCurrency);
                sellingNoVat += _exchangeRateToCurrencyContract * charge.Total;
            }
            return sellingNoVat;
        }

        private decimal? UnpaidAmountVatInvoice(AccAccountReceivableModel model, IQueryable<AccAccountingManagement> accountingManagements)
        {
            decimal? unpaidAmountVatInvoice = 0;
            //Lấy ra các phí thu (SELLING)
            var surcharges = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == AccountingConstants.TYPE_CHARGE_SELL && x.AcctManagementId != null);

            IQueryable<AccAccountingManagement> accountants = null;
            if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office); // && x.BillingOpsId == model.SaleMan
                accountants = from surcharge in surcharges
                              join acctMngt in accountingManagements on surcharge.AcctManagementId equals acctMngt.Id
                              join operation in operations on surcharge.Hblid equals operation.Hblid
                              select acctMngt;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office); // && x.SaleManId == model.SaleMan
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service);
                accountants = from surcharge in surcharges
                              join acctMngt in accountingManagements on surcharge.AcctManagementId equals acctMngt.Id
                              join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                              join trans in transactions on transDetail.JobId equals trans.Id
                              select acctMngt;
            }

            foreach (var acct in accountants)
            {
                var qtyService = !string.IsNullOrEmpty(acct.ServiceType) ? acct.ServiceType.Split(';').Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == acct.DatetimeCreated.Value.Date).ToList();
                if (currencyExchange.Count == 0)
                {
                    var maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                    currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                }
                var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, acct.Currency, model.ContractCurrency);
                // Chia đều cho số lượng service có trong VAT Invoice
                var _unpaidAmount = acct.UnpaidAmount;
                if (acct.UnpaidAmount == 0 || acct.UnpaidAmount == null)
                {
                    var paymentsInvoice = accountingPaymentRepo.Get(x => x.RefId == acct.Id.ToString());
                    _unpaidAmount = paymentsInvoice.Sum(s => s.Balance);// Unpaid sẽ sum theo Balance
                }
                _unpaidAmount = (_unpaidAmount * _exchangeRate) / qtyService;
                unpaidAmountVatInvoice += _unpaidAmount;
            }
            return unpaidAmountVatInvoice;
        }

        private decimal? ObhUnpaidAmountSoa(AccAccountReceivableModel model, IQueryable<AcctSoa> soas)
        {
            decimal? obhUnpaidAmount = 0;
            var surchargesPO = surchargeRepo.Get(x => x.PaymentObjectId == model.PartnerId && x.Type == AccountingConstants.TYPE_CHARGE_OBH);
            var surchargesPR = surchargeRepo.Get(x => x.PayerId == model.PartnerId && x.Type == AccountingConstants.TYPE_CHARGE_OBH);

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
                    if (currencyExchange.Count == 0)
                    {
                        var maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                        currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                    }
                    var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, soa.soaCurrency, model.ContractCurrency);
                    // Chia đều cho số lượng service có trong SOA
                    decimal? _obhUnpaid = 0;
                    var paymentsObh = accountingPaymentRepo.Get(x => x.RefId == soa.soaId.ToString());
                    _obhUnpaid = paymentsObh.Sum(s => s.Balance);// Unpaid sẽ sum theo Balance
                    _obhUnpaid = (_obhUnpaid * _exchangeRate) / qtyService;
                    obhUnpaidAmount += _obhUnpaid;
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
                    if (currencyExchange.Count == 0)
                    {
                        var maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                        currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                    }
                    var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, soa.soaCurrency, model.ContractCurrency);
                    // Chia đều cho số lượng service có trong SOA
                    decimal? _obhUnpaid = 0;
                    var paymentsObh = accountingPaymentRepo.Get(x => x.RefId == soa.soaId.ToString());
                    _obhUnpaid = paymentsObh.Sum(s => s.Balance);// Unpaid sẽ sum theo Balance
                    _obhUnpaid = (_obhUnpaid * _exchangeRate) / qtyService;
                    obhUnpaidAmount += _obhUnpaid;
                }
                #endregion --- Documentation ---
            }
            return obhUnpaidAmount;
        }

        private decimal? CalculatorOver1To15Day(AccAccountReceivableModel model)
        {
            decimal? over1To15Day = 0;
            //Lấy ra VAT Invoice có payment status # Paid & Status = Updated Invoice & Overdue days: từ 1 -15 ngày
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE
                                                           && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                           && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED
                                                           && x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 16 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 0);
            var unpaidAmountVatInvoice = UnpaidAmountVatInvoice(model, acctMngts);

            //Lấy ra SOA có payment status # paid & Overdue days: từ 1 -15 ngày
            var soas = soaRepo.Get(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                     && x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 16 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 0);
            var obhUnpaidAmountSoa = ObhUnpaidAmountSoa(model, soas);

            over1To15Day = unpaidAmountVatInvoice + obhUnpaidAmountSoa;
            return over1To15Day;
        }

        private decimal? CalculatorOver16To30Day(AccAccountReceivableModel model)
        {
            decimal? over16To30Day = 0;
            //Lấy ra VAT Invoice có payment status # Paid & Status = Updated Invoice & Overdue days: từ 16 -30 ngày
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE
                                                           && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                           && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED
                                                           && x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 31 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 15);
            var unpaidAmountVatInvoice = UnpaidAmountVatInvoice(model, acctMngts);

            //Lấy ra SOA có payment status # paid & Overdue days: từ 16 -30 ngày
            var soas = soaRepo.Get(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                     && x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 31 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 15);
            var obhUnpaidAmountSoa = ObhUnpaidAmountSoa(model, soas);

            over16To30Day = unpaidAmountVatInvoice + obhUnpaidAmountSoa;
            return over16To30Day;
        }

        private decimal? CalculatorOver30Day(AccAccountReceivableModel model)
        {
            decimal? over30Day = 0;
            //Lấy ra VAT Invoice có payment status # Paid & Status = Updated Invoice & Overdue days: trên 30 ngày
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE
                                                           && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                           && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED
                                                           && x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 30);
            var unpaidAmountVatInvoice = UnpaidAmountVatInvoice(model, acctMngts);

            //Lấy ra SOA có payment status # paid & Overdue days: trên 30 ngày
            var soas = soaRepo.Get(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                     && x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 30);
            var obhUnpaidAmountSoa = ObhUnpaidAmountSoa(model, soas);

            over30Day = unpaidAmountVatInvoice + obhUnpaidAmountSoa;
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

                model.AcRef = partner.ParentId ?? partner.Id;

                var contractPartner = contractPartnerRepo.Get(x => x.Active == true
                                                                && x.PartnerId == model.PartnerId
                                                                && x.OfficeId == model.Office.ToString()
                                                                && x.SaleService.Contains(model.Service)).FirstOrDefault();

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
                model.DebitAmount = (_sellingNoVat + _billingAmount + _obhAmount + _advanceAmount) - _creditAmount;

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

        public HandleState UpdateReceivable(AccAccountReceivableModel model)
        {
            try
            {
                var partner = partnerRepo.Get(x => x.Id == model.PartnerId).FirstOrDefault();
                if (partner == null) return new HandleState("Not found partner");

                model.AcRef = partner.ParentId ?? partner.Id;

                var contractPartner = contractPartnerRepo.Get(x => x.Active == true
                                                                && x.PartnerId == model.PartnerId
                                                                && x.OfficeId == model.Office.ToString()
                                                                && x.SaleService.Contains(model.Service)).FirstOrDefault();

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
                model.DatetimeModified = DateTime.Now;
                model.UserModified = currentUser.UserID;

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
                model.DebitAmount = (_sellingNoVat + _billingAmount + _obhAmount + _advanceAmount) - _creditAmount;

                AccAccountReceivable receivable = mapper.Map<AccAccountReceivable>(model);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Update(receivable, x => x.Id == receivable.Id, false);
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

        public HandleState InsertOrUpdateReceivable(ObjectReceivableModel model)
        {
            AccAccountReceivableModel receivableModel = new AccAccountReceivableModel()
            {
                PartnerId = model.PartnerId,
                Office = model.Office,
                Service = model.Service
            };

            var receivable = Get(x => x.PartnerId == receivableModel.PartnerId && x.Office == receivableModel.Office && x.Service == receivableModel.Service).FirstOrDefault();
            HandleState hs;
            var message = string.Empty;
            if (receivable == null)
            {
                hs = AddReceivable(receivableModel);
            }
            else
            {
                hs = UpdateReceivable(receivable);
                receivableModel = receivable;
            }
            return hs;
        }

        public HandleState CalculatorReceivable(CalculatorReceivableModel model)
        {
            HandleState hs = new HandleState();
            if (model != null && model.ObjectReceivable.Count() > 0)
            {
                // PartnerId, Office, Service # Empty And Null
                var objReceivalble = model.ObjectReceivable.Where(x => !string.IsNullOrEmpty(x.PartnerId)
                                                                  && (x.Office != null && x.Office != Guid.Empty)
                                                                  && !string.IsNullOrEmpty(x.Service))
                                                                  .GroupBy(g => new { g.PartnerId, g.Office, g.Service })
                                                                  .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Office = s.Key.Office, Service = s.Key.Service }).ToList();
                foreach (var obj in objReceivalble)
                {
                    if (!string.IsNullOrEmpty(obj.PartnerId) && obj.Office != null && obj.Office != Guid.Empty && !string.IsNullOrEmpty(obj.Service))
                    {
                        hs = InsertOrUpdateReceivable(obj);
                    }
                }

                // PartnerId, Office, Service is Empty Or Null
                var surchargeIds = model.ObjectReceivable.Where(x => (x.SurchargeId != null && x.SurchargeId != Guid.Empty)
                                                                  && string.IsNullOrEmpty(x.PartnerId)
                                                                  && (x.Office == null || x.Office == Guid.Empty)
                                                                  && string.IsNullOrEmpty(x.Service)).Select(s => s.SurchargeId).Distinct().ToList();
                if (surchargeIds.Count() > 0)
                {
                    var objectReceivables = GetObjectReceivableBySurchargeId(surchargeIds);
                    if (objectReceivables.Count() > 0)
                    {
                        foreach (var objectReceivable in objectReceivables)
                        {
                            hs = InsertOrUpdateReceivable(objectReceivable);
                        }
                    }
                }
            }
            return hs;
        }
        #endregion --- CRUD ---

        private List<ObjectReceivableModel> GetObjectReceivableBySurchargeId(List<Guid?> surchargeIds)
        {
            var surcharges = surchargeRepo.Get(x => surchargeIds.Contains(x.Id));

            #region --- OPERATION ---
            var operations = opsRepo.Get();
            var objOpsPO = from surcharge in surcharges
                           join operation in operations on surcharge.Hblid equals operation.Hblid
                           where !string.IsNullOrEmpty(surcharge.PaymentObjectId)
                           select new ObjectReceivableModel { PartnerId = surcharge.PaymentObjectId, Office = operation.OfficeId, Service = "CL" };
            var objOpsPR = from surcharge in surcharges
                           join operation in operations on surcharge.Hblid equals operation.Hblid
                           where !string.IsNullOrEmpty(surcharge.PayerId)
                           select new ObjectReceivableModel { PartnerId = surcharge.PayerId, Office = operation.OfficeId, Service = "CL" };
            var objOps = objOpsPO.Union(objOpsPR);
            #endregion --- OPERATION ---

            #region --- DOCUMENTATION ---
            var transDetails = transactionDetailRepo.Get();
            var transactions = transactionRepo.Get();
            var objDocPO = from surcharge in surcharges
                           join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                           join trans in transactions on transDetail.JobId equals trans.Id
                           where !string.IsNullOrEmpty(surcharge.PaymentObjectId)
                           select new ObjectReceivableModel { PartnerId = surcharge.PaymentObjectId, Office = transDetail.OfficeId, Service = trans.TransactionType };

            var objDocPR = from surcharge in surcharges
                           join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                           join trans in transactions on transDetail.JobId equals trans.Id
                           where !string.IsNullOrEmpty(surcharge.PayerId)
                           select new ObjectReceivableModel { PartnerId = surcharge.PayerId, Office = transDetail.OfficeId, Service = trans.TransactionType };
            var objDoc = objDocPO.Union(objDocPR);
            #endregion --- DOCUMENTATION ---

            var ObjectReceivable = objOps.Union(objDoc);
            var data = ObjectReceivable.Distinct().ToList();
            return data;
        }

        #region --- LIST & PAGING ---
        private IQueryable<AccountReceivableResult> GetARHasContract(IQueryable<AccAccountReceivable> acctReceivables, IQueryable<CatContract> partnerContracts, IQueryable<CatPartner> partners)
        {
            var selectQuery = from contract in partnerContracts
                              join acctReceivable in acctReceivables on contract.PartnerId equals acctReceivable.AcRef into acctReceivables2
                              from acctReceivable in acctReceivables2.DefaultIfEmpty()
                              where contract.SaleService.Contains(acctReceivable.Service) && contract.OfficeId == acctReceivable.Office.ToString()
                              select new { acctReceivable, contract };
            if (selectQuery == null || !selectQuery.Any()) return null;
            var groupByContract = selectQuery.GroupBy(g => new { g.contract.Id })
                .Select(s => new AccountReceivableResult
                {
                    AgreementId = s.Key.Id,
                    PartnerId = s.First().acctReceivable != null ? s.First().acctReceivable.AcRef : null,
                    PartnerCode = string.Empty, //Get data bên dưới
                    PartnerNameEn = string.Empty, //Get data bên dưới
                    PartnerNameLocal = string.Empty, //Get data bên dưới
                    PartnerNameAbbr = string.Empty, //Get data bên dưới
                    TaxCode = string.Empty, //Get data bên dưới
                    PartnerStatus = string.Empty, //Get data bên dưới
                    AgreementNo = s.First().contract.ContractNo,
                    AgreementType = s.First().contract.ContractType,
                    AgreementStatus = s.First().contract.Active == true ? AccountingConstants.STATUS_ACTIVE : AccountingConstants.STATUS_INACTIVE,
                    AgreementSalesmanId = s.First().contract.SaleManId,
                    AgreementCurrency = s.First().contract.CurrencyId,
                    OfficeId = s.First().contract.OfficeId, //Office of Argeement 
                    ArServiceCode = s.First().acctReceivable.Service,
                    ArServiceName = string.Empty, //Get data bên dưới
                    EffectiveDate = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? s.First().contract.TrialEffectDate : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? s.First().contract.EffectiveDate : null),
                    ExpriedDate = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? s.First().contract.TrialExpiredDate : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? s.First().contract.ExpiredDate : null),
                    ExpriedDay = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? (int?)((s.First().contract.TrialExpiredDate ?? DateTime.Today) - DateTime.Today).TotalDays : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? (int?)((s.First().contract.ExpiredDate ?? DateTime.Today) - DateTime.Today).TotalDays : null),
                    CreditLimited = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? s.First().contract.TrialCreditLimited : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEED || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? s.First().contract.CreditLimit : null),
                    CreditTerm = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? s.First().contract.TrialCreditDays : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? s.First().contract.PaymentTerm : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEED ? (int?)30 : null)),
                    CreditRateLimit = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEED || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? (s.First().contract.CreditLimitRate == null || s.First().contract.CreditLimitRate == 0 ? 120 : s.First().contract.CreditLimitRate) : null,
                    SaleDebitAmount = s.Select(se => se.contract).GroupBy(g => new { g.SaleManId }).Select(sel => sel.Select(sele => sele.CreditAmount).Sum()).Sum(),
                    SaleDebitRate = null, //Tính toán bên dưới
                    DebitAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.DebitAmount : null).Sum(),
                    ObhAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.ObhAmount : null).Sum(),
                    DebitRate = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? ((s.Select(se => se.acctReceivable != null ? se.acctReceivable.DebitAmount : null).Sum() + s.First().contract.CustomerAdvanceAmount) / (s.First().contract.TrialCreditLimited != 0 ? s.First().contract.TrialCreditLimited : null)) * 100 : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL ? ((s.Select(se => se.acctReceivable != null ? se.acctReceivable.DebitAmount : null).Sum() + s.First().contract.CustomerAdvanceAmount) / (s.First().contract.CreditLimit != 0 ? s.First().contract.CreditLimit : null)) * 100 : null),
                    CusAdvance = s.First().contract.CustomerAdvanceAmount,
                    BillingAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.BillingAmount : null).Sum(),
                    BillingUnpaid = s.Select(se => se.acctReceivable != null ? se.acctReceivable.BillingUnpaid : null).Sum(),
                    PaidAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.PaidAmount : null).Sum(),
                    CreditAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.CreditAmount : null).Sum(),
                    Over1To15Day = s.Select(se => se.acctReceivable != null ? se.acctReceivable.Over1To15Day : null).Sum(),
                    Over16To30Day = s.Select(se => se.acctReceivable != null ? se.acctReceivable.Over16To30Day : null).Sum(),
                    Over30Day = s.Select(se => se.acctReceivable != null ? se.acctReceivable.Over30Day : null).Sum(),
                    ArCurrency = s.First().acctReceivable != null ? s.First().acctReceivable.ContractCurrency : null,
                });

            var data = from contract in groupByContract
                       join partner in partners on contract.PartnerId equals partner.Id
                       select new AccountReceivableResult
                       {
                           AgreementId = contract.AgreementId,
                           PartnerId = contract.PartnerId,
                           PartnerCode = partner.AccountNo,
                           PartnerNameEn = partner.PartnerNameEn,
                           PartnerNameLocal = partner.PartnerNameVn,
                           PartnerNameAbbr = partner.ShortName,
                           TaxCode = partner.TaxCode,
                           PartnerStatus = partner.Active == true ? AccountingConstants.STATUS_ACTIVE : AccountingConstants.STATUS_INACTIVE,
                           AgreementNo = contract.AgreementNo,
                           AgreementType = contract.AgreementType,
                           AgreementStatus = contract.AgreementStatus,
                           AgreementSalesmanId = contract.AgreementSalesmanId,
                           AgreementCurrency = contract.AgreementCurrency,
                           OfficeId = contract.OfficeId,
                           ArServiceCode = contract.ArServiceCode,
                           ArServiceName = CustomData.Services.Where(w => w.Value == contract.ArServiceCode).Select(se => se.DisplayName).FirstOrDefault(),
                           EffectiveDate = contract.EffectiveDate,
                           ExpriedDate = contract.ExpriedDate,
                           ExpriedDay = contract.ExpriedDay,
                           CreditLimited = contract.CreditLimited,
                           CreditTerm = contract.CreditTerm,
                           CreditRateLimit = contract.CreditRateLimit,
                           SaleDebitAmount = contract.SaleDebitAmount,
                           SaleDebitRate = (contract.SaleDebitAmount / contract.CreditLimited) * 100,
                           DebitAmount = contract.DebitAmount,
                           ObhAmount = contract.ObhAmount,
                           DebitRate = contract.DebitRate,
                           CusAdvance = contract.CusAdvance,
                           BillingAmount = contract.BillingAmount,
                           BillingUnpaid = contract.BillingUnpaid,
                           PaidAmount = contract.PaidAmount,
                           CreditAmount = contract.CreditAmount,
                           Over1To15Day = contract.Over1To15Day,
                           Over16To30Day = contract.Over16To30Day,
                           Over30Day = contract.Over16To30Day,
                           ArCurrency = contract.ArCurrency
                       };
            return data;
        }

        private IQueryable<AccountReceivableResult> GetARNoContract(IQueryable<AccAccountReceivable> acctReceivables, IQueryable<CatContract> partnerContracts, IQueryable<CatPartner> partners)
        {
            var selectQuery = from acctReceivable in acctReceivables
                              join partnerContract in partnerContracts on acctReceivable.AcRef equals partnerContract.PartnerId into partnerContract2
                              from partnerContract in partnerContract2.DefaultIfEmpty()
                              where acctReceivable.AcRef != partnerContract.PartnerId
                              select acctReceivable;
            if (selectQuery == null || !selectQuery.Any()) return null;
            var groupByPartner = selectQuery.GroupBy(g => new { g.AcRef })
                .Select(s => new AccountReceivableResult
                {
                    PartnerId = s.Key.AcRef,
                    OfficeId = s.First().Office.ToString(), //Office of AR
                    ArServiceCode = s.Select(se => se.Service).FirstOrDefault(),
                    ArServiceName = string.Empty, //Get data bên dưới
                    DebitAmount = s.Select(se => se.DebitAmount).Sum(),
                    ObhAmount = s.Select(se => se.ObhAmount).Sum(),
                    BillingAmount = s.Select(se => se.BillingAmount).Sum(),
                    BillingUnpaid = s.Select(se => se.BillingUnpaid).Sum(),
                    PaidAmount = s.Select(se => se.PaidAmount).Sum(),
                    CreditAmount = s.Select(se => se.CreditAmount).Sum(),
                    Over1To15Day = s.Select(se => se.Over1To15Day).Sum(),
                    Over16To30Day = s.Select(se => se.Over16To30Day).Sum(),
                    Over30Day = s.Select(se => se.Over30Day).Sum(),
                    ArCurrency = s.First() != null ? s.First().ContractCurrency : null,
                });

            var data = from ar in groupByPartner
                       join partner in partners on ar.PartnerId equals partner.Id
                       select new AccountReceivableResult
                       {
                           PartnerId = ar.PartnerId,
                           PartnerCode = partner.AccountNo,
                           PartnerNameEn = partner.PartnerNameEn,
                           PartnerNameLocal = partner.PartnerNameVn,
                           PartnerNameAbbr = partner.ShortName,
                           TaxCode = partner.TaxCode,
                           PartnerStatus = partner.Active == true ? AccountingConstants.STATUS_ACTIVE : AccountingConstants.STATUS_INACTIVE,
                           OfficeId = ar.OfficeId,
                           ArServiceCode = ar.ArServiceCode,
                           ArServiceName = CustomData.Services.Where(w => w.Value == ar.ArServiceCode).Select(se => se.DisplayName).FirstOrDefault(),
                           DebitAmount = ar.DebitAmount,
                           ObhAmount = ar.ObhAmount,
                           BillingAmount = ar.BillingAmount,
                           BillingUnpaid = ar.BillingUnpaid,
                           PaidAmount = ar.PaidAmount,
                           CreditAmount = ar.CreditAmount,
                           Over1To15Day = ar.Over1To15Day,
                           Over16To30Day = ar.Over16To30Day,
                           Over30Day = ar.Over16To30Day,
                           ArCurrency = ar.ArCurrency
                       };
            return data;
        }

        private Expression<Func<AccAccountReceivable, bool>> ExpressionAcctReceivableQuery(AccountReceivableCriteria criteria)
        {
            Expression<Func<AccAccountReceivable, bool>> query = q => true;
            if (criteria != null && !string.IsNullOrEmpty(criteria.AcRefId))
            {
                query = query.And(x => x.AcRef == criteria.AcRefId);
            }
            return query;
        }

        private Expression<Func<CatContract, bool>> ExpressionContractPartnerQuery(AccountReceivableCriteria criteria)
        {
            Expression<Func<CatContract, bool>> query = q => true;
            if (!string.IsNullOrEmpty(criteria.SalesmanId))
            {
                query = query.And(x => x.SaleManId == criteria.SalesmanId);
            }
            if (criteria.OfficeId != null && criteria.OfficeId != Guid.Empty)
            {
                query = query.And(x => x.OfficeId == criteria.OfficeId.ToString());
            }
            if (!string.IsNullOrEmpty(criteria.AgreementStatus) && criteria.AgreementStatus != "All")
            {
                var _active = criteria.AgreementStatus == AccountingConstants.STATUS_ACTIVE ? true : false;
                query = query.And(x => x.Active == _active);
            }            
            return query;
        }

        private Expression<Func<AccountReceivableResult, bool>> ExpressionAccountReceivableQuery(AccountReceivableCriteria criteria)
        {
            Expression<Func<AccountReceivableResult, bool>> query = q => true;
            if (criteria.OverDueDay != 0)
            {
                if (criteria.OverDueDay == OverDueDayEnum.Over1_15)
                {
                    query = query.And(x => x.Over1To15Day > 0);
                }
                if (criteria.OverDueDay == OverDueDayEnum.Over16_30)
                {
                    query = query.And(x => x.Over16To30Day > 0);
                }
                if (criteria.OverDueDay == OverDueDayEnum.Over30)
                {
                    query = query.And(x => x.Over30Day > 0);
                }
            }
            if (criteria.DebitRateFrom != null && criteria.DebitRateFrom > -1)
            {
                if (criteria.DebitRateTo != null && criteria.DebitRateTo > -1)
                {
                    query = query.And(x => x.DebitRate >= criteria.DebitRateFrom && x.DebitRate <= criteria.DebitRateTo);
                }
                else
                {
                    query = query.And(x => x.DebitRate >= criteria.DebitRateFrom);
                }
            }
            else
            {
                if (criteria.DebitRateTo != null && criteria.DebitRateTo > -1)
                {
                    query = query.And(x => x.DebitRate >= criteria.DebitRateTo);
                }
            }
            if (!string.IsNullOrEmpty(criteria.AgreementExpiredDay) && criteria.AgreementExpiredDay != "All")
            {
                if (criteria.AgreementExpiredDay == "Normal")
                {
                    query = query.And(x => x.ExpriedDay > 30);
                }
                if (criteria.AgreementExpiredDay == "30Day")
                {
                    query = query.And(x => x.ExpriedDay == 30);
                }
                if (criteria.AgreementExpiredDay == "15Day")
                {
                    query = query.And(x => x.ExpriedDay > 0 && x.ExpriedDay < 16);
                }
                if (criteria.AgreementExpiredDay == "Expried")
                {
                    query = query.And(x => x.ExpriedDay < 1);
                }
            }
            return query;
        }

        private bool IsGetDataARNoContract(AccountReceivableCriteria criteria)
        {
            bool isGet = false;
            if (string.IsNullOrEmpty(criteria.SalesmanId)
                && (string.IsNullOrEmpty(criteria.AgreementStatus) || criteria.AgreementStatus == "All")
                && (string.IsNullOrEmpty(criteria.AgreementExpiredDay) || criteria.AgreementExpiredDay == "All"))
            {
                isGet = true;
            }
            return isGet;
        }

        private IQueryable<CatContract> QueryContractPartner(IQueryable<CatContract> partnerContracts, AccountReceivableCriteria criteria)
        {
            IQueryable<CatContract> resutl = null;
            if (partnerContracts != null)
            {
                var queryContractPartner = ExpressionContractPartnerQuery(criteria);
                resutl = partnerContracts.Where(queryContractPartner);
            }
            return resutl;
        }

        private IQueryable<object> GetDataTrialOfficial(AccountReceivableCriteria criteria)
        {
            var queryAcctReceivable = ExpressionAcctReceivableQuery(criteria);
            var acctReceivables = DataContext.Get(queryAcctReceivable);
            var partners = partnerRepo.Get();
            var contracts = contractPartnerRepo.Get(x => x.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL || x.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL);
            var partnerContracts = QueryContractPartner(contracts, criteria);
            var arPartnerContracts = GetARHasContract(acctReceivables, partnerContracts, partners);
            if (arPartnerContracts == null || !arPartnerContracts.Any())
            {
                return null;
            }
            else
            {
                var queryAccountReceivable = ExpressionAccountReceivableQuery(criteria);
                arPartnerContracts = arPartnerContracts.Where(queryAccountReceivable);
            }
            return arPartnerContracts;
        }

        private IQueryable<object> GetDataGuarantee(AccountReceivableCriteria criteria)
        {
            var queryAcctReceivable = ExpressionAcctReceivableQuery(criteria);
            var acctReceivables = DataContext.Get(queryAcctReceivable);
            var partners = partnerRepo.Get();
            var contracts = contractPartnerRepo.Get(x => x.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEED);
            var partnerContracts = QueryContractPartner(contracts, criteria);
            var arPartnerContracts = GetARHasContract(acctReceivables, partnerContracts, partners);
            if (arPartnerContracts == null || !arPartnerContracts.Any())
            {
                return null;
            }
            else
            {
                var queryAccountReceivable = ExpressionAccountReceivableQuery(criteria);
                arPartnerContracts = arPartnerContracts.Where(queryAccountReceivable);
            }
            var groupBySalesman = arPartnerContracts.ToList().GroupBy(g => new { g.AgreementSalesmanId })
                .Select(s => new AccountReceivableGroupSalemanResult
                {
                    SalesmanId = s.Key.AgreementSalesmanId,
                    SalesmanNameEn = string.Empty,
                    SalesmanFullName = string.Empty,
                    TotalCreditLimited = s.Select(se => se.CreditLimited).Sum(),
                    TotalDebitAmount = s.Select(se => se.DebitAmount).Sum(),
                    TotalDebitRate = s.Select(se => se.DebitRate).Sum(),
                    TotalBillingAmount = s.Select(se => se.BillingAmount).Sum(),
                    TotalBillingUnpaid = s.Select(se => se.BillingUnpaid).Sum(),
                    TotalPaidAmount = s.Select(se => se.PaidAmount).Sum(),
                    TotalObhAmount = s.Select(se => se.ObhAmount).Sum(),
                    TotalOver1To15Day = s.Select(se => se.Over1To15Day).Sum(),
                    TotalOver16To30Day = s.Select(se => se.Over16To30Day).Sum(),
                    TotalOver30Day = s.Select(se => se.Over30Day).Sum(),
                    ArPartners = s.ToList()
                });
            var data = from contract in groupBySalesman
                       join user in userRepo.Get() on contract.SalesmanId equals user.Id
                       join employee in employeeRepo.Get() on user.EmployeeId equals employee.Id
                       select new AccountReceivableGroupSalemanResult
                       {
                           SalesmanId = contract.SalesmanId,
                           SalesmanFullName = employee.EmployeeNameVn,
                           SalesmanNameEn = employee.EmployeeNameEn,
                           TotalCreditLimited = contract.TotalCreditLimited,
                           TotalDebitAmount = contract.TotalDebitAmount,
                           TotalDebitRate = contract.TotalDebitRate,
                           TotalBillingAmount = contract.TotalBillingAmount,
                           TotalBillingUnpaid = contract.TotalBillingUnpaid,
                           TotalPaidAmount = contract.TotalPaidAmount,
                           TotalObhAmount = contract.TotalObhAmount,
                           TotalOver1To15Day = contract.TotalOver1To15Day,
                           TotalOver16To30Day = contract.TotalOver16To30Day,
                           TotalOver30Day = contract.TotalOver30Day,
                           ArPartners = contract.ArPartners
                       };
            return data.AsQueryable();
        }

        private IQueryable<object> GetDataOther(AccountReceivableCriteria criteria)
        {
            var queryAcctReceivable = ExpressionAcctReceivableQuery(criteria);
            var acctReceivables = DataContext.Get(queryAcctReceivable);
            var partners = partnerRepo.Get();
            var partnerContractsAll = contractPartnerRepo.Get();

            var contractsCash = contractPartnerRepo.Get(x => x.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH);
            var partnerContractsCash = QueryContractPartner(contractsCash, criteria);
            var arPartnerContracts = GetARHasContract(acctReceivables, partnerContractsCash, partners);

            IQueryable<AccountReceivableResult> arPartnerNoContracts = null;
            if (IsGetDataARNoContract(criteria))
            {
                IQueryable<AccAccountReceivable> _acctReceivables = acctReceivables;
                if (criteria.OfficeId != null && criteria.OfficeId != Guid.Empty)
                {
                    _acctReceivables = acctReceivables.Where(x => x.Office == criteria.OfficeId);
                }
                arPartnerNoContracts = GetARNoContract(_acctReceivables, partnerContractsAll, partners);
            }

            IQueryable<AccountReceivableResult> dataResult = null;
            if (arPartnerContracts != null && arPartnerNoContracts != null)
            {
                dataResult = arPartnerContracts.Union(arPartnerNoContracts);
            }
            else if (arPartnerContracts != null && arPartnerNoContracts == null)
            {
                dataResult = arPartnerContracts;
            }
            else if (arPartnerContracts == null && arPartnerNoContracts != null)
            {
                dataResult = arPartnerNoContracts;
            }

            if (dataResult != null)
            {
                var queryAccountReceivable = ExpressionAccountReceivableQuery(criteria);
                dataResult = dataResult.Where(queryAccountReceivable);
            }
            return dataResult;
        }

        public IEnumerable<object> GetDataARByCriteria(AccountReceivableCriteria criteria)
        {
            IEnumerable<object> data = null;
            var arType = DataTypeEx.GetTypeAR(criteria.ArType);
            if (arType == TermData.AR_TrialOrOffical)
            {
                data = GetDataTrialOfficial(criteria);
            }
            else if (arType == TermData.AR_Guarantee)
            {
                data = GetDataGuarantee(criteria);
            }
            else if (arType == TermData.AR_Other)
            {
                data = GetDataOther(criteria);
            }
            return data;
        }

        public IEnumerable<object> Paging(AccountReceivableCriteria criteria, int page, int size, out int rowsCount)
        {
            if (criteria.ArType == 0)
            {
                rowsCount = 0;
                return null;
            }

            IEnumerable<object> data = GetDataARByCriteria(criteria);

            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            //Phân trang
            var _totalItem = data.Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

            return data;
        }

        #endregion --- LIST & PAGING ---

        #region --- DETAIL ---  
        private List<AccountReceivableGroupOfficeResult> GetARGroupOffice(IQueryable<AccountReceivableResult> arPartner)
        {
            //Group by Office of Argeement or Account Receivable
            //Do class AccountReceivableResult tự define nên trước khi group cần convert về toList
            var arGroupOffices = arPartner.ToList().GroupBy(g => new { g.OfficeId })
                .Select(se => new AccountReceivableGroupOfficeResult
                {
                    OfficeId = Guid.Parse(se.Key.OfficeId),
                    OfficeName = string.Empty,
                    TotalDebitAmount = se.Select(sel => sel.DebitAmount).Sum(),
                    TotalBillingAmount = se.Select(sel => sel.BillingAmount).Sum(),
                    TotalBillingUnpaid = se.Select(sel => sel.BillingAmount).Sum(),
                    TotalPaidAmount = se.Select(sel => sel.PaidAmount).Sum(),
                    TotalObhAmount = se.Select(sel => sel.ObhAmount).Sum(),
                    TotalOver1To15Day = se.Select(sel => sel.Over1To15Day).Sum(),
                    TotalOver15To30Day = se.Select(sel => sel.Over16To30Day).Sum(),
                    TotalOver30Day = se.Select(sel => sel.Over30Day).Sum(),
                    AccountReceivableGrpServices = se.Select(sel => new AccountReceivableServiceResult
                    {
                        OfficeId = Guid.Parse(sel.OfficeId),
                        ServiceName = sel.ArServiceName,
                        DebitAmount = sel.DebitAmount,
                        BillingAmount = sel.BillingAmount,
                        BillingUnpaid = sel.BillingUnpaid,
                        PaidAmount = sel.PaidAmount,
                        ObhAmount = sel.ObhAmount,
                        Over1To15Day = sel.Over1To15Day,
                        Over16To30Day = sel.Over16To30Day,
                        Over30Day = sel.Over30Day
                    }).ToList()
                });
            var offices = officeRepo.Get();
            var data = from ar in arGroupOffices
                       join office in offices on ar.OfficeId equals office.Id
                       select new AccountReceivableGroupOfficeResult
                       {
                           OfficeId = ar.OfficeId,
                           OfficeName = office.BranchNameEn,
                           TotalDebitAmount = ar.TotalDebitAmount,
                           TotalBillingAmount = ar.TotalBillingAmount,
                           TotalBillingUnpaid = ar.TotalBillingUnpaid,
                           TotalPaidAmount = ar.TotalPaidAmount,
                           TotalObhAmount = ar.TotalObhAmount,
                           TotalOver1To15Day = ar.TotalOver1To15Day,
                           TotalOver15To30Day = ar.TotalOver15To30Day,
                           TotalOver30Day = ar.TotalOver30Day,
                           AccountReceivableGrpServices = ar.AccountReceivableGrpServices
                       };
            return data.ToList();
        }

        public AccountReceivableDetailResult GetDetailAccountReceivableByArgeementId(Guid argeementId)
        {
            if (argeementId == null || argeementId == Guid.Empty) return null;
            var argeement = contractPartnerRepo.Get(x => x.Id == argeementId).FirstOrDefault();
            if (argeement == null) return null;

            var acctReceivables = DataContext.Get();
            var partners = partnerRepo.Get();
            var partnerContracts = contractPartnerRepo.Get(x => x.ContractType == argeement.ContractType);
            var arPartnerContracts = GetARHasContract(acctReceivables, partnerContracts, partners);

            var detail = new AccountReceivableDetailResult();
            var arPartners = arPartnerContracts.Where(x => x.AgreementId == argeementId);
            detail.AccountReceivable = arPartners.FirstOrDefault();
            detail.AccountReceivableGrpOffices = GetARGroupOffice(arPartners);
            return detail;
        }

        public AccountReceivableDetailResult GetDetailAccountReceivableByPartnerId(string partnerId)
        {
            if (string.IsNullOrEmpty(partnerId)) return null;
            var acctReceivables = DataContext.Get();
            var partners = partnerRepo.Get();
            var partnerContractsAll = contractPartnerRepo.Get();
            var arPartnerNoContracts = GetARNoContract(acctReceivables, partnerContractsAll, partners);

            var detail = new AccountReceivableDetailResult();
            var arPartners = arPartnerNoContracts.Where(x => x.PartnerId == partnerId);
            detail.AccountReceivable = arPartners.FirstOrDefault();
            detail.AccountReceivableGrpOffices = GetARGroupOffice(arPartners);
            return detail;
        }
        #endregion --- DETAIL ---
    }
}
