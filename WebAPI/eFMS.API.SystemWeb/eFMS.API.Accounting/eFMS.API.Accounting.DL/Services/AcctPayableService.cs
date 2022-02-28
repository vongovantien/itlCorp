using System;
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
        public AcctPayableService(
            IContextBase<AccAccountPayable> repository,
            IMapper mapper,
            ICurrentUser curUser,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<AccAccountPayablePayment> accountPayablePaymentRepo,
            IOptions<WebUrl> wUrl

            ) : base(repository, mapper)
        {
            currentUser = curUser;
            stringLocalizer = localizer;
            catPartnerRepository = catPartnerRepo;
            accountPayablePaymentRepository = accountPayablePaymentRepo;
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
            var data = GenerateDataPaging(criteria);

            var _totalItem = 0;
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
                _totalItem = data.Count();
                data = data.Skip((page - 1) * size).Take(size);
            }
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IQueryable<AccAccountPayableModel> GenerateDataPaging(AccountPayableCriteria criteria)
        {
            var data = GetDataAcctPayable(criteria);
            if (data == null || data.Count() == 0)
            {
                return null;
            }
            var payables = from payable in data
                           join partner in catPartnerRepository.Get(x => x.Active == true) on payable.PartnerId equals partner.Id
                           select new { payable, partner.ShortName, partner.AccountNo };
            var payableGrp = payables.GroupBy(x => new {x.payable.RefId, x.payable.PartnerId, x.payable.VoucherNo, x.payable.VoucherDate, x.payable.InvoiceNo, x.payable.BillingNo, x.payable.TransactionType });

            var acctPayables = new List<AccAccountPayableModel>();
            foreach(var item in payableGrp)
            {
                var acct = new AccAccountPayableModel();
                acct.RefId = item.Key.RefId;
                acct.ReferenceNo = item.Key.VoucherNo;
                acct.PartnerName = item.FirstOrDefault().ShortName;
                acct.AccountNo = item.FirstOrDefault().AccountNo;
                acct.TransactionType = item.FirstOrDefault().payable.TransactionType;
                acct.VoucherDate = item.Key.VoucherDate;
                acct.InvoiceNo = item.Key.InvoiceNo;
                acct.InvoiceDate = !string.IsNullOrEmpty(item.Key.InvoiceNo) ? item.FirstOrDefault().payable.InvoiceDate : null;
                acct.BillingNo = item.FirstOrDefault().payable.BillingNo;
                acct.Currency = item.FirstOrDefault().payable.Currency;
                acct.PaymentTerm = item.FirstOrDefault().payable.PaymentTerm;
                acct.PaymentDueDate = item.FirstOrDefault().payable.PaymentDueDate;
                var paymentAmount = GetAmountWithCurrency(string.Empty, item.Select(z => z.payable).AsQueryable());
                acct.TotalAmount = paymentAmount[0];
                acct.PaymentAmount = paymentAmount[1];
                acct.RemainAmount = paymentAmount[2];
                var paymentAmountVnd = GetAmountWithCurrency(AccountingConstants.CURRENCY_LOCAL, item.Select(z => z.payable).AsQueryable());
                acct.TotalAmountVnd = paymentAmountVnd[0];
                acct.PaymentAmountVnd = paymentAmountVnd[1];
                acct.RemainAmountVnd = paymentAmountVnd[2];
                acctPayables.Add(acct);
            }
            return acctPayables.AsQueryable();
        }

        /// <summary>
        /// Get amount with currency of payment
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private decimal?[] GetAmountWithCurrency(string currency, IQueryable<AcctPayablePaymentDetailModel> data)
        {
            var paymentdata = data.Where(x => x.TransactionType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE || (x.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE && x.PaymentType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE)).OrderBy(x => x.PaymentDatetimeCreated).GroupBy(x => new { x.PaymentNo, x.PaymentDate, x.PaymentType });
            string type = data.FirstOrDefault().InRangeType;
            decimal? total = 0;
            decimal? paidAmount = 0;
            decimal? remainAmount = 0;
            switch (type)
            {
                case "InRange":
                    // Transaction AP Phát sinh trong thời gian được chọn => sẽ để trống
                    total = null;
                    break;
                case "PMInRange":
                    // Transaction AP phát sinh trước thời gian được chọn và có AP payment trong khoảng thời gian được chọn => lấy remain amount của payment cũ nhất
                    if (currency == AccountingConstants.CURRENCY_LOCAL)
                    {
                        total = data.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.RemainAmountVnd ?? 0;
                    }
                    else if (currency == AccountingConstants.CURRENCY_USD)
                    {
                        total = data.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.RemainAmountUsd ?? 0;
                    }
                    else
                    {
                        total = data.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.OrgRemainAmount ?? 0;
                    }
                    break;
                case "OutRange":
                    // Transaction AP chưa trả hết(Unpaid/ Paid a Part) trước và không có payment trướcng khoảng thời được chọn => lấy remain amont current của AP đó, Trường hợp remain không có giá trị sẽ lấy Total Amount
                    if (currency == AccountingConstants.CURRENCY_LOCAL)
                    {
                        total = (data.FirstOrDefault().RemainAmountVnd == null || data.FirstOrDefault().RemainAmountVnd == 0) ? data.FirstOrDefault().TotalAmountVnd : data.FirstOrDefault().RemainAmountVnd;
                    }
                    else if (currency == AccountingConstants.CURRENCY_USD)
                    {
                        total = (data.FirstOrDefault().RemainAmountUsd == null || data.FirstOrDefault().RemainAmountUsd == 0) ? data.FirstOrDefault().TotalAmountUsd : data.FirstOrDefault().RemainAmountUsd;
                    }
                    else
                    {
                        total = (data.FirstOrDefault().OrgRemainAmount == null || data.FirstOrDefault().OrgRemainAmount == 0) ? data.FirstOrDefault().TotalAmount : data.FirstOrDefault().OrgRemainAmount;
                    }
                    break;
            }
            if (currency == AccountingConstants.CURRENCY_LOCAL)
            {
                foreach (var item in paymentdata)
                {
                    paidAmount += item.FirstOrDefault().PaymentAmountVnd;
                    remainAmount += item.FirstOrDefault().PaymentRemainAmountVnd;
                }
            }
            else if (currency == AccountingConstants.CURRENCY_USD)
            {
                foreach (var item in paymentdata)
                {
                    paidAmount += item.FirstOrDefault().PaymentAmountUsd;
                    remainAmount += item.FirstOrDefault().PaymentRemainAmountUsd;
                }
            }
            else
            {
                foreach (var item in paymentdata)
                {
                    paidAmount += item.FirstOrDefault().PaymentAmount;
                    remainAmount += item.FirstOrDefault().PaymentRemainAmount;
                }
            }

            return new[] { total, paidAmount, remainAmount };
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
        public IQueryable<AccAccountPayablePaymentModel> GetBy(string refNo, string type, string invoiceNo, string billingNo)
        {
            var payabledata = DataContext.Get(x => x.VoucherNo == refNo && x.TransactionType == type && (string.IsNullOrEmpty(x.InvoiceNo) || x.InvoiceNo == invoiceNo) && x.BillingNo == billingNo).FirstOrDefault();
            if (payabledata == null) return null;
            var paymentData = accountPayablePaymentRepository.Get(x => x.ReferenceNo == payabledata.ReferenceNo && (payabledata.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE ? x.PaymentType == AccountingConstants.PAYMENT_TYPE_NAME_NET_OFF : x.PaymentType == payabledata.TransactionType)).OrderBy(x => x.PaymentNo)
                .Select(x => new AccAccountPayablePaymentModel
                {
                    RefNo = x.PaymentNo,
                    PaymentDate = x.PaymentDate,
                    InvoiceNo = payabledata.InvoiceNo,
                    InvoiceDate = !string.IsNullOrEmpty(payabledata.InvoiceNo) ? payabledata.InvoiceDate : null,
                    DocNo = payabledata.BillingNo,
                    PaymentAmount = x.PaymentAmount,
                    RemainAmount = x.RemainAmount,
                    PaymentAmountVnd = x.PaymentAmountVnd,
                    RemainAmountVnd = x.RemainAmountVnd,
                    Currency = x.Currency
                });
            //var data = payabledata.Join(paymentData, pm => pm.ReferenceNo, re => re.ReferenceNo, (pm, re) => new { pm.VoucherNo, pm.TransactionType, pm.InvoiceNo, pm.InvoiceDate, pm.BillingNo, re.ReferenceNo, re.PaymentNo, re.PaymentDate, re.PaymentAmount, re.RemainAmount, re.PaymentAmountVnd, re.RemainAmountVnd, re.Currency });
            //var grpData = data.GroupBy(x => new { x.VoucherNo, x.InvoiceNo, x.ReferenceNo }).Select(x => new { x.Key, receipt = x.Select(z => z) });

            return paymentData;
        }


        private Expression<Func<AccAccountPayable, bool>> QueryPayable(AccountPayableCriteria criteria)
        {
            Expression<Func<AccAccountPayable, bool>> query = x => !string.IsNullOrEmpty(x.VoucherNo);
            if (!string.IsNullOrEmpty(criteria.SearchType) && !string.IsNullOrEmpty(criteria.ReferenceNos?.Trim()))
            {
                var searchNoList = criteria.ReferenceNos.Split('\n').Where(x => !string.IsNullOrEmpty(x?.Trim())).ToList();
                switch (criteria.SearchType)
                {
                    case "VoucherNo":
                        query = query.And(x => searchNoList.Any(z => z == x.VoucherNo));
                        break;
                    case "DocumentNo":
                        query = query.And(x => searchNoList.Any(z => z == x.BillingNo));
                        break;
                    case "VatInv":
                        query = query.And(x => searchNoList.Any(z => z == x.InvoiceNo));
                        break;
                }
            }
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(x => x.PartnerId == criteria.PartnerId);
            }
            //if (criteria.FromPaymentDate != null)
            //{
            //    query = query.And(x => criteria.FromPaymentDate.Value.Date <= x.PaymentDueDate.Value.Date && x.PaymentDueDate.Value.Date <= criteria.ToPaymentDate.Value.Date);
            //}
            if (criteria.Office != null && criteria.Office.Count > 0)
            {
                query = query.And(x => x.OfficeId != null && criteria.Office.Contains(x.OfficeId.ToString()));
            }
            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
            {
                query = query.And(x => criteria.PaymentStatus.Any(z => z == x.Status));
            }
            if (criteria.TransactionType != null && criteria.TransactionType.Count > 0)
            {
                query = query.And(x => criteria.TransactionType.Any(z => z.Contains(x.TransactionType)));
            }
            // Lấy các dòng có refno
            {
                query = query.And(x => !string.IsNullOrEmpty(x.ReferenceNo));
            }
            return query;
        }

        private IQueryable<AcctPayablePaymentDetailModel> GetDataAcctPayable(AccountPayableCriteria criteria)
        {
            IQueryable<AcctPayablePaymentDetailModel> results = null;

            var payableData = DataContext.Get(QueryPayable(criteria));
            if (payableData == null) return null;
            var paymentData = accountPayablePaymentRepository.Get();
            var data = from payable in payableData
                       join payment in paymentData on payable.ReferenceNo equals payment.ReferenceNo into paymentGrp
                       from payment in paymentGrp
                       select new AcctPayablePaymentDetailModel
                       {
                           RefId = payable.Id.ToString(),
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

                           PaymentNo = payment.PaymentNo,
                           PaymentDate = payment.PaymentDate,
                           PaymentAmount = payment.PaymentAmount,
                           PaymentAmountVnd = payment.PaymentAmountVnd,
                           PaymentRemainAmount = payment.RemainAmount,
                           PaymentRemainAmountVnd = payment.RemainAmountVnd,
                           StatusPayment = payment.Status,
                           PaymentDatetimeCreated = payment.DatetimeCreated,
                           CurrencyPayment = payment.Currency
                       };

            if (criteria.FromPaymentDate != null)
            {
                // Lấy AP có payment trong khoảng thời gian đã chọn theo voucher date
                var apInRangeDates = data.Where(x => DateTime.Parse(criteria.FromPaymentDate).Date <= x.VoucherDate.Value.Date && x.VoucherDate.Value.Date <= DateTime.Parse(criteria.ToPaymentDate).Date && !string.IsNullOrEmpty(x.PaymentNo)).Select(x => new { x, InRangeType = "InRange" });
                // Lấy AP trước khoảng thời gian đã chọn nhưng payment nằm trong khoảng thời gian
                var paymentInRangedates = data.Where(x => (DateTime.Parse(criteria.FromPaymentDate).Date > x.VoucherDate.Value.Date) && DateTime.Parse(criteria.FromPaymentDate).Date <= x.PaymentDate.Value.Date && x.PaymentDate.Value.Date <= DateTime.Parse(criteria.ToPaymentDate).Date).Select(x => new { x, InRangeType = "PMInRange" });
                // Lấy AP chưa trả hết trước khoảng thời gian đã chọn và payment không nằm trong khoảng thời gian
                var apUnpaidNotInRangeDates = data.Where(x => (DateTime.Parse(criteria.FromPaymentDate).Date > x.VoucherDate.Value.Date) && (x.Status != AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID) && DateTime.Parse(criteria.FromPaymentDate).Date > x.PaymentDate.Value.Date).Select(x => new { x, InRangeType = "OutRange" });
                var dataGrp = apInRangeDates;
                if (dataGrp.Count() > 0)
                {
                    dataGrp = dataGrp.Union(paymentInRangedates);
                }
                else
                {
                    dataGrp = paymentInRangedates;
                }
                if (dataGrp.Count() > 0)
                {
                    dataGrp = dataGrp.Union(apUnpaidNotInRangeDates);
                }
                else
                {
                    dataGrp = apUnpaidNotInRangeDates;
                }
                results = dataGrp.Select(dt => new AcctPayablePaymentDetailModel
                {
                    RefId = dt.x.RefId,
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

                    PaymentNo = dt.x.PaymentNo,
                    PaymentDate = dt.x.PaymentDate,
                    PaymentAmount = dt.x.PaymentAmount,
                    PaymentAmountVnd = dt.x.PaymentAmountVnd,
                    PaymentRemainAmount = dt.x.PaymentRemainAmount,
                    PaymentRemainAmountVnd = dt.x.PaymentRemainAmountVnd,
                    StatusPayment = dt.x.StatusPayment,
                    PaymentDatetimeCreated = dt.x.PaymentDatetimeCreated,
                    CurrencyPayment = dt.x.CurrencyPayment,
                    OrgRemainAmount = dt.x.OrgRemainAmount,

                    InRangeType = dt.InRangeType
                });
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public List<AcctPayablePaymentExport> GetDataExportPayablePaymentDetail(AccountPayableCriteria criteria)
        {
            var data = GetDataAcctPayable(criteria).Where(x => !string.IsNullOrEmpty(x.ReferenceNo));
            var grpData = data.GroupBy(x => new { x.PartnerId, x.VoucherNo, x.VoucherDate, x.InvoiceNo, x.BillingNo, x.TransactionType }).Select(x => new { x.Key, x });
            var result = new List<AcctPayablePaymentExport>();
            var partnerIds = data.Select(x => x.PartnerId).ToList();
            var partnerData = catPartnerRepository.Get(x => partnerIds.Any(z => z == x.Id));
            foreach (var item in grpData)
            {
                var payable = new AcctPayablePaymentExport();
                var partner = partnerData.Where(x => x.Id == item.Key.PartnerId).FirstOrDefault();
                payable.AcctRefNo = item.Key.VoucherNo;
                payable.AcctDate = item.Key.VoucherDate;
                payable.PartnerName = partner?.ShortName;
                payable.AccountNo = partner?.AccountNo;
                payable.TransactionType = item.Key.TransactionType;
                payable.InvoiceNo = item.x.FirstOrDefault()?.InvoiceNo;
                payable.Invoicedate = !string.IsNullOrEmpty(payable.InvoiceNo) ? item.x.FirstOrDefault()?.InvoiceDate : null;
                payable.DocNo = item.Key.BillingNo;
                payable.PaymentTerm = item.x.FirstOrDefault().PaymentTerm;
                payable.PaymentDueDate = item.x.FirstOrDefault().PaymentDueDate;

                switch (item.x.FirstOrDefault().InRangeType)
                {
                    case "InRange":
                        // Transaction AP Phát sinh trong thời gian được chọn => sẽ để trống
                        payable.BeginAmount = null;
                        payable.BeginAmountVND = null;
                        break;
                    case "PMInRange":
                        // Transaction AP phát sinh trước thời gian được chọn và có AP payment trong khoảng thời gian được chọn => lấy remain amount của payment cũ nhất
                        payable.BeginAmount = item.x.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.OrgRemainAmount ?? 0;
                        payable.BeginAmountVND = item.x.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.RemainAmountVnd ?? 0;
                        break;
                    case "OutRange":
                        // Transaction AP chưa trả hết(Unpaid/ Paid a Part) trước và không có payment trướcng khoảng thời được chọn => lấy remain amont current của AP đó, Trường hợp remain không có giá trị sẽ lấy Total Amount
                        payable.BeginAmount = (item.x.FirstOrDefault().OrgRemainAmount == null || item.x.FirstOrDefault().OrgRemainAmount == 0) ? item.x.FirstOrDefault().TotalAmount : item.x.FirstOrDefault().OrgRemainAmount;
                        payable.BeginAmountVND = (item.x.FirstOrDefault().RemainAmountVnd == null || item.x.FirstOrDefault().RemainAmountVnd == 0) ? item.x.FirstOrDefault().TotalAmountVnd : item.x.FirstOrDefault().RemainAmountVnd;
                        break;
                }
                payable.OriginCurrency = item.x.FirstOrDefault()?.Currency;

                payable.PaymentDetails = new List<AcctPayablePaymentDetail>();
                var paymentGrp = item.x.Where(x => x.TransactionType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE || (x.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE && x.PaymentType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE)).OrderBy(x => x.PaymentDatetimeCreated).GroupBy(x => new { x.PaymentNo, x.PaymentDate, x.PaymentType }).Select(x => new AcctPayablePaymentDetail
                {
                    PaymentRefNo = x.Key.PaymentNo,
                    PaymentDate = x.Key.PaymentDate,
                    OrgPaidAmount = x.FirstOrDefault().PaidAmountOrg ?? 0,
                    PaidAmountVND = x.FirstOrDefault().PaymentAmountVnd ?? 0,
                    OriginRemainAmount = x.FirstOrDefault().PaymentRemainAmount ?? 0,
                    RemainAmountVND = x.FirstOrDefault().PaymentRemainAmountVnd ?? 0,
                    OriginCurrency = x.FirstOrDefault().CurrencyPayment
                });
                payable.PaymentDetails.AddRange(paymentGrp);
                result.Add(payable);
            }
            return result;
        }
    }
}
