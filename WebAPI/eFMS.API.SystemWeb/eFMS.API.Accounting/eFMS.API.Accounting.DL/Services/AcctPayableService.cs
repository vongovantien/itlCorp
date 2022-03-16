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
        private readonly IContextBase<SysOffice> sysOfficeRepository;
        public AcctPayableService(
            IContextBase<AccAccountPayable> repository,
            IMapper mapper,
            ICurrentUser curUser,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<AccAccountPayablePayment> accountPayablePaymentRepo,
            IContextBase<SysOffice> sysOfficeRepo,
            IOptions<WebUrl> wUrl

            ) : base(repository, mapper)
        {
            currentUser = curUser;
            stringLocalizer = localizer;
            catPartnerRepository = catPartnerRepo;
            accountPayablePaymentRepository = accountPayablePaymentRepo;
            sysOfficeRepository = sysOfficeRepo;
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
        /// Get data paging in list detail
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
            //var payables = (from payable in data
            //               join partner in catPartnerRepository.Get(x => x.Active == true) on payable.PartnerId equals partner.Id into grpPartner
            //               from partner in grpPartner.DefaultIfEmpty()
            //               select new { payable, ShortName = partner != null ? partner.ShortName : string.Empty, AccountNo = partner != null ? partner.AccountNo : string.Empty }).ToList();
            var payableGrp = data.ToList().GroupBy(x => new { x.RefId, x.PartnerId, x.VoucherNo, x.VoucherDate, x.InvoiceNo, x.BillingNo, x.TransactionType }).ToList();

            var acctPayables = new List<AccAccountPayableModel>();
            var partnerData = catPartnerRepository.Get(x => x.Active == true);
            foreach (var item in payableGrp)
            {
                var acct = new AccAccountPayableModel();
                var advValue = item.Key.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE ? (-1) : 1; // dòng adv hiện giá trị âm
                var partner = partnerData.Where(x => x.Id == item.Key.PartnerId).FirstOrDefault();
                acct.RefId = item.Key.RefId?.ToString(); ;
                acct.ReferenceNo = item.Key.VoucherNo;
                acct.PartnerName = partner?.ShortName;
                acct.AccountNo = partner?.AccountNo;
                acct.TransactionType = item.Key.TransactionType;
                acct.VoucherDate = item.Key.VoucherDate;
                acct.InvoiceNo = item.Key.InvoiceNo;
                if (!string.IsNullOrEmpty(acct.InvoiceNo)) // [CR 15/03/22: Leyla invoice date null or =01/01/1900 => inv date = acct date + pm term ]
                {
                    acct.InvoiceDate = item.FirstOrDefault().InvoiceDate == null || item.FirstOrDefault().InvoiceDate.Value.Date == DateTime.Parse("01/01/1900").Date ? item.FirstOrDefault().PaymentDueDate : item.FirstOrDefault().InvoiceDate;
                }
                else
                {
                    acct.InvoiceDate = null;
                }
                acct.BillingNo = item.FirstOrDefault().BillingNo;
                acct.Currency = item.FirstOrDefault().Currency;
                acct.PaymentTerm = item.FirstOrDefault().PaymentTerm;
                acct.PaymentDueDate = item.FirstOrDefault().PaymentDueDate;
                var paymentAmount = GetAmountWithCurrency(string.Empty, item.Select(z => z).AsQueryable());
                acct.TotalAmount = paymentAmount[0] == null ? paymentAmount[0] : paymentAmount[0] * advValue;
                acct.PaymentAmount = paymentAmount[1] == null ? paymentAmount[1] : paymentAmount[1] * advValue;
                acct.RemainAmount = paymentAmount[2] == null ? paymentAmount[2] : paymentAmount[2] * advValue;
                var paymentAmountVnd = GetAmountWithCurrency(AccountingConstants.CURRENCY_LOCAL, item.Select(z => z).AsQueryable());
                acct.TotalAmountVnd = paymentAmountVnd[0] == null ? paymentAmountVnd[0] : paymentAmountVnd[0] * advValue;
                acct.PaymentAmountVnd = paymentAmountVnd[1] == null ? paymentAmountVnd[1] : paymentAmountVnd[1] * advValue;
                acct.RemainAmountVnd = paymentAmountVnd[2] == null ? paymentAmountVnd[2] : paymentAmountVnd[2] * advValue;
                acct.NotShowDetail = item.FirstOrDefault().InRangeType == "OutRange";
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
            var paymentdata = data.Where(x => x.TransactionType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE || (x.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE && x.PaymentType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE)).OrderBy(x => x.PaymentDatetimeCreated).GroupBy(x => new { x.PaymentAcctId, x.PaymentDate, x.PaymentType });
            string type = data.FirstOrDefault().InRangeType;
            decimal? total = 0;
            decimal? paidAmount = 0;
            decimal? remainAmount = 0;
            switch (type)
            {
                case "InRange":
                    // Transaction AP Phát sinh trong thời gian được chọn => lấy số dư đầu kì
                    if (currency == AccountingConstants.CURRENCY_LOCAL)
                    {
                        total = data.FirstOrDefault().TotalAmountVnd ?? 0;
                    }
                    else if (currency == AccountingConstants.CURRENCY_USD)
                    {
                        total = data.FirstOrDefault().TotalAmountUsd ?? 0;
                    }
                    else
                    {
                        total = data.FirstOrDefault().TotalAmount ?? 0;
                    }
                    break;
                case "PMInRange":
                    // Transaction AP phát sinh trước thời gian được chọn và có AP payment trong khoảng thời gian được chọn: Begin Amount => lấy remain amount của payment gần nhất (Cận dưới from date), trường hợp không có payment => Total Amount
                    if (currency == AccountingConstants.CURRENCY_LOCAL)
                    {
                        if (data.Any(x => x.PaymentRemainAmountVnd == null))
                        {
                            total = data.FirstOrDefault().TotalAmountVnd ?? 0;
                        }
                        else
                        {
                            total = data.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.PaymentRemainAmountVnd ?? 0;
                        }
                    }
                    else if (currency == AccountingConstants.CURRENCY_USD)
                    {
                        if (data.Any(x => x.PaymentRemainAmountUsd == null))
                        {
                            total = data.FirstOrDefault().TotalAmountUsd ?? 0;
                        }
                        else
                        {
                            total = data.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.PaymentRemainAmountUsd ?? 0;
                        }
                    }
                    else
                    {
                        if (data.Any(x => x.PaymentRemainAmount == null))
                        {
                            total = data.FirstOrDefault().TotalAmount ?? 0;
                        }
                        else
                        {
                            total = data.OrderByDescending(x => x.PaymentDate).FirstOrDefault()?.PaymentRemainAmount ?? 0;
                        }
                    }
                    break;
                case "OutRange":
                    // Transaction AP chưa trả hết(Unpaid/ Paid a Part) trước và không có payment trướcng khoảng thời được chọn => lấy remain amount của AP cận trên đó, Trường hợp remain không có giá trị sẽ lấy Total Amount
                    if (currency == AccountingConstants.CURRENCY_LOCAL)
                    {
                        total = (data.FirstOrDefault().RemainAmountVnd == null || data.FirstOrDefault().RemainAmountVnd == 0 || data.FirstOrDefault().PaymentRemainAmountVnd == null) ? data.FirstOrDefault().TotalAmount : data.FirstOrDefault().PaymentRemainAmountVnd;
                    }
                    else if (currency == AccountingConstants.CURRENCY_USD)
                    {
                        total = (data.FirstOrDefault().RemainAmountUsd == null || data.FirstOrDefault().RemainAmountUsd == 0 || data.FirstOrDefault().PaymentRemainAmountUsd == null) ? data.FirstOrDefault().TotalAmountUsd : data.FirstOrDefault().PaymentRemainAmountUsd;
                    }
                    else
                    {
                        total = (data.FirstOrDefault().OrgRemainAmount == null || data.FirstOrDefault().OrgRemainAmount == 0 || data.FirstOrDefault().PaymentRemainAmount == null) ? data.FirstOrDefault().TotalAmount : data.FirstOrDefault().PaymentRemainAmount;
                    }
                    break;
            }
            if (currency == AccountingConstants.CURRENCY_LOCAL)
            {
                foreach (var item in paymentdata)
                {
                    paidAmount += item.Sum(x => x.PaymentAmountVnd ?? 0);
                    remainAmount += item.FirstOrDefault().PaymentRemainAmountVnd;
                }
                // [CR: 15/03/22] TH ghi nhận trong kì thì lấy total = total amount ban đầu, remain = remain trên AP
                if (type == "InRange")
                {
                    remainAmount = data.FirstOrDefault().RemainAmountVnd;
                }
            }
            else if (currency == AccountingConstants.CURRENCY_USD)
            {
                foreach (var item in paymentdata)
                {
                    paidAmount += item.Sum(x => x.PaymentAmountUsd ?? 0);
                    remainAmount += item.FirstOrDefault().PaymentRemainAmountUsd;
                }
                if (type == "InRange")
                {
                    remainAmount = data.FirstOrDefault().RemainAmountUsd;
                }
            }
            else
            {
                foreach (var item in paymentdata)
                {
                    paidAmount += item.Sum(x => x.PaymentAmount ?? 0);
                    remainAmount += item.FirstOrDefault().PaymentRemainAmount;
                }
                if (type == "InRange")
                {
                    remainAmount = data.FirstOrDefault().OrgRemainAmount;
                }
            }

            
            if (type == "OutRange")
            {
                paidAmount = null;
                remainAmount = null;
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
            var payabledata = DataContext.Get(x => x.VoucherNo == refNo && x.TransactionType == type && (string.IsNullOrEmpty(x.InvoiceNo) || x.InvoiceNo == invoiceNo) && (string.IsNullOrEmpty(x.BillingNo) || x.BillingNo == billingNo)).FirstOrDefault();
            if (payabledata == null) return null;
            var advValue = payabledata.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE ? (-1) : 1; // dòng adv hiện giá trị âm
            var paymentData = accountPayablePaymentRepository.Get(x => x.ReferenceNo == payabledata.ReferenceNo && (payabledata.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE ? x.PaymentType == AccountingConstants.PAYMENT_TYPE_NAME_NET_OFF : x.PaymentType == payabledata.TransactionType)).OrderBy(x => x.PaymentDate).ThenBy(x => x.DatetimeCreated)
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


        private Expression<Func<AccAccountPayable, bool>> QueryPayable(AccountPayableCriteria criteria)
        {
            Expression<Func<AccAccountPayable, bool>> query = x => !string.IsNullOrEmpty(x.ReferenceNo);
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
            //if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
            //{
            //    query = query.And(x => criteria.PaymentStatus.Any(z => z == x.Status));
            //}
            if (criteria.TransactionType != null && criteria.TransactionType.Count > 0)
            {
                query = query.And(x => criteria.TransactionType.Any(z => z.ToLower().Contains(x.TransactionType.ToLower())));
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
                var apInRangeDates = data.Where(x => DateTime.Parse(criteria.FromPaymentDate).Date <= x.VoucherDate.Value.Date && x.VoucherDate.Value.Date <= DateTime.Parse(criteria.ToPaymentDate).Date &&
                (x.payment == null || !string.IsNullOrEmpty(x.payment.PaymentNo))).Select(x => new { x, InRangeType = "InRange" });
                // Lấy AP trước khoảng thời gian đã chọn nhưng payment nằm trong khoảng thời gian
                var paymentInRangedates = data.Where(x => (DateTime.Parse(criteria.FromPaymentDate).Date > x.VoucherDate.Value.Date) && x.payment != null && DateTime.Parse(criteria.FromPaymentDate).Date <= x.payment.PaymentDate.Value.Date && x.payment.PaymentDate.Value.Date <= DateTime.Parse(criteria.ToPaymentDate).Date).Select(x => new { x, InRangeType = "PMInRange" });

                var dataGrp = apInRangeDates;
                if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
                {
                    dataGrp = dataGrp.Where(d => criteria.PaymentStatus.Any(status => status == d.x.Status));
                }
                if (paymentInRangedates.Count() > 0)
                {
                    if (dataGrp.Count() > 0)
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
                if (apUnpaidNotInRangeDates != null && apUnpaidNotInRangeDates.Count() > 0)
                {
                    if (results.Count() > 0)
                    {
                        results = results.Union(apUnpaidNotInRangeDates.ToList());
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
        public List<AcctPayablePaymentExport> GetDataExportPayablePaymentDetail(AccountPayableCriteria criteria)
        {
            var data = GetDataAcctPayable(criteria).ToList();
            var grpData = data.OrderBy(x => x.VoucherDate).GroupBy(x => new { x.PartnerId, x.VoucherNo, x.VoucherDate, x.InvoiceNo, x.BillingNo, x.TransactionType, x.ReferenceNo }).Select(x => new { x.Key, x });
            var result = new List<AcctPayablePaymentExport>();
            var partnerIds = data.Select(x => x.PartnerId);
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
                if (!string.IsNullOrEmpty(payable.InvoiceNo)) // [CR 15/03/22: Leyla invoice date null or =01/01/1900 => inv date = acct date + pm term ]
                {
                    payable.Invoicedate = item.x.FirstOrDefault().InvoiceDate == null || item.x.FirstOrDefault().InvoiceDate.Value.Date == DateTime.Parse("01/01/1900").Date ? item.x.FirstOrDefault().PaymentDueDate : item.x.FirstOrDefault().InvoiceDate;
                }
                else
                {
                    payable.Invoicedate = null;
                }
                payable.DocNo = item.Key.BillingNo;
                payable.PaymentTerm = item.x.FirstOrDefault().PaymentTerm;
                payable.PaymentDueDate = item.x.FirstOrDefault().PaymentDueDate;
                payable.Status = item.x.FirstOrDefault().Status;
                payable.Description = item.x.FirstOrDefault().Description;

                var payableType = item.x.FirstOrDefault().InRangeType;
                switch (payableType)
                {
                    case "InRange":
                        // Transaction AP Phát sinh trong thời gian được chọn => sẽ để trống
                        payable.BeginAmount = item.x.FirstOrDefault().TotalAmount ?? 0;
                        payable.BeginAmountVND = item.x.FirstOrDefault().TotalAmountVnd ?? 0;
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
                var advValue = item.Key.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE ? (-1) : 1; // dòng adv hiện giá trị âm
                if (item.x.Any(z => !string.IsNullOrEmpty(z.PaymentNo)))
                {
                    var paymentGrp = item.x.Where(x => x.TransactionType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE || (x.TransactionType == AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE && x.PaymentType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE)).OrderBy(x => x.PaymentDatetimeCreated).GroupBy(x => new { x.PaymentAcctId, x.PaymentDate, x.PaymentType }).Select(x => new AcctPayablePaymentDetail
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
                        payable.BeginAmount = paymentGrp.FirstOrDefault().OriginRemainAmount;
                        payable.BeginAmountVND = paymentGrp.FirstOrDefault().RemainAmountVND;
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
            return result.OrderBy(x => x.PartnerName).ThenBy(x => x.AcctDate).ThenBy(x => x.AcctRefNo).ThenBy(x => x.BillingNo).ToList();
        }

        /// <summary>
        /// Get data export accounting template payable
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public List<AccountingTemplateExport> GetDataExportAccountingTemplate(AccountPayableCriteria criteria)
        {
            var data = GetDataAcctPayable(criteria).ToList();
            var grpData = data.GroupBy(payable => new { payable.PartnerId }).Select(payable => new { payable.Key, payable });
            var result = new List<AccountingTemplateExport>();
            var partnerIds = grpData.Select(x => x.Key.PartnerId);
            var partnerData = catPartnerRepository.Get(x => partnerIds.Any(z => z == x.Id));
            var office = sysOfficeRepository.Get(x => x.Id == currentUser.OfficeID).FirstOrDefault();
            var officeName = office?.BranchNameEn?.ToUpper();
            var _contactOffice = string.Format("{0}\nTel: {1}  Fax: {2}\nE-mail: {3}", office?.AddressEn, office?.Tel, office?.Fax, office?.Email);

            foreach (var item in grpData)
            {
                var grpPayable = item.payable.GroupBy(x => new { x.VoucherNo, x.VoucherDate, x.InvoiceNo, x.BillingNo, x.TransactionType, x.ReferenceNo });
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
                        var paymentGrp = payableItm.GroupBy(x => new { x.PaymentAcctId, x.PaymentDate, x.PaymentType }).Select(x => new { pm = x.Select(z => new { z.PaymentAmount, z.PaymentAmountVnd, z.PaymentRemainAmount, z.PaymentRemainAmountVnd }) });
                        if (paymentGrp.Count() > 0)
                        {
                            detail.OrgAmountGiam = paymentGrp.Sum(x => x.pm.Sum(z => z.PaymentAmount ?? 0));
                            detail.OrgAmountGiamVND = paymentGrp.Sum(x => x.pm.Sum(z => z.PaymentAmountVnd ?? 0));
                        }

                    }
                    else
                    {
                        detail.BeginAmount *= (-1);
                        detail.BeginAmountVND *= (-1);
                        detail.OrgAmountTang *= (-1);
                        detail.OrgAmountTangVND *= (-1);
                        var paymentGrp = payableItm.Where(x => x.PaymentType != AccountingConstants.PAYMENT_TYPE_NAME_ADVANCE).GroupBy(x => new { x.PaymentAcctId, x.PaymentDate, x.PaymentType }).Select(x => new { pm = x.Select(z => new { z.PaymentAmount, z.PaymentAmountVnd, z.PaymentRemainAmount, z.PaymentRemainAmountVnd }) });
                        if (paymentGrp.Count() > 0)
                        {
                            detail.OrgAmountGiam = paymentGrp.Sum(x => x.pm.Sum(z => z.PaymentAmount ?? 0)) * (-1);
                            detail.OrgAmountGiamVND = paymentGrp.Sum(x => x.pm.Sum(z => z.PaymentAmountVnd ?? 0)) * (-1);
                        }
                    }
                    result.Add(detail);
                }
            }
            return result.OrderBy(x => x.VoucherDate).ThenBy(x => x.VoucherNo).ThenBy(x => x.DocNo).ToList();
        }
    }
}
