using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.Receipt;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
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
        private readonly IContextBase<AcctSoa> soaRepository;
        private readonly IContextBase<AcctCdnote> cdNoteRepository;
        private readonly IContextBase<SysCompany> companyRepository;
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
            IContextBase<AcctSoa> soaRepo,
            IContextBase<AcctCdnote> cdNoteRepo,
            IContextBase<SysCompany> companyRepo
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
            soaRepository = soaRepo;
            cdNoteRepository = cdNoteRepo;
            companyRepository = companyRepo;
        }

        private IQueryable<AcctReceipt> GetQueryBy(AcctReceiptCriteria criteria)
        {
            Expression<Func<AcctReceipt, bool>> query = (x =>
            (x.CurrencyId ?? "").IndexOf(criteria.Currency ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            && (x.CustomerId ?? "").IndexOf(criteria.CustomerID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            );

            // Tìm theo status
            if (!string.IsNullOrEmpty(criteria.Status))
            {
                query = query.And(x => criteria.Status.Contains(x.Status));
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
            if (!string.IsNullOrEmpty(criteria.PaymentType) && criteria.PaymentType == "Invoice")
            {
                // Lấy ra thông tin hóa đơn
                IQueryable<AccAccountingManagement> invoices = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE
                && x.InvoiceNoReal.IndexOf(criteria.RefNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
                if (invoices != null && invoices.Count() > 0)
                {
                    List<Guid> receiptIds = null;
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

        private IQueryable<AcctReceiptModel> FormatReceipt(IQueryable<AcctReceipt> dataQuery)
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
            if (receipt == null)
            {
                return hs;
            }

            hs = DataContext.Delete(x => x.Id == receipt.Id);
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
            IQueryable<AcctReceiptModel> result = FormatReceipt(data);

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
            string ReceiptNo = "PT" + DateTime.Now.ToString("yyMM");

            IQueryable<string> codes = DataContext.Where(x => x.PaymentRefNo.Contains(ReceiptNo)).Select(x => x.PaymentRefNo);

            List<int> oders = new List<int>();

            if (codes != null & codes.Count() > 0)
            {
                foreach (string code in codes)
                {
                    if (code.Length > 9 && int.TryParse(code.Substring(code.Length - 4), out int _))
                    {
                        oders.Add(int.Parse(code.Substring(code.Length - 4)));
                    }
                }

                if (oders.Count() > 0)
                {
                    int maxCurrentOder = oders.Max();

                    ReceiptNo += (maxCurrentOder + 1).ToString("0000");
                }
                else
                {
                    ReceiptNo += "0001";
                }
            }
            else
            {
                ReceiptNo += "0001";
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

                        InvoiceId = x.invoice.Id.ToString(),
                        InvoiceNo = x.invoice.InvoiceNoReal,
                        Currency = x.invoice.Currency,
                        SerieNo = x.invoice.Serie,
                        InvoiceDate = x.invoice.Date,
                        UnpaidAmount = x.invoice.UnpaidAmount ?? 0,
                        Type = x.invoice.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE ? "DEBIT" : "OBH",
                        PaymentStatus = x.invoice.PaymentStatus,
                        PartnerName = x.grpPartner.ShortName,
                        TaxCode = x.grpPartner.TaxCode,
                        BillingDate = x.invoice.ConfirmBillingDate,

                    }).ToList();

                    int index = 1;
                    foreach (var item in results)
                    {
                        item.Index = index++;
                    }
                }
            }

            return results;
        }

        public AcctReceiptModel GetById(Guid id)
        {
            var receipt = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (receipt == null) return new AcctReceiptModel();
            var result = mapper.Map<AcctReceiptModel>(receipt);
            var paymentReceipts = new List<ReceiptInvoiceModel>();
            var acctPayments = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id).ToList().OrderBy(x => x.Type == "ADV");
            foreach (var acctPayment in acctPayments)
            {
                var invoice = acctMngtRepository.Get(x => x.Id.ToString() == acctPayment.RefId).FirstOrDefault();
                var partnerId = invoice?.PartnerId;
                var partner = catPartnerRepository.Get(x => x.Id == partnerId).FirstOrDefault();

                var payment = new ReceiptInvoiceModel();
             
                payment.PaymentId = acctPayment.Id;
                payment.InvoiceId = acctPayment.RefId;
                payment.InvoiceNo = acctPayment.BillingRefNo;
                payment.SerieNo = invoice?.Serie;
                payment.Type = acctPayment.Type;
                payment.PartnerName = partner?.ShortName;
                payment.TaxCode = partner?.TaxCode;
                payment.UnpaidAmount = invoice?.UnpaidAmount ?? 0;
                payment.Currency = acctPayment.CurrencyId;
                payment.PaidAmount = acctPayment.PaymentAmount;
                payment.InvoiceBalance = payment.UnpaidAmount - payment.PaidAmount;
                payment.RefAmount = acctPayment.RefAmount;
                payment.RefCurrency = acctPayment.RefCurrency;
                payment.PaymentStatus = invoice?.PaymentStatus;
                payment.BillingDate = invoice?.ConfirmBillingDate;
                payment.InvoiceDate = invoice?.Date;
                payment.Note = acctPayment.Note;
                payment.ReceiptExcUnpaidAmount = acctPayment.ReceiptExcUnpaidAmount;
                payment.ReceiptExcPaidAmount = acctPayment.ReceiptExcPaidAmount;
                payment.ReceiptExcInvoiceBalance = acctPayment.ReceiptExcBalance;
                paymentReceipts.Add(payment);
            }
            result.Payments = paymentReceipts;
            result.UserNameCreated = sysUserRepository.Where(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
            result.UserNameModified = sysUserRepository.Where(x => x.Id == result.UserModified).FirstOrDefault()?.Username;

            CatPartner partnerInfo = catPartnerRepository.Get(x => x.Id == result.CustomerId).FirstOrDefault();
            result.CustomerName = partnerInfo?.ShortName;
            result.TaxCode = partnerInfo?.TaxCode;

            return result;
        }

        public HandleState SaveReceipt(AcctReceiptModel receiptModel, SaveAction saveAction)
        {
            var hs = new HandleState();
            switch (saveAction)
            {
                case SaveAction.SAVEDRAFT_ADD:
                    hs = AddDraft(receiptModel);
                    break;
                case SaveAction.SAVEDRAFT_UPDATE:
                    hs = UpdateDraft(receiptModel);
                    break;
                case SaveAction.SAVEDONE:
                    hs = SaveDone(receiptModel);
                    break;
                case SaveAction.SAVECANCEL:
                    hs = SaveCancel(receiptModel);
                    break;
            }
            return hs;
        }

        private string GenerateAdvNo()
        {
            string advNo = "AD" + DateTime.Now.ToString("yyyy");
            string no = "0001";
            IQueryable<string> paymentNewests = acctPaymentRepository.Get(x => x.Type == "ADV" && x.BillingRefNo.Contains("AD") && x.BillingRefNo.Substring(2, 4) == DateTime.Now.ToString("yyyy"))
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

        private HandleState AddPayments(List<ReceiptInvoiceModel> payments, AcctReceipt receipt)
        {
            var hs = new HandleState();
            foreach (var payment in payments)
            {
                var _payment = new AccAccountingPayment();
                _payment.Id = Guid.NewGuid();
                _payment.ReceiptId = receipt.Id;
                _payment.BillingRefNo = payment.Type == "ADV" ? GenerateAdvNo() : payment.InvoiceNo;
                _payment.RefId = payment.InvoiceId;
                _payment.PaymentNo = payment.InvoiceNo + "_" + receipt.PaymentRefNo; //Invoice No + '_' + Receipt No

                //_payment.PaymentAmount = payment.PaidAmount;
                //_payment.Balance = payment.InvoiceBalance;
                _payment.PaymentAmount = payment.ReceiptExcPaidAmount;
                _payment.Balance = payment.ReceiptExcInvoiceBalance;
                _payment.ReceiptExcUnpaidAmount = payment.ReceiptExcUnpaidAmount;
                _payment.ReceiptExcPaidAmount = payment.ReceiptExcPaidAmount;
                _payment.ReceiptExcBalance = payment.ReceiptExcInvoiceBalance;

                _payment.CurrencyId = receipt.CurrencyId; //Currency Phiếu thu
                _payment.PaidDate = receipt.PaymentDate; //Payment Date Phiếu thu
                _payment.Type = payment.Type;
                _payment.ExchangeRate = receipt.ExchangeRate; //Exchange Rate Phiếu thu
                _payment.PaymentMethod = receipt.PaymentMethod; //Payment Method Phiếu thu
                _payment.RefAmount = payment.RefAmount;
                _payment.RefCurrency = payment.RefCurrency;
                _payment.Note = payment.Note;
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

        private HandleState UpdatePayments(List<ReceiptInvoiceModel> payments, AcctReceipt receipt)
        {
            var hsUpdate = new HandleState();
            foreach (var payment in payments)
            {
                var _payment = acctPaymentRepository.Get(x => x.Id == payment.PaymentId).FirstOrDefault();
                if (_payment != null)
                {
                    _payment.PaymentNo = payment.InvoiceNo + "_" + receipt.PaymentRefNo; //Invoice No + '_' + Receipt No

                    //_payment.PaymentAmount = payment.PaidAmount;
                    //_payment.Balance = payment.InvoiceBalance;

                    _payment.PaymentAmount = payment.ReceiptExcPaidAmount;
                    _payment.Balance = payment.ReceiptExcInvoiceBalance;
                    _payment.ReceiptExcUnpaidAmount = payment.ReceiptExcUnpaidAmount;
                    _payment.ReceiptExcPaidAmount = payment.ReceiptExcPaidAmount;
                    _payment.ReceiptExcBalance = payment.ReceiptExcInvoiceBalance;

                    _payment.CurrencyId = receipt.CurrencyId; //Currency Phiếu thu
                    _payment.PaidDate = receipt.PaymentDate; //Payment Date Phiếu thu
                    _payment.Type = payment.Type;
                    _payment.ExchangeRate = receipt.ExchangeRate; //Exchange Rate Phiếu thu
                    _payment.PaymentMethod = receipt.PaymentMethod; //Payment Method Phiếu thu
                    _payment.RefAmount = payment.RefAmount;
                    _payment.RefCurrency = payment.RefCurrency;
                    _payment.Note = payment.Note;
                    _payment.UserModified = currentUser.UserID;
                    _payment.DatetimeModified = DateTime.Now;
                    _payment.GroupId = currentUser.GroupId;
                    _payment.DepartmentId = currentUser.DepartmentId;
                    _payment.OfficeId = currentUser.OfficeID;
                    _payment.CompanyId = currentUser.CompanyID;

                    hsUpdate = acctPaymentRepository.Update(_payment, x => x.Id == _payment.Id);
                }
            }
            return hsUpdate;
        }

        private HandleState DeletePayments(List<Guid> ids)
        {
            var hsDelete = new HandleState();
            foreach (var id in ids)
            {
                hsDelete = acctPaymentRepository.Delete(x => x.Id == id);
            }
            return hsDelete;
        }

        private HandleState AddPaymentsNegative(List<AccAccountingPayment> payments, AcctReceipt receipt)
        {
            var hs = new HandleState();
            foreach (var payment in payments)
            {
                var invoice = acctMngtRepository.Get(x => x.Id.ToString() == payment.RefId).FirstOrDefault();
                var _payment = new AccAccountingPayment();
                _payment.Id = Guid.NewGuid();
                _payment.ReceiptId = receipt.Id;
                _payment.BillingRefNo = payment.Type == "ADV" ? GenerateAdvNo() : invoice.InvoiceNoReal;
                _payment.RefId = payment.RefId;
                _payment.PaymentNo = payment.BillingRefNo + "_" + receipt.PaymentRefNo;
                _payment.PaymentAmount = -payment.PaymentAmount; //Phát sinh payment amount âm
                _payment.Balance = invoice.UnpaidAmount - _payment.PaymentAmount; //Tính lại Balance
                _payment.CurrencyId = receipt.CurrencyId; //Currency Phiếu thu
                _payment.PaidDate = receipt.PaymentDate; //Payment Date Phiếu thu
                _payment.Type = payment.Type;
                _payment.ExchangeRate = receipt.ExchangeRate; //Exchange Rate Phiếu thu
                _payment.PaymentMethod = receipt.PaymentMethod; //Payment Method Phiếu thu
                _payment.RefAmount = payment.RefAmount;
                _payment.RefCurrency = payment.RefCurrency;
                _payment.Note = payment.Note;
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

        private HandleState UpdateInvoiceOfPayment(Guid receiptId)
        {
            var hsInvoiceUpdate = new HandleState();
            var payments = acctPaymentRepository.Get(x => x.ReceiptId == receiptId);
            foreach (var payment in payments)
            {
                var invoice = acctMngtRepository.Get(x => x.Id.ToString() == payment.RefId).FirstOrDefault();
                if (invoice != null)
                {
                    var totalAmountPaymentOfInvoice = acctPaymentRepository.Get(x => x.RefId == invoice.Id.ToString()).Select(s => s.PaymentAmount).Sum();
                    invoice.UnpaidAmount = invoice.TotalAmount - totalAmountPaymentOfInvoice; //Invoice Amount -  Total Payment Amount (Tất cả payment của Invoice)
                    invoice.PaidAmount = totalAmountPaymentOfInvoice; //Total Payment Amount (Tất cả  payment của từng VAT Invoice )
                    var _paymentStatus = invoice.PaymentStatus;
                    if (invoice.UnpaidAmount <= 0)
                    {
                        _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                    }
                    if (invoice.UnpaidAmount > 0 && invoice.UnpaidAmount < invoice.TotalAmount)
                    {
                        _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                    }
                    invoice.PaymentStatus = _paymentStatus;
                    invoice.UserModified = currentUser.UserID;
                    invoice.DatetimeModified = DateTime.Now;
                    hsInvoiceUpdate = acctMngtRepository.Update(invoice, x => x.Id == invoice.Id);
                }
            }
            return hsInvoiceUpdate;
        }

        private HandleState UpdateCusAdvanceOfAgreement(AcctReceiptModel receiptModel)
        {
            var hsAgreementUpdate = new HandleState();
            var totalAdv = receiptModel.Payments.Where(x => x.Type == "ADV").Select(s => s.PaidAmount).Sum();
            var receiptCusAdvance = receiptModel.CusAdvanceAmount;
            var agreement = catContractRepository.Get(x => x.Id == receiptModel.AgreementId).FirstOrDefault();
            if (agreement != null)
            {
                var _cusAdv = ((totalAdv - receiptCusAdvance) + agreement.CustomerAdvanceAmount) ?? 0;
                agreement.CustomerAdvanceAmount = _cusAdv < 0 ? 0 : _cusAdv;
                agreement.UserModified = currentUser.UserID;
                agreement.DatetimeModified = DateTime.Now;
                hsAgreementUpdate = catContractRepository.Update(agreement, x => x.Id == agreement.Id);
            }
            return hsAgreementUpdate;
        }

        public HandleState AddDraft(AcctReceiptModel receiptModel)
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

                AcctReceipt receipt = mapper.Map<AcctReceipt>(receiptModel);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Add(receipt, false);
                        if (hs.Success)
                        {
                            var hsPayment = AddPayments(receiptModel.Payments, receipt);
                            DataContext.SubmitChanges();
                            trans.Commit();
                        }
                        return hs;
                    }
                    catch (Exception ex)
                    {
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
                return new HandleState((object)ex.Message);
            }
        }

        public HandleState UpdateDraft(AcctReceiptModel receiptModel)
        {
            try
            {
                var receiptCurrent = DataContext.Get(x => x.Id == receiptModel.Id).FirstOrDefault();
                if (receiptCurrent == null) return new HandleState((object)"Not found receipt");
                if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_DONE) return new HandleState((object)"Not allow save draft. Receipt has been done");
                if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_CANCEL) return new HandleState((object)"Not allow save draft. Receipt has canceled");

                receiptModel.UserModified = currentUser.UserID;
                receiptModel.DatetimeModified = DateTime.Now;
                receiptModel.GroupId = currentUser.GroupId;
                receiptModel.DepartmentId = currentUser.DepartmentId;
                receiptModel.OfficeId = currentUser.OfficeID;
                receiptModel.CompanyId = currentUser.CompanyID;

                AcctReceipt receipt = mapper.Map<AcctReceipt>(receiptModel);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Update(receipt, x => x.Id == receipt.Id, false);
                        if (hs.Success)
                        {
                            var paymentsAdd = receiptModel.Payments.Where(x => x.PaymentId == Guid.Empty || x.PaymentId == null).ToList();
                            var paymentsUpdate = receiptModel.Payments.Where(x => x.PaymentId != Guid.Empty && x.PaymentId != null).ToList();
                            var paymentsDelete = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id && !paymentsUpdate.Select(se => se.PaymentId).Contains(x.Id)).Select(s => s.Id).ToList();
                            var hsPaymentAdd = AddPayments(paymentsAdd, receipt);
                            var hsPaymentUpdate = UpdatePayments(paymentsUpdate, receipt);
                            var hsPaymentDelete = DeletePayments(paymentsDelete);

                            DataContext.SubmitChanges();
                            trans.Commit();
                        }

                        return hs;
                    }
                    catch (Exception ex)
                    {
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
                return new HandleState((object)ex.Message);
            }
        }

        public HandleState SaveDone(AcctReceiptModel receiptModel)
        {
            try
            {
                var isAddNew = false;
                if (receiptModel.Id == Guid.Empty || receiptModel.Id == null)
                {
                    isAddNew = true;
                    receiptModel.Id = Guid.NewGuid();
                    receiptModel.UserCreated = receiptModel.UserModified = currentUser.UserID;
                    receiptModel.DatetimeCreated = receiptModel.DatetimeModified = DateTime.Now;
                }
                else
                {
                    isAddNew = false;
                    var receiptCurrent = DataContext.Get(x => x.Id == receiptModel.Id).FirstOrDefault();
                    if (receiptCurrent == null) return new HandleState((object)"Not found receipt");
                    if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_CANCEL) return new HandleState((object)"Not allow save done. Receipt has canceled");
                    receiptModel.UserModified = currentUser.UserID;
                    receiptModel.DatetimeModified = DateTime.Now;
                }
                receiptModel.Status = AccountingConstants.RECEIPT_STATUS_DONE;
                receiptModel.GroupId = currentUser.GroupId;
                receiptModel.DepartmentId = currentUser.DepartmentId;
                receiptModel.OfficeId = currentUser.OfficeID;
                receiptModel.CompanyId = currentUser.CompanyID;

                AcctReceipt receipt = mapper.Map<AcctReceipt>(receiptModel);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = isAddNew ? DataContext.Add(receipt, false) : DataContext.Update(receipt, x => x.Id == receipt.Id, false);
                        if (hs.Success)
                        {
                            var paymentsAdd = receiptModel.Payments.Where(x => x.PaymentId == Guid.Empty || x.PaymentId == null).ToList();
                            var hsPaymentAdd = AddPayments(paymentsAdd, receipt);
                            if (isAddNew == false)
                            {
                                var paymentsUpdate = receiptModel.Payments.Where(x => x.PaymentId != Guid.Empty && x.PaymentId != null).ToList();
                                var paymentsDelete = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id && !paymentsUpdate.Select(se => se.PaymentId).Contains(x.Id)).Select(s => s.Id).ToList();
                                var hsPaymentUpdate = UpdatePayments(paymentsUpdate, receipt);
                                var hsPaymentDelete = DeletePayments(paymentsDelete);
                            }
                            // Cập nhật invoice cho những payment
                            var hsUpdateInvoiceOfPayment = UpdateInvoiceOfPayment(receipt.Id);

                            DataContext.SubmitChanges();
                            trans.Commit();
                        }
                        return hs;
                    }
                    catch (Exception ex)
                    {
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
                return new HandleState((object)ex.Message);
            }
        }

        public HandleState SaveCancel(AcctReceiptModel receiptModel)
        {
            try
            {
                var receiptCurrent = DataContext.Get(x => x.Id == receiptModel.Id).FirstOrDefault();
                if (receiptCurrent == null) return new HandleState((object)"Not found receipt");

                receiptModel.Status = AccountingConstants.RECEIPT_STATUS_CANCEL;
                receiptModel.GroupId = currentUser.GroupId;
                receiptModel.DepartmentId = currentUser.DepartmentId;
                receiptModel.OfficeId = currentUser.OfficeID;
                receiptModel.CompanyId = currentUser.CompanyID;
                receiptModel.UserModified = currentUser.UserID;
                receiptModel.DatetimeModified = DateTime.Now;

                AcctReceipt receipt = mapper.Map<AcctReceipt>(receiptModel);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Update(receipt, x => x.Id == receipt.Id, false);
                        if (hs.Success)
                        {
                            // Lấy ra ds payment của Receipt
                            var paymentsReceipt = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id).ToList();
                            // Phát sinh những dòng payment âm 
                            var hsAddPaymentNegative = AddPaymentsNegative(paymentsReceipt, receipt);
                            // Cập nhật invoice cho những payment
                            var hsUpdateInvoiceOfPayment = UpdateInvoiceOfPayment(receipt.Id);
                            // Cập nhật Cus Advance của Agreement
                            var hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptModel);

                            DataContext.SubmitChanges();
                            trans.Commit();
                        }
                        return hs;
                    }
                    catch (Exception ex)
                    {
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
                return new HandleState((object)ex.Message);
            }
        }

        public ProcessClearInvoiceModel ProcessReceiptInvoice(ProcessReceiptInvoice criteria)
        {
            List<ReceiptInvoiceModel> invoiceList = new List<ReceiptInvoiceModel>();
            ProcessClearInvoiceModel results = new ProcessClearInvoiceModel();

            if (criteria.List.Count() > 0)
            {
                invoiceList = criteria.List.OrderBy(x => x.Index).ToList();

                decimal currentPaidAmount = criteria.PaidAmount;

                foreach (ReceiptInvoiceModel invoice in invoiceList)
                {
                    if (criteria.Currency == AccountingConstants.CURRENCY_LOCAL)
                    {
                        invoice.ReceiptExcUnpaidAmount = invoice.UnpaidAmount * criteria.FinalExchangeRate; // số tiền còn lại cần thu của invoice theo tỉ giá phiếu thu
                        if (currentPaidAmount - invoice.ReceiptExcUnpaidAmount > 0) // Trừ hết số tiền còn lại của invoice
                        {
                            if (invoice.Currency != AccountingConstants.CURRENCY_LOCAL)
                            {
                                invoice.PaidAmount = invoice.ReceiptExcUnpaidAmount;
                            }
                            else
                            {
                                invoice.PaidAmount = invoice.UnpaidAmount;
                            }

                            invoice.InvoiceBalance = 0;
                        }
                        else
                        {
                            invoice.PaidAmount = currentPaidAmount;
                            invoice.InvoiceBalance = invoice.ReceiptExcUnpaidAmount - invoice.PaidAmount;
                        }

                        invoice.ReceiptExcPaidAmount = NumberHelper.RoundNumber(invoice.PaidAmount / criteria.FinalExchangeRate ?? 0, 3);
                        invoice.ReceiptExcInvoiceBalance = NumberHelper.RoundNumber(invoice.InvoiceBalance / criteria.FinalExchangeRate ?? 0, 3);

                    }
                    else
                    {
                        invoice.ReceiptExcUnpaidAmount = NumberHelper.RoundNumber(invoice.UnpaidAmount / criteria.FinalExchangeRate, 3); // số tiền còn lại của invoice theo tỉ giá phiếu thu
                        if (currentPaidAmount - invoice.ReceiptExcUnpaidAmount > 0) // Trừ hết số tiền còn lại của invoice
                        {
                            if (invoice.Currency != AccountingConstants.CURRENCY_LOCAL)
                            {
                                invoice.PaidAmount = invoice.UnpaidAmount;
                            }
                            else
                            {
                                invoice.PaidAmount = invoice.ReceiptExcUnpaidAmount;
                            }

                            invoice.InvoiceBalance = 0;

                        }
                        else
                        {
                            invoice.PaidAmount = currentPaidAmount;
                            invoice.InvoiceBalance = invoice.ReceiptExcUnpaidAmount - invoice.PaidAmount;

                        }

                        invoice.ReceiptExcPaidAmount = NumberHelper.RoundNumber(invoice.PaidAmount * criteria.FinalExchangeRate ?? 0);
                        invoice.ReceiptExcInvoiceBalance = NumberHelper.RoundNumber(invoice.InvoiceBalance * criteria.FinalExchangeRate ?? 0);
                    }
                    currentPaidAmount -= (invoice.ReceiptExcUnpaidAmount ?? 0);

                }

                results.Invoices = invoiceList;
                if (currentPaidAmount > 0) // trường hợp thu dư
                {
                    CatPartner partnerInfo = catPartnerRepository.Get(x => x.Id == criteria.CustomerID)?.FirstOrDefault();
                    ReceiptInvoiceModel adv = new ReceiptInvoiceModel
                    {
                        PartnerName = partnerInfo?.ShortName,
                        Type = "ADV",
                        PaidAmount = currentPaidAmount,
                        InvoiceBalance = 0,
                        TaxCode = partnerInfo?.TaxCode,
                        Currency = criteria.Currency
                    };
                    results.Balance = currentPaidAmount;
                    results.Invoices.Add(adv);
                }
                else if (currentPaidAmount < 0) // trường hợp thu thiếu
                {
                    results.Balance = currentPaidAmount;
                }
                else
                {
                    results.Balance = 0;
                }
            }
            return results;
        }

        #region -- Get Customers Debit --

        public List<CustomerDebitCreditModel> GetDataIssueCustomerPayment(CustomerDebitCreditCriteria criteria)
        {
            var data = new List<CustomerDebitCreditModel>();
            var debits = GetDebitForIssueCustomerPayment(criteria);
            var obhs = GetObhForIssueCustomerPayment(criteria);
            var soaCredits = GetSoaCreditForIssueCustomerPayment(criteria);
            var creditNotes = GetCreditNoteForIssueCustomerPayment(criteria);
            if (debits != null)
            {
                data.AddRange(debits);
            }
            if (obhs != null)
            {
                data.AddRange(obhs);
            }
            if (soaCredits != null)
            {
                data.AddRange(soaCredits);
            }
            if (creditNotes != null)
            {
                data.AddRange(creditNotes);
            }
            return data;
        }

        private Expression<Func<AccAccountingManagement, bool>> InvoiceExpressionQuery(CustomerDebitCreditCriteria criteria, string type)
        {
            //Get Vat Invoice có Payment Status # Paid
            Expression<Func<AccAccountingManagement, bool>> query = q => q.Type == type && q.PaymentStatus != "Paid";
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(q => q.PartnerId == criteria.PartnerId);
            }

            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                var acctManagementIds = new List<Guid?>();
                if (criteria.SearchType.Equals("SOA"))
                {
                    acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Soano, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();                   
                }
                else if (criteria.SearchType.Equals("Debit Note/Invoice"))
                {
                    acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.DebitNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
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

                if (acctManagementIds != null)
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
                                transactions = csTransactionRepository.Get(x => x.CurrentStatus != TermData.Canceled
                                                                  && (
                                                                        (x.TransactionType.Contains("I") ? x.Eta.HasValue : x.Etd.HasValue)
                                                                        ?
                                                                        (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) >= criteria.FromDate.Value.Date && (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) <= criteria.ToDate.Value.Date
                                                                        : false
                                                                     )); //Import - ETA, Export - ETD
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
                            if (acctManagementIds != null)
                            {
                                query = query.And(x => acctManagementIds.Contains(x.Id));
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(criteria.Service))
            {
                var acctManagementIds = surchargeRepository.Get(x => criteria.Service.Contains(x.TransactionType)).Select(se => se.AcctManagementId).Distinct().ToList();
                if (acctManagementIds != null)
                {
                    query = query.And(x => acctManagementIds.Contains(x.Id));
                }
            }

            return query;
        }

        private Expression<Func<AcctSoa, bool>> SoaCreditExpressionQuery(CustomerDebitCreditCriteria criteria)
        {
            //Get SOA: Type = Credit & NetOff = false
            Expression<Func<AcctSoa, bool>> query = q => q.Type == "Credit" && q.NetOff == false;
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(q => q.Customer == criteria.PartnerId);
            }

            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                var soaNo = new List<string>();
                if (criteria.SearchType.Equals("SOA"))
                {
                    query = query.And(x => criteria.ReferenceNos.Contains(x.Soano));                   
                }
                else if (criteria.SearchType.Equals("Debit Note/Invoice"))
                {
                    soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.CreditNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("VAT Invoice"))
                {
                    soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("JOB NO"))
                {
                    soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("HBL"))
                {
                    soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("MBL"))
                {
                    soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                }
                else if (criteria.SearchType.Equals("Customs No"))
                {
                    soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.ClearanceNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                }

                if (soaNo != null)
                {
                    query = query.And(x => soaNo.Contains(x.Soano));
                }
            }

            if (criteria.FromDate != null && criteria.ToDate != null)
            {
                if (!string.IsNullOrEmpty(criteria.DateType))
                {
                    if (criteria.DateType == "Invoice Date")
                    {
                        /*var soaNos = surchargeRepository.Get(x => x.InvoiceDate.Value.Date >= criteria.FromDate.Value.Date && x.InvoiceDate.Value.Date <= criteria.ToDate.Value.Date).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNos != null)
                        {
                            query = query.And(x => soaNos.Contains(x.Soano));
                        }*/
                        query = query.And(x => false); //**Phí Credit nên ko có Invoice Date
                    }
                    else if (criteria.DateType == "Billing Date")
                    {
                        /*List<Guid> invoiceIds = acctMngtRepository.Get(x => x.ConfirmBillingDate.Value.Date >= criteria.FromDate.Value.Date && x.ConfirmBillingDate.Value.Date <= criteria.ToDate.Value.Date).Select(se => se.Id).Distinct().ToList();
                        var soaNos = surchargeRepository.Get(x => invoiceIds.Contains(x.AcctManagementId.Value)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNos != null)
                        {
                            query = query.And(x => soaNos.Contains(x.Soano));
                        }*/
                        query = query.And(x => false); //**Phí Credit nên ko có Billing Date
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
                                transactions = csTransactionRepository.Get(x => x.CurrentStatus != TermData.Canceled
                                                                  && (
                                                                        (x.TransactionType.Contains("I") ? x.Eta.HasValue : x.Etd.HasValue)
                                                                        ?
                                                                        (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) >= criteria.FromDate.Value.Date && (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) <= criteria.ToDate.Value.Date
                                                                        : false
                                                                     )); //Import - ETA, Export - ETD
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
                            var soaNos = surchargeRepository.Where(x => dateModeJobNos.Where(w => w == x.JobNo).Any()).Select(se => se.PaySoano).Distinct().ToList();
                            if (soaNos != null)
                            {
                                query = query.And(x => soaNos.Contains(x.Soano));
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(criteria.Service))
            {
                var soaNos = surchargeRepository.Get(x => criteria.Service.Contains(x.TransactionType)).Select(se => se.PaySoano).Distinct().ToList();
                if (soaNos != null)
                {
                    query = query.And(x => soaNos.Contains(x.Soano));
                }
            }
            return query;
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
                else if (criteria.SearchType.Equals("Debit Note/Invoice"))
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

                if (creditNo != null)
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
                                transactions = csTransactionRepository.Get(x => x.CurrentStatus != TermData.Canceled
                                                                  && (
                                                                        (x.TransactionType.Contains("I") ? x.Eta.HasValue : x.Etd.HasValue)
                                                                        ?
                                                                        (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) >= criteria.FromDate.Value.Date && (x.TransactionType.Contains("I") ? x.Eta.Value.Date : x.Etd.Value.Date) <= criteria.ToDate.Value.Date
                                                                        : false
                                                                     )); //Import - ETA, Export - ETD
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
                            if (creditNos != null)
                            {
                                query = query.And(x => creditNos.Contains(x.Code));
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(criteria.Service))
            {
                var creditNos = surchargeRepository.Get(x => criteria.Service.Contains(x.TransactionType)).Select(se => se.CreditNo).Distinct().ToList();
                if (creditNos != null)
                {
                    query = query.And(x => creditNos.Contains(x.Code));
                }
            }
            return query;
        }

        /// <summary>
        /// Get list Invoice by Criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
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
            var grpInvoiceCharge = query.GroupBy(g => g.inv).Select(s => new { Invoice = s.Key, Soa_DebitNo = s.Select(se => new { se.sur.Soano, se.sur.DebitNo }) });
            var data = grpInvoiceCharge.Select(se => new CustomerDebitCreditModel {
                RefNo = se.Soa_DebitNo.Any(w => !string.IsNullOrEmpty(w.Soano)) ? se.Soa_DebitNo.Where(w => !string.IsNullOrEmpty(w.Soano)).Select(s => s.Soano).FirstOrDefault() : se.Soa_DebitNo.Where(w => !string.IsNullOrEmpty(w.DebitNo)).Select(s => s.DebitNo).FirstOrDefault(),
                Type = "Debit",
                InvoiceNo = se.Invoice.InvoiceNoReal,
                InvoiceDate = se.Invoice.Date,
                PartnerId = se.Invoice.PartnerId,
                CurrencyId = se.Invoice.Currency,
                Amount = se.Invoice.TotalAmount,
                UnpaidAmount = se.Invoice.UnpaidAmount,
                UnpaidVnd = se.Invoice.UnpaidAmountVnd,
                UnpaidUsd = se.Invoice.UnpaidAmountUsd,
                PaymentTerm = se.Invoice.PaymentTerm,
                DueDate = se.Invoice.PaymentDueDate,
                PaymentStatus = se.Invoice.PaymentStatus,
                DepartmentId = se.Invoice.DepartmentId,
                OfficeId = se.Invoice.OfficeId,
                CompanyId = se.Invoice.CompanyId
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
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidVnd = inv.UnpaidVnd,
                               UnpaidUsd = inv.UnpaidUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               DepartmentName = dept != null ? dept.DeptNameAbbr : null,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId
                           };
            return joinData;
        }

        /// <summary>
        /// Get list Invoice Temp by Criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
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
            var grpInvoiceCharge = query.GroupBy(g => new { PartnerId = g.inv.PartnerId, RefNo = (g.sur.SyncedFrom == "CDNOTE" ? g.sur.DebitNo : (g.sur.SyncedFrom == "SOA" ? g.sur.Soano : null) ) })
                .Select(s => new { PartnerId = s.Key.PartnerId, RefNo = s.Key.RefNo, Invoice = s.Select(se => se.inv) });
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
                UnpaidVnd = se.Invoice.Sum(su => su.UnpaidAmountVnd),
                UnpaidUsd = se.Invoice.Sum(su => su.UnpaidAmountUsd),
                PaymentTerm = se.Invoice.Select(s => s.PaymentTerm).FirstOrDefault(),
                DueDate = se.Invoice.FirstOrDefault().PaymentDueDate,
                PaymentStatus = se.Invoice.Select(s => s.PaymentStatus).FirstOrDefault(),
                DepartmentId = se.Invoice.Select(s => s.DepartmentId).FirstOrDefault(),
                OfficeId = se.Invoice.Select(s => s.OfficeId).FirstOrDefault(),
                CompanyId = se.Invoice.Select(s => s.CompanyId).FirstOrDefault()
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
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidVnd = inv.UnpaidVnd,
                               UnpaidUsd = inv.UnpaidUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               DepartmentName = dept != null ? dept.DeptNameAbbr : null,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId
                           };
            return joinData;
        }

        /// <summary>
        /// Get list Credit Note/ SOA type Credit by Criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IQueryable<CustomerDebitCreditModel> GetSoaCreditForIssueCustomerPayment(CustomerDebitCreditCriteria criteria)
        {
            var expQuery = SoaCreditExpressionQuery(criteria);
            var soas = soaRepository.Get(expQuery);
            var surcharges = surchargeRepository.Get();
            var partners = catPartnerRepository.Get();
            var departments = departmentRepository.Get();
            var offices = officeRepository.Get();

            var query = from soa in soas
                        join sur in surcharges on soa.Soano equals sur.PaySoano
                        select new { soa, sur };
            var grpSoaCharge = query.GroupBy(g => g.soa).Select(s => new { Soa = s.Key, Surcharge = s.Select(se => se.sur) });
            var data = grpSoaCharge.Select(se => new CustomerDebitCreditModel
            {
                RefNo = se.Soa.Soano,
                Type = "Credit",
                InvoiceNo = null,
                InvoiceDate = null,
                PartnerId = se.Soa.Customer,
                CurrencyId = se.Soa.Currency,
                Amount = se.Soa.CreditAmount,
                UnpaidAmount = se.Soa.CreditAmount,
                UnpaidVnd = se.Surcharge.Sum(su => su.AmountVnd + su.VatAmountVnd),
                UnpaidUsd = se.Surcharge.Sum(su => su.AmountUsd + su.VatAmountUsd),
                PaymentTerm = null,
                DueDate = null,
                PaymentStatus = null,
                DepartmentId = se.Soa.DepartmentId,
                OfficeId = se.Soa.OfficeId,
                CompanyId = se.Soa.CompanyId
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
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidVnd = inv.UnpaidVnd,
                               UnpaidUsd = inv.UnpaidUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               DepartmentName = dept != null ? dept.DeptNameAbbr : null,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId
                           };
            return joinData;
        }

        /// <summary>
        /// Get list Credit Note/ SOA type Credit by Criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
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
            var grpCreditNoteCharge = query.GroupBy(g => g.credit).Select(s => new { CreditNote = s.Key, Surcharge = s.Select(se => se.sur) });
            var data = grpCreditNoteCharge.Select(se => new CustomerDebitCreditModel
            {
                RefNo = se.CreditNote.Code,
                Type = "Credit",
                InvoiceNo = null,
                InvoiceDate = null,
                PartnerId = se.CreditNote.PartnerId,
                CurrencyId = se.CreditNote.CurrencyId,
                Amount = se.CreditNote.Total,
                UnpaidAmount = se.CreditNote.Total,
                UnpaidVnd = se.Surcharge.Sum(su => su.AmountVnd + su.VatAmountVnd),
                UnpaidUsd = se.Surcharge.Sum(su => su.AmountUsd + su.VatAmountUsd),
                PaymentTerm = null,
                DueDate = null,
                PaymentStatus = null,
                DepartmentId = se.CreditNote.DepartmentId,
                OfficeId = se.CreditNote.OfficeId,
                CompanyId = se.CreditNote.CompanyId
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
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidVnd = inv.UnpaidVnd,
                               UnpaidUsd = inv.UnpaidUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               DepartmentId = inv.DepartmentId,
                               DepartmentName = dept != null ? dept.DeptNameAbbr : null,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId
                           };
            return joinData;
        }
        #endregion -- Get Customers Debit --
    }
}
