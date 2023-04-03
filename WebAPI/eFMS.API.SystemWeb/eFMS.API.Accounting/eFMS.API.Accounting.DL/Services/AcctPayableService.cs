﻿using System;
using System.Collections.Generic;
using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Linq.Expressions;
using eFMS.API.Accounting.DL.Models.AccountingPayable;

namespace eFMS.API.Accounting.DL.Services
{
    public class AcctPayableService : RepositoryBase<AccAccountPayable, AccAccountPayableModel>, IAcctPayableService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<AccAccountPayablePayment> accountPayablePaymentRepository;
        private readonly IContextBase<SysOffice> sysOfficeRepository;
        private readonly IContextBase<CatCurrencyExchange> exchangeRateRepository;

        public AcctPayableService(
            IContextBase<AccAccountPayable> repository,
            IMapper mapper,
            ICurrentUser curUser,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<AccAccountPayablePayment> accountPayablePaymentRepo,
            IContextBase<SysOffice> sysOfficeRepo,
            IContextBase<CatCurrencyExchange> exchangeRateRepo,
            IOptions<WebUrl> wUrl

            ) : base(repository, mapper)
        {
            currentUser = curUser;
            stringLocalizer = localizer;
            catPartnerRepository = catPartnerRepo;
            accountPayablePaymentRepository = accountPayablePaymentRepo;
            sysOfficeRepository = sysOfficeRepo;
            exchangeRateRepository = exchangeRateRepo;
        }

        /// <summary>
        /// Paging
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="rowsCount"></param>
        /// <returns></returns>
        public IQueryable<AccAccountPayableModel> Paging(AccountPayableCriteria criteria, int page, int size, out int rowsCount)
        {
            criteria.IsPaging = true;
            var data = GetDataAcctPayable(criteria, page * size);
            IQueryable<AccAccountPayableModel> result = null;

            int _totalItem = 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }

                if (data == null)
                {
                    rowsCount = 0;
                    return null;
                }
                var dataList = new List<AcctPayablePaymentDetailModel>();
                dataList.AddRange(data);
                _totalItem = dataList.Count;
                var pagingData = dataList.OrderByDescending(x => x.VoucherDate).GroupBy(x => new { x.RefId, x.PartnerId, x.VoucherNo, x.VoucherDate, x.InvoiceNo, x.BillingNo, x.TransactionType })
                    .Skip((page - 1) * size).Take(size).SelectMany(x => x).AsQueryable();
                ////data = data.Skip((page - 1) * size).Take(size);
                result = GenerateDataPaging(pagingData, criteria);
            }
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            return result;
        }

        /// <summary>
        /// Get data paging in list detail
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IQueryable<AccAccountPayableModel> GenerateDataPaging(IQueryable<AcctPayablePaymentDetailModel> data, AccountPayableCriteria criteria)
        {
            //var data = GetDataAcctPayable(criteria);
            //if (data == null || data.FirstOrDefault() == null)
            //{
            //    return null;
            //}
            //var payables = (from payable in data
            //               join partner in catPartnerRepository.Get(x => x.Active == true) on payable.PartnerId equals partner.Id into grpPartner
            //               from partner in grpPartner.DefaultIfEmpty()
            //               select new { payable, ShortName = partner != null ? partner.ShortName : string.Empty, AccountNo = partner != null ? partner.AccountNo : string.Empty }).ToList();
            var payableGrp = data.GroupBy(x => new { x.RefId, x.PartnerId, x.VoucherNo, x.VoucherDate, x.InvoiceNo, x.BillingNo, x.TransactionType });

            var acctPayables = new List<AccAccountPayableModel>();
            var partnerData = catPartnerRepository.Get(x => x.Active == true);
            foreach (var item in payableGrp)
            {
                var acct = new AccAccountPayableModel();
                var advValue = item.Key.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE ? (-1) : 1; // dòng adv hiện giá trị âm
                var partner = partnerData.Where(x => x.Id == item.Key.PartnerId).FirstOrDefault();
                var paymentAmount = GetAmountWithCurrency(item.Select(z => z).AsQueryable(), criteria.FromPaymentDate);
                var paymentAmountOrigin = paymentAmount.Where(x => x.Currency == "Origin").FirstOrDefault();
                var paymentAmountVnd = paymentAmount.Where(x => x.Currency == AccountingConstants.CURRENCY_LOCAL).FirstOrDefault();
                acctPayables.Add(new AccAccountPayableModel
                {
                    RefId = item.Key.RefId?.ToString(),
                    ReferenceNo = item.Key.VoucherNo,
                    PartnerName = partner?.ShortName,
                    AccountNo = partner?.AccountNo,
                    TransactionType = item.Key.TransactionType,
                    VoucherDate = item.Key.VoucherDate,
                    InvoiceNo = item.Key.InvoiceNo,
                    // InvoiceDate [CR 15/03/22: Leyla invoice date null or =01/01/1900 => inv date = acct date + pm term ]
                    InvoiceDate = item.Key.InvoiceNo == null ? null : (item.FirstOrDefault().InvoiceDate == null || item.FirstOrDefault().InvoiceDate.Value.Date == DateTime.Parse("01/01/1900").Date ? item.FirstOrDefault().PaymentDueDate : item.FirstOrDefault().InvoiceDate),
                    BravoRefNo = item.FirstOrDefault().ReferenceNo,
                    BillingNo = item.FirstOrDefault().BillingNo,
                    Currency = item.FirstOrDefault().Currency,
                    PaymentTerm = item.FirstOrDefault().PaymentTerm,
                    PaymentDueDate = item.FirstOrDefault().PaymentDueDate,
                    TotalAmount = paymentAmountOrigin.Total * advValue,
                    PaymentAmount = paymentAmountOrigin.PaidAmount * advValue,
                    RemainAmount = paymentAmountOrigin.RemainAmount * advValue,
                    TotalAmountVnd = paymentAmountVnd.Total * advValue,
                    PaymentAmountVnd = paymentAmountVnd.PaidAmount * advValue,
                    RemainAmountVnd = paymentAmountVnd.RemainAmount * advValue,
                    NotShowDetail = item.FirstOrDefault().InRangeType == "OutRange"
                });
            }
            return acctPayables.AsQueryable();
        }

        /// <summary>
        /// Get amount with currency of payment
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<AmountWithCurrencyModel> GetAmountWithCurrency(IQueryable<AcctPayablePaymentDetailModel> data, string toDate)
        {
            var paymentdata = data.Where(x => x.TransactionType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE || (x.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE && x.PaymentType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE)).OrderBy(x => x.PaymentDatetimeCreated).GroupBy(x => new { x.PaymentAcctId, x.PaymentDate, x.PaymentType });
            var firstDataGrp = data.FirstOrDefault();
            string type = firstDataGrp.InRangeType;
            decimal? total = 0, totalVnd = 0, totalUsd = 0;
            decimal? paidAmount = 0, paidAmountVnd = 0, paidAmountUsd = 0;
            decimal? remainAmount = 0, remainAmountVnd = 0, remainAmountUsd = 0;
            switch (type)
            {
                case "InRange":
                    // Transaction AP Phát sinh trong thời gian được chọn => lấy số dư đầu kì
                    {
                        totalVnd = firstDataGrp.TotalAmountVnd ?? 0;
                    }
                    {
                        totalUsd = firstDataGrp.TotalAmountUsd ?? 0;
                    }
                    {
                        total = firstDataGrp.TotalAmount ?? 0;
                    }
                    break;
                case "PMInRange":
                    // Transaction AP phát sinh trước thời gian được chọn và có AP payment trong khoảng thời gian được chọn: Begin Amount => lấy remain amount của payment gần nhất (Cận dưới from date), trường hợp không có payment => Total Amount
                    var outRangePM = paymentdata.Where(x => x.FirstOrDefault().PaymentDate < (DateTime.Parse(toDate).Date)).OrderByDescending(x => x.FirstOrDefault().PaymentDate).ThenByDescending(x => x.FirstOrDefault().PaymentDatetimeCreated).FirstOrDefault();
                    {
                        //if (data.Any(x => x.PaymentRemainAmountVnd == null))
                        if (outRangePM == null) // nếu không có payment thì lấy total amount
                        {
                            total = firstDataGrp.TotalAmount ?? 0;
                            totalVnd = firstDataGrp.TotalAmountVnd ?? 0;
                            totalUsd = firstDataGrp.TotalAmountUsd ?? 0;
                        }
                        else
                        {
                            total = outRangePM.FirstOrDefault()?.PaymentRemainAmount ?? 0;
                            totalVnd = outRangePM.FirstOrDefault()?.PaymentRemainAmountVnd ?? 0;// data.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.PaymentRemainAmountVnd ?? 0;
                            totalUsd = outRangePM.FirstOrDefault()?.PaymentRemainAmountUsd ?? 0;
                        }
                    }
                    break;
                case "OutRange":
                    // Transaction AP chưa trả hết(Unpaid/ Paid a Part) trước và không có payment trướcng khoảng thời được chọn => lấy remain amount của AP cận trên đó, Trường hợp remain không có giá trị sẽ lấy Total Amount
                    {
                        totalVnd = (firstDataGrp.RemainAmountVnd == null || firstDataGrp.RemainAmountVnd == 0 || firstDataGrp.PaymentRemainAmountVnd == null) ? firstDataGrp.TotalAmount : firstDataGrp.PaymentRemainAmountVnd;
                    }
                    {
                        totalUsd = (firstDataGrp.RemainAmountUsd == null || firstDataGrp.RemainAmountUsd == 0 || firstDataGrp.PaymentRemainAmountUsd == null) ? firstDataGrp.TotalAmountUsd : firstDataGrp.PaymentRemainAmountUsd;
                    }
                    {
                        total = (firstDataGrp.OrgRemainAmount == null || firstDataGrp.OrgRemainAmount == 0 || firstDataGrp.PaymentRemainAmount == null) ? firstDataGrp.TotalAmount : firstDataGrp.PaymentRemainAmount;
                    }
                    break;
            }
            {
                foreach (var item in paymentdata)
                {
                    paidAmount += item.Sum(x => x.PaymentAmount ?? 0);
                    remainAmount += item.FirstOrDefault().PaymentRemainAmount;

                    paidAmountVnd += item.Sum(x => x.PaymentAmountVnd ?? 0);
                    remainAmountVnd += item.FirstOrDefault().PaymentRemainAmountVnd;

                    paidAmountUsd += item.Sum(x => x.PaymentAmountUsd ?? 0);
                    remainAmountUsd += item.FirstOrDefault().PaymentRemainAmountUsd;
                }
                // [CR: 15/03/22] TH ghi nhận trong kì thì lấy total = total amount ban đầu, remain = remain trên AP
                if (type == "InRange")
                {
                    remainAmount = firstDataGrp.OrgRemainAmount;
                    remainAmountVnd = firstDataGrp.RemainAmountVnd;
                    remainAmountUsd = firstDataGrp.RemainAmountUsd;
                }
            }
            
            if (type == "OutRange")
            {
                paidAmount = paidAmountVnd = paidAmountUsd = null;
                remainAmount = remainAmountVnd = remainAmountUsd = null;
            }

            // TH transaction type adv nhưng không có payment
            //if (data.FirstOrDefault().TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE && data.Any(x => string.IsNullOrEmpty(x.PaymentNo)))
            //{
            //    if (currency == AccountingConstants.CURRENCY_LOCAL)
            //    {
            //        paidAmount = data.FirstOrDefault().PaymentAmountVnd;
            //        remainAmount = data.FirstOrDefault().PaymentRemainAmountVnd;
            //    }
            //    else if (currency == AccountingConstants.CURRENCY_USD)
            //    {
            //        paidAmount = data.FirstOrDefault().PaymentAmountUsd;
            //        remainAmount = data.FirstOrDefault().PaymentRemainAmountUsd;
            //    }
            //    else
            //    {
            //        paidAmount = data.FirstOrDefault().PaymentAmount;
            //        remainAmount = data.FirstOrDefault().PaymentRemainAmount;
            //    }
            //}
            var result = new List<AmountWithCurrencyModel>();
            result.Add(new AmountWithCurrencyModel() { Currency = "Origin", Total = total, PaidAmount = paidAmount, RemainAmount = remainAmount });
            result.Add(new AmountWithCurrencyModel() { Currency = AccountingConstants.CURRENCY_LOCAL, Total = totalVnd, PaidAmount = paidAmountVnd, RemainAmount = remainAmountVnd });
            result.Add(new AmountWithCurrencyModel() { Currency = AccountingConstants.CURRENCY_USD, Total = totalUsd, PaidAmount = paidAmountUsd, RemainAmount = remainAmountUsd });
            return result;
        }

        /// <summary>
        /// Is init search data payable payment history
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private bool IsInitSearch(AccountPayableCriteria criteria)
        {
            // Get data within current months if search without anything
            var firstDateOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (criteria.IsPaging == true &&
                string.IsNullOrEmpty(criteria.PartnerId) &&
                (string.IsNullOrEmpty(criteria.ReferenceNos)) &&
                (criteria.PaymentStatus.Contains("Unpaid") && criteria.PaymentStatus.Contains("Paid A Part")) &&
                (DateTime.Parse(criteria.FromPaymentDate) == firstDateOfMonth.Date && DateTime.Parse(criteria.ToPaymentDate).Date == firstDateOfMonth.AddMonths(1).AddDays(-1)) &&
                criteria.TransactionType == null &&
                (criteria.Office.Count == 1 && criteria.Office.Contains(currentUser.OfficeID.ToString())))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get payment detail of payable transaction
        /// </summary>
        /// <param name="refNo"></param>
        /// <param name="type"></param>
        /// <param name="invoiceNo"></param>
        /// <returns></returns>
        public IQueryable<AccAccountPayablePaymentModel> GetBy(AcctPayableViewDetailCriteria criteria)
        {
            var payabledata = DataContext.Get(x => x.VoucherNo == criteria.RefNo && x.TransactionType == criteria.Type && (string.IsNullOrEmpty(x.InvoiceNo) || x.InvoiceNo == criteria.InvoiceNo) && (string.IsNullOrEmpty(x.BillingNo) || x.BillingNo == criteria.BillingNo) && x.ReferenceNo == criteria.BravoNo).FirstOrDefault();
            if (payabledata == null) return null;
            var advValue = payabledata.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE ? (-1) : 1; // dòng adv hiện giá trị âm
            var paymentData = accountPayablePaymentRepository.Get(x => x.ReferenceNo == payabledata.ReferenceNo && (payabledata.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE ? x.PaymentType == AccountingConstants.PAYMENT_TYPE_NAME_NET_OFF : true)).OrderBy(x => x.PaymentDate).ThenBy(x => x.DatetimeCreated)
                .Select(x => new AccAccountPayablePaymentModel
                {
                    RefNo = x.PaymentNo,
                    PaymentDate = x.PaymentDate,
                    InvoiceNo = payabledata.InvoiceNo,
                    InvoiceDate = !string.IsNullOrEmpty(payabledata.InvoiceNo) ? payabledata.InvoiceDate : null,
                    DocNo = payabledata.BillingNo,
                    PaymentAmount = x.PaymentAmount * advValue,
                    RemainAmount = x.RemainAmount * advValue,
                    PaymentAmountVnd = x.PaymentAmountVnd * advValue,
                    RemainAmountVnd = x.RemainAmountVnd * advValue,
                    Currency = x.Currency
                });
            //var data = payabledata.Join(paymentData, pm => pm.ReferenceNo, re => re.ReferenceNo, (pm, re) => new { pm.VoucherNo, pm.TransactionType, pm.InvoiceNo, pm.InvoiceDate, pm.BillingNo, re.ReferenceNo, re.PaymentNo, re.PaymentDate, re.PaymentAmount, re.RemainAmount, re.PaymentAmountVnd, re.RemainAmountVnd, re.Currency });
            //var grpData = data.GroupBy(x => new { x.VoucherNo, x.InvoiceNo, x.ReferenceNo }).Select(x => new { x.Key, receipt = x.Select(z => z) });

            return paymentData;
        }


        private IQueryable<AccAccountPayable> QueryPayable(AccountPayableCriteria criteria)
        {
            var data = DataContext.Get(x => !string.IsNullOrEmpty(x.ReferenceNo));
            if (!string.IsNullOrEmpty(criteria.SearchType) && !string.IsNullOrEmpty(criteria.ReferenceNos?.Trim()))
            {
                var searchNoList = criteria.ReferenceNos.Split('\n').Where(x => !string.IsNullOrEmpty(x?.Trim())).ToList();
                switch (criteria.SearchType)
                {
                    case "VoucherNo":
                        data = data.Where(x => searchNoList.Any(z => z == x.VoucherNo));
                        break;
                    case "DocumentNo":
                        data = data.Where(x => searchNoList.Any(z => z == x.BillingNo));
                        break;
                    case "VatInv":
                        data = data.Where(x => searchNoList.Any(z => z == x.InvoiceNo));
                        break;
                }
            }
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                data = data.Where(x => x.PartnerId == criteria.PartnerId);
            }
            //if (criteria.FromPaymentDate != null)
            //{
            //    query = query.And(x => criteria.FromPaymentDate.Value.Date <= x.PaymentDueDate.Value.Date && x.PaymentDueDate.Value.Date <= criteria.ToPaymentDate.Value.Date);
            //}
            if (criteria.Office != null && criteria.Office.Count > 0)
            {
                data = data.Where(x => x.OfficeId != null && criteria.Office.Contains(x.OfficeId.ToString()));
            }
            //if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
            //{
            //    query = query.And(x => criteria.PaymentStatus.Any(z => z == x.Status));
            //}
            if (criteria.TransactionType != null && criteria.TransactionType.Count > 0)
            {
                data = data.Where(x => criteria.TransactionType.Any(z => z.ToLower().Contains(x.TransactionType.ToLower())));
            }
            // Lấy các dòng có refno
            {
                data = data.Where(x => !string.IsNullOrEmpty(x.ReferenceNo));
            }

            // Get data within 1 months if search without anything
            if (IsInitSearch(criteria))
            {
                var maxDate = (DataContext.Get().Max(x => x.VoucherDate) ?? DateTime.Now).AddDays(1).Date;
                var minDate = maxDate.AddMonths(-1).AddDays(-1).Date; // Start from 1 months ago
                data = data.Where(x => x.VoucherDate != null && (x.VoucherDate.Value > minDate && x.VoucherDate.Value < maxDate));
            }
            return data;
        }

        private IQueryable<AcctPayablePaymentDetailModel> GetDataAcctPayable(AccountPayableCriteria criteria, int? size = 1)
       {
            IQueryable<AcctPayablePaymentDetailModel> results = null;

            var payableData = QueryPayable(criteria);
            if(criteria.IsPaging == true)
            {
                payableData = payableData.Take(size ?? 1);
            }
            if (payableData == null) return null;
            var paymentData = accountPayablePaymentRepository.Get(x => x.PaymentType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE);
            var data = from payable in payableData
                       join payment in paymentData on payable.ReferenceNo equals payment.ReferenceNo into paymentGrp
                       from payment in paymentGrp.DefaultIfEmpty()
                       select new
                       {
                           RefId = payable.Id,
                           VoucherNo = payable.VoucherNo,
                           ReferenceNo = payable.ReferenceNo,
                           VoucherDate = payable.VoucherDate,
                           PaymentDueDate = payable.PaymentDueDate,
                           PartnerId = payable.PartnerId,
                           TransactionType = payable.TransactionType,
                           InvoiceNo = payable.InvoiceNo,
                           InvoiceDate = payable.InvoiceDate,
                           BillingNo = payable.BillingNo,
                           Currency = payable.Currency,
                           TotalAmount = payable.TotalAmount,
                           TotalAmountVnd = payable.TotalAmountVnd,
                           PaidAmountOrg = payable.PaymentAmount,
                           PaidAmountVnd = payable.PaymentAmountVnd,
                           PaymentTerm = payable.PaymentTerm,
                           payable.RemainAmount,
                           payable.RemainAmountVnd,
                           payable.Status,
                           payable.Description,

                           payment
                       };

            if (criteria.FromPaymentDate != null)
            {
                // Lấy AP có payment trong khoảng thời gian đã chọn theo voucher date
                var filterStatus = (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0);
                var apInRangeDates = data.Where(x => DateTime.Parse(criteria.FromPaymentDate).Date <= x.VoucherDate.Value.Date && x.VoucherDate.Value.Date <= DateTime.Parse(criteria.ToPaymentDate).Date &&
                (x.payment == null || !string.IsNullOrEmpty(x.payment.PaymentNo)) && (!filterStatus || criteria.PaymentStatus.Any(status => status == x.Status))).Select(x => new { x, InRangeType = "InRange" });
                // Lấy AP trước khoảng thời gian đã chọn nhưng payment nằm trong khoảng thời gian
                var paymentInRangedates = data.Where(x => (DateTime.Parse(criteria.FromPaymentDate).Date > x.VoucherDate.Value.Date) && x.payment != null && 
                (DateTime.Parse(criteria.FromPaymentDate).Date <= x.payment.PaymentDate.Value.Date && x.payment.PaymentDate.Value.Date <= DateTime.Parse(criteria.ToPaymentDate).Date)).Select(x => new { x, InRangeType = "PMInRange" });

                var dataGrp = apInRangeDates;
                if (paymentInRangedates != null && paymentInRangedates.FirstOrDefault() != null)
                {
                    if (filterStatus) // Filter status cho PMInRange
                    {
                        paymentInRangedates = paymentInRangedates.Where(d => (d.x.payment == null && criteria.PaymentStatus.Any(status => status == d.x.Status)) || (!string.IsNullOrEmpty(d.x.payment.PaymentNo) && criteria.PaymentStatus.Any(status => status == d.x.payment.Status)));
                    }
                    if (dataGrp != null && dataGrp.FirstOrDefault() != null)
                    {
                        dataGrp = dataGrp.Union(paymentInRangedates);
                    }
                    else
                    {
                        dataGrp = paymentInRangedates;
                    }
                }

                results = dataGrp.Where(dt => dt.x.RefId != null).Select(dt => new AcctPayablePaymentDetailModel
                {
                    RefId = dt.x.RefId.ToString(),
                    VoucherNo = dt.x.VoucherNo,
                    ReferenceNo = dt.x.ReferenceNo,
                    VoucherDate = dt.x.VoucherDate,
                    PaymentDueDate = dt.x.PaymentDueDate,
                    PartnerId = dt.x.PartnerId,
                    TransactionType = dt.x.TransactionType,
                    InvoiceNo = dt.x.InvoiceNo,
                    InvoiceDate = dt.x.InvoiceDate,
                    BillingNo = dt.x.BillingNo,
                    Currency = dt.x.Currency,
                    TotalAmount = dt.x.TotalAmount,
                    TotalAmountVnd = dt.x.TotalAmountVnd,
                    PaidAmountOrg = dt.x.PaidAmountOrg,
                    PaidAmountVnd = dt.x.PaidAmountVnd,
                    PaymentTerm = dt.x.PaymentTerm,
                    OrgRemainAmount = dt.x.RemainAmount,
                    RemainAmountVnd = dt.x.RemainAmountVnd,
                    Status = dt.x.Status,
                    Description = dt.x.Description,

                    PaymentNo = dt.x.payment == null ? string.Empty : dt.x.payment.PaymentNo,
                    PaymentType = dt.x.payment == null ? string.Empty : dt.x.payment.PaymentType,
                    PaymentDate = dt.x.payment == null ? null : dt.x.payment.PaymentDate,
                    PaymentAmount = dt.x.payment == null ? null : dt.x.payment.PaymentAmount,
                    PaymentAmountVnd = dt.x.payment == null ? null : dt.x.payment.PaymentAmountVnd,
                    PaymentRemainAmount = dt.x.payment == null ? null : dt.x.payment.RemainAmount,
                    PaymentRemainAmountVnd = dt.x.payment == null ? null : dt.x.payment.RemainAmountVnd,
                    StatusPayment = dt.x.payment == null ? string.Empty : dt.x.payment.Status,
                    PaymentDatetimeCreated = dt.x.payment == null ? null : dt.x.payment.DatetimeCreated,
                    CurrencyPayment = dt.x.payment == null ? string.Empty : dt.x.payment.Currency,
                    PaymentAcctId = dt.x.payment == null ? string.Empty : dt.x.payment.AcctId,

                    InRangeType = dt.InRangeType
                });

                // Lấy AP chưa trả hết trước khoảng thời gian đã chọn và payment không nằm trong khoảng thời gian
                var apUnpaidNotInRangeDates = data.Where(x => x.RefId != null && (DateTime.Parse(criteria.FromPaymentDate).Date > x.VoucherDate.Value.Date) && (x.payment == null || (DateTime.Parse(criteria.FromPaymentDate).Date > x.payment.PaymentDate.Value.Date && x.payment.Status != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID))).Select(dt => new AcctPayablePaymentDetailModel
                {
                    RefId = dt.RefId.ToString(),
                    VoucherNo = dt.VoucherNo,
                    ReferenceNo = dt.ReferenceNo,
                    VoucherDate = dt.VoucherDate,
                    PaymentDueDate = dt.PaymentDueDate,
                    PartnerId = dt.PartnerId,
                    TransactionType = dt.TransactionType,
                    InvoiceNo = dt.InvoiceNo,
                    InvoiceDate = dt.InvoiceDate,
                    BillingNo = dt.BillingNo,
                    Currency = dt.Currency,
                    TotalAmount = dt.TotalAmount,
                    TotalAmountVnd = dt.TotalAmountVnd,
                    PaidAmountOrg = dt.PaidAmountOrg,
                    PaidAmountVnd = dt.PaidAmountVnd,
                    PaymentTerm = dt.PaymentTerm,
                    OrgRemainAmount = dt.RemainAmount,
                    RemainAmountVnd = dt.RemainAmountVnd,
                    Status = dt.payment == null ? AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID : AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART,
                    Description = dt.Description,

                    PaymentNo = dt.payment == null ? string.Empty : dt.payment.PaymentNo,
                    PaymentType = dt.payment == null ? string.Empty : dt.payment.PaymentType,
                    PaymentDate = dt.payment == null ? null : dt.payment.PaymentDate,
                    PaymentAmount = dt.payment == null ? null : dt.payment.PaymentAmount,
                    PaymentAmountVnd = dt.payment == null ? null : dt.payment.PaymentAmountVnd,
                    PaymentRemainAmount = dt.payment == null ? null : dt.payment.RemainAmount,
                    PaymentRemainAmountVnd = dt.payment == null ? null : dt.payment.RemainAmountVnd,
                    StatusPayment = dt.payment == null ? string.Empty : dt.payment.Status,
                    PaymentDatetimeCreated = dt.payment == null ? null : dt.payment.DatetimeCreated,
                    CurrencyPayment = dt.payment == null ? string.Empty : dt.payment.Currency,
                    PaymentAcctId = dt.payment == null ? string.Empty : dt.payment.AcctId,

                    InRangeType = "OutRange"
                });
                if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
                {
                    apUnpaidNotInRangeDates = apUnpaidNotInRangeDates.Where(d => criteria.PaymentStatus.Any(status => status == d.Status));
                }
                if (apUnpaidNotInRangeDates != null && apUnpaidNotInRangeDates.FirstOrDefault() != null)
                {
                    if (results != null && results.FirstOrDefault() != null)
                    {
                        results = results.Union(apUnpaidNotInRangeDates);
                    }
                    else
                    {
                        results = apUnpaidNotInRangeDates;
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IQueryable<AcctPayablePaymentExport> GetDataExportPayablePaymentDetail(AccountPayableCriteria criteria)
        {
            var data = GetDataAcctPayable(criteria).AsEnumerable();
            //var grpData = dataList.Where(x => !string.IsNullOrEmpty(x.ReferenceNo)).OrderBy(x => x.VoucherDate).GroupBy(x => new { x.PartnerId, x.VoucherNo, x.VoucherDate, x.InvoiceNo, x.BillingNo, x.TransactionType, x.ReferenceNo });
            var grpData = from dt in data
                          where !string.IsNullOrEmpty(dt.ReferenceNo)
                          orderby dt.VoucherDate
                          group dt by new { dt.PartnerId, dt.VoucherNo, dt.VoucherDate, dt.InvoiceNo, dt.BillingNo, dt.TransactionType, dt.ReferenceNo } into dataGrp
                          select dataGrp.ToList();
            var result = new List<AcctPayablePaymentExport>();
            var partnerData = catPartnerRepository.Get(x => x.Active == true);

            foreach (var item in grpData)
            {
                var payable = new AcctPayablePaymentExport();
                var _key = item.FirstOrDefault();
                var partner = partnerData.FirstOrDefault(x => x.Id == _key.PartnerId);
                payable.AcctRefNo = _key.VoucherNo;
                payable.AcctDate = _key.VoucherDate;
                payable.PartnerName = partner?.ShortName;
                payable.AccountNo = partner?.AccountNo;
                payable.TransactionType = _key.TransactionType;
                var payableDetail = item.Select(x => x);
                payable.InvoiceNo = payableDetail.FirstOrDefault()?.InvoiceNo;
                if (!string.IsNullOrEmpty(payable.InvoiceNo)) // [CR 15/03/22: Leyla invoice date null or =01/01/1900 => inv date = acct date + pm term ]
                {
                    payable.Invoicedate = payableDetail.FirstOrDefault().InvoiceDate == null || payableDetail.FirstOrDefault().InvoiceDate.Value.Date == DateTime.Parse("01/01/1900").Date ? payableDetail.FirstOrDefault().PaymentDueDate : payableDetail.FirstOrDefault().InvoiceDate;
                }
                else
                {
                    payable.Invoicedate = null;
                }
                payable.DocNo = _key.BillingNo;
                
                payable.PaymentTerm = payableDetail.FirstOrDefault().PaymentTerm;
                payable.PaymentDueDate = payableDetail.FirstOrDefault().PaymentDueDate;
                payable.Status = payableDetail.FirstOrDefault().Status;
                payable.Description = payableDetail.FirstOrDefault().Description;

                var payableType = payableDetail.FirstOrDefault().InRangeType;
                switch (payableType)
                {
                    case "InRange":
                    case "PMInRange":
                        // Transaction AP Phát sinh trong thời gian được chọn => sẽ để trống
                        payable.BeginAmount = payableDetail.FirstOrDefault().TotalAmount ?? 0;
                        payable.BeginAmountVND = payableDetail.FirstOrDefault().TotalAmountVnd ?? 0;
                        break;
                    //case "PMInRange":
                    //    // Transaction AP phát sinh trước thời gian được chọn và có AP payment trong khoảng thời gian được chọn => lấy remain amount của payment cũ nhất
                    //    payable.BeginAmount = item.x.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.OrgRemainAmount ?? 0;
                    //    payable.BeginAmountVND = item.x.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.RemainAmountVnd ?? 0;
                    //    break;
                    case "OutRange":
                        // Transaction AP chưa trả hết(Unpaid/ Paid a Part) trước và không có payment trướcng khoảng thời được chọn => lấy remain amont current của AP đó, Trường hợp remain không có giá trị sẽ lấy Total Amount
                        payable.BeginAmount = (payableDetail.FirstOrDefault().OrgRemainAmount == null || payableDetail.FirstOrDefault().OrgRemainAmount == 0) ? payableDetail.FirstOrDefault().TotalAmount : payableDetail.FirstOrDefault().OrgRemainAmount;
                        payable.BeginAmountVND = (payableDetail.FirstOrDefault().RemainAmountVnd == null || payableDetail.FirstOrDefault().RemainAmountVnd == 0) ? payableDetail.FirstOrDefault().TotalAmountVnd : payableDetail.FirstOrDefault().RemainAmountVnd;
                        break;
                }
                payable.OriginCurrency = payableDetail.FirstOrDefault()?.Currency;

                payable.PaymentDetails = new List<AcctPayablePaymentDetail>();
                var advValue = _key.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE ? (-1) : 1; // dòng adv hiện giá trị âm
                if (payableDetail.FirstOrDefault(z => !string.IsNullOrEmpty(z.PaymentNo)) != null)
                {
                    var paymentGrp = payableDetail.Where(x => x.TransactionType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE || (x.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE && x.PaymentType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE)).OrderBy(x => x.PaymentDatetimeCreated).GroupBy(x => new { x.PaymentAcctId, x.PaymentDate, x.PaymentType }).Select(x => new AcctPayablePaymentDetail
                    {
                        PaymentRefNo = x.FirstOrDefault().PaymentNo,
                        PaymentDate = x.Key.PaymentDate,
                        PaymentDatetimeCreated = x.FirstOrDefault().PaymentDatetimeCreated,
                        OrgPaidAmount = (x.Sum(z => z.PaymentAmount ?? 0)) * advValue,
                        PaidAmountVND = x.Sum(z => z.PaymentAmountVnd ?? 0) * advValue,
                        //OriginRemainAmount = (payableType == "InRange" ? (payable.BeginAmount - (x.FirstOrDefault().PaidAmountOrg ?? 0)) : x.FirstOrDefault().PaymentRemainAmount ?? 0) * advValue,
                        //RemainAmountVND = (payableType == "InRange" ? (payable.BeginAmountVND - (x.FirstOrDefault().PaymentAmountVnd ?? 0)) : x.FirstOrDefault().PaymentRemainAmountVnd ?? 0) * advValue,
                        OriginRemainAmount = (x.OrderByDescending(z => z.PaymentDatetimeCreated).FirstOrDefault().PaymentRemainAmount ?? 0) * advValue,
                        RemainAmountVND = (x.OrderByDescending(z => z.PaymentDatetimeCreated).FirstOrDefault().PaymentRemainAmountVnd ?? 0) * advValue,
                        OriginCurrency = x.FirstOrDefault().CurrencyPayment
                    });
                    if (payableType == "OutRange")
                    {
                        var outRangePM = paymentGrp.Where(x => (DateTime.Parse(criteria.FromPaymentDate).Date > x.PaymentDate.Value.Date)).OrderByDescending(x => x.PaymentDate).ThenByDescending(x => x.PaymentDatetimeCreated).FirstOrDefault();
                        if (outRangePM != null && outRangePM.OriginRemainAmount != null) // nếu có payment gần cận dưới from date thì lấy remain của payment đó
                        {
                            payable.BeginAmount = outRangePM.OriginRemainAmount;
                            payable.BeginAmountVND = outRangePM.RemainAmountVND;
                        }
                    }
                    else
                    {
                        payable.PaymentDetails.AddRange(paymentGrp.OrderBy(x => x.PaymentDate).ThenBy(x => x.PaymentDatetimeCreated));
                    }
                }
                payable.BeginAmount *= advValue;
                payable.BeginAmountVND *= advValue;
                result.Add(payable);
            }
            return result.OrderBy(x => x.PartnerName).ThenBy(x => x.AcctDate).ThenBy(x => x.AcctRefNo).ThenBy(x => x.BillingNo).AsQueryable();
        }

        /// <summary>
        /// Get data export accounting template payable
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public List<AccountingTemplateExport> GetDataExportAccountingTemplate(AccountPayableCriteria criteria)
        {
            var data = GetDataAcctPayable(criteria);
            var dataList = new List<AcctPayablePaymentDetailModel>();
            dataList.AddRange(data);
            var grpData = dataList.Where(x => !string.IsNullOrEmpty(x.ReferenceNo)).GroupBy(payable => new { payable.PartnerId }).Select(payable => new { payable.Key, payable });
            //var grpData = from payable in data
            //              where !string.IsNullOrEmpty(payable.ReferenceNo)
            //              group payable by payable.PartnerId into dataGrp
            //              select dataGrp;
            var result = new List<AccountingTemplateExport>();
            //var partnerIds = grpData.Select(x => x.Key.PartnerId);
            var partnerData = catPartnerRepository.Get(x => x.Active == true);
            var office = sysOfficeRepository.Get(x => x.Id == currentUser.OfficeID).FirstOrDefault();
            var officeName = office?.BranchNameEn?.ToUpper();
            var _contactOffice = string.Format("{0}\nTel: {1}  Fax: {2}\nE-mail: {3}", office?.AddressEn, office?.Tel, office?.Fax, office?.Email);

            foreach (var item in grpData)
            {
                var grpPayable = item.payable.GroupBy(x => new { x.VoucherNo, x.VoucherDate, x.InvoiceNo, x.BillingNo, x.TransactionType, x.ReferenceNo });
                //var _key = item.FirstOrDefault();
                var partner = partnerData.Where(x => x.Id == item.Key.PartnerId).FirstOrDefault();
                foreach (var payableItm in grpPayable)
                {
                    var detail = new AccountingTemplateExport();
                    detail.OfficeName = officeName;
                    detail.ContactOffice = _contactOffice;
                    detail.PartnerId = payableItm.FirstOrDefault().PartnerId;
                    detail.PartnerName = partner?.ShortName;
                    detail.Code = payableItm.FirstOrDefault().VoucherNo?.Length > 1 ? payableItm.FirstOrDefault().VoucherNo.Substring(0, 2) : string.Empty; // Lấy 2 kí tự đầu mã CT
                    detail.Code = detail.Code.All(char.IsNumber) ? string.Empty : detail.Code;
                    detail.VoucherNo = payableItm.Key.VoucherNo;
                    detail.VoucherDate = payableItm.Key.VoucherDate;
                    detail.InvoiceNo = payableItm.Key.InvoiceNo;
                    if (!string.IsNullOrEmpty(detail.InvoiceNo))
                    {
                        detail.InvoiceDate = payableItm.FirstOrDefault().InvoiceDate == null || payableItm.FirstOrDefault().InvoiceDate.Value.Date == DateTime.Parse("01/01/1900").Date ? payableItm.FirstOrDefault().PaymentDueDate : payableItm.FirstOrDefault().InvoiceDate;
                    }
                    else
                    {
                        detail.InvoiceDate = null;
                    }
                    detail.DocNo = payableItm.Key.BillingNo;
                    detail.AccountNo = partner?.AccountNo;
                    detail.Currency = payableItm.FirstOrDefault().Currency;
                    detail.TransactionType = payableItm.Key.TransactionType;
                    detail.Description = payableItm.FirstOrDefault().Description;
                    detail.PaymentTerm = payableItm.FirstOrDefault().PaymentTerm;
                    detail.PaymentDueDate = payableItm.FirstOrDefault().PaymentDueDate;
                    detail.Status = payableItm.FirstOrDefault().Status;
                    detail.BeginAmount = payableItm.FirstOrDefault().InRangeType == "InRange" ? 0 : (payableItm.FirstOrDefault().TotalAmount ?? 0); // Hiện số dư đầu kì cho transaction ngoài kì
                    detail.BeginAmountVND = payableItm.FirstOrDefault().InRangeType == "InRange" ? 0 : (payableItm.FirstOrDefault().TotalAmountVnd ?? 0); // Hiện số dư đầu kì cho transaction ngoài kì
                    detail.OrgAmountTang = payableItm.FirstOrDefault().InRangeType != "InRange" ? 0 : (payableItm.FirstOrDefault().TotalAmount ?? 0); // Hiện số dư đầu kì cho transaction ngoài kì
                    detail.OrgAmountTangVND = payableItm.FirstOrDefault().InRangeType != "InRange" ? 0 : (payableItm.FirstOrDefault().TotalAmountVnd ?? 0); // Hiện số dư đầu kì cho transaction ngoài kì
                    detail.OrgAmountGiam = detail.OrgAmountGiamVND = 0;

                    if (payableItm.Key.TransactionType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE)
                    {
                        var paymentGrp = payableItm.GroupBy(x => new { x.PaymentAcctId, x.PaymentDate, x.PaymentType }).Select(x => new { pm = x.Select(z => new { z.PaymentNo, z.PaymentAmount, z.PaymentAmountVnd, z.PaymentRemainAmount, z.PaymentRemainAmountVnd, z.PaymentDate, z.PaymentDatetimeCreated }) });

                        if (payableItm.FirstOrDefault().InRangeType == "OutRange")
                        {
                            var outRangePM = paymentGrp.Where(x => string.IsNullOrEmpty(x.pm.FirstOrDefault().PaymentNo) || (DateTime.Parse(criteria.FromPaymentDate).Date > x.pm.FirstOrDefault().PaymentDate.Value.Date)).OrderByDescending(x => x.pm.FirstOrDefault().PaymentDate).ThenByDescending(x => x.pm.FirstOrDefault().PaymentDatetimeCreated).FirstOrDefault();
                            if (outRangePM != null && outRangePM.pm.FirstOrDefault()?.PaymentRemainAmount != null)
                            {
                                detail.BeginAmount = outRangePM.pm.FirstOrDefault().PaymentRemainAmount;
                                detail.BeginAmountVND = outRangePM.pm.FirstOrDefault().PaymentRemainAmountVnd;
                            }
                        }
                        else
                        {
                            if (paymentGrp != null &&  paymentGrp.FirstOrDefault() != null && paymentGrp.FirstOrDefault().pm.FirstOrDefault()?.PaymentAmount != null)
                            {
                                detail.OrgAmountGiam = paymentGrp.Sum(x => x.pm.Sum(z => z.PaymentAmount ?? 0));
                                detail.OrgAmountGiamVND = paymentGrp.Sum(x => x.pm.Sum(z => z.PaymentAmountVnd ?? 0));
                            }
                        }
                    }
                    else
                    {
                        detail.BeginAmount *= (-1);
                        detail.BeginAmountVND *= (-1);
                        detail.OrgAmountTang *= (-1);
                        detail.OrgAmountTangVND *= (-1);
                        var paymentGrp = payableItm.Where(x => x.PaymentType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE).GroupBy(x => new { x.PaymentAcctId, x.PaymentDate, x.PaymentType }).Select(x => new { pm = x.Select(z => new { z.PaymentNo, z.PaymentAmount, z.PaymentAmountVnd, z.PaymentRemainAmount, z.PaymentRemainAmountVnd, z.PaymentDate, z.PaymentDatetimeCreated }) });

                        if (payableItm.FirstOrDefault().InRangeType == "OutRange")
                        {
                            var outRangePM = paymentGrp.Where(x => string.IsNullOrEmpty(x.pm.FirstOrDefault().PaymentNo) || (DateTime.Parse(criteria.FromPaymentDate).Date > x.pm.FirstOrDefault().PaymentDate.Value.Date)).OrderByDescending(x => x.pm.FirstOrDefault().PaymentDate).ThenByDescending(x => x.pm.FirstOrDefault().PaymentDatetimeCreated).FirstOrDefault();
                            if (outRangePM != null && outRangePM.pm.FirstOrDefault()?.PaymentRemainAmount != null)
                            {
                                detail.BeginAmount = outRangePM.pm.FirstOrDefault().PaymentRemainAmount * (-1);
                                detail.BeginAmountVND = outRangePM.pm.FirstOrDefault().PaymentRemainAmountVnd * (-1);
                            }
                        }
                        else
                        {
                            if (paymentGrp != null && paymentGrp.FirstOrDefault() != null && paymentGrp.FirstOrDefault().pm.FirstOrDefault()?.PaymentAmount != null)
                            {
                                detail.OrgAmountGiam = paymentGrp.Sum(x => x.pm.Sum(z => z.PaymentAmount ?? 0)) * (-1);
                                detail.OrgAmountGiamVND = paymentGrp.Sum(x => x.pm.Sum(z => z.PaymentAmountVnd ?? 0)) * (-1);
                            }
                        }
                    }
                    result.Add(detail);
                }
            }
            return result.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ThenBy(x => x.DocNo).ToList();
        }

        //private decimal RateAmount(decimal? amountVND,decimal? amountUSD,string currencySrc,string currencyDest)
        //{
        //    var rate = exchangeRateRepository.Get().OrderByDescending(x => x.DatetimeCreated).FirstOrDefault().Rate;
        //    if (amountVND != 0 && amountUSD == 0 && currencyDest=="USD" && currencySrc=="VND")
        //    {
        //        return (decimal)amountVND / rate;
        //    }else if(amountVND == 0 && amountUSD != 0 && currencyDest=="VND" && currencySrc=="USD")
        //    {
        //        return (decimal)amountUSD * rate;
        //    }
        //    return currencyDest == "VND" ? (decimal)amountVND : (decimal)amountUSD;
        //}

        public GeneralAccPayableModel GetGeneralPayable(string partnerId,string currency)
        {
            var accPayable = DataContext.Get(x => x.PartnerId == partnerId && !string.IsNullOrEmpty(x.ReferenceNo.Trim())).ToList();
            var crdAvdAmount = currency=="VND"?DataContext.Get(x => x.PartnerId == partnerId && x.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE && x.Status!="Paid"&&!string.IsNullOrEmpty(x.ReferenceNo.Trim())).Sum(x => x.TotalAmountVnd): DataContext.Get(x => x.PartnerId == partnerId && x.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE && x.Status != "Paid").Sum(x => x.TotalAmountUsd);
            var partner = catPartnerRepository.Get(x => x.Id == partnerId).FirstOrDefault();

            return new GeneralAccPayableModel
            {
                CreditAmount = currency == "VND" ? accPayable.Where(x=> x.Status != "Paid" && (x.TransactionType == "OBH" || x.TransactionType == "CREDIT")).Sum(x => x.TotalAmountVnd)-crdAvdAmount : accPayable.Where(x => x.Status != "Paid" && (x.TransactionType == "OBH" || x.TransactionType == "CREDIT")).Sum(x => x.TotalAmountUsd)-crdAvdAmount,
                CreditPaidAmount = currency == "VND" ? accPayable.Where(x => x.Status == "Paid A Part" && (x.TransactionType == "OBH" || x.TransactionType == "CREDIT")).Sum(x => x.PaymentAmountVnd) : accPayable.Where(x => x.Status == "Paid A Part" && (x.TransactionType == "OBH" || x.TransactionType == "CREDIT")).Sum(x => x.PaymentAmountUsd),
                CreditUnpaidAmount = currency == "VND" ? accPayable.Where(x=>x.Status!="Paid"&& (x.TransactionType == "OBH" || x.TransactionType == "CREDIT")).Sum(x => x.RemainAmountVnd) : accPayable.Where(x => x.Status != "Paid" && (x.TransactionType == "OBH" || x.TransactionType == "CREDIT")).Sum(x => x.RemainAmountUsd),
                //CreditAmount = accPayable.Sum(x => RateAmount(x.TotalAmountVnd, x.TotalAmountUsd, x.Currency, currency)),
                //CreditPaidAmount = accPayable.Where(x => x.Status == "Paid" || x.Status == "Paid A Part").Sum(x => RateAmount(x.PaymentAmountVnd, x.PaymentAmountUsd, x.Currency, currency)),
                //CreditUnpaidAmount = accPayable.Sum(x => RateAmount(x.RemainAmountVnd, x.RemainAmountUsd, x.Currency, currency)),
                Currency = partner.Currency,
                PaymentTerm = partner.PaymentTerm==null?0: partner.PaymentTerm,
                CreditAdvanceAmount = crdAvdAmount
            };
        }
    }
}
