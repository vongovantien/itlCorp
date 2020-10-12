using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Collections.Generic;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;

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
                                                             Office = office.Code,
                                                             DocDate = ad.RequestDate,
                                                             ExchangeRate = GetExchangeRate(ad.RequestDate, ad.AdvanceCurrency)
                                                         };
                List<BravoAdvanceModel> data = queryAdv.ToList();
                foreach (var item in data)
                {
                    // Ds advance request
                    List<BravoAdvanceRequestModel> advR = AdvanceRequestRepository.Get(x => x.AdvanceNo == item.ReferenceNo).Select(x => new BravoAdvanceRequestModel
                    {
                        RowId = x.Id,
                        Ma_SpHt = x.JobId,
                        BillEntryNo = x.Hbl,
                        MasterBillNo = x.Mbl,
                        OriginalAmount = x.Amount,
                        Description = x.RequestNote,
                        DeptCode = GetDeptCode(x.JobId),
                    }).ToList();

                    if (advR.Count > 0)
                    {
                        item.Details = advR;
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
                                                                  Office = office.Code,
                                                                  ReferenceNo = voucher.VoucherId,
                                                                  CurrencyCode = voucher.Currency,
                                                                  ExchangeRate = GetExchangeRate(voucher.Date, voucher.Currency),
                                                                  Description0 = voucher.Description,
                                                                  AccountNo = voucher.AccountNo,
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
                                                                                  join chgDef in chargesDefault on charge.Id equals chgDef.ChargeId into chgDef2
                                                                                  from chgDef in chgDef2.DefaultIfEmpty()
                                                                                  join unit in catUnits on surcharge.UnitId equals unit.Id
                                                                                  select new BravoVoucherChargeModel
                                                                                  {
                                                                                      RowId = surcharge.Id,
                                                                                      ItemCode = charge.Code,
                                                                                      Description = charge.ChargeNameVn,
                                                                                      Unit = unit.UnitNameVn,
                                                                                      CurrencyCode = surcharge.CurrencyId,
                                                                                      ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, item.CurrencyCode),
                                                                                      BillEntryNo = surcharge.Hblno,
                                                                                      Ma_SpHt = surcharge.JobNo,
                                                                                      MasterBillNo = surcharge.Mblno,
                                                                                      DeptCode = string.IsNullOrEmpty(charge.ProductDept) ? GetDeptCode(surcharge.JobNo) : charge.ProductDept,
                                                                                      Quantity9 = surcharge.Quantity,
                                                                                      OriginalUnitPrice = surcharge.UnitPrice,
                                                                                      TaxRate = surcharge.Vatrate,
                                                                                      OriginalAmount = surcharge.Quantity * surcharge.UnitPrice,
                                                                                      OriginalAmount3 = GetOrgVatAmount(surcharge.Vatrate, surcharge.Quantity * surcharge.UnitPrice),
                                                                                      OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? obhP.AccountNo : null,
                                                                                      AtchDocNo = surcharge.InvoiceNo,
                                                                                      AtchDocDate = surcharge.InvoiceDate,
                                                                                      AtchDocSerieNo = surcharge.SeriesNo,
                                                                                      AccountNo = item.AccountNo, // AccountNo của voucher
                                                                                      ContracAccount = chgDef.CreditAccountNo,
                                                                                      VATAccount = chgDef.CreditVat,
                                                                                      ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type),
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
                IQueryable<AcctSettlementPayment> settlements = SettlementRepository.Get(x => Ids.Contains(x.Id) && x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);

                IQueryable<BravoSettlementModel> querySettlement = from settle in settlements
                                                                   join user in users on settle.Requester equals user.Id
                                                                   join employee in employees on user.EmployeeId equals employee.Id
                                                                   join office in offices on settle.OfficeId equals office.Id
                                                                   select new BravoSettlementModel
                                                                   {
                                                                       Stt = settle.Id,
                                                                       Office = office.Code,
                                                                       DocDate = settle.RequestDate,
                                                                       ReferenceNo = settle.SettlementNo,
                                                                       ExchangeRate = GetExchangeRate(settle.RequestDate, settle.SettlementCurrency),
                                                                       Description0 = settle.Note,
                                                                       CustomerName = employee.EmployeeNameVn,
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
                                                                                             ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, item.CurrencyCode),
                                                                                             BillEntryNo = surcharge.Hblno,
                                                                                             Ma_SpHt = surcharge.JobNo,
                                                                                             MasterBillNo = surcharge.Mblno,
                                                                                             DeptCode = string.IsNullOrEmpty(charge.ProductDept) ? GetDeptCode(surcharge.JobNo) : charge.ProductDept,
                                                                                             Quantity9 = surcharge.Quantity,
                                                                                             OriginalUnitPrice = surcharge.UnitPrice,
                                                                                             TaxRate = surcharge.Vatrate,
                                                                                             OriginalAmount = surcharge.Quantity * surcharge.UnitPrice,
                                                                                             OriginalAmount3 = GetOrgVatAmount(surcharge.Vatrate, surcharge.Quantity * surcharge.UnitPrice),
                                                                                             OBHPartnerCode = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? obhP.AccountNo : null,
                                                                                             AtchDocNo = surcharge.InvoiceNo,
                                                                                             AtchDocDate = surcharge.InvoiceDate,
                                                                                             AtchDocSerieNo = surcharge.SeriesNo,
                                                                                             ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type),
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
        /// Get data list cd note to sync accountant
        /// </summary>
        /// <param name="Ids">List Id of cd note</param>
        /// <param name="type">Type: DEBIT/CREDIT/ALL</param>
        /// <returns></returns>
        public List<SyncModel> GetListCdNoteToSync(List<Guid> ids, string type)
        {
            List<SyncModel> data = new List<SyncModel>();
            if (ids == null || ids.Count() == 0) return data;

            var cdNotes = cdNoteRepository.Get(x => ids.Contains(x.Id));
            if (type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_DEBIT)
            {
                cdNotes = cdNotes.Where(x => x.Type.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || x.Type.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_INVOICE);
            }
            else if (type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
            {
                cdNotes = cdNotes.Where(x => x.Type.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT);
            }
           
            foreach (var cdNote in cdNotes)
            {
                SyncModel sync = new SyncModel();
                sync.Stt = cdNote.Id.ToString();
                sync.BranchCode = string.Empty;
                sync.Office = offices.Where(x => x.Id == cdNote.OfficeId).FirstOrDefault()?.Code;
                sync.Transcode = string.Empty;
                sync.DocDate = cdNote.DatetimeCreated;
                sync.ReferenceNo = cdNote.Code;
                var cdNotePartner = partners.Where(x => x.Id == cdNote.PartnerId).FirstOrDefault();
                sync.CustomerCode = cdNotePartner?.AccountNo; //Partner Code
                sync.CustomerName = cdNotePartner?.PartnerNameVn; //Partner Local Name
                sync.CustomerMode = cdNotePartner?.PartnerMode;
                sync.LocalBranchCode = cdNotePartner?.InternalCode; //Parnter Internal Code
                sync.CurrencyCode = "VND"; //để trống
                sync.ExchangeRate = 1;
                sync.Description0 = string.Empty;
                sync.DataType = "CDNOTE";

                var charges = new List<ChargeSyncModel>();
                var surcharges = SurchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                foreach(var surcharge in surcharges)
                {
                    var charge = new ChargeSyncModel();
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
                    charge.OriginalUnitPrice = surcharge.UnitPrice;
                    charge.TaxRate = surcharge.Vatrate;
                    var _totalNoVat = surcharge.Quantity * surcharge.UnitPrice;
                    charge.OriginalAmount = _totalNoVat;
                    charge.OriginalAmount3 = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_totalNoVat * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;

                    var _partnerPayer = partners.Where(x => x.Id == surcharge.PayerId).FirstOrDefault();
                    var _partnerPaymentObject = partners.Where(x => x.Id == surcharge.PaymentObjectId).FirstOrDefault();
                    charge.OBHPartnerCode = cdNote.Type == AccountingConstants.ACCOUNTANT_TYPE_DEBIT || cdNote.Type == AccountingConstants.ACCOUNTANT_TYPE_INVOICE ? _partnerPayer?.AccountNo : _partnerPaymentObject?.AccountNo;
                    charge.ChargeType = surcharge.Type == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type);

                    if (cdNote.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
                    {
                        charge.AccountNo = string.Empty;
                        charge.ContraAccount = string.Empty;
                        charge.VATAccount = string.Empty;
                        charge.AtchDocNo = surcharge.InvoiceNo;
                        charge.AtchDocDate = surcharge.InvoiceDate;
                        charge.AtchDocSerialNo = surcharge.SeriesNo;
                    }

                    charges.Add(charge);
                }
                sync.Details = charges;

                data.Add(sync);
            }

            return data;
        }

        /// <summary>
        /// Get data list soa to sync accountant
        /// </summary>
        /// <param name="Ids">List Id of soa</param>
        /// <param name="type">Type: DEBIT/CREDIT/ALL</param>
        /// <returns></returns>
        public List<SyncModel> GetListSoaToSync(List<int> ids, string type)
        {
            List<SyncModel> data = new List<SyncModel>();
            if (ids == null || ids.Count() == 0) return data;

            var soas = soaRepository.Get(x => ids.Contains(x.Id));
            if (type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_DEBIT)
            {
                soas = soas.Where(x => x.Type.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_DEBIT);
            }
            else if (type?.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
            {
                soas = soas.Where(x => x.Type.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT);
            }
            
            foreach(var soa in soas)
            {
                SyncModel sync = new SyncModel();
                sync.Stt = soa.Id.ToString();
                sync.BranchCode = string.Empty;
                sync.Office = offices.Where(x => x.Id == soa.OfficeId).FirstOrDefault()?.Code;
                sync.Transcode = string.Empty;
                sync.DocDate = soa.DatetimeCreated;
                sync.ReferenceNo = soa.Soano;
                var soaPartner = partners.Where(x => x.Id == soa.Customer).FirstOrDefault();
                sync.CustomerCode = soaPartner?.AccountNo; //Partner Code
                sync.CustomerName = soaPartner?.PartnerNameVn; //Partner Local Name
                sync.CustomerMode = soaPartner?.PartnerMode;
                sync.LocalBranchCode = soaPartner?.InternalCode; //Parnter Internal Code
                sync.CurrencyCode = soa.Currency;
                sync.ExchangeRate = 1;
                sync.Description0 = soa.Note;
                sync.DataType = "SOA";

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
                    charge.CurrencyCode = surcharge.CurrencyId;
                    charge.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, soa.Currency);
                    charge.BillEntryNo = surcharge.Hblno;
                    charge.MasterBillNo = surcharge.Mblno;
                    charge.DeptCode = !string.IsNullOrEmpty(_charge?.ProductDept) ? _charge?.ProductDept : GetDeptCode(surcharge.JobNo);
                    charge.NganhCode = "FWD";
                    charge.Quantity9 = surcharge.Quantity;
                    charge.OriginalUnitPrice = surcharge.UnitPrice;
                    charge.TaxRate = surcharge.Vatrate;
                    var _totalNoVat = surcharge.Quantity * surcharge.UnitPrice;
                    charge.OriginalAmount = _totalNoVat;
                    charge.OriginalAmount3 = (surcharge.Vatrate != null) ? (surcharge.Vatrate < 101 & surcharge.Vatrate >= 0) ? ((_totalNoVat * surcharge.Vatrate) / 100 ?? 0) : Math.Abs(surcharge.Vatrate ?? 0) : 0;

                    var _partnerPayer = partners.Where(x => x.Id == surcharge.PayerId).FirstOrDefault();
                    var _partnerPaymentObject = partners.Where(x => x.Id == surcharge.PaymentObjectId).FirstOrDefault();
                    charge.OBHPartnerCode = soa.Type.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_DEBIT ? _partnerPayer?.AccountNo : _partnerPaymentObject?.AccountNo;
                    charge.ChargeType = surcharge.Type.ToUpper() == AccountingConstants.TYPE_CHARGE_SELL ? AccountingConstants.ACCOUNTANT_TYPE_DEBIT : (surcharge.Type == AccountingConstants.TYPE_CHARGE_BUY ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : surcharge.Type);

                    if (soa.Type.ToUpper() == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
                    {
                        charge.AccountNo = string.Empty;
                        charge.ContraAccount = string.Empty;
                        charge.VATAccount = string.Empty;
                        charge.AtchDocNo = surcharge.InvoiceNo;
                        charge.AtchDocDate = surcharge.InvoiceDate;
                        charge.AtchDocSerialNo = surcharge.SeriesNo;
                    }

                    charges.Add(charge);
                }
                sync.Details = charges;

                data.Add(sync);
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
                sync.Office = offices.Where(x => x.Id == invoice.OfficeId).FirstOrDefault()?.Code;
                sync.DocDate = invoice.Date; //Invoice Date
                sync.ReferenceNo = invoice.InvoiceNoReal; //Invoice No
                var invoicePartner = partners.Where(x => x.Id == invoice.PartnerId).FirstOrDefault();
                sync.CustomerCode = invoicePartner?.AccountNo; //Partner Code
                sync.CustomerName = invoicePartner?.PartnerNameVn; //Partner Local Name
                sync.CurrencyCode = invoice.Currency;
                sync.ExchangeRate = 1;
                sync.Description0 = invoice.PaymentNote;
                sync.DataType = "PAYMENT";

                var payments = accountingPaymentRepository.Get(x => x.RefId == invoice.Id.ToString());
                var details = new List<PaymentDetailModel>();
                foreach(var payment in payments)
                {
                    var detail = new PaymentDetailModel();
                    detail.RowId = payment.Id.ToString();
                    detail.OriginalAmount = payment.PaymentAmount;
                    detail.Description = string.Empty;
                    detail.ObhPartnerCode = string.Empty; //Để trống
                    detail.BankAccountNo = invoicePartner?.BankAccountNo; //Partner Bank Account no
                    detail.Stt_Cd_Htt = null;
                    detail.ChargeType = "DEBIT";

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
            foreach(var soa in soas)
            {
                PaymentModel sync = new PaymentModel();
                sync.Stt = soa.Id.ToString();
                sync.BranchCode = string.Empty;
                sync.Office = offices.Where(x => x.Id == soa.OfficeId).FirstOrDefault()?.Code;
                sync.DocDate = soa.DatetimeCreated; //Created Date SOA
                sync.ReferenceNo = soa.Soano; //SOA No
                var soaPartner = partners.Where(x => x.Id == soa.Customer).FirstOrDefault();
                sync.CustomerCode = soaPartner?.AccountNo; //Partner Code
                sync.CustomerName = soaPartner?.PartnerNameVn; //Partner Local Name
                sync.CurrencyCode = soa.Currency;
                sync.ExchangeRate = 1;
                sync.Description0 = soa.PaymentNote;
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
                    detail.Stt_Cd_Htt = null;
                    detail.ChargeType = "OBH";

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

                result = AdvanceRepository.SubmitChanges();
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
                    }
                }

                result = SettlementRepository.SubmitChanges();
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
                    }
                }

                result = DataContext.SubmitChanges();
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
                    }
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
                    }
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

        private decimal GetOrgVatAmount(decimal? vatrate, decimal? orgAmount)
        {
            decimal amount = 0;
            amount = (vatrate != null) ? (vatrate < 101 & vatrate >= 0) ? Math.Round(((orgAmount * vatrate) / 100 ?? 0), 3) : Math.Abs(vatrate ?? 0) : 0;
            return amount;
        }


        #endregion -- Private Method --

    }
}
