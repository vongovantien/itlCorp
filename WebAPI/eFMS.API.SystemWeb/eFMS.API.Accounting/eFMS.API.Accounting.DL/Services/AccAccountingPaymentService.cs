using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountingPayment;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Infrastructure.Extensions;
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
    public class AccAccountingPaymentService : RepositoryBase<AccAccountingPayment, AccAccountingPaymentModel>, IAccAccountingPaymentService
    {
        private readonly IContextBase<SysUser> userRepository;
        private readonly IContextBase<AccAccountingManagement> accountingManaRepository;
        private readonly IContextBase<CatPartner> partnerRepository;
        private readonly IContextBase<AcctSoa> soaRepository;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        private readonly IContextBase<AcctReceipt> acctReceiptRepository;
        private readonly IContextBase<AcctCdnote> cdNoteRepository;
        private readonly IContextBase<CustomsDeclaration> customsDeclarationRepository;
        private readonly IContextBase<CatContract> catContractRepository;
        private readonly IContextBase<SysEmployee> sysEmployeeRepository;
        private readonly IContextBase<CsTransaction> csTransactionRepository;
        private readonly ICurrentUser currentUser;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<AcctCreditManagementAr> acctCreditManagementArRepository;
        private readonly IContextBase<SysOffice> sysOfficeRepository;
        private readonly IContextBase<OpsTransaction> opsTransactionRepository;
        private readonly IContextBase<CsTransactionDetail> csTransactionDetailRepository;

        public AccAccountingPaymentService(IContextBase<AccAccountingPayment> repository,
            IMapper mapper,
            IContextBase<SysUser> userRepo,
            IContextBase<AccAccountingManagement> accountingManaRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<AcctSoa> soaRepo,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<AcctReceipt> acctReceiptRepo,
            IContextBase<AcctCdnote> cdNoteRepo,
            IContextBase<CustomsDeclaration> customsDeclarationRepo,
             IContextBase<CatContract> catContractRepo,
             IContextBase<SysEmployee> sysEmployeeRepo,
             IContextBase<CsTransaction> csTransRepo,
             ICurrencyExchangeService exchangeService,
             IContextBase<AcctCreditManagementAr> creditArRepo,
             IContextBase<SysOffice> sysOfficeRepo,
             IContextBase<OpsTransaction> opsTransactionRepo,
             IContextBase<CsTransactionDetail> csTransactionDetailRepo,
            ICurrentUser currUser) : base(repository, mapper)
        {
            userRepository = userRepo;
            accountingManaRepository = accountingManaRepo;
            partnerRepository = partnerRepo;
            soaRepository = soaRepo;
            surchargeRepository = surchargeRepo;
            currentUser = currUser;
            acctReceiptRepository = acctReceiptRepo;
            cdNoteRepository = cdNoteRepo;
            customsDeclarationRepository = customsDeclarationRepo;
            catContractRepository = catContractRepo;
            sysEmployeeRepository = sysEmployeeRepo;
            csTransactionRepository = csTransRepo;
            currencyExchangeService = exchangeService;
            acctCreditManagementArRepository = creditArRepo;
            sysOfficeRepository = sysOfficeRepo;
            opsTransactionRepository = opsTransactionRepo;
            csTransactionDetailRepository = csTransactionDetailRepo;
        }

        public IQueryable<AccAccountingPaymentModel> GetBy(string refNo, string type, string invoiceNo)
        {
            IQueryable<AccAccountingPayment> data = null;
            var receiptData = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE);
            refNo = refNo == "null" ? null : refNo;
            if (type == "DEBIT" || type == "OBH")
            {
                data = DataContext.Get(x => x.BillingRefNo == refNo && x.Type == type && (string.IsNullOrEmpty(invoiceNo) || x.InvoiceNo == invoiceNo)).OrderBy(x => x.PaidDate).ThenBy(x => x.PaymentNo);
            }
            else if (type == "NETOFF")
            {
                data = DataContext.Get(x => x.BillingRefNo == refNo && (!string.IsNullOrEmpty(refNo) || x.InvoiceNo == invoiceNo)).OrderBy(x => x.PaidDate).ThenBy(x => x.PaymentNo);
            }
            else
            {
                var receiptId = receiptData.Where(x => x.PaymentRefNo == refNo).FirstOrDefault()?.Id;
                data = DataContext.Get(x => x.ReceiptId == receiptId).OrderBy(x => x.PaidDate).ThenBy(x => x.PaymentNo);
            }
            var dataPM = data.Join(receiptData, pm => pm.ReceiptId, re => re.Id, (pm, rc) => new { pm.PaymentAmount, pm.ReceiptId, rc.PaymentRefNo, pm.CurrencyId, pm.Balance, rc.PaymentDate, rc.PaymentMethod, pm.Note });
            var results = new List<AccAccountingPaymentModel>();
            var grpData = dataPM.GroupBy(x => x.ReceiptId).Select(x => new { x.Key, receipt = x.Select(z => z) });
            foreach (var x in grpData)
            {
                var item = new AccAccountingPaymentModel();
                var receipt = x.receipt.FirstOrDefault();
                item.ReceiptNo = receipt.PaymentRefNo;
                item.PaymentAmount = type == "OBH" ? x.receipt.Sum(r => r.PaymentAmount ?? 0) : (receipt.PaymentAmount ?? 0);
                item.CurrencyId = receipt.CurrencyId;
                item.Balance = type == "OBH" ? x.receipt.Sum(r => r.Balance ?? 0) : receipt.Balance ?? 0;
                item.PaidDate = receipt.PaymentDate;
                item.PaymentMethod = receipt.PaymentMethod;
                item.Note = receipt.Note;
                results.Add(item);
            }

            return results?.OrderBy(x => x.ReceiptNo).AsQueryable();
        }

        public IQueryable<AccountingPaymentModel> Paging(PaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            criteria.IsPaging = true;
            var data = Query(criteria);
            //var _totalItem = data.Select(s => s.RefId).Distinct().Count();
            //rowsCount = (_totalItem > 0) ? _totalItem : 0;
            var _totalItem = 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }

                data = GetReferencesData(data, criteria);
                if (data == null)
                {
                    rowsCount = 0;
                    return null;
                }
                _totalItem = data.Count();
                data = data.Skip((page - 1) * size).Take(size);
            }
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            return data;
        }
        public IQueryable<AccountingPaymentModel> ExportAccountingPayment(PaymentCriteria criteria)
        {
            var data = Query(criteria);
            if (data == null) return null;
            data = GetReferencesData(data, criteria);
            return data;
        }

        private IQueryable<AccountingPaymentModel> GetReferencesData(IQueryable<AccountingPaymentModel> data, PaymentCriteria criteria)
        {
            IQueryable<AccountingPaymentModel> results = null;
            if (data?.Count() > 0)
            {
                if (criteria.PaymentType == PaymentType.Invoice)
                {
                    results = GetReferencesInvoiceData(data, criteria);
                }
                else
                {
                    results = GetReferencesOBHData(data);
                }
            }

            var creditData = GetCreditDataPayment(criteria);
            if (creditData?.Count() > 0)
            {
                results = results != null ? results.Union(creditData) : creditData;
            }
            var advData = GetReferencesAdvanceData(criteria);
            if (advData?.Count() > 0)
            {
                results = results != null ? results.Union(advData) : advData;
            }
            return results?.OrderBy(x => x.DatetimeSorting).ThenBy(x => x.RefNo).ThenBy(x => x.Type);
        }


        private Expression<Func<AccAccountingPayment, bool>> GetQueryADVPermission(PermissionRange rangeSearch, ICurrentUser user)
        {
            Expression<Func<AccAccountingPayment, bool>> perQuery = null;
            switch (rangeSearch)
            {
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    perQuery = x => x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Group:
                    perQuery = x => (x.GroupId == user.GroupId && x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Department:
                    perQuery = x => (x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Office:
                    perQuery = x => (x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == currentUser.UserID;
                    break;
                case PermissionRange.Company:
                    perQuery = x => x.CompanyId == user.CompanyID
                                                || x.UserCreated == currentUser.UserID;
                    break;
            }
            return perQuery;
        }

        private IQueryable<AccAccountingPayment> QueryInvoiceDataPayment(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingPayment, bool>> perQuery = GetQueryADVPermission(rangeSearch, _user);
            Expression<Func<AccAccountingPayment, bool>> query = x => (x.Type == "DEBIT" || x.Type == "OBH");
            if (criteria.ReferenceNos?.Count(x => !string.IsNullOrEmpty(x)) > 0)
            {
                switch (criteria.SearchType)
                {
                    case "VatInvoice":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "DebitInvoice":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.BillingRefNo, StringComparer.OrdinalIgnoreCase) || criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Soa":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.BillingRefNo, StringComparer.OrdinalIgnoreCase));
                        break;
                }
            }

            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var data = DataContext.Get(query);
            return data;
        }

        private IQueryable<AccountingPaymentModel> GetReferencesOBHData(IQueryable<AccountingPaymentModel> data)
        {
            var partners = partnerRepository.Get();
            var resultsQuery = (from soa in data
                                join partner in partners on soa.PartnerId equals partner.Id into grpPartners
                                from part in grpPartners.DefaultIfEmpty()
                                join payment in DataContext.Get() on soa.RefId.ToLower() equals payment.RefId into grpPayments
                                from detail in grpPayments.DefaultIfEmpty()
                                select new
                                {
                                    soa,
                                    part.ShortName,
                                    Balance = detail != null ? detail.Balance : 0,
                                    PaymentAmount = detail != null ? detail.PaymentAmount : 0
                                }).ToList();
            var resultGroups = resultsQuery.GroupBy(x => new
            {
                x.soa.RefId,
                x.soa.SOANo,
                x.soa.PartnerId,
                x.ShortName,
                x.soa.Currency,
                x.soa.IssuedDate,
                x.soa.DueDate,
                x.soa.Status,
                x.soa.OverdueDays,
                x.soa.ExtendDays,
                x.soa.ExtendNote
            });
            var results = resultGroups
                            .Select(x => new AccountingPaymentModel
                            {
                                RefId = x.Key.RefId,
                                SOANo = x.Key.SOANo,
                                PartnerId = x.Key.PartnerId,
                                PartnerName = x.Key.ShortName,
                                Amount = x.Sum(y => y.soa.Amount),
                                Currency = x.Key.Currency,
                                IssuedDate = x.Key.IssuedDate,
                                DueDate = x.Key.DueDate,
                                OverdueDays = x.Key.OverdueDays,
                                Status = x.Key.Status,
                                ExtendDays = x.Key.ExtendDays,
                                ExtendNote = x.Key.ExtendNote,
                                PaidAmount = x.Sum(c => c.PaymentAmount),
                                UnpaidAmount = x.Sum(c => c.Balance)
                            }).AsQueryable();
            return results;
        }


        private IQueryable<AccountingPaymentModel> GetReferencesInvoiceData(IQueryable<AccountingPaymentModel> data, PaymentCriteria criteria)
        {
            var partners = partnerRepository.Get(x => x.Active == true);
            var paymentData = QueryInvoiceDataPayment(criteria);

            var surchargeData = surchargeRepository.Get(x => x.AcctManagementId != null);
            var receiptData = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE);
            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
            {
                data = data.Where(x => (x.Type == "InvoiceTemp") || (x.Type == "Invoice" && criteria.PaymentStatus.Contains(x.Status ?? "Unpaid")));
            }
            var resultsQuery = (from invoice in data
                                join surcharge in surchargeData on invoice.RefId.ToLower() equals surcharge.AcctManagementId.ToString()
                                join partner in partners on invoice.PartnerId equals partner.Id into grpPartners
                                from part in grpPartners.DefaultIfEmpty()
                                join payments in paymentData on invoice.RefId equals payments.RefId into grpPayment
                                from payment in grpPayment.DefaultIfEmpty()
                                join rcpts in receiptData on (payment == null ? Guid.Empty : payment.ReceiptId) equals rcpts.Id into grpReceipts
                                from rcpt in grpReceipts.DefaultIfEmpty()
                                select new
                                {
                                    invoice,
                                    ShortName = part == null ? string.Empty : part.ShortName,
                                    BillingRefNo = string.IsNullOrEmpty(surcharge.Soano) ? surcharge.DebitNo : surcharge.Soano,
                                    BillingRefNoType = string.IsNullOrEmpty(surcharge.Soano) ? "DEBIT" : "SOA",
                                    InvoiceNo = invoice.Type != "Invoice" ? string.Empty : invoice.InvoiceNoReal,
                                    Type = surcharge.Type == "OBH" ? "OBH" : "DEBIT",
                                    payment,
                                    PaymentRefNo = rcpt == null ? null : rcpt.PaymentRefNo,
                                    PaymentDate = rcpt == null ? null : rcpt.PaymentDate,
                                });
            if (criteria.FromUpdatedDate != null)
            {
                resultsQuery = resultsQuery.Where(x => (x.PaymentRefNo == null && (x.Type == "OBH" || (x.Type == "DEBIT" && x.invoice.Status == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID))) || (x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date));
            }
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                if (criteria.SearchType == "ReceiptNo")
                {
                    var listReceiptInfo = acctReceiptRepository.Get(receipt => receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE && criteria.ReferenceNos.Contains(receipt.PaymentRefNo)).Select(x => x.PaymentRefNo).ToList();
                    resultsQuery = resultsQuery.Where(x => listReceiptInfo.Any(z => z == (x.payment == null ? null : x.PaymentRefNo)));
                }
            }
            var resultGroups = resultsQuery.ToList().GroupBy(x => new
            {
                x.invoice.PartnerId,
                x.BillingRefNo,
                x.BillingRefNoType,
                x.Type,
                x.ShortName,
                x.InvoiceNo,
            }).Select(s => new { invoice = s.Select(i => i.invoice).Distinct(), s.Key, payment = s.Select(f => new { f.payment?.ReceiptId, f.PaymentRefNo, f.payment?.Type, f.payment?.PaymentAmount, f.payment?.PaymentAmountVnd, f.payment?.PaymentAmountUsd, f.PaymentDate }) });
            var results = new List<AccountingPaymentModel>();
            foreach (var item in resultGroups)
            {
                var payment = new AccountingPaymentModel();
                var invoice = item.invoice.GroupBy(z => z.RefId).Select(z => z.FirstOrDefault());
                var statusOBH = string.Empty;
                if (item.Key.Type == "OBH")
                {
                    var unpaidOBH = invoice.Sum(x => x.UnpaidAmount ?? 0);
                    var totalPaidOBH = invoice.Sum(x => x.TotalAmount ?? 0);
                    if (unpaidOBH <= 0)
                    {
                        statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                    }
                    else if (unpaidOBH > 0 && unpaidOBH < totalPaidOBH)
                    {
                        statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                    }
                    else
                    {
                        statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                    }
                }
                if (criteria.FromUpdatedDate != null && item.Key.Type == "OBH")
                {
                    var validObh = (item.payment.Any(x => string.IsNullOrEmpty(x.PaymentRefNo)) && statusOBH == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID) || (item.payment.Any(x => x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date));
                    if (!validObh)
                    {
                        continue;
                    }
                }
                payment.RefNo = item.Key.BillingRefNo;
                payment.ReceiptId = item.payment.FirstOrDefault()?.ReceiptId;
                payment.Type = item.Key.Type;
                payment.PartnerId = item.Key.PartnerId;
                payment.InvoiceNoReal = item.Key.InvoiceNo;
                payment.PartnerName = item.Key.ShortName;
                payment.Amount = item.Key.Type == "OBH" ? invoice.Sum(z => z.Amount) : invoice.FirstOrDefault().Amount;
                payment.TotalAmountVnd = item.Key.Type == "OBH" ? invoice.Sum(z => z.TotalAmountVnd) : invoice.FirstOrDefault().TotalAmountVnd;
                payment.TotalAmountUsd = item.Key.Type == "OBH" ? invoice.Sum(z => z.TotalAmountUsd) : invoice.FirstOrDefault().TotalAmountUsd;
                payment.Currency = invoice.FirstOrDefault().Currency;
                payment.IssuedDate = item.Key.BillingRefNoType == "DEBIT" && item.Key.Type == "OBH" ? null : invoice.FirstOrDefault().IssuedDate;
                payment.Serie = invoice.FirstOrDefault().Serie;
                payment.DueDate = invoice.FirstOrDefault().DueDate;
                payment.OverdueDays = invoice.FirstOrDefault().OverdueDays;
                payment.DatetimeSorting = invoice.FirstOrDefault().IssuedDate;

                payment.Status = item.Key.Type != "OBH" ? invoice.FirstOrDefault()?.Status : statusOBH;
                payment.ExtendDays = invoice.FirstOrDefault()?.ExtendDays;
                payment.PaidAmount = payment.PaidAmountVnd = payment.PaidAmountUsd = 0;
                var invoiceObh = invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE).Select(x => new { x.UnpaidAmount, x.UnpaidAmountVnd, x.UnpaidAmountUsd, x.SourceModified, x.PaidAmount, x.PaidAmountVnd, x.PaidAmountUsd, x.Status });
                payment.UnpaidAmount = item.Key.Type == "OBH" ? invoiceObh.Sum(x => x?.UnpaidAmount ?? 0) :
                        invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE).FirstOrDefault()?.UnpaidAmount ?? 0;
                payment.UnpaidAmountVnd = item.Key.Type == "OBH" ? invoiceObh.Sum(x => x?.UnpaidAmountVnd ?? 0) :
                        invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE).FirstOrDefault()?.UnpaidAmountVnd ?? 0;
                payment.UnpaidAmountUsd = item.Key.Type == "OBH" ? invoiceObh.Sum(x => x?.UnpaidAmountUsd ?? 0) :
                        invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE).FirstOrDefault()?.UnpaidAmountUsd ?? 0;
                if (item.payment.Any(x => !string.IsNullOrEmpty(x.PaymentRefNo)))
                {
                    var paymentUniq = item.payment.Where(x => !string.IsNullOrEmpty(x.PaymentRefNo)).GroupBy(x => new { x.PaymentRefNo }).Select(x => new { key = x.Key, p = x.Select(z => new { z.PaymentAmount, z.PaymentAmountVnd, z.PaymentAmountUsd }) });
                    payment.PaidAmount = item.Key.Type == "OBH" ? paymentUniq.Sum(x => x.p.Sum(z => z.PaymentAmount ?? 0)) : paymentUniq.Sum(x => x.p.FirstOrDefault()?.PaymentAmount ?? 0);
                    payment.PaidAmountVnd = item.Key.Type == "OBH" ? paymentUniq.Sum(x => x.p.Sum(z => z.PaymentAmountVnd ?? 0)) : paymentUniq.Sum(x => x.p.FirstOrDefault()?.PaymentAmountVnd ?? 0);
                    payment.PaidAmountUsd = item.Key.Type == "OBH" ? paymentUniq.Sum(x => x.p.Sum(z => z.PaymentAmountUsd ?? 0)) : paymentUniq.Sum(x => x.p.FirstOrDefault()?.PaymentAmountUsd ?? 0);
                }

                var invoiceDe = invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE && x.SourceModified != null && x.SourceModified == "1" && x.Status == "Paid");
                invoiceObh = invoiceObh == null ? null : invoiceObh.Where(x => x.SourceModified != null && x.SourceModified == "1" && x.Status == "Paid");
                if ((invoiceDe != null && invoiceDe.Count() > 0) || (invoiceObh != null && invoiceObh.Count() > 0))
                {
                    payment.PaidAmount = item.Key.Type == "OBH" ? (invoiceObh == null ? 0 : invoiceObh.Sum(x => x.PaidAmount)) : (invoiceDe == null ? 0 : invoiceDe.FirstOrDefault().PaidAmount);
                    payment.PaidAmountVnd = item.Key.Type == "OBH" ? (invoiceObh == null ? 0 : invoiceObh.Sum(x => x.PaidAmountVnd)) : (invoiceDe == null ? 0 : invoiceDe.FirstOrDefault().PaidAmountVnd);
                    payment.PaidAmountUsd = item.Key.Type == "OBH" ? (invoiceObh == null ? 0 : invoiceObh.Sum(x => x.PaidAmountUsd)) : (invoiceDe == null ? 0 : invoiceDe.FirstOrDefault().PaidAmountUsd);
                }
                results.Add(payment);
            }
            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
            {
                results = results.Where(x => criteria.PaymentStatus.Contains(x.Status)).ToList();
            }
            return results.AsQueryable();
        }

        private Expression<Func<AcctSoa, bool>> SoaCreditExpressionQuery(PaymentCriteria criteria)
        {
            Expression<Func<AcctSoa, bool>> query = q => q.Type.ToUpper() == "CREDIT";
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(q => q.Customer == criteria.PartnerId);
            }
            if (criteria.Office != null && criteria.Office.Count > 0)
            {
                query = query.And(x => x.OfficeId != null && criteria.Office.Contains(x.OfficeId.ToString()));
            }
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                var soaNo = new List<string>();
                switch (criteria.SearchType)
                {
                    case "VatInvoice":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "DebitInvoice":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.CreditNo, StringComparer.OrdinalIgnoreCase) && string.IsNullOrEmpty(x.Soano)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "Soa":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.Soano));
                        break;
                    case "CreditNote":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.CreditNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "HBL":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "MBL":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "JobNo":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                }

                if (soaNo != null && soaNo.Count > 0)
                {
                    query = query.And(x => soaNo.Any(s => s == x.Soano));
                }
            }

            // Get data within 3 months if search without anything
            if (IsInitSearch(criteria))
            {
                var maxDate = (soaRepository.Get().Max(x => x.DatetimeModified) ?? DateTime.Now).AddDays(1).Date;
                var minDate = maxDate.AddMonths(-3).AddDays(-1).Date; // Start from 3 months ago
                query = query.And(x => x.DatetimeModified.Value > minDate && x.DatetimeModified.Value < maxDate);
            }
            return query;
        }

        /// <summary>
        /// Is init search data payment history
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private bool IsInitSearch(PaymentCriteria criteria)
        {
            // Get data within 3 months if search without anything
            if (criteria.IsPaging == true &&
                string.IsNullOrEmpty(criteria.PartnerId) &&
                (criteria.ReferenceNos == null || criteria.ReferenceNos.Count == 0) &&
                (criteria.PaymentStatus.Contains("Unpaid") && criteria.PaymentStatus.Contains("Paid A Part")) &&
                (criteria.IssuedDate == null) &&
                (criteria.FromUpdatedDate == null && criteria.ToUpdatedDate == null) &&
                (criteria.DueDate == null) &&
                (criteria.OverDueDays == OverDueDate.All) &&
                (criteria.PaymentType == PaymentType.Invoice) &&
                (criteria.Office.Count == 1 && criteria.Office.Contains(currentUser.OfficeID.ToString())))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get payment data with payment type = CREDIT
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IQueryable<AccountingPaymentModel> GetCreditDataPayment(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingPayment, bool>> perQuery = GetQueryADVPermission(rangeSearch, _user);
            var results = new List<AccountingPaymentModel>();
            Expression<Func<AccAccountingPayment, bool>> query = x => (x.PaymentType == "CREDIT" && (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId)));
            if (criteria.IssuedDate != null)
            {
                //query = query.And(x => false);
                return null;
            }
            if (criteria.DueDate != null)
            {
                query = query.And(x => false);
            }
            //switch (criteria.OverDueDays)
            //{
            //    case Common.OverDueDate.Between1_15:
            //    case Common.OverDueDate.Between16_30:
            //    case Common.OverDueDate.Between31_60:
            //    case Common.OverDueDate.Between61_90:
            //        query = query.And(x => false);
            //        break;
            //}
            if (criteria.OverDueDays != OverDueDate.All)
            {
                return null;
            }
            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var paymentData = DataContext.Get(query);
            var partners = partnerRepository.Get().Select(x => new { x.Id, x.ShortName });
            var querySoa = SoaCreditExpressionQuery(criteria);
            var soaData = soaRepository.Get(querySoa).Select(x => new { x.Id, x.Soano, x.Customer, x.NetOff, x.Currency, x.CreditAmount, x.DatetimeModified });
            var queryCdNote = CreditNoteExpressionQuery(criteria);
            var cdNoteData = cdNoteRepository.Get(queryCdNote).Select(x => new { x.Id, x.Code, x.PartnerId, x.NetOff, x.Total, x.CurrencyId, x.DatetimeModified });
            var surchargeData = surchargeRepository.Get(x => !string.IsNullOrEmpty(x.VoucherId) || !string.IsNullOrEmpty(x.VoucherIdre)).Select(x => new { x.Type, x.CreditNo, x.PaySoano, x.InvoiceNo, x.VoucherId, x.VoucherIdre, x.AmountVnd, x.VatAmountVnd, x.AmountUsd, x.VatAmountUsd });

            var creditSoaData = (from soa in soaData
                                 join surcharge in surchargeData on soa.Soano equals surcharge.PaySoano
                                 join payments in paymentData on soa.Id equals payments.RefId into grpPayment
                                 from payment in grpPayment.DefaultIfEmpty()
                                 join partner in partners on soa.Customer equals partner.Id into grpPartners
                                 from part in grpPartners.DefaultIfEmpty()
                                 select new
                                 {
                                     BillingRefNo = soa.Soano,
                                     Type = payment == null ? "CREDIT" : payment.Type,
                                     PartnerId = part.Id,
                                     part.ShortName,
                                     soa.NetOff,
                                     soa.Currency,
                                     VoucherId = surcharge.Type == "OBH" ? surcharge.VoucherIdre : surcharge.VoucherId,
                                     Amount = soa.CreditAmount,
                                     TotalAmountVnd = (surcharge.AmountVnd ?? 0) + (surcharge.VatAmountVnd ?? 0),
                                     TotalAmountUsd = (surcharge.AmountUsd ?? 0) + (surcharge.VatAmountUsd ?? 0),
                                     InvoiceNo = payment == null ? surcharge.InvoiceNo : payment.InvoiceNo,
                                     PaidAmount = payment == null ? 0 : payment.PaymentAmount,
                                     PaidAmountVnd = payment == null ? 0 : payment.PaymentAmountVnd,
                                     PaidAmountUsd = payment == null ? 0 : payment.PaymentAmountUsd,
                                     UnpaidAmount = payment == null ? soa.CreditAmount : (soa.Currency == AccountingConstants.CURRENCY_LOCAL ? (payment.UnpaidPaymentAmountVnd ?? 0) : (payment.UnpaidPaymentAmountUsd ?? 0)),
                                     UnpaidAmountVnd = payment == null ? (decimal?)null : (payment.UnpaidPaymentAmountVnd ?? 0),
                                     UnpaidAmountUsd = payment == null ? (decimal?)null : (payment.UnpaidPaymentAmountUsd ?? 0),
                                     PaidDate = payment == null ? null : payment.PaidDate,
                                     ReceiptId = payment == null ? null : payment.ReceiptId,
                                     DatetimeSorting = soa.DatetimeModified
                                 });

            var creditNoteData = (from cdNote in cdNoteData
                                  join surcharge in surchargeData on cdNote.Code equals surcharge.CreditNo
                                  join payments in paymentData on cdNote.Id.ToString() equals payments.RefId into grpPayment
                                  from payment in grpPayment.DefaultIfEmpty()
                                  join partner in partners on cdNote.PartnerId equals partner.Id into grpPartners
                                  from part in grpPartners.DefaultIfEmpty()
                                  select new
                                  {
                                      BillingRefNo = cdNote.Code,
                                      Type = payment == null ? "CREDIT" : payment.Type,
                                      PartnerId = part.Id,
                                      part.ShortName,
                                      cdNote.NetOff,
                                      Currency = cdNote.CurrencyId,
                                      VoucherId = surcharge.Type == "OBH" ? surcharge.VoucherIdre : surcharge.VoucherId,
                                      Amount = cdNote.Total,
                                      TotalAmountVnd = (surcharge.AmountVnd ?? 0) + (surcharge.VatAmountVnd ?? 0),
                                      TotalAmountUsd = (surcharge.AmountUsd ?? 0) + (surcharge.VatAmountUsd ?? 0),
                                      InvoiceNo = payment == null ? surcharge.InvoiceNo : payment.InvoiceNo,
                                      PaidAmount = payment == null ? 0 : payment.PaymentAmount,
                                      PaidAmountVnd = payment == null ? 0 : payment.PaymentAmountVnd,
                                      PaidAmountUsd = payment == null ? 0 : payment.PaymentAmountUsd,
                                      UnpaidAmount = payment == null ? cdNote.Total : cdNote.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (payment.UnpaidPaymentAmountVnd ?? 0) : (payment.UnpaidPaymentAmountUsd ?? 0),
                                      UnpaidAmountVnd = payment == null ? (decimal?)null : (payment.UnpaidPaymentAmountVnd ?? 0),
                                      UnpaidAmountUsd = payment == null ? (decimal?)null : (payment.UnpaidPaymentAmountUsd ?? 0),
                                      PaidDate = payment == null ? null : payment.PaidDate,
                                      ReceiptId = payment == null ? null : payment.ReceiptId,
                                      DatetimeSorting = cdNote.DatetimeModified
                                  });
            var data = (creditSoaData.Concat(creditNoteData)).ToList();
            var resultGroups = data.GroupBy(x => new
            {
                x.BillingRefNo,
                x.Type,
                x.PartnerId,
                x.ShortName,
            }).Select(x => new { grp = x.Key, payment = x.Select(z => new { z.NetOff, z.VoucherId, z.PaidDate, z.InvoiceNo, z.ReceiptId, z.Amount, z.TotalAmountVnd, z.TotalAmountUsd, z.Currency, z.PaidAmount, z.PaidAmountVnd, z.PaidAmountUsd, z.UnpaidAmount, z.UnpaidAmountVnd, z.UnpaidAmountUsd, z.DatetimeSorting }) });
            foreach (var item in resultGroups)
            {
                var payment = new AccountingPaymentModel();
                var acctPayment = item.payment.FirstOrDefault();
                payment.RefNo = item.grp.BillingRefNo;
                payment.ReceiptId = acctPayment.ReceiptId;
                payment.PaidDate = acctPayment.PaidDate;
                payment.Type = "NETOFF";
                payment.PartnerId = item.grp.PartnerId;
                payment.InvoiceNoReal = acctPayment.InvoiceNo;
                payment.PartnerName = item.grp.ShortName;
                payment.Amount = acctPayment.Amount ?? 0;
                payment.TotalAmountVnd = item.payment.Sum(x => x.TotalAmountVnd);
                payment.TotalAmountUsd = item.payment.Sum(x => x.TotalAmountUsd);
                payment.Currency = acctPayment.Currency;
                payment.PaidAmount = acctPayment.PaidAmount ?? 0;
                payment.PaidAmountVnd = acctPayment.PaidAmountVnd ?? 0;
                payment.PaidAmountUsd = acctPayment.PaidAmountUsd ?? 0;
                payment.UnpaidAmount = acctPayment.UnpaidAmount;
                payment.UnpaidAmountVnd = acctPayment.UnpaidAmountVnd == 0 ? payment.TotalAmountVnd : acctPayment.UnpaidAmountVnd;
                payment.UnpaidAmountUsd = acctPayment.UnpaidAmountUsd == 0 ? payment.TotalAmountUsd : acctPayment.UnpaidAmountUsd;
                payment.Status = acctPayment.NetOff == true ? "Paid" : "Unpaid";
                payment.PaidDate = acctPayment.PaidDate;
                payment.DatetimeSorting = acctPayment.DatetimeSorting;
                results.Add(payment);
            }
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                if (criteria.SearchType == "ReceiptNo")
                {
                    var listReceiptInfo = acctReceiptRepository.Get(receipt => receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE && criteria.ReferenceNos.Contains(receipt.PaymentRefNo)).Select(x => x.Id).ToList();
                    results = results.Where(x => listReceiptInfo.Any(z => z == x.ReceiptId)).ToList();
                }
            }
            if (criteria.FromUpdatedDate != null)
            {
                results = results.Where(x => x.PaidDate != null && x.PaidDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaidDate.Value.Date <= criteria.ToUpdatedDate.Value.Date).ToList();
            }
            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
            {
                results = results.Where(x => criteria.PaymentStatus.Contains(x.Status ?? "Unpaid")).ToList();
            }
            return results.AsQueryable();
        }


        private Expression<Func<AcctCdnote, bool>> CreditNoteExpressionQuery(PaymentCriteria criteria)
        {
            Expression<Func<AcctCdnote, bool>> query = q => q.Type == "CREDIT";
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(q => q.PartnerId == criteria.PartnerId);
            }
            if (criteria.Office != null && criteria.Office.Count > 0)
            {
                query = query.And(x => x.OfficeId != null && criteria.Office.Contains(x.OfficeId.ToString()));
            }
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                var creditNo = new List<string>();
                switch (criteria.SearchType)
                {
                    case "VatInvoice":
                        creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "Soa":
                        creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.PaySoano, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "DebitInvoice":
                    case "CreditNote":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.Code));
                        break;
                    case "HBL":
                        creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "MBL":
                        creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "JobNo":
                        creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                }

                if (creditNo != null && creditNo.Count > 0)
                {
                    query = query.And(x => creditNo.Contains(x.Code));
                }
            }

            // Get data within 3 months if search without anything
            if (IsInitSearch(criteria))
            {
                var maxDate = (cdNoteRepository.Get().Max(x => x.DatetimeModified) ?? DateTime.Now).AddDays(1).Date;
                var minDate = maxDate.AddMonths(-3).AddDays(-1).Date; // Start from 3 months ago
                query = query.And(x => x.DatetimeModified.Value > minDate && x.DatetimeModified.Value < maxDate);
            }
            return query;
        }

        /// <summary>
        /// Get payment data with type = Advance
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IQueryable<AccountingPaymentModel> GetReferencesAdvanceData(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingPayment, bool>> perQuery = GetQueryADVPermission(rangeSearch, _user);
            Expression<Func<AccAccountingPayment, bool>> query = x => x.PaymentType == "OTHER" && (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId));
            var receiptData = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE);
            if (criteria.ReferenceNos?.Count(x => !string.IsNullOrEmpty(x)) > 0)
            {
                switch (criteria.SearchType)
                {
                    case "ReceiptNo":
                        var receiptLst = receiptData.Where(x => criteria.ReferenceNos.Any(re => re == x.PaymentRefNo)).Select(x => x.Id).ToList();
                        if (receiptLst.Count > 0)
                        {
                            query = query.And(x => receiptLst.Any(r => r == x.ReceiptId));
                        }
                        else
                        {
                            query = query.And(x => false);
                        }
                        break;
                    default:
                        query = query.And(x => false);
                        break;
                }
            }

            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count() > 0 && !criteria.PaymentStatus.Contains("Other"))
            {
                query = query.And(x => false);
            }
            if (criteria.IssuedDate != null)
            {
                query = query.And(x => x.DatetimeCreated != null && x.DatetimeCreated.Value.Date <= criteria.IssuedDate.Value.Date);
            }
            if (criteria.DueDate != null)
            {
                query = query.And(x => false);
            }
            if (criteria.FromUpdatedDate != null)
            {
                query = query.And(x => x.PaidDate != null && x.PaidDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaidDate.Value.Date <= criteria.ToUpdatedDate.Value.Date);
            }
            if (criteria.Office != null && criteria.Office.Count > 0)
            {
                receiptData = receiptData.Where(x => x.OfficeId != null && criteria.Office.Contains(x.OfficeId.ToString()));
            }
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                case Common.OverDueDate.Between16_30:
                case Common.OverDueDate.Between31_60:
                case Common.OverDueDate.Between61_90:
                    query = query.And(x => false);
                    break;
            }
            // Get data within 3 months if search without anything
            if (IsInitSearch(criteria))
            {
                var maxDate = (DataContext.Get().Max(x => x.DatetimeModified) ?? DateTime.Now).AddDays(1).Date;
                var minDate = maxDate.AddMonths(-3).AddDays(-1).Date; // Start from 3 months ago
                query = query.And(x => x.DatetimeModified.Value > minDate && x.DatetimeModified.Value < maxDate);
            }
            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var paymentAdv = DataContext.Get(query);
            var results = new List<AccountingPaymentModel>();
            if (paymentAdv.Count() <= 0)
            {
                return results.AsQueryable();
            }
            var partners = partnerRepository.Get();
            var advData = (from adv in paymentAdv
                           join rcpt in receiptData on adv.ReceiptId equals rcpt.Id
                           join partner in partners on rcpt.CustomerId equals partner.Id into grpPartners
                           from part in grpPartners.DefaultIfEmpty()
                           select new
                           {
                               adv,
                               PartnerId = rcpt.CustomerId,
                               part.ShortName,
                               rcpt.PaymentRefNo,
                               Currency = rcpt.CurrencyId
                           }).ToList();
            var resultGroups = advData.GroupBy(x => new
            {
                x.PaymentRefNo,
                x.adv.Type,
                x.PartnerId,
                x.ShortName,
            }).Select(x => new { grp = x.Key, payment = x.Select(z => z.adv) });

            foreach (var item in resultGroups)
            {
                var payment = new AccountingPaymentModel();
                var acctPayment = item.payment.FirstOrDefault();
                payment.RefNo = item.grp.PaymentRefNo;
                payment.Type = Common.CustomData.PaymentTypeOther.Where(x => x.Value == item.grp.Type).Select(x => x.DisplayName).FirstOrDefault();
                payment.PartnerId = item.grp.PartnerId;
                payment.InvoiceNoReal = acctPayment.InvoiceNo;
                payment.PartnerName = item.grp.ShortName;
                payment.Amount = acctPayment.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (acctPayment.PaymentAmountVnd ?? 0) : (acctPayment.PaymentAmountUsd ?? 0);
                payment.TotalAmountVnd = (acctPayment.PaymentAmountVnd ?? 0);
                payment.TotalAmountUsd = (acctPayment.PaymentAmountUsd ?? 0);
                payment.Currency = acctPayment.CurrencyId;
                payment.PaidAmount = acctPayment.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (acctPayment.PaymentAmountVnd ?? 0) : (acctPayment.PaymentAmountUsd ?? 0);
                payment.PaidAmountVnd = (acctPayment.PaymentAmountVnd ?? 0);
                payment.PaidAmountUsd = (acctPayment.PaymentAmountUsd ?? 0);
                payment.UnpaidAmount = acctPayment.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (acctPayment.UnpaidPaymentAmountVnd ?? 0) : (acctPayment.UnpaidPaymentAmountUsd ?? 0);
                payment.UnpaidAmountVnd = (acctPayment.UnpaidPaymentAmountVnd ?? 0);
                payment.UnpaidAmountUsd = (acctPayment.UnpaidPaymentAmountUsd ?? 0);
                payment.Status = "Paid";
                payment.DatetimeSorting = acctPayment.DatetimeModified;
                results.Add(payment);
            }
            return results.AsQueryable();
        }

        private IQueryable<AccountingPaymentModel> Query(PaymentCriteria criteria)
        {
            IQueryable<AccountingPaymentModel> results = null;
            switch (criteria.PaymentType)
            {
                case Common.PaymentType.Invoice:
                    results = QueryInvoicePayment(criteria);
                    break;
                case Common.PaymentType.OBH:
                    results = QueryOBHPayment(criteria);
                    break;
            }
            return results;
        }

        // get charges that have type OBH and SOANo(Debit)
        private IQueryable<AccountingPaymentModel> QueryOBHPayment(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AcctSoa, bool>> perQuery = GetQueryOBHPermission(rangeSearch, _user);
            Expression<Func<AcctSoa, bool>> query = x => (x.Customer == criteria.PartnerId || criteria.PartnerId == null)
                                                      && (criteria.ReferenceNos.Contains(x.Soano) || criteria.ReferenceNos == null)
                                                      && !string.IsNullOrEmpty(x.PaymentStatus);
            if (criteria.PaymentStatus.Count > 0)
            {
                query = query.And(x => criteria.PaymentStatus.Contains(x.PaymentStatus));
            }
            if (criteria.IssuedDate != null)
            {
                query = query.And(x => x.DatetimeCreated != null && x.DatetimeCreated.Value.Date <= criteria.IssuedDate.Value.Date);
            }
            if (criteria.DueDate != null)
            {
                query = query.And(x => x.PaymentDueDate.Value.Date <= criteria.DueDate.Value.Date);
            }
            if (criteria.FromUpdatedDate != null)
            {
                query = query.And(x => x.PaymentDatetimeUpdated != null && x.PaymentDatetimeUpdated.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDatetimeUpdated.Value.Date <= criteria.ToUpdatedDate.Value.Date);
            }

            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var data = soaRepository.Get(query);
            if (data == null) return null;
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                    data = data.Where(x => x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 16 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 0);
                    break;
                case Common.OverDueDate.Between16_30:
                    data = data.Where(x => x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 31 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 15);
                    break;
                case Common.OverDueDate.Between31_60:
                    data = data.Where(x => x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 61 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 30);
                    break;
                case Common.OverDueDate.Between61_90:
                    data = data.Where(x => x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 90 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 60);
                    break;
            }

            if (data == null) return null;
            var surcharges = surchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH
                                                        && !string.IsNullOrEmpty(x.Soano)
                                                        && (
                                                        (criteria.ReferenceNos.Contains(x.Mblno) || criteria.ReferenceNos == null)
                                                        || (criteria.ReferenceNos.Contains(x.Hblno) || criteria.ReferenceNos == null)
                                                        || (criteria.ReferenceNos.Contains(x.Soano) || criteria.ReferenceNos == null))
                                                        );

            var dataJoin = (from soa in data
                            join charge in surcharges on soa.Soano equals charge.Soano
                            select new { soa, TotalOBH = charge.Total });
            var results = dataJoin?.OrderByDescending(x => x.soa.PaymentDatetimeUpdated).Select(x => new AccountingPaymentModel
            {
                RefId = x.soa.Id.ToString(),
                SOANo = x.soa.Soano,
                PartnerId = x.soa.Customer,
                Amount = x.TotalOBH,
                Currency = x.soa.Currency,
                IssuedDate = x.soa.DatetimeCreated,
                DueDate = x.soa.PaymentDueDate,
                OverdueDays = (DateTime.Today > x.soa.PaymentDueDate.Value.Date) ? (DateTime.Today - x.soa.PaymentDueDate.Value.Date).Days : 0,
                Status = x.soa.PaymentStatus,
                ExtendDays = x.soa.PaymentExtendDays,
                ExtendNote = x.soa.PaymentNote
            });
            return results;
        }

        private Expression<Func<AcctSoa, bool>> GetQueryOBHPermission(PermissionRange rangeSearch, ICurrentUser user)
        {
            Expression<Func<AcctSoa, bool>> perQuery = null;
            switch (rangeSearch)
            {
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    perQuery = x => x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Group:
                    perQuery = x => (x.GroupId == user.GroupId && x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Department:
                    perQuery = x => (x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Office:
                    perQuery = x => (x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == currentUser.UserID;
                    break;
                case PermissionRange.Company:
                    perQuery = x => x.CompanyId == user.CompanyID
                                                || x.UserCreated == currentUser.UserID;
                    break;
            }
            return perQuery;
        }

        private IQueryable<AccountingPaymentModel> QueryInvoicePayment(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingManagement, bool>> perQuery = GetQueryInvoicePermission(rangeSearch, _user);
            Expression<Func<AccAccountingManagement, bool>> query = x => x.InvoiceNoReal != null && (x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE || x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE)
                                                                      && (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId)); // TH synce inv từ bravo && x.Status != "New"
            var acctManagementIds = new List<Guid?>();
            if (criteria.ReferenceNos?.Count(x => !string.IsNullOrEmpty(x)) > 0)
            {
                switch (criteria.SearchType)
                {
                    case "VatInvoice":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.InvoiceNoReal));
                        break;
                    case "DebitInvoice":
                        acctManagementIds = surchargeRepository.Get(x => (criteria.ReferenceNos.Contains(x.DebitNo, StringComparer.OrdinalIgnoreCase) && string.IsNullOrEmpty(x.Soano))
                        || criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase)
                        ).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "Soa":
                        acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Soano, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "ReceiptNo":
                        var hasReceiptInfo = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE).Any(receipt => criteria.ReferenceNos.Contains(receipt.PaymentRefNo));
                        if (!hasReceiptInfo)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "CreditNote":
                        acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.CreditNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "HBL":
                        acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "MBL":
                        acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "JobNo":
                        acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                }
            }
            if (acctManagementIds != null && acctManagementIds.Count > 0)
            {
                query = query.And(x => acctManagementIds.Contains(x.Id));
            }

            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0 && !criteria.PaymentStatus.Any(x => x == "Paid"))
            {
                query = query.And(x => x.SourceModified == null && x.SourceModified != "1" && x.Status != "Paid");
            }
            if (criteria.IssuedDate != null)
            {
                query = query.And(x => x.Date != null && x.Date.Value.Date <= criteria.IssuedDate.Value.Date);
            }
            if (criteria.DueDate != null)
            {
                query = query.And(x => x.PaymentDueDate.Value.Date <= criteria.DueDate.Value.Date);
            }
            if (criteria.Office != null && criteria.Office.Count > 0)
            {
                query = query.And(x => x.OfficeId != null && criteria.Office.Contains(x.OfficeId.ToString()));
            }

            // Get data within 3 months if search without anything
            if (IsInitSearch(criteria))
            {
                var maxDate = (accountingManaRepository.Get().Max(x => x.Date) ?? DateTime.Now).AddDays(1).Date;
                var minDate = maxDate.AddMonths(-3).AddDays(-1).Date; // Start from 3 months ago
                query = query.And(x => x.Date.Value > minDate && x.Date.Value < maxDate);
            }

            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var data = accountingManaRepository.Get(query);
            if (data == null) return null;
            var results = data.OrderByDescending(x => x.PaymentDatetimeUpdated).Select(x => new AccountingPaymentModel
            {
                RefId = x.Id.ToString(),
                InvoiceNoReal = x.InvoiceNoReal,
                PartnerId = x.PartnerId,
                Amount = x.TotalAmount,
                Currency = x.Currency,
                IssuedDate = x.Date,
                Serie = x.Serie,
                DueDate = x.PaymentDueDate,
                OverdueDays = x.PaymentDueDate != null && (DateTime.Today > x.PaymentDueDate.Value.Date) ? (DateTime.Today - x.PaymentDueDate.Value.Date).Days : 0,
                Status = x.PaymentStatus ?? "Unpaid",
                ExtendDays = x.PaymentExtendDays,
                ExtendNote = x.PaymentNote,
                PaidAmount = x.PaidAmount,
                UnpaidAmount = x.UnpaidAmount,
                UnpaidAmountVnd = x.UnpaidAmountVnd,
                UnpaidAmountUsd = x.UnpaidAmountUsd,
                TotalAmount = x.TotalAmount,
                TotalAmountVnd = x.TotalAmountVnd,
                TotalAmountUsd = x.TotalAmountUsd,
                VoucherId = x.VoucherId,
                ConfirmBillingDate = x.ConfirmBillingDate,
                Type = x.Type,
                OfficeId = x.OfficeId,
                ServiceType = x.ServiceType,
                PaymentTerm = x.PaymentTerm,
                AccountNo = x.AccountNo,
                SourceModified = x.SourceModified,
                PaidAmountVnd = x.PaidAmountVnd,
                PaidAmountUsd = x.PaidAmountUsd
            });
            if (results == null) return null;
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                    results = results.Where(x => x.OverdueDays < 16 && x.OverdueDays > 0);
                    break;
                case Common.OverDueDate.Between16_30:
                    results = results.Where(x => x.OverdueDays < 31 && x.OverdueDays > 15);
                    break;
                case Common.OverDueDate.Between31_60:
                    results = results.Where(x => x.OverdueDays < 61 && x.OverdueDays > 30);
                    break;
                case Common.OverDueDate.Between61_90:
                    results = results.Where(x => x.OverdueDays < 91 && x.OverdueDays > 60);
                    break;
            }
            return results;
        }

        private Expression<Func<AccAccountingManagement, bool>> GetQueryInvoicePermission(PermissionRange rangeSearch, ICurrentUser user)
        {
            Expression<Func<AccAccountingManagement, bool>> perQuery = null;
            switch (rangeSearch)
            {
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    perQuery = x => x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Group:
                    perQuery = x => (x.GroupId == user.GroupId && x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Department:
                    perQuery = x => (x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Office:
                    perQuery = x => (x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == currentUser.UserID;
                    break;
                case PermissionRange.Company:
                    perQuery = x => x.CompanyId == user.CompanyID
                                                || x.UserCreated == currentUser.UserID;
                    break;
            }
            return perQuery;
        }

        public HandleState UpdateExtendDate(ExtendDateUpdatedModel model)
        {
            HandleState result = new HandleState();
            switch (model.PaymentType)
            {
                case Common.PaymentType.Invoice:
                    result = UpdateExtendDateVATInvoice(model);
                    break;
                case Common.PaymentType.OBH:
                    result = UpdateExtendDateOBH(model);
                    break;
            }
            return result;
        }

        private HandleState UpdateExtendDateOBH(ExtendDateUpdatedModel model)
        {
            string id = model.RefId;
            var soa = soaRepository.Get(x => x.Id == id).FirstOrDefault();
            soa.PaymentExtendDays = model.NumberDaysExtend;
            soa.PaymentNote = model.Note;
            soa.PaymentDueDate = soa.PaymentDueDate.Value.AddDays(model.NumberDaysExtend);
            soa.PaymentDatetimeUpdated = DateTime.Now;
            soa.UserModified = currentUser.UserID;
            soa.DatetimeModified = DateTime.Now;
            var result = soaRepository.Update(soa, x => x.Id == id);
            return result;
        }

        private HandleState UpdateExtendDateVATInvoice(ExtendDateUpdatedModel model)
        {

            HandleState hs = new HandleState();
            var refIdLst = DataContext.Get(x => x.BillingRefNo == model.RefId).Select(x => x.RefId).ToList();
            foreach (var refId in refIdLst)
            {
                Guid id = new Guid(refId);
                var vatInvoice = accountingManaRepository.Get(x => x.Id == id).FirstOrDefault();
                vatInvoice.PaymentExtendDays = model.NumberDaysExtend;
                vatInvoice.PaymentNote = model.Note;
                vatInvoice.PaymentDueDate = vatInvoice.PaymentDueDate.Value.AddDays(model.NumberDaysExtend);
                vatInvoice.PaymentDatetimeUpdated = DateTime.Now;
                vatInvoice.UserModified = currentUser.UserID;
                vatInvoice.DatetimeModified = DateTime.Now;
                accountingManaRepository.Update(vatInvoice, x => x.Id == id, false);
            }
            hs = accountingManaRepository.SubmitChanges();
            return hs;
        }

        public HandleState Delete(Guid id)
        {
            HandleState result = new HandleState();
            var item = DataContext.Get(x => x.Id == id).FirstOrDefault();
            AccAccountingManagement vatInvoice = null;
            AcctSoa soa = null;
            if (item.Type == "INVOICE")
            {
                vatInvoice = UpdateVATPaymentStatus(item);
            }
            else
            {
                soa = UpdateSOAPaymentStatus(item);
            }
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    result = DataContext.Delete(x => x.Id == id);
                    if (result.Success)
                    {
                        if (vatInvoice != null)
                        {
                            var hsVAT = accountingManaRepository.Update(vatInvoice, x => x.Id == vatInvoice.Id);
                        }
                        else if (soa != null)
                        {
                            var hsSOA = soaRepository.Update(soa, x => x.Id == soa.Id);
                        }
                        DataContext.SubmitChanges();
                        accountingManaRepository.SubmitChanges();
                        soaRepository.SubmitChanges();
                        trans.Commit();
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
                return result;
            }
        }

        private AccAccountingManagement UpdateVATPaymentStatus(AccAccountingPayment item)
        {
            var invoice = accountingManaRepository.Get(x => x.Id == new Guid(item.RefId)).FirstOrDefault();
            var totalPaid = DataContext.Get(x => x.RefId == item.RefId && x.Id != item.Id).Sum(x => x.PaymentAmount);
            if (totalPaid == 0)
            {
                invoice.PaymentStatus = "Unpaid";
            }
            else
            {
                invoice.PaymentStatus = "Paid A Part";
            }
            return invoice;
        }

        private AcctSoa UpdateSOAPaymentStatus(AccAccountingPayment item)
        {
            var soa = soaRepository.Get(x => x.Id == item.RefId).FirstOrDefault();
            var totalPaid = DataContext.Get(x => x.RefId == item.RefId && x.Id != item.Id).Sum(x => x.PaymentAmount);
            if (totalPaid == 0)
            {
                soa.PaymentStatus = "Unpaid";
            }
            else
            {
                soa.PaymentStatus = "Paid A Part";
            }
            return soa;

        }

        public List<AccountingPaymentImportModel> CheckValidImportInvoicePayment(List<AccountingPaymentImportModel> list)
        {
            IQueryable<CatPartner> partners = partnerRepository.Get();
            IQueryable<AccAccountingManagement> invoices = accountingManaRepository.Get();

            list.ForEach(item =>
            {
                CatPartner partner = partners.Where(x => x.AccountNo == item.PartnerAccount)?.FirstOrDefault();
                if (partner == null)
                {
                    item.PartnerAccountError = "Not found partner " + item.PartnerAccount;
                    item.IsValid = false;
                }
                else
                {
                    item.PartnerName = partner.ShortName;
                    var accountManagement = invoices.FirstOrDefault(x => x.Serie == item.SerieNo && x.InvoiceNoReal == item.InvoiceNo && x.PartnerId == partner.Id);
                    if (accountManagement == null)
                    {
                        item.PartnerAccountError = "Not found " + item.SerieNo + " and " + item.InvoiceNo + " of " + item.PartnerAccount;
                        item.IsValid = false;
                    }
                    else if (accountManagement.Currency != item.CurrencyId)
                    {
                        item.IsValid = false;
                        item.CurrencyIdError = "Currency not match with invoice";
                    }
                    else
                    {
                        if (accountManagement.PaymentStatus == "Paid")
                        {
                            item.InvoiceNoError = "This invoice has been paid";
                            item.IsValid = false;
                        }
                        else
                        {
                            item.PartnerId = partner.Id;
                            item.RefId = accountManagement.Id.ToString();

                            AccAccountingPayment lastItem = DataContext.Get(x => x.RefId == item.RefId)?.OrderByDescending(x => x.PaidDate).FirstOrDefault();
                            if (lastItem != null)
                            {
                                if (item.PaidDate < lastItem.PaidDate)
                                {
                                    item.PaidDateError = item.PaidDate + " invalid";
                                }
                            }
                        }
                    }
                }
            });
            return list;
        }

        public HandleState ImportInvoicePayment(List<AccountingPaymentImportModel> list)
        {
            List<AccAccountingPayment> results = new List<AccAccountingPayment>();
            List<AccAccountingManagement> managements = new List<AccAccountingManagement>();
            var groups = list.GroupBy(x => x.RefId);
            foreach (var group in groups)
            {
                var refPayment = accountingManaRepository.Get(x => x.Id == new Guid(group.Key)).FirstOrDefault();
                var existedPayments = DataContext.Get(x => x.RefId == refPayment.Id.ToString());
                decimal? totalExistedPayment = 0;
                if (group.Any())
                {
                    totalExistedPayment = existedPayments.Sum(x => x.PaymentAmount);
                    var ItemInGroups = group.OrderBy(x => x.PaidDate).ToList();
                    int i = 0;
                    bool isPaid = false;
                    foreach (var item in ItemInGroups)
                    {
                        i = i + 1;
                        int paymentNo = existedPayments.Count() + i;
                        totalExistedPayment = totalExistedPayment + item.PaymentAmount;
                        decimal? balance = refPayment.TotalAmount - totalExistedPayment;
                        if (balance <= 0)
                        {
                            isPaid = true;
                        }
                        AccAccountingPayment payment = new AccAccountingPayment
                        {
                            Id = Guid.NewGuid(),
                            RefId = item.RefId,
                            PaymentNo = item.InvoiceNo + "_" + string.Format("{0:00}", paymentNo),
                            PaymentAmount = item.PaymentAmount,
                            Balance = balance,
                            CurrencyId = refPayment.Currency,
                            PaidDate = item.PaidDate,
                            PaymentType = item.PaymentType,
                            Type = "INVOICE",
                            UserCreated = currentUser.UserID,
                            UserModified = currentUser.UserID,
                            DatetimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            GroupId = currentUser.GroupId,
                            DepartmentId = currentUser.DepartmentId,
                            OfficeId = currentUser.OfficeID,
                            CompanyId = currentUser.CompanyID,

                            PaymentMethod = item.PaymentMethod,
                            ExchangeRate = item.ExchangeRate
                        };
                        results.Add(payment);
                    }
                    if (isPaid == true)
                    {
                        refPayment.PaymentStatus = "Paid";
                        refPayment.PaymentDatetimeUpdated = DateTime.Now;
                    }
                    else
                    {
                        refPayment.PaymentStatus = "Paid A Part";
                        refPayment.PaymentDatetimeUpdated = DateTime.Now;
                    }
                    managements.Add(refPayment);
                }
            }
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Add(results);
                    if (hs.Success)
                    {
                        foreach (var item in managements)
                        {
                            var s = accountingManaRepository.Update(item, x => x.Id == item.Id);
                        }
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

        public ExtendDateUpdatedModel GetInvoiceExtendedDate(string refNo)
        {
            var billingNoList = DataContext.Get(x => x.BillingRefNo == refNo).Select(x => x.RefId).ToList();
            var invoice = accountingManaRepository.Get(x => billingNoList.Any(b => b.ToUpper() == x.Id.ToString())).FirstOrDefault();
            if (invoice == null) return null;
            return new ExtendDateUpdatedModel
            {
                RefId = refNo,
                Note = invoice.PaymentNote,
                NumberDaysExtend = invoice.PaymentExtendDays == null ? 0 : (int)invoice.PaymentExtendDays,
                PaymentType = PaymentType.Invoice
            };
        }

        public ExtendDateUpdatedModel GetOBHSOAExtendedDate(string id)
        {
            var soa = soaRepository.Get(x => x.Id == id).FirstOrDefault();
            if (soa == null) return null;
            return new ExtendDateUpdatedModel
            {
                RefId = id,
                Note = soa.PaymentNote,
                NumberDaysExtend = soa.PaymentExtendDays != null ? (int)soa.PaymentExtendDays : 0,
                PaymentType = PaymentType.OBH
            };
        }

        public HandleState ImportOBHPayment(List<AccountingPaymentOBHImportTemplateModel> list)
        {
            List<AccAccountingPayment> results = new List<AccAccountingPayment>();
            List<AcctSoa> soas = new List<AcctSoa>();
            var groups = list.GroupBy(x => x.RefId);
            foreach (var group in groups)
            {
                AcctSoa refSOA = soaRepository.Get(x => x.Id == group.Key).FirstOrDefault();
                IQueryable<CsShipmentSurcharge> surcharges = surchargeRepository.Get(x => x.Soano == refSOA.Soano && x.Type == AccountingConstants.TYPE_CHARGE_OBH);
                IQueryable<AccAccountingPayment> existedPayments = DataContext.Get(x => x.RefId == refSOA.Id.ToString());

                decimal? totalExistedPayment = 0;
                if (group.Any())
                {
                    totalExistedPayment = existedPayments.Sum(x => x.PaymentAmount);
                    var ItemInGroups = group.OrderBy(x => x.PaidDate).ToList();
                    int i = 0;
                    bool isPaid = false;
                    foreach (var item in ItemInGroups)
                    {
                        i = i + 1;
                        int paymentNo = existedPayments.Count() + i;
                        totalExistedPayment = totalExistedPayment + item.PaymentAmount;
                        decimal? balance = surcharges.Sum(x => x.Total) - totalExistedPayment;
                        if (balance <= 0)
                        {
                            isPaid = true;
                        }
                        AccAccountingPayment payment = new AccAccountingPayment
                        {
                            Id = Guid.NewGuid(),
                            RefId = item.RefId,
                            PaymentNo = item.SoaNo + "_" + string.Format("{0:00}", paymentNo),
                            PaymentAmount = item.PaymentAmount,
                            Balance = balance,
                            CurrencyId = refSOA.Currency,
                            PaidDate = item.PaidDate,
                            PaymentType = item.PaymentType,
                            Type = "OBH",
                            UserCreated = currentUser.UserID,
                            UserModified = currentUser.UserID,
                            DatetimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            GroupId = currentUser.GroupId,
                            DepartmentId = currentUser.DepartmentId,
                            OfficeId = currentUser.OfficeID,
                            CompanyId = currentUser.CompanyID,

                            PaymentMethod = item.PaymentMethod,
                            ExchangeRate = item.ExchangeRate,

                        };
                        results.Add(payment);
                    }
                    if (isPaid == true)
                    {
                        refSOA.PaymentStatus = "Paid";
                        refSOA.PaymentDatetimeUpdated = DateTime.Now;
                    }
                    else
                    {
                        refSOA.PaymentStatus = "Paid A Part";
                        refSOA.PaymentDatetimeUpdated = DateTime.Now;
                    }
                    soas.Add(refSOA);
                }
            }
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Add(results);
                    if (hs.Success)
                    {
                        foreach (var item in soas)
                        {
                            var s = soaRepository.Update(item, x => x.Id == item.Id);
                        }
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

        public List<AccountingPaymentOBHImportTemplateModel> CheckValidImportOBHPayment(List<AccountingPaymentOBHImportTemplateModel> list)
        {
            IQueryable<CatPartner> partners = partnerRepository.Get();
            IQueryable<AcctSoa> soas = soaRepository.Get();

            list.ForEach(item =>
            {
                CatPartner partner = partners.Where(x => x.AccountNo == item.PartnerId)?.FirstOrDefault();
                if (partner == null)
                {
                    item.PartnerAccountError = "'" + item.PartnerId + "' Not found";
                    item.IsValid = false;
                }
                else
                {
                    item.PartnerName = partner.ShortName;
                    var soa = soas.FirstOrDefault(x => x.Soano == item.SoaNo && x.Customer == partner.Id);
                    if (soa == null)
                    {
                        item.SoaNoError = "Not found SOA No '" + item.SoaNo + "' of '" + item.PartnerId + "'";
                        item.IsValid = false;
                    }
                    else if (soa.Currency.Trim() != item.CurrencyId.Trim())
                    {
                        item.IsValid = false;
                        item.CurrencyIdError = "Currency not match with soa";
                    }
                    else
                    {
                        if (soa.PaymentStatus == "Paid")
                        {
                            item.SoaNoError = "This SOA has been paid";
                            item.IsValid = false;
                        }
                        else
                        {
                            item.PartnerId = partner.AccountNo;
                            item.RefId = soa.Id.ToString();
                            var lastItem = DataContext.Get(x => x.RefId == item.RefId)?.OrderByDescending(x => x.PaidDate).FirstOrDefault();
                            if (lastItem != null)
                            {
                                if (item.PaidDate < lastItem.PaidDate)
                                {
                                    item.PaidDateError = item.PaidDate + " invalid";
                                }
                            }
                        }
                    }
                }
            });
            return list;
        }

        /// <summary>
        /// Get data export Statement of Receivable Customers CR #16617
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IQueryable<AccountingCustomerPaymentExport> GetDataExportAccountingCustomerPayment(PaymentCriteria criteria)
        {
            var data = Query(criteria).Where(x => x.Currency == AccountingConstants.CURRENCY_LOCAL); // CR: 16859 => lấy tất cả partner và lọc theo currency vnd của billing
            if (data == null) return null;
            var partners = partnerRepository.Get(x => x.Active == true);
            var paymentData = QueryInvoiceDataPayment(criteria);
            var surchargeData = surchargeRepository.Get(x => x.AcctManagementId != null);
            var receiptData = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE && (criteria.Office == null || criteria.Office.Count == 0 || criteria.Office.Contains(x.OfficeId.ToString())));
            var officeData = sysOfficeRepository.Get().ToLookup(x => x.Id);
            var resultsQuery = (from invoice in data
                                join surcharge in surchargeData on invoice.RefId.ToLower() equals surcharge.AcctManagementId.ToString()
                                join part in partners on invoice.PartnerId equals part.Id
                                join payments in paymentData on invoice.RefId.ToLower() equals payments.RefId into grpPayment
                                from payment in grpPayment.DefaultIfEmpty()
                                join rcpts in receiptData on (payment == null ? Guid.Empty : payment.ReceiptId) equals rcpts.Id into grpReceipts
                                from rcpt in grpReceipts.DefaultIfEmpty()
                                select new
                                {
                                    invoice,
                                    payment,
                                    surcharge.JobNo,
                                    surcharge.Hblno,
                                    surcharge.Mblno,
                                    surcharge.Hblid,
                                    CombineNo = surcharge.CombineBillingNo,
                                    PartnerCode = part.AccountNo,
                                    ParentCode = part != null ? part.ParentId : string.Empty,
                                    PartnerName = part.ShortName,
                                    BillingRefNo = string.IsNullOrEmpty(surcharge.Soano) ? surcharge.DebitNo : surcharge.Soano,
                                    BillingRefNoType = string.IsNullOrEmpty(surcharge.Soano) ? "DEBIT" : "SOA",
                                    Type = surcharge.Type == "OBH" ? "OBH" : "DEBIT",
                                    PaymentRefNo = rcpt == null ? null : rcpt.PaymentRefNo,
                                    PaymentDate = rcpt == null ? null : rcpt.PaymentDate,
                                    PaymentDatetimeCreated = payment == null ? null : payment.DatetimeCreated,
                                    CusAdvanceAmountVnd = rcpt == null ? null : rcpt.CusAdvanceAmountVnd,
                                    CusAdvanceAmountUsd = rcpt == null ? null : rcpt.CusAdvanceAmountUsd,
                                    AgreementId = rcpt == null ? null : rcpt.AgreementId,
                                });
            if (criteria.FromUpdatedDate != null)
            {
                resultsQuery = resultsQuery.Where(x => (x.PaymentRefNo == null && (x.Type == "OBH" || (x.Type == "DEBIT" && x.invoice.Status == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID))) || (x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date));
            }
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                if (criteria.SearchType == "ReceiptNo")
                {
                    var listReceiptInfo = receiptData.Where(receipt => criteria.ReferenceNos.Contains(receipt.PaymentRefNo)).Select(x => x.PaymentRefNo).ToList();
                    resultsQuery = resultsQuery.Where(x => listReceiptInfo.Any(z => z == (x.payment == null ? null : x.PaymentRefNo)));
                }
            }

            var resultGroups = resultsQuery.ToList().GroupBy(x => new
            {
                x.invoice.PartnerId,
                x.PartnerCode,
                x.PartnerName,
                x.ParentCode,
                x.BillingRefNo,
                x.BillingRefNoType
            }).Select(x => new { grp = x.Key, invoice = x.Select(z => z.invoice), surcharge = x.Select(z => new { z.JobNo, z.Mblno, z.Hblno, z.CombineNo, z.Hblid }), payment = x.Select(z => new { z.payment?.Id, z.payment?.ReceiptId, z.payment?.PaymentType, z.PaymentRefNo, invoicePayment = z.payment?.InvoiceNo, z.PaymentDate, z.PaymentDatetimeCreated, z.AgreementId, z.CusAdvanceAmountVnd, z.CusAdvanceAmountUsd, z.payment?.PaymentAmountVnd, z.payment?.PaymentAmountUsd, z.payment?.UnpaidPaymentAmountVnd, z.payment?.UnpaidPaymentAmountUsd, z.Type }) });
            var results = new List<AccountingCustomerPaymentExport>();
            var soaLst = soaRepository.Get().Select(x => new { x.Soano, x.UserCreated }).ToLookup(x => x.Soano);
            var cdNoteLst = cdNoteRepository.Get().ToLookup(x => x.Code);
            var opsLookup = opsTransactionRepository.Get(x => x.CurrentStatus != "Canceled").Select(x => new { x.Id, x.JobNo, x.Hblid, x.SalemanId }).ToLookup(x => x.Id);
            //var userLst = userRepository.Get().Select(x => new { x.Id, x.EmployeeId }).ToLookup(x => x.Id);
            //var employeeLst = sysEmployeeRepository.Get().Select(x => new { x.Id, x.EmployeeNameEn }).ToLookup(x => x.Id);
            var employeeLst = from emp in sysEmployeeRepository.Get()
                              join user in userRepository.Get() on emp.Id equals user.EmployeeId
                              select new
                              {
                                  user.Id,
                                  emp.EmployeeNameEn
                              };
            foreach (var item in resultGroups)
            {
                var isValidObh = true;
                var invoiceDebitGroup = item.invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE).GroupBy(x => new { x.RefId, x.InvoiceNoReal }).Select(x => new { x.Key, invc = x.Select(z => new { z.Type, z.UnpaidAmountVnd, z.TotalAmountVnd, z.TotalAmountUsd, z.InvoiceNoReal, z.IssuedDate, z.ConfirmBillingDate, z.DueDate, z.PaymentTerm, z.OverdueDays, z.AccountNo, z.OfficeId, z.SourceModified, z.PaidAmountVnd, z.PaidAmountUsd, z.Status }) });
                var numOfDebitInvoice = invoiceDebitGroup.Select(x => x.Key.InvoiceNoReal)?.Distinct().Count();
                var invoiceObhGroup = item.invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE).GroupBy(x => new { x.RefId }).Select(x => new { x.Key, invc = x.Select(z => new { z.Type, z.UnpaidAmountVnd, z.TotalAmountVnd, z.TotalAmountUsd, z.InvoiceNoReal, z.IssuedDate, z.ConfirmBillingDate, z.DueDate, z.PaymentTerm, z.OverdueDays, z.AccountNo, z.OfficeId, z.SourceModified, z.PaidAmountVnd, z.PaidAmountUsd, z.Status }) });

                // CR: TH 1 billing with multi invoice
                if (invoiceObhGroup.FirstOrDefault() != null && (numOfDebitInvoice == null || numOfDebitInvoice == 0 || (numOfDebitInvoice != null && numOfDebitInvoice > 1)))
                {
                    #region OBH Detail
                    var payment = new AccountingCustomerPaymentExport();
                    var statusOBH = string.Empty;
                    if (invoiceObhGroup != null && invoiceObhGroup.Count() > 0)
                    {
                        // Check if obh payment have valid status on search
                        var unpaidOBH = invoiceObhGroup.Sum(x => x?.invc.FirstOrDefault().UnpaidAmountVnd ?? 0);
                        var totalPaidOBH = invoiceObhGroup.Sum(x => x?.invc.FirstOrDefault().TotalAmountVnd ?? 0);
                        if (unpaidOBH <= 0)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                        }
                        else if (unpaidOBH > 0 && unpaidOBH < totalPaidOBH)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                        }
                        else
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                        }
                    }
                    payment.UnpaidAmountInv = 0;
                    payment.UnpaidAmountOBH = isValidObh ? (invoiceObhGroup?.Sum(x => x?.invc.FirstOrDefault()?.TotalAmountVnd ?? 0) ?? 0) : 0;
                    payment.UnpaidAmountInvUsd = 0;
                    payment.UnpaidAmountOBHUsd = isValidObh ? (invoiceObhGroup?.Sum(x => x?.invc.FirstOrDefault()?.TotalAmountUsd ?? 0) ?? 0) : 0;

                    payment.PaidAmount = payment.PaidAmountUsd = 0;
                    payment.PaidAmountOBH = payment.PaidAmountOBHUsd = 0;
                    payment.receiptDetail = new List<AccountingReceiptDetail>();
                    DateTime? dateTo = null;
                    if (criteria.DueDate != null || criteria.IssuedDate != null)
                    {
                        dateTo = criteria.IssuedDate != null ? criteria.IssuedDate.Value.Date : criteria.DueDate.Value.Date;
                    }
                    // Search billing with payment date <= date search
                    var receiptGroup = item.payment.Where(x => x.PaymentType == "OBH" && !string.IsNullOrEmpty(x.PaymentRefNo) && (dateTo == null || x.PaymentDate.Value.Date <= dateTo.Value.Date)).GroupBy(x => new { x.ReceiptId, x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentType, z.PaymentDate, z.PaymentDatetimeCreated, z.AgreementId, z.CusAdvanceAmountVnd, z.CusAdvanceAmountUsd, z.PaymentAmountVnd, z.PaymentAmountUsd, z.UnpaidPaymentAmountVnd, z.UnpaidPaymentAmountUsd }) }).OrderBy(x => x.Payment.FirstOrDefault().PaymentDatetimeCreated);
                    if (receiptGroup != null && receiptGroup.Count() > 0)
                    {
                        if (statusOBH != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID)
                        {
                            var obhGrp = receiptGroup.Where(z => z.Payment.Any(pm => pm.PaymentType == "OBH")).FirstOrDefault()?.Payment.Where(z => z.PaymentType == "OBH").FirstOrDefault();
                            payment.UnpaidAmountOBH = isValidObh ? obhGrp == null ? payment.UnpaidAmountOBH : obhGrp?.UnpaidPaymentAmountVnd ?? 0 : 0;
                            payment.UnpaidAmountOBHUsd = isValidObh ? obhGrp == null ? payment.UnpaidAmountOBHUsd : obhGrp?.UnpaidPaymentAmountUsd ?? 0 : 0;
                        }
                        foreach (var rcp in receiptGroup)
                        {
                            var detail = new AccountingReceiptDetail();
                            detail.ReceiptId = rcp.grp.ReceiptId;
                            detail.PaymentRefNo = rcp.grp.PaymentRefNo;
                            detail.PaymentDate = rcp.Payment.FirstOrDefault()?.PaymentDate;
                            var paymentOBH = rcp.Payment.Where(z => z.PaymentType == "OBH");
                            detail.PaidAmount = 0;
                            detail.PaidAmountOBH = isValidObh ? paymentOBH.Sum(x => x.PaymentAmountVnd ?? 0) : 0;
                            detail.PaidAmountUsd = 0;
                            detail.PaidAmountOBHUsd = isValidObh ? paymentOBH.Sum(x => x.PaymentAmountUsd ?? 0) : 0;
                            detail.CusAdvanceAmountVnd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountVnd ?? 0;
                            detail.CusAdvanceAmountUsd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountUsd ?? 0;
                            detail.AgreementId = rcp.Payment.FirstOrDefault()?.AgreementId;

                            payment.PaidAmount += (detail.PaidAmount ?? 0);
                            payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                            payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                            payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                            payment.receiptDetail.Add(detail);
                        }
                    }

                    // Special case cal paid without payment 
                    var invoiceObhPaid = invoiceObhGroup == null ? null : invoiceObhGroup.Where(x => x.invc.FirstOrDefault().SourceModified != null && x.invc.FirstOrDefault().SourceModified == "1" && x.invc.FirstOrDefault().Status == "Paid");
                    if (invoiceObhPaid != null && invoiceObhPaid.Count() > 0)
                    {
                        var detail = new AccountingReceiptDetail();
                        detail.ReceiptId = Guid.Empty;
                        detail.PaymentRefNo = "";
                        detail.PaymentDate = null;
                        detail.PaidAmount = 0;
                        detail.PaidAmountOBH = isValidObh ? invoiceObhPaid.Sum(z => z.invc.FirstOrDefault()?.PaidAmountVnd ?? 0) : 0;
                        detail.PaidAmountUsd = 0;
                        detail.PaidAmountOBHUsd = isValidObh ? invoiceObhPaid.Sum(z => z.invc.FirstOrDefault()?.PaidAmountUsd ?? 0) : 0;

                        payment.PaidAmount += (detail.PaidAmount ?? 0);
                        payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                        payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                        payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                        payment.receiptDetail.Add(detail);
                    }


                    if (dateTo != null && payment.receiptDetail.Count() == 0)
                    {
                        var pmType = invoiceObhGroup?.Count() > 0 ? "OBH" : string.Empty;
                        var pmDetail = from pm in paymentData.Where(x => x.BillingRefNo == item.grp.BillingRefNo && pmType.Contains(x.PaymentType))
                                       join rcp in receiptData on pm.ReceiptId equals rcp.Id
                                       select new
                                       {
                                           pm.ReceiptId,
                                           pm.PaymentType,
                                           pm.PaymentAmountVnd,
                                           pm.PaymentAmountUsd,
                                           pm.UnpaidPaymentAmountVnd,
                                           pm.UnpaidPaymentAmountUsd,
                                           rcp.PaymentDate,
                                           PaymentDatetimeCreated = rcp.DatetimeCreated,
                                           rcp.PaymentRefNo,
                                           rcp.AgreementId,
                                           rcp.CusAdvanceAmountUsd,
                                           rcp.CusAdvanceAmountVnd
                                       };
                        var receiptRMGroup = pmDetail.Where(x => dateTo == null || x.PaymentDate.Value.Date > dateTo.Value.Date).GroupBy(x => new { x.ReceiptId, x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentType, z.PaymentDate, z.PaymentDatetimeCreated, z.AgreementId, z.CusAdvanceAmountVnd, z.CusAdvanceAmountUsd, z.PaymentAmountVnd, z.PaymentAmountUsd, z.UnpaidPaymentAmountVnd, z.UnpaidPaymentAmountUsd }) }).OrderBy(x => x.Payment.FirstOrDefault().PaymentDatetimeCreated);
                        if (receiptRMGroup?.Count() > 0)
                        {
                            var obhGrp = receiptRMGroup.Where(z => z.Payment.Any(pm => pm.PaymentType == "OBH")).FirstOrDefault()?.Payment.Where(z => z.PaymentType == "OBH").FirstOrDefault();

                            if (obhGrp != null)
                            {
                                if (statusOBH != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID)
                                {
                                    payment.UnpaidAmountOBH = isValidObh ? obhGrp?.UnpaidPaymentAmountVnd ?? 0 : 0;
                                    payment.UnpaidAmountOBHUsd = isValidObh ? obhGrp?.UnpaidPaymentAmountUsd ?? 0 : 0;
                                }
                                //foreach (var rcp in receiptRMGroup)
                                //{
                                //    var detail = new AccountingReceiptDetail();
                                //    detail.ReceiptId = rcp.grp.ReceiptId;
                                //    detail.PaymentRefNo = rcp.grp.PaymentRefNo;
                                //    detail.PaymentDate = rcp.Payment.FirstOrDefault()?.PaymentDate;
                                //    var paymentOBH = rcp.Payment.Where(z => z.PaymentType == "OBH");
                                //    detail.PaidAmount = 0;
                                //    detail.PaidAmountOBH = isValidObh ? (paymentOBH?.Sum(x => x.PaymentAmountVnd ?? 0) ?? 0) : 0;
                                //    detail.PaidAmountUsd = 0;
                                //    detail.PaidAmountOBHUsd = isValidObh ? (paymentOBH?.Sum(x => x.PaymentAmountUsd ?? 0) ?? 0) : 0;
                                //    detail.CusAdvanceAmountVnd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountVnd ?? 0;
                                //    detail.CusAdvanceAmountUsd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountUsd ?? 0;
                                //    detail.AgreementId = rcp.Payment.FirstOrDefault()?.AgreementId;

                                //    payment.PaidAmount += (detail.PaidAmount ?? 0);
                                //    payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                                //    payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                                //    payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                                //    payment.receiptDetail.Add(detail);
                                //}
                            }
                        }
                    }
                    if (invoiceObhGroup != null && invoiceObhGroup.Count() > 0)
                    {
                        // Check if obh payment have valid status on search
                        if ((payment.UnpaidAmountOBH - payment.PaidAmountOBH) == payment.UnpaidAmountOBH)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                        }
                        else if ((payment.UnpaidAmountOBH - payment.PaidAmountOBH) > 0)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                        }
                        else if ((payment.UnpaidAmountOBH - payment.PaidAmountOBH) == 0)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                        }
                        if (criteria.PaymentStatus.Count > 0 && !criteria.PaymentStatus.Contains(statusOBH))
                        {
                            isValidObh = false;
                        }
                    }

                    if (criteria.FromUpdatedDate != null && invoiceObhGroup != null && invoiceObhGroup.Count() > 0 && isValidObh)
                    {
                        var validObh = (item.payment.Any(x => x.Type == "OBH" && string.IsNullOrEmpty(x.PaymentRefNo)) && statusOBH == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID) || (item.payment.Any(x => x.Type == "OBH" && x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date) && isValidObh);
                        if (!validObh)
                        {
                            isValidObh = false;
                        }
                    }
                    if (isValidObh)
                    {
                        payment.PartnerId = item.grp.PartnerId;
                        payment.PartnerCode = item.grp.PartnerCode;
                        payment.PartnerName = item.grp.PartnerName;
                        payment.ParentCode = item.grp.ParentCode == null ? string.Empty : partners.Where(x => x.Id == item.grp.ParentCode).FirstOrDefault()?.AccountNo;
                        payment.InvoiceNo = null;
                        payment.InvoiceDate = invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.IssuedDate;
                        payment.BillingRefNo = item.grp.BillingRefNo;
                        payment.BillingDate = null;
                        payment.DueDate = invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.DueDate;
                        payment.OverdueDays = invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.OverdueDays;
                        payment.PaymentTerm = invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.PaymentTerm;
                        payment.CombineNo = item.surcharge.Where(x => !string.IsNullOrEmpty(x.CombineNo)).FirstOrDefault()?.CombineNo;
                        if (invoiceObhGroup?.Count() > 0)
                        {
                            payment.AccountNo = invoiceObhGroup.FirstOrDefault().invc.FirstOrDefault()?.AccountNo;
                        }
                        payment.BranchName = officeData[(Guid)invoiceObhGroup.FirstOrDefault().invc.FirstOrDefault()?.OfficeId].FirstOrDefault()?.ShortName;

                        var soaDetail = soaLst[item.grp.BillingRefNo].FirstOrDefault();
                        var cdNoteDetail = cdNoteLst[item.grp.BillingRefNo].FirstOrDefault();
                        // Get saleman name
                        if (cdNoteDetail != null)
                        {
                            // Get saleman of shipment
                            var salemanOfShipment = opsLookup[cdNoteDetail.JobId].FirstOrDefault()?.SalemanId;
                            if (salemanOfShipment == null)
                            {
                                salemanOfShipment = csTransactionDetailRepository.Get(x => x.Id == item.surcharge.FirstOrDefault().Hblid).FirstOrDefault()?.SaleManId; // air/sea lấy saleman đại diện
                            }
                            if (!string.IsNullOrEmpty(salemanOfShipment))
                            {
                                var employeeData = employeeLst.Where(x => x.Id == salemanOfShipment).FirstOrDefault();
                                payment.Salesman = employeeData == null ? string.Empty : employeeData.EmployeeNameEn;
                            }
                        }
                        if (soaDetail != null)
                        {
                            #region saleman of contract
                            var agreementId = item.payment.Where(x => x.AgreementId != null).FirstOrDefault()?.AgreementId;
                            var salemanId = catContractRepository.Get(x => x.Id == agreementId).FirstOrDefault()?.SaleManId;
                            if (string.IsNullOrEmpty(salemanId))
                            {
                                salemanId = catContractRepository.Get(x => x.PartnerId == item.grp.PartnerId
                                                                                           && x.OfficeId.Contains(invoiceObhGroup.FirstOrDefault().invc.FirstOrDefault().OfficeId.ToString())).FirstOrDefault()?.SaleManId;
                            }
                            if (!string.IsNullOrEmpty(salemanId))
                            {
                                var employeeData = employeeLst.Where(x => x.Id == salemanId).FirstOrDefault();
                                payment.Salesman = employeeData == null ? string.Empty : employeeData.EmployeeNameEn;
                            }
                            #endregion
                        }

                        // Get creator name
                        if (soaDetail == null) // Billing là cdnote
                        {
                            var creatorId = cdNoteDetail?.UserCreated;
                            payment.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst.Where(x => x.Id == creatorId).FirstOrDefault()?.EmployeeNameEn;
                        }
                        else // Billing là soa
                        {
                            var creatorId = soaDetail?.UserCreated;
                            payment.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst.Where(x => x.Id == creatorId).FirstOrDefault()?.EmployeeNameEn;
                        }

                        results.Add(payment);
                    }
                    #endregion
                }

                if (numOfDebitInvoice != null && numOfDebitInvoice > 1)
                {
                    invoiceObhGroup = null;
                }

                // Get debits billing OR 1debit with 1obh
                var invoiceDebitGroupLst = invoiceDebitGroup.ToList();
                for (var i = 0; i < invoiceDebitGroupLst.Count(); i++)
                {
                    var invoiceDe = invoiceDebitGroupLst[i];
                    var payment = new AccountingCustomerPaymentExport();
                    var statusOBH = string.Empty;
                    var statusDebit = "Unpaid";
                    var isValidDebit = true;
                    if (invoiceDe.invc.Count() > 0)
                    {
                        statusDebit = invoiceDe.invc.FirstOrDefault().Status; // Lay status hien tai cua inv debit nhung sau do tinh lai theo paid amount
                    }
                    if (invoiceObhGroup != null && invoiceObhGroup.Count() > 0)
                    {
                        // Check if obh payment have valid status on search
                        // Lay status hien tai cua inv obh nhung sau do tinh lai theo paid amount
                        var unpaidOBH = invoiceObhGroup.Sum(x => x?.invc.FirstOrDefault().UnpaidAmountVnd ?? 0);
                        var totalPaidOBH = invoiceObhGroup.Sum(x => x?.invc.FirstOrDefault().TotalAmountVnd ?? 0);
                        if (unpaidOBH <= 0)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                        }
                        else if (unpaidOBH > 0 && unpaidOBH < totalPaidOBH)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                        }
                        else
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                        }
                    }
                    #region CR cal debit
                    payment.UnpaidAmountInv = invoiceDe.invc.FirstOrDefault()?.TotalAmountVnd ?? 0;
                    payment.UnpaidAmountOBH = isValidObh ? invoiceObhGroup?.Sum(x => x?.invc.FirstOrDefault()?.TotalAmountVnd ?? 0) ?? 0 : 0;
                    payment.UnpaidAmountInvUsd = invoiceDe.invc.FirstOrDefault()?.TotalAmountUsd ?? 0;
                    payment.UnpaidAmountOBHUsd = isValidObh ? invoiceObhGroup?.Sum(x => x?.invc.FirstOrDefault()?.TotalAmountUsd ?? 0) ?? 0 : 0;

                    payment.PaidAmount = payment.PaidAmountUsd = 0;
                    payment.PaidAmountOBH = payment.PaidAmountOBHUsd = 0;
                    payment.receiptDetail = new List<AccountingReceiptDetail>();
                    DateTime? dateTo = null;
                    if (criteria.DueDate != null || criteria.IssuedDate != null)
                    {
                        dateTo = criteria.IssuedDate != null ? criteria.IssuedDate.Value.Date : criteria.DueDate.Value.Date;
                    }
                    var receiptGroup = item.payment.Where(x => !string.IsNullOrEmpty(x.PaymentRefNo) && (numOfDebitInvoice == 1 || (numOfDebitInvoice > 1 && x.invoicePayment == invoiceDe.Key.InvoiceNoReal)) && (dateTo == null || x.PaymentDate.Value.Date <= dateTo.Value.Date)).GroupBy(x => new { x.ReceiptId, x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentType, z.PaymentDate, z.PaymentDatetimeCreated, z.AgreementId, z.CusAdvanceAmountVnd, z.CusAdvanceAmountUsd, z.PaymentAmountVnd, z.PaymentAmountUsd, z.UnpaidPaymentAmountVnd, z.UnpaidPaymentAmountUsd }) }).OrderBy(x => x.Payment.FirstOrDefault().PaymentDatetimeCreated);
                    if (receiptGroup != null && receiptGroup.Count() > 0)
                    {
                        var debitGrp = receiptGroup.Where(z => z.Payment.Any(pm => pm.PaymentType == "DEBIT")).FirstOrDefault()?.Payment.Where(z => z.PaymentType == "DEBIT").FirstOrDefault();
                        if (!string.IsNullOrEmpty(statusDebit) && statusDebit != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID && debitGrp != null)
                        {
                            payment.UnpaidAmountInv = debitGrp?.UnpaidPaymentAmountVnd ?? 0;
                            payment.UnpaidAmountInvUsd = debitGrp?.UnpaidPaymentAmountUsd ?? 0;
                        }
                        if (!string.IsNullOrEmpty(statusOBH) && isValidObh && statusOBH != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID)
                        {
                            var obhGrp = receiptGroup.Where(z => z.Payment.Any(pm => pm.PaymentType == "OBH")).FirstOrDefault()?.Payment.Where(z => z.PaymentType == "OBH").FirstOrDefault();
                            payment.UnpaidAmountOBH = isValidObh ? obhGrp == null ? payment.UnpaidAmountOBH : obhGrp?.UnpaidPaymentAmountVnd ?? 0 : 0;
                            payment.UnpaidAmountOBHUsd = isValidObh ? obhGrp == null ? payment.UnpaidAmountOBHUsd : obhGrp?.UnpaidPaymentAmountUsd ?? 0 : 0;
                        }
                        if (debitGrp != null || isValidObh)
                        {
                            foreach (var rcp in receiptGroup)
                            {
                                var detail = new AccountingReceiptDetail();
                                detail.ReceiptId = rcp.grp.ReceiptId;
                                detail.PaymentRefNo = rcp.grp.PaymentRefNo;
                                detail.PaymentDate = rcp.Payment.FirstOrDefault()?.PaymentDate;
                                var paymentDebit = rcp.Payment.Where(z => z.PaymentType == "DEBIT").FirstOrDefault();
                                var paymentOBH = rcp.Payment.Where(z => z.PaymentType == "OBH");
                                detail.PaidAmount = paymentDebit?.PaymentAmountVnd ?? 0;
                                detail.PaidAmountOBH = isValidObh ? (paymentOBH != null ? paymentOBH.Sum(x => x.PaymentAmountVnd ?? 0) : 0) : 0;
                                detail.PaidAmountUsd = paymentDebit?.PaymentAmountUsd ?? 0;
                                detail.PaidAmountOBHUsd = isValidObh ? (paymentOBH != null ? paymentOBH.Sum(x => x.PaymentAmountUsd ?? 0) : 0) : 0;
                                detail.CusAdvanceAmountVnd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountVnd ?? 0;
                                detail.CusAdvanceAmountUsd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountUsd ?? 0;
                                detail.AgreementId = rcp.Payment.FirstOrDefault()?.AgreementId;
                                //if (detail.PaidAmount > 0 || detail.PaidAmountOBH > 0 || detail.PaidAmountUsd > 0 || detail.PaidAmountOBHUsd > 0)
                                {
                                    payment.PaidAmount += (detail.PaidAmount ?? 0);
                                    payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                                    payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                                    payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                                    payment.receiptDetail.Add(detail);
                                }
                            }
                        }
                    }

                    var invoiceDebitPaid = invoiceDe == null ? null : invoiceDe.invc.Where(x => x.SourceModified != null && x.SourceModified == "1" && x.Status == "Paid");
                    var invoiceObhPaid = invoiceObhGroup == null ? null : invoiceObhGroup.Where(x => x.invc.FirstOrDefault().SourceModified != null && x.invc.FirstOrDefault().SourceModified == "1" && x.invc.FirstOrDefault().Status == "Paid");
                    if ((invoiceDebitPaid != null && invoiceDebitPaid.Count() > 0) || (invoiceObhPaid != null && invoiceObhPaid.Count() > 0))
                    {
                        var detail = new AccountingReceiptDetail();
                        detail.ReceiptId = Guid.Empty;
                        detail.PaymentRefNo = "";
                        detail.PaymentDate = null;
                        detail.PaidAmount = invoiceDebitPaid.FirstOrDefault()?.PaidAmountVnd ?? 0;
                        detail.PaidAmountOBH = isValidObh ? (invoiceObhPaid?.Sum(z => z.invc.FirstOrDefault()?.PaidAmountVnd ?? 0) ?? 0) : 0;
                        detail.PaidAmountUsd = invoiceDebitPaid.FirstOrDefault()?.PaidAmountUsd ?? 0;
                        detail.PaidAmountOBHUsd = isValidObh ? (invoiceObhPaid?.Sum(z => z.invc.FirstOrDefault()?.PaidAmountUsd ?? 0) ?? 0) : 0;
                        //if (detail.PaidAmount > 0 || detail.PaidAmountOBH > 0 || detail.PaidAmountUsd > 0 || detail.PaidAmountOBHUsd > 0)
                        {
                            payment.PaidAmount += (detail.PaidAmount ?? 0);
                            payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                            payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                            payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                            payment.receiptDetail.Add(detail);
                        }
                    }

                    invoiceDebitPaid = invoiceDe.invc;
                    if (dateTo != null && payment.receiptDetail.Count() == 0)
                    {
                        var pmType = invoiceDebitPaid?.Count() > 0 ? "DEBIT" : string.Empty;
                        pmType += invoiceObhGroup?.Count() > 0 ? (string.IsNullOrEmpty(pmType) ? "OBH" : ";OBH") : string.Empty;
                        var pmDetail = from pm in paymentData.Where(x => x.BillingRefNo == item.grp.BillingRefNo && pmType.Contains(x.PaymentType) && (numOfDebitInvoice == 1 || (numOfDebitInvoice > 1 && x.InvoiceNo == invoiceDe.Key.InvoiceNoReal)))
                                       join rcp in receiptData on pm.ReceiptId equals rcp.Id
                                       select new
                                       {
                                           pm.ReceiptId,
                                           pm.PaymentType,
                                           pm.PaymentAmountVnd,
                                           pm.PaymentAmountUsd,
                                           pm.UnpaidPaymentAmountVnd,
                                           pm.UnpaidPaymentAmountUsd,
                                           rcp.PaymentDate,
                                           PaymentDatetimeCreated = rcp.DatetimeCreated,
                                           rcp.PaymentRefNo,
                                           rcp.AgreementId,
                                           rcp.CusAdvanceAmountUsd,
                                           rcp.CusAdvanceAmountVnd
                                       };
                        var receiptRMGroup = pmDetail.Where(x => x.PaymentDate.Value.Date > dateTo.Value.Date).GroupBy(x => new { x.ReceiptId, x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentType, z.PaymentDate, z.PaymentDatetimeCreated, z.AgreementId, z.CusAdvanceAmountVnd, z.CusAdvanceAmountUsd, z.PaymentAmountVnd, z.PaymentAmountUsd, z.UnpaidPaymentAmountVnd, z.UnpaidPaymentAmountUsd }) }).OrderBy(x => x.Payment.FirstOrDefault().PaymentDatetimeCreated);
                        if (receiptRMGroup?.Count() > 0)
                        {
                            var debitGrp = receiptRMGroup.Where(z => z.Payment.Any(pm => pm.PaymentType == "DEBIT")).FirstOrDefault()?.Payment.Where(z => z.PaymentType == "DEBIT").FirstOrDefault();
                            var obhGrp = receiptRMGroup.Where(z => z.Payment.Any(pm => pm.PaymentType == "OBH")).FirstOrDefault()?.Payment.Where(z => z.PaymentType == "OBH").FirstOrDefault();

                            if (!string.IsNullOrEmpty(statusDebit) && statusDebit != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID && debitGrp != null)
                            {
                                payment.UnpaidAmountInv = debitGrp?.UnpaidPaymentAmountVnd ?? 0;
                                payment.UnpaidAmountInvUsd = debitGrp?.UnpaidPaymentAmountUsd ?? 0;
                            }
                            if (!string.IsNullOrEmpty(statusOBH) && isValidObh && statusOBH != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID && obhGrp != null)
                            {
                                payment.UnpaidAmountOBH = isValidObh ? obhGrp?.UnpaidPaymentAmountVnd ?? 0 : 0;
                                payment.UnpaidAmountOBHUsd = isValidObh ? obhGrp?.UnpaidPaymentAmountUsd ?? 0 : 0;

                            }
                        }
                    }

                    // Get status obh invoice theo thoi gian duoc chon
                    if ((payment.UnpaidAmountOBH - payment.PaidAmountOBH) == payment.UnpaidAmountOBH)
                    {
                        statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                    }
                    else if ((payment.UnpaidAmountOBH - payment.PaidAmountOBH) > 0)
                    {
                        statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                    }
                    else if ((payment.UnpaidAmountOBH - payment.PaidAmountOBH) == 0)
                    {
                        statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                    }
                    if (criteria.PaymentStatus.Count > 0 && !criteria.PaymentStatus.Contains(statusOBH))
                    {
                        isValidObh = false;
                    }

                    if (!isValidObh && invoiceDe.invc.Count() == 0) // if invoice obh invalid and invoice debit is null then break
                    {
                        continue;
                    }
                    if (criteria.FromUpdatedDate != null && invoiceObhGroup != null && invoiceObhGroup.Count() > 0)
                    {
                        var validObh = (item.payment.Any(x => x.Type == "OBH" && string.IsNullOrEmpty(x.PaymentRefNo)) && statusOBH == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID) || (item.payment.Any(x => x.Type == "OBH" && x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date) && isValidObh);
                        if (!validObh)
                        {
                            // if invoice obh is invalid => reset value to 0
                            invoiceObhGroup = null;
                            payment.UnpaidAmountOBH = payment.UnpaidAmountOBHUsd = payment.PaidAmountOBH = payment.PaidAmountOBHUsd = 0;
                            if (invoiceDe.invc.Count() == 0)  // if invoice obh invalid and invoice debit is null then break
                            {
                                continue;
                            }
                            foreach (var obh in payment.receiptDetail)
                            {
                                obh.PaidAmountOBH = obh.PaidAmountOBHUsd = 0;
                            }
                        }
                    }
                    else
                    {
                        if (!isValidObh)
                        {
                            // if invoice obh is invalid => reset value to 0
                            invoiceObhGroup = null;
                            payment.UnpaidAmountOBH = payment.UnpaidAmountOBHUsd = payment.PaidAmountOBH = payment.PaidAmountOBHUsd = 0;
                            foreach (var obh in payment.receiptDetail)
                            {
                                obh.PaidAmountOBH = obh.PaidAmountOBHUsd = 0;
                            }
                        }
                    }

                    // Get status debit invoice theo thoi gian duoc chon
                    if ((payment.UnpaidAmountInv - payment.PaidAmount) == payment.UnpaidAmountInv)
                    {
                        statusDebit = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                    }
                    else if ((payment.UnpaidAmountInv - payment.PaidAmount) > 0)
                    {
                        statusDebit = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                    }
                    else if ((payment.UnpaidAmountInv - payment.PaidAmount) == 0)
                    {
                        statusDebit = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                    }
                    if (criteria.PaymentStatus.Count > 0 && !criteria.PaymentStatus.Contains(statusDebit))
                    {
                        isValidDebit = false;
                    }
                    if (!isValidDebit)
                    {
                        // if invoice debit is invalid => reset value to 0
                        invoiceDe = null;
                        payment.UnpaidAmountInv = payment.UnpaidAmountInvUsd = payment.PaidAmount = payment.PaidAmountUsd = 0;
                        // if invoice debit invalid and invoice obh is null then break
                        if (!isValidObh || invoiceObhGroup == null || invoiceObhGroup.Count() == 0)
                        {
                            continue;
                        }
                        foreach (var db in payment.receiptDetail)
                        {
                            db.PaidAmountOBH = db.PaidAmountOBHUsd = 0;
                        }
                    }
                    payment.receiptDetail = payment.receiptDetail.Where(detail => detail.PaidAmount != 0 || detail.PaidAmountOBH != 0 || detail.PaidAmountUsd != 0 || detail.PaidAmountOBHUsd != 0).ToList();
                    #endregion
                    // get info remain columns if inv debit or obh has valid value
                    var sur = item.surcharge.FirstOrDefault();
                    payment.PartnerId = item.grp.PartnerId;
                    payment.PartnerCode = item.grp.PartnerCode;
                    payment.PartnerName = item.grp.PartnerName;
                    payment.ParentCode = item.grp.ParentCode == null ? string.Empty : partners.Where(x => x.Id == item.grp.ParentCode).FirstOrDefault()?.AccountNo;
                    payment.InvoiceNo = invoiceDe?.invc.Count() > 0 ? invoiceDe.invc.FirstOrDefault()?.InvoiceNoReal : null;
                    payment.InvoiceDate = invoiceDe?.invc.Count() > 0 ? invoiceDe.invc.FirstOrDefault()?.IssuedDate : invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.IssuedDate;
                    payment.BillingRefNo = item.grp.BillingRefNo;
                    payment.BillingDate = invoiceDe?.invc.FirstOrDefault()?.ConfirmBillingDate;
                    payment.DueDate = invoiceDe?.invc.Count() > 0 ? invoiceDe?.invc.FirstOrDefault()?.DueDate : invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.DueDate;
                    payment.OverdueDays = invoiceDe?.invc.Count() > 0 ? invoiceDe?.invc.FirstOrDefault()?.OverdueDays : invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.OverdueDays;
                    payment.PaymentTerm = invoiceDe?.invc.Count() > 0 ? invoiceDe?.invc.FirstOrDefault()?.PaymentTerm : invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.PaymentTerm;
                    if (invoiceDe?.invc.Count() > 0)
                    {
                        payment.AccountNo = invoiceDe.invc.FirstOrDefault()?.AccountNo;
                    }
                    if (invoiceObhGroup?.Count() > 0)
                    {
                        payment.AccountNo = (string.IsNullOrEmpty(payment.AccountNo) ? "" : (payment.AccountNo + " / ")) + invoiceObhGroup.FirstOrDefault().invc.FirstOrDefault()?.AccountNo;
                    }
                    payment.BranchName = invoiceDe?.invc.Count() > 0 ? officeData[(Guid)invoiceDe.invc.FirstOrDefault()?.OfficeId].FirstOrDefault()?.ShortName : officeData[(Guid)invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.OfficeId].FirstOrDefault()?.ShortName;
                    payment.CombineNo = item.surcharge.Where(x => !string.IsNullOrEmpty(x.CombineNo)).FirstOrDefault()?.CombineNo;
                    if (item.grp.BillingRefNoType == "DEBIT")
                    {
                        payment.JobNo = sur?.JobNo;
                        payment.MBL = sur?.Mblno;
                        payment.HBL = sur?.Hblno;
                        payment.CustomNo = customsDeclarationRepository.Get(x => x.JobNo == sur.JobNo).FirstOrDefault()?.ClearanceNo;
                    }
                    // Get saleman name
                    var soaDetail = soaLst[item.grp.BillingRefNo].FirstOrDefault();
                    var cdNoteDetail = cdNoteLst[item.grp.BillingRefNo].FirstOrDefault();
                    if (cdNoteDetail != null) // Billing là cdnote
                    {
                        // Get saleman of shipment
                        var salemanOfShipment = opsLookup[cdNoteDetail.JobId].FirstOrDefault()?.SalemanId;
                        if (salemanOfShipment == null)
                        {
                            salemanOfShipment = csTransactionDetailRepository.Get(x => x.Id == item.surcharge.FirstOrDefault().Hblid).FirstOrDefault()?.SaleManId; // air/sea lấy saleman đại diện
                        }
                        if (!string.IsNullOrEmpty(salemanOfShipment))
                        {
                            var employeeData = employeeLst.Where(x => x.Id == salemanOfShipment).FirstOrDefault();
                            payment.Salesman = employeeData == null ? string.Empty : employeeData.EmployeeNameEn;
                        }
                    }
                    if (soaDetail != null) // Billing là soa
                    {
                        #region saleman of contract
                        var agreementId = item.payment.Where(x => x.AgreementId != null).FirstOrDefault()?.AgreementId;
                        var salemanId = catContractRepository.Get(x => x.Id == agreementId).FirstOrDefault()?.SaleManId;
                        if (string.IsNullOrEmpty(salemanId))
                        {
                            salemanId = catContractRepository.Get(x => x.PartnerId == item.grp.PartnerId
                                                                                          && x.OfficeId.Contains(item.invoice.Where(z => z.OfficeId != null).FirstOrDefault().OfficeId.ToString())).FirstOrDefault()?.SaleManId;
                        }
                        if (!string.IsNullOrEmpty(salemanId))
                        {
                            var employeeId = employeeLst.Where(x => x.Id == salemanId).FirstOrDefault();
                            payment.Salesman = employeeId == null ? string.Empty : employeeId.EmployeeNameEn;
                        }
                        #endregion
                    }

                    // Get creator name
                    if (soaDetail == null) // Billing là cdnote
                    {
                        var creatorId = cdNoteDetail?.UserCreated;
                        payment.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst.Where(x => x.Id == creatorId).FirstOrDefault()?.EmployeeNameEn;
                    }
                    else // Billing là soa
                    {
                        var creatorId = soaDetail?.UserCreated;
                        payment.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst.Where(x => x.Id == creatorId).FirstOrDefault()?.EmployeeNameEn;
                    }

                    results.Add(payment);
                }
            }

            results = results?.OrderBy(x => x.PartnerId).ThenBy(x => x.InvoiceDate).ThenBy(x => x.InvoiceNo).ToList();

            // Caculate advance amount
            // [CR:23/11/21] update get adv amount data with payment type and receipt method
            #region Get ADVANCE AMOUNT row
            List<string> methodsAdv = new List<string> {
                AccountingConstants.PAYMENT_METHOD_CLEAR_ADVANCE,
                AccountingConstants.PAYMENT_METHOD_CLEAR_ADVANCE_BANK,
                AccountingConstants.PAYMENT_METHOD_CLEAR_ADVANCE_CASH,
                AccountingConstants.PAYMENT_METHOD_COLL_INTERNAL,
            };
            var receiptMethod = from rcpt in receiptData.Where(x => methodsAdv.Any(z => z == x.PaymentMethod) && (string.IsNullOrEmpty(criteria.PartnerId) || criteria.PartnerId == x.CustomerId))
                                join payment in DataContext.Get(x => (string.IsNullOrEmpty(criteria.PartnerId) || criteria.PartnerId == x.PartnerId)) on rcpt.Id equals payment.ReceiptId
                                select new
                                {
                                    PartnerId = payment.PartnerId,
                                    payment.OfficeId,
                                    CusAdvanceAmountUsd = rcpt.CusAdvanceAmountUsd ?? 0,
                                    CusAdvanceAmountVnd = rcpt.CusAdvanceAmountVnd ?? 0,
                                    rcpt.PaymentDate,
                                    ReceiptTimeCreated = rcpt.DatetimeCreated,
                                    AgreementAdvanceAmountUsd = rcpt.AgreementAdvanceAmountUsd ?? 0,
                                    AgreementAdvanceAmountVnd = rcpt.AgreementAdvanceAmountVnd ?? 0,
                                    rcpt.AgreementId
                                };
            var receiptAdv = from payment in DataContext.Get(x => x.Type == AccountingConstants.PAYMENT_TYPE_CODE_ADVANCE || x.Type == AccountingConstants.PAYMENT_TYPE_CODE_COLLECT_OBH && (string.IsNullOrEmpty(criteria.PartnerId) || criteria.PartnerId == x.PartnerId))
                             join rcpt in receiptData on payment.ReceiptId equals rcpt.Id
                             select new
                             {
                                 PartnerId = payment.PartnerId,
                                 payment.OfficeId,
                                 CusAdvanceAmountUsd = rcpt.CusAdvanceAmountUsd ?? 0,
                                 CusAdvanceAmountVnd = rcpt.CusAdvanceAmountVnd ?? 0,
                                 rcpt.PaymentDate,
                                 ReceiptTimeCreated = rcpt.DatetimeCreated,
                                 AgreementAdvanceAmountUsd = rcpt.AgreementAdvanceAmountUsd ?? 0,
                                 AgreementAdvanceAmountVnd = rcpt.AgreementAdvanceAmountVnd ?? 0,
                                 rcpt.AgreementId
                             };
            var paymentAdv = receiptMethod.Union(receiptAdv);

            var listAdvRemain = new List<AccountingCustomerPaymentExport>();
            var grpPartner = results.GroupBy(x => x.PartnerId).Select(x => x);
            if (!string.IsNullOrEmpty(criteria.PartnerId)) // if search only 1 partner then get this partner
            {
                partners = partners.Where(x => x.Id == criteria.PartnerId);
            }
            // [CR:23/11/21: get adv amount với PT có payment date gần nhất]
            if (criteria.IssuedDate != null || criteria.DueDate != null || criteria.FromUpdatedDate != null)
            {
                if (criteria.IssuedDate != null)
                {
                    paymentAdv = paymentAdv.Where(x => x.PaymentDate.Value.Date <= criteria.IssuedDate.Value.Date);
                }
                else if (criteria.DueDate != null)
                {
                    paymentAdv = paymentAdv.Where(x => x.PaymentDate.Value.Date <= criteria.DueDate.Value.Date);
                }
                else if (criteria.FromUpdatedDate != null)
                {
                    paymentAdv = paymentAdv.Where(x => (x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date));
                }
                var customerAdvLst = paymentAdv.Select(z => z.PartnerId).ToList();
                partners = partners.Where(x => customerAdvLst.Any(z => z == x.Id));
                foreach (var item in partners)
                {
                    var payment = new AccountingCustomerPaymentExport();
                    var pm = paymentAdv.Where(x => x.PartnerId == item.Id);
                    var pmAdvOrder = pm.OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.ReceiptTimeCreated).FirstOrDefault();
                    if (pmAdvOrder != null)
                    {
                        var dataCustomerInList = grpPartner.Where(x => x.Key == item.Id).FirstOrDefault();
                        var indexOfLastGrp = dataCustomerInList == null ? 0 : results.IndexOf(dataCustomerInList.Last());
                        payment.PartnerId = item.Id;
                        payment.PartnerCode = item.AccountNo;
                        payment.PartnerName = item.ShortName;
                        if (indexOfLastGrp > 0)
                        {
                            payment.ParentCode = dataCustomerInList.FirstOrDefault().ParentCode;
                        }
                        else
                        {
                            var parentData = partners.Where(x => x.Id == item.ParentId).FirstOrDefault();
                            payment.ParentCode = parentData?.AccountNo;
                        }
                        payment.BillingRefNo = "ADVANCE AMOUNT";
                        payment.BranchName = officeData[(Guid)pmAdvOrder.OfficeId].FirstOrDefault()?.ShortName;
                        var saleManId = catContractRepository.Get(x => x.Id == pmAdvOrder.AgreementId).FirstOrDefault()?.SaleManId;
                        if (!string.IsNullOrEmpty(saleManId))
                        {
                            var employeeId = employeeLst.Where(x => x.Id == saleManId).FirstOrDefault();
                            payment.Salesman = employeeId == null ? string.Empty : employeeId.EmployeeNameEn;
                        }
                        else
                        {
                            saleManId = catContractRepository.Get(x => x.Active == true && x.PartnerId == item.Id).FirstOrDefault()?.SaleManId;
                            payment.Salesman = employeeLst.Where(x => x.Id == saleManId).FirstOrDefault()?.EmployeeNameEn;
                        }
                        payment.AdvanceAmountVnd = pmAdvOrder.AgreementAdvanceAmountVnd;
                        payment.AdvanceAmountUsd = pmAdvOrder.AgreementAdvanceAmountUsd;
                        if (payment.AdvanceAmountVnd > 0 || payment.AdvanceAmountUsd > 0)
                        {
                            if (indexOfLastGrp > 0)
                            {
                                results.Insert(indexOfLastGrp + 1, payment);
                            }
                            else
                            {
                                listAdvRemain.Add(payment);
                            }
                        }
                    }
                }
            }
            #region Remove Old <=[CR:23/11/21: get adv amount với PT có payment date gần nhất]
            //var paymentAdv = from dataAdv in DataContext.Get(x => x.Type == "ADV" || x.Type == "COLL_OBH")
            //                 join rcpt in receiptData on dataAdv.ReceiptId equals rcpt.Id
            //                 select new
            //                 {
            //                     dataAdv,
            //                     CusAdvanceAmountUsd = rcpt.CusAdvanceAmountUsd ?? 0,
            //                     CusAdvanceAmountVnd = rcpt.CusAdvanceAmountVnd ?? 0,
            //                     rcpt.PaymentDate,
            //                     AgreementAdvanceAmountUsd = rcpt.AgreementAdvanceAmountUsd ?? 0,
            //                     AgreementAdvanceAmountVnd = rcpt.AgreementAdvanceAmountVnd ?? 0
            //                 };
            //if (criteria.IssuedDate != null)
            //{
            //    foreach (var item in grpPartner)
            //    {
            //        var payment = new AccountingCustomerPaymentExport();
            //        var pm = paymentAdv.Where(x => x.PartnerId == item.Key && x.PaymentDate.Value.Date <= criteria.IssuedDate.Value.Date).OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.ReceiptTimeCreated).FirstOrDefault();

            //        if (pm != null)
            //        {

            //            var indexOfLastGrp = results.IndexOf(item.Last());
            //            payment.PartnerId = item.FirstOrDefault().PartnerId;
            //            payment.PartnerCode = item.FirstOrDefault().PartnerCode;
            //            payment.PartnerName = item.FirstOrDefault().PartnerName;
            //            payment.ParentCode = item.FirstOrDefault().ParentCode;
            //            payment.BillingRefNo = "ADVANCE AMOUNT";
            //            payment.BranchName = officeData[(Guid)pm.OfficeId].FirstOrDefault()?.ShortName;
            //            payment.AdvanceAmountVnd = pm.AgreementAdvanceAmountVnd;
            //            payment.AdvanceAmountUsd = pm.AgreementAdvanceAmountUsd;
            //            if (payment.AdvanceAmountVnd > 0 || payment.AdvanceAmountUsd > 0)
            //            {
            //                results.Insert(indexOfLastGrp + 1, payment);
            //            }
            //        }
            //    }
            //}
            //else if (criteria.DueDate != null)
            //{
            //    foreach (var item in grpPartner)
            //    {
            //        var payment = new AccountingCustomerPaymentExport();
            //        var pm = paymentAdv.Where(x => x.PartnerId == item.Key && x.PaymentDate.Value.Date <= criteria.DueDate.Value.Date).OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.ReceiptTimeCreated).FirstOrDefault();

            //        if (pm != null)
            //        {

            //            var indexOfLastGrp = results.IndexOf(item.Last());
            //            payment.PartnerId = item.FirstOrDefault().PartnerId;
            //            payment.PartnerCode = item.FirstOrDefault().PartnerCode;
            //            payment.PartnerName = item.FirstOrDefault().PartnerName;
            //            payment.ParentCode = item.FirstOrDefault().ParentCode;
            //            payment.BillingRefNo = "ADVANCE AMOUNT";
            //            payment.BranchName = officeData[(Guid)pm.OfficeId].FirstOrDefault()?.ShortName;
            //            payment.AdvanceAmountVnd = pm.AgreementAdvanceAmountVnd;
            //            payment.AdvanceAmountUsd = pm.AgreementAdvanceAmountUsd;
            //            if (payment.AdvanceAmountVnd > 0 || payment.AdvanceAmountUsd > 0)
            //            {
            //                results.Insert(indexOfLastGrp + 1, payment);
            //            }
            //        }
            //    }
            //}
            //else if (criteria.FromUpdatedDate != null)
            //{
            //    foreach (var item in grpPartner)
            //    {
            //        var payment = new AccountingCustomerPaymentExport();
            //        var receiptList = new List<AccountingReceiptDetail>();
            //        foreach (var it in item)
            //        {
            //            receiptList.AddRange(it.receiptDetail);
            //        }
            //        var pm = paymentAdv.Where(x => x.PartnerId == item.FirstOrDefault().PartnerId && (x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date));

            //        foreach (var dt in pm)
            //        {
            //            var detail = new AccountingReceiptDetail();
            //            detail.ReceiptId = dt.dataAdv.ReceiptId;
            //            detail.CusAdvanceAmountVnd = dt.CusAdvanceAmountVnd;
            //            detail.CusAdvanceAmountUsd = dt.CusAdvanceAmountUsd;
            //            receiptList.Add(detail);
            //        }

            //        receiptList = receiptList.GroupBy(x => x.ReceiptId).Select(x => x.FirstOrDefault()).ToList();
            //        if (pm != null && pm.Count() > 0)
            //        {
            //            var indexOfLastGrp = results.IndexOf(item.Last());
            //            payment.PartnerId = item.FirstOrDefault().PartnerId;
            //            payment.PartnerCode = item.FirstOrDefault().PartnerCode;
            //            payment.PartnerName = item.FirstOrDefault().PartnerName;
            //            payment.ParentCode = item.FirstOrDefault().ParentCode;
            //            payment.BillingRefNo = "ADVANCE AMOUNT";
            //            payment.AdvanceAmountVnd = pm.Sum(x => x.dataAdv.PaymentAmountVnd ?? 0);
            //            payment.AdvanceAmountVnd += receiptList.Sum(x => x.CusAdvanceAmountVnd ?? 0);

            //            payment.AdvanceAmountUsd = pm.Sum(x => x.dataAdv.PaymentAmountUsd ?? 0);
            //            payment.AdvanceAmountUsd += receiptList.Sum(x => x.CusAdvanceAmountUsd ?? 0);
            //            payment.BranchName = pm.FirstOrDefault().dataAdv == null ? string.Empty : officeData[(Guid)pm.FirstOrDefault().dataAdv.OfficeId].FirstOrDefault()?.ShortName;
            //            if (payment.AdvanceAmountVnd > 0 || payment.AdvanceAmountUsd > 0)
            //            {
            //                results.Insert(indexOfLastGrp + 1, payment);
            //            }
            //        }
            //    }
            //}
            #endregion
            else // Case: filter without date search
            {
                var customerAdvLst = paymentAdv.Select(z => z.PartnerId).ToList();
                partners = partners.Where(x => customerAdvLst.Any(z => z == x.Id));
                foreach (var item in partners)
                {
                    var payment = new AccountingCustomerPaymentExport();
                    var agreementIds = new List<Guid>();
                    var dataCustomerInList = grpPartner.Where(x => x.Key == item.Id).FirstOrDefault();
                    var indexOfLastGrp = dataCustomerInList == null ? 0 : results.IndexOf(dataCustomerInList.Last());
                    if (indexOfLastGrp > 0)
                    {
                        foreach (var it in dataCustomerInList)
                        {
                            if (it.receiptDetail != null)
                            {
                                agreementIds.AddRange(it.receiptDetail.Where(x => x.AgreementId != null && agreementIds.Any(z => z == x.AgreementId)).Select(x => (Guid)x.AgreementId));
                            }
                        }
                    }
                    payment.PartnerId = item.Id;
                    payment.PartnerCode = item.AccountNo;
                    payment.PartnerName = item.ShortName;
                    if (indexOfLastGrp > 0)
                    {
                        payment.ParentCode = dataCustomerInList.FirstOrDefault().ParentCode;
                    }
                    else
                    {
                        var parentData = partners.Where(x => x.Id == item.ParentId).FirstOrDefault();
                        payment.ParentCode = parentData?.AccountNo;
                    }
                    payment.BillingRefNo = "ADVANCE AMOUNT";
                    var contractInfo = agreementIds.Count > 0 ? catContractRepository.Get(x => agreementIds.Any(ag => ag == x.Id)) : catContractRepository.Get(x => x.PartnerId == item.Id && x.Active == true);
                    // (criteria.Office == null || criteria.Office.Count == 0 || x.OfficeId.Split(';', StringSplitOptions.RemoveEmptyEntries).Any(z => criteria.Office.Contains(z))) case office in contract
                    if (contractInfo?.Count() > 0)
                    {
                        var saleManId = contractInfo.FirstOrDefault().SaleManId;
                        var employeeId = employeeLst.Where(x => x.Id == saleManId).FirstOrDefault();
                        payment.Salesman = employeeId == null ? string.Empty : employeeId.EmployeeNameEn;
                    }
                    var office = paymentAdv.Where(x => x.PartnerId == item.Id).FirstOrDefault()?.OfficeId;
                    payment.BranchName = office == null ? string.Empty : officeData[(Guid)office].FirstOrDefault()?.ShortName;
                    payment.AdvanceAmountVnd = contractInfo.Sum(x => x.CustomerAdvanceAmountVnd ?? 0);
                    payment.AdvanceAmountUsd = contractInfo.Sum(x => x.CustomerAdvanceAmountUsd ?? 0);
                    if (payment.AdvanceAmountVnd > 0 || payment.AdvanceAmountUsd > 0)
                    {
                        if (indexOfLastGrp > 0)
                        {
                            results.Insert(indexOfLastGrp + 1, payment);
                        }
                        else
                        {
                            listAdvRemain.Add(payment);
                        }
                    }
                }
            }
            if(listAdvRemain.Count > 0)
            {
                results.AddRange(listAdvRemain);
            }
            #endregion
            return results.AsQueryable();
        }

        /// <summary>
        /// Get data export Statement of Receivable Customers
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IQueryable<AccountingCustomerPaymentExport> GetDataExportAccountingCustomerPayment_Origin(PaymentCriteria criteria)
        {
            var data = Query(criteria);
            if (data == null) return null;
            var partners = partnerRepository.Get(x => x.PartnerType == "Customer" && x.Active == true);
            var paymentData = QueryInvoiceDataPayment(criteria);
            var surchargeData = surchargeRepository.Get(x => x.AcctManagementId != null);
            var receiptData = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE);
            var officeData = sysOfficeRepository.Get().ToLookup(x => x.Id);
            var resultsQuery = (from invoice in data
                                join surcharge in surchargeData on invoice.RefId.ToLower() equals surcharge.AcctManagementId.ToString()
                                join part in partners on invoice.PartnerId equals part.Id
                                join payments in paymentData on invoice.RefId.ToLower() equals payments.RefId into grpPayment
                                from payment in grpPayment.DefaultIfEmpty()
                                join rcpts in receiptData on (payment == null ? Guid.Empty : payment.ReceiptId) equals rcpts.Id into grpReceipts
                                from rcpt in grpReceipts.DefaultIfEmpty()
                                select new
                                {
                                    invoice,
                                    payment,
                                    surcharge.JobNo,
                                    surcharge.Hblno,
                                    surcharge.Mblno,
                                    PartnerCode = part.AccountNo,
                                    ParentCode = part != null ? part.ParentId : string.Empty,
                                    PartnerName = part.ShortName,
                                    BillingRefNo = string.IsNullOrEmpty(surcharge.Soano) ? surcharge.DebitNo : surcharge.Soano,
                                    BillingRefNoType = string.IsNullOrEmpty(surcharge.Soano) ? "DEBIT" : "SOA",
                                    Type = surcharge.Type == "OBH" ? "OBH" : "DEBIT",
                                    PaymentRefNo = rcpt == null ? null : rcpt.PaymentRefNo,
                                    PaymentDate = rcpt == null ? null : rcpt.PaymentDate,
                                    CusAdvanceAmountVnd = rcpt == null ? null : rcpt.CusAdvanceAmountVnd,
                                    CusAdvanceAmountUsd = rcpt == null ? null : rcpt.CusAdvanceAmountUsd,
                                    AgreementId = rcpt == null ? null : rcpt.AgreementId,
                                });
            if (criteria.FromUpdatedDate != null)
            {
                //resultsQuery = resultsQuery.Where(x => (x.payment == null || (x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date)));
                resultsQuery = resultsQuery.Where(x => (x.PaymentRefNo == null && (x.Type == "OBH" || (x.Type == "DEBIT" && x.invoice.Status == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID))) || (x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date));
            }
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                if (criteria.SearchType == "ReceiptNo")
                {
                    var listReceiptInfo = receiptData.Where(receipt => criteria.ReferenceNos.Contains(receipt.PaymentRefNo)).Select(x => x.PaymentRefNo).ToList();
                    resultsQuery = resultsQuery.Where(x => listReceiptInfo.Any(z => z == (x.payment == null ? null : x.PaymentRefNo)));
                }
            }

            var resultGroups = resultsQuery.ToList().GroupBy(x => new
            {
                x.invoice.PartnerId,
                x.PartnerCode,
                x.PartnerName,
                x.ParentCode,
                x.BillingRefNo,
                x.BillingRefNoType
            }).Select(x => new { grp = x.Key, invoice = x.Select(z => z.invoice), surcharge = x.Select(z => new { z.JobNo, z.Mblno, z.Hblno }), payment = x.Select(z => new { z.payment?.Id, z.payment?.ReceiptId, z.payment?.PaymentType, z.PaymentRefNo, invoicePayment = z.payment?.InvoiceNo, z.PaymentDate, z.AgreementId, z.CusAdvanceAmountVnd, z.CusAdvanceAmountUsd, z.payment?.PaymentAmountVnd, z.payment?.PaymentAmountUsd, z.payment?.UnpaidPaymentAmountVnd, z.Type }) });
            var results = new List<AccountingCustomerPaymentExport>();
            var soaLst = soaRepository.Get().Select(x => new { x.Soano, x.UserCreated }).ToLookup(x => x.Soano);
            var cdNoteLst = cdNoteRepository.Get().ToLookup(x => x.Code);
            var userLst = userRepository.Get().Select(x => new { x.Id, x.EmployeeId }).ToLookup(x => x.Id);
            var employeeLst = sysEmployeeRepository.Get().Select(x => new { x.Id, x.EmployeeNameEn }).ToLookup(x => x.Id);
            foreach (var item in resultGroups)
            {
                var isValidObh = true;
                //var invoice = item.invoice.GroupBy(x => x.RefId).Select(x => new { invc = x.Select(z => new { z.Type, z.UnpaidAmountVnd, z.TotalAmountVnd, z.TotalAmountUsd, z.InvoiceNoReal, z.IssuedDate, z.ConfirmBillingDate, z.DueDate, z.PaymentTerm, z.OverdueDays, z.AccountNo, z.OfficeId, z.SourceModified, z.PaidAmountVnd, z.PaidAmountUsd, z.Status }) });
                var invoiceDebitGroup = item.invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE).GroupBy(x => new { x.RefId, x.InvoiceNoReal }).Select(x => new { x.Key, invc = x.Select(z => new { z.Type, z.UnpaidAmountVnd, z.TotalAmountVnd, z.TotalAmountUsd, z.InvoiceNoReal, z.IssuedDate, z.ConfirmBillingDate, z.DueDate, z.PaymentTerm, z.OverdueDays, z.AccountNo, z.OfficeId, z.SourceModified, z.PaidAmountVnd, z.PaidAmountUsd, z.Status }) });
                var numOfDebitInvoice = invoiceDebitGroup.Select(x => x.Key.InvoiceNoReal)?.Distinct().Count();
                var invoiceObhGroup = item.invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE).GroupBy(x => new { x.RefId }).Select(x => new { x.Key, invc = x.Select(z => new { z.Type, z.UnpaidAmountVnd, z.TotalAmountVnd, z.TotalAmountUsd, z.InvoiceNoReal, z.IssuedDate, z.ConfirmBillingDate, z.DueDate, z.PaymentTerm, z.OverdueDays, z.AccountNo, z.OfficeId, z.SourceModified, z.PaidAmountVnd, z.PaidAmountUsd, z.Status }) });

                // CR: TH 1 billing with multi invoice
                if (invoiceObhGroup.FirstOrDefault() != null && (numOfDebitInvoice == null || numOfDebitInvoice == 0 || (numOfDebitInvoice != null && numOfDebitInvoice > 1)))
                {
                    #region OBH Detail
                    var payment = new AccountingCustomerPaymentExport();
                    var statusOBH = string.Empty;
                    if (invoiceObhGroup != null && invoiceObhGroup.Count() > 0)
                    {
                        // Check if obh payment have valid status on search
                        var unpaidOBH = invoiceObhGroup.Sum(x => x?.invc.FirstOrDefault().UnpaidAmountVnd ?? 0);
                        var totalPaidOBH = invoiceObhGroup.Sum(x => x?.invc.FirstOrDefault().TotalAmountVnd ?? 0);
                        if (unpaidOBH <= 0)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                        }
                        else if (unpaidOBH > 0 && unpaidOBH < totalPaidOBH)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                        }
                        else
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                        }
                        if (criteria.PaymentStatus.Count > 0 && !criteria.PaymentStatus.Contains(statusOBH))
                        {
                            isValidObh = false;
                        }
                    }

                    if (criteria.FromUpdatedDate != null && invoiceObhGroup != null && invoiceObhGroup.Count() > 0 && isValidObh)
                    {
                        var validObh = (item.payment.Any(x => x.Type == "OBH" && string.IsNullOrEmpty(x.PaymentRefNo)) && statusOBH == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID) || (item.payment.Any(x => x.Type == "OBH" && x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date));
                        if (!validObh)
                        {
                            isValidObh = false;
                        }
                    }
                    if (isValidObh)
                    {
                        
                        payment.PartnerId = item.grp.PartnerId;
                        payment.PartnerCode = item.grp.PartnerCode;
                        payment.PartnerName = item.grp.PartnerName;
                        payment.ParentCode = item.grp.ParentCode == null ? string.Empty : partners.Where(x => x.Id == item.grp.ParentCode).FirstOrDefault()?.AccountNo;
                        payment.InvoiceNo = null;
                        payment.InvoiceDate = invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.IssuedDate;
                        payment.BillingRefNo = item.grp.BillingRefNo;
                        payment.BillingDate = null;
                        payment.DueDate = null;
                        payment.OverdueDays = null;
                        payment.PaymentTerm = null;
                        if (invoiceObhGroup?.Count() > 0)
                        {
                            payment.AccountNo = invoiceObhGroup.FirstOrDefault().invc.FirstOrDefault()?.AccountNo;
                        }
                        payment.BranchName = officeData[(Guid)item.invoice.FirstOrDefault()?.OfficeId].FirstOrDefault()?.ShortName;

                        payment.UnpaidAmountInv = 0;
                        payment.UnpaidAmountOBH = isValidObh ? (invoiceObhGroup?.Sum(x => x?.invc.FirstOrDefault()?.TotalAmountVnd ?? 0) ?? 0) : 0;
                        payment.UnpaidAmountInvUsd = 0;
                        payment.UnpaidAmountOBHUsd = isValidObh ? (invoiceObhGroup?.Sum(x => x?.invc.FirstOrDefault()?.TotalAmountUsd ?? 0) ?? 0) : 0;

                        payment.PaidAmount = payment.PaidAmountUsd = 0;
                        payment.PaidAmountOBH = payment.PaidAmountOBHUsd = 0;
                        // Get saleman name
                        var salemanId = catContractRepository.Get(x => x.Active == true && x.PartnerId == item.grp.PartnerId
                                                                                       && x.OfficeId.Contains(item.invoice.FirstOrDefault().OfficeId.ToString())
                                                                                       && x.SaleService.Contains(item.invoice.FirstOrDefault().ServiceType)).FirstOrDefault()?.SaleManId;
                        if (!string.IsNullOrEmpty(salemanId))
                        {
                            var employeeId = userLst[salemanId].FirstOrDefault()?.EmployeeId;
                            payment.Salesman = salemanId == null ? string.Empty : employeeLst[employeeId].FirstOrDefault().EmployeeNameEn;
                        }
                        // Get creator name
                        var creatorId = soaLst[item.grp.BillingRefNo].FirstOrDefault()?.UserCreated;
                        if (string.IsNullOrEmpty(creatorId))
                        {
                            creatorId = cdNoteLst[item.grp.BillingRefNo].FirstOrDefault()?.UserCreated;
                            var creator = string.IsNullOrEmpty(creatorId) ? string.Empty : userLst[creatorId].FirstOrDefault()?.EmployeeId;
                            payment.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst[creator].FirstOrDefault()?.EmployeeNameEn;
                        }
                        else
                        {
                            var creator = string.IsNullOrEmpty(creatorId) ? string.Empty : userLst[creatorId].FirstOrDefault()?.EmployeeId;
                            payment.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst[creator].FirstOrDefault()?.EmployeeNameEn;
                        }

                        payment.receiptDetail = new List<AccountingReceiptDetail>();
                        var receiptGroup = item.payment.Where(x => x.PaymentType == "OBH" && !string.IsNullOrEmpty(x.PaymentRefNo)).GroupBy(x => new { x.ReceiptId, x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentType, z.PaymentDate, z.AgreementId, z.CusAdvanceAmountVnd, z.CusAdvanceAmountUsd, z.PaymentAmountVnd, z.PaymentAmountUsd, z.UnpaidPaymentAmountVnd }) });
                        if (receiptGroup != null && receiptGroup.Count() > 0)
                        {
                            foreach (var rcp in receiptGroup)
                            {
                                var detail = new AccountingReceiptDetail();
                                detail.ReceiptId = rcp.grp.ReceiptId;
                                detail.PaymentRefNo = rcp.grp.PaymentRefNo;
                                detail.PaymentDate = rcp.Payment.FirstOrDefault()?.PaymentDate;
                                var paymentOBH = rcp.Payment.Where(z => z.PaymentType == "OBH");
                                detail.PaidAmount = 0;
                                detail.PaidAmountOBH = isValidObh ? paymentOBH.Sum(x => x.PaymentAmountVnd ?? 0) : 0;
                                detail.PaidAmountUsd = 0;
                                detail.PaidAmountOBHUsd = isValidObh ? paymentOBH.Sum(x => x.PaymentAmountUsd ?? 0) : 0;
                                detail.CusAdvanceAmountVnd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountVnd ?? 0;
                                detail.CusAdvanceAmountUsd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountUsd ?? 0;
                                detail.AgreementId = rcp.Payment.FirstOrDefault()?.AgreementId;

                                payment.PaidAmount += (detail.PaidAmount ?? 0);
                                payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                                payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                                payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                                payment.receiptDetail.Add(detail);
                            }
                        }

                        var invoiceObhPaid = invoiceObhGroup == null ? null : invoiceObhGroup.Where(x => x.invc.FirstOrDefault().SourceModified != null && x.invc.FirstOrDefault().SourceModified == "1" && x.invc.FirstOrDefault().Status == "Paid");
                        if (invoiceObhPaid != null && invoiceObhPaid.Count() > 0)
                        {
                            var detail = new AccountingReceiptDetail();
                            detail.ReceiptId = Guid.Empty;
                            detail.PaymentRefNo = "";
                            detail.PaymentDate = null;
                            detail.PaidAmount = 0;
                            detail.PaidAmountOBH = isValidObh ? invoiceObhPaid.Sum(z => z.invc.FirstOrDefault()?.PaidAmountVnd ?? 0) : 0;
                            detail.PaidAmountUsd = 0;
                            detail.PaidAmountOBHUsd = isValidObh ? invoiceObhPaid.Sum(z => z.invc.FirstOrDefault()?.PaidAmountUsd ?? 0) : 0;

                            payment.PaidAmount += (detail.PaidAmount ?? 0);
                            payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                            payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                            payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                            payment.receiptDetail.Add(detail);
                        }


                        if (criteria.FromUpdatedDate != null && item.payment != null)
                        {
                            var receiptRemain = receiptData.Where(x => !payment.receiptDetail.Any(p => p.ReceiptId == x.Id));
                            if (receiptRemain != null && receiptRemain.Count() > 0)
                            {
                                var pmType = invoiceObhGroup?.Count() > 0 ? "OBH" : string.Empty;
                                var pmDetail = from pm in paymentData.Where(x => x.BillingRefNo == payment.BillingRefNo && pmType.Contains(x.PaymentType))
                                               join rcp in receiptRemain on pm.ReceiptId equals rcp.Id
                                               select new
                                               {
                                                   pm.ReceiptId,
                                                   pm.PaymentType,
                                                   pm.PaymentAmountVnd,
                                                   pm.PaymentAmountUsd,
                                                   pm.UnpaidPaymentAmountVnd,
                                                   rcp.PaymentDate,
                                                   rcp.PaymentRefNo,
                                                   rcp.AgreementId,
                                                   rcp.CusAdvanceAmountUsd,
                                                   rcp.CusAdvanceAmountVnd
                                               };
                                receiptGroup = pmDetail.GroupBy(x => new { x.ReceiptId, x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentType, z.PaymentDate, z.AgreementId, z.CusAdvanceAmountVnd, z.CusAdvanceAmountUsd, z.PaymentAmountVnd, z.PaymentAmountUsd, z.UnpaidPaymentAmountVnd }) });
                                foreach (var rcp in receiptGroup)
                                {
                                    var detail = new AccountingReceiptDetail();
                                    detail.ReceiptId = rcp.grp.ReceiptId;
                                    detail.PaymentRefNo = rcp.grp.PaymentRefNo;
                                    detail.PaymentDate = rcp.Payment.FirstOrDefault()?.PaymentDate;
                                    var paymentOBH = rcp.Payment.Where(z => z.PaymentType == "OBH");
                                    detail.PaidAmount = 0;
                                    detail.PaidAmountOBH = isValidObh ? (paymentOBH?.Sum(x => x.PaymentAmountVnd ?? 0) ?? 0) : 0;
                                    detail.PaidAmountUsd = 0;
                                    detail.PaidAmountOBHUsd = isValidObh ? (paymentOBH?.Sum(x => x.PaymentAmountUsd ?? 0) ?? 0) : 0;
                                    detail.CusAdvanceAmountVnd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountVnd ?? 0;
                                    detail.CusAdvanceAmountUsd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountUsd ?? 0;
                                    detail.AgreementId = rcp.Payment.FirstOrDefault()?.AgreementId;

                                    payment.PaidAmount += (detail.PaidAmount ?? 0);
                                    payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                                    payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                                    payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                                    payment.receiptDetail.Add(detail);
                                }
                            }
                        }
                        results.Add(payment);
                    }
                    #endregion
                }

                if (numOfDebitInvoice != null && numOfDebitInvoice > 1)
                {
                    invoiceObhGroup = null;
                }

                foreach (var invoiceDe in invoiceDebitGroup)
                {
                    var payment = new AccountingCustomerPaymentExport();
                    var statusOBH = string.Empty;
                    if (invoiceObhGroup != null && invoiceObhGroup.Count() > 0)
                    {
                        // Check if obh payment have valid status on search
                        var unpaidOBH = invoiceObhGroup.Sum(x => x?.invc.FirstOrDefault().UnpaidAmountVnd ?? 0);
                        var totalPaidOBH = invoiceObhGroup.Sum(x => x?.invc.FirstOrDefault().TotalAmountVnd ?? 0);
                        if (unpaidOBH <= 0)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                        }
                        else if (unpaidOBH > 0 && unpaidOBH < totalPaidOBH)
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                        }
                        else
                        {
                            statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                        }
                        if (criteria.PaymentStatus.Count > 0 && !criteria.PaymentStatus.Contains(statusOBH))
                        {
                            isValidObh = false;
                        }
                    }
                    if (!isValidObh && invoiceDe.invc.Count() == 0)
                    {
                        continue;
                    }
                    if (criteria.FromUpdatedDate != null && invoiceObhGroup != null && invoiceObhGroup.Count() > 0)
                    {
                        var validObh = (item.payment.Any(x => x.Type == "OBH" && string.IsNullOrEmpty(x.PaymentRefNo)) && statusOBH == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID) || (item.payment.Any(x => x.Type == "OBH" && x.PaymentDate != null && x.PaymentDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDate.Value.Date <= criteria.ToUpdatedDate.Value.Date));
                        if (!validObh)
                        {
                            invoiceObhGroup = null;
                            if (invoiceDe.invc.Count() == 0)
                            {
                                continue;
                            }
                        }
                    }
                    var sur = item.surcharge.FirstOrDefault();
                    payment.PartnerId = item.grp.PartnerId;
                    payment.PartnerCode = item.grp.PartnerCode;
                    payment.PartnerName = item.grp.PartnerName;
                    payment.ParentCode = item.grp.ParentCode == null ? string.Empty : partners.Where(x => x.Id == item.grp.ParentCode).FirstOrDefault()?.AccountNo;
                    payment.InvoiceNo = invoiceDe.invc.Count() > 0 ? invoiceDe.invc.FirstOrDefault()?.InvoiceNoReal : null;
                    payment.InvoiceDate = invoiceDe.invc.Count() > 0 ? invoiceDe.invc.FirstOrDefault()?.IssuedDate : invoiceObhGroup.FirstOrDefault()?.invc.FirstOrDefault()?.IssuedDate;
                    payment.BillingRefNo = item.grp.BillingRefNo;
                    payment.BillingDate = invoiceDe.invc.FirstOrDefault()?.ConfirmBillingDate;
                    payment.DueDate = invoiceDe.invc.FirstOrDefault()?.DueDate;
                    payment.OverdueDays = invoiceDe.invc.FirstOrDefault()?.OverdueDays;
                    payment.PaymentTerm = invoiceDe.invc.FirstOrDefault()?.PaymentTerm;
                    if (invoiceDe.invc.Count() > 0)
                    {
                        payment.AccountNo = invoiceDe.invc.FirstOrDefault()?.AccountNo;
                    }
                    if (invoiceObhGroup?.Count() > 0)
                    {
                        payment.AccountNo = (string.IsNullOrEmpty(payment.AccountNo) ? "" : (payment.AccountNo + " / ")) + invoiceObhGroup.FirstOrDefault().invc.FirstOrDefault()?.AccountNo;
                    }
                    payment.BranchName = officeData[(Guid)invoiceDe.invc.FirstOrDefault()?.OfficeId].FirstOrDefault()?.ShortName;

                    // [CR]: Unpaid => TotalAmount
                    //payment.UnpaidAmountInv = invoiceDe.FirstOrDefault()?.invc.FirstOrDefault()?.UnpaidAmountVnd ?? 0;
                    //payment.UnpaidAmountOBH = invoiceObh.Sum(x => x?.invc.FirstOrDefault()?.UnpaidAmountVnd ?? 0);
                    payment.UnpaidAmountInv = invoiceDe.invc.FirstOrDefault()?.TotalAmountVnd ?? 0;
                    payment.UnpaidAmountOBH = isValidObh ? invoiceObhGroup?.Sum(x => x?.invc.FirstOrDefault()?.TotalAmountVnd ?? 0) ?? 0 : 0;
                    payment.UnpaidAmountInvUsd = invoiceDe.invc.FirstOrDefault()?.TotalAmountUsd ?? 0;
                    payment.UnpaidAmountOBHUsd = isValidObh ? invoiceObhGroup?.Sum(x => x?.invc.FirstOrDefault()?.TotalAmountUsd ?? 0) ?? 0 : 0;

                    payment.PaidAmount = payment.PaidAmountUsd = 0;
                    payment.PaidAmountOBH = payment.PaidAmountOBHUsd = 0;
                    if (item.grp.BillingRefNoType == "DEBIT")
                    {
                        payment.JobNo = sur?.JobNo;
                        payment.MBL = sur?.Mblno;
                        payment.HBL = sur?.Hblno;
                        payment.CustomNo = customsDeclarationRepository.Get(x => x.JobNo == sur.JobNo).FirstOrDefault()?.ClearanceNo;
                    }
                    // Get saleman name
                    var salemanId = catContractRepository.Get(x => x.Active == true && x.PartnerId == item.grp.PartnerId
                                                                                   && x.OfficeId.Contains(item.invoice.FirstOrDefault().OfficeId.ToString())
                                                                                   && x.SaleService.Contains(item.invoice.FirstOrDefault().ServiceType)).FirstOrDefault()?.SaleManId;
                    if (!string.IsNullOrEmpty(salemanId))
                    {
                        var employeeId = userLst[salemanId].FirstOrDefault()?.EmployeeId;
                        payment.Salesman = salemanId == null ? string.Empty : employeeLst[employeeId].FirstOrDefault().EmployeeNameEn;
                    }
                    // Get creator name
                    var creatorId = soaLst[item.grp.BillingRefNo].FirstOrDefault()?.UserCreated;
                    if (string.IsNullOrEmpty(creatorId))
                    {
                        creatorId = cdNoteLst[item.grp.BillingRefNo].FirstOrDefault()?.UserCreated;
                        var creator = string.IsNullOrEmpty(creatorId) ? string.Empty : userLst[creatorId].FirstOrDefault()?.EmployeeId;
                        payment.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst[creator].FirstOrDefault()?.EmployeeNameEn;
                    }
                    else
                    {
                        var creator = string.IsNullOrEmpty(creatorId) ? string.Empty : userLst[creatorId].FirstOrDefault()?.EmployeeId;
                        payment.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst[creator].FirstOrDefault()?.EmployeeNameEn;
                    }

                    payment.receiptDetail = new List<AccountingReceiptDetail>();
                    var receiptGroup = item.payment.Where(x => !string.IsNullOrEmpty(x.PaymentRefNo) && (numOfDebitInvoice == 1 || (numOfDebitInvoice > 1 && x.invoicePayment == invoiceDe.Key.InvoiceNoReal))).GroupBy(x => new { x.ReceiptId, x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentType, z.PaymentDate, z.AgreementId, z.CusAdvanceAmountVnd, z.CusAdvanceAmountUsd, z.PaymentAmountVnd, z.PaymentAmountUsd, z.UnpaidPaymentAmountVnd }) });
                    if (receiptGroup != null && receiptGroup.Count() > 0)
                    {
                        foreach (var rcp in receiptGroup)
                        {
                            var detail = new AccountingReceiptDetail();
                            detail.ReceiptId = rcp.grp.ReceiptId;
                            detail.PaymentRefNo = rcp.grp.PaymentRefNo;
                            detail.PaymentDate = rcp.Payment.FirstOrDefault()?.PaymentDate;
                            var paymentDebit = rcp.Payment.Where(z => z.PaymentType == "DEBIT").FirstOrDefault();
                            var paymentOBH = rcp.Payment.Where(z => z.PaymentType == "OBH");
                            detail.PaidAmount = paymentDebit?.PaymentAmountVnd ?? 0;
                            detail.PaidAmountOBH = isValidObh ? (paymentOBH != null ? paymentOBH.Sum(x => x.PaymentAmountVnd ?? 0) : 0) : 0;
                            detail.PaidAmountUsd = paymentDebit?.PaymentAmountUsd ?? 0;
                            detail.PaidAmountOBHUsd = isValidObh ? (paymentOBH != null ? paymentOBH.Sum(x => x.PaymentAmountUsd ?? 0) : 0) : 0;
                            detail.CusAdvanceAmountVnd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountVnd ?? 0;
                            detail.CusAdvanceAmountUsd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountUsd ?? 0;
                            detail.AgreementId = rcp.Payment.FirstOrDefault()?.AgreementId;

                            payment.PaidAmount += (detail.PaidAmount ?? 0);
                            payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                            payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                            payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                            payment.receiptDetail.Add(detail);
                        }
                    }

                    var invoiceDebitPaid = invoiceDe == null ? null : invoiceDe.invc.Where(x => x.SourceModified != null && x.SourceModified == "1" && x.Status == "Paid");
                    var invoiceObhPaid = invoiceObhGroup == null ? null : invoiceObhGroup.Where(x => x.invc.FirstOrDefault().SourceModified != null && x.invc.FirstOrDefault().SourceModified == "1" && x.invc.FirstOrDefault().Status == "Paid");
                    if ((invoiceDebitPaid != null && invoiceDebitPaid.Count() > 0) || (invoiceObhPaid != null && invoiceObhPaid.Count() > 0))
                    {
                        var detail = new AccountingReceiptDetail();
                        detail.ReceiptId = Guid.Empty;
                        detail.PaymentRefNo = "";
                        detail.PaymentDate = null;
                        detail.PaidAmount = invoiceDebitPaid.FirstOrDefault()?.PaidAmountVnd ?? 0;
                        detail.PaidAmountOBH = isValidObh ? (invoiceObhPaid?.Sum(z => z.invc.FirstOrDefault()?.PaidAmountVnd ?? 0) ?? 0) : 0;
                        detail.PaidAmountUsd = invoiceDebitPaid.FirstOrDefault()?.PaidAmountUsd ?? 0;
                        detail.PaidAmountOBHUsd = isValidObh ? (invoiceObhPaid?.Sum(z => z.invc.FirstOrDefault()?.PaidAmountUsd ?? 0) ?? 0) : 0;

                        payment.PaidAmount += (detail.PaidAmount ?? 0);
                        payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                        payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                        payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                        payment.receiptDetail.Add(detail);
                    }

                    invoiceDebitPaid = invoiceDe.invc;
                    if (criteria.FromUpdatedDate != null && item.payment != null)
                    {
                        var receiptRemain = receiptData.Where(x => !payment.receiptDetail.Any(p => p.ReceiptId == x.Id));
                        if (receiptRemain != null && receiptRemain.Count() > 0)
                        {
                            var pmType = invoiceDebitPaid?.Count() > 0 ? "DEBIT" : string.Empty;
                            pmType += invoiceObhGroup?.Count() > 0 ? (string.IsNullOrEmpty(pmType) ? "OBH" : ";OBH") : string.Empty;
                            var pmDetail = from pm in paymentData.Where(x => x.BillingRefNo == payment.BillingRefNo && pmType.Contains(x.PaymentType) && (numOfDebitInvoice == 1 || (numOfDebitInvoice > 1 && x.InvoiceNo == invoiceDe.Key.InvoiceNoReal)))
                                           join rcp in receiptRemain on pm.ReceiptId equals rcp.Id
                                           select new
                                           {
                                               pm.ReceiptId,
                                               pm.PaymentType,
                                               pm.PaymentAmountVnd,
                                               pm.PaymentAmountUsd,
                                               pm.UnpaidPaymentAmountVnd,
                                               rcp.PaymentDate,
                                               rcp.PaymentRefNo,
                                               rcp.AgreementId,
                                               rcp.CusAdvanceAmountUsd,
                                               rcp.CusAdvanceAmountVnd
                                           };
                            receiptGroup = pmDetail.GroupBy(x => new { x.ReceiptId, x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentType, z.PaymentDate, z.AgreementId, z.CusAdvanceAmountVnd, z.CusAdvanceAmountUsd, z.PaymentAmountVnd, z.PaymentAmountUsd, z.UnpaidPaymentAmountVnd }) });
                            foreach (var rcp in receiptGroup)
                            {
                                var detail = new AccountingReceiptDetail();
                                detail.ReceiptId = rcp.grp.ReceiptId;
                                detail.PaymentRefNo = rcp.grp.PaymentRefNo;
                                detail.PaymentDate = rcp.Payment.FirstOrDefault()?.PaymentDate;
                                var paymentDebit = rcp.Payment.Where(z => z.PaymentType == "DEBIT").FirstOrDefault();
                                var paymentOBH = rcp.Payment.Where(z => z.PaymentType == "OBH");
                                detail.PaidAmount = paymentDebit?.PaymentAmountVnd ?? 0;
                                detail.PaidAmountOBH = isValidObh ? (paymentOBH?.Sum(x => x.PaymentAmountVnd ?? 0) ?? 0) : 0;
                                detail.PaidAmountUsd = paymentDebit?.PaymentAmountUsd ?? 0;
                                detail.PaidAmountOBHUsd = isValidObh ? (paymentOBH?.Sum(x => x.PaymentAmountUsd ?? 0) ?? 0) : 0;
                                detail.CusAdvanceAmountVnd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountVnd ?? 0;
                                detail.CusAdvanceAmountUsd = rcp.Payment.FirstOrDefault()?.CusAdvanceAmountUsd ?? 0;
                                detail.AgreementId = rcp.Payment.FirstOrDefault()?.AgreementId;

                                payment.PaidAmount += (detail.PaidAmount ?? 0);
                                payment.PaidAmountOBH += isValidObh ? (detail.PaidAmountOBH ?? 0) : 0;
                                payment.PaidAmountUsd += (detail.PaidAmountUsd ?? 0);
                                payment.PaidAmountOBHUsd += isValidObh ? (detail.PaidAmountOBHUsd ?? 0) : 0;
                                payment.receiptDetail.Add(detail);
                            }
                        }
                    }
                    results.Add(payment);
                }
            }

            results = results?.OrderBy(x => x.PartnerId).ThenBy(x => x.InvoiceDate).ThenBy(x => x.InvoiceNo).ToList();

            // Caculate advance amount
            var grpPartner = results.GroupBy(x => x.PartnerId).Select(x => x);
            var paymentAdv = from dataAdv in DataContext.Get(x => x.Type == "ADV" || x.Type == "COLL_OBH")
                             join rcpt in receiptData on dataAdv.ReceiptId equals rcpt.Id
                             select new
                             {
                                 dataAdv,
                                 CusAdvanceAmountUsd = rcpt.CusAdvanceAmountUsd ?? 0,
                                 CusAdvanceAmountVnd = rcpt.CusAdvanceAmountVnd ?? 0,
                                 rcpt.PaymentDate,
                                 AgreementAdvanceAmountUsd = rcpt.AgreementAdvanceAmountUsd ?? 0,
                                 AgreementAdvanceAmountVnd = rcpt.AgreementAdvanceAmountVnd ?? 0
                             };
            if(criteria.IssuedDate != null)
            {
                foreach (var item in grpPartner)
                {
                    var payment = new AccountingCustomerPaymentExport();
                    var pm = paymentAdv.Where(x => x.dataAdv.PartnerId == item.Key && x.PaymentDate.Value.Date <= criteria.IssuedDate.Value.Date && (x.AgreementAdvanceAmountVnd > 0 || x.AgreementAdvanceAmountUsd > 0)).OrderByDescending(x => x.dataAdv.DatetimeCreated).FirstOrDefault();

                    if (pm != null)
                    {

                        var indexOfLastGrp = results.IndexOf(item.Last());
                        payment.PartnerId = item.FirstOrDefault().PartnerId;
                        payment.PartnerCode = item.FirstOrDefault().PartnerCode;
                        payment.PartnerName = item.FirstOrDefault().PartnerName;
                        payment.ParentCode = item.FirstOrDefault().ParentCode;
                        payment.BillingRefNo = "ADVANCE AMOUNT";
                        payment.BranchName = officeData[(Guid)pm.dataAdv.OfficeId].FirstOrDefault()?.ShortName;
                        payment.AdvanceAmountVnd = pm.AgreementAdvanceAmountVnd;
                        payment.AdvanceAmountUsd = pm.AgreementAdvanceAmountUsd;
                        if (payment.AdvanceAmountVnd > 0 || payment.AdvanceAmountUsd > 0)
                        {
                            results.Insert(indexOfLastGrp + 1, payment);
                        }
                    }
                }
            }
            else if (criteria.DueDate != null)
            {
                foreach (var item in grpPartner)
                {
                    var payment = new AccountingCustomerPaymentExport();
                    var pm = paymentAdv.Where(x => x.dataAdv.PartnerId == item.Key && x.PaymentDate.Value.Date <= criteria.DueDate.Value.Date && (x.AgreementAdvanceAmountVnd > 0 || x.AgreementAdvanceAmountUsd > 0)).OrderByDescending(x => x.dataAdv.PaidDate).ThenByDescending(x=>x.dataAdv.DatetimeCreated).FirstOrDefault();

                    if (pm != null)
                    {

                        var indexOfLastGrp = results.IndexOf(item.Last());
                        payment.PartnerId = item.FirstOrDefault().PartnerId;
                        payment.PartnerCode = item.FirstOrDefault().PartnerCode;
                        payment.PartnerName = item.FirstOrDefault().PartnerName;
                        payment.ParentCode = item.FirstOrDefault().ParentCode;
                        payment.BillingRefNo = "ADVANCE AMOUNT";
                        payment.BranchName = officeData[(Guid)pm.dataAdv.OfficeId].FirstOrDefault()?.ShortName;
                        payment.AdvanceAmountVnd = pm.AgreementAdvanceAmountVnd;
                        payment.AdvanceAmountUsd = pm.AgreementAdvanceAmountUsd;
                        if (payment.AdvanceAmountVnd > 0 || payment.AdvanceAmountUsd > 0)
                        {
                            results.Insert(indexOfLastGrp + 1, payment);
                        }
                    }
                }
            }
            else if (criteria.FromUpdatedDate != null)
            {
                foreach (var item in grpPartner)
                {
                    var payment = new AccountingCustomerPaymentExport();
                    var receiptList = new List<AccountingReceiptDetail>();
                    foreach (var it in item)
                    {
                        receiptList.AddRange(it.receiptDetail);
                    }
                    var pm = paymentAdv.Where(x => x.dataAdv.PartnerId == item.FirstOrDefault().PartnerId && (x.dataAdv.PaidDate != null && x.dataAdv.PaidDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.dataAdv.PaidDate.Value.Date <= criteria.ToUpdatedDate.Value.Date));
                    foreach (var dt in pm)
                    {
                        var detail = new AccountingReceiptDetail();
                        detail.ReceiptId = dt.dataAdv.ReceiptId;
                        detail.CusAdvanceAmountVnd = dt.CusAdvanceAmountVnd;
                        detail.CusAdvanceAmountUsd = dt.CusAdvanceAmountUsd;
                        receiptList.Add(detail);
                    }

                    receiptList = receiptList.GroupBy(x => x.ReceiptId).Select(x => x.FirstOrDefault()).ToList();
                    if (pm != null && pm.Count() > 0)
                    {
                        var indexOfLastGrp = results.IndexOf(item.Last());
                        payment.PartnerId = item.FirstOrDefault().PartnerId;
                        payment.PartnerCode = item.FirstOrDefault().PartnerCode;
                        payment.PartnerName = item.FirstOrDefault().PartnerName;
                        payment.ParentCode = item.FirstOrDefault().ParentCode;
                        payment.BillingRefNo = "ADVANCE AMOUNT";
                        payment.AdvanceAmountVnd = pm.Sum(x => x.dataAdv.PaymentAmountVnd ?? 0);
                        payment.AdvanceAmountVnd += receiptList.Sum(x => x.CusAdvanceAmountVnd ?? 0);

                        payment.AdvanceAmountUsd = pm.Sum(x => x.dataAdv.PaymentAmountUsd ?? 0);
                        payment.AdvanceAmountUsd += receiptList.Sum(x => x.CusAdvanceAmountUsd ?? 0);
                        payment.BranchName = pm.FirstOrDefault().dataAdv == null ? string.Empty : officeData[(Guid)pm.FirstOrDefault().dataAdv.OfficeId].FirstOrDefault()?.ShortName;
                        if (payment.AdvanceAmountVnd > 0 || payment.AdvanceAmountUsd > 0)
                        {
                            results.Insert(indexOfLastGrp + 1, payment);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in grpPartner)
                {
                    var payment = new AccountingCustomerPaymentExport();
                    var agreementIds = new List<Guid>();
                    foreach (var it in item)
                    {
                        if (it.receiptDetail != null)
                        {
                            agreementIds.AddRange(it.receiptDetail.Where(x => x.AgreementId != null).Select(x => (Guid)x.AgreementId));
                        }
                    }
                    var indexOfLastGrp = results.IndexOf(item.Last());
                    payment.PartnerId = item.FirstOrDefault().PartnerId;
                    payment.PartnerCode = item.FirstOrDefault().PartnerCode;
                    payment.PartnerName = item.FirstOrDefault().PartnerName;
                    payment.ParentCode = item.FirstOrDefault().ParentCode;
                    payment.BillingRefNo = "ADVANCE AMOUNT";
                    var contractInfo = agreementIds.Count > 0 ? catContractRepository.Get(x => agreementIds.Any(ag => ag == x.Id)) : catContractRepository.Get(x => x.PartnerId == item.Key);
                    payment.AdvanceAmountVnd = contractInfo.Sum(x => x.CustomerAdvanceAmountVnd ?? 0);
                    payment.AdvanceAmountUsd = contractInfo.Sum(x => x.CustomerAdvanceAmountUsd ?? 0);
                    if (payment.AdvanceAmountVnd > 0 || payment.AdvanceAmountUsd > 0)
                    {
                        results.Insert(indexOfLastGrp + 1, payment);
                    }
                }
            }
            return results.AsQueryable();
        }

        public IQueryable<AccountingAgencyPaymentExport> GetDataExportAccountingAgencyPayment(PaymentCriteria criteria)
        {
            //GetDebit
            var data = Query(criteria);
            if (data == null) return null;
            var partners = criteria.PartnerId != null? partnerRepository.Get(x => x.PartnerType == "Agent" && x.Id == criteria.PartnerId): partnerRepository.Get(x=>x.PartnerType =="Agent");
            var paymentData = QueryInvoiceDataPaymentAgent(criteria);
            var surchargeData = surchargeRepository.Get().Select(x=>new { x.JobNo,x.Hblno,x.Mblno,x.AcctManagementId,x.DebitNo,x.Soano,x.Type,x.AmountUsd,x.VatAmountUsd,x.AmountVnd,x.VatAmountVnd});
            var receiptData = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE);
            var transData =  csTransactionRepository.Get().Select(x=>new { x.JobNo,x.Eta,x.Etd});
            var resultsQuery = (from invoice in data
                                join surcharge in surchargeData on invoice.RefId equals surcharge.AcctManagementId.ToString() 
                                join part in partners on invoice.PartnerId equals part.Id
                                join payment in paymentData on invoice.RefId.ToLower() equals payment.RefId.ToLower() into grpPayment
                                from payment in grpPayment.DefaultIfEmpty()
                                join rcpt in receiptData on payment.ReceiptId equals rcpt.Id into grpReceipts
                                from rcpt in grpReceipts.DefaultIfEmpty()
                                join trans in transData on surcharge.JobNo equals trans.JobNo into grpJob
                                from trans in grpJob.DefaultIfEmpty()
                                select new
                                {
                                    invoice,
                                    payment,
                                    surcharge.JobNo,
                                    surcharge.Hblno,
                                    surcharge.Mblno,
                                    surcharge,
                                    trans,
                                    Type = surcharge.Type == "OBH" ? "OBH" : "DEBIT",
                                    PartnerCode = part.AccountNo,
                                    ParentCode = part != null ? part.ParentId : string.Empty,
                                    PartnerName = part.ShortName,
                                    BillingRefNo = string.IsNullOrEmpty(surcharge.Soano) ? surcharge.DebitNo : surcharge.Soano,
                                    PaymentRefNo = rcpt == null ? null : rcpt.PaymentRefNo,
                                    PaymentDate = rcpt == null ? null : rcpt.PaymentDate
                                });
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                if (criteria.SearchType == "ReceiptNo")
                {
                    var listReceiptInfo = acctReceiptRepository.Get(receipt => receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE && criteria.ReferenceNos.Contains(receipt.PaymentRefNo)).Select(x => x.PaymentRefNo).ToList();
                    resultsQuery = resultsQuery.Where(x => listReceiptInfo.Any(z => z == (x.payment == null ? null : x.PaymentRefNo)));
                }
            }
            var resultGroups = resultsQuery.ToList().GroupBy(x => new
            {
                x.invoice.PartnerId,
                x.PartnerCode,
                x.PartnerName,
                x.ParentCode,
                x.BillingRefNo,
                x.JobNo,
                x.Mblno,
                x.Hblno,
                x.Type,
            }).Select(x => new { grp = x.Key
           , invoice = x.Select(z => z.invoice)
            //, surcharge = x.Select(z => new { z.JobNo, z.Mblno, z.Hblno, z.AmountUsd, z.VatAmountUsd,z.AmountVnd,z.VatAmountVnd })
            ,sur = x.Select(z=>z.surcharge)
            //, surcharge = x.Select(z => new { z.JobNo, z.Mblno, z.Hblno, z.totalUsd, z.totalVnd, z.VoucherId})
            , payment = x.Select(z => new { z.payment?.Id, z.payment?.ReceiptId, z.payment?.PaymentType, z.PaymentRefNo, z.PaymentDate, z.payment?.PaymentAmountUsd, z.payment?.UnpaidPaymentAmountUsd, z.payment?.PaymentAmountVnd, z.payment?.UnpaidPaymentAmountVnd })
            ,trans = x.Select(z=>new { z.trans})
            });

            var results = new List<AccountingAgencyPaymentExport>();
            var soaLst = soaRepository.Get().Select(x => new { x.Soano, x.UserCreated }).ToLookup(x => x.Soano);
            var cdNoteLst = cdNoteRepository.Get().ToLookup(x => x.Code);
            var userLst = userRepository.Get().Select(x => new { x.Id, x.EmployeeId }).ToLookup(x => x.Id);
            var employeeLst = sysEmployeeRepository.Get().Select(x => new { x.Id, x.EmployeeNameEn }).ToLookup(x => x.Id);
            foreach (var item in resultGroups)
            {
                var agent = new AccountingAgencyPaymentExport();
                var invoice = item.invoice.GroupBy(x => new { x.RefId, x.TotalAmountUsd, x.TotalAmountVnd }).Select(x => new { grp = x.Key, invc = x.Select(z => new { z.Type, z.UnpaidAmountVnd, z.TotalAmountUsd, z.InvoiceNoReal, z.IssuedDate, z.ConfirmBillingDate, z.OverdueDays, z.PaymentTerm, z.TotalAmountVnd, z.DueDate }) });
                var invoiceDe = invoice.Where(x => x.invc.FirstOrDefault().Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE);
                var invoiceObh = invoice.Where(x => x.invc.FirstOrDefault().Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);
                var statusOBH = string.Empty;
                if (item.grp.Type == "OBH")
                {
                    // Check if obh payment have valid status on search
                    var unpaidOBH = item.invoice.Sum(x => x.UnpaidAmount ?? 0);
                    var totalPaidOBH = item.invoice.Sum(x => x.TotalAmount ?? 0);

                    if (unpaidOBH <= 0)
                    {
                        statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                    }
                    else if (unpaidOBH > 0 && unpaidOBH < totalPaidOBH)
                    {
                        statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                    }
                    else
                    {
                        statusOBH = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                    }
                }

                agent.Status = item.grp.Type != "OBH" ? item.invoice.FirstOrDefault()?.Status : statusOBH;
                agent.AgentPartnerCode = item.grp.PartnerCode;
                agent.AgentPartnerName = item.grp.PartnerName;
                agent.AgentParentCode = item.grp.ParentCode == null ? string.Empty : partners.Where(x => x.Id == item.grp.ParentCode).FirstOrDefault()?.AccountNo;
                agent.InvoiceNo = invoiceDe.Count() > 0 ? invoiceDe.FirstOrDefault().invc.FirstOrDefault()?.InvoiceNoReal : null;
                agent.InvoiceDate = invoiceDe.Count() > 0 ? invoice.FirstOrDefault().invc.FirstOrDefault()?.IssuedDate : null;
                agent.DebitNo = item.grp.BillingRefNo;

                agent.JobNo = item.grp.JobNo;
                agent.HBL = item.grp.Hblno;
                agent.MBL = item.grp.Mblno;

                if (invoice.FirstOrDefault()?.invc.FirstOrDefault()?.DueDate != null)
                {
                    var dueDate = invoice.FirstOrDefault().invc.FirstOrDefault()?.DueDate.Value.AddDays((double)invoice.FirstOrDefault().invc.FirstOrDefault()?.PaymentTerm);
                    agent.DueDate = dueDate;
                    agent.CreditTerm = invoice.FirstOrDefault().invc.FirstOrDefault()?.PaymentTerm;
                    agent.OverDueDays = invoice.FirstOrDefault().invc.FirstOrDefault()?.OverdueDays;
                }
                else if (agent.VoucherNo != null && invoice.FirstOrDefault()?.invc.FirstOrDefault()?.DueDate != null)
                {
                    var dueDate = invoice.FirstOrDefault().invc.FirstOrDefault()?.DueDate.Value.AddDays((double)invoice.FirstOrDefault().invc.FirstOrDefault()?.PaymentTerm);
                    agent.DueDate = dueDate;
                    agent.CreditTerm = invoice.FirstOrDefault().invc.FirstOrDefault()?.PaymentTerm;
                    agent.OverDueDays = invoice.FirstOrDefault().invc.FirstOrDefault()?.OverdueDays;
                }

                //agent.VoucherNo = item.surcharge.Where(x => x.VoucherId != null).FirstOrDefault()?.VoucherId;

                var trans = item.trans.FirstOrDefault();
                //var trans = csTransactionRepository.Get(x => x.JobNo == agent.JobNo).FirstOrDefault();
                agent.EtaDate = trans.trans?.Eta;
                agent.EtdDate = trans.trans?.Etd;

                //var it = surchargeData.Where(x => x.Hblno == item.grp.Hblno && (!string.IsNullOrEmpty(x.Soano)? x.Soano == item.grp.BillingRefNo: x.DebitNo == item.grp.BillingRefNo) && x.Type == (item.grp.Type != "OBH"?"SELL":"OBH"));
                var it = item.sur.Where(x => x.Hblno == item.grp.Hblno && (!string.IsNullOrEmpty(x.Soano) ? x.Soano == item.grp.BillingRefNo : x.DebitNo == item.grp.BillingRefNo) && x.Type == (item.grp.Type != "OBH" ? "SELL" : "OBH"));
                var surgrp = it.GroupBy(x => new { x.AmountUsd, x.VatAmountUsd, x.AmountVnd, x.VatAmountVnd }).Select(x => new { grp = x.Key });

                agent.UnpaidAmountInvUsd = surgrp.Sum(x => x.grp.AmountUsd);
                agent.UnpaidAmountOBHUsd = surgrp.Sum(x => x.grp.VatAmountUsd);

                agent.UnpaidAmountInv = surgrp.Sum(x => x.grp.AmountVnd);
                agent.UnpaidAmountOBH = surgrp.Sum(x => x.grp.VatAmountVnd);

                agent.DebitAmountUsd = agent.UnpaidAmountInvUsd + agent.UnpaidAmountOBHUsd;
                agent.DebitAmountVnd = agent.UnpaidAmountInv + agent.UnpaidAmountOBH;

                agent.DebitUsd = agent.DebitVnd = 0;
                agent.CreditUsd = agent.CreditVnd = 0;

                // Get saleman name
                var salemanId = catContractRepository.Get(x => x.Active == true && x.PartnerId == item.grp.PartnerId
                                                                               && x.OfficeId.Contains(item.invoice.FirstOrDefault().OfficeId.ToString())
                                                                               && x.SaleService.Contains(item.invoice.FirstOrDefault().ServiceType)).FirstOrDefault()?.SaleManId;
                if (!string.IsNullOrEmpty(salemanId))
                {
                    var employeeId = userLst[salemanId].FirstOrDefault()?.EmployeeId;
                    agent.Salesman = salemanId == null ? string.Empty : employeeLst[employeeId].FirstOrDefault().EmployeeNameEn;
                }
                // Get creator name
                var creatorId = soaLst[item.grp.BillingRefNo].FirstOrDefault()?.UserCreated;
                if (string.IsNullOrEmpty(creatorId))
                {
                    creatorId = cdNoteLst[item.grp.BillingRefNo].FirstOrDefault()?.UserCreated;
                    var creator = string.IsNullOrEmpty(creatorId) ? string.Empty : userLst[creatorId].FirstOrDefault()?.EmployeeId;
                    agent.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst[creator].FirstOrDefault()?.EmployeeNameEn;
                }
                else
                {
                    var creator = string.IsNullOrEmpty(creatorId) ? string.Empty : userLst[creatorId].FirstOrDefault()?.EmployeeId;
                    agent.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst[creator].FirstOrDefault()?.EmployeeNameEn;
                }

                agent.details = new List<AccountingAgencyPaymentExportDetail>();

                var receiptGroup = item.payment.Where(x => !string.IsNullOrEmpty(x.PaymentRefNo)).GroupBy(x => new { x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentType, z.PaymentDate, z.PaymentAmountUsd, z.UnpaidPaymentAmountUsd, z.PaymentAmountVnd, z.UnpaidPaymentAmountVnd }) });
                foreach (var rcp in receiptGroup)
                {
                    var detail = new AccountingAgencyPaymentExportDetail();
                    detail.RefNo = rcp.grp.PaymentRefNo;
                    detail.PaidDate = rcp.Payment.FirstOrDefault()?.PaymentDate;
                    var paymentDebit = rcp.Payment.Where(z => z.PaymentType == "DEBIT").FirstOrDefault();
                    var paymentOBH = rcp.Payment.Where(z => z.PaymentType == "OBH");

                    if (agent.DebitAmountUsd > 0)
                    {
                        detail.DebitUsd = paymentDebit?.PaymentAmountUsd ?? 0 + paymentOBH.Sum(x => x.PaymentAmountUsd ?? 0);
                        agent.DebitUsd += (detail.DebitUsd ?? 0);

                        detail.DebitVnd = paymentDebit?.PaymentAmountVnd ?? 0 + paymentOBH.Sum(x => x.PaymentAmountVnd ?? 0);
                        agent.DebitVnd += (detail.DebitVnd ?? 0);
                    }
                    agent.details.Add(detail);
                }
                results.Add(agent);
            }

            //GetCredit
            var dataCredit = GetCreditDataPaymentAgent(criteria);

            var filterPaidDate = new List<AccountingAgencyPaymentExport>();
            var filterUnpaid = new List<AccountingAgencyPaymentExport>();

            if (criteria.FromUpdatedDate != null )
            {
                filterPaidDate = results.Where(x => x.details.FirstOrDefault()?.PaidDate != null && x.details.FirstOrDefault()?.PaidDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.details.FirstOrDefault()?.PaidDate.Value.Date <= criteria.ToUpdatedDate.Value.Date).ToList();

                filterUnpaid = results.Where(x => x.Status == "Unpaid" && x.details.Count() == 0).ToList();

                results = filterPaidDate.Concat(filterUnpaid).ToList();
            }
            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
            {
                results = results.Where(x => criteria.PaymentStatus.Contains(x.Status ?? "Unpaid")).ToList();
            }
            if (criteria.DueDate != null )
            {
                results = results.Where(x => x.DueDate != null && x.DueDate.Value.Date == criteria.DueDate.Value.Date).ToList();
            }
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                    results = results.Where(x => x.OverDueDays>=1 && x.OverDueDays<=15).ToList();
                    break;
                case Common.OverDueDate.Between16_30:
                    results = results.Where(x => x.OverDueDays > 15 && x.OverDueDays <= 30).ToList();
                    break;
                case Common.OverDueDate.Between31_60:
                    results = results.Where(x => x.OverDueDays > 31 && x.OverDueDays <= 60).ToList();
                    break;
                case Common.OverDueDate.Between61_90:
                    results = results.Where(x => x.OverDueDays > 61 && x.OverDueDays <= 90).ToList();
                    break;
            }
            
            return results.Concat(dataCredit).AsQueryable().OrderByDescending(x => x.JobNo).ThenBy(x=>x.AgentPartnerName);
        }


        private IQueryable<AccountingAgencyPaymentExport> GetCreditDataPaymentAgent(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingPayment, bool>> perQuery = GetQueryADVPermission(rangeSearch, _user);
            var results = new List<AccountingAgencyPaymentExport>();
            Expression<Func<AccAccountingPayment, bool>> query = x => (x.PaymentType == "CREDIT" && (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId)));
            if (criteria.IssuedDate != null)
            {
                query = query.And(x => false);
            }
            if (criteria.DueDate != null)
            {
                query = query.And(x => false);
            }
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                case Common.OverDueDate.Between16_30:
                case Common.OverDueDate.Between31_60:
                case Common.OverDueDate.Between61_90:
                    query = query.And(x => false);
                    break;
            }
            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var paymentData = DataContext.Get(query);
            var partners = partnerRepository.Get().Select(x => new { x.Id, x.ShortName,x.AccountNo, x.ParentId });
            var querySoa = SoaCreditExpressionQuery(criteria);
            var soaData = soaRepository.Get(querySoa).Select(x => new { x.Id, x.Soano, x.Customer, x.NetOff, x.Currency, x.CreditAmount ,x.OfficeId});
            var queryCdNote = CreditNoteExpressionQuery(criteria);
            var cdNoteData = cdNoteRepository.Get(queryCdNote).Select(x => new { x.Id, x.Code, x.PartnerId, x.NetOff, x.Total, x.CurrencyId,x.OfficeId });
            var surchargeData = surchargeRepository.Get(x => !string.IsNullOrEmpty(x.VoucherId) || !string.IsNullOrEmpty(x.VoucherIdre)).Select(x => new { x.Type, x.CreditNo, x.PaySoano, x.InvoiceNo, x.VoucherId, x.VoucherIdre, x.AmountVnd, x.VatAmountVnd, x.AmountUsd, x.VatAmountUsd,x.JobNo,x.Mblno,x.Hblno,x.VoucherIdredate,x.VoucherIddate });
            var contractData = catContractRepository.Get();
            var receiptData = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE);

            var creditSoaData = (from soa in soaData
                                 join surcharge in surchargeData on soa.Soano equals surcharge.PaySoano
                                 join payments in paymentData on soa.Id equals payments.RefId into grpPayment
                                 from payment in grpPayment.DefaultIfEmpty()
                                 join partner in partners on soa.Customer equals partner.Id into grpPartners
                                 from part in grpPartners.DefaultIfEmpty()
                                 join con in contractData on part.ParentId equals con.PartnerId into grpContracts
                                 from con in grpContracts.DefaultIfEmpty()
                                 select new
                                 {
                                     PartnerId = part.Id,
                                     part.AccountNo,
                                     part.ParentId,
                                     part.ShortName,

                                     surcharge.JobNo,
                                     surcharge.Hblno,
                                     surcharge.Mblno,
                                     BillingRefNo = soa.Soano,

                                     SurAmountUsd = surcharge.AmountUsd,
                                     SurVatAmountUsd = surcharge.VatAmountUsd,
                                     SurAmountVnd = surcharge.AmountVnd,
                                     SurVatAmountVnd = surcharge.VatAmountVnd,
                                     soa.NetOff,
                                     soa.OfficeId,
                                     VoucherId = surcharge.VoucherId == null ? surcharge.VoucherIdre : surcharge.VoucherId,
                                     DueDate = surcharge.VoucherId == null ? surcharge.VoucherIdredate : surcharge.VoucherIddate,
                                     CreditTerm = con.PaymentTerm,});

            var creditNoteData = (from cdNote in cdNoteData
                                  join surcharge in surchargeData on cdNote.Code equals surcharge.CreditNo
                                  join payments in paymentData on cdNote.Id.ToString() equals payments.RefId into grpPayment
                                  from payment in grpPayment.DefaultIfEmpty()
                                  join partner in partners on cdNote.PartnerId equals partner.Id into grpPartners
                                  from part in grpPartners.DefaultIfEmpty()
                                  join con in contractData on part.ParentId equals con.PartnerId into grpContracts
                                  from con in grpContracts.DefaultIfEmpty()
                                  select new
                                  {
                                      PartnerId = part.Id,
                                      part.AccountNo,
                                      part.ParentId,
                                      part.ShortName,

                                      surcharge.JobNo,
                                      surcharge.Hblno,
                                      surcharge.Mblno,
                                      BillingRefNo = cdNote.Code,

                                      SurAmountUsd = surcharge.AmountUsd,
                                      SurVatAmountUsd = surcharge.VatAmountUsd,
                                      SurAmountVnd = surcharge.AmountVnd,
                                      SurVatAmountVnd = surcharge.VatAmountVnd,
                                      cdNote.NetOff,
                                      cdNote.OfficeId,
                                      VoucherId = surcharge.VoucherId == null ? surcharge.VoucherIdre : surcharge.VoucherId,
                                      DueDate = surcharge.VoucherId == null ? surcharge.VoucherIdredate : surcharge.VoucherIddate,
                                      CreditTerm = con.PaymentTerm,
                                  });
            var data = (creditSoaData.Concat(creditNoteData)).ToList();

            var resultGroups = data.GroupBy(x => new
            {
                x.BillingRefNo,
                x.PartnerId,
                x.AccountNo,
                x.ShortName,
                x.ParentId,
                x.JobNo,
                x.Mblno,
                x.Hblno,
            }).Select(x => new
            {
                grp = x.Key
            ,
                payment = x.Select(z => new { z.VoucherId ,z.NetOff})
            ,
                credit = x.Select(z => new { z.CreditTerm, z.DueDate })
            ,
                surcharge = x.Select(z => new { z.SurAmountUsd, z.SurVatAmountUsd, z.SurAmountVnd, z.SurVatAmountVnd, z.Hblno })
            });
            foreach (var item in resultGroups)
            {
                var agent = new AccountingAgencyPaymentExport();

                agent.AgentPartnerCode = item.grp.AccountNo;
                agent.AgentPartnerName = item.grp.ShortName;
                agent.AgentParentCode = item.grp.ParentId == null ? string.Empty : partners.Where(x => x.Id == item.grp.ParentId).FirstOrDefault()?.AccountNo;
                agent.CreditNo = item.grp.BillingRefNo;
                agent.Status = item.payment.FirstOrDefault().NetOff == true ? "Paid" : "Unpaid";

                agent.JobNo = item.grp.JobNo;
                agent.HBL = item.grp.Hblno;
                agent.MBL = item.grp.Mblno;
                agent.VoucherNo = item.payment.Where(x => x.VoucherId != null).FirstOrDefault()?.VoucherId;

                var trans = csTransactionRepository.Get(x => x.JobNo == agent.JobNo).FirstOrDefault();
                agent.EtaDate = trans?.Eta;
                agent.EtdDate = trans?.Etd;

                agent.DueDate = item.credit.FirstOrDefault()?.DueDate;
                if (agent.DueDate != null && item.credit.FirstOrDefault()?.CreditTerm != null )
                {
                    var dueDate = agent.DueDate.Value.AddDays((double)(item.credit.FirstOrDefault()?.CreditTerm ?? 0));
                    agent.DueDate = dueDate;
                    agent.OverDueDays = (DateTime.Today > agent.DueDate.Value.Date) ? (DateTime.Today - agent.DueDate.Value.Date).Days : 0;
                    agent.CreditTerm = item.credit.FirstOrDefault()?.CreditTerm;
                }else if (agent.VoucherNo != null && item.credit.FirstOrDefault()?.CreditTerm != null)
                {
                    var dueDate = agent.DueDate.Value.AddDays((double)(item.credit.FirstOrDefault()?.CreditTerm ?? 0));
                    agent.DueDate = dueDate;
                    agent.OverDueDays = (DateTime.Today > agent.DueDate.Value.Date) ? (DateTime.Today - agent.DueDate.Value.Date).Days : 0;
                    agent.CreditTerm = item.credit.FirstOrDefault()?.CreditTerm;
                }else if (agent.VoucherNo != null)
                {
                    var am = accountingManaRepository.Get(x => x.VoucherId == agent.VoucherNo).FirstOrDefault();
                    if (am != null)
                    {
                        var dueDate = agent.DueDate.Value.AddDays((double)(am.PaymentTerm ?? 0));
                        agent.DueDate = dueDate;
                        agent.OverDueDays = (DateTime.Today > agent.DueDate.Value.Date) ? (DateTime.Today - agent.DueDate.Value.Date).Days : 0;
                        agent.CreditTerm = am.PaymentTerm ?? 0;
                    }
                }

                //agent.UnpaidAmountInvUsd = item.surcharge.Where(x => x.Hblno == item.grp.Hblno).Sum(x => x.SurAmountUsd);
                //agent.UnpaidAmountOBHUsd = item.surcharge.Where(x => x.Hblno == item.grp.Hblno).Sum(x => x.SurVatAmountUsd);

                //agent.UnpaidAmountInv = item.surcharge.Where(x => x.Hblno == item.grp.Hblno).Sum(x => x.SurAmountVnd);
                //agent.UnpaidAmountOBH = item.surcharge.Where(x => x.Hblno == item.grp.Hblno).Sum(x => x.SurVatAmountVnd);
                var it = item.surcharge.Where(x => x.Hblno == item.grp.Hblno).GroupBy(x => new { x.SurAmountUsd,x.SurVatAmountUsd,x.SurAmountVnd,x.SurVatAmountVnd });

                agent.UnpaidAmountInvUsd = it.Sum(x => x.Key.SurAmountUsd);
                agent.UnpaidAmountOBHUsd = it.Sum(x => x.Key.SurVatAmountUsd);

                agent.UnpaidAmountInv = it.Sum(x=>x.Key.SurAmountVnd);
                agent.UnpaidAmountOBH = it.Sum(x=>x.Key.SurVatAmountVnd);

                agent.CreditAmountUsd = agent.UnpaidAmountInvUsd + agent.UnpaidAmountOBHUsd;
                agent.CreditAmountVnd = agent.UnpaidAmountInv + agent.UnpaidAmountOBH;

                agent.CreditUsd = 0;
                agent.CreditVnd = 0;
                agent.details = new List<AccountingAgencyPaymentExportDetail>();

                var pay = DataContext.Get(x => x.BillingRefNo == agent.CreditNo).OrderBy(x => x.PaidDate).ThenBy(x => x.PaymentNo);
                var dataPM = pay.Join(receiptData, pm => pm.ReceiptId, re => re.Id, (pm, rc) => new { rc.PaidAmountUsd, rc.PaidAmountVnd, rc.PaymentRefNo, rc.PaymentDate, rc.CreditAmountUsd, rc.CreditAmountVnd, pm.PaymentAmountUsd, pm.PaymentAmountVnd });
                var receiptGroup = dataPM.GroupBy(x => new { x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentDate, z.PaidAmountUsd, z.PaidAmountVnd, z.CreditAmountVnd, z.CreditAmountUsd, z.PaymentAmountUsd, z.PaymentAmountVnd }) });

                foreach (var rcp in receiptGroup)
                {
                    var detail = new AccountingAgencyPaymentExportDetail();
                    detail.RefNo = rcp.grp.PaymentRefNo;
                    detail.PaidDate = rcp.Payment.FirstOrDefault()?.PaymentDate;

                    detail.CreditUsd = rcp.Payment.FirstOrDefault().PaymentAmountUsd;
                    agent.CreditUsd += (detail.CreditUsd ?? 0);

                    detail.CreditVnd = rcp.Payment.FirstOrDefault().PaymentAmountVnd;
                    agent.CreditVnd += (detail.CreditVnd ?? 0);

                    agent.details.Add(detail);
                }
                results.Add(agent);
            }
            //if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            //{
            //    if (criteria.SearchType == "ReceiptNo")
            //    {
            //        var listReceiptInfo = acctReceiptRepository.Get(receipt => receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE && criteria.ReferenceNos.Contains(receipt.PaymentRefNo)).Select(x => x.Id).ToList();
            //        results = results.Where(x => listReceiptInfo.Any(z => z == x.ReceiptId)).ToList();
            //    }
            //}

            var filterPaidDate = new List<AccountingAgencyPaymentExport>();
            var filterUnpaid = new List<AccountingAgencyPaymentExport>();

            if (criteria.FromUpdatedDate != null)
            {
                filterPaidDate = results.Where(x => x.details.FirstOrDefault()?.PaidDate != null && x.details.FirstOrDefault()?.PaidDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.details.FirstOrDefault()?.PaidDate.Value.Date <= criteria.ToUpdatedDate.Value.Date).ToList();

                filterUnpaid = results.Where(x => x.Status == "Unpaid" && x.details.Count() == 0).ToList();

                results = filterPaidDate.Concat(filterUnpaid).ToList();
            }

            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
            {
                results = results.Where(x => criteria.PaymentStatus.Contains(x.Status ?? "Unpaid")).ToList();
            }
            if (criteria.DueDate != null)
            {
                results = results.Where(x => x.DueDate != null && x.DueDate.Value.Date == criteria.DueDate.Value.Date).ToList();
            }
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                    results = results.Where(x => x.OverDueDays >= 0 && x.OverDueDays <= 15).ToList();
                    break;
                case Common.OverDueDate.Between16_30:
                    results = results.Where(x => x.OverDueDays > 15 && x.OverDueDays <= 30).ToList();
                    break;
                case Common.OverDueDate.Between31_60:
                    results = results.Where(x => x.OverDueDays > 31 && x.OverDueDays <= 60).ToList();
                    break;
                case Common.OverDueDate.Between61_90:
                    results = results.Where(x => x.OverDueDays > 61 && x.OverDueDays <= 90).ToList();
                    break;
            }

            return results.AsQueryable();
        }

        private IQueryable<AccAccountingPayment> QueryInvoiceDataPaymentAgent(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingPayment, bool>> perQuery = GetQueryADVPermission(rangeSearch, _user);
            Expression<Func<AccAccountingPayment, bool>> query = x => (x.Type == "DEBIT" || x.Type == "OBH");
            if (criteria.ReferenceNos?.Count(x => !string.IsNullOrEmpty(x)) > 0)
            {
                switch (criteria.SearchType)
                {
                    case "VatInvoice":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "DebitInvoice":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.BillingRefNo, StringComparer.OrdinalIgnoreCase) || criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Soa":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.BillingRefNo, StringComparer.OrdinalIgnoreCase));
                        break;
                }
            }

            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            if (criteria.Office?.Count > 0)
            {
                query = query.And(x => criteria.Office.Contains(x.OfficeId.ToString(), StringComparer.OrdinalIgnoreCase));
            }

            var data = DataContext.Get(query);
            return data;
        }

    }
}
