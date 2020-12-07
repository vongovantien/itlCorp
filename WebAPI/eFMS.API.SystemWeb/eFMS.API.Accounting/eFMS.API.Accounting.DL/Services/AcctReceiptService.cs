using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.Receipt;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
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
using AutoMapper.QueryableExtensions;

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
        public AcctReceiptService(
            IContextBase<AcctReceipt> repository,
            IMapper mapper,
            ICurrentUser curUser,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<AccAccountingManagement> acctMngtRepo,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<CatContract> catContractRepo,
            IContextBase<AccAccountingPayment> acctPaymentRepo,
            IContextBase<SysUser> sysUserRepo
            ) : base(repository, mapper)
        {
            currentUser = curUser;
            acctMngtRepository = acctMngtRepo;
            catPartnerRepository = catPartnerRepo;
            catContractRepository = catContractRepo;
            acctPaymentRepository = acctPaymentRepo;
            sysUserRepository = sysUserRepo;

        }

        private IQueryable<AcctReceipt> GetQueryBy(AcctReceiptCriteria criteria)
        {
            Expression<Func<AcctReceipt, bool>> query = (x => 
            (x.CurrencyId ?? "").IndexOf(criteria.Currency ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            && (x.CustomerId ?? "").IndexOf(criteria.CustomerID ?? "",StringComparison.OrdinalIgnoreCase) >= 0         
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
                    d.CustomerName = item.CustomerId == null ? null :catPartnerRepository.Get(x => x.Id == item.CustomerId.ToString()).FirstOrDefault().ShortName;
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
            if (criteria.AgreementID != Guid.Empty)
            {
                CatContract agreement = catContractRepository.Get(x => x.Id == criteria.AgreementID).FirstOrDefault();
                if (agreement != null)
                {
                    agreementService = agreement.SaleService;
                    agreementBaseOn = agreement.BaseOn;
                }
            }

            Expression<Func<AccAccountingManagement, bool>> queryInvoice = null;
            queryInvoice = x => ((x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE || x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE)
             && (x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID || x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART)
             && ((x.ServiceType ?? "").Contains(agreementService ?? "", StringComparison.OrdinalIgnoreCase)));

            // Trường hợp Base On không có giá trị hoặc Base On = 'Invoice Date' thì search theo ngày Invoice Date
            if (string.IsNullOrEmpty(agreementBaseOn) || agreementBaseOn == AccountingConstants.AGREEMENT_BASE_ON_INVOICE_DATE)
            {
                if (criteria.FromDate != null && criteria.ToDate != null)
                {
                    queryInvoice = queryInvoice.And(x => x.Date.Value.Date >= criteria.FromDate.Value.Date && x.Date.Value.Date <= criteria.ToDate.Value.Date);
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

            if (invoices != null)
            {
                IQueryable<CatPartner> partners = catPartnerRepository.Get();

                var queryReceiptInvoice = from invoice in invoices
                                          join partner in partners on invoice.PartnerId equals partner.Id into grpPartners
                                          from grpPartner in grpPartners.DefaultIfEmpty()
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
                        BillingDate = x.invoice.ConfirmBillingDate
                    }).ToList();
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
            var acctPayments = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id).ToList();
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
                payment.RefAmount = acctPayment.RefAmount;
                payment.RefCurrency = acctPayment.RefCurrency;
                payment.PaymentStatus = invoice?.PaymentStatus;
                payment.BillingDate = invoice?.ConfirmBillingDate;
                payment.InvoiceDate = invoice?.Date;
                payment.Note = acctPayment.Note;

                paymentReceipts.Add(payment);
            }
            result.Payments = paymentReceipts;
            result.UserNameCreated = sysUserRepository.Where(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
            result.UserNameModified = sysUserRepository.Where(x => x.Id == result.UserModified).FirstOrDefault()?.Username;
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
                _payment.PaymentAmount = payment.PaidAmount;
                _payment.Balance = payment.InvoiceBalance;
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
                    _payment.PaymentAmount = payment.PaidAmount;
                    _payment.Balance = payment.InvoiceBalance;
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
            foreach(var id in ids)
            {
                hsDelete = acctPaymentRepository.Delete(x => x.Id == id);
            }
            return hsDelete;
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
                        HandleState hs = DataContext.Update(receipt, x => x.Id == receipt.Id ,false);
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
            return new HandleState();
        }

        public HandleState SaveCancel(AcctReceiptModel receiptModel)
        {
            return new HandleState();
        }

    }
}
