using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccountingService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingService
    {
        #region --Dependencies--
        private readonly ICurrentUser currentUser;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<AcctAdvancePayment> AdvanceRepository;
        private readonly IContextBase<AcctAdvanceRequest> AdvanceRequestRepository;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<SysUser> UserRepository;
        private readonly IContextBase<SysEmployee> EmployeeRepository;
        private readonly IContextBase<SysOffice> SysOfficeRepository;
        private readonly IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo;
        private readonly IContextBase<CatPartner> PartnerRepository;
        private readonly IContextBase<CsShipmentSurcharge> SurchargeRepository;
        private readonly IContextBase<CatCharge> CatChargeRepository;
        private readonly IContextBase<CatChargeDefaultAccount> CatChargeDefaultRepository;
        private readonly IContextBase<AcctSettlementPayment> SettlementRepository;
        private readonly IContextBase<CatUnit> CatUnitRepository;
        private readonly IContextBase<AcctCdnote> cdNoteRepository;
        private readonly IContextBase<AcctSoa> soaRepository;
        private readonly IContextBase<AccAccountingPayment> accountingPaymentRepository;
        private readonly IContextBase<CsTransactionDetail> csTransactionDetailRepository;
        private readonly IContextBase<SysNotifications> sysNotifyRepository;
        private readonly IContextBase<SysUserNotification> sysUserNotifyRepository;
        private readonly IOptions<WebUrl> webUrl;
        private readonly IOptions<ApiUrl> apiUrl;
        private readonly IContextBase<SysSentEmailHistory> sentEmailHistoryRepo;
        private readonly IContextBase<CatDepartment> departmentRepo;
        private readonly IContextBase<SysUserLevel> sysUserLevelRepo;
        private readonly IContextBase<AcctReceipt> receiptRepository;
        private readonly IContextBase<CatContract> contractRepository;
        private readonly IContextBase<CatPartnerEmail> partnerEmailRepository;
        private readonly IContextBase<CustomsDeclaration> customsDeclarationRepository;
        private readonly IUserBaseService userBaseService;
        #endregion --Dependencies--

        readonly IQueryable<SysUser> users;
        readonly IQueryable<SysEmployee> employees;
        readonly IQueryable<SysOffice> offices;
        readonly IQueryable<CatPartner> partners;
        readonly IQueryable<CatPartner> obhPartners;
        readonly IQueryable<CatCharge> charges;
        readonly IQueryable<CatChargeDefaultAccount> chargesDefault;
        readonly IQueryable<CatUnit> catUnits;


        public AccountingService(
            ICurrencyExchangeService exchangeService,
            IContextBase<SysUser> UserRepo,
            IContextBase<SysOffice> SysOfficeRepo,
            IContextBase<SysEmployee> EmployeeRepo,
            IContextBase<AcctAdvancePayment> AdvanceRepo,
            IContextBase<AcctSettlementPayment> SettlementRepo,
            IContextBase<AccAccountingManagement> repository,
            IContextBase<AcctAdvanceRequest> AdvanceRequestRepo,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<CatPartner> PartnerRepo,
            IContextBase<CsShipmentSurcharge> SurchargeRepo,
            IContextBase<CatCurrencyExchange> catCurrencyExchange,
            IContextBase<CatCharge> CatChargeRepo,
            IContextBase<CatChargeDefaultAccount> CatChargeDefaultRepo,
            IContextBase<CatUnit> CatUnitRepo,
            IContextBase<AcctCdnote> acctCdNote,
            IContextBase<AcctSoa> acctSoa,
            IContextBase<AccAccountingPayment> accAccountingPayment,
            IContextBase<CsTransactionDetail> csTransactionDetailRepo,
            IContextBase<SysNotifications> sysNotifyRepo,
            IContextBase<SysUserNotification> sysUserNotifyRepo,
            IOptions<WebUrl> wUrl,
            IOptions<ApiUrl> aUrl,
            IContextBase<SysSentEmailHistory> sentEmailHistory,
            IContextBase<CatDepartment> catDepartment,
            IContextBase<SysUserLevel> sysUserLevel,
            IContextBase<AcctReceipt> receiptRepo,
            IContextBase<CatContract> contractRepo,
            IContextBase<CatPartnerEmail> partnerEmailRepo,
            IContextBase<CustomsDeclaration> customsDeclarationRepo,
            IUserBaseService userBase,
            ICurrentUser cUser,
            IMapper mapper) : base(repository, mapper)
        {
            AdvanceRepository = AdvanceRepo;
            stringLocalizer = localizer;
            AdvanceRequestRepository = AdvanceRequestRepo;
            UserRepository = UserRepo;
            SysOfficeRepository = SysOfficeRepo;
            EmployeeRepository = EmployeeRepo;
            currencyExchangeService = exchangeService;
            catCurrencyExchangeRepo = catCurrencyExchange;
            PartnerRepository = PartnerRepo;
            SurchargeRepository = SurchargeRepo;
            CatChargeRepository = CatChargeRepo;
            CatChargeDefaultRepository = CatChargeDefaultRepo;
            SettlementRepository = SettlementRepo;
            CatUnitRepository = CatUnitRepo;
            cdNoteRepository = acctCdNote;
            soaRepository = acctSoa;
            currentUser = cUser;
            accountingPaymentRepository = accAccountingPayment;
            csTransactionDetailRepository = csTransactionDetailRepo;
            sysNotifyRepository = sysNotifyRepo;
            sysUserNotifyRepository = sysUserNotifyRepo;
            webUrl = wUrl;
            apiUrl = aUrl;
            sentEmailHistoryRepo = sentEmailHistory;
            departmentRepo = catDepartment;
            sysUserLevelRepo = sysUserLevel;
            receiptRepository = receiptRepo;
            userBaseService = userBase;
            contractRepository = contractRepo;
            partnerEmailRepository = partnerEmailRepo;
            customsDeclarationRepository = customsDeclarationRepo;
            // ---

            users = UserRepository.Get();
            employees = EmployeeRepository.Get();
            offices = SysOfficeRepository.Get();
            partners = PartnerRepository.Get();
            obhPartners = PartnerRepository.Get();
            charges = CatChargeRepository.Get();
            chargesDefault = CatChargeDefaultRepository.Get();
            catUnits = CatUnitRepository.Get();
        }

        public List<BravoAdvanceModel> GetListAdvanceToSyncBravo(List<Guid> Ids)
        {
            List<BravoAdvanceModel> result = new List<BravoAdvanceModel>();

            if (Ids.Count > 0)
            {
                IQueryable<AcctAdvancePayment> adv = AdvanceRepository.Get(x => Ids.Contains(x.Id) && x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);

                IQueryable<BravoAdvanceModel> queryAdv = from ad in adv
                                                         join u in users on ad.Requester equals u.Id
                                                         join employee in employees on u.EmployeeId equals employee.Id
                                                         join office in offices on ad.OfficeId equals office.Id
                                                         select new BravoAdvanceModel
                                                         {
                                                             Stt = ad.Id,
                                                             ReferenceNo = ad.AdvanceNo,
                                                             CurrencyCode = ad.AdvanceCurrency,
                                                             Description0 = ad.AdvanceNote,
                                                             CustomerName = employee.EmployeeNameVn,
                                                             //CustomerCode = !string.IsNullOrEmpty(employee.PersonalId) ? employee.PersonalId : employee.StaffCode,
                                                             CustomerCode = employee.StaffCode, //Chỉ lấy StaffCode [06/01/2021]
                                                             OfficeCode = office.Code,
                                                             DocDate = ad.RequestDate.HasValue ? ad.RequestDate.Value.Date : ad.RequestDate, //Chỉ lấy Date (không lấy Time)
                                                             ExchangeRate = GetExchangeRate(ad.RequestDate, ad.AdvanceCurrency),
                                                             DueDate = ad.PaymentTerm,
                                                             PaymentMethod = ad.PaymentMethod == "Bank" ? "Bank Transfer" : ad.PaymentMethod
                                                         };
                List<BravoAdvanceModel> data = queryAdv.ToList();
                foreach (var item in data)
                {
                    // Ds advance request
                    List<BravoAdvanceRequestModel> advRGrps = AdvanceRequestRepository
                        .Get(x => x.AdvanceNo == item.ReferenceNo)
                        .GroupBy(x => new { x.Hblid })
                        .Select(x => new BravoAdvanceRequestModel
                        {
                            RowId = x.First().Id,
                            Ma_SpHt = x.First().JobId,
                            BillEntryNo = x.First().Hbl,
                            MasterBillNo = x.First().Mbl,
                            OriginalAmount = x.Sum(d => d.Amount),
                            DeptCode = GetDeptCode(x.First().JobId),
                            Description = GetCustomerHBL(x.Key.Hblid) + " " + x.First().JobId + " " + x.First().Hbl,
                        }).ToList();

                    if (advRGrps.Count > 0)
                    {
                        item.Details = advRGrps;
                    }
                }
                result = data;
            }
            return result;
        }

        public List<BravoVoucherModel> GetListVoucherToSyncBravo(List<Guid> Ids)
        {
            List<BravoVoucherModel> result = new List<BravoVoucherModel>();

            if (Ids.Count > 0)
            {
                // Get List voucher
                IQueryable<AccAccountingManagement> vouchers = DataContext.Get(x => x.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE && Ids.Contains(x.Id));

                IQueryable<BravoVoucherModel> queryVouchers = from voucher in vouchers
                                                              join p in partners on voucher.PartnerId equals p.Id
                                                              join office in offices on voucher.OfficeId equals office.Id into officeGrp
                                                              from office in officeGrp.DefaultIfEmpty()
                                                              select new BravoVoucherModel
                                                              {
                                                                  Stt = voucher.Id,
                                                                  CustomerCode = p.AccountNo,
                                                                  CustomerName = p.PartnerNameVn,
                                                                  CustomerMode = p.PartnerMode,
                                                                  LocalBranchCode = p.InternalCode,
                                                                  DocDate = voucher.Date.HasValue ? voucher.Date.Value.Date : voucher.Date, //Chỉ lấy Date (không lấy Time)
                                                                  OfficeCode = office.Code,
                                                                  ReferenceNo = voucher.VoucherId,
                                                                  CurrencyCode = voucher.Currency,
                                                                  ExchangeRate = GetExchangeRate(voucher.Date, voucher.Currency),
                                                                  Description0 = voucher.Description,
                                                                  AccountNo = voucher.AccountNo,
                                                                  PaymentMethod = voucher.PaymentMethod,
                                                                  PaymentTerm = voucher.PaymentTerm
                                                              };

                List<BravoVoucherModel> data = queryVouchers.ToList();
                if (data.Count > 0)
                {
                    foreach (BravoVoucherModel item in data)
                    {
                        // Ds surcharge của voucher
                        IQueryable<CsShipmentSurcharge> surcharges = SurchargeRepository.Get(x => x.AcctManagementId == item.Stt);

                        IQueryable<BravoVoucherChargeModel> queryChargesVoucher = from surcharge in surcharges
                                                                                  join charge in charges on surcharge.ChargeId equals charge.Id
                                                                                  join obhP in partners on surcharge.PaymentObjectId equals obhP.Id into obhPGrps
                                                                                  from obhP in obhPGrps.DefaultIfEmpty()
                                                                                  join partner in obhPartners on surcharge.PayerId equals partner.Id into partnerGrps
                                                                                  from partnerGrp in partnerGrps.DefaultIfEmpty()
                                                                                  join chgDef in chargesDefault on charge.Id equals chgDef.ChargeId into chgDef2
                                                                                  from chgDef in chgDef2.DefaultIfEmpty()
                                                                                  join unit in catUnits on surcharge.UnitId equals unit.Id
                                                                                  select new BravoVoucherChargeModel
                                                                                  {
                                                                                      RowId = surcharge.Id,
                                                                                      ItemCode = charge.Code,
                                                                                      Description = charge.ChargeNameVn,
                                                                                      Unit = unit.UnitNameVn,
                                                                                      // CR 14952
                                                                                      CurrencyCode = (item.AccountNo == "3311" || item.AccountNo == "3313") ? item.CurrencyCode : surcharge.CurrencyId,
                                                                                      ExchangeRate = (item.AccountNo == "3311" || item.AccountNo == "3313") && item.CurrencyCode == AccountingConstants.CURRENCY_LOCAL ? 1
                                                                                      : currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_LOCAL),
                                                                                      BillEntryNo = surcharge.Hblno,
                                                                                      Ma_SpHt = surcharge.JobNo,
                                                                                      MasterBillNo = surcharge.Mblno,
                                                                                      DeptCode = string.IsNullOrEmpty(charge.ProductDept) ? GetDeptCode(surcharge.JobNo) : charge.ProductDept,
                                                                                      Quantity9 = surcharge.Quantity,
                                                                                      TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100, //Thuế suất /100

                                                                                      // CR 14952
                                                                                      OriginalUnitPrice = GetOriginalUnitPriceWithAccountNo(surcharge.UnitPrice ?? 0, item.AccountNo, surcharge.FinalExchangeRate ?? 1),
                                                                                      OriginalAmount = GetOriginAmountWithAccountNo(item.AccountNo, surcharge), //CR: 15500
                                                                                      OriginalAmount3 = GetOrgVatAmountWithAccountNo(item.AccountNo, surcharge), //CR: 15500
                                                                                      Amount = surcharge.AmountVnd,
                                                                                      Amount3 = surcharge.VatAmountVnd,

                                                                                      OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? obhP.AccountNo : null,
                                                                                      AtchDocNo = surcharge.InvoiceNo,
                                                                                      AtchDocDate = surcharge.InvoiceDate.HasValue ? surcharge.InvoiceDate.Value.Date : surcharge.InvoiceDate, //Chỉ lấy Date (không lấy Time)
                                                                                      AtchDocSerialNo = surcharge.SeriesNo,
                                                                                      AccountNo = item.AccountNo, // AccountNo của voucher
                                                                                      ContracAccount = chgDef.CreditAccountNo,
                                                                                      VATAccount = chgDef.CreditVat,
                                                                                      ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT + (!string.IsNullOrEmpty(charge.Mode) ? "-" + charge.Mode.ToUpper() : string.Empty) : surcharge.Type),
                                                                                      CustomerCodeBook = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? partnerGrp.AccountNo : obhP.AccountNo,
                                                                                      DueDate = item.PaymentTerm ?? 0,
                                                                                      CustomerCodeVAT = GetCustomerCodeVAT(surcharge),
                                                                                      CustomerCodeTransfer = item.PaymentMethod == "Bank Transfer" ? item.CustomerCode : null
                                                                                  };
                        if (queryChargesVoucher.Count() > 0)
                        {
                            item.Details = queryChargesVoucher.ToList();
                        }
                    }
                    result = data;
                }
            }
            return result;
        }

        public List<BravoSettlementModel> GetListSettlementToSyncBravo(List<Guid> Ids)
        {
            List<BravoSettlementModel> result = new List<BravoSettlementModel>();
            if (Ids.Count() > 0)
            {
                // Get list settlement
                // IQueryable<AcctSettlementPayment> settlements = SettlementRepository.Get(x => Ids.Contains(x.Id));
                IQueryable<AcctSettlementPayment> settlements = SettlementRepository.Get(x => Ids.Contains(x.Id) && x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);

                IQueryable<BravoSettlementModel> querySettlement = from settle in settlements
                                                                   join user in users on settle.Requester equals user.Id
                                                                   join employee in employees on user.EmployeeId equals employee.Id
                                                                   join office in offices on settle.OfficeId equals office.Id
                                                                   join partner in partners on settle.Payee equals partner.Id into payeeGrps
                                                                   from partner in payeeGrps.DefaultIfEmpty()
                                                                   select new BravoSettlementModel
                                                                   {
                                                                       Stt = settle.Id,
                                                                       OfficeCode = office.Code,
                                                                       DocDate = settle.RequestDate,
                                                                       ReferenceNo = settle.SettlementNo,
                                                                       ExchangeRate = GetExchangeRate(settle.RequestDate, settle.SettlementCurrency),
                                                                       Description0 = settle.Note,
                                                                       CustomerName = partner != null ? partner.ShortName : employee.EmployeeNameVn,
                                                                       // CustomerCode = partner != null ? partner.AccountNo : (!string.IsNullOrEmpty(employee.PersonalId) ? employee.PersonalId : employee.StaffCode),
                                                                       CustomerCode = partner != null ? partner.AccountNo : employee.StaffCode, //[06/01/2021]
                                                                       PaymentMethod = settle.PaymentMethod == "Bank" ? "Bank Transfer" : settle.PaymentMethod,
                                                                       CustomerMode = partner != null ? partner.PartnerMode : "External",
                                                                       LocalBranchCode = partner != null ? partner.InternalCode : null,
                                                                       Payee = settle.Payee
                                                                   };
                if (querySettlement != null && querySettlement.Count() > 0)
                {
                    List<BravoSettlementModel> data = querySettlement.ToList();
                    if (data.Count() > 0)
                    {
                        foreach (BravoSettlementModel item in data)
                        {
                            // Ds Surcharge của settlement.
                            IQueryable<CsShipmentSurcharge> surcharges = SurchargeRepository.Get(x => x.SettlementCode == item.ReferenceNo);

                            //*Note: Nếu charge là OBH thì OriginalAmount = Thành tiền trước thuế + Tiền thuế; OriginalAmount3 = 0; 
                            //Ngược lại OriginalAmount = Thành tiền trước thuế, OriginalAmount3 = Tiền thuế

                            IQueryable<BravoSettlementRequestModel> querySettlementReq = from surcharge in surcharges
                                                                                         join charge in charges on surcharge.ChargeId equals charge.Id
                                                                                         join obhP in partners on surcharge.PaymentObjectId equals obhP.Id into obhPGrps
                                                                                         from obhP in obhPGrps.DefaultIfEmpty()
                                                                                         join unit in catUnits on surcharge.UnitId equals unit.Id
                                                                                         select new BravoSettlementRequestModel
                                                                                         {
                                                                                             RowId = surcharge.Id,
                                                                                             ItemCode = charge.Code,
                                                                                             Description = charge.ChargeNameVn,
                                                                                             Unit = unit.UnitNameVn,
                                                                                             CurrencyCode = surcharge.CurrencyId,
                                                                                             ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_LOCAL),
                                                                                             BillEntryNo = surcharge.Hblno,
                                                                                             Ma_SpHt = surcharge.JobNo,
                                                                                             MasterBillNo = surcharge.Mblno,
                                                                                             DeptCode = string.IsNullOrEmpty(charge.ProductDept) ? GetDeptCode(surcharge.JobNo) : charge.ProductDept,
                                                                                             Quantity9 = surcharge.Quantity,
                                                                                             OriginalUnitPrice = surcharge.UnitPrice,
                                                                                             TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100, //Thuế suất /100
                                                                                             OriginalAmount = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? NumberHelper.RoundNumber(surcharge.Total, (surcharge.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? 0 : 2)) + NumberHelper.RoundNumber((surcharge.Total - surcharge.NetAmount) ?? 0, (surcharge.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? 0 : 2)) : NumberHelper.RoundNumber(surcharge.Total, (surcharge.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? 0 : 2)), //CR: 15500
                                                                                             OriginalAmount3 = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? 0 : GetOrgVatAmount(surcharge.Vatrate, surcharge.Quantity * surcharge.UnitPrice, surcharge.CurrencyId), //CR: 15500
                                                                                             OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? obhP.AccountNo : null,
                                                                                             AtchDocNo = surcharge.InvoiceNo,
                                                                                             AtchDocDate = surcharge.InvoiceDate.HasValue ? surcharge.InvoiceDate.Value.Date : surcharge.InvoiceDate, //Chỉ lấy Date (không lấy Time)
                                                                                             AtchDocSerialNo = surcharge.SeriesNo,
                                                                                             ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT + (!string.IsNullOrEmpty(charge.Mode) ? "-" + charge.Mode.ToUpper() : string.Empty) : surcharge.Type),
                                                                                             // CustomerCodeBook = obhP.AccountNo
                                                                                             CustomerCodeBook = GetPayeeCode(item.Payee, item.PaymentMethod, obhP.AccountNo, surcharge.Type, surcharge.PayerId), //CR: 15500
                                                                                             CustomerCodeVAT = GetCustomerCodeVAT(surcharge),
                                                                                             CustomerCodeTransfer = GetCustomerCodeTransfer(item.PaymentMethod, item.CustomerCode)
                                                                                         };
                            if (querySettlementReq.Count() > 0)
                            {
                                item.Details = querySettlementReq.ToList();
                            }
                        }
                        result = data;
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Get data list cd note debit to sync accountant
        /// </summary>
        /// <param name="Ids">List Id of cd note</param>
        /// <param name="type">Type: DEBIT/INVOICE</param>
        /// <returns></returns>
        public List<SyncModel> GetListCdNoteToSync(List<Guid> ids)
        {
            List<SyncModel> data = new List<SyncModel>();
            if (ids == null || ids.Count() == 0) return data;

            var cdNotes = cdNoteRepository.Get(x => ids.Contains(x.Id) && (x.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE));
            foreach (var cdNote in cdNotes)
            {
                SyncModel sync = new SyncModel();
                sync.Stt = cdNote.Id.ToString();
                sync.BranchCode = string.Empty;
                sync.OfficeCode = offices.Where(x => x.Id == cdNote.OfficeId).FirstOrDefault()?.Code;
                sync.Transcode = string.Empty;
                sync.DocDate = cdNote.DatetimeCreated.HasValue ? cdNote.DatetimeCreated.Value.Date : cdNote.DatetimeCreated; //Chỉ lấy Date (không lấy Time)
                sync.ReferenceNo = cdNote.Code;
                var cdNotePartner = partners.Where(x => x.Id == cdNote.PartnerId).FirstOrDefault();
                sync.CustomerCode = cdNotePartner?.AccountNo; //Partner Code
                sync.CustomerName = cdNotePartner?.PartnerNameVn; //Partner Local Name
                sync.CustomerMode = cdNotePartner?.PartnerMode;
                sync.LocalBranchCode = cdNotePartner?.InternalCode; //Parnter Internal Code
                sync.CurrencyCode0 = cdNote.CurrencyId;
                sync.ExchangeRate0 = cdNote.ExchangeRate ?? 1;
                sync.Description0 = cdNote.Note;

                //Lấy email của partner có type là Billing & office = office Debit Note (nếu ko có thì lấy Billing Email của Partner) [CR: 12/01/2021]
                var emailPartner = partnerEmailRepository.Get(x => x.PartnerId == cdNote.PartnerId && x.OfficeId == cdNote.OfficeId && x.Type == "Billing").FirstOrDefault()?.Email;
                var _billingEmailPartner = !string.IsNullOrEmpty(emailPartner) ? emailPartner : cdNotePartner?.BillingEmail;
                var _emailEInvoice = !string.IsNullOrEmpty(_billingEmailPartner) ? _billingEmailPartner.Replace(";", ",") : null;
                sync.EmailEInvoice = _emailEInvoice;

                sync.DataType = "CDNOTE";

                int decimalRound = 0;
                var charges = new List<ChargeSyncModel>();
                var surcharges = SurchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                var servicesOfDebitNote = new List<string> { surcharges.Select(s => s.TransactionType).FirstOrDefault() };
                var _dueDate = GetDueDate(cdNotePartner, servicesOfDebitNote);

                foreach (var surcharge in surcharges)
                {
                    var charge = new ChargeSyncModel();
                    charge.RowId = surcharge.Id.ToString();
                    charge.Ma_SpHt = surcharge.JobNo;
                    var _charge = CatChargeRepository.Get(x => x.Id == surcharge.ChargeId).FirstOrDefault();
                    charge.ItemCode = _charge?.Code;
                    var _description = GetDescriptionForSyncAcct(_charge?.ChargeNameVn, surcharge.TransactionType, surcharge.JobNo, surcharge.Mblno, surcharge.Hblno);
                    charge.Description = _description;
                    var _unit = CatUnitRepository.Get(x => x.Id == surcharge.UnitId).FirstOrDefault();
                    charge.Unit = _unit?.UnitNameVn; //Unit Name En
                    charge.BillEntryNo = surcharge.Hblno;
                    charge.MasterBillNo = surcharge.Mblno;
                    charge.DeptCode = !string.IsNullOrEmpty(_charge?.ProductDept) ? _charge?.ProductDept : GetDeptCode(surcharge.JobNo);
                    charge.NganhCode = "FWD";
                    charge.Quantity9 = surcharge.Quantity;

                    var _partnerPaymentObject = partners.Where(x => x.Id == surcharge.PaymentObjectId).FirstOrDefault(); //CR: 24-11-2020
                    charge.OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? _partnerPaymentObject?.AccountNo : string.Empty;
                    charge.ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type);

                    //Đối với phí DEBIT - Quy đổi theo currency của Debit Note
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL)
                    {
                        if (sync.CurrencyCode0 != AccountingConstants.CURRENCY_LOCAL)
                        {
                            decimalRound = 2;
                        }

                        charge.CurrencyCode = cdNote.CurrencyId; //Set Currency Charge = Currency Debit Note
                        charge.ExchangeRate = cdNote.ExchangeRate; //Set Exchange Rate of Charge = Exchange Rate of Debit Note
                        // Exchange Rate from currency original charge to currency debit note
                        var _exchargeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, cdNote.CurrencyId);
                        var _unitPrice = surcharge.UnitPrice * _exchargeRate;
                        charge.OriginalUnitPrice = _unitPrice ?? 0; //Đơn giá không cần làm tròn
                        charge.TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100; //Thuế suất /100
                        //var _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * _unitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                        //charge.OriginalAmount = _netAmount; //Thành tiền chưa thuế đã làm tròn
                        //var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate * _exchargeRate ?? 0) : 0;
                        //charge.OriginalAmount3 = NumberHelper.RoundNumber(_taxMoney, decimalRound); //Tiền thuế (có làm tròn)

                        //CR: 15500
                        decimal _netAmount = 0;
                        decimal _taxMoney = 0;
                        if (sync.CurrencyCode0 == AccountingConstants.CURRENCY_LOCAL)
                        {
                            _netAmount = surcharge.AmountVnd ?? 0;
                            _taxMoney = surcharge.VatAmountVnd ?? 0;
                        }
                        else if (sync.CurrencyCode0 == AccountingConstants.CURRENCY_USD)
                        {
                            _netAmount = surcharge.AmountUsd ?? 0;
                            _taxMoney = surcharge.VatAmountUsd ?? 0;
                        }
                        else if (sync.CurrencyCode0 == surcharge.CurrencyId)
                        {
                            _netAmount = surcharge.NetAmount ?? 0;
                            _taxMoney = (surcharge.Total - surcharge.NetAmount) ?? 0;
                        }
                        else
                        {
                            _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * _unitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                            _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate * _exchargeRate ?? 0) : 0;
                            _taxMoney = NumberHelper.RoundNumber(_taxMoney, decimalRound); //Tiền thuế (có làm tròn)
                        }

                        charge.OriginalAmount = _netAmount; //Thành tiến chưa thuế
                        charge.OriginalAmount3 = _taxMoney; //Tiền thuế
                    }

                    //Đối với phí OBH - Giữ nguyên giá trị, không cần quy đổi
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                    {
                        if (surcharge.CurrencyId != AccountingConstants.CURRENCY_LOCAL)
                        {
                            decimalRound = 2;
                        }

                        charge.CurrencyCode = surcharge.CurrencyId;
                        charge.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_LOCAL); //Lấy giá trị FinalExchangeRate, nếu không có thì quy đổi về Currency Local theo ExchangeDate
                        charge.OriginalUnitPrice = surcharge.UnitPrice; //Đơn giá
                        charge.TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100; //Thuế suất /100
                        //var _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * surcharge.UnitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                        //charge.OriginalAmount = _netAmount; //Thành tiền chưa thuế đã làm tròn
                        //var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                        //charge.OriginalAmount3 = NumberHelper.RoundNumber(_taxMoney, decimalRound); //Tiền thuế (có làm tròn)

                        //Đối với phí OBH thì OriginalAmount sẽ là thành tiền sau thuế (thành tiến trước thuế + tiền thuế), OriginalAmount3 gán bằng 0
                        decimal _amount = 0;
                        //CR: 15500
                        if (sync.CurrencyCode0 == AccountingConstants.CURRENCY_LOCAL)
                        {
                            _amount = (surcharge.AmountVnd + surcharge.VatAmountVnd) ?? 0;
                        }
                        else if (sync.CurrencyCode0 == AccountingConstants.CURRENCY_USD)
                        {
                            _amount = (surcharge.AmountUsd + surcharge.VatAmountUsd) ?? 0;
                        }
                        else if (sync.CurrencyCode0 == surcharge.CurrencyId)
                        {
                            _amount = surcharge.Total;
                        }
                        else
                        {
                            var _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * surcharge.UnitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                            var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                            _amount = _netAmount + _taxMoney;
                        }

                        charge.OriginalAmount = _amount;
                        charge.OriginalAmount3 = 0;

                        charge.AtchDocNo = surcharge.InvoiceNo; //Số Invoice trên phiếu OBH
                        charge.AtchDocSerialNo = surcharge.SeriesNo; //Số Serie trên phiếu OBH
                    }

                    charge.DueDate = _dueDate;

                    charges.Add(charge);
                }
                sync.Details = charges;

                data.Add(sync);
            }

            return data;
        }

        /// <summary>
        /// Get data list cd note credit to sync accountant
        /// </summary>
        /// <param name="Ids">List Id of cd note</param>
        /// <param name="type">Type: CREDIT</param>
        /// <returns></returns>
        public List<SyncCreditModel> GetListCdNoteCreditToSync(List<RequestGuidTypeListModel> models)
        {
            List<SyncCreditModel> data = new List<SyncCreditModel>();
            if (models == null || models.Count() == 0) return data;

            foreach (var model in models)
            {
                var cdNote = cdNoteRepository.Get(x => model.Id == x.Id && x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).FirstOrDefault();
                if (cdNote != null)
                {
                    SyncCreditModel sync = new SyncCreditModel();
                    sync.Stt = cdNote.Id.ToString();
                    sync.BranchCode = string.Empty;
                    sync.OfficeCode = offices.Where(x => x.Id == cdNote.OfficeId).FirstOrDefault()?.Code;
                    sync.Transcode = string.Empty;
                    sync.DocDate = cdNote.DatetimeCreated.HasValue ? cdNote.DatetimeCreated.Value.Date : cdNote.DatetimeCreated; //Chỉ lấy Date (không lấy Time)
                    sync.ReferenceNo = cdNote.Code;
                    var cdNotePartner = partners.Where(x => x.Id == cdNote.PartnerId).FirstOrDefault();
                    sync.CustomerCode = cdNotePartner?.AccountNo; //Partner Code
                    sync.CustomerName = cdNotePartner?.PartnerNameVn; //Partner Local Name
                    sync.CustomerMode = cdNotePartner?.PartnerMode;
                    sync.LocalBranchCode = cdNotePartner?.InternalCode; //Parnter Internal Code
                    sync.CurrencyCode = cdNote.CurrencyId;
                    sync.ExchangeRate = cdNote.ExchangeRate ?? 1;
                    sync.Description0 = cdNote.Note;
                    sync.DataType = "CDNOTE";
                    sync.PaymentMethod = model.PaymentMethod; //Set Payment Method = giá trị truyền vào

                    int decimalRound = 0;
                    if (sync.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)
                    {
                        decimalRound = 2;
                    }

                    var charges = new List<ChargeCreditSyncModel>();
                    var surcharges = SurchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                    foreach (var surcharge in surcharges)
                    {
                        var charge = new ChargeCreditSyncModel();
                        charge.RowId = surcharge.Id.ToString();
                        charge.Ma_SpHt = surcharge.JobNo;
                        var _charge = CatChargeRepository.Get(x => x.Id == surcharge.ChargeId).FirstOrDefault();
                        charge.ItemCode = _charge?.Code;
                        charge.Description = _charge?.ChargeNameVn;
                        var _unit = CatUnitRepository.Get(x => x.Id == surcharge.UnitId).FirstOrDefault();
                        charge.Unit = _unit?.UnitNameVn; //Unit Name En
                        charge.CurrencyCode = surcharge.CurrencyId;
                        charge.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_LOCAL);
                        charge.BillEntryNo = surcharge.Hblno;
                        charge.MasterBillNo = surcharge.Mblno;
                        charge.DeptCode = !string.IsNullOrEmpty(_charge?.ProductDept) ? _charge?.ProductDept : GetDeptCode(surcharge.JobNo);
                        charge.NganhCode = "FWD";
                        charge.Quantity9 = surcharge.Quantity;
                        charge.OriginalUnitPrice = surcharge.UnitPrice ?? 0; //Đơn giá không cần làm tròn
                        charge.TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100; //Thuế suất /100
                        //var _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * surcharge.UnitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                        //charge.OriginalAmount = _netAmount; //Thành tiền chưa thuế đã làm tròn
                        //var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                        //charge.OriginalAmount3 = NumberHelper.RoundNumber(_taxMoney, decimalRound); //Tiền thuế (có làm tròn)

                        //CR: 15500
                        decimal _netAmount = 0;
                        decimal _taxMoney = 0;
                        if (sync.CurrencyCode == AccountingConstants.CURRENCY_LOCAL)
                        {
                            _netAmount = surcharge.AmountVnd ?? 0;
                            _taxMoney = surcharge.VatAmountVnd ?? 0;
                        }
                        else if (sync.CurrencyCode == AccountingConstants.CURRENCY_USD)
                        {
                            _netAmount = surcharge.AmountUsd ?? 0;
                            _taxMoney = surcharge.VatAmountUsd ?? 0;
                        }
                        else if (sync.CurrencyCode == surcharge.CurrencyId)
                        {
                            _netAmount = surcharge.NetAmount ?? 0;
                            _taxMoney = (surcharge.Total - surcharge.NetAmount) ?? 0;
                        }
                        else
                        {
                            _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * surcharge.UnitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                            _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                            _taxMoney = NumberHelper.RoundNumber(_taxMoney, decimalRound); //Tiền thuế (có làm tròn)
                        }

                        if (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY)
                        {
                            charge.OriginalAmount = _netAmount; //Thành tiền chưa thuế
                            charge.OriginalAmount3 = _taxMoney; //Tiền thuế
                        }

                        if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                        {
                            charge.OriginalAmount = _netAmount + _taxMoney; //Thành tiền chưa thuế + Tiền thuế
                            charge.OriginalAmount3 = 0;
                        }

                        var _partnerPaymentObject = partners.Where(x => x.Id == surcharge.PaymentObjectId).FirstOrDefault();
                        charge.OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? _partnerPaymentObject?.AccountNo : string.Empty;
                        charge.ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT + (!string.IsNullOrEmpty(_charge.Mode) ? "-" + _charge.Mode.ToUpper() : string.Empty) : surcharge.Type);
                        charge.AccountNo = string.Empty;
                        charge.ContraAccount = string.Empty;
                        charge.VATAccount = string.Empty;
                        charge.AtchDocNo = surcharge.InvoiceNo;
                        charge.AtchDocDate = surcharge.InvoiceDate.HasValue ? surcharge.InvoiceDate.Value.Date : surcharge.InvoiceDate; //Chỉ lấy Date (không lấy Time)
                        charge.AtchDocSerialNo = surcharge.SeriesNo;
                        charge.CustomerCodeBook = sync.CustomerCode;

                        charge.CustomerCodeVAT = !string.IsNullOrEmpty(surcharge.InvoiceNo) ? sync.CustomerCode : null;
                        charge.CustomerCodeTransfer = sync.PaymentMethod == "Bank Transfer" ? sync.CustomerCode : null;

                        charges.Add(charge);
                    }
                    sync.Details = charges;

                    data.Add(sync);
                }
            }
            return data;
        }

        /// <summary>
        /// Get data list soa debit to sync accountant
        /// </summary>
        /// <param name="Ids">List Id of soa</param>
        /// <param name="type">Type: DEBIT</param>
        /// <returns></returns>
        public List<SyncModel> GetListSoaToSync(List<int> ids)
        {
            List<SyncModel> data = new List<SyncModel>();
            if (ids == null || ids.Count() == 0) return data;

            var soas = soaRepository.Get(x => ids.Contains(x.Id) && x.Type.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_DEBIT);
            foreach (var soa in soas)
            {
                SyncModel sync = new SyncModel();
                sync.Stt = soa.Id.ToString();
                sync.BranchCode = string.Empty;
                sync.OfficeCode = offices.Where(x => x.Id == soa.OfficeId).FirstOrDefault()?.Code;
                sync.Transcode = string.Empty;
                sync.DocDate = soa.DatetimeCreated.HasValue ? soa.DatetimeCreated.Value.Date : soa.DatetimeCreated; //Chỉ lấy Date (không lấy Time)
                sync.ReferenceNo = soa.Soano;
                var soaPartner = partners.Where(x => x.Id == soa.Customer).FirstOrDefault();
                sync.CustomerCode = soaPartner?.AccountNo; //Partner Code
                sync.CustomerName = soaPartner?.PartnerNameVn; //Partner Local Name
                sync.CustomerMode = soaPartner?.PartnerMode;
                sync.LocalBranchCode = soaPartner?.InternalCode; //Parnter Internal Code
                sync.CurrencyCode0 = soa.Currency;
                sync.ExchangeRate0 = currencyExchangeService.CurrencyExchangeRateConvert(null, soa.DatetimeCreated, soa.Currency, AccountingConstants.CURRENCY_LOCAL);
                sync.Description0 = soa.Note;

                //Lấy email của partner có type là Billing & office = office SOA (nếu ko có thì lấy Billing Email của Partner) [CR: 12/01/2021]
                var emailPartner = partnerEmailRepository.Get(x => x.PartnerId == soa.Customer && x.OfficeId == soa.OfficeId && x.Type == "Billing").FirstOrDefault()?.Email;
                var _billingEmailPartner = !string.IsNullOrEmpty(emailPartner) ? emailPartner : soaPartner?.BillingEmail;
                var _emailEInvoice = !string.IsNullOrEmpty(_billingEmailPartner) ? _billingEmailPartner.Replace(";", ",") : null;
                sync.EmailEInvoice = _emailEInvoice;

                sync.DataType = "SOA";

                int decimalRound = 0;
                var charges = new List<ChargeSyncModel>();
                var surcharges = SurchargeRepository.Get(x => x.Soano == soa.Soano || x.PaySoano == soa.Soano);
                var servicesOfSoaDebit = surcharges.Select(s => s.TransactionType).Distinct().ToList();
                var _dueDate = GetDueDate(soaPartner, servicesOfSoaDebit);

                foreach (var surcharge in surcharges)
                {
                    var charge = new ChargeSyncModel();
                    charge.RowId = surcharge.Id.ToString();
                    charge.Ma_SpHt = surcharge.JobNo;
                    var _charge = CatChargeRepository.Get(x => x.Id == surcharge.ChargeId).FirstOrDefault();
                    charge.ItemCode = _charge?.Code;
                    var _description = GetDescriptionForSyncAcct(_charge?.ChargeNameVn, surcharge.TransactionType, surcharge.JobNo, surcharge.Mblno, surcharge.Hblno);
                    charge.Description = _description;
                    var _unit = CatUnitRepository.Get(x => x.Id == surcharge.UnitId).FirstOrDefault();
                    charge.Unit = _unit?.UnitNameVn; //Unit Name En
                    charge.BillEntryNo = surcharge.Hblno;
                    charge.MasterBillNo = surcharge.Mblno;
                    charge.DeptCode = !string.IsNullOrEmpty(_charge?.ProductDept) ? _charge?.ProductDept : GetDeptCode(surcharge.JobNo);
                    charge.NganhCode = "FWD";
                    charge.Quantity9 = surcharge.Quantity;

                    var _partnerPaymentObject = partners.Where(x => x.Id == surcharge.PaymentObjectId).FirstOrDefault(); //CR: 24-11-2020
                    charge.OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? _partnerPaymentObject?.AccountNo : string.Empty;
                    charge.ChargeType = surcharge.Type.ToUpper() == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type);

                    //Đối với phí DEBIT - Quy đổi theo currency của SOA (Type Debit)
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL)
                    {
                        if (sync.CurrencyCode0 != AccountingConstants.CURRENCY_LOCAL)
                        {
                            decimalRound = 2;
                        }

                        charge.CurrencyCode = sync.CurrencyCode0; //Set Currency Charge = Currency SOA
                        charge.ExchangeRate = sync.ExchangeRate0; //Set Exchange Rate of Charge = Exchange Rate of SOA
                        // Exchange Rate from currency original charge to currency SOA
                        var _exchargeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, sync.CurrencyCode0);
                        var _unitPrice = surcharge.UnitPrice * _exchargeRate;
                        charge.OriginalUnitPrice = _unitPrice ?? 0; //Đơn giá làm tròn
                        charge.TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100; //Thuế suất /100

                        //CR: 15500
                        decimal _netAmount = 0;
                        decimal _taxMoney = 0;
                        if (sync.CurrencyCode0 == AccountingConstants.CURRENCY_LOCAL)
                        {
                            _netAmount = surcharge.AmountVnd ?? 0;
                            _taxMoney = surcharge.VatAmountVnd ?? 0;
                        }
                        else if (sync.CurrencyCode0 == AccountingConstants.CURRENCY_USD)
                        {
                            _netAmount = surcharge.AmountUsd ?? 0;
                            _taxMoney = surcharge.VatAmountUsd ?? 0;
                        }
                        else if (sync.CurrencyCode0 == surcharge.CurrencyId)
                        {
                            _netAmount = surcharge.NetAmount ?? 0;
                            _taxMoney = (surcharge.Total - surcharge.NetAmount) ?? 0;
                        }
                        else
                        {
                            _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * _unitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                            _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate * _exchargeRate ?? 0) : 0;
                            _taxMoney = NumberHelper.RoundNumber(_taxMoney, decimalRound); //Tiền thuế (có làm tròn)
                        }

                        charge.OriginalAmount = _netAmount; //Thành tiền chưa thuế
                        charge.OriginalAmount3 = _taxMoney; //Tiền thuế
                    }

                    //Đối với phí OBH - Giữ nguyên giá trị, không cần quy đổi
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                    {
                        if (surcharge.CurrencyId != AccountingConstants.CURRENCY_LOCAL)
                        {
                            decimalRound = 2;
                        }

                        charge.CurrencyCode = surcharge.CurrencyId;
                        charge.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_LOCAL); //Lấy giá trị FinalExchangeRate, nếu không có thì quy đổi về Currency Local theo ExchangeDate
                        charge.OriginalUnitPrice = surcharge.UnitPrice; //Đơn giá
                        charge.TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100; //Thuế suất /100
                        //var _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * surcharge.UnitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                        //charge.OriginalAmount = _netAmount; //Thành tiền chưa thuế đã làm tròn
                        //var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                        //charge.OriginalAmount3 = NumberHelper.RoundNumber(_taxMoney, decimalRound); //Tiền thuế (có làm tròn)

                        //Đối với phí OBH thì OriginalAmount sẽ là thành tiền sau thuế (thành tiến trước thuế + tiền thuế), OriginalAmount3 gán bằng 0
                        decimal _amount = 0;
                        //CR: 15500
                        if (sync.CurrencyCode0 == AccountingConstants.CURRENCY_LOCAL)
                        {
                            _amount = (surcharge.AmountVnd + surcharge.VatAmountVnd) ?? 0;
                        }
                        else if (sync.CurrencyCode0 == AccountingConstants.CURRENCY_USD)
                        {
                            _amount = (surcharge.AmountUsd + surcharge.VatAmountUsd) ?? 0;
                        }
                        else if (sync.CurrencyCode0 == surcharge.CurrencyId)
                        {
                            _amount = surcharge.Total;
                        }
                        else
                        {
                            var _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * surcharge.UnitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                            var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                            _amount = _netAmount + _taxMoney;
                        }

                        charge.OriginalAmount = _amount;
                        charge.OriginalAmount3 = 0;

                        charge.AtchDocNo = surcharge.InvoiceNo; //Số Invoice trên phiếu OBH
                        charge.AtchDocSerialNo = surcharge.SeriesNo; //Số Serie trên phiếu OBH
                    }

                    charge.DueDate = _dueDate;

                    charges.Add(charge);
                }
                sync.Details = charges.OrderByDescending(x => x.Ma_SpHt).ToList(); // Sắp xếp theo số job giảm dần

                data.Add(sync);
            }
            return data;
        }

        /// <summary>
        /// Get data list soa type credit to sync accountant
        /// </summary>
        /// <param name="Ids">List Id of soa</param>
        /// <param name="type">Type: CREDIT</param>
        /// <returns></returns>
        public List<SyncCreditModel> GetListSoaCreditToSync(List<RequestIntTypeListModel> models)
        {
            List<SyncCreditModel> data = new List<SyncCreditModel>();
            if (models == null || models.Count() == 0) return data;

            foreach (var model in models)
            {
                var soa = soaRepository.Get(x => model.Id == x.Id && x.Type.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT).FirstOrDefault();
                if (soa != null)
                {
                    SyncCreditModel sync = new SyncCreditModel();
                    sync.Stt = soa.Id.ToString();
                    sync.BranchCode = string.Empty;
                    sync.OfficeCode = offices.Where(x => x.Id == soa.OfficeId).FirstOrDefault()?.Code;
                    sync.Transcode = string.Empty;
                    sync.DocDate = soa.DatetimeCreated.HasValue ? soa.DatetimeCreated.Value.Date : soa.DatetimeCreated; //Chỉ lấy Date (không lấy Time)
                    sync.ReferenceNo = soa.Soano;
                    var soaPartner = partners.Where(x => x.Id == soa.Customer).FirstOrDefault();
                    sync.CustomerCode = soaPartner?.AccountNo; //Partner Code
                    sync.CustomerName = soaPartner?.PartnerNameVn; //Partner Local Name
                    sync.CustomerMode = soaPartner?.PartnerMode;
                    sync.LocalBranchCode = soaPartner?.InternalCode; //Parnter Internal Code
                    sync.CurrencyCode = soa.Currency;
                    sync.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(null, soa.DatetimeCreated, soa.Currency, AccountingConstants.CURRENCY_LOCAL);
                    sync.Description0 = soa.Note;
                    sync.DataType = "SOA";
                    sync.PaymentMethod = model.PaymentMethod; //Set Payment Method = giá trị truyền vào

                    int decimalRound = 0;
                    if (sync.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)
                    {
                        decimalRound = 2;
                    }

                    var charges = new List<ChargeCreditSyncModel>();
                    var surcharges = SurchargeRepository.Get(x => x.Soano == soa.Soano || x.PaySoano == soa.Soano);
                    foreach (var surcharge in surcharges)
                    {
                        var charge = new ChargeCreditSyncModel();
                        charge.RowId = surcharge.Id.ToString();
                        charge.Ma_SpHt = surcharge.JobNo;
                        var _charge = CatChargeRepository.Get(x => x.Id == surcharge.ChargeId).FirstOrDefault();
                        charge.ItemCode = _charge?.Code;
                        charge.Description = _charge?.ChargeNameVn;
                        var _unit = CatUnitRepository.Get(x => x.Id == surcharge.UnitId).FirstOrDefault();
                        charge.Unit = _unit?.UnitNameVn; //Unit Name En
                        charge.CurrencyCode = surcharge.CurrencyId;
                        charge.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, soa.Currency);
                        charge.BillEntryNo = surcharge.Hblno;
                        charge.MasterBillNo = surcharge.Mblno;
                        charge.DeptCode = !string.IsNullOrEmpty(_charge?.ProductDept) ? _charge?.ProductDept : GetDeptCode(surcharge.JobNo);
                        charge.NganhCode = "FWD";
                        charge.Quantity9 = surcharge.Quantity;
                        charge.OriginalUnitPrice = surcharge.UnitPrice ?? 0; //Không cần làm tròn
                        charge.TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100; //Thuế suất /100
                        //var _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * surcharge.UnitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                        //charge.OriginalAmount = _netAmount; //Thành tiền chưa thuế đã làm tròn
                        //var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                        //charge.OriginalAmount3 = NumberHelper.RoundNumber(_taxMoney, decimalRound); //Tiền thuế (có làm tròn)

                        //CR:15500
                        decimal _netAmount = 0;
                        decimal _taxMoney = 0;
                        if (sync.CurrencyCode == AccountingConstants.CURRENCY_LOCAL)
                        {
                            _netAmount = surcharge.AmountVnd ?? 0;
                            _taxMoney = surcharge.VatAmountVnd ?? 0;
                        }
                        else if (sync.CurrencyCode == AccountingConstants.CURRENCY_USD)
                        {
                            _netAmount = surcharge.AmountUsd ?? 0;
                            _taxMoney = surcharge.VatAmountUsd ?? 0;
                        }
                        else if (sync.CurrencyCode == surcharge.CurrencyId)
                        {
                            _netAmount = surcharge.NetAmount ?? 0;
                            _taxMoney = (surcharge.Total - surcharge.NetAmount) ?? 0;
                        }
                        else
                        {
                            _netAmount = NumberHelper.RoundNumber((surcharge.Quantity * surcharge.UnitPrice) ?? 0, decimalRound); //Net Amount làm tròn
                            _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_netAmount * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                            _taxMoney = NumberHelper.RoundNumber(_taxMoney, decimalRound); //Tiền thuế (có làm tròn)
                        }

                        if (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY)
                        {
                            charge.OriginalAmount = _netAmount; //Thành tiền chưa thuế
                            charge.OriginalAmount3 = _taxMoney; //Tiền thuế
                        }

                        if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                        {
                            charge.OriginalAmount = _netAmount + _taxMoney; //Thành tiền chưa thuế + Tiền thuế
                            charge.OriginalAmount3 = 0;
                        }

                        var _partnerPaymentObject = partners.Where(x => x.Id == surcharge.PaymentObjectId).FirstOrDefault();
                        charge.OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? _partnerPaymentObject?.AccountNo : string.Empty;
                        charge.ChargeType = surcharge.Type.ToUpper() == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT + (!string.IsNullOrEmpty(_charge.Mode) ? "-" + _charge.Mode.ToUpper() : string.Empty) : surcharge.Type);
                        charge.AccountNo = string.Empty;
                        charge.ContraAccount = string.Empty;
                        charge.VATAccount = string.Empty;
                        charge.AtchDocNo = surcharge.InvoiceNo;
                        charge.AtchDocDate = surcharge.InvoiceDate.HasValue ? surcharge.InvoiceDate.Value.Date : surcharge.InvoiceDate; //Chỉ lấy Date (không lấy Time)
                        charge.AtchDocSerialNo = surcharge.SeriesNo;
                        charge.CustomerCodeBook = sync.CustomerCode;

                        charge.CustomerCodeVAT = !string.IsNullOrEmpty(surcharge.InvoiceNo) ? sync.CustomerCode : null;
                        charge.CustomerCodeTransfer = sync.PaymentMethod == "Bank Transfer" ? sync.CustomerCode : null;

                        charges.Add(charge);
                    }
                    sync.Details = charges.OrderByDescending(x => x.Ma_SpHt).ToList();  // Sắp xếp theo số job giảm dần

                    data.Add(sync);
                }
            }
            return data;
        }

        public List<PaymentModel> GetListInvoicePaymentToSync(List<Guid> ids)
        {
            List<PaymentModel> data = new List<PaymentModel>();
            if (ids == null || ids.Count() == 0) return data;

            var invoices = DataContext.Get(x => ids.Contains(x.Id));
            foreach (var invoice in invoices)
            {
                PaymentModel sync = new PaymentModel();
                sync.Stt = invoice.Id.ToString();
                sync.BranchCode = string.Empty;
                sync.OfficeCode = offices.Where(x => x.Id == invoice.OfficeId).FirstOrDefault()?.Code;
                sync.DocDate = invoice.Date.HasValue ? invoice.Date.Value.Date : invoice.Date; //Invoice Date
                sync.ReferenceNo = invoice.InvoiceNoReal; //Invoice No
                var invoicePartner = partners.Where(x => x.Id == invoice.PartnerId).FirstOrDefault();
                sync.CustomerCode = invoicePartner?.AccountNo; //Partner Code
                sync.CustomerName = invoicePartner?.PartnerNameVn; //Partner Local Name
                sync.CurrencyCode = invoice.Currency;
                sync.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(null, invoice.DatetimeCreated, invoice.Currency, AccountingConstants.CURRENCY_LOCAL);
                sync.Description0 = invoice.PaymentNote;
                sync.PaymentMethod = invoice.PaymentMethod;
                sync.DataType = "PAYMENT";

                var payments = accountingPaymentRepository.Get(x => x.RefId == invoice.Id.ToString());
                var details = new List<PaymentDetailModel>();
                foreach (var payment in payments)
                {
                    var detail = new PaymentDetailModel();
                    detail.RowId = payment.Id.ToString();
                    detail.OriginalAmount = payment.PaymentAmount;
                    detail.Description = string.Empty;
                    detail.ObhPartnerCode = string.Empty; //Để trống
                    detail.BankAccountNo = invoicePartner?.BankAccountNo; //Partner Bank Account no
                    detail.Stt_Cd_Htt = invoice.ReferenceNo; //ReferenceNo of Invoice (Bravo Updated)
                    detail.ChargeType = "DEBIT";
                    detail.DebitAccount = string.Empty;
                    detail.NganhCode = "FWD";

                    details.Add(detail);
                }
                sync.Details = details;

                data.Add(sync);
            }
            return data;
        }

        public List<PaymentModel> GetListObhPaymentToSync(List<int> ids)
        {
            List<PaymentModel> data = new List<PaymentModel>();
            if (ids == null || ids.Count() == 0) return data;

            var soas = soaRepository.Get(x => ids.Contains(x.Id));
            foreach (var soa in soas)
            {
                PaymentModel sync = new PaymentModel();
                sync.Stt = soa.Id.ToString();
                sync.BranchCode = string.Empty;
                sync.OfficeCode = offices.Where(x => x.Id == soa.OfficeId).FirstOrDefault()?.Code;
                sync.DocDate = soa.DatetimeCreated.HasValue ? soa.DatetimeCreated.Value.Date : soa.DatetimeCreated; //Created Date SOA
                sync.ReferenceNo = soa.Soano; //SOA No
                var soaPartner = partners.Where(x => x.Id == soa.Customer).FirstOrDefault();
                sync.CustomerCode = soaPartner?.AccountNo; //Partner Code
                sync.CustomerName = soaPartner?.PartnerNameVn; //Partner Local Name
                sync.CurrencyCode = soa.Currency;
                sync.ExchangeRate = 1;
                sync.Description0 = soa.PaymentNote;
                sync.PaymentMethod = "Bank Transfer"; //Tạm set Bank Transfer
                sync.DataType = "PAYMENT";

                var payments = accountingPaymentRepository.Get(x => x.RefId == soa.Id.ToString());
                var details = new List<PaymentDetailModel>();
                foreach (var payment in payments)
                {
                    var detail = new PaymentDetailModel();
                    detail.RowId = payment.Id.ToString();
                    detail.OriginalAmount = payment.PaymentAmount;
                    detail.Description = string.Empty;
                    detail.ObhPartnerCode = string.Empty; //Để trống
                    detail.BankAccountNo = soaPartner?.BankAccountNo; //Partner Bank Account no
                    detail.Stt_Cd_Htt = string.Empty; //Sẽ update sau
                    detail.ChargeType = "OBH";
                    detail.DebitAccount = string.Empty;
                    detail.NganhCode = "FWD";

                    details.Add(detail);
                }
                sync.Details = details;

                data.Add(sync);
            }
            return data;
        }

        public HandleState SyncListAdvanceToBravo(List<Guid> ids, out List<Guid> data)
        {
            HandleState result = new HandleState();
            List<Guid> invalidAdvances = new List<Guid>();
            if (ids.Count > 0)
            {
                invalidAdvances = AdvanceRepository.Get(x => ids.Contains(x.Id) && (
                x.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE
                || !string.IsNullOrEmpty(x.VoucherNo)
                || x.SyncStatus == AccountingConstants.STATUS_SYNCED
                ))
                    .Select(x => x.Id)
                    .ToList();

                if (invalidAdvances.Count > 0)
                {
                    data = invalidAdvances;
                    return new HandleState("Danh sách advance không hợp lệ");
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (Guid id in ids)
                        {
                            AcctAdvancePayment adv = AdvanceRepository.Get(x => x.Id == id)?.FirstOrDefault();
                            if (adv != null)
                            {
                                adv.UserModified = currentUser.UserID;
                                adv.DatetimeModified = DateTime.Now;
                                adv.LastSyncDate = DateTime.Now;
                                adv.SyncStatus = AccountingConstants.STATUS_SYNCED;

                                AdvanceRepository.Update(adv, x => x.Id == id, false);
                            }
                        }
                        HandleState hs = AdvanceRepository.SubmitChanges();
                        if (hs.Success)
                        {
                            trans.Commit();

                            data = new List<Guid>();
                            return hs;
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        data = new List<Guid>();
                        return new HandleState((object)ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }

            data = invalidAdvances;
            return result;
        }

        public HandleState SyncListSettlementToBravo(List<Guid> ids, out List<Guid> data)
        {
            HandleState result = new HandleState();
            List<Guid> invalidSettlements = new List<Guid>();
            if (ids.Count > 0)
            {
                invalidSettlements = SettlementRepository.Get(x => ids.Contains(x.Id) && (
                x.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE
                || x.SyncStatus == AccountingConstants.STATUS_SYNCED
                ))
                    .Select(x => x.Id)
                    .ToList();

                if (invalidSettlements.Count > 0)
                {
                    data = invalidSettlements;
                    return new HandleState("Danh sách settlement không hợp lệ");
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (Guid id in ids)
                        {
                            AcctSettlementPayment settle = SettlementRepository.Get(x => x.Id == id)?.FirstOrDefault();
                            if (settle != null)
                            {
                                settle.UserModified = currentUser.UserID;
                                settle.DatetimeModified = DateTime.Now;
                                settle.LastSyncDate = DateTime.Now;
                                settle.SyncStatus = AccountingConstants.STATUS_SYNCED;

                                SettlementRepository.Update(settle, x => x.Id == id, false);

                                IQueryable<CsShipmentSurcharge> surcharges = SurchargeRepository.Get(x => x.SettlementCode == settle.SettlementNo);
                                if(surcharges != null && surcharges.Count() > 0)
                                {
                                    foreach (var surcharge in surcharges)
                                    {
                                        if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                                        {
                                            //Charge OBH sẽ lưu vào PaySyncedFrom
                                            surcharge.PaySyncedFrom = "SETTLEMENT";
                                        }
                                        if (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY)
                                        {
                                            //Charge BUY sẽ lưu vào SyncedFrom
                                            surcharge.SyncedFrom = "SETTLEMENT";
                                        }
                                        surcharge.UserModified = currentUser.UserID;
                                        surcharge.DatetimeModified = DateTime.Now;

                                        SurchargeRepository.Update(surcharge, x => x.Id == surcharge.Id, false);
                                    }
                                }
                            }
                        }
                        HandleState hsCharge = SurchargeRepository.SubmitChanges();
                        result = SettlementRepository.SubmitChanges();

                        trans.Commit();
                        data = new List<Guid>();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        data = new List<Guid>();
                        return new HandleState((object)ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }

            }

            data = invalidSettlements;
            return result;
        }

        public HandleState SyncListVoucherToBravo(List<Guid> ids, out List<Guid> data)
        {
            HandleState result = new HandleState();
            List<Guid> invalidSVouchers = new List<Guid>();
            if (ids.Count > 0)
            {
                invalidSVouchers = DataContext.Get(x => ids.Contains(x.Id) && x.SyncStatus == AccountingConstants.STATUS_SYNCED)
                    .Select(x => x.Id)
                    .ToList();

                if (invalidSVouchers.Count > 0)
                {
                    data = invalidSVouchers;
                    return new HandleState("Danh sách voucher không hợp lệ");
                }
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (Guid id in ids)
                        {
                            AccAccountingManagement voucher = DataContext.Get(x => x.Id == id)?.FirstOrDefault();
                            if (voucher != null)
                            {
                                voucher.UserModified = currentUser.UserID;
                                voucher.DatetimeModified = DateTime.Now;
                                voucher.LastSyncDate = DateTime.Now;
                                voucher.SyncStatus = AccountingConstants.STATUS_SYNCED;

                                DataContext.Update(voucher, x => x.Id == id, false);

                                //Update SyncedFrom equal VOUCHER by Id of Voucher
                                var surcharges = SurchargeRepository.Get(x => x.AcctManagementId == voucher.Id);
                                foreach (var surcharge in surcharges)
                                {
                                    surcharge.SyncedFrom = "VOUCHER";
                                    surcharge.UserModified = currentUser.UserID;
                                    surcharge.DatetimeModified = DateTime.Now;
                                    var hsUpdateSurcharge = SurchargeRepository.Update(surcharge, x => x.Id == surcharge.Id, false);
                                }
                            }
                        }
                        var smSurcharge = SurchargeRepository.SubmitChanges();
                        result = DataContext.SubmitChanges();

                        trans.Commit();

                        data = new List<Guid>();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        data = new List<Guid>();
                        return new HandleState((object)ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }

            }

            data = invalidSVouchers;
            return result;
        }

        public HandleState SyncListCdNoteToAccountant(List<Guid> ids)
        {
            var cdNotes = cdNoteRepository.Get(x => ids.Contains(x.Id));
            if (cdNotes == null) return new HandleState((object)"Không tìm thấy cd note");
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var cdNote in cdNotes)
                    {
                        cdNote.UserModified = currentUser.UserID;
                        cdNote.DatetimeModified = DateTime.Now;
                        cdNote.SyncStatus = AccountingConstants.STATUS_SYNCED;
                        cdNote.LastSyncDate = DateTime.Now;
                        
                        var surcharges = SurchargeRepository.Get(x => x.DebitNo == cdNote.Code || x.CreditNo == cdNote.Code);
                        //Tồn tại CDNote có [type Credit & Currency ngoại tệ] hoặc list charge có tồn tại ngoại tệ
                        if (cdNote.Type == "CREDIT" && (cdNote.CurrencyId != AccountingConstants.CURRENCY_LOCAL || surcharges.Any(x => x.CurrencyId != AccountingConstants.CURRENCY_LOCAL)))
                        {
                            cdNote.Note += " Request Voucher";
                        }
                        else
                        {
                            //Update PaySyncedFrom or SyncedFrom equal CDNOTE by CDNote Code
                            foreach (var surcharge in surcharges)
                            {
                                if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                                {
                                    surcharge.PaySyncedFrom = (cdNote.Code == surcharge.CreditNo) ? "CDNOTE" : null;
                                    surcharge.SyncedFrom = (cdNote.Code == surcharge.DebitNo) ? "CDNOTE" : null;
                                }
                                else
                                {
                                    //Charge BUY or SELL sẽ lưu vào SyncedFrom
                                    surcharge.SyncedFrom = "CDNOTE";
                                }
                                surcharge.UserModified = currentUser.UserID;
                                surcharge.DatetimeModified = DateTime.Now;
                                var hsUpdateSurcharge = SurchargeRepository.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }
                        }
                        var hsUpdateCdNote = cdNoteRepository.Update(cdNote, x => x.Id == cdNote.Id, false);
                    }
                    
                    var smSurcharge = SurchargeRepository.SubmitChanges();
                    var sm = cdNoteRepository.SubmitChanges();
                    trans.Commit();
                    return sm;
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

        public HandleState SyncListSoaToAccountant(List<int> ids)
        {
            var soas = soaRepository.Get(x => ids.Contains(x.Id));
            if (soas == null) return new HandleState((object)"Không tìm thấy soa");
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var soa in soas)
                    {
                        soa.UserModified = currentUser.UserID;
                        soa.DatetimeModified = DateTime.Now;
                        soa.SyncStatus = AccountingConstants.STATUS_SYNCED;
                        soa.LastSyncDate = DateTime.Now;
                        
                        var surcharges = SurchargeRepository.Get(x => x.Soano == soa.Soano || x.PaySoano == soa.Soano);
                        //Tồn tại SOA có [type Credit & Currency ngoại tệ] hoặc list charge có tồn tại ngoại tệ
                        if (soa.Type == "Credit" && (soa.Currency != AccountingConstants.CURRENCY_LOCAL || surcharges.Any(x => x.CurrencyId != AccountingConstants.CURRENCY_LOCAL)))
                        {
                            soa.Note += " Request Voucher";
                        }
                        else
                        {
                            //Update PaySyncedFrom or SyncedFrom equal SOA by SOA No
                            foreach (var surcharge in surcharges)
                            {
                                if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                                {
                                    surcharge.PaySyncedFrom = (soa.Soano == surcharge.PaySoano) ? "SOA" : null;
                                    surcharge.SyncedFrom = (soa.Soano == surcharge.Soano) ? "SOA" : null;
                                }
                                else
                                {
                                    //Charge BUY or SELL sẽ lưu vào SyncedFrom
                                    surcharge.SyncedFrom = "SOA";
                                }
                                surcharge.UserModified = currentUser.UserID;
                                surcharge.DatetimeModified = DateTime.Now;
                                var hsUpdateSurcharge = SurchargeRepository.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }
                        }
                        var hsUpdateSOA = soaRepository.Update(soa, x => x.Id == soa.Id, false);
                    }
                    var smSurcharge = SurchargeRepository.SubmitChanges();
                    var sm = soaRepository.SubmitChanges();
                    trans.Commit();
                    return sm;
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

        #region -- Private Method --

        private decimal GetExchangeRate(DateTime? date, string currency)
        {
            decimal exchangeRate = 0;
            // List tỷ giá theo ngày
            List<CatCurrencyExchange> currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == date).ToList();

            if (currencyExchange.Count == 0)
            {
                // Lấy ngày mới nhất
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

            exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, currency, AccountingConstants.CURRENCY_LOCAL);

            return exchangeRate;
        }

        private string GetDeptCode(string JobNo)
        {
            string deptCode = "ITLCS";

            if (JobNo.Contains("LOG"))
            {
                deptCode = "ITLOPS";
            }
            else if (JobNo.Contains("A"))
            {
                deptCode = "ITLAIR";
            }
            else if (JobNo.Contains("S"))
            {
                deptCode = "ITLCS";
            }

            return deptCode;
        }

        private decimal GetOrgVatAmount(decimal? vatrate, decimal? orgAmount, string currency)
        {
            decimal amount = 0;
            amount = (vatrate != null) ? (vatrate < 101 & vatrate >= 0) ? NumberHelper.RoundNumber(((orgAmount * vatrate) / 100 ?? 0), 3) : Math.Abs(vatrate ?? 0) : 0;
            if (currency == AccountingConstants.CURRENCY_LOCAL)
            {
                amount = CalculateRoundStandard(NumberHelper.RoundNumber(amount, 2));
            }
            return amount;
        }

        public decimal CalculateRoundStandard(decimal num)
        {
            var d = (num % 1);
            if ((double)d < 0.5)
            {
                return Math.Round(num);
            }
            else if ((double)d >= 0.5)
            {
                return Math.Ceiling(num);
            }
            else
            {
                return num;
            }
        }

        private string GetCustomerHBL(Guid? Id)
        {
            string customerName = "";
            CsTransactionDetail hbl = csTransactionDetailRepository.Get(x => x.Id == Id).FirstOrDefault();
            if (hbl != null)
            {
                CatPartner partner = PartnerRepository.Get(x => x.Id == hbl.CustomerId).FirstOrDefault();
                if (partner != null) customerName = partner.PartnerNameVn;
            }
            return customerName;
        }

        private string GetLinkCdNote(string cdNoteNo, Guid jobId)
        {
            string _link = string.Empty;
            if (cdNoteNo.Contains("CL"))
            {
                _link = string.Format("home/operation/job-management/job-edit/{0}?tab=CDNOTE", jobId.ToString());
            }
            else
            {
                string prefixService = "home/documentation/";
                if (cdNoteNo.Contains("IT"))
                {
                    prefixService += "inland-trucking";
                }
                else if (cdNoteNo.Contains("AE"))
                {
                    prefixService += "air-export";
                }
                else if (cdNoteNo.Contains("AI"))
                {
                    prefixService += "air-import";
                }
                else if (cdNoteNo.Contains("SEC"))
                {
                    prefixService += "sea-consol-export";
                }
                else if (cdNoteNo.Contains("SIC"))
                {
                    prefixService += "sea-consol-import";
                }
                else if (cdNoteNo.Contains("SEF"))
                {
                    prefixService += "sea-fcl-export";
                }
                else if (cdNoteNo.Contains("SIF"))
                {
                    prefixService += "sea-fcl-import";
                }
                else if (cdNoteNo.Contains("SEL"))
                {
                    prefixService += "sea-lcl-export";
                }
                else if (cdNoteNo.Contains("SIL"))
                {
                    prefixService += "sea-lcl-import";
                }
                _link = string.Format(@"{0}/{1}?tab=CDNOTE", prefixService, jobId.ToString());
            }
            return _link;
        }

        private decimal GetOriginalUnitPriceWithAccountNo(decimal unitPrice, string accountNo, decimal finalExchange = 1)
        {
            decimal _unitPrice = 0;
            if (!string.IsNullOrEmpty(accountNo) && (accountNo == "3311" || accountNo == "3313"))
            {
                _unitPrice = NumberHelper.RoundNumber(unitPrice * finalExchange);
            }
            else
            {
                _unitPrice = unitPrice;
            }

            return _unitPrice;
        }

        private decimal GetOriginAmountWithAccountNo(string accountNo, CsShipmentSurcharge surcharge)
        {
            decimal _originAmount = 0;
            if (!string.IsNullOrEmpty(accountNo) && (accountNo == "3311" || accountNo == "3313"))
            {
                _originAmount = surcharge.AmountVnd ?? 0;
                if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                {
                    _originAmount = (surcharge.AmountVnd + surcharge.VatAmountVnd) ?? 0;
                }
            }
            else
            {
                // Tính toán như cũ
                //_originAmount = (surcharge.Quantity * surcharge.UnitPrice) ?? 0;
                if (surcharge.CurrencyId == AccountingConstants.CURRENCY_LOCAL)
                {
                    //_originAmount = NumberHelper.RoundNumber(_originAmount);
                    _originAmount = surcharge.AmountVnd ?? 0;
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                    {
                        _originAmount = (surcharge.AmountVnd + surcharge.VatAmountVnd) ?? 0;
                    }
                }
                else if (surcharge.CurrencyId == AccountingConstants.CURRENCY_USD)
                {
                    _originAmount = surcharge.AmountUsd ?? 0;
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                    {
                        _originAmount = (surcharge.AmountUsd + surcharge.VatAmountUsd) ?? 0;
                    }
                }
                else
                {
                    _originAmount = surcharge.NetAmount ?? 0;
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                    {
                        _originAmount = surcharge.Total;
                    }
                }
            }


            return _originAmount;
        }

        private decimal GetOrgVatAmountWithAccountNo(string accountNo, CsShipmentSurcharge surcharge)
        {
            decimal _orgVatAmout = 0;
            if (!string.IsNullOrEmpty(accountNo) && (accountNo == "3311" || accountNo == "3313"))
            {
                _orgVatAmout = surcharge.VatAmountVnd ?? 0;
                if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                {
                    _orgVatAmout = 0;
                }
            }
            else
            {
                // Tính toán như cũ
                //_orgVatAmout = GetOrgVatAmount(surcharge.Vatrate, surcharge.Quantity * surcharge.UnitPrice, surcharge.CurrencyId);
                if (surcharge.CurrencyId == AccountingConstants.CURRENCY_LOCAL)
                {
                    _orgVatAmout = surcharge.VatAmountVnd ?? 0;
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                    {
                        _orgVatAmout = 0;
                    }
                }
                else if (surcharge.CurrencyId == AccountingConstants.CURRENCY_USD)
                {
                    _orgVatAmout = surcharge.VatAmountUsd ?? 0;
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                    {
                        _orgVatAmout = 0;
                    }
                }
                else
                {
                    _orgVatAmout = (surcharge.Total - surcharge.NetAmount) ?? 0;
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
                    {
                        _orgVatAmout = 0;
                    }
                }
            }

            return _orgVatAmout;
        }

        private string GetServiceNameOfCdNote(string cdNoteNo)
        {
            string _serviceName = string.Empty;
            if (cdNoteNo.Contains("CL"))
            {
                _serviceName = "Custom Logistic";
            }
            else
            {
                if (cdNoteNo.Contains("IT"))
                {
                    _serviceName = "Inland Trucking";
                }
                else if (cdNoteNo.Contains("AE"))
                {
                    _serviceName = "Air Export";
                }
                else if (cdNoteNo.Contains("AI"))
                {
                    _serviceName = "Air Import";
                }
                else if (cdNoteNo.Contains("SEC"))
                {
                    _serviceName = "Sea Consol Export";
                }
                else if (cdNoteNo.Contains("SIC"))
                {
                    _serviceName = "Sea Consol Import";
                }
                else if (cdNoteNo.Contains("SEF"))
                {
                    _serviceName = "Sea FCL Export";
                }
                else if (cdNoteNo.Contains("SIF"))
                {
                    _serviceName = "Sea FCL Import";
                }
                else if (cdNoteNo.Contains("SEL"))
                {
                    _serviceName = "Sea LCL Export";
                }
                else if (cdNoteNo.Contains("SIL"))
                {
                    _serviceName = "Sea LCL Import";
                }
            }
            return _serviceName;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PayeeId">Payee of Settlement</param>
        /// <param name="paymentMethod">Payment Method of Settlement</param>
        /// <param name="accountNo">Account No of PaymentObjectID (Surcharge)</param>
        /// <param name="typeCharge">Type of Surcharge</param>
        /// <param name="payerId">PayerId of Surcharge</param>
        /// <returns></returns>
        private string GetPayeeCode(string PayeeId, string paymentMethod, string accountNo, string typeCharge, string payerId)
        {
            string PayeeCode = string.Empty;
            CatPartner payee = PartnerRepository.Get(x => x.Id == PayeeId)?.FirstOrDefault();
            //Nếu Payment Method của Settle là Other & Partner Mode của đối tượng Payee (Settlement) là External thì lấy AccountNo theo đối tượng Payee của Settment
            if (paymentMethod == AccountingConstants.PAYMENT_METHOD_OTHER && !string.IsNullOrEmpty(PayeeId))// && payee?.PartnerMode == "External")
            {
                PayeeCode = payee.AccountNo;
            }
            else
            {
                //Nếu Type là OBH thì AccountNo lấy theo đối tượng PayerID của Surcharge; Ngược lại lấy theo đối tượng PaymentObjectID
                /*if (typeCharge == AccountingConstants.TYPE_CHARGE_OBH)
                {
                    //var payer = PartnerRepository.Get(x => x.Id == payerId)?.FirstOrDefault();
                    //PayeeCode = payer?.AccountNo;
                    PayeeCode = payee.AccountNo;
                }
                else
                {
                    PayeeCode = accountNo;
                }*/
                PayeeCode = accountNo;
            }
            return PayeeCode;
        }

        private string GetCustomerCodeVAT(CsShipmentSurcharge surcharge)
        {
            string codeVat = string.Empty;
            if (!string.IsNullOrEmpty(surcharge.InvoiceNo))
            {
                if (surcharge.Type == "BUY")
                {
                    var partner = PartnerRepository.Get(x => x.Id == surcharge.PaymentObjectId)?.FirstOrDefault();
                    codeVat = partner?.AccountNo;
                }
                if (surcharge.Type == "OBH")
                {
                    var partner = PartnerRepository.Get(x => x.Id == surcharge.PayerId)?.FirstOrDefault();
                    codeVat = partner?.AccountNo;
                }
            }
            return codeVat;
        }

        private string GetCustomerCodeTransfer(string paymentMethod, string realPartnerTransfer)
        {
            string codeTransfer = null;
            if (paymentMethod == AccountingConstants.PAYMENT_METHOD_BANK)
            {
                codeTransfer = realPartnerTransfer;
            }
            return codeTransfer;
        }

        /// <summary>
        /// Get due date by partner & service
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        private decimal? GetDueDate(CatPartner partner, List<string> services)
        {
            decimal? dueDate = 1;
            if (partner != null)
            {
                var _partnerId = (partner.Id == partner.ParentId || string.IsNullOrEmpty(partner.ParentId)) ? partner.Id : partner.ParentId;
                var contracts = contractRepository.Get(x => x.PartnerId == _partnerId && x.Active == true && services.Where(w => x.SaleService.Contains(w)).Any());
                if (contracts != null)
                {
                    // Ưu tiên Official >> Trial >> Cash (Default là 1)
                    var contractsOffice = contracts.Where(x => x.ContractType == "Official").FirstOrDefault();                    
                    if (contractsOffice != null)
                    {
                        dueDate = contractsOffice.PaymentTerm ?? 30; //PaymentTerm không có value sẽ default là 30
                        return dueDate;
                    }
                    else
                    {
                        var contractsTrial = contracts.Where(x => x.ContractType == "Trial").FirstOrDefault();
                        if (contractsTrial != null)
                        {
                            dueDate = contractsTrial.PaymentTerm ?? 30; //PaymentTerm không có value sẽ default là 30
                            return dueDate;
                        }
                        //else
                        //{
                        //    var contractCash = contracts.Where(x => x.ContractType == "Cash").FirstOrDefault();
                        //    if (contractCash != null)
                        //    {
                        //        return dueDate;
                        //    }
                        //}
                    }
                }
            }
            return dueDate;
        }

        private string GetDescriptionForSyncAcct(string chargeName, string transactionType, string jobNo, string mblNo, string hblNo)
        {
            var _description = string.Empty;
            if (transactionType == "CL")
            {
                var customNos = customsDeclarationRepository.Get(x => x.JobNo == jobNo).OrderBy(o => o.DatetimeModified).FirstOrDefault()?.ClearanceNo;
                var _customNo = !string.IsNullOrEmpty(customNos) ? string.Format("TK:{0}", customNos) : string.Empty;
                _description = string.Format("{0} {1} {2}", chargeName, hblNo, _customNo); //Format: ChargeName + HBL + ClearanceNo cũ nhất [CR: 13-01-2020]
            }
            else
            {
                var _hblNo = !string.IsNullOrEmpty(hblNo) && !hblNo.Trim().Equals("N/H") ? hblNo : mblNo;
                _description = string.Format("{0} {1}", chargeName, _hblNo); //Format: ChargeName + HBL [CR: 12-01-2020]
            }
            return _description;
        }

        #endregion -- Private Method --

        #region --- Send Mail & Push Notification to Accountant ---

        public void SendMailAndPushNotificationToAccountant(List<SyncCreditModel> syncCreditModels)
        {
            if (syncCreditModels.Count > 0)
            {
                foreach (var syncCreditModel in syncCreditModels)
                {
                    string type = syncCreditModel.DataType;
                    string creatorEnName = string.Empty;
                    string refNo = string.Empty;
                    string partnerEn = string.Empty;
                    string taxCode = string.Empty;
                    string serviceName = string.Empty;
                    string amountCurr = string.Empty;
                    string urlFunc = string.Empty;
                    string catagory = string.Empty;
                    List<string> emails = new List<string>();
                    var employeeIdCurrentUser = userBaseService.GetEmployeeIdOfUser(currentUser.UserID);
                    var emailCurrentUser = userBaseService.GetEmployeeByEmployeeId(employeeIdCurrentUser)?.Email;
                    emails.Add(emailCurrentUser);

                    int decRound = 0;
                    if (syncCreditModel.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)
                    {
                        decRound = 2;
                    }

                    if (type == "SOA")
                    {
                        var soa = soaRepository.Get(x => x.Id == int.Parse(syncCreditModel.Stt)).FirstOrDefault();
                        var employeeId = UserRepository.Get(x => x.Id == soa.UserCreated).FirstOrDefault()?.EmployeeId;
                        creatorEnName = EmployeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameEn;
                        refNo = soa.Soano;
                        var partner = PartnerRepository.Get(x => x.Id == soa.Customer).FirstOrDefault();
                        partnerEn = partner?.PartnerNameEn;
                        taxCode = partner?.TaxCode;
                        serviceName = DataTypeEx.GetServiceNameOfSoa(soa.ServiceTypeId).ToString();
                        var amount = soa.DebitAmount - soa.CreditAmount;
                        var amountStr = string.Format("{0:n" + decRound + "}", Math.Abs(amount ?? 0));
                        amountCurr = (amount < 0 ? "(" + amountStr + ")" : amountStr) + " " + soa.Currency;
                        urlFunc = string.Format(@"home/accounting/statement-of-account/detail?no={0}&currency=VND", soa.Soano);

                        catagory = "SOA_CREDIT";

                        var employeeIdCreator = userBaseService.GetEmployeeIdOfUser(soa.UserCreated);
                        var emailCreator = userBaseService.GetEmployeeByEmployeeId(employeeIdCreator)?.Email;
                        if (!string.IsNullOrEmpty(emailCreator))
                        {
                            emails.Add(emailCreator);
                        }
                    }
                    if (type == "CDNOTE")
                    {
                        var creditNote = cdNoteRepository.Get(x => x.Id == Guid.Parse(syncCreditModel.Stt)).FirstOrDefault();
                        var employeeId = UserRepository.Get(x => x.Id == creditNote.UserCreated).FirstOrDefault()?.EmployeeId;
                        creatorEnName = EmployeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameEn;
                        refNo = creditNote.Code;
                        var partner = PartnerRepository.Get(x => x.Id == creditNote.PartnerId).FirstOrDefault();
                        partnerEn = partner?.PartnerNameEn;
                        taxCode = partner?.TaxCode;
                        serviceName = GetServiceNameOfCdNote(creditNote.Code);
                        var listAmounGrpByCurrency = SurchargeRepository.Get(x => x.CreditNo == creditNote.Code).GroupBy(g => new { g.CurrencyId }).Select(s => new { amountCurrency = string.Format("{0:n" + (s.Key.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? 0 : 2) + "}", s.Select(se => se.Total).Sum()) + " " + s.Key.CurrencyId }).ToList();
                        amountCurr = string.Join("; ", listAmounGrpByCurrency.Select(s => s.amountCurrency));
                        urlFunc = GetLinkCdNote(creditNote.Code, creditNote.JobId);

                        catagory = "CDNOTE_CREDIT";
                        var employeeIdCreator = userBaseService.GetEmployeeIdOfUser(creditNote.UserCreated);
                        var emailCreator = userBaseService.GetEmployeeByEmployeeId(employeeIdCreator)?.Email;
                        if (!string.IsNullOrEmpty(emailCreator))
                        {
                            emails.Add(emailCreator);
                        }
                    }

                    //Send Mail
                    SendEmailToAccountant(catagory, creatorEnName, refNo, partnerEn, taxCode, serviceName, amountCurr, urlFunc, syncCreditModel.PaymentMethod, emails);
                    //Push Notification
                    PushNotificationToAccountant(catagory, creatorEnName, refNo, serviceName, amountCurr, urlFunc);
                }
            }
        }

        public void SendMailAndPushNotificationDebitToAccountant(List<SyncModel> syncModels)
        {
            if (syncModels.Count > 0)
            {
                foreach (var syncModel in syncModels)
                {
                    string type = syncModel.DataType;
                    string creatorEnName = string.Empty;
                    string refNo = string.Empty;
                    string partnerEn = string.Empty;
                    string taxCode = string.Empty;
                    string serviceName = string.Empty;
                    string amountCurr = string.Empty;
                    string urlFunc = string.Empty;
                    string catagory = string.Empty;
                    List<string> emails = new List<string>();
                    var employeeIdCurrentUser = userBaseService.GetEmployeeIdOfUser(currentUser.UserID);
                    var emailCurrentUser = userBaseService.GetEmployeeByEmployeeId(employeeIdCurrentUser)?.Email;
                    emails.Add(emailCurrentUser);

                    int decRound = 0;
                    if (syncModel.CurrencyCode0 != AccountingConstants.CURRENCY_LOCAL)
                    {
                        decRound = 2;
                    }

                    if (type == "SOA")
                    {
                        var soa = soaRepository.Get(x => x.Id == int.Parse(syncModel.Stt)).FirstOrDefault();
                        var employeeId = UserRepository.Get(x => x.Id == soa.UserCreated).FirstOrDefault()?.EmployeeId;
                        creatorEnName = EmployeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameEn;
                        refNo = soa.Soano;
                        var partner = PartnerRepository.Get(x => x.Id == soa.Customer).FirstOrDefault();
                        partnerEn = partner?.PartnerNameEn;
                        taxCode = partner?.TaxCode;
                        serviceName = DataTypeEx.GetServiceNameOfSoa(soa.ServiceTypeId).ToString();
                        var amount = soa.DebitAmount - soa.CreditAmount;
                        var amountStr = string.Format("{0:n" + decRound + "}", Math.Abs(amount ?? 0));
                        amountCurr = (amount < 0 ? "(" + amountStr + ")" : amountStr) + " " + soa.Currency;
                        urlFunc = string.Format(@"home/accounting/statement-of-account/detail?no={0}&currency=VND", soa.Soano);

                        catagory = "SOA_DEBIT";
                        var employeeIdCreator = userBaseService.GetEmployeeIdOfUser(soa.UserCreated);
                        var emailCreator = userBaseService.GetEmployeeByEmployeeId(employeeIdCreator)?.Email;
                        if (!string.IsNullOrEmpty(emailCreator))
                        {
                            emails.Add(emailCreator);
                        }
                    }
                    if (type == "CDNOTE")
                    {
                        var debitNote = cdNoteRepository.Get(x => x.Id == Guid.Parse(syncModel.Stt)).FirstOrDefault();
                        var employeeId = UserRepository.Get(x => x.Id == debitNote.UserCreated).FirstOrDefault()?.EmployeeId;
                        creatorEnName = EmployeeRepository.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameEn;
                        refNo = debitNote.Code;
                        var partner = PartnerRepository.Get(x => x.Id == debitNote.PartnerId).FirstOrDefault();
                        partnerEn = partner?.PartnerNameEn;
                        taxCode = partner?.TaxCode;
                        serviceName = GetServiceNameOfCdNote(debitNote.Code);
                        var listAmounGrpByCurrency = SurchargeRepository.Get(x => x.DebitNo == debitNote.Code).GroupBy(g => new { g.CurrencyId }).Select(s => new { amountCurrency = string.Format("{0:n" + (s.Key.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? 0 : 2) + "}", s.Select(se => se.Total).Sum()) + " " + s.Key.CurrencyId }).ToList();
                        amountCurr = string.Join("; ", listAmounGrpByCurrency.Select(s => s.amountCurrency));
                        urlFunc = GetLinkCdNote(debitNote.Code, debitNote.JobId);

                        catagory = "CDNOTE_" + debitNote.Type;
                        var employeeIdCreator = userBaseService.GetEmployeeIdOfUser(debitNote.UserCreated);
                        var emailCreator = userBaseService.GetEmployeeByEmployeeId(employeeIdCreator)?.Email;
                        if (!string.IsNullOrEmpty(emailCreator))
                        {
                            emails.Add(emailCreator);
                        }
                    }

                    //Send Mail
                    SendEmailToAccountant(catagory, creatorEnName, refNo, partnerEn, taxCode, serviceName, amountCurr, urlFunc, "Bank Transfer / Cash", emails);
                    //Push Notification
                    PushNotificationToAccountant(catagory, creatorEnName, refNo, serviceName, amountCurr, urlFunc);
                }
            }
        }

        private void SendEmailToAccountant(string catagory, string creatorEnName, string refNo, string partnerEn, string taxCode, string serviceName, string amountCurr, string urlFunc, string paymentMethod, List<string> emailCcs)
        {
            string _type1 = string.Empty;
            string _type2 = string.Empty;
            if (catagory == "SOA_DEBIT")
            {
                _type1 = "VAT Invoice";
                _type2 = "SOA";
            }
            else if (catagory == "SOA_CREDIT")
            {
                _type1 = "Voucher";
                _type2 = "SOA";
            }
            else if (catagory == "CDNOTE_DEBIT")
            {
                _type1 = "VAT Invoice";
                _type2 = "Debit Note";
            }
            else if (catagory == "CDNOTE_CREDIT")
            {
                _type1 = "Voucher";
                _type2 = "Credit Note";
            }
            else if (catagory == "CDNOTE_INVOICE")
            {
                _type1 = "VAT Invoice";
                _type2 = "Invoice";
            }

            string subject = string.Format(@"eFMS - {0} Request - {1} {2}", _type1, _type2, refNo);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color: #004080'>" +
                                            "<p><i>Dear Accountant Team,</i></p>" +
                                            "<p>" +
                                                "<div>You received a <b>[SOA_CDNote]</b> from <b>[CreatorEnName]</b> as info bellow:</div>" +
                                                "<div><i>Bạn có nhận một đề nghị thanh toán bằng <b>[SOA_CDNote]</b> từ <b>[CreatorEnName]</b> với thông tin như sau: </i></div>" +
                                            "</p>" +
                                            "<ul>" +
                                                "<li>Ref No/ <i>Số tham chiếu</i>: <b><i>[RefNo]</i></b></li>" +
                                                "<li>Partner Name/ <i>Tên đối tượng</i>: <b><i>[PartnerEn]</i></b></li>" +
                                                "<li>Tax Code/ <i>Mã số thuế</i>: <b><i>[Taxcode]</i></b></li>" +
                                                "<li>Service/ <i>Dịch vụ</i>: <b><i>[ServiceName]</i></b></li>" +
                                                "<li>Amount/ <i>Số tiền</i>: <b><i>[AmountCurr]</i></b></li>" +
                                                "<li>Payment Method/ <i>Phương Thức thanh toán</i>: <b><i>[PaymentMethod]</i></b></li>" +
                                            "</ul>" +
                                            "<p>" +
                                                "<div>You can <span><a href='[Url]/[lang]/#/[UrlFunc]' target='_blank'>click here</a></span> to view detail.</div>" +
                                                "<div><i>Bạn click <span><a href='[Url]/[lang]/#/[UrlFunc]' target='_blank'>vào đây</a></span> để xem chi tiết </i></div>" +
                                            "</p>" +
                                            "<p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p>" +
                                         "</div>");
            body = body.Replace("[SOA_CDNote]", _type2);
            body = body.Replace("[CreatorEnName]", creatorEnName);
            body = body.Replace("[RefNo]", refNo);
            body = body.Replace("[PartnerEn]", partnerEn);
            body = body.Replace("[Taxcode]", taxCode);
            body = body.Replace("[ServiceName]", serviceName);
            body = body.Replace("[AmountCurr]", amountCurr);
            body = body.Replace("[PaymentMethod]", !string.IsNullOrEmpty(paymentMethod) ? paymentMethod : "Credit");
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", urlFunc);
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            var emailAccountantDept = departmentRepo.Get(x => x.DeptType == AccountingConstants.DeptTypeAccountant && x.BranchId == currentUser.OfficeID).FirstOrDefault()?.Email;
            List<string> emails = emailAccountantDept.Split(';').Where(x => x.ToString() != string.Empty).ToList();

            List<string> toEmails = emails;
            List<string> attachments = null;

            List<string> emailCCs = emailCcs;
            List<string> emailBCCs = new List<string> { "alex.phuong@itlvn.com", "andy.hoa@itlvn.com" };
            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs, emailBCCs);

            #region --- Ghi Log Send Mail ---
            var logSendMail = new SysSentEmailHistory
            {
                SentUser = SendMail._emailFrom,
                Receivers = string.Join("; ", toEmails),
                Ccs = string.Join("; ", emailCCs),
                Bccs = string.Join("; ", emailBCCs),
                Subject = subject,
                Sent = sendMailResult,
                SentDateTime = DateTime.Now,
                Body = body
            };
            var hsLogSendMail = sentEmailHistoryRepo.Add(logSendMail);
            var hsSm = sentEmailHistoryRepo.SubmitChanges();
            #endregion --- Ghi Log Send Mail ---
        }

        private void PushNotificationToAccountant(string catagory, string creatorEnName, string refNo, string serviceName, string amountCurr, string urlFunc)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var idAccountantDept = departmentRepo.Get(x => x.DeptType == AccountingConstants.DeptTypeAccountant && x.BranchId == currentUser.OfficeID).FirstOrDefault()?.Id;
                    // Danh sách user Id của group thuộc department Accountant (Không lấy manager của department Acct)
                    var idUserGroupAccts = sysUserLevelRepo.Get(x => x.GroupId != AccountingConstants.SpecialGroup && x.DepartmentId == idAccountantDept).Select(s => s.UserId);

                    string _type1 = string.Empty;
                    string _type2 = string.Empty;
                    if (catagory == "SOA_DEBIT")
                    {
                        _type1 = "VAT Invoice";
                        _type2 = "SOA";
                    }
                    else if (catagory == "SOA_CREDIT")
                    {
                        _type1 = "Voucher";
                        _type2 = "SOA";
                    }
                    else if (catagory == "CDNOTE_DEBIT")
                    {
                        _type1 = "VAT Invoice";
                        _type2 = "Debit Note";
                    }
                    else if (catagory == "CDNOTE_CREDIT")
                    {
                        _type1 = "Voucher";
                        _type2 = "Credit Note";
                    }
                    else if (catagory == "CDNOTE_INVOICE")
                    {
                        _type1 = "VAT Invoice";
                        _type2 = "Invoice";
                    }

                    string title = string.Format(@"{0} Request - {1}: {2}", _type1, _type2, refNo);
                    string description = string.Format(@"You received a <b>{0}</b> from <b>{1}</b>. Ref No <b>{2}</b> of <b>{3}</b> with Amount <b>{4}</b>", _type2, creatorEnName, refNo, serviceName, amountCurr);

                    // Add Notification
                    SysNotifications sysNotification = new SysNotifications
                    {
                        Id = Guid.NewGuid(),
                        Title = title,
                        Description = description,
                        Type = "User",
                        UserCreated = currentUser.UserID,
                        DatetimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        UserModified = currentUser.UserID,
                        Action = "Detail",
                        ActionLink = urlFunc,
                        IsClosed = false,
                        IsRead = false,
                        UserIds = string.Join(",", idUserGroupAccts.ToList())
                    };
                    HandleState hsSysNotification = sysNotifyRepository.Add(sysNotification, false);
                    if (hsSysNotification.Success)
                    {
                        foreach (var idUserGroupAcct in idUserGroupAccts)
                        {
                            SysUserNotification userNotifySync = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                Status = "New",
                                NotitficationId = sysNotification.Id,
                                UserId = idUserGroupAcct,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            var hsUserSync = sysUserNotifyRepository.Add(userNotifySync, false);
                        }
                    }
                    var smNotify = sysNotifyRepository.SubmitChanges();
                    var smUserNotify = sysUserNotifyRepository.SubmitChanges();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        #endregion --- Send Mail & Push Notification to Accountant ---

        public List<PaymentModel> GetListReceiptToAccountant(List<Guid> ids)
        {
            List<PaymentModel> data = new List<PaymentModel>();
            if (ids == null || ids.Count() == 0) return data;

            var receipts = receiptRepository.Get(x => ids.Contains(x.Id));
            foreach (var receipt in receipts)
            {
                PaymentModel sync = new PaymentModel();
                sync.Stt = receipt.Id.ToString();
                var officeCode = offices.Where(x => x.Id == receipt.OfficeId).FirstOrDefault()?.Code;
                sync.BranchCode = officeCode;
                sync.OfficeCode = officeCode;
                sync.DocDate = receipt.PaymentDate.HasValue ? receipt.PaymentDate.Value.Date : receipt.PaymentDate; //Payment Date (Chỉ lấy Date, không lấy time)
                sync.ReferenceNo = receipt.PaymentRefNo; //Receipt No
                var invoicePartner = partners.Where(x => x.Id == receipt.CustomerId).FirstOrDefault();
                sync.CustomerCode = invoicePartner?.AccountNo; //Partner Code
                sync.CustomerName = invoicePartner?.PartnerNameVn; //Partner Local Name
                sync.CurrencyCode = receipt.CurrencyId;
                sync.ExchangeRate = receipt.ExchangeRate;
                sync.Description0 = receipt.Description;
                sync.PaymentMethod = receipt.PaymentMethod;
                sync.DataType = "PAYMENT";

                var payments = accountingPaymentRepository.Get(x => x.ReceiptId == receipt.Id);
                var details = new List<PaymentDetailModel>();
                foreach (var payment in payments)
                {
                    var invoice = DataContext.Get(x => x.Id.ToString() == payment.RefId).FirstOrDefault();
                    var detail = new PaymentDetailModel();
                    detail.RowId = payment.Id.ToString();
                    detail.OriginalAmount = payment.PaymentAmount;
                    detail.Description = payment.Note;
                    detail.ObhPartnerCode = string.Empty; //Để trống
                    detail.BankAccountNo = invoicePartner?.BankAccountNo; //Partner Bank Account no
                    detail.Stt_Cd_Htt = invoice?.ReferenceNo; //ReferenceNo of Invoice (Bravo Updated)
                    detail.ChargeType = payment.Type;
                    detail.DebitAccount = invoice?.AccountNo; //Account No of Invoice
                    detail.NganhCode = "FWD";

                    details.Add(detail);
                }
                sync.Details = details;

                data.Add(sync);
            }
            return data;
        }

        public HandleState SyncListReceiptToAccountant(List<Guid> ids)
        {
            var receipts = receiptRepository.Get(x => ids.Contains(x.Id));
            if (receipts == null) return new HandleState((object)"Không tìm thấy phiếu thu");
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var receipt in receipts)
                    {
                        receipt.UserModified = currentUser.UserID;
                        receipt.DatetimeModified = DateTime.Now;
                        receipt.SyncStatus = AccountingConstants.STATUS_SYNCED;
                        receipt.LastSyncDate = DateTime.Now;
                        var hsUpdateReceipt = receiptRepository.Update(receipt, x => x.Id == receipt.Id, false);
                    }
                    var sm = receiptRepository.SubmitChanges();
                    trans.Commit();
                    return sm;
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

        public bool CheckCdNoteSynced(Guid idCdNote)
        {
            var cdNote = cdNoteRepository.Get(x => x.Id == idCdNote).FirstOrDefault();
            if (cdNote != null)
            {
                var surchargeCreditObhs = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH && x.CreditNo == cdNote.Code && !string.IsNullOrEmpty(x.PaySyncedFrom));
                var surchargeDebitObhs = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH && x.DebitNo == cdNote.Code && !string.IsNullOrEmpty(x.SyncedFrom));
                var surchargeCredits = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_BUY && x.CreditNo == cdNote.Code && !string.IsNullOrEmpty(x.SyncedFrom));
                var surchargeDebits = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL && x.DebitNo == cdNote.Code && !string.IsNullOrEmpty(x.SyncedFrom));
                if (surchargeCreditObhs.Any() || surchargeDebitObhs.Any() || surchargeCredits.Any() || surchargeDebits.Any())
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckSoaSynced(int idSoa)
        {
            var soa = soaRepository.Get(x => x.Id == idSoa).FirstOrDefault();
            if (soa != null)
            {
                var surchargeCreditObhs = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH && x.PaySoano == soa.Soano && !string.IsNullOrEmpty(x.PaySyncedFrom));
                var surchargeDebitObhs = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH && x.Soano == soa.Soano && !string.IsNullOrEmpty(x.SyncedFrom));
                var surchargeCredits = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_BUY && x.PaySoano == soa.Soano && !string.IsNullOrEmpty(x.SyncedFrom));
                var surchargeDebits = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL && x.Soano == soa.Soano && !string.IsNullOrEmpty(x.SyncedFrom));
                if (surchargeCreditObhs.Any() || surchargeDebitObhs.Any() || surchargeCredits.Any() || surchargeDebits.Any())
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckVoucherSynced(Guid idVoucher)
        {
            var voucher = DataContext.Get(x => x.Id == idVoucher).FirstOrDefault();
            if (voucher != null)
            {
                //Voucher không issue cho phí Obh Partner (OBH-Debit)
                var surchargeCreditObhs = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH && x.AcctManagementId == voucher.Id && !string.IsNullOrEmpty(x.PaySyncedFrom));
                var surchargeCredits = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_BUY && x.AcctManagementId == voucher.Id && !string.IsNullOrEmpty(x.SyncedFrom));
                var surchargeDebits = SurchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL && x.AcctManagementId == voucher.Id && !string.IsNullOrEmpty(x.SyncedFrom));
                if (surchargeCreditObhs.Any() || surchargeCredits.Any() || surchargeDebits.Any())
                {
                    return true;
                }
            }
            return false;
        }
    }
}
