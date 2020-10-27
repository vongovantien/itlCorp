using eFMS.API.ForPartner.Service.Models;
using eFMS.API.ForPartner.DL.Models;
using ITL.NetCore.Connection.BL;
using System;
using ITL.NetCore.Connection.EF;
using eFMS.IdentityServer.DL.UserManager;
using AutoMapper;
using System.Linq;
using ITL.NetCore.Common;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Hosting;
using eFMS.API.Common.Helpers;
using Microsoft.Extensions.Options;
using eFMS.API.Common;
using Newtonsoft.Json;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.Common.Globals;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.DL.Service
{
    public class AccAccountingManagementService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingManagementService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysPartnerApi> sysPartnerApiRepository;
        private readonly IHostingEnvironment environment;
        private readonly IOptions<AuthenticationSetting> configSetting;
        private readonly IContextBase<AcctAdvancePayment> acctAdvanceRepository;
        private readonly IContextBase<AcctSoa> acctSOARepository;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepo;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CatPartner> partnerRepo;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<AcctSettlementPayment> acctSettlementRepo;
        private readonly IContextBase<AcctCdnote> acctCdNoteRepo;
        private readonly IActionFuncLogService actionFuncLogService;
        private readonly IContextBase<AcctSettlementPayment> settlementPaymentRepo;
        private readonly IContextBase<CatCurrencyExchange> currencyExchangeRepo;

        public AccAccountingManagementService(
            IContextBase<AccAccountingManagement> repository,
            IContextBase<SysPartnerApi> sysPartnerApiRep,
            IContextBase<AcctAdvancePayment> acctAdvanceRepo,
            IContextBase<AcctSoa> acctSOARepo,
            IOptions<AuthenticationSetting> config,
            IHostingEnvironment env,
            IMapper mapper,
            ICurrentUser cUser,
            IContextBase<CsShipmentSurcharge> csShipmentSurcharge,
            IStringLocalizer<ForPartnerLanguageSub> localizer,
            IContextBase<CatPartner> catPartner,
            ICurrencyExchangeService exchangeService,
            IContextBase<CatCurrencyExchange> catCurrencyExchange,
            IContextBase<AcctSettlementPayment> acctSettlementPayment,
            IContextBase<AcctCdnote> acctCdnote,
            IActionFuncLogService actionFuncLog,
            IContextBase<AcctSettlementPayment> settlementPayment,
            IContextBase<CatCurrencyExchange> currencyExchange
            ) : base(repository, mapper)
        {
            currentUser = cUser;
            sysPartnerApiRepository = sysPartnerApiRep;
            acctAdvanceRepository = acctAdvanceRepo;
            acctSOARepository = acctSOARepo;
            environment = env;
            configSetting = config;
            surchargeRepo = csShipmentSurcharge;
            stringLocalizer = localizer;
            partnerRepo = catPartner;
            currencyExchangeService = exchangeService;
            acctSettlementRepo = acctSettlementPayment;
            acctCdNoteRepo = acctCdnote;
            actionFuncLogService = actionFuncLog;
            settlementPaymentRepo = settlementPayment;
            currencyExchangeRepo = currencyExchange;
        }

        public AccAccountingManagementModel GetById(Guid id)
        {
            AccAccountingManagement accounting = DataContext.Get(x => x.Id == id).FirstOrDefault();
            AccAccountingManagementModel result = mapper.Map<AccAccountingManagementModel>(accounting);
            return result;
        }
        public bool ValidateApiKey(string apiKey)
        {
            bool isValid = false;
            SysPartnerApi partnerAPiInfo = sysPartnerApiRepository.Get(x => x.ApiKey == apiKey).FirstOrDefault();
            if (partnerAPiInfo != null && partnerAPiInfo.Active == true)
            {
                isValid = true;
            }
            return isValid;
        }

        public bool ValidateHashString(object body, string apiKey, string hash)
        {
            bool valid = false;
            if (body != null)
            {
                string bodyString = apiKey + configSetting.Value.PartnerShareKey;

                string eFmsHash = Md5Helper.CreateMD5(bodyString.ToLower());

                if (eFmsHash.ToLower() == hash.ToLower())
                {
                    valid = true;
                }
                else
                {
                    valid = false;
                }
            }

            return valid;
        }

        public string GenerateHashStringTest(object body, string apiKey)
        {
            object data = body;
            string bodyString = apiKey + configSetting.Value.PartnerShareKey;
            return Md5Helper.CreateMD5(bodyString.ToLower());
        }

        #region --- CRUD INVOICE ---
        public HandleState InsertInvoice(InvoiceCreateInfo model, string apiKey, string funcController)
        {
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;

            var hsInsertInvoice = InsertInvoice(model, currentUser);

            #region -- Ghi Log --
            var modelLog = new SysActionFuncLogModel
            {
                FuncLocal = string.Format(@"InsertInvoice ({0})", funcController),
                ObjectRequest = JsonConvert.SerializeObject(model),
                ObjectResponse = JsonConvert.SerializeObject(hsInsertInvoice),
                Major = "Tạo Hóa Đơn"
            };
            var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
            #endregion

            return hsInsertInvoice;
        }

        private HandleState InsertInvoice(InvoiceCreateInfo model, ICurrentUser _currentUser)
        {
            try
            {
                var debitCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_DEBIT).ToList();
                var obhCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_CHARGE_OBH).ToList();
                var partner = partnerRepo.Get(x => x.AccountNo == model.PartnerCode).FirstOrDefault();
                if (partner == null)
                {
                    string message = string.Format(@"Không tìm thấy partner {0}. Vui lòng kiểm tra lại Partner Code!", model.PartnerCode);
                    return new HandleState((object)message);
                }

                var invoiceDebit = ModelInvoiceDebit(model, partner, debitCharges, _currentUser);
                var invoiceObh = obhCharges.Count > 0 ? ModelInvoiceObh(model, partner, obhCharges, _currentUser) : null;
                
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hsDebit = DataContext.Add(invoiceDebit, false);
                        HandleState hsObh = new HandleState();
                        if (invoiceObh != null)
                        {
                            hsObh = DataContext.Add(invoiceObh, false);
                        }
                        if (hsDebit.Success && hsObh.Success)
                        {
                            foreach (var debitCharge in debitCharges)
                            {
                                CsShipmentSurcharge surchargeDebit = surchargeRepo.Get(x => x.Id == debitCharge.ChargeId).FirstOrDefault();
                                surchargeDebit.AcctManagementId = invoiceDebit.Id;
                                surchargeDebit.InvoiceNo = invoiceDebit.InvoiceNoReal;
                                surchargeDebit.InvoiceDate = invoiceDebit.Date;
                                surchargeDebit.VoucherId = invoiceDebit.VoucherId;
                                surchargeDebit.VoucherIddate = invoiceDebit.Date;
                                surchargeDebit.SeriesNo = invoiceDebit.Serie;
                                surchargeDebit.FinalExchangeRate = CalculatorExchangeRate(debitCharge.ExchangeRate, surchargeDebit.ExchangeDate, surchargeDebit.CurrencyId, invoiceDebit.Currency); //debitCharge.ExchangeRate;
                                surchargeDebit.ReferenceNo = debitCharge.ReferenceNo;
                                surchargeDebit.DatetimeModified = DateTime.Now;
                                surchargeDebit.UserModified = _currentUser.UserID;
                                var updateSurchargeDebit = surchargeRepo.Update(surchargeDebit, x => x.Id == surchargeDebit.Id, false);
                            }

                            //Cập nhật số RefNo cho phí OBH
                            foreach (var obhCharge in obhCharges)
                            {
                                CsShipmentSurcharge surchargeObh = surchargeRepo.Get(x => x.Id == obhCharge.ChargeId).FirstOrDefault();
                                surchargeObh.AcctManagementId = invoiceObh.Id;
                                surchargeObh.InvoiceNo = invoiceObh.InvoiceNoReal;
                                surchargeObh.InvoiceDate = invoiceObh.Date;
                                surchargeObh.VoucherId = invoiceObh.VoucherId;
                                surchargeObh.VoucherIddate = invoiceObh.Date;
                                surchargeObh.SeriesNo = invoiceObh.Serie;
                                surchargeObh.FinalExchangeRate = CalculatorExchangeRate(obhCharge.ExchangeRate, surchargeObh.ExchangeDate, surchargeObh.CurrencyId, invoiceObh.Currency); //obhCharge.ExchangeRate;
                                surchargeObh.ReferenceNo = obhCharge.ReferenceNo;
                                surchargeObh.DatetimeModified = DateTime.Now;
                                surchargeObh.UserModified = _currentUser.UserID;
                                var updateSurchargeObh = surchargeRepo.Update(surchargeObh, x => x.Id == surchargeObh.Id, false);
                            }

                            var smSurcharge = surchargeRepo.SubmitChanges();
                            var sm = DataContext.SubmitChanges();
                            trans.Commit();
                        }
                        if (!hsDebit.Success)
                        {
                            return hsDebit;
                        }
                        if (!hsObh.Success)
                        {
                            return hsObh;
                        }
                        return hsDebit;
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

        private AccAccountingManagement ModelInvoiceDebit(InvoiceCreateInfo model, CatPartner partner, List<ChargeInvoice> debitCharges, ICurrentUser _currentUser)
        {
            AccAccountingManagement invoice = new AccAccountingManagement();
            invoice.Id = Guid.NewGuid();
            invoice.PartnerId = partner?.Id;
            invoice.InvoiceNoReal = invoice.InvoiceNoTempt = model.InvoiceNo;
            invoice.Date = model.InvoiceDate;
            invoice.Serie = model.SerieNo;
            invoice.Status = ForPartnerConstants.ACCOUNTING_INVOICE_STATUS_UPDATED; //Set default "Updated Invoice"
            invoice.PaymentStatus = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID; //Set default "Unpaid"
            invoice.Type = ForPartnerConstants.ACCOUNTING_INVOICE_TYPE; //Type is Invoice
            invoice.VoucherId = GenerateVoucherId(invoice.Type, string.Empty); //Auto Gen VoucherId
            invoice.ReferenceNo = debitCharges[0].ReferenceNo; //Cập nhật Reference No cho Invoice
            invoice.Currency = model.Currency; //Currency of Invoice
            invoice.TotalAmount = invoice.UnpaidAmount = CalculatorTotalAmount(debitCharges, model.Currency); // Calculator Total Amount
            invoice.UserCreated = invoice.UserModified = _currentUser.UserID;
            invoice.DatetimeCreated = invoice.DatetimeModified = DateTime.Now;
            invoice.GroupId = _currentUser.GroupId;
            invoice.DepartmentId = _currentUser.DepartmentId;

            var firstCharge = surchargeRepo.Get(x => x.Id == debitCharges[0].ChargeId).Select(s => new { s.OfficeId, s.CompanyId }).FirstOrDefault();
            invoice.OfficeId = firstCharge?.OfficeId ?? Guid.Empty; //Lấy OfficeId của first charge
            invoice.CompanyId = firstCharge?.CompanyId ?? Guid.Empty; //Lấy CompanyId của first charge

            invoice.PaymentTerm = debitCharges[0].PaymentTerm;
            invoice.PaymentMethod = ForPartnerConstants.PAYMENT_METHOD_BANK_OR_CASH; //Set default "Bank Transfer / Cash"
            invoice.AccountNo = SetAccountNoForInvoice(partner?.PartnerMode, model.Currency);
            invoice.Description = model.Description;
            return invoice;
        }

        private AccAccountingManagement ModelInvoiceObh(InvoiceCreateInfo model, CatPartner partner, List<ChargeInvoice> obhCharges, ICurrentUser _currentUser)
        {
            var _referenceNo = obhCharges[0].ReferenceNo;
            AccAccountingManagement invoice = new AccAccountingManagement();
            invoice.Id = Guid.NewGuid();
            invoice.PartnerId = partner?.Id;
            invoice.InvoiceNoReal = invoice.InvoiceNoTempt = _referenceNo; //Lấy số referenceNo
            invoice.Date = model.InvoiceDate;
            invoice.Serie = "OBH/" + model.InvoiceDate.Value.ToString("yy");
            invoice.Status = ForPartnerConstants.ACCOUNTING_INVOICE_STATUS_UPDATED; //Set default "Updated Invoice"
            invoice.PaymentStatus = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID; //Set default "Unpaid"
            invoice.Type = ForPartnerConstants.ACCOUNTING_INVOICE_TEMP_TYPE; //Type is InvoiceTemp (Hóa đơn tạm)
            invoice.VoucherId = _referenceNo; //Lấy số referenceNo
            invoice.ReferenceNo = _referenceNo; //Cập nhật Reference No cho Invoice
            invoice.Currency = model.Currency; //Currency of Invoice
            invoice.TotalAmount = invoice.UnpaidAmount = CalculatorTotalAmount(obhCharges, model.Currency); // Calculator Total Amount
            invoice.UserCreated = invoice.UserModified = _currentUser.UserID;
            invoice.DatetimeCreated = invoice.DatetimeModified = DateTime.Now;
            invoice.GroupId = _currentUser.GroupId;
            invoice.DepartmentId = _currentUser.DepartmentId;

            var firstCharge = surchargeRepo.Get(x => x.Id == obhCharges[0].ChargeId).Select(s => new { s.OfficeId, s.CompanyId }).FirstOrDefault();
            invoice.OfficeId = firstCharge?.OfficeId ?? Guid.Empty; //Lấy OfficeId của first charge
            invoice.CompanyId = firstCharge?.CompanyId ?? Guid.Empty; //Lấy CompanyId của first charge

            invoice.PaymentTerm = obhCharges[0].PaymentTerm;
            invoice.PaymentMethod = ForPartnerConstants.PAYMENT_METHOD_BANK_OR_CASH; //Set default "Bank Transfer / Cash"
            invoice.AccountNo = string.Empty; //Để trống
            invoice.Description = model.Description;
            return invoice;
        }

        public HandleState UpdateInvoice(InvoiceUpdateInfo model, string apiKey)
        {
            return new HandleState();
        }

        public HandleState DeleteInvoice(InvoiceInfo model, string apiKey, string funcController)
        {
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;

            var hsDeleteInvoice = DeleteInvoice(model, currentUser);
            
            #region -- Ghi Log --
            var modelLog = new SysActionFuncLogModel
            {
                FuncLocal = string.Format(@"DeleteInvoice ({0})", funcController),
                ObjectRequest = JsonConvert.SerializeObject(model),
                ObjectResponse = JsonConvert.SerializeObject(hsDeleteInvoice),
                Major = "Xóa Hóa Đơn"
            };
            var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
            #endregion

            return hsDeleteInvoice;
        }

        HandleState DeleteInvoice(InvoiceInfo model, ICurrentUser _currentUser)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var data = DataContext.Get(x => x.ReferenceNo == model.ReferenceNo
                                                 && x.InvoiceNoReal == model.InvoiceNo
                                                 && x.Serie == model.SerieNo
                                                 && x.Type == ForPartnerConstants.ACCOUNTING_INVOICE_TYPE).FirstOrDefault();
                    if (data == null) return new HandleState((object)"Không tìm thấy hóa đơn");

                    HandleState hs = DataContext.Delete(x => x.ReferenceNo == model.ReferenceNo
                                                          && x.InvoiceNoReal == model.InvoiceNo
                                                          && x.Serie == model.SerieNo
                                                          && x.Type == ForPartnerConstants.ACCOUNTING_INVOICE_TYPE, false);
                    if (hs.Success)
                    {
                        var charges = surchargeRepo.Get(x => x.ReferenceNo == model.ReferenceNo);
                        foreach (var charge in charges)
                        {
                            charge.AcctManagementId = null;
                            charge.ReferenceNo = null;
                            charge.InvoiceNo = null;
                            charge.InvoiceDate = null;
                            charge.VoucherId = null;
                            charge.VoucherIddate = null;
                            charge.SeriesNo = null;
                            charge.FinalExchangeRate = null;
                            charge.AmountVnd = charge.VatAmountVnd = null;
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = _currentUser.UserID;
                            var updateSur = surchargeRepo.Update(charge, x => x.Id == charge.Id, false);

                            UpdateStatusRemovedInvForSOA(charge.Soano);
                            UpdateStatusRemovedInvForDebitNote(charge.DebitNo);
                        }

                        var smSoa = acctSOARepository.SubmitChanges();
                        var smDebitNote = acctCdNoteRepo.SubmitChanges();
                        var smSur = surchargeRepo.SubmitChanges();
                        var sm = DataContext.SubmitChanges();
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
        
        private HandleState UpdateStatusRemovedInvForSOA(string soaNo)
        {
            var hsUpdate = new HandleState();
            var soa = acctSOARepository.Get(x => x.Soano == soaNo && x.SyncStatus == ForPartnerConstants.STATUS_SYNCED).FirstOrDefault();
            if (soa != null)
            {
                soa.SyncStatus = ForPartnerConstants.STATUS_REMOVED_INV;
                soa.UserModified = currentUser.UserID;
                soa.DatetimeModified = DateTime.Now;
                hsUpdate = acctSOARepository.Update(soa, x => x.Id == soa.Id, false);
            }
            return hsUpdate;
        }

        private HandleState UpdateStatusRemovedInvForDebitNote(string debitNo)
        {
            var hsUpdate = new HandleState();
            var debitNote = acctCdNoteRepo.Get(x => x.Code == debitNo && x.SyncStatus == ForPartnerConstants.STATUS_SYNCED).FirstOrDefault();
            if (debitNote != null)
            {
                debitNote.SyncStatus = ForPartnerConstants.STATUS_REMOVED_INV;
                debitNote.UserModified = currentUser.UserID;
                debitNote.DatetimeModified = DateTime.Now;
                hsUpdate = acctCdNoteRepo.Update(debitNote, x => x.Id == debitNote.Id, false);
            }
            return hsUpdate;
        }

        #endregion --- CRUD INVOICE ---

        #region --- PRIVATE METHOD ---
        private SysPartnerApi GetInfoPartnerByApiKey(string apiKey)
        {
            SysPartnerApi partnerApi = sysPartnerApiRepository.Get(x => x.ApiKey == apiKey).FirstOrDefault();
            return partnerApi;
        }

        private ICurrentUser SetCurrentUserPartner(ICurrentUser currentUser, string apiKey)
        {
            SysPartnerApi partnerApi = GetInfoPartnerByApiKey(apiKey);
            currentUser.UserID = (partnerApi != null) ? partnerApi.UserId.ToString() : Guid.Empty.ToString();
            currentUser.GroupId = 0;
            currentUser.DepartmentId = 0;
            currentUser.OfficeID = Guid.Empty;
            currentUser.CompanyID = partnerApi?.CompanyId ?? Guid.Empty;

            return currentUser;
        }

        private string GenerateVoucherId(string acctMngtType, string voucherType)
        {
            if (string.IsNullOrEmpty(acctMngtType)) return string.Empty;
            int monthCurrent = DateTime.Now.Month;
            string year = DateTime.Now.Year.ToString();
            string month = monthCurrent.ToString().PadLeft(2, '0');//Nếu tháng < 10 thì gắn thêm số 0 phía trước, VD: 09
            string no = "001";

            IQueryable<string> voucherNewests = null;
            string _prefixVoucher = string.Empty;
            if (acctMngtType == ForPartnerConstants.ACCOUNTING_INVOICE_TYPE)
            {
                _prefixVoucher = "FDT";
            }
            else if (acctMngtType == ForPartnerConstants.ACCOUNTING_VOUCHER_TYPE)
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

        private decimal? CalculatorTotalAmount(List<ChargeInvoice> charges, string currencyInvoice)
        {
            decimal? total = 0;
            if (!string.IsNullOrEmpty(currencyInvoice))
            {
                charges.ForEach(fe =>
                {
                    var surcharge = surchargeRepo.Get(x => x.Id == fe.ChargeId).FirstOrDefault();
                    //Tỷ giá của Currency Charge so với Local
                    var _exchangeForLocal = CalculatorExchangeRate(fe.ExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, currencyInvoice);
                    decimal? exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(_exchangeForLocal, surcharge.ExchangeDate, surcharge.CurrencyId, currencyInvoice);
                    total += exchangeRate * surcharge.Total;
                });
            }
            return total;
        }
        
        private string SetAccountNoForInvoice(string partnerMode, string currencyInvoice)
        {
            if (string.IsNullOrEmpty(partnerMode))
            {
                partnerMode = ForPartnerConstants.PARTNER_MODE_INTERNAL;
            }

            string accountNo = string.Empty;
            if (partnerMode == ForPartnerConstants.PARTNER_MODE_INTERNAL && currencyInvoice == ForPartnerConstants.CURRENCY_LOCAL)
            {
                accountNo = "1313";
            }
            else if (partnerMode == ForPartnerConstants.PARTNER_MODE_EXTERNAL && currencyInvoice == ForPartnerConstants.CURRENCY_LOCAL)
            {
                accountNo = "13111";
            }
            else if (partnerMode == ForPartnerConstants.PARTNER_MODE_INTERNAL && currencyInvoice == ForPartnerConstants.CURRENCY_USD)
            {
                accountNo = "13122";
            }
            else if (partnerMode == ForPartnerConstants.PARTNER_MODE_EXTERNAL && currencyInvoice == ForPartnerConstants.CURRENCY_USD)
            {
                accountNo = "1314";
            }
            return accountNo;
        }
        
        private decimal? CalculatorExchangeRate(decimal? exchangeRateNew, DateTime? exchangeDate, string currencyFrom, string currencyTo)
        {
            if (currencyFrom == currencyTo) return exchangeRateNew;

            var currencyExchanges = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == exchangeDate.Value.Date).ToList();
            var exchangeFrom = currencyFrom != ForPartnerConstants.CURRENCY_LOCAL ? currencyExchanges.Where(x => x.CurrencyFromId == currencyFrom).FirstOrDefault()?.Rate : 1; //Lấy tỉ giá currencyFrom so với VND
            var exchangeTo = currencyTo != ForPartnerConstants.CURRENCY_LOCAL ? currencyExchanges.Where(x => x.CurrencyFromId == currencyTo).FirstOrDefault()?.Rate : 1; //Lấy tỉ giá currencyTo so với VND
            var _rateFromCFToCT = exchangeFrom / exchangeTo; //Tỷ giá currencyFrom so với currencyTo

            var _exchangeRate = _rateFromCFToCT * exchangeRateNew;
            return _exchangeRate;
        }
        
        #endregion --- PRIVATE METHOD ---

        #region --- Advance ---
        public HandleState RemoveVoucherAdvance(string voucherNo, string apiKey)
        {
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;

            var hsRemoveVoucherAdvance = RemoveVoucherAdvance(voucherNo, currentUser);
            
            #region -- Ghi Log --
            var modelLog = new SysActionFuncLogModel
            {
                FuncLocal = "RemoveVoucherAdvance",
                ObjectRequest = JsonConvert.SerializeObject(voucherNo),
                ObjectResponse = JsonConvert.SerializeObject(hsRemoveVoucherAdvance),
                Major = "Hủy Phiếu Chi"
            };
            var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
            #endregion

            return hsRemoveVoucherAdvance;
        }

        private HandleState RemoveVoucherAdvance(string voucherNo, ICurrentUser _currentUser)
        {
            HandleState result = new HandleState();
            try
            {
                if (string.IsNullOrEmpty(voucherNo))
                {
                    return new HandleState(LanguageSub.MSG_DATA_NOT_FOUND);
                }
                AcctAdvancePayment adv = acctAdvanceRepository.Get(x => x.VoucherNo == voucherNo)?.FirstOrDefault();
                if (adv == null)
                {
                    return new HandleState(LanguageSub.MSG_DATA_NOT_FOUND);
                }

                adv.VoucherNo = null;
                adv.VoucherDate = null;
                adv.PaymentTerm = null;

                adv.UserModified = _currentUser.UserID;
                adv.DatetimeModified = DateTime.Now;

                result = acctAdvanceRepository.Update(adv, x => x.Id == adv.Id);

                if (!result.Success)
                {
                    return new HandleState("Error");
                }

                return result;

            }
            catch (Exception ex)
            {
                return new HandleState("Error");
            }
        }

        public HandleState UpdateVoucherAdvance(VoucherAdvance model, string apiKey)
        {            
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;

            var hsUpdateVoucherAdvance = UpdateVoucherAdvance(model, currentUser);

            #region -- Ghi Log --
            var modelLog = new SysActionFuncLogModel
            {
                FuncLocal = "UpdateVoucherAdvance",
                ObjectRequest = JsonConvert.SerializeObject(model),
                ObjectResponse = JsonConvert.SerializeObject(hsUpdateVoucherAdvance),
                Major = "Cập nhật thông tin Advance  (Phiếu chi)"
            };
            var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
            #endregion

            return hsUpdateVoucherAdvance;
        }

        private HandleState UpdateVoucherAdvance(VoucherAdvance model, ICurrentUser _currentUser)
        {
            HandleState result = new HandleState();
            try
            {
                AcctAdvancePayment adv = acctAdvanceRepository.Get(x => x.Id == model.AdvanceID)?.FirstOrDefault();
                if (adv == null)
                {
                    return new HandleState("Not found advance  " + model.AdvanceNo);
                }

                if (adv.StatusApproval == ForPartnerConstants.STATUS_APPROVAL_DONE)
                {
                    adv.PaymentTerm = model.PaymentTerm ?? 7; // Mặc định thời hạn thanh toán cho phiếu tạm ứng là 7 ngày
                    if (model.PaymentTerm != null)
                    {
                        DateTime? deadlineDate = null;
                        deadlineDate = adv.DeadlinePayment.Value.AddDays((double)model.PaymentTerm);
                        adv.DeadlinePayment = deadlineDate;
                    }
                    adv.VoucherNo = model.VoucherNo;
                    adv.VoucherDate = model.VoucherDate;
                    adv.UserModified = _currentUser.UserID;
                    adv.DatetimeModified = DateTime.Now;

                    result = acctAdvanceRepository.Update(adv, x => x.Id == adv.Id);

                    if (!result.Success)
                    {
                        return new HandleState("Update fail");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        #endregion ---Advance ---

        #region --- REJECT & REMOVE DATA ---
        public HandleState RejectData(RejectData model, string apiKey)
        {
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;

            var result = new HandleState();
            switch (model.Type?.ToUpper())
            {
                case "ADVANCE":
                    result = RejectAdvance(model.ReferenceID, model.Reason);
                    break;
                case "SETTLEMENT":
                    result = RejectSettlement(model.ReferenceID, model.Reason);
                    break;
                case "SOA":
                    result = RejectSoa(model.ReferenceID, model.Reason);
                    break;
                case "CDNOTE":
                    result = RejectCdNote(model.ReferenceID, model.Reason);
                    break;
                case "VOUCHER":
                    result = RejectVoucher(model.ReferenceID, model.Reason);
                    break;
                case "PAYMENT":
                    result = RejectPayment(model.ReferenceID, model.Reason);
                    break;
                default:
                    result = new HandleState((object)"Không tìm thấy loại reject");                    
                    break ;
            }

            #region -- Ghi Log --
            var modelLog = new SysActionFuncLogModel
            {
                FuncLocal = "RejectData",
                ObjectRequest = JsonConvert.SerializeObject(model),
                ObjectResponse = JsonConvert.SerializeObject(result),
                Major = "Reject Data " + model.Type?.ToUpper()
            };
            var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
            #endregion

            return result;
        }

        private HandleState RejectAdvance(string id, string reason)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var _id = Guid.Empty;
                    Guid.TryParse(id, out _id);
                    var advance = acctAdvanceRepository.Get(x => x.Id == _id).FirstOrDefault();
                    if (advance == null) return new HandleState((object)"Không tìm thấy advance");
                    advance.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
                    advance.UserModified = currentUser.UserID;
                    advance.DatetimeModified = DateTime.Now;
                    advance.ReasonReject = reason;
                    HandleState hs = acctAdvanceRepository.Update(advance, x => x.Id == advance.Id, false);
                    if (hs.Success)
                    {
                        var smAdvance = acctAdvanceRepository.SubmitChanges();
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

        private HandleState RejectSettlement(string id, string reason)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var _id = Guid.Empty;
                    Guid.TryParse(id, out _id);
                    var settlement = acctSettlementRepo.Get(x => x.Id == _id).FirstOrDefault();
                    if (settlement == null) return new HandleState((object)"Không tìm thấy settlement");
                    settlement.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
                    settlement.UserModified = currentUser.UserID;
                    settlement.DatetimeModified = DateTime.Now;
                    settlement.ReasonReject = reason;
                    HandleState hs = acctSettlementRepo.Update(settlement, x => x.Id == settlement.Id, false);
                    if (hs.Success)
                    {
                        var smSettlement = acctSettlementRepo.SubmitChanges();
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

        private HandleState RejectSoa(string id, string reason)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var _id = 0;
                    int.TryParse(id, out _id);
                    var soa = acctSOARepository.Get(x => x.Id == _id).FirstOrDefault();
                    if (soa == null) return new HandleState((object)"Không tìm thấy SOA");
                    soa.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
                    soa.UserModified = currentUser.UserID;
                    soa.DatetimeModified = DateTime.Now;
                    soa.ReasonReject = reason;
                    HandleState hs = acctSOARepository.Update(soa, x => x.Id == soa.Id, false);
                    if (hs.Success)
                    {
                        var smSoa = acctSOARepository.SubmitChanges();
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

        private HandleState RejectCdNote(string id, string reason)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var _id = Guid.Empty;
                    Guid.TryParse(id, out _id);
                    var cdNote = acctCdNoteRepo.Get(x => x.Id == _id).FirstOrDefault();
                    if (cdNote == null) return new HandleState((object)"Không tìm thấy CDNote");
                    cdNote.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
                    cdNote.UserModified = currentUser.UserID;
                    cdNote.DatetimeModified = DateTime.Now;
                    cdNote.ReasonReject = reason;
                    HandleState hs = acctCdNoteRepo.Update(cdNote, x => x.Id == cdNote.Id, false);
                    if (hs.Success)
                    {
                        var smCdNote = acctCdNoteRepo.SubmitChanges();
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

        private HandleState RejectVoucher(string id, string reason)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var _id = Guid.Empty;
                    Guid.TryParse(id, out _id);
                    var voucher = DataContext.Get(x => x.Id == _id).FirstOrDefault();
                    if (voucher == null) return new HandleState((object)"Không tìm thấy voucher");
                    voucher.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
                    voucher.UserModified = currentUser.UserID;
                    voucher.DatetimeModified = DateTime.Now;
                    voucher.ReasonReject = reason;
                    HandleState hs = DataContext.Update(voucher, x => x.Id == voucher.Id, false);
                    if (hs.Success)
                    {
                        var sm = DataContext.SubmitChanges();
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
        
        private HandleState RejectPayment(string id, string reason)
        {
            var _idSoa = 0;
            int.TryParse(id, out _idSoa);
            if (_idSoa != 0)
            {
                var soa = acctSOARepository.Get(x => x.Id == _idSoa).FirstOrDefault();
                if (soa == null) return new HandleState((object)"Không tìm thấy payment");
            }
            else
            {
                var _idInvoice = Guid.Empty;
                Guid.TryParse(id, out _idInvoice);
                var invoice = (_idInvoice != Guid.Empty) ? DataContext.Get(x => x.Id == _idInvoice).FirstOrDefault() : null;
                if (invoice == null) return new HandleState((object)"Không tìm thấy payment");
            }
            return new HandleState();
        }

        /// <summary>
        /// Type là VOUCHER => eFMS sẽ xóa mã VOUCHER tương ứng
        /// Type là CDNOTE/SOA/SETTLEMENT => Reset trạng thái "Reject" Cho từng phiều tương ứng
        /// </summary>
        /// <param name="model"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public HandleState RemoveVoucher(RejectData model, string apiKey)
        {
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;

            var result = new HandleState();
            switch (model.Type?.ToUpper())
            {
                case "SETTLEMENT":
                    result = RejectSettlement(model.ReferenceID, model.Reason);
                    break;
                case "SOA":
                    result = RejectSoa(model.ReferenceID, model.Reason);
                    break;
                case "CDNOTE":
                    result = RejectCdNote(model.ReferenceID, model.Reason);
                    break;
                case "VOUCHER":
                    result = DeleteVoucher(model.ReferenceID);
                    break;
                default:
                    result = new HandleState((object)"Không tìm thấy loại remove voucher");
                    break;
            }

            #region -- Ghi Log --
            var modelLog = new SysActionFuncLogModel
            {
                FuncLocal = "RemoveVoucher",
                ObjectRequest = JsonConvert.SerializeObject(model),
                ObjectResponse = JsonConvert.SerializeObject(result),
                Major = "Remove Voucher " + model.Type?.ToUpper()
            };
            var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
            #endregion

            return result;
        }

        private HandleState DeleteVoucher(string id)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var _id = Guid.Empty;
                    Guid.TryParse(id, out _id);
                    var voucher = DataContext.Get(x => x.Id == _id).FirstOrDefault();
                    if (voucher == null) return new HandleState((object)"Không tìm thấy voucher");
                    HandleState hs = DataContext.Delete(x => x.Id == voucher.Id, false);
                    if (hs.Success)
                    {
                        var charges = surchargeRepo.Get(x => x.AcctManagementId == voucher.Id);
                        foreach (CsShipmentSurcharge charge in charges)
                        {
                            charge.AcctManagementId = null;
                            if (charge.Type == ForPartnerConstants.ACCOUNTING_VOUCHER_TYPE)
                            {
                                charge.VoucherId = null;
                                charge.VoucherIddate = null;
                                charge.InvoiceNo = null;
                                charge.InvoiceDate = null;
                                charge.SeriesNo = null;
                            }

                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = currentUser.UserID;
                            surchargeRepo.Update(charge, x => x.Id == charge.Id, false);
                        }

                        // Cập nhật Settlement: VoucherNo, VoucherDate
                        if (voucher.Type == ForPartnerConstants.ACCOUNTING_VOUCHER_TYPE)
                        {
                            List<string> listSettlementCode = charges.Select(x => x.SettlementCode).Distinct().ToList();
                            if (listSettlementCode.Count() > 0)
                            {
                                foreach (string code in listSettlementCode)
                                {
                                    AcctSettlementPayment settlement = settlementPaymentRepo.Get(x => x.SettlementNo == code).FirstOrDefault();
                                    if (settlement != null)
                                    {
                                        settlement.VoucherDate = null;
                                        settlement.VoucherNo = null;
                                        settlement.UserModified = currentUser.UserID;
                                        settlement.DatetimeModified = DateTime.Now;
                                    }
                                    settlementPaymentRepo.Update(settlement, x => x.Id == settlement.Id, false);
                                }                                
                            }
                        }
                        var smSurcharge = surchargeRepo.SubmitChanges();
                        var smSettle = settlementPaymentRepo.SubmitChanges();
                        var sm = DataContext.SubmitChanges();
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
        #endregion --- REJECT & REMOVE DATA ---

    }
}


