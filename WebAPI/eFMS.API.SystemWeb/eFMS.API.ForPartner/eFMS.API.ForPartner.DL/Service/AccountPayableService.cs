using AutoMapper;
using eFMS.API.Common.Helpers;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Models.Payable;
using eFMS.API.ForPartner.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public AccountPayableService(IContextBase<AccAccountPayable> repository,
            IContextBase<CatPartner> partnerRepo,
            ICurrentUser cUser,
            IContextBase<SysOffice> officeRepo,
            IContextBase<AccAccountPayablePayment> paymentRepo,
            IContextBase<SysPartnerApi> sysPartnerApiRepo,
            IContextBase<CatCharge> catChargeRepo,
            IMapper mapper) : base(repository, mapper)
        {
            currentUser = cUser;
            partnerRepository = partnerRepo;
            officeRepository = officeRepo;
            paymentRepository = paymentRepo;
            sysPartnerApiRepository = sysPartnerApiRepo;
            catChargeRepository = catChargeRepo;
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

                    var voucherDetail = model.Details.Where(x => x.TransactionType != "NONE");
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
                                    Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID,
                                    CompanyId = currentUser.CompanyID,
                                    OfficeId = office.Id,
                                    GroupId = currentUser.GroupId,
                                    DepartmentId = currentUser.DepartmentId,
                                    TransactionType = c.FirstOrDefault().TransactionType,
                                    VoucherNo = c.FirstOrDefault().VoucherNo,
                                    BillingNo = model.DocCode,
                                    BillingType = model.DocType,
                                    ExchangeRate = c.FirstOrDefault().ExchangeRate,
                                    PaymentTerm = c.FirstOrDefault().PaymentTerm,
                                    PaymentDueDate = c.FirstOrDefault().VoucherDate,
                                    InvoiceNo = c.FirstOrDefault().InvoiceNo,
                                    InvoiceDate = c.FirstOrDefault().InvoiceDate,
                                    Over16To30Day = 0,
                                    Over1To15Day = 0,
                                    Over30Day = 0,
                                    UserCreated = currentUser.UserID,
                                    DatetimeCreated = DateTime.Now,
                                    UserModified = currentUser.UserID,
                                    DatetimeModified = DateTime.Now
                                };
                                payables.Add(payable);
                            });
                            foreach (var item in payables)
                            {
                                item.TotalAmount = item.Currency == ForPartnerConstants.CURRENCY_LOCAL ? item.TotalAmountVnd : item.TotalAmountUsd;

                                await DataContext.AddAsync(item, false);
                            }
                        }
                        //else => TH có số bravo no
                        {
                            var grpVoucherDetail = voucherDetail.Where(z => !string.IsNullOrEmpty(z.BravoRefNo)).GroupBy(x => new { x.VoucherNo, x.TransactionType, model.DocType, model.DocCode, x.BravoRefNo })
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
                                    BillingNo = model.DocCode,
                                    BillingType = model.DocType,
                                    ExchangeRate = c.FirstOrDefault().ExchangeRate,
                                    PaymentTerm = c.FirstOrDefault().PaymentTerm,
                                    PaymentDueDate = c.FirstOrDefault().VoucherDate,
                                    InvoiceNo = c.FirstOrDefault().InvoiceNo,
                                    InvoiceDate = c.FirstOrDefault().InvoiceDate,
                                    Over16To30Day = 0,
                                    Over1To15Day = 0,
                                    Over30Day = 0,
                                    UserCreated = currentUser.UserID,
                                    DatetimeCreated = DateTime.Now,
                                    UserModified = currentUser.UserID,
                                    DatetimeModified = DateTime.Now
                                };

                                payables.Add(payable);
                            });
                            foreach (var item in payables)
                            {
                                item.TotalAmount = item.Currency == ForPartnerConstants.CURRENCY_LOCAL ? item.TotalAmountVnd : item.TotalAmountUsd;
                                item.RemainAmount = item.TotalAmountVnd; ;
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
                                BillingNo = model.DocCode,
                                BillingType = model.DocType,
                                ExchangeRate = c.FirstOrDefault().ExchangeRate,
                                PaymentTerm = c.FirstOrDefault().PaymentTerm,
                                PaymentDueDate = c.FirstOrDefault().VoucherDate,
                                InvoiceNo = c.FirstOrDefault().InvoiceNo,
                                InvoiceDate = c.FirstOrDefault().InvoiceDate,
                                Over16To30Day = 0,
                                Over1To15Day = 0,
                                Over30Day = 0,
                                UserCreated = currentUser.UserID,
                                DatetimeCreated = DateTime.Now,
                                UserModified = currentUser.UserID,
                                DatetimeModified = DateTime.Now
                            };
                            payables.Add(payable);
                        });
                        foreach (var item in payables)
                        {
                            item.TotalAmount = item.Currency == ForPartnerConstants.CURRENCY_LOCAL ? item.TotalAmountVnd : item.TotalAmountUsd;
                            item.RemainAmount = item.TotalAmountVnd; ;
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

        public bool IsPayableHasPayment(VoucherSyncDeleteModel model)
        {

            bool IshasPayment = false;

            List<AccAccountPayable> payable = DataContext.Get(x => x.VoucherNo == model.VoucherNo
            && x.BillingNo == model.DocCode
            && x.BillingType == model.DocType).ToList();
            if (payable.Count > 0)
            {
                List<string> refNos = payable.Select(x => x.ReferenceNo).Distinct().ToList();
                IshasPayment = paymentRepository.Any(x => refNos.Contains(x.ReferenceNo));

            }

            return IshasPayment;
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
        public HandleState InsertAccountPayablePayment(List<AccAccountPayableModel> accountPayables, string apiKey)
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
                        foreach (var detail in acc.Details)
                        {

                            if (detail.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT) || detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_OBH)
                            {
                                #region CREDIT
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

                                if (detail.PayOriginAmount != 0 && detail.RemainOriginAmount != 0)
                                {
                                    accPayablePayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                }
                                else if (detail.RemainOriginAmount == 0)
                                {
                                    accPayablePayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                }

                                accPayablePayment.PaymentAmount = detail.PayOriginAmount;
                                accPayablePayment.PaymentAmountVnd = detail.PayAmountVND;
                                accPayablePayment.PaymentAmountUsd = detail.PayAmountUSD;

                                accPayablePayment.RemainAmount = detail.RemainOriginAmount;
                                accPayablePayment.RemainAmountVnd = detail.RemainAmountVND;
                                accPayablePayment.RemainAmountUsd = detail.RemainAmountUSD;

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
                                payableExisted.RemainAmountVnd = payableExisted.TotalAmountVnd - payableExisted.PaymentAmountVnd;
                                payableExisted.RemainAmountUsd = payableExisted.TotalAmountUsd - payableExisted.PaymentAmountUsd;
                                if (payableExisted.PaymentAmount != 0 && payableExisted.RemainAmount != 0)
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
                                DataContext.Update(payableExisted, x => x.Id == payableExisted.Id, false);
                            }
                            else if (detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_ADV)
                            {
                                #region ADV
                                if (string.IsNullOrEmpty(detail.AdvRefNo?.Trim()))
                                {
                                    return new HandleState((object)string.Format("Dữ liệu chứng từ {0} loại ADV chưa có ref no.", acc.PaymentNo));
                                }
                                var payableAdvExisted = DataContext.Get(x => x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_ADV && x.ReferenceNo == detail.AdvRefNo && x.OfficeId == office.Id).FirstOrDefault();
                                if (payableAdvExisted == null)
                                {
                                    return new HandleState((object)string.Format("Chứng từ {0} và số ref {1} chưa ghi nhận.", acc.PaymentNo, detail.AdvRefNo));
                                }
                                var payablePayment = paymentRepository.Get(x => x.AcctId == detail.AcctId && x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CLEAR_ADV && x.OfficeId == office.Id).FirstOrDefault();
                                if (payablePayment != null)
                                {
                                    return new HandleState((object)string.Format("Đã tồn tại ghi nhận CT {0} và ref {1}.", acc.PaymentNo, detail.BravoRefNo));
                                }
                                // phát sinh dòng công nợ ứng trước và dòng ghi nhận có status Unpaid
                                var accPayablePayment = new AccAccountPayablePayment();
                                accPayablePayment.Id = Guid.NewGuid();
                                accPayablePayment.PartnerId = partner.Id;
                                accPayablePayment.PaymentNo = acc.PaymentNo;
                                accPayablePayment.ReferenceNo = detail.AdvRefNo;
                                accPayablePayment.AcctId = detail.AcctId;
                                accPayablePayment.PaymentType = ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CLEAR_ADV;
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

                                accPayablePayment.PaymentAmount = 0;
                                accPayablePayment.PaymentAmountVnd = 0;
                                accPayablePayment.PaymentAmountUsd = 0;

                                accPayablePayment.RemainAmount = detail.PayOriginAmount + detail.RemainOriginAmount;
                                accPayablePayment.RemainAmountVnd = detail.PayAmountVND + detail.RemainAmountVND;
                                accPayablePayment.RemainAmountUsd = detail.PayAmountUSD + detail.RemainAmountUSD;

                                accPayablePayment.CompanyId = currentUser.CompanyID;
                                accPayablePayment.OfficeId = office.Id;
                                accPayablePayment.DepartmentId = currentUser.DepartmentId;
                                accPayablePayment.GroupId = currentUser.GroupId;
                                accPayablePayment.UserCreated = accPayablePayment.UserModified = currentUser.UserID;
                                accPayablePayment.DatetimeCreated = accPayablePayment.DatetimeModified = DateTime.Now;

                                AccAccountPayable payable = new AccAccountPayable
                                {
                                    Id = Guid.NewGuid(),
                                    Currency = detail.Currency,
                                    PartnerId = partner.Id,
                                    PaymentAmount = 0,
                                    PaymentAmountVnd = 0,
                                    PaymentAmountUsd = 0,
                                    RemainAmount = detail.RemainOriginAmount,
                                    RemainAmountVnd = detail.RemainAmountVND,
                                    RemainAmountUsd = detail.RemainAmountUSD,
                                    TotalAmount = detail.PayOriginAmount + detail.RemainOriginAmount,
                                    TotalAmountVnd = detail.PayAmountVND + detail.RemainAmountVND,
                                    TotalAmountUsd = detail.PayAmountUSD + detail.RemainAmountUSD,
                                    ReferenceNo = detail.AdvRefNo,
                                    Status = accPayablePayment.Status,
                                    CompanyId = currentUser.CompanyID,
                                    OfficeId = office.Id,
                                    GroupId = currentUser.GroupId,
                                    DepartmentId = currentUser.DepartmentId,
                                    TransactionType = ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_ADV,
                                    VoucherNo = null,
                                    InvoiceNo = null,
                                    InvoiceDate = null,
                                    BillingNo = acc.PaymentNo,
                                    BillingType = null,
                                    ExchangeRate = detail.ExchangeRate,
                                    PaymentTerm = 0,
                                    PaymentDueDate = null,
                                    Over16To30Day = 0,
                                    Over1To15Day = 0,
                                    Over30Day = 0,
                                    UserCreated = currentUser.UserID,
                                    DatetimeCreated = DateTime.Now,
                                    UserModified = currentUser.UserID,
                                    DatetimeModified = DateTime.Now,
                                };
                                #endregion
                                listInsertPayment.Add(accPayablePayment);
                                listInsertPayable.Add(payable);
                            }
                            else if (detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_COMBINE || detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CR_COMBINE)
                            {
                                #region COMBINE
                                var payableCreditExisted = DataContext.Get(x => (x.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT) || x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_OBH) && x.ReferenceNo == detail.BravoRefNo && x.OfficeId == office.Id).FirstOrDefault();
                                if (payableCreditExisted == null)
                                {
                                    return new HandleState((object)string.Format("Chứng từ {0} và số ref {1} chưa ghi nhận.", acc.PaymentNo, detail.BravoRefNo));
                                }
                                var payableAdvExisted = DataContext.Get(x => x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_ADV && x.ReferenceNo == detail.AdvRefNo && x.OfficeId == office.Id).FirstOrDefault();
                                if (payableAdvExisted == null)
                                {
                                    return new HandleState((object)string.Format("Chứng từ {0} và số ref {1} chưa ghi nhận.", acc.PaymentNo, detail.AdvRefNo));
                                }
                                var payablePaymentCreditExisted = paymentRepository.Get(x => x.AcctId == detail.AcctId && (x.PaymentType.Contains(ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT) || x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_OBH) && x.OfficeId == office.Id).FirstOrDefault();
                                if (payablePaymentCreditExisted != null)
                                {
                                    return new HandleState((object)string.Format("Đã tồn tại ghi nhận CT {0} và type credit - ref {1}.", acc.PaymentNo, detail.AcctId));
                                }
                                var payablePaymentAdvExisted = paymentRepository.Get(x => x.AcctId == detail.AcctId && (x.PaymentType == (detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CR_COMBINE ? ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CLEAR_ADV : ForPartnerConstants.PAYABLE_PAYMENT_TYPE_NETOFF)) && x.OfficeId == office.Id).FirstOrDefault();
                                if (payablePaymentAdvExisted != null)
                                {
                                    return new HandleState((object)string.Format("Đã tồn tại ghi nhận CT {0} và type Adv - ref {1}.", acc.PaymentNo, detail.AcctId));
                                }
                                var creditPayment = new AccAccountPayablePayment();
                                var advPayment = new AccAccountPayablePayment();
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
                                if (detail.PayOriginAmount != 0 && detail.RemainOriginAmount != 0)
                                {
                                    creditPayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                }
                                else if (detail.RemainOriginAmount == 0)
                                {
                                    creditPayment.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                }

                                creditPayment.PaymentAmount = detail.PayOriginAmount;
                                creditPayment.PaymentAmountVnd = detail.PayAmountVND;
                                creditPayment.PaymentAmountUsd = detail.PayAmountUSD;

                                creditPayment.RemainAmount = detail.RemainOriginAmount;
                                creditPayment.RemainAmountVnd = detail.RemainAmountVND;
                                creditPayment.RemainAmountUsd = detail.RemainAmountUSD;

                                creditPayment.CompanyId = currentUser.CompanyID;
                                creditPayment.OfficeId = office.Id;
                                creditPayment.DepartmentId = currentUser.DepartmentId;
                                creditPayment.GroupId = currentUser.GroupId;
                                creditPayment.UserCreated = creditPayment.UserModified = currentUser.UserID;
                                creditPayment.DatetimeCreated = creditPayment.DatetimeModified = DateTime.Now;

                                advPayment = creditPayment;
                                advPayment.Id = Guid.NewGuid();
                                advPayment.ReferenceNo = detail.AdvRefNo;
                                advPayment.PaymentType = (detail.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CR_COMBINE ? ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CLEAR_ADV : ForPartnerConstants.PAYABLE_PAYMENT_TYPE_NETOFF); // type CRCOMBINE => ghi nhận CN để clear adv

                                // Update paid amount
                                // Type CREDIT
                                payableCreditExisted.PaymentAmount = (payableCreditExisted.PaymentAmount ?? 0) + creditPayment.PaymentAmount;
                                payableCreditExisted.PaymentAmountVnd = (payableCreditExisted.PaymentAmountVnd ?? 0) + creditPayment.PaymentAmountVnd;
                                payableCreditExisted.PaymentAmountUsd = (payableCreditExisted.PaymentAmountUsd ?? 0) + creditPayment.PaymentAmountUsd;
                                payableCreditExisted.RemainAmount = payableCreditExisted.TotalAmount - payableCreditExisted.PaymentAmount;
                                payableCreditExisted.RemainAmountVnd = payableCreditExisted.TotalAmountVnd - payableCreditExisted.PaymentAmountVnd;
                                payableCreditExisted.RemainAmountUsd = payableCreditExisted.TotalAmountUsd - payableCreditExisted.PaymentAmountUsd;
                                if (payableCreditExisted.PaymentAmount != 0 && payableCreditExisted.RemainAmount != 0)
                                {
                                    payableCreditExisted.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                }
                                else if (payableCreditExisted.RemainAmount == 0)
                                {
                                    payableCreditExisted.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                }
                                payableCreditExisted.DatetimeModified = DateTime.Now;
                                payableCreditExisted.UserModified = currentUser.UserID;
                                // Type ADV
                                payableAdvExisted.PaymentAmount = (payableAdvExisted.PaymentAmount ?? 0) + creditPayment.PaymentAmount;
                                payableAdvExisted.PaymentAmountVnd = (payableAdvExisted.PaymentAmountVnd ?? 0) + creditPayment.PaymentAmountVnd;
                                payableAdvExisted.PaymentAmountUsd = (payableAdvExisted.PaymentAmountUsd ?? 0) + creditPayment.PaymentAmountUsd;
                                payableAdvExisted.RemainAmount = payableAdvExisted.TotalAmount - payableAdvExisted.PaymentAmount;
                                payableAdvExisted.RemainAmountVnd = payableAdvExisted.TotalAmountVnd - payableAdvExisted.PaymentAmountVnd;
                                payableAdvExisted.RemainAmountUsd = payableAdvExisted.TotalAmountUsd - payableAdvExisted.PaymentAmountUsd;
                                if (payableAdvExisted.PaymentAmount != 0 && payableAdvExisted.RemainAmount != 0)
                                {
                                    payableAdvExisted.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART;
                                }
                                else if (payableAdvExisted.RemainAmount == 0)
                                {
                                    payableAdvExisted.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_PAID;
                                }
                                payableAdvExisted.DatetimeModified = DateTime.Now;
                                payableAdvExisted.UserModified = currentUser.UserID;
                                #endregion
                                listInsertPayment.Add(creditPayment);
                                listInsertPayment.Add(advPayment);
                                var hsPayableCredit = DataContext.Update(payableCreditExisted, x => x.Id == payableCreditExisted.Id, false);
                                var hsPayableAdv = DataContext.Update(payableAdvExisted, x => x.Id == payableAdvExisted.Id, false);
                            }
                        }
                        if (listInsertPayment.Count > 0)
                        {
                            hsPayablePM = paymentRepository.Add(listInsertPayment);
                        }
                        if (!hsPayablePM.Success)
                        {
                            return new HandleState("Ghi nhận thất bại. " + hsPayablePM.Message?.ToString());
                        }
                        if (listInsertPayable.Count > 0)
                        {
                            hsPayable = DataContext.Add(listInsertPayable);
                        }
                        if (hsPayable.Success)
                        {
                            DataContext.SubmitChanges();
                        }
                        trans.Commit();
                    }
                    return hsPayablePM;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("CancelAccountPayablePayment", ex.ToString());
                    return new HandleState((object)ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
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
                        var office = officeRepository.Get(x => x.Code == acc.OfficeCODE).FirstOrDefault();
                        var existPayment = DataContext.Get(x => x.BillingNo == acc.PaymentNo && x.OfficeId == office.Id);
                        if (acc.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_COMBINE)
                        {
                            existPayment = existPayment.Where(x => x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_ADV || x.TransactionType.Contains(ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_CREDIT) || x.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_OBH);
                        }
                        else
                        {
                            existPayment = existPayment.Where(x => x.TransactionType == acc.TransactionType);
                        }
                        if (existPayment == null || existPayment.Count() == 0)
                        {
                            return new HandleState((object)"Không tìm thấy hóa đơn " + acc.PaymentNo);
                        }
                        foreach (var pm in existPayment)
                        {
                            pm.PaymentAmount = pm.PaymentAmountUsd = pm.PaymentAmountVnd = 0;
                            pm.RemainAmount = pm.TotalAmount;
                            pm.RemainAmountVnd = pm.TotalAmountVnd;
                            pm.RemainAmountUsd = pm.TotalAmountUsd;
                            pm.Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
                            pm.UserModified = currentUser.UserID;
                            pm.DatetimeModified = DateTime.Now;
                            var hsUpdDatacontext = DataContext.Update(pm, x => x.Id == pm.Id, false);
                            if (!hsUpdDatacontext.Success)
                            {
                                return new HandleState((object)"Hủy ghi nhận thất bại. " + hsUpdDatacontext.Message?.ToString());
                            }
                        }
                        var deletePayable = new List<Guid>();
                        if (acc.TransactionType == ForPartnerConstants.PAYABLE_TRANSACTION_TYPE_COMBINE)
                        {
                            deletePayable = paymentRepository.Get(x => x.PaymentNo == acc.PaymentNo && x.OfficeId == office.Id && (x.PaymentType.Contains(ForPartnerConstants.PAYABLE_PAYMENT_TYPE_CREDIT) || x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_OBH || x.PaymentType == ForPartnerConstants.PAYABLE_PAYMENT_TYPE_NETOFF)).Select(x => x.Id).ToList();
                        }
                        else
                        {
                            deletePayable = paymentRepository.Get(x => x.PaymentNo == acc.PaymentNo && x.OfficeId == office.Id && acc.TransactionType.Contains(x.PaymentType)).Select(x => x.Id).ToList();
                        }
                        var hsDel = paymentRepository.Delete(x => deletePayable.Any(z => z == x.Id), false);
                        if (!hsDel.Success)
                        {
                            return new HandleState((object)"Hủy ghi nhận thất bại. " + hsDel.Message?.ToString());
                        }
                    }

                    hsDelPayable = paymentRepository.SubmitChanges();
                    hs = DataContext.SubmitChanges();
                    if (!hs.Success || !hsDelPayable.Success)
                    {
                        return new HandleState((object)"Hủy ghi nhận thất bại.");
                    }
                    trans.Commit();
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("CancelAccountPayablePayment", ex.ToString());
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