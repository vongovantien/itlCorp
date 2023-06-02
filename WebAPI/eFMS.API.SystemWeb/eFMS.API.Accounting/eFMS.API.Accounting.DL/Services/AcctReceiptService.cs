using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Accounting.DL.Models.Receipt;
using eFMS.API.Accounting.Service.Contexts;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
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
        private readonly IContextBase<AcctDebitManagementAr> debitMngtArRepository;
        private readonly IContextBase<AccAccountPayable> accountPayableRepository;
        private readonly IContextBase<AcctSettlementPayment> settlementPaymentRepository;
        private readonly IContextBase<AccAccountPayablePayment> payablePaymentRepository;
        private readonly IContextBase<SysGroup> groupRepository;

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
            IOptions<WebUrl> wUrl,
            IContextBase<AcctDebitManagementAr> debitMngtArRepo,
            IContextBase<AccAccountPayable> accountPayableRepo,
            IContextBase<AcctSettlementPayment> settlementPaymentRepo,
            IContextBase<AccAccountPayablePayment> payablePaymentRepo,
            IContextBase<SysGroup> groupRepo
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
            debitMngtArRepository = debitMngtArRepo;
            accountPayableRepository = accountPayableRepo;
            settlementPaymentRepository = settlementPaymentRepo;
            payablePaymentRepository = payablePaymentRepo;
            groupRepository = groupRepo;
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

            if (!string.IsNullOrEmpty(criteria.PaymentMethod))
            {
                query = query.And(x => x.PaymentMethod == criteria.PaymentMethod);
            }

            if (!string.IsNullOrEmpty(criteria.SyncStatus))
            {
                query = query.And(x => criteria.SyncStatus == x.SyncStatus);
            }

            // Tìm theo Receipt Type
            if (!string.IsNullOrEmpty(criteria.Class))
            {
                query = query.And(x => criteria.Class == x.Class);
            }


            // Tìm theo ngày sync/ngày thu
            if (!string.IsNullOrEmpty(criteria.DateType) && criteria.DateType == "Last Sync")
            {
                query = query.And(x => x.LastSyncDate.Value.Date >= criteria.DateFrom.Value.Date && x.LastSyncDate.Value.Date <= criteria.DateTo.Value.Date);
            }
            if (!string.IsNullOrEmpty(criteria.DateType) && criteria.DateType == "Paid Date" && criteria.DateFrom.HasValue && criteria.DateTo.HasValue)
            {
                query = query.And(x => x.PaymentDate.Value.Date >= criteria.DateFrom.Value.Date && x.PaymentDate.Value.Date <= criteria.DateTo.Value.Date);
            }
            if (!string.IsNullOrEmpty(criteria.DateType) && criteria.DateType == "Create Date" && criteria.DateFrom.HasValue && criteria.DateTo.HasValue)
            {
                query = query.And(x => x.DatetimeCreated.Value.Date >= criteria.DateFrom.Value.Date && x.DatetimeCreated.Value.Date <= criteria.DateTo.Value.Date);
            }

            // Tìm theo số phiếu thu/số invoice
            if (string.IsNullOrEmpty(criteria.PaymentType) || (!string.IsNullOrEmpty(criteria.PaymentType) && criteria.PaymentType == "Payment"))
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
            if(!string.IsNullOrEmpty(criteria.PaymentType) && criteria.PaymentType == "ARCB No")
            {
                query = query.And(x => (x.Arcbno ?? "").IndexOf(criteria.RefNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0 || (x.SubArcbno ?? "").IndexOf(criteria.RefNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
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
                    d.CustomerName = item.CustomerId == null ? null : catPartnerRepository.Get(x => x.Id == item.CustomerId.ToString())?.FirstOrDefault()?.ShortName;

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
                            var payments = acctPaymentRepository.Get(x => x.ReceiptId == id).ToList();
                            if (payments.Count > 0)
                            {
                                var paymentIds = payments.Select(x => x.Id).ToList();
                                var hsDeletePayment = DeletePayments(paymentIds);
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

            var resultPaging = data.Skip((page - 1) * size).Take(size);
            IQueryable<AcctReceiptModel> result = FormatReceipt(resultPaging, criteria);

            return result;
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

        private SysOffice GetOffice(Guid Id)
        {
            return officeRepository.Get(x => x.Id == Id).FirstOrDefault();
        }

        public string GenerateReceiptNo()
        {
            string prefix = "PT";
            var userCurrentOffice = GetOffice(currentUser.OfficeID);
            if (userCurrentOffice != null)
            {
                if (userCurrentOffice.Code == "ITLHAN")
                {
                    prefix = "H" + prefix;
                }
                if (userCurrentOffice.Code == "ITLDAD")
                {
                    prefix = "D" + prefix;
                }

            }
            string ReceiptNo = prefix + DateTime.Now.ToString("yy");

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

        public string GenerateReceiptNoV2(AcctReceiptModel receipt, string officeCode)
        {
            string receiptNo = string.Empty;

            //SysOffice userCurrentOffice = officeRepository.Get(x => x.Id == currentUser.OfficeID).FirstOrDefault();

            switch (receipt.PaymentMethod)
            {
                case "Bank Transfer":
                case "Collected Amount Agency":
                    string prefix = string.Empty;

                    switch (officeCode)
                    {
                        case "ITLHCM":
                            prefix = "SFV";
                            if (receipt.CurrencyId == AccountingConstants.CURRENCY_LOCAL)
                            {
                                prefix += "V";
                            }
                            else
                            {
                                prefix += "U";
                            }
                            break;
                        case "ITLHAN":
                            prefix = "BC";
                            if (receipt.CurrencyId == AccountingConstants.CURRENCY_USD)
                            {
                                prefix += "N";
                            }
                            break;
                        case "ITLDAD":
                            prefix = "DFV";
                            if (receipt.CurrencyId == AccountingConstants.CURRENCY_LOCAL)
                            {
                                prefix += "V";
                            }
                            else
                            {
                                prefix += "U";
                            }

                            break;
                        default:
                            break;
                    }
                    receiptNo = GenerateReceiptNoWithPrefix(prefix, receipt);

                    break;
                case "Clear-Advance":
                case "Clear-Advance-Bank":
                case "Clear-Advance-Cash":
                case "Other":
                    string prefix2 = string.Empty;

                    switch (officeCode)
                    {
                        case "ITLHCM":
                            prefix2 = "SFBT";
                            break;
                        case "ITLHAN":
                            prefix2 = "BT";
                            break;
                        case "ITLDAD":
                            prefix2 = "DFBT";
                            break;
                        default:
                            break;
                    }
                    receiptNo = GenerateReceiptNoWithPrefix(prefix2, receipt);

                    break;
                case "Internal":
                case "COLL - Internal":
                case "Management Fee":
                case "Other Fee":
                case "COLL - Extra":
                case "OBH - Internal":
                case "Paid Amount Agency":
                case "Collect OBH Agency":
                case "Pay OBH Agency":
                case "Advance Agency":
                case "Bank Fee Agency":
                case "Receive From Pay OBH":
                case "Receive From Collect OBH":
                //case "Clear Credit From OBH":
                case "Clear Credit Agency":
                //case "Clear Debit From OBH":
                case "Clear Debit Agency":
                    string prefix3 = string.Empty;

                    switch (officeCode)
                    {
                        case "ITLHCM":
                            prefix3 = "SF";
                            break;
                        case "ITLHAN":
                            prefix3 = "PK";
                            break;
                        case "ITLDAD":
                            prefix3 = "DF";
                            break;
                        default:
                            break;
                    }
                    receiptNo = GenerateReceiptNoWithPrefix(prefix3, receipt);
                    break;
                default:
                    break;
            }

            return receiptNo;
        }

        //AGCBYYMMDDXXXX
        /// <summary>
        /// Generate Combine Receipt No
        /// </summary>
        /// <returns></returns>
        private string GenerateCombineReceiptNo()
        {
            string receiptNo = "ARCB";
            var combineDatas = DataContext.Get(x => x.DatetimeCreated.Value.Year == DateTime.Now.Year && x.DatetimeCreated.Value.Month == DateTime.Now.Month && x.DatetimeCreated.Value.Day == DateTime.Now.Day && !string.IsNullOrEmpty(x.Arcbno) && x.Arcbno.Contains(receiptNo));
            string formatNum = "0000";
            receiptNo += DateTime.Now.ToString("yyMMdd");
            if (combineDatas != null && combineDatas.Count() > 0)
            {
                var orderNoList = combineDatas.Select(x => new { OrderNo = int.Parse(x.Arcbno.Substring(receiptNo.Length, 4)) });
                var maxOrder = orderNoList.Select(x => x.OrderNo).Max();
                receiptNo += (maxOrder + 1).ToString(formatNum);
            }
            else
            {
                receiptNo += 1.ToString(formatNum);
            }
            return receiptNo;
        }

        private string GenerateReceiptNoWithPrefix(string prefix, AcctReceiptModel receipt)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                string patternPT = @"^(" + "PT" + DateTime.Now.ToString("yy") + ")";
                patternPT += @"\d{5}";
                Regex regexPT = new Regex(patternPT);
                var ptReceipts = DataContext.Get(x => !string.IsNullOrEmpty(x.PaymentRefNo) && regexPT.IsMatch(x.PaymentRefNo)).OrderByDescending(x => x.PaymentRefNo).ToList();
                if (ptReceipts != null && ptReceipts.Count > 0)
                {
                    string lastReceiptNo = ptReceipts.FirstOrDefault().PaymentRefNo;
                    string lastReceiptNoNumber = lastReceiptNo.Substring(4, 5);
                    int lastReceiptNoNumberInt = int.Parse(lastReceiptNoNumber);
                    string newReceiptNoNumber = (lastReceiptNoNumberInt + 1).ToString("00000");
                    return "PT" + DateTime.Now.ToString("yy") + newReceiptNoNumber;
                }
                else
                {
                    return "PT" + DateTime.Now.ToString("yy") + "00001";
                }
            }
            string receiptNo = prefix; // default

            string pattern = @"^(" + prefix + ")";
            pattern += @"\d{3}\/\d{2}";
            // pattern += @"\d{3}\/\(" + receipt.PaymentDate?.ToString("MM") + ")$";

            Regex regex = new Regex(pattern);

            IQueryable<AcctReceipt> acctReceipts = DataContext.Where(x => !string.IsNullOrEmpty(x.PaymentRefNo) 
            && regex.IsMatch(x.PaymentRefNo) && x.PaymentDate.Value.Month == receipt.PaymentDate.Value.Month && x.PaymentDate.Value.Year == receipt.PaymentDate.Value.Year).OrderByDescending(x => x.DatetimeCreated);
            if (acctReceipts.Count() > 0)
            {
                // remove /MM
                //var receiptRefNoOrder = acctReceipts.Select(x => new { x.PaymentRefNo, x.PaymentDate, Order = int.Parse(x.PaymentRefNo.Substring(0, x.PaymentRefNo.Length - (x.PaymentRefNo.IndexOf("/") - 1))
                //    .Substring(x.PaymentRefNo.Substring(0, x.PaymentRefNo.Length - (x.PaymentRefNo.IndexOf("/") - 1)).Length - 3, 3))
                //}); 
                var receiptRefNoOrder = acctReceipts.Select(x => new {
                    x.PaymentRefNo,
                    x.PaymentDate,
                    Order = int.Parse(x.PaymentRefNo.Substring(prefix.Length, 3))
                }); // remove /MM
                int listRefNoOrderedMax = receiptRefNoOrder.Select(a => a.Order).Max();
                var orderReceiptNewest = receiptRefNoOrder.First(x => x.Order == listRefNoOrderedMax);
            

                if (orderReceiptNewest.PaymentDate?.ToString("MM") == receipt.PaymentDate?.ToString("MM") 
                    || orderReceiptNewest.PaymentDate?.ToString("MM") == DateTime.Now.ToString("MM"))
                {
                    receiptNo = string.Format(@"{0}{1}/{2}", prefix, (listRefNoOrderedMax + 1).ToString("000"), receipt.PaymentDate?.ToString("MM"));
                }
                else // next tháng
                {
                    receiptNo = string.Format(@"{0}{1}/{2}", prefix, "001", receipt.PaymentDate.HasValue ? receipt.PaymentDate?.ToString("MM") : DateTime.Now.ToString("MM"));
                }
            }
            else 
            {
                receiptNo = string.Format(@"{0}{1}/{2}", prefix, "001", receipt.PaymentDate.HasValue ? receipt.PaymentDate?.ToString("MM") : DateTime.Now.ToString("MM"));
            }

            return receiptNo;
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
            var parentPartner = catPartnerRepository.Get(x => x.Id == result.CustomerId).FirstOrDefault()?.ParentId;
            var partnerInfos = catPartnerRepository.Get(x => x.Id == result.CustomerId || x.ParentId == parentPartner);
            var creditArs = creditMngtArRepository.Get(x => !string.IsNullOrEmpty(x.ReferenceNo));
            if (listOBH.Count() > 0)
            {
                var OBHGrp = listOBH.GroupBy(x => new { x.BillingRefNo, x.Negative, x.CurrencyId });

                List<ReceiptInvoiceModel> items = OBHGrp.Select(s => new ReceiptInvoiceModel
                {
                    RefNo = s.Key.BillingRefNo,
                    Type = "OBH",
                    InvoiceNo = null,
                    Amount = s.FirstOrDefault().RefAmount,
                    UnpaidAmount = s.Key.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? s.FirstOrDefault().UnpaidPaymentAmountVnd :  s.FirstOrDefault().UnpaidPaymentAmountUsd,
                    UnpaidAmountVnd = s.FirstOrDefault().UnpaidPaymentAmountVnd,
                    UnpaidAmountUsd = s.FirstOrDefault().UnpaidPaymentAmountUsd,
                    PaidAmountVnd = s.Sum(x => x.PaymentAmountVnd),
                    PaidAmountUsd = s.Sum(x => x.PaymentAmountUsd),
                    TotalPaidVnd = s.Sum(x => x.PaymentAmountVnd),
                    TotalPaidUsd = s.Sum(x => x.PaymentAmountUsd),
                    Notes = s.FirstOrDefault().Note,
                    OfficeId = s.FirstOrDefault().OfficeInvoiceId,
                    OfficeName = officeRepository.Get(x => x.Id == s.FirstOrDefault().OfficeInvoiceId)?.FirstOrDefault().ShortName,
                    DepartmentId = s.FirstOrDefault().DeptInvoiceId,
                    DepartmentName = departmentRepository.Get(x => x.Id == s.FirstOrDefault().DeptInvoiceId)?.FirstOrDefault()?.DeptNameAbbr,
                    CompanyId = s.FirstOrDefault().CompanyInvoiceId,
                    RefIds = listOBH.Where(x => x.BillingRefNo == s.Key.BillingRefNo).Select(x => x.RefId).ToList(),
                    CreditNo = s.FirstOrDefault().CreditNo,
                    Hblid = s.FirstOrDefault().Hblid,
                    Mbl = GetHBLInfo(s.FirstOrDefault().Hblid).MBL,
                    Hbl = GetHBLInfo(s.FirstOrDefault().Hblid).HBLNo,
                    JobNo = GetHBLInfo(s.FirstOrDefault().Hblid).JobNo,
                    PaymentStatus = receipt.Type == "Customer" ? GetPaymentStatus(listOBH.Where(x => x.BillingRefNo == s.Key.BillingRefNo).Select(x => x.RefId).ToList()) :
                                    GetPaymentStatusAgent(listOBH.Where(x => x.BillingRefNo == s.Key.BillingRefNo).Select(x => x.RefId).ToList(), s.FirstOrDefault().Hblid),
                    ExchangeRateBilling = s.FirstOrDefault().ExchangeRateBilling,
                    PartnerId = s.FirstOrDefault()?.PartnerId?.ToString(),
                    Negative = s.FirstOrDefault()?.Negative,
                    PaymentType = s.FirstOrDefault().PaymentType
                }).ToList();

                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.PartnerId))
                    {
                        var agnecy = partnerInfos.FirstOrDefault(x => x.Id == item.PartnerId);
                        item.PartnerName = agnecy?.ShortName;
                        item.TaxCode = agnecy?.AccountNo;
                    }
                    if (!string.IsNullOrEmpty(receipt.Arcbno) && receipt.PaymentMethod.ToLower().Contains("credit"))
                    {
                        item.ReferenceNo = creditArs.Where(x => x.Code == item.RefNo && x.Hblid == item.Hblid).FirstOrDefault()?.ReferenceNo;
                    }
                }
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
                    string _ReferenceNo = string.Empty;

                    if (acctPayment.Hblid != null && acctPayment.Hblid != Guid.Empty)
                    {
                        //CsTransactionDetail hbl = csTransactionDetailRepository.Get(x => x.Id == acctPayment.Hblid)?.FirstOrDefault();
                        //if (hbl != null)
                        //{
                        //    CsTransaction job = csTransactionRepository.Get(x => x.Id == hbl.JobId)?.FirstOrDefault();
                        //    _Hbl = hbl.Hwbno;
                        //    _Mbl = hbl.Mawb;
                        //    _jobNo = job?.JobNo;
                        //}
                        var surcharge = surchargeRepository.Get(x => x.Hblid == acctPayment.Hblid).FirstOrDefault();
                        if (surcharge != null)
                        {
                            _Hbl = surcharge.Hblno;
                            _Mbl = surcharge.Mblno;
                            _jobNo = surcharge?.JobNo;
                            _ReferenceNo = surcharge?.ReferenceNo;
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
                    payment.PaymentStatus = acctPayment.Type == "DEBIT" ? GetPaymentStatus(new List<string> { acctPayment.RefId }) : GetPaymentStatusAgent(new List<string> { acctPayment.RefId }, acctPayment.Hblid);
                    payment.JobNo = _jobNo;
                    payment.Mbl = _Mbl;
                    payment.Hbl = _Hbl;
                    payment.Hblid = acctPayment.Hblid;
                    payment.CreditNo = acctPayment.CreditNo;
                    payment.CreditAmountVnd = acctPayment.CreditAmountVnd;
                    payment.CreditAmountUsd = acctPayment.CreditAmountUsd;
                    payment.VoucherId = acctPayment.Type == "CREDIT" ? _voucherId : null;
                    payment.VoucherIdre = acctPayment.Type == "CREDIT" ? _voucherIdre : null;
                    payment.ExchangeRateBilling = acctPayment.ExchangeRateBilling;
                    payment.PartnerId = acctPayment?.PartnerId?.ToString();
                    if (!string.IsNullOrEmpty(payment.PartnerId))
                    {
                        var agency = partnerInfos.FirstOrDefault(x => x.Id == acctPayment.PartnerId);
                        payment.PartnerName = agency?.ShortName;
                        payment.TaxCode = agency?.AccountNo;
                    }
                    payment.Negative = acctPayment.Negative;
                    payment.PaymentType = acctPayment.PaymentType;
                    payment.NetOff = acctPayment.NetOff;
                    payment.NetOffUsd = acctPayment.NetOffUsd;
                    payment.NetOffVnd = acctPayment.NetOffVnd;
                    payment.RefCurrency = acctPayment.RefCurrency;

                    List<string> _creditNos = new List<string>();
                    if (!string.IsNullOrEmpty(acctPayment.CreditNo))
                    {
                        _creditNos = acctPayment.CreditNo.Split(",").ToList();
                    }
                    payment.CreditNos = _creditNos;

                    payment.ReferenceNo = acctPayment.Type == "CREDIT" ? creditArs.FirstOrDefault(x => x.Code == acctPayment.BillingRefNo && x.Hblid == acctPayment.Hblid)?.ReferenceNo : _ReferenceNo;

                    paymentReceipts.Add(payment);
                }
            }
            result.Payments = paymentReceipts;
            result.UserNameCreated = sysUserRepository.Where(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
            result.UserNameModified = sysUserRepository.Where(x => x.Id == result.UserModified).FirstOrDefault()?.Username;

            //CatPartner partnerInfo = catPartnerRepository.Get(x => x.Id == result.CustomerId).FirstOrDefault();
            if (!string.IsNullOrEmpty(result.CustomerId))
            {
                result.CustomerName = partnerInfos.FirstOrDefault(x => x.Id == result.CustomerId)?.ShortName;
            }

            SysOffice receiptOffice = officeRepository.Get(x => x.Id == (result.OfficeId ?? Guid.Empty))?.FirstOrDefault();
            result.ReceiptInternalOfficeCode = receiptOffice.InternalCode;

            // Check có tồn tại 1 invoice thu chưa hết.
            if (result.Status == AccountingConstants.RECEIPT_STATUS_DONE)
            {
                result.IsReceiptBankFee = result.Payments.Any(x => !string.IsNullOrEmpty(x.PaymentStatus) && x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART);
                if (result.IsReceiptBankFee == true)
                {
                    IEnumerable<ReceiptInvoiceModel> paymentPaidApart = result.Payments.Where(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART);
                    IEnumerable<ReceiptInvoiceModel> obhPaidAprt = paymentPaidApart.Where(x => x.Type == "OBH");
                    IEnumerable<ReceiptInvoiceModel> debitPaidAprt = paymentPaidApart.Where(x => x.Type == "DEBIT");

                    if (obhPaidAprt.Count() > 0)
                    {
                        foreach (var item in obhPaidAprt)
                        {
                            // filter ID without Paid
                            List<string> ids = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE && item.RefIds.Contains(x.Id.ToString())
                            && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID).Select(x => x.Id.ToString()).ToList();

                            item.RefIds = ids;
                            decimal totalBalanceOBH = 0;
                            decimal totalBalanceOBHVnd = 0;
                            decimal totalBalanceOBHUsd = 0;

                            foreach (var i in ids)
                            {
                                AccAccountingManagement invoice = acctMngtRepository.Get(x => x.Id.ToString() == i)?.FirstOrDefault();
                                totalBalanceOBHVnd = invoice.UnpaidAmountVnd ?? 0;
                                totalBalanceOBHUsd = invoice.UnpaidAmountUsd ?? 0;
                                if (item.RefCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalBalanceOBH = totalBalanceOBHVnd;
                                }
                                else
                                {
                                    totalBalanceOBH = totalBalanceOBHUsd;
                                }
                            }

                            item.Balance = totalBalanceOBH;
                            item.BalanceVnd = totalBalanceOBHVnd;
                            item.BalanceUsd = totalBalanceOBHUsd;
                        }
                    }

                    if (debitPaidAprt.Count() > 0)
                    {
                        foreach (var item in debitPaidAprt)
                        {
                            if (receipt.Type == "Customer")
                            {
                                AccAccountingManagement invoice = acctMngtRepository.Get(x => x.Id.ToString() == item.RefIds.FirstOrDefault())?.FirstOrDefault();
                                item.BalanceVnd = invoice.UnpaidAmountVnd;
                                item.BalanceUsd = invoice.UnpaidAmountUsd;
                                if (item.RefCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    item.Balance = item.BalanceVnd;
                                }
                                else
                                {
                                    item.Balance = item.BalanceUsd;
                                }
                            }
                            else
                            {
                                var debitInvoice = debitMngtArRepository.Get(x => item.RefIds.Contains(x.AcctManagementId.ToString()) && x.Hblid == item.Hblid).FirstOrDefault();
                                item.BalanceVnd = debitInvoice?.UnpaidAmountVnd;
                                item.BalanceUsd = debitInvoice?.UnpaidAmountUsd;
                                if (item.RefCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    item.Balance = item.BalanceVnd;
                                }
                                else
                                {
                                    item.Balance = item.BalanceUsd;
                                }
                            }
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

            if(result.ReferenceId != null)
            {
                AcctReceipt receiptRef = DataContext.Get(x => x.Id == result.ReferenceId)?.FirstOrDefault();
                if(receiptRef != null)
                {
                    result.ReferenceNo = receiptRef.PaymentRefNo + "_" + receiptRef.Class;
                }
            }

            if (result.ObhpartnerId != null)
            {
                CatPartner obhP = catPartnerRepository.Get(x => x.Id == result.ObhpartnerId.ToString())?.FirstOrDefault();

                result.ObhPartnerName = obhP?.ShortName;
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
            //CsTransactionDetail hbl = csTransactionDetailRepository.Get(x => x.Id == hblId)?.FirstOrDefault();
            //if (hbl != null)
            //{
            //    CsTransaction job = csTransactionRepository.Get(x => x.Id == hbl.JobId)?.FirstOrDefault();
            //    result.HBLNo = hbl.Hwbno;
            //    result.MBL = hbl.Mawb;
            //    result.JobNo = job?.JobNo;
            //}
            var surcharge = surchargeRepository.Get(x => x.Hblid == hblId).FirstOrDefault();
            if (surcharge != null)
            {
                result.HBLNo = surcharge.Hblno;
                result.MBL = surcharge.Mblno;
                result.JobNo = surcharge?.JobNo;
            }
            return result;
        }

        private string GetPaymentSatusOBH(IEnumerable<AccAccountingManagement> invoices)
        {
            string _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
            if (invoices.Count() > 0)
            {
                _paymentStatus = GetPaymentStatusInvoice(invoices.ToList());
            }
            return _paymentStatus;
        }

        private string GetPaymentStatusInvoice(List<AccAccountingManagement> listInvoices)
        {
            string _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;

            if (listInvoices.Count == 1)
            {
                _paymentStatus = listInvoices.FirstOrDefault().PaymentStatus;
            }
            else
            {
                bool isPaid = listInvoices.All(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
                if (isPaid == true)
                {
                    _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                }
                else if (listInvoices.Any(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART)
                    || listInvoices.Any(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID))
                {
                    _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                }
            }

            return _paymentStatus;
        }

        private string GetPaymentStatus(List<string> Ids)
        {
            string _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
            List<AccAccountingManagement> invs = acctMngtRepository.Get(x => Ids.Contains(x.Id.ToString())).ToList();

            if (invs.Count > 0)
            {
                _paymentStatus = GetPaymentStatusInvoice(invs);
            }
            return _paymentStatus;
        }

        /// <summary>
        /// Get payment status type obh for agency
        /// </summary>
        /// <param name="invoices"></param>
        /// <returns></returns>
        private string GetPaymentStatusAgent(List<string> invoiceIds, Guid? hblId)
        {
            string _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
            if (invoiceIds.Count > 0)
            {
                var debitAR = debitMngtArRepository.Get(x => x.Hblid == hblId && invoiceIds.Contains(x.AcctManagementId.ToString()));
                var creditAR = from payment in accountPayableRepository.Get(x => invoiceIds.Contains(x.AcctManagementId))
                               join credit in creditMngtArRepository.Get(x => x.Hblid == hblId) on new { payment.PartnerId, payment.ReferenceNo } equals new { credit.PartnerId, credit.ReferenceNo }
                               select credit;
                if (debitAR.Count() > 0)
                {
                    bool isPaid = debitAR.All(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
                    if (isPaid == true)
                    {
                        _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                    }
                    else if (debitAR.Any(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART)
                        || debitAR.Any(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID))
                    {
                        _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                    }
                }
                if(creditAR.Count() > 0)
                {
                    _paymentStatus = creditAR.FirstOrDefault()?.PaymentStatus ?? _paymentStatus;
                }
                else
                {
                    var invoices = acctMngtRepository.Get(x => invoiceIds.Contains(x.Id.ToString())).ToList();
                    _paymentStatus = GetPaymentStatusInvoice(invoices);
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

        private AccAccountingPayment GeneratePaymentOBH(ReceiptInvoiceModel paymentGroupOBH, AcctReceipt receipt, AccAccountingManagement invTemp, AccAccountingPayment paymentExisted = null)
        {
            AccAccountingPayment _payment = new AccAccountingPayment();
            if (paymentExisted != null)
            {
                _payment = mapper.Map<AccAccountingPayment>(paymentExisted);
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

                _payment.Note = paymentGroupOBH.Notes; // Cùng một notes
                _payment.RefAmount = paymentGroupOBH.Amount ?? 0; // Tổng UnpaidAmount của group OBH

                _payment.UserModified = currentUser.UserID;
                _payment.DatetimeModified = receipt.DatetimeCreated;
            }
            else
            {
                _payment.Id = Guid.Empty;
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
                _payment.DatetimeCreated = _payment.DatetimeModified = receipt.DatetimeCreated;
                _payment.GroupId = currentUser.GroupId;
                _payment.DepartmentId = currentUser.DepartmentId;
                _payment.OfficeId = currentUser.OfficeID;
                _payment.CompanyId = currentUser.CompanyID;
            }
            return _payment;
        }

        private HandleState GenerateListPaymentOBH(AcctReceipt receipt, List<ReceiptInvoiceModel> paymentOBHGrps)
        {
            List<AccAccountingPayment> results = new List<AccAccountingPayment>();
            try
            {
                foreach (ReceiptInvoiceModel paymentOBH in paymentOBHGrps)
                {
                    // lấy ra tất cả các hóa đơn tạm trước đó
                    var paymentOBHIds = paymentOBH.RefIds;
                    var existedPayment = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id && paymentOBHIds.Contains(x.RefId)).ToList();

                    // lấy ra tất cả các hóa đơn tạm theo group
                    List<AccAccountingManagement> invoicesTemp = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE
                    && paymentOBH.RefIds.Contains(x.Id.ToString()) && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID)
                             .OrderBy(x => (paymentOBH.CurrencyId == AccountingConstants.CURRENCY_LOCAL) ? x.UnpaidAmountVnd : x.UnpaidAmountUsd)
                             .ToList(); // xắp xếp theo unpaidAmount

                    decimal remainOBHAmountVnd = paymentOBH.PaidAmountVnd ?? 0; // Tổng tiền thu VND trên group OBH
                    decimal remainOBHAmountUsd = paymentOBH.PaidAmountUsd ?? 0;// Tổng tiền thu USD trên group OBH

                    foreach (var invTemp in invoicesTemp)
                    {
                        var currentPayment = existedPayment.Where(x => invTemp.Id.ToString().Contains(x.RefId)).FirstOrDefault();
                        // Tổng Số tiền amount OBH đã thu trên group.
                        if (invTemp.Currency == AccountingConstants.CURRENCY_LOCAL)
                        {
                            if (invTemp.UnpaidAmount <= remainOBHAmountVnd && invTemp.UnpaidAmountVnd <= remainOBHAmountVnd)
                            {
                                if (remainOBHAmountVnd > 0)
                                {
                                    // Phát sinh payment                                
                                    AccAccountingPayment _paymentOBH = GeneratePaymentOBH(paymentOBH, receipt, invTemp, currentPayment);
                                    _paymentOBH.PaymentAmount = _paymentOBH.PaymentAmountVnd = _paymentOBH.TotalPaidVnd = invTemp.UnpaidAmountVnd;// Số tiền thu
                                    _paymentOBH.PaymentAmountUsd = _paymentOBH.TotalPaidUsd = invTemp.UnpaidAmountUsd;

                                    _paymentOBH.Balance = _paymentOBH.BalanceVnd = invTemp.UnpaidAmountVnd - _paymentOBH.PaymentAmountVnd; // Số tiền còn lại
                                    _paymentOBH.BalanceUsd = invTemp.UnpaidAmountUsd - _paymentOBH.PaymentAmountUsd;

                                    // _paymentOBH.PaymentAmountUsd = null;
                                    //_paymentOBH.BalanceUsd = null;

                                    remainOBHAmountVnd = remainOBHAmountVnd - _paymentOBH.PaymentAmountVnd ?? 0; // Số tiền amount OBH còn lại để clear tiếp phiếu hđ tạm tiếp theo sau.
                                    remainOBHAmountUsd = remainOBHAmountUsd - _paymentOBH.PaymentAmountUsd ?? 0; // Số tiền amount OBH còn lại để clear tiếp phiếu hđ tạm tiếp theo sau.

                                    if (receipt.Type == "Agent" && receipt.Class == AccountingConstants.RECEIPT_CLASS_NET_OFF && receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE)
                                    {
                                        _paymentOBH.NetOffUsd = _paymentOBH.TotalPaidUsd;
                                        _paymentOBH.NetOffVnd = _paymentOBH.TotalPaidVnd;
                                    }
                                    results.Add(_paymentOBH);
                                }
                            }
                            else
                            {
                                AccAccountingPayment _paymentOBH = GeneratePaymentOBH(paymentOBH, receipt, invTemp, currentPayment);
                                _paymentOBH.PaymentAmount = _paymentOBH.PaymentAmountVnd = _paymentOBH.TotalPaidVnd = remainOBHAmountVnd;
                                _paymentOBH.PaymentAmountUsd = _paymentOBH.TotalPaidUsd = remainOBHAmountUsd;

                                _paymentOBH.Balance = _paymentOBH.BalanceVnd = invTemp.UnpaidAmountVnd - _paymentOBH.PaymentAmountVnd;
                                _paymentOBH.BalanceUsd = invTemp.UnpaidAmountUsd - _paymentOBH.PaymentAmountUsd;

                                remainOBHAmountVnd = remainOBHAmountVnd - _paymentOBH.PaymentAmountVnd ?? 0;
                                remainOBHAmountUsd = remainOBHAmountUsd - _paymentOBH.PaymentAmountUsd ?? 0;

                                if (receipt.Type == "Agent" && receipt.Class == AccountingConstants.RECEIPT_CLASS_NET_OFF && receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE)
                                {
                                    _paymentOBH.NetOffUsd = _paymentOBH.TotalPaidUsd;
                                    _paymentOBH.NetOffVnd = _paymentOBH.TotalPaidVnd;
                                }
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
                                    AccAccountingPayment _paymentOBH = GeneratePaymentOBH(paymentOBH, receipt, invTemp, currentPayment);
                                    _paymentOBH.PaymentAmount = _paymentOBH.PaymentAmountUsd = _paymentOBH.TotalPaidUsd = invTemp.UnpaidAmountUsd;
                                    _paymentOBH.PaymentAmountVnd = _paymentOBH.TotalPaidVnd = invTemp.UnpaidAmountVnd;

                                    _paymentOBH.Balance = _paymentOBH.BalanceUsd = invTemp.UnpaidAmountUsd - _paymentOBH.PaymentAmountUsd;
                                    _paymentOBH.BalanceVnd = invTemp.UnpaidAmountVnd - _paymentOBH.PaymentAmountVnd;

                                    remainOBHAmountUsd = remainOBHAmountUsd - _paymentOBH.PaymentAmountUsd ?? 0;
                                    remainOBHAmountVnd = remainOBHAmountVnd - _paymentOBH.PaymentAmountVnd ?? 0;

                                    if (receipt.Type == "Agent" && receipt.Class == AccountingConstants.RECEIPT_CLASS_NET_OFF && receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE)
                                    {
                                        _paymentOBH.NetOffUsd = _paymentOBH.TotalPaidUsd;
                                        _paymentOBH.NetOffVnd = _paymentOBH.TotalPaidVnd;
                                    }
                                    results.Add(_paymentOBH);
                                }
                                else
                                {
                                    AccAccountingPayment _paymentOBH = GeneratePaymentOBH(paymentOBH, receipt, invTemp, currentPayment);
                                    _paymentOBH.PaymentAmount = _paymentOBH.PaymentAmountUsd = remainOBHAmountUsd;
                                    _paymentOBH.PaymentAmountVnd = remainOBHAmountVnd;

                                    _paymentOBH.Balance = _paymentOBH.BalanceUsd = invTemp.UnpaidAmountUsd - _paymentOBH.PaymentAmountUsd;

                                    remainOBHAmountVnd = remainOBHAmountVnd - _paymentOBH.PaymentAmountVnd ?? 0;
                                    remainOBHAmountUsd = remainOBHAmountUsd - _paymentOBH.PaymentAmountUsd ?? 0;

                                    if (receipt.Type == "Agent" && receipt.Class == AccountingConstants.RECEIPT_CLASS_NET_OFF && receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE)
                                    {
                                        _paymentOBH.NetOffUsd = _paymentOBH.TotalPaidUsd;
                                        _paymentOBH.NetOffVnd = _paymentOBH.TotalPaidVnd;
                                    }
                                    results.Add(_paymentOBH);
                                }
                            }
                            else
                            {
                                AccAccountingPayment _paymentOBH = GeneratePaymentOBH(paymentOBH, receipt, invTemp, currentPayment);
                                _paymentOBH.PaymentAmount = _paymentOBH.PaymentAmountUsd = _paymentOBH.TotalPaidUsd = remainOBHAmountUsd;
                                _paymentOBH.PaymentAmountVnd = _paymentOBH.TotalPaidVnd = remainOBHAmountVnd;

                                _paymentOBH.Balance = _paymentOBH.BalanceUsd = invTemp.UnpaidAmount - _paymentOBH.PaymentAmountUsd;
                                _paymentOBH.BalanceVnd = invTemp.UnpaidAmountVnd - _paymentOBH.PaymentAmountVnd;

                                remainOBHAmountVnd = remainOBHAmountVnd - _paymentOBH.PaymentAmountVnd ?? 0;
                                remainOBHAmountUsd = remainOBHAmountUsd - _paymentOBH.PaymentAmountUsd ?? 0;

                                if (receipt.Type == "Agent" && receipt.Class == AccountingConstants.RECEIPT_CLASS_NET_OFF && receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE)
                                {
                                    _paymentOBH.NetOffUsd = _paymentOBH.TotalPaidUsd;
                                    _paymentOBH.NetOffVnd = _paymentOBH.TotalPaidVnd;
                                }
                                results.Add(_paymentOBH);

                            }
                        }
                    }
                }
                foreach (var item in results)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        acctPaymentRepository.Add(item, false);
                    }
                    else
                    {
                        acctPaymentRepository.Update(item, x => item.Id == x.Id, false);
                    }
                }

                var hs = acctPaymentRepository.SubmitChanges();
                if (hs.Success)
                {
                    var idPayments = results.Select(x => x.Id).ToList();
                    var paymentOBHDelete = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id && x.Type == "OBH" && !idPayments.Contains(x.Id)).Select(x => x.Id);
                    if (paymentOBHDelete.Count() > 0)
                    {
                        acctPaymentRepository.Delete(x => paymentOBHDelete.Contains(x.Id));
                    }
                }
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_GenerateListPaymentOBH_LOG", "Error: " + ex.ToString() + " - Data: " + JsonConvert.SerializeObject(results));
                return new HandleState();
            }
        }

        private HandleState GenerateListCreditDebitPayment(AcctReceipt receipt, List<ReceiptInvoiceModel> payments)
        {
            List<AccAccountingPayment> results = new List<AccAccountingPayment>();

            var existingPayment = acctPaymentRepository.Get(x=> x.ReceiptId == receipt.Id).ToList();
            try
            {
                var listIdUpds = new List<Guid>();
                foreach (ReceiptInvoiceModel payment in payments)
                {
                    AccAccountingPayment _payment = new AccAccountingPayment();
                    var currentPayment = existingPayment.Where(x => x.Id == payment.Id).FirstOrDefault();
                    if (currentPayment != null)
                    {
                        _payment = mapper.Map<AccAccountingPayment>(currentPayment);
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

                        _payment.PaidDate = receipt.PaymentDate; //Payment Date Phiếu thu
                        _payment.ExchangeRate = receipt.ExchangeRate; //Exchange Rate Phiếu thu

                        _payment.PaymentMethod = receipt.PaymentMethod; //Payment Method Phiếu thu
                        _payment.RefAmount = payment.Amount; // Số tiền unpaid của hóa đơn
                        _payment.Note = payment.Notes;
                        _payment.DeptInvoiceId = payment.DepartmentId;
                        _payment.OfficeInvoiceId = payment.OfficeId;
                        _payment.CompanyInvoiceId = payment.CompanyId;
                        _payment.CreditAmountVnd = payment.CreditAmountVnd;
                        _payment.CreditAmountUsd = payment.CreditAmountUsd;
                        _payment.NetOff = payment.NetOff;
                        if (receipt.Type == "Agent" && receipt.Class == AccountingConstants.RECEIPT_CLASS_NET_OFF && receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE)
                        {
                            _payment.NetOffUsd = payment.TotalPaidUsd;
                            _payment.NetOffVnd = payment.TotalPaidVnd;
                        }
                        else
                        {
                            _payment.NetOffUsd = payment.NetOffUsd;
                            _payment.NetOffVnd = payment.NetOffVnd;
                        }

                        _payment.UserModified = currentUser.UserID;
                        _payment.DatetimeModified = receipt.DatetimeCreated;

                        string _creditNo = string.Empty;
                        if (payment.CreditNos != null && payment.CreditNos.Count > 0)
                        {
                            _creditNo = string.Join(",", payment.CreditNos);
                        }
                        _payment.CreditNo = _creditNo;

                        listIdUpds.Add(_payment.Id);
                        acctPaymentRepository.Update(_payment, x => x.Id == _payment.Id, false);
                    }
                    else
                    {
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
                        _payment.VoucherNo = payment.VoucherId;
                        _payment.Type = payment.Type;  // OBH/DEBIT

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
                        _payment.PartnerId = !string.IsNullOrEmpty(receipt.Arcbno) ? payment.PartnerId : receipt.CustomerId;
                        _payment.PaymentType = payment.PaymentType;
                        _payment.NetOff = payment.NetOff;
                        if (receipt.Type == "Agent" && receipt.Class == AccountingConstants.RECEIPT_CLASS_NET_OFF && receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE)
                        {
                            _payment.NetOffUsd = payment.TotalPaidUsd;
                            _payment.NetOffVnd = payment.TotalPaidVnd;
                        }
                        else
                        {
                            _payment.NetOffUsd = payment.NetOffUsd;
                            _payment.NetOffVnd = payment.NetOffVnd;
                        }

                        _payment.Hblid = payment.Hblid;
                        _payment.UserCreated = _payment.UserModified = currentUser.UserID;
                        _payment.DatetimeCreated = _payment.DatetimeModified = receipt.DatetimeCreated;
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
                        listIdUpds.Add(_payment.Id);

                        acctPaymentRepository.Add(_payment, false);
                    }
                }
                var hs = acctPaymentRepository.SubmitChanges();
                if (hs.Success)
                {
                    var paymentOBHDelete = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id && x.Type != "OBH" && !listIdUpds.Contains(x.Id)).Select(x => x.Id);
                    if (paymentOBHDelete.Count() > 0)
                    {
                        acctPaymentRepository.Delete(x => paymentOBHDelete.Contains(x.Id));
                    }
                }
                return hs;
            }
            catch(Exception ex)
            {
                new LogHelper("eFMS_AR_GenerateListCreditDebitPayment_LOG", "Error: " + ex.ToString() + " - Data: " + JsonConvert.SerializeObject(payments));
                return new HandleState();
            }
        }

        private HandleState AddPayments(List<ReceiptInvoiceModel> listReceiptInvoice, AcctReceipt receipt)
        {
            HandleState hs = new HandleState();

            // Lọc ra tất cả các Payment OBH group để generate các payment theo hđ tạm trong group.
            List<ReceiptInvoiceModel> paymentOBHGrps = listReceiptInvoice.Where(x => x.Type == "OBH").ToList();
            List<AccAccountingPayment> listPaymentOBH = new List<AccAccountingPayment>();

            if (paymentOBHGrps.Count > 0)
            {
                hs = GenerateListPaymentOBH(receipt, paymentOBHGrps);
            }

            List<ReceiptInvoiceModel> paymentDebitAndCredit = listReceiptInvoice.Where(x => x.Type != "OBH").ToList();
            //List<AccAccountingPayment> listPaymentDebitCredit = new List<AccAccountingPayment>();

            if (paymentDebitAndCredit.Count > 0)
            {
                hs = GenerateListCreditDebitPayment(receipt, paymentDebitAndCredit);
            }

            //hs = acctPaymentRepository.Add(listPaymentOBH, false);
            //hs = acctPaymentRepository.Add(listPaymentDebitCredit, false);

            //hs = acctPaymentRepository.SubmitChanges();
            return hs;
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
                if(invoice != null)
                {
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
                    _payment.VoucherNo = payment.VoucherNo;


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
               
            }
            return hs;
        }

        private string GetAndUpdateStatusInvoice(AccAccountingManagement invoice, SaveAction action = SaveAction.SAVEDONE)
        {
            string _paymentStatus = invoice.PaymentStatus;
            if (invoice.UnpaidAmount <= 0)
            {
                if(action == SaveAction.SAVECANCEL)
                {
                    if (invoice.TotalAmountVnd < 0) // Hóa đơn giảm
                    {
                        _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                    }
                    else
                    {
                        _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;

                    }
                }
                else
                {
                    _paymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                }

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

        private string GetAndUpdateStatusDebitAr(AcctDebitManagementAr invoice)
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

        private HandleState UpdateInvoiceOfPayment(AcctReceipt receipt)
        {
            HandleState hsInvoiceUpdate = new HandleState();
            IQueryable<AccAccountingPayment> payments = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id);
            foreach (AccAccountingPayment payment in payments)
            {
                switch (payment.Type)
                {
                    case "DEBIT":
                    case "CREDIT":
                        decimal totalAmountPayment = payment.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (payment.TotalPaidVnd ?? 0) : (payment.TotalPaidUsd ?? 0);
                        decimal totalAmountVndPaymentOfInv = payment.TotalPaidVnd ?? 0;
                        decimal totalAmountUsdPaymentOfInv = payment.TotalPaidUsd ?? 0;

                        // Tổng thu của invoice bao gôm VND/ USD.
                        AccAccountingManagement invoice = acctMngtRepository.Get(x => x.Id.ToString() == payment.RefId && x.Type != AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE).FirstOrDefault();

                        invoice.PaidAmountUsd = (invoice.PaidAmountUsd ?? 0) + totalAmountUsdPaymentOfInv;
                        invoice.PaidAmountVnd = (invoice.PaidAmountVnd ?? 0) + totalAmountVndPaymentOfInv;

                        invoice.UnpaidAmountUsd = (invoice.TotalAmountUsd ?? 0) - invoice.PaidAmountUsd;
                        invoice.UnpaidAmountVnd = (invoice.TotalAmountVnd ?? 0) - invoice.PaidAmountVnd;

                        invoice.UserModified = currentUser.UserID;
                        invoice.DatetimeModified = DateTime.Now;

                        if (invoice.Currency == AccountingConstants.CURRENCY_LOCAL)
                        {
                            invoice.PaidAmount = invoice.PaidAmountVnd;
                            invoice.UnpaidAmount = invoice.UnpaidAmountVnd;
                        }
                        else
                        {
                            invoice.PaidAmount = invoice.PaidAmountUsd;
                            invoice.UnpaidAmount = invoice.UnpaidAmountUsd;
                        }
                        invoice.PaymentStatus = GetAndUpdateStatusInvoice(invoice);

                        if (invoice.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID)
                        {
                            invoice.UnpaidAmount = invoice.UnpaidAmountVnd = invoice.UnpaidAmountUsd = 0;
                        }

                        hsInvoiceUpdate = acctMngtRepository.Update(invoice, x => x.Id == invoice.Id);

                        // Update AR Debit
                        //if (hsInvoiceUpdate.Success && receipt.Type == "Agent")
                        //{
                        //    AcctDebitManagementAr invoiceDebitAr = debitMngtArRepository.Get(x => x.AcctManagementId.ToString() == payment.RefId)?.FirstOrDefault();
                        //    if (invoiceDebitAr != null)
                        //    {
                        //        invoiceDebitAr.PaidAmountUsd = (invoiceDebitAr.PaidAmountUsd ?? 0) + totalAmountUsdPaymentOfInv;
                        //        invoiceDebitAr.PaidAmountVnd = (invoiceDebitAr.PaidAmountVnd ?? 0) + totalAmountVndPaymentOfInv;

                        //        invoiceDebitAr.UnpaidAmountUsd = (invoiceDebitAr.TotalAmountUsd ?? 0) - invoiceDebitAr.PaidAmountUsd;
                        //        invoiceDebitAr.UnpaidAmountVnd = (invoiceDebitAr.TotalAmountVnd ?? 0) - invoiceDebitAr.PaidAmountVnd;

                        //        invoiceDebitAr.UserModified = currentUser.UserID;
                        //        invoiceDebitAr.DatetimeModified = DateTime.Now;

                        //        if (invoice.Currency == AccountingConstants.CURRENCY_LOCAL)
                        //        {
                        //            invoiceDebitAr.PaidAmount = invoiceDebitAr.PaidAmountVnd;
                        //            invoiceDebitAr.UnpaidAmount = invoiceDebitAr.UnpaidAmountVnd;
                        //        }
                        //        else
                        //        {
                        //            invoiceDebitAr.PaidAmount = invoiceDebitAr.PaidAmountUsd;
                        //            invoiceDebitAr.UnpaidAmount = invoiceDebitAr.UnpaidAmountUsd;
                        //        }
                        //        invoiceDebitAr.PaymentStatus = GetAndUpdateStatusDebitAr(invoiceDebitAr);

                        //        if (invoiceDebitAr.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID)
                        //        {
                        //            invoiceDebitAr.UnpaidAmount = invoiceDebitAr.UnpaidAmountVnd = invoiceDebitAr.UnpaidAmountUsd = 0;
                        //        }

                        //        hsInvoiceUpdate = debitMngtArRepository.Update(invoiceDebitAr, x => x.Id == invoiceDebitAr.Id);
                        //    }
                        //}
                        break;
                    case "OBH":
                        // Lấy ra từng hóa đơn tạm để cấn trừ
                        IQueryable<AccAccountingManagement> invoicesTemp = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE 
                        && x.Id.ToString() == payment.RefId );
                        if (invoicesTemp != null && invoicesTemp.Count() > 0)
                        {
                            decimal remainAmount = payment.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (payment.TotalPaidVnd ?? 0) : (payment.TotalPaidUsd ?? 0); // Số tiền đã thu của phiếu thu;
                            decimal remainAmountUsd = payment.TotalPaidUsd ?? 0;
                            decimal remainAmountVnd = payment.TotalPaidVnd ?? 0; 
                            foreach (AccAccountingManagement item in invoicesTemp)
                            {
                                //1. Số tiền còn lại của payment lớn hơn số tiền phải thu của invoice
                                if (remainAmountVnd > 0 && remainAmountVnd >= item.UnpaidAmountVnd)
                                {
                                    decimal _invoiceAmountUnpaid = (item.UnpaidAmount ?? 0); // số tiền còn lại cần thu
                                    decimal _invoiceAmountUnpaidVnd = (item.UnpaidAmountVnd ?? 0);
                                    decimal _invoiceAmountUnpaidUsd = (item.UnpaidAmountUsd ?? 0);

                                    // item.PaidAmount = (item.PaidAmount ?? 0) + item.UnpaidAmount;
                                    item.PaidAmountVnd = (item.PaidAmountVnd ?? 0) + item.UnpaidAmountVnd;
                                    item.PaidAmountUsd = (item.PaidAmountUsd ?? 0) + item.UnpaidAmountUsd;

                                    // Số tiền còn lại của hóa đơn
                                    // item.UnpaidAmount = (item.UnpaidAmount ?? 0) - _invoiceAmountUnpaid;
                                    item.UnpaidAmountUsd = (item.UnpaidAmountUsd ?? 0) - _invoiceAmountUnpaidUsd;
                                    item.UnpaidAmountVnd = (item.UnpaidAmountVnd ?? 0) - _invoiceAmountUnpaidVnd;
                                }
                                else
                                {
                                    // item.PaidAmount = (item.PaidAmount ?? 0) + remainAmount;
                                    item.PaidAmountVnd = (item.PaidAmountVnd ?? 0) + remainAmountVnd; 
                                    item.PaidAmountUsd = (item.PaidAmountUsd ?? 0) + remainAmountUsd;

                                    // item.UnpaidAmount = (item.UnpaidAmount ?? 0) - item.PaidAmount;
                                    item.UnpaidAmountVnd = (item.UnpaidAmountVnd ?? 0) - remainAmountVnd;
                                    item.UnpaidAmountUsd = (item.UnpaidAmountUsd ?? 0) - remainAmountUsd;

                                    remainAmountUsd = 0;
                                    remainAmountVnd = 0;
                                    remainAmount = 0;
                                }

                                item.UserModified = currentUser.UserID;
                                item.DatetimeModified = DateTime.Now;

                                if (item.Currency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    item.PaidAmount = item.PaidAmountVnd;
                                    item.UnpaidAmount = item.UnpaidAmountVnd;
                                }
                                else
                                {
                                    item.PaidAmount = item.PaidAmountUsd;
                                    item.UnpaidAmount = item.UnpaidAmountUsd;
                                }
                                item.PaymentStatus = GetAndUpdateStatusInvoice(item);
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

        private HandleState UpdateInvoiceOfPaymentCancel(AcctReceipt receipt)
        {
            HandleState hsInvoiceUpdate = new HandleState();

            List<AccAccountingPayment> payments = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id && (x.Type == "DEBIT" || x.Type == "OBH" || x.Type == "CREDIT")).Where(x => x.Negative == true).ToList();

            if (payments.Count > 0)
            {
                foreach (var payment in payments)
                {
                    AccAccountingManagement invoice = acctMngtRepository.Get(x => x.Id.ToString() == payment.RefId).FirstOrDefault();

                    invoice.PaidAmountVnd = (invoice.PaidAmountVnd ?? 0) + (payment.TotalPaidVnd ?? 0);
                    invoice.PaidAmountUsd = (invoice.PaidAmountUsd ?? 0) + (payment.TotalPaidUsd ?? 0);

                    invoice.UnpaidAmountVnd = (invoice.TotalAmountVnd ?? 0) - invoice.PaidAmountVnd;
                    invoice.UnpaidAmountUsd = (invoice.TotalAmountUsd ?? 0) - invoice.PaidAmountUsd;

                    invoice.UserModified = currentUser.UserID;
                    invoice.DatetimeModified = DateTime.Now;
                    if (invoice.Currency == AccountingConstants.CURRENCY_LOCAL)
                    {
                        invoice.PaidAmount = invoice.PaidAmountVnd;
                        invoice.UnpaidAmount = invoice.UnpaidAmountVnd;
                    }
                    else
                    {
                        invoice.PaidAmount = invoice.PaidAmountUsd;
                        invoice.UnpaidAmount = invoice.UnpaidAmountUsd;
                    }
                    invoice.PaymentStatus = GetAndUpdateStatusInvoice(invoice, SaveAction.SAVECANCEL);

                    hsInvoiceUpdate = acctMngtRepository.Update(invoice, x => x.Id == invoice.Id);

                    // Update AR Debit
                    //if (hsInvoiceUpdate.Success && receipt.Type == "Agent")
                    //{
                    //    AcctDebitManagementAr arDebits = debitMngtArRepository.Get(x => x.AcctManagementId.ToString() == payment.RefId)?.FirstOrDefault();
                    //    if (arDebits != null)
                    //    {
                    //        arDebits.PaidAmountVnd = (arDebits.PaidAmountVnd ?? 0) + (payment.TotalPaidVnd ?? 0);
                    //        arDebits.PaidAmountUsd = (arDebits.PaidAmountUsd ?? 0) + (payment.TotalPaidUsd ?? 0);

                    //        arDebits.UnpaidAmountVnd = (arDebits.TotalAmountVnd ?? 0) - invoice.PaidAmountVnd;
                    //        arDebits.UnpaidAmountUsd = (arDebits.TotalAmountUsd ?? 0) - invoice.PaidAmountUsd;

                    //        arDebits.UserModified = currentUser.UserID;
                    //        arDebits.DatetimeModified = DateTime.Now;
                    //        if (invoice.Currency == AccountingConstants.CURRENCY_LOCAL)
                    //        {
                    //            arDebits.PaidAmount = arDebits.PaidAmountVnd;
                    //            arDebits.UnpaidAmount = arDebits.UnpaidAmountVnd;
                    //        }
                    //        else
                    //        {
                    //            arDebits.PaidAmount = arDebits.PaidAmountUsd;
                    //            arDebits.UnpaidAmount = arDebits.UnpaidAmountUsd;
                    //        }
                    //        arDebits.PaymentStatus = GetAndUpdateStatusInvoice(invoice);

                    //        hsInvoiceUpdate = debitMngtArRepository.Update(arDebits, x => x.Id == arDebits.Id);
                    //    }

                    //}
                }
            }

            return hsInvoiceUpdate;
        }

        /// <summary>
        /// Update Customer Advance Amount Of Agreement
        /// </summary>
        /// <param name="receipt"></param>
        /// <param name="action"></param>
        /// <param name="advUsd"></param>
        /// <param name="advVnd"></param>
        /// <returns></returns>
        private HandleState UpdateCusAdvanceOfAgreement(AcctReceipt receipt, SaveAction action, out decimal advUsd, out decimal advVnd)
        {
            HandleState hsAgreementUpdate = new HandleState();
            CatContract agreement = catContractRepository.Get(x => x.Id == receipt.AgreementId).FirstOrDefault();
            if(agreement == null)
            {
                advUsd = 0;
                advVnd = 0;
                return hsAgreementUpdate;
            }
            try
            {
                IQueryable<AccAccountingPayment> payments = acctPaymentRepository.Where(x => x.ReceiptId == receipt.Id
                && (x.Type == "ADV" || x.Type == AccountingConstants.PAYMENT_TYPE_CODE_COLLECT_OBH ||
                x.Type == AccountingConstants.COLLECT_OBH_AGENCY || x.Type == AccountingConstants.ADVANCE_AGENCY)
                && receipt.Class != AccountingConstants.RECEIPT_CLASS_COLLECT_OBH_OTHER);

                decimal? totalAdvPaymentVnd = payments
                  .Select(s => s.PaymentAmountVnd ?? 0)
                  .Sum();

                decimal? totalAdvPaymentUsd = payments
                  .Select(s => s.PaymentAmountUsd ?? 0)
                  .Sum();

                if (action != SaveAction.SAVECANCEL)
                {
                    if (agreement != null)
                    {
                        decimal _cusAdvUsd = 0;
                        decimal _cusAdvVnd = 0;

                        // 16485
                        if (receipt.Class == AccountingConstants.RECEIPT_CLASS_CLEAR_DEBIT && receipt.PaymentMethod == AccountingConstants.PAYMENT_METHOD_COLL_INTERNAL)
                        {
                            _cusAdvUsd = (totalAdvPaymentUsd ?? 0) + (agreement.CustomerAdvanceAmountUsd ?? 0) - (receipt.PaidAmountUsd ?? 0);
                            _cusAdvVnd = (totalAdvPaymentVnd ?? 0) + (agreement.CustomerAdvanceAmountVnd ?? 0) - (receipt.PaidAmountVnd ?? 0);
                        }
                        else
                        {
                            _cusAdvUsd = (totalAdvPaymentUsd ?? 0) + (agreement.CustomerAdvanceAmountUsd ?? 0) - (receipt.CusAdvanceAmountUsd ?? 0);
                            _cusAdvVnd = (totalAdvPaymentVnd ?? 0) + (agreement.CustomerAdvanceAmountVnd ?? 0) - (receipt.CusAdvanceAmountVnd ?? 0);
                        }

                        agreement.CustomerAdvanceAmountUsd = _cusAdvUsd;
                        agreement.CustomerAdvanceAmountVnd = _cusAdvVnd;
                    }
                }
                else
                {
                    if (agreement != null)
                    {
                        // 16485
                        if (receipt.Class == AccountingConstants.RECEIPT_CLASS_CLEAR_DEBIT && receipt.PaymentMethod == AccountingConstants.PAYMENT_METHOD_COLL_INTERNAL)
                        {
                            agreement.CustomerAdvanceAmountUsd = (agreement.CustomerAdvanceAmountUsd ?? 0) + (receipt.PaidAmountUsd ?? 0) - (totalAdvPaymentUsd ?? 0);
                            agreement.CustomerAdvanceAmountVnd = (agreement.CustomerAdvanceAmountVnd ?? 0) + (receipt.PaidAmountVnd ?? 0) - (totalAdvPaymentVnd ?? 0);
                        }
                        else
                        {
                            agreement.CustomerAdvanceAmountUsd = (agreement.CustomerAdvanceAmountUsd ?? 0) + (receipt.CusAdvanceAmountUsd ?? 0) - (totalAdvPaymentUsd ?? 0);
                            agreement.CustomerAdvanceAmountVnd = (agreement.CustomerAdvanceAmountVnd ?? 0) + (receipt.CusAdvanceAmountVnd ?? 0) - (totalAdvPaymentVnd ?? 0);
                        }

                    }
                }

                agreement.UserModified = currentUser.UserID;
                agreement.DatetimeModified = DateTime.Now;

                advUsd = agreement.CustomerAdvanceAmountUsd ?? 0;
                advVnd = agreement.CustomerAdvanceAmountVnd ?? 0;

                hsAgreementUpdate = catContractRepository.Update(agreement, x => x.Id == agreement.Id);
                return hsAgreementUpdate;
            }
            catch(Exception ex)
            {
                advUsd = 0;
                advVnd = 0;
                new LogHelper("eFMS_AR_UpdateCusAdvanceOfAgreement_LOG", ex.ToString() + " -  Data:"+ JsonConvert.SerializeObject(receipt));
                return new HandleState();
            }
        }

        private HandleState AddDraft(AcctReceiptModel receiptModel)
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
                            // Cập nhật cấn trừ debit
                            //if (receiptModel.Type == "Agent")
                            //{
                            //    var hsDebit = UpdateAccountingDebitAR(receiptModel.Payments, SaveAction.SAVEDRAFT_ADD);
                            //    if (!hsDebit.Success)
                            //    {
                            //        new LogHelper("eFMS_SaveReceipt_UpdateDebitAR_LOG", hsDebit.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(receiptModel));
                            //    }
                            //}
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

                            //HandleState hsPaymentDelete = DeletePayments(paymentsOldDelete);


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
                            var listInvoice = receiptModel.Payments
                                .Where(x => (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH)).ToList();
                                //.Select(x => x.InvoiceNo).ToList();
                            // Check payment hien tai
                            IQueryable<AccAccountingPayment> hasPayments = GetPaymentStepOrderReceipt(receiptData, listInvoice);

                            if (hasPayments.Count() > 0)
                            {
                                var query = from p in hasPayments
                                            join r in DataContext.Get() on p.ReceiptId equals r.Id
                                            where r.Status == AccountingConstants.RECEIPT_STATUS_DRAFT
                                            select new { r.Id, p.InvoiceNo, r.PaymentRefNo };

                                if (query != null && query.Count() > 0)
                                {
                                    throw new Exception(string.Format(
                                        "You can not done this receipt, because {0} - {1} have payment time before than this receipt. please done the first receipts firstly!",
                                        query.FirstOrDefault()?.InvoiceNo,
                                        query.FirstOrDefault()?.PaymentRefNo
                                        ));
                                }
                            }
                            HandleState hs = DataContext.Add(receiptData);
                            if (hs.Success)
                            {
                                AcctReceipt receiptCurrent = DataContext.Get(x => x.Id == receiptModel.Id).FirstOrDefault();

                                // Phát sinh Payment
                                HandleState hsPaymentUpdate = AddPayments(receiptModel.Payments, receiptCurrent);
                                if (!hsPaymentUpdate.Success)
                                {
                                    throw new Exception("Có lỗi khi Add/Update Payment" + hsPaymentUpdate.Message?.ToString());
                                }
                                // cấn trừ cho hóa đơn
                                hs = UpdateInvoiceOfPayment(receiptData);
                                if (!hs.Success)
                                {
                                    throw new Exception("Có lỗi khi update cấn trừ hóa đơn" + hs.Message?.ToString());
                                }

                                // Cập nhật CusAdvance cho hợp đồng
                                if (receiptModel.AgreementId != null)
                                {
                                    HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptModel, SaveAction.SAVEDONE, out decimal advUsd, out decimal advVnd);
                                    if (!hsUpdateCusAdvOfAgreement.Success)
                                    {
                                        throw new Exception("Có lỗi khi update thông tin hợp đồng" + hsUpdateCusAdvOfAgreement.Message?.ToString());
                                    }
                                    // cập nhật lại adv lũy tiến cho receipt
                                    receiptData.AgreementAdvanceAmountVnd = advVnd;
                                    receiptData.AgreementAdvanceAmountUsd = advUsd;
                                    hs = DataContext.Update(receiptData, x => x.Id == receiptData.Id);
                                }

                                trans.Commit();
                            }

                            return hs;
                        }
                        else
                        {
                            isAddNew = false;

                            AcctReceipt receipt = DataContext.Get(x => x.Id == receiptModel.Id).FirstOrDefault();

                            receiptModel.Arcbno = receipt.Arcbno;
                            receiptModel.SubArcbno = receipt.SubArcbno;
                            receiptModel.ReceiptMode = receipt.ReceiptMode;
                            receiptModel.UserCreated = receipt.UserCreated;
                            receiptModel.DatetimeCreated = receipt.DatetimeCreated;
                            receiptModel.GroupId = receipt.GroupId;
                            receiptModel.DepartmentId = receipt.DepartmentId;
                            receiptModel.OfficeId = receipt.OfficeId;
                            receiptModel.CompanyId = receipt.CompanyId;

                            receiptModel.UserModified = currentUser.UserID;
                            receiptModel.DatetimeModified = DateTime.Now;

                            receiptModel.Status = AccountingConstants.RECEIPT_STATUS_DONE;
                            receiptModel.DatetimeModified = DateTime.Now;
                            receiptModel.PaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.PaidAmountVnd : receiptModel.PaidAmountUsd;
                            receiptModel.FinalPaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.FinalPaidAmountVnd : receiptModel.FinalPaidAmountUsd;

                            AcctReceipt receiptCurrent = mapper.Map<AcctReceipt>(receiptModel);

                            // Xóa các payment hiện tại, add các payment mới khi update
                            List<Guid> paymentsDelete = acctPaymentRepository.Get(x => x.ReceiptId == receiptCurrent.Id).Select(x => x.Id).ToList();
                            HandleState hsPaymentUpdate = AddPayments(receiptModel.Payments, receiptCurrent);
                            if (!hsPaymentUpdate.Success)
                            {
                                throw new Exception("Có lỗi khi Add/Update Payment" + hsPaymentUpdate.Message?.ToString());
                            }
                            //HandleState hsPaymentDelete = DeletePayments(paymentsDelete);
                            //if (!hsPaymentDelete.Success)
                            //{
                            //    throw new Exception("Có lỗi khi Add/Update Payment" + hsPaymentDelete.Message.ToString());
                            //}

                            // Check payment hien tai
                            IQueryable<AccAccountingPayment> hasPayments = GetPaymentStepOrderReceipt(receiptCurrent);

                            if (hasPayments.Count() > 0)
                            {
                                var query = from p in hasPayments
                                            join r in DataContext.Get() on p.ReceiptId equals r.Id
                                            where r.Status == AccountingConstants.RECEIPT_STATUS_DRAFT
                                            select new { r.Id, p.InvoiceNo, r.PaymentRefNo };

                                if (query != null && query.Count() > 0)
                                {
                                    return new HandleState((object)string.Format(
                                        "You can not done this receipt, because {0} - {1} have payment time before than this receipt. please done the first receipts firstly!",
                                        query.FirstOrDefault()?.InvoiceNo,
                                        query.FirstOrDefault()?.PaymentRefNo
                                        ));
                                }
                            }
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
                                hs = UpdateInvoiceOfPayment(receiptCurrent);

                                if (!hs.Success)
                                {
                                    throw new Exception("Có lỗi khi update cấn trừ hóa đơn" + hs.Message?.ToString());
                                }

                                if (receipt.Arcbno == receipt.SubArcbno && Common.CustomData.PaymentMethodGeneral.Any(c => c.Value == receipt.PaymentMethod))
                                {
                                    GenerateSubGeneralCombine(new List<AcctReceipt>() { receiptCurrent }, receipt.Arcbno);
                                }

                                if (!string.IsNullOrEmpty(receiptCurrent.Arcbno))
                                {
                                    UpdateBalanceReceipt(receiptCurrent.SubArcbno);
                                }

                                if (receiptModel.AgreementId != null)
                                {
                                    HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptCurrent, SaveAction.SAVEDONE, out decimal advUsd, out decimal advVnd);

                                    if (!hsUpdateCusAdvOfAgreement.Success)
                                    {
                                        throw new Exception("Có lỗi khi update thông tin hợp đồng" + hsUpdateCusAdvOfAgreement.Message?.ToString());
                                    }
                                    receiptCurrent.AgreementAdvanceAmountVnd = advVnd;
                                    receiptCurrent.AgreementAdvanceAmountUsd = advUsd;
                                    hs = DataContext.Update(receiptCurrent, x => x.Id == receiptCurrent.Id);
                                }

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
                        hs = UpdateInvoiceOfPayment(receiptCurrent);
                        if (!hs.Success)
                        {
                            throw new Exception("Có lỗi khi update cấn trừ hóa đơn" + hs.Message?.ToString());
                        }
                        if (receiptCurrent.AgreementId != null)
                        {
                            HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptCurrent, SaveAction.SAVEDONE, out decimal advUsd, out decimal advVnd);
                            if (!hs.Success)
                            {
                                throw new Exception("Có lỗi khi update hợp đồng" + hs.Message?.ToString());
                            }
                            receiptCurrent.AgreementAdvanceAmountUsd = advUsd;
                            receiptCurrent.AgreementAdvanceAmountVnd = advVnd;

                            hs = DataContext.Update(receiptCurrent, x => x.Id == receiptCurrent.Id);
                        }
                        trans.Commit();
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

                if (receiptCurrent.Class == AccountingConstants.RECEIPT_CLASS_ADVANCE)
                {
                    var hasReceiptNewest = DataContext.Get(x => x.CustomerId == receiptCurrent.CustomerId
                    && x.Id != receiptCurrent.Id
                    && x.Status == AccountingConstants.RECEIPT_STATUS_DONE
                    && DateTime.Compare(x.DatetimeCreated ?? DateTime.Now, receiptCurrent.DatetimeCreated ?? DateTime.Now) > 0
                    );
                    if (hasReceiptNewest.Count() > 0)
                    {
                        CatPartner partnerInfo = catPartnerRepository.Get(x => x.Id == receiptCurrent.CustomerId).FirstOrDefault();
                        string _customerName = partnerInfo?.ShortName;
                        return new HandleState((object)string.Format(
                            "You can not cancel this receipt, because {0} have payment time later than this receipt time. Please cancel the lastest receipts first!", _customerName
                            ));
                    }
                }
                else if (receiptCurrent.Class == AccountingConstants.RECEIPT_CLASS_CLEAR_DEBIT ||
                    receiptCurrent.Class == AccountingConstants.CLEAR_CREDIT_AGENCY ||
                    receiptCurrent.Class == AccountingConstants.CLEAR_DEBIT_AGENCY)
                {
                    // kiểm tra hóa đơn phát sinh payment # với payment trong receipt current.
                    List<string> receiptCurrentPaymentInvoiceList = acctPaymentRepository.Get(x => x.ReceiptId == receiptCurrent.Id).Where(x => !string.IsNullOrEmpty(x.InvoiceNo)).Select(x => x.InvoiceNo).ToList();
                    List<string> receiptCurrentPaymentVoucherList = acctPaymentRepository.Get(x => x.ReceiptId == receiptCurrent.Id).Where(x => !string.IsNullOrEmpty(x.VoucherNo)).Select(x => x.VoucherNo).ToList();

                    List<string> parentPartners = new List<string>();
                    if (!string.IsNullOrEmpty(receiptCurrent.Arcbno))
                    {
                        var parent = catPartnerRepository.Get(x => x.Id == receiptCurrent.CustomerId).FirstOrDefault()?.ParentId; 
                        if (parent != null)
                        {
                            parentPartners.AddRange(catPartnerRepository.Get(x => x.ParentId == parent).Select(x => x.Id));
                        }
                    }
                    IQueryable<AccAccountingPayment> hasPayments = acctPaymentRepository.Get(x => x.ReceiptId != receiptCurrent.Id
                    && (x.PartnerId == receiptCurrent.CustomerId || parentPartners.Contains(x.PartnerId))
                    && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH || x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
                    && DateTime.Compare(x.DatetimeCreated ?? DateTime.Now, receiptCurrent.DatetimeCreated ?? DateTime.Now) > 0
                    && (receiptCurrentPaymentInvoiceList.Count == 0 || receiptCurrentPaymentInvoiceList.Any(invoice => invoice == x.InvoiceNo))
                    && (receiptCurrentPaymentVoucherList.Count == 0 || receiptCurrentPaymentVoucherList.Any(voucher => voucher == x.VoucherNo))
                    );
                    if (hasPayments.Count() > 0)
                    {
                        var query = (from p in hasPayments
                                     join r in DataContext.Get() on p.ReceiptId equals r.Id
                                     where r.Status == AccountingConstants.RECEIPT_STATUS_DONE
                                     select new { r.Id, r.PaymentMethod, r.PaymentRefNo, p.InvoiceNo, p.VoucherNo, p.RefId, p.Type, p.Hblid }).FirstOrDefault();

                        if (query != null)
                        {
                            if (query.PaymentMethod.ToLower().Contains("credit"))
                            {
                                var payable = accountPayableRepository.Get(x => !string.IsNullOrEmpty(x.ReferenceNo) && x.AcctManagementId == query.RefId).FirstOrDefault();
                                return new HandleState((object)string.Format(
                                    "You can not cancel this receipt, because {0} - {1} - {2} have payment time later than this receipt. Please cancel the lastest receipts first!",
                                        payable?.BillingNo, query.Type, query.PaymentRefNo
                                ));
                            }
                            else
                            {
                                var debitAR = debitMngtArRepository.Get(x => x.AcctManagementId.ToString() == query.RefId && (query.Hblid == null || x.Hblid == query.Hblid)).FirstOrDefault();
                                return new HandleState((object)string.Format(
                                    "You can not cancel this receipt, because {0} - {1} - {2} have payment time later than this receipt. Please cancel the lastest receipts first!",
                                        debitAR?.RefNo, query.Type, query.PaymentRefNo
                                ));
                            }
                        }

                        var receiptDraft = (from p in hasPayments
                                            join r in DataContext.Get() on p.ReceiptId equals r.Id
                                            where r.Status == AccountingConstants.RECEIPT_STATUS_DRAFT
                                            select new { r.Id, r.PaymentMethod, p.InvoiceNo, p.VoucherNo, r.PaymentRefNo, p.RefId, p.Hblid, p.Type }).FirstOrDefault();
                        if (receiptDraft != null)
                        {
                            if (receiptDraft.PaymentMethod.ToLower().Contains("credit"))
                            {
                                var payable = accountPayableRepository.Get(x => !string.IsNullOrEmpty(x.ReferenceNo) && x.AcctManagementId == receiptDraft.RefId).FirstOrDefault();
                                return new HandleState((object)string.Format(
                                "You can not cancel this receipt, because {0} - {1} - {2} have payment time later than this receipt. Please remove the lastest receipts first!",
                                //receiptDraft.PaymentMethod.ToLower().Contains("credit") ? receiptDraft.VoucherNo : receiptDraft.InvoiceNo,
                                payable?.BillingNo, receiptDraft.Type, receiptDraft.PaymentRefNo
                                ));
                            }
                            else
                            {
                                var debitAR = debitMngtArRepository.Get(x => x.AcctManagementId.ToString() == receiptDraft.RefId && (receiptDraft.Hblid == null || x.Hblid == receiptDraft.Hblid)).FirstOrDefault();
                                return new HandleState((object)string.Format(
                                "You can not cancel this receipt, because {0} - {1} - {2} have payment time later than this receipt. Please remove the lastest receipts first!",
                                debitAR?.RefNo, receiptDraft.Type, receiptDraft.PaymentRefNo
                                ));
                            }
                        }
                    }
                }

                receiptCurrent.Status = AccountingConstants.RECEIPT_STATUS_CANCEL;
                receiptCurrent.UserModified = currentUser.UserID;
                receiptCurrent.DatetimeModified = DateTime.Now;

                List<AccAccountingPayment> paymentDeitOBH = new List<AccAccountingPayment>();
                HandleState hs = new HandleState();
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {

                        List<AccAccountingPayment> paymentsReceipt = acctPaymentRepository.Get(x => x.ReceiptId == receiptCurrent.Id).ToList();
                        paymentDeitOBH = paymentsReceipt.Where(x => (x.Type == "DEBIT" || x.Type == "OBH" || x.Type == "CREDIT")).ToList();

                        HandleState hsAddPaymentNegative = AddPaymentsNegative(paymentDeitOBH, receiptCurrent);

                        HandleState hsUpdateInvoiceOfPayment = UpdateInvoiceOfPaymentCancel(receiptCurrent);
                        if (!hsUpdateInvoiceOfPayment.Success)
                        {
                            throw new Exception("Có lỗi khi update cấn trừ hóa đơn" + hsUpdateInvoiceOfPayment.Message?.ToString());
                        }
                        // Cập nhật Netoff cho CREDIT or SOA.
                        //List<AccAccountingPayment> paymentCredit = paymentsReceipt.Where(x => (x.Type != "DEBIT" && x.Type != "OBH")).ToList();
                        //if (paymentCredit.Count > 0)
                        //{
                        //    foreach (var item in paymentCredit)
                        //    {
                        //        if (item.Type == "CREDITNOTE")
                        //        {
                        //            UpdateNetOffCredit(item, true);
                        //        }
                        //        if (item.Type == "CREDITSOA")
                        //        {
                        //            UpdateNetOffSoa(item, true);
                        //        }
                        //    }
                        //}
                        // Cập nhật Cus Advance của Agreement
                        if (receiptCurrent.AgreementId != null)
                        {
                            if (string.IsNullOrEmpty(receiptCurrent.Arcbno)
                                || (!string.IsNullOrEmpty(receiptCurrent.Arcbno) &&
                                (receiptCurrent.PaymentMethod == AccountingConstants.COLLECT_OBH_AGENCY || receiptCurrent.PaymentMethod == AccountingConstants.ADVANCE_AGENCY)))
                            {
                                HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptCurrent, SaveAction.SAVECANCEL, out decimal advUsd, out decimal advVnd);
                                if (!hsUpdateCusAdvOfAgreement.Success)
                                {
                                    throw new Exception("Có lỗi khi update hợp đồng" + hsUpdateCusAdvOfAgreement.Message?.ToString());
                                }
                                receiptCurrent.AgreementAdvanceAmountUsd = advUsd;
                                receiptCurrent.AgreementAdvanceAmountVnd = advVnd;
                            }
                        }
                        hs = DataContext.Update(receiptCurrent, x => x.Id == receiptCurrent.Id);

                        if (hs.Success)
                        {
                            var hsUpd = UpdateBalanceReceipt(receiptCurrent.SubArcbno);
                        }
                        trans.Commit();

                        
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

                // Cập nhật cấn trừ debit
                if (receiptCurrent.Type == "Agent")
                {
                    if (receiptCurrent.PaymentMethod.ToLower().Contains("credit"))
                    {
                        var creditReceipt = mapper.Map<AcctReceiptModel>(receiptCurrent);
                        var paymentInv = paymentDeitOBH.Where(x => x.Type != "DEBIT" && x.Negative != true).Select(x => new ReceiptInvoiceModel
                        {
                            CurrencyId = x.CurrencyId,
                            Type = x.Type,
                            RefIds = new List<string> { x.RefId },
                            Hblid = x.Hblid,
                            PartnerId = x.PartnerId,
                            RefNo = x.BillingRefNo,
                            PaidAmount = x.PaymentAmount,
                            PaidAmountUsd = x.PaymentAmountUsd,
                            PaidAmountVnd = x.PaymentAmountVnd,
                            OfficeId = x.OfficeId
                        }).ToList();
                        creditReceipt.Payments = new List<ReceiptInvoiceModel>();
                        creditReceipt.Payments.AddRange(paymentInv);

                        var hsCredit = UpdateCreditARCombine(new List<AcctReceiptModel>() { creditReceipt }, SaveAction.SAVECANCEL);
                        if (!hsCredit.Success)
                        {
                            new LogHelper("eFMS_SaveCancel_UpdateCreditARCombine_LOG", hsCredit.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(creditReceipt));
                        }
                        hsCredit = AddPaymentsCreditCombine(new List<AcctReceiptModel>() { creditReceipt }, SaveAction.SAVECANCEL);
                        if (!hsCredit.Success)
                        {
                            new LogHelper("eFMS_SaveCancel_AddPaymentsCreditCombine_LOG", hsCredit.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(creditReceipt));
                        }
                    }
                    else
                    {
                        var paymentInv = paymentDeitOBH.Where(x => x.Type != "CREDIT").Select(x => new ReceiptInvoiceModel
                        {
                            CurrencyId = x.CurrencyId,
                            Type = x.Type,
                            RefIds = new List<string> { x.RefId },
                            Hblid = x.Hblid,
                            PartnerId = x.PartnerId,
                            RefNo = x.BillingRefNo,
                            OfficeId = x.OfficeId
                        }).ToList();
                        var hsDebit = UpdateAccountingDebitAR(paymentInv, receiptId, SaveAction.SAVECANCEL);
                        if (!hsDebit.Success)
                        {
                            new LogHelper("eFMS_SaveCancel_UpdateAccountingDebitAR_LOG", hsDebit.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(paymentInv));
                        }
                    }
                }
                return hs;
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
                if (criteria.ReceiptType.ToLower() == "customer")
                {
                    if (paidVnd > 0)
                    {
                        if (invoice.PaidAmountVnd == 0)
                        {
                            if (invoice.NetOffVnd != 0 && invoice.NetOffVnd != null)
                            {
                                paidVnd = paidVnd - (invoice.TotalPaidVnd ?? 0);
                            }
                            else
                            {
                                invoice.PaidAmountVnd = invoice.TotalPaidVnd = invoice.UnpaidAmountVnd;
                                paidVnd = paidVnd - (invoice.PaidAmountVnd ?? 0);
                            }
                        }
                        else if (paidVnd < invoice.PaidAmountVnd)
                        {
                            invoice.PaidAmountVnd = invoice.TotalPaidVnd = paidVnd;
                            paidVnd = 0;
                        }
                        else if ((invoice.NetOffVnd != 0 && invoice.NetOffVnd != null)
                            || invoice.Type == AccountingConstants.PAYMENT_TYPE_CODE_ADVANCE)
                        {
                            paidVnd = paidVnd - (invoice.PaidAmountVnd ?? 0);
                        }
                        else
                        {
                            paidVnd = paidVnd - (invoice.TotalPaidVnd ?? 0);
                        }
                    }
                    else
                    {
                        invoice.PaidAmountVnd = invoice.TotalPaidVnd = 0;
                    }
                }

                if (paidUsd > 0)
                {
                    if (invoice.PaidAmountUsd == 0)
                    {
                        if (invoice.NetOffUsd != 0 && invoice.NetOffUsd != null)
                        {
                            paidUsd = paidUsd - (invoice.TotalPaidUsd ?? 0);
                        }
                        else
                        {
                            invoice.PaidAmountUsd = invoice.TotalPaidUsd = invoice.UnpaidAmountUsd;
                            paidUsd = paidUsd - (invoice.PaidAmountUsd ?? 0);
                        }
                    }
                    else if (paidUsd < invoice.PaidAmountUsd)
                    {
                        invoice.PaidAmountUsd = invoice.TotalPaidUsd = paidUsd;
                        paidUsd = 0;
                    }
                    else if ((invoice.NetOffUsd != 0 && invoice.NetOffUsd != null)
                        || invoice.Type == AccountingConstants.PAYMENT_TYPE_CODE_ADVANCE)
                    {
                        paidUsd = paidUsd - (invoice.PaidAmountUsd ?? 0);
                    }
                    else
                    {
                        paidUsd = paidUsd - (invoice.TotalPaidUsd ?? 0);
                    }
                    if (criteria.ReceiptType.ToLower() == "agent")
                    {
                        if (invoice.UnpaidAmountUsd == invoice.PaidAmountUsd)
                        {
                            invoice.PaidAmountVnd = invoice.TotalPaidVnd = invoice.UnpaidAmountVnd;
                        }
                        else
                        {
                            invoice.PaidAmountVnd = NumberHelper.RoundNumber((invoice.PaidAmountUsd ?? 0) * (invoice.ExchangeRateBilling ?? 0), 0);
                            invoice.TotalPaidVnd = NumberHelper.RoundNumber((invoice.TotalPaidUsd ?? 0) * (invoice.ExchangeRateBilling ?? 0), 0);
                        }
                    }
                }
                else
                {
                    invoice.PaidAmountUsd = invoice.TotalPaidUsd = 0;
                    if (criteria.ReceiptType.ToLower() == "agent")
                    {
                        invoice.PaidAmountVnd = invoice.TotalPaidVnd = 0;
                    }
                }
            }
            return new ProcessClearInvoiceModel
            {
                Invoices = invoiceList,
                CusAdvanceAmountVnd = (criteria.ReceiptType.ToLower() == "agent") ? NumberHelper.RoundNumber(paidUsd * criteria.FinalExchangeRate, 0) : NumberHelper.RoundNumber(paidVnd, 0),
                CusAdvanceAmountUsd = NumberHelper.RoundNumber(paidUsd, 2),
            };
        }

        public IQueryable<CustomerDebitCreditModel> GetDataIssueCustomerPayment(CustomerDebitCreditCriteria criteria)
        {
            List<CustomerDebitCreditModel> data = new List<CustomerDebitCreditModel>();
            IQueryable<CustomerDebitCreditModel> creditNotes = null;
            IQueryable<CustomerDebitCreditModel> debits = null;
            IQueryable<CustomerDebitCreditModel> obhs = null;
            IQueryable<CustomerDebitCreditModel> soaCredits = null;

            debits = GetDebitForIssueCustomerPayment(criteria);
            obhs = GetObhForIssueCustomerPayment(criteria);


            if (debits != null)
            {
                data.AddRange(debits);
            }
            if (obhs != null)
            {
                data.AddRange(obhs);
            }

            return data.AsQueryable();
            //if (soaCredits != null && soaCredits.Count() > 0)
            //{
            //    data.AddRange(soaCredits);
            //}
            //if (creditNotes != null && creditNotes.Count() > 0)
            //{
            //    data.AddRange(creditNotes);
            //}

            //if (data.Count == 0)
            //{
            //    return data;
            //}
            //foreach (var item in data)
            //{
            //    item.DepartmentName = GetDepartmentName(item.RefNo);
            //}
            //return data;
        }
        public AgencyDebitCreditDetailModel GetDataIssueAgencyPayment(CustomerDebitCreditCriteria criteria)
        {
            AgencyDebitCreditDetailModel data = new AgencyDebitCreditDetailModel();
            //data.Invoices = new List<AgencyDebitCreditModel>();

            //IQueryable<AgencyDebitCreditModel> creditNote = null;
            IQueryable<AgencyDebitCreditModel> debits = null;
            IQueryable<AgencyDebitCreditModel> obhs = null;
            IQueryable<AgencyDebitCreditModel> dataMerge = null;
            //IQueryable<AgencyDebitCreditModel> soaCredit = null;
            debits = GetDebitForIssueAgentPayment(criteria);
            obhs = GetObhForIssueAgencyPayment(criteria);
            // Bỏ rule load danh sách credit
            //switch (criteria.SearchType)
            //{
            //    case "Credit Note":
            //        creditNote = GetCreditNoteForIssueAgencyPayment(criteria);
            //        break;
            //    case "SOA":
            //        soaCredit = GetSoaCreditForIssueAgentPayment(criteria);
            //        break;
            //    case "VAT Invoice":
            //        debits = GetDebitForIssueAgentPayment(criteria);
            //        obhs = GetObhForIssueAgencyPayment(criteria);
            //        break;
            //    default:
            //        debits = GetDebitForIssueAgentPayment(criteria);
            //        obhs = GetObhForIssueAgencyPayment(criteria);
            //        soaCredit = GetSoaCreditForIssueAgentPayment(criteria);
            //        creditNote = GetCreditNoteForIssueAgencyPayment(criteria);
            //        break;
            //}

            //if (creditNote != null && creditNote.Count() > 0)
            //{
            //    data.Invoices.AddRange(creditNote);
            //}
            //if (soaCredit != null && soaCredit.Count() > 0)
            //{
            //    data.Invoices.AddRange(soaCredit);
            //}
            List<AgencyDebitCreditModel> debitAgents = new List<AgencyDebitCreditModel>();
            if (debits != null)
            {
                debitAgents.AddRange(debits);
            }
            if (obhs != null)
            {
                debitAgents.AddRange(obhs);
            }
            if (debitAgents == null || debitAgents.Count == 0)
            {
                var newItem = new List<GroupShimentAgencyModel>();
                data.GroupShipmentsAgency = newItem.AsQueryable();
                return data;
            }
            var groupShipmentAgency = debitAgents.OrderBy(x => x.RefNo).Select(s => new GroupShimentAgencyModel
            {
                Hblid = s.Hblid,
                JobNo = s.JobNo,
                Mbl = s.Mbl,
                Hbl = s.Hbl,
                UnpaidAmountUsd = s.UnpaidAmountUsd ?? 0,
                UnpaidAmountVnd = s.UnpaidAmountVnd ?? 0,
                Invoices = new List<AgencyDebitCreditModel>() { s }
            }).AsQueryable();
            data.GroupShipmentsAgency = groupShipmentAgency;
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public AgencyDebitCreditDetailModel GetDataIssueCreditAgency(CustomerDebitCreditCriteria criteria)
        {
            AgencyDebitCreditDetailModel data = new AgencyDebitCreditDetailModel();

            var partner = catPartnerRepository.Get(x => x.PartnerType == "Agent" && x.Active == true);
            List<string> partnerIds = new List<string>();
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                var currentPartner = partner.FirstOrDefault(x => x.Id == criteria.PartnerId);
                partnerIds = partner.Where(x => x.ParentId == currentPartner.ParentId).Select(x => x.Id).ToList();
            }
            var payables = accountPayableRepository.Get(x => (partnerIds.Count == 0 || partnerIds.Contains(x.PartnerId)) && !string.IsNullOrEmpty(x.ReferenceNo) && x.Status != "Paid" && x.TransactionType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE);
            var creditManagementAr = creditMngtArRepository.Get(x => (partnerIds.Count == 0 || partnerIds.Contains(x.PartnerId)) && !string.IsNullOrEmpty(x.ReferenceNo) && x.PaymentStatus != "Paid");

            if (criteria.Office != null && criteria.Office.Count > 0)
            {
                payables = payables.Where(x => x.OfficeId != null && criteria.Office.Contains(x.OfficeId.ToString()));
            }
            if (criteria.ReferenceNos?.Count(x => !string.IsNullOrEmpty(x)) > 0)
            {
                switch (criteria.SearchType)
                {
                    case "VatInvoice":
                        payables = payables.Where(x => criteria.ReferenceNos.Contains(x.InvoiceNo) || criteria.ReferenceNos.Contains(x.VoucherNo));
                        break;
                    case "Debit/Credit/Invoice":
                        payables = payables.Where(x => criteria.ReferenceNos.Contains(x.BillingNo) || criteria.ReferenceNos.Contains(x.InvoiceNo) || criteria.ReferenceNos.Contains(x.VoucherNo));
                        break;
                    case "Soa":
                    case "CreditNote":
                        payables = payables.Where(x => criteria.ReferenceNos.Contains(x.BillingNo));
                        break;
                    //case "ReceiptNo":
                    //payables = payables.Where(x => false);
                    //break;
                    //case "CreditNote":
                    //    payables = payables.Where(x => criteria.ReferenceNos.Contains(x.BillingNo));
                    //break;
                    case "HBL":
                        creditManagementAr = creditManagementAr.Where(x => criteria.ReferenceNos.Contains(x.Hblno));
                        var hblNos = creditManagementAr.Select(x => x.Code).ToList();
                        if (hblNos.Count > 0)
                        {
                            payables = payables.Where(x => hblNos.Contains(x.BillingNo));
                        }
                        else
                        {
                            payables = payables.Where(x => false);
                        }
                        break;
                    case "MBL":
                        creditManagementAr = creditManagementAr.Where(x => criteria.ReferenceNos.Contains(x.Mblno));
                        var mblNos = creditManagementAr.Select(x => x.Code).ToList();
                        if (mblNos.Count > 0)
                        {
                            payables = payables.Where(x => mblNos.Contains(x.BillingNo));
                        }
                        else
                        {
                            payables = payables.Where(x => false);
                        }
                        break;
                    case "JobNo":
                        creditManagementAr = creditManagementAr.Where(x => criteria.ReferenceNos.Contains(x.JobNo));
                        var jobNos = creditManagementAr.Select(x => x.Code).ToList();
                        if (jobNos.Count > 0)
                        {
                            payables = payables.Where(x => jobNos.Contains(x.BillingNo));
                        }
                        else
                        {
                            payables = payables.Where(x => false);
                        }
                        break;
                }
            }

            var opsTransactions = opsTransactionRepository.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED).Select(x => new { x.JobNo, x.ServiceDate });
            var transactionServices = csTransactionRepository.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED && criteria.Service.Contains(x.TransactionType)).Select(x => new { x.JobNo, x.ServiceDate, x.TransactionType });

            if (criteria.FromDate != null)
            {
                if (criteria.DateType == "Accounting Date")
                {
                    payables = payables.Where(x => (criteria.FromDate.Value.Date <= x.VoucherDate.Value.Date && x.VoucherDate.Value.Date <= criteria.ToDate.Value.Date) ||
                            (criteria.FromDate.Value.Date <= x.InvoiceDate.Value.Date && x.InvoiceDate.Value.Date <= criteria.ToDate.Value.Date));
                }
                if (criteria.DateType == "Service Date")
                {
                    opsTransactions = opsTransactions.Where(x => criteria.FromDate.Value.Date <= x.ServiceDate.Value.Date && x.ServiceDate.Value.Date <= criteria.ToDate.Value.Date);
                    transactionServices = transactionServices.Where(x => criteria.FromDate.Value.Date <= x.ServiceDate.Value.Date && x.ServiceDate.Value.Date <= criteria.ToDate.Value.Date);
                }
                if (criteria.DateType == "Issued Date")
                {
                    var _billingNo = new List<string>();
                    var soas = soaRepository.Get(x => x.Status == AccountingConstants.STATUS_SOA_ISSUED_VOUCHER && criteria.FromDate.Value.Date <= x.DatetimeCreated.Value.Date && x.DatetimeCreated.Value.Date <= criteria.ToDate.Value.Date).Select(x => x.Soano);
                    _billingNo.AddRange(soas);
                    var cdNotes = cdNoteRepository.Get(x => x.Status == AccountingConstants.STATUS_SOA_ISSUED_VOUCHER && criteria.FromDate.Value.Date <= x.DatetimeCreated.Value.Date && x.DatetimeCreated.Value.Date <= criteria.ToDate.Value.Date).Select(x => x.Code);
                    _billingNo.AddRange(cdNotes);
                    var settlements = settlementPaymentRepository.Get(x => x.SyncStatus == AccountingConstants.STATUS_SYNCED
                        && !string.IsNullOrEmpty(x.VoucherNo) && criteria.FromDate.Value.Date <= x.DatetimeCreated.Value.Date && x.DatetimeCreated.Value.Date <= criteria.ToDate.Value.Date).Select(x => x.SettlementNo);
                    _billingNo.AddRange(settlements);
                    payables = payables.Where(x => _billingNo.Contains(x.BillingNo));
                    creditManagementAr = creditManagementAr.Where(x => _billingNo.Contains(x.Code));
                }
            }

            if (criteria.Service?.Count > 0)
            {
                var jobNos = new List<string>();
                if (criteria.Service.Contains("CL"))
                {
                    jobNos = opsTransactions.Select(x => x.JobNo).ToList();
                }
                if (criteria.Service.Any(x => x.Contains("I")) || criteria.Service.Any(x => x.Contains("E")))
                {
                    transactionServices = transactionServices.Where(x => criteria.Service.Contains(x.TransactionType));
                    jobNos.AddRange(transactionServices.Select(x => x.JobNo).ToList());
                }
                creditManagementAr = creditManagementAr.Where(x => jobNos.Any(z => z == x.JobNo));
            }

            var offices = officeRepository.Get();
            var result = from payable in payables
                         join credit in creditManagementAr on new { payable.VoucherNo, payable.ReferenceNo } equals new { credit.VoucherNo, credit.ReferenceNo }
                         join part in partner on payable.PartnerId equals part.Id
                         join office in offices on payable.OfficeId equals office.Id
                         select new AgencyDebitCreditModel
                         {
                             RefNo = payable.BillingNo,
                             VoucherId = payable.VoucherNo,
                             PartnerId = payable.PartnerId,
                             Type = credit.TransactionType,
                             InvoiceNo = payable.InvoiceNo,
                             InvoiceDate = payable.InvoiceDate,
                             PartnerName = part.ShortName,
                             TaxCode = part.TaxCode,
                             CurrencyId = credit.Currency,
                             Amount = credit.Currency == AccountingConstants.CURRENCY_LOCAL ? credit.AmountVnd : credit.AmountUsd,
                             UnpaidAmount = credit.Currency == AccountingConstants.CURRENCY_LOCAL ? credit.RemainVnd : credit.RemainUsd,
                             UnpaidAmountVnd = credit.RemainVnd,
                             UnpaidAmountUsd = credit.RemainUsd,
                             PaidAmount = credit.RemainUsd,
                             PaidAmountVnd = credit.RemainVnd,
                             PaidAmountUsd = credit.RemainUsd,
                             RemainAmount = 0,
                             RemainAmountUsd = 0,
                             RemainAmountVnd = 0,
                             PaymentTerm = payable.PaymentTerm,
                             DueDate = payable.PaymentDueDate,
                             PaymentStatus = credit.PaymentStatus,
                             OfficeId = payable.OfficeId,
                             OfficeName = office != null ? office.ShortName : null,
                             CompanyId = payable.CompanyId,
                             RefIds = new List<string> { payable.AcctManagementId },
                             Mbl = credit.Mblno,
                             Hbl = credit.Hblno,
                             JobNo = credit.JobNo,
                             Hblid = credit.Hblid,
                             ExchangeRateBilling = credit.ExchangeRate,
                             PaymentType = credit.TransactionType,
                             DepartmentId = GetDepartmentId(payable.BillingNo),
                             DepartmentName = GetDepartmentName(payable.BillingNo),
                             ReferenceNo = credit.ReferenceNo
                         };
            if (result == null || result.Count() == 0)
            {
                var newItem = new List<GroupShimentAgencyModel>();
                data.GroupShipmentsAgency = newItem.AsQueryable();
                return data;
            }
            var groupShipmentAgency = result.OrderBy(x => x.RefNo).Select(s => new GroupShimentAgencyModel
            {
                Hblid = s.Hblid,
                JobNo = s.JobNo,
                Mbl = s.Mbl,
                Hbl = s.Hbl,
                UnpaidAmountUsd = s.UnpaidAmountUsd ?? 0,
                UnpaidAmountVnd = s.UnpaidAmountVnd ?? 0,
                Invoices = new List<AgencyDebitCreditModel>() { s }
            }).AsQueryable();
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
            #region Delete old
            //Expression<Func<AcctDebitManagementAr, bool>> expQueryDebitAr = q => q.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;

            //if (!string.IsNullOrEmpty(criteria.PartnerId))
            //{
            //    List<string> childPartnerIds = catPartnerRepository.Get(x => x.ParentId == criteria.PartnerId)
            //            .Select(x => x.Id)
            //            .ToList();
            //    expQueryDebitAr = expQueryDebitAr.And(q => q.PartnerId == criteria.PartnerId || childPartnerIds.Contains(q.PartnerId));

            //    var acctManagementIds = acctMngtRepository.Get(x => x.PartnerId == criteria.PartnerId || childPartnerIds.Contains(criteria.PartnerId));

            //    if (criteria.Service != null && criteria.Service.Count > 0)
            //    {
            //        var ids = acctManagementIds.Where(x => criteria.Service.Contains(x.ServiceType)).Select(x => x.Id).ToList();
            //        if (acctManagementIds != null && ids.Count > 0)
            //        {
            //            expQueryDebitAr = expQueryDebitAr.And(x => ids.Contains(x.AcctManagementId ?? new Guid()));
            //        }
            //    }

            //    if (criteria.Office != null && criteria.Office.Count > 0)
            //    {
            //        var ids = acctManagementIds.Where(x => criteria.Office.Contains(x.OfficeId.ToString())).Select(x => x.Id).ToList();

            //        if (acctManagementIds != null && ids.Count > 0)
            //        {
            //            expQueryDebitAr = expQueryDebitAr.And(x => ids.Contains(x.AcctManagementId ?? new Guid()));
            //        }
            //    }
            //}

            //// var expQuery = InvoiceExpressionQuery(criteria, AccountingConstants.ACCOUNTING_INVOICE_TYPE);
            //var debitsAr = debitMngtArRepository.Get(expQueryDebitAr);
            //var surcharges = surchargeRepository.Get();
            //var partners = catPartnerRepository.Get();
            //var departments = departmentRepository.Get();
            //var offices = officeRepository.Get();
            //var invoice = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTANT_TYPE_INVOICE && x.PartnerId == criteria.PartnerId);

            //var query = from inv in debitsAr
            //            join sur in surcharges on inv.AcctManagementId equals sur.AcctManagementId
            //            where inv.Hblid == sur.Hblid
            //            select new { inv, sur };
            //if (criteria.ReferenceNos.Count > 0)
            //{
            //    switch (criteria.SearchType)
            //    {
            //        case "HBL":
            //            query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Hblno, StringComparer.OrdinalIgnoreCase));
            //            break;
            //        case "MBL":
            //            query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Mblno, StringComparer.OrdinalIgnoreCase));
            //            break;
            //        case "Job No":
            //            query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.JobNo, StringComparer.OrdinalIgnoreCase));
            //            break;
            //        case "Customs No":
            //            query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.ClearanceNo, StringComparer.OrdinalIgnoreCase));
            //            break;
            //        case "VAT Invoice":
            //            query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.InvoiceNo, StringComparer.OrdinalIgnoreCase));
            //            break;
            //        case "SOA":
            //            query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Soano, StringComparer.OrdinalIgnoreCase));
            //            break;
            //        case "Debit/Credit/Invoice":
            //            query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.DebitNo, StringComparer.OrdinalIgnoreCase)
            //            || criteria.ReferenceNos.Contains(x.sur.InvoiceNo, StringComparer.OrdinalIgnoreCase)
            //            || criteria.ReferenceNos.Contains(x.sur.Soano, StringComparer.OrdinalIgnoreCase));
            //            break;
            //        default:
            //            break;
            //    }
            //}

            //var data = query.Select(x => new AgencyDebitCreditModel
            //{
            //    RefNo = x.inv.RefNo,
            //    Type = AccountingConstants.ACCOUNTANT_TYPE_DEBIT,
            //    PartnerId = x.inv.PartnerId,
            //    RefIds = new List<string> { x.inv.AcctManagementId.ToString() },
            //    Hblid = x.inv.Hblid,
            //    JobNo = x.sur.JobNo,
            //    Mbl = x.sur.Mblno,
            //    Hbl = x.sur.Hblno,
            //    UnpaidAmount = x.inv.UnpaidAmount,
            //    UnpaidAmountUsd = x.inv.UnpaidAmountUsd,
            //    UnpaidAmountVnd = x.inv.UnpaidAmountVnd,
            //    PaymentStatus = x.inv.PaymentStatus
            //});
            #endregion
            var expQuery = InvoiceExpressionQuery(criteria, AccountingConstants.ACCOUNTING_INVOICE_TYPE);
            var invoice = expQuery.Apply(acctMngtRepository.Get())?.Take(100);
            var surcharges = surchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL && x.AcctManagementId != null && !string.IsNullOrEmpty(x.SyncedFrom));
            var offices = officeRepository.Get();
            var department = departmentRepository.Get();
            var query = from inv in invoice
                        join sur in surcharges on inv.Id equals sur.AcctManagementId
                        select new
                        {
                            inv,
                            sur.Hblid,
                            sur.JobNo,
                            sur.Hblno,
                            sur.Mblno,
                            sur.ClearanceNo,
                            sur.Soano,
                            sur.DebitNo,
                            sur.InvoiceNo,
                            sur.AmountUsd,
                            sur.VatAmountUsd,
                            sur.AmountVnd,
                            sur.VatAmountVnd,
                            sur.SyncedFrom,
                            sur.FinalExchangeRate,
                            sur.ReferenceNo
                        };

            if (criteria.ReferenceNos.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "HBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "MBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Job No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Customs No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.ClearanceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "VAT Invoice":
                        invoice = invoice.Where(x => criteria.ReferenceNos.Contains(x.InvoiceNoReal, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "SOA":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.Soano, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Debit/Credit/Invoice":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.DebitNo, StringComparer.OrdinalIgnoreCase)
                        || criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase)
                        || criteria.ReferenceNos.Contains(x.Soano, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }

            var partners = catPartnerRepository.Get(x => x.Active == true);
            List<string> childPartnerIds = new List<string>();
            if (criteria.IsCombineReceipt)
            {
                var currentPartner = partners.Where(x => x.Id == criteria.PartnerId).FirstOrDefault();
                childPartnerIds = partners.Where(x => x.ParentId == currentPartner.ParentId)
                            .Select(x => x.Id)
                            .ToList();
                partners = partners.Where(x => x.Id == criteria.PartnerId || childPartnerIds.Contains(x.ParentId));
            }
            else
            {
                partners = partners.Where(x => x.Active == true && (x.Id == criteria.PartnerId || x.ParentId == criteria.PartnerId));
                childPartnerIds = partners.Where(x => x.ParentId == criteria.PartnerId)
                            .Select(x => x.Id)
                            .ToList();
            }

            var debitsAr = debitMngtArRepository.Get(q => q.PartnerId == criteria.PartnerId || childPartnerIds.Contains(q.PartnerId));
            if (criteria.Service != null && criteria.Service.Count > 0)
            {
                query = query.Where(x => criteria.Service.Contains(x.inv.ServiceType, StringComparer.OrdinalIgnoreCase));
            }

            if (query.FirstOrDefault() == null)
                return null;

            var data = from acct in query
                       join inv in debitsAr on new { AcctManagementId = acct.inv.Id, acct.Hblid } equals new { AcctManagementId = (inv.AcctManagementId ?? Guid.Empty), Hblid = (inv.Hblid ?? Guid.Empty) } into debitGrp
                       from inv in debitGrp.DefaultIfEmpty()
                       select new
                       {
                           acct,
                           RefNo = inv == null ? (acct.SyncedFrom.Contains("SOA") ? acct.Soano : acct.DebitNo) : inv.RefNo,
                           Type = AccountingConstants.ACCOUNTANT_TYPE_DEBIT,
                           //PartnerId = acct.PartnerId,
                           //RefIds = new List<string> { acct.Id.ToString() },
                           Hblid = acct.Hblid,
                           JobNo = acct.JobNo,
                           Mbl = acct.Mblno,
                           Hbl = acct.Hblno,
                           UnpaidAmount = inv != null ? inv.UnpaidAmount : (acct.inv.Currency == AccountingConstants.CURRENCY_LOCAL ? (acct.AmountVnd + acct.VatAmountVnd) : (acct.AmountUsd + acct.VatAmountUsd)),
                           UnpaidAmountUsd = inv != null ? inv.UnpaidAmountUsd : (acct.AmountUsd + acct.VatAmountUsd),
                           UnpaidAmountVnd = inv != null ? inv.UnpaidAmountVnd : (acct.AmountVnd + acct.VatAmountVnd),
                           //PaidAmount = inv != null ? inv.PaidAmount : (acct.AmountUsd + acct.VatAmountUsd),
                           //PaidAmountUsd = inv != null ? inv.PaidAmountUsd : (acct.AmountUsd + acct.VatAmountUsd),
                           //PaidAmountVnd = inv != null ? inv.PaidAmountVnd : (acct.AmountVnd + acct.VatAmountVnd),
                           //RemainAmount = inv != null ? (inv.UnpaidAmountUsd ?? 0) : (acct.AmountUsd + acct.VatAmountUsd),
                           //RemainAmountUsd = inv != null ? (inv.UnpaidAmountUsd ?? 0) : (acct.AmountUsd + acct.VatAmountUsd),
                           //RemainAmountVnd = inv != null ? (inv.UnpaidAmountVnd ?? 0) : (acct.AmountVnd + acct.VatAmountVnd),
                           PaymentStatus = inv != null ? inv.PaymentStatus : acct.inv.PaymentStatus,
                           DebitStatus = inv != null ? inv.PaymentStatus : AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID,
                           //InvoiceNo = acct.InvoiceNoReal,
                           //InvoiceDate = acct.Date,
                           //CurrencyId = acct.Currency,
                           //Amount = acct.TotalAmount,
                           //PaymentTerm = acct.PaymentTerm,
                           //DueDate = acct.PaymentDueDate,
                           //OfficeId = acct.OfficeId,
                           //CompanyId = acct.CompanyId,
                           ExchangeRateBilling = acct.inv.Currency != AccountingConstants.CURRENCY_LOCAL ? acct.FinalExchangeRate : GetExchangeRateDebitBilling(acct.Soano, acct.DebitNo),
                           isExistDebitAR = inv != null ? true : false,
                           ReferenceNo = acct.ReferenceNo

                       };
            data = data.Where(x => x.DebitStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
            var groupData = data.GroupBy(x => new { x.RefNo, x.Type, x.acct.inv.PartnerId, x.Hblid });
            var result = groupData.Select(inv => new AgencyDebitCreditModel
            {
                RefNo = inv.Key.RefNo,
                Type = inv.Key.Type,
                PartnerId = inv.Key.PartnerId,
                RefIds = new List<string> { inv.FirstOrDefault().acct.inv.Id.ToString() },
                Hblid = inv.Key.Hblid,
                JobNo = inv.FirstOrDefault().JobNo,
                Mbl = inv.FirstOrDefault().Mbl,
                Hbl = inv.FirstOrDefault().Hbl,
                UnpaidAmount = inv.FirstOrDefault().isExistDebitAR == true ? inv.FirstOrDefault().UnpaidAmount : inv.Sum(x => x.UnpaidAmount),
                UnpaidAmountUsd = inv.FirstOrDefault().isExistDebitAR == true ? inv.FirstOrDefault().UnpaidAmountUsd : inv.Sum(x => x.UnpaidAmountUsd),
                UnpaidAmountVnd = inv.FirstOrDefault().isExistDebitAR == true ? inv.FirstOrDefault().UnpaidAmountVnd : inv.Sum(x => x.UnpaidAmountVnd),
                PaidAmount = inv.FirstOrDefault().isExistDebitAR == true ? inv.FirstOrDefault().UnpaidAmount : inv.Sum(x => x.UnpaidAmount),
                PaidAmountUsd = inv.FirstOrDefault().isExistDebitAR == true ? inv.FirstOrDefault().UnpaidAmountUsd : inv.Sum(x => x.UnpaidAmountUsd),
                PaidAmountVnd = inv.FirstOrDefault().isExistDebitAR == true ? inv.FirstOrDefault().UnpaidAmountVnd : inv.Sum(x => x.UnpaidAmountVnd),
                //PaidAmount = inv.FirstOrDefault().isExistDebitAR == true ? inv.FirstOrDefault().RemainAmount : inv.Sum(x => x.RemainAmount),
                //PaidAmountUsd = inv.FirstOrDefault().isExistDebitAR == true ? inv.FirstOrDefault().RemainAmountUsd : inv.Sum(x => x.RemainAmountUsd),
                //PaidAmountVnd = inv.FirstOrDefault().isExistDebitAR == true ? inv.FirstOrDefault().RemainAmountVnd : inv.Sum(x => x.RemainAmountVnd),
                PaymentStatus = inv.FirstOrDefault().PaymentStatus,
                InvoiceNo = inv.FirstOrDefault().acct.inv.InvoiceNoReal,
                InvoiceDate = inv.FirstOrDefault().acct.inv.Date,
                CurrencyId = inv.FirstOrDefault().acct.inv.Currency,
                Amount = inv.FirstOrDefault().acct.inv.TotalAmount,
                PaymentTerm = inv.FirstOrDefault().acct.inv.PaymentTerm,
                DueDate = inv.FirstOrDefault().acct.inv.PaymentDueDate,
                OfficeId = inv.FirstOrDefault().acct.inv.OfficeId,
                CompanyId = inv.FirstOrDefault().acct.inv.CompanyId,
                ExchangeRateBilling = inv.FirstOrDefault().ExchangeRateBilling,
                ReferenceNo = inv.FirstOrDefault().ReferenceNo,
                DepartmentId = GetDepartmentId(inv.Key.RefNo)
            }).ToList();

            var joinData = from d in result
                           join par in partners on d.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
                           join ofi in offices on d.OfficeId equals ofi.Id into ofiGrp
                           from ofi in ofiGrp.DefaultIfEmpty()
                           join dept in department on d.DepartmentId equals dept.Id into deptGrp
                           from dept in deptGrp.DefaultIfEmpty()
                           select new AgencyDebitCreditModel
                           {
                               RefNo = d.RefNo,
                               PartnerId = d.PartnerId,
                               Type = d.Type,
                               InvoiceNo = d.InvoiceNo,
                               InvoiceDate = d.InvoiceDate,
                               PartnerName = par.ShortName,
                               TaxCode = par.TaxCode,
                               CurrencyId = d.CurrencyId,
                               Amount = d.Amount,
                               UnpaidAmount = d.UnpaidAmount,
                               UnpaidAmountVnd = d.UnpaidAmountVnd,
                               UnpaidAmountUsd = d.UnpaidAmountUsd,
                               PaidAmount = d.PaidAmount,
                               PaidAmountVnd = d.PaidAmountVnd,
                               PaidAmountUsd = d.PaidAmountUsd,
                               //RemainAmount = d.UnpaidAmount - d.PaidAmount,
                               //RemainAmountVnd = d.UnpaidAmountVnd - d.PaidAmountVnd,
                               //RemainAmountUsd = d.UnpaidAmountUsd - d.PaidAmountUsd,
                               PaymentTerm = d.PaymentTerm,
                               DueDate = d.DueDate,
                               PaymentStatus = d.PaymentStatus,
                               OfficeId = d.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               //CompanyId = d.CompanyId,
                               RefIds = d.RefIds,
                               Mbl = d.Mbl,
                               Hbl = d.Hbl,
                               JobNo = d.JobNo,
                               Hblid = d.Hblid,
                               ExchangeRateBilling = d.ExchangeRateBilling,
                               PaymentType = d.Type,
                               DepartmentId = d.DepartmentId,
                               DepartmentName = dept != null ? dept.DeptNameAbbr : null,
                               ReferenceNo = d.ReferenceNo
                           };
            return joinData.AsQueryable();
        }

        /// <summary>
        /// Get OBH Invoice Agency
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IQueryable<AgencyDebitCreditModel> GetObhForIssueAgencyPayment(CustomerDebitCreditCriteria criteria)
        {
            var expQuery = InvoiceExpressionQuery(criteria, AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);
            var invoiceTemps = expQuery.Apply(acctMngtRepository.Get());
            if (invoiceTemps.FirstOrDefault() == null)
            {
                return null;
            }
            var surcharges = surchargeRepository.Get(x => x.AcctManagementId != null && x.Type == AccountingConstants.TYPE_CHARGE_OBH && (!string.IsNullOrEmpty(x.SyncedFrom) || !string.IsNullOrEmpty(x.PaySyncedFrom)));
            var query = from inv in invoiceTemps
                        join sur in surcharges on inv.Id equals sur.AcctManagementId
                        select new
                        {
                            inv,
                            sur.Hblid,
                            sur.JobNo,
                            sur.Hblno,
                            sur.Mblno,
                            sur.ClearanceNo,
                            sur.Soano,
                            sur.DebitNo,
                            sur.InvoiceNo,
                            sur.AmountUsd,
                            sur.VatAmountUsd,
                            sur.AmountVnd,
                            sur.VatAmountVnd,
                            sur.SyncedFrom,
                            sur.FinalExchangeRate,
                            sur.ReferenceNo
                        };

            if (criteria.ReferenceNos.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "HBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "MBL":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Job No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Customs No":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.ClearanceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "VAT Invoice":
                        break;
                    case "SOA":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.Soano, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Debit/Credit/Invoice":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.DebitNo, StringComparer.OrdinalIgnoreCase)
                        || criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase)
                        || criteria.ReferenceNos.Contains(x.Soano, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }

            var partners = catPartnerRepository.Get(x => x.Active == true);
            List<string> childPartnerIds = new List<string>();
            if (criteria.IsCombineReceipt)
            {
                var currentPartner = partners.FirstOrDefault(x => x.Id == criteria.PartnerId);
                childPartnerIds = partners.Where(x => x.ParentId == currentPartner.ParentId)
                            .Select(x => x.Id)
                            .ToList();
                partners = partners.Where(x => x.Id == criteria.PartnerId || childPartnerIds.Contains(x.ParentId));
            }
            else
            {
                partners = partners.Where(x => x.Active == true && (x.Id == criteria.PartnerId || x.ParentId == criteria.PartnerId));
                childPartnerIds = partners.Where(x => x.ParentId == criteria.PartnerId)
                            .Select(x => x.Id)
                            .ToList();
            }
            var debitsAr = debitMngtArRepository.Get(q => q.PartnerId == criteria.PartnerId || childPartnerIds.Contains(q.PartnerId));
            if (query.FirstOrDefault() == null)
                return null;

            var dataOBH = from acct in query
                          join inv in debitsAr on new { AcctManagementId = acct.inv.Id, acct.Hblid } equals new { AcctManagementId = (inv.AcctManagementId ?? Guid.Empty), Hblid = (inv.Hblid ?? Guid.Empty) } into debitGrp
                          from inv in debitGrp.DefaultIfEmpty()
                          select new
                          {
                              acct,
                              RefNo = inv == null ? (acct.SyncedFrom.Contains("SOA") ? acct.Soano : acct.DebitNo) : inv.RefNo,
                              Type = AccountingConstants.ACCOUNTANT_TYPE_DEBIT,
                              UnpaidAmount = inv != null ? inv.UnpaidAmount : (acct.inv.Currency == AccountingConstants.CURRENCY_LOCAL ? (acct.AmountVnd + acct.VatAmountVnd) : (acct.AmountUsd + acct.VatAmountUsd)),
                              UnpaidAmountUsd = inv != null ? inv.UnpaidAmountUsd : (acct.AmountUsd + acct.VatAmountUsd),
                              UnpaidAmountVnd = inv != null ? inv.UnpaidAmountVnd : (acct.AmountVnd + acct.VatAmountVnd),
                              PaidAmount = inv != null ? inv.PaidAmountUsd : (acct.AmountUsd + acct.VatAmountUsd),
                              PaidAmountUsd = inv != null ? inv.PaidAmountUsd : (acct.AmountUsd + acct.VatAmountUsd),
                              PaidAmountVnd = inv != null ? inv.PaidAmountVnd : (acct.AmountUsd + acct.VatAmountUsd),
                              //RemainAmount = inv != null ? (inv.UnpaidAmountUsd - (inv.PaidAmountUsd ?? 0)) : (acct.AmountUsd + acct.VatAmountUsd),
                              //RemainAmountUsd = inv != null ? (inv.UnpaidAmountUsd - (inv.PaidAmountUsd ?? 0)) : (acct.AmountUsd + acct.VatAmountUsd),
                              //RemainAmountVnd = inv != null ? (inv.UnpaidAmountVnd - (inv.PaidAmountVnd ?? 0)) : (acct.AmountVnd + acct.VatAmountVnd),
                              PaymentStatus = inv != null ? inv.PaymentStatus : acct.inv.PaymentStatus,
                              OBHStatus = inv != null ? inv.PaymentStatus : AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID,
                              ExchangeRateBilling = GetExchangeRateDebitBilling(acct.Soano, acct.DebitNo),
                              isExistDebitAR = inv != null ? true : false,
                              ReferenceNo = acct.ReferenceNo
                          };
            dataOBH = dataOBH.Where(x => x.OBHStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);
            //(g.acct.sur.SyncedFrom == "CDNOTE" ? g.acct.sur.DebitNo : (g.acct.sur.SyncedFrom == "SOA" ? g.acct.sur.Soano : null))
            var grpInvoiceCharge = dataOBH.OrderBy(x => x.RefNo).GroupBy(g => new { g.acct.inv.PartnerId, RefNo = g.RefNo, g.acct.JobNo, g.acct.Mblno, g.acct.Hblno, g.acct.Hblid })
                .Select(s => new
                {
                    s.Key.PartnerId,
                    s.Key.RefNo,
                    Invoice = s.Select(se => se.acct.inv),
                    Job = s.Key,
                    s.FirstOrDefault().isExistDebitAR
                ,
                    total = s.Select(t => new { t.PaidAmount, t.PaidAmountVnd, t.PaidAmountUsd })
                });
            var data = grpInvoiceCharge.Select(se => new AgencyDebitCreditModel
            {
                RefNo = se.RefNo,
                Type = "OBH",
                InvoiceNo = string.Empty,
                InvoiceDate = se.Invoice.FirstOrDefault().Date,
                PartnerId = se.PartnerId,
                CurrencyId = se.Invoice.FirstOrDefault().Currency,
                Amount = se.Invoice.Sum(su => su.TotalAmount),
                UnpaidAmount = se.Invoice.Sum(su => su.UnpaidAmount),
                UnpaidAmountVnd = se.Invoice.Sum(su => su.UnpaidAmountVnd),
                UnpaidAmountUsd = se.Invoice.Sum(su => su.UnpaidAmountUsd),
                PaidAmount = se.total.Sum(su => su.PaidAmount),
                PaidAmountUsd = se.total.Sum(su => su.PaidAmountUsd),
                PaidAmountVnd = se.total.Sum(su => su.PaidAmountVnd),
                PaymentTerm = se.Invoice.FirstOrDefault().PaymentTerm,
                DueDate = se.Invoice.FirstOrDefault().PaymentDueDate,
                PaymentStatus = GetPaymentSatusOBH(se.Invoice),
                OfficeId = se.Invoice.FirstOrDefault().OfficeId,
                CompanyId = se.Invoice.FirstOrDefault().CompanyId,
                RefIds = se.Invoice.Select(s => s.Id.ToString()).Distinct().ToList(),
                JobNo = se.Job.JobNo,
                Mbl = se.Job.Mblno,
                Hbl = se.Job.Hblno,
                Hblid = se.Job.Hblid,
                ExchangeRateBilling = GetExchangeRateDebitOBHBilling(se.RefNo),
                ReferenceNo = se.Invoice.FirstOrDefault().ReferenceNo,
                DepartmentId = GetDepartmentId(se.RefNo)
            }).ToList();

            var offices = officeRepository.Get();
            var department = departmentRepository.Get();
            var joinData = from inv in data
                           join par in partners on inv.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
                           join ofi in offices on inv.OfficeId equals ofi.Id into ofiGrp
                           from ofi in ofiGrp.DefaultIfEmpty()
                           join dept in department on inv.DepartmentId equals dept.Id into deptGrp
                           from dept in deptGrp.DefaultIfEmpty()
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
                               UnpaidAmount = inv.UnpaidAmount,
                               UnpaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmountUsd = inv.UnpaidAmountUsd,
                               PaidAmount = inv.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID ? inv.UnpaidAmount : inv.PaidAmount,
                               PaidAmountVnd = inv.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID ? inv.UnpaidAmountVnd : inv.PaidAmountVnd,
                               PaidAmountUsd = inv.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID ? inv.UnpaidAmountUsd : inv.PaidAmountUsd,
                               //RemainAmount = inv.UnpaidAmount - inv.PaidAmount,
                               //RemainAmountVnd = inv.UnpaidAmountVnd - inv.PaidAmountVnd,
                               //RemainAmountUsd = inv.UnpaidAmountUsd - inv.PaidAmountUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId,
                               RefIds = inv.RefIds,
                               JobNo = inv.JobNo,
                               Hbl = inv.Hbl,
                               Mbl = inv.Mbl,
                               Hblid = inv.Hblid,
                               ExchangeRateBilling = inv.ExchangeRateBilling,
                               PaymentType = inv.Type,
                               DepartmentId = inv.DepartmentId,
                               DepartmentName = dept != null ? dept.DeptNameAbbr : null,
                               //DepartmentId = GetDepartmentId(inv.RefNo),
                               //DepartmentName = GetDepartmentName(inv.RefNo),
                               ReferenceNo = inv.ReferenceNo
                           };
            return joinData.AsQueryable();
        }

        private CriteriaBuilder<AccAccountingManagement> InvoiceExpressionQuery(CustomerDebitCreditCriteria criteria, string type)
        {
            //Get Vat Invoice có Payment Status # Paid
            // Ds các đối tượng con
            List<string> childPartnerIds = new List<string>();
            if (criteria.IsCombineReceipt)
            {
                var currentPartner = catPartnerRepository.Get(x => x.Id == criteria.PartnerId).FirstOrDefault();
                childPartnerIds = catPartnerRepository.Get(x => x.ParentId == currentPartner.ParentId)
                        .Select(x => x.Id)
                        .ToList();
            }
            else
            {
                childPartnerIds = catPartnerRepository.Get(x => x.ParentId == criteria.PartnerId)
                     .Select(x => x.Id)
                        .ToList();
            }
            var criteriaBuilder = new CriteriaBuilder<AccAccountingManagement>()
                .Where(q => q.Type == type && q.PaymentStatus != "Paid")
              .WhereIf(!string.IsNullOrEmpty(criteria.PartnerId),
                      q => q.PartnerId == criteria.PartnerId || childPartnerIds.Contains(q.PartnerId))
              .WhereIf(criteria.FromDate != null && criteria.ToDate != null,
                      q => !string.IsNullOrEmpty(criteria.DateType) ||
                      (criteria.DateType == "Invoice Date" && q.Date.Value.Date >= criteria.FromDate.Value.Date && q.Date.Value.Date <= criteria.ToDate.Value.Date) ||
                      (criteria.DateType == "Billing Date" && q.ConfirmBillingDate.Value.Date >= criteria.FromDate.Value.Date && q.ConfirmBillingDate.Value.Date <= criteria.ToDate.Value.Date))
              .WhereIf(criteria.FromDate == null,
                      q => q.Date.Value.Date <= DateTime.Now.Date)
              .WhereIf((criteria.Office != null && criteria.Office.Count > 0),
                      q => criteria.Office.Contains(q.OfficeId.ToString()));

            #region Delete old
            //Expression<Func<AccAccountingManagement, bool>> query = q => q.Type == type && q.PaymentStatus != "Paid";
            //if (!string.IsNullOrEmpty(criteria.PartnerId))
            //{
            //    // Ds các đối tượng con
            //    //List<string> childPartnerIds = catPartnerRepository.Get(x => x.ParentId == criteria.PartnerId)
            //    //        .Select(x => x.Id)
            //    //        .ToList();
            //    query = query.And(q => q.PartnerId == criteria.PartnerId || childPartnerIds.Contains(q.PartnerId));
            //}

            //if (criteria.FromDate != null && criteria.ToDate != null)
            //{
            //    if (!string.IsNullOrEmpty(criteria.DateType))
            //    {
            //        switch (criteria.DateType)
            //        {
            //            case "Invoice Date":
            //                query = query = query.And(x => x.Date.Value.Date >= criteria.FromDate.Value.Date && x.Date.Value.Date <= criteria.ToDate.Value.Date);
            //                break;
            //            case "Billing Date":
            //                query = query.And(x => x.ConfirmBillingDate.Value.Date >= criteria.FromDate.Value.Date && x.ConfirmBillingDate.Value.Date <= criteria.ToDate.Value.Date);
            //                break;
            //            case "Service Date":
            //                break;
            //            default:
            //                break;
            //        }
            //    }
            //} else
            //{
            //    query = query = query.And(x => x.Date.Value.Date <= DateTime.Now.Date);
            //}

            //if (criteria.Office != null && criteria.Office.Count > 0)
            //{
            //    query = query.And(x => criteria.Office.Contains(x.OfficeId.ToString()));
            //}
            #endregion
            return criteriaBuilder;
        }

        private bool IsMatchService(string invoiceService, List<string> serviceTerm)
        {
            bool isMatch = true;

            isMatch = invoiceService.Split(';').Intersect(serviceTerm).Any();
            if (!string.IsNullOrEmpty(invoiceService))
            {
                var serviceList = invoiceService.Split(";").ToList();
                if (serviceList.Count > 0)
                {
                    foreach (string item in serviceList)
                    {
                        if(serviceTerm.Contains(item))
                        {
                            return true;
                        }
                        isMatch = serviceTerm.Contains(item);
                    }
                    
                }
            }
            
            return isMatch;

        }

        private Expression<Func<AcctCreditManagementAr, bool>> CreditARExpressionQuery(CustomerDebitCreditCriteria criteria)
        {
            Expression<Func<AcctCreditManagementAr, bool>> expQueryCreditAR = q => q.NetOff == false;

            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                List<string> childPartnerIds = catPartnerRepository.Get(x => x.ParentId == criteria.PartnerId)
                        .Select(x => x.Id)
                        .ToList();
                expQueryCreditAR = expQueryCreditAR.And(q => q.PartnerId == criteria.PartnerId || childPartnerIds.Contains(q.PartnerId));

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
                        if (criteria.Service != null && criteria.Service.Count > 0)
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

            if (criteria.Service != null && criteria.Service.Count > 0)
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
            var invoices = expQuery.Apply(acctMngtRepository.Get());

            if (invoices.Count() == 0)
            {
                return null;
            }
            var surcharges = surchargeRepository.Get();
           
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
                    case "VAT Invoice":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.inv.InvoiceNoReal, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "SOA":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Soano, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Debit/Credit/Invoice":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.DebitNo, StringComparer.OrdinalIgnoreCase)
                        || criteria.ReferenceNos.Contains(x.sur.InvoiceNo, StringComparer.OrdinalIgnoreCase)
                        || criteria.ReferenceNos.Contains(x.sur.Soano, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }

            if (query == null || query.Count() == 0)
            {
                return null;
            }
            var grpInvoiceCharge = query.GroupBy(g => g.inv).Select(s => new { Invoice = s.Key, Surcharge = s.Select(se => se.sur) });
            var data = grpInvoiceCharge.Select(se => new CustomerDebitCreditModel
            {
                //RefNo = se.Soa_DebitNo.Any(w => !string.IsNullOrEmpty(w.Soano))
                //? se.Soa_DebitNo.Where(w => !string.IsNullOrEmpty(w.Soano)).Select(s => s.Soano).FirstOrDefault()
                //: se.Soa_DebitNo.Where(w => !string.IsNullOrEmpty(w.DebitNo)).Select(s => s.DebitNo).FirstOrDefault(),
                RefNo = se.Surcharge.Any(w => !string.IsNullOrEmpty(w.Soano))
                ? se.Surcharge.Where(w => !string.IsNullOrEmpty(w.Soano)).Select(s => s.Soano).FirstOrDefault()
                : se.Surcharge.Where(w => !string.IsNullOrEmpty(w.DebitNo)).Select(s => s.DebitNo).FirstOrDefault(),
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
                OfficeId = se.Invoice.OfficeId,
                CompanyId = se.Invoice.CompanyId,
                RefIds = new List<string> { se.Invoice.Id.ToString() },
                ExchangeRateBilling = GetExchangeRateDebitBilling(se.Surcharge.FirstOrDefault().Soano, se.Surcharge.FirstOrDefault().DebitNo)
            });

            var partners = catPartnerRepository.Get();
            var departments = departmentRepository.Get();
            var offices = officeRepository.Get();
            var joinData = from inv in data
                           join par in partners on inv.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
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
                               UnpaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmountUsd = inv.UnpaidAmountUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId,
                               RefIds = inv.RefIds,
                               ExchangeRateBilling = inv.ExchangeRateBilling,
                               PaymentType = inv.Type,
                               DepartmentId = GetDepartmentId(inv.RefNo),
                               DepartmentName = GetDepartmentName(inv.RefNo),
                           };

            return joinData;
        }

        private decimal? GetExchangeRateDebitBilling(string _soaNo, string _debitNo)
        {
            decimal? exchangeRate = null;
            //var data = soa_debit.ToList().First();
            //string _soaNo = ObjectHelper.GetValueBy(data, "Soano");
            //string _debitNo = ObjectHelper.GetValueBy(data, "DebitNo");
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
            var invoiceTemps = expQuery.Apply(acctMngtRepository.Get());

            if (invoiceTemps.Count() == 0)
            {
                return null;
            }
            var surcharges = surchargeRepository.Get();     
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
                    case "VAT Invoice":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.inv.InvoiceNoReal, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "SOA":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.Soano, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Debit/Credit/Invoice":
                        query = query.Where(x => criteria.ReferenceNos.Contains(x.sur.DebitNo, StringComparer.OrdinalIgnoreCase)
                        || criteria.ReferenceNos.Contains(x.sur.InvoiceNo, StringComparer.OrdinalIgnoreCase)
                        || criteria.ReferenceNos.Contains(x.sur.Soano, StringComparer.OrdinalIgnoreCase));
                        break;
                    default:
                        break;
                }
            }

            if (query == null || query.Count() == 0)
            {
                return null;
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
                PaymentStatus = GetPaymentSatusOBH(se.Invoice),
                OfficeId = se.Invoice.Select(s => s.OfficeId).FirstOrDefault(),
                CompanyId = se.Invoice.Select(s => s.CompanyId).FirstOrDefault(),
                RefIds = se.Invoice.Select(s => s.Id.ToString()).Distinct().ToList(),
                ExchangeRateBilling = GetExchangeRateDebitOBHBilling(se.RefNo)
            });

            var partners = catPartnerRepository.Get();
            var departments = departmentRepository.Get();
            var offices = officeRepository.Get();

            var joinData = from inv in data
                           join par in partners on inv.PartnerId equals par.Id into parGrp
                           from par in parGrp.DefaultIfEmpty()
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
                               UnpaidAmountVnd = inv.UnpaidAmountVnd,
                               UnpaidAmountUsd = inv.UnpaidAmountUsd,
                               PaymentTerm = inv.PaymentTerm,
                               DueDate = inv.DueDate,
                               PaymentStatus = inv.PaymentStatus,
                               OfficeId = inv.OfficeId,
                               OfficeName = ofi != null ? ofi.ShortName : null,
                               CompanyId = inv.CompanyId,
                               RefIds = inv.RefIds,
                               ExchangeRateBilling = inv.ExchangeRateBilling,
                               PaymentType = inv.Type,
                               DepartmentId = GetDepartmentId(inv.RefNo),
                               DepartmentName = GetDepartmentName(inv.RefNo)
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

        public List<ObjectReceivableModel> GetListReceivableReceipt(Guid receiptId)
        {
            var payments = acctPaymentRepository.Get(x => x.ReceiptId == receiptId);
            var invoiceIds = payments.Where(x => x.Type == "DEBIT" || x.Type == "OBH").Select(s => Guid.Parse(s.RefId)).Distinct().ToList();
            var soaIds = payments.Where(x => x.Type == "CREDITSOA").Select(s => s.RefId).Distinct().ToList();
            var paySoaNos = new List<string>();
            if (soaIds.Count > 0)
            {
                paySoaNos = soaRepository.Get(x => soaIds.Any(s => s == x.Id)).Select(s => s.Soano).Distinct().ToList();
            }
            IQueryable<CsShipmentSurcharge> surcharges = null;
            if (invoiceIds.Count > 0 || paySoaNos.Count > 0)
            {
                Expression<Func<CsShipmentSurcharge, bool>> query = chg => false;

                if (invoiceIds.Count > 0)
                {
                    query = query.Or(x => invoiceIds.Any(i => i == x.AcctManagementId));
                }
               
                if (paySoaNos.Count > 0)
                {
                    query = query.Or(x => paySoaNos.Any(p => p == x.PaySoano));
                }
                surcharges = surchargeRepository.Get(query);
            }
            var hs = new HandleState();
            if (surcharges == null) return new List<ObjectReceivableModel>();

            var objectReceivablesModel = accAccountReceivableService.GetObjectReceivableBySurcharges(surcharges);

            return objectReceivablesModel;
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

            deptId = GetDepartmentId(refNo);
            if (deptId != 0)
            {
                CatDepartment department = departmentRepository.Get(x => x.Id == deptId).FirstOrDefault();

                dept = department?.DeptNameAbbr;

            }
            return dept;
        }

        private int? GetDepartmentId(string refNo)
        {
            int? deptId = 0;
            if (string.IsNullOrEmpty(refNo))
            {
                return deptId;
            }
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
                else // get department from settlement payment
                {
                    AcctSettlementPayment settle = settlementPaymentRepository.Get(x => x.SettlementNo == refNo).FirstOrDefault();
                    deptId = settle != null ? settle.DepartmentId : deptId;
                }
            }

            return deptId;
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
                var emailSettingDept = emailSettingRepository.Get(x => x.DeptId == Id);
                if (emailSettingDept.FirstOrDefault() == null)
                {
                    if (!string.IsNullOrEmpty(dept.Email))
                    {
                        toEmails = dept.Email.Split(";").ToList();
                    }
                }

                var emailAlertSettng = emailSettingDept.Where(x => x.EmailType == AccountingConstants.EMAIL_SETTING_AR_ALERT).FirstOrDefault();
                if (emailAlertSettng != null && !string.IsNullOrEmpty(emailAlertSettng.EmailInfo))
                {
                    toEmails = emailAlertSettng.EmailInfo.Split(";").ToList();
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

        public async Task<AcctReceiptAdvanceModelExport> GetDataExportReceiptAdvance(AcctReceiptCriteria criteria)
        {
            List<string> methodsAdv = new List<string> {
                AccountingConstants.PAYMENT_METHOD_CLEAR_ADVANCE,
                AccountingConstants.PAYMENT_METHOD_CLEAR_ADVANCE_BANK,
                AccountingConstants.PAYMENT_METHOD_CLEAR_ADVANCE_CASH,
                AccountingConstants.PAYMENT_METHOD_COLL_INTERNAL,
            };
            Expression<Func<AcctReceipt, bool>> query = (x =>
               (x.CustomerId ?? "").IndexOf(criteria.CustomerID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
               && x.Status == AccountingConstants.RECEIPT_STATUS_DONE
              );
            if (!string.IsNullOrEmpty(criteria.DateType) && criteria.DateType == "Paid Date" && criteria.DateFrom.HasValue && criteria.DateTo.HasValue)
            {
                query = query.And(x => x.PaymentDate.Value.Date >= criteria.DateFrom.Value.Date && x.PaymentDate.Value.Date <= criteria.DateTo.Value.Date);
            }

            var receiptExpre = await DataContext.GetAsync(query);

            var receiptWithPaymentMethod = receiptExpre.Where(x => methodsAdv.Contains(x.PaymentMethod));
            List<Guid> receiptWithoutPaymentMethodIds = receiptExpre.Where(x => !methodsAdv.Contains(x.PaymentMethod)).Select(x => x.Id).ToList();

            List<Guid?> queryPayment = acctPaymentRepository.Get(x => receiptWithoutPaymentMethodIds.Contains(x.ReceiptId ?? Guid.Empty)
            && (x.Type == AccountingConstants.PAYMENT_TYPE_CODE_ADVANCE || x.Type == AccountingConstants.PAYMENT_TYPE_CODE_COLLECT_OBH))
            .Select(x => x.ReceiptId)
            .Distinct()
            .ToList();

            var receiptWithPayment = receiptExpre.Where(x => queryPayment.Contains(x.Id));

            var receipts = receiptWithPaymentMethod.Union(receiptWithPayment);

            if (receipts.Count() == 0)
            {
                return null;
            }
            AcctReceiptAdvanceModelExport result = new AcctReceiptAdvanceModelExport();

            CatPartner partner = catPartnerRepository.Get(x => x.Id.ToString() == criteria.CustomerID)?.FirstOrDefault();
            if (partner == null)
            {
                return null;
            }

            result.TaxCode = partner.TaxCode;
            result.PartnerNameVn = partner.PartnerNameVn;
            result.PartnerNameEn = partner.PartnerNameEn;
            result.UserExport = currentUser.UserName;

            result.Details = receipts.AsQueryable()
                .OrderBy(x => x.PaymentDate)
                .ThenBy(x => x.DatetimeCreated)
                .Select(receipt => new AcctReceiptAdvanceRowModel
                {
                    Description = receipt.Description,
                    ReceiptNo = receipt.PaymentRefNo,
                    PaidDate = receipt.PaymentDate,
                    CusAdvanceAmountVnd = receipt.PaymentMethod == AccountingConstants.PAYMENT_METHOD_COLL_INTERNAL ? (receipt.PaidAmountVnd ?? 0) : (receipt.CusAdvanceAmountVnd ?? 0), // Trừ ứng trước
                    CusAdvanceAmountUsd = receipt.PaymentMethod == AccountingConstants.PAYMENT_METHOD_COLL_INTERNAL ? (receipt.PaidAmountUsd ?? 0) : (receipt.CusAdvanceAmountUsd ?? 0),
                    AgreementCusAdvanceUsd = receipt.AgreementAdvanceAmountUsd ?? 0, // Số dư ứng trước
                    AgreementCusAdvanceVnd = receipt.AgreementAdvanceAmountVnd ?? 0,
                    TotalAdvancePaymentUsd = GetTotalAdvancePayment(receipt.Id, AccountingConstants.CURRENCY_USD), // tổng tiền ứng trước
                    TotalAdvancePaymentVnd = GetTotalAdvancePayment(receipt.Id, AccountingConstants.CURRENCY_LOCAL),
                    Office = GetOffice(receipt.OfficeId ?? Guid.Empty).Code
                });

            return result;
        }

        private decimal GetTotalAdvancePayment(Guid receiptId, string currency)
        {
            decimal totalAdv = 0;

            IQueryable<AccAccountingPayment> payments = acctPaymentRepository.Get(x => x.ReceiptId == receiptId && (x.Type == AccountingConstants.PAYMENT_TYPE_CODE_ADVANCE
            || x.Type == AccountingConstants.PAYMENT_TYPE_CODE_COLLECT_OBH));
            if (currency == AccountingConstants.CURRENCY_LOCAL)
            {
                totalAdv = payments.Sum(x => x.PaymentAmountVnd) ?? 0;
            }
            else
            {
                totalAdv = payments.Sum(x => x.PaymentAmountUsd) ?? 0;
            }

            return totalAdv;
        }

        public bool ValidateCusAgreement(Guid agreementId, decimal cusVnd, decimal cusUsd)
        {
            bool valid = true;
            CatContract contract = catContractRepository.Get(x => x.Id == agreementId).FirstOrDefault();
            if (contract != null && (contract.CustomerAdvanceAmountUsd != null || contract.CustomerAdvanceAmountUsd != null))
            {
                if ((contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL && cusVnd > contract.CustomerAdvanceAmountVnd ) 
                    || (contract.CreditCurrency != AccountingConstants.CURRENCY_LOCAL && cusUsd > contract.CustomerAdvanceAmountUsd))
                {
                    valid = false;
                }
            }

            return valid;
        }

        private IQueryable<AccAccountingPayment> GetPaymentStepOrderReceipt(AcctReceipt receiptCurrent, List<ReceiptInvoiceModel> invoice = null)
        {
            //List<string> receiptCurrentPaymentInvoiceList = new List<string>();
            if (invoice != null)
            {
                //receiptCurrentPaymentInvoiceList.AddRange(invoice.Select(x => x.InvoiceNo));
                if (receiptCurrent.Type == "Customer")
                {
                    IQueryable<AccAccountingPayment> hasPayments = acctPaymentRepository.Get(x => x.ReceiptId != receiptCurrent.Id
                                  && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH)
                                  && DateTime.Compare(x.DatetimeCreated ?? DateTime.Now, receiptCurrent.DatetimeCreated ?? DateTime.Now) < 0
                                  //&& receiptCurrentPaymentInvoiceList.Any(inv => inv == x.InvoiceNo)
                                  && invoice.Any(z => z.InvoiceNo == x.InvoiceNo && (receiptCurrent.Type == "Customer" ? true : z.Hblid == x.Hblid))
                                  );
                    return hasPayments;
                }
                else
                {
                    var invoiceNoList = invoice.Where(x => !string.IsNullOrEmpty(x.InvoiceNo)).ToList();
                    var voucherNoList = invoice.Where(x => !string.IsNullOrEmpty(x.VoucherId)).ToList();
                    IQueryable<AccAccountingPayment> hasPayments = acctPaymentRepository.Get(x => x.ReceiptId != receiptCurrent.Id
                                  && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH || x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
                                  && DateTime.Compare(x.DatetimeCreated ?? DateTime.Now, receiptCurrent.DatetimeCreated ?? DateTime.Now) < 0
                                  );
                    if(invoiceNoList.Count() > 0)
                    {
                        hasPayments = hasPayments.Where(x => invoiceNoList.Any(z => z.InvoiceNo == x.InvoiceNo && z.Hblid == x.Hblid));
                    }
                    if(voucherNoList.Count > 0)
                    {
                        hasPayments = hasPayments.Where(x => voucherNoList.Any(z => z.VoucherId == x.VoucherNo && z.Hblid == x.Hblid));
                    }
                    if (invoiceNoList.Count <= 0 && voucherNoList.Count <= 0)
                    {
                        hasPayments = hasPayments.Where(x => false);
                    }

                    return hasPayments;
                }
            }
            else
            {
                if (receiptCurrent.Type == "Customer")
                {
                    var receiptCurrentPaymentInvoiceList = acctPaymentRepository.Get(x => x.ReceiptId == receiptCurrent.Id
                                    && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH)).ToList();
                    IQueryable<AccAccountingPayment> hasPayments = acctPaymentRepository.Get(x => x.ReceiptId != receiptCurrent.Id
                                  && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH)
                                  && DateTime.Compare(x.DatetimeCreated ?? DateTime.Now, receiptCurrent.DatetimeCreated ?? DateTime.Now) < 0
                                  && receiptCurrentPaymentInvoiceList.Any(inv => inv.InvoiceNo == x.InvoiceNo && (receiptCurrent.Type == "Customer" ? true : inv.Hblid == x.Hblid)));
                    return hasPayments;
                }
                else
                {
                    var receiptCurrentPaymentInvoiceList = acctPaymentRepository.Get(x => x.ReceiptId == receiptCurrent.Id
                                && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH || x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)).ToList();
                    var invoiceNoList = receiptCurrentPaymentInvoiceList.Where(x => !string.IsNullOrEmpty(x.InvoiceNo)).ToList();
                    var voucherNoList = receiptCurrentPaymentInvoiceList.Where(x => !string.IsNullOrEmpty(x.VoucherNo)).ToList();
                    IQueryable<AccAccountingPayment> hasPayments = acctPaymentRepository.Get(x => x.ReceiptId != receiptCurrent.Id
                                  && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH || x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
                                  && DateTime.Compare(x.DatetimeCreated ?? DateTime.Now, receiptCurrent.DatetimeCreated ?? DateTime.Now) < 0);
                    if (invoiceNoList.Count > 0)
                    {
                        hasPayments = hasPayments.Where(x => invoiceNoList.Any(z => z.InvoiceNo == x.InvoiceNo && z.Hblid == x.Hblid));
                    }
                    if (voucherNoList.Count > 0)
                    {
                        hasPayments = hasPayments.Where(x => voucherNoList.Any(z => z.VoucherNo == x.VoucherNo && z.Hblid == x.Hblid));
                    }
                    if (invoiceNoList.Count <= 0 && voucherNoList.Count <= 0)
                    {
                        hasPayments = hasPayments.Where(x => false);
                    }

                    return hasPayments;
                }
            }
            //IQueryable<AccAccountingPayment> hasPayments = acctPaymentRepository.Get(x => x.ReceiptId != receiptCurrent.Id
            //                   && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH)
            //                   && DateTime.Compare(x.DatetimeCreated ?? DateTime.Now, receiptCurrent.DatetimeCreated ?? DateTime.Now) < 0
            //                   && receiptCurrentPaymentInvoiceList.Any(inv => inv == x.InvoiceNo)
            //                   );

            //return hasPayments;
        }

        public async Task<HandleState> QuickUpdate(Guid Id, ReceiptQuickUpdateModel model)
        {
            HandleState hs = new HandleState();

            List<AcctReceipt> receiptListAsync = await DataContext.GetAsync(x => x.Id == Id && string.IsNullOrEmpty(x.Arcbno));
            if (receiptListAsync.Count == 0)
            {
                return hs;
            }
            AcctReceipt receiptCurrent = receiptListAsync.FirstOrDefault();
            if (!string.IsNullOrEmpty(model.PaymentMethod))
            {
                receiptCurrent.PaymentMethod = model.PaymentMethod;
            }
            if (!string.IsNullOrEmpty(model.PaymentRefNo))
            {
                receiptCurrent.PaymentRefNo = model.PaymentRefNo;
            }
            if (!string.IsNullOrEmpty(model.BankAccountNo))
            {
                receiptCurrent.BankAccountNo = model.BankAccountNo;
            }
            receiptCurrent.ObhpartnerId = model.OBHPartnerId;
            receiptCurrent.PaymentDate = model.PaymentDate;

            hs = DataContext.Update(receiptCurrent, x => x.Id == receiptCurrent.Id);

            return hs;
        }

        /// <summary>
        /// Update Accounting Debit
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <returns></returns>
        public HandleState UpdateAccountingDebitAR(List<ReceiptInvoiceModel> payments, Guid receiptId, SaveAction saveAction)
        {
            HandleState hs = new HandleState();
            if(payments.Count == 0)
            {
                return hs;
            }
            //var hblIds = payments.Select(x => x.Hblid).ToList();
            //var invoiceDebitArs = debitMngtArRepository.Get(x => hblIds.Contains(x.Hblid));
            var accountingMng = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTANT_TYPE_INVOICE || x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);
            payments = payments.Where(x => x.Type == "DEBIT" || x.Type == "OBH").ToList();
            #region //DELETE
            //foreach (var payment in payments)
            //{
            //    var invoiceDebitAr = invoiceDebitArs.Where(x => payment.RefIds.Contains(x.AcctManagementId.ToString()) && x.Hblid == payment.Hblid).FirstOrDefault();
            //    if(invoiceDebitAr == null)
            //    {
            //        continue;
            //    }

            //    invoiceDebitAr.PaidAmountUsd = invoiceDebitAr.PaidAmountVnd = 0;
            //    invoiceDebitAr.UnpaidAmountUsd = invoiceDebitAr.TotalAmountUsd;
            //    invoiceDebitAr.UnpaidAmountVnd = invoiceDebitAr.TotalAmountVnd;

            //    //if (saveAction == SaveAction.SAVEDONE)
            //    //{
            //    //    var invoice = accountingMng.Where(x => payment.RefIds.Contains(x.Id.ToString())).FirstOrDefault();
            //    //    invoiceDebitAr.PaidAmountUsd = (invoiceDebitAr.PaidAmountUsd ?? 0) + invoice.PaidAmountUsd;
            //    //    invoiceDebitAr.PaidAmountVnd = (invoiceDebitAr.PaidAmountVnd ?? 0) + invoice.PaidAmountVnd;
            //    //}
            //    //else
            //    {
            //        var paymentInvs = acctPaymentRepository.Get(x => payment.RefIds.Contains(x.RefId) && x.Hblid == payment.Hblid && x.Negative != true);
            //        var receiptIds = paymentInvs.Select(x => x.ReceiptId).ToList();
            //        if (receiptIds.Count > 0)
            //        {
            //            var receiptDone = DataContext.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE && receiptIds.Contains(x.Id)).Select(x => x.Id).ToList();
            //            paymentInvs = paymentInvs.Where(x => receiptDone.Contains(x.ReceiptId ?? Guid.Empty));
            //        }
            //        invoiceDebitAr.PaidAmountUsd = (invoiceDebitAr.PaidAmountUsd ?? 0) + paymentInvs.Sum(z => z.PaymentAmountUsd ?? 0);
            //        invoiceDebitAr.PaidAmountVnd = (invoiceDebitAr.PaidAmountVnd ?? 0) + paymentInvs.Sum(z => z.PaymentAmountVnd ?? 0);
            //    }

            //    invoiceDebitAr.UnpaidAmountUsd = (invoiceDebitAr.TotalAmountUsd ?? 0) - invoiceDebitAr.PaidAmountUsd;
            //    invoiceDebitAr.UnpaidAmountVnd = (invoiceDebitAr.TotalAmountVnd ?? 0) - invoiceDebitAr.PaidAmountVnd;

            //    invoiceDebitAr.UserModified = currentUser.UserID;
            //    invoiceDebitAr.DatetimeModified = DateTime.Now;

            //    if (payment.CurrencyId == AccountingConstants.CURRENCY_LOCAL)
            //    {
            //        invoiceDebitAr.PaidAmount = invoiceDebitAr.PaidAmountVnd;
            //        invoiceDebitAr.UnpaidAmount = invoiceDebitAr.UnpaidAmountVnd;
            //    }
            //    else
            //    {
            //        invoiceDebitAr.PaidAmount = invoiceDebitAr.PaidAmountUsd;
            //        invoiceDebitAr.UnpaidAmount = invoiceDebitAr.UnpaidAmountUsd;
            //    }
            //    invoiceDebitAr.PaymentStatus = GetAndUpdateStatusDebitAr(invoiceDebitAr);

            //    if (invoiceDebitAr.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID)
            //    {
            //        invoiceDebitAr.UnpaidAmount = invoiceDebitAr.UnpaidAmountVnd = invoiceDebitAr.UnpaidAmountUsd = 0;
            //    }

            //    debitMngtArRepository.Update(invoiceDebitAr, x => x.Id == invoiceDebitAr.Id, false);
            //}
            #endregion
            var debitData = payments.Select(x => new
            {
                x.PartnerId,
                x.RefNo,
                x.JobNo,
                x.Mbl,
                x.Hbl,
                x.Hblid,
                x.CurrencyId,
                ReceiptId = receiptId.ToString(),
                RefIds = string.Join(";", x.RefIds.Select(z => z)),
                UserId = currentUser.UserID,
                DepartmentId = currentUser.DepartmentId,
                OfficeId = x.OfficeId,
                CompanyId = currentUser.CompanyID
            }).ToList();
            var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@payments",
                    Value = DataHelper.ToDataTable(debitData),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[ReceiptInvoiceTable]"
                },
                new SqlParameter(){ ParameterName="@saveAction", Value=saveAction}
            };
            var result = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_AddAndUpdateDebitMngAR>(parameters);
            if (!result.FirstOrDefault().Status)
            {
                hs = new HandleState(false, result.FirstOrDefault().Message);
            }
            return hs;
        }

        #region COMBINE RECEIPT
        /// <summary>
        /// Update credit AR
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <returns></returns>
        public HandleState UpdateCreditARCombine(List<AcctReceiptModel> receiptModels, SaveAction saveAction)
        {
            try
            {
                foreach (var receipt in receiptModels)
                {
                    foreach (var model in receipt.Payments)
                    {
                        AcctCreditManagementAr creditItem = null;
                        if (saveAction == SaveAction.SAVECANCEL)
                        {
                            var payable = accountPayableRepository.Get(x => model.RefIds.Contains(x.AcctManagementId) && x.PartnerId == model.PartnerId).FirstOrDefault();
                            creditItem = creditMngtArRepository.Get(x => x.VoucherNo == payable.VoucherNo && x.ReferenceNo == payable.ReferenceNo && x.Hblid == model.Hblid && x.PartnerId == model.PartnerId).FirstOrDefault();
                            if (creditItem != null)
                            {
                                creditItem.RemainUsd += model.PaidAmountUsd;
                                creditItem.RemainVnd += model.PaidAmountVnd;
                            }
                        }
                        else if (saveAction == SaveAction.SAVEDONE)
                        {
                            creditItem = creditMngtArRepository.Get(x => x.Code == model.RefNo && x.ReferenceNo == model.ReferenceNo && x.Hblid == model.Hblid && x.PartnerId == model.PartnerId).FirstOrDefault();
                            if (creditItem != null)
                            {
                                creditItem.RemainUsd = creditItem.RemainUsd - model.PaidAmountUsd;
                                creditItem.RemainVnd = creditItem.RemainVnd - model.PaidAmountVnd;
                            }
                        }
                        if (creditItem != null)
                        {
                            // Check status credit
                            if (creditItem.RemainUsd == 0)
                            {
                                creditItem.PaymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                            }
                            else if (creditItem.RemainUsd != 0 && creditItem.RemainUsd < creditItem.AmountUsd)
                            {
                                creditItem.PaymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                            }
                            else
                            {
                                creditItem.PaymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                            }
                            creditMngtArRepository.Update(creditItem, x => x.Id == creditItem.Id, false);
                        }
                    }
                }
                var hs = creditMngtArRepository.SubmitChanges();
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_UpdateCreditARCombine_LOG", "Error:" + ex.ToString() + " -Data:" + JsonConvert.SerializeObject(receiptModels));
                return new HandleState(ex.Message);
            }
        }

        /// <summary>
        /// Add payment for credit payment
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <returns></returns>
        public HandleState AddPaymentsCreditCombine(List<AcctReceiptModel> receiptModels, SaveAction saveAction)
        {
            foreach (var receipt in receiptModels)
            {
                if (saveAction == SaveAction.SAVECANCEL)
                {
                    foreach (var model in receipt.Payments)
                    {
                        var payable = accountPayableRepository.Get(x => x.BillingNo == model.RefNo && model.RefIds.Contains(x.AcctManagementId) && x.PartnerId == model.PartnerId
                        && (x.TransactionType.Contains(AccountingConstants.TRANSACTION_TYPE_PAYABLE_CREDIT) || x.TransactionType == AccountingConstants.TRANSACTION_TYPE_PAYABLE_OBH)).FirstOrDefault();
                        IQueryable<AccAccountPayablePayment> existPayment = payablePaymentRepository.Get(x => x.OfficeId == model.OfficeId && x.PaymentNo == payable.VoucherNo
                            && x.ReferenceNo == payable.ReferenceNo && x.PartnerId == model.PartnerId && x.AcctId == receipt.Id.ToString());
                        List<Guid> deletePayable = existPayment.Select(x => x.Id).ToList();

                        foreach (var item in existPayment)
                        {
                            payable.PaymentAmount -= item.PaymentAmount ?? 0;
                            payable.PaymentAmountUsd -= item.PaymentAmountUsd ?? 0;
                            payable.PaymentAmountVnd -= item.PaymentAmountVnd ?? 0;
                        }
                        payable.RemainAmount = payable.TotalAmount - payable.PaymentAmount;
                        payable.RemainAmountVnd = payable.TotalAmountVnd - payable.PaymentAmountVnd;
                        payable.RemainAmountUsd = payable.TotalAmountUsd - payable.PaymentAmountUsd;
                        if (payable.RemainAmount == 0)
                        {
                            payable.Status = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                        }
                        else if (payable.PaymentAmount != 0 && payable.RemainAmount != 0)
                        {
                            payable.Status = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                        }
                        else
                        {
                            payable.Status = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                        }
                        payable.UserModified = currentUser.UserID;
                        payable.DatetimeModified = DateTime.Now;

                        HandleState hsUpdDatacontext = accountPayableRepository.Update(payable, x => x.Id == payable.Id, false);
                        HandleState hsDelPayments = payablePaymentRepository.Delete(x => deletePayable.Any(z => z == x.Id), false);
                    }
                }
                else
                {
                    var partner = catPartnerRepository.Get(x => x.Id == receipt.Payments.First().PartnerId).FirstOrDefault();
                    var paymentGrp = receipt.Payments.GroupBy(x => new { x.VoucherId, x.PartnerId, x.ReferenceNo, x.OfficeId })
                        .Select(x => new
                        {
                            x.Key,
                            invoices = x.Select(z => z)
                        });
                    var paymentsCurrent = accountPayableRepository.Get(x => !string.IsNullOrEmpty(x.ReferenceNo));
                    foreach (var model in paymentGrp)
                    {
                        var payableExisted = paymentsCurrent.Where(x => x.PartnerId == model.Key.PartnerId && (x.TransactionType.Contains(AccountingConstants.TRANSACTION_TYPE_PAYABLE_CREDIT) || x.TransactionType == AccountingConstants.TRANSACTION_TYPE_PAYABLE_OBH)
                        && (x.ReferenceNo == model.Key.ReferenceNo) && x.OfficeId == model.Key.OfficeId).FirstOrDefault();
                        var accPayablePayment = new AccAccountPayablePayment();
                        var creditPos = payableExisted.TotalAmount < 0 ? (-1) : 1; // Xét credit âm
                        accPayablePayment.Id = Guid.NewGuid();
                        accPayablePayment.PartnerId = partner.Id;
                        accPayablePayment.PaymentNo = payableExisted.VoucherNo;
                        accPayablePayment.ReferenceNo = model.Key.ReferenceNo;
                        accPayablePayment.AcctId = receipt.Id.ToString();
                        accPayablePayment.PaymentType = payableExisted.TransactionType;
                        accPayablePayment.Currency = model.invoices.FirstOrDefault().CurrencyId;
                        accPayablePayment.ExchangeRate = model.invoices.FirstOrDefault()?.ExchangeRateBilling;
                        accPayablePayment.Status = payableExisted.Status;
                        accPayablePayment.PaymentMethod = receipt.PaymentMethod;
                        accPayablePayment.PaymentDate = receipt.PaymentDate;

                        accPayablePayment.PaymentAmount = model.invoices.Sum(x => x.PaidAmountUsd ?? 0) != 0 ? (Math.Abs(model.invoices.Sum(x => x.PaidAmountUsd ?? 0)) * creditPos) : model.invoices.Sum(x => x.PaidAmountUsd ?? 0);
                        accPayablePayment.PaymentAmountVnd = model.invoices.Sum(x => x.PaidAmountVnd ?? 0) != 0 ? (Math.Abs(model.invoices.Sum(x => x.PaidAmountVnd ?? 0)) * creditPos) : model.invoices.Sum(x => x.PaidAmountVnd ?? 0);
                        accPayablePayment.PaymentAmountUsd = model.invoices.Sum(x => x.PaidAmountUsd ?? 0) != 0 ? (Math.Abs(model.invoices.Sum(x => x.PaidAmountUsd ?? 0)) * creditPos) : model.invoices.Sum(x => x.PaidAmountUsd ?? 0);

                        accPayablePayment.RemainAmount = model.invoices.Sum(x => x.RemainAmountUsd ?? 0) != 0 ? (Math.Abs(model.invoices.Sum(x => x.RemainAmountUsd ?? 0)) * creditPos) : model.invoices.Sum(x => x.RemainAmountUsd ?? 0);
                        accPayablePayment.RemainAmountVnd = model.invoices.Sum(x => x.RemainAmountVnd ?? 0) != 0 ? (Math.Abs(model.invoices.Sum(x => x.RemainAmountVnd ?? 0)) * creditPos) : model.invoices.Sum(x => x.RemainAmountVnd ?? 0);
                        accPayablePayment.RemainAmountUsd = model.invoices.Sum(x => x.RemainAmountUsd ?? 0) != 0 ? (Math.Abs(model.invoices.Sum(x => x.RemainAmountUsd ?? 0)) * creditPos) : model.invoices.Sum(x => x.RemainAmountUsd ?? 0);
                        // Get status
                        if (accPayablePayment.RemainAmount == 0)
                        {
                            accPayablePayment.Status = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                        }
                        else if (accPayablePayment.PaymentAmount != 0 && accPayablePayment.RemainAmount != 0)
                        {
                            accPayablePayment.Status = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                        }
                        else
                        {
                            payableExisted.Status = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                        }

                        accPayablePayment.CompanyId = currentUser.CompanyID;
                        accPayablePayment.OfficeId = model.Key.OfficeId;
                        accPayablePayment.DepartmentId = currentUser.DepartmentId;
                        accPayablePayment.GroupId = currentUser.GroupId;
                        accPayablePayment.UserCreated = accPayablePayment.UserModified = currentUser.UserID;
                        accPayablePayment.DatetimeCreated = accPayablePayment.DatetimeModified = DateTime.Now;

                        // Update paid amount payable table
                        payableExisted.PaymentAmount = (payableExisted.PaymentAmount ?? 0) + accPayablePayment.PaymentAmount;
                        payableExisted.PaymentAmountVnd = (payableExisted.PaymentAmountVnd ?? 0) + accPayablePayment.PaymentAmountVnd;
                        payableExisted.PaymentAmountUsd = (payableExisted.PaymentAmountUsd ?? 0) + accPayablePayment.PaymentAmountUsd;
                        payableExisted.RemainAmount = payableExisted.TotalAmount - payableExisted.PaymentAmount;
                        if (payableExisted.Currency == AccountingConstants.CURRENCY_LOCAL)
                        {
                            payableExisted.RemainAmountVnd = payableExisted.TotalAmountVnd - payableExisted.PaymentAmountVnd;
                            payableExisted.RemainAmountUsd = payableExisted.RemainAmount == 0 ? 0 : (payableExisted.TotalAmountUsd - payableExisted.PaymentAmountUsd);
                        }
                        else
                        {
                            payableExisted.RemainAmountVnd = (payableExisted.RemainAmount == 0 ? 0 : (payableExisted.TotalAmountVnd - payableExisted.PaymentAmountVnd));
                            payableExisted.RemainAmountUsd = payableExisted.TotalAmountUsd - payableExisted.PaymentAmountUsd;
                        }
                        if (payableExisted.PaymentAmount != 0 && payableExisted.RemainAmount != 0) // Status
                        {
                            payableExisted.Status = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                        }
                        else if (payableExisted.RemainAmount == 0)
                        {
                            payableExisted.Status = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                        }
                        else
                        {
                            payableExisted.Status = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                        }
                        payableExisted.DatetimeModified = DateTime.Now;
                        payableExisted.UserModified = currentUser.UserID;

                        payablePaymentRepository.Add(accPayablePayment, false);
                        accountPayableRepository.Update(payableExisted, x => x.Id == payableExisted.Id, false);
                    }
                }
            }
            var hs = accountPayableRepository.SubmitChanges();
            if (hs.Success)
            {
                payablePaymentRepository.SubmitChanges();
            }
            return hs;
        }

        /// <summary>
        /// Get detail receipt combine
        /// </summary>
        /// <param name="_arcbNo"></param>
        /// <returns></returns>
        public List<AcctReceiptModel> GetByReceiptCombine(string _arcbNo, string _getType)
        {
            var _orgArcbNo = DataContext.Get(x => x.SubArcbno == _arcbNo && x.Status != AccountingConstants.RECEIPT_STATUS_CANCEL).FirstOrDefault()?.Arcbno;
            if (string.IsNullOrEmpty(_orgArcbNo))
            {
                var receiptTemp = DataContext.Get(x => x.Arcbno == _arcbNo).FirstOrDefault();
                AcctReceiptModel result = mapper.Map<AcctReceiptModel>(receiptTemp);
                var partners = catPartnerRepository.Get(x => x.Active == true);
                var partnerInfo = partners.FirstOrDefault(x => x.Id == result.CustomerId);
                result.CustomerName = partnerInfo?.ShortName;
                result.ObhPartnerName = result.ObhpartnerId == null ? null : partners.FirstOrDefault(x => x.Id == result.ObhpartnerId.ToString())?.ShortName;
                //result.UserNameCreated = sysUserRepository.Where(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
                //result.UserNameModified = sysUserRepository.Where(x => x.Id == result.UserModified).FirstOrDefault()?.Username;

                var contract = catContractRepository.Get(x => x.Active == true);
                result.SalemanId = result.AgreementId == null ? string.Empty : contract.FirstOrDefault(x => x.Id == result.AgreementId)?.SaleManId;
                result.SalemanName = result.SalemanId == null ? string.Empty : sysUserRepository.Where(x => x.Id == result.SalemanId).FirstOrDefault()?.Username;
                SysOffice receiptOffice = officeRepository.Get(x => x.Id == (result.OfficeId ?? Guid.Empty))?.FirstOrDefault();
                result.OfficeName = receiptOffice.ShortName;

                return new List<AcctReceiptModel>() { result };
            }

            var receipts = DataContext.Get(x => x.Status != AccountingConstants.RECEIPT_STATUS_CANCEL).ToList();
            if (_arcbNo == _orgArcbNo)
            {
                receipts = receipts.Where(x => x.Arcbno == _orgArcbNo).ToList();
            }
            else
            {
                receipts = receipts.Where(x => x.SubArcbno == _arcbNo).ToList();
            }

            if(!string.IsNullOrEmpty(_getType) && _getType == "existing")
            {
                receipts = new List<AcctReceipt> { receipts.Where(x => x.SubArcbno == _arcbNo).FirstOrDefault() };
            }

            var resultData = new List<AcctReceiptModel>();
            foreach (var receipt in receipts)
            {
                AcctReceiptModel result = mapper.Map<AcctReceiptModel>(receipt);
                List<ReceiptInvoiceModel> paymentReceipts = new List<ReceiptInvoiceModel>();
                IOrderedEnumerable<AccAccountingPayment> acctPayments = acctPaymentRepository.Get(x => x.ReceiptId == receipt.Id)
                    .ToList()
                    .OrderBy(x => x.PaymentType == "OTHER");

                var partners = catPartnerRepository.Get(x => x.Active == true);
                var partnerInfo = partners.FirstOrDefault(x => x.Id == result.CustomerId);
                result.CustomerName = partnerInfo?.ShortName;
                result.ObhPartnerName = result.ObhpartnerId == null ? null : partners.FirstOrDefault(x => x.Id == result.ObhpartnerId.ToString())?.ShortName;

                var creditArs = creditMngtArRepository.Get(x => !string.IsNullOrEmpty(x.ReferenceNo));

                IEnumerable<AccAccountingPayment> listOBH = acctPayments.Where(x => x.Type == "OBH").OrderBy(x => x.DatetimeCreated);
                //var partnerInfos = partners.Where(x => x.Id == result.CustomerId || x.ParentId == result.CustomerId);

                if (listOBH.Count() > 0)
                {
                    var OBHGrp = listOBH.GroupBy(x => new { x.BillingRefNo, x.Negative, x.CurrencyId });

                    List<ReceiptInvoiceModel> items = OBHGrp.Select(s => new ReceiptInvoiceModel
                    {
                        RefNo = s.Key.BillingRefNo,
                        Type = "OBH",
                        InvoiceNo = null,
                        VoucherId = s.FirstOrDefault().VoucherNo,
                        Amount = s.FirstOrDefault().RefAmount,
                        UnpaidAmount = s.Key.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? s.FirstOrDefault().UnpaidPaymentAmountVnd : s.FirstOrDefault().UnpaidPaymentAmountUsd,
                        UnpaidAmountVnd = s.FirstOrDefault().UnpaidPaymentAmountVnd,
                        UnpaidAmountUsd = s.FirstOrDefault().UnpaidPaymentAmountUsd,
                        PaidAmountVnd = s.Sum(x => x.PaymentAmountVnd),
                        PaidAmountUsd = s.Sum(x => x.PaymentAmountUsd),
                        TotalPaidVnd = s.Sum(x => x.PaymentAmountVnd),
                        TotalPaidUsd = s.Sum(x => x.PaymentAmountUsd),
                        Notes = s.FirstOrDefault().Note,
                        OfficeId = s.FirstOrDefault().OfficeInvoiceId,
                        OfficeName = officeRepository.Get(x => x.Id == s.FirstOrDefault().OfficeInvoiceId)?.FirstOrDefault().ShortName,
                        DepartmentId = s.FirstOrDefault().DeptInvoiceId,
                        DepartmentName = departmentRepository.Get(x => x.Id == s.FirstOrDefault().DeptInvoiceId)?.FirstOrDefault()?.DeptNameAbbr,
                        CompanyId = s.FirstOrDefault().CompanyInvoiceId,
                        RefIds = listOBH.Where(x => x.BillingRefNo == s.Key.BillingRefNo).Select(x => x.RefId).ToList(),
                        CreditNo = s.FirstOrDefault().CreditNo,
                        Hblid = s.FirstOrDefault().Hblid,
                        Mbl = GetHBLInfo(s.FirstOrDefault().Hblid).MBL,
                        Hbl = GetHBLInfo(s.FirstOrDefault().Hblid).HBLNo,
                        JobNo = GetHBLInfo(s.FirstOrDefault().Hblid).JobNo,
                        PaymentStatus = receipt.Type == "Customer" ? GetPaymentStatus(listOBH.Where(x => x.BillingRefNo == s.Key.BillingRefNo).Select(x => x.RefId).ToList()) :
                                        GetPaymentStatusAgent(listOBH.Where(x => x.BillingRefNo == s.Key.BillingRefNo).Select(x => x.RefId).ToList(), s.FirstOrDefault().Hblid),
                        ExchangeRateBilling = s.FirstOrDefault().ExchangeRateBilling,
                        PartnerId = s.FirstOrDefault()?.PartnerId?.ToString(),
                        //Negative = s.FirstOrDefault()?.Negative,
                        PaymentType = s.FirstOrDefault().PaymentType
                    }).ToList();

                    foreach (var item in items)
                    {
                        if (!string.IsNullOrEmpty(item.PartnerId))
                        {
                            var agency = partners.FirstOrDefault(x => x.Id == item.PartnerId);
                            item.PartnerName = agency?.ShortName;
                            item.TaxCode = agency?.AccountNo;
                        }
                        if (!string.IsNullOrEmpty(receipt.Arcbno) && receipt.PaymentMethod.ToLower().Contains("credit"))
                        {
                            item.ReferenceNo = creditArs.Where(x => x.Code == item.RefNo && x.Hblid == item.Hblid).FirstOrDefault()?.ReferenceNo;
                        }
                    }
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
                        string _ReferenceNo = string.Empty;

                        if (acctPayment.Hblid != null && acctPayment.Hblid != Guid.Empty)
                        {
                            var surcharge = surchargeRepository.Get(x => x.Hblid == acctPayment.Hblid && x.AcctManagementId != null && x.AcctManagementId.ToString().Contains(acctPayment.RefId)).FirstOrDefault();
                            if (surcharge != null)
                            {
                                _Hbl = surcharge.Hblno;
                                _Mbl = surcharge.Mblno;
                                _jobNo = surcharge?.JobNo;
                                _ReferenceNo = surcharge?.ReferenceNo;
                            }
                        }

                        //string _voucherId = string.Empty;
                        string _voucherIdre = string.Empty;
                        partnerInfo = partners.FirstOrDefault(x => x.Id == acctPayment.PartnerId);

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
                        payment.PaymentStatus =  (receipt.Type == "Customer" ? GetPaymentStatus(new List<string> { acctPayment.RefId }) : GetPaymentStatusAgent(new List<string> { acctPayment.RefId }, acctPayment.Hblid));
                        payment.JobNo = _jobNo;
                        payment.Mbl = _Mbl;
                        payment.Hbl = _Hbl;
                        payment.Hblid = acctPayment.Hblid;
                        payment.CreditNo = acctPayment.CreditNo;
                        payment.CreditAmountVnd = acctPayment.CreditAmountVnd;
                        payment.CreditAmountUsd = acctPayment.CreditAmountUsd;
                        payment.VoucherId = acctPayment.Type == "CREDIT" ? acctPayment.VoucherNo : null;
                        payment.VoucherIdre = acctPayment.Type == "CREDITNOTE" ? _voucherIdre : null;
                        payment.ExchangeRateBilling = acctPayment.ExchangeRateBilling;
                        payment.PartnerId = acctPayment?.PartnerId?.ToString();
                        payment.PartnerName = partnerInfo?.ShortName;
                        //payment.Negative = acctPayment.Negative;
                        payment.PaymentType = acctPayment.PaymentType;
                        //payment.NetOff = acctPayment.NetOff;
                        //payment.NetOffUsd = acctPayment.NetOffUsd;
                        //payment.NetOffVnd = acctPayment.NetOffVnd;
                        payment.RefCurrency = acctPayment.RefCurrency;
                        payment.ReferenceNo = acctPayment.Type == "CREDIT" ? creditArs.FirstOrDefault(x => x.Code == acctPayment.BillingRefNo && x.Hblid == acctPayment.Hblid)?.ReferenceNo : _ReferenceNo;

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
                var _users = sysUserRepository.Get(x => x.Active == true);
                result.UserNameCreated = _users.Where(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
                result.UserNameModified = _users.Where(x => x.Id == result.UserModified).FirstOrDefault()?.Username;

                var contract = catContractRepository.Get(x => x.Active == true);
                result.SalemanId = result.AgreementId == null ? string.Empty : contract.FirstOrDefault(x => x.Id == result.AgreementId)?.SaleManId;
                result.SalemanName = result.SalemanId == null ? string.Empty : _users.Where(x => x.Id == result.SalemanId).FirstOrDefault()?.Username;
                SysOffice receiptOffice = officeRepository.Get(x => x.Id == (result.OfficeId ?? Guid.Empty))?.FirstOrDefault();
                result.OfficeName = receiptOffice.ShortName;
                //result.ReceiptInternalOfficeCode = receiptOffice.InternalCode;

                // Check có tồn tại 1 invoice thu chưa hết.
                if (result.Status == AccountingConstants.RECEIPT_STATUS_DONE)
                {
                    result.IsReceiptBankFee = result.Payments.Any(x => !string.IsNullOrEmpty(x.PaymentStatus) && x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART);
                    if (result.IsReceiptBankFee == true)
                    {
                        IEnumerable<ReceiptInvoiceModel> paymentPaidApart = result.Payments.Where(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART);
                        IEnumerable<ReceiptInvoiceModel> obhPaidAprt = paymentPaidApart.Where(x => x.Type == "OBH");
                        IEnumerable<ReceiptInvoiceModel> debitPaidAprt = paymentPaidApart.Where(x => x.Type == "DEBIT");

                        if (obhPaidAprt.Count() > 0)
                        {
                            foreach (var item in obhPaidAprt)
                            {
                                // filter ID without Paid
                                List<string> ids = acctMngtRepository.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE && item.RefIds.Contains(x.Id.ToString())
                                && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID).Select(x => x.Id.ToString()).ToList();

                                item.RefIds = ids;
                                //decimal totalBalanceOBH = 0;
                                //decimal totalBalanceOBHVnd = 0;
                                //decimal totalBalanceOBHUsd = 0;

                                //foreach (var i in ids)
                                //{
                                //    AccAccountingManagement invoice = acctMngtRepository.Get(x => x.Id.ToString() == i)?.FirstOrDefault();
                                //    totalBalanceOBHVnd = invoice.UnpaidAmountVnd ?? 0;
                                //    totalBalanceOBHUsd = invoice.UnpaidAmountUsd ?? 0;
                                //    if (item.RefCurrency == AccountingConstants.CURRENCY_LOCAL)
                                //    {
                                //        totalBalanceOBH = totalBalanceOBHVnd;
                                //    }
                                //    else
                                //    {
                                //        totalBalanceOBH = totalBalanceOBHUsd;
                                //    }
                                //}

                                //item.Balance = totalBalanceOBH;
                                //item.BalanceVnd = totalBalanceOBHVnd;
                                //item.BalanceUsd = totalBalanceOBHUsd;
                            }
                        }

                        //if (debitPaidAprt.Count() > 0)
                        //{
                        //    foreach (var item in debitPaidAprt)
                        //    {
                        //        if (receipt.Type == "Customer")
                        //        {
                        //            AccAccountingManagement invoice = acctMngtRepository.Get(x => x.Id.ToString() == item.RefIds.FirstOrDefault())?.FirstOrDefault();
                        //            item.BalanceVnd = invoice.UnpaidAmountVnd;
                        //            item.BalanceUsd = invoice.UnpaidAmountUsd;
                        //            if (item.RefCurrency == AccountingConstants.CURRENCY_LOCAL)
                        //            {
                        //                item.Balance = item.BalanceVnd;
                        //            }
                        //            else
                        //            {
                        //                item.Balance = item.BalanceUsd;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            var debitInvoice = debitMngtArRepository.Get(x => item.RefIds.Contains(x.AcctManagementId.ToString()) && x.Hblid == item.Hblid).FirstOrDefault();
                        //            item.BalanceVnd = debitInvoice.UnpaidAmountVnd;
                        //            item.BalanceUsd = debitInvoice.UnpaidAmountUsd;
                        //            if (item.RefCurrency == AccountingConstants.CURRENCY_LOCAL)
                        //            {
                        //                item.Balance = item.BalanceVnd;
                        //            }
                        //            else
                        //            {
                        //                item.Balance = item.BalanceUsd;
                        //            }
                        //        }
                        //    }
                        //}
                    }
                }

                //Số phiếu con đã reject
                var totalRejectReceiptSync = receiptSyncRepository.Get(x => x.ReceiptId == receipt.Id && x.SyncStatus == AccountingConstants.STATUS_REJECTED).Count();
                if (totalRejectReceiptSync > 0)
                {
                    result.SubRejectReceipt = receipt.SyncStatus != "Rejected" ? " - Rejected(" + totalRejectReceiptSync + ")" : string.Empty;
                }

                //if (result.ReferenceId != null)
                //{
                //    AcctReceipt receiptRef = DataContext.Get(x => x.Id == result.ReferenceId)?.FirstOrDefault();
                //    if (receiptRef != null)
                //    {
                //        result.ReferenceNo = receiptRef.PaymentRefNo + "_" + receiptRef.Class;
                //    }
                //}

                if (result.ObhpartnerId != null)
                {
                    CatPartner obhP = partners.Where(x => x.Id == result.ObhpartnerId.ToString())?.FirstOrDefault();

                    result.ObhPartnerName = obhP?.ShortName;
                }

                resultData.Add(result);
            }
            if(resultData.Count > 0)
            {
                resultData = resultData.OrderByDescending(x => x.DatetimeCreated).OrderBy(x => x.SubArcbno).ToList();
            }
            return resultData;
        }

        /// <summary>
        /// Check payment has exist in other draft with billing no
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <returns></returns>
        public HandleState CheckExitedCombineReceipt(List<AcctReceiptModel> receiptModels)
        {
            {
                var result = new HandleState();
                if (!receiptModels.Any(x => x.Id == Guid.Empty))
                {
                    return result;
                }
                var generalCombines = receiptModels.Where(x => Common.CustomData.PaymentMethodGeneral.Any(c => c.Value == x.PaymentMethod)).ToList();
                var subArcbNos = generalCombines.Select(x => x.SubArcbno).ToList();
                if (generalCombines.Count > 0)
                {
                    var existedGeneralCombine = DataContext.Get(x => x.Arcbno == generalCombines[0].Arcbno && subArcbNos.Contains(x.SubArcbno) && x.CustomerId == generalCombines[0].CustomerId && x.Status != AccountingConstants.RECEIPT_STATUS_CANCEL).ToList();
                    if (existedGeneralCombine != null && existedGeneralCombine.Count > 0)
                    {
                        var receiptIds = existedGeneralCombine.Select(x => x.Id.ToString()).ToList();
                        var payments = acctPaymentRepository.Get(x => receiptIds.Any(z => z == x.ReceiptId.ToString()));
                        var existedModels = from receipt in existedGeneralCombine
                                            join pm in payments on receipt.Id equals pm.ReceiptId
                                            select new
                                            {
                                                Id = receipt.Id,
                                                CustomerId = pm.PartnerId,
                                                PaymentMethod = receipt.PaymentMethod,
                                                ObhpartnerId = receipt.ObhpartnerId,
                                                OfficeId = pm.OfficeId
                                            };
                        var data = from item in generalCombines
                                   join existedModel in existedModels on item.PaymentMethod equals existedModel.PaymentMethod
                                   where item.Payments.FirstOrDefault().PartnerId == existedModel.CustomerId && item.ObhpartnerId == existedModel.ObhpartnerId && item.OfficeId == existedModel.OfficeId
                                   select new
                                   {
                                       PartnerName = item.Payments.FirstOrDefault().PartnerName,
                                       PartnerId = item.Payments.FirstOrDefault().PartnerId,
                                       PaymentMethod = item.PaymentMethod,
                                       ObhpartnerId = item.ObhpartnerId,
                                       OfficeId = item.OfficeId
                                   };
                        if (data.Count() > 0)
                        {
                            var customerName = catPartnerRepository.Get(x=>x.Id == data.FirstOrDefault().PartnerId).FirstOrDefault()?.ShortName;
                            return new HandleState(false, (object)("Customer " + customerName + " with office and payment method has existed"));
                        }
                    }
                }
                
                var debitCombines = receiptModels.Except(generalCombines);
                if (debitCombines.Count() > 0)
                {
                    foreach (var model in debitCombines)
                    {
                        var hs = CheckExistCreditDebitTransaction(model);
                        if (!hs.Success)
                        {
                            return hs;
                        }
                    }
                }
                return result;
            }
        }

        //Check exist list receiptModels in database
        private HandleState CheckExistCreditDebitTransaction(AcctReceiptModel receiptModel)
        {
            var result = new HandleState();
            var refIds = string.Join(";", receiptModel.Payments.Where(x => x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH ||
                x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).Select(x => string.Join(";", x.RefIds)));
            var accReceipts = DataContext.Get(x => x.Status != AccountingConstants.RECEIPT_STATUS_CANCEL && x.Status != AccountingConstants.RECEIPT_STATUS_DONE && x.Type == "Agent");
            if (!string.IsNullOrEmpty(refIds))
            {
                var paymentDB = acctPaymentRepository.Get(x => refIds.Contains(x.RefId));
                var existedItem = (from item in receiptModel.Payments
                                   from pm in paymentDB
                                   where item.RefIds.Contains(pm.RefId) && (pm.Hblid == null || pm.Hblid == item.Hblid)
                                   join receipt in accReceipts on pm.ReceiptId equals receipt.Id
                                   select new
                                   {
                                       item.Hbl,
                                       item.RefNo,
                                       receipt.PaymentRefNo
                                   }).FirstOrDefault();
                if (existedItem != null)
                {
                    var errMessage = string.Format("There is {0} - {1} in the Draft {2}, please update Done that receipt or Remove it first?", existedItem.RefNo, existedItem.Hbl, existedItem.PaymentRefNo);
                    return new HandleState(false, (object)errMessage);
                }
            }
            return result;
        }

        //------------ADD AND UPDATE--------------------
        /// <summary>
        /// Save combine with action
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <param name="saveAction"></param>
        /// <returns></returns>
        public HandleState SaveCombineReceipt(List<AcctReceiptModel> receiptModels, SaveAction saveAction)
        {
            var hs = new HandleState();
            switch (saveAction)
            {
                case SaveAction.SAVEDRAFT_ADD:
                    currentUser.Action = "ReceiptSaveDraft";
                    hs = AddDraftCombine(receiptModels);
                    break;
                case SaveAction.SAVEDRAFT_UPDATE:
                    currentUser.Action = "ReceiptUpdateDraft";
                    hs = UpdateDraftCombine(receiptModels);
                    break;
                case SaveAction.SAVEDONE:
                    currentUser.Action = "ReceiptSaveDone";
                    hs = SaveDoneCombine(receiptModels);
                    break;
            }
            return hs;
        }

        //------------ADD DRAFT COMBINE--------------------
        /// <summary>
        /// add draft combine
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <returns></returns>
        private HandleState AddDraftCombine(List<AcctReceiptModel> receiptModels)
        {
            try
            {

                var _arcbNo = $"{Guid.NewGuid().ToString().Substring(0, 8)}";
                foreach (var item in receiptModels)
                {
                    item.Arcbno = item.SubArcbno = _arcbNo;
                }
                var generalCombines = receiptModels.Where(x => Common.CustomData.PaymentMethodGeneral.Any(c => c.Value == x.PaymentMethod)).ToList();
                var hsGeneral = AddDraftGeneralCombine(generalCombines, _arcbNo);
                if (!hsGeneral.Success)
                {
                    return hsGeneral;
                }

                var credeitCombines = receiptModels.Where(x => Common.CustomData.PaymentMethodCreditCombine.Any(c => c.Value == x.PaymentMethod)).ToList();
                if (credeitCombines.Count > 0)
                {
                    var hsCredit = AddDraftDebitCombine(credeitCombines, _arcbNo, "credit");
                    if (!hsCredit.Success)
                    {
                        return hsCredit;
                    }
                }
                var debitCombines = receiptModels.Where(x => Common.CustomData.PaymentMethodDebitCombine.Any(c => c.Value == x.PaymentMethod)).ToList();
                if (debitCombines.Count > 0)
                {
                    var hsDebit = AddDraftDebitCombine(debitCombines, _arcbNo, "debit");
                    if (!hsDebit.Success)
                    {
                        return hsDebit;
                    }
                }
                var hs = DataContext.SubmitChanges();
                if (hs.Success)
                {
                    receiptModels = UpdateArcbNoReceipt(receiptModels, _arcbNo);
                }
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        /// <summary>
        /// add draft general combine
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <param name="_arcbNo"></param>
        /// <returns></returns>
        private HandleState AddDraftGeneralCombine(List<AcctReceiptModel> receiptModels, string _arcbNo)
        {
            try
            {
                var receiptGrp = receiptModels.GroupBy(x => new { x.OfficeId, x.PaymentMethod, x.ObhpartnerId });
                foreach (var item in receiptGrp)
                {
                    var model = mapper.Map<AcctReceiptModel>(item.Select(x => x).FirstOrDefault());
                    model.Id = Guid.NewGuid();
                    model.Status = AccountingConstants.RECEIPT_STATUS_DRAFT;
                    model.UserCreated = model.UserModified = currentUser.UserID;
                    model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                    model.GroupId = currentUser.GroupId;
                    model.DepartmentId = currentUser.DepartmentId;
                    model.CompanyId = currentUser.CompanyID;
                    model.OfficeId = item.Key.OfficeId;
                    model.Arcbno = _arcbNo;
                    var _officeCode = officeRepository.Get(x => x.Id == model.OfficeId).FirstOrDefault()?.Code;
                    model.PaymentRefNo = GenerateReceiptNoV2(model, _officeCode);
                    model.PaidAmount = item.Sum(x => x.PaidAmountUsd ?? 0);
                    model.FinalPaidAmount = item.Sum(x => x.FinalPaidAmountUsd ?? 0);

                    model.PaidAmountUsd = item.Sum(x => x.PaidAmountUsd ?? 0);
                    model.PaidAmountVnd = item.Sum(x => x.PaidAmountVnd ?? 0);
                    model.FinalPaidAmountUsd = item.Sum(x => x.FinalPaidAmountUsd ?? 0);
                    model.FinalPaidAmountVnd = item.Sum(x => x.FinalPaidAmountVnd ?? 0);
                    model.Class = model.PaymentMethod; // Class = Payment Method
                    model.ReceiptMode = GetReceiptMode(model.PaymentMethod);

                    CatPartner partner = catPartnerRepository.Get(x => x.Id == model.CustomerId)?.FirstOrDefault();
                    if (partner != null)
                    {
                        model.Type = partner.PartnerType == "Agent" ? "Agent" : "Customer";
                    }

                    AcctReceipt receipt = mapper.Map<AcctReceipt>(model);
                    //receipt.CustomerId = model.Payments[0].PartnerId;

                    HandleState hs = DataContext.Add(receipt);
                    if (hs.Success)
                    {
                        var payments = new List<ReceiptInvoiceModel>();
                        foreach (var data in item.Select(z => z))
                        {
                            payments.AddRange(data.Payments);
                        }
                        HandleState hsPayment = AddPayments(payments, receipt);
                        if (!hsPayment.Success)
                        {
                            new LogHelper("eFMS_AR_SaveDraftGeneralPayment_LOG", hsPayment.Message?.ToString());
                        }
                    }
                }
                var resultSubmit = DataContext.SubmitChanges();
                return resultSubmit;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveDraftGeneral_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        /// <summary>
        /// Add draft debit combine
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <param name="_arcbNo"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        private HandleState AddDraftDebitCombine(List<AcctReceiptModel> receiptModels, string _arcbNo, string _type)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var model in receiptModels)
                    {
                        if (model.Id == Guid.Empty)
                        {
                            model.Id = Guid.NewGuid();
                            model.Status = AccountingConstants.RECEIPT_STATUS_DRAFT;
                            model.UserCreated = model.UserModified = currentUser.UserID;
                            model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                            model.GroupId = currentUser.GroupId;
                            model.DepartmentId = currentUser.DepartmentId;
                            model.CompanyId = currentUser.CompanyID;
                            model.OfficeId = model.Payments.FirstOrDefault().OfficeId;
                            var _officeCode = officeRepository.Get(x => x.Id == model.OfficeId).FirstOrDefault()?.Code;
                            if (string.IsNullOrEmpty(model.PaymentRefNo))
                            {
                                model.PaymentRefNo = GenerateReceiptNoV2(model, _officeCode);
                            }
                            model.Class = model.PaymentMethod; // Class = Payment Method
                            model.ReceiptMode = GetReceiptMode(model.PaymentMethod);

                            //if(model.Type == "CREDIT") 
                            //{
                            //    model.Class = AccountingConstants.RECEIPT_CLASS_NET_OFF;
                            //}
                            //else if (model.Type == "DEBIT")
                            //{
                            //    model.Class = AccountingConstants.RECEIPT_CLASS_CLEAR_DEBIT;
                            //}
                            //model.Arcbno = _arcbNo;
                            model.PaidAmount = model.PaidAmountUsd = model.FinalPaidAmountUsd;
                            model.PaidAmountVnd = model.FinalPaidAmountVnd;

                            CatPartner partner = catPartnerRepository.Get(x => x.Id == model.CustomerId)?.FirstOrDefault();
                            if (partner != null)
                            {
                                model.Type = partner.PartnerType == "Agent" ? "Agent" : "Customer";
                            }

                            AcctReceipt receipt = mapper.Map<AcctReceipt>(model);
                            //receipt.CustomerId = model.Payments[0].PartnerId;
                            HandleState hs = DataContext.Add(receipt, false);

                            if (hs.Success)
                            {
                                HandleState hsPayment = AddPayments(model.Payments, receipt);
                                if (!hsPayment.Success)
                                {
                                    new LogHelper("eFMS_AR_AddDraftDebitCombine_LOG", hsPayment.Message?.ToString());
                                    trans.Rollback();
                                }
                            }
                        }
                        else
                        {
                            AcctReceipt receiptCurrent = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                            if (receiptCurrent == null) return new HandleState((object)"Not found receipt");
                            if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_DONE) return new HandleState((object)"Not allow save draft. Receipt has been done");
                            if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_CANCEL) return new HandleState((object)"Not allow save draft. Receipt has canceled");

                            receiptCurrent.PaymentRefNo = string.IsNullOrEmpty(model.PaymentRefNo) ? receiptCurrent.PaymentRefNo : model.PaymentRefNo;
                            receiptCurrent.UserModified = currentUser.UserID;
                            receiptCurrent.DatetimeModified = DateTime.Now;
                            receiptCurrent.PaymentMethod = model.PaymentMethod;
                            receiptCurrent.OfficeId = model.OfficeId;
                            receiptCurrent.Description = model.Description;
                            receiptCurrent.Class = model.PaymentMethod; // Class = Payment Method
                            receiptCurrent.ReceiptMode = GetReceiptMode(model.PaymentMethod);

                            receiptCurrent.PaidAmount = model.PaidAmountUsd;
                            receiptCurrent.FinalPaidAmount = model.FinalPaidAmountUsd;
                            model.PaidAmount = model.PaidAmountUsd = model.FinalPaidAmountUsd;
                            model.PaidAmountVnd = model.FinalPaidAmountVnd;

                            CatPartner partner = catPartnerRepository.Get(x => x.Id == model.CustomerId)?.FirstOrDefault();
                            if (partner != null)
                            {
                                model.Type = partner.PartnerType == "Agent" ? "Agent" : "Customer";
                            }

                            AcctReceipt receipt = mapper.Map<AcctReceipt>(receiptCurrent);
                            HandleState hsData = DataContext.Update(receipt, x => x.Id == receipt.Id, false);
                            if (hsData.Success)
                            {
                                HandleState hsPaymentUpdate = AddPayments(model.Payments, receipt);

                                if (!hsPaymentUpdate.Success)
                                {
                                    if (!hsPaymentUpdate.Success)
                                    {
                                        new LogHelper("eFMS_UpdateDraftReceiptsCombine_LOG", "Update payment fail: " + hsPaymentUpdate.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(model));
                                        trans.Rollback();
                                    }
                                }
                            }
                        }
                    }
                    var resultSubmit = DataContext.SubmitChanges();
                    if (resultSubmit.Success)
                    {
                        trans.Commit();
                    }
                    return resultSubmit;
                }
                catch (Exception ex)
                {
                    new LogHelper("eFMS_AR_SaveDraftGeneral_LOG", ex.ToString());
                    return new HandleState((object)ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }

        }

        //------------UPDATE DRAFT COMBINE--------------------
        /// <summary>
        /// update draft combine
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <returns></returns>
        private HandleState UpdateDraftCombine(List<AcctReceiptModel> receiptModels)
        {
            try
            {
                var _arcbNo = receiptModels.Where(x => !string.IsNullOrEmpty(x.Arcbno)).FirstOrDefault()?.Arcbno;
                foreach (var item in receiptModels)
                {
                    item.Arcbno = _arcbNo;
                }
                var generalCombines = receiptModels.Where(x => Common.CustomData.PaymentMethodGeneral.Any(c => c.Value == x.PaymentMethod)).ToList();
                if (generalCombines.Count > 0)
                {
                    //var existingGeneralCombines = generalCombines.Where(x => x.Id != Guid.Empty).ToList();
                    var hsUpdateGneral = UpdateDraftReceiptsCombine(generalCombines, _arcbNo);
                    if (!hsUpdateGneral.Success)
                    {
                        return hsUpdateGneral;
                    }

                    //var newGeneralCombines = generalCombines.Where(x => x.Id == Guid.Empty).ToList();
                    //hsUpdateGneral = AddDraftGeneralCombine(newGeneralCombines, _arcbNo);
                    //if (!hsUpdateGneral.Success)
                    //{
                    //    return hsUpdateGneral;
                    //}
                }
                var credeitCombines = receiptModels.Where(x => Common.CustomData.PaymentMethodCreditCombine.Any(c => c.Value == x.PaymentMethod)).ToList();
                if (credeitCombines.Count > 0)
                {
                    //var existingCreditCombines = credeitCombines.Where(x => x.Id != Guid.Empty).ToList();
                    //var hsCredit = UpdateDraftReceiptsCombine(credeitCombines, "credit");
                    //if (!hsCredit.Success)
                    //{
                    //    return hsCredit;
                    //}

                    //var newCreditCombines = credeitCombines.Where(x => x.Id == Guid.Empty).ToList();
                    var hsCredit = AddDraftDebitCombine(credeitCombines, _arcbNo, "credit");
                    if (!hsCredit.Success)
                    {
                        return hsCredit;
                    }
                }
                var debitCombines = receiptModels.Where(x => Common.CustomData.PaymentMethodDebitCombine.Any(c => c.Value == x.PaymentMethod)).ToList();
                if (debitCombines.Count > 0)
                {
                    //var existingDebitCombines = debitCombines.Where(x => x.Id != Guid.Empty).ToList();
                    //var hsDebit = UpdateDraftReceiptsCombine(existingDebitCombines, "debit");
                    //if (!hsDebit.Success)
                    //{
                    //    return hsDebit;
                    //}
                    //var newDebitCombines = debitCombines.Where(x => x.Id == Guid.Empty).ToList();
                    var hsDebit = AddDraftDebitCombine(debitCombines, _arcbNo, "debit");
                    if (!hsDebit.Success)
                    {
                        return hsDebit;
                    }
                }
                var hs = DataContext.SubmitChanges();
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        /// <summary>
        /// Update draft receipt combine
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        private HandleState UpdateDraftReceiptsCombine(List<AcctReceiptModel> receiptModels, string _arcbNo)
        {
            try
            {
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var receiptGrp = receiptModels.GroupBy(x => new { x.OfficeId, x.PaymentMethod, x.ObhpartnerId, x.Id });
                        foreach (var receiptModel in receiptGrp)
                        {
                            var existedReceipt = receiptModel.Where(x => x.Id != Guid.Empty && x.Status == AccountingConstants.RECEIPT_STATUS_DRAFT);
                            if(existedReceipt.Count() > 0)
                            {
                                AcctReceipt receiptCurrent = DataContext.Get(x => x.Id == existedReceipt.FirstOrDefault().Id).FirstOrDefault();
                                if (receiptCurrent == null) return new HandleState((object)"Not found receipt");
                                if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_DONE) return new HandleState((object)"Not allow save draft. Receipt has been done");
                                if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_CANCEL) return new HandleState((object)"Not allow save draft. Receipt has canceled");

                                receiptCurrent.UserModified = currentUser.UserID;
                                receiptCurrent.DatetimeModified = DateTime.Now;
                                receiptCurrent.PaymentMethod = receiptModel.FirstOrDefault().PaymentMethod;
                                receiptCurrent.OfficeId = receiptModel.FirstOrDefault().OfficeId;
                                receiptCurrent.Description = receiptModel.FirstOrDefault().Description;
                                receiptCurrent.PaidAmount = receiptModel.Sum(x => x.Payments.FirstOrDefault().PaidAmount ?? 0);
                                receiptCurrent.PaidAmountUsd = receiptModel.Sum(x => x.Payments.FirstOrDefault().PaidAmountUsd ?? 0);
                                receiptCurrent.PaidAmountVnd = receiptModel.Sum(x => x.Payments.FirstOrDefault().PaidAmountVnd ?? 0);
                                receiptCurrent.FinalPaidAmount = receiptModel.Sum(x => x.Payments.FirstOrDefault().PaidAmount ?? 0);
                                receiptCurrent.FinalPaidAmountUsd = receiptModel.Sum(x => x.Payments.FirstOrDefault().PaidAmountUsd ?? 0);
                                receiptCurrent.FinalPaidAmountVnd = receiptModel.Sum(x => x.Payments.FirstOrDefault().PaidAmountVnd ?? 0);

                                //CatPartner partner = catPartnerRepository.Get(x => x.Id == receiptModel.CustomerId)?.FirstOrDefault();
                                //if (partner != null)
                                //{
                                //    receiptModel.Type = partner.PartnerType == "Agent" ? "Agent" : "Customer";
                                //}
                                AcctReceipt receipt = mapper.Map<AcctReceipt>(receiptCurrent);
                                HandleState hsData = DataContext.Update(receipt, x => x.Id == receipt.Id, false);
                                if (hsData.Success)
                                {
                                    var payments = new List<ReceiptInvoiceModel>();
                                    foreach (var data in receiptModel.Select(z => z))
                                    {
                                        payments.AddRange(data.Payments);
                                    }
                                    HandleState hsPaymentUpdate = AddPayments(payments, receipt);

                                    //HandleState hsPaymentDelete = DeletePayments(paymentsOldDelete);
                                    if (!hsPaymentUpdate.Success)
                                    {
                                        if (!hsPaymentUpdate.Success)
                                        {
                                            new LogHelper("eFMS_UpdateDraftReceiptsCombine_LOG", "Update payment fail: " + hsPaymentUpdate.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(receiptModel));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var newList = receiptModel.Select(x => x).ToList();
                                var hsAdd = AddDraftGeneralCombine(newList, _arcbNo);
                                if (!hsAdd.Success)
                                {
                                    new LogHelper("eFMS_UpdateAddDraftGeneralCombine_LOG", "Update payment fail: " + hsAdd.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(newList));
                                }
                            }
                        }
                        var hs = DataContext.SubmitChanges();
                        if (hs.Success)
                        {
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
                new LogHelper("eFMS_AR_UpdateDraftGeneralCombine_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        //------------SAVE DONE COMBINE--------------------
        /// <summary>
        /// Save done receipt combine
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <returns></returns>
        private HandleState SaveDoneCombine(List<AcctReceiptModel> receiptModels)
        {
            try
            {
                var _arcbNo = receiptModels.Where(x => !string.IsNullOrEmpty(x.Arcbno)).FirstOrDefault()?.Arcbno;
                bool isAddNew = false;
                if (string.IsNullOrEmpty(_arcbNo))
                {
                    isAddNew = true;
                    _arcbNo = $"{Guid.NewGuid().ToString().Substring(0, 8)}";
                }
                foreach (var item in receiptModels)
                {
                    item.Arcbno =  _arcbNo;
                    item.SubArcbno = isAddNew ? _arcbNo : item.SubArcbno;
                }
                var generalCombines = receiptModels.Where(x => Common.CustomData.PaymentMethodGeneral.Any(c => c.Value == x.PaymentMethod)).ToList();
                if (generalCombines.Count > 0)
                {
                    var hsGeneral = SaveDoneGeneralCombine(generalCombines, _arcbNo);
                    if (!hsGeneral.Success)
                    {
                        return hsGeneral;
                    }
                }

                var credeitCombines = receiptModels.Where(x => Common.CustomData.PaymentMethodCreditCombine.Any(c => c.Value == x.PaymentMethod)).ToList();
                if (credeitCombines.Count > 0)
                {
                    var hsCredit = SaveDoneCreditCombine(credeitCombines, _arcbNo, "credit");
                    if (!hsCredit.Success)
                    {
                        return hsCredit;
                    }
                    //var hsCreditAr = UpdateCreditARCombine(credeitCombines);
                    //if (!hsCreditAr.Success)
                    //{
                    //    new LogHelper("eFMS_AR_UpdateCreditARCombine_LOG", credeitCombines + hsCreditAr.Exception?.Message);
                    //}
                    //var hsCreditPayment = AddPaymentsCreditCombine(credeitCombines);
                    //if (!hsCreditPayment.Success)
                    //{
                    //    new LogHelper("eFMS_AR_AddPaymentsCreditCombine_LOG", credeitCombines + hsCreditPayment.Exception?.Message);
                    //}
                }
                var debitCombines = receiptModels.Where(x => Common.CustomData.PaymentMethodDebitCombine.Any(c => c.Value == x.PaymentMethod)).ToList();
                if (debitCombines.Count > 0)
                {
                    var hsDebit = SaveDoneCreditCombine(debitCombines, _arcbNo, "debit");
                    if (!hsDebit.Success)
                    {
                        return hsDebit;
                    }
                }

                if (isAddNew)
                {
                    receiptModels = UpdateArcbNoReceipt(receiptModels, _arcbNo);
                    _arcbNo = receiptModels.FirstOrDefault()?.Arcbno;
                }
                UpdateBalanceReceipt(_arcbNo);

                return new HandleState();
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveReceipt_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        /// <summary>
        /// Update ArcbNo Receipt
        /// </summary>
        /// <param name="_arcbNo"></param>
        /// <returns></returns>
        private List<AcctReceiptModel> UpdateArcbNoReceipt(List<AcctReceiptModel> receiptModels, string _arcbNo)
        {
            var _arcbNoRandom = _arcbNo;
            var receiptCombineAdd = DataContext.Get(x => x.Arcbno == _arcbNoRandom);
            _arcbNo = GenerateCombineReceiptNo();
            foreach(var item in receiptModels)
            {
                item.Arcbno = _arcbNo;
                item.SubArcbno = item.SubArcbno.Replace(_arcbNoRandom, _arcbNo);
            }
            foreach (var item in receiptCombineAdd)
            {
                item.Arcbno = _arcbNo;
                item.SubArcbno = item.SubArcbno.Replace(_arcbNoRandom, _arcbNo);
                DataContext.Update(item, x => x.Id == item.Id, false);
            }
            var hsUpdReceipt = DataContext.SubmitChanges();
            return receiptModels;
        }

        /// <summary>
        /// Save done general combine
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <param name="_arcbNo"></param>
        /// <returns></returns>
        private HandleState SaveDoneGeneralCombine(List<AcctReceiptModel> receiptModels, string _arcbNo)
        {
            try
            {
                var receiptGrp = receiptModels.GroupBy(x => new { x.OfficeId, x.PaymentMethod, x.ObhpartnerId, x.Id });
                List<AcctReceipt> generateCombines = new List<AcctReceipt>();
                foreach (var receiptModel in receiptGrp)
                {
                    var existedReceipt = receiptModel.Where(x => x.Id != Guid.Empty && x.Status == AccountingConstants.RECEIPT_STATUS_DRAFT);
                    if (existedReceipt.Count() > 0)
                    {
                        HandleState hs = new HandleState();
                        AcctReceipt receipt = DataContext.Get(x => x.Id == existedReceipt.FirstOrDefault().Id).FirstOrDefault();
                        if (receipt == null) return new HandleState((object)"Not found receipt");
                        if (receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE)
                        {
                            throw new Exception("You can not update this receipt, because this receipt had done!");
                        }

                        receipt.UserModified = currentUser.UserID;
                        receipt.DatetimeModified = DateTime.Now;
                        receipt.Class = receiptModel.Key.PaymentMethod; // Class = Payment Method
                        receipt.ReceiptMode = GetReceiptMode(receiptModel.Key.PaymentMethod);

                        receipt.Status = AccountingConstants.RECEIPT_STATUS_DONE;
                        receipt.PaidAmount = existedReceipt.Sum(x => x.PaidAmountUsd ?? 0);
                        receipt.PaidAmountUsd = existedReceipt.Sum(x => x.PaidAmountUsd ?? 0);
                        receipt.PaidAmountVnd = existedReceipt.Sum(x => x.PaidAmountVnd ?? 0);
                        receipt.FinalPaidAmount = existedReceipt.Sum(x => x.FinalPaidAmountUsd ?? 0);
                        receipt.FinalPaidAmountUsd = existedReceipt.Sum(x => x.FinalPaidAmountUsd ?? 0);
                        receipt.FinalPaidAmountVnd = existedReceipt.Sum(x => x.FinalPaidAmountVnd ?? 0);

                        hs = DataContext.Update(receipt, x => x.Id == receipt.Id);
                        if (hs.Success)
                        {
                            generateCombines.Add(receipt);
                            var payments = new List<ReceiptInvoiceModel>();
                            foreach (var data in receiptModel.Select(z => z))
                            {
                                payments.AddRange(data.Payments);
                            }
                            HandleState hsPaymentUpdate = AddPayments(payments, receipt);
                            if (!hsPaymentUpdate.Success)
                            {
                                throw new Exception("Có lỗi khi Add/Update Payment" + hsPaymentUpdate.Message?.ToString());
                            }

                            //var hsInvoice = UpdateInvoiceOfPayment(receipt);

                            //if (!hsInvoice.Success)
                            //{
                            //    throw new Exception("Có lỗi khi update cấn trừ hóa đơn" + hsInvoice.Message?.ToString());
                            //}


                            if (receipt.AgreementId != null && (receipt.PaymentMethod == AccountingConstants.COLLECT_OBH_AGENCY || receipt.PaymentMethod == AccountingConstants.ADVANCE_AGENCY))
                            {
                                HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receipt, SaveAction.SAVEDONE, out decimal advUsd, out decimal advVnd);

                                if (!hsUpdateCusAdvOfAgreement.Success)
                                {
                                    throw new Exception("Có lỗi khi update thông tin hợp đồng" + hsUpdateCusAdvOfAgreement.Message?.ToString());
                                }
                                receipt.AgreementAdvanceAmountVnd = advVnd;
                                receipt.AgreementAdvanceAmountUsd = advUsd;
                                hs = DataContext.Update(receipt, x => x.Id == receipt.Id);
                            }
                        }
                    }
                    else
                    {
                        existedReceipt = receiptModel.Where(x => x.Id == Guid.Empty && (string.IsNullOrEmpty(x.Status) || x.Status == AccountingConstants.RECEIPT_STATUS_DRAFT));
                        var model = mapper.Map<AcctReceiptModel>(existedReceipt.Select(x => x).FirstOrDefault());
                        model.Id = Guid.NewGuid();
                        model.Status = AccountingConstants.RECEIPT_STATUS_DONE;
                        model.UserCreated = model.UserModified = currentUser.UserID;
                        model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                        model.GroupId = currentUser.GroupId;
                        model.DepartmentId = currentUser.DepartmentId;
                        model.CompanyId = currentUser.CompanyID;
                        model.OfficeId = receiptModel.Key.OfficeId;
                        model.Arcbno = _arcbNo;
                        var _officeCode = officeRepository.Get(x => x.Id == model.OfficeId).FirstOrDefault()?.Code;
                        if (string.IsNullOrEmpty(model.PaymentRefNo))
                        {
                            model.PaymentRefNo = GenerateReceiptNoV2(model, _officeCode);
                        }
                        model.PaidAmount = receiptModel.Sum(x => x.PaidAmountUsd ?? 0);
                        model.FinalPaidAmount = receiptModel.Sum(x => x.FinalPaidAmountUsd ?? 0);

                        model.PaidAmountUsd = receiptModel.Sum(x => x.PaidAmountUsd ?? 0);
                        model.PaidAmountVnd = receiptModel.Sum(x => x.PaidAmountVnd ?? 0);
                        model.FinalPaidAmountUsd = receiptModel.Sum(x => x.FinalPaidAmountUsd ?? 0);
                        model.FinalPaidAmountVnd = receiptModel.Sum(x => x.FinalPaidAmountVnd ?? 0);
                        model.Class = model.PaymentMethod; // Class = Payment Method
                        model.ReceiptMode = GetReceiptMode(model.PaymentMethod);

                        CatPartner partner = catPartnerRepository.Get(x => x.Id == model.CustomerId)?.FirstOrDefault();
                        if (partner != null)
                        {
                            model.Type = partner.PartnerType == "Agent" ? "Agent" : "Customer";
                        }

                        AcctReceipt receipt = mapper.Map<AcctReceipt>(model);

                        HandleState hs = DataContext.Add(receipt);
                        if (hs.Success)
                        {
                            generateCombines.Add(receipt);
                            var payments = new List<ReceiptInvoiceModel>();
                            foreach (var data in receiptModel.Select(z => z))
                            {
                                payments.AddRange(data.Payments);
                            }
                            HandleState hsPayment = AddPayments(payments, receipt);
                            if (!hsPayment.Success)
                            {
                                new LogHelper("eFMS_AR_SaveDoneGeneralPayment_LOG", hsPayment.Message?.ToString());
                            }

                            if (receipt.AgreementId != null && (receipt.PaymentMethod == AccountingConstants.COLLECT_OBH_AGENCY || receipt.PaymentMethod == AccountingConstants.ADVANCE_AGENCY))
                            {
                                HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receipt, SaveAction.SAVEDONE, out decimal advUsd, out decimal advVnd);

                                if (!hsUpdateCusAdvOfAgreement.Success)
                                {
                                    throw new Exception("Có lỗi khi update thông tin hợp đồng" + hsUpdateCusAdvOfAgreement.Message?.ToString());
                                }
                                receipt.AgreementAdvanceAmountVnd = advVnd;
                                receipt.AgreementAdvanceAmountUsd = advUsd;
                                hs = DataContext.Update(receipt, x => x.Id == receipt.Id);
                            }
                        }
                    }
                }
                var hsResult = DataContext.SubmitChanges();
                if (hsResult.Success)
                {
                    generateCombines = generateCombines.Where(x => x.Arcbno == x.SubArcbno).ToList();
                    if (generateCombines.Count > 0) // Gen new general sub combine
                    {
                        GenerateSubGeneralCombine(generateCombines, _arcbNo);
                    }
                }
                return hsResult;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveDoneGeneralCombine_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        /// <summary>
        /// Auto generate new general combine
        /// </summary>
        /// <param name="generateCombines"></param>
        /// <param name="_arcbNo"></param>
        /// <returns></returns>
        private HandleState GenerateSubGeneralCombine(List<AcctReceipt> generateCombines, string _arcbNo)
        {
            var receiptGrp = generateCombines.Where(x => x.ObhpartnerId != null && (x.PaymentMethod == AccountingConstants.COLLECT_OBH_AGENCY || x.PaymentMethod == AccountingConstants.PAY_OBH_AGENCY))
                .GroupBy(x => new { x.Id, x.PaymentMethod, x.ObhpartnerId, x.OfficeId });
            try
            {
                foreach (var receiptModel in receiptGrp)
                {

                    var obhPartnerData = catPartnerRepository.Get(x => x.Id == receiptModel.Key.ObhpartnerId.ToString() && !string.IsNullOrEmpty(x.InternalCode)).FirstOrDefault();
                    if (obhPartnerData == null)
                    {
                        return new HandleState();
                    }
                    var officeMapping = officeRepository.Get(x => x.InternalCode == obhPartnerData.InternalCode).FirstOrDefault();
                    if (officeMapping == null)
                    {
                        return new HandleState();
                    }

                    var receipt = mapper.Map<AcctReceiptModel>(receiptModel.Select(x => x).FirstOrDefault());
                    receipt.Id = Guid.NewGuid();
                    receipt.Status = AccountingConstants.RECEIPT_STATUS_DRAFT;

                    receipt.UserCreated = receipt.UserModified = currentUser.UserID;
                    receipt.DatetimeCreated = receipt.DatetimeModified = DateTime.Now;
                    receipt.CompanyId = currentUser.CompanyID;
                    receipt.OfficeId = officeMapping.Id;
                    var departmentAR = departmentRepository.Get(x => x.BranchId == officeMapping.Id && x.DeptType == "AR" && x.Active == true).FirstOrDefault();
                    if (departmentAR != null)
                    {
                        receipt.DepartmentId = departmentAR.Id;
                        var groupAR = groupRepository.Get(x => x.DepartmentId == departmentAR.Id && x.Active == true).FirstOrDefault();
                        if (groupAR != null)
                        {
                            receipt.GroupId = groupAR.Id;
                        }
                    }

                    receipt.Arcbno = _arcbNo;
                    receipt.SubArcbno = string.Format("{0}-{1}", _arcbNo, officeMapping.Code);

                    var _officeCode = officeRepository.Get(x => x.Id == receipt.OfficeId).FirstOrDefault()?.Code;

                    if (receipt.PaymentMethod == AccountingConstants.COLLECT_OBH_AGENCY)
                    {
                        receipt.PaymentMethod = AccountingConstants.PAY_OBH_AGENCY;
                    }
                    else
                    {
                        receipt.PaymentMethod = AccountingConstants.COLLECT_OBH_AGENCY;
                    }
                    {
                        receipt.PaymentRefNo = GenerateReceiptNoV2(mapper.Map<AcctReceiptModel>(receipt), _officeCode);
                    }
                    receipt.Class = receipt.PaymentMethod; // Class = Payment Method
                    receipt.ReceiptMode = GetReceiptMode(receipt.PaymentMethod);
                    receipt.ObhpartnerId = null;

                    CatPartner partner = catPartnerRepository.Get(x => x.Id == receipt.CustomerId)?.FirstOrDefault();
                    if (partner != null)
                    {
                        receipt.Type = partner.PartnerType == "Agent" ? "Agent" : "Customer";
                    }

                    //AcctReceipt receipt = mapper.Map<AcctReceipt>(model);

                    HandleState hs = DataContext.Add(receipt);
                    if (hs.Success)
                    {
                        var payments = new List<ReceiptInvoiceModel>();
                        var receiptIds = receiptModel.Select(x => x.Id).ToList();
                        var paymentReceipt = acctPaymentRepository.Get(x => receiptIds.Contains(x.ReceiptId ?? Guid.Empty));
                        foreach (var item in paymentReceipt)
                        {
                            var _payment = new AccAccountingPayment();
                            _payment = mapper.Map<AccAccountingPayment>(item);
                            _payment.Id = Guid.NewGuid();
                            _payment.ReceiptId = receipt.Id;
                            _payment.BillingRefNo = GenerateAdvNo();
                            _payment.PaymentNo = receipt.PaymentRefNo;
                            _payment.Type = receipt.PaymentMethod;
                            _payment.PaymentMethod = receipt.PaymentMethod; //Payment Method Phiếu thu
                            _payment.Note = string.Empty;
                            _payment.DeptInvoiceId = receipt.DepartmentId;
                            _payment.OfficeInvoiceId = receipt.OfficeId;
                            _payment.CompanyInvoiceId = receipt.CompanyId;
                            _payment.UserCreated = _payment.UserModified = receipt.UserCreated;
                            _payment.DatetimeCreated = _payment.DatetimeModified = DateTime.Now;
                            _payment.GroupId = receipt.GroupId;
                            _payment.DepartmentId = receipt.DepartmentId;
                            _payment.OfficeId = receipt.OfficeId;
                            _payment.CompanyId = receipt.CompanyId;
                            acctPaymentRepository.Add(_payment, false);
                        }
                        var hsPayment = acctPaymentRepository.SubmitChanges();
                        if (!hsPayment.Success)
                        {
                            new LogHelper("eFMS_AR_SaveDoneGeneralPayment_LOG", hsPayment.Message?.ToString() + " - Data: " + JsonConvert.SerializeObject(receipt));
                        }

                        //if (receipt.AgreementId != null && (receipt.PaymentMethod == AccountingConstants.COLLECT_OBH_AGENCY || receipt.PaymentMethod == AccountingConstants.ADVANCE_AGENCY))
                        //{
                        //    HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receipt, SaveAction.SAVEDONE, out decimal advUsd, out decimal advVnd);

                        //    if (!hsUpdateCusAdvOfAgreement.Success)
                        //    {
                        //        throw new Exception("Có lỗi khi update thông tin hợp đồng" + hsUpdateCusAdvOfAgreement.Message?.ToString());
                        //    }
                        //    receipt.AgreementAdvanceAmountVnd = advVnd;
                        //    receipt.AgreementAdvanceAmountUsd = advUsd;
                        //    hs = DataContext.Update(receipt, x => x.Id == receipt.Id);
                        //}
                    }

                }
                return new HandleState();
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_GenerateSubGeneralCombine_LOG", ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        /// <summary>
        /// Save done credit combine
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <param name="_arcbNo"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        private HandleState SaveDoneCreditCombine(List<AcctReceiptModel> receiptModels, string _arcbNo, string _type = null)
        {
            try
            {
                CatPartner partner = catPartnerRepository.Get(x => x.Id == receiptModels[0].CustomerId)?.FirstOrDefault();
                foreach (var receiptModel in receiptModels)
                {
                    receiptModel.Type = partner.PartnerType == "Agent" ? "Agent" : "Customer";
                    if (receiptModel.Id == Guid.Empty || receiptModel.Id == null)
                    {
                        receiptModel.Id = Guid.NewGuid();
                        receiptModel.UserCreated = receiptModel.UserModified = currentUser.UserID;
                        receiptModel.DatetimeCreated = receiptModel.DatetimeModified = DateTime.Now;
                        receiptModel.GroupId = currentUser.GroupId;
                        receiptModel.DepartmentId = currentUser.DepartmentId;
                        receiptModel.OfficeId = currentUser.OfficeID;
                        receiptModel.CompanyId = currentUser.CompanyID;
                        receiptModel.Status = AccountingConstants.RECEIPT_STATUS_DONE;
                        receiptModel.Class = receiptModel.PaymentMethod;
                        var _officeCode = officeRepository.Get(x => x.Id == receiptModel.OfficeId).FirstOrDefault()?.Code;
                        if (string.IsNullOrEmpty(receiptModel.PaymentRefNo))
                        {
                            receiptModel.PaymentRefNo = GenerateReceiptNoV2(receiptModel, _officeCode);
                        }
                        //receiptModel.PaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.PaidAmountVnd : receiptModel.PaidAmountUsd;
                        receiptModel.FinalPaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.FinalPaidAmountVnd : receiptModel.FinalPaidAmountUsd;
                        receiptModel.PaidAmount = receiptModel.PaidAmountUsd = receiptModel.FinalPaidAmountUsd;
                        receiptModel.PaidAmountVnd = receiptModel.FinalPaidAmountVnd;
                        receiptModel.ReceiptMode = GetReceiptMode(receiptModel.PaymentMethod);

                        AcctReceipt receiptData = mapper.Map<AcctReceipt>(receiptModel);
                        var listInvoice = receiptModel.Payments
                            .Where(x => (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.TYPE_CHARGE_OBH || x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)).ToList();
                        //.Select(x => x.InvoiceNo).ToList();
                        // Check payment hien tai
                        IQueryable<AccAccountingPayment> hasPayments = GetPaymentStepOrderReceipt(receiptData, listInvoice);

                        if (hasPayments.Count() > 0)
                        {
                            var query = from p in hasPayments
                                        join r in DataContext.Get() on p.ReceiptId equals r.Id
                                        where r.Status == AccountingConstants.RECEIPT_STATUS_DRAFT
                                        select new { r.Id, p.InvoiceNo, r.PaymentRefNo };

                            if (query != null && query.Count() > 0)
                            {
                                throw new Exception(string.Format(
                                    "You can not done this receipt, because {0} - {1} have payment time before than this receipt. please done the first receipts firstly!",
                                    query.FirstOrDefault()?.InvoiceNo,
                                    query.FirstOrDefault()?.PaymentRefNo
                                    ));
                            }
                        }
                        HandleState hs = DataContext.Add(receiptData);
                        if (hs.Success)
                        {
                            AcctReceipt receiptCurrent = DataContext.Get(x => x.Id == receiptModel.Id).FirstOrDefault();

                            // Phát sinh Payment
                            HandleState hsPaymentUpdate = AddPayments(receiptModel.Payments, receiptCurrent);
                            if (!hsPaymentUpdate.Success)
                            {
                                throw new Exception("Có lỗi khi Add/Update Payment" + hsPaymentUpdate.Message?.ToString());
                            }
                            // cấn trừ cho hóa đơn
                            hs = UpdateInvoiceOfPayment(receiptData);
                            if (!hs.Success)
                            {
                                throw new Exception("Có lỗi khi update cấn trừ hóa đơn" + hs.Message?.ToString());
                            }

                            // Cập nhật CusAdvance cho hợp đồng
                            //if (receiptModel.AgreementId != null)
                            //{
                            //    HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptModel, SaveAction.SAVEDONE, out decimal advUsd, out decimal advVnd);
                            //    if (!hsUpdateCusAdvOfAgreement.Success)
                            //    {
                            //        throw new Exception("Có lỗi khi update thông tin hợp đồng" + hsUpdateCusAdvOfAgreement.Message.ToString());
                            //    }
                            //    // cập nhật lại adv lũy tiến cho receipt
                            //    receiptData.AgreementAdvanceAmountVnd = advVnd;
                            //    receiptData.AgreementAdvanceAmountUsd = advUsd;
                            //    hs = DataContext.Update(receiptData, x => x.Id == receiptData.Id);
                            //}

                        }

                        return hs;
                    }
                    else
                    {
                        AcctReceipt receipt = DataContext.Get(x => x.Id == receiptModel.Id).FirstOrDefault();
                        if(receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE)
                        {
                            throw new Exception("You can not update this receipt, because this receipt had done!");
                        }

                        receiptModel.PaymentRefNo = string.IsNullOrEmpty(receiptModel.PaymentRefNo) ? receipt.PaymentRefNo : receiptModel.PaymentRefNo;
                        receiptModel.UserCreated = receipt.UserCreated;
                        receiptModel.DatetimeCreated = receipt.DatetimeCreated;
                        receiptModel.GroupId = receipt.GroupId;
                        receiptModel.DepartmentId = receipt.DepartmentId;
                        receiptModel.OfficeId = receipt.OfficeId;
                        receiptModel.CompanyId = receipt.CompanyId;

                        receiptModel.UserModified = currentUser.UserID;
                        receiptModel.DatetimeModified = DateTime.Now;
                        receiptModel.Class = receiptModel.PaymentMethod;
                        receiptModel.ReceiptMode = GetReceiptMode(receiptModel.PaymentMethod);

                        receiptModel.Status = AccountingConstants.RECEIPT_STATUS_DONE;
                        receiptModel.DatetimeModified = DateTime.Now;
                        //receiptModel.PaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.PaidAmountVnd : receiptModel.PaidAmountUsd;
                        receiptModel.FinalPaidAmount = receiptModel.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? receiptModel.FinalPaidAmountVnd : receiptModel.FinalPaidAmountUsd;
                        receiptModel.PaidAmount = receiptModel.PaidAmountUsd = receiptModel.FinalPaidAmountUsd;
                        receiptModel.PaidAmountVnd = receiptModel.FinalPaidAmountVnd;

                        AcctReceipt receiptCurrent = mapper.Map<AcctReceipt>(receiptModel);

                        // Xóa các payment hiện tại, add các payment mới khi update
                        List<Guid> paymentsDelete = acctPaymentRepository.Get(x => x.ReceiptId == receiptCurrent.Id).Select(x => x.Id).ToList();
                        HandleState hsPaymentUpdate = AddPayments(receiptModel.Payments, receiptCurrent);
                        if (!hsPaymentUpdate.Success)
                        {
                            throw new Exception("Có lỗi khi Add/Update Payment" + hsPaymentUpdate.Message?.ToString());
                        }
                        //HandleState hsPaymentDelete = DeletePayments(paymentsDelete);
                        //if (!hsPaymentDelete.Success)
                        //{
                        //    throw new Exception("Có lỗi khi Add/Update Payment" + hsPaymentDelete.Message.ToString());
                        //}

                        // Check payment hien tai
                        IQueryable<AccAccountingPayment> hasPayments = GetPaymentStepOrderReceipt(receiptCurrent);

                        if (hasPayments.Count() > 0)
                        {
                            var query = from p in hasPayments
                                        join r in DataContext.Get(r => r.Status == AccountingConstants.RECEIPT_STATUS_DRAFT) on p.ReceiptId equals r.Id
                                        select new { r.Id, p.InvoiceNo, r.PaymentRefNo };

                            if (query != null && query.Count() > 0)
                            {
                                return new HandleState((object)string.Format(
                                    "You can not done this receipt, because {0} - {1} have payment time before than this receipt. please done the first receipts firstly!",
                                    query.FirstOrDefault()?.InvoiceNo,
                                    query.FirstOrDefault()?.PaymentRefNo
                                    ));
                            }
                        }
                        // Done Receipt
                        HandleState hs = new HandleState();
                        if (receiptCurrent == null) return new HandleState((object)"Not found receipt");

                        if (receiptCurrent.Status == AccountingConstants.RECEIPT_STATUS_CANCEL) return new HandleState((object)"Not allow save done. Receipt has canceled");

                        receiptCurrent.Status = AccountingConstants.RECEIPT_STATUS_DONE;
                        receiptCurrent.UserModified = currentUser.UserID;
                        receiptCurrent.DatetimeModified = DateTime.Now;


                        hs = DataContext.Update(receiptCurrent, x => x.Id == receiptCurrent.Id, false);
                        if (hs.Success)
                        {
                            hs = UpdateInvoiceOfPayment(receiptCurrent);

                            if (!hs.Success)
                            {
                                throw new Exception("Có lỗi khi update cấn trừ hóa đơn" + hs.Message?.ToString());
                            }


                            //if (receiptModel.AgreementId != null)
                            //{
                            //    HandleState hsUpdateCusAdvOfAgreement = UpdateCusAdvanceOfAgreement(receiptCurrent, SaveAction.SAVEDONE, out decimal advUsd, out decimal advVnd);

                            //    if (!hsUpdateCusAdvOfAgreement.Success)
                            //    {
                            //        throw new Exception("Có lỗi khi update thông tin hợp đồng" + hsUpdateCusAdvOfAgreement.Message.ToString());
                            //    }
                            //    receiptCurrent.AgreementAdvanceAmountVnd = advVnd;
                            //    receiptCurrent.AgreementAdvanceAmountUsd = advUsd;
                            //    hs = DataContext.Update(receiptCurrent, x => x.Id == receiptCurrent.Id, false);
                            //}

                        }

                    }
                }
                var hsResult = DataContext.SubmitChanges();
                return hsResult;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_AR_SaveDoneCreditCombine_LOG", "Error: " + ex.ToString() + " - Data: " + JsonConvert.SerializeObject(receiptModels));
                return new HandleState((object)ex.Message);
            }
        }

        /// <summary>
        /// Update Balance Receipt
        /// </summary>
        /// <param name="_arcbNo"></param>
        /// <returns></returns>
        private HandleState UpdateBalanceReceipt(string _arcbNo)
        {
            var receipts = DataContext.Get(x => x.Arcbno == _arcbNo && x.Status == AccountingConstants.RECEIPT_STATUS_DONE);
            var combineGrp = receipts.GroupBy(x => x.SubArcbno);
            foreach (var grp in combineGrp)
            {
                var receiptGrp = receipts.Where(x => x.SubArcbno == grp.Key);
                var sumCreditReceipts = receiptGrp.Where(x => x.ReceiptMode == "Credit").Sum(x => x.FinalPaidAmount ?? 0);
                var sumDebitReceipts = receiptGrp.Where(x => x.ReceiptMode == "Debit").Sum(x => x.FinalPaidAmount ?? 0);
                var _isBanlance = sumCreditReceipts == sumDebitReceipts;
                foreach (var item in receiptGrp)
                {
                    item.IsBalanceReceipt = _isBanlance;
                    DataContext.Update(item, x => x.Id == item.Id, false);
                }
            }
            var hs = DataContext.SubmitChanges();
            return hs;
        }

        /// <summary>
        /// Get receipt mode
        /// </summary>
        /// <param name="paymentMethod"></param>
        /// <returns></returns>
        private string GetReceiptMode(string paymentMethod)
        {
            string _mode = string.Empty;
            switch (paymentMethod)
            {
                case "Advance Agency":
                case "Clear Debit Agency":
                case "Collect OBH Agency":
                case "Paid Amount Agency":
                    _mode = "Debit";
                    break;
                case "Bank Fee Agency":
                case "Clear Credit Agency":
                case "Collected Amount Agency":
                case "Pay OBH Agency":
                    _mode = "Credit";
                    break;
            }
            return _mode;
        }
        #endregion
    }
}
