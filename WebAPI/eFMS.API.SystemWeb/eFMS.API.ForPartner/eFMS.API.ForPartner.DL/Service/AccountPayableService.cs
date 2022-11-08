﻿using AutoMapper;
using eFMS.API.Common.Helpers;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Models.Payable;
using eFMS.API.ForPartner.Service.Contexts;
using eFMS.API.ForPartner.Service.Models;
using eFMS.API.ForPartner.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.DL.Service
{

    public class AccountPayableService : RepositoryBase<AccAccountPayable, AccountPayableModel>, IAccountPayableService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatPartner> partnerRepository;
        private readonly IContextBase<SysOffice> officeRepository;
        private readonly IContextBase<AccAccountPayablePayment> paymentRepository;
        private readonly IContextBase<SysPartnerApi> sysPartnerApiRepository;
        private readonly IContextBase<CatCharge> catChargeRepository;
        private readonly IContextBase<AcctSoa> acctSOARepository;
        private readonly IContextBase<AcctCdnote> acctCdNoteRepository;
        private readonly IContextBase<AcctSettlementPayment> settlementPaymentRepository;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        private readonly IContextBase<AcctCreditManagementAr> creditManagementArRepository;
        private readonly IContextBase<AccAccountingManagement> accountingManagementRepository;
        public AccountPayableService(IContextBase<AccAccountPayable> repository,
            IContextBase<CatPartner> partnerRepo,
            ICurrentUser cUser,
            IContextBase<SysOffice> officeRepo,
            IContextBase<AccAccountPayablePayment> paymentRepo,
            IContextBase<SysPartnerApi> sysPartnerApiRepo,
            IContextBase<CatCharge> catChargeRepo,
            IContextBase<AcctSoa> acctSOARepo,
            IContextBase<AcctCdnote> acctCdNoteRepo,
            IContextBase<AcctSettlementPayment> settlementPaymentRepo,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<AcctCreditManagementAr> creditManagementArRepo,
            IContextBase<AccAccountingManagement> accountingManagementRepo,
            IMapper mapper) : base(repository, mapper)
        {
            currentUser = cUser;
            partnerRepository = partnerRepo;
            officeRepository = officeRepo;
            paymentRepository = paymentRepo;
            sysPartnerApiRepository = sysPartnerApiRepo;
            catChargeRepository = catChargeRepo;
            acctSOARepository = acctSOARepo;
            acctCdNoteRepository = acctCdNoteRepo;
            settlementPaymentRepository = settlementPaymentRepo;
            surchargeRepository = surchargeRepo;
            creditManagementArRepository = creditManagementArRepo;
            accountingManagementRepository = accountingManagementRepo;
        }

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

        public async Task<HandleState> InsertAccPayable(VoucherSyncCreateModel model)
        {
            HandleState hsAddPayable = new HandleState();
            List<AccAccountPayable> payables = new List<AccAccountPayable>();
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    CatPartner customer = partnerRepository.Get(x => x.AccountNo == model.CustomerCode)?.FirstOrDefault();
                    SysOffice office = officeRepository.Get(x => x.Code == model.OfficeCode)?.FirstOrDefault();
                    var paymentMethod = model.PaymentMethod.ToLower().Contains("net") ? ForPartnerConstants.PAYMENT_METHOD_NETOFF : model.PaymentMethod;
                    var paymentStatus = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;

                    var voucherDetail = model.Details.Where(x => x.TransactionType != "NONE"
                                            && x.TransactionType != ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CLEAR_ADV
                                            && x.TransactionType != ForPartnerConstants.TRANSACTION_TYPE_BALANCE
                                            && x.TransactionType != ForPartnerConstants.TYPE_DEBIT
                                            && x.JobNo != ForPartnerConstants.TRANSACTION_TYPE_BALANCE).ToList();
                    voucherDetail.ToList().ForEach(x => x.TransactionType = x.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT) ? ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT : x.TransactionType);
                    var billingNo = GetBillingNameFromId(model.DocID, model.DocCode, model.DocType);
                    // Update TransactionType without charge mode

                    // Get surcharges info => comment use case hd có tiền âm
                    //var listCharges = model.Details.Select(x => x.ChargeId).ToList();
                    //var sucharges = surchargeRepository.Get(x => listCharges.Any(chg => chg == x.Id));

                    if (paymentMethod.ToLower() == ForPartnerConstants.PAYMENT_METHOD_BANK.ToLower()
                        || paymentMethod.ToLower() == ForPartnerConstants.PAYMENT_METHOD_CASH.ToLower())
                    {
                        if (voucherDetail.Any(z => string.IsNullOrEmpty(z.BravoRefNo)))
                        {
                            // Bank Transfer/Cash và không có số ref sẽ ghi nhận công nợ => Paid
                            var noneRefNoVoucherDetail = voucherDetail.Where(z => string.IsNullOrEmpty(z.BravoRefNo));
                            var grpVoucherDetail = noneRefNoVoucherDetail.GroupBy(z => new { z.VoucherNo, z.VoucherDate }).Select(z => z).ToList();
                            grpVoucherDetail.ForEach(c =>
                            {
                                //var giamValue = sucharges.Where(x => x.Id == c.FirstOrDefault().ChargeId).FirstOrDefault().UnitPrice < 0 ? (-1) : 1;
                                AccAccountPayable payable = new AccAccountPayable
                                {
                                    Id = Guid.NewGuid(),
                                    Currency = c.FirstOrDefault().Currency,
                                    PartnerId = customer?.Id,
                                    PaymentAmount = 0,
                                    PaymentAmountVnd = 0,
                                    PaymentAmountUsd = 0,
                                    RemainAmount = 0,
                                    RemainAmountVnd = 0,
                                    RemainAmountUsd = 0,
                                    TotalAmountVnd = c.Sum(pa => pa.VatAmountVnd + pa.AmountVnd), // * giamValue,
                                    TotalAmountUsd = c.Sum(pa => pa.VatAmountUsd + pa.AmountUsd), // * giamValue,
                                    ReferenceNo = c.FirstOrDefault().BravoRefNo,
                                    Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID,
                                    CompanyId = currentUser.CompanyID,
                                    OfficeId = office.Id,
                                    GroupId = currentUser.GroupId,
                                    DepartmentId = currentUser.DepartmentId,
                                    TransactionType = c.FirstOrDefault().TransactionType,
                                    VoucherNo = c.FirstOrDefault().VoucherNo,
                                    BillingNo = billingNo,
                                    BillingType = model.DocType,
                                    ExchangeRate = c.FirstOrDefault().ExchangeRate,
                                    PaymentTerm = c.FirstOrDefault().PaymentTerm,
                                    VoucherDate = c.FirstOrDefault().VoucherDate,
                                    PaymentDueDate = c.FirstOrDefault().VoucherDate?.AddDays((double)(c.FirstOrDefault().PaymentTerm)),
                                    InvoiceNo = c.FirstOrDefault().InvoiceNo,
                                    InvoiceDate = c.FirstOrDefault().InvoiceDate,
                                    Over16To30Day = 0,
                                    Over1To15Day = 0,
                                    Over30Day = 0,
                                    UserCreated = currentUser.UserID,
                                    DatetimeCreated = DateTime.Now,
                                    UserModified = currentUser.UserID,
                                    DatetimeModified = DateTime.Now,
                                    Description = c.FirstOrDefault().Description
                                };
                                payables.Add(payable);
                            });
                            foreach (var item in payables)
                            {
                                item.TotalAmount = item.PaymentAmount = item.Currency == ForPartnerConstants.CURRENCY_LOCAL ? item.TotalAmountVnd : item.TotalAmountUsd;

                                item.PaymentAmountVnd = item.TotalAmountVnd;
                                item.PaymentAmountUsd = item.TotalAmountUsd;
                                await DataContext.AddAsync(item, false);
                            }
                        }
                        //else => TH có số bravo no
                        {
                            List<AccAccountPayable> payablesWithRefNo = new List<AccAccountPayable>();
                            var grpVoucherDetail = voucherDetail.Where(z => !string.IsNullOrEmpty(z.BravoRefNo))
                                                                .GroupBy(x => new { x.VoucherNo, x.TransactionType, model.DocType, model.DocCode, x.BravoRefNo })
                                                                .Select(s => s).ToList();
                            grpVoucherDetail.ForEach(c =>
                            {
                                AccAccountPayable payable = new AccAccountPayable
                                {
                                    Id = Guid.NewGuid(),
                                    Currency = c.FirstOrDefault().Currency,
                                    PartnerId = customer?.Id,
                                    PaymentAmount = 0,
                                    PaymentAmountVnd = 0,
                                    PaymentAmountUsd = 0,
                                    RemainAmount = 0,
                                    RemainAmountVnd = 0,
                                    RemainAmountUsd = 0,
                                    TotalAmountVnd = c.Sum(pa => pa.VatAmountVnd + pa.AmountVnd),
                                    TotalAmountUsd = c.Sum(pa => pa.VatAmountUsd + pa.AmountUsd),
                                    ReferenceNo = c.FirstOrDefault().BravoRefNo,
                                    Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID,
                                    CompanyId = currentUser.CompanyID,
                                    OfficeId = office.Id,
                                    GroupId = currentUser.GroupId,
                                    DepartmentId = currentUser.DepartmentId,
                                    TransactionType = c.FirstOrDefault().TransactionType,
                                    VoucherNo = c.FirstOrDefault().VoucherNo,
                                    BillingNo = billingNo,
                                    BillingType = model.DocType,
                                    ExchangeRate = c.FirstOrDefault().ExchangeRate,
                                    PaymentTerm = c.FirstOrDefault().PaymentTerm,
                                    VoucherDate = c.FirstOrDefault().VoucherDate,
                                    PaymentDueDate = c.FirstOrDefault().VoucherDate?.AddDays((double)(c.FirstOrDefault().PaymentTerm)),
                                    InvoiceNo = c.FirstOrDefault().InvoiceNo,
                                    InvoiceDate = c.FirstOrDefault().InvoiceDate,
                                    Over16To30Day = 0,
                                    Over1To15Day = 0,
                                    Over30Day = 0,
                                    UserCreated = currentUser.UserID,
                                    DatetimeCreated = DateTime.Now,
                                    UserModified = currentUser.UserID,
                                    DatetimeModified = DateTime.Now,
                                    Description = c.FirstOrDefault().Description
                                };

                                payablesWithRefNo.Add(payable);
                            });
                            foreach (var item in payablesWithRefNo)
                            {
                                item.TotalAmount = item.Currency == ForPartnerConstants.CURRENCY_LOCAL ? item.TotalAmountVnd : item.TotalAmountUsd;
                                item.RemainAmount = item.TotalAmount;
                                item.RemainAmountVnd = item.TotalAmountVnd;
                                item.RemainAmountUsd = item.TotalAmountUsd;

                                await DataContext.AddAsync(item, false);
                            }
                        }
                    }
                    else if (paymentMethod.ToLower() == ForPartnerConstants.PAYMENT_METHOD_OTHER.ToLower()
                            || paymentMethod.ToLower() == ForPartnerConstants.PAYMENT_METHOD_NETOFF.ToLower())
                    {
                        var invChargeIds = new List<Guid>();
                        if (customer.PartnerMode == ForPartnerConstants.PARTNER_MODE_INTERNAL)
                        {
                            invChargeIds = catChargeRepository.Get(x => x.Mode == ForPartnerConstants.CHARGE_MODE_NINV).Select(x => x.Id).ToList();
                        }
                        var grpVoucherDetail = voucherDetail.Where(z => !invChargeIds.Any(chg => chg == z.ChargeId)).GroupBy(z => customer.PartnerMode == ForPartnerConstants.PARTNER_MODE_INTERNAL ? new { z.VoucherNo, z.VoucherDate, z.BravoRefNo } : new { VoucherNo = string.Empty, VoucherDate = (DateTime?)null, z.BravoRefNo }).Select(z => z).ToList();
                        grpVoucherDetail.ForEach(c =>
                        {
                            AccAccountPayable payable = new AccAccountPayable
                            {
                                Id = Guid.NewGuid(),
                                Currency = c.FirstOrDefault().Currency,
                                PartnerId = customer?.Id,
                                PaymentAmount = 0,
                                PaymentAmountVnd = 0,
                                PaymentAmountUsd = 0,
                                RemainAmount = 0,
                                RemainAmountVnd = 0,
                                RemainAmountUsd = 0,
                                TotalAmountVnd = c.Sum(pa => pa.VatAmountVnd + pa.AmountVnd),
                                TotalAmountUsd = c.Sum(pa => pa.VatAmountUsd + pa.AmountUsd),
                                ReferenceNo = c.FirstOrDefault().BravoRefNo,
                                Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID,
                                CompanyId = currentUser.CompanyID,
                                OfficeId = office.Id,
                                GroupId = currentUser.GroupId,
                                DepartmentId = currentUser.DepartmentId,
                                TransactionType = c.FirstOrDefault().TransactionType,
                                VoucherNo = c.FirstOrDefault().VoucherNo,
                                BillingNo = billingNo,
                                BillingType = model.DocType,
                                ExchangeRate = c.FirstOrDefault().ExchangeRate,
                                PaymentTerm = c.FirstOrDefault().PaymentTerm,
                                VoucherDate = c.FirstOrDefault().VoucherDate,
                                PaymentDueDate = c.FirstOrDefault().VoucherDate?.AddDays((double)(c.FirstOrDefault().PaymentTerm)),
                                InvoiceNo = c.FirstOrDefault().InvoiceNo,
                                InvoiceDate = c.FirstOrDefault().InvoiceDate,
                                Over16To30Day = 0,
                                Over1To15Day = 0,
                                Over30Day = 0,
                                UserCreated = currentUser.UserID,
                                DatetimeCreated = DateTime.Now,
                                UserModified = currentUser.UserID,
                                DatetimeModified = DateTime.Now,
                                Description = c.FirstOrDefault().Description
                            };
                            payables.Add(payable);
                        });
                        foreach (var item in payables)
                        {
                            item.TotalAmount = item.Currency == ForPartnerConstants.CURRENCY_LOCAL ? item.TotalAmountVnd : item.TotalAmountUsd;
                            item.RemainAmount = item.TotalAmount; ;
                            item.RemainAmountVnd = item.TotalAmountVnd;
                            item.RemainAmountUsd = item.TotalAmountUsd;

                            await DataContext.AddAsync(item, false);
                        }
                    }
                    hsAddPayable = DataContext.SubmitChanges();
                    if (hsAddPayable.Success)
                    {
                        trans.Commit();
                    }
                    return hsAddPayable;
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
        /// Add Credit Mangagement
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<HandleState> AddCreditMangagement(VoucherSyncCreateModel model)
        {
            HandleState hsUpdate = new HandleState();
            using (var trans = creditManagementArRepository.DC.Database.BeginTransaction())
            {
                try
                {
                    var detailBravoRefNos = model.Details.Where(x => !string.IsNullOrEmpty(x.BravoRefNo)).ToList();
                    if (detailBravoRefNos.Count == 0)
                    {
                        return hsUpdate;
                    }
                    var partner = partnerRepository.Get(x => x.AccountNo == model.CustomerCode).FirstOrDefault();
                    SysOffice office = officeRepository.Get(x => x.Code == model.OfficeCode).FirstOrDefault();

                    //var creditManagements = creditManagementArRepository.Get(x => x.PartnerId == partner.Id && x.OfficeId == office.Id.ToString());
                    var shipments = detailBravoRefNos.GroupBy(x => new { x.JobNo, x.MblNo, x.Hblno, x.BravoRefNo });
                    //var listChargeIds = model.Details.Select(x => x.ChargeId).ToList();
                    //var surcharges = surchargeRepository.Get(x => listChargeIds.Contains(x.Id));
                    var _code = string.Empty;
                    var _type = string.Empty;
                    decimal? exchangeRateUsd = 0;
                    if (model.DocType == "SOA" || model.DocType == "CDNOTE")
                    {
                        _code = model.DocCode;
                        _type = model.DocType == "SOA" ? "CREDITSOA" : "CREDITNOTE";
                        exchangeRateUsd = model.DocType == "SOA" ? acctSOARepository.Get(x => x.Soano == _code).FirstOrDefault()?.ExcRateUsdToLocal : acctCdNoteRepository.Get(x => x.Code == _code).FirstOrDefault()?.ExcRateUsdToLocal;
                    }
                    foreach (var item in shipments)
                    {
                        // Get detail to update Credit AR
                        var acctCredit = new AcctCreditManagementAr();
                        var surcharges = surchargeRepository.Where(x => x.JobNo == item.Key.JobNo && x.Mblno == item.Key.MblNo && x.Hblno == item.Key.Hblno);
                        var detailBilling = surcharges.FirstOrDefault();
                        //.Select(x => new { Code = string.IsNullOrEmpty(x.CreditNo) ? x.PaySoano : x.CreditNo, Type = string.IsNullOrEmpty(x.CreditNo) ? "CREDITSOA" : "CREDITNOTE", x.Hblid }).FirstOrDefault();
                        var syncFrom = string.Empty;
                        if (string.IsNullOrEmpty(_code))
                        {
                            _type = detailBilling.Type == "OBH" ? (detailBilling.PaySyncedFrom.Contains("SOA") ? "CREDITSOA" : "CREDITNOTE") : (detailBilling.SyncedFrom.Contains("SOA") ? "CREDITSOA" : "CREDITNOTE");
                            _code = _type == "CREDITSOA" ? detailBilling.PaySoano : detailBilling.CreditNo;
                        }
                        decimal totalVnd = 0, totalUsd = 0;
                        if (_type == "CREDITSOA")
                        {
                            totalVnd = surcharges.Where(x => x.PaySoano == _code).Sum(x => (x.AmountVnd ?? 0) + (x.VatAmountVnd ?? 0));
                            totalUsd = surcharges.Where(x => x.PaySoano == _code).Sum(x => (x.AmountUsd ?? 0) + (x.VatAmountUsd ?? 0));
                        }
                        else if (_type == "CREDITNOTE")
                        {
                            totalVnd = surcharges.Where(x => x.CreditNo == _code).Sum(x => (x.AmountVnd ?? 0) + (x.VatAmountVnd ?? 0));
                            totalUsd = surcharges.Where(x => x.CreditNo == _code).Sum(x => (x.AmountUsd ?? 0) + (x.VatAmountUsd ?? 0));
                        }
                        acctCredit.Id = Guid.NewGuid();
                        acctCredit.Code = _code;
                        acctCredit.Type = _type;
                        acctCredit.PartnerId = partner.Id;
                        acctCredit.JobNo = item.Key.JobNo;
                        acctCredit.Mblno = item.Key.MblNo;
                        acctCredit.Hblno = item.Key.Hblno;
                        acctCredit.Hblid = detailBilling.Hblid;
                        acctCredit.SurchargeId = string.Join(';', item.Select(x => x.ChargeId).Distinct());
                        acctCredit.Currency = item.FirstOrDefault().Currency;
                        acctCredit.ExchangeRate = item.FirstOrDefault().ExchangeRate;
                        acctCredit.ExchangeRateUsdToLocal = exchangeRateUsd;
                        acctCredit.AmountVnd = acctCredit.RemainVnd = item.Sum(x => x.AmountVnd + x.VatAmountVnd);
                        acctCredit.AmountUsd = acctCredit.RemainUsd = item.Sum(x => x.AmountUsd + x.VatAmountUsd);
                        if (acctCredit.Currency == ForPartnerConstants.CURRENCY_LOCAL)
                        {
                            acctCredit.AmountUsd = acctCredit.RemainUsd = totalUsd;
                        }
                        else
                        {
                            acctCredit.AmountVnd = acctCredit.RemainVnd = totalVnd;
                        }
                        acctCredit.CompanyId = currentUser.CompanyID;
                        acctCredit.OfficeId = office.Id.ToString();
                        acctCredit.DepartmentId = currentUser.DepartmentId;
                        acctCredit.DatetimeCreated = acctCredit.DatetimeModified = DateTime.Now;
                        acctCredit.UserCreated = acctCredit.UserModified = currentUser.UserID;
                        acctCredit.NetOff = false;
                        acctCredit.ReferenceNo = item.Key.BravoRefNo;
                        acctCredit.Source = "Bravo";
                        hsUpdate = await creditManagementArRepository.AddAsync(acctCredit);
                        if (!hsUpdate.Success)
                        {
                            new LogHelper("AddCreditMangagement_Fail", JsonConvert.SerializeObject(acctCredit));
                        }
                        else
                        {
                            trans.Commit();
                        }
                    }
                    return hsUpdate;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("AddCreditMangagement_Exception", JsonConvert.SerializeObject(model));
                    return new HandleState((object)ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public bool IsPayableHasPayment(VoucherSyncDeleteModel model)
        {

            bool IshasPayment = false;
            var office = officeRepository.Get(x => x.Code == model.OfficeCode).FirstOrDefault();
            List<AccAccountPayable> payable = DataContext.Get(x => x.VoucherNo == model.VoucherNo
            && x.BillingNo == model.DocCode
            && x.BillingType == model.DocType
            && x.OfficeId == office.Id).ToList();
            if (payable.Count > 0)
            {
                IshasPayment = paymentRepository.Any(x => payable.Any(pa => pa.ReferenceNo == x.ReferenceNo && (pa.TransactionType != ForPartnerConstants.PAYABLE_PAYMENT_TYPE_ADV ? pa.TransactionType == x.PaymentType : (x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT || x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_NETOFF))));
            }

            return IshasPayment;
        }

        /// <summary>
        /// Get billing name from billing id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string GetBillingNameFromId(Guid docID, string docCode, string docType)
        {
            string _billingNo = string.Empty;
            switch (docType)
            {
                case "SOA":
                    _billingNo = acctSOARepository.Get(x => x.Id.ToString() == docID.ToString())?.FirstOrDefault()?.Soano;
                    break;
                case "CDNOTE":
                    _billingNo = acctCdNoteRepository.Get(x => x.Id == docID)?.FirstOrDefault()?.Code;
                    break;
                case "SETTLEMENT":
                    _billingNo = settlementPaymentRepository.Get(x => x.Id == docID)?.FirstOrDefault()?.SettlementNo;
                    break;
                default:
                    break;
            }
            return _billingNo;
        }

        /// <summary>
        /// Check data input
        /// </summary>
        /// <param name="accountPayables"></param>
        /// <returns></returns>
        public string CheckIsValidPayable(List<AccAccountPayableModel> accountPayables)
        {
            if (accountPayables.Count == 0)
            {
                return "Không có dữ liệu insert.";
            }
            var officeCodes = accountPayables.Select(x => x.OfficeCode).ToList();
            foreach (var office in officeCodes)
            {
                var invalidOffice = officeRepository.Get(x => x.Code == office).FirstOrDefault();
                if (invalidOffice == null)
                {
                    return string.Format("Office {0} không tồn tại", office);
                }
            }
            var customerCodes = accountPayables.Select(x => x.CustomerCode).ToList();
            foreach (var customer in customerCodes)
            {
                var invalidCustomer = partnerRepository.Get(x => x.AccountNo == customer && x.Active == true).FirstOrDefault();
                if (invalidCustomer == null)
                {
                    return string.Format("Customer {0} không tồn tại", customer);
                }
            }
            foreach (var acc in accountPayables)
            {
                if (string.IsNullOrEmpty(acc.PaymentNo?.Trim()))
                {
                    return string.Format("Số phiếu chứng từ không hợp lệ");
                }
                if (acc.Details.Any(x => string.IsNullOrEmpty(x.AcctId)))
                {
                    return string.Format("Mã định danh của {0} đang trống.", acc.PaymentNo);
                }
                foreach (var detail in acc.Details)
                {
                    if (!detail.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT) && detail.TransactionType != ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_OBH && detail.TransactionType != ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_ADV && !detail.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_COMBINE))
                    {
                        return string.Format("Loại giao dịch {0} không hợp lệ", detail.TransactionType);
                    }
                    if (detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_COMBINE)
                    {
                        if (string.IsNullOrEmpty(detail.BravoRefNo?.Trim()) || string.IsNullOrEmpty(detail.AdvRefNo?.Trim()))
                        {
                            return string.Format("Mã giao dịch giảm Công nợ ứng trươc và Phải Trả không thể trống Phiếu {0}", acc.PaymentNo);
                        }
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Ghi nhận công nợ giảm trừ
        /// </summary>
        /// <param name="accountPayables"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public async Task<HandleState> InsertAccountPayablePayment(List<AccAccountPayableModel> accountPayables, string apiKey)
        {
            var hsPayable = new HandleState();
            var hsPayablePM = new HandleState();
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.CompanyID = _currentUser.CompanyID;
            currentUser.Action = "InsertAccountPayable";
            var listInsertPayment = new List<AccAccountPayablePayment>();
            var listInsertPayable = new List<AccAccountPayable>();

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var acc in accountPayables)
                    {
                        var partner = partnerRepository.Get(x => x.AccountNo == acc.CustomerCode).FirstOrDefault();
                        var office = officeRepository.Get(x => x.Code == acc.OfficeCode).FirstOrDefault();
                        var acctMngIds = accountingManagementRepository.Get(x => x.VoucherId == acc.PaymentNo && x.Date.Value.Date == acc.PaymentDate.Date && x.PartnerId == partner.Id).Select(x => x.Id).ToList();

                        var detailGroup = acc.Details.GroupBy(x => new { VoucherNo = string.IsNullOrEmpty(x.VoucherNo) ? null : x.VoucherNo, x.TransactionType, x.BravoRefNo, x.AdvRefNo, x.AcctId, x.Currency, x.ExchangeRate });
                        foreach (var acctPM in detailGroup)
                        {
                            var detail = acctPM.Key;
                            // Transaction type = OBH sẽ tính như CREDIT
                            if (detail.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT) || detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_OBH)
                            {
                                #region CREDIT
                                // Check existed transaction credit
                                var payableExisted = DataContext.Get(x => (x.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT) || x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_OBH) && (x.ReferenceNo == detail.BravoRefNo) && x.OfficeId == office.Id).FirstOrDefault();
                                if (payableExisted == null)
                                {
                                    return new HandleState((object)string.Format("Chứng từ {0} và số ref {1} chưa ghi nhận.", acc.PaymentNo, detail.BravoRefNo));
                                }
                                // Check trùng mã định danh [acctId] trong lịch sử ghi nhận
                                var payablePayment = paymentRepository.Get(x => x.AcctId == detail.AcctId && x.PaymentType == detail.TransactionType && x.OfficeId == office.Id).FirstOrDefault();
                                if (payablePayment != null)
                                {
                                    return new HandleState((object)string.Format("Đã tồn tại ghi nhận CT {0} và ref {1}.", acc.PaymentNo, detail.AcctId));
                                }
                                var accPayablePayment = new AccAccountPayablePayment();
                                var creditPos = payableExisted.TotalAmount < 0 ? (-1) : 1; // Xét credit âm
                                accPayablePayment.Id = Guid.NewGuid();
                                accPayablePayment.PartnerId = partner.Id;
                                accPayablePayment.PaymentNo = acc.PaymentNo;
                                accPayablePayment.ReferenceNo = detail.BravoRefNo;
                                accPayablePayment.AcctId = detail.AcctId;
                                accPayablePayment.PaymentType = detail.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT) ? ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT : detail.TransactionType;
                                accPayablePayment.Currency = detail.Currency;
                                accPayablePayment.ExchangeRate = detail.ExchangeRate;
                                accPayablePayment.Status = payableExisted.Status;
                                accPayablePayment.PaymentMethod = acc.PaymentMethod;
                                accPayablePayment.PaymentDate = acc.PaymentDate;

                                if (acctPM.Sum(x => x.PayOriginAmount) != 0 && acctPM.Sum(x => x.RemainOriginAmount) != 0)
                                {
                                    accPayablePayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                }
                                else if (acctPM.Sum(x => x.RemainOriginAmount) == 0)
                                {
                                    accPayablePayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                }

                                accPayablePayment.PaymentAmount = acctPM.Sum(x => x.PayOriginAmount) != 0 ? (Math.Abs(acctPM.Sum(x => x.PayOriginAmount)) * creditPos) : acctPM.Sum(x => x.PayOriginAmount);
                                accPayablePayment.PaymentAmountVnd = acctPM.Sum(x => x.PayAmountVND) != 0 ? (Math.Abs(acctPM.Sum(x => x.PayAmountVND)) * creditPos) : acctPM.Sum(x => x.PayAmountVND);
                                accPayablePayment.PaymentAmountUsd = acctPM.Sum(x => x.PayAmountUSD) != 0 ? (Math.Abs(acctPM.Sum(x => x.PayAmountUSD)) * creditPos) : acctPM.Sum(x => x.PayAmountUSD);

                                accPayablePayment.RemainAmount = acctPM.Sum(x => x.RemainOriginAmount) != 0 ? (Math.Abs(acctPM.Sum(x => x.RemainOriginAmount)) * creditPos) : acctPM.Sum(x => x.RemainOriginAmount);
                                accPayablePayment.RemainAmountVnd = acctPM.Sum(x => x.RemainAmountVND) != 0 ? (Math.Abs(acctPM.Sum(x => x.RemainAmountVND)) * creditPos) : acctPM.Sum(x => x.RemainAmountVND);
                                accPayablePayment.RemainAmountUsd = acctPM.Sum(x => x.RemainAmountUSD) != 0 ? (Math.Abs(acctPM.Sum(x => x.RemainAmountUSD)) * creditPos) : acctPM.Sum(x => x.RemainAmountUSD);

                                accPayablePayment.CompanyId = currentUser.CompanyID;
                                accPayablePayment.OfficeId = office.Id;
                                accPayablePayment.DepartmentId = currentUser.DepartmentId;
                                accPayablePayment.GroupId = currentUser.GroupId;
                                accPayablePayment.UserCreated = accPayablePayment.UserModified = currentUser.UserID;
                                accPayablePayment.DatetimeCreated = accPayablePayment.DatetimeModified = DateTime.Now;

                                // Update paid amount payable table
                                payableExisted.PaymentAmount = (payableExisted.PaymentAmount ?? 0) + accPayablePayment.PaymentAmount;
                                payableExisted.PaymentAmountVnd = (payableExisted.PaymentAmountVnd ?? 0) + accPayablePayment.PaymentAmountVnd;
                                payableExisted.PaymentAmountUsd = (payableExisted.PaymentAmountUsd ?? 0) + accPayablePayment.PaymentAmountUsd;
                                payableExisted.RemainAmount = payableExisted.TotalAmount - payableExisted.PaymentAmount;
                                if (payableExisted.Currency == ForPartnerConstants.CURRENCY_LOCAL)
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
                                    payableExisted.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                }
                                else if (payableExisted.RemainAmount == 0)
                                {
                                    payableExisted.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                }
                                payableExisted.DatetimeModified = DateTime.Now;
                                payableExisted.UserModified = currentUser.UserID;
                                #endregion
                                listInsertPayment.Add(accPayablePayment);
                                await DataContext.UpdateAsync(payableExisted, x => x.Id == payableExisted.Id);
                            }
                            else if (detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_ADV)
                            {
                                #region ADV
                                if (string.IsNullOrEmpty(detail.AdvRefNo?.Trim()))
                                {
                                    return new HandleState((object)string.Format("Dữ liệu chứng từ {0} loại ADV chưa có ref no.", acc.PaymentNo));
                                }
                                // Check existed adv refno payment type adv
                                var payablePayment = paymentRepository.Get(x => x.AcctId == detail.AcctId && x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_ADV && x.OfficeId == office.Id).FirstOrDefault();
                                if (payablePayment != null)
                                {
                                    return new HandleState((object)string.Format("Đã tồn tại ghi nhận CT {0} và ref {1}.", acc.PaymentNo, detail.AdvRefNo));
                                }
                                // phát sinh dòng công nợ ứng trước và dòng ghi nhận có status Unpaid
                                var accPayablePayment = new AccAccountPayablePayment();
                                accPayablePayment.Id = Guid.NewGuid();
                                accPayablePayment.PartnerId = partner.Id;
                                accPayablePayment.PaymentNo = acc.PaymentNo;
                                accPayablePayment.ReferenceNo = detail.AdvRefNo;
                                accPayablePayment.AcctId = detail.AcctId;
                                accPayablePayment.PaymentType = ForPartnerConstants.PAYABLE_PAYMENT_TYPE_ADV;
                                accPayablePayment.Currency = detail.Currency;
                                accPayablePayment.ExchangeRate = detail.ExchangeRate;
                                accPayablePayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                                accPayablePayment.PaymentMethod = acc.PaymentMethod;
                                accPayablePayment.PaymentDate = acc.PaymentDate;
                                //if (detail.PayOriginAmount != 0 && detail.RemainOriginAmount != 0)
                                //{
                                //    accPayablePayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                //}
                                //else if (detail.RemainOriginAmount == 0)
                                //{
                                //    accPayablePayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                //}

                                accPayablePayment.PaymentAmount = acctPM.Sum(x => x.PayOriginAmount);
                                accPayablePayment.PaymentAmountVnd = acctPM.Sum(x => x.PayAmountVND);
                                accPayablePayment.PaymentAmountUsd = acctPM.Sum(x => x.PayAmountUSD);

                                accPayablePayment.RemainAmount = acctPM.Sum(x => x.RemainOriginAmount);
                                accPayablePayment.RemainAmountVnd = acctPM.Sum(x => x.RemainAmountVND);
                                accPayablePayment.RemainAmountUsd = acctPM.Sum(x => x.RemainAmountUSD);

                                accPayablePayment.CompanyId = currentUser.CompanyID;
                                accPayablePayment.OfficeId = office.Id;
                                accPayablePayment.DepartmentId = currentUser.DepartmentId;
                                accPayablePayment.GroupId = currentUser.GroupId;
                                accPayablePayment.UserCreated = accPayablePayment.UserModified = currentUser.UserID;
                                accPayablePayment.DatetimeCreated = accPayablePayment.DatetimeModified = DateTime.Now;

                                var payableBillingExited = DataContext.Get(x => (x.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT) || x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_OBH) && x.PartnerId == partner.Id && x.VoucherNo == (string.IsNullOrEmpty(detail.VoucherNo) ? acc.PaymentNo : detail.VoucherNo) && x.OfficeId == office.Id).FirstOrDefault();
                                AccAccountPayable payable = new AccAccountPayable
                                {
                                    Id = Guid.NewGuid(),
                                    Currency = detail.Currency,
                                    PartnerId = partner.Id,
                                    PaymentAmount = 0,
                                    PaymentAmountVnd = 0,
                                    PaymentAmountUsd = 0,
                                    RemainAmount = acctPM.Sum(x => x.PayOriginAmount) + acctPM.Sum(x => x.RemainOriginAmount),
                                    RemainAmountVnd = acctPM.Sum(x => x.PayAmountVND) + acctPM.Sum(x => x.RemainAmountVND),
                                    RemainAmountUsd = acctPM.Sum(x => x.PayAmountUSD) + acctPM.Sum(x => x.RemainAmountUSD),
                                    TotalAmount = acctPM.Sum(x => x.PayOriginAmount) + acctPM.Sum(x => x.RemainOriginAmount),
                                    TotalAmountVnd = acctPM.Sum(x => x.PayAmountVND) + acctPM.Sum(x => x.RemainAmountVND),
                                    TotalAmountUsd = acctPM.Sum(x => x.PayAmountUSD) + acctPM.Sum(x => x.RemainAmountUSD),
                                    ReferenceNo = detail.AdvRefNo,
                                    Status = accPayablePayment.Status,
                                    CompanyId = currentUser.CompanyID,
                                    OfficeId = office.Id,
                                    GroupId = currentUser.GroupId,
                                    DepartmentId = currentUser.DepartmentId,
                                    TransactionType = ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_ADV,
                                    VoucherNo = string.IsNullOrEmpty(detail.VoucherNo) ? acc.PaymentNo : detail.VoucherNo,
                                    InvoiceNo = null,
                                    InvoiceDate = null,
                                    BillingNo = payableBillingExited?.BillingNo,
                                    BillingType = payableBillingExited?.BillingType,
                                    ExchangeRate = detail.ExchangeRate,
                                    PaymentTerm = 0,
                                    VoucherDate = payableBillingExited == null ? acc.PaymentDate : payableBillingExited.VoucherDate,
                                    PaymentDueDate = null,
                                    Over16To30Day = 0,
                                    Over1To15Day = 0,
                                    Over30Day = 0,
                                    UserCreated = currentUser.UserID,
                                    DatetimeCreated = DateTime.Now,
                                    UserModified = currentUser.UserID,
                                    DatetimeModified = DateTime.Now,
                                    Description = null
                                };
                                #endregion
                                listInsertPayment.Add(accPayablePayment);
                                listInsertPayable.Add(payable);
                            }
                            else if (detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_COMBINE || detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CR_COMBINE)
                            {
                                #region COMBINE && CRCOMBINE
                                // Check existed transaction credit
                                var payableCreditExisted = DataContext.Get(x => (x.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT) || x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_OBH) && x.ReferenceNo == detail.BravoRefNo && x.OfficeId == office.Id).FirstOrDefault();
                                if (payableCreditExisted == null)
                                {
                                    return new HandleState((object)string.Format("Chứng từ {0} và số ref {1} chưa ghi nhận.", acc.PaymentNo, detail.BravoRefNo));
                                }
                                // Check existed transaction advance
                                var payableAdvExisted = new AccAccountPayable();
                                if (detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CR_COMBINE) // CRCOMBINE
                                {
                                    payableAdvExisted = DataContext.Get(x => (x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT || x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_OBH) && x.ReferenceNo == detail.AdvRefNo && x.OfficeId == office.Id).FirstOrDefault();
                                }
                                else
                                {
                                    payableAdvExisted = DataContext.Get(x => x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_ADV && x.ReferenceNo == detail.AdvRefNo && x.OfficeId == office.Id).FirstOrDefault();
                                }
                                if (payableAdvExisted == null)
                                {
                                    return new HandleState((object)string.Format("Chứng từ {0} và số ref {1} chưa ghi nhận.", acc.PaymentNo, detail.AdvRefNo));
                                }

                                // Check existed credit refno payment
                                var payablePaymentCreditExisted = paymentRepository.Get(x => x.AcctId == detail.AcctId && (x.PaymentType.Contains(ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT) || x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_OBH) && x.OfficeId == office.Id).FirstOrDefault();
                                if (payablePaymentCreditExisted != null)
                                {
                                    return new HandleState((object)string.Format("Đã tồn tại ghi nhận CT {0} và type credit - ref {1}.", acc.PaymentNo, detail.AcctId));
                                }
                                // Check existed adv refno payment CRCOMBINE=>type credit, COMBINE=>type netoff
                                var payablePaymentAdvExisted = new AccAccountPayablePayment();
                                if (detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CR_COMBINE) // CRCOMBINE
                                {
                                    payablePaymentAdvExisted = paymentRepository.Get(x => x.AcctId == detail.AcctId && x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT && x.OfficeId == office.Id).FirstOrDefault();
                                }
                                else
                                {
                                    payablePaymentAdvExisted = paymentRepository.Get(x => x.AcctId == detail.AcctId && x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_NETOFF && x.OfficeId == office.Id).FirstOrDefault();
                                }
                                if (payablePaymentAdvExisted != null)
                                {
                                    return new HandleState((object)string.Format("Đã tồn tại ghi nhận CT {0} và type Adv - ref {1}.", acc.PaymentNo, detail.AcctId));
                                }

                                // CREDIT payment
                                var creditPayment = new AccAccountPayablePayment();
                                var creditPos = payableCreditExisted.TotalAmount < 0 ? (-1) : 1; // Xét credit âm
                                creditPayment.Id = Guid.NewGuid();
                                creditPayment.PartnerId = partner.Id;
                                creditPayment.PaymentNo = acc.PaymentNo;
                                creditPayment.ReferenceNo = detail.BravoRefNo;
                                creditPayment.AcctId = detail.AcctId;
                                creditPayment.PaymentType = ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT;
                                creditPayment.Currency = detail.Currency;
                                creditPayment.ExchangeRate = detail.ExchangeRate;
                                creditPayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                                creditPayment.PaymentMethod = acc.PaymentMethod;
                                creditPayment.PaymentDate = acc.PaymentDate;

                                creditPayment.PaymentAmount = acctPM.Sum(x => x.PayOriginAmount) != 0 ? (Math.Abs(acctPM.Sum(x => x.PayOriginAmount)) * creditPos) : acctPM.Sum(x => x.PayOriginAmount);
                                creditPayment.PaymentAmountVnd = acctPM.Sum(x => x.PayAmountVND) != 0 ? (Math.Abs(acctPM.Sum(x => x.PayAmountVND)) * creditPos) : acctPM.Sum(x => x.PayAmountVND);
                                creditPayment.PaymentAmountUsd = acctPM.Sum(x => x.PayAmountUSD) != 0 ? (Math.Abs(acctPM.Sum(x => x.PayAmountUSD)) * creditPos) : acctPM.Sum(x => x.PayAmountUSD);

                                creditPayment.RemainAmount = payableCreditExisted.RemainAmount - creditPayment.PaymentAmount;
                                creditPayment.RemainAmountVnd = payableCreditExisted.RemainAmountVnd - acctPM.Sum(x => x.PayAmountVND);
                                creditPayment.RemainAmountUsd = payableCreditExisted.RemainAmountUsd - acctPM.Sum(x => x.PayAmountUSD);
                                // status credit
                                if ((payableCreditExisted.PaymentAmount + creditPayment.PaymentAmount) != 0 && creditPayment.RemainAmount != 0)
                                {
                                    creditPayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                }
                                else if (creditPayment.RemainAmount == 0)
                                {
                                    creditPayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                }

                                creditPayment.CompanyId = currentUser.CompanyID;
                                creditPayment.OfficeId = office.Id;
                                creditPayment.DepartmentId = currentUser.DepartmentId;
                                creditPayment.GroupId = currentUser.GroupId;
                                creditPayment.UserCreated = creditPayment.UserModified = currentUser.UserID;
                                creditPayment.DatetimeCreated = creditPayment.DatetimeModified = DateTime.Now;
                                listInsertPayment.Add(creditPayment);

                                // ADV payment
                                var advPayment = mapper.Map<AccAccountPayablePayment>(creditPayment);
                                creditPos = payableAdvExisted.TotalAmount < 0 ? (-1) : 1; // Xét credit âm
                                advPayment.Id = Guid.NewGuid();
                                advPayment.ReferenceNo = detail.AdvRefNo;
                                advPayment.PaymentAmount = acctPM.Sum(x => x.PayOriginAmount) != 0 ? (Math.Abs(acctPM.Sum(x => x.PayOriginAmount)) * creditPos) : acctPM.Sum(x => x.PayOriginAmount);
                                advPayment.PaymentAmountVnd = acctPM.Sum(x => x.PayAmountVND) != 0 ? (Math.Abs(acctPM.Sum(x => x.PayAmountVND)) * creditPos) : acctPM.Sum(x => x.PayAmountVND);
                                advPayment.PaymentAmountUsd = acctPM.Sum(x => x.PayAmountUSD) != 0 ? (Math.Abs(acctPM.Sum(x => x.PayAmountUSD)) * creditPos) : acctPM.Sum(x => x.PayAmountUSD);
                                advPayment.RemainAmount = payableAdvExisted.RemainAmount - advPayment.PaymentAmount;
                                advPayment.RemainAmountVnd = payableAdvExisted.RemainAmountVnd - advPayment.PaymentAmountVnd;
                                advPayment.RemainAmountUsd = payableAdvExisted.RemainAmountUsd - advPayment.PaymentAmountUsd;
                                // status adv refno
                                if ((payableAdvExisted.PaymentAmount + advPayment.PaymentAmount) != 0 && advPayment.RemainAmount != 0)
                                {
                                    advPayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                }
                                else if (advPayment.RemainAmount == 0)
                                {
                                    advPayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                }
                                // Payment Type adv refno
                                if (detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CR_COMBINE)
                                {
                                    advPayment.PaymentType = ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT; // type CRCOMBINE => ghi nhận CN để Credit
                                }
                                else
                                {
                                    advPayment.PaymentType = ForPartnerConstants.PAYABLE_PAYMENT_TYPE_NETOFF; // type COMBINE => ghi nhận CN để NetOff
                                }
                                listInsertPayment.Add(advPayment);

                                // Update paid amount
                                // Type CREDIT
                                payableCreditExisted.PaymentAmount = (payableCreditExisted.PaymentAmount ?? 0) + creditPayment.PaymentAmount;
                                payableCreditExisted.PaymentAmountVnd = (payableCreditExisted.PaymentAmountVnd ?? 0) + creditPayment.PaymentAmountVnd;
                                payableCreditExisted.PaymentAmountUsd = (payableCreditExisted.PaymentAmountUsd ?? 0) + creditPayment.PaymentAmountUsd;
                                payableCreditExisted.RemainAmount = payableCreditExisted.TotalAmount - payableCreditExisted.PaymentAmount;
                                if (payableCreditExisted.Currency == ForPartnerConstants.CURRENCY_LOCAL) // clear remain usd nếu remain vnd = 0
                                {
                                    payableCreditExisted.RemainAmountVnd = payableCreditExisted.TotalAmountVnd - payableCreditExisted.PaymentAmountVnd;
                                    payableCreditExisted.RemainAmountUsd = payableCreditExisted.RemainAmount == 0 ? 0 : (payableCreditExisted.TotalAmountUsd - payableCreditExisted.PaymentAmountUsd);
                                }
                                else // clear remain vnd nếu remain usd = 0
                                {
                                    payableCreditExisted.RemainAmountVnd = (payableCreditExisted.RemainAmount == 0 ? 0 : (payableCreditExisted.TotalAmountVnd - payableCreditExisted.PaymentAmountVnd));
                                    payableCreditExisted.RemainAmountUsd = payableCreditExisted.TotalAmountUsd - payableCreditExisted.PaymentAmountUsd;
                                }

                                payableCreditExisted.Status = creditPayment.Status;
                                //if (payableCreditExisted.PaymentAmount != 0 && payableCreditExisted.RemainAmount != 0)
                                //{
                                //    payableCreditExisted.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                //}
                                //else if (payableCreditExisted.RemainAmount == 0)
                                //{
                                //    payableCreditExisted.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                //}
                                payableCreditExisted.DatetimeModified = DateTime.Now;
                                payableCreditExisted.UserModified = currentUser.UserID;
                                // Type ADV
                                payableAdvExisted.PaymentAmount = (payableAdvExisted.PaymentAmount ?? 0) + advPayment.PaymentAmount;
                                payableAdvExisted.PaymentAmountVnd = (payableAdvExisted.PaymentAmountVnd ?? 0) + advPayment.PaymentAmountVnd;
                                payableAdvExisted.PaymentAmountUsd = (payableAdvExisted.PaymentAmountUsd ?? 0) + advPayment.PaymentAmountUsd;
                                payableAdvExisted.RemainAmount = payableAdvExisted.TotalAmount - payableAdvExisted.PaymentAmount;
                                if (payableAdvExisted.Currency == ForPartnerConstants.CURRENCY_LOCAL) // clear remain usd nếu remain vnd = 0
                                {
                                    payableAdvExisted.RemainAmountVnd = payableAdvExisted.TotalAmountVnd - payableAdvExisted.PaymentAmountVnd;
                                    payableAdvExisted.RemainAmountUsd = payableAdvExisted.RemainAmount == 0 ? 0 : (payableAdvExisted.TotalAmountUsd - payableAdvExisted.PaymentAmountUsd);
                                }
                                else // clear remain vnd nếu remain usd = 0
                                {
                                    payableAdvExisted.RemainAmountVnd = (payableAdvExisted.RemainAmount == 0 ? 0 : (payableAdvExisted.TotalAmountVnd - payableAdvExisted.PaymentAmountVnd));
                                    payableAdvExisted.RemainAmountUsd = payableAdvExisted.TotalAmountUsd - payableAdvExisted.PaymentAmountUsd;
                                }

                                payableAdvExisted.Status = advPayment.Status;
                                //if (payableAdvExisted.PaymentAmount != 0 && payableAdvExisted.RemainAmount != 0)
                                //{
                                //    payableAdvExisted.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                //}
                                //else if (payableAdvExisted.RemainAmount == 0)
                                //{
                                //    payableAdvExisted.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                //}
                                payableAdvExisted.DatetimeModified = DateTime.Now;
                                payableAdvExisted.UserModified = currentUser.UserID;
                                #endregion

                                var hsPayableCredit = await DataContext.UpdateAsync(payableCreditExisted, x => x.Id == payableCreditExisted.Id);
                                var hsPayableAdv = await DataContext.UpdateAsync(payableAdvExisted, x => x.Id == payableAdvExisted.Id);
                            }
                        }
                        if (listInsertPayment.Count > 0)
                        {
                            foreach (var item in listInsertPayment)
                            {
                                hsPayablePM = await paymentRepository.AddAsync(item);
                                if (!hsPayablePM.Success)
                                {
                                    new LogHelper("InsertAccountPayablePayment", hsPayablePM.Message?.ToString());
                                    return new HandleState("Ghi nhận thất bại. " + hsPayablePM.Message?.ToString());
                                }
                                
                            }
                        }
                        //if (!hsPayablePM.Success)
                        //{
                        //    new LogHelper("InsertAccountPayablePayment", hsPayablePM.Message?.ToString());
                        //    return new HandleState("Ghi nhận thất bại. " + hsPayablePM.Message?.ToString());

                        //}
                        //else
                        //{
                        //    HandleState hsAddP = paymentRepository.SubmitChanges();
                        //    if (hsAddP.Success == false)
                        //    {
                        //        new LogHelper("InsertAccountPayablePayment", hsAddP.Message?.ToString());
                        //        return new HandleState("Ghi nhận thất bại. " + hsAddP.Message?.ToString());

                        //    }
                        //}

                        if (listInsertPayable.Count > 0)
                        {
                            foreach (var item in listInsertPayable)
                            {
                                hsPayable = await DataContext.AddAsync(item, false);
                            }
                            DataContext.SubmitChanges();
                        }
                        if (!hsPayable.Success)
                        {
                            new LogHelper("InsertAccountPayablePayment_Fail", hsPayable.Message?.ToString());
                        }
                        else
                        {
                            await UpdateCreditMangagement(listInsertPayment, acctMngIds);
                        }
                        trans.Commit();
                    }
                    return hsPayablePM;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("InsertAccountPayablePayment", ex.ToString());
                    return new HandleState((object)ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        /// <summary>
        /// Update trừ cn trên credit
        /// </summary>
        /// <param name="payable"></param>
        /// <param name="acctIds"></param>
        /// <returns></returns>
        private async Task<HandleState> UpdateCreditMangagement(List<AccAccountPayablePayment> payables, List<Guid> acctIds)
        {
            var hs = new HandleState();
            var surcharges = await surchargeRepository.GetAsync(x => acctIds.Contains((Guid)x.AcctManagementId));
            var shipments = surcharges.GroupBy(x => new { x.JobNo, x.Mblno, x.Hblno, x.Hblid })
                .Select(x => new
                {
                    x.Key.JobNo,
                    x.Key.Hblid
                ,
                    Code = x.FirstOrDefault().Type == "OBH" ? (x.FirstOrDefault().PaySyncedFrom.Contains("SOA") ? x.FirstOrDefault().PaySoano : x.FirstOrDefault().CreditNo) :
                (x.FirstOrDefault().SyncedFrom.Contains("SOA") ? x.FirstOrDefault().PaySoano : x.FirstOrDefault().CreditNo)
                });
            foreach (var payable in payables)
            {
                var creditMng = creditManagementArRepository.Get(x => x.PartnerId == payable.PartnerId && x.ReferenceNo == payable.ReferenceNo && (x.RemainVnd > 0 || x.RemainUsd > 0));
                var data = from cre in creditMng
                           join shp in shipments on new { cre.JobNo, Hblid = (Guid)cre.Hblid, cre.Code } equals new { shp.JobNo, shp.Hblid, shp.Code }
                           select cre;
                foreach (var item in data)
                {
                    item.RemainVnd = item.RemainVnd == 0 ? item.RemainVnd : (item.RemainVnd - payable.PaymentAmountVnd);
                    item.RemainUsd = item.RemainUsd == 0 ? item.RemainUsd : (item.RemainUsd - payable.PaymentAmountUsd);
                    if(payable.Currency == ForPartnerConstants.CURRENCY_LOCAL)
                    {
                        if (item.RemainVnd == 0)
                            item.RemainUsd = 0;
                    }
                    else
                    {
                        if (item.RemainUsd == 0)
                            item.RemainVnd = 0;
                    }
                    await creditManagementArRepository.UpdateAsync(item, x => x.Id == item.Id, false);
                }
                hs = creditManagementArRepository.SubmitChanges();
            }
            if (!hs.Success)
            {
                new LogHelper("UpdateCreditMangagement_Fail", hs.Message?.ToString());
            }
            return hs;
        }

        /// <summary>
        /// Hủy các ghi nhận giảm trừ công nợ
        /// </summary>
        /// <param name="accountPayables"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public HandleState CancelAccountPayablePayment(List<CancelPayablePayment> accountPayables, string apiKey)
        {
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.Action = "CancelAccountPayablePayment";

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = new HandleState();
                    var hsDelPayable = new HandleState();
                    foreach (var acc in accountPayables)
                    {
                        SysOffice office = officeRepository.Get(x => x.Code == acc.OfficeCODE).FirstOrDefault();
                        if (office == null)
                        {
                            throw new Exception(acc.OfficeCODE + " không tồn tại");
                        }
                        IQueryable<AccAccountPayablePayment> existPayment = paymentRepository.Get(x => x.PaymentNo == acc.PaymentNo
                        && x.OfficeId == office.Id
                        && x.ReferenceNo == acc.BravoRefNo
                        && x.AcctId == acc.AcctId
                        );
                        if (acc.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_COMBINE)
                        {
                            existPayment = existPayment.Where(x => x.PaymentType.Contains(ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT)
                            || x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_OBH
                            || x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_NETOFF);
                        }
                        else
                        {
                            existPayment = existPayment.Where(x => (!acc.TransactionType.Contains(ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT) && !acc.TransactionType.Contains(ForPartnerConstants.PAYABLE_PAYMENT_TYPE_OBH)) ? x.PaymentType == acc.TransactionType : true);
                        }
                        if (existPayment == null || existPayment.Count() == 0)
                        {
                            throw new Exception("Không tìm thấy thanh toán " + acc.PaymentNo);
                        }
                        List<Guid> deletePayable = existPayment.Select(x => x.Id).ToList();
                        HandleState hsDelPayments = paymentRepository.Delete(x => deletePayable.Any(z => z == x.Id), false);

                        IQueryable<AccAccountPayable> accPaybles = DataContext.Get(x => x.OfficeId == office.Id
                        && x.ReferenceNo == acc.BravoRefNo);

                        if (acc.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_COMBINE)
                        {
                            accPaybles = accPaybles.Where(x => x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_ADV
                            || x.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT)
                            || x.TransactionType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_NETOFF);
                        }
                        else
                        {
                            accPaybles = accPaybles.Where(x => (!acc.TransactionType.Contains(ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT) && !acc.TransactionType.Contains(ForPartnerConstants.PAYABLE_PAYMENT_TYPE_OBH)) ? x.TransactionType == acc.TransactionType : true);
                        }
                        if (accPaybles == null || accPaybles.Count() == 0)
                        {
                            throw new Exception("Không tìm thấy thông tin công nợ " + acc.BravoRefNo);
                        }
                        foreach (var pm in accPaybles)
                        {
                            pm.PaymentAmount = pm.PaymentAmountUsd = pm.PaymentAmountVnd = 0;
                            pm.RemainAmount = pm.TotalAmount;
                            pm.RemainAmountVnd = pm.TotalAmountVnd;
                            pm.RemainAmountUsd = pm.TotalAmountUsd;
                            pm.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                            pm.UserModified = currentUser.UserID;
                            pm.DatetimeModified = DateTime.Now;

                            HandleState hsUpdDatacontext = DataContext.Update(pm, x => x.Id == pm.Id, false);
                        }

                        var creditMngs = creditManagementArRepository.Get(x => x.OfficeId == office.Id.ToString() && x.ReferenceNo == acc.BravoRefNo);
                        foreach (var acct in creditMngs)
                        {
                            acct.RemainVnd = acct.AmountVnd;
                            acct.RemainUsd = acct.AmountUsd;
                            acct.DatetimeModified = DateTime.Now;
                            creditManagementArRepository.Update(acct, x => x.Id == acct.Id, false);
                        }
                    }

                    hsDelPayable = paymentRepository.SubmitChanges();
                    if (!hsDelPayable.Success)
                    {
                        throw new Exception("Hủy ghi nhận thất bại. " + hs.Message?.ToString());
                    }
                    var hsUpdateCreditMng = creditManagementArRepository.SubmitChanges();
                    if (!hsUpdateCreditMng.Success)
                    {
                        new LogHelper("CancelPayablePayment_UpdateCreditManagement_Fail", hsUpdateCreditMng.Message?.ToString() + " \n " + JsonConvert.SerializeObject(accountPayables));
                    }
                    hs = DataContext.SubmitChanges();
                    if (!hs.Success)
                    {
                        trans.Rollback();
                        throw new Exception("Hủy ghi nhận thất bại. " + hs.Message?.ToString());
                    }
                    trans.Commit();
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("CancelAccountPayablePayment_Fail", ex.ToString());
                    return new HandleState((object)ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        /// <summary>
        /// Delete payable transaction and payment after delete voucher
        /// </summary>
        /// <param name="model"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public async Task<HandleState> DeleteAccountPayable(VoucherSyncDeleteModel model, string apiKey)
        {
            HandleState hsAddPayable = new HandleState();
            List<AccAccountPayable> payables = new List<AccAccountPayable>();
            ICurrentUser _currentUser = SetCurrentUserPartner(currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.CompanyID = _currentUser.CompanyID;
            currentUser.Action = "DeleteAccountPayable";
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var office = officeRepository.Get(x => x.Code == model.OfficeCode).FirstOrDefault();
                    var payableDelete = DataContext.Get(x => x.VoucherNo == model.VoucherNo && x.VoucherDate.Value.Date == model.VoucherDate.Date && x.BillingNo == model.DocCode && x.OfficeId == office.Id).ToList();
                    var hsPayable = new HandleState();
                    if (payableDelete?.Count() > 0)
                    {
                        var paymentDelete = paymentRepository.Get(x => payableDelete.Any(pa => pa.ReferenceNo == x.ReferenceNo && (pa.TransactionType != ForPartnerConstants.PAYABLE_PAYMENT_TYPE_ADV ? pa.TransactionType == x.PaymentType : (x.PaymentType.Contains(ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT)
                        || x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_OBH
                        || x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_NETOFF)))).ToList();
                        var hsPaymentDel = new HandleState();
                        if (paymentDelete.Count > 0)
                        {
                            hsPaymentDel = await paymentRepository.DeleteAsync(x => paymentDelete.Any(pm => pm.Id == x.Id));
                        }
                        hsPayable = await DataContext.DeleteAsync(x => payableDelete.Any(pa => pa.Id == x.Id));
                        if (hsPayable.Success)
                        {
                            await DeleteCreditManagement(payableDelete);
                            trans.Commit();
                        }
                        else
                        {
                            new LogHelper("DeleteAccountPayable_Fail", hsPayable.Message?.ToString());
                            trans.Dispose();
                        }
                    }
                    return hsPayable;
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
        /// Delete Credit Management AR when delete voucher
        /// </summary>
        /// <param name="payables"></param>
        /// <returns></returns>
        private async Task<HandleState> DeleteCreditManagement(List<AccAccountPayable> payables)
        {
            HandleState hsDeleteCredit = new HandleState();
            currentUser.Action = "DeleteCreditManagement";
            using (var trans = creditManagementArRepository.DC.Database.BeginTransaction())
            {
                try
                {
                    var refNos = payables.Select(x => x.ReferenceNo).ToList();
                    var billingNos = payables.Select(x => x.BillingNo).ToList();
                    var partnerId = payables.FirstOrDefault().PartnerId;
                    var creditMngIds = creditManagementArRepository.Get(x => x.PartnerId == partnerId && billingNos.Contains(x.Code) && refNos.Contains(x.ReferenceNo)).Select(x => x.Id).ToList();
                    hsDeleteCredit = await creditManagementArRepository.DeleteAsync(x => creditMngIds.Contains(x.Id));
                    if (!hsDeleteCredit.Success)
                    {
                        new LogHelper("DeleteCreditManagement_Fail", hsDeleteCredit.Message?.ToString());
                    }
                    else
                    {
                        trans.Commit();
                    }
                    return hsDeleteCredit;
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
    }
}