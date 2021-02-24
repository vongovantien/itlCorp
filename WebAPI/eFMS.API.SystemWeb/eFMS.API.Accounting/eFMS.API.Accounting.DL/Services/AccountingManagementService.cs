using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccountingManagementService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingManagementService
    {
        private readonly ICurrentUser currentUser;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepo;
        private readonly IContextBase<CatCurrencyExchange> currencyExchangeRepo;
        private readonly IContextBase<OpsTransaction> opsTransactionRepo;
        private readonly IContextBase<CsTransaction> csTransactionRepo;
        private readonly IContextBase<CsTransactionDetail> csTransactionDetailRepo;
        private readonly IContextBase<CatCharge> chargeRepo;
        private readonly IContextBase<CatChargeDefaultAccount> chargeDefaultRepo;
        private readonly IContextBase<CatUnit> unitRepo;
        private readonly IContextBase<CustomsDeclaration> customsDeclarationRepo;
        private readonly IContextBase<AcctCdnote> cdNoteRepo;
        private readonly IContextBase<CatPartner> partnerRepo;
        private readonly IContextBase<SysUser> userRepo;
        private readonly IContextBase<CatPlace> placeRepo;
        private readonly IContextBase<AcctSettlementPayment> settlementPaymentRepo;
        private readonly IContextBase<SysEmployee> employeeRepo;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<AcctSoa> soaRepo;
        private readonly IContextBase<AccAccountingPayment> accountingPaymentRepository;
        private readonly IContextBase<CatContract> catContractRepository;
        private readonly IContextBase<SysNotifications> sysNotifyRepository;
        private readonly IContextBase<SysUserNotification> sysUserNotifyRepository;


        public AccountingManagementService(IContextBase<AccAccountingManagement> repository,
            IMapper mapper,
            ICurrentUser cUser,
            ICurrencyExchangeService exchangeService,
            IContextBase<CsShipmentSurcharge> surcharge,
            IContextBase<CatCurrencyExchange> currencyExchange,
            IContextBase<OpsTransaction> opsTransaction,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CsTransactionDetail> csTransactionDetail,
            IContextBase<CatCharge> charge,
            IContextBase<CatChargeDefaultAccount> chargeDefault,
            IContextBase<CatUnit> unit,
            IContextBase<CustomsDeclaration> customsDeclaration,
            IContextBase<AcctCdnote> cdNote,
            IContextBase<CatPartner> partner,
            IContextBase<SysUser> user,
            IContextBase<CatPlace> place,
            IContextBase<AcctSettlementPayment> settlementPayment,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<SysEmployee> employee,
            IContextBase<AccAccountingPayment> accountingPaymentRepo,
            IContextBase<CatContract> catContractRepo,
            IContextBase<SysNotifications> sysNotifyRepo,
            IContextBase<SysUserNotification> sysUserNotifyRepo,

            IContextBase<AcctSoa> soa) : base(repository, mapper)
        {
            currentUser = cUser;
            currencyExchangeService = exchangeService;
            surchargeRepo = surcharge;
            currencyExchangeRepo = currencyExchange;
            opsTransactionRepo = opsTransaction;
            csTransactionRepo = csTransaction;
            csTransactionDetailRepo = csTransactionDetail;
            chargeRepo = charge;
            chargeDefaultRepo = chargeDefault;
            unitRepo = unit;
            customsDeclarationRepo = customsDeclaration;
            cdNoteRepo = cdNote;
            partnerRepo = partner;
            userRepo = user;
            placeRepo = place;
            settlementPaymentRepo = settlementPayment;
            employeeRepo = employee;
            stringLocalizer = localizer;
            soaRepo = soa;
            accountingPaymentRepository = accountingPaymentRepo;
            catContractRepository = catContractRepo;
            sysUserNotifyRepository = sysUserNotifyRepo;
            sysNotifyRepository = sysNotifyRepo;

        }

        #region --- DELETE ---
        public HandleState Delete(Guid id)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    AccAccountingManagement data = DataContext.Get(x => x.Id == id).FirstOrDefault();
                    if (data == null)
                    {
                        return new HandleState((object)"Data not found");
                    }
                    if (data.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID || data.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART)
                    {
                        return new HandleState((object)data.InvoiceNoReal + " Had payment");
                    }
                    if (!string.IsNullOrEmpty(data.ReferenceNo))
                    {
                        return new HandleState((object)"Not allow delete. Invoice had updated from accountant system");
                    }
                    HandleState hs = DataContext.Delete(x => x.Id == id, false);
                    if (hs.Success)
                    {
                        var charges = surchargeRepo.Get(x => x.AcctManagementId == id);
                        foreach (CsShipmentSurcharge item in charges)
                        {
                            item.AcctManagementId = null;
                            item.InvoiceNo = null;
                            item.InvoiceDate = null;
                            item.VoucherId = null;
                            item.VoucherIddate = null;
                            item.SeriesNo = null;

                            // item.AmountVnd = item.VatAmountVnd = null;
                            // Tính lại do 2 field kế toán edit
                            AmountSurchargeResult amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(item);
                          
                            item.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                            item.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)

                            item.DatetimeModified = DateTime.Now;
                            item.UserModified = currentUser.UserID;

                            surchargeRepo.Update(item, x => x.Id == item.Id, false);

                            UpdateStatusSoaAfterDeleteAcctMngt(data.Type, item);
                        }

                        // Remove VoucherDate, VoucherNo trong settlement.
                        if (data.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                        {

                            List<string> listSettlementCode = charges.Select(x => x.SettlementCode).Distinct().ToList();

                            if (listSettlementCode.Count() > 0)
                            {
                                foreach (string code in listSettlementCode)
                                {
                                    UpdateVoucherSettlement(code, null, null);
                                }

                                settlementPaymentRepo.SubmitChanges();
                            }
                        }

                        soaRepo.SubmitChanges();
                        surchargeRepo.SubmitChanges();
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

        public int CheckDeletePermission(Guid id)
        {
            var detail = GetById(id);
            ICurrentUser _currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.accManagement);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Delete);
            int code = GetPermissionToAction(detail, permissionRangeDelete, _currentUser);
            return code;
        }
        #endregion --- DELETE ---

        #region --- LIST PAGING ---
        public List<AccAccountingManagementResult> Paging(AccAccountingManagementCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetData(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            //Phân trang
            var _totalItem = data.Select(s => s.Id).Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

            return data.ToList();
        }

        private Expression<Func<AccAccountingManagement, bool>> ExpressionQuery(AccAccountingManagementCriteria criteria)
        {
            Expression<Func<AccAccountingManagement, bool>> query = q => true;
            if (!string.IsNullOrEmpty(criteria.TypeOfAcctManagement))
            {
                query = query.And(x => x.Type == criteria.TypeOfAcctManagement);
            }

            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                query = query.And(x => criteria.ReferenceNos.Contains(x.VoucherId) || criteria.ReferenceNos.Contains(x.InvoiceNoReal) || criteria.ReferenceNos.Contains(x.InvoiceNoTempt));
            }

            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(x => x.PartnerId == criteria.PartnerId);
            }

            if (criteria.FromIssuedDate != null && criteria.ToIssuedDate != null)
            {
                query = query.And(x => x.DatetimeCreated.Value.Date >= criteria.FromIssuedDate.Value.Date && x.DatetimeCreated.Value.Date <= criteria.ToIssuedDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(criteria.CreatorId))
            {
                query = query.And(x => x.UserCreated == criteria.CreatorId);
            }

            if (!string.IsNullOrEmpty(criteria.InvoiceStatus))
            {
                query = query.And(x => x.Status == criteria.InvoiceStatus);
            }

            if (!string.IsNullOrEmpty(criteria.VoucherType))
            {
                query = query.And(x => x.VoucherType == criteria.VoucherType);
            }
            return query;
        }

        private IQueryable<AccAccountingManagement> GetAcctMngtPermission()
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.accManagement);
            PermissionRange _permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (_permissionRange == PermissionRange.None) return null;

            IQueryable<AccAccountingManagement> acctMngts = null;
            switch (_permissionRange)
            {
                case PermissionRange.None:
                    break;
                case PermissionRange.All:
                    acctMngts = DataContext.Get();
                    break;
                case PermissionRange.Owner:
                    acctMngts = DataContext.Get(x => x.UserCreated == _user.UserID);
                    break;
                case PermissionRange.Group:
                    acctMngts = DataContext.Get(x => x.GroupId == _user.GroupId
                                            && x.DepartmentId == _user.DepartmentId
                                            && x.OfficeId == _user.OfficeID
                                            && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Department:
                    acctMngts = DataContext.Get(x => x.DepartmentId == _user.DepartmentId
                                            && x.OfficeId == _user.OfficeID
                                            && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Office:
                    acctMngts = DataContext.Get(x => x.OfficeId == _user.OfficeID
                                            && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Company:
                    acctMngts = DataContext.Get(x => x.CompanyId == _user.CompanyID);
                    break;
            }
            return acctMngts;
        }

        private IQueryable<AccAccountingManagementResult> GetData(AccAccountingManagementCriteria criteria)
        {
            var query = ExpressionQuery(criteria);
            var partners = partnerRepo.Get();
            var users = userRepo.Get();
            var acct = GetAcctMngtPermission().Where(query);//DataContext.Get(query);
            var data = from acc in acct
                       join pat in partners on acc.PartnerId equals pat.Id into pat2
                       from pat in pat2.DefaultIfEmpty()
                       join user in users on acc.UserCreated equals user.Id into user2
                       from user in user2.DefaultIfEmpty()
                       select new AccAccountingManagementResult
                       {
                           Id = acc.Id,
                           VoucherId = acc.VoucherId,
                           InvoiceNoTempt = acc.InvoiceNoTempt,
                           InvoiceNoReal = acc.InvoiceNoReal,
                           PartnerId = acc.PartnerId,
                           PartnerName = pat.ShortName,
                           TotalAmount = acc.TotalAmount,
                           Currency = acc.Currency,
                           Date = acc.Date, //Invoice Date or Voucher Date
                           Type = acc.Type,
                           AccountNo = acc.AccountNo,
                           Description = acc.Description,
                           DatetimeCreated = acc.DatetimeCreated, //Issue Date
                           CreatorName = user.Username,
                           Status = acc.Status, //Status Invoice,
                           Serie = acc.Serie,
                           DatetimeModified = acc.DatetimeModified,
                           PaymentStatus = acc.PaymentStatus,
                           PaymentDueDate = acc.PaymentDueDate,
                           LastSyncDate = acc.LastSyncDate,
                           SyncStatus = acc.SyncStatus,
                           ReferenceNo = acc.ReferenceNo,
                           ReasonReject = acc.ReasonReject
                       };
            return data.ToArray().OrderByDescending(o => o.DatetimeModified).AsQueryable();
        }
        #endregion --- LIST PAGING ---

        #region --- INVOICE ---
        private List<ChargeOfAccountingManagementModel> GetChargeSellForInvoice(Expression<Func<ChargeOfAccountingManagementModel, bool>> query)
        {
            //Chỉ lấy những phí từ shipment (IsFromShipment = true) có type là SELL (DEBIT)
            var surcharges = surchargeRepo.Get(x => x.IsFromShipment == true && x.Type == AccountingConstants.TYPE_CHARGE_SELL);
            var opsts = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDes = csTransactionDetailRepo.Get();
            var charges = chargeRepo.Get();
            // Chỉ lấy những charge có Type là Công Nợ
            var chargesDefault = chargeDefaultRepo.Get(x => x.Type == "Công Nợ");
            var units = unitRepo.Get();
            var partners = partnerRepo.Get();
            var querySellOperation = from sur in surcharges
                                     join ops in opsts on sur.Hblid equals ops.Hblid
                                     join chg in charges on sur.ChargeId equals chg.Id into chg2
                                     from chg in chg2.DefaultIfEmpty()
                                     join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                     from chgDef in chgDef2.DefaultIfEmpty()
                                     join uni in units on sur.UnitId equals uni.Id into uni2
                                     from uni in uni2.DefaultIfEmpty()
                                     join pat in partners on sur.PaymentObjectId equals pat.Id into pat2
                                     from pat in pat2.DefaultIfEmpty()
                                     select new ChargeOfAccountingManagementModel
                                     {
                                         SurchargeId = sur.Id,
                                         ChargeId = sur.ChargeId,
                                         ChargeCode = chg.Code,
                                         ChargeName = chg.ChargeNameVn,
                                         JobNo = ops.JobNo,
                                         Hbl = ops.Hwbno,
                                         ContraAccount = chgDef.CreditAccountNo,
                                         OrgAmount = sur.NetAmount,
                                         Vat = sur.Vatrate,
                                         OrgVatAmount = sur.Total - sur.NetAmount, //Tiền thuế = Amount Sau thuế - Amount Trước thuế
                                         VatAccount = chgDef.CreditVat, //Account Credit No (VAT)
                                         Currency = sur.CurrencyId,
                                         ExchangeDate = sur.ExchangeDate,
                                         FinalExchangeRate = sur.FinalExchangeRate,
                                         ExchangeRate = sur.FinalExchangeRate,
                                         AmountVnd = sur.AmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                         VatAmountVnd = sur.VatAmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                         VatPartnerId = sur.PaymentObjectId,
                                         VatPartnerCode = pat.TaxCode, //Tax code
                                         VatPartnerName = pat.ShortName,
                                         VatPartnerAddress = pat.AddressVn,
                                         ObhPartnerCode = null,
                                         ObhPartner = null,
                                         InvoiceNo = sur.InvoiceNo,
                                         Serie = sur.SeriesNo,
                                         InvoiceDate = sur.InvoiceDate,
                                         CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                         Qty = sur.Quantity,
                                         UnitName = uni.UnitNameVn,
                                         UnitPrice = sur.UnitPrice,
                                         Mbl = ops.Mblno,
                                         SoaNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : string.Empty,
                                         SettlementCode = null,
                                         AcctManagementId = sur.AcctManagementId,
                                         RequesterId = null,
                                         ChargeType = sur.Type,
                                         IsFromShipment = sur.IsFromShipment                                         
                                     };
            querySellOperation = querySellOperation.Where(query);
            var querySellDocumentation = from sur in surcharges
                                         join cstd in csTransDes on sur.Hblid equals cstd.Id
                                         join cst in csTrans on cstd.JobId equals cst.Id
                                         join chg in charges on sur.ChargeId equals chg.Id into chg2
                                         from chg in chg2.DefaultIfEmpty()
                                         join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                         from chgDef in chgDef2.DefaultIfEmpty()
                                         join uni in units on sur.UnitId equals uni.Id into uni2
                                         from uni in uni2.DefaultIfEmpty()
                                         join pat in partners on sur.PaymentObjectId equals pat.Id into pat2
                                         from pat in pat2.DefaultIfEmpty()
                                         select new ChargeOfAccountingManagementModel
                                         {
                                             SurchargeId = sur.Id,
                                             ChargeId = sur.ChargeId,
                                             ChargeCode = chg.Code,
                                             ChargeName = chg.ChargeNameVn,
                                             JobNo = cst.JobNo,
                                             Hbl = cstd.Hwbno,
                                             ContraAccount = chgDef.CreditAccountNo,
                                             OrgAmount = sur.NetAmount,
                                             Vat = sur.Vatrate,
                                             OrgVatAmount = sur.Total - sur.NetAmount, //Tiền thuế = Amount Sau thuế - Amount Trước thuế
                                             VatAccount = chgDef.CreditVat, //Account Credit No (VAT)
                                             Currency = sur.CurrencyId,
                                             ExchangeDate = sur.ExchangeDate,
                                             FinalExchangeRate = sur.FinalExchangeRate,
                                             ExchangeRate = sur.FinalExchangeRate,
                                             AmountVnd = sur.AmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                             VatAmountVnd = sur.VatAmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                             VatPartnerId = sur.PaymentObjectId,
                                             VatPartnerCode = pat.TaxCode, //Tax code
                                             VatPartnerName = pat.ShortName,
                                             VatPartnerAddress = pat.AddressVn,
                                             ObhPartnerCode = null,
                                             ObhPartner = null,
                                             InvoiceNo = sur.InvoiceNo,
                                             Serie = sur.SeriesNo,
                                             InvoiceDate = sur.InvoiceDate,
                                             CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                             Qty = sur.Quantity,
                                             UnitName = uni.UnitNameVn,
                                             UnitPrice = sur.UnitPrice,
                                             Mbl = cst.Mawb,
                                             SoaNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : string.Empty,
                                             SettlementCode = null,
                                             AcctManagementId = sur.AcctManagementId,
                                             RequesterId = null,
                                             ChargeType = sur.Type,
                                             IsFromShipment = sur.IsFromShipment
                                         };
            querySellDocumentation = querySellDocumentation.Where(query);
            var mergeSell = querySellOperation.Union(querySellDocumentation);
            var dataSell = mergeSell.ToList();
            return dataSell;
        }

        public List<PartnerOfAcctManagementResult> GetChargeSellForInvoiceByCriteria(PartnerOfAcctManagementCriteria criteria)
        {
            //Chỉ lấy ra những charge chưa issue Invoice hoặc Voucher
            Expression<Func<ChargeOfAccountingManagementModel, bool>> query = chg => (chg.AcctManagementId == Guid.Empty || chg.AcctManagementId == null);


            if (criteria.CdNotes != null && criteria.CdNotes.Count > 0)
            {

                query = query.And(x => criteria.CdNotes.Where(w => !string.IsNullOrEmpty(w))
                .Contains(x.CdNoteNo, StringComparer.OrdinalIgnoreCase));

            }

            if (criteria.SoaNos != null && criteria.SoaNos.Count > 0)
            {
                query = query.And(x => criteria.SoaNos.Where(w => !string.IsNullOrEmpty(w))
                .Contains(x.SoaNo, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.JobNos != null && criteria.JobNos.Count > 0)
            {
                query = query.And(x => criteria.JobNos.Where(w => !string.IsNullOrEmpty(w))
                .Contains(x.JobNo, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.Hbls != null && criteria.Hbls.Count > 0)
            {
                query = query.And(x => criteria.Hbls.Where(w => !string.IsNullOrEmpty(w))
                .Contains(x.Hbl, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.Mbls != null && criteria.Mbls.Count > 0)
            {
                query = query.And(x => criteria.Mbls.Where(w => !string.IsNullOrEmpty(w))
                .Contains(x.Mbl, StringComparer.OrdinalIgnoreCase));
            }

            //SELLING (DEBIT)
            var charges = GetChargeSellForInvoice(query);
            // Group by theo Partner
            var chargeGroupByPartner = charges.GroupBy(g => new { PartnerId = g.VatPartnerId }).Select(s =>
            {
                List<string> jobNoGrouped = s.Select(x => x.JobNo).ToList();

                return new PartnerOfAcctManagementResult
                {
                    PartnerId = s.Key.PartnerId,
                    PartnerName = s.FirstOrDefault()?.VatPartnerName,
                    PartnerAddress = s.FirstOrDefault()?.VatPartnerAddress,
                    SettlementRequester = null,
                    InputRefNo = string.Empty,
                    Charges = s.ToList(),
                    Service = GetTransactionType(jobNoGrouped)
                };
            }).ToList();

            string _inputRefNoHadFoundCharge = string.Empty;
            chargeGroupByPartner.ForEach(fe =>
            {
                if (criteria.CdNotes != null && criteria.CdNotes.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.CdNotes.Contains(x.CdNoteNo)).Select(s => s.CdNoteNo).Distinct());
                }

                if (criteria.SoaNos != null && criteria.SoaNos.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.SoaNos.Contains(x.SoaNo)).Select(s => s.SoaNo).Distinct());
                }

                if (criteria.JobNos != null && criteria.JobNos.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.JobNos.Contains(x.JobNo)).Select(s => s.JobNo).Distinct());
                }

                if (criteria.Hbls != null && criteria.Hbls.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.Hbls.Contains(x.Hbl)).Select(s => s.Hbl).Distinct());
                }
                fe.InputRefNo = _inputRefNoHadFoundCharge;
            });
            return chargeGroupByPartner;
        }
        #endregion --- INVOICE ---

        #region --- VOUCHER ---
        private List<ChargeOfAccountingManagementModel> GetChargeBuySellForVoucher(Expression<Func<ChargeOfAccountingManagementModel, bool>> query)
        {
            //Chỉ lấy những phí có type là SELL (DEBIT) OR BUY (CREDIT)
            var surcharges = surchargeRepo.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL || x.Type == AccountingConstants.TYPE_CHARGE_BUY);
            var opsts = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDes = csTransactionDetailRepo.Get();
            var charges = chargeRepo.Get();
            // Chỉ lấy những charge có Type là Công Nợ
            var chargesDefault = chargeDefaultRepo.Get(x => x.Type == "Công Nợ");
            var units = unitRepo.Get();
            var partners = partnerRepo.Get();
            var settlements = settlementPaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);
            var queryBuySellOperation = from sur in surcharges
                                        join ops in opsts on sur.Hblid equals ops.Hblid
                                        join chg in charges on sur.ChargeId equals chg.Id into chg2
                                        from chg in chg2.DefaultIfEmpty()
                                        join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                        from chgDef in chgDef2.DefaultIfEmpty()
                                        join uni in units on sur.UnitId equals uni.Id into uni2
                                        from uni in uni2.DefaultIfEmpty()
                                        join pat in partners on sur.PaymentObjectId equals pat.Id into pat2
                                        from pat in pat2.DefaultIfEmpty()
                                        join sett in settlements on sur.SettlementCode equals sett.SettlementNo into sett2
                                        from sett in sett2.DefaultIfEmpty()
                                        join p in partners on sett.Payee equals p.Id into payeeGrps
                                        from payeeGrp in payeeGrps.DefaultIfEmpty()
                                        select new ChargeOfAccountingManagementModel
                                        {
                                            SurchargeId = sur.Id,
                                            ChargeId = sur.ChargeId,
                                            ChargeCode = chg.Code,
                                            ChargeName = chg.ChargeNameVn,
                                            JobNo = ops.JobNo,
                                            Hbl = ops.Hwbno,
                                            ContraAccount = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? chgDef.CreditAccountNo : chgDef.DebitAccountNo,
                                            OrgAmount = sur.NetAmount,
                                            Vat = sur.Vatrate,
                                            OrgVatAmount = sur.Total - sur.NetAmount, //Tiền thuế = Amount Sau thuế - Amount Trước thuế
                                            VatAccount = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? chgDef.CreditVat : chgDef.DebitVat,
                                            Currency = sur.CurrencyId,
                                            ExchangeDate = sur.ExchangeDate,
                                            FinalExchangeRate = sur.FinalExchangeRate,
                                            ExchangeRate = sur.FinalExchangeRate, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                            AmountVnd = sur.AmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                            VatAmountVnd = sur.VatAmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                            VatPartnerId = sur.PaymentObjectId,
                                            VatPartnerCode = pat.TaxCode, //Tax code
                                            VatPartnerName = pat.ShortName,
                                            VatPartnerAddress = pat.AddressVn,
                                            ObhPartnerCode = null,
                                            ObhPartner = null,
                                            InvoiceNo = sur.InvoiceNo,
                                            Serie = sur.SeriesNo,
                                            InvoiceDate = sur.InvoiceDate,
                                            CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                            Qty = sur.Quantity,
                                            UnitName = uni.UnitNameVn,
                                            UnitPrice = sur.UnitPrice,
                                            Mbl = ops.Mblno,
                                            SoaNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : sur.PaySoano,
                                            SettlementCode = sett.SettlementNo,
                                            AcctManagementId = sur.AcctManagementId,
                                            RequesterId = sett.Requester,
                                            ChargeType = sur.Type,
                                            IsFromShipment = sur.IsFromShipment,
                                            PayeeIdSettle = payeeGrp.Id ?? null,
                                            PayeeNameSettle = payeeGrp.ShortName,
                                            PayeeAddressSettle = payeeGrp.AddressVn,
                                            IsSynced = !string.IsNullOrEmpty(sur.SyncedFrom) && (sur.SyncedFrom.Equals("SOA") || sur.SyncedFrom.Equals("CDNOTE") || sur.SyncedFrom.Equals("VOUCHER") || sur.SyncedFrom.Equals("SETTLEMENT"))
                                        };
            queryBuySellOperation = queryBuySellOperation.Where(query);
            var queryBuySellDocumentation = from sur in surcharges
                                            join cstd in csTransDes on sur.Hblid equals cstd.Id
                                            join cst in csTrans on cstd.JobId equals cst.Id
                                            join chg in charges on sur.ChargeId equals chg.Id into chg2
                                            from chg in chg2.DefaultIfEmpty()
                                            join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                            from chgDef in chgDef2.DefaultIfEmpty()
                                            join uni in units on sur.UnitId equals uni.Id into uni2
                                            from uni in uni2.DefaultIfEmpty()
                                            join pat in partners on sur.PaymentObjectId equals pat.Id into pat2
                                            from pat in pat2.DefaultIfEmpty()
                                            join sett in settlements on sur.SettlementCode equals sett.SettlementNo into sett2
                                            from sett in sett2.DefaultIfEmpty()
                                            join p in partners on sett.Payee equals p.Id into payeeGrps
                                            from payeeGrp in payeeGrps.DefaultIfEmpty()
                                            select new ChargeOfAccountingManagementModel
                                            {
                                                SurchargeId = sur.Id,
                                                ChargeId = sur.ChargeId,
                                                ChargeCode = chg.Code,
                                                ChargeName = chg.ChargeNameVn,
                                                JobNo = cst.JobNo,
                                                Hbl = cstd.Hwbno,
                                                ContraAccount = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? chgDef.CreditAccountNo : chgDef.DebitAccountNo,
                                                OrgAmount = sur.NetAmount,
                                                Vat = sur.Vatrate,
                                                OrgVatAmount = sur.Total - sur.NetAmount, //Tiền thuế = Amount Sau thuế - Amount Trước thuế
                                                VatAccount = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? chgDef.CreditVat : chgDef.DebitVat,
                                                Currency = sur.CurrencyId,
                                                ExchangeDate = sur.ExchangeDate,
                                                FinalExchangeRate = sur.FinalExchangeRate,
                                                ExchangeRate = sur.FinalExchangeRate, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                                AmountVnd = sur.AmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                                VatAmountVnd = sur.VatAmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                                VatPartnerId = sur.PaymentObjectId,
                                                VatPartnerCode = pat.TaxCode, //Tax code
                                                VatPartnerName = pat.ShortName,
                                                VatPartnerAddress = pat.AddressVn,
                                                ObhPartnerCode = null,
                                                ObhPartner = null,
                                                InvoiceNo = sur.InvoiceNo,
                                                Serie = sur.SeriesNo,
                                                InvoiceDate = sur.InvoiceDate,
                                                CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                                Qty = sur.Quantity,
                                                UnitName = uni.UnitNameVn,
                                                UnitPrice = sur.UnitPrice,
                                                Mbl = cst.Mawb,
                                                SoaNo = sur.Type == AccountingConstants.TYPE_CHARGE_SELL ? sur.Soano : sur.PaySoano,
                                                SettlementCode = sett.SettlementNo,
                                                AcctManagementId = sur.AcctManagementId,
                                                RequesterId = sett.Requester,
                                                ChargeType = sur.Type,
                                                IsFromShipment = sur.IsFromShipment,
                                                PayeeIdSettle = payeeGrp.Id ?? null,
                                                PayeeNameSettle = payeeGrp.ShortName,
                                                PayeeAddressSettle = payeeGrp.AddressVn,
                                                IsSynced = !string.IsNullOrEmpty(sur.SyncedFrom) && (sur.SyncedFrom.Equals("SOA") || sur.SyncedFrom.Equals("CDNOTE") || sur.SyncedFrom.Equals("VOUCHER") || sur.SyncedFrom.Equals("SETTLEMENT"))
                                            };
            queryBuySellDocumentation = queryBuySellDocumentation.Where(query);
            var mergeBuySell = queryBuySellOperation.Union(queryBuySellDocumentation);
            var dataBuySell = mergeBuySell.ToList();
            return dataBuySell;
        }

        private List<ChargeOfAccountingManagementModel> GetChargeObhBuyForVoucher(Expression<Func<ChargeOfAccountingManagementModel, bool>> query)
        {
            //Chỉ lấy những phí có type là OBH
            var surcharges = surchargeRepo.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH);
            var opsts = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDes = csTransactionDetailRepo.Get();
            var charges = chargeRepo.Get();
            // Chỉ lấy những charge có Type là Công Nợ
            var chargesDefault = chargeDefaultRepo.Get(x => x.Type == "Công Nợ");
            var units = unitRepo.Get();
            var partners = partnerRepo.Get();
            var obhPartners = partnerRepo.Get();
            var settlements = settlementPaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);
            var queryObhBuyOperation = from sur in surcharges
                                       join ops in opsts on sur.Hblid equals ops.Hblid
                                       join chg in charges on sur.ChargeId equals chg.Id into chg2
                                       from chg in chg2.DefaultIfEmpty()
                                       join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                       from chgDef in chgDef2.DefaultIfEmpty()
                                       join uni in units on sur.UnitId equals uni.Id into uni2
                                       from uni in uni2.DefaultIfEmpty()
                                       join pat in partners on sur.PayerId equals pat.Id into pat2
                                       from pat in pat2.DefaultIfEmpty()
                                       join obhPat in obhPartners on sur.PaymentObjectId equals obhPat.Id into obhPat2
                                       from obhPat in obhPat2.DefaultIfEmpty()
                                       join sett in settlements on sur.SettlementCode equals sett.SettlementNo into sett2
                                       from sett in sett2.DefaultIfEmpty()
                                       join p in partners on sett.Payee equals p.Id into payeeGrps
                                       from payeeGrp in payeeGrps.DefaultIfEmpty()
                                       select new ChargeOfAccountingManagementModel
                                       {
                                           SurchargeId = sur.Id,
                                           ChargeId = sur.ChargeId,
                                           ChargeCode = chg.Code,
                                           ChargeName = chg.ChargeNameVn,
                                           JobNo = ops.JobNo,
                                           Hbl = ops.Hwbno,
                                           ContraAccount = chgDef.CreditAccountNo ?? chgDef.DebitAccountNo,
                                           OrgAmount = sur.NetAmount,
                                           Vat = sur.Vatrate,
                                           OrgVatAmount = sur.Total - sur.NetAmount, //Tiền thuế = Amount Sau thuế - Amount Trước thuế
                                           VatAccount = chgDef.CreditVat ?? chgDef.DebitVat,
                                           Currency = sur.CurrencyId,
                                           ExchangeDate = sur.ExchangeDate,
                                           FinalExchangeRate = sur.FinalExchangeRate,
                                           ExchangeRate = sur.FinalExchangeRate, // Không Tính toán bên dưới -> Lấy giá trị lưu do kế toán chỉnh sửa
                                           AmountVnd = sur.AmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                           VatAmountVnd = sur.VatAmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                           VatPartnerId = sur.PayerId,
                                           VatPartnerCode = pat.TaxCode, //Tax code
                                           VatPartnerName = pat.ShortName,
                                           VatPartnerAddress = pat.AddressVn,
                                           ObhPartnerCode = obhPat.TaxCode, //Tax code
                                           ObhPartner = obhPat.ShortName, //Abbr
                                           InvoiceNo = sur.InvoiceNo,
                                           Serie = sur.SeriesNo,
                                           InvoiceDate = sur.InvoiceDate,
                                           CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                           Qty = sur.Quantity,
                                           UnitName = uni.UnitNameVn,
                                           UnitPrice = sur.UnitPrice,
                                           Mbl = ops.Mblno,
                                           SoaNo = sur.PaySoano,
                                           SettlementCode = sett.SettlementNo,
                                           AcctManagementId = sur.AcctManagementId,
                                           RequesterId = sett.Requester,
                                           ChargeType = sur.Type,
                                           IsFromShipment = sur.IsFromShipment,
                                           PayeeIdSettle = payeeGrp.Id ?? null,
                                           PayeeNameSettle = payeeGrp.ShortName,
                                           PayeeAddressSettle = payeeGrp.AddressVn,
                                           IsSynced = !string.IsNullOrEmpty(sur.PaySyncedFrom) && (sur.PaySyncedFrom.Equals("SOA") || sur.PaySyncedFrom.Equals("CDNOTE") || sur.PaySyncedFrom.Equals("VOUCHER") || sur.PaySyncedFrom.Equals("SETTLEMENT"))
                                       };
            queryObhBuyOperation = queryObhBuyOperation.Where(query);
            var queryObhBuyDocumentation = from sur in surcharges
                                           join cstd in csTransDes on sur.Hblid equals cstd.Id
                                           join cst in csTrans on cstd.JobId equals cst.Id
                                           join chg in charges on sur.ChargeId equals chg.Id into chg2
                                           from chg in chg2.DefaultIfEmpty()
                                           join chgDef in chargesDefault on chg.Id equals chgDef.ChargeId into chgDef2
                                           from chgDef in chgDef2.DefaultIfEmpty()
                                           join uni in units on sur.UnitId equals uni.Id into uni2
                                           from uni in uni2.DefaultIfEmpty()
                                           join pat in partners on sur.PayerId equals pat.Id into pat2
                                           from pat in pat2.DefaultIfEmpty()
                                           join obhPat in obhPartners on sur.PaymentObjectId equals obhPat.Id into obhPat2
                                           from obhPat in obhPat2.DefaultIfEmpty()
                                           join sett in settlements on sur.SettlementCode equals sett.SettlementNo into sett2
                                           from sett in sett2.DefaultIfEmpty()
                                           join p in partners on sett.Payee equals p.Id into payeeGrps
                                           from payeeGrp in payeeGrps.DefaultIfEmpty()
                                           select new ChargeOfAccountingManagementModel
                                           {
                                               SurchargeId = sur.Id,
                                               ChargeId = sur.ChargeId,
                                               ChargeCode = chg.Code,
                                               ChargeName = chg.ChargeNameVn,
                                               JobNo = cst.JobNo,
                                               Hbl = cstd.Hwbno,
                                               ContraAccount = chgDef.CreditAccountNo ?? chgDef.DebitAccountNo,
                                               OrgAmount = sur.NetAmount,
                                               Vat = sur.Vatrate,
                                               OrgVatAmount = sur.Total - sur.NetAmount, //Tiền thuế = Amount Sau thuế - Amount Trước thuế
                                               VatAccount = chgDef.CreditVat ?? chgDef.DebitVat,
                                               Currency = sur.CurrencyId,
                                               ExchangeDate = sur.ExchangeDate,
                                               FinalExchangeRate = sur.FinalExchangeRate,
                                               ExchangeRate = sur.FinalExchangeRate, // Không Tính toán bên dưới -> Lấy giá trị lưu do kế toán chỉnh sửa
                                               AmountVnd = sur.AmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                               VatAmountVnd = sur.VatAmountVnd, // Không Tính toán bên dưới -> Lấy giá trị đã lưu do kế toán chỉnh sửa
                                               VatPartnerId = sur.PayerId,
                                               VatPartnerCode = pat.TaxCode, //Tax code
                                               VatPartnerName = pat.ShortName,
                                               VatPartnerAddress = pat.AddressVn,
                                               ObhPartnerCode = obhPat.TaxCode, //Tax code
                                               ObhPartner = obhPat.ShortName, //Abbr
                                               InvoiceNo = sur.InvoiceNo,
                                               Serie = sur.SeriesNo,
                                               InvoiceDate = sur.InvoiceDate,
                                               CdNoteNo = sur.CreditNo ?? sur.DebitNo,
                                               Qty = sur.Quantity,
                                               UnitName = uni.UnitNameVn,
                                               UnitPrice = sur.UnitPrice,
                                               Mbl = cst.Mawb,
                                               SoaNo = sur.PaySoano,
                                               SettlementCode = sett.SettlementNo,
                                               AcctManagementId = sur.AcctManagementId,
                                               RequesterId = sett.Requester,
                                               ChargeType = sur.Type,
                                               IsFromShipment = sur.IsFromShipment,
                                               PayeeIdSettle = payeeGrp.Id ?? null,
                                               PayeeNameSettle = payeeGrp.ShortName,
                                               PayeeAddressSettle = payeeGrp.AddressVn,
                                               IsSynced = !string.IsNullOrEmpty(sur.PaySyncedFrom) && (sur.PaySyncedFrom.Equals("SOA") || sur.PaySyncedFrom.Equals("CDNOTE") || sur.PaySyncedFrom.Equals("VOUCHER") || sur.PaySyncedFrom.Equals("SETTLEMENT"))
                                           };
            queryObhBuyDocumentation = queryObhBuyDocumentation.Where(query);
            var mergeObhBuy = queryObhBuyOperation.Union(queryObhBuyDocumentation);
            var dataObhBuy = mergeObhBuy.ToList();
            return dataObhBuy;
        }

        public List<ChargeOfAccountingManagementModel> GetChargeForVoucher(Expression<Func<ChargeOfAccountingManagementModel, bool>> query)
        {
            //BUY & SELL
            var queryBuySell = GetChargeBuySellForVoucher(query);
            //OBH Payee (BUY - Credit)
            var queryObhBuy = GetChargeObhBuyForVoucher(query);
            //Merge data
            var dataMerge = queryBuySell.Union(queryObhBuy);
            var result = dataMerge.ToList();

            result.ForEach(fe =>
            {
                string _syncedFromBy = null;
                var surcharge = surchargeRepo.Get(x => x.Id == fe.SurchargeId).FirstOrDefault();
                if (surcharge != null)
                {
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY || surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL)
                    {
                        if (surcharge.SyncedFrom == "SOA")
                        {
                            _syncedFromBy = fe.SoaNo;
                        }
                        if (surcharge.SyncedFrom == "CDNOTE")
                        {
                            _syncedFromBy = fe.CdNoteNo;
                        }
                        if (surcharge.SyncedFrom == "VOUCHER")
                        {
                            _syncedFromBy = surcharge.VoucherId;
                        }
                    }
                    else if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                    {
                        if (surcharge.PaySyncedFrom == "SOA")
                        {
                            _syncedFromBy = fe.SoaNo;
                        }
                        if (surcharge.PaySyncedFrom == "CDNOTE")
                        {
                            _syncedFromBy = fe.CdNoteNo;
                        }
                        if (surcharge.PaySyncedFrom == "VOUCHER")
                        {
                            _syncedFromBy = surcharge.VoucherId;
                        }
                    }
                    fe.SyncedFromBy = _syncedFromBy;
                }
            });
            return result;
        }

        public List<PartnerOfAcctManagementResult> GetChargeForVoucherByCriteria(PartnerOfAcctManagementCriteria criteria)
        {
            //Chỉ lấy ra những charge chưa issue Invoice hoặc Voucher và chưa được Sync
            Expression<Func<ChargeOfAccountingManagementModel, bool>> query = chg => (chg.AcctManagementId == Guid.Empty || chg.AcctManagementId == null) && chg.IsSynced == false;

            if (criteria.CdNotes != null && criteria.CdNotes.Count > 0)
            {
                query = query.And(x => criteria.CdNotes.Where(w => !string.IsNullOrEmpty(w)).Contains(x.CdNoteNo, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.SoaNos != null && criteria.SoaNos.Count > 0)
            {
                query = query.And(x => criteria.SoaNos.Where(w => !string.IsNullOrEmpty(w)).Contains(x.SoaNo, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.JobNos != null && criteria.JobNos.Count > 0)
            {
                query = query.And(x => criteria.JobNos.Where(w => !string.IsNullOrEmpty(w)).Contains(x.JobNo, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.Hbls != null && criteria.Hbls.Count > 0)
            {
                query = query.And(x => criteria.Hbls.Where(w => !string.IsNullOrEmpty(w)).Contains(x.Hbl, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.Mbls != null && criteria.Mbls.Count > 0)
            {
                query = query.And(x => criteria.Mbls.Where(w => !string.IsNullOrEmpty(w)).Contains(x.Mbl, StringComparer.OrdinalIgnoreCase));
            }

            if (criteria.SettlementCodes != null && criteria.SettlementCodes.Count > 0)
            {
                query = query.And(x => criteria.SettlementCodes.Where(w => !string.IsNullOrEmpty(w)).Contains(x.SettlementCode, StringComparer.OrdinalIgnoreCase));
            }
            else
            {
                //Chỉ lấy Phí chứng từ khi không search theo Settlement
                query = query.And(x => x.IsFromShipment == true);
            }

            var charges = GetChargeForVoucher(query);

            var chargeGroupByPartner = new List<PartnerOfAcctManagementResult>();

            if (criteria.SettlementCodes != null && criteria.SettlementCodes.Count > 0)
            {
                // Group by theo RequesterId
                if (charges.Any(x => x.PayeeIdSettle != null))
                {
                    chargeGroupByPartner = charges.GroupBy(g => new { g.PayeeIdSettle }).Select(s =>
                    {
                        List<string> jobNoGrouped = s.Select(x => x.JobNo).ToList();
                        return new PartnerOfAcctManagementResult
                        {
                            PartnerId = s.Key.PayeeIdSettle,
                            PartnerName = s.FirstOrDefault()?.PayeeNameSettle,
                            PartnerAddress = s.FirstOrDefault()?.PayeeAddressSettle,
                            SettlementRequesterId = null,
                            SettlementRequester = null, //Tính toán bên dưới
                            InputRefNo = string.Empty, //Tính toán bên dưới
                            Charges = s.ToList(),
                            Service = GetTransactionType(jobNoGrouped)
                        };
                    }).ToList();
                }
                else
                {
                    chargeGroupByPartner = charges.GroupBy(g => new { g.RequesterId }).Select(s =>
                    {
                        List<string> jobNoGrouped = s.Select(x => x.JobNo).ToList();
                        return new PartnerOfAcctManagementResult
                        {
                            PartnerId = null,
                            PartnerName = null,
                            PartnerAddress = null,
                            SettlementRequesterId = s.Key.RequesterId,
                            SettlementRequester = null, //Tính toán bên dưới
                            InputRefNo = string.Empty, //Tính toán bên dưới
                            Charges = s.ToList(),
                            Service = GetTransactionType(jobNoGrouped)
                        };
                    }).ToList();
                }

            }
            else
            {
                // Group by theo Partner
                chargeGroupByPartner = charges.GroupBy(g => new { PartnerId = g.VatPartnerId }).Select(s =>
                    {
                        List<string> jobNoGrouped = s.Select(x => x.JobNo).ToList();

                        return new PartnerOfAcctManagementResult
                        {
                            PartnerId = s.Key.PartnerId,
                            PartnerName = s.FirstOrDefault()?.VatPartnerName,
                            PartnerAddress = s.FirstOrDefault()?.VatPartnerAddress,
                            SettlementRequesterId = null,
                            SettlementRequester = null,
                            InputRefNo = string.Empty, //Tính toán bên dưới
                            Charges = s.ToList(),
                            Service = GetTransactionType(jobNoGrouped)
                        };
                    }).ToList();

            }

            string _inputRefNoHadFoundCharge = string.Empty;
            chargeGroupByPartner.ForEach(fe =>
            {
                if (criteria.CdNotes != null && criteria.CdNotes.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.CdNotes.Contains(x.CdNoteNo)).Select(s => s.CdNoteNo).Distinct());
                }

                if (criteria.SoaNos != null && criteria.SoaNos.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.SoaNos.Contains(x.SoaNo)).Select(s => s.SoaNo).Distinct());
                }

                if (criteria.JobNos != null && criteria.JobNos.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.JobNos.Contains(x.JobNo)).Select(s => s.JobNo).Distinct());
                }

                if (criteria.Hbls != null && criteria.Hbls.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.Hbls.Contains(x.Hbl)).Select(s => s.Hbl).Distinct());
                }

                if (criteria.SettlementCodes != null && criteria.SettlementCodes.Count > 0)
                {
                    _inputRefNoHadFoundCharge = string.Join(", ", fe.Charges.Where(x => criteria.SettlementCodes.Contains(x.SettlementCode)).Select(s => s.SettlementCode).Distinct());

                    var employeeIdRequester = userRepo.Get(x => x.Id == fe.SettlementRequesterId).FirstOrDefault()?.EmployeeId;
                    fe.SettlementRequester = employeeRepo.Get(x => x.Id == employeeIdRequester).FirstOrDefault()?.EmployeeNameVn;
                }

                fe.InputRefNo = _inputRefNoHadFoundCharge;
            });
            return chargeGroupByPartner;
        }
        #endregion --- VOUCHER ---

        #region --- CREATE/UPDATE ---
        public HandleState AddAcctMgnt(AccAccountingManagementModel model)
        {
            try
            {
                model.Id = Guid.NewGuid();
                model.Status = "New";
                model.UserCreated = model.UserModified = currentUser.UserID;
                model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                model.GroupId = currentUser.GroupId;
                model.DepartmentId = currentUser.DepartmentId;
                model.OfficeId = currentUser.OfficeID;
                model.CompanyId = currentUser.CompanyID;

                DateTime? dueDate = null;
                if (model.Date.HasValue)
                {
                    dueDate = model.Date.Value.AddDays(30 + (double)(model.PaymentTerm ?? 0));
                }
                model.PaymentDueDate = dueDate;
                model.PaymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;

                List<string> jobNoGrouped = model.Charges.GroupBy(x => x.JobNo, (x) => new { jobNo = x.JobNo }).Select(x => x.Key).ToList();
                model.ServiceType = GetTransactionType(jobNoGrouped);

                AccAccountingManagement accounting = mapper.Map<AccAccountingManagement>(model);

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        List<ChargeOfAccountingManagementModel> chargesOfAcct = model.Charges;
                        decimal _totalAmount = 0;
                        foreach (ChargeOfAccountingManagementModel chargeOfAcct in chargesOfAcct)
                        {
                            CsShipmentSurcharge charge = surchargeRepo.Get(x => x.Id == chargeOfAcct.SurchargeId).FirstOrDefault();
                            charge.AcctManagementId = accounting.Id;
                            charge.FinalExchangeRate = chargeOfAcct.ExchangeRate; //Cập nhật lại Final Exchange Rate

                            #region -- Tính lại giá trị các field: NetAmount, Total, AmountUsd, VatAmountUsd --                            
                            var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(charge);
                            charge.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                            charge.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                            charge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                            charge.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                            charge.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                            charge.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                            #endregion -- Tính lại giá trị các field: NetAmount, Total, AmountUsd, VatAmountUsd --

                            // CR: 14405
                            charge.VatAmountVnd = chargeOfAcct.VatAmountVnd;
                            charge.AmountVnd = chargeOfAcct.AmountVnd;

                            if (accounting.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                            {
                                charge.VoucherId = accounting.VoucherId;
                                charge.VoucherIddate = accounting.Date;

                                // CR: 14344
                                charge.InvoiceNo = chargeOfAcct.InvoiceNo;
                                charge.InvoiceDate = chargeOfAcct.InvoiceDate;
                                charge.SeriesNo = chargeOfAcct.Serie;
                            }
                            if (accounting.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                            {
                                charge.InvoiceNo = accounting.InvoiceNoReal;
                                charge.InvoiceDate = accounting.Date;
                                charge.SeriesNo = accounting.Serie; //Cập nhật lại Serie No cho charge
                                charge.VoucherIddate = accounting.Date; //Cập nhật VoucherDate
                                charge.VoucherId = accounting.VoucherId; //Cập nhật VoucherId
                            }
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = currentUser.UserID;

                            surchargeRepo.Update(charge, x => x.Id == charge.Id, false);

                            var soa = soaRepo.Get(x => x.Soano == charge.Soano || x.Soano == charge.PaySoano).FirstOrDefault();
                            if (soa != null)
                            {
                                //Cập nhật status cho SOA: Issued Invoice, Issued Voucher
                                UpdateStatusSOA(soa, accounting.Type);
                            }

                            _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, accounting.Currency);
                        }
                        // Cập nhật Settlement: VoucherNo, VoucherDate
                        if (accounting.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                        {

                            List<string> listSettlementCode = chargesOfAcct.Select(x => x.SettlementCode).Distinct().ToList();

                            if (listSettlementCode.Count() > 0)
                            {
                                foreach (string code in listSettlementCode)
                                {
                                    UpdateVoucherSettlement(code, accounting.VoucherId, accounting.Date);
                                }

                                settlementPaymentRepo.SubmitChanges();
                            }
                        }

                        //Tính toán total amount theo currency
                        accounting.TotalAmount = accounting.UnpaidAmount = _totalAmount;
                        HandleState hs = DataContext.Add(accounting);

                        surchargeRepo.SubmitChanges();
                        soaRepo.SubmitChanges();
                        trans.Commit();
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

        public HandleState UpdateAcctMngt(AccAccountingManagementModel model)
        {
            try
            {
                AccAccountingManagement acctCurrent = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                var accountingType = (acctCurrent.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE ? "Vat Invoice" : "Voucher");
                if (acctCurrent == null) return new HandleState("Not found " + accountingType);

                if (acctCurrent.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID || acctCurrent.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART)
                {
                    return new HandleState(acctCurrent.InvoiceNoReal + " Had Payment");
                }

                AccAccountingManagement accounting = mapper.Map<AccAccountingManagement>(model);

                accounting.UserModified = currentUser.UserID;
                accounting.DatetimeModified = DateTime.Now;

                // cập nhật lại payment status cho invoice này
                if (acctCurrent.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE && acctCurrent.Status == AccountingConstants.ACCOUNTING_INVOICE_STATUS_NEW)
                {
                    accounting.PaymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                }

                DateTime? dueDate = null;
                if (accounting.Date.HasValue)
                {
                    dueDate = accounting.Date.Value.AddDays(30 + (double)(accounting.PaymentTerm ?? 0));
                }
                accounting.PaymentDueDate = dueDate;

                // Cập nhật lại service
                List<string> jobNoGrouped = model.Charges.GroupBy(x => x.JobNo, (x) => new { jobNo = x.JobNo }).Select(x => x.Key).ToList();
                accounting.ServiceType = GetTransactionType(jobNoGrouped);

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        // Gỡ bỏ hết charge có tham chiếu tới Id Accounting
                        // Gỡ bỏ luôn các Invoice No, Invoice Date, Voucher No, Voucher Date
                        var surchargeOfAcctCurrent = surchargeRepo.Get(x => x.AcctManagementId == accounting.Id);
                        foreach (var surchargeOfAcct in surchargeOfAcctCurrent)
                        {
                            surchargeOfAcct.AcctManagementId = null;
                            surchargeOfAcct.FinalExchangeRate = null;
                            if (accounting.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                            {
                                surchargeOfAcct.VoucherId = null;
                                surchargeOfAcct.VoucherIddate = null;

                                // CR: 14344
                                surchargeOfAcct.InvoiceNo = null;
                                surchargeOfAcct.InvoiceDate = null;
                                surchargeOfAcct.SeriesNo = null;
                            }
                            if (accounting.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                            {
                                surchargeOfAcct.InvoiceNo = null;
                                surchargeOfAcct.InvoiceDate = null;
                                surchargeOfAcct.SeriesNo = null;
                                surchargeOfAcct.VoucherIddate = null;
                                surchargeOfAcct.VoucherId = null;
                            }
                            surchargeOfAcct.DatetimeModified = DateTime.Now;
                            surchargeOfAcct.UserModified = currentUser.UserID;
                            surchargeRepo.Update(surchargeOfAcct, x => x.Id == surchargeOfAcct.Id, false);
                        }

                        //Update lại
                        var chargesOfAcctUpdate = model.Charges;
                        decimal _totalAmount = 0;
                        foreach (var chargeOfAcct in chargesOfAcctUpdate)
                        {
                            var charge = surchargeRepo.Get(x => x.Id == chargeOfAcct.SurchargeId).FirstOrDefault();
                            charge.AcctManagementId = accounting.Id;
                            charge.FinalExchangeRate = chargeOfAcct.ExchangeRate; //Cập nhật lại Final Exchange Rate

                            #region -- Tính lại giá trị các field: NetAmount, Total, AmountUsd, VatAmountUsd --                            
                            var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(charge);
                            charge.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                            charge.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                            charge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                            charge.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                            charge.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                            charge.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                            #endregion -- Tính lại giá trị các field: NetAmount, Total, AmountUsd, VatAmountUsd --

                            // CR: 14405
                            charge.AmountVnd = chargeOfAcct.AmountVnd;
                            charge.VatAmountVnd = chargeOfAcct.VatAmountVnd;
                            if (accounting.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                            {
                                charge.VoucherId = accounting.VoucherId;
                                charge.VoucherIddate = accounting.Date;
                                // CR: 14344
                                charge.InvoiceNo = chargeOfAcct.InvoiceNo;
                                charge.InvoiceDate = chargeOfAcct.InvoiceDate;
                                charge.SeriesNo = chargeOfAcct.Serie;
                            }
                            if (accounting.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                            {
                                charge.InvoiceNo = accounting.InvoiceNoReal;
                                charge.InvoiceDate = accounting.Date;
                                charge.SeriesNo = accounting.Serie; //Cập nhật lại Serie No cho charge
                                charge.VoucherIddate = accounting.Date; //Cập nhật VoucherDate
                                charge.VoucherId = accounting.VoucherId; //Cập nhật VoucherId
                            }
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = currentUser.UserID;
                            surchargeRepo.Update(charge, x => x.Id == charge.Id, false);

                            var soa = soaRepo.Get(x => x.Soano == charge.Soano || x.Soano == charge.PaySoano).FirstOrDefault();
                            if (soa != null)
                            {
                                //Cập nhật status cho SOA: Issued Invoice, Issued Voucher
                                UpdateStatusSOA(soa, accounting.Type);
                            }

                            _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, accounting.Currency);
                        }

                        // Cập nhật Settlement: VoucherNo, VoucherDate
                        if (accounting.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
                        {

                            List<string> listSettlementCode = chargesOfAcctUpdate.Select(x => x.SettlementCode).Distinct().ToList();

                            if (listSettlementCode.Count() > 0)
                            {
                                foreach (string code in listSettlementCode)
                                {
                                    UpdateVoucherSettlement(code, accounting.VoucherId, accounting.Date);
                                }

                                settlementPaymentRepo.SubmitChanges();
                            }
                        }

                        //Tính toán total amount theo currency
                        accounting.TotalAmount = accounting.UnpaidAmount = _totalAmount;
                        var hs = DataContext.Update(accounting, x => x.Id == accounting.Id);
                        surchargeRepo.SubmitChanges();
                        soaRepo.SubmitChanges();
                        trans.Commit();

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
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        private void UpdateStatusSOA(AcctSoa soa, string typeAcctMngt)
        {
            soa.UserModified = currentUser.UserID;
            soa.DatetimeModified = DateTime.Now;
            if (typeAcctMngt == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
            {
                soa.Status = AccountingConstants.STATUS_SOA_ISSUED_VOUCHER;
            }
            if (typeAcctMngt == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
            {
                soa.Status = AccountingConstants.STATUS_SOA_ISSUED_INVOICE;
            }
            if (string.IsNullOrEmpty(typeAcctMngt))
            {
                soa.Status = "New";
            }
            soaRepo.Update(soa, x => x.Id == soa.Id, false);
        }

        private void UpdateStatusSoaAfterDeleteAcctMngt(string typeAcctMngt, CsShipmentSurcharge charge)
        {
            if (typeAcctMngt == AccountingConstants.ADVANCE_TYPE_INVOICE)
            {
                var soa = soaRepo.Get(x => x.Soano == charge.Soano || x.Soano == charge.PaySoano).FirstOrDefault();
                if (soa != null)
                {
                    //Tồn tại Voucher thì update Status là Issued Voucher
                    if ((!string.IsNullOrEmpty(charge.Soano) || !string.IsNullOrEmpty(charge.PaySoano)) && !string.IsNullOrEmpty(charge.VoucherId))
                    {
                        UpdateStatusSOA(soa, AccountingConstants.ACCOUNTING_VOUCHER_TYPE);
                    }
                    else
                    {
                        //Cập nhật status là NEW cho SOA
                        UpdateStatusSOA(soa, null);
                    }
                }
            }

            if (typeAcctMngt == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
            {
                var soa = soaRepo.Get(x => x.Soano == charge.Soano || x.Soano == charge.PaySoano).FirstOrDefault();
                if (soa != null)
                {
                    //Tồn tại Invoice thì update Status là Issued Invoice
                    if ((!string.IsNullOrEmpty(charge.Soano) || !string.IsNullOrEmpty(charge.PaySoano)) && !string.IsNullOrEmpty(charge.InvoiceNo))
                    {
                        UpdateStatusSOA(soa, AccountingConstants.ACCOUNTING_INVOICE_TYPE);
                    }
                    else
                    {
                        //Cập nhật status là NEW cho SOA
                        UpdateStatusSOA(soa, null);
                    }
                }
            }
        }

        private decimal CaculatorTotalAmount(AccAccountingManagementModel model)
        {
            decimal total = 0;
            if (!string.IsNullOrEmpty(model.Currency))
            {
                if (model.Currency == AccountingConstants.CURRENCY_LOCAL)
                {
                    total = model.Charges.Sum(x => x.AmountVnd + x.VatAmountVnd ?? 0);
                }
                else
                {
                    model.Charges.ForEach(fe =>
                    {
                        decimal exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(fe.FinalExchangeRate, fe.ExchangeDate, fe.Currency, model.Currency);
                        total += (exchangeRate * (fe.OrgVatAmount + fe.OrgAmount)) ?? 0;
                    });
                }
            }
            return total;
        }
        
        /// <summary>
        /// Generate Voucher ID
        /// </summary>
        /// <param name="acctMngtType">Invoice or Voucher</param>
        /// <param name="voucherType">Voucher Type of Voucher</param>
        /// <returns></returns>
        public string GenerateVoucherId(string acctMngtType, string voucherType)
        {
            if (string.IsNullOrEmpty(acctMngtType)) return string.Empty;
            int monthCurrent = DateTime.Now.Month;
            string year = DateTime.Now.Year.ToString();
            string month = monthCurrent.ToString().PadLeft(2, '0');//Nếu tháng < 10 thì gắn thêm số 0 phía trước, VD: 09
            string no = "001";

            IQueryable<string> voucherNewests = null;
            string _prefixVoucher = string.Empty;
            if (acctMngtType == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
            {
                _prefixVoucher = "FDT";
            }
            else if (acctMngtType == AccountingConstants.ACCOUNTING_VOUCHER_TYPE)
            {
                if (string.IsNullOrEmpty(voucherType)) return string.Empty;
                _prefixVoucher = GetPrefixVoucherByVoucherType(voucherType);
            }
            voucherNewests = Get(x => x.Type == acctMngtType && x.VoucherId.Contains(_prefixVoucher) && x.VoucherId.Substring(0, 4) == year && x.VoucherId.Substring(11, 2) == month)
                .OrderByDescending(o => o.VoucherId).Select(s => s.VoucherId);

            string voucherNewest = voucherNewests.FirstOrDefault();
            if (!string.IsNullOrEmpty(voucherNewest))
            {
                var _noNewest = voucherNewest.Substring(7, 3);
                if (_noNewest != "999")
                {
                    no = (Convert.ToInt32(_noNewest) + 1).ToString();
                    no = no.PadLeft(3, '0');
                }
            }
            string voucher = year + _prefixVoucher + no + "/" + month;
            return voucher;
        }

        private string GetPrefixVoucherByVoucherType(string voucherType)
        {
            string _prefixVoucher = string.Empty;
            if (string.IsNullOrEmpty(voucherType)) return _prefixVoucher;
            switch (voucherType.Trim().ToUpper())
            {
                case "CASH RECEIPT":
                    _prefixVoucher = "FPT";
                    break;
                case "CASH PAYMENT":
                    _prefixVoucher = "FPC";
                    break;
                case "DEBIT SLIP":
                    _prefixVoucher = "FBN";
                    break;
                case "CREDIT SLIP":
                    _prefixVoucher = "FBC";
                    break;
                case "PURCHASING NOTE":
                    _prefixVoucher = "FNM";
                    break;
                case "OTHER ENTRY":
                    _prefixVoucher = "FPK";
                    break;
                default:
                    _prefixVoucher = string.Empty;
                    break;
            }
            return _prefixVoucher;
        }

        public string GenerateInvoiceNoTemp()
        {
            string no = "0000001";
            int outNum = 0;
            // var invoiceNos = Get(x => int.TryParse(x.InvoiceNoTempt, out outNum)).OrderByDescending(o => o.InvoiceNoTempt).Select(s => s.InvoiceNoTempt);
            var invoiceNos = Get(x => int.TryParse(x.InvoiceNoTempt, out outNum)).Select(s => s.InvoiceNoTempt.PadLeft(7, '0')).OrderByDescending(o => o);
            var invoiceNoNewest = invoiceNos.FirstOrDefault();
            if (!string.IsNullOrEmpty(invoiceNoNewest))
            {
                no = (Convert.ToInt32(invoiceNoNewest) + 1).ToString();
                no = no.PadLeft(7, '0');
            }
            return no;
        }

        private string GetTransactionType(List<string> jobNos)
        {
            List<string> listTransactionType = new List<string>();
            if (jobNos.Count() > 0)
            {
                foreach (string jobNo in jobNos)
                {
                    IQueryable<CsTransaction> docTransaction = csTransactionRepo.Get(x => x.JobNo == jobNo);
                    if (docTransaction.Count() > 0)
                    {
                        listTransactionType.Add(docTransaction?.FirstOrDefault()?.TransactionType);
                    }
                    else
                    {
                        IQueryable<OpsTransaction> opsTransaction = opsTransactionRepo.Get(x => x.JobNo == jobNo);
                        listTransactionType.Add("CL");
                    }
                }
            }
            if (listTransactionType.Count() > 0)
            {
                List<string> transactionDistint = new List<string>(listTransactionType.Distinct());
                return transactionDistint.Aggregate((i, j) => i + ";" + j);
            }

            return string.Empty;
        }
        #endregion -- CREATE/UPDATE ---

        #region --- DETAIL ---
        public AccAccountingManagementModel GetById(Guid id)
        {
            var accounting = DataContext.Get(x => x.Id == id).FirstOrDefault();
            var result = mapper.Map<AccAccountingManagementModel>(accounting);
            if (accounting != null)
            {
                Expression<Func<ChargeOfAccountingManagementModel, bool>> expressionQuery = chg => chg.AcctManagementId == id;
                if (accounting.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                {
                    result.Charges = GetChargeSellForInvoice(expressionQuery);
                }
                else
                {
                    result.Charges = GetChargeForVoucher(expressionQuery);
                }

                result.UserNameCreated = userRepo.Where(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
                result.UserNameModified = userRepo.Where(x => x.Id == result.UserModified).FirstOrDefault()?.Username;
            }
            return result;
        }

        public AccAccountingManagementModel GetAcctMngtById(Guid id)
        {
            var detail = GetById(id);
            if (detail == null) return null;
            ICurrentUser _currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.accManagement);

            var permissionRangeWrite = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Write);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Delete);
            detail.Permission = new PermissionAllowBase
            {
                AllowUpdate = GetPermissionToAction(detail, permissionRangeWrite, _currentUser) != 403 ? true : false,
                AllowDelete = GetPermissionToAction(detail, permissionRangeDelete, _currentUser) != 403 ? true : false
            };
            return detail;
        }

        public int CheckDetailPermission(Guid id)
        {
            var detail = GetById(id);
            ICurrentUser _currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.accManagement);
            var permissionRangeDetail = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Detail);
            int code = GetPermissionToAction(detail, permissionRangeDetail, _currentUser);
            return code;
        }

        #endregion --- DETAIL ---

        #region --- EXPORT ---
        private string GetCustomNoOldOfShipment(string jobNo)
        {
            var LookupCustomDeclaration = customsDeclarationRepo.Get().ToLookup(x => x.JobNo);
            var customNos = LookupCustomDeclaration[jobNo].OrderBy(o => o.DatetimeModified).Select(s => s.ClearanceNo).FirstOrDefault();
            return customNos;
        }

        public List<AccountingManagementExport> GetDataAcctMngtExport(AccAccountingManagementCriteria criteria)
        {
            var query = ExpressionQuery(criteria);
            var accountings = GetAcctMngtPermission().Where(query);
            var data = new List<AccountingManagementExport>();
            var partners = partnerRepo.Get();
            var LookupPartner = partners.ToLookup(x => x.Id);
            var charges = chargeRepo.Get();
            var LookupCharge = charges.ToLookup(x => x.Id);
            var surchargeList = surchargeRepo.Get();
            var LookupSurcharge = surchargeList.ToLookup(x => x.AcctManagementId);
            var chargeDefaultList = chargeDefaultRepo.Get();
            var LookupChargeDefault = chargeDefaultList.ToLookup(x => x.ChargeId);
            var unitList = unitRepo.Get();
            var LookupUnit = unitList.ToLookup(x => x.Id);
            foreach (var acct in accountings)
            {
                var surcharges = LookupSurcharge[acct.Id];
                var partnerAcct = LookupPartner[acct.PartnerId].FirstOrDefault();

                foreach (var surcharge in surcharges)
                {
                    var _charge = LookupCharge[surcharge.ChargeId].FirstOrDefault();
                    var _chargeId = _charge?.Id ?? Guid.Empty;
                    var _productDept = _charge?.ProductDept;
                    var _chargeDefault = LookupChargeDefault[_chargeId].FirstOrDefault();
                    string _deptCode = string.Empty;
                    
                    if (!string.IsNullOrEmpty(_productDept))
                    {
                        _deptCode = _productDept;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(surcharge.JobNo))
                        {
                            if (surcharge.JobNo.Contains("LOG"))
                            {
                                //_deptCode = "OPS";
                                _deptCode = "ITLOPS";
                            }
                            else if (surcharge.JobNo.Contains("A"))
                            {
                                //_deptCode = "AIR";
                                _deptCode = "ITLAIR";
                            }
                            else if (surcharge.JobNo.Contains("S"))
                            {
                                //_deptCode = "SEA";
                                _deptCode = "ITLCS";
                            }
                        }
                    }
                    string _paymentMethod = string.Empty;
                    if (!string.IsNullOrEmpty(acct.PaymentMethod))
                    {
                        if (acct.PaymentMethod.Equals("Cash"))
                        {
                            _paymentMethod = "TM";
                        }
                        else if (acct.PaymentMethod.Equals("Bank Transfer"))
                        {
                            _paymentMethod = "CK";
                        }
                        else if (acct.PaymentMethod.Equals("Bank Transfer / Cash"))
                        {
                            _paymentMethod = "CK/TM";
                        }
                    }
                    string _statusInvoice = string.Empty;
                    if (acct.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
                    {
                        if (acct.Status != "New")
                        {
                            _statusInvoice = "Đã phát hành";
                        }
                    }
                    var item = new AccountingManagementExport();
                    item.SurchargeId = surcharge.Id;
                    item.ChargeId = surcharge.ChargeId;
                    item.ChargeCode = _charge.Code;
                    item.ChargeName = _charge.ChargeNameVn;
                    item.JobNo = surcharge.JobNo;
                    item.Hbl = surcharge.Hblno;
                    item.OrgAmount = surcharge.NetAmount;
                    item.Vat = surcharge.Vatrate;
                    item.OrgVatAmount = surcharge.Total - surcharge.NetAmount;

                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL)
                    {
                        item.VatAccount = _chargeDefault?.CreditVat;
                    }
                    else if(surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY)
                    {
                        item.VatAccount = _chargeDefault?.DebitVat;
                    } else
                    {
                        item.VatAccount = _chargeDefault?.CreditVat ?? _chargeDefault?.DebitVat;
                    }

                    item.Currency = surcharge.CurrencyId;
                    item.ExchangeDate = surcharge.ExchangeDate;
                    item.FinalExchangeRate = surcharge.FinalExchangeRate;
                    item.ExchangeRate = surcharge.FinalExchangeRate;
                    item.AmountVnd = surcharge.AmountVnd;
                    item.VatAmountVnd = surcharge.VatAmountVnd;

                    string _vatPartnerId = null;
                    CatPartner _obhPartner = null;
                    string _cdNoteCode = string.Empty;
                    string _soaNo = string.Empty;
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                    {
                        _vatPartnerId = surcharge.PayerId;
                        _obhPartner = LookupPartner[surcharge.PaymentObjectId].FirstOrDefault();
                        _cdNoteCode = surcharge.CreditNo;
                        _soaNo = surcharge.PaySoano;
                    }
                    else
                    {
                        _vatPartnerId = surcharge.PaymentObjectId;
                        _cdNoteCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? surcharge.DebitNo : surcharge.CreditNo;
                        _soaNo = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? surcharge.Soano : surcharge.PaySoano;
                    }
                    var _vatPartner = LookupPartner[_vatPartnerId].FirstOrDefault();

                    item.VatPartnerId = _vatPartnerId;
                    item.VatPartnerCode = _vatPartner?.TaxCode; //Taxcode
                    item.VatPartnerName = _vatPartner?.ShortName;
                    item.VatPartnerAddress = _vatPartner?.AddressVn;
                    item.ObhPartnerCode = _obhPartner?.TaxCode; //Taxcode ObhPartner
                    item.ObhPartner = _obhPartner?.ShortName; //Abbr ObhPartner
                    item.InvoiceNo = surcharge.InvoiceNo;
                    item.Serie = surcharge.SeriesNo;
                    item.InvoiceDate = surcharge.InvoiceDate;
                    item.CdNoteNo = _cdNoteCode;
                    item.Qty = surcharge.Quantity;

                    var _unit = LookupUnit[surcharge.UnitId].FirstOrDefault();
                    item.UnitName = _unit?.UnitNameVn;

                    item.UnitPrice = surcharge.UnitPrice;
                    item.Mbl = surcharge.Mblno;
                    item.SoaNo = _soaNo;
                    item.SettlementCode = surcharge.SettlementCode;
                    item.AcctManagementId = surcharge.AcctManagementId;

                    item.Date = acct.Date; //Date trên VAT Invoice Or Voucher
                    item.VoucherId = acct.VoucherId; //VoucherId trên VAT Invoice Or Voucher
                    item.PartnerId = partnerAcct?.AccountNo; //Partner ID trên VAT Invoice Or Voucher

                    item.AccountNo = string.Empty;//acct.AccountNo; //Account No trên VAT Invoice Or Voucher
                    item.ContraAccount = string.Empty;
                    item.TkNoVat = string.Empty;
                    item.TkCoVat = string.Empty;
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL) //Debit charge
                    {
                        item.AccountNo = acct.AccountNo;
                        item.ContraAccount = _chargeDefault?.CreditAccountNo;
                        item.TkNoVat = acct.AccountNo;
                        item.TkCoVat = item.VatAccount;
                    }
                    else //Credit hoặc OBH charge
                    {
                        item.AccountNo = _chargeDefault?.DebitAccountNo;
                        item.ContraAccount = acct.AccountNo;
                        item.TkNoVat = item.VatAccount;
                        item.TkCoVat = acct.AccountNo;
                    }

                    item.VatPartnerNameEn = _vatPartner?.PartnerNameEn; //Partner Name En của Charge
                    item.VatPartnerNameVn = _vatPartner?.PartnerNameVn; //Partner Name Local của Charge
                    item.Description = acct.Description;
                    item.IsTick = true; //Default is True
                    item.PaymentTerm = acct.PaymentTerm ?? 0; //Default is 0                    
                    item.DepartmentCode = _deptCode;
                    item.CustomNo = surcharge.ClearanceNo;//GetCustomNoOldOfShipment(surcharge.JobNo); //Xem xét sau
                    item.PaymentMethod = _paymentMethod;
                    item.StatusInvoice = _statusInvoice; //Tình trạng hóa đơn (Dùng cho Invoice)
                    item.VatPartnerEmail = _vatPartner?.Email; //Email Partner của charge
                    item.ReleaseDateEInvoice = null;
                    item.Vat = item.Vat ?? 0;

                    data.Add(item);
                }
            }

            return data;
        }


        #endregion --- EXPORT ---

        #region --- IMPORT ---
        public List<AcctMngtVatInvoiceImportModel> CheckVatInvoiceImport(List<AcctMngtVatInvoiceImportModel> list)
        {
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.VoucherId))
                {
                    item.VoucherId = stringLocalizer[AccountingLanguageSub.MSG_VOUCHER_ID_EMPTY];
                    item.IsValid = false;
                }
                else
                {

                    // Danh sách có 2 dòng Voucher ID giống nhau
                    if (list.Count(x => x.VoucherId?.ToLower() == item.VoucherId?.ToLower()) > 1)
                    {
                        item.VoucherId = string.Format(stringLocalizer[AccountingLanguageSub.MSG_VOUCHER_ID_DUPLICATE], item.VoucherId);
                        item.IsValid = false;
                    }
                    else
                    {
                        IQueryable<AccAccountingManagement> invoices = DataContext.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE);
                        // Không tìm thấy voucher ID
                        if (!invoices.Any(x => x.VoucherId == item.VoucherId))
                        {
                            item.VoucherId = stringLocalizer[AccountingLanguageSub.MSG_VOUCHER_ID_NOT_EXIST, item.VoucherId];
                            item.IsValid = false;
                        }
                        // Voucher ID đã thanh toán hết
                        if (invoices.Any(x =>
                        x.VoucherId == item.VoucherId
                        && !string.IsNullOrEmpty(x.PaymentStatus)
                        && x.PaymentStatus != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID))
                        {
                            item.VoucherId = stringLocalizer[AccountingLanguageSub.MSG_VOUCHER_ID_HAD_PAYMENT, item.VoucherId];
                            item.IsValid = false;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(item.RealInvoiceNo))
                            {
                                item.RealInvoiceNo = stringLocalizer[AccountingLanguageSub.MSG_INVOICE_NO_NOT_EMPTY];
                                item.IsValid = false;
                            }
                            if (string.IsNullOrEmpty(item.SerieNo))
                            {
                                item.SerieNo = stringLocalizer[AccountingLanguageSub.MSG_SERIE_NO_NOT_EMPTY];
                                item.IsValid = false;
                            }
                            if (!string.IsNullOrEmpty(item.PaymentStatus) && item.PaymentStatus.ToLower() != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID.ToLower())
                            {
                                item.PaymentStatus = stringLocalizer[AccountingLanguageSub.MSG_PAYMENT_STATUS_INVALID, item.PaymentStatus];
                                item.IsValid = false;
                            }
                            else
                            {
                                // Trùng Invoice, Serie #
                                if (CheckExistedInvoiceNoTempSerie(item.RealInvoiceNo, item.SerieNo, item.VoucherId))
                                {
                                    item.RealInvoiceNo = stringLocalizer[AccountingLanguageSub.MSG_INVOICE_NO_EXISTED, item.RealInvoiceNo];
                                    item.SerieNo = stringLocalizer[AccountingLanguageSub.MSG_SERIE_NO_EXISTED, item.SerieNo];
                                    item.IsValid = false;
                                }
                            }
                        }
                    }
                }
            });
            return list;
        }

        public ResultHandle ImportVatInvoice(List<AcctMngtVatInvoiceImportModel> list)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (AcctMngtVatInvoiceImportModel item in list)
                    {
                        AccAccountingManagement vatInvoice = DataContext.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE && x.VoucherId == item.VoucherId)?.FirstOrDefault();

                        if (vatInvoice.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID)
                        {
                            continue;
                        }
                        vatInvoice.InvoiceNoTempt = vatInvoice.InvoiceNoReal = item.RealInvoiceNo;
                        vatInvoice.Serie = item.SerieNo;
                        vatInvoice.Date = item.InvoiceDate;
                        vatInvoice.Status = AccountingConstants.ACCOUNTING_INVOICE_STATUS_UPDATED;
                        vatInvoice.UserModified = currentUser.UserID;
                        vatInvoice.DatetimeModified = DateTime.Now;

                        DateTime? dueDate = null;
                        if (vatInvoice.Date.HasValue)
                        {
                            dueDate = vatInvoice.Date.Value.AddDays(30 + (double)(vatInvoice.PaymentTerm ?? 0));
                        }
                        vatInvoice.PaymentDueDate = dueDate;
                        vatInvoice.PaymentDatetimeUpdated = DateTime.Now;

                        // Handle PaymentStatus
                        if (!string.IsNullOrEmpty(item.PaymentStatus) && item.PaymentStatus.ToLower() == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID.ToLower())
                        {
                            // Tạo 1 paymentNo cho AR
                            AccAccountingPayment payment = new AccAccountingPayment
                            {
                                Id = Guid.NewGuid(),
                                RefId = vatInvoice.Id.ToString(),
                                PaymentNo = item.RealInvoiceNo + "_" + string.Format("{0:00}", 1),
                                PaymentAmount = vatInvoice.TotalAmount,
                                Balance = 0,
                                CurrencyId = vatInvoice.Currency,
                                PaymentType = AccountingConstants.ACCOUNTING_PAYMENT_TYPE_NORMAL,
                                PaidDate = item.InvoiceDate,
                                Type = "INVOICE",
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                GroupId = currentUser.GroupId,
                                DepartmentId = currentUser.DepartmentId,
                                OfficeId = currentUser.OfficeID,
                                CompanyId = currentUser.CompanyID
                            };

                            // Cập nhật lại Status, số tiền thanh toán cho invoice này
                            vatInvoice.PaymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                            vatInvoice.PaidAmount = vatInvoice.TotalAmount;
                            vatInvoice.PaymentDatetimeUpdated = DateTime.Now;

                            accountingPaymentRepository.Add(payment, false);
                        }

                        IQueryable<CsShipmentSurcharge> surchargeOfAcctCurrent = surchargeRepo.Get(x => x.AcctManagementId == vatInvoice.Id);

                        if (surchargeOfAcctCurrent != null)
                        {
                            foreach (var surcharge in surchargeOfAcctCurrent)
                            {
                                surcharge.InvoiceNo = item.RealInvoiceNo;
                                surcharge.InvoiceDate = item.InvoiceDate;
                                surcharge.SeriesNo = item.SerieNo;

                                surcharge.DatetimeModified = DateTime.Now;
                                surcharge.UserModified = currentUser.UserID;

                                surchargeRepo.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }
                        }

                        DataContext.Update(vatInvoice, x => x.VoucherId == item.VoucherId, false);

                    }
                    var hs = surchargeRepo.SubmitChanges();
                    var t = DataContext.SubmitChanges();
                    var ty = accountingPaymentRepository.SubmitChanges();

                    trans.Commit();

                    return new ResultHandle { Status = true, Message = "Import Vat Invoice successfully" };

                }
                catch (Exception ex)
                {
                    HandleState result = new HandleState(ex.Message);
                    return new ResultHandle { Data = new object { }, Message = ex.Message, Status = true };
                }
                finally
                {
                    trans.Dispose();
                }
            }

        }
        #endregion --- IMPORT ---

        private int GetPermissionToAction(AccAccountingManagementModel model, PermissionRange permissionRange, ICurrentUser currentUser)
        {
            int code = 403;
            switch (permissionRange)
            {
                case PermissionRange.All:
                    code = 200;
                    break;
                case PermissionRange.Owner:
                    if (model.UserCreated == currentUser.UserID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId == currentUser.GroupId
                        && model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
            }
            return code;
        }

        private bool CheckExistedInvoiceNoTempSerie(string invoiceNoTemp, string serie, string voucherId)
        {
            bool isExited = false;

            isExited = DataContext.Get(x => x.InvoiceNoTempt == invoiceNoTemp && x.Serie == serie && x.VoucherId != voucherId && x.Type == AccountingConstants.ADVANCE_TYPE_INVOICE).Any();

            return isExited;
        }

        public CatContractInvoiceModel GetContractForInvoice(AccMngtContractInvoiceCriteria model)
        {
            string acRef = null;
            CatContractInvoiceModel result = new CatContractInvoiceModel { };

            CatPartner partner = partnerRepo.Get(p => p.Id == model.PartnerId)?.FirstOrDefault();
            if (partner == null)
            {
                return result;
            }

            if (!string.IsNullOrEmpty(partner.ParentId))
            {
                acRef = partner.ParentId;
            }
            else
            {
                acRef = partner.Id; // Đối tượng công nợ là chính nó
            }
            CatPartner partnerRef = partnerRepo.Get(p => p.Id == acRef)?.FirstOrDefault();


            Expression<Func<CatContract, bool>> queryContractByCriteria = null;
            queryContractByCriteria = x => (
            (x.OfficeId ?? "").Contains(model.Office ?? "", StringComparison.OrdinalIgnoreCase)
            && (x.SaleService.Contains(model.Service ?? "", StringComparison.OrdinalIgnoreCase)
            && x.PartnerId == partnerRef.Id));

            IQueryable<CatContract> agreements = catContractRepository.Get(queryContractByCriteria);

            if (agreements != null && agreements.Count() > 0)
            {
                result.ContractNo = agreements.FirstOrDefault().ContractNo;
                result.ContractType = agreements.FirstOrDefault().ContractType;
                result.PaymentTerm = agreements.FirstOrDefault().PaymentTerm;
            }

            return result;
        }

        public List<Guid> GetSurchargeIdByAcctMngtId(Guid? acctMngt)
        {
            var surchargeIds = surchargeRepo.Get(x => x.AcctManagementId == acctMngt).Select(s => s.Id).ToList();
            return surchargeIds;
        }

        public ChargeAccountingMngtTotalViewModel CalculateListChargeAccountingMngt(List<ChargeOfAccountingManagementModel> charges)
        {
            ChargeAccountingMngtTotalViewModel result = new ChargeAccountingMngtTotalViewModel();
            if (charges.Count() > 0)
            {
                foreach (ChargeOfAccountingManagementModel charge in charges)
                {
                    charge.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(charge.FinalExchangeRate, charge.ExchangeDate, charge.Currency, AccountingConstants.CURRENCY_LOCAL);
                    charge.OrgVatAmount = (charge.Vat != null) ? (charge.Vat < 101 & charge.Vat >= 0) ? NumberHelper.RoundNumber(((charge.OrgAmount * charge.Vat) / 100 ?? 0), 3) : Math.Abs(charge.Vat ?? 0) : 0;
                    charge.AmountVnd = NumberHelper.RoundNumber((charge.OrgAmount ?? 0) * (charge.ExchangeRate ?? 0));
                    charge.VatAmountVnd = NumberHelper.RoundNumber((charge.OrgVatAmount ?? 0) * (charge.ExchangeRate ?? 0));
                }
                result.Charges = charges;
                result.TotalAmountVat = charges.Sum(x => x.VatAmountVnd);
                result.TotalAmountVnd = charges.Sum(x => x.AmountVnd);
                result.TotalAmount = result.TotalAmountVat + result.TotalAmountVnd;
            }

            return result;
        }

        public void UpdateVoucherSettlement(string _settleCode, string _voucherNo, DateTime? _voucherDate)
        {
            AcctSettlementPayment settlement = settlementPaymentRepo.Get(x => x.SettlementNo == _settleCode).FirstOrDefault();
            if (settlement != null)
            {
                settlement.VoucherDate = _voucherDate;
                settlement.VoucherNo = _voucherNo;
                settlement.UserModified = currentUser.UserID;
                settlement.DatetimeModified = DateTime.Now;
            }

            settlementPaymentRepo.Update(settlement, x => x.Id == settlement.Id, false);
        }

        private string GetPayeeIdFromSettlement(string settleCode)
        {
            string payeeId = null;

            AcctSettlementPayment settlement = settlementPaymentRepo.Get(x => x.SettlementNo == settleCode).FirstOrDefault();
            if (settlement != null)
            {
                payeeId = settlement.Payee;
            }

            return payeeId;
        }
    }
}
