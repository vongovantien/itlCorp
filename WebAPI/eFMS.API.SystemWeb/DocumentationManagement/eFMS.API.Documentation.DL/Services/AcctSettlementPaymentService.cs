using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctSettlementPaymentService : RepositoryBase<AcctSettlementPayment, AcctSettlementPaymentModel>, IAcctSettlementPaymentService
    {
        private readonly ICurrentUser currentUser;
        private readonly IOptions<WebUrl> webUrl;
        public AcctSettlementPaymentService(IContextBase<AcctSettlementPayment> repository, IMapper mapper, ICurrentUser user, IOpsTransactionService ops, IOptions<WebUrl> url) : base(repository, mapper)
        {
            currentUser = user;
            webUrl = url;
        }

        public List<AcctSettlementPaymentResult> Paging(AcctSettlementPaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var settlement = dc.AcctSettlementPayment;
            var user = dc.SysUser;
            var surcharge = dc.CsShipmentSurcharge;
            var opst = dc.OpsTransaction;
            var csTrans = dc.CsTransaction;
            var csTransDe = dc.CsTransactionDetail;
            var custom = dc.CustomsDeclaration;
            var advRequest = dc.AcctAdvanceRequest;
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = dc.CatCurrencyExchange.Where(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

            List<string> refNo = new List<string>();
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                refNo = (from set in settlement
                         join sur in surcharge on set.SettlementNo equals sur.SettlementCode into sc
                         from sur in sc.DefaultIfEmpty()
                         join ops in opst on sur.Hblid equals ops.Hblid into op
                         from ops in op.DefaultIfEmpty()
                         join cstd in csTransDe on sur.Hblid equals cstd.Id into csd
                         from cstd in csd.DefaultIfEmpty()
                         join cst in csTrans on cstd.JobId equals cst.Id into cs
                         from cst in cs.DefaultIfEmpty()
                         join cus in custom on new { JobNo = (cst.JobNo != null ? cst.JobNo : ops.JobNo), HBL = (cstd.Hwbno != null ? cstd.Hwbno : ops.Hwbno), MBL = (cstd.Mawb != null ? cstd.Mawb : ops.Mblno) } equals new { JobNo = cus.JobNo, HBL = cus.Hblid, MBL = cus.Mblid } into cus1
                         from cus in cus1.DefaultIfEmpty()
                         join req in advRequest on new { JobNo = (cst.JobNo != null ? cst.JobNo : ops.JobNo), HBL = (cstd.Hwbno != null ? cstd.Hwbno : ops.Hwbno), MBL = (cstd.Mawb != null ? cstd.Mawb : ops.Mblno) } equals new { JobNo = req.JobId, HBL = req.Hbl, MBL = req.Mbl } into req1
                         from req in req1.DefaultIfEmpty()
                         where
                         (
                              criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ?
                              (
                                  (
                                         (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(set.SettlementNo) : 1 == 1)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(ops.Hwbno) : 1 == 1)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(ops.Mblno) : 1 == 1)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(ops.JobNo) : 1 == 1)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(cstd.Hwbno) : 1 == 1)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(cstd.Mawb) : 1 == 1)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(cst.JobNo) : 1 == 1)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(cus.ClearanceNo) : 1 == 1)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(req.AdvanceNo) : 1 == 1)
                                  )
                              )
                              :
                              (
                                  1 == 1
                              )
                         )
                         select sur.SettlementCode).ToList();
            }

            var data = from set in settlement
                       join u in user on set.Requester equals u.Id into u2
                       from u in u2.DefaultIfEmpty()
                       join sur in surcharge on set.SettlementNo equals sur.SettlementCode into sc
                       from sur in sc.DefaultIfEmpty()
                       where
                       (
                            criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ? refNo.Contains(set.SettlementNo) : 1 == 1                           
                       )
                       &&
                       (
                            !string.IsNullOrEmpty(criteria.Requester) ?
                                set.Requester == criteria.Requester
                            :
                                1 == 1
                       )
                       &&
                       (
                            criteria.RequestDateFrom.HasValue && criteria.RequestDateTo.HasValue ?
                                //Convert RequestDate về date nếu RequestDate có value
                                set.RequestDate.Value.Date >= (criteria.RequestDateFrom.HasValue ? criteria.RequestDateFrom.Value.Date : criteria.RequestDateFrom)
                                && set.RequestDate.Value.Date <= (criteria.RequestDateTo.HasValue ? criteria.RequestDateTo.Value.Date : criteria.RequestDateTo)
                            :
                                1 == 1
                       )
                       &&
                       (
                            !string.IsNullOrEmpty(criteria.StatusApproval) && !criteria.StatusApproval.Equals("All") ?
                                set.StatusApproval == criteria.StatusApproval
                            :
                                1 == 1
                       )
                       &&
                       (
                           !string.IsNullOrEmpty(criteria.PaymentMethod) && !criteria.PaymentMethod.Equals("All") ?
                                set.PaymentMethod == criteria.PaymentMethod
                           :
                                1 == 1
                       )
                       select new AcctSettlementPaymentResult
                       {
                           Id = set.Id,
                           Amount = sur.Total,
                           SettlementNo = set.SettlementNo,
                           SettlementCurrency = set.SettlementCurrency,
                           Requester = set.Requester,
                           RequesterName = u.Username,
                           RequestDate = set.RequestDate,
                           StatusApproval = set.StatusApproval,
                           PaymentMethod = set.PaymentMethod,
                           Note = set.Note,
                           ChargeCurrency = sur.CurrencyId
                       };

            data = data.GroupBy(x => new
            {
                x.Id,
                x.SettlementNo,
                x.SettlementCurrency,
                x.Requester,
                x.RequesterName,
                x.RequestDate,
                x.StatusApproval,
                x.PaymentMethod,
                x.Note
            }
            ).Select(s => new AcctSettlementPaymentResult
            {
                Id = s.Key.Id,
                Amount = s.Sum(su => su.Amount * GetRateCurrencyExchange(currencyExchange, su.ChargeCurrency, su.SettlementCurrency)),
                SettlementNo = s.Key.SettlementNo,
                SettlementCurrency = s.Key.SettlementCurrency,
                Requester = s.Key.Requester,
                RequesterName = s.Key.RequesterName,
                RequestDate = s.Key.RequestDate,
                StatusApproval = s.Key.StatusApproval,
                StatusApprovalName = CustomData.StatusApproveAdvance.Where(x => x.Value == s.Key.StatusApproval).Select(x => x.DisplayName).FirstOrDefault(),
                PaymentMethod = s.Key.PaymentMethod,
                PaymentMethodName = CustomData.PaymentMethod.Where(x => x.Value == s.Key.PaymentMethod).Select(x => x.DisplayName).FirstOrDefault(),
                Note = s.Key.Note
            }
            ).OrderByDescending(orb => orb.DatetimeModified);

            //Phân trang
            rowsCount = (data.Count() > 0) ? data.Count() : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

            return data.ToList();
        }

        public List<ShipmentOfSettlementResult> GetShipmentOfSettlements(string settlementNo)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var settlement = dc.AcctSettlementPayment;
            var surcharge = dc.CsShipmentSurcharge;
            var opst = dc.OpsTransaction;
            var csTrans = dc.CsTransaction;
            var csTransDe = dc.CsTransactionDetail;
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = dc.CatCurrencyExchange.Where(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

            var data = from set in settlement
                       join sur in surcharge on set.SettlementNo equals sur.SettlementCode into sc
                       from sur in sc.DefaultIfEmpty()
                       join ops in opst on sur.Hblid equals ops.Hblid into op
                       from ops in op.DefaultIfEmpty()
                       join cstd in csTransDe on sur.Hblid equals cstd.Id into csd
                       from cstd in csd.DefaultIfEmpty()
                       join cst in csTrans on cstd.JobId equals cst.Id into cs
                       from cst in cs.DefaultIfEmpty()
                       where
                            sur.SettlementCode == settlementNo
                       select new ShipmentOfSettlementResult
                       {
                           JobId = (cst.JobNo != null ? cst.JobNo : ops.JobNo),
                           HBL = (cstd.Hwbno != null ? cstd.Hwbno : ops.Hwbno),
                           MBL = (cstd.Mawb != null ? cstd.Mawb : ops.Mblno),
                           Amount = sur.Total,
                           ChargeCurrency = sur.CurrencyId,
                           SettlementCurrency = set.SettlementCurrency
                       };

            data = data.GroupBy(x => new
                {
                    x.JobId,
                    x.HBL,
                    x.MBL,
                    x.SettlementCurrency
                }
            ).Select(s => new ShipmentOfSettlementResult
                {
                    JobId = s.Key.JobId,
                    Amount = s.Sum(su => su.Amount * GetRateCurrencyExchange(currencyExchange, su.ChargeCurrency, su.SettlementCurrency)),
                    HBL = s.Key.HBL,
                    MBL = s.Key.MBL,
                    SettlementCurrency = s.Key.SettlementCurrency
                }
            );

            return data.ToList();
        }

        public HandleState DeleteSettlementPayment(string settlementNo)
        {
            try
            {
                var userCurrenct = currentUser.UserID;

                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;

                var settlement = dc.AcctSettlementPayment.Where(x => x.SettlementNo == settlementNo).FirstOrDefault();
                if (settlement == null) return new HandleState("Not Found Settlement Payment");
                if(!settlement.StatusApproval.Equals("New") && !settlement.StatusApproval.Equals("Denied"))
                {
                    return new HandleState("Not allow delete. Settlements are awaiting approval.");
                }
                dc.AcctSettlementPayment.Remove(settlement);
                var surcharge = dc.CsShipmentSurcharge.Where(x => x.SettlementCode == settlementNo).ToList();
                if (surcharge != null && surcharge.Count > 0)
                {
                    surcharge.ForEach(sur =>
                    {
                        sur.SettlementCode = null;
                        sur.UserModified = userCurrenct;
                        sur.DatetimeModified = DateTime.Now;   
                    });
                    dc.CsShipmentSurcharge.UpdateRange(surcharge);
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        #region ----Method Private----
        private string CreateSettlementNo(eFMSDataContext dc)
        {
            string year = (DateTime.Now.Year.ToString()).Substring(2, 2);
            string month = DateTime.Now.ToString("DDMMYYYY").Substring(2, 2);
            string prefix = "SM" + year + month + "/";
            string stt;

            //Lấy ra dòng cuối cùng của table acctSettlementPayment
            var rowlast = dc.AcctSettlementPayment.OrderByDescending(x => x.SettlementNo).FirstOrDefault();

            if (rowlast == null)
            {
                stt = "0001";
            }
            else
            {
                var settlementCurrent = rowlast.SettlementNo;
                var prefixCurrent = settlementCurrent.Substring(2, 4);
                //Reset về 1 khi qua tháng tiếp theo
                if (prefixCurrent != (year + month))
                {
                    stt = "0001";
                }
                else
                {
                    stt = (Convert.ToInt32(settlementCurrent.Substring(7, 4)) + 1).ToString();
                    stt = stt.PadLeft(4, '0');
                }
            }

            return prefix + stt;
        }

        private decimal GetRateCurrencyExchange(List<CatCurrencyExchange> currencyExchange, string currencyFrom, string currencyTo)
        {
            if (currencyFrom != currencyTo)
            {
                var get1 = currencyExchange.Where(x => x.CurrencyFromId == currencyFrom && x.CurrencyToId == currencyTo).FirstOrDefault();
                if (get1 != null)
                {
                    return get1.Rate;
                }
                else
                {
                    var get2 = currencyExchange.Where(x => x.CurrencyFromId == currencyTo && x.CurrencyToId == currencyFrom).FirstOrDefault();
                    if (get2 != null)
                    {
                        return 1 / get2.Rate;
                    }
                    else
                    {
                        var get3 = currencyExchange.Where(x => x.CurrencyFromId == currencyFrom || x.CurrencyFromId == currencyTo).ToList();
                        if (get3.Count > 1)
                        {
                            if (get3[0].CurrencyFromId == currencyFrom && get3[1].CurrencyFromId == currencyTo)
                            {
                                return get3[0].Rate / get3[1].Rate;
                            }
                            else
                            {
                                return get3[1].Rate / get3[0].Rate;
                            }
                        }
                        else
                        {
                            //Nến không tồn tại Currency trong Exchange thì return về 0
                            return 0;
                        }
                    }
                }
            }
            return 1;
        }
        #endregion
    }
}
