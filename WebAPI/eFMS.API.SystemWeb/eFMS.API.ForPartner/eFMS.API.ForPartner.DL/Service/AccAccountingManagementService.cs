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
using eFMS.API.ForPartner.Service.Contexts;
using ITL.NetCore.Connection;
using System.Data.SqlClient;
using eFMS.API.ForPartner.Service.ViewModels;
using System.Data;
using eFMS.API.ForPartner.DL.ViewModel;
using eFMS.API.ForPartner.DL.Models.Receivable;

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
        private readonly IContextBase<SysCompany> companyRepository;
        private readonly IContextBase<CatContract> catContractRepository;
        private readonly IContextBase<AcctAdvanceRequest> acctAdvanceRequestRepository;
        private readonly IContextBase<AcctReceiptSync> receiptSyncRepository;
        private readonly IContextBase<AccAccountReceivable> accReceivableRepository;

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
            IContextBase<AcctReceipt> receiptRepo,
            IContextBase<SysCompany> companyRepo,
            IContextBase<CatContract> catContractRepo,
            IContextBase<AcctAdvanceRequest> acctAdvanceRequestRepo,
            IContextBase<AcctReceiptSync> receiptSyncRepo,
            IContextBase<AccAccountReceivable> accReceivableRepo
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
            companyRepository = companyRepo;
            catContractRepository = catContractRepo;
            acctAdvanceRequestRepository = acctAdvanceRequestRepo;
            receiptSyncRepository = receiptSyncRepo;
            accReceivableRepository = accReceivableRepo;
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
            currentUser.Action = "InsertInvoice";

            var hsInsertInvoice = InsertInvoice(model, currentUser);
            return hsInsertInvoice;
        }

        private HandleState InsertInvoice(InvoiceCreateInfo model, ICurrentUser _currentUser)
        {
            var chargeInvoiceDebitUpdate = new List<ChargeInvoiceUpdateTable>();
            var chargeInvoiceObhUpdate = new List<ChargeInvoiceUpdateTable>();
            var invoiceDebit = new AccAccountingManagement();
            var invoicesObh = new List<AccAccountingManagement>();

            decimal kickBackExcRate = companyRepository.Get(x => x.Id == _currentUser.CompanyID).FirstOrDefault()?.KbExchangeRate ?? 20000;

            HandleState hsDebit = new HandleState();
            HandleState hsObh = new HandleState();
            try
            {
                var debitCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_DEBIT).ToList();
                var obhCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_CHARGE_OBH).ToList();
                var surchargesLookupId = surchargeRepo.Get().ToLookup(x => x.Id);

                /*var chargeNotExistsInSurcharge = GetChargeNotExistsInSurcharge(model.Charges, surchargesLookupId);
                if (!string.IsNullOrEmpty(chargeNotExistsInSurcharge))
                {
                    string message = string.Format(@"ChargeId: {0} không tồn tại. Vui lòng kiểm tra lại", chargeNotExistsInSurcharge);
                    return new HandleState((object)message);
                }

                var chargeObhNotMatchCurrency = GetChargeNotMatchCurrencyInSurcharge(obhCharges, surchargesLookupId);
                if (!string.IsNullOrEmpty(chargeObhNotMatchCurrency))
                {
                    string message = string.Format(@"ChargeId: {0} có currency không hợp lệ. Vui lòng kiểm tra lại", chargeObhNotMatchCurrency);
                    return new HandleState((object)message);
                }*/

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

                invoiceDebit = debitCharges.Count > 0 ? ModelInvoiceDebit(model, partner, debitCharges, _currentUser, surchargesLookupId) : null;
                invoicesObh = obhCharges.Count > 0 ? ModelInvoicesObh(model, partner, obhCharges, _currentUser, surchargesLookupId) : null;
                
                var debitChargesUpdate = new List<CsShipmentSurcharge>();
                var obhChargesUpdate = new List<CsShipmentSurcharge>();
                
                if (invoiceDebit != null)
                {
                    decimal _totalAmountInvoiceDebit = 0;
                    decimal _totalAmountVndInvoiceDebit = 0;
                    decimal _totalAmountUsdInvoiceDebit = 0;

                    foreach (var debitCharge in debitCharges)
                    {
                        CsShipmentSurcharge surchargeDebit = surchargesLookupId[debitCharge.ChargeId].FirstOrDefault();
                        surchargeDebit.AcctManagementId = invoiceDebit.Id;
                        surchargeDebit.InvoiceNo = invoiceDebit.InvoiceNoReal;
                        surchargeDebit.InvoiceDate = invoiceDebit.Date;
                        surchargeDebit.VoucherId = invoiceDebit.VoucherId;
                        surchargeDebit.VoucherIddate = invoiceDebit.Date;
                        surchargeDebit.SeriesNo = invoiceDebit.Serie;

                        if (invoiceDebit.Currency != ForPartnerConstants.CURRENCY_LOCAL)
                        {
                            //if (surchargeDebit.FinalExchangeRate != debitCharge.ExchangeRate)
                            //{                               
                                surchargeDebit.FinalExchangeRate = CalculatorExchangeRate(debitCharge.ExchangeRate, surchargeDebit.ExchangeDate, surchargeDebit.CurrencyId, invoiceDebit.Currency);

                                #region -- Tính lại giá trị các field  dựa vào FinalExchangeRate mới: NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                                    var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(surchargeDebit, kickBackExcRate);
                                    surchargeDebit.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                                    surchargeDebit.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                                    // surchargeDebit.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                                    // surchargeDebit.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                                    surchargeDebit.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                                    surchargeDebit.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)

                                    // 16164 - Tính theo tỷ giá của bravo  
                                    surchargeDebit.AmountVnd = NumberHelper.RoundNumber(amountSurcharge.AmountUsd * (debitCharge.ExchangeRate ?? 0), 0);
                                    surchargeDebit.VatAmountVnd = NumberHelper.RoundNumber(amountSurcharge.VatAmountUsd * (debitCharge.ExchangeRate ?? 0), 0);

                                #endregion -- Tính lại giá trị các field  dựa vào FinalExchangeRate mới: NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                            // }
                        }

                        surchargeDebit.ReferenceNo = debitCharge.ReferenceNo;
                        surchargeDebit.DatetimeModified = DateTime.Now;
                        surchargeDebit.UserModified = _currentUser.UserID;

                        debitChargesUpdate.Add(surchargeDebit);

                        _totalAmountInvoiceDebit += currencyExchangeService.ConvertAmountChargeToAmountObj(surchargeDebit, invoiceDebit.Currency);
                        _totalAmountVndInvoiceDebit += (surchargeDebit.AmountVnd + surchargeDebit.VatAmountVnd) ?? 0;
                        _totalAmountUsdInvoiceDebit += (surchargeDebit.AmountUsd + surchargeDebit.VatAmountUsd) ?? 0;
                    }

                    invoiceDebit.TotalAmount = invoiceDebit.UnpaidAmount = _totalAmountInvoiceDebit;

                    //Task: 15631 - Andy - 14/04/2021
                    invoiceDebit.TotalAmountVnd = invoiceDebit.UnpaidAmountVnd = _totalAmountVndInvoiceDebit;
                    invoiceDebit.TotalAmountUsd = invoiceDebit.UnpaidAmountUsd = _totalAmountUsdInvoiceDebit;

                    var _transactionTypes = debitChargesUpdate.Select(s => s.TransactionType).Distinct().ToList();                    
                    invoiceDebit.ServiceType = string.Join(";", _transactionTypes);

                    //Task: 15631 - Andy - 14/04/2021
                    invoiceDebit.PaymentDueDate = GetDueDateIssueAcctMngt(invoiceDebit.PartnerId, invoiceDebit.PaymentTerm, _transactionTypes, invoiceDebit.Date, invoiceDebit.ConfirmBillingDate);
                }

                if (invoicesObh != null)
                {
                    foreach (var invoiceObh in invoicesObh)
                    {
                        decimal _totalAmountInvoiceObh = 0;
                        decimal _totalAmountVndInvoiceObh = 0;
                        decimal _totalAmountUsdInvoiceObh = 0;

                        //Cập nhật số RefNo cho phí OBH
                        var _obhCharges = obhCharges.Where(x => x.ReferenceNo == invoiceObh.ReferenceNo);
                        foreach (var obhCharge in _obhCharges)
                        {
                            CsShipmentSurcharge surchargeObh = surchargesLookupId[obhCharge.ChargeId].FirstOrDefault();
                            surchargeObh.AcctManagementId = invoiceObh.Id;
                            //surchargeObh.InvoiceNo = null; //CR: 07/12/2020; đã change [25/02/2021]
                            //surchargeObh.InvoiceDate = null; //CR: 07/12/2020; đã change [25/02/2021]
                            surchargeObh.VoucherId = invoiceObh.VoucherId;
                            surchargeObh.VoucherIddate = invoiceObh.Date;
                            //surchargeObh.SeriesNo = null; //CR: 07/12/2020; đã change [25/02/2021]
                            surchargeObh.FinalExchangeRate = obhCharge.ExchangeRate; //Lấy exchangeRate từ Bravo trả về

                            if (invoiceObh.Currency != ForPartnerConstants.CURRENCY_LOCAL)
                            {
                                //if (surchargeObh.FinalExchangeRate != obhCharge.ExchangeRate)
                                //{
                                    #region -- Tính lại giá trị các field dựa vào FinalExchangeRate mới: NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                                    var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(surchargeObh, kickBackExcRate);
                                    surchargeObh.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                                    surchargeObh.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                                    // surchargeObh.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                                    // surchargeObh.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                                    surchargeObh.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                                    surchargeObh.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)

                                    // 16164 - Tính theo tỷ giá của bravo
                                    surchargeObh.AmountVnd = NumberHelper.RoundNumber(amountSurcharge.AmountUsd * (obhCharge.ExchangeRate ?? 0), 0);
                                    surchargeObh.VatAmountVnd = NumberHelper.RoundNumber(amountSurcharge.VatAmountUsd * (obhCharge.ExchangeRate ?? 0), 0);
                                    #endregion -- Tính lại giá trị các field  dựa vào FinalExchangeRate mới: NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                                // }
                            }

                            surchargeObh.ReferenceNo = obhCharge.ReferenceNo;
                            surchargeObh.DatetimeModified = DateTime.Now;
                            surchargeObh.UserModified = _currentUser.UserID;

                            obhChargesUpdate.Add(surchargeObh);

                            _totalAmountInvoiceObh += currencyExchangeService.ConvertAmountChargeToAmountObj(surchargeObh, invoiceObh.Currency);
                            _totalAmountVndInvoiceObh += (surchargeObh.AmountVnd + surchargeObh.VatAmountVnd) ?? 0;
                            _totalAmountUsdInvoiceObh += (surchargeObh.AmountUsd + surchargeObh.VatAmountUsd) ?? 0;
                        }

                        invoiceObh.TotalAmount = invoiceObh.UnpaidAmount = _totalAmountInvoiceObh;

                        //Task: 15631 - Andy - 14/04/2021
                        invoiceObh.TotalAmountVnd = invoiceObh.UnpaidAmountVnd = _totalAmountVndInvoiceObh;
                        invoiceObh.TotalAmountUsd = invoiceObh.UnpaidAmountUsd = _totalAmountUsdInvoiceObh;

                        var _transactionTypes = obhChargesUpdate.Select(s => s.TransactionType).Distinct().ToList();
                        invoiceObh.ServiceType = string.Join(";", _transactionTypes);

                        //Task: 15631 - Andy - 14/04/2021
                        invoiceObh.PaymentDueDate = GetDueDateIssueAcctMngt(invoiceObh.PartnerId, invoiceObh.PaymentTerm, _transactionTypes, invoiceObh.Date, invoiceObh.ConfirmBillingDate);
                    }
                }

                if (invoiceDebit != null)
                {
                    //Update Charge Debit (if valuable)
                    chargeInvoiceDebitUpdate = mapper.Map<List<ChargeInvoiceUpdateTable>>(debitChargesUpdate);
                    var updateSurchargeDebit = UpdateSurchargeForInvoice(chargeInvoiceDebitUpdate);
                    if (!updateSurchargeDebit.Status)
                    {
                        WriteLogInsertInvoice(updateSurchargeDebit.Status, model.InvoiceNo, invoiceDebit, invoicesObh, chargeInvoiceDebitUpdate, chargeInvoiceObhUpdate, updateSurchargeDebit.Message.ToString());
                        return new HandleState((object)updateSurchargeDebit.Message);
                    }
                }
                
                if (invoicesObh != null)
                {
                    //Update Charge OBH (if valuable)
                    chargeInvoiceObhUpdate = mapper.Map<List<ChargeInvoiceUpdateTable>>(obhChargesUpdate);
                    var updateSurchargeObh = UpdateSurchargeForInvoice(chargeInvoiceObhUpdate);
                    if (!updateSurchargeObh.Status)
                    {
                        WriteLogInsertInvoice(updateSurchargeObh.Status, model.InvoiceNo, invoiceDebit, invoicesObh, chargeInvoiceDebitUpdate, chargeInvoiceObhUpdate, updateSurchargeObh.Message.ToString());
                        return new HandleState((object)updateSurchargeObh.Message);
                    }
                }
                
                if (invoiceDebit != null)
                {
                    //Insert Invoice Debit (if valuable)
                    hsDebit = DataContext.Add(invoiceDebit);
                }
                
                if (invoicesObh != null)
                {
                    //Insert list Invoice Temp (Invoice OBH)
                    var invoiceObhListAdd = mapper.Map<List<InvoiceTable>>(invoicesObh);
                    var addInvoiceObh = InsertInvoiceList(invoiceObhListAdd);
                    if (!addInvoiceObh.Status)
                    {
                        hsObh = new HandleState((object)addInvoiceObh.Message);
                    }
                }

                if (!hsDebit.Success)
                {
                    WriteLogInsertInvoice(hsDebit.Success, model.InvoiceNo, invoiceDebit, invoicesObh, chargeInvoiceDebitUpdate, chargeInvoiceObhUpdate, hsDebit.Message.ToString());
                    return hsDebit;
                }
                if (!hsObh.Success)
                {
                    WriteLogInsertInvoice(hsObh.Success, model.InvoiceNo, invoiceDebit, invoicesObh, chargeInvoiceDebitUpdate, chargeInvoiceObhUpdate, hsObh.Message.ToString());
                    return hsObh;
                }
                WriteLogInsertInvoice(hsDebit.Success, model.InvoiceNo, invoiceDebit, invoicesObh, chargeInvoiceDebitUpdate, chargeInvoiceObhUpdate, "Create Invoice Successful");
                return hsDebit;                
            }
            catch (Exception ex)
            {
                WriteLogInsertInvoice(false, model.InvoiceNo,invoiceDebit, invoicesObh, chargeInvoiceDebitUpdate, chargeInvoiceObhUpdate, ex.ToString());
                return new HandleState((object)ex.Message);
            }
        }

        /// <summary>
        /// Get Due Date dựa vào Contract Partner Invoice
        /// Task: 15631 - Andy - 14/04/2021
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="paymentTerm"></param>
        /// <param name="serviceTypes"></param>
        /// <param name="invoiceDate">Ngày Invoice</param>
        /// <param name="billingDate">Ngày Billing</param>
        /// <returns></returns>
        private DateTime? GetDueDateIssueAcctMngt(string partnerId, decimal? paymentTerm, List<string> serviceTypes, DateTime? invoiceDate, DateTime? billingDate)
        {
            DateTime? dueDate = null;
            var contractPartner = catContractRepository.Get(x => x.Active == true && x.PartnerId == partnerId && x.OfficeId.Contains(currentUser.OfficeID.ToString()) && serviceTypes.Any(a => x.SaleService.Contains(a))).FirstOrDefault();
            if (contractPartner == null)
            {
                var acRefPartner = partnerRepo.Get(x => x.Id == partnerId).FirstOrDefault()?.ParentId;
                contractPartner = catContractRepository.Get(x => x.Active == true && x.PartnerId == acRefPartner && x.OfficeId.Contains(currentUser.OfficeID.ToString()) && serviceTypes.Any(a => x.SaleService.Contains(a))).FirstOrDefault();
            }

            if (contractPartner != null)
            {
                //Nếu Base On là Invoice Date: Due Date = Invoice Date + Payment Term
                if (contractPartner.BaseOn == "Invoice Date")
                {
                    dueDate = invoiceDate.HasValue ? invoiceDate.Value.AddDays((double)(paymentTerm ?? 0)) : invoiceDate;
                }
                //Nếu Base On là Billing Date : Due Date = Billing date + Payment Term
                if (contractPartner.BaseOn == "Billing Date")
                {
                    dueDate = billingDate.HasValue ? billingDate.Value.AddDays((double)(paymentTerm ?? 0)) : billingDate;
                }
            }
            else
            {
                //Nếu Partner không có contract thì lấy Invoice Date của Invoice
                dueDate = invoiceDate;
            }
            return dueDate;
        }

        private sp_UpdateChargeInvoiceUpdate UpdateSurchargeForInvoice(List<ChargeInvoiceUpdateTable> surcharges)
        {            
            var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@Charges",
                    Value = DataHelper.ToDataTable(surcharges),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[ChargeInvoiceUpdateTable]"
                }
            };
            var result = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_UpdateChargeInvoiceUpdate>(parameters);
            return result.FirstOrDefault();
        }

        private sp_InsertListInvoice InsertInvoiceList(List<InvoiceTable> invoices)
        {
            var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@Invoices",
                    Value = DataHelper.ToDataTable(invoices),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[InvoiceTable]"
                }
            };
            var result = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_InsertListInvoice>(parameters);
            return result.FirstOrDefault();
        }

        private void WriteLogInsertInvoice(bool status, string invoiceNo, AccAccountingManagement invoiceDebit, List<AccAccountingManagement> invoicesObh, List<ChargeInvoiceUpdateTable> debitCharges, List<ChargeInvoiceUpdateTable> obhCharges, string message)
        {
            string logMessage = string.Format("InsertInvoice by {0} at {1} \n ** Message: {2} \n ** InvoiceDebit: {3} \n ** InvoicesObh: {4} \n ** DebitCharges: {5} \n ** ObhCharges: {6} \n\n---------------------------\n\n",
                            currentUser.UserID,
                            DateTime.Now.ToString("dd/MM/yyyy HH:ss:mm"),
                            message,
                            invoiceDebit != null ? JsonConvert.SerializeObject(invoiceDebit) : "{}",
                            invoicesObh != null ? JsonConvert.SerializeObject(invoicesObh) : "[]",
                            debitCharges !=null ? JsonConvert.SerializeObject(debitCharges) : "[]",
                            obhCharges != null ? JsonConvert.SerializeObject(obhCharges) : "[]");
            string logName = string.Format("InsertInvoice_{0}_{1}", (status ? "Success" : "Fail"), invoiceNo);
            new LogHelper(logName, logMessage);
        }
        
        /// <summary>
        /// Get model Invoice Debit
        /// </summary>
        /// <param name="model"></param>
        /// <param name="partner"></param>
        /// <param name="debitCharges"></param>
        /// <param name="_currentUser"></param>
        /// <returns></returns>
        private AccAccountingManagement ModelInvoiceDebit(InvoiceCreateInfo model, CatPartner partner, List<ChargeInvoice> debitCharges, ICurrentUser _currentUser, ILookup<Guid, CsShipmentSurcharge> surchargeLookupId)
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
            invoice.TotalAmount = invoice.UnpaidAmount = 0; //TÍNH TOÁN BÊN NGOÀI
            invoice.UserCreated = invoice.UserModified = _currentUser.UserID;
            invoice.DatetimeCreated = invoice.DatetimeModified = DateTime.Now;
            invoice.GroupId = _currentUser.GroupId;
            invoice.DepartmentId = _currentUser.DepartmentId;

            var firstCharge = surchargeLookupId[debitChargeFirst.ChargeId].FirstOrDefault();
            invoice.OfficeId = firstCharge?.OfficeId ?? Guid.Empty; //Lấy OfficeId của first charge
            invoice.CompanyId = firstCharge?.CompanyId ?? Guid.Empty; //Lấy CompanyId của first charge

            invoice.PaymentTerm = debitChargeFirst?.PaymentTerm;
            invoice.PaymentMethod = ForPartnerConstants.PAYMENT_METHOD_BANK_OR_CASH; //Set default "Bank Transfer / Cash"
            invoice.AccountNo = !string.IsNullOrEmpty(debitChargeFirst.AccountNo) ? debitChargeFirst.AccountNo : SetAccountNoForInvoice(partner?.PartnerMode, model.Currency);
            invoice.Description = model.Description;
            invoice.ServiceType = string.Empty; //TÍNH TOÁN BÊN NGOÀI

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
        private List<AccAccountingManagement> ModelInvoicesObh(InvoiceCreateInfo model, CatPartner partner, List<ChargeInvoice> obhCharges, ICurrentUser _currentUser, ILookup<Guid, CsShipmentSurcharge> surchargeLookupId)
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
                invoice.TotalAmount = invoice.UnpaidAmount = 0; //TÍNH TOÁN BÊN NGOÀI
                invoice.UserCreated = invoice.UserModified = _currentUser.UserID;
                invoice.DatetimeCreated = invoice.DatetimeModified = DateTime.Now;
                invoice.GroupId = _currentUser.GroupId;
                invoice.DepartmentId = _currentUser.DepartmentId;

                var firstCharge = surchargeLookupId[obhChargeFirst.ChargeId].FirstOrDefault();
                invoice.OfficeId = firstCharge?.OfficeId ?? Guid.Empty; //Lấy OfficeId của first charge
                invoice.CompanyId = firstCharge?.CompanyId ?? Guid.Empty; //Lấy CompanyId của first charge

                invoice.PaymentTerm = obhChargeFirst.PaymentTerm;
                invoice.PaymentMethod = ForPartnerConstants.PAYMENT_METHOD_BANK_OR_CASH; //Set default "Bank Transfer / Cash"
                invoice.AccountNo = obhChargeFirst.AccountNo;
                invoice.Description = model.Description;
                invoice.ServiceType = string.Empty; //TÍNH TOÁN BÊN NGOÀI

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
            currentUser.Action = "DeleteInvoice";

            var hsDeleteInvoice = DeleteInvoice(model, currentUser);
            return hsDeleteInvoice;
        }

        HandleState DeleteInvoice(InvoiceInfo model, ICurrentUser _currentUser)
        {
            string invoiceType = ForPartnerConstants.ACCOUNTING_INVOICE_TYPE;
            List<Guid?> IdsInvoiceTemps = new List<Guid?>();
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    AccAccountingManagement data = null;
                    HandleState hs = new HandleState();

                    if (model.SerieNo == ForPartnerConstants.TYPE_CHARGE_OBH)
                    {
                        invoiceType = ForPartnerConstants.ACCOUNTING_INVOICE_TEMP_TYPE;
                        data = DataContext.Get(x => x.Type == ForPartnerConstants.ACCOUNTING_INVOICE_TEMP_TYPE
                                                && x.ReferenceNo == model.ReferenceNo && x.InvoiceNoReal == model.ReferenceNo).FirstOrDefault();

                        CsShipmentSurcharge charge = surchargeRepo.First(x => x.ReferenceNo == model.ReferenceNo && x.Type == ForPartnerConstants.TYPE_CHARGE_OBH);
                        if(charge == null || string.IsNullOrEmpty(charge.SyncedFrom))
                        {
                            return new HandleState((object)"Không tìm thấy hóa đơn");
                        }

                        // Find and Delete Inoice Temp with same debit/soa
                        IQueryable<CsShipmentSurcharge> surchargeHadSynced = null;
                        if (charge.SyncedFrom == ForPartnerConstants.SYNCED_FROM_CDNOTE)
                        {
                            surchargeHadSynced = surchargeRepo.Get(x => x.DebitNo == charge.DebitNo 
                            &&  x.SyncedFrom == ForPartnerConstants.SYNCED_FROM_CDNOTE 
                            && x.Type == ForPartnerConstants.TYPE_CHARGE_OBH);
                        }
                        else if(charge.SyncedFrom == ForPartnerConstants.SYNCED_FROM_SOA)
                        {
                            surchargeHadSynced = surchargeRepo.Get(x => x.Soano == charge.Soano
                           && x.SyncedFrom == ForPartnerConstants.SYNCED_FROM_SOA
                           && x.Type == ForPartnerConstants.TYPE_CHARGE_OBH);
                        }

                        if (surchargeHadSynced != null && surchargeHadSynced.Count() > 0)
                        {
                            IdsInvoiceTemps = surchargeHadSynced.Select(x => x.AcctManagementId).Distinct().ToList();

                            foreach (var Id in IdsInvoiceTemps)
                            {
                                DataContext.Delete(x => x.Id == Id, false);
                            }
                        }
                    }
                    else
                    {
                        data = DataContext.Get(x => x.ReferenceNo == model.ReferenceNo
                                                && x.InvoiceNoReal == model.InvoiceNo
                                                && x.Serie == model.SerieNo
                                                && x.Type == ForPartnerConstants.ACCOUNTING_INVOICE_TYPE).FirstOrDefault();


                        hs = DataContext.Delete(x => x.ReferenceNo == model.ReferenceNo
                                                              && x.InvoiceNoReal == model.InvoiceNo
                                                              && x.Serie == model.SerieNo
                                                              && x.Type == ForPartnerConstants.ACCOUNTING_INVOICE_TYPE, false);
                    }
                   
                    if (data == null) return new HandleState((object)"Không tìm thấy hóa đơn");

                   
                    if (hs.Success)
                    {
                        IQueryable<CsShipmentSurcharge> charges = null;
                        if(invoiceType == ForPartnerConstants.ACCOUNTING_INVOICE_TYPE)
                        {
                            charges = surchargeRepo.Get(x => x.ReferenceNo == model.ReferenceNo);
                        }
                        else
                        {
                            charges = surchargeRepo.Get(x => IdsInvoiceTemps.Contains(x.AcctManagementId) && x.Type == ForPartnerConstants.TYPE_CHARGE_OBH);
                        }
                        
                        if(charges != null)
                        {
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

                                if(invoiceType == ForPartnerConstants.ACCOUNTING_INVOICE_TYPE)
                                {
                                    //Update Status Removed Inv For SOA (SOA synced)
                                    UpdateStatusRemovedInvForSOA(charge.Soano);
                                    //Update Status Removed Inv For Debit Note (Debit Note synced)
                                    UpdateStatusRemovedInvForDebitNote(charge.DebitNo);
                                }

                                var updateSur = surchargeRepo.Update(charge, x => x.Id == charge.Id, false);
                            }
                            if (invoiceType == ForPartnerConstants.ACCOUNTING_INVOICE_TEMP_TYPE)
                            {
                                string soaNo = charges.First().Soano;
                                string debitNo = charges.First().DebitNo;

                                UpdateStatusRemovedInvForSOA(soaNo);
                                UpdateStatusRemovedInvForDebitNote(debitNo);
                            }
                        }
                       

                        var smSoa = acctSOARepository.SubmitChanges();
                        var smDebitNote = acctCdNoteRepo.SubmitChanges();
                        var smSur = surchargeRepo.SubmitChanges();
                        var sm = DataContext.SubmitChanges();

                        // Tính lại công nợ của hđ vừa hủy từ bravo.
                        //if(sm.Success)
                        //{
                        //    CalculatorInvoiceReceivable(data);
                        //}

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

        public void CalculatorInvoiceReceivable(AccAccountingManagement invoice)
        {
            AccAccountReceivableModel receivable = new AccAccountReceivableModel();
            CatPartner partner = partnerRepo.Get(x => x.Id == invoice.PartnerId).FirstOrDefault();
            // Hđ của đối tượng này đang cho bao nhiêu service.

            var totalInvoiceServices = invoice.ServiceType.Split(";").ToList();
            //Không tính công nợ cho đối tượng Internal
            if (partner != null && partner.PartnerMode != "Internal")
            {
                receivable.PartnerId = invoice.PartnerId;
                receivable.Office = invoice.OfficeId;
                receivable.Service = invoice.ServiceType;
                receivable.AcRef = partner.ParentId ?? partner.Id;
                CatContract contractPartner = catContractRepository.Get(x => x.Active == true
                                                                && x.PartnerId == invoice.PartnerId
                                                                && x.OfficeId.Contains(invoice.OfficeId.ToString())
                                                                && x.SaleService.Contains(invoice.ServiceType)).FirstOrDefault();
                if (contractPartner == null)
                {
                    // Lấy currency local và use created of partner gán cho Receivable
                    receivable.ContractId = null;
                    receivable.ContractCurrency = ForPartnerConstants.CURRENCY_LOCAL;
                    receivable.SaleMan = null;
                    receivable.UserCreated = partner.UserCreated;
                    receivable.UserModified = partner.UserCreated;
                    receivable.GroupId = partner.GroupId;
                    receivable.DepartmentId = partner.DepartmentId;
                    receivable.OfficeId = partner.OfficeId;
                    receivable.CompanyId = partner.CompanyId;
                }
                else
                {
                    // Lấy currency của contract & user created of contract gán cho Receivable
                    receivable.ContractId = contractPartner.Id;
                    receivable.ContractCurrency = contractPartner.CurrencyId;
                    receivable.SaleMan = contractPartner.SaleManId;
                    receivable.UserCreated = contractPartner.UserCreated;
                    receivable.UserModified = contractPartner.UserCreated;
                    receivable.GroupId = null;
                    receivable.DepartmentId = null;
                    receivable.OfficeId = invoice.OfficeId;
                    receivable.CompanyId = contractPartner.CompanyId;
                }
            }

            // lấy thông tin + Cập nhật lại công nợcủa đối tượng
            var receivables = accReceivableRepository.Get(x => x.PartnerId == invoice.PartnerId);
            if (receivables.Count() > 0)
            {
                decimal? _totalAmount = invoice.Currency != ForPartnerConstants.CURRENCY_LOCAL ? invoice.TotalAmountUsd : invoice.TotalAmountVnd;
                decimal? _totalUnpaid = invoice.Currency != ForPartnerConstants.CURRENCY_LOCAL ? invoice.UnpaidAmountUsd : invoice.UnpaidAmountVnd;
                decimal? _totalPaid = invoice.Currency != ForPartnerConstants.CURRENCY_LOCAL ? invoice.PaidAmountUsd : invoice.PaidAmountVnd;

                if (totalInvoiceServices.Count == 1)
                {
                    AccAccountReceivable receivableCurrent = receivables.Where(x => x.Service == totalInvoiceServices.First())?.FirstOrDefault();
                    if (receivableCurrent != null)
                    {
                        receivableCurrent.BillingAmount = (receivableCurrent.BillingAmount ?? 0) - _totalAmount;
                        receivableCurrent.BillingUnpaid = (receivableCurrent.BillingUnpaid ?? 0) - _totalUnpaid;
                        receivableCurrent.PaidAmount = (receivableCurrent.BillingAmount ?? 0) - _totalPaid;
                        receivableCurrent.SellingNoVat = (receivableCurrent.SellingNoVat ?? 0) + _totalAmount;

                        receivableCurrent.DebitAmount = (receivableCurrent.SellingNoVat ?? 0) + (receivableCurrent.BillingUnpaid ?? 0) + (receivableCurrent.ObhUnpaid ?? 0) + (receivableCurrent.AdvanceAmount ?? 0);
                        accReceivableRepository.Update(receivableCurrent, x => x.Id == receivableCurrent.Id, false);
                    }
                }

                if (totalInvoiceServices.Count > 1)
                {
                    // Chi đều cho các service.
                    _totalAmount /= totalInvoiceServices.Count;
                    _totalPaid /= totalInvoiceServices.Count;
                    _totalUnpaid /= totalInvoiceServices.Count;
                    foreach (var service in totalInvoiceServices)
                    {
                        AccAccountReceivable receivableCurrent = receivables.Where(x => x.Service == service)?.FirstOrDefault();
                        if (receivableCurrent != null)
                        {
                            receivableCurrent.BillingAmount = (receivableCurrent.BillingAmount ?? 0) - _totalAmount;
                            receivableCurrent.BillingUnpaid = (receivableCurrent.BillingUnpaid ?? 0) - _totalUnpaid;
                            receivableCurrent.PaidAmount = (receivableCurrent.BillingAmount ?? 0) - _totalPaid;
                            receivableCurrent.SellingNoVat = (receivableCurrent.SellingNoVat ?? 0) + _totalAmount;

                            receivableCurrent.DebitAmount = (receivableCurrent.SellingNoVat ?? 0) + (receivableCurrent.BillingUnpaid ?? 0) + (receivableCurrent.ObhUnpaid ?? 0) + (receivableCurrent.AdvanceAmount ?? 0);

                            accReceivableRepository.Update(receivableCurrent, x => x.Id == receivableCurrent.Id, false);
                        }
                    }
                }

                accReceivableRepository.SubmitChanges();

            }

        }

        private HandleState UpdateStatusRemovedInvForSOA(string soaNo)
        {
            var hsUpdate = new HandleState();
            if (string.IsNullOrEmpty(soaNo))
            {
                return hsUpdate;
            }
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
            if (string.IsNullOrEmpty(debitNo))
            {
                return hsUpdate;
            }
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
        private string GetChargeNotExistsInSurcharge(List<ChargeInvoice> charges, ILookup<Guid, CsShipmentSurcharge> surchargeLookupId)
        {
            string message = string.Empty;
            //var surchargeLookupId = surchargeRepo.Get().ToLookup(x => x.Id);
            foreach (var charge in charges)
            {
                var surcharge = surchargeLookupId[charge.ChargeId].FirstOrDefault();
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
        private string GetChargeNotMatchCurrencyInSurcharge(List<ChargeInvoice> charges, ILookup<Guid, CsShipmentSurcharge> surchargeLookupId)
        {
            string message = string.Empty;
            //var surchargeLookupId = surchargeRepo.Get().ToLookup(x => x.Id);
            foreach (var charge in charges)
            {
                var surcharge = surchargeLookupId[charge.ChargeId].FirstOrDefault();
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
            //if (string.IsNullOrEmpty(acctMngtType)) return string.Empty;
            //int monthCurrent = DateTime.Now.Month;
            //string year = DateTime.Now.Year.ToString();
            //string month = monthCurrent.ToString().PadLeft(2, '0');//Nếu tháng < 10 thì gắn thêm số 0 phía trước, VD: 09
            //string no = "001";

            //IQueryable<string> voucherNewests = null;
            //string _prefixVoucher = string.Empty;
            //if (acctMngtType == ForPartnerConstants.ACCOUNTING_INVOICE_TYPE)
            //{
            //    _prefixVoucher = "FDT";
            //}
            //else if (acctMngtType == ForPartnerConstants.ACCOUNTING_VOUCHER_TYPE)
            //{
            //    if (string.IsNullOrEmpty(voucherType)) return string.Empty;
            //    _prefixVoucher = GetPrefixVoucherByVoucherType(voucherType);
            //}
            //voucherNewests = Get(x => x.Type == acctMngtType && x.VoucherId.Contains(_prefixVoucher) && x.VoucherId.Substring(0, 4) == year && x.VoucherId.Substring(11, 2) == month)
            //    .OrderByDescending(o => o.VoucherId).Select(s => s.VoucherId);

            //string voucherNewest = voucherNewests.FirstOrDefault();
            //if (!string.IsNullOrEmpty(voucherNewest))
            //{
            //    var _noNewest = voucherNewest.Substring(7, 3);
            //    if (_noNewest != "999")
            //    {
            //        no = (Convert.ToInt32(_noNewest) + 1).ToString();
            //        no = no.PadLeft(3, '0');
            //    }
            //}
            //string voucher = year + _prefixVoucher + no + "/" + month;
            //return voucher;

            string funcName = "dbo.fnc_GenerateVoucherID";
            SqlParameter[] parameters = new[] { new SqlParameter("@Type", acctMngtType), new SqlParameter("@VoucherType", voucherType) };

            var result = ((eFMSDataContext)DataContext.DC).ExecuteFuncScalar(funcName, parameters);


            return result.ToString();
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

            var currencyExcLookup = currencyExchangeRepo.Get().ToLookup(x => x.DatetimeCreated.Value.Date);

            var currencyExchanges = currencyExcLookup[exchangeDate.Value.Date].ToList();
            if (currencyExchanges.Count == 0)
            {
                DateTime? maxDateCreated = currencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchanges = currencyExcLookup[maxDateCreated.Value.Date].ToList();
            }

            currencyExchanges = currencyExchanges.OrderByDescending(x => x.DatetimeCreated).ToList();
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

        private string CheckChargeDebitExistInvoice(IQueryable<CsShipmentSurcharge> surcharges, string code)
        {
            string message = string.Empty;
            var charge = surcharges.Where(x => x.AcctManagementId != null && x.AcctManagementId != Guid.Empty).FirstOrDefault();
            if (charge != null)
            {
                var invoice = DataContext.Get(x => x.Id == charge.AcctManagementId).FirstOrDefault();
                if (invoice != null)
                {
                    var createdDate = invoice.DatetimeCreated != null ? invoice.DatetimeCreated.Value.ToString("dd/MM/yyyy HH:ss:mm") : string.Empty;
                    message = string.Format("Phiếu chứng từ {0} đã được đồng bộ Hóa đơn {1} vào ngày {2}. Vui lòng kiểm tra đồng bộ Hủy Hóa Đơn trước khi trả phiếu!", code, invoice.InvoiceNoReal, createdDate);
                }
            }
            return message;
        }

        private sp_UpdateRejectCharge UpdateRejectCharge(List<RejectChargeTable> charges)
        {
            var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@Charges",
                    Value = DataHelper.ToDataTable(charges),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[RejectChargeTable]"
                }
            };
            var result = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_UpdateRejectCharge>(parameters);
            return result.FirstOrDefault();
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
            currentUser.Action = "RemoveVoucherAdvance";

            var hsRemoveVoucherAdvance = RemoveVoucherAdvance(voucherNo, currentUser);
            return hsRemoveVoucherAdvance;
        }

        private HandleState RemoveVoucherAdvance(string voucherNo, ICurrentUser _currentUser)
        {
            HandleState result = new HandleState();
            try
            {
                var advs = acctAdvanceRepository.Get(x => x.VoucherNo == voucherNo);
                if (advs != null && advs.Count() > 0)
                {
                    foreach (var adv in advs)
                    {

                        adv.VoucherNo = null;
                        adv.VoucherDate = null;
                        adv.PaymentTerm = null;
                        adv.SyncStatus = ForPartnerConstants.STATUS_REJECTED;

                        adv.UserModified = _currentUser.UserID;
                        adv.DatetimeModified = DateTime.Now;

                        acctAdvanceRepository.Update(adv, x => x.Id == adv.Id, false);
                    }

                    result = acctAdvanceRepository.SubmitChanges();
                }
                else
                {
                    return new HandleState((object)"Không tìm thấy phiếu chi");
                }

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
            currentUser.Action = "UpdateVoucherAdvance";

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
                        return new HandleState((object)"Update Advance fail");
                    }

                    if(model.Detail != null && model.Detail.Count > 0)
                    {
                        foreach (var item in model.Detail)
                        {
                            IQueryable<AcctAdvanceRequest> advReq = acctAdvanceRequestRepository.Get(x => x.AdvanceNo == adv.AdvanceNo && x.JobId == item.JobNo && x.Mbl == item.MBL
                            && (x.Hbl == item.HBL || x.CustomNo == item.HBL)); // do khi đẩy đi vừa là số HBL, vừa là số TK
                            if (advReq != null && advReq.Count() > 0)
                            {
                                foreach (var advR in advReq)
                                {
                                    advR.ReferenceNo = item.ReferenceNo;
                                    var resultAdr = acctAdvanceRequestRepository.Update(advR, x => x.Id == advR.Id, false);
                                }
                            }
                        }
                        HandleState hsUpdateAdvR = acctAdvanceRequestRepository.SubmitChanges();
                        if (!hsUpdateAdvR.Success)
                        {
                            return new HandleState((object)"Update Advance Request fail");
                        }
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
                    currentUser.Action = "RejectDataAdvance";
                    result = RejectAdvance(model.ReferenceID, model.Reason);
                    break;
                case "SETTLEMENT":
                    currentUser.Action = "RejectDataSettlement";
                    result = RejectSettlement(model.ReferenceID, model.Reason);
                    break;
                case "SOA":
                    currentUser.Action = "RejectDataSOA";
                    result = RejectSoa(model.ReferenceID, model.Reason);
                    break;
                case "CDNOTE":
                    currentUser.Action = "RejectDataCDNOTE";
                    result = RejectCdNote(model.ReferenceID, model.Reason);
                    break;
                case "VOUCHER":
                    currentUser.Action = "RejectDataVoucher";
                    result = RejectVoucher(model.ReferenceID, model.Reason);
                    break;
                case "PAYMENT":
                    currentUser.Action = "RejectDataPayment";
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
            var _id = Guid.Empty;
            Guid.TryParse(id, out _id);
            AcctAdvancePayment advance = acctAdvanceRepository.Get(x => x.Id == _id).FirstOrDefault();
            if (advance == null) return new HandleState((object)"Không tìm thấy advance");

            advance.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
            advance.UserModified = currentUser.UserID;
            advance.DatetimeModified = DateTime.Now;
            advance.ReasonReject = reason;

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {                    
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
            var _id = Guid.Empty;
            Guid.TryParse(id, out _id);
            AcctSettlementPayment settlement = acctSettlementRepo.Get(x => x.Id == _id).FirstOrDefault();
            if (settlement == null) return new HandleState((object)"Không tìm thấy settlement");

            settlement.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
            settlement.UserModified = currentUser.UserID;
            settlement.DatetimeModified = DateTime.Now;
            settlement.ReasonReject = reason;

            IQueryable<CsShipmentSurcharge> surcharges = surchargeRepo.Get(x => x.SettlementCode == settlement.SettlementNo);

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
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
                                ActionLink = string.Format(@"home/accounting/settlement-payment/{0}", settlement.Id),
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

                            if(surcharges != null && surcharges.Count() > 0)
                            {
                                foreach (var item in surcharges)
                                {
                                    if (item.Type == ForPartnerConstants.TYPE_CHARGE_OBH)
                                    {
                                        item.PaySyncedFrom = null;
                                    }
                                    if (item.Type == ForPartnerConstants.TYPE_CHARGE_BUY)
                                    {
                                        item.SyncedFrom = null;

                                        // Reset nếu kt đã update UpdateVoucherExpense
                                        item.VoucherId = null;
                                        item.VoucherIddate = null;
                                    }

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
            var soa = acctSOARepository.Get(x => x.Id == id).FirstOrDefault();
            if (soa == null) return new HandleState((object)"Không tìm thấy SOA");

            IQueryable<CsShipmentSurcharge> surcharges = null;
            if (soa.Type == "Credit")
            {
                surcharges = surchargeRepo.Get(x => x.PaySoano == soa.Soano);
            }
            if (soa.Type == "Debit")
            {
                surcharges = surchargeRepo.Get(x => x.Soano == soa.Soano);
                //Check tồn tại phí đã đồng bộ Invoice from Bravo
                var existInvoice = CheckChargeDebitExistInvoice(surcharges, soa.Soano);
                if (!string.IsNullOrEmpty(existInvoice))
                {
                    return new HandleState((object)existInvoice);
                }
            }

            soa.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
            soa.UserModified = currentUser.UserID;
            soa.DatetimeModified = DateTime.Now;
            soa.ReasonReject = reason;

            //Update PaySyncedFrom or SyncedFrom equal NULL by SoaNo (Sử dụng Store Procudure để Update Charge)
            if (surcharges != null)
            {
                var rejectCharges = new List<RejectChargeTable>();
                foreach (var surcharge in surcharges)
                {
                    var rejectCharge = new RejectChargeTable();
                    rejectCharge.Id = surcharge.Id;
                    if (surcharge.Type == "OBH")
                    {
                        rejectCharge.PaySyncedFrom = (soa.Soano == surcharge.PaySoano) ? null : surcharge.PaySyncedFrom;
                        rejectCharge.SyncedFrom = (soa.Soano == surcharge.Soano) ? null : surcharge.SyncedFrom;
                    }
                    else
                    {
                        rejectCharge.SyncedFrom = null;
                    }
                    rejectCharge.UserModified = currentUser.UserID;
                    rejectCharge.DatetimeModified = DateTime.Now;
                    rejectCharges.Add(rejectCharge);
                }

                var updateRejectCharge = UpdateRejectCharge(rejectCharges);
                string logName = string.Format("Reject_SOA_{0}_UpdateCharge", soa.Soano);
                string logMessage = string.Format(" * DataCharge: {0} \n * Result: {1}",
                    JsonConvert.SerializeObject(rejectCharges),
                    JsonConvert.SerializeObject(updateRejectCharge));
                new LogHelper(logName, logMessage);
                if (!updateRejectCharge.Status)
                {
                    return new HandleState((object)updateRejectCharge.Message);
                }
            }

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
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
                            /*if (surcharges != null)
                            {
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
                            }*/
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
            var _id = Guid.Empty;
            Guid.TryParse(id, out _id);
            var cdNote = acctCdNoteRepo.Get(x => x.Id == _id).FirstOrDefault();
            if (cdNote == null) return new HandleState((object)"Không tìm thấy CDNote");

            var surcharges = surchargeRepo.Get(x => (cdNote.Type == "CREDIT" ? x.CreditNo : x.DebitNo) == cdNote.Code);

            if (cdNote.Type != "CREDIT")
            {
                //Check tồn tại phí đã đồng bộ Invoice from Bravo
                var existInvoice = CheckChargeDebitExistInvoice(surcharges, cdNote.Code);
                if (!string.IsNullOrEmpty(existInvoice))
                {
                    return new HandleState((object)existInvoice);
                }
            }

            cdNote.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
            cdNote.UserModified = currentUser.UserID;
            cdNote.DatetimeModified = DateTime.Now;
            cdNote.ReasonReject = reason;
            
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
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
            var _id = Guid.Empty;
            Guid.TryParse(id, out _id);
            var voucher = DataContext.Get(x => x.Id == _id).FirstOrDefault();
            if (voucher == null) return new HandleState((object)"Không tìm thấy voucher");

            voucher.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
            voucher.UserModified = currentUser.UserID;
            voucher.DatetimeModified = DateTime.Now;
            voucher.ReasonReject = reason;

            var surcharges = surchargeRepo.Get(x => (x.Type == ForPartnerConstants.TYPE_CHARGE_OBH ? x.PayerAcctManagementId : x.AcctManagementId) == voucher.Id);

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    
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

                            //Update SyncedFrom/PaySyncedFrom equal NULL by Id of Voucher
                            foreach (var surcharge in surcharges)
                            {
                                if (surcharge.Type == ForPartnerConstants.TYPE_CHARGE_OBH)
                                {
                                    surcharge.PaySyncedFrom = null;
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

        /// <summary>
        /// Reject ReceiptSync
        /// </summary>
        /// <param name="id">id of receiptSync</param>
        /// <param name="reason">reason reject</param>
        /// <returns></returns>
        private HandleState RejectPayment(string id, string reason)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var _id = Guid.Empty;
                    Guid.TryParse(id, out _id);
                    var receiptSync = receiptSyncRepository.Get(x => x.Id == _id).FirstOrDefault();
                    if (receiptSync == null) return new HandleState((object)"Không tìm thấy phiếu thu");
                    var receipt = receiptRepository.Get(x => x.Id == receiptSync.ReceiptId).FirstOrDefault();
                    if (receipt == null) return new HandleState((object)"Không tìm thấy phiếu thu");

                    receiptSync.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
                    receiptSync.UserModified = currentUser.UserID;
                    receiptSync.DatetimeModified = DateTime.Now;
                    receiptSync.ReasonReject = reason;

                    HandleState hs = receiptSyncRepository.Update(receiptSync, x => x.Id == receiptSync.Id, false);
                    if (hs.Success)
                    {
                        HandleState smReceipt = receiptSyncRepository.SubmitChanges();
                        if (smReceipt.Success)
                        {
                            //Update Reject Receipt                            
                            var receiptSyncs = receiptSyncRepository.Get(x => x.ReceiptId == receiptSync.ReceiptId).ToList();
                            var hsUpdateRejectReceipt = UpdateRejectReceipt(receipt, receiptSyncs);

                            //Push Notification ReceiptSync
                            string title = string.Format(@"Accountant Rejected Data Receipt {0}", receiptSync.ReceiptSyncNo);
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
            }
        }

        private HandleState UpdateRejectReceipt(AcctReceipt receipt, List<AcctReceiptSync> receiptSyncs)
        {
            HandleState hs = new HandleState();
            //Các phiếu ReceiptSync của Receipt đã reject hết >> Update status Rejected cho Receipt
            var totalReceiptSync = receiptSyncs.Count();
            var receiptSyncRejected = receiptSyncs.Where(x => x.SyncStatus == ForPartnerConstants.STATUS_REJECTED).Count();
            if (totalReceiptSync == receiptSyncRejected && totalReceiptSync != 0)
            {
                receipt.SyncStatus = ForPartnerConstants.STATUS_REJECTED;
                receipt.UserModified = currentUser.UserID;
                receipt.DatetimeModified = DateTime.Now;
                receipt.ReasonReject = string.Join("; ", receiptSyncs.Select(s => string.Format("{0} ({1})", s.ReasonReject, s.ReceiptSyncNo))); //Cộng dồn Reason của từng ReceiptSync
                hs = receiptRepository.Update(receipt, x => x.Id == receipt.Id);
            }
            return hs;
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
            var _id = Guid.Empty;
            Guid.TryParse(id, out _id);
            var voucher = DataContext.Get(x => x.Id == _id).FirstOrDefault();
            if (voucher == null) return new HandleState((object)"Không tìm thấy voucher");
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    HandleState hs = DataContext.Delete(x => x.Id == voucher.Id, false);
                    if (hs.Success)
                    {
                        var charges = surchargeRepo.Get(x => (x.Type == ForPartnerConstants.TYPE_CHARGE_OBH ? x.PayerAcctManagementId : x.AcctManagementId) == voucher.Id);
                        foreach (CsShipmentSurcharge charge in charges)
                        {                                                       
                            charge.InvoiceNo = null;
                            charge.InvoiceDate = null;
                            charge.SeriesNo = null;
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = currentUser.UserID;
                            if (charge.Type == ForPartnerConstants.TYPE_CHARGE_OBH)
                            {
                                charge.PayerAcctManagementId = null;
                                charge.VoucherIdre = null;
                                charge.VoucherIdredate = null;
                                charge.PaySyncedFrom = null;
                            }
                            else
                            {
                                charge.AcctManagementId = null;
                                charge.VoucherId = null;
                                charge.VoucherIddate = null;
                                charge.SyncedFrom = null;
                            }
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

        private List<ObjectReceivableModel> GetListObjectReceivableBySurchargeIds(List<Guid> surchargeIds)
        {
            var surcharges = surchargeRepo.Get(x => surchargeIds.Any(a => a == x.Id));
            var objPO = from surcharge in surcharges
                        where !string.IsNullOrEmpty(surcharge.PaymentObjectId)
                        select new ObjectReceivableModel { PartnerId = surcharge.PaymentObjectId, Office = surcharge.OfficeId, Service = surcharge.TransactionType };
            var objPR = from surcharge in surcharges
                        where !string.IsNullOrEmpty(surcharge.PayerId)
                        select new ObjectReceivableModel { PartnerId = surcharge.PayerId, Office = surcharge.OfficeId, Service = surcharge.TransactionType };
            var objMerge = objPO.Union(objPR).ToList();
            var objectReceivables = objMerge.GroupBy(g => new { Service = g.Service, PartnerId = g.PartnerId, Office = g.Office })
                .Select(s => new ObjectReceivableModel { PartnerId = s.Key.PartnerId, Service = s.Key.Service, Office = s.Key.Office });
            return objectReceivables.ToList();
        }
        
        public CalculatorReceivableNotAuthorizeModel GetCalculatorReceivableNotAuthorizeModelBySurchargeIds(List<Guid> surchargeIds, string apiKey, string action)
        {
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            CalculatorReceivableNotAuthorizeModel modelReceivable = new CalculatorReceivableNotAuthorizeModel
            {
                UserID = _currentUser.UserID,
                GroupId = _currentUser.GroupId,
                DepartmentId = _currentUser.DepartmentId,
                OfficeID = _currentUser.OfficeID,
                CompanyID = _currentUser.CompanyID,
                Action = action,
                ObjectReceivable = GetListObjectReceivableBySurchargeIds(surchargeIds)
            };
            return modelReceivable;
        }

        public List<Guid> GetSurchargeIdsByRefNoInvoice(string referenceNo)
        {
            var surchargeIds = surchargeRepo.Get(x => x.ReferenceNo == referenceNo).Select(s => s.Id).ToList();
            return surchargeIds;
        }
        
        public HandleState UpdateVoucherExpense(VoucherExpense voucherExpense, string apiKey)
        {
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;
            currentUser.Action = "UpdateVoucherExpense";

            var hsUpdateVoucherExpense = UpdateVoucherExpense(voucherExpense, currentUser);
            return hsUpdateVoucherExpense;
        }

        private HandleState UpdateVoucherExpense(VoucherExpense voucherExpense, ICurrentUser _currentUser)
        {
            var expenses = voucherExpense.Detail;
            var hs = new HandleState();
            try
            {
                //foreach (var expense in expenses)
                //{
                //    var surcharge = surchargeRepo.Get(x => x.Id == expense.RowID).FirstOrDefault();
                //    if (surcharge != null)
                //    {
                //        surcharge.UserModified = _currentUser.UserID;
                //        surcharge.DatetimeModified = DateTime.Now;
                //        if (surcharge.Type == ForPartnerConstants.TYPE_CHARGE_OBH)
                //        {
                //            surcharge.VoucherIdre = expense.VoucherNO;
                //            surcharge.VoucherIdredate = expense.VoucherDate;
                //        }
                //        else
                //        {
                //            surcharge.VoucherId = expense.VoucherNO;
                //            surcharge.VoucherIddate = expense.VoucherDate;
                //        }
                //        hs = surchargeRepo.Update(surcharge, x => x.Id == surcharge.Id);
                //    }
                //}

                StatusModel result = UpdateSurchargeVoucherExpense(expenses);
                if(!result.Status)
                {
                    return new HandleState((object)result.Message);
                }
                return new HandleState(result.Status,"Success");
            }
            catch(Exception ex)
            {
                return new HandleState((object)"Error");
            }
        }

        private StatusModel UpdateSurchargeVoucherExpense(List<VoucherExpenseCharge> listExpense)
        {
            var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@Charges",
                    Value = DataHelper.ToDataTable(listExpense),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[VoucherExpense]"
                }
            };
            var result = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_UpdateVoucherExpense>(parameters);
            return result.FirstOrDefault();
        }
    }
}


