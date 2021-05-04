﻿using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountReceivable;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
        private readonly IContextBase<AcctCdnote> cdNoteRepo;

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
            IContextBase<SysOffice> sysOffice,
            IContextBase<AcctCdnote> acctCdNote) : base(repository, mapper)
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
            cdNoteRepo = acctCdNote;
        }

        #region --- CALCULATOR VALUE ---
        private decimal? SumTotalAmountOfInvoices(IQueryable<AccAccountingManagement> invoices, string contractCurrency)
        {
            decimal? totalAmount = 0;
            var _invoices = invoices.Distinct();
            foreach (var invoice in _invoices)
            {
                if (contractCurrency == AccountingConstants.CURRENCY_LOCAL)
                {
                    totalAmount += invoice.TotalAmountVnd;
                }
                else if (contractCurrency == AccountingConstants.CURRENCY_USD)
                {
                    totalAmount += invoice.TotalAmountUsd;
                }
                else //Ngoại tệ khác
                {
                    decimal _exchangeRateToCurrencyContract = 0;
                    // List tỷ giá theo ngày tạo hóa đơn
                    var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == invoice.DatetimeCreated.Value.Date).ToList();
                    if (currencyExchange.Count == 0)
                    {
                        // Lấy ngày mới nhất
                        DateTime? maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                        currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                    }
                    _exchangeRateToCurrencyContract = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, invoice.Currency, contractCurrency);
                    totalAmount += NumberHelper.RoundNumber(_exchangeRateToCurrencyContract * (invoice.TotalAmount ?? 0), 2);
                }
            }
            return totalAmount;
        }

        public decimal? SumUnpaidAmountOfInvoices(IQueryable<AccAccountingManagement> invoices, string contractCurrency)
        {
            decimal? unpaidAmount = 0;
            var _invoices = invoices.Distinct();
            foreach (var invoice in _invoices)
            {
                //Số lượng Service có trong VAT Invoice
                var qtyService = !string.IsNullOrEmpty(invoice.ServiceType) ? invoice.ServiceType.Split(';').Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                decimal? _unpaidAmount = 0;
                if (contractCurrency == AccountingConstants.CURRENCY_LOCAL)
                {
                    _unpaidAmount = invoice.UnpaidAmountVnd;
                }
                else if (contractCurrency == AccountingConstants.CURRENCY_USD)
                {
                    _unpaidAmount = invoice.UnpaidAmountUsd;
                }
                else //Ngoại tệ khác
                {
                    // List tỷ giá theo ngày tạo hóa đơn
                    var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == invoice.DatetimeCreated.Value.Date).ToList();
                    if (currencyExchange.Count == 0)
                    {
                        //Ngày tạo mới nhất
                        var maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                        currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                    }
                    var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, invoice.Currency, contractCurrency);
                    _unpaidAmount = invoice.UnpaidAmount * _exchangeRate;
                }

                // Chia đều cho số lượng service có trong VAT Invoice
                _unpaidAmount = _unpaidAmount / qtyService;
                unpaidAmount += _unpaidAmount;
            }
            return unpaidAmount;
        }

        public decimal? SumPaidAmountOfInvoices(IQueryable<AccAccountingManagement> invoices, string contractCurrency)
        {
            decimal? paidAmount = 0;
            var _invoices = invoices.Distinct();
            foreach (var invoice in _invoices)
            {
                //Số lượng Service có trong VAT Invoice
                var qtyService = !string.IsNullOrEmpty(invoice.ServiceType) ? invoice.ServiceType.Split(';').Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                decimal? _paidAmount = invoice.PaidAmount;
                if (contractCurrency == AccountingConstants.CURRENCY_LOCAL)
                {
                    _paidAmount = invoice.PaidAmountVnd;
                }
                else if (contractCurrency == AccountingConstants.CURRENCY_USD)
                {
                    _paidAmount = invoice.PaidAmountUsd;
                }
                else //Ngoại tệ khác
                {
                    // List tỷ giá theo ngày tạo hóa đơn
                    var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == invoice.DatetimeCreated.Value.Date).ToList();
                    if (currencyExchange.Count == 0)
                    {
                        //Lấy ngày tạo mới nhất
                        var maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                        currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                    }
                    var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, invoice.Currency, contractCurrency);
                    _paidAmount = NumberHelper.RoundNumber((invoice.PaidAmount * _exchangeRate) ?? 0, 2);
                }

                // Chia đều cho số lượng service có trong VAT Invoice
                _paidAmount = _paidAmount / qtyService;
                paidAmount += _paidAmount;
            }
            return paidAmount;
        }

        private decimal? UnpaidAmountVatInvoice(AccAccountReceivableModel model, IQueryable<AccAccountingManagement> accountingManagements)
        {
            decimal? unpaidAmountVatInvoice = 0;
            //Lấy ra các phí thu (SELLING) đã issue VAT Invoice
            var surcharges = surchargeRepo.Get(x => x.OfficeId == model.Office
                                                 && x.TransactionType == model.Service
                                                 && x.PaymentObjectId == model.PartnerId
                                                 && x.Type == AccountingConstants.TYPE_CHARGE_SELL
                                                 && string.IsNullOrEmpty(x.InvoiceNo)
                                                 && x.AcctManagementId != null);

            var invoices = from acctMngt in accountingManagements
                           join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                           select acctMngt;
            /*if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office && x.CurrentStatus != "Canceled");
                accountants = from acctMngt in accountingManagements
                              join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                              join operation in operations on surcharge.Hblid equals operation.Hblid
                              select acctMngt;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office);
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service && x.CurrentStatus != "Canceled");
                accountants = from acctMngt in accountingManagements 
                              join surcharge in surcharges on  acctMngt.Id equals surcharge.AcctManagementId
                              join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                              join trans in transactions on transDetail.JobId equals trans.Id
                              select acctMngt;
            }*/

            if (invoices == null) return unpaidAmountVatInvoice;
            unpaidAmountVatInvoice = SumUnpaidAmountOfInvoices(invoices, model.ContractCurrency);
            return unpaidAmountVatInvoice;
        }

        private decimal? UnpaidAmountInvoiceTemp(AccAccountReceivableModel model, IQueryable<AccAccountingManagement> accountingManagements)
        {
            decimal? unpaidAmount = 0;
            //Lấy ra các phí thu (OBH - OBH Partner) đã issue VAT Invoice
            var surcharges = surchargeRepo.Get(x => x.OfficeId == model.Office
                                                 && x.TransactionType == model.Service
                                                 && x.PaymentObjectId == model.PartnerId
                                                 && x.Type == AccountingConstants.TYPE_CHARGE_OBH
                                                 && string.IsNullOrEmpty(x.InvoiceNo)
                                                 && x.AcctManagementId != null);

            var invoices = from acctMngt in accountingManagements
                           join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                           select acctMngt;

            //Service là Custom Logistic
            /*if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office && x.CurrentStatus != "Canceled");
                invoices = from surcharge in surcharges
                           join acctMngt in accountingManagements on surcharge.AcctManagementId equals acctMngt.Id
                           join operation in operations on surcharge.Hblid equals operation.Hblid
                           select acctMngt;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office);
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service && x.CurrentStatus != "Canceled");
                invoices = from surcharge in surcharges
                           join acctMngt in accountingManagements on surcharge.AcctManagementId equals acctMngt.Id
                           join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                           join trans in transactions on transDetail.JobId equals trans.Id
                           select acctMngt;
            }*/

            if (invoices == null) return unpaidAmount;
            unpaidAmount = SumUnpaidAmountOfInvoices(invoices, model.ContractCurrency);
            return unpaidAmount;
        }

        /// <summary>
        /// Billing Amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorBillingAmount(AccAccountReceivableModel model)
        {
            decimal? billingAmount = 0;
            //Get VAT Invoice have payment status # Paid
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE 
                                                           && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
            //Get Debit charge (SELLING) đã issue VAT Invoice
            var surcharges = surchargeRepo.Get(x => x.OfficeId == model.Office
                                                 && x.TransactionType == model.Service
                                                 && x.PaymentObjectId == model.PartnerId 
                                                 && x.Type == AccountingConstants.TYPE_CHARGE_SELL 
                                                 && x.AcctManagementId != null);

            var invoices = from surcharge in surcharges
                           join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                           select acctMngt;

            //Service là Custom Logistic
            /*if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office && x.CurrentStatus != "Canceled");
                invoices = from surcharge in surcharges
                           join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                           join operation in operations on surcharge.Hblid equals operation.Hblid
                           select acctMngt;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office);
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service && x.CurrentStatus != "Canceled");
                invoices = from surcharge in surcharges
                           join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                           join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                           join trans in transactions on transDetail.JobId equals trans.Id
                           select acctMngt;

            }*/

            if (invoices == null) return billingAmount;           
            billingAmount = SumTotalAmountOfInvoices(invoices, model.ContractCurrency);
            return billingAmount;
        }

        /// <summary>
        /// Billing Unpaid
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorBillingUnpaid(AccAccountReceivableModel model)
        {
            decimal? billingUnpaid = 0;
            //Get VAT Invoice have type Invoice & payment status # Paid
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE 
                                                           && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
            billingUnpaid = UnpaidAmountVatInvoice(model, acctMngts);
            return billingUnpaid;
        }

        /// <summary>
        /// Paid Amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorPaidAmount(AccAccountReceivableModel model)
        {
            decimal? paidAmount = 0;
            //Get VAT Invoice have payment status is Paid A Part & status is Updated Invoice
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE 
                                                           && x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART 
                                                           && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED);
            //Get Debit charge (SELLING) và đã issue VAT Invoice
            var surcharges = surchargeRepo.Get(x => x.OfficeId == model.Office
                                                 && x.TransactionType == model.Service 
                                                 && x.PaymentObjectId == model.PartnerId 
                                                 && x.Type == AccountingConstants.TYPE_CHARGE_SELL 
                                                 && x.AcctManagementId != null);

            var invoices = from surcharge in surcharges
                           join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                           select acctMngt;
            /*if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office && x.CurrentStatus != "Canceled");
                accountants = from surcharge in surcharges
                              join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                              join operation in operations on surcharge.Hblid equals operation.Hblid
                              select acctMngt;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office);
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service && x.CurrentStatus != "Canceled");
                accountants = from surcharge in surcharges
                              join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                              join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                              join trans in transactions on transDetail.JobId equals trans.Id
                              select acctMngt;
            }*/

            if (invoices == null) return paidAmount;
            paidAmount = SumPaidAmountOfInvoices(invoices, model.ContractCurrency);            
            return paidAmount;
        }

        /// <summary>
        /// OBH Amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorObhAmount(AccAccountReceivableModel model)
        {
            decimal? obhAmount = 0;
            //Get OBH charge by OBH Partner (PaymentObjectId)
            var surcharges = surchargeRepo.Get(x => x.OfficeId == model.Office
                                                 && x.TransactionType == model.Service 
                                                 && x.PaymentObjectId == model.PartnerId 
                                                 && x.Type == AccountingConstants.TYPE_CHARGE_OBH);
            
            var charges = surcharges;
            /*if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office && x.CurrentStatus != "Canceled");
                charges = from surcharge in surcharges
                          join operation in operations on surcharge.Hblid equals operation.Hblid
                          select surcharge;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office);
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service && x.CurrentStatus != "Canceled");
                charges = from surcharge in surcharges
                          join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                          join trans in transactions on transDetail.JobId equals trans.Id
                          select surcharge;
            }*/
            if (charges == null) return obhAmount;

            foreach (var charge in charges)
            {
                obhAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, model.ContractCurrency);
            }
            return obhAmount;
        }

        /// <summary>
        /// OBH Unpaid
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorObhUnpaid(AccAccountReceivableModel model)
        {
            decimal? obhUnpaid = 0;
            //Get VAT Invoice have type invoice temp & payment status # Paid
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE 
                                                           && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
            obhUnpaid = UnpaidAmountInvoiceTemp(model, acctMngts);
            return obhUnpaid;
        }

        /// <summary>
        /// OBH Billing
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorObhBilling(AccAccountReceivableModel model)
        {
            decimal? obhBilling = 0;
            //Get VAT Invoice have type Invoice Temp & payment status # Paid
            var acctMngts = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE 
                                                           && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
            //Get Debit charge (OBH - OBH Partner) và đã issue VAT Invoice temp
            var surcharges = surchargeRepo.Get(x => x.OfficeId == model.Office
                                                 && x.TransactionType == model.Service 
                                                 && x.PaymentObjectId == model.PartnerId 
                                                 && x.Type == AccountingConstants.TYPE_CHARGE_OBH 
                                                 && x.AcctManagementId != null);

            var invoices = from surcharge in surcharges
                           join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                           select acctMngt;

            //Service là Custom Logistic
            /*if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office && x.CurrentStatus != "Canceled");
                invoices = from surcharge in surcharges
                           join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                           join operation in operations on surcharge.Hblid equals operation.Hblid
                           select acctMngt;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office);
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service && x.CurrentStatus != "Canceled");
                invoices = from surcharge in surcharges
                           join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                           join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                           join trans in transactions on transDetail.JobId equals trans.Id
                           select acctMngt;

            }*/

            if (invoices == null) return obhBilling;
            obhBilling = SumTotalAmountOfInvoices(invoices, model.ContractCurrency);
            return obhBilling;
        }

        /// <summary>
        /// Advance Amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorAdvanceAmount(AccAccountReceivableModel model)
        {
            decimal? advanceAmount = 0;
            var surcharges = surchargeRepo.Get(x => x.OfficeId == model.Office
                                                 && x.TransactionType == model.Service 
                                                 && (x.PaymentObjectId == model.PartnerId || x.PayerId == model.PartnerId));
            var charges = surcharges;
            /*if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office && x.CurrentStatus != "Canceled");
                charges = from surcharge in surcharges
                          join operation in operations on surcharge.Hblid equals operation.Hblid
                          select surcharge;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office);
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service && x.CurrentStatus != "Canceled");
                charges = from surcharge in surcharges
                          join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                          join trans in transactions on transDetail.JobId equals trans.Id
                          select surcharge;
            }*/

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
                if (model.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                {
                    advanceAmount += advanceRequest.AmountVnd;
                }
                else if (model.ContractCurrency == AccountingConstants.CURRENCY_USD)
                {
                    advanceAmount += advanceRequest.AmountUsd;
                }
                else //Ngoại tệ khác
                {
                    // List tỷ giá theo ngày tạo hóa đơn
                    var currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == advanceRequest.DatetimeCreated.Value.Date).ToList();
                    if (currencyExchange.Count == 0)
                    {
                        //Ngày tạo mới nhất
                        var maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                        currencyExchange = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                    }
                    var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, advanceRequest.RequestCurrency, model.ContractCurrency);
                    advanceAmount += NumberHelper.RoundNumber(_exchangeRate * (advanceRequest.Amount ?? 0), 2);
                }                
            }
            return advanceAmount;
        }

        /// <summary>
        /// Credit Amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorCreditAmount(AccAccountReceivableModel model)
        {
            decimal? creditAmount = 0;
            //Lấy ra các phí chi (BUYING) chưa có CreditNote/SOA type Credit hoặc có tồn tại CreditNote/SOA type Credit có NetOff = false
            var surcharges = surchargeRepo.Get(x => x.OfficeId == model.Office
                                                 && x.TransactionType == model.Service 
                                                 && x.PaymentObjectId == model.PartnerId 
                                                 && x.Type == AccountingConstants.TYPE_CHARGE_BUY);

            var charges = surcharges;
            //Service là Custom Logistic
            /*if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office && x.CurrentStatus != "Canceled");
                charges = from surcharge in surcharges
                          join operation in operations on surcharge.Hblid equals operation.Hblid
                          select surcharge;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office);
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service && x.CurrentStatus != "Canceled");
                charges = from surcharge in surcharges
                          join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                          join trans in transactions on transDetail.JobId equals trans.Id
                          select surcharge;
            }*/
            if (charges == null) return creditAmount;

            var isExistSOACredit = charges.Any(x => !string.IsNullOrEmpty(x.PaySoano));
            if (isExistSOACredit)
            {
                //SOA Credit & NetOff = false
                var soaCredits = soaRepo.Get(x => x.Type == "Credit" && x.NetOff == false);
                charges = from chg in charges
                          join soa in soaCredits on chg.PaySoano equals soa.Soano
                          select chg;
            }

            var isExistCreditNote = charges.Any(x => !string.IsNullOrEmpty(x.CreditNo));
            if (isExistCreditNote)
            {
                //Credit Note & NetOff = false
                var creditNotes = cdNoteRepo.Get(x => x.Type == "CREDIT" && x.NetOff == false);
                charges = from chg in charges
                          join cn in creditNotes on chg.CreditNo equals cn.Code
                          select chg;
            }

            foreach (var charge in charges)
            {
                creditAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, model.ContractCurrency);
            }
            return creditAmount;
        }

        /// <summary>
        /// Selling Amount No VAT
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorSellingNoVat(AccAccountReceivableModel model)
        {
            decimal? sellingNoVat = 0;
            //Lấy ra các phí thu (SELLING) chưa có Invoice
            var surcharges = surchargeRepo.Get(x => x.OfficeId == model.Office
                                                 && x.TransactionType == model.Service 
                                                 && x.PaymentObjectId == model.PartnerId 
                                                 && x.Type == AccountingConstants.TYPE_CHARGE_SELL 
                                                 && string.IsNullOrEmpty(x.InvoiceNo) 
                                                 && x.AcctManagementId == null);

            var charges = surcharges;
            //Service là Custom Logistic
            /*if (model.Service == "CL")
            {
                var operations = opsRepo.Get(x => x.OfficeId == model.Office && x.CurrentStatus != "Canceled");
                charges = from surcharge in surcharges
                          join operation in operations on surcharge.Hblid equals operation.Hblid
                          select surcharge;
            }
            else
            {
                var transDetails = transactionDetailRepo.Get(x => x.OfficeId == model.Office);
                var transactions = transactionRepo.Get(x => x.TransactionType == model.Service && x.CurrentStatus != "Canceled");
                charges = from surcharge in surcharges
                          join transDetail in transDetails on surcharge.Hblid equals transDetail.Id
                          join trans in transactions on transDetail.JobId equals trans.Id
                          select surcharge;
            }*/
            if (charges == null) return sellingNoVat;

            foreach (var charge in charges)
            {
                sellingNoVat += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, model.ContractCurrency);
            }
            return sellingNoVat;
        }

        /// <summary>
        /// Over 1 To 15 Day
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorOver1To15Day(AccAccountReceivableModel model)
        {
            decimal? over1To15Day = 0;
            //Lấy ra VAT Invoice có (type = Invoice or InvoiceTemp) & payment status # Paid & Status = Updated Invoice & Overdue days: từ 1 -15 ngày
            var acctMngts = accountingManagementRepo.Get(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                           && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED
                                                           && x.PaymentDueDate.HasValue 
                                                           && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 16 
                                                           && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 0);
            //Invoices
            var invoices = acctMngts.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE);
            //Invoices Temp
            var invoiceTemps = acctMngts.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);
            var unpaidAmountInv = UnpaidAmountVatInvoice(model, invoices);
            var unpaidAmountInvTemp = UnpaidAmountInvoiceTemp(model, invoiceTemps);
            over1To15Day = unpaidAmountInv + unpaidAmountInvTemp;
            return over1To15Day;
        }

        /// <summary>
        /// Over 16 To 30 Day
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorOver16To30Day(AccAccountReceivableModel model)
        {
            decimal? over16To30Day = 0;
            //Lấy ra VAT Invoice có (type = Invoice or InvoiceTemp) & payment status # Paid & Status = Updated Invoice & Overdue days: từ 16 -30 ngày
            var acctMngts = accountingManagementRepo.Get(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                           && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED
                                                           && x.PaymentDueDate.HasValue 
                                                           && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 31 
                                                           && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 15);
            //Invoices
            var invoices = acctMngts.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE);
            //Invoices Temp
            var invoiceTemps = acctMngts.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);
            var unpaidAmountInv = UnpaidAmountVatInvoice(model, invoices);
            var unpaidAmountInvTemp = UnpaidAmountInvoiceTemp(model, invoiceTemps);
            over16To30Day = unpaidAmountInv + unpaidAmountInvTemp;
            return over16To30Day;
        }

        /// <summary>
        /// Over 30 Day
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal? CalculatorOver30Day(AccAccountReceivableModel model)
        {
            decimal? over30Day = 0;
            //Lấy ra VAT Invoice có (type = Invoice or InvoiceTemp) & payment status # Paid & Status = Updated Invoice & Overdue days: trên 30 ngày
            var acctMngts = accountingManagementRepo.Get(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                           && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED
                                                           && x.PaymentDueDate.HasValue 
                                                           && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 30);
            //Invoices
            var invoices = acctMngts.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE);
            //Invoices Temp
            var invoiceTemps = acctMngts.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);
            var unpaidAmountInv = UnpaidAmountVatInvoice(model, invoices);
            var unpaidAmountInvTemp = UnpaidAmountInvoiceTemp(model, invoiceTemps);
            over30Day = unpaidAmountInv + unpaidAmountInvTemp;
            return over30Day;
        }

        #endregion --- CALCULATOR VALUE ---

        #region --- CRUD ---
        private CatContract CalculatorAgreement(CatContract agreement)
        {
            //Get DS Công nợ có cùng PartnerId, Saleman, Service, Office của Agreement
            var receivables = DataContext.Get(x => x.PartnerId == agreement.PartnerId
                                                && x.SaleMan == agreement.SaleManId
                                                && agreement.SaleService.Contains(x.Service)
                                                && agreement.OfficeId.Contains(x.Office.ToString()));

            agreement.BillingAmount = receivables.Sum(su => su.BillingAmount + su.ObhBilling); //Sum BillingAmount + BillingOBH
            //Credit Amount ~ Debit Amount
            agreement.CreditAmount = receivables.Sum(su => su.DebitAmount); //Sum DebitAmount
            agreement.UnpaidAmount = receivables.Sum(su => su.BillingUnpaid + su.ObhAmount); //Sum BillingUnpaid + BillingOBH
            agreement.PaidAmount = receivables.Sum(su => su.PaidAmount); //Sum PaidAmount

            decimal? _creditRate = agreement.CreditRate;
            if (agreement.ContractType == "Trial")
            {
                _creditRate = ((agreement.CreditAmount + agreement.CustomerAdvanceAmount) / agreement.TrialCreditLimited) * 100; //((DebitAmount + CusAdv)/TrialCreditLimit)*100
            }
            if (agreement.ContractType == "Official")
            {
                _creditRate = ((agreement.CreditAmount + agreement.CustomerAdvanceAmount) / agreement.CreditLimit) * 100; //((DebitAmount + CusAdv)/CreditLimit)*100
            }
            if (agreement.ContractType == "Parent Contract")
            {
                //???
            }
            agreement.CreditRate = _creditRate;
            return agreement;
        }

        private HandleState UpdateAgreementPartner(string partnerId)
        {
            var hs = new HandleState();
            var partner = partnerRepo.Get(x => x.Id == partnerId).FirstOrDefault();
            if (partner == null) return hs;
            //Agreement của partner
            var contractPartner = contractPartnerRepo.Get(x => x.Active == true
                                                            && x.PartnerId == partnerId).FirstOrDefault();
            if (contractPartner != null)
            {
                var agreementPartner = CalculatorAgreement(contractPartner);
                hs = contractPartnerRepo.Update(agreementPartner, x => x.Id == agreementPartner.Id);
            }
            else
            {
                //Agreement của AcRef của partner
                var contractParent = contractPartnerRepo.Get(x => x.Active == true 
                                                               && x.PartnerId == partner.ParentId).FirstOrDefault();
                if (contractParent != null)
                {
                    var agreementParent = CalculatorAgreement(contractParent);
                    hs = contractPartnerRepo.Update(agreementParent, x => x.Id == agreementParent.Id);
                }
            }
            return hs;
        }

        public async Task<HandleState> AddReceivable(AccAccountReceivableModel model)
        {
            try
            {
                model.Id = Guid.NewGuid();
                var partner = partnerRepo.Get(x => x.Id == model.PartnerId).FirstOrDefault();
                if (partner == null) return new HandleState("Not found partner");

                //Không tính công nợ cho đối tượng Internal
                if (partner.PartnerMode == "Internal") return new HandleState();

                model.AcRef = partner.ParentId ?? partner.Id;

                var contractPartner = contractPartnerRepo.Get(x => x.Active == true
                                                                && x.PartnerId == model.PartnerId
                                                                && x.OfficeId.Contains(model.Office.ToString())
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
                    model.OfficeId = model.Office;
                    model.CompanyId = contractPartner.CompanyId;
                }
                model.DatetimeCreated = model.DatetimeModified = DateTime.Now;

                var _billingAmount = CalculatorBillingAmount(model);
                var _billingUnpaid = CalculatorBillingUnpaid(model);
                var _paidAmount = CalculatorPaidAmount(model);
                var _obhUnpaid = CalculatorObhUnpaid(model);
                var _obhAmount = CalculatorObhAmount(model) + _obhUnpaid; //Cộng thêm OBH Unpaid                
                var _obhBilling = CalculatorObhBilling(model);
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
                model.ObhBilling = _obhBilling;
                model.AdvanceAmount = _advanceAmount;
                model.CreditAmount = _creditAmount;
                model.SellingNoVat = _sellingNoVat;
                model.Over1To15Day = _over1To15Day;
                model.Over16To30Day = _over16To30Day;
                model.Over30Day = _over30Day;
                model.DebitAmount = _sellingNoVat + _billingUnpaid + _obhAmount + _advanceAmount;

                AccAccountReceivable receivable = mapper.Map<AccAccountReceivable>(model);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = await DataContext.AddAsync(receivable, false);
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
                        new LogHelper("eFMS_Receivable_Add_LOG", ex.ToString());
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

        public async Task<HandleState> UpdateReceivable(AccAccountReceivableModel model)
        {
            try
            {
                var partner = partnerRepo.Get(x => x.Id == model.PartnerId).FirstOrDefault();
                if (partner == null) return new HandleState("Not found partner");

                model.AcRef = partner.ParentId ?? partner.Id;

                var contractPartner = contractPartnerRepo.Get(x => x.Active == true
                                                                && x.PartnerId == model.PartnerId
                                                                && x.OfficeId.Contains(model.Office.ToString())
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
                    model.OfficeId = model.OfficeId;
                    model.CompanyId = contractPartner.CompanyId;
                }
                model.DatetimeModified = DateTime.Now;

                var _billingAmount = CalculatorBillingAmount(model);
                var _billingUnpaid = CalculatorBillingUnpaid(model);
                var _paidAmount = CalculatorPaidAmount(model);
                var _obhUnpaid = CalculatorObhUnpaid(model);
                var _obhAmount = CalculatorObhAmount(model) + _obhUnpaid; //Cộng thêm OBH Unpaid
                var _obhBilling = CalculatorObhBilling(model);
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
                model.ObhBilling = _obhBilling;
                model.AdvanceAmount = _advanceAmount;
                model.CreditAmount = _creditAmount;
                model.SellingNoVat = _sellingNoVat;
                model.Over1To15Day = _over1To15Day;
                model.Over16To30Day = _over16To30Day;
                model.Over30Day = _over30Day;
                model.DebitAmount = _sellingNoVat + _billingUnpaid + _obhAmount + _advanceAmount;

                AccAccountReceivable receivable = mapper.Map<AccAccountReceivable>(model);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = await DataContext.UpdateAsync(receivable, x => x.Id == receivable.Id, false);
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
                        new LogHelper("eFMS_Receivable_Update_LOG", ex.ToString());
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

        public async Task<HandleState> InsertOrUpdateReceivable(ObjectReceivableModel model)
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
                hs = await AddReceivable(receivableModel);
            }
            else
            {
                hs = await UpdateReceivable(receivable);
                receivableModel = receivable;
            }
            return hs;
        }

        public async Task<HandleState> CalculatorReceivable(CalculatorReceivableModel model)
        {
            HandleState hs = new HandleState();
            if (model != null && model.ObjectReceivable.Count() > 0)
            {
                // PartnerId, Office, Service # Empty And # Null
                var objReceivalble = model.ObjectReceivable.Where(x => !string.IsNullOrEmpty(x.PartnerId)
                                                                  && (x.Office != null && x.Office != Guid.Empty)
                                                                  && !string.IsNullOrEmpty(x.Service))
                                                                  .GroupBy(g => new { g.PartnerId, g.Office, g.Service })
                                                                  .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Office = s.Key.Office, Service = s.Key.Service }).ToList();
                foreach (var obj in objReceivalble)
                {
                    if (!string.IsNullOrEmpty(obj.PartnerId) && obj.Office != null && obj.Office != Guid.Empty && !string.IsNullOrEmpty(obj.Service))
                    {
                        hs = await InsertOrUpdateReceivable(obj);
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
                            hs = await InsertOrUpdateReceivable(objectReceivable);
                        }
                    }
                }
            }
            return hs;
        }

        public async Task<HandleState> CalculatorReceivableNotAuthorize(CalculatorReceivableNotAuthorizeModel model)
        {
            currentUser.UserID = model.UserID;
            currentUser.GroupId = model.GroupId;
            currentUser.DepartmentId = model.DepartmentId;
            currentUser.OfficeID = model.OfficeID;
            currentUser.CompanyID = model.CompanyID;
            currentUser.Action = model.Action;
            var hs = await CalculatorReceivable(model);
            return hs;
        }

        #endregion --- CRUD ---

        /// <summary>
        /// Get Object Receivable By list Surcharges Id
        /// </summary>
        /// <param name="surchargeIds">List Id of surcharge</param>
        /// <returns></returns>
        public List<ObjectReceivableModel> GetObjectReceivableBySurchargeId(List<Guid?> surchargeIds)
        {
            var surcharges = surchargeRepo.Get(x => surchargeIds.Any(a => a == x.Id));
            var data = GetObjectReceivableBySurcharges(surcharges);
            return data;
        }

        /// <summary>
        /// Get Object Receivable By Surcharges
        /// </summary>
        /// <param name="surcharges"></param>
        /// <returns></returns>
        public List<ObjectReceivableModel> GetObjectReceivableBySurcharges(IQueryable<CsShipmentSurcharge> surcharges)
        {
            var objPO = from surcharge in surcharges
                        where !string.IsNullOrEmpty(surcharge.PaymentObjectId)
                        select new ObjectReceivableModel { PartnerId = surcharge.PaymentObjectId, Office = surcharge.OfficeId, Service = surcharge.TransactionType };
            var objPR = from surcharge in surcharges
                        where !string.IsNullOrEmpty(surcharge.PayerId)
                        select new ObjectReceivableModel { PartnerId = surcharge.PayerId, Office = surcharge.OfficeId, Service = surcharge.TransactionType };
            var objMerge = objPO.Union(objPR).ToList();
            var objectReceivables = objMerge.GroupBy(g => new { Service = g.Service, PartnerId =  g.PartnerId, Office = g.Office })
                .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Service = s.Key.Service, Office = s.Key.Office });
            return objectReceivables.ToList();
        }

        #region --- LIST & PAGING ---
        private IQueryable<AccountReceivableResult> GetARHasContract(IQueryable<AccAccountReceivable> acctReceivables, IQueryable<CatContract> partnerContracts, IQueryable<CatPartner> partners)
        {
            var users = userRepo.Get();
            var employees = employeeRepo.Get();

            var selectQuery = from contract in partnerContracts
                              join acctReceivable in acctReceivables on contract.PartnerId equals acctReceivable.AcRef into acctReceivables2
                              from acctReceivable in acctReceivables2.DefaultIfEmpty()
                              where contract.SaleService.Contains(acctReceivable.Service) && contract.OfficeId.Contains(acctReceivable.Office.ToString())
                              select new { acctReceivable, contract };
            if (selectQuery == null || !selectQuery.Any()) return null;

            var contractsGuaranteed = selectQuery.Where(x => x.contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEED)
                .Select(s => new AccountReceivableResult
                {
                    AgreementSalesmanId = s.contract.SaleManId,
                    DebitAmount = s.acctReceivable.DebitAmount
                }).ToList();
            // Group by Contract ID, Service AR, Office AR
            var groupByContract = selectQuery.GroupBy(g => new { g.contract.Id, g.acctReceivable.Service, g.acctReceivable.Office })
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
                    OfficeId = s.Key.Office.ToString(), //Office AR chứa trong Office Argeement
                    ArServiceCode = s.Key.Service,
                    ArServiceName = string.Empty, //Get data bên dưới
                    EffectiveDate = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? s.First().contract.TrialEffectDate : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? s.First().contract.EffectiveDate : null),
                    ExpriedDate = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? s.First().contract.TrialExpiredDate : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? s.First().contract.ExpiredDate : null),
                    ExpriedDay = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? (int?)((s.First().contract.TrialExpiredDate ?? DateTime.Today) - DateTime.Today).TotalDays : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? (int?)((s.First().contract.ExpiredDate ?? DateTime.Today) - DateTime.Today).TotalDays : 0),
                    CreditLimited = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? (s.First().contract.TrialCreditLimited ?? 0) : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? (s.First().contract.CreditLimit ?? 0) : 0),
                    CreditTerm = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? (s.First().contract.TrialCreditDays ?? 0) : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? (s.First().contract.PaymentTerm ?? 0) : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEED ? (int?)30 : 0)),
                    CreditRateLimit = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEED || s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH ? (s.First().contract.CreditLimitRate == null || s.First().contract.CreditLimitRate == 0 ? 120 : s.First().contract.CreditLimitRate) : 0,
                    SaleCreditLimited = null, //Get data bên dưới
                    SaleDebitAmount = contractsGuaranteed.Where(w => w.AgreementSalesmanId == s.First().contract.SaleManId).Sum(su => su.DebitAmount),
                    SaleDebitRate = null, //Tính toán bên dưới
                    DebitAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.DebitAmount : 0).Sum(),
                    ObhAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.ObhAmount : 0).Sum(),
                    DebitRate = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ? ((s.Select(se => se.acctReceivable != null ? se.acctReceivable.DebitAmount : null).Sum() + (s.First().contract.CustomerAdvanceAmount ?? 0)) / (s.First().contract.TrialCreditLimited != 0 && s.First().contract.TrialCreditLimited != null ? s.First().contract.TrialCreditLimited : 1)) * 100 : (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL ? ((s.Select(se => se.acctReceivable != null ? se.acctReceivable.DebitAmount : null).Sum() + (s.First().contract.CustomerAdvanceAmount ?? 0)) / (s.First().contract.CreditLimit != 0 && s.First().contract.CreditLimit != null ? s.First().contract.CreditLimit : 1)) * 100 : null),
                    CusAdvance = s.First().contract.CustomerAdvanceAmount ?? 0,
                    BillingAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.BillingAmount : 0).Sum(),
                    BillingUnpaid = s.Select(se => se.acctReceivable != null ? se.acctReceivable.BillingUnpaid : 0).Sum(),
                    PaidAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.PaidAmount : 0).Sum(),
                    CreditAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.CreditAmount : 0).Sum(),
                    Over1To15Day = s.Select(se => se.acctReceivable != null ? se.acctReceivable.Over1To15Day : 0).Sum(),
                    Over16To30Day = s.Select(se => se.acctReceivable != null ? se.acctReceivable.Over16To30Day : 0).Sum(),
                    Over30Day = s.Select(se => se.acctReceivable != null ? se.acctReceivable.Over30Day : 0).Sum(),
                    ArCurrency = s.First().acctReceivable != null ? s.First().acctReceivable.ContractCurrency : null,
                });

            var data = from contract in groupByContract
                       join partner in partners on contract.PartnerId equals partner.Id
                       join user in users on contract.AgreementSalesmanId equals user.Id into users2
                       from user in users2.DefaultIfEmpty()
                       join employee in employees on user.EmployeeId equals employee.Id into employees2
                       from employee in employees2.DefaultIfEmpty()
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
                           AgreementSalesmanName = employee.EmployeeNameVn,
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
                           SaleCreditLimited = user.CreditLimit,
                           SaleDebitAmount = contract.SaleDebitAmount,
                           SaleDebitRate = (contract.SaleDebitAmount / (user.CreditLimit != null && user.CreditLimit != 0 ? user.CreditLimit : 1)) * 100,
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
                           Over30Day = contract.Over30Day,
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
                    OfficeId = s.First() != null ? s.First().Office.ToString() : null, //Office of AR
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
                    ArCurrency = s.Select(se => se.ContractCurrency).FirstOrDefault(),
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
                           Over30Day = ar.Over30Day,
                           ArCurrency = ar.ArCurrency
                       };
            return data;
        }

        private Expression<Func<AccAccountReceivable, bool>> ExpressionAcctReceivableQuery(AccountReceivableCriteria criteria)
        {
            //Chỉ lấy những AR có Office # null
            Expression<Func<AccAccountReceivable, bool>> query = q => q.Office != null;
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
                query = query.And(x => x.OfficeId.Contains(criteria.OfficeId.ToString()));
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
            IQueryable<CatContract> result = null;
            if (partnerContracts != null)
            {
                var queryContractPartner = ExpressionContractPartnerQuery(criteria);
                result = partnerContracts.Where(queryContractPartner);
            }
            return result;
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
                arPartnerContracts = GetArPartnerContractGroupByAgreementId(arPartnerContracts);
                var queryAccountReceivable = ExpressionAccountReceivableQuery(criteria);
                arPartnerContracts = arPartnerContracts.Where(queryAccountReceivable).Where(x => x.DebitAmount > 0);
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
                    ArPartners = s.ToList().GroupBy(g => new { g.AgreementId }).Select(se => new AccountReceivableResult
                    {
                        AgreementId = se.Key.AgreementId,
                        AgreementNo = se.First().AgreementNo,
                        AgreementStatus = se.First().AgreementStatus,
                        PartnerId = se.First().PartnerId,
                        PartnerCode = se.First().PartnerCode,
                        PartnerNameAbbr = se.First().PartnerNameAbbr,
                        PartnerNameEn = se.First().PartnerNameEn,
                        PartnerNameLocal = se.First().PartnerNameLocal,
                        DebitAmount = se.Sum(sum => sum.DebitAmount),
                        BillingAmount = se.Sum(sum => sum.BillingAmount),
                        BillingUnpaid = se.Sum(sum => sum.BillingUnpaid),
                        PaidAmount = se.Sum(sum => sum.PaidAmount),
                        ObhAmount = se.Sum(sum => sum.ObhAmount),
                        Over1To15Day = se.Sum(sum => sum.Over1To15Day),
                        Over16To30Day = se.Sum(sum => sum.Over16To30Day),
                        Over30Day = se.Sum(sum => sum.Over30Day)
                    }).ToList()
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
            arPartnerContracts = GetArPartnerContractGroupByAgreementId(arPartnerContracts);

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

        private IQueryable<AccountReceivableResult> GetArPartnerContractGroupByAgreementId(IQueryable<AccountReceivableResult> arPartnerContracts)
        {
            if (arPartnerContracts == null) return null;
            var groupbyAgreementId = arPartnerContracts.ToList()
                    .GroupBy(g => new { g.AgreementId })
                    .Select(s => new AccountReceivableResult
                    {
                        AgreementId = s.Key.AgreementId,
                        PartnerId = s.First().PartnerId,
                        PartnerCode = s.First().PartnerCode,
                        PartnerNameEn = s.First().PartnerNameEn,
                        PartnerNameLocal = s.First().PartnerNameLocal,
                        PartnerNameAbbr = s.First().PartnerNameAbbr,
                        TaxCode = s.First().TaxCode,
                        PartnerStatus = s.First().PartnerStatus,
                        AgreementNo = s.First().AgreementNo,
                        AgreementType = s.First().AgreementType,
                        AgreementStatus = s.First().AgreementStatus,
                        AgreementSalesmanId = s.First().AgreementSalesmanId,
                        AgreementSalesmanName = s.First().AgreementSalesmanName,
                        AgreementCurrency = s.First().AgreementCurrency,
                        OfficeId = s.First().OfficeId,
                        ArServiceCode = s.First().ArServiceCode,
                        ArServiceName = s.First().ArServiceName,
                        EffectiveDate = s.First().EffectiveDate,
                        ExpriedDate = s.First().ExpriedDate,
                        ExpriedDay = s.First().ExpriedDay,
                        CreditLimited = s.First().CreditLimited,
                        CreditTerm = s.First().CreditTerm,
                        CreditRateLimit = s.First().CreditRateLimit,
                        SaleCreditLimited = s.First().SaleCreditLimited,
                        SaleDebitAmount = s.First().SaleDebitAmount,
                        SaleDebitRate = s.First().SaleDebitRate,
                        DebitAmount = s.Sum(sum => sum.DebitAmount),
                        ObhAmount = s.Sum(sum => sum.ObhAmount),
                        DebitRate = s.Sum(sum => sum.DebitRate),
                        CusAdvance = s.First().CusAdvance,
                        BillingAmount = s.Sum(sum => sum.BillingAmount),
                        BillingUnpaid = s.Sum(sum => sum.BillingUnpaid),
                        PaidAmount = s.Sum(sum => sum.PaidAmount),
                        CreditAmount = s.Sum(sum => sum.CreditAmount),
                        Over1To15Day = s.Sum(sum => sum.Over1To15Day),
                        Over16To30Day = s.Sum(sum => sum.Over16To30Day),
                        Over30Day = s.Sum(sum => sum.Over30Day),
                        ArCurrency = s.First().ArCurrency
                    }).AsQueryable();
            return groupbyAgreementId;
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
                    TotalBillingUnpaid = se.Select(sel => sel.BillingUnpaid).Sum(),
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

            var acctReceivables = DataContext.Get(x => x.Office != null);
            var partners = partnerRepo.Get();
            var partnerContracts = contractPartnerRepo.Get(x => x.ContractType == argeement.ContractType);
            var arPartnerContracts = GetARHasContract(acctReceivables, partnerContracts, partners);

            var detail = new AccountReceivableDetailResult();
            var arPartners = arPartnerContracts.Where(x => x.AgreementId == argeementId);
            detail.AccountReceivable = arPartners.ToList().GroupBy(g => new { g.AgreementId }).Select(s => new AccountReceivableResult
            {
                AgreementId = s.Key.AgreementId,
                PartnerId = s.Select(se => se.PartnerId).FirstOrDefault(),
                PartnerCode = s.Select(se => se.PartnerCode).FirstOrDefault(),
                PartnerNameEn = s.Select(se => se.PartnerNameEn).FirstOrDefault(),
                PartnerNameLocal = s.Select(se => se.PartnerNameLocal).FirstOrDefault(),
                PartnerNameAbbr = s.Select(se => se.PartnerNameAbbr).FirstOrDefault(),
                TaxCode = s.Select(se => se.TaxCode).FirstOrDefault(),
                PartnerStatus = s.Select(se => se.PartnerStatus).FirstOrDefault(),
                AgreementNo = s.Select(se => se.AgreementNo).FirstOrDefault(),
                AgreementType = s.Select(se => se.AgreementType).FirstOrDefault(),
                AgreementStatus = s.Select(se => se.AgreementStatus).FirstOrDefault(),
                AgreementSalesmanId = s.Select(se => se.AgreementSalesmanId).FirstOrDefault(),
                AgreementSalesmanName = s.Select(se => se.AgreementSalesmanName).FirstOrDefault(),
                AgreementCurrency = s.Select(se => se.AgreementCurrency).FirstOrDefault(),
                OfficeId = s.Select(se => se.OfficeId).FirstOrDefault(),
                ArServiceCode = s.Select(se => se.ArServiceCode).FirstOrDefault(),
                ArServiceName = s.Select(se => se.ArServiceName).FirstOrDefault(),
                EffectiveDate = s.Select(se => se.EffectiveDate).FirstOrDefault(),
                ExpriedDate = s.Select(se => se.ExpriedDate).FirstOrDefault(),
                ExpriedDay = s.Select(se => se.ExpriedDay).FirstOrDefault(),
                CreditLimited = s.Select(se => se.CreditLimited).FirstOrDefault(),
                CreditTerm = s.Select(se => se.CreditTerm).FirstOrDefault(),
                CreditRateLimit = s.Select(se => se.CreditRateLimit).FirstOrDefault(),
                SaleCreditLimited = s.Select(se => se.SaleCreditLimited).FirstOrDefault(),
                SaleDebitAmount = s.Select(se => se.SaleDebitAmount).FirstOrDefault(),
                SaleDebitRate = s.Select(se => se.SaleDebitRate).FirstOrDefault(),
                DebitAmount = s.Sum(sum => sum.DebitAmount),
                ObhAmount = s.Sum(sum => sum.ObhAmount),
                DebitRate = s.Sum(sum => sum.DebitRate),
                CusAdvance = s.Select(se => se.CusAdvance).FirstOrDefault(),
                BillingAmount = s.Sum(sum => sum.BillingAmount),
                BillingUnpaid = s.Sum(sum => sum.BillingUnpaid),
                PaidAmount = s.Sum(sum => sum.PaidAmount),
                CreditAmount = s.Sum(sum => sum.CreditAmount),
                Over1To15Day = s.Sum(sum => sum.Over1To15Day),
                Over16To30Day = s.Sum(sum => sum.Over16To30Day),
                Over30Day = s.Sum(sum => sum.Over30Day),
                ArCurrency = s.Select(se => se.ArCurrency).FirstOrDefault()
            }).FirstOrDefault();
            detail.AccountReceivableGrpOffices = GetARGroupOffice(arPartners);
            return detail;
        }

        public AccountReceivableDetailResult GetDetailAccountReceivableByPartnerId(string partnerId)
        {
            if (string.IsNullOrEmpty(partnerId)) return null;
            var acctReceivables = DataContext.Get(x => x.Office != null);
            var partners = partnerRepo.Get();
            var partnerContractsAll = contractPartnerRepo.Get();
            var arPartnerNoContracts = GetARNoContract(acctReceivables, partnerContractsAll, partners);

            var detail = new AccountReceivableDetailResult();
            var arPartners = arPartnerNoContracts.Where(x => x.PartnerId == partnerId);
            detail.AccountReceivable = arPartners.ToList().GroupBy(g => new { g.PartnerId }).Select(s => new AccountReceivableResult
            {
                PartnerId = s.Key.PartnerId,
                PartnerCode = s.Select(se => se.PartnerCode).FirstOrDefault(),
                PartnerNameEn = s.Select(se => se.PartnerNameEn).FirstOrDefault(),
                PartnerNameLocal = s.Select(se => se.PartnerNameLocal).FirstOrDefault(),
                PartnerNameAbbr = s.Select(se => se.PartnerNameAbbr).FirstOrDefault(),
                TaxCode = s.Select(se => se.TaxCode).FirstOrDefault(),
                PartnerStatus = s.Select(se => se.PartnerStatus).FirstOrDefault(),
                OfficeId = s.Select(se => se.OfficeId).FirstOrDefault(),
                ArServiceCode = s.Select(se => se.ArServiceCode).FirstOrDefault(),
                ArServiceName = s.Select(se => se.ArServiceName).FirstOrDefault(),
                DebitAmount = s.Sum(sum => sum.DebitAmount),
                ObhAmount = s.Sum(sum => sum.ObhAmount),
                BillingAmount = s.Sum(sum => sum.BillingAmount),
                BillingUnpaid = s.Sum(sum => sum.BillingUnpaid),
                PaidAmount = s.Sum(sum => sum.PaidAmount),
                CreditAmount = s.Sum(sum => sum.CreditAmount),
                Over1To15Day = s.Sum(sum => sum.Over1To15Day),
                Over16To30Day = s.Sum(sum => sum.Over16To30Day),
                Over30Day = s.Sum(sum => sum.Over30Day),
                ArCurrency = s.Select(se => se.ArCurrency).FirstOrDefault()
            }).FirstOrDefault();
            detail.AccountReceivableGrpOffices = GetARGroupOffice(arPartners);
            return detail;
        }
        #endregion --- DETAIL ---
    }
}
