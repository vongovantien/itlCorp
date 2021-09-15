﻿using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.Receipt;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Services
{
    public class AcctReceiptService : RepositoryBase<AcctReceipt, AcctReceiptModel>, IAcctReceiptService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<AccAccountingManagement> acctMngtRepository;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<CatContract> catContractRepository;
        private readonly IContextBase<AccAccountingPayment> acctPaymentRepository;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        private readonly IContextBase<CatDepartment> departmentRepository;
        private readonly IContextBase<SysOffice> officeRepository;
        private readonly IContextBase<OpsTransaction> opsTransactionRepository;
        private readonly IContextBase<CsTransaction> csTransactionRepository;
        private readonly IContextBase<CsTransactionDetail> csTransactionDetailRepository;
        private readonly IContextBase<AcctSoa> soaRepository;
        private readonly IContextBase<AcctCdnote> cdNoteRepository;
        private readonly IContextBase<SysCompany> companyRepository;
        private readonly IAccAccountReceivableService accAccountReceivableService;
        private readonly IContextBase<AcctReceiptSync> receiptSyncRepository;
        private readonly IContextBase<AcctCreditManagementAr> creditMngtArRepository;
        private readonly IContextBase<SysEmailTemplate> emailTemplaterepository;
        private readonly IContextBase<SysEmailSetting> emailSettingRepository;
        private readonly IOptions<WebUrl> webUrl;

        public AcctReceiptService(
            IContextBase<AcctReceipt> repository,
            IMapper mapper,
            ICurrentUser curUser,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<AccAccountingManagement> acctMngtRepo,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<CatContract> catContractRepo,
            IContextBase<AccAccountingPayment> acctPaymentRepo,
            IContextBase<SysUser> sysUserRepo,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<CatDepartment> departmentRepo,
            IContextBase<SysOffice> officeRepo,
            IContextBase<OpsTransaction> opsTransactionRepo,
            IContextBase<CsTransaction> csTransactionRepo,
            IContextBase<CsTransactionDetail> csTransactionDetailRepo,
            IContextBase<AcctSoa> soaRepo,
            IContextBase<AcctCdnote> cdNoteRepo,
            IContextBase<SysCompany> companyRepo,
            IAccAccountReceivableService accAccountReceivable,
            IContextBase<AcctReceiptSync> receiptSyncRepo,
            IContextBase<AcctCreditManagementAr> creditMngtArRepo,
            IContextBase<SysEmailTemplate> emailTemplate,
            IContextBase<SysEmailSetting> emailSetting,
            IOptions<WebUrl> wUrl
            ) : base(repository, mapper)
        {
            currentUser = curUser;
            acctMngtRepository = acctMngtRepo;
            catPartnerRepository = catPartnerRepo;
            catContractRepository = catContractRepo;
            acctPaymentRepository = acctPaymentRepo;
            sysUserRepository = sysUserRepo;
            surchargeRepository = surchargeRepo;
            departmentRepository = departmentRepo;
            officeRepository = officeRepo;
            opsTransactionRepository = opsTransactionRepo;
            csTransactionRepository = csTransactionRepo;
            csTransactionDetailRepository = csTransactionDetailRepo;
            soaRepository = soaRepo;
            cdNoteRepository = cdNoteRepo;
            companyRepository = companyRepo;
            accAccountReceivableService = accAccountReceivable;
            receiptSyncRepository = receiptSyncRepo;
            creditMngtArRepository = creditMngtArRepo;
            emailTemplaterepository = emailTemplate;
            emailSettingRepository = emailSetting;
            webUrl = wUrl;
        }

        private IQueryable<AcctReceipt> GetQueryBy(AcctReceiptCriteria criteria)
        {
            //[ADD][16236][27/08/2021][Collect Amount: Nếu receipt Currency Là VND: Lấy Giá trị Cột VND, Nếu receipt Currency là USD là Cột Collect USD]
            //Expression<Func<AcctReceipt, bool>> query = (x =>
            //(x.CurrencyId ?? "").IndexOf(criteria.Currency ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //&& (x.CustomerId ?? "").IndexOf(criteria.CustomerID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //);
            Expression<Func<AcctReceipt, bool>> query = (x =>
              (x.CustomerId ?? "").IndexOf(criteria.CustomerID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
             );
            //[END]

            // Tìm theo status
            if (!string.IsNullOrEmpty(criteria.Status))
            {
                query = query.And(x => criteria.Status == x.Status);
            }

            if (!string.IsNullOrEmpty(criteria.SyncStatus))
            {
                query = query.And(x => criteria.SyncStatus == x.SyncStatus);
            }

            // Tìm theo ngày sync/ngày thu
            if (!string.IsNullOrEmpty(criteria.DateType) && criteria.DateType == "Sync")
            {
                query = query.And(x => x.LastSyncDate.Value.Date >= criteria.DateFrom.Value.Date && x.FromDate.Value.Date <= criteria.DateTo.Value.Date);
            }
            if (!string.IsNullOrEmpty(criteria.DateType) && criteria.DateType == "Paid")
            {
                query = query.And(x => x.PaymentDate.Value.Date >= criteria.DateFrom.Value.Date && x.ToDate.Value.Date <= criteria.DateTo.Value.Date);
            }

            // Tìm theo số phiếu thu/số invoice
            if (string.IsNullOrEmpty(criteria.PaymentType))
            {
                query = query.And(x => (x.PaymentRefNo ?? "").IndexOf(criteria.RefNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (!string.IsNullOrEmpty(criteria.TypeReceipt))
            {
                query = query.And(x => x.Type == criteria.TypeReceipt);

            }
            if (!string.IsNullOrEmpty(criteria.PaymentType) && criteria.PaymentType == "Invoice")
            {
                // Lấy ra thông tin hóa đơn
                IQueryable<AccAccountingManagement> invoices = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE
                && x.InvoiceNoReal.IndexOf(criteria.RefNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
                if (invoices != null && invoices.Count() > 0)
                {
                    List<Guid> receiptIds = new List<Guid>();
                    foreach (AccAccountingManagement invoice in invoices)
                    {
                        // check hđ có payment
                        AccAccountingPayment paymentInfo = acctPaymentRepository.Get(x => x.RefId == invoice.Id.ToString())?.FirstOrDefault();
                        if (paymentInfo != null && !string.IsNullOrEmpty(paymentInfo.ReceiptId.ToString()))
                        {
                            // lấy thông tin phiếu thu
                            AcctReceipt receipt = DataContext.Get(x => x.Id == paymentInfo.ReceiptId)?.FirstOrDefault();
                            if (receipt != null)
                            {
                                receiptIds.Add(receipt.Id);
                            }
                        }
                    }
                    if (receiptIds.Count > 0)
                    {
                        query = query.And(x => receiptIds.Contains(x.Id));
                    }
                }
            }

            IQueryable<AcctReceipt> dataQuery = DataContext.Get(query);
            dataQuery = dataQuery?.OrderByDescending(x => x.DatetimeModified);

            return dataQuery;
        }

        private IQueryable<AcctReceiptModel> FormatReceipt(IQueryable<AcctReceipt> dataQuery, AcctReceiptCriteria criteria)
        {
            List<AcctReceiptModel> list = new List<AcctReceiptModel>();

            if (dataQuery != null && dataQuery.Count() > 0)
            {
                foreach (var item in dataQuery)
                {
                    AcctReceiptModel d = mapper.Map<AcctReceiptModel>(item);

                    d.UserNameCreated = item.UserCreated == null ? null : sysUserRepository.Get(u => u.Id == item.UserCreated).FirstOrDefault().Username;
                    d.UserNameModified = item.UserModified == null ? null : sysUserRepository.Get(u => u.Id == item.UserModified).FirstOrDefault().Username;
                    d.CustomerName = item.CustomerId == null ? null : catPartnerRepository.Get(x => x.Id == item.CustomerId.ToString()).FirstOrDefault().ShortName;

                    //[ADD][16236][27/08/2021][Collect Amount: Nếu receipt Currency Là VND: Lấy Giá trị Cột VND, Nếu receipt Currency là USD là Cột Collect USD]
                    if (criteria.Currency != null)
                    {
                        d.CurrencyId = criteria.Currency == "VND" ? "VND" : "USD";
                    }
                    //[END]
                    list.Add(d);
                }
            }
            return list.AsQueryable();
        }

        public bool CheckAllowPermissionAction(Guid id, PermissionRange range)
        {
            AcctReceipt receipt = DataContext.Get(o => o.Id == id).FirstOrDefault();
            if (receipt == null)
            {
                return false;
            }

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = receipt.UserCreated,
                CompanyId = receipt.CompanyId,
                DepartmentId = receipt.DepartmentId,
                OfficeId = receipt.OfficeId,
                GroupId = receipt.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, range, currentUser);

            if (code == 403) return false;

            return true;
        }

        public HandleState Delete(Guid id)
        {
            HandleState hs = new HandleState();
            AcctReceipt receipt = DataContext.Get(o => o.Id == id).FirstOrDefault();

            if (receipt == null) return new HandleState((object)"Not found receipt");
            if (receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE) return new HandleState((object)"Not allow delete. Receipt has been done");
            if (receipt.Status == AccountingConstants.RECEIPT_STATUS_CANCEL) return new HandleState((object)"Not allow delete. Receipt has canceled");
            if (receipt.Status == AccountingConstants.RECEIPT_STATUS_DRAFT)
            {
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        hs = DataContext.Delete(x => x.Id == receipt.Id);
                        if (hs.Success)
                        {
                            var payments = acctPaymentRepository.Get(x => x.ReceiptId == id).Select(x => x.Id).ToList();
                            if (payments.Count() > 0)
                            {
                                var hsDeletePayment = DeletePayments(payments);
                            }
                        }

                        trans.Commit();
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
            return hs;
        }

        public IQueryable<AcctReceiptModel> Paging(AcctReceiptCriteria criteria, int page, int size, out int rowsCount)
        {
            IQueryable<AcctReceipt> data = GetQueryBy(criteria);

            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange permissionRangeList = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);

            data = QueryByPermission(data, permissionRangeList, _user);
            rowsCount = data.Count();

            if (page == 0)
            {
                page = 1;
                size = rowsCount;
            }
            IQueryable<AcctReceiptModel> result = FormatReceipt(data, criteria);

            return result.Skip((page - 1) * size).Take(size);
        }

        public IQueryable<AcctReceipt> Query(AcctReceiptCriteria criteria)
        {
            return GetQueryBy(criteria);
        }

        public IQueryable<AcctReceipt> QueryByPermission(IQueryable<AcctReceipt> data, PermissionRange range, ICurrentUser currentUser)
        {
            switch (range)
            {
                case PermissionRange.None:
                    data = null;
                    break;
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    data = data.Where(x => x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Group:
                    data = data.Where(x => x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Department:
                    data = data.Where(x => x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Office:
                    data = data.Where(x => x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Company:
                    data = data.Where(x => x.CompanyId == currentUser.CompanyID);
                    break;
            }
            return data;
        }

        public string GenerateReceiptNo()
        {
            string ReceiptNo = "PT" + DateTime.Now.ToString("yy");

            IQueryable<string> codes = DataContext.Where(x => x.PaymentRefNo.Contains(ReceiptNo)).Select(x => x.PaymentRefNo);

            List<int> oders = new List<int>();

            if (codes != null & codes.Count() > 0)
            {
                foreach (string code in codes)
                {
                    if (code.Length > 8 && int.TryParse(code.Substring(code.Length - 4), out int _))
                    {
                        oders.Add(int.Parse(code.Substring(code.Length - 4)));
                    }
                }

                if (oders.Count() > 0)
                {
                    int maxCurrentOder = oders.Max();

                    ReceiptNo += (maxCurrentOder + 1).ToString("00000");
                }
                else
                {
                    ReceiptNo += "00001";
                }
            }
            else
            {
                ReceiptNo += "00001";
            }

            return ReceiptNo;
        }

        public List<ReceiptInvoiceModel> GetInvoiceForReceipt(ReceiptInvoiceCriteria criteria)
        {
            List<ReceiptInvoiceModel> results = new List<ReceiptInvoiceModel>();
            string agreementService = string.Empty;
            string agreementBaseOn = string.Empty;
            if (!string.IsNullOrEmpty(criteria.AgreementID))
            {
                CatContract agreement = catContractRepository.Get(x => x.Id.ToString() == criteria.AgreementID).FirstOrDefault();
                if (agreement != null)
                {
                    agreementService = agreement.SaleService;
                    agreementBaseOn = agreement.BaseOn;
                }
            }
            //Các đối tượng con có A/C Ref là đối tượng trừ công nợ
            List<string> partnerChild = catPartnerRepository.Get(x => x.ParentId == criteria.CustomerID).Select(x => x.Id).ToList();

            Expression<Func<AccAccountingManagement, bool>> queryInvoice = null;
            queryInvoice = x => (
            (x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE || x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE)
             && (x.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED)
             && (partnerChild.Contains(x.PartnerId))
             && (x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID || x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART)
             && ((agreementService ?? "").Contains(x.ServiceType ?? "", StringComparison.OrdinalIgnoreCase))
             );

            // Trường hợp Base On của contract không có giá trị hoặc Base On = 'Invoice Date' thì search theo ngày Invoice Date
            if (string.IsNullOrEmpty(agreementBaseOn) || agreementBaseOn == AccountingConstants.AGREEMENT_BASE_ON_INVOICE_DATE)
            {
                if (criteria.FromDate != null && criteria.ToDate != null)
                {
                    queryInvoice = queryInvoice.And(x => x.Date.HasValue && x.Date.Value.Date >= criteria.FromDate.Value.Date && x.Date.Value.Date <= criteria.ToDate.Value.Date);
                }
            }
            else
            {
                if (agreementBaseOn == AccountingConstants.AGREEMENT_BASE_ON_CONFIRMED_BILLING)
                {
                    if (criteria.FromDate != null && criteria.ToDate != null)
                    {
                        queryInvoice = queryInvoice.And(x => x.ConfirmBillingDate.Value.Date >= criteria.FromDate.Value.Date && x.ConfirmBillingDate.Value.Date <= criteria.ToDate.Value.Date);
                    }
                }
            }

            IQueryable<AccAccountingManagement> invoices = acctMngtRepository.Get(queryInvoice); // Get danh sách hóa đơn

            if (invoices != null && invoices.Count() > 0)
            {
                IQueryable<CatPartner> partners = catPartnerRepository.Get();

                var queryReceiptInvoice = from invoice in invoices
                                          join partner in partners on invoice.PartnerId equals partner.Id into grpPartners
                                          from grpPartner in grpPartners.DefaultIfEmpty()
                                          orderby invoice.PaymentStatus
                                          select new { invoice, grpPartner };

                if (queryReceiptInvoice != null)
                {
                    results = queryReceiptInvoice.Select(x => new ReceiptInvoiceModel
                    {

                        //InvoiceId = x.invoice.Id.ToString(),
                        //InvoiceNo = x.invoice.InvoiceNoReal,
                        //Currency = x.invoice.Currency,
                        //SerieNo = x.invoice.Serie,
                        //InvoiceDate = x.invoice.Date,
                        //UnpaidAmount = x.invoice.UnpaidAmount ?? 0,
                        //Type = x.invoice.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE ? "DEBIT" : "OBH",
                        //PaymentStatus = x.invoice.PaymentStatus,
                        //PartnerName = x.grpPartner.ShortName,
                        //TaxCode = x.grpPartner.TaxCode,
                        //BillingDate = x.invoice.ConfirmBillingDate,

                    }).ToList();

                    int index = 1;
                    //foreach (var item in results)
                    //{
                    //    item.Index = index++;
                    //}
                }
            }

            return results;
        }

        public AcctReceiptModel GetById(Guid id)
        {
            AcctReceipt receipt = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (receipt == null) return new AcctReceiptModel();

            AcctReceiptModel result = mapper.Map<AcctReceiptModel>(receipt);
            List<ReceiptInvoiceModel> paymentReceipts = new List<ReceiptInvoiceModel>();
            IOrderedEnumerable<AccAccountingPayment> acctPayments = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id)
                .ToList()
                .OrderBy(x => x.PaymentType == "OTHER");

            IEnumerable<AccAccountingPayment> listOBH = acctPayments.Where(x => x.Type == "OBH").OrderBy(x => x.DatetimeCreated);
            if (listOBH.Count() > 0)
            {
                var OBHGrp = listOBH.GroupBy(x => new { x.BillingRefNo, x.Negative, x.CurrencyId });

                List<ReceiptInvoiceModel> items = OBHGrp.Select(s => new ReceiptInvoiceModel
                {
                    RefNo = s.Key.BillingRefNo,
                    Type = "OBH",
                    InvoiceNo = null,
                    Amount = s.FirstOrDefault().RefAmount,
                    UnpaidAmount = s.Key.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? s.Sum(x => x.UnpaidPaymentAmountVnd) : s.Sum(x => x.UnpaidPaymentAmountUsd),
                    UnpaidAmountVnd = s.FirstOrDefault().UnpaidPaymentAmountVnd,
                    UnpaidAmountUsd = s.FirstOrDefault().UnpaidPaymentAmountUsd,
                    PaidAmountVnd = s.Sum(x => x.PaymentAmountVnd),
                    PaidAmountUsd = s.Sum(x => x.PaymentAmountUsd),
                    TotalPaidVnd = s.FirstOrDefault().TotalPaidVnd,
                    TotalPaidUsd = s.FirstOrDefault().TotalPaidUsd,
                    Notes = s.FirstOrDefault().Note,
                    OfficeId = s.FirstOrDefault().OfficeInvoiceId,
                    OfficeName = officeRepository.Get(x => x.Id == s.FirstOrDefault().OfficeInvoiceId)?.FirstOrDefault().ShortName,
                    CompanyId = s.FirstOrDefault().CompanyInvoiceId,
                    RefIds = listOBH.Where(x => x.BillingRefNo == s.Key.BillingRefNo).Select(x => x.RefId).ToList(),
                    CreditNo = s.FirstOrDefault().CreditNo,
                    Hblid = s.FirstOrDefault().Hblid,
                    Mbl = GetHBLInfo(s.FirstOrDefault().Hblid).MBL,
                    Hbl = GetHBLInfo(s.FirstOrDefault().Hblid).HBLNo,
                    JobNo = GetHBLInfo(s.FirstOrDefault().Hblid).JobNo,
                    PaymentStatus = GetPaymentStatus(listOBH.Where(x => x.BillingRefNo == s.Key.BillingRefNo).Select(x => x.RefId).ToList()),
                    ExchangeRateBilling = s.FirstOrDefault().ExchangeRateBilling,
                    PartnerId = s.FirstOrDefault()?.PartnerId?.ToString(),
                    Negative = s.FirstOrDefault()?.Negative,
                    PaymentType = s.FirstOrDefault().PaymentType
                }).ToList();

                paymentReceipts.AddRange(items);
            }

            IEnumerable<AccAccountingPayment> listDebitCredit = acctPayments.Where(x => x.Type != "OBH").OrderBy(x => x.DatetimeCreated);
            if (listDebitCredit.Count() > 0)
            {
                foreach (var acctPayment in listDebitCredit)
                {
                    CatDepartment dept = departmentRepository.Get(x => x.Id == acctPayment.DeptInvoiceId)?.FirstOrDefault();
                    SysOffice office = officeRepository.Get(x => x.Id == acctPayment.OfficeInvoiceId)?.FirstOrDefault();
                    string _jobNo = string.Empty;
                    string _Hbl = string.Empty;
                    string _Mbl = string.Empty;

                    if (acctPayment.Hblid != null && acctPayment.Hblid != Guid.Empty)
                    {
                        CsTransactionDetail hbl = csTransactionDetailRepository.Get(x => x.Id == acctPayment.Hblid)?.FirstOrDefault();
                        if (hbl != null)
                        {
                            CsTransaction job = csTransactionRepository.Get(x => x.Id == hbl.JobId)?.FirstOrDefault();
                            _Hbl = hbl.Hwbno;
                            _Mbl = hbl.Mawb;
                            _jobNo = job?.JobNo;
                        }
                    }

                    string _voucherId = string.Empty;
                    string _voucherIdre = string.Empty;
                    if (acctPayment.Type == "CREDITNOTE")
                    {
                        CsShipmentSurcharge surcharge = surchargeRepository.Get(x => x.CreditNo == acctPayment.BillingRefNo)?.FirstOrDefault();
                        if (surcharge != null)
                        {
                            _voucherId = surcharge.VoucherId;
                            _voucherIdre = surcharge.VoucherIdre;
                        }

                    }

                    ReceiptInvoiceModel payment = new ReceiptInvoiceModel();
                    payment.Id = acctPayment.Id;
                    payment.PaymentId = acctPayment.Id;
                    payment.RefNo = acctPayment.BillingRefNo;
                    payment.InvoiceNo = acctPayment.InvoiceNo;
                    payment.CreditType = acctPayment.Type;
                    payment.Type = acctPayment.Type;
                    payment.CurrencyId = acctPayment.RefCurrency;
                    payment.Amount = acctPayment.RefAmount;
                    payment.UnpaidAmount = acctPayment.RefAmount;
                    payment.UnpaidAmountUsd = acctPayment.UnpaidPaymentAmountUsd;
                    payment.UnpaidAmountVnd = acctPayment.UnpaidPaymentAmountVnd;
                    payment.TotalPaidVnd = acctPayment.TotalPaidVnd;
                    payment.TotalPaidUsd = acctPayment.TotalPaidUsd;
                    payment.PaidAmountUsd = acctPayment.PaymentAmountUsd;
                    payment.PaidAmountVnd = acctPayment.PaymentAmountVnd;
                    payment.Notes = acctPayment.Note;
                    payment.DepartmentId = acctPayment.DeptInvoiceId;
                    payment.OfficeId = acctPayment.OfficeInvoiceId;
                    payment.DepartmentName = dept?.DeptNameAbbr;
                    payment.OfficeName = office?.ShortName;
                    payment.RefIds = string.IsNullOrEmpty(acctPayment.RefId) ? null : acctPayment.RefId.Split(',').ToList();
                    payment.PaymentStatus = acctPayment.Type == "DEBIT" ? GetPaymentStatus(new List<string> { acctPayment.RefId }) : null;
                    payment.JobNo = _jobNo;
                    payment.Mbl = _Mbl;
                    payment.Hbl = _Hbl;
                    payment.Hblid = acctPayment.Hblid;
                    payment.CreditNo = acctPayment.CreditNo;
                    payment.CreditAmountVnd = acctPayment.CreditAmountVnd;
                    payment.CreditAmountUsd = acctPayment.CreditAmountUsd;
                    payment.VoucherId = acctPayment.Type == "CREDITNOTE" ? _voucherId : null;
                    payment.VoucherIdre = acctPayment.Type == "CREDITNOTE" ? _voucherIdre : null;
                    payment.ExchangeRateBilling = acctPayment.ExchangeRateBilling;
                    payment.PartnerId = acctPayment?.PartnerId?.ToString();
                    payment.Negative = acctPayment.Negative;
                    payment.PaymentType = acctPayment.PaymentType;
                    payment.NetOff = acctPayment.NetOff;
                    payment.NetOffUsd = acctPayment.NetOffUsd;
                    payment.NetOffVnd = acctPayment.NetOffVnd;

                    List<string> _creditNos = new List<string>();
                    if (!string.IsNullOrEmpty(acctPayment.CreditNo))
                    {
                        _creditNos = acctPayment.CreditNo.Split(",").ToList();
                    }
                    payment.CreditNos = _creditNos;
                    paymentReceipts.Add(payment);
                }
            }
            result.Payments = paymentReceipts;
            result.UserNameCreated = sysUserRepository.Where(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
            result.UserNameModified = sysUserRepository.Where(x => x.Id == result.UserModified).FirstOrDefault()?.Username;

            CatPartner partnerInfo = catPartnerRepository.Get(x => x.Id == result.CustomerId).FirstOrDefault();
            result.CustomerName = partnerInfo?.ShortName;

            // Check có tồn tại 1 invoice thu chưa hết.
            if (result.Status == AccountingConstants.RECEIPT_STATUS_DONE)
            {
                result.IsReceiptBankFee = result.Payments.Any(x => !string.IsNullOrEmpty(x.PaymentStatus) && x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART);
                if (result.IsReceiptBankFee == true)
                {
                    var obhPaidAprt = result.Payments.Where(x => x.Type == "OBH" && x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART);
                    if (obhPaidAprt.Count() > 0)
                    {
                        foreach (var item in obhPaidAprt)
                        {
                            // filter ID without Paid
                            var ids = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE && item.RefIds.Contains(x.Id.ToString())
                           && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID).Select(x => x.Id.ToString()).ToList();

                            item.RefIds = ids;
                        }
                    }
                }
            }

            //Số phiếu con đã reject
            var totalRejectReceiptSync = receiptSyncRepository.Get(x => x.ReceiptId == receipt.Id && x.SyncStatus == AccountingConstants.STATUS_REJECTED).Count();
            if (totalRejectReceiptSync > 0)
            {
                result.SubRejectReceipt = receipt.SyncStatus != "Rejected" ? " - Rejected(" + totalRejectReceiptSync + ")" : string.Empty;
            }
            return result;
        }

        private HBLInfo GetHBLInfo(Guid? hblId)
        {
            HBLInfo result = new HBLInfo();
            if (hblId == Guid.Empty)
            {
                return result;
            }
            CsTransactionDetail hbl = csTransactionDetailRepository.Get(x => x.Id == hblId)?.FirstOrDefault();
            if (hbl != null)
            {
                CsTransaction job = csTransactionRepository.Get(x => x.Id == hbl.JobId)?.FirstOrDefault();
                result.HBLNo = hbl.Hwbno;
                result.MBL = hbl.Mawb;
                result.JobNo = job?.JobNo;
            }
            return result;
        }

        private string GetPaymentStatus(List<string> Ids)
        {
            string _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
            var inv = acctMngtRepository.Get(x => Ids.Contains(x.Id.ToString())).ToList();
            if (inv.Count > 0)
            {
                if (inv.Count == 1)
                {
                    _paymentStatus = inv.FirstOrDefault().PaymentStatus;
                }
                else
                {
                    bool isPaid = inv.All(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
                    if (isPaid == true)
                    {
                        _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                    }
                    else if (inv.Any(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART))
                    {
                        _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                    }
                }
            }
            return _paymentStatus;
        }

        public HandleState SaveReceipt(AcctReceiptModel receiptModel, SaveAction saveAction)
        {
            var hs = new HandleState();
            switch (saveAction)
            {
                case SaveAction.SAVEDRAFT_ADD:
                    currentUser.Action = "ReceiptSaveDraft";
                    hs = AddDraft(receiptModel);
                    break;
                case SaveAction.SAVEDRAFT_UPDATE:
                    currentUser.Action = "ReceiptUpdateDraft";
                    hs = UpdateDraft(receiptModel);
                    break;
                case SaveAction.SAVEDONE:
                    currentUser.Action = "ReceiptSaveDone";
                    hs = SaveDone(receiptModel);
                    break;
                case SaveAction.SAVECANCEL:
                    currentUser.Action = "ReceiptSaveCancel";
                    hs = SaveCancel(receiptModel.Id);
                    break;
            }
            return hs;
        }

        private string GenerateAdvNo()
        {
            string advNo = "AD" + DateTime.Now.ToString("yyyy");
            string no = "0001";
            IQueryable<string> paymentNewests = acctPaymentRepository.Get(x => x.PaymentType == "OTHER" && x.BillingRefNo.Contains("AD") && x.BillingRefNo.Substring(2, 4) == DateTime.Now.ToString("yyyy"))
                .OrderByDescending(o => o.BillingRefNo).Select(s => s.BillingRefNo);
            string paymentNewest = paymentNewests.FirstOrDefault();
            if (!string.IsNullOrEmpty(paymentNewest))
            {
                var _noNewest = paymentNewest.Substring(6, 4);
                if (_noNewest != "9999")
                {
                    no = (Convert.ToInt32(_noNewest) + 1).ToString();
                    no = no.PadLeft(4, '0');
                }
            }
            return advNo + no;
        }

        private AccAccountingPayment GeneratePaymentOBH(ReceiptInvoiceModel paymentGroupOBH, AcctReceipt receipt, AccAccountingManagement invTemp)
        {
            AccAccountingPayment _payment = new AccAccountingPayment();

            _payment.Id = Guid.NewGuid();
            _payment.ReceiptId = receipt.Id;
            _payment.RefId = invTemp.Id.ToString(); // ID hóa đơn tạm
            _payment.BillingRefNo = paymentGroupOBH.RefNo; // Cùng một BillingRefNo SOA DEBIT /DEBITNOTE
            _payment.PaymentNo = invTemp.InvoiceNoReal + "_" + receipt.PaymentRefNo;
            _payment.InvoiceNo = invTemp.InvoiceNoTempt;
            _payment.Type = "OBH";
            _payment.PaymentType = "OBH";
            _payment.CurrencyId = receipt.CurrencyId; // Theo currency của phiếu thu
            _payment.PaidDate = receipt.PaymentDate; //Payment Date Phiếu thu
            _payment.ExchangeRate = receipt.ExchangeRate; //Exchange Rate Phiếu thu
            _payment.PaymentMethod = receipt.PaymentMethod; //Payment Method Phiếu thu

            _payment.PaymentAmountVnd = _payment.TotalPaidVnd = paymentGroupOBH.PaidAmountVnd ?? 0;
            _payment.PaymentAmountUsd = _payment.TotalPaidUsd = paymentGroupOBH.PaidAmountUsd ?? 0;
            _payment.BalanceVnd = (paymentGroupOBH.UnpaidAmountVnd ?? 0) - (paymentGroupOBH.PaidAmountVnd ?? 0);
            _payment.BalanceUsd = (paymentGroupOBH.UnpaidAmountUsd ?? 0) - (paymentGroupOBH.PaidAmountUsd ?? 0);
            _payment.UnpaidPaymentAmountVnd = paymentGroupOBH.UnpaidAmountVnd ?? 0;
            _payment.UnpaidPaymentAmountUsd = paymentGroupOBH.UnpaidAmountUsd ?? 0;

            _payment.RefCurrency = invTemp.Currency; // currency của hóa đơn
            _payment.Note = paymentGroupOBH.Notes; // Cùng một notes
            _payment.RefAmount = paymentGroupOBH.Amount ?? 0; // Tổng UnpaidAmount của group OBH
            _payment.DeptInvoiceId = paymentGroupOBH.DepartmentId;
            _payment.OfficeInvoiceId = paymentGroupOBH.OfficeId;
            _payment.CompanyInvoiceId = paymentGroupOBH.CompanyId;
            _payment.Hblid = paymentGroupOBH.Hblid;
            _payment.CreditNo = paymentGroupOBH.CreditNo;
            _payment.ExchangeRateBilling = paymentGroupOBH.ExchangeRateBilling;
            _payment.PartnerId = paymentGroupOBH.PartnerId;

            _payment.UserCreated = _payment.UserModified = currentUser.UserID;
            _payment.DatetimeCreated = _payment.DatetimeModified = DateTime.Now;
            _payment.GroupId = currentUser.GroupId;
            _payment.DepartmentId = currentUser.DepartmentId;
            _payment.OfficeId = currentUser.OfficeID;
            _payment.CompanyId = currentUser.CompanyID;

            return _payment;
        }

        private List<AccAccountingPayment> GenerateListPaymentOBH(AcctReceipt receipt, List<ReceiptInvoiceModel> paymentOBHGrps)
        {
            List<AccAccountingPayment> results = new List<AccAccountingPayment>();

            foreach (ReceiptInvoiceModel paymentOBH in paymentOBHGrps)
            {
                // lấy ra tất cả các hóa đơn tạm theo group
                List<AccAccountingManagement> invoicesTemp = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE
                && paymentOBH.RefIds.Contains(x.Id.ToString()) && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID)
                         .OrderBy(x => (paymentOBH.CurrencyId == AccountingConstants.CURRENCY_LOCAL) ? x.UnpaidAmountVnd : x.UnpaidAmountUsd)
                         .ToList(); // xắp xếp theo unpaidAmount

                decimal remainOBHAmountVnd = paymentOBH.PaidAmountVnd ?? 0; // Tổng tiền thu VND trên group OBH
                decimal remainOBHAmountUsd = paymentOBH.PaidAmountUsd ?? 0;// Tổng tiền thu USD trên group OBH

                foreach (var invTemp in invoicesTemp)
                {
                    // Tổng Số tiền amount OBH đã thu trên group.
                    if (invTemp.Currency == AccountingConstants.CURRENCY_LOCAL)
                    {
                        if (invTemp.UnpaidAmount <= remainOBHAmountVnd && invTemp.UnpaidAmountVnd <= remainOBHAmountVnd)
                        {
                            if (remainOBHAmountVnd > 0)
                            {
                                // Phát sinh payment
                                AccAccountingPayment _paymentOBH = GeneratePaymentOBH(paymentOBH, receipt, invTemp);
                                _paymentOBH.PaymentAmount = _paymentOBH.PaymentAmountVnd = invTemp.UnpaidAmountVnd;// Số tiền thu
                                _paymentOBH.PaymentAmountUsd = invTemp.UnpaidAmountUsd;

                                _paymentOBH.Balance = _paymentOBH.BalanceVnd = invTemp.UnpaidAmountVnd - _paymentOBH.PaymentAmountVnd; // Số tiền còn lại
                                _paymentOBH.BalanceUsd = invTemp.UnpaidAmountUsd - _paymentOBH.PaymentAmountUsd;

                                // _paymentOBH.PaymentAmountUsd = null;
                                //_paymentOBH.BalanceUsd = null;

                                remainOBHAmountVnd = remainOBHAmountVnd - _paymentOBH.PaymentAmountVnd ?? 0; // Số tiền amount OBH còn lại để clear tiếp phiếu hđ tạm tiếp theo sau.
                                remainOBHAmountUsd = remainOBHAmountUsd - _paymentOBH.PaymentAmountUsd ?? 0; // Số tiền amount OBH còn lại để clear tiếp phiếu hđ tạm tiếp theo sau.
                                results.Add(_paymentOBH);
                            }
                        }
                        else
                        {
                            AccAccountingPayment _paymentOBH = GeneratePaymentOBH(paymentOBH, receipt, invTemp);
                            _paymentOBH.PaymentAmount = _paymentOBH.PaymentAmountVnd = remainOBHAmountVnd;
                            _paymentOBH.PaymentAmountUsd = remainOBHAmountUsd;

                            _paymentOBH.Balance = _paymentOBH.BalanceVnd = invTemp.UnpaidAmountVnd - _paymentOBH.PaymentAmountVnd;
                            _paymentOBH.BalanceUsd = invTemp.UnpaidAmountUsd - _paymentOBH.PaymentAmountUsd;

                            remainOBHAmountVnd = remainOBHAmountVnd - _paymentOBH.PaymentAmountVnd ?? 0;
                            remainOBHAmountUsd = remainOBHAmountUsd - _paymentOBH.PaymentAmountUsd ?? 0;

                            results.Add(_paymentOBH);
                        }
                    }
                    else
                    {
                        if (invTemp.UnpaidAmount <= remainOBHAmountUsd && invTemp.UnpaidAmountUsd <= remainOBHAmountUsd)
                        {
                            if (remainOBHAmountUsd > 0)
                            {
                                // Phát sinh payment
                                AccAccountingPayment _paymentOBH = GeneratePaymentOBH(paymentOBH, receipt, invTemp);
                                _paymentOBH.PaymentAmount = _paymentOBH.PaymentAmountUsd = invTemp.UnpaidAmountUsd;
                                _paymentOBH.PaymentAmountVnd = invTemp.UnpaidAmountVnd;

                                _paymentOBH.Balance = _paymentOBH.BalanceUsd = invTemp.UnpaidAmountUsd - _paymentOBH.PaymentAmountUsd;
                                _paymentOBH.BalanceVnd = invTemp.UnpaidAmountVnd - _paymentOBH.PaymentAmountVnd;

                                remainOBHAmountUsd = remainOBHAmountUsd - _paymentOBH.PaymentAmountUsd ?? 0;
                                remainOBHAmountVnd = remainOBHAmountVnd - _paymentOBH.PaymentAmountVnd ?? 0;

                                results.Add(_paymentOBH);
                            }
                            else
                            {
                                AccAccountingPayment _paymentOBH = GeneratePaymentOBH(paymentOBH, receipt, invTemp);
                                _paymentOBH.PaymentAmount = _paymentOBH.PaymentAmountUsd = remainOBHAmountUsd;
                                _paymentOBH.PaymentAmountVnd = remainOBHAmountVnd;

                                _paymentOBH.Balance = _paymentOBH.BalanceUsd = invTemp.UnpaidAmountUsd - _paymentOBH.PaymentAmountUsd;

                                remainOBHAmountVnd = remainOBHAmountVnd - _paymentOBH.PaymentAmountVnd ?? 0;
                                remainOBHAmountUsd = remainOBHAmountUsd - _paymentOBH.PaymentAmountUsd ?? 0;

                                results.Add(_paymentOBH);
                            }
                        }
                    }
                }
            }

            return results;
        }

        private List<AccAccountingPayment> GenerateListCreditDebitPayment(AcctReceipt receipt, List<ReceiptInvoiceModel> payments)
        {
            List<AccAccountingPayment> results = new List<AccAccountingPayment>();

            foreach (ReceiptInvoiceModel payment in payments)
            {
                AccAccountingPayment _payment = new AccAccountingPayment();
                _payment.Id = Guid.NewGuid();
                _payment.ReceiptId = receipt.Id;
                _payment.BillingRefNo = payment.PaymentType == "OTHER" ? GenerateAdvNo() : payment.RefNo;

                if (payment.PaymentType == "OTHER")
                {
                    _payment.PaymentNo = receipt.PaymentRefNo;
                }
                else
                {
                    _payment.PaymentNo = payment.InvoiceNo + "_" + receipt.PaymentRefNo; //Invoice No + '_' + Receipt No
                }

                if (payment.RefIds != null)
                {
                    _payment.RefId = string.Join(",", payment.RefIds);
                }

                _payment.InvoiceNo = payment.InvoiceNo;
                _payment.Type = payment.Type;  // OBH/DEBIT
                _payment.PaymentAmountUsd = (payment.PaidAmountUsd ?? 0);
                _payment.PaymentAmountVnd = (payment.PaidAmountVnd ?? 0);

                if (payment.CurrencyId == AccountingConstants.CURRENCY_LOCAL)
                {
                    _payment.PaymentAmount = payment.PaidAmountVnd ?? 0;
                    _payment.Balance = (payment.UnpaidAmountVnd ?? 0) - (payment.PaidAmountVnd ?? 0);
                }
                else
                {
                    _payment.PaymentAmount = _payment.PaymentAmountUsd = (payment.PaidAmountUsd ?? 0);
                    _payment.Balance = (payment.UnpaidAmountUsd ?? 0) - (payment.PaidAmountUsd ?? 0);
                }
                _payment.PaymentAmountUsd = (payment.PaidAmountUsd ?? 0);
                _payment.PaymentAmountVnd = (payment.PaidAmountVnd ?? 0);
                _payment.TotalPaidVnd = (payment.TotalPaidVnd ?? 0); // đã bao gồm credit cấn trừ
                _payment.TotalPaidUsd = (payment.TotalPaidUsd ?? 0);

                _payment.BalanceUsd = (payment.UnpaidAmountUsd ?? 0) - (payment.PaidAmountUsd ?? 0);
                _payment.BalanceVnd = (payment.UnpaidAmountVnd ?? 0) - (payment.PaidAmountVnd ?? 0);
                _payment.UnpaidPaymentAmountUsd = payment.UnpaidAmountUsd ?? 0;
                _payment.UnpaidPaymentAmountVnd = payment.UnpaidAmountVnd ?? 0;
                _payment.CurrencyId = receipt.CurrencyId; //Currency Phiếu thu
                _payment.PaidDate = receipt.PaymentDate; //Payment Date Phiếu thu
                _payment.ExchangeRate = receipt.ExchangeRate; //Exchange Rate Phiếu thu
                _payment.ExchangeRateBilling = payment.ExchangeRateBilling; //Exchange Rate trên payment (SOADebit,DebitNote,Invoice)
                _payment.PaymentMethod = receipt.PaymentMethod; //Payment Method Phiếu thu
                _payment.RefCurrency = payment.CurrencyId; // currency của hóa đơn
                _payment.RefAmount = payment.Amount; // Số tiền unpaid của hóa đơn
                _payment.Note = payment.Notes;
                _payment.DeptInvoiceId = payment.DepartmentId;
                _payment.OfficeInvoiceId = payment.OfficeId;
                _payment.CompanyInvoiceId = payment.CompanyId;
                _payment.CreditAmountVnd = payment.CreditAmountVnd;
                _payment.CreditAmountUsd = payment.CreditAmountUsd;
                _payment.PartnerId = receipt.CustomerId;
                _payment.PaymentType = payment.PaymentType;
                _payment.NetOff = payment.NetOff;
                _payment.NetOffUsd = payment.NetOffUsd;
                _payment.NetOffVnd = payment.NetOffVnd;

                _payment.Hblid = payment.Hblid;
                _payment.UserCreated = _payment.UserModified = currentUser.UserID;
                _payment.DatetimeCreated = _payment.DatetimeModified = DateTime.Now;
                _payment.GroupId = currentUser.GroupId;
                _payment.DepartmentId = currentUser.DepartmentId;
                _payment.OfficeId = currentUser.OfficeID;
                _payment.CompanyId = currentUser.CompanyID;

                string _creditNo = string.Empty;

                if (payment.CreditNos != null && payment.CreditNos.Count > 0)
                {
                    _creditNo = string.Join(",", payment.CreditNos);
                }
                _payment.CreditNo = _creditNo;

                results.Add(_payment);
            }
            return results;
        }

        private HandleState AddPayments(List<ReceiptInvoiceModel> listReceiptInvoice, AcctReceipt receipt)
        {
            HandleState hs = new HandleState();

            // Lọc ra tất cả các Payment OBH group để generate các payment theo hđ tạm trong group.
            List<ReceiptInvoiceModel> paymentOBHGrps = listReceiptInvoice.Where(x => x.Type == "OBH").ToList();
            List<AccAccountingPayment> listPaymentOBH = new List<AccAccountingPayment>();

            if (paymentOBHGrps.Count > 0)
            {
                listPaymentOBH = GenerateListPaymentOBH(receipt, paymentOBHGrps);
            }

            List<ReceiptInvoiceModel> paymentDebitAndCredit = listReceiptInvoice.Where(x => x.Type != "OBH").ToList();
            List<AccAccountingPayment> listPaymentDebitCredit = new List<AccAccountingPayment>();

            if (paymentDebitAndCredit.Count > 0)
            {
                listPaymentDebitCredit = GenerateListCreditDebitPayment(receipt, paymentDebitAndCredit);
            }

            hs = acctPaymentRepository.Add(listPaymentOBH, false);
            hs = acctPaymentRepository.Add(listPaymentDebitCredit, false);

            hs = acctPaymentRepository.SubmitChanges();
            return hs;
        }

        private HandleState UpdatePayments(List<ReceiptInvoiceModel> listReceiptInvoice, AcctReceipt receipt)
        {
            HandleState hsUpdate = new HandleState();
            return hsUpdate;
        }

        private HandleState DeletePayments(List<Guid> ids)
        {
            HandleState hsDelete = new HandleState();
            foreach (var id in ids)
            {
                hsDelete = acctPaymentRepository.Delete(x => x.Id == id);
            }
            return hsDelete;
        }

        private HandleState AddPaymentsNegative(List<AccAccountingPayment> payments, AcctReceipt receipt)
        {
            HandleState hs = new HandleState();
            foreach (var payment in payments)
            {
                AccAccountingManagement invoice = acctMngtRepository.Get(x => x.Id.ToString() == payment.RefId).FirstOrDefault();
                AccAccountingPayment _payment = new AccAccountingPayment();

                _payment.Id = Guid.NewGuid();
                _payment.ReceiptId = receipt.Id;

                _payment.BillingRefNo = payment.PaymentType == "OTHER" ? GenerateAdvNo() : payment.BillingRefNo;

                if (payment.PaymentType == "OTHER")
                {
                    _payment.PaymentNo = receipt.PaymentRefNo;
                }
                else
                {
                    _payment.PaymentNo = payment.InvoiceNo + "_" + receipt.PaymentRefNo; //Invoice No + '_' + Receipt No
                }

                //Phát sinh payment amount âm
                _payment.PaymentAmount = -payment.PaymentAmount;
                _payment.PaymentAmountVnd = -payment.PaymentAmountVnd;
                _payment.PaymentAmountUsd = -payment.PaymentAmountUsd;

                // Tính lại Balance
                _payment.Balance = invoice.UnpaidAmount - _payment.PaymentAmount;
                _payment.BalanceVnd = invoice.UnpaidAmountVnd - _payment.PaymentAmountVnd;
                _payment.BalanceUsd = invoice.UnpaidAmountUsd - _payment.PaymentAmountUsd;
                _payment.TotalPaidVnd = -payment.TotalPaidVnd;
                _payment.TotalPaidUsd = -payment.TotalPaidUsd;
                _payment.NetOffUsd = -payment.NetOffUsd;
                _payment.NetOffVnd = -payment.NetOffVnd;

                _payment.UnpaidPaymentAmountVnd = payment.UnpaidPaymentAmountVnd;
                _payment.UnpaidPaymentAmountUsd = payment.UnpaidPaymentAmountUsd;
                _payment.InvoiceNo = payment.InvoiceNo;
                _payment.RefId = payment.RefId;
                _payment.CurrencyId = receipt.CurrencyId; //Currency Phiếu thu
                _payment.PaidDate = receipt.PaymentDate; //Payment Date Phiếu thu
                _payment.Type = payment.Type;
                _payment.ExchangeRate = receipt.ExchangeRate; //Exchange Rate Phiếu thu
                _payment.ExchangeRateBilling = payment.ExchangeRateBilling; //Exchange Rate trên payment (SOADebit,DebitNote,Invoice)

                _payment.PaymentMethod = receipt.PaymentMethod; //Payment Method Phiếu thu
                _payment.RefAmount = payment.RefAmount;
                _payment.RefCurrency = payment.RefCurrency;
                _payment.Note = payment.Note;
                _payment.DeptInvoiceId = payment.DeptInvoiceId;
                _payment.OfficeInvoiceId = payment.OfficeInvoiceId;
                _payment.CompanyInvoiceId = payment.CompanyInvoiceId;
                _payment.CreditNo = payment.CreditNo;
                _payment.CreditAmountVnd = -payment.CreditAmountVnd;
                _payment.CreditAmountUsd = -payment.CreditAmountUsd;
                _payment.Hblid = payment.Hblid;
                _payment.PartnerId = payment.PartnerId;


                _payment.Negative = true;
                _payment.PaymentType = payment.PaymentType;
                _payment.UserCreated = _payment.UserModified = currentUser.UserID;
                _payment.DatetimeCreated = _payment.DatetimeModified = DateTime.Now;
                _payment.GroupId = currentUser.GroupId;
                _payment.DepartmentId = currentUser.DepartmentId;
                _payment.OfficeId = currentUser.OfficeID;
                _payment.CompanyId = currentUser.CompanyID;

                hs = acctPaymentRepository.Add(_payment);
            }
            return hs;
        }

        private string GetAndUpdateStatusInvoice(AccAccountingManagement invoice)
        {
            string _paymentStatus = invoice.PaymentStatus;
            if (invoice.UnpaidAmount <= 0)
            {
                _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
            }
            else if (invoice.UnpaidAmount > 0 && invoice.UnpaidAmount < invoice.TotalAmount)
            {
                _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
            }
            else
            {
                _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
            }

            return _paymentStatus;
        }

        private HandleState UpdateNetOffCredit(AccAccountingPayment payment, bool isCancel = false)
        {
            HandleState hs = new HandleState();

            AcctCreditManagementAr creditsAr = creditMngtArRepository.Get(x => x.Type == AccountingConstants.CREDIT_NOTE_TYPE_CODE && x.Code == payment.BillingRefNo)?.FirstOrDefault();
            if (creditsAr != null)
            {
                if (isCancel)
                {
                    creditsAr.RemainVnd = creditsAr.RemainVnd + payment.PaymentAmountVnd;
                    creditsAr.RemainUsd = creditsAr.RemainUsd + payment.PaymentAmountUsd;
                    creditsAr.NetOff = false;

                }
                else if (creditsAr.NetOff != true)
                {
                    creditsAr.RemainVnd = creditsAr.RemainVnd - payment.PaymentAmountVnd;
                    creditsAr.RemainUsd = creditsAr.RemainUsd - payment.PaymentAmountUsd;

                    if (creditsAr.Currency == AccountingConstants.CURRENCY_LOCAL && creditsAr.RemainVnd <= 0)
                    {
                        creditsAr.NetOff = true;
                    }
                    else if (creditsAr.Currency != AccountingConstants.CURRENCY_LOCAL && creditsAr.RemainUsd <= 0)
                    {
                        creditsAr.NetOff = true;
                    }
                }
                hs = creditMngtArRepository.Update(creditsAr, x => x.Id == creditsAr.Id);
            }

            return hs;
        }

        private HandleState UpdateNetOffSoa(AccAccountingPayment payment, bool isCancel = false)
        {
            HandleState hs = new HandleState();
            AcctCreditManagementAr creditsAr = creditMngtArRepository.Get(x => x.Type == AccountingConstants.CREDIT_SOA_TYPE_CODE && x.Code == payment.BillingRefNo)?.FirstOrDefault();

            if (creditsAr != null)
            {
                if (isCancel)
                {
                    creditsAr.RemainVnd = creditsAr.RemainVnd + payment.PaymentAmountVnd;
                    creditsAr.RemainUsd = creditsAr.RemainUsd + payment.PaymentAmountUsd;
                    creditsAr.NetOff = false;
                }
                else if (creditsAr.NetOff != true)
                {
                    creditsAr.RemainVnd = creditsAr.RemainVnd - payment.PaymentAmountVnd;
                    creditsAr.RemainUsd = creditsAr.RemainUsd - payment.PaymentAmountUsd;

                    if (creditsAr.Currency == AccountingConstants.CURRENCY_LOCAL && creditsAr.RemainVnd <= 0)
                    {
                        creditsAr.NetOff = true;
                    }
                    else if (creditsAr.Currency != AccountingConstants.CURRENCY_LOCAL && creditsAr.RemainUsd <= 0)
                    {
                        creditsAr.NetOff = true;
                    }
                }
                hs = creditMngtArRepository.Update(creditsAr, x => x.Id == creditsAr.Id);
            }

            return hs;
        }

        private HandleState UpdateInvoiceOfPayment(Guid receiptId)
        {
            HandleState hsInvoiceUpdate = new HandleState();
            IQueryable<AccAccountingPayment> payments = acctPaymentRepository.Get(x => x.ReceiptId == receiptId);
            foreach (AccAccountingPayment payment in payments)
            {
                switch (payment.Type)
                {
                    case "DEBIT":
                        // Tổng thu của invoice bao gôm VND/USD.
                        AccAccountingManagement invoice = acctMngtRepository.Get(x => x.Id.ToString() == payment.RefId && x.Type != AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE).FirstOrDefault();

                        decimal totalAmountPayment = payment.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (payment.TotalPaidVnd ?? 0) : (payment.TotalPaidUsd ?? 0);
                        decimal totalAmountVndPaymentOfInv = payment.TotalPaidVnd ?? 0;
                        decimal totalAmountUsdPaymentOfInv = payment.TotalPaidUsd ?? 0;

                        invoice.PaidAmount = (invoice.PaidAmount ?? 0) + totalAmountPayment;
                        invoice.PaidAmountUsd = (invoice.PaidAmountUsd ?? 0) + totalAmountUsdPaymentOfInv;
                        invoice.PaidAmountVnd = (invoice.PaidAmountVnd ?? 0) + totalAmountVndPaymentOfInv;

                        invoice.UnpaidAmount = (invoice.TotalAmount ?? 0) - invoice.PaidAmount;
                        invoice.UnpaidAmountUsd = (invoice.TotalAmountUsd ?? 0) - invoice.PaidAmountUsd;
                        invoice.UnpaidAmountVnd = (invoice.TotalAmountVnd ?? 0) - invoice.PaidAmountVnd;

                        invoice.PaymentStatus = GetAndUpdateStatusInvoice(invoice);
                        invoice.UserModified = currentUser.UserID;
                        invoice.DatetimeModified = DateTime.Now;

                        if (invoice.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID)
                        {
                            invoice.UnpaidAmount = invoice.UnpaidAmountVnd = invoice.UnpaidAmountUsd = 0;
                        }

                        hsInvoiceUpdate = acctMngtRepository.Update(invoice, x => x.Id == invoice.Id);
                        break;
                    case "OBH":
                        // Lấy ra từng hóa đơn tạm để cấn trừ
                        IQueryable<AccAccountingManagement> invoicesTemp = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE && x.Id.ToString() == payment.RefId);
                        if (invoicesTemp != null && invoicesTemp.Count() > 0)
                        {
                            decimal remainAmount = payment.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (payment.TotalPaidVnd ?? 0) : (payment.TotalPaidUsd ?? 0); // Số tiền đã thu của hóa đơn;
                            decimal remainAmountUsd = payment.TotalPaidUsd ?? 0;
                            decimal remainAmountVnd = payment.TotalPaidVnd ?? 0;
                            foreach (AccAccountingManagement item in invoicesTemp)
                            {
                                //1. Số tiền còn lại của payment lớn hơn số tiền phải thu của invoice
                                if (remainAmount > 0 && remainAmount >= item.UnpaidAmount)
                                {
                                    decimal _invoiceAmountUnpaid = (item.UnpaidAmount ?? 0); // số tiền còn lại cần thu
                                    decimal _invoiceAmountUnpaidVnd = (item.UnpaidAmountVnd ?? 0);
                                    decimal _invoiceAmountUnpaidUsd = (item.UnpaidAmountUsd ?? 0);

                                    item.PaidAmount = (item.PaidAmount ?? 0) + item.UnpaidAmount;
                                    item.PaidAmountVnd = (item.PaidAmountVnd ?? 0) + item.UnpaidAmountVnd;
                                    item.PaidAmountUsd = (item.PaidAmountUsd ?? 0) + item.UnpaidAmountUsd;

                                    // Số tiền còn lại của hóa đơn
                                    item.UnpaidAmount = (item.UnpaidAmount ?? 0) - _invoiceAmountUnpaid;
                                    item.UnpaidAmountUsd = (item.UnpaidAmountUsd ?? 0) - _invoiceAmountUnpaidUsd;
                                    item.UnpaidAmountVnd = (item.UnpaidAmountVnd ?? 0) - _invoiceAmountUnpaidVnd;

                                    if (item.Currency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        // Cập nhật lại số tiền còn lại sau khi trừ cho hđ này
                                        remainAmount = remainAmount - _invoiceAmountUnpaidVnd;
                                    }
                                    else
                                    {
                                        remainAmount = remainAmount - _invoiceAmountUnpaidUsd;
                                    }

                                    remainAmountVnd = remainAmountVnd - _invoiceAmountUnpaidVnd;
                                    remainAmountUsd = remainAmountUsd - _invoiceAmountUnpaidUsd;
                                }
                                else
                                {
                                    item.PaidAmount = (item.PaidAmount ?? 0) + remainAmount;
                                    item.PaidAmountVnd = (item.PaidAmountVnd ?? 0) + remainAmountVnd;
                                    item.PaidAmountUsd = (item.PaidAmountUsd ?? 0) + remainAmountUsd;

                                    item.UnpaidAmount = (item.UnpaidAmount ?? 0) - item.PaidAmount;
                                    item.UnpaidAmountVnd = (item.UnpaidAmountVnd ?? 0) - item.PaidAmountVnd;
                                    item.UnpaidAmountUsd = (item.UnpaidAmountUsd ?? 0) - item.PaidAmountUsd;

                                    remainAmountUsd = 0;
                                    remainAmountVnd = 0;
                                    remainAmount = 0;
                                }

                                item.PaymentStatus = GetAndUpdateStatusInvoice(item);
                                item.UserModified = currentUser.UserID;
                                item.DatetimeModified = DateTime.Now;

                                if (item.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID)
                                {
                                    item.UnpaidAmount = item.UnpaidAmountVnd = item.UnpaidAmountUsd = 0;
                                }

                                hsInvoiceUpdate = acctMngtRepository.Update(item, x => x.Id == item.Id);
                            }
                        }
                        break;
                    case "CREDITNOTE":
                        HandleState hsCredit = UpdateNetOffCredit(payment);
                        break;
                    case "CREDITSOA":
                        HandleState hsSoa = UpdateNetOffSoa(payment);
                        break;
                    default:
                        break;
                }

            }
            return hsInvoiceUpdate;
        }

        private HandleState UpdateInvoiceOfPaymentCancel(Guid receiptId)
        {
            HandleState hsInvoiceUpdate = new HandleState();

            var payments = acctPaymentRepository.Get(x => x.ReceiptId == receiptId && (x.Type == "DEBIT" || x.Type == "OBH")).Where(x => x.PaymentAmount < 0).ToList();

            if (payments.Count > 0)
            {
                foreach (var payment in payments)
                {
                    AccAccountingManagement invoice = acctMngtRepository.Get(x => x.Id.ToString() == payment.RefId).FirstOrDefault();

                    invoice.PaidAmount = (invoice.PaidAmount ?? 0) + (payment.PaymentAmount ?? 0);
                    invoice.PaidAmountVnd = (invoice.PaidAmountVnd ?? 0) + (payment.PaymentAmountVnd ?? 0);
                    invoice.PaidAmountUsd = (invoice.PaidAmountUsd ?? 0) + (payment.PaymentAmountUsd ?? 0);

                    invoice.UnpaidAmount = (invoice.TotalAmount ?? 0) - invoice.PaidAmount;
                    invoice.UnpaidAmountVnd = (invoice.TotalAmountVnd ?? 0) - invoice.PaidAmountVnd;
                    invoice.UnpaidAmountUsd = (invoice.TotalAmountUsd ?? 0) - invoice.PaidAmountUsd;

                    invoice.PaymentStatus = GetAndUpdateStatusInvoice(invoice);

                    invoice.UserModified = currentUser.UserID;
                    invoice.DatetimeModified = DateTime.Now;

                    hsInvoiceUpdate = acctMngtRepository.Update(invoice, x => x.Id == invoice.Id);
                }
            }

            return hsInvoiceUpdate;
        }
        private HandleState UpdateCusAdvanceOfAgreement(AcctReceipt receipt, SaveAction action)
        {
            HandleState hsAgreementUpdate = new HandleState();
            CatContract agreement = catContractRepository.Get(x => x.Id == receipt.AgreementId).FirstOrDefault();
            decimal? totalAdvPayment = acctPaymentRepository.Where(x => x.ReceiptId == receipt.Id && x.Type == "ADV")
              .Select(s => s.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? s.PaymentAmountVnd : s.PaymentAmountUsd)
              .Sum();


            if (action != SaveAction.SAVECANCEL)
            {
                if (agreement != null)
                {
                    decimal _cusAdv = 0;
                    if (agreement.CreditCurrency != AccountingConstants.CURRENCY_LOCAL)
                    {
                        _cusAdv = (totalAdvPayment ?? 0) + (agreement.CustomerAdvanceAmount ?? 0) - (receipt.CusAdvanceAmountUsd ?? 0);
                    }
                    else
                    {
                        _cusAdv = (totalAdvPayment ?? 0) + (agreement.CustomerAdvanceAmount ?? 0) - (receipt.CusAdvanceAmountVnd ?? 0);
                    }
                    agreement.CustomerAdvanceAmount = _cusAdv;
                }
            }
            else
            {
                if (agreement != null)
                {
                    if (agreement.CreditCurrency != AccountingConstants.CURRENCY_LOCAL)
                    {
                        agreement.CustomerAdvanceAmount = (agreement.CustomerAdvanceAmount ?? 0) + (receipt.CusAdvanceAmountUsd ?? 0) - (totalAdvPayment ?? 0);
                    }
                    else
                    {
                        agreement.CustomerAdvanceAmount = (agreement.CustomerAdvanceAmount ?? 0) + (receipt.CusAdvanceAmountVnd ?? 0) - (totalAdvPayment ?? 0);
                    }
                }
            }

            agreement.UserModified = currentUser.UserID;
            agreement.DatetimeModified = DateTime.Now;

            hsAgreementUpdate = catContractRepository.Update(agreement, x => x.Id == agreement.Id);
            return hsAgreementUpdate;
        }

        public HandleState AddDraft(AcctReceiptModel receiptModel)
        {
            try
            {

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        receiptModel.Id = Guid.NewGuid();
                        receiptModel.Status = AccountingConstants.RECEIPT_STATUS_DRAFT;
                        receiptModel.UserCreated = receiptModel.UserModified = currentUser.UserID;
                        receiptModel.DatetimeCreated = receiptModel.DatetimeModified = DateTime.Now;
                        receiptModel.GroupId = currentUser.GroupId;
                        receiptModel.DepartmentId = currentUser.DepartmentId;
                        receiptModel.OfficeId = currentUser.OfficeID;
                        receiptModel.CompanyId = currentUser.CompanyID;
                        receiptModel.PaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.PaidAmountVnd : receiptModel.PaidAmountUsd;
                        receiptModel.FinalPaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.FinalPaidAmountVnd : receiptModel.FinalPaidAmountUsd;

                        CatPartner partner = catPartnerRepository.Get(x => x.Id == receiptModel.CustomerId)?.FirstOrDefault();
                        if (partner != null)
                        {
                            receiptModel.Type = partner.PartnerType == "Agent" ? "Agent" : "Customer";
                        }

                        AcctReceipt receipt = mapper.Map<AcctReceipt>(receiptModel);
                        HandleState hs = DataContext.Add(receipt, false);
                        if (hs.Success)
                        {
                            HandleState hsPayment = AddPayments(receiptModel.Payments, receipt);
                            DataContext.SubmitChanges();
                            trans.Commit();
                        }
                        return hs;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                        return new HandleState((object)ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        public HandleState UpdateDraft(AcctReceiptModel receiptModel)
        {
            try
            {
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        AcctReceipt receiptCurrent = DataContext.Get(x => x.Id == receiptModel.Id).FirstOrDefault();
                        if (receiptCurrent == null) return new HandleState((object)"Not found receipt");
                        if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_DONE) return new HandleState((object)"Not allow save draft. Receipt has been done");
                        if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_CANCEL) return new HandleState((object)"Not allow save draft. Receipt has canceled");

                        receiptModel.UserModified = currentUser.UserID;
                        receiptModel.DatetimeModified = DateTime.Now;
                        receiptModel.GroupId = currentUser.GroupId;
                        receiptModel.DepartmentId = currentUser.DepartmentId;
                        receiptModel.OfficeId = currentUser.OfficeID;
                        receiptModel.CompanyId = currentUser.CompanyID;
                        receiptModel.ReferenceId = receiptCurrent.ReferenceId;

                        receiptModel.PaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.PaidAmountVnd : receiptModel.PaidAmountUsd;
                        receiptModel.FinalPaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.FinalPaidAmountVnd : receiptModel.FinalPaidAmountUsd;

                        CatPartner partner = catPartnerRepository.Get(x => x.Id == receiptModel.CustomerId)?.FirstOrDefault();
                        if (partner != null)
                        {
                            receiptModel.Type = partner.PartnerType == "Agent" ? "Agent" : "Customer";
                        }

                        AcctReceipt receipt = mapper.Map<AcctReceipt>(receiptModel);
                        HandleState hs = DataContext.Update(receipt, x => x.Id == receipt.Id, false);
                        if (hs.Success)
                        {
                            // Xóa các payment hiện tại, add các payment mới khi update
                            List<Guid> paymentsOldDelete = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id).Select(x => x.Id).ToList();
                            HandleState hsPaymentUpdate = AddPayments(receiptModel.Payments, receipt);

                            HandleState hsPaymentDelete = DeletePayments(paymentsOldDelete);

                            DataContext.SubmitChanges();
                            trans.Commit();
                        }

                        return hs;
                    }
                    catch (Exception ex)
                    {
                        new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                        trans.Rollback();
                        return new HandleState((object)ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        public HandleState SaveDone(AcctReceiptModel receiptModel)
        {
            try
            {
                bool isAddNew = false;

                CatPartner partner = catPartnerRepository.Get(x => x.Id == receiptModel.CustomerId)?.FirstOrDefault();
                if (partner != null)
                {
                    receiptModel.Type = partner.PartnerType == "Agent" ? "Agent" : "Customer";
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        if (receiptModel.Id == Guid.Empty || receiptModel.Id == null)
                        {
                            isAddNew = true;
                            receiptModel.Id = Guid.NewGuid();
                            receiptModel.UserCreated = receiptModel.UserModified = currentUser.UserID;
                            receiptModel.DatetimeCreated = receiptModel.DatetimeModified = DateTime.Now;
                            receiptModel.GroupId = currentUser.GroupId;
                            receiptModel.DepartmentId = currentUser.DepartmentId;
                            receiptModel.OfficeId = currentUser.OfficeID;
                            receiptModel.CompanyId = currentUser.CompanyID;
                            receiptModel.Status = AccountingConstants.RECEIPT_STATUS_DONE;
                            receiptModel.UserModified = currentUser.UserID;
                            receiptModel.DatetimeModified = DateTime.Now;
                            receiptModel.PaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.PaidAmountVnd : receiptModel.PaidAmountUsd;
                            receiptModel.FinalPaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.FinalPaidAmountVnd : receiptModel.FinalPaidAmountUsd;

                            AcctReceipt receiptData = mapper.Map<AcctReceipt>(receiptModel);
                            HandleState hs = DataContext.Add(receiptData);
                            if (hs.Success)
                            {
                                AcctReceipt receiptCurrent = DataContext.Get(x => x.Id == receiptModel.Id).FirstOrDefault();

                                // Phát sinh Payment
                                HandleState hsPaymentUpdate = AddPayments(receiptModel.Payments, receiptCurrent);
                                // cấn trừ cho hóa đơn
                                hs = UpdateInvoiceOfPayment(receiptModel.Id);
                                // Cập nhật CusAdvance cho hợp đồng
                                HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptModel, SaveAction.SAVEDONE);

                                //TODO: Tính lại công nợ trên hợp đồng (Tính công nợ ở bên ngoài Controller)
                                trans.Commit();
                            }

                            return hs;
                        }
                        else
                        {
                            isAddNew = false;
                            AcctReceipt receiptCurrent = DataContext.Get(x => x.Id == receiptModel.Id).FirstOrDefault();

                            // Xóa các payment hiện tại, add các payment mới khi update
                            List<Guid> paymentsDelete = acctPaymentRepository.Get(x => x.ReceiptId == receiptCurrent.Id).Select(x => x.Id).ToList();
                            HandleState hsPaymentUpdate = AddPayments(receiptModel.Payments, receiptCurrent);

                            HandleState hsPaymentDelete = DeletePayments(paymentsDelete);

                            // Done Receipt
                            HandleState hs = new HandleState();
                            if (receiptCurrent == null) return new HandleState((object)"Not found receipt");

                            if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_CANCEL) return new HandleState((object)"Not allow save done. Receipt has canceled");

                            receiptCurrent.Status = AccountingConstants.RECEIPT_STATUS_DONE;
                            receiptCurrent.UserModified = currentUser.UserID;
                            receiptCurrent.DatetimeModified = DateTime.Now;


                            hs = DataContext.Update(receiptCurrent, x => x.Id == receiptCurrent.Id);
                            if (hs.Success)
                            {
                                // cấn trừ cho hóa đơn
                                hs = UpdateInvoiceOfPayment(receiptModel.Id);

                                // Cập nhật CusAdvance cho hợp đồng
                                HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptCurrent, SaveAction.SAVEDONE);
                                //TODO: Tính lại công nợ trên hợp đồng (Tính bên ngoài Controller)
                                trans.Commit();
                            }

                            return hs;
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                        return new HandleState((object)ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }


            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }

        }

        public HandleState SaveDoneReceipt(Guid receiptId)
        {
            try
            {
                HandleState hs = new HandleState();
                AcctReceipt receiptCurrent = DataContext.Get(x => x.Id == receiptId).FirstOrDefault();
                if (receiptCurrent == null) return new HandleState((object)"Not found receipt");

                if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_CANCEL) return new HandleState((object)"Not allow save done. Receipt has canceled");

                receiptCurrent.Status = AccountingConstants.RECEIPT_STATUS_DONE;
                receiptCurrent.UserModified = currentUser.UserID;
                receiptCurrent.DatetimeModified = DateTime.Now;

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        hs = DataContext.Update(receiptCurrent, x => x.Id == receiptCurrent.Id, false);
                        if (hs.Success)
                        {
                            // cấn trừ cho hóa đơn
                            hs = UpdateInvoiceOfPayment(receiptId);
                            //TODO: Tính lại công nợ trên hợp đồng (Tính bên ngoài Controller)
                            // Cập nhật Cus Advance của Agreement
                            HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptCurrent, SaveAction.SAVEDONE);

                            DataContext.SubmitChanges();
                            trans.Commit();
                        }
                        return hs;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                        return new HandleState((object)ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        public HandleState SaveCancel(Guid receiptId)
        {
            try
            {
                AcctReceipt receiptCurrent = DataContext.Get(x => x.Id == receiptId).FirstOrDefault();
                if (receiptCurrent == null) return new HandleState((object)"Not found receipt");

                if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_DRAFT)
                {
                    return new HandleState((object)"Trạng thái của phiếu thu không hợp lệ");
                }

                receiptCurrent.Status = AccountingConstants.RECEIPT_STATUS_CANCEL;
                receiptCurrent.GroupId = currentUser.GroupId;
                receiptCurrent.DepartmentId = currentUser.DepartmentId;
                receiptCurrent.OfficeId = currentUser.OfficeID;
                receiptCurrent.CompanyId = currentUser.CompanyID;

                receiptCurrent.UserModified = currentUser.UserID;
                receiptCurrent.DatetimeModified = DateTime.Now;

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Update(receiptCurrent, x => x.Id == receiptCurrent.Id, false);
                        if (hs.Success)
                        {
                            // Lấy ra ds payment của Receipt
                            List<AccAccountingPayment> paymentsReceipt = acctPaymentRepository.Get(x => x.ReceiptId == receiptCurrent.Id).ToList();
                            // Phát sinh những dòng payment âm cho DEBIT & OBH
                            List<AccAccountingPayment> paymentDeitOBH = paymentsReceipt.Where(x => (x.Type == "DEBIT" || x.Type == "OBH")).ToList();
                            HandleState hsAddPaymentNegative = AddPaymentsNegative(paymentDeitOBH, receiptCurrent);
                            // Cập nhật invoice cho những payment DEBIT, OBH
                            HandleState hsUpdateInvoiceOfPayment = UpdateInvoiceOfPaymentCancel(receiptCurrent.Id);
                            // Cập nhật Netoff cho CREDIT or SOA.
                            List<AccAccountingPayment> paymentCredit = paymentsReceipt.Where(x => (x.Type != "DEBIT" && x.Type != "OBH")).ToList();
                            if (paymentCredit.Count > 0)
                            {
                                foreach (var item in paymentCredit)
                                {
                                    if (item.Type == "CREDITNOTE")
                                    {
                                        UpdateNetOffCredit(item, true);
                                    }
                                    if (item.Type == "CREDITSOA")
                                    {
                                        UpdateNetOffSoa(item, true);
                                    }
                                }
                            }
                            // Cập nhật Cus Advance của Agreement
                            HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptCurrent, SaveAction.SAVECANCEL);

                            DataContext.SubmitChanges();
                            trans.Commit();
                        }
                        return hs;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                        return new HandleState((object)ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        public ProcessClearInvoiceModel ProcessReceiptInvoice(ProcessReceiptInvoice criteria)
        {
            var invoiceList = criteria.List.OrderByDescending(x => x.Type == "OBH").ToList();
            var paidVnd = criteria.PaidAmountVnd;
            var paidUsd = criteria.PaidAmountUsd;
            foreach (var invoice in invoiceList)
            {
                if (paidVnd > 0)
                {
                    if (invoice.PaidAmountVnd == 0)
                    {
                        invoice.PaidAmountVnd = invoice.TotalPaidVnd = invoice.UnpaidAmountVnd;
                    }
                    if (paidVnd < invoice.PaidAmountVnd)
                    {
                        invoice.PaidAmountVnd = invoice.TotalPaidVnd = paidVnd;
                    }
                    if (invoice.Type == AccountingConstants.PAYMENT_TYPE_CODE_ADVANCE)
                    {
                        paidVnd = paidVnd - (invoice.PaidAmountVnd ?? 0);
                    }
                    if (invoice.NetOffVnd != 0 && invoice.NetOffVnd != null && invoice.NetOffUsd != 0 && invoice.NetOffUsd != null)
                    {
                        paidVnd = paidVnd - (invoice.PaidAmountVnd ?? 0);
                    }
                    else
                    {
                        paidVnd = paidVnd - (invoice.TotalPaidVnd ?? 0);
                    }
                }
                if (paidUsd > 0)
                {
                    if (invoice.PaidAmountUsd == 0)
                    {
                        invoice.PaidAmountUsd = invoice.TotalPaidUsd = invoice.UnpaidAmountUsd;
                    }
                    if (paidUsd < invoice.PaidAmountUsd)
                    {
                        invoice.PaidAmountUsd = invoice.TotalPaidUsd = paidUsd;
                    }
                    if (invoice.Type == AccountingConstants.PAYMENT_TYPE_CODE_ADVANCE)
                    {
                        paidUsd = paidUsd - (invoice.PaidAmountUsd ?? 0);
                    }
                    if (invoice.NetOffVnd != 0 || invoice.NetOffVnd != null || invoice.NetOffUsd != 0 || invoice.NetOffUsd != null)
                    {
                        paidUsd = paidUsd - (invoice.PaidAmountUsd ?? 0);
                    }
                    else
                    {
                        paidUsd = paidUsd - (invoice.TotalPaidUsd ?? 0);
                    }
                }
            }
            return new ProcessClearInvoiceModel
            {
                Invoices = invoiceList,
                CusAdvanceAmountVnd = NumberHelper.RoundNumber(paidVnd, 0),
                CusAdvanceAmountUsd = NumberHelper.RoundNumber(paidUsd, 2),
            };
        }

        public List<CustomerDebitCreditModel> GetDataIssueCustomerPayment(CustomerDebitCreditCriteria criteria)
        {
            List<CustomerDebitCreditModel> data = new List<CustomerDebitCreditModel>();
            IQueryable<CustomerDebitCreditModel> creditNotes = null;
            IQueryable<CustomerDebitCreditModel> debits = null;
            IQueryable<CustomerDebitCreditModel> obhs = null;
            IQueryable<CustomerDebitCreditModel> soaCredits = null;
            if (criteria.SearchType.Equals("Credit Note"))
            {
                creditNotes = GetCreditNoteForIssueCustomerPayment(criteria);
            }
            else
            {
                debits = GetDebitForIssueCustomerPayment(criteria);
                obhs = GetObhForIssueCustomerPayment(criteria);
                //soaCredits = GetSoaCreditForIssueCustomerPayment(criteria);
                //creditNotes = GetCreditNoteForIssueCustomerPayment(criteria);
            }

            if (debits != null && debits.Count() > 0)
            {
                data.AddRange(debits);
            }
            if (obhs != null && obhs.Count() > 0)
            {
                data.AddRange(obhs);
            }
            if (soaCredits != null && soaCredits.Count() > 0)
            {
                data.AddRange(soaCredits);
            }
            if (creditNotes != null && creditNotes.Count() > 0)
            {
                data.AddRange(creditNotes);
            }

            if (data.Count == 0)
            {
                return data;
            }
            foreach (var item in data)
            {
                item.DepartmentName = GetDepartmentName(item.RefNo);
            }
            return data;
        }
        public AgencyDebitCreditDetailModel GetDataIssueAgencyPayment(CustomerDebitCreditCriteria criteria)
        {
            AgencyDebitCreditDetailModel data = new AgencyDebitCreditDetailModel();
            data.Invoices = new List<AgencyDebitCreditModel>();

            IQueryable<AgencyDebitCreditModel> creditNote = null;
            IQueryable<AgencyDebitCreditModel> debits = null;
            IQueryable<AgencyDebitCreditModel> obhs = null;
            IQueryable<AgencyDebitCreditModel> soaCredit = null;
            switch (criteria.SearchType)
            {
                case "Credit Note":
                    creditNote = GetCreditNoteForIssueAgencyPayment(criteria);
                    break;
                case "SOA":
                    soaCredit = GetSoaCreditForIssueAgentPayment(criteria);
                    break;
                default:
                    debits = GetDebitForIssueAgentPayment(criteria);
                    obhs = GetObhForIssueAgencyPayment(criteria);
                    soaCredit = GetSoaCreditForIssueAgentPayment(criteria);
                    creditNote = GetCreditNoteForIssueAgencyPayment(criteria);
                    break;
            }

            if (creditNote != null && creditNote.Count() > 0)
            {
                data.Invoices.AddRange(creditNote);
            }
            if (soaCredit != null && soaCredit.Count() > 0)
            {
                data.Invoices.AddRange(soaCredit);
            }
            if (debits != null && debits.Count() > 0)
            {
                data.Invoices.AddRange(debits);
            }
            if (obhs != null && obhs.Count() > 0)
            {
                data.Invoices.AddRange(obhs);
            }
            if (data.Invoices.Count == 0)
            {
                data.GroupShipmentsAgency = new List<GroupShimentAgencyModel>();
                return data;
            }
            var groupShipmentAgency = data.Invoices.GroupBy(g => new { g.JobNo, g.Hbl, g.Mbl, g.Hblid }).Select(s => new GroupShimentAgencyModel
            {
                Hblid = s.Key.Hblid,
                JobNo = s.Key.JobNo,
                Mbl = s.Key.Mbl,
                Hbl = s.Key.Hbl,
                UnpaidAmountUsd = s.Select(t => t.UnpaidAmountUsd).Sum(),
                UnpaidAmountVnd = s.Select(t => t.UnpaidAmountVnd).Sum(),
                Invoices = s.ToList()
            }).ToList();

            data.GroupShipmentsAgency = groupShipmentAgency;
            return data;
        }

        private IQueryable<AgencyDebitCreditModel> GetCreditNoteForIssueAgencyPayment(CustomerDebitCreditCriteria criteria)
        {
            var expQueryCreditAR = CreditARExpressionQuery(criteria);
            var creditNotes = creditMngtArRepository.Get(expQueryCreditAR);

            var data = MappingAgencyDebitCreditModel(creditNotes, criteria);

            return data;
        }

        private IQueryable<AgencyDebitCreditModel> GetSoaCreditForIssueAgentPayment(CustomerDebitCreditCriteria criteria)
        {
            var expQueryCreditAR = CreditARExpressionQuery(criteria);
            var creditNotes = creditMngtArRepository.Get(expQueryCreditAR);

            var data = MappingAgencyDebitCreditModel(creditNotes, criteria, "soa");

            return data;
        }

        private IQueryable<AgencyDebitCreditModel> MappingAgencyDebitCreditModel(IEnumerable<AcctCreditManagementAr> creditNotes, CustomerDebitCreditCriteria criteria, string type = "credit")
        {
            var surcharges = surchargeRepository.Get();
            var partners = catPartnerRepository.Get();
            var departments = departmentRepository.Get();
            var offices = officeRepository.Get();

            var query = from credit in creditNotes
                        join sur in surcharges on credit.Code equals type == "credit" ? sur.CreditNo : sur.PaySoano
                        select new { credit, sur };

            if (criteria.ReferenceNos.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "HBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Hblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "MBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Mblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Job No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.JobNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Customs No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.ClearanceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }
            var grpCreditNoteCharge = query.GroupBy(g => new { g.sur.JobNo, g.sur.Hblno, g.sur.Mblno, g.credit, g.sur.Hblid })
                .Select(s => new { Job = s.Key, Surcharge = s.Select(se => se.sur) });

            var data = grpCreditNoteCharge.Select(se => new AgencyDebitCreditModel
            {
                RefNo = se.Job.credit.Code,
                Type = se.Job.credit.Type,
                InvoiceNo = null,
                InvoiceDate = null,
                PartnerId = se.Job.credit.PartnerId,
                CurrencyId = se.Job.credit.Currency,
                Amount = se.Job.credit.Currency != AccountingConstants.CURRENCY_LOCAL ? se.Job.credit.AmountUsd : se.Job.credit.AmountVnd,
                UnpaidAmount = se.Job.credit.Currency == AccountingConstants.CURRENCY_LOCAL ? se.Job.credit.RemainVnd : se.Job.credit.RemainUsd,
                UnpaidAmountVnd = se.Job.credit.RemainVnd,
                UnpaidAmountUsd = se.Job.credit.RemainUsd,
                PaymentTerm = null,
                DueDate = null,
                PaymentStatus = null,
                DepartmentId = se.Job.credit.DepartmentId,
                OfficeId = Guid.Parse(se.Job.credit.OfficeId),
                CompanyId = se.Job.credit.CompanyId,
                RefIds = new List<string> { se.Job.credit.Id.ToString() },
                JobNo = se.Job.JobNo,
                Mbl = se.Job.Mblno,
                Hbl = se.Job.Hblno,
                Hblid = se.Job.Hblid,
                VoucherId = se.Surcharge.FirstOrDefault().VoucherId,
                VoucherIdre = se.Surcharge.FirstOrDefault().VoucherIdre,
                ExchangeRateBilling = se.Job.credit.ExchangeRateUsdToLocal
            });

            var joinData = from inv in data
                           join par in partners on inv.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
                           join dept in departments on inv.DepartmentId equals dept.Id into deptGrp
                           from dept in deptGrp.DefaultIfEmpty()
                           join ofi in offices on inv.OfficeId equals ofi.Id into ofiGrp
                           from ofi in ofiGrp.DefaultIfEmpty()
                           select new AgencyDebitCreditModel
                           {
                               RefNo = inv.RefNo,
                               InvoiceNo = inv.InvoiceNo,
                               InvoiceDate = inv.InvoiceDate,
                               PartnerId = inv.PartnerId,
                               PartnerName = par.ShortName,
                               TaxCode = par.TaxCode,
                               CurrencyId = inv.CurrencyId,
                               Amount = inv.Amount,
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmountUsd = inv.UnpaidAmountUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               DepartmentName = dept != null ? dept.DeptNameAbbr : null,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId,
                               RefIds = inv.RefIds,
                               JobNo = inv.JobNo,
                               Mbl = inv.Mbl,
                               Hbl = inv.Hbl,
                               Type = inv.Type,
                               PaymentType = "CREDIT",
                               Hblid = inv.Hblid,
                               ExchangeRateBilling = inv.ExchangeRateBilling
                           };
            return joinData.AsQueryable();
        }

        private IQueryable<AgencyDebitCreditModel> GetDebitForIssueAgentPayment(CustomerDebitCreditCriteria criteria)
        {
            var expQuery = InvoiceExpressionQuery(criteria, AccountingConstants.ACCOUNTING_INVOICE_TYPE);
            var invoices = acctMngtRepository.Get(expQuery);
            var surcharges = surchargeRepository.Get();
            var partners = catPartnerRepository.Get();
            var departments = departmentRepository.Get();
            var offices = officeRepository.Get();

            var query = from inv in invoices
                        join sur in surcharges on inv.Id equals sur.AcctManagementId
                        select new { inv, sur };
            if (criteria.ReferenceNos.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "HBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Hblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "MBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Mblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Job No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.JobNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Customs No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.ClearanceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }

            var grpInvoiceCharge = query.GroupBy(g => new { g.inv, g.sur.Hblno, g.sur.JobNo, g.sur.Mblno, g.sur.Hblid }).Select(s => new { Invoice = s.Key, Surcharge = s.Select(se => se.sur), Soa_DebitNo = s.Select(se => new { se.sur.Soano, se.sur.DebitNo }) });
            var data = grpInvoiceCharge.Select(se => new AgencyDebitCreditModel
            {
                RefNo = se.Soa_DebitNo.Any(w => !string.IsNullOrEmpty(w.Soano))
                ? se.Soa_DebitNo.Where(w => !string.IsNullOrEmpty(w.Soano)).Select(s => s.Soano).FirstOrDefault()
                : se.Soa_DebitNo.Where(w => !string.IsNullOrEmpty(w.DebitNo)).Select(s => s.DebitNo).FirstOrDefault(),
                Type = "DEBIT",
                InvoiceNo = se.Invoice.inv.InvoiceNoReal,
                InvoiceDate = se.Invoice.inv.Date,
                PartnerId = se.Invoice.inv.PartnerId,
                CurrencyId = se.Invoice.inv.Currency,
                Amount = se.Invoice.inv.TotalAmount,
                UnpaidAmount = se.Invoice.inv.UnpaidAmount,
                UnpaidAmountVnd = se.Invoice.inv.UnpaidAmountVnd,
                UnpaidAmountUsd = se.Invoice.inv.UnpaidAmountUsd,
                PaymentTerm = se.Invoice.inv.PaymentTerm,
                DueDate = se.Invoice.inv.PaymentDueDate,
                PaymentStatus = se.Invoice.inv.PaymentStatus,
                DepartmentId = se.Invoice.inv.DepartmentId,
                OfficeId = se.Invoice.inv.OfficeId,
                CompanyId = se.Invoice.inv.CompanyId,
                RefIds = new List<string> { se.Invoice.inv.Id.ToString() },
                JobNo = se.Invoice.JobNo,
                Mbl = se.Invoice.Mblno,
                Hbl = se.Invoice.Hblno,
                Hblid = se.Invoice.Hblid,
                ExchangeRateBilling = GetExchangeRateDebitBilling(se.Soa_DebitNo)
            });
            var joinData = from inv in data
                           join par in partners on inv.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
                           join dept in departments on inv.DepartmentId equals dept.Id into deptGrp
                           from dept in deptGrp.DefaultIfEmpty()
                           join ofi in offices on inv.OfficeId equals ofi.Id into ofiGrp
                           from ofi in ofiGrp.DefaultIfEmpty()
                           select new AgencyDebitCreditModel
                           {
                               RefNo = inv.RefNo,
                               Type = inv.Type,
                               InvoiceNo = inv.InvoiceNo,
                               InvoiceDate = inv.InvoiceDate,
                               PartnerId = inv.PartnerId,
                               PartnerName = par.ShortName,
                               TaxCode = par.TaxCode,
                               CurrencyId = inv.CurrencyId,
                               Amount = inv.Amount,
                               //PaidAmountUsd = inv.UnpaidAmountUsd,
                               //PaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmountUsd = inv.UnpaidAmountUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               DepartmentName = dept != null ? dept.DeptNameAbbr : null,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId,
                               RefIds = inv.RefIds,
                               Mbl = inv.Mbl,
                               Hbl = inv.Hbl,
                               JobNo = inv.JobNo,
                               Hblid = inv.Hblid,
                               ExchangeRateBilling = inv.ExchangeRateBilling,
                               PaymentType = inv.Type
                           };
            return joinData;
        }

        private IQueryable<AgencyDebitCreditModel> GetObhForIssueAgencyPayment(CustomerDebitCreditCriteria criteria)
        {
            var expQuery = InvoiceExpressionQuery(criteria, AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);
            var invoiceTemps = acctMngtRepository.Get(expQuery);
            var surcharges = surchargeRepository.Get();
            var partners = catPartnerRepository.Get();
            var departments = departmentRepository.Get();
            var offices = officeRepository.Get();

            var query = from inv in invoiceTemps
                        join sur in surcharges on inv.Id equals sur.AcctManagementId
                        select new { inv, sur };

            if (criteria.ReferenceNos.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "HBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Hblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "MBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Mblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Job No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.JobNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Customs No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.ClearanceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }
            var grpInvoiceCharge = query.GroupBy(g => new { g.inv.PartnerId, RefNo = (g.sur.SyncedFrom == "CDNOTE" ? g.sur.DebitNo : (g.sur.SyncedFrom == "SOA" ? g.sur.Soano : null)), g.sur.JobNo, g.sur.Mblno, g.sur.Hblno, g.sur.Hblid })
                .Select(s => new { s.Key.PartnerId, s.Key.RefNo, Invoice = s.Select(se => se.inv), Surcharge = s.Select(se => se.sur), Job = s.Key });
            var data = grpInvoiceCharge.Select(se => new AgencyDebitCreditModel
            {
                RefNo = se.RefNo,
                Type = "OBH",
                InvoiceNo = string.Empty,
                InvoiceDate = se.Invoice.Select(s => s.Date).FirstOrDefault(),
                PartnerId = se.PartnerId,
                CurrencyId = se.Invoice.Select(s => s.Currency).FirstOrDefault(),
                Amount = se.Invoice.Sum(su => su.TotalAmount),
                UnpaidAmount = se.Invoice.FirstOrDefault().UnpaidAmount,
                UnpaidAmountVnd = se.Invoice.FirstOrDefault().UnpaidAmountVnd,
                UnpaidAmountUsd = se.Invoice.FirstOrDefault().UnpaidAmountUsd,
                PaymentTerm = se.Invoice.Select(s => s.PaymentTerm).FirstOrDefault(),
                DueDate = se.Invoice.FirstOrDefault().PaymentDueDate,
                PaymentStatus = se.Invoice.Select(s => s.PaymentStatus).FirstOrDefault(),
                DepartmentId = se.Invoice.Select(s => s.DepartmentId).FirstOrDefault(),
                OfficeId = se.Invoice.Select(s => s.OfficeId).FirstOrDefault(),
                CompanyId = se.Invoice.Select(s => s.CompanyId).FirstOrDefault(),
                RefIds = se.Invoice.Select(s => s.Id.ToString()).Distinct().ToList(),
                JobNo = se.Job.JobNo,
                Mbl = se.Job.Mblno,
                Hbl = se.Job.Hblno,
                Hblid = se.Job.Hblid,
                ExchangeRateBilling = GetExchangeRateDebitOBHBilling(se.RefNo)
            });
            var joinData = from inv in data
                           join par in partners on inv.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
                           join dept in departments on inv.DepartmentId equals dept.Id into deptGrp
                           from dept in deptGrp.DefaultIfEmpty()
                           join ofi in offices on inv.OfficeId equals ofi.Id into ofiGrp
                           from ofi in ofiGrp.DefaultIfEmpty()
                           select new AgencyDebitCreditModel
                           {
                               RefNo = inv.RefNo,
                               Type = inv.Type,
                               InvoiceNo = inv.InvoiceNo,
                               InvoiceDate = inv.InvoiceDate,
                               PartnerId = inv.PartnerId,
                               PartnerName = par.ShortName,
                               TaxCode = par.TaxCode,
                               CurrencyId = inv.CurrencyId,
                               Amount = inv.Amount,
                               //PaidAmountUsd = inv.UnpaidAmountUsd,
                               //PaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmountUsd = inv.UnpaidAmountUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               DepartmentName = dept != null ? dept.DeptNameAbbr : null,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId,
                               RefIds = inv.RefIds,
                               JobNo = inv.JobNo,
                               Hbl = inv.Hbl,
                               Mbl = inv.Mbl,
                               Hblid = inv.Hblid,
                               ExchangeRateBilling = inv.ExchangeRateBilling,
                               PaymentType = inv.Type
                           };
            return joinData;
        }

        private Expression<Func<AccAccountingManagement, bool>> InvoiceExpressionQuery(CustomerDebitCreditCriteria criteria, string type)
        {
            //Get Vat Invoice có Payment Status # Paid
            Expression<Func<AccAccountingManagement, bool>> query = q => q.Type == type && q.PaymentStatus != "Paid";
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                // Ds các đối tượng con
                List<string> childPartnerIds = catPartnerRepository.Get(x => x.ParentId == criteria.PartnerId)
                        .Select(x => x.Id)
                        .ToList();
                query = query.And(q => q.PartnerId == criteria.PartnerId || childPartnerIds.Contains(q.PartnerId));
            }

            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                var acctManagementIds = new List<Guid?>();
                if (criteria.SearchType.Equals("SOA"))
                {
                    acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Soano, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("Debit/Credit/Invoice"))
                {
                    acctManagementIds = surchargeRepository.Get(x => (criteria.ReferenceNos.Contains(x.DebitNo, StringComparer.OrdinalIgnoreCase))
                    || (criteria.ReferenceNos.Contains(x.CreditNo, StringComparer.OrdinalIgnoreCase)
                    || (criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase))
                    )).Select(se => se.AcctManagementId).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("VAT Invoice"))
                {
                    query = query.And(x => criteria.ReferenceNos.Contains(x.InvoiceNoReal));
                }
                else if (criteria.SearchType.Equals("JOB NO"))
                {
                    acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("HBL"))
                {
                    acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("MBL"))
                {
                    acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("Customs No"))
                {
                    acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.ClearanceNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                }

                if (acctManagementIds != null && acctManagementIds.Count > 0)
                {
                    query = query.And(x => acctManagementIds.Contains(x.Id));
                }
            }

            if (criteria.FromDate != null && criteria.ToDate != null)
            {
                if (!string.IsNullOrEmpty(criteria.DateType))
                {
                    if (criteria.DateType == "Invoice Date")
                    {
                        query = query.And(x => x.Date.Value.Date >= criteria.FromDate.Value.Date && x.Date.Value.Date <= criteria.ToDate.Value.Date);
                    }
                    else if (criteria.DateType == "Billing Date")
                    {
                        query = query.And(x => x.ConfirmBillingDate.Value.Date >= criteria.FromDate.Value.Date && x.ConfirmBillingDate.Value.Date <= criteria.ToDate.Value.Date);
                    }
                    else if (criteria.DateType == "Service Date")
                    {
                        IQueryable<OpsTransaction> operations = null;
                        IQueryable<CsTransaction> transactions = null;
                        if (!string.IsNullOrEmpty(criteria.Service))
                        {
                            if (criteria.Service.Contains("CL"))
                            {
                                operations = opsTransactionRepository.Get(x => x.CurrentStatus != TermData.Canceled && (x.ServiceDate.HasValue ? x.ServiceDate.Value.Date >= criteria.FromDate.Value.Date && x.ServiceDate.Value.Date <= criteria.ToDate.Value.Date : false));
                            }
                            if (criteria.Service.Contains("I") || criteria.Service.Contains("A"))
                            {
                                transactions = csTransactionRepository.Get(x => x.CurrentStatus != TermData.Canceled && (x.ServiceDate.HasValue ? (criteria.FromDate.Value.Date <= x.ServiceDate.Value.Date &&
                                                                                                                    x.ServiceDate.Value.Date <= criteria.ToDate.Value.Date) : false));
                            }
                        }

                        var dateModeJobNos = new List<string>();
                        if (operations != null)
                        {
                            dateModeJobNos = operations.Select(s => s.JobNo).ToList();
                        }
                        if (transactions != null)
                        {
                            dateModeJobNos.AddRange(transactions.Select(s => s.JobNo).ToList());
                        }
                        if (dateModeJobNos.Count > 0)
                        {
                            var acctManagementIds = surchargeRepository.Where(x => dateModeJobNos.Where(w => w == x.JobNo).Any()).Select(se => se.AcctManagementId).Distinct().ToList();
                            if (acctManagementIds != null && acctManagementIds.Count > 0)
                            {
                                query = query.And(x => acctManagementIds.Contains(x.Id));
                            }
                        }
                        else
                        {
                            query = query.And(x => false);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(criteria.Service))
            {
                var acctManagementIds = surchargeRepository.Get(x => criteria.Service.Contains(x.TransactionType)).Select(se => se.AcctManagementId).Distinct().ToList();
                if (acctManagementIds != null && acctManagementIds.Count > 0)
                {
                    query = query.And(x => acctManagementIds.Contains(x.Id));
                }
            }

            return query;
        }

        private Expression<Func<AcctCreditManagementAr, bool>> CreditARExpressionQuery(CustomerDebitCreditCriteria criteria)
        {

            Expression<Func<AcctCreditManagementAr, bool>> expQueryCreditAR = q => q.NetOff == false;

            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                expQueryCreditAR = expQueryCreditAR.And(q => q.PartnerId == criteria.PartnerId);
            }

            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
                switch (criteria.SearchType)
                {
                    case "Credit Note":
                    case "SOA":
                    case "Debit/Credit/Invoice":
                        expQueryCreditAR = expQueryCreditAR.And(x => criteria.ReferenceNos.Contains(x.Code));
                        break;
                    default:
                        break;
                }

            //if (criteria.FromDate != null && criteria.ToDate != null)
            //{
            //    if (!string.IsNullOrEmpty(criteria.DateType))
            //    {
            //        if (criteria.DateType == "Invoice Date")
            //        {
            //            /*var soaNos = surchargeRepository.Get(x => x.InvoiceDate.Value.Date >= criteria.FromDate.Value.Date && x.InvoiceDate.Value.Date <= criteria.ToDate.Value.Date).Select(se => se.PaySoano).Distinct().ToList();
            //            if (soaNos != null)
            //            {
            //                query = query.And(x => soaNos.Contains(x.Soano));
            //            }*/
            //            query = query.And(x => false); //**Phí Credit nên ko có Invoice Date
            //        }
            //        else if (criteria.DateType == "Billing Date")
            //        {
            //            /*List<Guid> invoiceIds = acctMngtRepository.Get(x => x.ConfirmBillingDate.Value.Date >= criteria.FromDate.Value.Date && x.ConfirmBillingDate.Value.Date <= criteria.ToDate.Value.Date).Select(se => se.Id).Distinct().ToList();
            //            var soaNos = surchargeRepository.Get(x => invoiceIds.Contains(x.AcctManagementId.Value)).Select(se => se.PaySoano).Distinct().ToList();
            //            if (soaNos != null)
            //            {
            //                query = query.And(x => soaNos.Contains(x.Soano));
            //            }*/
            //            query = query.And(x => false); //**Phí Credit nên ko có Billing Date
            //        }
            //        else if (criteria.DateType == "Service Date")
            //        {
            //            IQueryable<OpsTransaction> operations = null;
            //            IQueryable<CsTransaction> transactions = null;
            //            if (!string.IsNullOrEmpty(criteria.Service))
            //            {
            //                if (criteria.Service.Contains("CL"))
            //                {
            //                    operations = opsTransactionRepository.Get(x => x.CurrentStatus != TermData.Canceled && (x.ServiceDate.HasValue ? x.ServiceDate.Value.Date >= criteria.FromDate.Value.Date && x.ServiceDate.Value.Date <= criteria.ToDate.Value.Date : false));
            //                }
            //                if (criteria.Service.Contains("I") || criteria.Service.Contains("A"))
            //                {
            //                    transactions = csTransactionRepository.Get(x => x.CurrentStatus != TermData.Canceled && (x.ServiceDate.HasValue ? (criteria.FromDate.Value.Date <= x.ServiceDate.Value.Date &&
            //                                                                                                        x.ServiceDate.Value.Date <= criteria.ToDate.Value.Date) : false));
            //                }
            //            }

            //            var dateModeJobNos = new List<string>();
            //            if (operations != null)
            //            {
            //                dateModeJobNos = operations.Select(s => s.JobNo).ToList();
            //            }
            //            if (transactions != null)
            //            {
            //                dateModeJobNos.AddRange(transactions.Select(s => s.JobNo).ToList());
            //            }
            //            if (dateModeJobNos.Count > 0)
            //            {
            //                var soaNos = surchargeRepository.Where(x => dateModeJobNos.Where(w => w == x.JobNo).Any()).Select(se => se.PaySoano).Distinct().ToList();
            //                if (soaNos != null && soaNos.Count > 0)
            //                {
            //                    query = query.And(x => soaNos.Contains(x.Soano));
            //                }
            //            }
            //            else
            //            {
            //                query = query.And(x => false);
            //            }
            //        }
            //    }
            //}

            //if (!string.IsNullOrEmpty(criteria.Service))
            //{
            //    var soaNos = surchargeRepository.Get(x => criteria.Service.Contains(x.TransactionType)).Select(se => se.PaySoano).Distinct().ToList();
            //    if (soaNos != null && soaNos.Count > 0)
            //    {
            //        query = query.And(x => soaNos.Contains(x.Soano));
            //    }
            //}

            return expQueryCreditAR;
        }

        private Expression<Func<AcctCdnote, bool>> CreditNoteExpressionQuery(CustomerDebitCreditCriteria criteria)
        {
            //Get CDNote: Type = CREDIT & NetOff = false
            Expression<Func<AcctCdnote, bool>> query = q => q.Type == "CREDIT" && q.NetOff == false;
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(q => q.PartnerId == criteria.PartnerId);
            }

            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                var creditNo = new List<string>();
                if (criteria.SearchType.Equals("SOA"))
                {
                    creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.PaySoano, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("Credit Note") || criteria.SearchType.Equals("Debit/Credit/Invoice"))
                {
                    query = query.And(x => criteria.ReferenceNos.Contains(x.Code));
                }
                else if (criteria.SearchType.Equals("VAT Invoice"))
                {
                    creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("JOB NO"))
                {
                    creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("HBL"))
                {
                    creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("MBL"))
                {
                    creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("Customs No"))
                {
                    creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.ClearanceNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                }

                if (creditNo != null && creditNo.Count > 0)
                {
                    query = query.And(x => creditNo.Contains(x.Code));
                }
            }

            if (criteria.FromDate != null && criteria.ToDate != null)
            {
                if (!string.IsNullOrEmpty(criteria.DateType))
                {
                    if (criteria.DateType == "Invoice Date")
                    {
                        /*var creditNos = surchargeRepository.Get(x => x.InvoiceDate.Value.Date >= criteria.FromDate.Value.Date && x.InvoiceDate.Value.Date <= criteria.ToDate.Value.Date).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNos != null)
                        {
                            query = query.And(x => creditNos.Contains(x.Code));
                        }*/
                        query = query.And(x => false);
                    }
                    else if (criteria.DateType == "Billing Date")
                    {
                        /*List<Guid> invoiceIds = acctMngtRepository.Get(x => x.ConfirmBillingDate.Value.Date >= criteria.FromDate.Value.Date && x.ConfirmBillingDate.Value.Date <= criteria.ToDate.Value.Date).Select(se => se.Id).Distinct().ToList();
                        var creditNos = surchargeRepository.Get(x => invoiceIds.Contains(x.AcctManagementId.Value)).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNos != null)
                        {
                            query = query.And(x => creditNos.Contains(x.Code));
                        }*/
                        query = query.And(x => false);
                    }
                    else if (criteria.DateType == "Service Date")
                    {
                        IQueryable<OpsTransaction> operations = null;
                        IQueryable<CsTransaction> transactions = null;
                        if (!string.IsNullOrEmpty(criteria.Service))
                        {
                            if (criteria.Service.Contains("CL"))
                            {
                                operations = opsTransactionRepository.Get(x => x.CurrentStatus != TermData.Canceled && (x.ServiceDate.HasValue ? x.ServiceDate.Value.Date >= criteria.FromDate.Value.Date && x.ServiceDate.Value.Date <= criteria.ToDate.Value.Date : false));
                            }
                            if (criteria.Service.Contains("I") || criteria.Service.Contains("A"))
                            {
                                transactions = csTransactionRepository.Get(x => x.CurrentStatus != TermData.Canceled && (x.ServiceDate.HasValue ? (criteria.FromDate.Value.Date <= x.ServiceDate.Value.Date &&
                                                                                                                    x.ServiceDate.Value.Date <= criteria.ToDate.Value.Date) : false));
                            }
                        }

                        var dateModeJobNos = new List<string>();
                        if (operations != null)
                        {
                            dateModeJobNos = operations.Select(s => s.JobNo).ToList();
                        }
                        if (transactions != null)
                        {
                            dateModeJobNos.AddRange(transactions.Select(s => s.JobNo).ToList());
                        }
                        if (dateModeJobNos.Count > 0)
                        {
                            var creditNos = surchargeRepository.Where(x => dateModeJobNos.Where(w => w == x.JobNo).Any()).Select(se => se.CreditNo).Distinct().ToList();
                            if (creditNos != null && creditNos.Count > 0)
                            {
                                query = query.And(x => creditNos.Contains(x.Code));
                            }
                        }
                        else
                        {
                            query = query.And(x => false);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(criteria.Service))
            {
                var creditNos = surchargeRepository.Get(x => criteria.Service.Contains(x.TransactionType)).Select(se => se.CreditNo).Distinct().ToList();
                if (creditNos != null && creditNos.Count > 0)
                {
                    query = query.And(x => creditNos.Contains(x.Code));
                }
            }
            return query;
        }

        private IQueryable<CustomerDebitCreditModel> GetDebitForIssueCustomerPayment(CustomerDebitCreditCriteria criteria)
        {
            var expQuery = InvoiceExpressionQuery(criteria, AccountingConstants.ACCOUNTING_INVOICE_TYPE);
            var invoices = acctMngtRepository.Get(expQuery);
            var surcharges = surchargeRepository.Get();
            var partners = catPartnerRepository.Get();
            var departments = departmentRepository.Get();
            var offices = officeRepository.Get();

            var query = from inv in invoices
                        join sur in surcharges on inv.Id equals sur.AcctManagementId
                        select new { inv, sur };

            if (criteria.ReferenceNos.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "HBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Hblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "MBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Mblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Job No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.JobNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Customs No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.ClearanceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }

            var grpInvoiceCharge = query.GroupBy(g => g.inv).Select(s => new { Invoice = s.Key, Soa_DebitNo = s.Select(se => new { se.sur.Soano, se.sur.DebitNo }), Surcharge = s.Select(se => se.sur) });
            var data = grpInvoiceCharge.Select(se => new CustomerDebitCreditModel
            {
                RefNo = se.Soa_DebitNo.Any(w => !string.IsNullOrEmpty(w.Soano))
                ? se.Soa_DebitNo.Where(w => !string.IsNullOrEmpty(w.Soano)).Select(s => s.Soano).FirstOrDefault()
                : se.Soa_DebitNo.Where(w => !string.IsNullOrEmpty(w.DebitNo)).Select(s => s.DebitNo).FirstOrDefault(),
                Type = "DEBIT",
                InvoiceNo = se.Invoice.InvoiceNoReal,
                InvoiceDate = se.Invoice.Date,
                PartnerId = se.Invoice.PartnerId,
                CurrencyId = se.Invoice.Currency,
                Amount = se.Invoice.TotalAmount,
                UnpaidAmount = se.Invoice.UnpaidAmount,
                UnpaidAmountVnd = se.Invoice.UnpaidAmountVnd,
                UnpaidAmountUsd = se.Invoice.UnpaidAmountUsd,
                PaymentTerm = se.Invoice.PaymentTerm,
                DueDate = se.Invoice.PaymentDueDate,
                PaymentStatus = se.Invoice.PaymentStatus,
                DepartmentId = se.Invoice.DepartmentId,
                OfficeId = se.Invoice.OfficeId,
                CompanyId = se.Invoice.CompanyId,
                RefIds = new List<string> { se.Invoice.Id.ToString() },
                ExchangeRateBilling = GetExchangeRateDebitBilling(se.Soa_DebitNo)
            });
            var joinData = from inv in data
                           join par in partners on inv.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
                           join dept in departments on inv.DepartmentId equals dept.Id into deptGrp
                           from dept in deptGrp.DefaultIfEmpty()
                           join ofi in offices on inv.OfficeId equals ofi.Id into ofiGrp
                           from ofi in ofiGrp.DefaultIfEmpty()
                           select new CustomerDebitCreditModel
                           {
                               RefNo = inv.RefNo,
                               Type = inv.Type,
                               InvoiceNo = inv.InvoiceNo,
                               InvoiceDate = inv.InvoiceDate,
                               PartnerId = inv.PartnerId,
                               PartnerName = par.ShortName,
                               TaxCode = par.TaxCode,
                               CurrencyId = inv.CurrencyId,
                               Amount = inv.Amount,
                               //PaidAmountUsd = inv.UnpaidAmountUsd,
                               //PaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmountUsd = inv.UnpaidAmountUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId,
                               RefIds = inv.RefIds,
                               ExchangeRateBilling = inv.ExchangeRateBilling,
                               PaymentType = inv.Type
                           };

            return joinData;
        }

        private decimal? GetExchangeRateDebitBilling(IEnumerable<object> soa_debit)
        {
            decimal? exchangeRate = null;
            var data = soa_debit.ToList().First();
            string _soaNo = ObjectHelper.GetValueBy(data, "Soano");
            string _debitNo = ObjectHelper.GetValueBy(data, "DebitNo");
            if (!string.IsNullOrEmpty(_soaNo))
            {
                var soa = soaRepository.Get(x => x.Soano == _soaNo)?.FirstOrDefault();
                if (soa != null)
                {
                    exchangeRate = soa.ExcRateUsdToLocal;
                }
            }
            else if (!string.IsNullOrEmpty(_debitNo))
            {
                {
                    var debit = cdNoteRepository.Get(x => x.Code == _debitNo)?.FirstOrDefault();
                    if (debit != null)
                    {
                        exchangeRate = debit.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? debit.ExcRateUsdToLocal : debit.ExchangeRate;
                    }
                }
            }
            return exchangeRate;

        }

        private decimal? GetExchangeRateDebitOBHBilling(string refNo)
        {
            decimal? exchangeRate = null;

            var debit = cdNoteRepository.Get(x => x.Code == refNo)?.FirstOrDefault();
            if (debit != null)
            {
                exchangeRate = debit.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? debit.ExcRateUsdToLocal : debit.ExchangeRate;
            }
            else
            {
                var soa = soaRepository.Get(x => x.Soano == refNo)?.FirstOrDefault();
                if (soa != null)
                {
                    exchangeRate = soa.ExcRateUsdToLocal;
                }
            }
            return exchangeRate;
        }

        private IQueryable<CustomerDebitCreditModel> GetObhForIssueCustomerPayment(CustomerDebitCreditCriteria criteria)
        {
            var expQuery = InvoiceExpressionQuery(criteria, AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);
            var invoiceTemps = acctMngtRepository.Get(expQuery);
            var surcharges = surchargeRepository.Get();
            var partners = catPartnerRepository.Get();
            var departments = departmentRepository.Get();
            var offices = officeRepository.Get();

            var query = from inv in invoiceTemps
                        join sur in surcharges on inv.Id equals sur.AcctManagementId
                        select new { inv, sur };

            if (criteria.ReferenceNos.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "HBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Hblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "MBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Mblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Job No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.JobNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Customs No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.ClearanceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }

            var grpInvoiceCharge = query.GroupBy(g => new { g.inv.PartnerId, RefNo = (g.sur.SyncedFrom == "CDNOTE" ? g.sur.DebitNo : (g.sur.SyncedFrom == "SOA" ? g.sur.Soano : null)) })
                .Select(s => new { s.Key.PartnerId, s.Key.RefNo, Invoice = s.Select(se => se.inv), Surcharge = s.Select(x => x.sur) });
            var data = grpInvoiceCharge.Select(se => new CustomerDebitCreditModel
            {
                RefNo = se.RefNo,
                Type = "OBH",
                InvoiceNo = string.Empty,
                InvoiceDate = se.Invoice.Select(s => s.Date).FirstOrDefault(),
                PartnerId = se.PartnerId,
                CurrencyId = se.Invoice.Select(s => s.Currency).FirstOrDefault(),
                Amount = se.Invoice.Sum(su => su.TotalAmount),
                UnpaidAmount = se.Invoice.Sum(su => su.UnpaidAmount),
                UnpaidAmountVnd = se.Invoice.Sum(su => su.UnpaidAmountVnd),
                UnpaidAmountUsd = se.Invoice.Sum(su => su.UnpaidAmountUsd),
                PaymentTerm = se.Invoice.Select(s => s.PaymentTerm).FirstOrDefault(),
                DueDate = se.Invoice.FirstOrDefault().PaymentDueDate,
                PaymentStatus = se.Invoice.Select(s => s.PaymentStatus).FirstOrDefault(),
                DepartmentId = se.Invoice.Select(s => s.DepartmentId).FirstOrDefault(),
                OfficeId = se.Invoice.Select(s => s.OfficeId).FirstOrDefault(),
                CompanyId = se.Invoice.Select(s => s.CompanyId).FirstOrDefault(),
                RefIds = se.Invoice.Select(s => s.Id.ToString()).Distinct().ToList(),
                ExchangeRateBilling = GetExchangeRateDebitOBHBilling(se.RefNo)

            });
            var joinData = from inv in data
                           join par in partners on inv.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
                           join dept in departments on inv.DepartmentId equals dept.Id into deptGrp
                           from dept in deptGrp.DefaultIfEmpty()
                           join ofi in offices on inv.OfficeId equals ofi.Id into ofiGrp
                           from ofi in ofiGrp.DefaultIfEmpty()
                           select new CustomerDebitCreditModel
                           {
                               RefNo = inv.RefNo,
                               Type = inv.Type,
                               InvoiceNo = inv.InvoiceNo,
                               InvoiceDate = inv.InvoiceDate,
                               PartnerId = inv.PartnerId,
                               PartnerName = par.ShortName,
                               TaxCode = par.TaxCode,
                               CurrencyId = inv.CurrencyId,
                               Amount = inv.Amount,
                               //PaidAmountUsd = inv.UnpaidAmountUsd,
                               //PaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmountUsd = inv.UnpaidAmountUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId,
                               RefIds = inv.RefIds,
                               ExchangeRateBilling = inv.ExchangeRateBilling,
                               PaymentType = inv.Type
                           };

            return joinData;
        }

        private IQueryable<CustomerDebitCreditModel> GetSoaCreditForIssueCustomerPayment(CustomerDebitCreditCriteria criteria)
        {
            // var expQuery = SoaCreditExpressionQuery(criteria);
            // var soas = soaRepository.Get(expQuery);
            var soas = soaRepository.Get();

            var surcharges = surchargeRepository.Get();
            var partners = catPartnerRepository.Get();
            var departments = departmentRepository.Get();
            var offices = officeRepository.Get();

            var query = from soa in soas
                        join sur in surcharges on soa.Soano equals sur.PaySoano
                        select new { soa, sur };
            if (criteria.ReferenceNos.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "HBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Hblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "MBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Mblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Job No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.JobNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Customs No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.ClearanceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }
            var grpSoaCharge = query.GroupBy(g => g.soa).Select(s => new { Soa = s.Key, Surcharge = s.Select(se => se.sur) });
            var data = grpSoaCharge.Select(se => new CustomerDebitCreditModel
            {
                RefNo = se.Soa.Soano,
                InvoiceNo = null,
                InvoiceDate = null,
                PartnerId = se.Soa.Customer,
                CurrencyId = se.Soa.Currency,
                Amount = se.Soa.CreditAmount,
                UnpaidAmount = se.Soa.CreditAmount,
                UnpaidAmountVnd = se.Surcharge.Sum(su => su.AmountVnd + su.VatAmountVnd),
                UnpaidAmountUsd = se.Surcharge.Sum(su => su.AmountUsd + su.VatAmountUsd),
                PaymentTerm = null,
                DueDate = null,
                PaymentStatus = null,
                DepartmentId = se.Soa.DepartmentId,
                OfficeId = se.Soa.OfficeId,
                CompanyId = se.Soa.CompanyId,
                RefIds = new List<string> { se.Soa.Id },
                ExchangeRateBilling = se.Soa.ExcRateUsdToLocal
            });
            var joinData = from inv in data
                           join par in partners on inv.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
                           join dept in departments on inv.DepartmentId equals dept.Id into deptGrp
                           from dept in deptGrp.DefaultIfEmpty()
                           join ofi in offices on inv.OfficeId equals ofi.Id into ofiGrp
                           from ofi in ofiGrp.DefaultIfEmpty()
                           select new CustomerDebitCreditModel
                           {
                               RefNo = inv.RefNo,
                               InvoiceNo = inv.InvoiceNo,
                               InvoiceDate = inv.InvoiceDate,
                               PartnerId = inv.PartnerId,
                               PartnerName = par.ShortName,
                               TaxCode = par.TaxCode,
                               CurrencyId = inv.CurrencyId,
                               Amount = inv.Amount,
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmountUsd = inv.UnpaidAmountUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId,
                               RefIds = inv.RefIds,
                               ExchangeRateBilling = inv.ExchangeRateBilling,
                               Type = "CREDITSOA",
                               PaymentType = "CREDIT"

                           };

            return joinData;
        }

        private IQueryable<CustomerDebitCreditModel> GetCreditNoteForIssueCustomerPayment(CustomerDebitCreditCriteria criteria)
        {
            var expQuery = CreditNoteExpressionQuery(criteria);
            var creditNotes = cdNoteRepository.Get(expQuery);
            var surcharges = surchargeRepository.Get();
            var partners = catPartnerRepository.Get();
            var departments = departmentRepository.Get();
            var offices = officeRepository.Get();

            var query = from credit in creditNotes
                        join sur in surcharges on credit.Code equals sur.CreditNo
                        select new { credit, sur };

            if (criteria.ReferenceNos.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "HBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Hblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "MBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Mblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Job No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.JobNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Customs No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.ClearanceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }
            var grpCreditNoteCharge = query.GroupBy(g => g.credit).Select(s => new { CreditNote = s.Key, Surcharge = s.Select(se => se.sur) });
            var data = grpCreditNoteCharge.Select(se => new CustomerDebitCreditModel
            {
                RefNo = se.CreditNote.Code,
                InvoiceNo = null,
                InvoiceDate = null,
                PartnerId = se.CreditNote.PartnerId,
                CurrencyId = se.CreditNote.CurrencyId,
                Amount = se.CreditNote.Total,
                UnpaidAmount = se.CreditNote.Total,
                UnpaidAmountVnd = se.Surcharge.Sum(su => su.AmountVnd + su.VatAmountVnd),
                UnpaidAmountUsd = se.Surcharge.Sum(su => su.AmountUsd + su.VatAmountUsd),
                PaymentTerm = null,
                DueDate = null,
                PaymentStatus = null,
                DepartmentId = se.CreditNote.DepartmentId,
                OfficeId = se.CreditNote.OfficeId,
                CompanyId = se.CreditNote.CompanyId,
                RefIds = new List<string> { se.CreditNote.Id.ToString() },
                VoucherId = se.Surcharge.FirstOrDefault().VoucherId,
                VoucherIdre = se.Surcharge.FirstOrDefault().VoucherIdre,
                ExchangeRateBilling = se.CreditNote.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? se.CreditNote.ExcRateUsdToLocal : se.CreditNote.ExchangeRate
            });
            var joinData = from inv in data
                           join par in partners on inv.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
                           join dept in departments on inv.DepartmentId equals dept.Id into deptGrp
                           from dept in deptGrp.DefaultIfEmpty()
                           join ofi in offices on inv.OfficeId equals ofi.Id into ofiGrp
                           from ofi in ofiGrp.DefaultIfEmpty()
                           select new CustomerDebitCreditModel
                           {
                               RefNo = inv.RefNo,
                               InvoiceNo = inv.InvoiceNo,
                               InvoiceDate = inv.InvoiceDate,
                               PartnerId = inv.PartnerId,
                               PartnerName = par.ShortName,
                               TaxCode = par.TaxCode,
                               CurrencyId = inv.CurrencyId,
                               Amount = inv.Amount,
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmountUsd = inv.UnpaidAmountUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId,
                               RefIds = inv.RefIds,
                               Type = "CREDITNOTE",
                               PaymentType = "CREDIT",
                               VoucherId = inv.VoucherId,
                               VoucherIdre = inv.VoucherIdre,
                               ExchangeRateBilling = inv.ExchangeRateBilling,
                           };

            return joinData;
        }

        public async Task<HandleState> CalculatorReceivableForReceipt(Guid receiptId)
        {
            //Get list payment of Receipt
            var payments = acctPaymentRepository.Get(x => x.ReceiptId == receiptId);
            //Get list Invoice of payments
            var invoiceIds = payments.Where(x => x.Type == "DEBIT").Select(s => Guid.Parse(s.RefId)).Distinct().ToList();
            //Get list Invoice Temp of payments
            var invoiceTempIds = payments.Where(x => x.Type == "OBH").Select(s => Guid.Parse(s.RefId)).Distinct().ToList();
            //Get list Soa Credit of payments
            var soaIds = payments.Where(x => x.Type == "CREDITSOA").Select(s => s.RefId).Distinct().ToList();
            var paySoaNos = new List<string>();
            if (soaIds.Count > 0)
            {
                paySoaNos = soaRepository.Get(x => soaIds.Any(s => s == x.Id)).Select(s => s.Soano).Distinct().ToList();
            }
            IQueryable<CsShipmentSurcharge> surcharges = null;
            if (invoiceIds.Count > 0 || invoiceTempIds.Count > 0 || paySoaNos.Count > 0)
            {
                Expression<Func<CsShipmentSurcharge, bool>> query = chg => false;

                if (invoiceIds.Count > 0)
                {
                    query = query.Or(x => invoiceIds.Any(i => i == x.AcctManagementId));
                }
                if (invoiceTempIds.Count > 0)
                {
                    query = query.Or(x => invoiceTempIds.Any(t => t == x.PayerAcctManagementId));
                }
                if (paySoaNos.Count > 0)
                {
                    query = query.Or(x => paySoaNos.Any(p => p == x.PaySoano));
                }
                surcharges = surchargeRepository.Get(query);
            }
            var hs = new HandleState();
            if (surcharges == null) return hs;

            var objectReceivablesModel = accAccountReceivableService.GetObjectReceivableBySurcharges(surcharges);
            //Tính công nợ cho Partner, Service, Office có trong Receipt
            hs = await accAccountReceivableService.InsertOrUpdateReceivableAsync(objectReceivablesModel);
            return hs;
        }

        public bool CheckPaymentPaid(List<ReceiptInvoiceModel> Payments)
        {
            bool result = false;

            List<string> refIds = Payments.Select(x => x.RefIds).SelectMany(i => i).ToList();
            if (refIds.Count == 0)
            {
                return result;
            }
            result = acctMngtRepository.Any(x => refIds.Contains(x.Id.ToString()) && x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);

            return result;
        }

        private string GetDepartmentName(string refNo)
        {
            string dept = string.Empty;
            if (string.IsNullOrEmpty(refNo))
            {
                return dept;
            }
            int? deptId = 0;

            AcctCdnote cd = cdNoteRepository.Get(x => x.Code == refNo)?.FirstOrDefault();
            if (cd != null)
            {
                deptId = cd.DepartmentId;
            }
            else
            {
                AcctSoa soaDebitOBH = soaRepository.Get(x => x.Soano == refNo)?.FirstOrDefault();
                if (soaDebitOBH != null)
                {
                    deptId = soaDebitOBH?.DepartmentId;
                }
            }
            if (deptId != 0)
            {
                CatDepartment department = departmentRepository.Get(x => x.Id == deptId).FirstOrDefault();

                dept = department?.DeptNameAbbr;

            }
            return dept;
        }

        public void AlertReceiptToDeppartment(List<int> Ids, AcctReceiptModel receipt)
        {
            foreach (int Id in Ids)
            {
                CatDepartment dept = departmentRepository.Get(x => x.Id == Id)?.FirstOrDefault();
                if (dept == null)
                {
                    break;
                }
                List<string> toEmails = new List<string>();
                var emailSettingDept = emailSettingRepository.Get(x => x.DeptId == Id)?.FirstOrDefault();
                if (emailSettingDept == null)
                {
                    if (!string.IsNullOrEmpty(dept.Email))
                    {
                        toEmails = dept.Email.Split(";").ToList();
                    }
                }
                else if (emailSettingDept.EmailType == AccountingConstants.EMAIL_SETTING_AR_ALERT && !string.IsNullOrEmpty(emailSettingDept.EmailInfo))
                {
                    toEmails = emailSettingDept.EmailInfo.Split(";").ToList();
                }

                if (toEmails.Count > 0)
                {
                    SendMailAlertReceipt(toEmails, receipt);
                }
            }
        }

        private void SendMailAlertReceipt(List<string> toEmails, AcctReceiptModel receipt)
        {
            CatPartner partner = catPartnerRepository.Get(x => x.Id == receipt.CustomerId)?.FirstOrDefault();
            SysEmailTemplate emailTemplate = emailTemplaterepository.Get(x => x.Code == "AR-RECEIPT")?.FirstOrDefault();

            StringBuilder sb = new StringBuilder(emailTemplate.Subject);
            sb.Replace("{{receiptType}}", receipt.Class);
            sb.Replace("{{customerName}}", partner.PartnerNameEn);
            sb.Replace("{{customerCode}}", partner.TaxCode);

            StringBuilder bd = new StringBuilder(emailTemplate.Body);
            string paidAmountVnd = string.Format("{0:n0}", receipt.FinalPaidAmountVnd);
            string paidAmountUsd = string.Format("{0:n2}", receipt.FinalPaidAmountUsd);
            bd.Replace("{{receiptType}}", receipt.Class);
            bd.Replace("{{receiptNo}}", receipt.PaymentRefNo);
            bd.Replace("{{finalPaidAmountVnd}}", paidAmountVnd);
            bd.Replace("{{finalPaidAmountUSD}}", paidAmountUsd);
            bd.Replace("{{url}}", webUrl.Value.Url.ToString() + "/en/#/home/accounting/account-receivable-payable/receipt/" + receipt.Id.ToString());

            var sendMailResult = SendMail.Send(sb.ToString(), bd.ToString(), toEmails, null, null, null);
        }
    }
}
