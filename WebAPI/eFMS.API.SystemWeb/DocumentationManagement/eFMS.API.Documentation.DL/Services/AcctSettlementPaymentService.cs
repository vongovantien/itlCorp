using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.SettlementPayment;
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

        #region --- List Settlement Payment ---
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
        #endregion --- List Settlement Payment ---

        public HandleState DeleteSettlementPayment(string settlementNo)
        {
            try
            {
                var userCurrenct = currentUser.UserID;

                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;

                var settlement = dc.AcctSettlementPayment.Where(x => x.SettlementNo == settlementNo).FirstOrDefault();
                if (settlement == null) return new HandleState("Not Found Settlement Payment");
                if (!settlement.StatusApproval.Equals("New") && !settlement.StatusApproval.Equals("Denied"))
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

        #region ---Detail Settlement Payment---
        public AcctSettlementPaymentModel GetSettlementPaymentById(Guid idSettlement)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var settlement = dc.AcctSettlementPayment.SingleOrDefault(x => x.Id == idSettlement);
            var settlementMap = mapper.Map<AcctSettlementPaymentModel>(settlement);
            return settlementMap;
        }

        public List<ShipmentSettlement> GetListShipmentSettlementBySettlementNo(string settlementNo)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var surcharge = dc.CsShipmentSurcharge;
            var settlement = dc.AcctSettlementPayment;
            var opsTrans = dc.OpsTransaction;
            var csTransD = dc.CsTransactionDetail;
            var csTrans = dc.CsTransaction;
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = dc.CatCurrencyExchange.Where(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

            var dataQuery = from sur in surcharge
                            join opst in opsTrans on sur.Hblid equals opst.Hblid into opst2
                            from opst in opst2.DefaultIfEmpty()
                            join cstd in csTransD on sur.Hblid equals cstd.Id into cstd2
                            from cstd in cstd2.DefaultIfEmpty()
                            join cst in csTrans on cstd.JobId equals cst.Id into cst2
                            from cst in cst2.DefaultIfEmpty()
                            join settle in settlement on sur.SettlementCode equals settle.SettlementNo into settle2
                            from settle in settle2.DefaultIfEmpty()
                            where sur.SettlementCode == settlementNo

                            select new ShipmentSettlement
                            {
                                SettlementNo = sur.SettlementCode,
                                JobId = (opst.JobNo == null ? cst.JobNo : opst.JobNo),
                                HBL = (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno),
                                MBL = (opst.Mblno == null ? cstd.Mawb : opst.Mblno),
                                CurrencyShipment = settle.SettlementCurrency,
                                TotalAmount = sur.Total * GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settle.SettlementCurrency)
                            };

            dataQuery = dataQuery
                        .GroupBy(x => new { x.SettlementNo, x.JobId, x.HBL, x.MBL, x.CurrencyShipment })
                        .Select(x => new ShipmentSettlement
                        {
                            SettlementNo = x.Key.SettlementNo,
                            JobId = x.Key.JobId,
                            HBL = x.Key.HBL,
                            MBL = x.Key.MBL,
                            CurrencyShipment = x.Key.CurrencyShipment,
                            TotalAmount = x.Sum(su => su.TotalAmount)
                        });

            var shipmentSettlement = new List<ShipmentSettlement>();
            foreach (var item in dataQuery)
            {
                shipmentSettlement.Add(new ShipmentSettlement
                    {
                        SettlementNo = item.SettlementNo,
                        JobId = item.JobId,
                        MBL = item.MBL,
                        HBL = item.HBL,
                        TotalAmount = item.TotalAmount,
                        CurrencyShipment = item.CurrencyShipment,
                        ChargeSettlements = GetChargesSettlementBySettlementNoAndShipment(item.SettlementNo, item.JobId, item.MBL, item.HBL)
                    }
                );
            }
            return shipmentSettlement.ToList();
        }

        public List<ChargeSettlement> GetChargesSettlementBySettlementNoAndShipment(string settlementNo, string JobId, string MBL, string HBL)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var surcharge = dc.CsShipmentSurcharge;
            var charge = dc.CatCharge;
            var unit = dc.CatUnit;
            var payer = dc.CatPartner;
            var payee = dc.CatPartner;
            var opsTrans = dc.OpsTransaction;
            var csTransD = dc.CsTransactionDetail;
            var csTrans = dc.CsTransaction;

            var data = from sur in surcharge
                       join cc in charge on sur.ChargeId equals cc.Id into cc2
                       from cc in cc2.DefaultIfEmpty()
                       join u in unit on sur.UnitId equals u.Id into u2
                       from u in u2.DefaultIfEmpty()
                       join par in payer on sur.PayerId equals par.Id into par2
                       from par in par2.DefaultIfEmpty()
                       join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                       from pae in pae2.DefaultIfEmpty()
                       join opst in opsTrans on sur.Hblid equals opst.Hblid into opst2
                       from opst in opst2.DefaultIfEmpty()
                       join cstd in csTransD on sur.Hblid equals cstd.Id into cstd2
                       from cstd in cstd2.DefaultIfEmpty()
                       join cst in csTrans on cstd.JobId equals cst.Id into cst2
                       from cst in cst2.DefaultIfEmpty()
                       where
                                sur.SettlementCode == settlementNo
                            && (opst.JobNo == null ? cst.JobNo : opst.JobNo) == JobId
                            && (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno) == HBL
                            && (opst.Mblno == null ? cstd.Mawb : opst.Mblno) == MBL
                       select new ChargeSettlement
                       {
                           Id = sur.Id,
                           SettlementNo = sur.SettlementCode,
                           ChargeId = sur.ChargeId,
                           ChargeName = cc.ChargeNameEn,
                           Quantity = sur.Quantity,
                           UnitID = sur.UnitId,
                           UnitName = u.UnitNameEn,
                           UnitPrice = sur.UnitPrice,
                           Currency = sur.CurrencyId,
                           VATRate = sur.Vatrate,
                           Amount = sur.Total,
                           PayerID = sur.PayerId,
                           Payer = par.ShortName,
                           PaymentObjectID = sur.PaymentObjectId,
                           OBHPartner = pae.ShortName,
                           InvoiceNo = sur.InvoiceNo,
                           SeriesNo = sur.SeriesNo,
                           InvoiceDate = sur.InvoiceDate,
                           CustomNo = sur.ClearanceNo,
                           ContNo = sur.ContNo,
                           Note = sur.Notes
                       };

            return data.ToList();
        }
        #endregion ---Detail Settlement Payment---

        #region ---Payment Management---
        public List<AdvancePaymentMngt> GetAdvancePaymentMngts(string JobId, string MBL, string HBL)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var advance = dc.AcctAdvancePayment;
            var request = dc.AcctAdvanceRequest;
            //Chỉ lấy những advance có status là Done
            var data = from req in request
                       join ad in advance on req.AdvanceNo equals ad.AdvanceNo into ad2
                       from ad in ad2.DefaultIfEmpty()
                       where 
                            ad.StatusApproval == "Done" 
                       &&   req.JobId == JobId 
                       &&   req.Mbl == MBL 
                       &&   req.Hbl == HBL
                       select new AdvancePaymentMngt {
                           AdvanceNo = ad.AdvanceNo,
                           TotalAmount = req.Amount.Value,
                           AdvanceCurrency = ad.AdvanceCurrency,
                           AdvanceDate = ad.DatetimeCreated
                       };
            data = data.GroupBy(x => new { x.AdvanceNo, x.AdvanceCurrency, x.AdvanceDate })
                .Select(s=> new AdvancePaymentMngt {
                    AdvanceNo = s.Key.AdvanceNo,
                    TotalAmount = s.Sum(su => su.TotalAmount),
                    AdvanceCurrency = s.Key.AdvanceCurrency,
                    AdvanceDate = s.Key.AdvanceDate
                });

            var dataResult = new List<AdvancePaymentMngt>();
            foreach(var item in data)
            {
                dataResult.Add(new AdvancePaymentMngt
                {
                    AdvanceNo = item.AdvanceNo,
                    TotalAmount = item.TotalAmount,
                    AdvanceCurrency = item.AdvanceCurrency,
                    AdvanceDate = item.AdvanceDate,
                    ChargeAdvancePaymentMngts = request.Where(x => x.AdvanceNo == item.AdvanceNo && x.JobId == JobId && x.Mbl == MBL && x.Hbl == HBL).Select(x=>new ChargeAdvancePaymentMngt { AdvanceNo = x.AdvanceNo, TotalAmount = x.Amount.Value, AdvanceCurrency = x.RequestCurrency, Description = x.Description }).ToList()
                });
            }
            return dataResult;
        }

        public List<SettlementPaymentMngt> GetSettlementPaymentMngts(string JobId, string MBL, string HBL)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var settlement = dc.AcctSettlementPayment;
            var surcharge = dc.CsShipmentSurcharge;
            var charge = dc.CatCharge;
            var payee = dc.CatPartner;
            var payer = dc.CatPartner;
            var opsTrans = dc.OpsTransaction;
            var csTransD = dc.CsTransactionDetail;
            var csTrans = dc.CsTransaction;
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = dc.CatCurrencyExchange.Where(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

            //Chỉ lấy ra những settlement có status là done
            var data = from settle in settlement
                       join sur in surcharge on settle.SettlementNo equals sur.SettlementCode into sur2
                       from sur in sur2.DefaultIfEmpty()
                       join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                       from pae in pae2.DefaultIfEmpty()
                       join opst in opsTrans on sur.Hblid equals opst.Hblid into opst2
                       from opst in opst2.DefaultIfEmpty()
                       join cstd in csTransD on sur.Hblid equals cstd.Id into cstd2
                       from cstd in cstd2.DefaultIfEmpty()
                       join cst in csTrans on cstd.JobId equals cst.Id into cst2
                       from cst in cst2.DefaultIfEmpty()
                       where
                                settle.StatusApproval == "Done"
                            && (opst.JobNo == null ? cst.JobNo : opst.JobNo) == JobId
                            && (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno) == HBL
                            && (opst.Mblno == null ? cstd.Mawb : opst.Mblno) == MBL
                       select new SettlementPaymentMngt
                       {
                           SettlementNo = settle.SettlementNo,
                           TotalAmount = sur.Total,
                           SettlementCurrency = settle.SettlementCurrency,
                           ChargeCurrency = sur.CurrencyId,
                           SettlementDate = settle.DatetimeCreated
                       };

            data = data.GroupBy(x => new { x.SettlementNo, x.SettlementCurrency, x.SettlementDate })
                .Select(s => new SettlementPaymentMngt
                {
                    SettlementNo = s.Key.SettlementNo,
                    TotalAmount = s.Sum(su => su.TotalAmount * GetRateCurrencyExchange(currencyExchange, su.ChargeCurrency, su.SettlementCurrency)),
                    SettlementCurrency = s.Key.SettlementCurrency,
                    SettlementDate = s.Key.SettlementDate
                });

            var dataResult = new List<SettlementPaymentMngt>();
            foreach (var item in data)
            {
                dataResult.Add(new SettlementPaymentMngt
                {
                    SettlementNo = item.SettlementNo,
                    TotalAmount = item.TotalAmount,
                    SettlementCurrency = item.SettlementCurrency,
                    SettlementDate = item.SettlementDate,
                    ChargeSettlementPaymentMngts =
                      (from sur in surcharge
                       join cc in charge on sur.ChargeId equals cc.Id into cc2
                       from cc in cc2.DefaultIfEmpty()
                       join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                       from pae in pae2.DefaultIfEmpty()
                       join par in payer on sur.PayerId equals par.Id into par2
                       from par in par2.DefaultIfEmpty()
                       join opst in opsTrans on sur.Hblid equals opst.Hblid into opst2
                       from opst in opst2.DefaultIfEmpty()
                       join cstd in csTransD on sur.Hblid equals cstd.Id into cstd2
                       from cstd in cstd2.DefaultIfEmpty()
                       join cst in csTrans on cstd.JobId equals cst.Id into cst2
                       from cst in cst2.DefaultIfEmpty()
                       where
                            sur.SettlementCode == item.SettlementNo
                        && (opst.JobNo == null ? cst.JobNo : opst.JobNo) == JobId
                        && (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno) == HBL
                        && (opst.Mblno == null ? cstd.Mawb : opst.Mblno) == MBL
                       select new ChargeSettlementPaymentMngt {
                           SettlementNo = item.SettlementNo,
                           ChargeName = cc.ChargeNameEn,
                           TotalAmount = sur.Total,
                           SettlementCurrency = sur.CurrencyId,
                           OBHPartner = pae.ShortName,
                           Payer = par.ShortName
                       }).ToList()
                });
            }
            return dataResult;
        }
        #endregion ---Payment Management---

        #region -- Get Exists Charge --
        public List<Shipments> GetShipmentsCreditPayer()
        {
            //Chỉ lấy ra những phí Credit(BUY) & Payer
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var surcharge = dc.CsShipmentSurcharge;
            var opsTrans = dc.OpsTransaction.Where(x => x.CurrentStatus != "Canceled");
            var csTrans = dc.CsTransaction;
            var csTransDe = dc.CsTransactionDetail;
            var data = from sur in surcharge
                       join opst in opsTrans on sur.Hblid equals opst.Hblid into opst2
                       from opst in opst2.DefaultIfEmpty()
                       join cstd in csTransDe on sur.Hblid equals cstd.Id into cstd2
                       from cstd in cstd2.DefaultIfEmpty()
                       join cst in csTrans on cstd.JobId equals cst.Id into cst2
                       from cst in cst2.DefaultIfEmpty()
                       where
                        (
                                sur.Type == "BUY"
                            ||
                                (sur.PayerId != null && sur.CreditNo != null)
                        )
                        && (opst.JobNo == null ? cst.JobNo : opst.JobNo) != null
                        && (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno) != null
                        && (opst.Mblno == null ? cstd.Mawb : opst.Mblno) != null
                       select new Shipments {
                           //Id = sur.Id,
                           JobId = (opst.JobNo == null ? cst.JobNo : opst.JobNo),
                           HBL = (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno),
                           MBL = (opst.Mblno == null ? cstd.Mawb : opst.Mblno)
                       };

            var dataResult = data.GroupBy(x => new { x.JobId, x.HBL, x.MBL }).Select(s => new Shipments {
                
                JobId = s.Key.JobId,
                HBL = s.Key.HBL,
                MBL = s.Key.MBL
            });
            return dataResult.ToList();
        }


        #endregion -- Get Exists Charge --
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
