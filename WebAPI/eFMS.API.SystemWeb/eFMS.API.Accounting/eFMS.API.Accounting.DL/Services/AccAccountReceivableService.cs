using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountReceivable;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.ViewModel;
using eFMS.API.Accounting.Service.Contexts;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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
            if (invoices == null) return totalAmount;
            foreach (var invoice in invoices)
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
                    var currencyExchange = currencyExchangeRepo.Get(x => x.Active == true && x.DatetimeCreated.Value.Date == invoice.DatetimeCreated.Value.Date).ToList();
                    if (currencyExchange.Count == 0)
                    {
                        // Lấy ngày mới nhất
                        DateTime? maxDateCreated = currencyExchangeRepo.Get(x => x.Active == true).Max(s => s.DatetimeCreated);
                        currencyExchange = currencyExchangeRepo.Get(x => x.Active == true && x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
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
            if (invoices == null) return unpaidAmount;
            foreach (var invoice in invoices)
            {
                //Số lượng Service có trong VAT Invoice
                var qtyService = !string.IsNullOrEmpty(invoice.ServiceType) ? invoice.ServiceType.Split(';').Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                qtyService = (qtyService == 0) ? 1 : qtyService;
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
                    var currencyExchange = currencyExchangeRepo.Get(x => x.Active == true && x.DatetimeCreated.Value.Date == invoice.DatetimeCreated.Value.Date).ToList();
                    if (currencyExchange.Count == 0)
                    {
                        //Ngày tạo mới nhất
                        var maxDateCreated = currencyExchangeRepo.Get(x => x.Active == true).Max(s => s.DatetimeCreated);
                        currencyExchange = currencyExchangeRepo.Get(x => x.Active == true && x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
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
            if (invoices == null) return paidAmount;
            foreach (var invoice in invoices)
            {
                //Số lượng Service có trong VAT Invoice
                var qtyService = !string.IsNullOrEmpty(invoice.ServiceType) ? invoice.ServiceType.Split(';').Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                qtyService = (qtyService == 0) ? 1 : qtyService;
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
                    var currencyExchange = currencyExchangeRepo.Get(x => x.Active == true && x.DatetimeCreated.Value.Date == invoice.DatetimeCreated.Value.Date).ToList();
                    if (currencyExchange.Count == 0)
                    {
                        //Lấy ngày tạo mới nhất
                        var maxDateCreated = currencyExchangeRepo.Get(x => x.Active == true).Max(s => s.DatetimeCreated);
                        currencyExchange = currencyExchangeRepo.Get(x => x.Active == true && x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
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

        /// <summary>
        /// Billing Amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorBillingAmount(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges, IQueryable<AccAccountingManagement> accAccountings)
        {
            //Get Debit charge (SELLING) đã issue VAT Invoice
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL
                                             && x.AcctManagementId != null);
            if (surcharges.Count() == 0)
            {
                return models;
            }
            //Get VAT Invoice have payment status # Paid
            var acctMngts = accAccountings.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE
                                                   && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);

            var invoices = from surcharge in surcharges
                           join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                           select new ReceivableInvoice
                           {
                               Office = surcharge.OfficeId,
                               PartnerId = surcharge.PaymentObjectId,
                               Service = surcharge.TransactionType,
                               Invoice = acctMngt
                           };

            //Group by Office, PartnerId, Service
            var grpInvoices = invoices.ToList()
                .GroupBy(g => new { g.Office, g.PartnerId, g.Service }).Select(s => new ReceivableInvoices
                {
                    Office = s.Key.Office,
                    PartnerId = s.Key.PartnerId,
                    Service = s.Key.Service,
                    Invoices = s.Select(se => se.Invoice).ToList()
                });

            models.ForEach(fe =>
            {
                IQueryable<AccAccountingManagement> invGrps = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service)
                        .Select(se => se.Invoices.AsQueryable())
                        .FirstOrDefault();
                if (invGrps == null)
                {
                    return;
                }
                // Group By InvoiceID
                IQueryable<AccAccountingManagement> invoiceQ = invGrps.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                {
                    Id = s.Key.Id,
                    DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                    TotalAmount = s.FirstOrDefault().TotalAmount,
                    TotalAmountVnd = s.FirstOrDefault().TotalAmountVnd,
                    TotalAmountUsd = s.FirstOrDefault().TotalAmountUsd,

                    PaidAmount = s.FirstOrDefault().PaidAmount,
                    PaidAmountUsd = s.FirstOrDefault().PaidAmountUsd,
                    PaidAmountVnd = s.FirstOrDefault().PaidAmountVnd,

                    UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                    UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                    UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                    ServiceType = s.FirstOrDefault().ServiceType

                });
                fe.BillingAmount = SumTotalAmountOfInvoices(invoiceQ, fe.ContractCurrency);
                fe.BillingUnpaid = SumUnpaidAmountOfInvoices(invoiceQ, fe.ContractCurrency);
            });

            return models;
        }

        /// <summary>
        /// Billing Unpaid
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorBillingUnpaid(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges, IQueryable<AccAccountingManagement> accAccountings)
        {
            //Lấy ra các phí thu (SELLING) đã issue VAT Invoice
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL
                                             && x.AcctManagementId != null);
            if (surcharges.Count() == 0)
            {
                return models;
            }

            //Get VAT Invoice have type Invoice & payment status # Paid
            var acctMngts = accAccountings.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE
                                                   && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);

            var invoices = from acctMngt in acctMngts
                           join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                           select new ReceivableInvoice
                           {
                               Office = surcharge.OfficeId,
                               PartnerId = surcharge.PaymentObjectId,
                               Service = surcharge.TransactionType,
                               Invoice = acctMngt
                           };

            //Group by Office, PartnerId, Service
            var grpInvoices = invoices.ToList()
                .GroupBy(g => new { Office = g.Office, PartnerId = g.PartnerId, Service = g.Service }).Select(s => new ReceivableInvoices
                {
                    Office = s.Key.Office,
                    PartnerId = s.Key.PartnerId,
                    Service = s.Key.Service,
                    Invoices = s.Select(se => se.Invoice).ToList()
                });

            models.ForEach(fe =>
            {
                var invs = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service).Select(se => se.Invoices.AsQueryable()).FirstOrDefault();
                fe.BillingUnpaid = SumUnpaidAmountOfInvoices(invs, fe.ContractCurrency);
            });

            return models;
        }

        /// <summary>
        /// Paid Amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorPaidAmount(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges, IQueryable<AccAccountingManagement> accAccountings)
        {
            //Get Debit charge (SELLING) và đã issue VAT Invoice
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL
                                             && x.AcctManagementId != null);
            if (surcharges.Count() == 0)
            {
                return models;
            }

            //Get VAT Invoice have payment status is Paid A Part & status is Updated Invoice
            var acctMngts = accAccountings.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE
                                                   && x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART
                                                   && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED);

            var invoices = from surcharge in surcharges
                           join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                           select new ReceivableInvoice
                           {
                               Office = surcharge.OfficeId,
                               PartnerId = surcharge.PaymentObjectId,
                               Service = surcharge.TransactionType,
                               Invoice = acctMngt
                           };
            if (invoices.Count() == 0)
            {
                return models;
            }
            //Group by Office, PartnerId, Service
            var grpInvoices = invoices.ToList()
                .GroupBy(g => new { g.Office, g.PartnerId, g.Service }).Select(s => new ReceivableInvoices
                {
                    Office = s.Key.Office,
                    PartnerId = s.Key.PartnerId,
                    Service = s.Key.Service,
                    Invoices = s.Select(se => se.Invoice).ToList()
                });

            models.ForEach(fe =>
            {
                IQueryable<AccAccountingManagement> invGrps = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service)
                        .Select(se => se.Invoices.AsQueryable())
                        .FirstOrDefault();
                if (invGrps == null)
                {
                    return;
                }
                // Group By InvoiceID
                IQueryable<AccAccountingManagement> invoiceQ = invGrps.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                {
                    Id = s.Key.Id,
                    DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                    TotalAmount = s.FirstOrDefault().TotalAmount,
                    TotalAmountVnd = s.FirstOrDefault().TotalAmountVnd,
                    TotalAmountUsd = s.FirstOrDefault().TotalAmountUsd,

                    PaidAmount = s.FirstOrDefault().PaidAmount,
                    PaidAmountUsd = s.FirstOrDefault().PaidAmountUsd,
                    PaidAmountVnd = s.FirstOrDefault().PaidAmountVnd,

                    UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                    UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                    UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                    ServiceType = s.FirstOrDefault().ServiceType

                });
                fe.PaidAmount = SumPaidAmountOfInvoices(invoiceQ, fe.ContractCurrency);
            });

            return models;
        }

        /// <summary>
        /// OBH Amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorObhAmount(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges)
        {
            //Get OBH charge by OBH Partner (PaymentObjectId)
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH && string.IsNullOrEmpty(x.ReferenceNo));

            models.ForEach(fe =>
            {
                var _charges = surcharges.Where(x => x.OfficeId == fe.Office && x.PaymentObjectId == fe.PartnerId && x.TransactionType == fe.Service);
                decimal? obhAmount = 0;
                foreach (var charge in _charges)
                {
                    obhAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, fe.ContractCurrency);
                }
                fe.ObhAmount = obhAmount + fe.ObhUnpaid; //Cộng thêm OBH Unpaid thuộc Receivable (ObhUnpaid cần phải được tính toán trước)
            });

            return models;
        }

        /// <summary>
        /// OBH Unpaid
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorObhUnpaid(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges, IQueryable<AccAccountingManagement> accAccountings)
        {
            //Lấy ra các phí thu (OBH - OBH Partner) đã issue VAT Invoice
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH
                                             && x.AcctManagementId != null);
            if (surcharges.Count() == 0)
            {
                return models;
            }
            //Get VAT Invoice have type invoice temp & payment status # Paid
            var acctMngts = accAccountings.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE
                                                   && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);

            var invoices = from acctMngt in acctMngts
                           join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                           select new ReceivableInvoice
                           {
                               Office = surcharge.OfficeId,
                               PartnerId = surcharge.PaymentObjectId,
                               Service = surcharge.TransactionType,
                               Invoice = acctMngt
                           };

            //Group by Office, PartnerId, Service
            var grpInvoices = invoices.ToList()
                .GroupBy(g => new { g.Office, g.PartnerId, g.Service }).Select(s => new ReceivableInvoices
                {
                    Office = s.Key.Office,
                    PartnerId = s.Key.PartnerId,
                    Service = s.Key.Service,
                    Invoices = s.Select(se => se.Invoice).ToList()
                });

            models.ForEach(fe =>
            {
                IQueryable<AccAccountingManagement> invGrps = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service)
                        .Select(se => se.Invoices.AsQueryable())
                        .FirstOrDefault();
                if (invGrps == null)
                {
                    return;
                }
                // Group By InvoiceID
                IQueryable<AccAccountingManagement> invoiceQ = invGrps.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                {
                    Id = s.Key.Id,
                    DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                    TotalAmount = s.FirstOrDefault().TotalAmount,
                    TotalAmountVnd = s.FirstOrDefault().TotalAmountVnd,
                    TotalAmountUsd = s.FirstOrDefault().TotalAmountUsd,

                    PaidAmount = s.FirstOrDefault().PaidAmount,
                    PaidAmountUsd = s.FirstOrDefault().PaidAmountUsd,
                    PaidAmountVnd = s.FirstOrDefault().PaidAmountVnd,

                    UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                    UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                    UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                    ServiceType = s.FirstOrDefault().ServiceType

                });
                fe.ObhUnpaid = SumUnpaidAmountOfInvoices(invoiceQ, fe.ContractCurrency);
            });
            return models;
        }

        /// <summary>
        /// OBH Paid
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorObhPaid(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges, IQueryable<AccAccountingManagement> accAccountings)
        {
            //Lấy ra các phí thu (OBH - OBH Partner) đã issue VAT Invoice
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH
                                             && x.AcctManagementId != null);

            if (surcharges.Count() == 0)
            {
                return models;
            }
            //Get VAT Invoice have type invoice temp & payment status = Paid A Part & status = Updated Invoice
            var acctMngts = accAccountings.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE
                                                   && x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART
                                                   && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED);

            var invoices = from acctMngt in acctMngts
                           join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                           select new ReceivableInvoice
                           {
                               Office = surcharge.OfficeId,
                               PartnerId = surcharge.PaymentObjectId,
                               Service = surcharge.TransactionType,
                               Invoice = acctMngt
                           };

            //Group by Office, PartnerId, Service
            var grpInvoices = invoices.ToList()
                .GroupBy(g => new { Office = g.Office, PartnerId = g.PartnerId, Service = g.Service }).Select(s => new ReceivableInvoices
                {
                    Office = s.Key.Office,
                    PartnerId = s.Key.PartnerId,
                    Service = s.Key.Service,
                    Invoices = s.Select(se => se.Invoice).ToList()
                });

            models.ForEach(fe =>
            {
                IQueryable<AccAccountingManagement> invGrps = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service)
                         .Select(se => se.Invoices.AsQueryable())
                         .FirstOrDefault();
                if (invGrps == null)
                {
                    return;
                }
                // Group By InvoiceID
                IQueryable<AccAccountingManagement> invoiceQ = invGrps.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                {
                    Id = s.Key.Id,
                    DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                    TotalAmount = s.FirstOrDefault().TotalAmount,
                    TotalAmountVnd = s.FirstOrDefault().TotalAmountVnd,
                    TotalAmountUsd = s.FirstOrDefault().TotalAmountUsd,

                    PaidAmount = s.FirstOrDefault().PaidAmount,
                    PaidAmountUsd = s.FirstOrDefault().PaidAmountUsd,
                    PaidAmountVnd = s.FirstOrDefault().PaidAmountVnd,

                    UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                    UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                    UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                    ServiceType = s.FirstOrDefault().ServiceType

                });
                fe.ObhPaid = SumPaidAmountOfInvoices(invoiceQ, fe.ContractCurrency);
            });

            return models;
        }

        /// <summary>
        /// OBH Billing
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorObhBilling(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges, IQueryable<AccAccountingManagement> accAccountings)
        {
            //Get Debit charge (OBH - OBH Partner) và đã issue VAT Invoice temp
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH
                                             && x.AcctManagementId != null);
            if (surcharges.Count() == 0)
            {
                return models;
            }
            //Get VAT Invoice have type Invoice Temp & payment status # Paid
            var acctMngts = accAccountings.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE
                                                   && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);

            var invoices = from surcharge in surcharges
                           join acctMngt in acctMngts on surcharge.AcctManagementId equals acctMngt.Id
                           select new ReceivableInvoice
                           {
                               Office = surcharge.OfficeId,
                               PartnerId = surcharge.PaymentObjectId,
                               Service = surcharge.TransactionType,
                               Invoice = acctMngt
                           };

            //Group by Office, PartnerId, Service
            var grpInvoices = invoices.ToList()
                .GroupBy(g => new { g.Office, g.PartnerId, g.Service }).Select(s => new ReceivableInvoices
                {
                    Office = s.Key.Office,
                    PartnerId = s.Key.PartnerId,
                    Service = s.Key.Service,
                    Invoices = s.Select(se => se.Invoice).ToList()
                });

            models.ForEach(fe =>
            {
                IQueryable<AccAccountingManagement> invGrps = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service)
                        .Select(se => se.Invoices.AsQueryable())
                        .FirstOrDefault();
                if (invGrps == null)
                {
                    return;
                }
                // Group By InvoiceID
                IQueryable<AccAccountingManagement> invoiceQ = invGrps.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                {
                    Id = s.Key.Id,
                    DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                    TotalAmount = s.FirstOrDefault().TotalAmount,
                    TotalAmountVnd = s.FirstOrDefault().TotalAmountVnd,
                    TotalAmountUsd = s.FirstOrDefault().TotalAmountUsd,

                    PaidAmount = s.FirstOrDefault().PaidAmount,
                    PaidAmountUsd = s.FirstOrDefault().PaidAmountUsd,
                    PaidAmountVnd = s.FirstOrDefault().PaidAmountVnd,

                    UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                    UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                    UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                    ServiceType = s.FirstOrDefault().ServiceType

                });
                fe.ObhBilling = SumTotalAmountOfInvoices(invoiceQ, fe.ContractCurrency);
            });

            return models;
        }

        /// <summary>
        /// Advance Amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorAdvanceAmount(List<AccAccountReceivableModel> models)
        {
            var surcharges = surchargeRepo.Get(x => models.Any(a => a.Office == x.OfficeId && a.Service == x.TransactionType && (a.PartnerId == x.PaymentObjectId || a.PartnerId == x.PayerId)));
            models.ForEach(fe =>
            {
                var charges = surcharges.Where(x => fe.Office == x.OfficeId && fe.Service == x.TransactionType && (fe.PartnerId == x.PaymentObjectId || fe.PartnerId == x.PayerId));
                decimal? advanceAmount = 0;
                if (charges.Count() > 0)
                {
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
                        if (fe.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                        {
                            advanceAmount += advanceRequest.AmountVnd;
                        }
                        else if (fe.ContractCurrency == AccountingConstants.CURRENCY_USD)
                        {
                            advanceAmount += advanceRequest.AmountUsd;
                        }
                        else //Ngoại tệ khác
                        {
                            // List tỷ giá theo ngày tạo hóa đơn
                            var currencyExchange = currencyExchangeRepo.Get(x => x.Active == true && x.DatetimeCreated.Value.Date == advanceRequest.DatetimeCreated.Value.Date).ToList();
                            if (currencyExchange.Count == 0)
                            {
                                //Ngày tạo mới nhất
                                var maxDateCreated = currencyExchangeRepo.Get(x => x.Active == true).Max(s => s.DatetimeCreated);
                                currencyExchange = currencyExchangeRepo.Get(x => x.Active == true && x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                            }
                            var _exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, advanceRequest.RequestCurrency, fe.ContractCurrency);
                            advanceAmount += NumberHelper.RoundNumber(_exchangeRate * (advanceRequest.Amount ?? 0), 2);
                        }
                    }
                }
                fe.AdvanceAmount = advanceAmount;
            });
            return models;
        }

        /// <summary>
        /// Credit Amount
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorCreditAmount(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges)
        {
            //Lấy ra các phí chi (BUYING) chưa có CreditNote/SOA type Credit hoặc có tồn tại CreditNote/SOA type Credit có NetOff = false
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_BUY);

            models.ForEach(fe =>
            {
                var _charges = surcharges.Where(x => x.OfficeId == fe.Office && x.PaymentObjectId == fe.PartnerId && x.TransactionType == fe.Service);

                var isExistSOACredit = _charges.Any(x => !string.IsNullOrEmpty(x.PaySoano));
                if (isExistSOACredit)
                {
                    //SOA Credit & NetOff = false
                    var soaCredits = soaRepo.Get(x => x.Type == "Credit" && x.NetOff == false);
                    _charges = from chg in _charges
                               join soa in soaCredits on chg.PaySoano equals soa.Soano
                               select chg;
                }

                var isExistCreditNote = _charges.Any(x => !string.IsNullOrEmpty(x.CreditNo));
                if (isExistCreditNote)
                {
                    //Credit Note & NetOff = false
                    var creditNotes = cdNoteRepo.Get(x => x.Type == "CREDIT" && x.NetOff == false);
                    _charges = from chg in _charges
                               join cn in creditNotes on chg.CreditNo equals cn.Code
                               select chg;
                }

                decimal? creditAmount = 0;
                foreach (var charge in _charges)
                {
                    creditAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, fe.ContractCurrency);
                }
                fe.CreditAmount = creditAmount;
            });

            return models;
        }

        /// <summary>
        /// Selling Amount No VAT
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorSellingNoVat(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges)
        {
            //Lấy ra các phí thu (SELLING) chưa có Invoice
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL
                                             && string.IsNullOrEmpty(x.InvoiceNo)
                                             && x.AcctManagementId == null);

            models.ForEach(fe =>
            {
                var _charges = surcharges.Where(x => x.OfficeId == fe.Office && x.PaymentObjectId == fe.PartnerId && x.TransactionType == fe.Service);
                decimal? sellingNoVat = 0;
                foreach (var charge in _charges)
                {
                    sellingNoVat += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, fe.ContractCurrency);
                }
                fe.SellingNoVat = sellingNoVat;
            });

            return models;
        }

        /// <summary>
        /// Over 1 To 15 Day
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorOver1To15Day(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges, IQueryable<AccAccountingManagement> accAccountings)
        {
            //Lấy ra VAT Invoice có (type = Invoice or InvoiceTemp) & payment status # Paid & Status = Updated Invoice & Overdue days: từ 1 -15 ngày
            var acctMngts = accAccountings.Where(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                   && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED
                                                   && x.PaymentDueDate.HasValue
                                                   && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 16
                                                   && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 0);

            //Lấy ra các phí thu (SELLING) or phí thu (OBH - OBH Partner) đã issue VAT Invoice
            var surcharges = charges.Where(x => (x.Type == AccountingConstants.TYPE_CHARGE_SELL || x.Type == AccountingConstants.TYPE_CHARGE_OBH)
                                             && x.AcctManagementId != null);

            var invoices = from acctMngt in acctMngts
                           join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                           select new ReceivableInvoice
                           {
                               Office = surcharge.OfficeId,
                               PartnerId = surcharge.PaymentObjectId,
                               Service = surcharge.TransactionType,
                               Invoice = acctMngt
                           };
            if (invoices.Count() == 0)
            {
                return models;
            }
            //Group by Office, PartnerId, Service
            var grpInvoices = invoices.ToList()
                .GroupBy(g => new { Office = g.Office, PartnerId = g.PartnerId, Service = g.Service }).Select(s => new ReceivableInvoices
                {
                    Office = s.Key.Office,
                    PartnerId = s.Key.PartnerId,
                    Service = s.Key.Service,
                    Invoices = s.Select(se => se.Invoice).ToList()
                });

            models.ForEach(fe =>
            {
                var invs = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service).Select(se => se.Invoices.AsQueryable()).FirstOrDefault();
                if (invs != null)
                {
                    // Group By InvoiceID
                    IQueryable<AccAccountingManagement> invoiceQ = invs.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                    {
                        Id = s.Key.Id,
                        DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                        TotalAmount = s.FirstOrDefault().TotalAmount,
                        TotalAmountVnd = s.FirstOrDefault().TotalAmountVnd,
                        TotalAmountUsd = s.FirstOrDefault().TotalAmountUsd,

                        PaidAmount = s.FirstOrDefault().PaidAmount,
                        PaidAmountUsd = s.FirstOrDefault().PaidAmountUsd,
                        PaidAmountVnd = s.FirstOrDefault().PaidAmountVnd,

                        UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                        UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                        UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                        ServiceType = s.FirstOrDefault().ServiceType

                    });
                    fe.Over1To15Day = SumUnpaidAmountOfInvoices(invoiceQ, fe.ContractCurrency);
                }

            });

            return models;
        }

        /// <summary>
        /// Over 16 To 30 Day
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorOver16To30Day(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges, IQueryable<AccAccountingManagement> accAccountings)
        {
            //Lấy ra VAT Invoice có (type = Invoice or InvoiceTemp) & payment status # Paid & Status = Updated Invoice & Overdue days: từ 16 -30 ngày
            var acctMngts = accAccountings.Where(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                   && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED
                                                   && x.PaymentDueDate.HasValue
                                                   && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 31
                                                   && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 15);

            //Lấy ra các phí thu (SELLING) or phí thu (OBH - OBH Partner) đã issue VAT Invoice
            var surcharges = charges.Where(x => (x.Type == AccountingConstants.TYPE_CHARGE_SELL || x.Type == AccountingConstants.TYPE_CHARGE_OBH)
                                             && x.AcctManagementId != null);

            var invoices = from acctMngt in acctMngts
                           join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                           select new ReceivableInvoice
                           {
                               Office = surcharge.OfficeId,
                               PartnerId = surcharge.PaymentObjectId,
                               Service = surcharge.TransactionType,
                               Invoice = acctMngt
                           };

            if (invoices.Count() == 0)
            {
                return models;
            }
            //Group by Office, PartnerId, Service
            var grpInvoices = invoices.ToList()
                .GroupBy(g => new { Office = g.Office, PartnerId = g.PartnerId, Service = g.Service }).Select(s => new ReceivableInvoices
                {
                    Office = s.Key.Office,
                    PartnerId = s.Key.PartnerId,
                    Service = s.Key.Service,
                    Invoices = s.Select(se => se.Invoice).ToList()
                });

            models.ForEach(fe =>
            {
                var invs = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service).Select(se => se.Invoices.AsQueryable()).FirstOrDefault();
                if (invs != null)
                {
                    // Group By InvoiceID
                    IQueryable<AccAccountingManagement> invoiceQ = invs.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                    {
                        Id = s.Key.Id,
                        DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                        TotalAmount = s.FirstOrDefault().TotalAmount,
                        TotalAmountVnd = s.FirstOrDefault().TotalAmountVnd,
                        TotalAmountUsd = s.FirstOrDefault().TotalAmountUsd,

                        PaidAmount = s.FirstOrDefault().PaidAmount,
                        PaidAmountUsd = s.FirstOrDefault().PaidAmountUsd,
                        PaidAmountVnd = s.FirstOrDefault().PaidAmountVnd,

                        UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                        UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                        UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                        ServiceType = s.FirstOrDefault().ServiceType

                    });
                    fe.Over16To30Day = SumUnpaidAmountOfInvoices(invoiceQ, fe.ContractCurrency);
                }

            });

            return models;
        }

        /// <summary>
        /// Over 30 Day
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<AccAccountReceivableModel> CalculatorOver30Day(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges, IQueryable<AccAccountingManagement> accAccountings)
        {
            //Lấy ra VAT Invoice có (type = Invoice or InvoiceTemp) & payment status # Paid & Status = Updated Invoice & Overdue days: trên 30 ngày
            var acctMngts = accAccountings.Where(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                   && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED
                                                   && x.PaymentDueDate.HasValue
                                                   && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 30);

            //Lấy ra các phí thu (SELLING) or phí thu (OBH - OBH Partner) đã issue VAT Invoice
            var surcharges = charges.Where(x => (x.Type == AccountingConstants.TYPE_CHARGE_SELL || x.Type == AccountingConstants.TYPE_CHARGE_OBH)
                                             && x.AcctManagementId != null);

            var invoices = from acctMngt in acctMngts
                           join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                           select new ReceivableInvoice
                           {
                               Office = surcharge.OfficeId,
                               PartnerId = surcharge.PaymentObjectId,
                               Service = surcharge.TransactionType,
                               Invoice = acctMngt
                           };
            if (invoices.Count() == 0)
            {
                return models;
            }
            //Group by Office, PartnerId, Service
            var grpInvoices = invoices.ToList()
                .GroupBy(g => new { Office = g.Office, PartnerId = g.PartnerId, Service = g.Service }).Select(s => new ReceivableInvoices
                {
                    Office = s.Key.Office,
                    PartnerId = s.Key.PartnerId,
                    Service = s.Key.Service,
                    Invoices = s.Select(se => se.Invoice).ToList()
                });

            models.ForEach(fe =>
            {
                var invs = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service).Select(se => se.Invoices.AsQueryable()).FirstOrDefault();
                if (invs != null)
                {
                    // Group By InvoiceID
                    IQueryable<AccAccountingManagement> invoiceQ = invs.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                    {
                        Id = s.Key.Id,
                        DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                        TotalAmount = s.FirstOrDefault().TotalAmount,
                        TotalAmountVnd = s.FirstOrDefault().TotalAmountVnd,
                        TotalAmountUsd = s.FirstOrDefault().TotalAmountUsd,

                        PaidAmount = s.FirstOrDefault().PaidAmount,
                        PaidAmountUsd = s.FirstOrDefault().PaidAmountUsd,
                        PaidAmountVnd = s.FirstOrDefault().PaidAmountVnd,

                        UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                        UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                        UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                        ServiceType = s.FirstOrDefault().ServiceType

                    });
                    fe.Over30Day = SumUnpaidAmountOfInvoices(invoiceQ, fe.ContractCurrency);
                }

            });

            return models;
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
            if (receivables != null && receivables.Count() > 0)
            {
                var partnerChildIds = partnerRepo.Get(x => x.ParentId == agreement.PartnerId).Select(s => s.Id).ToList();
                List<CatContract> contractChildOfPartner = new List<CatContract>();
                if (partnerChildIds != null)
                {
                    contractChildOfPartner = contractPartnerRepo.Get(x => partnerChildIds.Any(a => a == x.PartnerId.ToString()) && x.ContractType == "Parent Contract").ToList();
                }

                agreement.BillingAmount = receivables.Sum(su => (su.BillingAmount ?? 0) + (su.ObhBilling ?? 0)); //Sum BillingAmount + BillingOBH
                //Credit Amount ~ Debit Amount
                agreement.DebitAmount = receivables.Sum(su => su.DebitAmount ?? 0) + contractChildOfPartner.Sum(su => su.DebitAmount ?? 0); //Sum DebitAmount + Debit Amount (của các đối tượng con)
                agreement.UnpaidAmount = receivables.Sum(su => (su.BillingUnpaid ?? 0) + (su.ObhUnpaid ?? 0)) + contractChildOfPartner.Sum(su => su.UnpaidAmount ?? 0); //Sum BillingUnpaid + ObhUnpaid + BillingUnpaid (của các đối tượng con)
                agreement.PaidAmount = receivables.Sum(su => (su.PaidAmount ?? 0) + (su.ObhPaid ?? 0)) + contractChildOfPartner.Sum(su => su.PaidAmount ?? 0); //Sum PaidAmount + ObhPaid + PaidAmount (của các đối tượng con)

                decimal? _creditRate = agreement.CreditRate;
                if (agreement.ContractType == "Trial")
                {
                    _creditRate = agreement.TrialCreditLimited == null ? 0 : (((agreement.DebitAmount ?? 0) + (agreement.CreditCurrency == AccountingConstants.CURRENCY_LOCAL ? (agreement.CustomerAdvanceAmountVnd ?? 0) : (agreement.CustomerAdvanceAmountUsd ?? 0))) / agreement.TrialCreditLimited) * 100; //((DebitAmount + CusAdv)/TrialCreditLimit)*100
                }
                if (agreement.ContractType == "Official")
                {
                    _creditRate = agreement.CreditLimit == null ? 0 : (((agreement.DebitAmount ?? 0) + (agreement.CreditCurrency == AccountingConstants.CURRENCY_LOCAL ? (agreement.CustomerAdvanceAmountVnd ?? 0) : (agreement.CustomerAdvanceAmountUsd ?? 0))) / agreement.CreditLimit) * 100; //((DebitAmount + CusAdv)/CreditLimit)*100
                }
                if (agreement.ContractType == "Parent Contract")
                {
                    var parentId = partnerRepo.Get(x => x.Id == agreement.PartnerId).FirstOrDefault()?.ParentId;
                    if (parentId != null)
                    {
                        //Lấy Credit Rate của đối tượng cha (Partner)
                        var creditRateContractParent = contractPartnerRepo.Get(x => x.PartnerId.ToString() == parentId && x.Active == true).FirstOrDefault()?.CreditRate;
                        _creditRate = (creditRateContractParent != null) ? creditRateContractParent : agreement.CreditRate;
                    }
                }
                agreement.CreditRate = _creditRate;
                agreement.DatetimeModified = DateTime.Now;
                agreement.UserModified = currentUser.UserID;

                if (agreement.CreditRate > AccountingConstants.MAX_CREDIT_LIMIT_RATE_CONTRACT)
                {
                    agreement.IsOverLimit = true; // vượt hạn mức
                }
                else
                {
                    agreement.IsOverLimit = false;
                }

                agreement.IsOverDue = receivables.Any(x => !DataTypeEx.IsNullOrValue(x.Over1To15Day, 0)
                || !DataTypeEx.IsNullOrValue(x.Over16To30Day, 0)
                || !DataTypeEx.IsNullOrValue(x.Over30Day, 0)
                );
            }
            return agreement;
        }

        private HandleState UpdateAgreementPartners(List<string> partnerIds, List<Guid?> agreementIds)
        {
            var hs = new HandleState();
            foreach (var partnerId in partnerIds)
            {
                var partner = partnerRepo.Get(x => x.Id == partnerId).FirstOrDefault();
                if (partner != null)
                {
                    //Agreement của partner
                    var contractPartner = contractPartnerRepo.Get(x => x.Active == true
                                                                    && x.PartnerId == partner.ParentId
                                                                    && agreementIds.Contains(x.Id)).FirstOrDefault();
                    if (contractPartner != null)
                    {
                        var agreementPartner = CalculatorAgreement(contractPartner);
                        hs = contractPartnerRepo.Update(agreementPartner, x => x.Id == agreementPartner.Id);
                    }
                    else
                    {
                        //Agreement của AcRef của partner
                        var contractParent = contractPartnerRepo.Get(x => x.Active == true
                                                                       && x.PartnerId == partner.ParentId
                                                                       && agreementIds.Contains(x.Id)).FirstOrDefault();
                        if (contractParent != null)
                        {
                            var agreementParent = CalculatorAgreement(contractParent);

                            hs = contractPartnerRepo.Update(agreementParent, x => x.Id == agreementParent.Id);
                        }
                    }
                }
            }
            return hs;
        }

        private async Task<HandleState> UpdateAgreementPartnersAsync(List<string> partnerIds, List<Guid?> agreementIds)
        {
            var hs = new HandleState();
            foreach (var partnerId in partnerIds)
            {
                var partner = partnerRepo.Get(x => x.Id == partnerId).FirstOrDefault();
                if (partner != null)
                {
                    //Agreement của partner
                    var contractPartner = contractPartnerRepo.Get(x => x.Active == true
                                                                    && x.PartnerId == partnerId
                                                                    && agreementIds.Contains(x.Id)).FirstOrDefault();
                    if (contractPartner != null)
                    {
                        var agreementPartner = CalculatorAgreement(contractPartner);
                        hs = await contractPartnerRepo.UpdateAsync(agreementPartner, x => x.Id == agreementPartner.Id);
                    }
                    else
                    {
                        //Agreement của AcRef của partner
                        var contractParent = contractPartnerRepo.Get(x => x.Active == true
                                                                       && x.PartnerId == partner.ParentId
                                                                       && agreementIds.Contains(x.Id)).FirstOrDefault();
                        if (contractParent != null)
                        {
                            var agreementParent = CalculatorAgreement(contractParent);

                            hs = await contractPartnerRepo.UpdateAsync(agreementParent, x => x.Id == agreementParent.Id);
                        }
                    }
                }
            }
            return hs;
        }

        public async Task<HandleState> CalculatorReceivableAsync(List<ObjectReceivableModel> model)
        {
            var receivables = new List<AccAccountReceivableModel>();
            HandleState hs = new HandleState();

            receivables = CalculatorReceivableData(model);
            var receivablesModel = mapper.Map<List<ReceivableTable>>(receivables);

            var hsInsertOrUpdate = InsertOrUpdateReceivableList(receivablesModel);
            if (!hsInsertOrUpdate.Status)
            {
                hs = new HandleState((object)hsInsertOrUpdate.Message);
            }
            WriteLogInsertOrUpdateReceivable(hsInsertOrUpdate.Status, hsInsertOrUpdate.Message, receivables);

            //Cập nhật giá trị công nợ vào Agreement của list Partner sau khi Insert or Update Receivable thành công
            var partnerIds = receivables.Select(s => s.PartnerId).ToList();
            var agreementIds = receivables.Select(s => s.ContractId).ToList();
            UpdateAgreementPartners(partnerIds, agreementIds);

            return hs;
        }

        public async Task<HandleState> InsertOrUpdateReceivableAsync(List<ObjectReceivableModel> models)
        {
            //Insert Or Update Receivable Multiple
            var receivables = new List<AccAccountReceivableModel>();
            HandleState hs = new HandleState();

            try
            {
                receivables = CalculatorReceivableData(models);
                var receivablesModel = mapper.Map<List<ReceivableTable>>(receivables);

                var hsInsertOrUpdate = InsertOrUpdateReceivableList(receivablesModel);
                if (!hsInsertOrUpdate.Status)
                {
                    hs = new HandleState((object)hsInsertOrUpdate.Message);
                }
                WriteLogInsertOrUpdateReceivable(hsInsertOrUpdate.Status, hsInsertOrUpdate.Message, receivables);

                //Cập nhật giá trị công nợ vào Agreement của list Partner sau khi Insert or Update Receivable thành công
                var partnerIds = receivables.Select(s => s.PartnerId).ToList();
                var agreementIds = receivables.Select(s => s.ContractId).ToList();

                await UpdateAgreementPartnersAsync(partnerIds, agreementIds);

                return hs;
            }
            catch (Exception ex)
            {
                WriteLogInsertOrUpdateReceivable(false, ex.Message, receivables);
                return new HandleState((object)ex.Message);
            }
        }

        private List<AccAccountReceivableModel> CalculatorReceivableData(List<ObjectReceivableModel> models)
        {
            var receivables = new List<AccAccountReceivableModel>();
            foreach (var model in models)
            {
                var receivable = new AccAccountReceivableModel();
                receivable.Over30Day = 0;
                receivable.Over1To15Day = 0;
                receivable.Over16To30Day = 0;
                receivable.ObhAmount = 0;
                receivable.ObhBilling = 0;
                receivable.ObhPaid = 0;
                receivable.ObhUnpaid = 0;
                receivable.DebitAmount = 0;
                var partner = partnerRepo.Get(x => x.Id == model.PartnerId).FirstOrDefault();
                //Không tính công nợ cho đối tượng Internal
                if (partner != null && partner.PartnerMode != "Internal")
                {
                    receivable.PartnerId = model.PartnerId;
                    receivable.Office = model.Office;
                    receivable.Service = model.Service;
                    receivable.AcRef = partner.ParentId ?? partner.Id;
                    var contractPartner = contractPartnerRepo.Get(x => x.Active == true
                                                                    && x.PartnerId == model.PartnerId
                                                                    && x.OfficeId.Contains(model.Office.ToString())
                                                                    && x.SaleService.Contains(model.Service)).OrderBy(x => x.ContractType)
                                                                    .ThenBy(c => c.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL)
                                                                    .FirstOrDefault();
                    if (contractPartner == null)
                    {
                        // Lấy currency local và use created of partner gán cho Receivable
                        receivable.ContractId = null;
                        receivable.ContractCurrency = AccountingConstants.CURRENCY_LOCAL;
                        receivable.SaleMan = null;
                        receivable.UserCreated = partner.UserCreated;
                        receivable.UserModified = partner.UserCreated;
                        receivable.GroupId = partner.GroupId;
                        receivable.DepartmentId = partner.DepartmentId;
                        receivable.OfficeId = partner.OfficeId;
                        receivable.CompanyId = partner.CompanyId;
                    }
                    else
                    {
                        // Lấy currency của contract & user created of contract gán cho Receivable
                        receivable.ContractId = contractPartner.Id;
                        receivable.ContractCurrency = contractPartner.CreditCurrency;
                        receivable.SaleMan = contractPartner.SaleManId;
                        receivable.UserCreated = contractPartner.UserCreated;
                        receivable.UserModified = contractPartner.UserCreated;
                        receivable.GroupId = null;
                        receivable.DepartmentId = null;
                        receivable.OfficeId = model.Office;
                        receivable.CompanyId = contractPartner.CompanyId;
                    }
                }
                receivables.Add(receivable);
            }

            if (receivables.Count > 0)
            {
                //Surcharge thuộc Office, Service, PartnerId của Receivable
                var surcharges = surchargeRepo.Get(x => models.Any(a => a.Office == x.OfficeId && a.Service == x.TransactionType && a.PartnerId == x.PaymentObjectId));
                //List Invoice (Type = Invoice or InvoiceTemp)
                var invoices = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE || x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);

                receivables = CalculatorBillingAmount(receivables, surcharges, invoices); //Billing Amount, UnpaidAmo
                                                                                          //receivables = CalculatorBillingUnpaid(receivables, surcharges, invoices); //Billing Unpaid
                receivables = CalculatorPaidAmount(receivables, surcharges, invoices); //Paid Amount
                receivables = CalculatorObhUnpaid(receivables, surcharges, invoices); //Obh Unpaid
                receivables = CalculatorObhPaid(receivables, surcharges, invoices); //Obh Paid
                receivables = CalculatorObhAmount(receivables, surcharges); //Obh Amount: Cộng thêm OBH Unpaid (đã cộng bên trong)
                receivables = CalculatorObhBilling(receivables, surcharges, invoices); //Obh Billing
                receivables = CalculatorAdvanceAmount(receivables); //Advance Amount
                receivables = CalculatorCreditAmount(receivables, surcharges); //Credit Amount
                receivables = CalculatorSellingNoVat(receivables, surcharges); //Selling No Vat
                receivables = CalculatorOver1To15Day(receivables, surcharges, invoices); //Over 1 To 15 Day
                receivables = CalculatorOver16To30Day(receivables, surcharges, invoices); //Over 16 To 30 Day
                receivables = CalculatorOver30Day(receivables, surcharges, invoices); //Over 30 Day
            }
            receivables.ForEach(fe => {
                //Calculator Debit Amount
                fe.DebitAmount = (fe.SellingNoVat ?? 0) + (fe.BillingUnpaid ?? 0) + (fe.ObhAmount ?? 0) + (fe.AdvanceAmount ?? 0); // Công nợ chưa billing
                fe.DatetimeCreated = DateTime.Now;
                fe.DatetimeModified = DateTime.Now;
            });
            return receivables;
        }

        public HandleState InsertOrUpdateReceivable(List<ObjectReceivableModel> models)
        {
            //Insert Or Update Receivable Multiple
            var receivables = new List<AccAccountReceivableModel>();
            HandleState hs = new HandleState();

            try
            {
                receivables = CalculatorReceivableData(models);
                var receivablesModel = mapper.Map<List<ReceivableTable>>(receivables);

                var hsInsertOrUpdate = InsertOrUpdateReceivableList(receivablesModel);
                if (!hsInsertOrUpdate.Status)
                {
                    hs = new HandleState((object)hsInsertOrUpdate.Message);
                }
                WriteLogInsertOrUpdateReceivable(hsInsertOrUpdate.Status, hsInsertOrUpdate.Message, receivables);

                //Cập nhật giá trị công nợ vào Agreement của list Partner sau khi Insert or Update Receivable thành công
                var partnerIds = receivables.Select(s => s.PartnerId).ToList();
                var agreementIds = receivables.Select(s => s.ContractId).ToList();

                UpdateAgreementPartners(partnerIds, agreementIds);

                return hs;
            }
            catch (Exception ex)
            {
                WriteLogInsertOrUpdateReceivable(false, ex.Message, receivables);
                return new HandleState((object)ex.Message);
            }
        }

        public HandleState CalculatorReceivable(CalculatorReceivableModel model)
        {
            HandleState hs = new HandleState();
            currentUser.Action = "CalculatorReceivable";
            if (model != null && model.ObjectReceivable.Count() > 0)
            {
                // PartnerId, Office, Service # Empty And # Null
                var objReceivalble = model.ObjectReceivable.Where(x => !string.IsNullOrEmpty(x.PartnerId)
                                                                  && (x.Office != null && x.Office != Guid.Empty)
                                                                  && !string.IsNullOrEmpty(x.Service))
                                                                  .GroupBy(g => new { g.PartnerId, g.Office, g.Service })
                                                                  .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Office = s.Key.Office, Service = s.Key.Service }).ToList();
                if (objReceivalble.Count > 0)
                {
                    hs = InsertOrUpdateReceivable(objReceivalble);
                }
            }
            return hs;
        }

        public async Task<HandleState> CalculatorReceivableAsync(CalculatorReceivableModel model)
        {
            HandleState hs = new HandleState();
            currentUser.Action = "CalculatorReceivable";
            if (model != null && model.ObjectReceivable.Count() > 0)
            {
                // PartnerId, Office, Service # Empty And # Null
                var objReceivalble = model.ObjectReceivable.Where(x => !string.IsNullOrEmpty(x.PartnerId)
                                                                  && (x.Office != null && x.Office != Guid.Empty)
                                                                  && !string.IsNullOrEmpty(x.Service))
                                                                  .GroupBy(g => new { g.PartnerId, g.Office, g.Service })
                                                                  .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Office = s.Key.Office, Service = s.Key.Service }).ToList();
                if (objReceivalble.Count > 0)
                {
                    hs = await InsertOrUpdateReceivableAsync(objReceivalble);
                }
            }
            return hs;
        }

        public HandleState CalculatorReceivableNotAuthorize(CalculatorReceivableNotAuthorizeModel model)
        {
            currentUser.UserID = model.UserID;
            currentUser.GroupId = model.GroupId;
            currentUser.DepartmentId = model.DepartmentId;
            currentUser.OfficeID = model.OfficeID;
            currentUser.CompanyID = model.CompanyID;
            currentUser.Action = model.Action;
            var hs = CalculatorReceivable(model);
            return hs;
        }

        private sp_InsertOrUpdateReceivable InsertOrUpdateReceivableList(List<ReceivableTable> receivables)
        {
            var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@Receivables",
                    Value = DataHelper.ToDataTable(receivables),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[ReceivableTable]"
                }
            };
            var result = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_InsertOrUpdateReceivable>(parameters);
            return result.FirstOrDefault();
        }

        private void WriteLogInsertOrUpdateReceivable(bool status, string message, List<AccAccountReceivableModel> receivables)
        {
            string logMessage = string.Format("InsertOrUpdateReceivable by {0} at {1} \n ** Message: {2} \n ** Receivables: {3} \n\n---------------------------\n\n",
                            currentUser.Action,
                            DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                            message,
                            receivables != null ? JsonConvert.SerializeObject(receivables) : "[]");
            string logName = string.Format("InsertOrUpdateReceivable_{0}_eFMS_LOG", (status ? "Success" : "Fail"));
            new LogHelper(logName, logMessage);
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
            var objectReceivables = objMerge.GroupBy(g => new { Service = g.Service, PartnerId = g.PartnerId, Office = g.Office })
                .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Service = s.Key.Service, Office = s.Key.Office });
            return objectReceivables.ToList();
        }

        #region --- LIST & PAGING ---
        private IQueryable<AccountReceivableResult> GetARHasContract(IQueryable<AccAccountReceivable> acctReceivables, IQueryable<CatContract> partnerContracts, IQueryable<CatPartner> partners)
        {
            var users = userRepo.Get();
            var employees = employeeRepo.Get();
            var acRefPartner = partnerRepo.Get();

            var selectQuery = from contract in partnerContracts
                              //join acctReceivable in acctReceivables on contract.PartnerId equals acctReceivable.PartnerId into acctReceivables2
                              join acctReceivable in acctReceivables on contract.Id equals acctReceivable.ContractId into acctReceivables2
                              from acctReceivable in acctReceivables2.DefaultIfEmpty()
                              where contract.SaleService.Contains(acctReceivable.Service) && contract.OfficeId.Contains(acctReceivable.Office.ToString(), StringComparison.OrdinalIgnoreCase)
                              select new { acctReceivable, contract };
            if (selectQuery == null || !selectQuery.Any()) return null;

            var contractsGuaranteed = selectQuery.Where(x => x.contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEED)
                .Select(s => new AccountReceivableResult
                {
                    AgreementSalesmanId = s.contract.SaleManId,
                    DebitAmount = s.acctReceivable.DebitAmount
                }).ToList();
            // Group by Contract ID, Service AR, Office AR
            var groupByContract = selectQuery.GroupBy(g => new { g.contract.Id, g.acctReceivable.Service, g.acctReceivable.Office ,g.contract.OfficeId })
                .Select(s => new AccountReceivableResult
                {
                    AgreementId = s.Key.Id,
                    PartnerId = s.First().acctReceivable != null ? s.First().acctReceivable.PartnerId : null,
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
                    OfficeContract = s.Key.OfficeId.ToString(), //Office AR chứa trong Office Argeement
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
                    ObhUnPaidAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.ObhUnpaid : 0).Sum(),
                    ObhAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.ObhAmount : 0).Sum(),
                    ObhPaidAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.ObhPaid : 0).Sum(),
                    ObhBillingAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.ObhBilling : 0).Sum(),
                    DebitRate = s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL ?
                                                                Math.Round((
                                                                    s.First().contract.TrialCreditLimited != 0 && s.First().contract.TrialCreditLimited != null ?
                                                                    (s.Select(se => se.acctReceivable != null ? se.acctReceivable.DebitAmount : null).Sum() + (s.First().contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL ? (s.First().contract.CustomerAdvanceAmountVnd ?? 0) : (s.First().contract.CustomerAdvanceAmountUsd ?? 0))) /(s.First().contract.TrialCreditLimited)
                                                                    :0) * 100 ?? 0,3) :
                                (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL ?
                                                                Math.Round((
                                                                    s.First().contract.CreditLimit != 0 && s.First().contract.CreditLimit != null ?
                                                                    (s.Select(se => se.acctReceivable != null ? se.acctReceivable.DebitAmount : null).Sum() + (s.First().contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL ? (s.First().contract.CustomerAdvanceAmountVnd ?? 0) : (s.First().contract.CustomerAdvanceAmountUsd ?? 0))) / (s.First().contract.CreditLimit)
                                                                    : 0) * 100 ?? 0, 3):0),
                    CusAdvanceVnd = s.First().contract.CustomerAdvanceAmountVnd ?? 0,
                    CusAdvanceUsd = s.First().contract.CustomerAdvanceAmountUsd ?? 0,
                    BillingAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.BillingAmount : 0).Sum(),
                    BillingUnpaid = s.Select(se => se.acctReceivable != null ? se.acctReceivable.BillingUnpaid : 0).Sum(),
                    PaidAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.PaidAmount : 0).Sum(),
                    CreditAmount = s.Select(se => se.acctReceivable != null ? se.acctReceivable.CreditAmount : 0).Sum(),
                    Over1To15Day = s.Select(se => se.acctReceivable != null ? se.acctReceivable.Over1To15Day : 0).Sum(),
                    Over16To30Day = s.Select(se => se.acctReceivable != null ? se.acctReceivable.Over16To30Day : 0).Sum(),
                    Over30Day = s.Select(se => se.acctReceivable != null ? se.acctReceivable.Over30Day : 0).Sum(),
                    ArCurrency = s.First().acctReceivable != null ? s.First().acctReceivable.ContractCurrency : null,
                    CreditCurrency = s.First().contract.CreditCurrency,
                    ParentNameAbbr = string.Empty, //Get data bên dưới,
                    DatetimeModified = s.FirstOrDefault().acctReceivable.DatetimeModified,
                    IsOverDue = s.FirstOrDefault().contract.IsOverDue,
                    IsOverLimit = s.FirstOrDefault().contract.IsOverLimit,
                    IsExpired = s.FirstOrDefault().contract.IsExpired,
                    
                });

            var data = from contract in groupByContract
                       join partner in partners on contract.PartnerId equals partner.Id
                       join parent in acRefPartner on partner.ParentId equals parent.Id into parents
                       from parent in parents.DefaultIfEmpty()
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
                           ObhBillingAmount = contract.ObhBillingAmount,
                           ObhPaidAmount = contract.ObhPaidAmount,
                           ObhUnPaidAmount = contract.ObhUnPaidAmount,
                           DebitRate = contract.DebitRate,
                           CusAdvanceUsd = contract.CusAdvanceUsd,
                           CusAdvanceVnd = contract.CusAdvanceVnd,
                           BillingAmount = contract.BillingAmount,
                           BillingUnpaid = contract.BillingUnpaid,
                           PaidAmount = contract.PaidAmount,
                           CreditAmount = contract.CreditAmount,
                           Over1To15Day = contract.Over1To15Day,
                           Over16To30Day = contract.Over16To30Day,
                           Over30Day = contract.Over30Day,
                           ArCurrency = contract.ArCurrency,
                           CreditCurrency = contract.CreditCurrency,
                           ParentNameAbbr = parent.ShortName,
                           DatetimeModified = contract.DatetimeModified,
                           OfficeContract = contract.OfficeContract,
                           IsOverDue = contract.IsOverDue,
                           IsExpired = contract.IsExpired,
                           IsOverLimit = contract.IsOverLimit
                       };
            return data;
        }

        private IQueryable<AccountReceivableResult> GetARNoContract(IQueryable<AccAccountReceivable> acctReceivables, IQueryable<CatContract> partnerContracts, IQueryable<CatPartner> partners)
        {
            var selectQuery = from acctReceivable in acctReceivables
                                  //join partnerContract in partnerContracts on acctReceivable.AcRef equals partnerContract.PartnerId into partnerContract2
                              join partnerContract in partnerContracts on acctReceivable.PartnerId equals partnerContract.PartnerId into partnerContract2
                              from partnerContract in partnerContract2.DefaultIfEmpty()
                              where acctReceivable.PartnerId != partnerContract.PartnerId
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
                query = query.And(x => x.PartnerId == criteria.AcRefId);
            }
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
            if (criteria.OfficeIds != null)
                query = query.And(x => x.OfficeId != null && criteria.OfficeIds.Contains(x.OfficeId.ToString()));

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

        private IQueryable<AccountReceivableResult> GetDataTrialOfficial(AccountReceivableCriteria criteria)
        {
            var queryAcctReceivable = ExpressionAcctReceivableQuery(criteria);
            var acctReceivables = DataContext.Get(queryAcctReceivable);

            var partners = QueryPartner(criteria);

            var contracts = contractPartnerRepo.Get(x => x.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL
            || x.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL
            || x.ContractType == AccountingConstants.ARGEEMENT_TYPE_PARENT
            || x.ContractType == AccountingConstants.ARGEEMENT_TYPE_CASH);

            var partnerContracts = QueryContractPartner(contracts, criteria);

            IQueryable<AccountReceivableResult> arPartnerContracts = GetARHasContract(acctReceivables, partnerContracts, partners);

            if (arPartnerContracts == null || !arPartnerContracts.Any())
            {
                return null;
            }
            else
            {
                arPartnerContracts = GetArPartnerContractGroupByAgreementId(arPartnerContracts);
                var queryAccountReceivable = ExpressionAccountReceivableQuery(criteria);
                arPartnerContracts = arPartnerContracts.Where(queryAccountReceivable).OrderByDescending(x => x.DatetimeModified);

                IQueryable<AccountReceivableResult> arPartnerNoContracts = GetARNoContract(acctReceivables, partnerContracts, partners);
                if (arPartnerNoContracts != null)
                    arPartnerContracts = arPartnerContracts.Concat(arPartnerNoContracts).OrderByDescending(x => x.DatetimeModified);
            }

            var res = new List<AccountReceivableResult>();
            res = arPartnerContracts.ToList();
            
            res = res.Where(x => x.DebitAmount > 0).ToList();

            if (criteria.DebitRateTo != null && criteria.DebitRateFrom != null)
                res = res.Where(x => x.DebitRate >= criteria.DebitRateFrom && x.DebitRate <= criteria.DebitRateTo).ToList();
            if (criteria.AgreementStatus!= null && criteria.AgreementStatus != "All")
                res = res.Where(x => x.AgreementStatus == criteria.AgreementStatus).ToList();
            if (criteria.AgreementExpiredDay !=null && criteria.AgreementExpiredDay!="All")
            {
                switch (criteria.AgreementExpiredDay)
                {
                    case "Normal":
                        res = res.Where(x => x.ExpriedDay > 30).ToList();
                        break;
                    case "30Day":
                        res = res.Where(x => x.ExpriedDay == 30).ToList();
                        break;
                    case "15Day":
                        res = res.Where(x => x.ExpriedDay <= 15).ToList();
                        break;
                    case "Expired":
                        res = res.Where(x => x.ExpriedDay <= 0).ToList();
                        break;
                }
            }
            if (criteria.Staffs != null && criteria.Staffs.Count > 0)
                res = res.Where(x => criteria.Staffs.Contains(x.AgreementSalesmanId)).ToList();
            if (criteria.OfficeIds != null && criteria.OfficeIds.Count > 0)
                res = res.Where(x => x.OfficeId != null && criteria.OfficeIds.Contains(x.OfficeId)).ToList();

            switch (criteria.OverDueDay)
            {
                case OverDueDayEnum.Over1_15:
                    res = res.Where(x => x.Over1To15Day > 0).ToList();
                    break;
                case OverDueDayEnum.Over16_30:
                    res = res.Where(x => x.Over16To30Day > 0).ToList();
                    break;
                case OverDueDayEnum.Over30:
                    res = res.Where(x => x.Over30Day > 0).ToList();
                    break;
                case OverDueDayEnum.All:
                    break;
            }

            return res.AsQueryable().OrderByDescending(x=>x.DebitRate);
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
                    .GroupBy(g => new { g.AgreementId})
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
                        CusAdvanceUsd = s.First().CusAdvanceUsd,
                        CusAdvanceVnd = s.First().CusAdvanceVnd,
                        BillingAmount = s.Sum(sum => sum.BillingAmount),
                        BillingUnpaid = s.Sum(sum => sum.BillingUnpaid),
                        PaidAmount = s.Sum(sum => sum.PaidAmount),
                        CreditAmount = s.Sum(sum => sum.CreditAmount),
                        Over1To15Day = s.Sum(sum => sum.Over1To15Day),
                        Over16To30Day = s.Sum(sum => sum.Over16To30Day),
                        Over30Day = s.Sum(sum => sum.Over30Day),
                        ArCurrency = s.First().ArCurrency,
                        ParentNameAbbr = s.First().ParentNameAbbr,
                        ObhBillingAmount = s.Sum(sum=>sum.ObhBillingAmount),
                        ObhPaidAmount=s.Sum(sum=>sum.ObhPaidAmount),
                        ObhUnPaidAmount = s.Sum(sum=>sum.ObhUnPaidAmount),
                        DatetimeModified = s.First().DatetimeModified,
                        OfficeContract = s.First().OfficeContract,
                        IsExpired = s.First().IsExpired,
                        IsOverLimit = s.First().IsOverLimit,
                        IsOverDue = s.First().IsOverDue,
                    }).OrderByDescending(s=>s.DebitRate).AsQueryable();
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

        private IQueryable<CatPartner> QueryPartner(AccountReceivableCriteria criteria)
        {
            Expression<Func<CatPartner, bool>> query = q => true;
            if (criteria.PartnerType == ParterTypeEnum.Customer.ToString())
                query = query.And(x => x.PartnerType.Contains(ParterTypeEnum.Customer.ToString()));
            if (criteria.PartnerType == ParterTypeEnum.Agent.ToString())
                query = query.And(x => x.PartnerType.Contains(ParterTypeEnum.Agent.ToString()));

            return partnerRepo.Get(query);
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
                    Currency = se.FirstOrDefault().ArCurrency,
                    TotalObhBillingAmount = se.Select(sel => sel.ObhBillingAmount).Sum(),
                    TotalObhUnPaidAmount = se.Select(sel => sel.ObhUnPaidAmount).Sum(),
                    TotalObhPaidAmount = se.Select(sel => sel.ObhPaidAmount).Sum(),
                    AccountReceivableGrpServices = se.Select(sel => new AccountReceivableServiceResult
                    {
                        OfficeId = Guid.Parse(sel.OfficeId),
                        ServiceName = sel.ArServiceName,
                        ServiceCode = sel.ArServiceCode,
                        DebitAmount = sel.DebitAmount,
                        BillingAmount = sel.BillingAmount,
                        BillingUnpaid = sel.BillingUnpaid,
                        PaidAmount = sel.PaidAmount,
                        ObhAmount = sel.ObhAmount,
                        Over1To15Day = sel.Over1To15Day,
                        Over16To30Day = sel.Over16To30Day,
                        Over30Day = sel.Over30Day,
                        Currency = sel.ArCurrency,
                        ObhBillingAmount = sel.ObhBillingAmount,
                        ObhPaidAmount = sel.ObhPaidAmount,
                        ObhUnPaidAmount = sel.ObhUnPaidAmount
                    }).ToList()
                });
            var offices = officeRepo.Get();
            var data = from ar in arGroupOffices
                       join office in offices on ar.OfficeId equals office.Id
                       select new AccountReceivableGroupOfficeResult
                       {
                           OfficeId = ar.OfficeId,
                           OfficeName = office.BranchNameEn,
                           OfficeNameAbbr = office.ShortName,
                           TotalDebitAmount = ar.TotalDebitAmount,
                           TotalBillingAmount = ar.TotalBillingAmount,
                           TotalBillingUnpaid = ar.TotalBillingUnpaid,
                           TotalPaidAmount = ar.TotalPaidAmount,
                           TotalObhAmount = ar.TotalObhAmount,
                           TotalOver1To15Day = ar.TotalOver1To15Day,
                           TotalOver15To30Day = ar.TotalOver15To30Day,
                           TotalOver30Day = ar.TotalOver30Day,
                           Currency = ar.Currency,
                           TotalObhBillingAmount = ar.TotalObhBillingAmount,
                           TotalObhPaidAmount = ar.TotalObhPaidAmount,
                           TotalObhUnPaidAmount = ar.TotalObhUnPaidAmount,
                           AccountReceivableGrpServices = ar.AccountReceivableGrpServices
                       };
            return data.ToList();
        }

        public AccountReceivableDetailResult GetDetailAccountReceivableByArgeementId(Guid argeementId)
        {
            if (argeementId == null || argeementId == Guid.Empty) return null;
            var argeement = contractPartnerRepo.Get(x => x.Id == argeementId).FirstOrDefault();
            if (argeement == null) return null;

            var acctReceivables = DataContext.Get(x => x.Office != null && x.ContractId == argeementId);
            var partners = partnerRepo.Get();
            var partnerContracts = contractPartnerRepo.Get(x => x.ContractType == argeement.ContractType);
            var arPartnerContracts = GetARHasContract(acctReceivables, partnerContracts, partners);

            var detail = new AccountReceivableDetailResult();
            if (arPartnerContracts == null) return new AccountReceivableDetailResult();
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
                ObhBillingAmount = s.Sum(sum => sum.ObhBillingAmount),
                ObhPaidAmount = s.Sum(sum => sum.ObhPaidAmount),
                ObhUnPaidAmount = s.Sum(sum => sum.ObhUnPaidAmount),
                DebitRate = s.Sum(sum => sum.DebitRate),
                CusAdvanceVnd = s.Select(se => se.CusAdvanceVnd).FirstOrDefault(),
                CusAdvanceUsd = s.Select(se => se.CusAdvanceUsd).FirstOrDefault(),
                BillingAmount = s.Sum(sum => sum.BillingAmount),
                BillingUnpaid = s.Sum(sum => sum.BillingUnpaid),
                PaidAmount = s.Sum(sum => sum.PaidAmount),
                CreditAmount = s.Sum(sum => sum.CreditAmount),
                Over1To15Day = s.Sum(sum => sum.Over1To15Day),
                Over16To30Day = s.Sum(sum => sum.Over16To30Day),
                Over30Day = s.Sum(sum => sum.Over30Day),
                ArCurrency = s.Select(se => se.ArCurrency).FirstOrDefault(),
                IsExpired = s.FirstOrDefault().IsExpired,
                IsOverLimit = s.FirstOrDefault().IsOverLimit,
                IsOverDue = s.FirstOrDefault().IsOverDue,
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

        public IEnumerable<object> GetDataARSumaryExport(AccountReceivableCriteria criteria)
        {
            IEnumerable<object> data = GetDataARByCriteria(criteria);
            return data;
        }

        public IEnumerable<object> GetDataDebitDetail(Guid argeementId, string option, string officeId, string serviceCode,int overDueDay = 0)
        {
            if (argeementId == null || argeementId == Guid.Empty) return null;
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("argid", argeementId),
                SqlParam.GetParameter("option", option),
                SqlParam.GetParameter("officeId",!string.IsNullOrEmpty(officeId)?officeId:""),
                SqlParam.GetParameter("serviceCode",!string.IsNullOrEmpty(serviceCode)?serviceCode:""),
                SqlParam.GetParameter("overDueDay",overDueDay)
            };
            var data = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDebitDetailByArgId>(parameters);
            return data;
        }
        #endregion --- DETAIL ---

        #region -- Update AcctManagement and overdays AccountReceivable after change payment term contract
        /// <summary>
        /// UpdateDueDateAndOverDays
        /// </summary>
        /// <param name="contractModel"></param>
        /// <returns></returns>
        public async Task<HandleState> UpdateDueDateAndOverDaysAfterChangePaymentTerm(CatContractModel contractModel)
        {
            var listInvoices = new List<AccAccountingManagement>();
            var hs = UpdateDueDateAcctManagement(contractModel, out listInvoices);
            if (listInvoices.Count > 0)
            {
                await UpdateOverDayAcctReceivables(contractModel, listInvoices);
            }
            return hs;
        }

        /// <summary>
        /// Update payment due date
        /// </summary>
        /// <param name="contractModel"></param>
        /// <param name="listInvoices"></param>
        /// <returns></returns>
        public HandleState UpdateDueDateAcctManagement(CatContractModel contractModel, out List<AccAccountingManagement> listInvoices)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                HandleState hs = new HandleState();
                listInvoices = new List<AccAccountingManagement>();
                try
                {
                    currentUser.Action = "UpdateDueDateAcctMngAfterChangePaymentTerm";
                    var invoiceData = accountingManagementRepo.Get().Where(x => x.PartnerId == contractModel.PartnerId &&
                                             contractModel.SaleService.Contains(x.ServiceType) &&
                                            contractModel.OfficeId.Contains(x.OfficeId.ToString()) &&
                                            (x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE || x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE) &&
                                            x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID).ToList();
                    if (invoiceData.Count > 0)
                    {
                        foreach (var invoice in invoiceData)
                        {
                            //Nếu Base On là Invoice Date: Due Date = Invoice Date + Payment Term
                            if (contractModel.BaseOn == "Invoice Date")
                            {
                                invoice.PaymentTerm = contractModel.PaymentTerm;
                                invoice.PaymentDueDate = invoice.Date.HasValue ? invoice.Date.Value.AddDays((double)(contractModel.PaymentTerm ?? 0)) : invoice.PaymentDueDate;
                            }
                            //Nếu Base On là Billing Date : Due Date = Billing date + Payment Term
                            if (contractModel.BaseOn == "Confirmed Billing")
                            {
                                invoice.PaymentTerm = contractModel.PaymentTerm;
                                invoice.PaymentDueDate = invoice.ConfirmBillingDate.HasValue ? invoice.ConfirmBillingDate.Value.AddDays((double)(contractModel.PaymentTerm ?? 0)) : invoice.PaymentDueDate;
                            }
                            var hsPaymentMgn = accountingManagementRepo.Update(invoice, x => x.Id == invoice.Id, false);
                        }
                    }
                    if (contractModel.BaseOn == "Invoice Date" || contractModel.BaseOn == "Confirmed Billing")
                    {
                        listInvoices = invoiceData;
                        accountingManagementRepo.SubmitChanges();
                    }
                    trans.Commit();
                    return hs;
                }
                catch (Exception ex)
                {
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        /// <summary>
        /// Update over due amount
        /// </summary>
        /// <param name="contractModel"></param>
        /// <param name="invoiceData"></param>
        /// <returns></returns>
        public Task<HandleState> UpdateOverDayAcctReceivables(CatContractModel contractModel, List<AccAccountingManagement> invoiceData)
        {
            var acctReceivablesModel = new List<AccAccountReceivableModel>();
            try
            {
                HandleState hs = new HandleState();
                currentUser.Action = "UpdateOverDayReceivableAfterChangePaymentTerm";
                //Get DS Công nợ có cùng PartnerId, Saleman, Service, Office của Agreement
                var receivables = DataContext.Get(x => x.PartnerId == contractModel.PartnerId
                                                    && x.SaleMan == contractModel.SaleManId
                                                    && contractModel.SaleService.Contains(x.Service)
                                                    && contractModel.OfficeId.Contains(x.Office.ToString())).ToList();
                if (receivables != null && receivables.Count() > 0)
                {
                    var acctMngts = invoiceData.Where(x => x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                           && x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED
                                                           && x.PaymentDueDate.HasValue).AsQueryable();
                    //Surcharge thuộc Office, Service, PartnerId của Receivable
                    var surcharges = surchargeRepo.Get(x => receivables.Any(a => a.Office == x.OfficeId && a.Service == x.TransactionType && a.PartnerId == x.PaymentObjectId));

                    var resultReceivables = CalculatorOverDaysAmount(receivables, surcharges, acctMngts);
                    var receivablesModel = mapper.Map<List<ReceivableTable>>(resultReceivables);
                    acctReceivablesModel = mapper.Map<List<AccAccountReceivableModel>>(resultReceivables);
                    var hsInsertOrUpdate = InsertOrUpdateReceivableList(receivablesModel);
                    if (!hsInsertOrUpdate.Status)
                    {
                        hs = new HandleState((object)hsInsertOrUpdate.Message);
                    }
                    WriteLogInsertOrUpdateReceivable(hsInsertOrUpdate.Status, hsInsertOrUpdate.Message, acctReceivablesModel);
                }
                return Task.FromResult(hs);
            }
            catch (Exception ex)
            {
                WriteLogInsertOrUpdateReceivable(false, ex.Message, acctReceivablesModel);
                return Task.FromResult(new HandleState((object)ex.Message));
            }
        }

        /// <summary>
        /// Caculate Over amount of Receivables
        /// </summary>
        /// <param name="models"></param>
        /// <param name="surcharges"></param>
        /// <param name="accAccountings"></param>
        /// <returns></returns>
        private List<AccAccountReceivable> CalculatorOverDaysAmount(List<AccAccountReceivable> models, IQueryable<CsShipmentSurcharge> surcharges, IQueryable<AccAccountingManagement> accAccountings)
        {
            // Get công nợ quá hạn từ 1->15 ngày
            var invoiceOver1To15 = accAccountings.Where(x => (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 31
                                                       && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 15);
            var invoices = from acctMngt in invoiceOver1To15
                           join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                           select new ReceivableInvoice
                           {
                               Office = surcharge.OfficeId,
                               PartnerId = surcharge.PaymentObjectId,
                               Service = surcharge.TransactionType,
                               Invoice = acctMngt
                           };
            if (invoices.Count() > 0)
            {
                //Group by Office, PartnerId, Service
                var grpInvoices = invoices.ToList()
                    .GroupBy(g => new { Office = g.Office, PartnerId = g.PartnerId, Service = g.Service }).Select(s => new ReceivableInvoices
                    {
                        Office = s.Key.Office,
                        PartnerId = s.Key.PartnerId,
                        Service = s.Key.Service,
                        Invoices = s.Select(se => se.Invoice).ToList()
                    });

                models.ForEach(fe =>
                {
                    var invs = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service).Select(se => se.Invoices.AsQueryable()).FirstOrDefault();
                    if (invs != null)
                    {
                    // Group By InvoiceID
                    IQueryable<AccAccountingManagement> invoiceQ = invs.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                        {
                            Id = s.Key.Id,
                            DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                            UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                            UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                            UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                            ServiceType = s.FirstOrDefault().ServiceType

                        });
                        fe.Over1To15Day = SumUnpaidAmountOfInvoices(invoiceQ, fe.ContractCurrency);
                    }

                });
            }
            else
            {
                models.ForEach(fe => fe.Over1To15Day = 0);
            }

            // Get công nợ quá hạn từ 16->30 ngày
            var invoiceOver16To30 = accAccountings.Where(x => (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 15
                                                       && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 31);
            invoices = from acctMngt in invoiceOver16To30
                       join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                       select new ReceivableInvoice
                       {
                           Office = surcharge.OfficeId,
                           PartnerId = surcharge.PaymentObjectId,
                           Service = surcharge.TransactionType,
                           Invoice = acctMngt
                       };
            if (invoices.Count() > 0)
            {

                //Group by Office, PartnerId, Service
                var grpInvoices = invoices.ToList()
                    .GroupBy(g => new { Office = g.Office, PartnerId = g.PartnerId, Service = g.Service }).Select(s => new ReceivableInvoices
                    {
                        Office = s.Key.Office,
                        PartnerId = s.Key.PartnerId,
                        Service = s.Key.Service,
                        Invoices = s.Select(se => se.Invoice).ToList()
                    });

                models.ForEach(fe =>
                {
                    var invs = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service).Select(se => se.Invoices.AsQueryable()).FirstOrDefault();
                    if (invs != null)
                    {
                    // Group By InvoiceID
                    IQueryable<AccAccountingManagement> invoiceQ = invs.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                        {
                            Id = s.Key.Id,
                            DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                            UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                            UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                            UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                            ServiceType = s.FirstOrDefault().ServiceType

                        });
                        fe.Over16To30Day = SumUnpaidAmountOfInvoices(invoiceQ, fe.ContractCurrency);
                    }

                });
            }
            else
            {
                models.ForEach(fe => fe.Over16To30Day = 0);
            }

            // Get công nợ quá hạn hơn 30 ngày
            var invoiceOver30 = accAccountings.Where(x => (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 30);
            invoices = from acctMngt in invoiceOver30
                       join surcharge in surcharges on acctMngt.Id equals surcharge.AcctManagementId
                       select new ReceivableInvoice
                       {
                           Office = surcharge.OfficeId,
                           PartnerId = surcharge.PaymentObjectId,
                           Service = surcharge.TransactionType,
                           Invoice = acctMngt
                       };
            if (invoices.Count() > 0)
            {

                //Group by Office, PartnerId, Service
                var grpInvoices = invoices.ToList()
                    .GroupBy(g => new { Office = g.Office, PartnerId = g.PartnerId, Service = g.Service }).Select(s => new ReceivableInvoices
                    {
                        Office = s.Key.Office,
                        PartnerId = s.Key.PartnerId,
                        Service = s.Key.Service,
                        Invoices = s.Select(se => se.Invoice).ToList()
                    });

                models.ForEach(fe =>
                {
                    var invs = grpInvoices.Where(x => x.Office == fe.Office && x.PartnerId == fe.PartnerId && x.Service == fe.Service).Select(se => se.Invoices.AsQueryable()).FirstOrDefault();
                    if (invs != null)
                    {
                    // Group By InvoiceID
                    IQueryable<AccAccountingManagement> invoiceQ = invs.GroupBy(g => new { g.Id }).Select(s => new AccAccountingManagement
                        {
                            Id = s.Key.Id,
                            DatetimeCreated = s.FirstOrDefault().DatetimeCreated,
                            UnpaidAmount = s.FirstOrDefault().UnpaidAmount,
                            UnpaidAmountVnd = s.FirstOrDefault().UnpaidAmountVnd,
                            UnpaidAmountUsd = s.FirstOrDefault().UnpaidAmountUsd,
                            ServiceType = s.FirstOrDefault().ServiceType

                        });
                        fe.Over30Day = SumUnpaidAmountOfInvoices(invoiceQ, fe.ContractCurrency);
                    }

                });
            }
            else
            {
                models.ForEach(fe => fe.Over30Day = 0);
            }
            return models;
        }
        #endregion
    }
}
