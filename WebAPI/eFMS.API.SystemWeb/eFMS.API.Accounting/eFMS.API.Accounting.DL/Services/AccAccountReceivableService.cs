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
        private readonly IContextBase<AcctAdvanceRequest> advanceRequestRepo;

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
            IContextBase<AcctAdvanceRequest> advanceRequest) : base(repository, mapper)
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
            if (model.ObjectReceivable.Count() > 0)
            {
                // PartnerId, Office, Service # Empty And Null
                var objReceivalble = model.ObjectReceivable.Where(x => !string.IsNullOrEmpty(x.PartnerId)
                                                                  && (x.Office != null && x.Office != Guid.Empty)
                                                                  && !string.IsNullOrEmpty(x.Service))
                                                                  .GroupBy(g => new { g.PartnerId, g.Office, g.Service })
                                                                  .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Office = s.Key.Office, Service = s.Key.Service } ).ToList();
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
        #endregion --- LIST & PAGING ---

        #region --- DETAIL ---
        #endregion --- DETAIL ---
    }
}
