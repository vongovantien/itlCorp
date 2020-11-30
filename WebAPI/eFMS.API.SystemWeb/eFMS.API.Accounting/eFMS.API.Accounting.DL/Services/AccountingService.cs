using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
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
                                                             CustomerCode = employee.StaffCode,
                                                             OfficeCode = office.Code,
                                                             DocDate = ad.RequestDate,
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
                                                                  DocDate = voucher.Date,
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
                                                                                      OriginalAmount = GetOriginAmountWithAccountNo(item.AccountNo, surcharge),
                                                                                      OriginalAmount3 = GetOrgVatAmountWithAccountNo(item.AccountNo, surcharge),
                                                                                      Amount = surcharge.AmountVnd,
                                                                                      Amount3 = surcharge.VatAmountVnd,

                                                                                      OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? obhP.AccountNo : null,
                                                                                      AtchDocNo = surcharge.InvoiceNo,
                                                                                      AtchDocDate = surcharge.InvoiceDate,
                                                                                      AtchDocSerialNo = surcharge.SeriesNo,
                                                                                      AccountNo = item.AccountNo, // AccountNo của voucher
                                                                                      ContracAccount = chgDef.CreditAccountNo,
                                                                                      VATAccount = chgDef.CreditVat,
                                                                                      ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type),
                                                                                      CustomerCodeBook = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? partnerGrp.AccountNo : obhP.AccountNo,
                                                                                      DueDate = item.PaymentTerm ?? 0
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
                                                                       CustomerName = employee.EmployeeNameVn,
                                                                       CustomerCode = employee.StaffCode,
                                                                       PaymentMethod = settle.PaymentMethod == "Bank" ? "Bank Transfer" : settle.PaymentMethod,
                                                                       CustomerMode = partner.PartnerMode ?? "Internal"
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
                                                                                             OriginalAmount = surcharge.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? Math.Round(surcharge.Quantity * surcharge.UnitPrice ?? 0, 0) : Math.Round(surcharge.Quantity * surcharge.UnitPrice ?? 0, 2),
                                                                                             OriginalAmount3 = GetOrgVatAmount(surcharge.Vatrate, surcharge.Quantity * surcharge.UnitPrice, surcharge.CurrencyId),
                                                                                             OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? obhP.AccountNo : null,
                                                                                             AtchDocNo = surcharge.InvoiceNo,
                                                                                             AtchDocDate = surcharge.InvoiceDate,
                                                                                             AtchDocSerialNo = surcharge.SeriesNo,
                                                                                             ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type),
                                                                                             CustomerCodeBook = obhP.AccountNo
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
                sync.DocDate = cdNote.DatetimeCreated;
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

                int decimalRound = 0;
                var charges = new List<ChargeSyncModel>();
                var surcharges = SurchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                foreach (var surcharge in surcharges)
                {
                    var charge = new ChargeSyncModel();
                    charge.RowId = surcharge.Id.ToString();
                    charge.Ma_SpHt = surcharge.JobNo;
                    var _charge = CatChargeRepository.Get(x => x.Id == surcharge.ChargeId).FirstOrDefault();
                    charge.ItemCode = _charge?.Code;
                    charge.Description = _charge?.ChargeNameVn;
                    var _unit = CatUnitRepository.Get(x => x.Id == surcharge.UnitId).FirstOrDefault();
                    charge.Unit = _unit?.UnitNameVn; //Unit Name En
                    charge.BillEntryNo = surcharge.Hblno;
                    charge.MasterBillNo = surcharge.Mblno;
                    charge.DeptCode = !string.IsNullOrEmpty(_charge?.ProductDept) ? _charge?.ProductDept : GetDeptCode(surcharge.JobNo);
                    charge.NganhCode = "FWD";
                    charge.Quantity9 = surcharge.Quantity;

                    // var _partnerPayer = partners.Where(x => x.Id == surcharge.PayerId).FirstOrDefault();
                    var _partnerPaymentObject = partners.Where(x => x.Id == surcharge.PaymentObjectId).FirstOrDefault(); //CR: 24-11-2020
                    charge.OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? _partnerPaymentObject?.AccountNo : string.Empty;
                    charge.ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type);

                    //Đối với phí DEBIT - Quy đổi theo currency của Debit Note
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL)
                    {
                        if (sync.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)
                        {
                            decimalRound = 2;
                        }

                        charge.CurrencyCode = cdNote.CurrencyId; //Set Currency Charge = Currency Debit Note
                        charge.ExchangeRate = cdNote.ExchangeRate; //Set Exchange Rate of Charge = Exchange Rate of Debit Note
                        // Exchange Rate from currency original charge to currency debit note
                        var _exchargeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, cdNote.CurrencyId);
                        var _unitPrice = surcharge.UnitPrice * _exchargeRate;
                        charge.OriginalUnitPrice = Math.Round(_unitPrice ?? 0, decimalRound); //Đơn giá
                        charge.TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100; //Thuế suất /100
                        var _totalNoVat = surcharge.Quantity * _unitPrice;
                        charge.OriginalAmount = Math.Round(_totalNoVat ?? 0, decimalRound); //Thành tiền chưa thuế
                        var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_totalNoVat * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate * _exchargeRate ?? 0) : 0;
                        charge.OriginalAmount3 = Math.Round(_taxMoney, decimalRound); //Tiền thuế
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
                        var _totalNoVat = surcharge.Quantity * surcharge.UnitPrice;
                        charge.OriginalAmount = Math.Round(_totalNoVat ?? 0, decimalRound); //Thành tiền chưa thuế
                        var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_totalNoVat * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                        charge.OriginalAmount3 = Math.Round(_taxMoney, decimalRound); //Tiền thuế
                    }
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
                    sync.DocDate = cdNote.DatetimeCreated;
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
                        charge.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, "VND");
                        charge.BillEntryNo = surcharge.Hblno;
                        charge.MasterBillNo = surcharge.Mblno;
                        charge.DeptCode = !string.IsNullOrEmpty(_charge?.ProductDept) ? _charge?.ProductDept : GetDeptCode(surcharge.JobNo);
                        charge.NganhCode = "FWD";
                        charge.Quantity9 = surcharge.Quantity;
                        charge.OriginalUnitPrice = Math.Round(surcharge.UnitPrice ?? 0, decimalRound); //Đơn giá
                        charge.TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100; //Thuế suất /100
                        var _totalNoVat = surcharge.Quantity * surcharge.UnitPrice;
                        charge.OriginalAmount = Math.Round(_totalNoVat ?? 0, decimalRound); //Thành tiền chưa thuế
                        var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_totalNoVat * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                        charge.OriginalAmount3 = Math.Round(_taxMoney, decimalRound); //Tiền thuế

                        var _partnerPaymentObject = partners.Where(x => x.Id == surcharge.PaymentObjectId).FirstOrDefault();
                        charge.OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? _partnerPaymentObject?.AccountNo : string.Empty;
                        charge.ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type);
                        charge.CustomerCodeBook = sync.CustomerCode;

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
                sync.DocDate = soa.DatetimeCreated;
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

                int decimalRound = 0;
                var charges = new List<ChargeSyncModel>();
                var surcharges = SurchargeRepository.Get(x => x.Soano == soa.Soano || x.PaySoano == soa.Soano);
                foreach (var surcharge in surcharges)
                {
                    var charge = new ChargeSyncModel();
                    charge.RowId = surcharge.Id.ToString();
                    charge.Ma_SpHt = surcharge.JobNo;
                    var _charge = CatChargeRepository.Get(x => x.Id == surcharge.ChargeId).FirstOrDefault();
                    charge.ItemCode = _charge?.Code;
                    charge.Description = _charge?.ChargeNameVn;
                    var _unit = CatUnitRepository.Get(x => x.Id == surcharge.UnitId).FirstOrDefault();
                    charge.Unit = _unit?.UnitNameVn; //Unit Name En
                    charge.BillEntryNo = surcharge.Hblno;
                    charge.MasterBillNo = surcharge.Mblno;
                    charge.DeptCode = !string.IsNullOrEmpty(_charge?.ProductDept) ? _charge?.ProductDept : GetDeptCode(surcharge.JobNo);
                    charge.NganhCode = "FWD";
                    charge.Quantity9 = surcharge.Quantity;

                    // var _partnerPayer = partners.Where(x => x.Id == surcharge.PayerId).FirstOrDefault();
                    var _partnerPaymentObject = partners.Where(x => x.Id == surcharge.PaymentObjectId).FirstOrDefault(); //CR: 24-11-2020
                    charge.OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? _partnerPaymentObject?.AccountNo : string.Empty;
                    charge.ChargeType = surcharge.Type.ToUpper() == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type);

                    //Đối với phí DEBIT - Quy đổi theo currency của SOA (Type Debit)
                    if (surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL)
                    {
                        if (sync.CurrencyCode != AccountingConstants.CURRENCY_LOCAL)
                        {
                            decimalRound = 2;
                        }

                        charge.CurrencyCode = sync.CurrencyCode; //Set Currency Charge = Currency SOA
                        charge.ExchangeRate = sync.ExchangeRate; //Set Exchange Rate of Charge = Exchange Rate of SOA
                        // Exchange Rate from currency original charge to currency SOA
                        var _exchargeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, sync.CurrencyCode);
                        var _unitPrice = surcharge.UnitPrice * _exchargeRate;
                        charge.OriginalUnitPrice = Math.Round(_unitPrice ?? 0, decimalRound); //Đơn giá
                        charge.TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100; //Thuế suất /100
                        var _totalNoVat = surcharge.Quantity * _unitPrice;
                        charge.OriginalAmount = Math.Round(_totalNoVat ?? 0, decimalRound); //Thành tiền chưa thuế
                        var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_totalNoVat * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate * _exchargeRate ?? 0) : 0;
                        charge.OriginalAmount3 = Math.Round(_taxMoney, decimalRound); //Tiền thuế
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
                        var _totalNoVat = surcharge.Quantity * surcharge.UnitPrice;
                        charge.OriginalAmount = Math.Round(_totalNoVat ?? 0, decimalRound); //Thành tiền chưa thuế
                        var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_totalNoVat * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                        charge.OriginalAmount3 = Math.Round(_taxMoney, decimalRound); //Tiền thuế
                    }
                    charges.Add(charge);
                }
                sync.Details = charges;

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
                    sync.DocDate = soa.DatetimeCreated;
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
                        charge.OriginalUnitPrice = Math.Round(surcharge.UnitPrice ?? 0, decimalRound);
                        charge.TaxRate = surcharge.Vatrate < 0 ? null : (decimal?)(surcharge.Vatrate ?? 0) / 100; //Thuế suất /100
                        var _totalNoVat = surcharge.Quantity * surcharge.UnitPrice;
                        charge.OriginalAmount = Math.Round(_totalNoVat ?? 0, decimalRound); //Thành tiền chưa thuế
                        var _taxMoney = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_totalNoVat * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;
                        charge.OriginalAmount3 = Math.Round(_taxMoney, decimalRound); //Tiền thuế

                        var _partnerPaymentObject = partners.Where(x => x.Id == surcharge.PaymentObjectId).FirstOrDefault();
                        charge.OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? _partnerPaymentObject?.AccountNo : string.Empty;
                        charge.ChargeType = surcharge.Type.ToUpper() == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type);
                        charge.AccountNo = string.Empty;
                        charge.ContraAccount = string.Empty;
                        charge.VATAccount = string.Empty;
                        charge.AtchDocNo = surcharge.InvoiceNo;
                        charge.AtchDocDate = surcharge.InvoiceDate;
                        charge.AtchDocSerialNo = surcharge.SeriesNo;
                        charge.CustomerCodeBook = sync.CustomerCode;

                        charges.Add(charge);
                    }
                    sync.Details = charges;

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
                sync.DocDate = invoice.Date; //Invoice Date
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
                sync.DocDate = soa.DatetimeCreated; //Created Date SOA
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

                                // Notify
                                SysNotifications sysNotify = new SysNotifications
                                {
                                    Id = Guid.NewGuid(),
                                    Action = "Detail",
                                    DatetimeCreated = DateTime.Now,
                                    DatetimeModified = DateTime.Now,
                                    IsClosed = false,
                                    IsRead = false,
                                    Type = "User",
                                    UserCreated = currentUser.UserID,
                                    UserModified = currentUser.UserID,
                                    Title = string.Format(@"Advance {0} has been synced", adv.AdvanceNo),
                                    Description = "",
                                    ActionLink = string.Format(@"home/accounting/advance-payment/{0}/approve", adv.Id),
                                    UserIds = currentUser.UserID + "," + adv.UserCreated,
                                };

                                sysNotifyRepository.Add(sysNotify, false);

                                SysUserNotification sysUserNotify = new SysUserNotification
                                {
                                    Id = Guid.NewGuid(),
                                    Status = "New",
                                    NotitficationId = sysNotify.Id,
                                    UserCreated = currentUser.UserID,
                                    UserModified = currentUser.UserID,
                                    DatetimeCreated = DateTime.Now,
                                    DatetimeModified = DateTime.Now,
                                    UserId = currentUser.UserID
                                };

                                sysUserNotifyRepository.Add(sysUserNotify, false);
                                if (adv.UserCreated != currentUser.UserID)
                                {
                                    SysUserNotification sysUserNotifySync = new SysUserNotification
                                    {
                                        Id = Guid.NewGuid(),
                                        Status = "New",
                                        NotitficationId = sysNotify.Id,
                                        UserCreated = currentUser.UserID,
                                        UserModified = currentUser.UserID,
                                        DatetimeCreated = DateTime.Now,
                                        DatetimeModified = DateTime.Now,
                                        UserId = adv.UserCreated,
                                    };
                                    sysUserNotifyRepository.Add(sysUserNotifySync, false);
                                }
                            }

                        }
                        HandleState hs = AdvanceRepository.SubmitChanges();
                        if (hs.Success)
                        {
                            sysNotifyRepository.SubmitChanges();
                            sysUserNotifyRepository.SubmitChanges();
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
                                // Notify
                                SysNotifications sysNotify = new SysNotifications
                                {
                                    Id = Guid.NewGuid(),
                                    Action = "Detail",
                                    DatetimeCreated = DateTime.Now,
                                    DatetimeModified = DateTime.Now,
                                    IsClosed = false,
                                    IsRead = false,
                                    Type = "User",
                                    UserCreated = currentUser.UserID,
                                    UserModified = currentUser.UserID,
                                    Title = string.Format(@"Settlement {0} has been synced", settle.SettlementNo),
                                    Description = "",
                                    ActionLink = string.Format(@"home/accounting/settlement-payment/{0}/approve", settle.Id),
                                    UserIds = currentUser.UserID + "," + settle.UserCreated,
                                };

                                sysNotifyRepository.Add(sysNotify, false);

                                SysUserNotification sysUserNotify = new SysUserNotification
                                {
                                    Id = Guid.NewGuid(),
                                    Status = "New",
                                    NotitficationId = sysNotify.Id,
                                    UserCreated = currentUser.UserID,
                                    UserModified = currentUser.UserID,
                                    DatetimeCreated = DateTime.Now,
                                    DatetimeModified = DateTime.Now,
                                    UserId = currentUser.UserID
                                };

                                sysUserNotifyRepository.Add(sysUserNotify, false);
                                if (settle.UserCreated != currentUser.UserID)
                                {
                                    SysUserNotification sysUserNotifySync = new SysUserNotification
                                    {
                                        Id = Guid.NewGuid(),
                                        Status = "New",
                                        NotitficationId = sysNotify.Id,
                                        UserCreated = currentUser.UserID,
                                        UserModified = currentUser.UserID,
                                        DatetimeCreated = DateTime.Now,
                                        DatetimeModified = DateTime.Now,
                                        UserId = settle.UserCreated,
                                    };
                                    sysUserNotifyRepository.Add(sysUserNotifySync, false);
                                }
                            }
                        }

                        result = SettlementRepository.SubmitChanges();
                        if (result.Success)
                        {
                            sysNotifyRepository.SubmitChanges();
                            sysUserNotifyRepository.SubmitChanges();
                        }

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

                                // Notify
                                SysNotifications sysNotify = new SysNotifications
                                {
                                    Id = Guid.NewGuid(),
                                    Action = "Detail",
                                    DatetimeCreated = DateTime.Now,
                                    DatetimeModified = DateTime.Now,
                                    IsClosed = false,
                                    IsRead = false,
                                    Type = "User",
                                    UserCreated = currentUser.UserID,
                                    UserModified = currentUser.UserID,
                                    Title = string.Format(@"Voucher {0} has been synced", voucher.VoucherId),
                                    Description = "",
                                    ActionLink = string.Format(@"home/accounting/management/voucher/{0}", voucher.Id),
                                    UserIds = currentUser.UserID + "," + voucher.UserCreated,
                                };

                                sysNotifyRepository.Add(sysNotify, false);

                                SysUserNotification sysUserNotify = new SysUserNotification
                                {
                                    Id = Guid.NewGuid(),
                                    Status = "New",
                                    NotitficationId = sysNotify.Id,
                                    UserCreated = currentUser.UserID,
                                    UserModified = currentUser.UserID,
                                    DatetimeCreated = DateTime.Now,
                                    DatetimeModified = DateTime.Now,
                                    UserId = currentUser.UserID,
                                };

                                sysUserNotifyRepository.Add(sysUserNotify, false);
                                if (voucher.UserCreated != currentUser.UserID)
                                {
                                    SysUserNotification sysUserNotifySync = new SysUserNotification
                                    {
                                        Id = Guid.NewGuid(),
                                        Status = "New",
                                        NotitficationId = sysNotify.Id,
                                        UserCreated = currentUser.UserID,
                                        UserModified = currentUser.UserID,
                                        DatetimeCreated = DateTime.Now,
                                        DatetimeModified = DateTime.Now,
                                        UserId = voucher.UserCreated
                                    };
                                    sysUserNotifyRepository.Add(sysUserNotifySync, false);
                                }
                            }
                        }

                        result = DataContext.SubmitChanges();
                        if (result.Success)
                        {
                            sysNotifyRepository.SubmitChanges();
                            sysUserNotifyRepository.SubmitChanges();
                        }
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
                        var hsUpdateCdNote = cdNoteRepository.Update(cdNote, x => x.Id == cdNote.Id, false);

                        string description = string.Format(@"CD Note {0} has been synced", cdNote.Code);
                        // Add Notification
                        SysNotifications sysNotification = new SysNotifications
                        {
                            Id = Guid.NewGuid(),
                            Title = description,
                            Description = "",
                            Type = "User",
                            UserCreated = currentUser.UserID,
                            DatetimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            UserModified = currentUser.UserID,
                            Action = "Detail",
                            ActionLink = GetLinkCdNote(cdNote.Code, cdNote.JobId),
                            IsClosed = false,
                            IsRead = false,
                            UserIds = currentUser.UserID + "," + cdNote.UserCreated,
                        };
                        HandleState hsSysNotification = sysNotifyRepository.Add(sysNotification, false);
                        if (hsSysNotification.Success)
                        {
                            SysUserNotification userNotifySync = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                Status = "New",
                                NotitficationId = sysNotification.Id,
                                UserId = currentUser.UserID,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            var hsUserSync = sysUserNotifyRepository.Add(userNotifySync, false);
                            if (cdNote.UserCreated != currentUser.UserID)
                            {
                                SysUserNotification userNotifyCreated = new SysUserNotification
                                {
                                    Id = Guid.NewGuid(),
                                    DatetimeCreated = DateTime.Now,
                                    DatetimeModified = DateTime.Now,
                                    Status = "New",
                                    NotitficationId = sysNotification.Id,
                                    UserId = cdNote.UserCreated,
                                    UserCreated = currentUser.UserID,
                                    UserModified = currentUser.UserID,
                                };
                                var hsUserCreated = sysUserNotifyRepository.Add(userNotifyCreated, false);
                            }
                        }
                    }
                    var smNotify = sysNotifyRepository.SubmitChanges();
                    var smUserNotify = sysUserNotifyRepository.SubmitChanges();
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
                        var hsUpdateSOA = soaRepository.Update(soa, x => x.Id == soa.Id, false);

                        string description = string.Format(@"SOA No {0} has been synced", soa.Soano);
                        // Add Notification
                        SysNotifications sysNotification = new SysNotifications
                        {
                            Id = Guid.NewGuid(),
                            Title = description,
                            Description = "",
                            Type = "User",
                            UserCreated = currentUser.UserID,
                            DatetimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            UserModified = currentUser.UserID,
                            Action = "Detail",
                            ActionLink = string.Format(@"home/accounting/statement-of-account/detail?no={0}&currency=VND", soa.Soano),
                            IsClosed = false,
                            IsRead = false,
                            UserIds = soa.UserCreated + "," + currentUser.UserID,
                        };
                        HandleState hsSysNotification = sysNotifyRepository.Add(sysNotification, false);
                        if (hsSysNotification.Success)
                        {
                            SysUserNotification userNotifySync = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                Status = "New",
                                NotitficationId = sysNotification.Id,
                                UserId = currentUser.UserID,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            var hsUserSync = sysUserNotifyRepository.Add(userNotifySync, false);
                            if (soa.UserCreated != currentUser.UserID)
                            {
                                SysUserNotification userNotifyCreated = new SysUserNotification
                                {
                                    Id = Guid.NewGuid(),
                                    DatetimeCreated = DateTime.Now,
                                    DatetimeModified = DateTime.Now,
                                    Status = "New",
                                    NotitficationId = sysNotification.Id,
                                    UserId = soa.UserCreated,
                                    UserCreated = currentUser.UserID,
                                    UserModified = currentUser.UserID,
                                };
                                var hsUserCreated = sysUserNotifyRepository.Add(userNotifyCreated, false);
                            }
                        }
                    }
                    var smNotify = sysNotifyRepository.SubmitChanges();
                    var smUserNotify = sysUserNotifyRepository.SubmitChanges();
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
                //_deptCode = "OPS";
                deptCode = "ITLOPS";
            }
            else if (JobNo.Contains("A"))
            {
                //_deptCode = "AIR";
                deptCode = "ITLAIR";
            }
            else if (JobNo.Contains("S"))
            {
                //_deptCode = "SEA";
                deptCode = "ITLCS";
            }

            return deptCode;
        }
        private decimal GetOrgVatAmount(decimal? vatrate, decimal? orgAmount, string currency)
        {
            decimal amount = 0;
            amount = (vatrate != null) ? (vatrate < 101 & vatrate >= 0) ? Math.Round(((orgAmount * vatrate) / 100 ?? 0), 3) : Math.Abs(vatrate ?? 0) : 0;
            if (currency == AccountingConstants.CURRENCY_LOCAL)
            {
                amount = Math.Round(amount, 0);
            }
            return amount;
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
                _unitPrice = Math.Round(unitPrice * finalExchange);
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
            }
            else
            {
                // Tính toán như cũ
                _originAmount = (surcharge.Quantity * surcharge.UnitPrice) ?? 0;
                if (surcharge.CurrencyId == AccountingConstants.CURRENCY_LOCAL)
                {
                    _originAmount = Math.Round(_originAmount, 0);
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
            }
            else
            {
                // Tính toán như cũ
                _orgVatAmout = GetOrgVatAmount(surcharge.Vatrate, surcharge.Quantity * surcharge.UnitPrice, surcharge.CurrencyId);
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
                    }

                    //Send Mail
                    SendEmailToAccountant(type, creatorEnName, refNo, partnerEn, taxCode, serviceName, amountCurr, urlFunc, syncCreditModel.PaymentMethod);
                    //Push Notification
                    PushNotificationToAccountant(type, creatorEnName, refNo, serviceName, amountCurr, urlFunc);
                }
            }
        }

        private void SendEmailToAccountant(string type, string creatorEnName, string refNo, string partnerEn, string taxCode, string serviceName, string amountCurr, string urlFunc, string paymentMethod)
        {
            string _type = type == "CDNOTE" ? "Credit Note" : "SOA";
            string subject = string.Format(@"eFMS - Voucher Request - {0} {1}", _type, refNo);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color: #004080'>" +
                                            "<p><i>Dear Accountant Team,</i></p>" +
                                            "<p>" +
                                                "<div>You received a <b>[SOA_CreditNote]</b> from <b>[CreatorEnName]</b> as info bellow:</div>" +
                                                "<div><i>Bạn có nhận một đề nghị thanh toán chi phí bằng <b>[SOA_CreditNote]</b> từ <b>[CreatorEnName]</b> với thông tin như sau: </i></div>" +
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
            body = body.Replace("[SOA_CreditNote]", _type);
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

            List<string> emailCCs = new List<string> { };
            List<string> emailBCCs = new List<string> { "alex.phuong@itlvn.com" };
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

        private void PushNotificationToAccountant(string type, string creatorEnName, string refNo, string serviceName, string amountCurr, string urlFunc)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var idAccountantDept = departmentRepo.Get(x => x.DeptType == AccountingConstants.DeptTypeAccountant && x.BranchId == currentUser.OfficeID).FirstOrDefault()?.Id;
                    // Danh sách user Id của group thuộc department Accountant (Không lấy manager của department Acct)
                    var idUserGroupAccts = sysUserLevelRepo.Get(x => x.GroupId != AccountingConstants.SpecialGroup && x.DepartmentId == idAccountantDept).Select(s => s.UserId);

                    string _type = type == "CDNOTE" ? "Credit Note" : "SOA";
                    string title = string.Format(@"Voucher Request - {0}: {1}", _type, refNo);
                    string description = string.Format(@"You received a <b>{0}</b> from <b>{1}</b>. Ref No <b>{2}</b> of <b>{3}</b> with Amount <b>{4}</b>", _type, creatorEnName, refNo, serviceName, amountCurr);

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
    }
}
