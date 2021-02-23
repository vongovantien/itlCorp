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
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        private readonly IContextBase<AcctSettlementPayment> settlementPaymentRepo;
        private readonly IContextBase<CatCurrencyExchange> currencyExchangeRepo;
        private readonly IContextBase<SysNotifications> sysNotificationRepository;
        private readonly IContextBase<SysUserNotification> sysUserNotificationRepository;
        private readonly IContextBase<AcctReceipt> receiptRepository;

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
            IContextBase<AcctSettlementPayment> settlementPayment,
            IContextBase<CatCurrencyExchange> currencyExchange,
            IContextBase<SysNotifications> sysNotifyRepo,
            IContextBase<SysUserNotification> sysUsernotifyRepo,
            IContextBase<AcctReceipt> receiptRepo
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
            settlementPaymentRepo = settlementPayment;
            currencyExchangeRepo = currencyExchange;
            sysNotificationRepository = sysNotifyRepo;
            sysUserNotificationRepository = sysUsernotifyRepo;
            receiptRepository = receiptRepo;
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
        public HandleState InsertInvoice(InvoiceCreateInfo model, string apiKey)
        {
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;

            var hsInsertInvoice = InsertInvoice(model, currentUser);
            return hsInsertInvoice;
        }

        private HandleState InsertInvoice(InvoiceCreateInfo model, ICurrentUser _currentUser)
        {
            try
            {
                var debitCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_DEBIT).ToList();
                var obhCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_CHARGE_OBH).ToList();

                var chargeNotExistsInSurcharge = GetChargeNotExistsInSurcharge(model.Charges);
                if (!string.IsNullOrEmpty(chargeNotExistsInSurcharge))
                {
                    string message = string.Format(@"ChargeId: {0} không tồn tại. Vui lòng kiểm tra lại", chargeNotExistsInSurcharge);
                    return new HandleState((object)message);
                }

                var chargeObhNotMatchCurrency = GetChargeNotMatchCurrencyInSurcharge(obhCharges);
                if (!string.IsNullOrEmpty(chargeObhNotMatchCurrency))
                {
                    string message = string.Format(@"ChargeId: {0} có currency không hợp lệ. Vui lòng kiểm tra lại", chargeObhNotMatchCurrency);
                    return new HandleState((object)message);
                }

                var matchedChargeDebit = CheckMatchedCharge(debitCharges);
                if (matchedChargeDebit)
                {
                    string message = string.Format(@"Charge type DEBIT không có cùng số ReferenceNo hoặc Currency. Vui lòng kiểm tra lại");
                    return new HandleState((object)message);
                }
                
                var partner = partnerRepo.Get(x => x.AccountNo == model.PartnerCode).FirstOrDefault();
                if (partner == null)
                {
                    string message = string.Format(@"Không tìm thấy partner {0}. Vui lòng kiểm tra lại Partner Code", model.PartnerCode);
                    return new HandleState((object)message);
                }

                if (debitCharges.Count > 0)
                {
                    var invoiceDebitByRefNo = DataContext.Get(x => x.ReferenceNo == debitCharges[0].ReferenceNo).FirstOrDefault();
                    if (invoiceDebitByRefNo != null)
                    {
                        string message = string.Format("Số Reference No {0} đã tồn tại trong hóa đơn {1}. Vui lòng kiểm tra lại", debitCharges[0].ReferenceNo, invoiceDebitByRefNo.InvoiceNoReal);
                        return new HandleState((object)message);
                    }
                }
                
                var invoiceDebit = debitCharges.Count > 0 ? ModelInvoiceDebit(model, partner, debitCharges, _currentUser) : null;
                var invoicesObh = obhCharges.Count > 0 ? ModelInvoicesObh(model, partner, obhCharges, _currentUser) : null;

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hsDebit = new HandleState();
                        if (invoiceDebit != null)
                        {
                            //Insert Invoice (Invoice Debit)
                            hsDebit = DataContext.Add(invoiceDebit, false);
                        }
                        HandleState hsObh = new HandleState();
                        if (invoicesObh != null)
                        {
                            //Insert list Invoice Temp (Invoice OBH)
                            foreach (var invoiceObh in invoicesObh)
                            {
                                hsObh = DataContext.Add(invoiceObh, false);
                            }
                        }

                        if (hsDebit.Success)
                        {
                            if (invoiceDebit != null)
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

                                    #region -- Tính lại giá trị các field  dựa vào FinalExchangeRate mới: NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                                    var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(surchargeDebit);
                                    surchargeDebit.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                                    surchargeDebit.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                                    surchargeDebit.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                                    surchargeDebit.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                                    surchargeDebit.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                                    surchargeDebit.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                                    #endregion -- Tính lại giá trị các field  dựa vào FinalExchangeRate mới: NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

                                    surchargeDebit.ReferenceNo = debitCharge.ReferenceNo;
                                    surchargeDebit.DatetimeModified = DateTime.Now;
                                    surchargeDebit.UserModified = _currentUser.UserID;
                                    var updateSurchargeDebit = surchargeRepo.Update(surchargeDebit, x => x.Id == surchargeDebit.Id, false);
                                }
                            }
                        }

                        if (hsObh.Success)
                        {
                            if (invoicesObh != null)
                            {
                                foreach (var invoiceObh in invoicesObh)
                                {
                                    //Cập nhật số RefNo cho phí OBH
                                    foreach (var obhCharge in obhCharges)
                                    {
                                        CsShipmentSurcharge surchargeObh = surchargeRepo.Get(x => x.Id == obhCharge.ChargeId).FirstOrDefault();
                                        surchargeObh.AcctManagementId = invoiceObh.Id;
                                        surchargeObh.InvoiceNo = null; //CR: 07/12/2020
                                        surchargeObh.InvoiceDate = null; //CR: 07/12/2020
                                        surchargeObh.VoucherId = invoiceObh.VoucherId;
                                        surchargeObh.VoucherIddate = invoiceObh.Date;
                                        surchargeObh.SeriesNo = null; //CR: 07/12/2020
                                        surchargeObh.FinalExchangeRate = obhCharge.ExchangeRate; //Lấy exchangeRate từ Bravo trả về

                                        #region -- Tính lại giá trị các field dựa vào FinalExchangeRate mới: NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                                        var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(surchargeObh);
                                        surchargeObh.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                                        surchargeObh.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                                        surchargeObh.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                                        surchargeObh.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                                        surchargeObh.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                                        surchargeObh.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                                        #endregion -- Tính lại giá trị các field  dựa vào FinalExchangeRate mới: NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

                                        surchargeObh.ReferenceNo = obhCharge.ReferenceNo;
                                        surchargeObh.DatetimeModified = DateTime.Now;
                                        surchargeObh.UserModified = _currentUser.UserID;
                                        var updateSurchargeObh = surchargeRepo.Update(surchargeObh, x => x.Id == surchargeObh.Id, false);
                                    }
                                }
                            }
                        }

                        var smSurcharge = surchargeRepo.SubmitChanges();
                        DataContext.SubmitChanges();
                        trans.Commit();

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
                        string logErr = string.Format("Lỗi InsertInvoice {0} \n Message: {1} \n invoiceDebit: {2} \n debitCharges: {3} \n obhCharges: {4}", 
                            currentUser.UserID, 
                            ex.ToString(),
                            JsonConvert.SerializeObject(invoiceDebit),
                            JsonConvert.SerializeObject(invoicesObh),
                            JsonConvert.SerializeObject(debitCharges),
                            JsonConvert.SerializeObject(obhCharges));
                        new LogHelper("InsertInvoice", logErr);
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

        /// <summary>
        /// Get model Invoice Debit
        /// </summary>
        /// <param name="model"></param>
        /// <param name="partner"></param>
        /// <param name="debitCharges"></param>
        /// <param name="_currentUser"></param>
        /// <returns></returns>
        private AccAccountingManagement ModelInvoiceDebit(InvoiceCreateInfo model, CatPartner partner, List<ChargeInvoice> debitCharges, ICurrentUser _currentUser)
        {
            var debitChargeFirst = debitCharges[0];
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
            invoice.ReferenceNo = debitChargeFirst?.ReferenceNo; //Cập nhật Reference No cho Invoice
            invoice.Currency = debitChargeFirst?.Currency ?? model.Currency; //Currency of Invoice
            invoice.TotalAmount = invoice.UnpaidAmount = CalculatorTotalAmount(debitCharges, invoice.Currency); // Calculator Total Amount
            invoice.UserCreated = invoice.UserModified = _currentUser.UserID;
            invoice.DatetimeCreated = invoice.DatetimeModified = DateTime.Now;
            invoice.GroupId = _currentUser.GroupId;
            invoice.DepartmentId = _currentUser.DepartmentId;

            var firstCharge = surchargeRepo.Get(x => x.Id == debitChargeFirst.ChargeId).Select(s => new { s.OfficeId, s.CompanyId }).FirstOrDefault();
            invoice.OfficeId = firstCharge?.OfficeId ?? Guid.Empty; //Lấy OfficeId của first charge
            invoice.CompanyId = firstCharge?.CompanyId ?? Guid.Empty; //Lấy CompanyId của first charge

            invoice.PaymentTerm = debitChargeFirst?.PaymentTerm;
            invoice.PaymentMethod = ForPartnerConstants.PAYMENT_METHOD_BANK_OR_CASH; //Set default "Bank Transfer / Cash"
            invoice.AccountNo = !string.IsNullOrEmpty(debitChargeFirst.AccountNo) ? debitChargeFirst.AccountNo : SetAccountNoForInvoice(partner?.PartnerMode, model.Currency);
            invoice.Description = model.Description;
            var _serviceTypeCharge = surchargeRepo.Get(x => debitCharges.Select(se => se.ChargeId).Contains(x.Id)).Select(s => s.TransactionType).Distinct().ToList();
            invoice.ServiceType = string.Format(";", _serviceTypeCharge);
            return invoice;
        }

        /// <summary>
        /// Get list model Invoice OBH (Invoice Temp)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="partner"></param>
        /// <param name="obhCharges"></param>
        /// <param name="_currentUser"></param>
        /// <returns></returns>
        private List<AccAccountingManagement> ModelInvoicesObh(InvoiceCreateInfo model, CatPartner partner, List<ChargeInvoice> obhCharges, ICurrentUser _currentUser)
        {
            List<AccAccountingManagement> invoices = new List<AccAccountingManagement>();
            var grpObhCharges = obhCharges.GroupBy(x => x.ReferenceNo).Select(se => new { ReferenceNo = se.Key, Charges = se.ToList() });
            foreach (var grpObhCharge in grpObhCharges)
            {
                var _obhCharges = grpObhCharge.Charges;
                var obhChargeFirst = grpObhCharge.Charges[0];
                AccAccountingManagement invoice = new AccAccountingManagement();
                invoice.Id = Guid.NewGuid();
                invoice.PartnerId = partner?.Id;
                invoice.InvoiceNoReal = invoice.InvoiceNoTempt = grpObhCharge.ReferenceNo; //Lấy số referenceNo
                invoice.Date = model.InvoiceDate;
                invoice.Serie = "OBH/" + model.InvoiceDate.Value.ToString("yy");
                invoice.Status = ForPartnerConstants.ACCOUNTING_INVOICE_STATUS_UPDATED; //Set default "Updated Invoice"
                invoice.PaymentStatus = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID; //Set default "Unpaid"
                invoice.Type = ForPartnerConstants.ACCOUNTING_INVOICE_TEMP_TYPE; //Type is InvoiceTemp (Hóa đơn tạm)
                invoice.VoucherId = grpObhCharge?.ReferenceNo; //Lấy số referenceNo
                invoice.ReferenceNo = grpObhCharge?.ReferenceNo; //Cập nhật Reference No cho Invoice
                invoice.Currency = obhChargeFirst?.Currency ?? model.Currency; //Currency of Invoice
                invoice.TotalAmount = invoice.UnpaidAmount = CalculatorTotalAmount(_obhCharges, invoice.Currency); // Calculator Total Amount
                invoice.UserCreated = invoice.UserModified = _currentUser.UserID;
                invoice.DatetimeCreated = invoice.DatetimeModified = DateTime.Now;
                invoice.GroupId = _currentUser.GroupId;
                invoice.DepartmentId = _currentUser.DepartmentId;

                var firstCharge = surchargeRepo.Get(x => x.Id == obhChargeFirst.ChargeId).Select(s => new { s.OfficeId, s.CompanyId }).FirstOrDefault();
                invoice.OfficeId = firstCharge?.OfficeId ?? Guid.Empty; //Lấy OfficeId của first charge
                invoice.CompanyId = firstCharge?.CompanyId ?? Guid.Empty; //Lấy CompanyId của first charge

                invoice.PaymentTerm = obhChargeFirst.PaymentTerm;
                invoice.PaymentMethod = ForPartnerConstants.PAYMENT_METHOD_BANK_OR_CASH; //Set default "Bank Transfer / Cash"
                invoice.AccountNo = obhChargeFirst.AccountNo;
                invoice.Description = model.Description;
                var _serviceTypeCharge = surchargeRepo.Get(x => _obhCharges.Select(se => se.ChargeId).Contains(x.Id)).Select(s => s.TransactionType).Distinct().ToList();
                invoice.ServiceType = string.Format(";", _serviceTypeCharge);

                invoices.Add(invoice);
            }
            return invoices;
        }

        public HandleState UpdateInvoice(InvoiceUpdateInfo model, string apiKey)
        {
            return new HandleState();
        }

        public HandleState DeleteInvoice(InvoiceInfo model, string apiKey)
        {
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;

            var hsDeleteInvoice = DeleteInvoice(model, currentUser);
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
                            //charge.FinalExchangeRate = null;
                            //charge.AmountVnd = charge.VatAmountVnd = null;
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = _currentUser.UserID;
                            charge.SyncedFrom = null;
                            var updateSur = surchargeRepo.Update(charge, x => x.Id == charge.Id, false);

                            //Update Status Removed Inv For SOA (SOA synced)
                            UpdateStatusRemovedInvForSOA(charge.Soano);
                            //Update Status Removed Inv For Debit Note (Debit Note synced)
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
                    return new HandleState((object)ex.Message);
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

        /// <summary>
        /// Lấy ds chuỗi các charge có ChargeId không tồn tại trong surcharge
        /// </summary>
        /// <param name="charges"></param>
        /// <returns></returns>
        private string GetChargeNotExistsInSurcharge(List<ChargeInvoice> charges)
        {
            string message = string.Empty;
            foreach (var charge in charges)
            {
                var surcharge = surchargeRepo.Get(x => x.Id == charge.ChargeId).FirstOrDefault();
                if (surcharge == null)
                {
                    message += (!string.IsNullOrEmpty(message) ? ", " : string.Empty) + charge.ChargeId.ToString();
                }
            }
            return message;
        }

        /// <summary>
        /// Lấy ds chuỗi các charge có currency không khớp với currency trong surcharge
        /// </summary>
        /// <param name="charges"></param>
        /// <returns></returns>
        private string GetChargeNotMatchCurrencyInSurcharge(List<ChargeInvoice> charges)
        {
            string message = string.Empty;
            foreach (var charge in charges)
            {
                var surcharge = surchargeRepo.Get(x => x.Id == charge.ChargeId).FirstOrDefault();
                if (surcharge != null)
                {
                    if (surcharge.CurrencyId != charge.Currency)
                    {
                        message += (!string.IsNullOrEmpty(message) ? ", " : string.Empty) + charge.ChargeId.ToString() + "(Currency hợp lệ: " + surcharge.CurrencyId + ")";
                    }
                }
            }
            return message;
        }

        private bool CheckMatchedCharge(List<ChargeInvoice> charges)
        {
            var isExists = false;
            if (charges.Count > 0)
            {
                var firstCharge = charges[0];
                isExists = charges.Any(x => x.Currency != firstCharge.Currency || x.ReferenceNo != firstCharge.ReferenceNo);
            }
            return isExists;
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
            int _roundDecimal = currencyInvoice == ForPartnerConstants.CURRENCY_LOCAL ? 0 : 2; //Local round 0, ngoại tệ round 2
            if (!string.IsNullOrEmpty(currencyInvoice))
            {
                charges.ForEach(fe =>
                {
                    var surcharge = surchargeRepo.Get(x => x.Id == fe.ChargeId).FirstOrDefault();
                    if (surcharge != null)
                    {
                        //Tỷ giá của Currency Charge so với Local
                        var _exchangeForLocal = CalculatorExchangeRate(fe.ExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, currencyInvoice);
                        decimal exchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(_exchangeForLocal, surcharge.ExchangeDate, surcharge.CurrencyId, currencyInvoice);

                        decimal _netAmount = NumberHelper.RoundNumber((surcharge.UnitPrice * surcharge.Quantity * exchangeRate) ?? 0, _roundDecimal);
                        decimal _vatAmount = 0;
                        if (surcharge.Vatrate != null)
                        {
                            decimal vatAmount = surcharge.Vatrate < 0 ? Math.Abs(surcharge.Vatrate ?? 0) : ((surcharge.UnitPrice * surcharge.Quantity * surcharge.Vatrate) ?? 0) / 100;
                            _vatAmount = NumberHelper.RoundNumber(vatAmount * exchangeRate, _roundDecimal);
                        }
                        total += _netAmount + _vatAmount;
                    }
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

        /// <summary>
        /// Công thức lấy tỷ giá quy đổi ngược
        /// </summary>
        /// <param name="exchangeRateNew">Tỷ giá của currencyTo so với CurrencyLocal (VD: 1 USD[CurrencyTo] = 23.000 VND[CurrencyLocal])</param>
        /// <param name="exchangeDate">Ngày Exchange của charge</param>
        /// <param name="currencyFrom">Currency gốc của charge</param>
        /// <param name="currencyTo">Currency của Invoice/</param>
        /// <returns>ExchangeRate</returns>
        private decimal? CalculatorExchangeRate(decimal? exchangeRateNew, DateTime? exchangeDate, string currencyFrom, string currencyTo)
        {
            if (currencyFrom == currencyTo) return exchangeRateNew;

            var currencyExchanges = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == exchangeDate.Value.Date).ToList();
            if (currencyExchanges.Count == 0)
            {
                DateTime? maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchanges = currencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }
            var exchangeFrom = currencyFrom != ForPartnerConstants.CURRENCY_LOCAL ? currencyExchanges.Where(x => x.CurrencyFromId == currencyFrom).FirstOrDefault()?.Rate : 1; //Lấy tỉ giá currencyFrom so với VND
            var exchangeTo = currencyTo != ForPartnerConstants.CURRENCY_LOCAL ? currencyExchanges.Where(x => x.CurrencyFromId == currencyTo).FirstOrDefault()?.Rate : 1; //Lấy tỉ giá currencyTo so với VND
            var _rateFromCFToCT = exchangeFrom / exchangeTo; //Tỷ giá currencyFrom so với currencyTo

            var _exchangeRate = _rateFromCFToCT * exchangeRateNew;
            return _exchangeRate;
        }

        private string GetLinkCdNote(string cdNoteNo, Guid jobId)
        {
            string _link = string.Empty;
            if (cdNoteNo.Contains("CL"))
            {
                _link = string.Format(@"home/operation/job-management/job-edit/{0}?tab=CDNOTE", jobId.ToString());
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
            return hsRemoveVoucherAdvance;
        }

        private HandleState RemoveVoucherAdvance(string voucherNo, ICurrentUser _currentUser)
        {
            HandleState result = new HandleState();
            try
            {
                var adv = acctAdvanceRepository.Get(x => x.VoucherNo == voucherNo).FirstOrDefault();
                if (adv == null)
                {
                    return new HandleState((object)"Không tìm thấy phiếu chi");
                }

                adv.VoucherNo = null;
                adv.VoucherDate = null;
                adv.PaymentTerm = null;

                adv.UserModified = _currentUser.UserID;
                adv.DatetimeModified = DateTime.Now;

                result = acctAdvanceRepository.Update(adv, x => x.Id == adv.Id);

                if (!result.Success)
                {
                    return new HandleState((object)"Error");
                }

                return result;

            }
            catch (Exception ex)
            {
                return new HandleState((object)"Error");
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
                    string mesg = "Not found advance  " + model.AdvanceNo;
                    return new HandleState((object)mesg);
                }

                if (adv.StatusApproval == ForPartnerConstants.STATUS_APPROVAL_DONE)
                {
                    adv.PaymentTerm = model.PaymentTerm ?? adv.PaymentTerm; // Mặc định thời hạn thanh toán cho phiếu tạm ứng là 7 ngày
                    if (model.PaymentTerm != null)
                    {
                        DateTime? deadlineDate = null;
                        deadlineDate = model.VoucherDate.HasValue ? model.VoucherDate.Value.AddDays((double)adv.PaymentTerm) : adv.RequestDate.Value.AddDays((double)model.PaymentTerm);
                        adv.DeadlinePayment = deadlineDate;
                    }
                    adv.VoucherNo = model.VoucherNo;
                    adv.VoucherDate = model.VoucherDate;
                    adv.UserModified = _currentUser.UserID;
                    adv.DatetimeModified = DateTime.Now;

                    result = acctAdvanceRepository.Update(adv, x => x.Id == adv.Id);

                    if (!result.Success)
                    {
                        return new HandleState((object)"Update fail");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new HandleState((object)ex.Message);
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
                    result = new HandleState((object)"Reject type không hợp lệ (Các type hợp lệ: ADVANCE, SETTLEMENT, SOA, CDNOTE, VOUCHER, PAYMENT)");
                    break;
            }
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
                    AcctAdvancePayment advance = acctAdvanceRepository.Get(x => x.Id == _id).FirstOrDefault();
                    if (advance == null) return new HandleState((object)"Không tìm thấy advance");

                    advance.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
                    advance.UserModified = currentUser.UserID;
                    advance.DatetimeModified = DateTime.Now;
                    advance.ReasonReject = reason;

                    HandleState hs = acctAdvanceRepository.Update(advance, x => x.Id == advance.Id, false);
                    if (hs.Success)
                    {
                        HandleState smAdvance = acctAdvanceRepository.SubmitChanges();
                        if (smAdvance.Success)
                        {
                            string title = string.Format(@"Accountant Rejected Data Advance {0}", advance.AdvanceNo);
                            SysNotifications sysNotify = new SysNotifications
                            {
                                Id = Guid.NewGuid(),
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                Type = "User",
                                Title = title,
                                IsClosed = false,
                                IsRead = false,
                                Description = reason,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                                Action = "Detail",
                                ActionLink = string.Format(@"home/accounting/advance-payment/{0}", advance.Id),
                                UserIds = advance.UserCreated
                            };
                            sysNotificationRepository.Add(sysNotify);

                            SysUserNotification sysUserNotify = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                UserId = advance.UserCreated,
                                Status = "New",
                                NotitficationId = sysNotify.Id,
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            sysUserNotificationRepository.Add(sysUserNotify);
                        }
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
                    AcctSettlementPayment settlement = acctSettlementRepo.Get(x => x.Id == _id).FirstOrDefault();
                    if (settlement == null) return new HandleState((object)"Không tìm thấy settlement");

                    settlement.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
                    settlement.UserModified = currentUser.UserID;
                    settlement.DatetimeModified = DateTime.Now;
                    settlement.ReasonReject = reason;

                    HandleState hs = acctSettlementRepo.Update(settlement, x => x.Id == settlement.Id, false);
                    if (hs.Success)
                    {
                        HandleState smSettlement = acctSettlementRepo.SubmitChanges();
                        if (smSettlement.Success)
                        {
                            string title = string.Format(@"Accountant Rejected Data Settlement {0}", settlement.SettlementNo);
                            SysNotifications sysNotify = new SysNotifications
                            {
                                Id = Guid.NewGuid(),
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                Type = "User",
                                Title = title,
                                IsClosed = false,
                                IsRead = false,
                                Description = reason,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                                Action = "Detail",
                                ActionLink = string.Format(@"home/accounting/settlement-payment/", settlement.Id),
                                UserIds = settlement.UserCreated
                            };
                            sysNotificationRepository.Add(sysNotify);

                            SysUserNotification sysUserNotify = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                UserId = settlement.UserCreated,
                                Status = "New",
                                NotitficationId = sysNotify.Id,
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            sysUserNotificationRepository.AddAsync(sysUserNotify);

                            IQueryable<CsShipmentSurcharge> surcharges = surchargeRepo.Get(x => x.SettlementCode == settlement.SettlementNo);

                            if(surcharges != null && surcharges.Count() > 0)
                            {
                                foreach (var item in surcharges)
                                {
                                    item.PaySyncedFrom = null;
                                    item.SyncedFrom = null;

                                    var hsUpdateSurcharge = surchargeRepo.Update(item, x => x.Id == item.Id, false);
                                }

                                surchargeRepo.SubmitChanges();
                            }
                        }
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
                        HandleState smSoa = acctSOARepository.SubmitChanges();
                        if (smSoa.Success)
                        {
                            string title = string.Format(@"Accountant Rejected Data SOA {0}", soa.Soano);
                            SysNotifications sysNotify = new SysNotifications
                            {
                                Id = Guid.NewGuid(),
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                Type = "User",
                                Title = title,
                                IsClosed = false,
                                IsRead = false,
                                Description = reason,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                                Action = "Detail",
                                ActionLink = string.Format(@"home/accounting/statement-of-account/detail?no={0}&currency=VND", soa.Soano),
                                UserIds = soa.UserCreated

                            };
                            sysNotificationRepository.Add(sysNotify);

                            SysUserNotification sysUserNotify = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                UserId = soa.UserCreated,
                                Status = "New",
                                NotitficationId = sysNotify.Id,
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            sysUserNotificationRepository.Add(sysUserNotify);

                            //Update PaySyncedFrom or SyncedFrom equal NULL by SoaNo
                            var surcharges = surchargeRepo.Get(x => x.Soano == soa.Soano || x.PaySoano == soa.Soano);
                            foreach (var surcharge in surcharges)
                            {
                                if (surcharge.Type == "OBH")
                                {
                                    surcharge.PaySyncedFrom = (soa.Soano == surcharge.PaySoano) ? null : surcharge.PaySyncedFrom;
                                    surcharge.SyncedFrom = (soa.Soano == surcharge.Soano) ? null : surcharge.SyncedFrom;
                                }
                                else
                                {
                                    surcharge.SyncedFrom = null;
                                }
                                surcharge.UserModified = currentUser.UserID;
                                surcharge.DatetimeModified = DateTime.Now;
                                var hsUpdateSurcharge = surchargeRepo.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }
                            var smSurcharge = surchargeRepo.SubmitChanges();
                        }
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
                        HandleState smCdNote = acctCdNoteRepo.SubmitChanges();
                        if (smCdNote.Success)
                        {
                            string title = string.Format(@"Accountant Rejected Data CDNote {0}", cdNote.Code);
                            SysNotifications sysNotify = new SysNotifications
                            {
                                Id = Guid.NewGuid(),
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                Type = "User",
                                Title = title,
                                IsClosed = false,
                                IsRead = false,
                                Description = reason,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                                Action = "Detail",
                                ActionLink = GetLinkCdNote(cdNote.Code, cdNote.JobId),
                                UserIds = cdNote.UserCreated
                            };
                            var hsNotifi = sysNotificationRepository.Add(sysNotify);

                            SysUserNotification sysUserNotify = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                UserId = cdNote.UserCreated,
                                Status = "New",
                                NotitficationId = sysNotify.Id,
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            var hsUserNotifi = sysUserNotificationRepository.Add(sysUserNotify);

                            //Update PaySyncedFrom or SyncedFrom equal NULL CDNote Code
                            var surcharges = surchargeRepo.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);
                            foreach (var surcharge in surcharges)
                            {
                                if (surcharge.Type == "OBH")
                                {
                                    surcharge.PaySyncedFrom = (cdNote.Code == surcharge.CreditNo) ? null : surcharge.PaySyncedFrom;
                                    surcharge.SyncedFrom = (cdNote.Code == surcharge.DebitNo) ? null : surcharge.SyncedFrom;
                                }
                                else
                                {
                                    surcharge.SyncedFrom = null;
                                }
                                surcharge.UserModified = currentUser.UserID;
                                surcharge.DatetimeModified = DateTime.Now;
                                var hsUpdateSurcharge = surchargeRepo.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }
                            var smSurcharge = surchargeRepo.SubmitChanges();
                        }
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
                        if (sm.Success)
                        {
                            string title = string.Format(@"Accountant Rejected Data Voucher {0}", voucher.VoucherId);
                            SysNotifications sysNotify = new SysNotifications
                            {
                                Id = Guid.NewGuid(),
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                Type = "User",
                                Title = title,
                                IsClosed = false,
                                IsRead = false,
                                Description = reason,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                                Action = "Detail",
                                ActionLink = string.Format(@"home/accounting/management/voucher/{0}", voucher.Id),
                                UserIds = voucher.UserCreated
                            };
                            sysNotificationRepository.Add(sysNotify);

                            SysUserNotification sysUserNotify = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                UserId = voucher.UserCreated,
                                Status = "New",
                                NotitficationId = sysNotify.Id,
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            sysUserNotificationRepository.Add(sysUserNotify);

                            //Update SyncedFrom equal NULL by Id of Voucher
                            var surcharges = surchargeRepo.Get(x => x.AcctManagementId == voucher.Id);
                            foreach (var surcharge in surcharges)
                            {
                                surcharge.SyncedFrom = null;
                                surcharge.UserModified = currentUser.UserID;
                                surcharge.DatetimeModified = DateTime.Now;
                                var hsUpdateSurcharge = surchargeRepo.Update(surcharge, x => x.Id == surcharge.Id, false);
                            }
                            var smSurcharge = surchargeRepo.SubmitChanges();
                        }
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
            return new HandleState();
            //Tạm thời comment, khi nào xong phần receipt sẽ mở lại
            /*
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var _id = Guid.Empty;
                    Guid.TryParse(id, out _id);
                    var receipt = receiptRepository.Get(x => x.Id == _id).FirstOrDefault();
                    if (receipt == null) return new HandleState((object)"Không tìm thấy phiếu thu");

                    receipt.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
                    receipt.UserModified = currentUser.UserID;
                    receipt.DatetimeModified = DateTime.Now;
                    receipt.ReasonReject = reason;

                    HandleState hs = receiptRepository.Update(receipt, x => x.Id == receipt.Id, false);
                    if (hs.Success)
                    {
                        HandleState smReceipt = receiptRepository.SubmitChanges();
                        if (smReceipt.Success)
                        {
                            string title = string.Format(@"Accountant Rejected Data Receipt {0}", receipt.PaymentRefNo);
                            SysNotifications sysNotify = new SysNotifications
                            {
                                Id = Guid.NewGuid(),
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                Type = "User",
                                Title = title,
                                IsClosed = false,
                                IsRead = false,
                                Description = reason,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                                Action = "Detail",
                                ActionLink = "", //Cập nhật sau
                                UserIds = receipt.UserCreated
                            };
                            sysNotificationRepository.Add(sysNotify);

                            SysUserNotification sysUserNotify = new SysUserNotification
                            {
                                Id = Guid.NewGuid(),
                                UserId = receipt.UserCreated,
                                Status = "New",
                                NotitficationId = sysNotify.Id,
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                UserCreated = currentUser.UserID,
                                UserModified = currentUser.UserID,
                            };
                            sysUserNotificationRepository.Add(sysUserNotify);
                        }
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
            }*/
        }

        /// <summary>
        /// Type là VOUCHER/PAYMENT => eFMS sẽ xóa mã VOUCHER/PAYMENT tương ứng
        /// Type là CDNOTE/SOA/SETTLEMENT => Reset trạng thái "Rejected" Cho từng phiều tương ứng
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
                    result = DeleteVoucher(model.ReferenceID, model.Reason);
                    break;
                case "PAYMENT":
                    result = DeletePayment(model.ReferenceID, model.Reason);
                    break;
                default:
                    result = new HandleState((object)"Type không hợp lệ (Các type hợp lệ: SETTLEMENT, SOA, CDNOTE, VOUCHER, PAYMENT)");
                    break;
            }
            return result;
        }

        private HandleState DeleteVoucher(string id, string reason)
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
                            charge.VoucherId = null;
                            charge.VoucherIddate = null;
                            charge.InvoiceNo = null;
                            charge.InvoiceDate = null;
                            charge.SeriesNo = null;
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = currentUser.UserID;
                            charge.SyncedFrom = null; //Update SyncedFrom equal NULL
                            charge.ReferenceNo = null;
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

                        string title = string.Format(@"Accountant Removed Data Voucher {0}", voucher.VoucherId);
                        SysNotifications sysNotify = new SysNotifications
                        {
                            Id = Guid.NewGuid(),
                            DatetimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            Type = "User",
                            Title = title,
                            IsClosed = false,
                            IsRead = false,
                            Description = reason,
                            UserCreated = currentUser.UserID,
                            UserModified = currentUser.UserID,
                            Action = "Detail",
                            ActionLink = string.Format(@"home/accounting/management/voucher/{0}", voucher.Id),
                            UserIds = voucher.UserCreated

                        };
                        var hsNotifi = sysNotificationRepository.Add(sysNotify, false);

                        SysUserNotification sysUserNotify = new SysUserNotification
                        {
                            Id = Guid.NewGuid(),
                            UserId = voucher.UserCreated,
                            Status = "New",
                            NotitficationId = sysNotify.Id,
                            DatetimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            UserCreated = currentUser.UserID,
                            UserModified = currentUser.UserID,
                        };
                        var hsUserNotifi = sysUserNotificationRepository.Add(sysUserNotify, false);
                        
                        var smNotifi = sysNotificationRepository.SubmitChanges();
                        var smUserNotifi = sysUserNotificationRepository.SubmitChanges();
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
        
        private HandleState DeletePayment(string id, string reason)
        {
            //Cập nhật sau
            return new HandleState();
        }

        #endregion --- REJECT & REMOVE DATA ---

    }
}


