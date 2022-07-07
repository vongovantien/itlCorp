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

        readonly Guid? HM = Guid.Empty;
        readonly Guid? BH = Guid.Empty;

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

            HM = officeRepo.Get(x => x.Code == AccountingConstants.OFFICE_HM)?.FirstOrDefault()?.Id;
            BH = officeRepo.Get(x => x.Code == AccountingConstants.OFFICE_BH)?.FirstOrDefault()?.Id;
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
        private List<sp_GetBillingWithSalesman> GetDataBillingSalesman(string partner, Guid? officeId, string service, string type, string paymentStatus)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@partner", Value = partner},
                new SqlParameter(){ ParameterName = "@officeId", Value = officeId},
                new SqlParameter(){ ParameterName = "@service", Value = service},
                new SqlParameter(){ ParameterName = "@type", Value = type},
                new SqlParameter(){ ParameterName = "@paymentStatus", Value = paymentStatus},
                new SqlParameter(){ ParameterName = "@salesman", Value = null },
                new SqlParameter(){ ParameterName = "@overDue", Value = null },
            };
            var data = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetBillingWithSalesman>(parameters);
            return data;
        }
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
        private List<AccAccountReceivableModel> CalculatorObhAmount(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges)
        {
            //Get OBH charge by OBH Partner (PaymentObjectId)
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH && string.IsNullOrEmpty(x.ReferenceNo));

            IQueryable<OpsTransaction> opsJob = opsRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED
                                          && x.ServiceDate.Value.Date <= DateTime.Now.Date);

            IQueryable<CsTransaction> csJob = transactionRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED
                                           && x.ServiceDate.Value.Date <= DateTime.Now.Date);

            models.ForEach(fe =>
            {
                // var _charges = surcharges.Where(x => x.OfficeId == fe.Office && x.PaymentObjectId == fe.PartnerId && x.TransactionType == fe.Service);
                var _charges = Enumerable.Empty<CsShipmentSurcharge>().AsQueryable();

                if (fe.Service == "CL")
                {
                    _charges = from ops in opsJob
                               join sur in surcharges on ops.Hblid equals sur.Hblid into grpOps
                               from surGrp in grpOps.DefaultIfEmpty()
                               where surGrp.OfficeId == fe.Office && surGrp.PaymentObjectId == fe.PartnerId && surGrp.TransactionType == fe.Service
                               select surGrp;
                }
                else
                {
                    _charges = from cs in csJob
                               join csd in transactionDetailRepo.Get() on cs.Id equals csd.JobId
                               join sur in surcharges on csd.Id equals sur.Hblid into grpOps
                               from surGrp in grpOps.DefaultIfEmpty()
                               where surGrp.OfficeId == fe.Office && surGrp.PaymentObjectId == fe.PartnerId && surGrp.TransactionType == fe.Service
                               select surGrp;
                }
                decimal? obhAmount = 0;
                foreach (var charge in _charges)
                {
                    obhAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, fe.ContractCurrency);
                }
                fe.ObhAmount = obhAmount + fe.ObhUnpaid; //Cộng thêm OBH Unpaid thuộc Receivable (ObhUnpaid cần phải được tính toán trước)
            });

            return models;

        }
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
        private List<AccAccountReceivableModel> CalculatorSellingNoVat(List<AccAccountReceivableModel> models, IQueryable<CsShipmentSurcharge> charges)
        {
            //Lấy ra các phí thu (SELLING) chưa có Invoice
            var surcharges = charges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL
                                             && string.IsNullOrEmpty(x.InvoiceNo)
                                             && x.AcctManagementId == null);

            IQueryable<OpsTransaction> opsJob = opsRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED
                                           && x.ServiceDate.Value.Date <= DateTime.Now.Date);

            IQueryable<CsTransaction> csJob = transactionRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED
                                           && x.ServiceDate.Value.Date <= DateTime.Now.Date);
            models.ForEach(fe =>
            {

                // var _charges = surcharges.Where(x => x.OfficeId == fe.Office && x.PaymentObjectId == fe.PartnerId && x.TransactionType == fe.Service);
                var _charges = Enumerable.Empty<CsShipmentSurcharge>().AsQueryable();
                if (fe.Service == "CL")
                {
                    _charges = from ops in opsJob
                               join sur in surcharges on ops.Hblid equals sur.Hblid into grpOps
                               from surGrp in grpOps.DefaultIfEmpty()
                               where surGrp.OfficeId == fe.Office && surGrp.PaymentObjectId == fe.PartnerId && surGrp.TransactionType == fe.Service
                               select surGrp;
                }
                else
                {
                    _charges = from cs in csJob
                               join csd in transactionDetailRepo.Get() on cs.Id equals csd.JobId
                               join sur in surcharges on csd.Id equals sur.Hblid into grpOps
                               from surGrp in grpOps.DefaultIfEmpty()
                               where surGrp.OfficeId == fe.Office && surGrp.PaymentObjectId == fe.PartnerId && surGrp.TransactionType == fe.Service
                               select surGrp;
                }
                if (_charges.Count() > 0)
                {
                    decimal? sellingNoVat = 0;
                    foreach (var charge in _charges)
                    {
                        sellingNoVat += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, fe.ContractCurrency);
                    }
                    fe.SellingNoVat = sellingNoVat;
                }
            });

            return models;

        }
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
            var receivables = DataContext.Get(x => x.PartnerId == agreement.PartnerId
                                                && x.SaleMan == agreement.SaleManId
                                                && agreement.SaleService.Contains(x.Service)
                                                && agreement.OfficeId.Contains(x.Office.ToString()));
            return ModifiedAgreementWithReceivables(agreement, receivables);
        }

        private CatContract ModifiedAgreementWithReceivables(CatContract agreement, IQueryable<AccAccountReceivable> receivables)
        {
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
                    _creditRate = agreement.TrialCreditLimited == null ? 0 : (((agreement.DebitAmount ?? 0) - (agreement.CreditCurrency == AccountingConstants.CURRENCY_LOCAL ? (agreement.CustomerAdvanceAmountVnd ?? 0) : (agreement.CustomerAdvanceAmountUsd ?? 0))) / agreement.TrialCreditLimited) * 100; //((DebitAmount - CusAdv)/TrialCreditLimit)*100
                }
                if (agreement.ContractType == "Official")
                {
                    _creditRate = agreement.CreditLimit == null ? 0 : (((agreement.DebitAmount ?? 0) - (agreement.CreditCurrency == AccountingConstants.CURRENCY_LOCAL ? (agreement.CustomerAdvanceAmountVnd ?? 0) : (agreement.CustomerAdvanceAmountUsd ?? 0))) / agreement.CreditLimit) * 100; //((DebitAmount - CusAdv)/CreditLimit)*100
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

                agreement.IsOverDue = receivables.Any(x => !DataTypeEx.IsNullOrValue(x.Over30Day, 0)); // CR 17536
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
                    var contractPartner = contractPartnerRepo.Get(x => x.PartnerId == partnerId
                                                                    && agreementIds.Contains(x.Id));
                    if (contractPartner.Count() > 0)
                    {
                        foreach (var item in contractPartner)
                        {
                            var agreementPartner = CalculatorAgreement(item);
                            await contractPartnerRepo.UpdateAsync(item, x => x.Id == agreementPartner.Id);
                        }
                    }
                    else
                    {
                        //Agreement của AcRef của partner
                        var contractParent = contractPartnerRepo.Get(x => x.Active == true
                                                                       && x.PartnerId == partner.ParentId
                                                                       && agreementIds.Contains(x.Id)).FirstOrDefault();
                        foreach (var item in contractPartner)
                        {
                            var agreementPartner = CalculatorAgreement(item);
                            await contractPartnerRepo.UpdateAsync(item, x => x.Id == agreementPartner.Id);
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
            WriteLogInsertOrUpdateReceivable(hsInsertOrUpdate.Status, hsInsertOrUpdate.Message, receivables, model);

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
                WriteLogInsertOrUpdateReceivable(hsInsertOrUpdate.Status, hsInsertOrUpdate.Message, receivables, models);

                //Cập nhật giá trị công nợ vào Agreement của list Partner sau khi Insert or Update Receivable thành công
                var partnerIds = receivables.Select(s => s.PartnerId).ToList();
                var agreementIds = receivables.Select(s => s.ContractId).ToList();

                await UpdateAgreementPartnersAsync(partnerIds, agreementIds);

                return hs;
            }
            catch (Exception ex)
            {
                WriteLogInsertOrUpdateReceivable(false, ex.Message, receivables, models);
                return new HandleState((object)ex.Message);
            }
        }

        private List<AccAccountReceivableModel> CalculatorReceivableData(List<ObjectReceivableModel> models)
        {
            var receivables = GenerateListReceivableModelFromContract(models);

            List<AccAccountReceivableModel> newReceivableRecord = new List<AccAccountReceivableModel>();
            if (receivables.Count > 0)
            {
                var surcharges = surchargeRepo.Get(x => models.Any(a => a.Office == x.OfficeId && a.Service == x.TransactionType && a.PartnerId == x.PaymentObjectId));
                var invoices = accountingManagementRepo.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE || x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);

                receivables = CalculatorBillingAmount(receivables, surcharges, invoices); //Billing Amount, UnpaidAmount
                receivables = CalculatorPaidAmount(receivables, surcharges, invoices); //Paid Amount
                receivables = CalculatorObhBilling(receivables, surcharges, invoices); //Obh Billing
                receivables = CalculatorObhUnpaid(receivables, surcharges, invoices); //Obh Unpaid
                receivables = CalculatorObhPaid(receivables, surcharges, invoices); //Obh Paid
                receivables = CalculatorObhAmount(receivables, surcharges); //Obh Amount: Cộng thêm OBH Unpaid (đã cộng bên trong)
                receivables = CalculatorSellingNoVat(receivables, surcharges); //Selling No Vat

                // receivables = CalculatorOver1To15Day(receivables, surcharges, invoices); //Over 1 To 15 Day
                // receivables = CalculatorOver16To30Day(receivables, surcharges, invoices); //Over 16 To 30 Day
                // receivables = CalculatorOver30Day(receivables, surcharges, invoices); //Over 30 Day
            }
            receivables.ForEach(fe =>
            {
                //Calculator Debit Amount
                fe.DebitAmount = (fe.SellingNoVat ?? 0) + (fe.BillingUnpaid ?? 0) + (fe.ObhAmount ?? 0); // Công nợ chưa billing
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
                WriteLogInsertOrUpdateReceivable(hsInsertOrUpdate.Status, hsInsertOrUpdate.Message, receivables, models);

                //Cập nhật giá trị công nợ vào Agreement của list Partner sau khi Insert or Update Receivable thành công
                var partnerIds = receivables.Select(s => s.PartnerId).ToList();
                var agreementIds = receivables.Select(s => s.ContractId).ToList();

                UpdateAgreementPartners(partnerIds, agreementIds);

                return hs;
            }
            catch (Exception ex)
            {
                WriteLogInsertOrUpdateReceivable(false, ex.Message, receivables, models);
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
                                                                  .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Office = s.Key.Office, Service = s.Key.Service, }).ToList();
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

        private void WriteLogInsertOrUpdateReceivable(bool status, string message, List<AccAccountReceivableModel> receivables, List<ObjectReceivableModel> models = null)
        {
            string logMessage = string.Format("InsertOrUpdateReceivable by {0} at {1} \n ** models {2} \n ** Message: {3} \n ** Receivables: {4} \n\n---------------------------\n\n",
                            currentUser.Action,
                            DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                            models != null ? JsonConvert.SerializeObject(models) : "{}",
                            message,
                            receivables != null ? JsonConvert.SerializeObject(receivables) : "[]");
            string logName = string.Format("InsertOrUpdateReceivable_{0}_eFMS_LOG", (status ? "Success" : "Fail"));
            new LogHelper(logName, logMessage);
        }
        #endregion --- CRUD ---

        public List<ObjectReceivableModel> GetObjectReceivableBySurchargeId(List<Guid?> surchargeIds)
        {
            var surcharges = surchargeRepo.Get(x => surchargeIds.Any(a => a == x.Id));
            var data = GetObjectReceivableBySurcharges(surcharges);
            return data;
        }

        public List<ObjectReceivableModel> GetObjectReceivableBySurcharges(IQueryable<CsShipmentSurcharge> surcharges)
        {
            surcharges = surcharges.Where(x => x.OfficeId != HM && x.OfficeId != BH);
            if (surcharges.Count() == 0)
            {
                return new List<ObjectReceivableModel>();
            }

            var objectReceivables = surcharges.GroupBy(g => new { Service = g.TransactionType, PartnerId = g.PaymentObjectId, Office = g.OfficeId })
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
            var groupByContract = selectQuery.GroupBy(g => new { g.contract.Id, g.acctReceivable.Service, g.acctReceivable.Office, g.contract.OfficeId })
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
                                                                    (s.Select(se => se.acctReceivable != null ? se.acctReceivable.DebitAmount : null).Sum()) / (s.First().contract.TrialCreditLimited)
                                                                    : 0) * 100 ?? 0, 3) :
                                (s.First().contract.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL ?
                                                                Math.Round((
                                                                    s.First().contract.CreditLimit != 0 && s.First().contract.CreditLimit != null ?
                                                                    (s.Select(se => se.acctReceivable != null ? se.acctReceivable.DebitAmount : null).Sum()) / (s.First().contract.CreditLimit)
                                                                    : 0) * 100 ?? 0, 3) : 0),
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
                           ArCurrency = ar.ArCurrency,
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
                query = query.And(x => x.Office != null && criteria.OfficeIds.Contains(x.Office.ToString().ToLower()));
            //if (currentUser!= null)
            //    query = query.And(x => x.OfficeId == currentUser.OfficeID );
            if (criteria.Staffs != null && criteria.Staffs.Count > 0)
                query = query.And(x => criteria.Staffs.Contains(x.SaleMan));
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

                //IQueryable<AccountReceivableResult> arPartnerNoContracts = GetARNoContract(acctReceivables, partnerContracts, partners);
                //if (arPartnerNoContracts != null)
                //    arPartnerContracts = arPartnerContracts.Concat(arPartnerNoContracts).OrderByDescending(x => x.DatetimeModified);
            }

            var res = new List<AccountReceivableResult>();
            res = arPartnerContracts.ToList();

            res = res.Where(x => x.DebitAmount > 0).ToList();

            if (criteria.DebitRateTo != null && criteria.DebitRateFrom != null)
                res = res.Where(x => x.DebitRate >= criteria.DebitRateFrom && x.DebitRate <= criteria.DebitRateTo).ToList();
            if (criteria.AgreementStatus != null && criteria.AgreementStatus != "All")
                res = res.Where(x => x.AgreementStatus == criteria.AgreementStatus).ToList();
            if (criteria.AgreementExpiredDay != null && criteria.AgreementExpiredDay != "All")
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

            return res.AsQueryable().OrderByDescending(x => x.DebitRate);
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
                        ObhBillingAmount = s.Sum(sum => sum.ObhBillingAmount),
                        ObhPaidAmount = s.Sum(sum => sum.ObhPaidAmount),
                        ObhUnPaidAmount = s.Sum(sum => sum.ObhUnPaidAmount),
                        DatetimeModified = s.First().DatetimeModified,
                        OfficeContract = s.First().OfficeContract,
                        IsExpired = s.First().IsExpired,
                        IsOverLimit = s.First().IsOverLimit,
                        IsOverDue = s.First().IsOverDue,
                        ArOfficeIds = s.Select(x => x.OfficeId).Distinct().ToList(),
                        ArServices = s.Select(x => x.ArServiceCode).Distinct().ToList(),
                    }).OrderByDescending(s => s.DebitRate).AsQueryable();
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
            else if (arType == TermData.AR_NoAgreement)
            {
                data = GetDataNoAgreement(criteria);
            }
            return data;
        }

        private IEnumerable<object> GetDataNoAgreement(AccountReceivableCriteria criteria)
        {
            var queryAcctReceivable = ExpressionAcctReceivableQuery(criteria);
            var acctReceivables = DataContext.Get(queryAcctReceivable).Where(x => x.ContractId == null && x.DebitAmount > 0);
            var partners = QueryPartner(criteria);

            var partnerContractsAll = contractPartnerRepo.Get(x => x.ContractType != AccountingConstants.ARGEEMENT_TYPE_CASH);
            IQueryable<AccountReceivableResult> arPartnerNoContracts = GetARNoAgreement(acctReceivables, partnerContractsAll, partners);
            if (arPartnerNoContracts != null)
                arPartnerNoContracts = arPartnerNoContracts.Where(x => x.DebitAmount > 0);
            return arPartnerNoContracts;
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
                    Services = se.Select(sel => sel.ArServiceCode).Distinct().ToList(),
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
                           AccountReceivableGrpServices = ar.AccountReceivableGrpServices,
                           Services = ar.Services
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

            var contractType = argeement.ContractType;
            var _currenyContract = argeement.CreditCurrency;
            var _cusAdvVnd = argeement.CustomerAdvanceAmountVnd ?? 0;
            var _cusAdvUsd = argeement.CustomerAdvanceAmountUsd ?? 0;
            var _creditLimit = contractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL ? argeement.CreditLimit : argeement.TrialCreditLimited;
            decimal cusAdvRate = 0;
            if (contractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL)
            {
                cusAdvRate = (_currenyContract == AccountingConstants.CURRENCY_LOCAL ? (_cusAdvVnd / _creditLimit) * 100 : (_cusAdvUsd / _creditLimit) * 100) ?? 0;
            }
            if (contractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL)
            {
                cusAdvRate = (_currenyContract == AccountingConstants.CURRENCY_LOCAL ? (_cusAdvVnd / _creditLimit) * 100 : (_cusAdvUsd / _creditLimit) * 100) ?? 0;

            }
            cusAdvRate = Math.Round(cusAdvRate, 3);

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
                DebitRate = s.Sum(sum => sum.DebitRate) - cusAdvRate,
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

        public AccountReceivableDetailResult GetDetailAccountReceivableByPartnerId(string partnerId, string saleManId)
        {
            if (string.IsNullOrEmpty(partnerId)) return null;

            var acctReceivables = DataContext.Get(x => x.PartnerId == partnerId && x.ContractId == null && x.Office != null && x.SaleMan == saleManId);
            var partners = partnerRepo.Get(x => x.Id == partnerId);

            var groupByPartner = acctReceivables.GroupBy(g => new { g.AcRef, g.Office, g.Service,g.SaleMan })
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
                    ObhBillingAmount = s.Select(se => se.ObhBilling).Sum(),
                    ObhPaidAmount = s.Select(se => se.ObhPaid).Sum(),
                    ObhUnPaidAmount = s.Select(se => se.ObhUnpaid).Sum(),
                    ArSalesmanId = s.Key.SaleMan
                });
            var users = userRepo.Get();

            var arPartnerNoContracts = from ar in groupByPartner
                                       join partner in partners on ar.PartnerId equals partner.Id
                                       join u in users on ar.ArSalesmanId equals u.Id
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
                                           ArCurrency = ar.ArCurrency,
                                           ObhBillingAmount = ar.ObhBillingAmount,
                                           ObhPaidAmount = ar.ObhPaidAmount,
                                           ObhUnPaidAmount = ar.ObhUnPaidAmount,
                                           ArSalesmanId = ar.ArSalesmanId,
                                           ArSalesmanName = u.Username
                                       };

            var detail = new AccountReceivableDetailResult();
            var arPartners = arPartnerNoContracts.Where(x => x.PartnerId == partnerId);
            detail.AccountReceivable = arPartners.ToList().GroupBy(g => new { g.PartnerId, g.ArServiceCode, g.OfficeId,g.ArSalesmanId }).Select(s => new AccountReceivableResult
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
                ArCurrency = s.Select(se => se.ArCurrency).FirstOrDefault(),
                ArSalesmanId = s.Key.ArSalesmanId,
                ArSalesmanName = s.Select(x => x.ArSalesmanName).FirstOrDefault()
            }).FirstOrDefault();
            detail.AccountReceivableGrpOffices = GetARGroupOffice(arPartners);
            return detail;
        }

        public IEnumerable<object> GetDataARSumaryExport(AccountReceivableCriteria criteria)
        {
            IEnumerable<object> data = GetDataARByCriteria(criteria);
            return data;
        }

        public List<sp_GetBillingWithSalesman> GetDataDebitDetail(AcctReceivableDebitDetailCriteria model)
        {
            // if (model.ArgeementId == null || model.ArgeementId == Guid.Empty) return null;
            //DbParameter[] parameters =
            //{
            //    SqlParam.GetParameter("argid", model.ArgeementId),
            //    SqlParam.GetParameter("option", model.Option),
            //    SqlParam.GetParameter("officeId",!string.IsNullOrEmpty(model.OfficeId)?model.OfficeId:""),
            //    SqlParam.GetParameter("serviceCode",!string.IsNullOrEmpty(model.ServiceCode)?model.ServiceCode:""),
            //    SqlParam.GetParameter("overDueDay",model.OverDueDay)
            //};

            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@partner", Value = model.PartnerId },
                new SqlParameter(){ ParameterName = "@officeId", Value = model.OfficeId },
                new SqlParameter(){ ParameterName = "@service", Value = model.Service },
                new SqlParameter(){ ParameterName = "@type", Value = model.Type },
                new SqlParameter(){ ParameterName = "@paymentStatus", Value = model.PaymentStatus },
                new SqlParameter(){ ParameterName = "@salesman", Value = model.Salesman },
                new SqlParameter(){ ParameterName = "@overDue", Value = model.OverDue },
            };
            var data = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetBillingWithSalesman>(parameters);
            return data;
        }
        #endregion --- DETAIL ---

        //private bool checkingPaidLoop2(Guid acctManagementID)
        //{
        //    if (acctManagementID == null || accountingManagementRepo.Get(x => x.Id == acctManagementID).FirstOrDefault().PaymentStatus != "Paid") return true;
        //    return false;
        //}

        public DebitAmountDetail GetDebitAmountDetailByContract(AccAccountReceivableCriteria criteria)
        {
            DebitAmountDetail debitAmountDetail = new DebitAmountDetail();
            var contract = contractPartnerRepo.Get(x => x.Id == criteria.AgreementId).FirstOrDefault();
            var partner = partnerRepo.Get(x => x.Id == criteria.PartnerId.ToString()).FirstOrDefault();
            debitAmountDetail.DebitAmountGeneralInfo = new DebitAmountGeneralInfo
            {
                ContractNo = contract.ContractNo,
                EffectiveDate = contract.EffectiveDate,
                ContracType = contract.ContractType,
                ExpiredDate = contract.ExpiredDate,
                PartnerCode = partner.TaxCode,
                PartnerName = partner.ShortName,
                Currency = contract.CurrencyId
            };
            debitAmountDetail.DebitAmountDetails = GetDebitAmountDetailbyPartnerId(criteria.AgreementId, criteria.PartnerId, criteria.AgreementSalesmanId).ToList();
            //debitAmountDetail.DebitAmountDetails.ForEach(x =>
            //{
            //    if (!checkingPaidLoop2(x.AcctManagementID)) debitAmountDetail.DebitAmountDetails.Remove(x);
            //});
            //for (int i = 0; i < debitAmountDetail.DebitAmountDetails.Count(); i++)
            //{
            //    if (debitAmountDetail.DebitAmountDetails[i].AcctManagementID == Guid.Empty) continue;
            //    //debitAmountDetail.DebitAmountDetails[i].PaymentStatus = debitAmountDetail.DebitAmountDetails[i].PaymentStatus == null ? "Unpaid" : debitAmountDetail.DebitAmountDetails[i].PaymentStatus;
            //    if (!checkingPaidLoop2(debitAmountDetail.DebitAmountDetails[i].AcctManagementID))
            //    {
            //        debitAmountDetail.DebitAmountDetails.RemoveAt(i);
            //        i--;
            //    }
            //}

            //debitAmountDetail.DebitAmountDetails.ToList().ForEach(x =>
            //{
            //    if (x.InvoiceNo == null && x.ServiceDate > DateTime.Now)
            //    {
            //        debitAmountDetail.DebitAmountDetails.Remove(x);
            //    }
            //});

            return debitAmountDetail;
        }

        public List<sp_GetDebitAmountDetailByContract> GetDebitAmountDetailbyPartnerId(Guid argeementId, Guid partnerID, Guid salemanID)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("partnerID", partnerID),
                SqlParam.GetParameter("argeementId", argeementId),
                SqlParam.GetParameter("salemanId", salemanID)
            };
            var data = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDebitAmountDetailByContract>(parameters);
            return data;
        }

        #region -- Update AcctManagement and overdays AccountReceivable after change payment term contract

        /// <summary>
        /// Update due date invoice và công nợ quá hạn sau khi update HĐ
        /// </summary>
        /// <param name="contractModel"></param>
        /// <returns></returns>
        public async Task<HandleState> UpdateDueDateAndOverDaysAfterChangePaymentTerm(CatContractModel contractModel)
        {
            var listInvoices = new List<AccAccountingManagement>();
            contractModel.SaleService = contractModel.SaleService.ToLower();
            contractModel.OfficeId = contractModel.OfficeId.ToLower();

            var hs = UpdateDueDateAcctManagement(contractModel, out listInvoices);
          
            return hs;
        }

        /// <summary>
        /// Update payment due date invoices
        /// </summary>
        /// <param name="contractModel"></param>
        /// <param name="listInvoices"></param>
        /// <returns></returns>
        public HandleState UpdateDueDateAcctManagement(CatContractModel contractModel, out List<AccAccountingManagement> listInvoices)
        {
            using (var trans = accountingManagementRepo.DC.Database.BeginTransaction())
            {
                HandleState hs = new HandleState();
                listInvoices = new List<AccAccountingManagement>();
                try
                {
                    currentUser.Action = "UpdateDueDateAcctMngAfterChangePaymentTerm";
                    var invoiceData = accountingManagementRepo.Get().Where(x => x.PartnerId == contractModel.PartnerId &&
                                             (!string.IsNullOrEmpty(x.ServiceType) && contractModel.SaleService.Contains(x.ServiceType.ToLower())) &&
                                            (x.OfficeId != null && contractModel.OfficeId.Contains(x.OfficeId.ToString().ToLower())) &&
                                            x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID).ToList();
                    var acctOBH = accountingManagementRepo.Get().Where(x => x.PartnerId == contractModel.PartnerId && contractModel.SaleService.Contains(x.ServiceType.ToLower()) &&
                                             contractModel.OfficeId.ToLower().Contains(x.OfficeId.ToString().ToLower()) &&
                                             x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);
                    if (acctOBH.Count() > 0)
                    {
                        var surcharges = surchargeRepo.Get(x => x.AcctManagementId != null && x.PaymentObjectId == contractModel.PartnerId);
                        var invOBHsurcharges = from acc in acctOBH
                                               join surcharge in surcharges on acc.Id equals surcharge.AcctManagementId
                                               select new
                                               {
                                                   acc,
                                                   BillingNo = string.IsNullOrEmpty(surcharge.Soano) ? surcharge.DebitNo : surcharge.Soano
                                               };
                        var invOBHGrp = invOBHsurcharges.GroupBy(x => x.BillingNo);
                        foreach(var item in invOBHGrp)
                        {
                            if(item.Any(x=>x.acc.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID))
                            {
                                invoiceData.AddRange(item.Select(x => x.acc));
                            }
                        }
                    }
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
                        hs = accountingManagementRepo.SubmitChanges();
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
        #endregion
        public HandleState CalculatorReceivableOverDue1To15Day(List<string> partnerIds, out List<Guid?> contractIds)
        {
            currentUser.Action = "CalculateOverDue";
            currentUser.UserID = "System";
            HandleState hs = CalculatorReceivableOverDue(partnerIds, 1, out List<Guid?> contractIdsNeedUpdate);
            contractIds = contractIdsNeedUpdate;
            return hs;
        }

        public HandleState CalculatorReceivableOverDue15To30Day(List<string> partnerIds, out List<Guid?> contractIds)
        {
            currentUser.Action = "CalculateOverDue";
            currentUser.UserID = "System";
            HandleState hs = CalculatorReceivableOverDue(partnerIds, 2, out List<Guid?> contractIdsNeedUpdate);
            contractIds = contractIdsNeedUpdate;
            return hs;
        }
        public HandleState CalculatorReceivableOverDue30Day(List<string> partnerIds, out List<Guid?> contractIds)
        {
            currentUser.Action = "CalculateOverDue";
            currentUser.UserID = "System";
            HandleState hs = CalculatorReceivableOverDue(partnerIds, 3, out List<Guid?> contractIdsNeedUpdate);
            contractIds = contractIdsNeedUpdate;
            return hs;
        }

        private IQueryable<AccAccountingManagement> GetOverDuePartnerWithType(List<string> partnerIds, int type)
        {
            var invoiceOverDue = Enumerable.Empty<AccAccountingManagement>().AsQueryable();
            Expression<Func<AccAccountingManagement, bool>> query = x => (x.Type != AccountingConstants.ACCOUNTING_VOUCHER_TYPE
                                                      && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID
                                                      && x.PaymentDueDate != null);
            if (partnerIds.Count() > 0)
            {
                query = query.And(x => partnerIds.Contains(x.PartnerId));
            }
            invoiceOverDue = accountingManagementRepo.Get(query);

            switch (type)
            {
                case 1: // 1 - 15
                    invoiceOverDue = invoiceOverDue.Where(x => x.PaymentDueDate != null && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 16 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 0);
                    break;
                case 2: // 15 - 30
                    invoiceOverDue = invoiceOverDue.Where(x => x.PaymentDueDate != null && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days <= 30 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 15);
                    break;
                case 3: // 30
                    invoiceOverDue = invoiceOverDue.Where(x => (DateTime.Now.Date - x.PaymentDueDate.Value.Date).TotalDays > 30);
                    break;
                default:
                    break;
            }

            return invoiceOverDue;
        }

        private HandleState CalculatorReceivableOverDue(List<string> partnerIds, int type, out List<Guid?> contractIds)
        {
            HandleState hs = new HandleState();
            contractIds = new List<Guid?>();

            var invoiceOverDue = Enumerable.Empty<GetArBBillingWithSalesman>().AsQueryable();
            var surcharges = Enumerable.Empty<CsShipmentSurcharge>().AsQueryable();
            string overDueParam = string.Empty;
            var receivables = Enumerable.Empty<AccAccountReceivable>().AsQueryable();
            try
            {
                if (partnerIds.Count > 0)
                {
                    receivables = DataContext.Get(x => partnerIds.Contains(x.PartnerId));
                    if (receivables.Count() > 0)
                    {
                        switch (type)
                        {
                            case 1:
                                foreach (var item in receivables)
                                {
                                    item.Over1To15Day = 0;
                                    DataContext.Update(item, x => x.Id == item.Id, false);
                                }

                                break;
                            case 2:
                                foreach (var item in receivables)
                                {
                                    item.Over16To30Day = 0;
                                    DataContext.Update(item, x => x.Id == item.Id, false);
                                }
                                break;
                            case 3:
                                foreach (var item in receivables)
                                {
                                    item.Over30Day = 0;
                                    DataContext.Update(item, x => x.Id == item.Id, false);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    Expression<Func<AccAccountReceivable, bool>> query = q => q.PartnerId != null && q.Service != null && q.Office != null;
                    receivables = DataContext.Get(query);
                }
                switch (type)
                {
                    case 1: // 1 - 15
                        overDueParam = "Over1To15";
                        break;
                    case 2: // 15 - 30
                        overDueParam = "Over15To30";
                        break;
                    case 3: // 30
                        overDueParam = "Over30";
                        break;
                    default:
                        overDueParam = null;
                        break;
                }
                var parameters = new[]{
                new SqlParameter(){ ParameterName = "@partner", Value = partnerIds.Count > 0 ? string.Join("|", partnerIds) : null },
                new SqlParameter(){ ParameterName = "@officeId", Value = null },
                new SqlParameter(){ ParameterName = "@service", Value = null },
                new SqlParameter(){ ParameterName = "@type", Value = null },
                new SqlParameter(){ ParameterName = "@paymentStatus", Value = null },
                new SqlParameter(){ ParameterName = "@salesman", Value = null },
                new SqlParameter(){ ParameterName = "@overDue", Value = overDueParam },
            };
                var invoiceOverDueWithSalesman = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetBillingWithSalesman>(parameters);
                if (invoiceOverDueWithSalesman.Count > 0)
                {
                    var invoiceOverDueWithSalesmanGrp = invoiceOverDueWithSalesman.GroupBy(x => new { x.SalesmanId, x.OfficeId, x.PartnerId, x.Service })
                        .Select(x => new { x.Key.SalesmanId, x.Key.PartnerId, x.Key.OfficeId, x.Key.Service, invoices = x });
                    if (invoiceOverDueWithSalesmanGrp.Count() > 0)
                    {
                        foreach (var item in invoiceOverDueWithSalesmanGrp)
                        {
                            var partner = partnerRepo.Get(x => x.Id == item.PartnerId).FirstOrDefault();

                            var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId && x.Office == item.OfficeId && x.Service == item.Service && x.SaleMan == item.SalesmanId);
                            if (currentReceivable != null)
                            {
                                // kiểm tra hop dong hien tai cua sales
                                var currentContract = contractPartnerRepo.First(x => x.PartnerId == item.PartnerId
                                 && x.SaleManId == item.SalesmanId
                                 && x.Active == true
                                 && x.OfficeId.Contains(item.OfficeId.ToString())
                                 && x.SaleService.Contains(item.Service));

                                if (currentContract != null)
                                {
                                    currentReceivable.ContractId = currentContract.Id;
                                    currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                                }

                                decimal? totalAmountUnpaid = 0;
                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalAmountUnpaid += invoice.UnpaidAmountVND;
                                    }
                                    else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalAmountUnpaid += invoice.UnpaidAmountUSD;
                                    }
                                }
                                switch (type)
                                {
                                    case 1: // 1 - 15
                                        currentReceivable.Over1To15Day = totalAmountUnpaid;
                                        DataContext.Update(currentReceivable, x => x.Id == currentReceivable.Id, false);
                                        break;
                                    case 2: // 15 - 30
                                        currentReceivable.Over16To30Day = totalAmountUnpaid;
                                        DataContext.Update(currentReceivable, x => x.Id == currentReceivable.Id, false);
                                        break;
                                    case 3: // 30
                                        currentReceivable.Over30Day = totalAmountUnpaid;
                                        DataContext.Update(currentReceivable, x => x.Id == currentReceivable.Id, false);
                                        break;
                                    default:
                                        break;
                                }
                                if (currentReceivable.ContractId != null)
                                {
                                    contractIds.Add(currentReceivable.ContractId);
                                }
                            }
                            else
                            {
                                var contract = contractPartnerRepo.First(x => x.PartnerId == item.PartnerId
                                && x.SaleManId == item.SalesmanId
                                && x.Active == true
                                && x.OfficeId.Contains(item.OfficeId.ToString())
                                && x.SaleService.Contains(item.Service));

                                if (contract != null)
                                {
                                    decimal? totalAmountUnpaid = 0;
                                    var invoicesData = item.invoices;

                                    foreach (var invoice in invoicesData)
                                    {
                                        if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                        {
                                            totalAmountUnpaid += invoice.UnpaidAmountVND;
                                        }
                                        else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                        {
                                            totalAmountUnpaid += invoice.UnpaidAmountUSD;
                                        }
                                    }

                                    var newAr = new AccAccountReceivableModel
                                    {
                                        Id = Guid.NewGuid(),
                                        PartnerId = item.PartnerId,
                                        Office = item.OfficeId,
                                        ContractCurrency = contract.CreditCurrency,
                                        Service = item.Service,
                                        AcRef = partner.ParentId ?? partner.Id,
                                        ContractId = contract.Id,
                                        SaleMan = item.SalesmanId,
                                        UserCreated = contract.UserCreated,
                                        UserModified = contract.UserModified,
                                        OfficeId = null,
                                        CompanyId = contract.CompanyId,
                                    };
                                    switch (type)
                                    {
                                        case 1:
                                            newAr.Over1To15Day = totalAmountUnpaid;
                                            DataContext.Add(newAr, false);
                                            break;
                                        case 2:
                                            newAr.Over16To30Day = totalAmountUnpaid;
                                            DataContext.Add(newAr, false);
                                            break;
                                        case 3:
                                            newAr.Over30Day = totalAmountUnpaid;
                                            DataContext.Add(newAr, false);
                                            break;
                                        default:
                                            break;
                                    }

                                    contractIds.Add(newAr.ContractId);
                                }
                                else
                                {
                                    decimal? totalAmountUnpaid = 0;
                                    var invoicesData = item.invoices;

                                    foreach (var invoice in invoicesData)
                                    {
                                        totalAmountUnpaid += invoice.UnpaidAmountVND;
                                    }
                                    var newAr = new AccAccountReceivableModel
                                    {
                                        Id = Guid.NewGuid(),
                                        PartnerId = item.PartnerId,
                                        Office = item.OfficeId,
                                        Service = item.Service,
                                        AcRef = partner.ParentId ?? partner.Id,
                                        ContractId = null,
                                        SaleMan = item.SalesmanId,
                                        ContractCurrency = "VND",
                                    };
                                    switch (type)
                                    {
                                        case 1:
                                            newAr.Over1To15Day = totalAmountUnpaid;
                                            DataContext.Add(newAr, false);
                                            break;
                                        case 2:
                                            newAr.Over16To30Day = totalAmountUnpaid;
                                            DataContext.Add(newAr, false);
                                            break;
                                        case 3:
                                            newAr.Over30Day = totalAmountUnpaid;
                                            DataContext.Add(newAr, false);
                                            break;
                                        default:
                                            break;
                                    }
                                }

                            }
                        }
                    }
                }
                else
                {
                   
                    Expression<Func<AccAccountReceivable, bool>> query = q => q.PartnerId != null && q.Service != null && q.Office != null;
                    switch (type)
                    {
                        case 1: // 1 - 15
                            query = query.And(x => !DataTypeEx.IsNullOrValue(x.Over1To15Day, 0));
                            break;
                        case 2: // 15 - 30
                            query = query.And(x => !DataTypeEx.IsNullOrValue(x.Over16To30Day, 0));
                            break;
                        case 3: // 30
                            query = query.And(x => !DataTypeEx.IsNullOrValue(x.Over30Day, 0));
                            break;
                        default:
                            overDueParam = null;
                            break;
                    }
                    if (partnerIds.Count > 0)
                    {
                        query = query.And(x => partnerIds.Contains(x.PartnerId));
                    }
                    var receivablesNeedReset = DataContext.Get(query);
                    switch (type)
                    {
                        case 1: // 1 - 15
                            foreach (var ar in receivablesNeedReset)
                            {
                                ar.Over1To15Day = 0;
                                DataContext.Update(ar, x => x.Id == ar.Id, false);
                            }
                            break;
                        case 2: // 15 - 30
                            foreach (var ar in receivablesNeedReset)
                            {
                                ar.Over16To30Day = 0;
                                DataContext.Update(ar, x => x.Id == ar.Id, false);
                            }
                            break;
                        case 3: // 30
                            foreach (var ar in receivablesNeedReset)
                            {
                                ar.Over30Day = 0;
                                DataContext.Update(ar, x => x.Id == ar.Id, false);
                            }
                            break;
                        default:
                            break;
                    }

                    contractIds = receivables.Where(x => x.ContractId != null).Select(x => x.ContractId).Distinct().ToList();
                }

                hs = DataContext.SubmitChanges();
                contractIds = contractIds.Distinct().ToList();

                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_CalculateOverDueLog", ex.ToString());
                return new HandleState(ex.Message);
            }
           
        }

        public async Task<HandleState> CalculatorReceivableDebitAmountAsync(List<ObjectReceivableModel> models)
        {
            var receivables = new List<AccAccountReceivableModel>();
            HandleState hs = new HandleState();

            try
            {
                receivables = CalculatorReceivableDataDebitAmount(models);

                var receivablesModel = mapper.Map<List<ReceivableTable>>(receivables);
                var hsInsertOrUpdate = InsertOrUpdateReceivableList(receivablesModel);
                if (!hsInsertOrUpdate.Status)
                {
                    hs = new HandleState((object)hsInsertOrUpdate.Message);
                }
                WriteLogInsertOrUpdateReceivable(hsInsertOrUpdate.Status, hsInsertOrUpdate.Message, receivables, models);

                var partnerIds = receivables.Select(s => s.PartnerId).Distinct().ToList();
                var agreementIds = receivables.Select(s => s.ContractId).ToList();

                await UpdateAgreementPartnersAsync(partnerIds, agreementIds);

                return hs;
            }
            catch (Exception ex)
            {
                WriteLogInsertOrUpdateReceivable(false, ex.Message, receivables, models);
                return new HandleState((object)ex.Message);
            }
        }

        public async Task<HandleState> CalculateAgreementFlag(List<Guid?> contractIds, string flag)
        {
            HandleState hs = new HandleState();
            if (contractIds.Count > 0)
            {
                foreach (var Id in contractIds)
                {
                    var contract = contractPartnerRepo.Get(x => x.Id == Id)?.FirstOrDefault();
                    if (contract != null)
                    {
                        var receivables = DataContext.Get(x => x.ContractId == Id);

                        if (flag == "overdue")
                        {
                            contract.IsOverDue = receivables.Any(x => !DataTypeEx.IsNullOrValue(x.Over30Day, 0));
                        }

                        hs = await contractPartnerRepo.UpdateAsync(contract, x => x.Id == Id, false);
                    }
                }

                hs = contractPartnerRepo.SubmitChanges();
            }

            return hs;
        }

        List<AccAccountReceivableModel> GenerateListReceivableModelFromContract(List<ObjectReceivableModel> models)
        {
            var receivables = new List<AccAccountReceivableModel>();
            foreach (var model in models)
            {
                var partner = partnerRepo.Get(x => x.Id == model.PartnerId).FirstOrDefault();
                //Không tính công nợ cho đối tượng Internal
                if (partner != null && partner.PartnerMode != "Internal")
                {
                    var contracts = contractPartnerRepo.Get(x => x.PartnerId == model.PartnerId);
                    if (contracts.Count() > 0)
                    {
                        foreach (var contract in contracts)
                        {
                            AccAccountReceivableModel receivable = new AccAccountReceivableModel();
                            receivable.Over30Day = 0;
                            receivable.Over1To15Day = 0;
                            receivable.Over16To30Day = 0;
                            receivable.ObhAmount = 0;
                            receivable.ObhBilling = 0;
                            receivable.ObhPaid = 0;
                            receivable.ObhUnpaid = 0;
                            receivable.DebitAmount = 0;
                            receivable.PartnerId = model.PartnerId;
                            receivable.Office = model.Office;
                            receivable.Service = model.Service;
                            receivable.AcRef = partner.ParentId ?? partner.Id;

                            if (contract.Active == true && contract.OfficeId.Contains(model.Office.ToString()) == true
                                && contract.SaleService.Contains(model.Service) == true)
                            {

                                // Lấy currency của contract & user created of contract gán cho Receivable
                                receivable.ContractId = contract.Id;
                                receivable.ContractCurrency = contract.CreditCurrency;
                                receivable.SaleMan = contract.SaleManId;
                                receivable.UserCreated = contract.UserCreated;
                                receivable.UserModified = contract.UserCreated;
                                receivable.GroupId = null;
                                receivable.DepartmentId = null;
                                receivable.OfficeId = model.Office;
                                receivable.CompanyId = contract.CompanyId;
                            }
                            else
                            {
                                // Lấy currency local và use created of partner gán cho Receivable
                                receivable.ContractId = null;
                                receivable.ContractCurrency = AccountingConstants.CURRENCY_LOCAL;
                                receivable.SaleMan = contract.SaleManId;
                                receivable.UserCreated = partner.UserCreated;
                                receivable.UserModified = partner.UserCreated;
                                receivable.GroupId = partner.GroupId;
                                receivable.DepartmentId = partner.DepartmentId;
                                receivable.OfficeId = partner.OfficeId;
                                receivable.CompanyId = partner.CompanyId;
                            }
                            receivables.Add(receivable);
                        }
                    }
                }
            }

            return receivables;
        }

        private List<AccAccountReceivableModel> CalculatorReceivableDataDebitAmount(List<ObjectReceivableModel> models)
        {
            var receivables = new List<AccAccountReceivableModel>();
            var surchargesDb = surchargeRepo.Get(x => models.Any(a => a.Office == x.OfficeId && a.Service == x.TransactionType && a.PartnerId == x.PaymentObjectId));
           
            foreach (var fe in models)
            {
                var partner = partnerRepo.Get(x => x.Id == fe.PartnerId).FirstOrDefault();
                if(partner == null || partner.PartnerMode == "Internal")
                {
                    continue;
                }
                var surcharges = surchargesDb.Where(x => (x.Type == AccountingConstants.TYPE_CHARGE_SELL)
                                                 && string.IsNullOrEmpty(x.InvoiceNo)
                                                 && x.AcctManagementId == null);
                var surchargesOBH = surchargesDb.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH && x.AcctManagementId == null);

                IQueryable<OpsTransaction> opsJob = opsRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED
                                               && x.ServiceDate.Value.Date <= DateTime.Now.Date);
                IQueryable<CsTransaction> csJob = transactionRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED
                                               && x.ServiceDate.Value.Date <= DateTime.Now.Date);

                var currentReceivables = DataContext.Get(x => x.PartnerId == fe.PartnerId && x.Office == fe.Office && x.Service == fe.Service).ToList();
                foreach (var item in currentReceivables)
                {
                    item.SellingNoVat = 0;
                    item.ObhAmount = 0;
                    item.BillingUnpaid = 0;
                    item.ObhUnpaid = 0;
                    item.BillingAmount = 0;
                    item.ObhBilling = 0;
                    item.PaidAmount = 0;
                    item.ObhPaid = 0;
                    if(item.ContractId != null)
                    {
                        var contractC = contractPartnerRepo.First(x => x.PartnerId == item.PartnerId && x.Id == item.ContractId && x.SaleManId == item.SaleMan);
                        if(contractC == null || contractC.Active == false)
                        {
                            item.ContractId = null; // những khách cũ k đi hàng, hd đang inactive, cn vẫn tính có contract.
                        }
                    }
                }
                receivables.AddRange(mapper.Map<List<AccAccountReceivableModel>>(currentReceivables));
                var receivableIdModified = new List<Guid>();

                #region SellingNoVat
                IQueryable<SalesmanSurcharge> _chargeWithSalemanSell = Enumerable.Empty<SalesmanSurcharge>().AsQueryable();
                if (fe.Service == "CL")
                {
                    _chargeWithSalemanSell = from ops in opsJob
                                         join sur in surcharges on ops.Hblid equals sur.Hblid into grpOps
                                         from surGrp in grpOps.DefaultIfEmpty()
                                         where surGrp.OfficeId == fe.Office
                                         && surGrp.PaymentObjectId == fe.PartnerId
                                         && surGrp.TransactionType == fe.Service
                                         select new SalesmanSurcharge
                                         {
                                             Surcharge = surGrp,
                                             Salesman = ops.SalemanId,
                                             Office = surGrp.OfficeId,
                                             PartnerId = surGrp.PaymentObjectId,
                                             TransactionType = surGrp.TransactionType
                                         };
                }
                else
                {
                    _chargeWithSalemanSell = from cs in csJob
                                         join csd in transactionDetailRepo.Get() on cs.Id equals csd.JobId
                                         join sur in surcharges on csd.Id equals sur.Hblid into grpOps
                                         from surGrp in grpOps.DefaultIfEmpty()
                                         where surGrp.OfficeId == fe.Office
                                         && surGrp.PaymentObjectId == fe.PartnerId
                                         && surGrp.TransactionType == fe.Service
                                         select new SalesmanSurcharge
                                         {
                                             Surcharge = surGrp,
                                             Salesman = csd.SaleManId,
                                             Office = surGrp.OfficeId,
                                             PartnerId = surGrp.PaymentObjectId,
                                             TransactionType = surGrp.TransactionType
                                         };
                }
                if (_chargeWithSalemanSell.Count() > 0)
                {
                    var _chargeGrpWithSalesman = _chargeWithSalemanSell
                   .GroupBy(x => new { x.PartnerId, x.Office, x.TransactionType, x.Salesman })
                   .Select(x => new { x.Key.Salesman, Surcharges = x.Select(i => i.Surcharge), x.Key.PartnerId, x.Key.Office, Service = x.Key.TransactionType });

                    foreach (var item in _chargeGrpWithSalesman)
                    {
                        var surchargeInGrp = item.Surcharges;
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                        && x.Office == item.Office && x.Service == item.Service && x.SaleMan == item.Salesman);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.Salesman
                             && x.Active == true
                             && x.OfficeId.Contains(item.Office.ToString())
                             && x.SaleService.Contains(item.Service));
                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            } 
                            decimal? totalAmount = 0;
                            foreach (var charge in surchargeInGrp)
                            {
                                totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, currentReceivable.ContractCurrency);
                            }
                            currentReceivable.SellingNoVat = totalAmount;
                            receivableIdModified.Add(currentReceivable.Id);

                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.Salesman
                             && x.Active == true
                             && x.OfficeId.Contains(item.Office.ToString())
                             && x.SaleService.Contains(item.Service));

                            if (contract != null)
                            {
                                decimal? sellingNoVat = 0;
                                foreach (var charge in surchargeInGrp)
                                {
                                    sellingNoVat += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, contract.CreditCurrency);
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.Salesman,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                    SellingNoVat = sellingNoVat
                                });
                            }
                            else
                            {
                                decimal? sellingNoVat = 0;
                                foreach (var charge in surchargeInGrp)
                                {
                                    sellingNoVat += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, AccountingConstants.CURRENCY_LOCAL);
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.Salesman,
                                    ContractCurrency = "VND",
                                    SellingNoVat = sellingNoVat
                                });
                            }
                        }
                    }
                } 
                #endregion SellingNoVat

                #region OBH Amount
                IQueryable<SalesmanSurcharge> _chargeWithSalemanOBH = Enumerable.Empty<SalesmanSurcharge>().AsQueryable();
                if (fe.Service == "CL")
                {
                    _chargeWithSalemanOBH = from ops in opsJob
                                         join sur in surchargesOBH on ops.Hblid equals sur.Hblid into grpOps
                                         from surGrp in grpOps.DefaultIfEmpty()
                                         where surGrp.OfficeId == fe.Office
                                         && surGrp.PaymentObjectId == fe.PartnerId
                                         && surGrp.TransactionType == fe.Service
                                         select new SalesmanSurcharge
                                         {
                                             Surcharge = surGrp,
                                             Salesman = ops.SalemanId,
                                             Office = surGrp.OfficeId,
                                             PartnerId = surGrp.PaymentObjectId,
                                             TransactionType = surGrp.TransactionType
                                         };
                }
                else
                {
                    _chargeWithSalemanOBH = from cs in csJob
                                         join csd in transactionDetailRepo.Get() on cs.Id equals csd.JobId
                                         join sur in surchargesOBH on csd.Id equals sur.Hblid into grpOps
                                         from surGrp in grpOps.DefaultIfEmpty()
                                         where surGrp.OfficeId == fe.Office
                                         && surGrp.PaymentObjectId == fe.PartnerId
                                         && surGrp.TransactionType == fe.Service
                                         select new SalesmanSurcharge
                                         {
                                             Surcharge = surGrp,
                                             Salesman = csd.SaleManId,
                                             Office = surGrp.OfficeId,
                                             PartnerId = surGrp.PaymentObjectId,
                                             TransactionType = surGrp.TransactionType
                                         };
                }
                if (_chargeWithSalemanOBH.Count() > 0)
                {
                    var _chargeGrpWithSalesmanOBH = _chargeWithSalemanOBH
                    .GroupBy(x => new { x.PartnerId, x.Office, x.TransactionType, x.Salesman })
                    .Select(x => new { x.Key.Salesman, Surcharges = x.Select(i => i.Surcharge), x.Key.PartnerId, x.Key.Office, Service = x.Key.TransactionType });

                    foreach (var item in _chargeGrpWithSalesmanOBH)
                    {
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                            && x.Office == item.Office && x.Service == item.Service && x.SaleMan == item.Salesman);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.Salesman
                             && x.Active == true
                             && x.OfficeId.Contains(item.Office.ToString())
                             && x.SaleService.Contains(item.Service));
                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }
                            decimal? obhNoVat = 0;
                            var surchargeInGrp = item.Surcharges;
                            foreach (var charge in surchargeInGrp)
                            {
                                obhNoVat += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, currentReceivable.ContractCurrency);
                            }
                            currentReceivable.ObhAmount =  obhNoVat;
                            receivableIdModified.Add(currentReceivable.Id);
                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                            && x.SaleManId == item.Salesman
                            && x.Active == true
                            && x.OfficeId.Contains(item.Office.ToString())
                            && x.SaleService.Contains(item.Service));
                            var surchargeInGrp = item.Surcharges;

                            if (contract != null)
                            {
                                decimal? obhTotal = 0;
                                foreach (var charge in surchargeInGrp)
                                {
                                    obhTotal += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, contract.CreditCurrency);
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    ObhAmount = obhTotal,
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.Salesman,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                });
                            }
                            else
                            {
                                decimal? obhTotal = 0;
                                foreach (var charge in surchargeInGrp)
                                {
                                    obhTotal += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, AccountingConstants.CURRENCY_LOCAL);
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    ObhAmount = obhTotal,
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.Salesman,
                                    ContractCurrency = "VND",
                                    SellingNoVat = 0
                                });
                            }
                        }
                    }
                }
                #endregion OBH Amount

                #region Billing Amount - Billing Unpaid
                var dataInvoicesWithSalesmanInvoice = GetDataBillingSalesman(fe.PartnerId, fe.Office, fe.Service, AccountingConstants.ACCOUNTING_INVOICE_TYPE, null);
                var dataGrpSalesmanInvoice = dataInvoicesWithSalesmanInvoice.GroupBy(x => new { x.SalesmanId, x.PartnerId, x.OfficeId, x.Service })
                    .Select(x => new { x.Key.SalesmanId, x.Key.PartnerId, x.Key.OfficeId, x.Key.Service, invoices = x });
                if (dataGrpSalesmanInvoice.Count() > 0)
                {
                    foreach (var item in dataGrpSalesmanInvoice)
                    {
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                            && x.Office == item.OfficeId && x.Service == fe.Service && x.SaleMan == item.SalesmanId);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.SalesmanId
                             && x.Active == true
                             && x.OfficeId.Contains(item.OfficeId.ToString())
                             && x.SaleService.Contains(item.Service));

                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }
                            decimal? totalAmount = 0;
                            decimal? totalUnpaidAmount = 0;
                            decimal? totalUnpaidAmountPerService = 0;
                            decimal? totalAmountPerService = 0;
                            var invoicesData = item.invoices;
                            foreach (var invoice in invoicesData)
                            {
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalAmount += invoice.TotalAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalAmount += invoice.TotalAmountUSD;
                                }

                                int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                       .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                qtyService = (qtyService == 0) ? 1 : qtyService;
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalUnpaidAmount += invoice.UnpaidAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                }
                                totalUnpaidAmountPerService = (totalUnpaidAmount / qtyService);
                                totalAmountPerService = (totalAmount / qtyService);
                            }

                            currentReceivable.BillingAmount = totalAmountPerService;
                            currentReceivable.BillingUnpaid = totalUnpaidAmountPerService;

                            receivableIdModified.Add(currentReceivable.Id);
                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                            && x.SaleManId == item.SalesmanId
                            && x.Active == true
                            && x.OfficeId.Contains(item.OfficeId.ToString())
                            && x.SaleService.Contains(item.Service));
                            if (contract != null)
                            {
                                decimal? totalAmount = 0;
                                decimal? totalUnpaidAmount = 0;
                                decimal? totalUnpaidAmountPerService = 0;
                                decimal? totalAmountPerService = 0;

                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalAmount += invoice.TotalAmountVND;
                                        totalUnpaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalAmount += invoice.TotalAmountUSD;
                                        totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                    }

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalUnpaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                    }
                                    totalUnpaidAmountPerService = totalUnpaidAmount / qtyService;
                                    totalAmountPerService = totalAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.SalesmanId,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                    BillingAmount = totalAmountPerService,
                                    BillingUnpaid = totalUnpaidAmountPerService
                                });
                            }
                            else
                            {
                                decimal? totalAmount = 0;
                                decimal? totalUnpaidAmount = 0;
                                decimal? totalUnpaidAmountPerService = 0;
                                decimal? totalAmountPerService = 0;

                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    totalAmount += invoice.TotalAmountVND;
                                    totalUnpaidAmount += invoice.UnpaidAmountVND;

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;

                                    totalUnpaidAmountPerService = totalUnpaidAmount / qtyService;
                                    totalAmountPerService = totalAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.SalesmanId,
                                    ContractCurrency = "VND",
                                    BillingAmount = totalAmountPerService,
                                    BillingUnpaid = totalUnpaidAmountPerService
                                });
                            }
                        }
                    }
                }
               
                #endregion

                #region OBH Billing - OBH Unpaid
                var dataInvoicesWithSalesmanInvoiceTemp = GetDataBillingSalesman(fe.PartnerId, fe.Office, fe.Service, AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE, null);
                var dataGrpSalesmanInvoiceTemp = dataInvoicesWithSalesmanInvoiceTemp.GroupBy(x => new { x.SalesmanId, x.PartnerId, x.OfficeId, x.Service })
                  .Select(x => new { x.Key.SalesmanId, x.Key.PartnerId, x.Key.OfficeId, x.Key.Service, invoices = x });
                if (dataGrpSalesmanInvoiceTemp.Count() > 0)
                {
                    foreach (var item in dataGrpSalesmanInvoiceTemp)
                    {
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                           && x.Office == item.OfficeId && x.Service == item.Service && x.SaleMan == item.SalesmanId);
                        if(currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.SalesmanId
                             && x.Active == true
                             && x.OfficeId.Contains(item.OfficeId.ToString())
                             && x.SaleService.Contains(item.Service));
                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }

                            decimal? totalAmount = 0;
                            decimal? totalUnpaidAmount = 0;
                            decimal? totalUnpaidAmountPerService = 0;
                            decimal? totalAmountPerService = 0;

                            var invoicesData = item.invoices;
                            foreach (var invoice in invoicesData)
                            {
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalAmount += invoice.TotalAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalAmount += invoice.TotalAmountUSD;
                                }

                                int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                       .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                qtyService = (qtyService == 0) ? 1 : qtyService;
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalUnpaidAmount += invoice.UnpaidAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                }
                                totalUnpaidAmountPerService = totalUnpaidAmount / qtyService;
                                totalAmountPerService = totalAmount / qtyService;
                            }
                            currentReceivable.ObhBilling = totalAmountPerService;
                            currentReceivable.ObhUnpaid = totalUnpaidAmountPerService;
                            currentReceivable.ObhAmount += currentReceivable.ObhUnpaid;

                            receivableIdModified.Add(currentReceivable.Id);
                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                            && x.SaleManId == item.SalesmanId
                            && x.Active == true
                            && x.OfficeId.Contains(item.OfficeId.ToString())
                            && x.SaleService.Contains(item.Service));
                            if(contract != null)
                            {
                                decimal? totalAmount = 0;
                                decimal? totalUnpaidAmount = 0;
                                decimal? totalUnpaidAmountPerService = 0;
                                decimal? totalAmountPerService = 0;

                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalAmount += invoice.TotalAmountVND;
                                        totalUnpaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalAmount += invoice.TotalAmountUSD;
                                        totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                    }

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalUnpaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                    }
                                    totalUnpaidAmountPerService = totalUnpaidAmount / qtyService;
                                    totalAmountPerService = totalAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.SalesmanId,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                    ObhBilling = totalAmountPerService,
                                    ObhUnpaid = totalUnpaidAmountPerService,
                                    ObhAmount = totalUnpaidAmountPerService
                                });
                            }
                            else
                            {
                                decimal? totalAmount = 0;
                                decimal? totalUnpaidAmount = 0;
                                decimal? totalUnpaidAmountPerService = 0;
                                decimal? totalAmountPerService = 0;

                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    totalAmount += invoice.TotalAmountVND;
                                    totalUnpaidAmount += invoice.UnpaidAmountVND;

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;

                                    totalUnpaidAmountPerService = totalUnpaidAmount / qtyService;
                                    totalAmountPerService = totalAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.SalesmanId,
                                    ContractCurrency = "VND",
                                    ObhBilling = totalAmountPerService,
                                    ObhUnpaid = totalUnpaidAmountPerService,
                                    ObhAmount = totalUnpaidAmountPerService
                                });
                            }
                        }
                    }
                }
                
                #endregion

                #region OBH Paid
                var dataInvoicesWithSalesmanInvoiceTempPaidApart = GetDataBillingSalesman(fe.PartnerId, fe.Office, fe.Service, AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE, AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART);
                var dataGrpSalesmanInvoiceTempPaidApart = dataInvoicesWithSalesmanInvoiceTempPaidApart.GroupBy(x => new { x.SalesmanId, x.PartnerId, x.OfficeId, x.Service })
                  .Select(x => new { x.Key.SalesmanId, x.Key.PartnerId, x.Key.OfficeId, x.Key.Service, invoices = x });
                if(dataGrpSalesmanInvoiceTempPaidApart.Count() > 0)
                {
                    foreach (var item in dataGrpSalesmanInvoiceTempPaidApart)
                    {
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                           && x.Office == item.OfficeId && x.Service == item.Service && x.SaleMan == item.SalesmanId);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.SalesmanId
                             && x.Active == true
                             && x.OfficeId.Contains(item.OfficeId.ToString())
                             && x.SaleService.Contains(item.Service));
                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }
                            decimal? totalPaidAmount = 0;
                            decimal? totalPaidAmountPerService = 0;
                            var invoicesData = item.invoices;
                            foreach (var invoice in invoicesData)
                            {
                                int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                       .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                qtyService = (qtyService == 0) ? 1 : qtyService;
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalPaidAmount += invoice.PaidAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalPaidAmount += invoice.PaidAmountUSD;
                                }
                                totalPaidAmountPerService = totalPaidAmount / qtyService;
                            }
                            currentReceivable.ObhPaid = totalPaidAmountPerService;
                            receivableIdModified.Add(currentReceivable.Id);
                        }
                        else
                        {
                           var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                           && x.SaleManId == item.SalesmanId
                           && x.Active == true
                           && x.OfficeId.Contains(item.OfficeId.ToString())
                           && x.SaleService.Contains(item.Service));
                            if (contract != null)
                            {
                                decimal? totalPaidAmount = 0;
                                decimal? totalPaidAmountPerService = 0;
                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalPaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalPaidAmount += invoice.UnpaidAmountUSD;
                                    }

                                    totalPaidAmountPerService = totalPaidAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.SalesmanId,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                    ObhPaid = totalPaidAmountPerService

                                });
                            }
                            else
                            {
                                decimal? totalPaidAmount = 0;
                                decimal? totalPaidAmountPerService = 0;
                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    totalPaidAmount += invoice.UnpaidAmountVND;

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;

                                    totalPaidAmountPerService = totalPaidAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.SalesmanId,
                                    ContractCurrency = "VND",
                                    ObhPaid = totalPaidAmountPerService
                                });
                            }
                        }
                    }
                }
               
                #endregion

                #region  Paid Amount
                var dataInvoicesWithSalesmanInvoicePaidApart = GetDataBillingSalesman(fe.PartnerId, fe.Office, fe.Service, AccountingConstants.ACCOUNTANT_TYPE_INVOICE, AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART);
                var dataGrpSalesmanInvoicePaidApart = dataInvoicesWithSalesmanInvoicePaidApart.GroupBy(x => new { x.SalesmanId, x.PartnerId, x.OfficeId, x.Service })
                  .Select(x => new { x.Key.SalesmanId, x.Key.PartnerId, x.Key.OfficeId, x.Key.Service, invoices = x });
                if (dataGrpSalesmanInvoicePaidApart.Count() > 0)
                {
                    foreach (var item in dataGrpSalesmanInvoicePaidApart)
                    {
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                           && x.Office == item.OfficeId && x.Service == item.Service && x.SaleMan == item.SalesmanId);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.SalesmanId
                             && x.Active == true
                             && x.OfficeId.Contains(item.OfficeId.ToString())
                             && x.SaleService.Contains(item.Service));
                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }
                            decimal? totalPaidAmount = 0;
                            decimal? totalPaidAmountPerService = 0;
                            var invoicesData = item.invoices;
                            foreach (var invoice in invoicesData)
                            {
                                int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                       .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                qtyService = (qtyService == 0) ? 1 : qtyService;
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalPaidAmount += invoice.PaidAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalPaidAmount += invoice.PaidAmountUSD;
                                }
                                totalPaidAmountPerService = totalPaidAmount / qtyService;
                            }
                            currentReceivable.PaidAmount = totalPaidAmountPerService;

                            receivableIdModified.Add(currentReceivable.Id);
                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                            && x.SaleManId == item.SalesmanId
                            && x.Active == true
                            && x.OfficeId.Contains(item.OfficeId.ToString())
                            && x.SaleService.Contains(item.Service));
                            if(contract != null)
                            {
                                decimal? totalPaidAmount = 0;
                                decimal? totalPaidAmountPerService = 0;
                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalPaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalPaidAmount += invoice.UnpaidAmountUSD;
                                    }

                                    totalPaidAmountPerService = totalPaidAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.SalesmanId,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                    PaidAmount = totalPaidAmountPerService
                                });
                            } 
                            else
                            {
                                decimal? totalPaidAmount = 0;
                                decimal? totalPaidAmountPerService = 0;
                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    totalPaidAmount += invoice.UnpaidAmountVND;

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;

                                    totalPaidAmountPerService = totalPaidAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.SalesmanId,
                                    ContractCurrency = "VND",
                                    PaidAmount = totalPaidAmountPerService
                                });
                            }
                        }
                    }
                }
               
                #endregion
               
            }

            receivables.ForEach(fe =>
            {
                fe.DebitAmount = (fe.SellingNoVat ?? 0) + (fe.BillingUnpaid ?? 0) + (fe.ObhAmount ?? 0);
                fe.DatetimeCreated = DateTime.Now;
                fe.DatetimeModified = DateTime.Now;
            });

            return receivables;
        }

        public List<ObjectReceivableModel> CalculatorReceivableByBillingCode(string code, string billingType)
        {
            if(string.IsNullOrEmpty(code) || string.IsNullOrEmpty(billingType))
            {
                return new List<ObjectReceivableModel>();
            }
            Expression<Func<CsShipmentSurcharge, bool>> surchargesQuery = q => true && q.OfficeId != HM && q.OfficeId != BH;
            switch (billingType)
            {
                case "SOA":
                    surchargesQuery = surchargesQuery.And(x => x.Soano == code);
                    break;
                case "DEBIT":
                    surchargesQuery = surchargesQuery.And(x => x.DebitNo == code);
                    break;
                case "SETTLEMENT":
                    surchargesQuery = surchargesQuery.And(x => x.SettlementCode == code);
                    break;
                default:
                    break;
            }
            var surcharges = surchargeRepo.Get(surchargesQuery);
            var objectReceivablesModel = GetObjectReceivableBySurcharges(surcharges);
            return objectReceivablesModel;
        }

        private bool IsMatchService(string saleService, string serviceTerm)
        {
            bool isMatch = true;
            if (string.IsNullOrEmpty(serviceTerm))
            {
                return isMatch;
            }
            if (!string.IsNullOrEmpty(saleService))
            {
                var saleServiceList = saleService.Split(";").ToList();
                var serviceTermList = serviceTerm.Split(";").ToList();
                if (serviceTermList.Count > 0)
                {
                    isMatch = saleServiceList.Any(serviceTermList.Contains);
                }
            }

            return isMatch;
        }

        private IQueryable<AccountReceivableResult> GetARNoAgreement(IQueryable<AccAccountReceivable> acctReceivables, IQueryable<CatContract> partnerContracts, IQueryable<CatPartner> partners)
        {
            var users = userRepo.Get();
            var selectQuery = from acctReceivable in acctReceivables
                              join partner in partners on acctReceivable.PartnerId equals partner.Id into partner2
                              from partner in partner2.DefaultIfEmpty()
                              where acctReceivable.PartnerId == partner.Id
                              select acctReceivable;
            if (selectQuery == null || !selectQuery.Any()) return null;

            var groupByPartner = selectQuery.GroupBy(g => new { g.AcRef,g.SaleMan })
                .Select(s => new AccountReceivableResult
                {
                    PartnerId = s.Key.AcRef,
                    ArSalesmanName = s.Key.SaleMan,
                    OfficeId = s.First() != null ? s.First().Office.ToString() : null, //Office of AR
                    ArServiceCode = s.Select(se => se.Service).FirstOrDefault(),
                    //ArServiceName = string.Empty, //Get data bên dưới
                    ArServiceName = string.Join(";", CustomData.Services.Where(w =>s.Select(se => se.Service).Contains(w.Value)).Select(se => se.DisplayName)), 
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
                    ObhBillingAmount = s.Select(se => se.ObhBilling).Sum(),
                    ObhPaidAmount = s.Select(se => se.ObhPaid).Sum(),
                    ObhUnPaidAmount = s.Select(se => se.ObhUnpaid).Sum(),
                    ArOfficeIds = s.Select(x => x.Office.ToString()).Distinct().ToList(),
                    ArServices = s.Select(x => x.Service).Distinct().ToList()
                });

            var data = from ar in groupByPartner
                       join partner in partners on ar.PartnerId equals partner.Id
                       join user in users on ar.ArSalesmanName equals user.Id into user2
                       from user in user2.DefaultIfEmpty()
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
                           ArServiceName = ar.ArServiceName,
                           DebitAmount = ar.DebitAmount,
                           ObhAmount = ar.ObhAmount,
                           BillingAmount = ar.BillingAmount,
                           BillingUnpaid = ar.BillingUnpaid,
                           PaidAmount = ar.PaidAmount,
                           CreditAmount = ar.CreditAmount,
                           Over1To15Day = ar.Over1To15Day,
                           Over16To30Day = ar.Over16To30Day,
                           Over30Day = ar.Over30Day,
                           ArCurrency = ar.ArCurrency,
                           ObhBillingAmount = ar.ObhBillingAmount,
                           ObhPaidAmount = ar.ObhPaidAmount,
                           ObhUnPaidAmount = ar.ObhUnPaidAmount,
                           ArSalesmanName = user!= null?user.Username:"",
                           ArSalesmanId = user != null ? user.Id : "",
                           ArOfficeIds = ar.ArOfficeIds,
                           ArServices = ar.ArServices
                       };
            return data;
        }

        public IEnumerable<object> GetDebitDetailByPartnerId(ArDebitDetailCriteria model)
        {
            if (string.IsNullOrEmpty(model.PartnerId)) return null;
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("partnerId", model.PartnerId),
                SqlParam.GetParameter("saleManId",!string.IsNullOrEmpty( model.ArSalesManId)?model.ArSalesManId:""),
                SqlParam.GetParameter("option", model.Option),
                SqlParam.GetParameter("officeId",!string.IsNullOrEmpty(model.OfficeId)?model.OfficeId:""),
                SqlParam.GetParameter("serviceCode",!string.IsNullOrEmpty(model.ServiceCode)?model.ServiceCode:""),
                SqlParam.GetParameter("overDueDay",model.OverDueDay)
            };
            var data = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDebitDetailByPartnerId>(parameters);
            return data;
        }

        public async Task<HandleState> MoveReceivableData(AccountReceivableMoveDataSalesman model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    HandleState result = new HandleState();
                    var receivables = DataContext.Get(x => x.PartnerId == model.PartnerId && x.SaleMan == model.FromSalesman);
                    var receivablesToUpdate = new List<AccAccountReceivable>();
                    foreach (var item in model.ServiceOffice)
                    {
                        var receivableItem = receivables.FirstOrDefault(x => x.Service == item.Service && x.Office.ToString() == item.Office);
                        if(receivableItem != null)
                        {
                            receivablesToUpdate.Add(receivableItem);
                        }
                    }

                    List<Guid> contractIds = new List<Guid>();
                    if (receivablesToUpdate.Count() > 0)
                    {
                        foreach (var item in receivablesToUpdate)
                        {
                            contractIds.Add(item.ContractId ?? Guid.Empty);
                            item.ContractId = model.ContractId;
                            item.SaleMan = model.ToSalesman;
                            var hs = await DataContext.UpdateAsync(item, x => x.Id == item.Id, false);
                        }

                        result = DataContext.SubmitChanges();
                        if (result.Success)
                        {
                            var newContract = contractPartnerRepo.Get(x => x.Id == model.ContractId)?.FirstOrDefault();
                            if (newContract != null)
                            {
                                var contractNeedUpdate = ModifiedAgreementWithReceivables(newContract, receivablesToUpdate.AsQueryable());
                                await contractPartnerRepo.UpdateAsync(contractNeedUpdate, x => x.Id == contractNeedUpdate.Id, false);
                            }
                            var cIDs = contractIds.ToList().Distinct().ToList();
                            foreach (var item in cIDs)
                            {
                                var contractNeedReset = contractPartnerRepo.Get(x => x.Id == item)?.FirstOrDefault();
                                if (newContract != null)
                                {
                                    contractNeedReset.BillingAmount = 0;
                                    contractNeedReset.CustomerAdvanceAmountUsd = 0;
                                    contractNeedReset.CustomerAdvanceAmountVnd = 0;
                                    contractNeedReset.DebitAmount = 0;
                                    contractNeedReset.PaidAmount = 0;
                                    contractNeedReset.UnpaidAmount = 0;
                                    contractNeedReset.CreditAmount = 0;
                                    contractNeedReset.CreditRate = 0;
                                    contractNeedReset.IsOverLimit = false;
                                    contractNeedReset.IsOverDue = false;
                                }
                                HandleState hsUpdateReset = await contractPartnerRepo.UpdateAsync(contractNeedReset, x => x.Id == contractNeedReset.Id, false);
                            }
                            var hsUpdateListContract = contractPartnerRepo.SubmitChanges();
                            if (hsUpdateListContract.Success)
                            {
                                trans.Commit();
                            }
                            else
                            {
                                trans.Rollback();
                            }
                        }
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState((object)ex.ToString());
                }
                finally
                {
                    trans.Dispose();
                }
            }                
        }
    }
}
