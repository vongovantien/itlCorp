using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ReportResults;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Contexts;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.Services
{
    public class AcctSettlementPaymentService : RepositoryBase<AcctSettlementPayment, AcctSettlementPaymentModel>, IService.IAcctSettlementPaymentService
    {
        private readonly ICurrentUser currentUser;
        private readonly IOptions<WebUrl> webUrl;
        readonly IContextBase<AcctApproveSettlement> acctApproveSettlementRepo;
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<CsShipmentSurcharge> csShipmentSurchargeRepo;
        readonly IContextBase<OpsTransaction> opsTransactionRepo;
        readonly IContextBase<CsTransaction> csTransactionRepo;
        readonly IContextBase<CsTransactionDetail> csTransactionDetailRepo;
        readonly IContextBase<CustomsDeclaration> customsDeclarationRepo;
        readonly IContextBase<AcctAdvancePayment> acctAdvancePaymentRepo;
        readonly IContextBase<AcctAdvanceRequest> acctAdvanceRequestRepo;
        readonly IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo;
        readonly IContextBase<SysUserGroup> sysUserGroupRepo;
        readonly IContextBase<SysGroup> sysGroupRepo;
        readonly IContextBase<CatDepartment> catDepartmentRepo;
        readonly IContextBase<SysEmployee> sysEmployeeRepo;
        readonly IContextBase<CatCharge> catChargeRepo;
        readonly IContextBase<CatUnit> catUnitRepo;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<SysBranch> sysBranchRepo;

        public AcctSettlementPaymentService(IContextBase<AcctSettlementPayment> repository,
            IMapper mapper,
            ICurrentUser user,
            IOptions<WebUrl> url,
            IContextBase<AcctApproveSettlement> acctApproveSettlement,
            IContextBase<SysUser> sysUser,
            IContextBase<CsShipmentSurcharge> csShipmentSurcharge,
            IContextBase<OpsTransaction> opsTransaction,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CsTransactionDetail> csTransactionDetail,
            IContextBase<CustomsDeclaration> customsDeclaration,
            IContextBase<AcctAdvancePayment> acctAdvancePayment,
            IContextBase<AcctAdvanceRequest> acctAdvanceRequest,
            IContextBase<CatCurrencyExchange> catCurrencyExchange,
            IContextBase<SysUserGroup> sysUserGroup,
            IContextBase<SysGroup> sysGroup,
            IContextBase<CatDepartment> catDepartment,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<CatCharge> catCharge,
            IContextBase<CatUnit> catUnit,
            IContextBase<CatPartner> catPartner,
            IContextBase<SysBranch> sysBranch) : base(repository, mapper)
        {
            currentUser = user;
            webUrl = url;
            acctApproveSettlementRepo = acctApproveSettlement;
            sysUserRepo = sysUser;
            csShipmentSurchargeRepo = csShipmentSurcharge;
            opsTransactionRepo = opsTransaction;
            csTransactionRepo = csTransaction;
            csTransactionDetailRepo = csTransactionDetail;
            customsDeclarationRepo = customsDeclaration;
            acctAdvancePaymentRepo = acctAdvancePayment;
            acctAdvanceRequestRepo = acctAdvanceRequest;
            catCurrencyExchangeRepo = catCurrencyExchange;
            sysUserGroupRepo = sysUserGroup;
            sysGroupRepo = sysGroup;
            catDepartmentRepo = catDepartment;
            sysEmployeeRepo = sysEmployee;
            catChargeRepo = catCharge;
            catUnitRepo = catUnit;
            catPartnerRepo = catPartner;
            sysBranchRepo = sysBranch;
        }

        #region --- LIST SETTLEMENT PAYMENT ---
        public List<AcctSettlementPaymentResult> Paging(AcctSettlementPaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = QueryData(criteria);

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

        public IQueryable<AcctSettlementPaymentResult> QueryData(AcctSettlementPaymentCriteria criteria)
        {
            var settlement = DataContext.Get();
            var approveSettle = acctApproveSettlementRepo.Get(x => x.IsDeputy == false);
            var user = sysUserRepo.Get();
            var surcharge = csShipmentSurchargeRepo.Get();
            var opst = opsTransactionRepo.Get();
            var csTrans = csTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            var custom = customsDeclarationRepo.Get();
            var advRequest = acctAdvanceRequestRepo.Get();
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

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
                       join apr in approveSettle on set.SettlementNo equals apr.SettlementNo into apr1
                       from apr in apr1.DefaultIfEmpty()
                       where
                       (
                            criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ? refNo.Contains(set.SettlementNo) : 1 == 1
                       )
                       &&
                       (
                            !string.IsNullOrEmpty(criteria.Requester) ?
                            (
                                    set.Requester == criteria.Requester
                                || (apr.Manager == criteria.Requester && apr.ManagerAprDate != null)
                                || (apr.Accountant == criteria.Requester && apr.AccountantAprDate != null)
                                || (apr.ManagerApr == criteria.Requester && apr.ManagerAprDate != null)
                                || (apr.AccountantApr == criteria.Requester && apr.AccountantAprDate != null)
                            )
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
                           Amount = sur.Total != null ? sur.Total : 0,
                           SettlementNo = set.SettlementNo,
                           SettlementCurrency = set.SettlementCurrency,
                           Requester = set.Requester,
                           RequesterName = u.Username,
                           RequestDate = set.RequestDate,
                           StatusApproval = set.StatusApproval,
                           PaymentMethod = set.PaymentMethod,
                           Note = set.Note,
                           ChargeCurrency = sur.CurrencyId,
                           DatetimeModified = set.DatetimeModified
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
                x.Note,
                x.DatetimeModified
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
                StatusApprovalName = Common.CustomData.StatusApproveAdvance.Where(x => x.Value == s.Key.StatusApproval).Select(x => x.DisplayName).FirstOrDefault(),
                PaymentMethod = s.Key.PaymentMethod,
                PaymentMethodName = Common.CustomData.PaymentMethod.Where(x => x.Value == s.Key.PaymentMethod).Select(x => x.DisplayName).FirstOrDefault(),
                Note = s.Key.Note,
                DatetimeModified = s.Key.DatetimeModified
            }
            ).OrderByDescending(orb => orb.DatetimeModified);
            return data;
        }

        public List<ShipmentOfSettlementResult> GetShipmentOfSettlements(string settlementNo)
        {
            var settlement = DataContext.Get();
            var surcharge = csShipmentSurchargeRepo.Get();
            var opst = opsTransactionRepo.Get();
            var csTrans = csTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

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
                           Amount = sur.Total != null ? sur.Total : 0, 
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
        #endregion --- LIST SETTLEMENT PAYMENT ---

        public HandleState DeleteSettlementPayment(string settlementNo)
        {
            try
            {
                var userCurrenct = currentUser.UserID;
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;

                var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
                if (settlement == null) return new HandleState("Not Found Settlement Payment");
                if (!settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_NEW) && !settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_DENIED))
                {
                    return new HandleState("Not allow delete. Settlements are awaiting approval.");
                }
                dc.AcctSettlementPayment.Remove(settlement);

                //Phí chừng từ (chỉ cập nhật lại SettlementCode bằng null)
                var surchargeShipment = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo && x.IsFromShipment == true).ToList();
                if (surchargeShipment != null && surchargeShipment.Count > 0)
                {
                    surchargeShipment.ForEach(sur =>
                    {
                        sur.SettlementCode = null;
                        sur.UserModified = userCurrenct;
                        sur.DatetimeModified = DateTime.Now;
                    });
                    dc.CsShipmentSurcharge.UpdateRange(surchargeShipment);
                }
                //Phí hiện trường (Xóa khỏi surcharge)
                var surchargeScene = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo && x.IsFromShipment == false).ToList();
                if (surchargeScene != null && surchargeScene.Count > 0)
                {
                    dc.CsShipmentSurcharge.RemoveRange(surchargeScene);
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

        #region --- DETAILS SETTLEMENT PAYMENT ---
        public AcctSettlementPaymentModel GetSettlementPaymentById(Guid idSettlement)
        {
            var settlement = DataContext.Get(x => x.Id == idSettlement).FirstOrDefault();
            var settlementMap = mapper.Map<AcctSettlementPaymentModel>(settlement);
            return settlementMap;
        }

        public List<ShipmentSettlement> GetListShipmentSettlementBySettlementNo(string settlementNo)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var settlement = DataContext.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

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
                                TotalAmount = (sur.Total != null ? sur.Total : 0) * GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settle.SettlementCurrency)
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
            return shipmentSettlement.OrderByDescending(x => x.JobId).ToList();
        }

        public List<ShipmentChargeSettlement> GetChargesSettlementBySettlementNoAndShipment(string settlementNo, string JobId, string MBL, string HBL)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();
            var payer = catPartnerRepo.Get();
            var payee = catPartnerRepo.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();

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
                       select new ShipmentChargeSettlement
                       {
                           Id = sur.Id,
                           JobId = JobId,
                           MBL = MBL,
                           HBL = HBL,
                           ChargeCode = cc.Code,
                           Hblid = sur.Hblid,
                           Type = sur.Type,
                           SettlementCode = sur.SettlementCode,
                           ChargeId = sur.ChargeId,
                           ChargeName = cc.ChargeNameEn,
                           Quantity = sur.Quantity,
                           UnitId = sur.UnitId,
                           UnitName = u.UnitNameEn,
                           UnitPrice = sur.UnitPrice,
                           CurrencyId = sur.CurrencyId,
                           Vatrate = sur.Vatrate,
                           Total = sur.Total != null ? sur.Total : 0,
                           PayerId = sur.PayerId,
                           Payer = (sur.Type == Constants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName),//par.ShortName,
                           PaymentObjectId = sur.PaymentObjectId,
                           OBHPartnerName = (sur.Type == Constants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),//pae.ShortName,
                           InvoiceNo = sur.InvoiceNo,
                           SeriesNo = sur.SeriesNo,
                           InvoiceDate = sur.InvoiceDate,
                           ClearanceNo = sur.ClearanceNo,
                           ContNo = sur.ContNo,
                           Notes = sur.Notes,
                           IsFromShipment = sur.IsFromShipment,
                           TypeOfFee = sur.TypeOfFee
                       };

            return data.ToList();
        }

        public IQueryable<ShipmentChargeSettlement> GetListShipmentChargeSettlementNoGroup(string settlementNo)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();
            var payer = catPartnerRepo.Get();
            var payee = catPartnerRepo.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();

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
                       select new ShipmentChargeSettlement
                       {
                           Id = sur.Id,
                           JobId = (opst.JobNo == null ? cst.JobNo : opst.JobNo),
                           MBL = (opst.Mblno == null ? cstd.Mawb : opst.Mblno),
                           HBL = (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno),
                           ChargeCode = cc.Code,
                           Hblid = sur.Hblid,
                           Type = sur.Type,
                           SettlementCode = sur.SettlementCode,
                           ChargeId = sur.ChargeId,
                           ChargeName = cc.ChargeNameEn,
                           Quantity = sur.Quantity,
                           UnitId = sur.UnitId,
                           UnitName = u.UnitNameEn,
                           UnitPrice = sur.UnitPrice,
                           CurrencyId = sur.CurrencyId,
                           Vatrate = sur.Vatrate,
                           Total = sur.Total != null ? sur.Total : 0,
                           PayerId = sur.PayerId,
                           Payer = (sur.Type == Constants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName),//par.ShortName,
                           PaymentObjectId = sur.PaymentObjectId,
                           OBHPartnerName = (sur.Type == Constants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),//pae.ShortName,
                           InvoiceNo = sur.InvoiceNo,
                           SeriesNo = sur.SeriesNo,
                           InvoiceDate = sur.InvoiceDate,
                           ClearanceNo = sur.ClearanceNo,
                           ContNo = sur.ContNo,
                           Notes = sur.Notes,
                           IsFromShipment = sur.IsFromShipment,
                           TypeOfFee = sur.TypeOfFee
                       };

            return data.OrderByDescending(x => x.JobId);
        }

        #endregion --- DETAILS SETTLEMENT PAYMENT ---

        #region --- PAYMENT MANAGEMENT ---
        public List<AdvancePaymentMngt> GetAdvancePaymentMngts(string JobId, string MBL, string HBL)
        {
            var advance = acctAdvancePaymentRepo.Get();
            var request = acctAdvanceRequestRepo.Get();
            //Chỉ lấy những advance có status là Done
            var data = from req in request
                       join ad in advance on req.AdvanceNo equals ad.AdvanceNo into ad2
                       from ad in ad2.DefaultIfEmpty()
                       where
                            ad.StatusApproval == Constants.STATUS_APPROVAL_DONE
                       && req.JobId == JobId
                       && req.Mbl == MBL
                       && req.Hbl == HBL
                       select new AdvancePaymentMngt
                       {
                           AdvanceNo = ad.AdvanceNo,
                           TotalAmount = req.Amount.Value,
                           AdvanceCurrency = ad.AdvanceCurrency,
                           AdvanceDate = ad.DatetimeCreated
                       };
            data = data.GroupBy(x => new { x.AdvanceNo, x.AdvanceCurrency, x.AdvanceDate })
                .Select(s => new AdvancePaymentMngt
                {
                    AdvanceNo = s.Key.AdvanceNo,
                    TotalAmount = s.Sum(su => su.TotalAmount),
                    AdvanceCurrency = s.Key.AdvanceCurrency,
                    AdvanceDate = s.Key.AdvanceDate
                });

            var dataResult = new List<AdvancePaymentMngt>();
            foreach (var item in data)
            {
                dataResult.Add(new AdvancePaymentMngt
                {
                    AdvanceNo = item.AdvanceNo,
                    TotalAmount = item.TotalAmount,
                    AdvanceCurrency = item.AdvanceCurrency,
                    AdvanceDate = item.AdvanceDate,
                    ChargeAdvancePaymentMngts = request.Where(x => x.AdvanceNo == item.AdvanceNo && x.JobId == JobId && x.Mbl == MBL && x.Hbl == HBL).Select(x => new ChargeAdvancePaymentMngt { AdvanceNo = x.AdvanceNo, TotalAmount = x.Amount.Value, AdvanceCurrency = x.RequestCurrency, Description = x.Description }).ToList()
                });
            }
            return dataResult;
        }

        public List<SettlementPaymentMngt> GetSettlementPaymentMngts(string JobId, string MBL, string HBL)
        {
            var settlement = DataContext.Get();
            var surcharge = csShipmentSurchargeRepo.Get();
            var charge = catChargeRepo.Get();
            var payee = catPartnerRepo.Get();
            var payer = catPartnerRepo.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();
            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

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
                                settle.StatusApproval == Constants.STATUS_APPROVAL_DONE
                            && (opst.JobNo == null ? cst.JobNo : opst.JobNo) == JobId
                            && (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno) == HBL
                            && (opst.Mblno == null ? cstd.Mawb : opst.Mblno) == MBL
                       select new SettlementPaymentMngt
                       {
                           SettlementNo = settle.SettlementNo,
                           TotalAmount = sur.Total != null ? sur.Total : 0,
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
                       select new ChargeSettlementPaymentMngt
                       {
                           SettlementNo = item.SettlementNo,
                           ChargeName = cc.ChargeNameEn,
                           TotalAmount = sur.Total != null ? sur.Total : 0,
                           SettlementCurrency = sur.CurrencyId,
                           OBHPartner = (sur.Type == Constants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),
                           Payer = (sur.Type == Constants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName)
                       }).ToList()
                });
            }
            return dataResult;
        }
        #endregion --- PAYMENT MANAGEMENT ---

        #region -- GET EXISITS CHARGE --
        public List<ShipmentChargeSettlement> GetExistsCharge(string JobId, string HBL, string MBL)
        {
            //Chỉ lấy ra những phí chứng từ (thuộc credit hoặc đối tượng obh credit)
            var surcharge = csShipmentSurchargeRepo
                .Get(x =>
                        x.IsFromShipment == true
                    && (x.Type == Constants.TYPE_CHARGE_BUY || (x.PayerId != null && x.CreditNo != null))
                );
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();
            var payer = catPartnerRepo.Get();
            var payee = catPartnerRepo.Get();
            var opsTrans = opsTransactionRepo.Get(x => x.CurrentStatus != "Canceled");
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();

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
                                !string.IsNullOrEmpty(JobId) ? (opst.JobNo == null ? cst.JobNo : opst.JobNo) == JobId : 1 == 1
                            &&
                                !string.IsNullOrEmpty(HBL) ? (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno) == HBL : 1 == 1
                            &&
                                !string.IsNullOrEmpty(MBL) ? (opst.Mblno == null ? cstd.Mawb : opst.Mblno) == MBL : 1 == 1
                       select new ShipmentChargeSettlement
                       {
                           Id = sur.Id,
                           JobId = JobId,
                           MBL = MBL,
                           HBL = HBL,
                           Hblid = sur.Hblid,
                           Type = sur.Type,
                           SettlementCode = sur.SettlementCode,
                           ChargeId = sur.ChargeId,
                           ChargeName = cc.ChargeNameEn,
                           Quantity = sur.Quantity,
                           UnitId = sur.UnitId,
                           UnitName = u.UnitNameEn,
                           UnitPrice = sur.UnitPrice,
                           CurrencyId = sur.CurrencyId,
                           Vatrate = sur.Vatrate,
                           Total = sur.Total != null ? sur.Total : 0,
                           PayerId = sur.PayerId,
                           Payer = par.ShortName,
                           PaymentObjectId = sur.PaymentObjectId,
                           OBHPartnerName = pae.ShortName,
                           InvoiceNo = sur.InvoiceNo,
                           SeriesNo = sur.SeriesNo,
                           InvoiceDate = sur.InvoiceDate,
                           ClearanceNo = sur.ClearanceNo,
                           ContNo = sur.ContNo,
                           Notes = sur.Notes,
                           IsFromShipment = sur.IsFromShipment
                       };

            return data.ToList();
        }
        #endregion -- GET EXISITS CHARGE --

        #region -- INSERT & UPDATE SETTLEMENT PAYMENT --
        public bool CheckDuplicateShipmentSettlement(CheckDuplicateShipmentSettlementCriteria criteria)
        {
            //Kiem tra Shipment đã tồn tại trong surcharge hay chưa
            var shipmentOperaExists = (from sur in csShipmentSurchargeRepo.Get()
                                       join ops in opsTransactionRepo.Get() on sur.Hblid equals ops.Hblid into ops2
                                       from ops in ops2.DefaultIfEmpty()
                                       where ops.JobNo == criteria.JobNo
                                       select ops.JobNo).Any();
            var shipmentDocExists = (from sur in csShipmentSurchargeRepo.Get()
                                     join cstd in csTransactionDetailRepo.Get() on sur.Hblid equals cstd.Id into csd
                                     from cstd in csd.DefaultIfEmpty()
                                     join cst in csTransactionRepo.Get() on cstd.JobId equals cst.Id into cs
                                     from cst in cs.DefaultIfEmpty()
                                     where cst.JobNo == criteria.JobNo
                                     select cst.JobNo).Any();

            var result = false;
            if (criteria.SurchargeID == Guid.Empty)
            {
                if (shipmentOperaExists == true || shipmentDocExists == true)
                {
                    result = csShipmentSurchargeRepo.Get(x =>
                           x.ChargeId == criteria.ChargeID
                        && x.Hblid == criteria.HBLID
                        && (criteria.TypeCharge == Constants.TYPE_CHARGE_BUY ? x.PaymentObjectId == criteria.Partner : (criteria.TypeCharge == Constants.TYPE_CHARGE_OBH ? x.PayerId == criteria.Partner : 1 == 1))
                        && (string.IsNullOrEmpty(criteria.CustomNo) ? 1 == 1 : x.ClearanceNo == criteria.CustomNo)
                        && (string.IsNullOrEmpty(criteria.InvoiceNo) ? 1 == 1 : x.InvoiceNo == criteria.InvoiceNo)
                        && (string.IsNullOrEmpty(criteria.ContNo) ? 1 == 1 : x.ContNo == criteria.ContNo)
                        ).Any();
                }
            }
            else
            {
                if (shipmentOperaExists == true || shipmentDocExists == true)
                {
                    result = csShipmentSurchargeRepo.Get(x =>
                       x.Id != criteria.SurchargeID
                    && x.ChargeId == criteria.ChargeID
                    && x.Hblid == criteria.HBLID
                    && (criteria.TypeCharge == Constants.TYPE_CHARGE_BUY ? x.PaymentObjectId == criteria.Partner : (criteria.TypeCharge == Constants.TYPE_CHARGE_OBH ? x.PayerId == criteria.Partner : 1 == 1))
                    && (string.IsNullOrEmpty(criteria.CustomNo) ? 1 == 1 : x.ClearanceNo == criteria.CustomNo)
                    && (string.IsNullOrEmpty(criteria.InvoiceNo) ? 1 == 1 : x.InvoiceNo == criteria.InvoiceNo)
                    && (string.IsNullOrEmpty(criteria.ContNo) ? 1 == 1 : x.ContNo == criteria.ContNo)
                    ).Any();
                }
            }
            return result;
        }

        public HandleState AddSettlementPayment(CreateUpdateSettlementModel model)
        {
            try
            {
                var userCurrent = currentUser.UserID;
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var settlement = mapper.Map<AcctSettlementPayment>(model.Settlement);
                settlement.Id = model.Settlement.Id = Guid.NewGuid();
                settlement.SettlementNo = model.Settlement.SettlementNo = CreateSettlementNo();
                settlement.StatusApproval = Constants.STATUS_APPROVAL_NEW;
                settlement.UserCreated = settlement.UserModified = userCurrent;
                settlement.DatetimeCreated = settlement.DatetimeModified = DateTime.Now;

                var hs = DataContext.Add(settlement);
                if (hs.Success)
                {
                    //Lấy các phí chứng từ IsFromShipment = true
                    var chargeShipment = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && x.IsFromShipment == true).Select(s => s.Id).ToList();
                    if (chargeShipment.Count > 0)
                    {
                        var listChargeShipment = csShipmentSurchargeRepo.Get(x => chargeShipment.Contains(x.Id)).ToList();
                        listChargeShipment.ForEach(req =>
                        {
                            req.SettlementCode = settlement.SettlementNo;
                            req.UserModified = userCurrent;
                            req.DatetimeModified = DateTime.Now;
                        });
                        dc.CsShipmentSurcharge.UpdateRange(listChargeShipment);
                    }

                    //Lấy các phí hiện trường IsFromShipment = false & thực hiện insert các charge mới
                    var chargeScene = model.ShipmentCharge.Where(x => x.Id == Guid.Empty && x.IsFromShipment == false).ToList();
                    if (chargeScene.Count > 0)
                    {
                        var listChargeSceneAdd = mapper.Map<List<CsShipmentSurcharge>>(chargeScene);
                        listChargeSceneAdd.ForEach(req =>
                        {
                            req.Id = Guid.NewGuid();
                            req.SettlementCode = settlement.SettlementNo;
                            req.DatetimeCreated = req.DatetimeModified = DateTime.Now;
                            req.UserCreated = req.UserModified = userCurrent;
                        });
                        dc.CsShipmentSurcharge.AddRange(listChargeSceneAdd);
                    }
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState UpdateSettlementPayment(CreateUpdateSettlementModel model)
        {
            try
            {
                var userCurrent = currentUser.UserID;
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;

                var settlement = mapper.Map<AcctSettlementPayment>(model.Settlement);

                var settlementCurrent = DataContext.Get(x => x.Id == settlement.Id).FirstOrDefault();
                settlement.DatetimeCreated = settlementCurrent.DatetimeCreated;
                settlement.UserCreated = settlementCurrent.UserCreated;

                settlement.DatetimeModified = DateTime.Now;
                settlement.UserModified = userCurrent;
                //Cập nhật lại Status Approval là NEW nếu Status Approval hiện tại là DENIED
                if (settlementCurrent.StatusApproval.Equals(Constants.STATUS_APPROVAL_DENIED))
                {
                    settlement.StatusApproval = Constants.STATUS_APPROVAL_NEW;
                }

                var hs = DataContext.Update(settlement, x => x.Id == settlement.Id);
                if (hs.Success)
                {
                    //Start --Phí chứng từ (IsFromShipment = true)--
                    //Cập nhật SettlementCode = null cho các SettlementNo
                    var chargeShipmentOld = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlement.SettlementNo && x.IsFromShipment == true).ToList();
                    if (chargeShipmentOld.Count > 0)
                    {
                        chargeShipmentOld.ForEach(req =>
                        {
                            req.SettlementCode = null;
                            req.UserModified = userCurrent;
                            req.DatetimeModified = DateTime.Now;
                        });
                        dc.CsShipmentSurcharge.UpdateRange(chargeShipmentOld);
                    }
                    //Cập nhật SettlementCode = SettlementNo cho các SettlementNo
                    var chargeShipmentUpdate = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && x.IsFromShipment == true).Select(s => s.Id).ToList();
                    if (chargeShipmentUpdate.Count > 0)
                    {
                        var listChargeShipmentUpdate = csShipmentSurchargeRepo.Get(x => chargeShipmentUpdate.Contains(x.Id)).ToList();
                        listChargeShipmentUpdate.ForEach(req =>
                        {
                            req.SettlementCode = settlement.SettlementNo;
                            req.UserModified = userCurrent;
                            req.DatetimeModified = DateTime.Now;
                        });
                        dc.CsShipmentSurcharge.UpdateRange(listChargeShipmentUpdate);
                    }
                    //End --Phí chứng từ (IsFromShipment = true)--

                    //Start --Phí hiện trường (IsFromShipment = false)--
                    var chargeScene = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlement.SettlementNo && x.IsFromShipment == false).ToList();
                    var idsChargeScene = chargeScene.Select(x => x.Id);
                    //Add các phí hiện trường mới (nếu có)
                    var chargeSceneAdd = model.ShipmentCharge.Where(x => x.Id == Guid.Empty && x.IsFromShipment == false).ToList();
                    if (chargeSceneAdd.Count > 0)
                    {
                        var listChargeSceneAdd = mapper.Map<List<CsShipmentSurcharge>>(chargeSceneAdd);
                        listChargeSceneAdd.ForEach(req =>
                        {
                            req.Id = Guid.NewGuid();
                            req.SettlementCode = settlement.SettlementNo;
                            req.DatetimeCreated = req.DatetimeModified = DateTime.Now;
                            req.UserCreated = req.UserModified = userCurrent;
                        });
                        dc.CsShipmentSurcharge.AddRange(listChargeSceneAdd);
                    }

                    //Cập nhật lại các thông tin của phí hiện trường (nếu có edit chỉnh sửa phí hiện trường)
                    var chargeSceneUpdate = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && idsChargeScene.Contains(x.Id) && x.IsFromShipment == false);
                    var idChargeSceneUpdate = chargeSceneUpdate.Select(s => s.Id).ToList();
                    if (chargeSceneUpdate.Count() > 0)
                    {
                        var listChargeExists = csShipmentSurchargeRepo.Get(x => idChargeSceneUpdate.Contains(x.Id));
                        var listChargeSceneUpdate = mapper.Map<List<CsShipmentSurcharge>>(chargeSceneUpdate);
                        listChargeSceneUpdate.ForEach(req => {
                            req.UserCreated = listChargeExists.Where(x => x.Id == req.Id).First().UserCreated;
                            req.DatetimeCreated = listChargeExists.Where(x => x.Id == req.Id).First().DatetimeCreated;
                            req.UserModified = userCurrent;
                            req.DatetimeModified = DateTime.Now;
                        });

                        dc.CsShipmentSurcharge.UpdateRange(listChargeSceneUpdate);
                    }

                    //Xóa các phí hiện trường đã chọn xóa của user
                    var chargeSceneRemove = chargeScene.Where(x => !model.ShipmentCharge.Select(s => s.Id).Contains(x.Id)).ToList();
                    if (chargeSceneRemove.Count > 0)
                    {
                        dc.CsShipmentSurcharge.RemoveRange(chargeSceneRemove);
                    }
                    //End --Phí hiện trường (IsFromShipment = false)--
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        #endregion -- INSERT & UPDATE SETTLEMENT PAYMENT --

        #region --- PREVIEW SETTLEMENT PAYMENT ---
        public decimal GetAdvanceAmountByShipmentAndCurrency(string JobId, string MBL, string HBL, string Currency)
        {
            //Chỉ lấy ra các charge có status advance là done
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            var advance = acctAdvancePaymentRepo.Get(x => x.StatusApproval == Constants.STATUS_APPROVAL_DONE);
            var request = acctAdvanceRequestRepo.Get(x => x.JobId == JobId && x.Mbl == MBL && x.Hbl == HBL);
            var query = from adv in advance
                        join req in request on adv.AdvanceNo equals req.AdvanceNo into req1
                        from req in req1.DefaultIfEmpty()
                        select req;
            var advanceAmount = query.Sum(x => x.Amount * GetRateCurrencyExchange(currencyExchange, x.RequestCurrency, Currency));
            return advanceAmount.Value;
        }

        public AscSettlementPaymentRequestReportParams GetFirstShipmentOfSettlement(string settlementNo)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var charge = catChargeRepo.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();

            var advRequest = acctAdvanceRequestRepo.Get();
            var advPayment = acctAdvancePaymentRepo.Get();
            var settlement = DataContext.Get(x => x.StatusApproval == Constants.STATUS_APPROVAL_DONE);
            var settleApprove = acctApproveSettlementRepo.Get(x => x.IsDeputy == false);

            var customer = catPartnerRepo.Get();
            var consignee = catPartnerRepo.Get();
            var consigner = catPartnerRepo.Get();

            var data = from sur in surcharge
                       join cat in charge on sur.ChargeId equals cat.Id into cat2
                       from cat in cat2.DefaultIfEmpty()
                       join opst in opsTrans on sur.Hblid equals opst.Hblid into opst2
                       from opst in opst2.DefaultIfEmpty()
                       join cstd in csTransDe on sur.Hblid equals cstd.Id into cstd2
                       from cstd in cstd2.DefaultIfEmpty()
                       join cst in csTrans on cstd.JobId equals cst.Id into cst2
                       from cst in cst2.DefaultIfEmpty()
                       join request in advRequest on new { JobId = (opst.JobNo == null ? cst.JobNo : opst.JobNo), HBL = (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno), MBL = (opst.Mblno == null ? cstd.Mawb : opst.Mblno) } equals new { JobId = request.JobId, HBL = request.Hbl, MBL = request.Mbl } into request1
                       from request in request1.DefaultIfEmpty()
                       join advance in advPayment on request.AdvanceNo equals advance.AdvanceNo into advance1
                       from advance in advance1.DefaultIfEmpty()
                       join cus in customer on (opst.CustomerId == null ? cstd.CustomerId : opst.CustomerId) equals cus.Id into cus1
                       from cus in cus1.DefaultIfEmpty()
                       join cnee in consignee on cstd.ConsigneeId equals cnee.Id into cnee1
                       from cnee in cnee1.DefaultIfEmpty()
                       join cner in consigner on cstd.ShipperId equals cner.Id into cner1
                       from cner in cner1.DefaultIfEmpty()
                       where
                            sur.SettlementCode == settlementNo
                       select new AscSettlementPaymentRequestReportParams
                       {
                           JobId = (opst.JobNo == null ? cst.JobNo : opst.JobNo),
                           AdvDate = (!string.IsNullOrEmpty(advance.StatusApproval) && advance.StatusApproval == Constants.STATUS_APPROVAL_DONE ? advance.DatetimeModified.Value.ToString("dd/MM/yyyy") : ""),
                           SettlementNo = settlementNo,
                           Customer = cus.PartnerNameVn != null ? cus.PartnerNameVn : "",
                           Consignee = cnee.PartnerNameVn != null ? cnee.PartnerNameVn : "",
                           Consigner = cner.PartnerNameVn != null ? cner.PartnerNameVn : "",
                           ContainerQty = opst.SumContainers.HasValue ? opst.SumContainers.Value.ToString() + "/" : "",
                           GW = opst.SumGrossWeight.HasValue ? opst.SumGrossWeight.Value : 0,
                           NW = opst.SumNetWeight.HasValue ? opst.SumNetWeight.Value : 0,
                           CustomsId = (sur.ClearanceNo == null ? "" : sur.ClearanceNo),
                           PSC = opst.SumPackages.HasValue ? opst.SumPackages.Value : 0,
                           CBM = opst.SumCbm.HasValue ? opst.SumCbm.Value : 0,
                           HBL = (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno),
                           MBL = (opst.Mblno == null ? cstd.Mawb : opst.Mblno),
                           StlCSName = ""
                       };

            return data.OrderByDescending(x => x.JobId).FirstOrDefault();
        }

        public IQueryable<AscSettlementPaymentRequestReport> GetChargeOfSettlement(string settlementNo, string currency)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var charge = catChargeRepo.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();

            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

            var data = from sur in surcharge
                       join cat in charge on sur.ChargeId equals cat.Id into cat2
                       from cat in cat2.DefaultIfEmpty()
                       join opst in opsTrans on sur.Hblid equals opst.Hblid into opst2
                       from opst in opst2.DefaultIfEmpty()
                       join cstd in csTransDe on sur.Hblid equals cstd.Id into cstd2
                       from cstd in cstd2.DefaultIfEmpty()
                       join cst in csTrans on cstd.JobId equals cst.Id into cst2
                       from cst in cst2.DefaultIfEmpty()
                       where
                            sur.SettlementCode == settlementNo
                       select new AscSettlementPaymentRequestReport
                       {
                           AdvID = settlementNo,
                           AdvDate = DateTime.Now,
                           AdvContact = "",
                           AdvAddress = "",
                           StlDescription = "",
                           AdvanceNo = "",
                           AdvValue = 0,
                           AdvCurrency = "",
                           Remains = 0,
                           AdvanceDate = DateTime.Now,
                           No = 0,
                           CustomsID = "",
                           JobID = (opst.JobNo == null ? cst.JobNo : opst.JobNo),
                           HBL = (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno),
                           Description = cat.ChargeNameEn,
                           InvoiceNo = sur.InvoiceNo == null ? "" : sur.InvoiceNo,
                           Amount = (sur.Total != null ? sur.Total : 0) * GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, currency),
                           Debt = false,
                           Currency = "",
                           Note = sur.Notes,
                           AdvDpManagerID = "",
                           AdvDpManagerStickDeny = true,
                           AdvDpManagerStickApp = true,
                           AdvDpManagerName = "",
                           AdvDpSignDate = DateTime.Now,
                           AdvAcsDpManagerID = "",
                           AdvAcsDpManagerStickDeny = true,
                           AdvAcsDpManagerStickApp = true,
                           AdvAcsDpManagerName = "",
                           AdvAcsSignDate = DateTime.Now,
                           AdvBODID = "",
                           AdvBODStickDeny = true,
                           AdvBODStickApp = true,
                           AdvBODName = "",
                           AdvBODSignDate = DateTime.Now,
                           SltAcsCashierName = "",
                           SltCashierDate = DateTime.Now,
                           Saved = true,
                           ClearStatus = true,
                           Status = "",
                           AcsApproval = true,
                           SltDpComment = "",
                           Shipper = "",
                           ShipmentInfo = "",
                           MBLNO = (opst.Mblno == null ? cstd.Mawb : opst.Mblno),
                           VAT = 0,
                           BFVATAmount = 0,
                           ContainerQty = "",
                           Noofpieces = 0,
                           UnitPieaces = "",
                           GrossWeight = 0,
                           NW = 0,
                           CBM = 0,
                           ShipperHBL = "",
                           ConsigneeHBL = "",
                           ModeSettle = "",
                           STT = 0,
                           Series = "",
                           InvoiceDate = DateTime.Now,
                           Inword = "",
                           InvoiceID = "",
                           Commodity = "",
                           ServiceType = "",
                           SltCSName = "",
                           SltCSStickDeny = true,
                           SltCSStickApp = true,
                           SltCSSignDate = DateTime.Now,
                       };

            return data.OrderByDescending(x => x.JobID);
        }

        public Crystal Preview(string settlementNo)
        {
            Crystal result = null;

            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();

            var listSettlementPayment = new List<AscSettlementPaymentRequestReport>();

            listSettlementPayment = GetChargeOfSettlement(settlementNo, settlement.SettlementCurrency).ToList();

            var parameter = new AscSettlementPaymentRequestReportParams();
            parameter = GetFirstShipmentOfSettlement(settlementNo);
            parameter.SettleRequester = settlement != null && !string.IsNullOrEmpty(settlement.Requester) ? GetEmployeeByUserId(settlement.Requester).EmployeeNameVn : "";
            parameter.SettleRequestDate = settlement.RequestDate != null ? settlement.RequestDate.Value.ToString("dd/MM/yyyy") : "";

            //Lấy thông tin các User Approve Settlement
            var infoSettleAprove = GetInfoApproveSettlementNoCheckBySettlementNo(settlementNo);
            parameter.StlDpManagerName = infoSettleAprove != null ? infoSettleAprove.ManagerName : "";
            parameter.StlDpManagerSignDate = infoSettleAprove != null && infoSettleAprove.ManagerAprDate.HasValue ? infoSettleAprove.ManagerAprDate.Value.ToString("dd/MM/yyyy") : "";
            parameter.StlAscDpManagerName = infoSettleAprove != null ? infoSettleAprove.AccountantName : "";
            parameter.StlAscDpManagerSignDate = infoSettleAprove != null && infoSettleAprove.AccountantAprDate.HasValue ? infoSettleAprove.AccountantAprDate.Value.ToString("dd/MM/yyyy") : "";
            parameter.StlBODSignDate = infoSettleAprove != null && infoSettleAprove.BuheadAprDate.HasValue ? infoSettleAprove.BuheadAprDate.Value.ToString("dd/MM/yyyy") : "";

            parameter.CompanyName = Constants.COMPANY_NAME;
            parameter.CompanyAddress1 = Constants.COMPANY_ADDRESS1;
            parameter.CompanyAddress2 = "Tel‎: (‎84‎-‎8‎) ‎3948 6888  Fax‎: +‎84 8 38488 570‎";
            parameter.Website = Constants.COMPANY_WEBSITE;
            parameter.Contact = currentUser.UserID;//Get user login

            //Lấy ra tổng Advance Amount của các charge thuộc Settlement
            decimal advanceAmount = 0;
            var shipments = listSettlementPayment.GroupBy(x => new { x.JobID, x.MBLNO, x.HBL }).Select(x => new { JobID = x.Key.JobID, MBLNO = x.Key.MBLNO, HBL = x.Key.HBL });
            foreach (var item in shipments)
            {
                advanceAmount += GetAdvanceAmountByShipmentAndCurrency(item.JobID, item.MBLNO, item.HBL, settlement.SettlementCurrency);
            }
            parameter.AdvValue = advanceAmount > 0 ? String.Format("{0:n}", advanceAmount) : "";
            parameter.AdvCurrency = advanceAmount > 0 ? settlement.SettlementCurrency : "";

            //Chuyển tiền Amount thành chữ
            decimal _amount = advanceAmount > 0 ? advanceAmount : 0;
            var _inword = "";
            if (_amount > 0)
            {
                var _currency = settlement.SettlementCurrency == Constants.CURRENCY_LOCAL ?
                           (_amount % 1 > 0 ? "đồng lẻ" : "đồng chẵn")
                        :
                        settlement.SettlementCurrency;

                _inword = InWordCurrency.ConvertNumberCurrencyToString(_amount, _currency);
            }
            parameter.Inword = _inword;

            result = new Crystal
            {
                ReportName = "AcsSettlementPaymentRequest.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listSettlementPayment);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
        #endregion --- PREVIEW SETTLEMENT PAYMENT ---

        #region --- APPROVAL SETTLEMENT PAYMENT ---

        public HandleState InsertOrUpdateApprovalSettlement(AcctApproveSettlementModel settlement)
        {
            try
            {
                var userCurrent = currentUser.UserID;

                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;

                var acctApprove = mapper.Map<AcctApproveSettlement>(settlement);

                if (!string.IsNullOrEmpty(settlement.SettlementNo))
                {
                    var settle = DataContext.Get(x => x.SettlementNo == settlement.SettlementNo).FirstOrDefault();
                    if (settle.StatusApproval != Constants.STATUS_APPROVAL_NEW && settle.StatusApproval != Constants.STATUS_APPROVAL_DENIED && settle.StatusApproval != Constants.STATUS_APPROVAL_DONE)
                    {
                        return new HandleState("Awaiting Approval");
                    }
                }

                //Lấy ra các user Leader, Manager Dept của user requester, user Accountant, BUHead(nếu có) của user requester
                acctApprove.Leader = GetLeaderIdOfUser(userCurrent);
                acctApprove.Manager = GetManagerIdOfUser(userCurrent);
                acctApprove.Accountant = GetAccountantId();
                acctApprove.Buhead = GetBUHeadId();


                var checkExistsApproveBySettlementNo = acctApproveSettlementRepo.Get(x => x.SettlementNo == acctApprove.SettlementNo && x.IsDeputy == false).FirstOrDefault();
                if (checkExistsApproveBySettlementNo == null) //Insert AcctApproveSettlement
                {
                    acctApprove.Id = Guid.NewGuid();
                    acctApprove.RequesterAprDate = DateTime.Now;
                    acctApprove.UserCreated = acctApprove.UserModified = userCurrent;
                    acctApprove.DateCreated = acctApprove.DateModified = DateTime.Now;
                    acctApprove.IsDeputy = false;
                    dc.AcctApproveSettlement.Add(acctApprove);
                }
                else //Update AcctApproveSettlement by SettlementNo
                {
                    checkExistsApproveBySettlementNo.RequesterAprDate = DateTime.Now;
                    checkExistsApproveBySettlementNo.UserModified = userCurrent;
                    checkExistsApproveBySettlementNo.DateModified = DateTime.Now;
                    dc.AcctApproveSettlement.Update(checkExistsApproveBySettlementNo);
                }
                dc.SaveChanges();

                var emailLeaderOrManager = "";
                var userLeaderOrManager = "";
                //Send mail đề nghị approve đến Leader(Nếu có) nếu không có thì send tới Manager Dept
                //Lấy ra Leader của User & Manager Dept của User Requester
                if (string.IsNullOrEmpty(acctApprove.Leader))
                {
                    userLeaderOrManager = acctApprove.Manager;
                    //Lấy ra employeeId của managerIdOfUser
                    var employeeIdOfUserManager = GetEmployeeIdOfUser(userLeaderOrManager);
                    //Lấy ra email của Manager
                    emailLeaderOrManager = GetEmployeeByEmployeeId(employeeIdOfUserManager).Email;
                }
                else
                {
                    userLeaderOrManager = acctApprove.Leader;
                    //Lấy ra employeeId của managerIdOfUser
                    var employeeIdOfUserLeader = GetEmployeeIdOfUser(userLeaderOrManager);
                    //Lấy ra email của Leader (hiện tại chưa có nên gán rỗng)
                    emailLeaderOrManager = GetEmployeeByEmployeeId(employeeIdOfUserLeader).Email;
                }

                if (string.IsNullOrEmpty(emailLeaderOrManager)) return new HandleState("Not Found Leader or Manager");

                var sendMailResult = SendMailSuggestApproval(acctApprove.SettlementNo, userLeaderOrManager, emailLeaderOrManager);

                return !sendMailResult ? new HandleState("Send Mail Suggest Approval Fail") : new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message.ToString());
            }
        }

        public HandleState UpdateApproval(Guid settlementId)
        {
            var userCurrent = currentUser.UserID;

            var userAprNext = "";
            var emailUserAprNext = "";

            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var settlement = DataContext.Get(x => x.Id == settlementId).FirstOrDefault();

            if (settlement == null) return new HandleState("Not Found Settlement Payment");

            var approve = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo && x.IsDeputy == false).FirstOrDefault();

            if (approve == null) return new HandleState("Not Found Settlement Approval by SettlementNo is " + settlement.SettlementNo);

            //Lấy ra dept code của userApprove dựa vào userApprove
            var deptCodeOfUser = GetInfoDeptOfUser(userCurrent).Code;


            //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
            var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(settlement.SettlementNo, userCurrent, deptCodeOfUser);
            if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
            if (approve != null && settlement != null)
            {
                if (userCurrent == GetLeaderIdOfUser(settlement.Requester) || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    settlement.StatusApproval = Constants.STATUS_APPROVAL_LEADERAPPROVED;
                    approve.LeaderAprDate = DateTime.Now;//Cập nhật ngày Approve của Leader

                    //Lấy email của Department Manager
                    userAprNext = GetManagerIdOfUser(userCurrent);
                    var userAprNextId = GetEmployeeIdOfUser(userAprNext);
                    emailUserAprNext = GetEmployeeByEmployeeId(userAprNextId).Email;
                }
                else if (userCurrent == GetManagerIdOfUser(settlement.Requester) || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    settlement.StatusApproval = Constants.STATUS_APPROVAL_DEPARTMENTAPPROVED;
                    approve.ManagerAprDate = DateTime.Now;//Cập nhật ngày Approve của Manager
                    approve.ManagerApr = userCurrent;

                    //Lấy email của Accountant Manager
                    userAprNext = GetAccountantId();
                    var userAprNextId = GetEmployeeIdOfUser(userAprNext);
                    emailUserAprNext = GetEmployeeByEmployeeId(userAprNextId).Email;
                }
                else if (userCurrent == GetAccountantId() || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    settlement.StatusApproval = Constants.STATUS_APPROVAL_DONE;
                    approve.AccountantAprDate = approve.BuheadAprDate = DateTime.Now;//Cập nhật ngày Approve của Accountant & BUHead
                    approve.AccountantApr = userCurrent;
                    approve.BuheadApr = approve.Buhead;

                    //Send mail approval success when Accountant approved, mail send to requester
                    SendMailApproved(settlement.SettlementNo, DateTime.Now);
                }

                settlement.UserModified = approve.UserModified = userCurrent;
                settlement.DatetimeModified = approve.DateModified = DateTime.Now;

                dc.AcctSettlementPayment.Update(settlement);
                dc.AcctApproveSettlement.Update(approve);
                dc.SaveChanges();
            }

            if (userCurrent == GetAccountantId())
            {
                return new HandleState();
            }
            else
            {
                if (string.IsNullOrEmpty(emailUserAprNext)) return new HandleState("Not found email of user " + userAprNext);

                //Send mail đề nghị approve
                var sendMailResult = SendMailSuggestApproval(settlement.SettlementNo, userAprNext, emailUserAprNext);

                return sendMailResult ? new HandleState() : new HandleState("Send Mail Suggest Approval Fail");
            }
        }

        public HandleState DeniedApprove(Guid settlementId, string comment)
        {
            var userCurrent = currentUser.UserID;

            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var settlement = DataContext.Get(x => x.Id == settlementId).FirstOrDefault();

            if (settlement == null) return new HandleState("Not Found Settlement Payment");

            var approve = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo && x.IsDeputy == false).FirstOrDefault();
            if (approve == null) return new HandleState("Not Found Approve Settlement by SettlementNo " + settlement.SettlementNo);

            //Lấy ra dept code của userApprove dựa vào userApprove
            var deptCodeOfUser = GetInfoDeptOfUser(userCurrent).Code;

            //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
            var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(settlement.SettlementNo, userCurrent, deptCodeOfUser);
            if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
            if (approve != null && settlement != null)
            {
                if (userCurrent == GetLeaderIdOfUser(settlement.Requester) || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    approve.LeaderAprDate = DateTime.Now;//Cập nhật ngày Denie của Leader
                }
                else if (userCurrent == GetManagerIdOfUser(settlement.Requester) || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    approve.ManagerAprDate = DateTime.Now;//Cập nhật ngày Denie của Manager
                    approve.ManagerApr = userCurrent; //Cập nhật user manager denie                   
                }
                else if (userCurrent == GetAccountantId() || GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent))
                {
                    approve.AccountantAprDate = DateTime.Now;//Cập nhật ngày Denie của Accountant
                    approve.AccountantApr = userCurrent; //Cập nhật user accountant denie
                }

                approve.UserModified = userCurrent;
                approve.DateModified = DateTime.Now;
                approve.Comment = comment;
                approve.IsDeputy = true;
                dc.AcctApproveSettlement.Update(approve);

                //Cập nhật lại advance status của Settlement Payment
                settlement.StatusApproval = Constants.STATUS_APPROVAL_DENIED;
                settlement.UserModified = userCurrent;
                settlement.DatetimeModified = DateTime.Now;
                dc.AcctSettlementPayment.Update(settlement);

                dc.SaveChanges();
            }

            //Send mail denied approval
            var sendMailResult = SendMailDeniedApproval(settlement.SettlementNo, comment, DateTime.Now);
            return sendMailResult ? new HandleState() : new HandleState("Send Mail Denie Approval Fail");
        }

        public AcctApproveSettlementModel GetInfoApproveSettlementBySettlementNo(string settlementNo)
        {
            var userCurrent = currentUser.UserID;
            var approveSettlement = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementNo && x.IsDeputy == false).FirstOrDefault();
            var aprSettlementMap = new AcctApproveSettlementModel();

            if (approveSettlement != null)
            {
                aprSettlementMap = mapper.Map<AcctApproveSettlementModel>(approveSettlement);

                //Kiểm tra User đăng nhập vào có thuộc các user Approve Settlement không, nếu không thuộc bất kỳ 1 user nào thì gán cờ IsApproved bằng true
                //Kiểm tra xem dept đã approve chưa, nếu dept của user đó đã approve thì gán cờ IsApproved bằng true           
                aprSettlementMap.IsApproved = CheckUserInApproveSettlementAndDeptApproved(userCurrent, aprSettlementMap);
                aprSettlementMap.RequesterName = string.IsNullOrEmpty(aprSettlementMap.Requester) ? null : GetEmployeeByUserId(aprSettlementMap.Requester).EmployeeNameVn;
                aprSettlementMap.LeaderName = string.IsNullOrEmpty(aprSettlementMap.Leader) ? null : GetEmployeeByUserId(aprSettlementMap.Leader).EmployeeNameVn;
                aprSettlementMap.ManagerName = string.IsNullOrEmpty(aprSettlementMap.Manager) ? null : GetEmployeeByUserId(aprSettlementMap.Manager).EmployeeNameVn;
                aprSettlementMap.AccountantName = string.IsNullOrEmpty(aprSettlementMap.Accountant) ? null : GetEmployeeByUserId(aprSettlementMap.Accountant).EmployeeNameVn;
                aprSettlementMap.BUHeadName = string.IsNullOrEmpty(aprSettlementMap.Buhead) ? null : GetEmployeeByUserId(aprSettlementMap.Buhead).EmployeeNameVn;
            }
            else
            {
                //Mặc định nếu chưa send request thì gán IsApproved bằng true (nhằm để disable button Approve & Deny)
                aprSettlementMap.IsApproved = true;
            }
            return aprSettlementMap;
        }

        public AcctApproveSettlementModel GetInfoApproveSettlementNoCheckBySettlementNo(string settlementNo)
        {
            var settleApprove = acctApproveSettlementRepo.Get(x => x.IsDeputy == false && x.SettlementNo == settlementNo);

            var userRequest = sysUserRepo.Get();
            var empRequest = sysEmployeeRepo.Get();
            var userManager = sysUserRepo.Get();
            var empManager = sysEmployeeRepo.Get();
            var userAccountant = sysUserRepo.Get();
            var empAccountant = sysEmployeeRepo.Get();

            var data = from settleapr in settleApprove
                       join ureq in userRequest on settleapr.Requester equals ureq.Id into ureq1
                       from ureq in ureq1.DefaultIfEmpty()
                       join ereq in empRequest on ureq.EmployeeId equals ereq.Id into ereq2
                       from ereq in ereq2.DefaultIfEmpty()
                       join uman in userManager on settleapr.Manager equals uman.Id into uman1
                       from uman in uman1.DefaultIfEmpty()
                       join eman in empManager on uman.EmployeeId equals eman.Id into eman2
                       from eman in eman2.DefaultIfEmpty()
                       join uacct in userAccountant on settleapr.Accountant equals uacct.Id into uacct1
                       from uacct in uacct1.DefaultIfEmpty()
                       join eacct in empAccountant on uacct.EmployeeId equals eacct.Id into eacct2
                       from eacct in eacct2.DefaultIfEmpty()
                       select new AcctApproveSettlementModel
                       {
                           Id = settleapr.Id,
                           SettlementNo = settleapr.SettlementNo,
                           Requester = settleapr.Requester,
                           RequesterName = ereq.EmployeeNameVn,
                           RequesterAprDate = settleapr.RequesterAprDate,
                           ManagerName = eman.EmployeeNameVn,
                           ManagerAprDate = settleapr.ManagerAprDate,
                           AccountantName = eacct.EmployeeNameVn,
                           AccountantAprDate = settleapr.AccountantAprDate,
                           BuheadAprDate = settleapr.BuheadAprDate
                       };
            return data.FirstOrDefault();
        }
        #endregion --- APPROVAL SETTLEMENT PAYMENT ---

        #region --- COPY CHARGE ---        
        /// <summary>
        /// Lấy danh sách phí hiện trường dựa vào SettlementNo
        /// </summary>
        /// <param name="settlementNo"></param>
        /// <returns></returns>
        public List<ShipmentChargeSettlement> CopyChargeFromSettlementOldToSettlementNew(ShipmentsCopyCriteria criteria)
        {
            var chargesCopy = new List<ShipmentChargeSettlement>();
            if (criteria.charges.Count == 0 || criteria.shipments.Count == 0) return chargesCopy;

            foreach (var shipment in criteria.shipments)
            {
                foreach (var charge in criteria.charges)
                {
                    var chargeCopy = new ShipmentChargeSettlement();
                    chargeCopy = charge;
                    chargeCopy.Id = Guid.Empty;
                    chargeCopy.JobId = shipment.JobId;
                    chargeCopy.HBL = shipment.HBL;
                    chargeCopy.MBL = shipment.MBL;
                    chargeCopy.SettlementCode = null;

                    chargesCopy.Add(chargeCopy);
                }
            }
            return chargesCopy;
        }
        #endregion --- COPY CHARGE ---

        #region ----METHOD PRIVATE----
        private string CreateSettlementNo()
        {
            string year = (DateTime.Now.Year.ToString()).Substring(2, 2);
            string month = DateTime.Now.ToString("DDMMYYYY").Substring(2, 2);
            string prefix = "SM" + year + month + "/";
            string stt;

            //Lấy ra dòng cuối cùng của table acctSettlementPayment
            var rowlast = DataContext.Get().OrderByDescending(x => x.SettlementNo).FirstOrDefault();

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
            if (currencyExchange.Count == 0) return 0;

            currencyFrom = currencyFrom.Trim();
            currencyTo = currencyTo.Trim();

            if (currencyFrom != currencyTo)
            {
                var get1 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyFrom && x.CurrencyToId.Trim() == currencyTo).OrderByDescending(x => x.Rate).FirstOrDefault();
                if (get1 != null)
                {
                    return get1.Rate;
                }
                else
                {
                    var get2 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyTo && x.CurrencyToId.Trim() == currencyFrom).OrderByDescending(x => x.Rate).FirstOrDefault();
                    if (get2 != null)
                    {
                        return 1 / get2.Rate;
                    }
                    else
                    {
                        var get3 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyFrom || x.CurrencyFromId.Trim() == currencyTo).OrderByDescending(x => x.Rate).ToList();
                        if (get3.Count > 1)
                        {
                            if (get3[0].CurrencyFromId.Trim() == currencyFrom && get3[1].CurrencyFromId.Trim() == currencyTo)
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

        /// <summary>
        /// Lấy ra danh sách JobId dựa vào SettlementNo
        /// </summary>
        /// <param name="settlementNo"></param>
        /// <returns></returns>
        private IQueryable<string> GetJobIdBySettlementNo(string settlementNo)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var opst = opsTransactionRepo.Get();
            var csTrans = csTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            var query = from sur in surcharge
                        join ops in opst on sur.Hblid equals ops.Hblid into op
                        from ops in op.DefaultIfEmpty()
                        join cstd in csTransDe on sur.Hblid equals cstd.Id into csd
                        from cstd in csd.DefaultIfEmpty()
                        join cst in csTrans on cstd.JobId equals cst.Id into cs
                        from cst in cs.DefaultIfEmpty()
                        where
                            sur.SettlementCode == settlementNo && (ops.JobNo == null ? cst.JobNo : ops.JobNo) != null
                        select new { JobId = (ops.JobNo == null ? cst.JobNo : ops.JobNo) };
            var listJobId = query
                            .GroupBy(x => new { x.JobId })
                            .Select(s => s.Key.JobId);
            return listJobId;
        }

        /// <summary>
        /// Lấy ra danh sách shipment dựa vào SettlementNo
        /// </summary>
        /// <param name="settlementNo"></param>
        /// <returns></returns>
        private IQueryable<Shipments> GetShipmentBySettlementNo(string settlementNo)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var opst = opsTransactionRepo.Get();
            var csTrans = csTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            var query = from sur in surcharge
                        join ops in opst on sur.Hblid equals ops.Hblid into op
                        from ops in op.DefaultIfEmpty()
                        join cstd in csTransDe on sur.Hblid equals cstd.Id into csd
                        from cstd in csd.DefaultIfEmpty()
                        join cst in csTrans on cstd.JobId equals cst.Id into cs
                        from cst in cs.DefaultIfEmpty()
                        where
                            sur.SettlementCode == settlementNo
                        && (ops.JobNo == null ? cst.JobNo : ops.JobNo) != null
                        && (ops.Hwbno == null ? cstd.Hwbno : ops.Hwbno) != null
                        && (ops.Mblno == null ? cstd.Mawb : ops.Mblno) != null
                        select new Shipments
                        {
                            JobId = (ops.JobNo == null ? cst.JobNo : ops.JobNo),
                            HBL = (ops.Hwbno == null ? cstd.Hwbno : ops.Hwbno),
                            MBL = (ops.Mblno == null ? cstd.Mawb : ops.Mblno)
                        };
            var listShipment = query
                            .GroupBy(x => new { x.JobId, x.MBL, x.HBL })
                            .Select(s => new Shipments { JobId = s.Key.JobId, MBL = s.Key.MBL, HBL = s.Key.HBL });
            return listShipment;
        }

        /// <summary>
        /// Lấy ra danh sách AdvanceNo(chỉ lấy ra các advance có status là done) dựa vào Shipment
        /// </summary>
        /// <param name="JobId"></param>
        /// <param name="MBL"></param>
        /// <param name="HBL"></param>
        /// <returns></returns>
        private IQueryable<string> GetAdvanceNoByShipment(string JobId, string MBL, string HBL)
        {
            //Chỉ lấy ra những Advance thuộc shipment có status là done
            var request = acctAdvanceRequestRepo.Get();
            var advance = acctAdvancePaymentRepo.Get();
            var query = from req in request
                        join ad in advance on req.AdvanceNo equals ad.AdvanceNo into ad2
                        from ad in ad2.DefaultIfEmpty()
                        where
                               ad.StatusApproval == Constants.STATUS_APPROVAL_DONE
                            && req.JobId == JobId
                            && req.Mbl == MBL
                            && req.Hbl == HBL
                        select new
                        {
                            AdvanceNo = req.AdvanceNo
                        };
            var listAdvanceNo = query.GroupBy(x => new { x.AdvanceNo }).Where(x => x.Key.AdvanceNo != null).Select(s => s.Key.AdvanceNo);
            return listAdvanceNo;
        }

        //Lấy ra groupId của User
        private int GetGroupIdOfUser(string userId)
        {
            //Lấy ra groupId của user
            var grpIdOfUser = sysUserGroupRepo.Get(x => x.UserId == userId).FirstOrDefault().GroupId;
            return grpIdOfUser;
        }

        //Lấy Info của Group 
        private SysGroup GetInfoGroupOfUser(string userId)
        {
            var grpIdOfUser = GetGroupIdOfUser(userId);
            var infoGrpOfUser = sysGroupRepo.Get(x => x.Id == grpIdOfUser).FirstOrDefault();
            return infoGrpOfUser;
        }

        //Lấy Info Dept của User
        private CatDepartment GetInfoDeptOfUser(string userId, string idBranch = "27d26acb-e247-47b7-961e-afa7b3d7e11e")
        {
            var deptIdOfUser = GetInfoGroupOfUser(userId).DepartmentId;
            var deptOfUser = catDepartmentRepo.Get(x => x.BranchId == Guid.Parse(idBranch) && x.Id == deptIdOfUser).FirstOrDefault();
            return deptOfUser;
        }

        //Lấy ra Leader của User
        //Leader đây chính là ManagerID của Group
        private string GetLeaderIdOfUser(string userId)
        {
            var leaderIdOfUser = GetInfoGroupOfUser(userId).ManagerId;
            return leaderIdOfUser;
        }

        //Lấy ra ManagerId của User
        private string GetManagerIdOfUser(string userId, string idBranch = "27d26acb-e247-47b7-961e-afa7b3d7e11e")
        {
            //Lấy ra deptId của User
            var deptIdOfUser = GetInfoGroupOfUser(userId).DepartmentId;
            //Lấy ra mangerId của User
            var managerIdOfUser = catDepartmentRepo.Get(x => x.BranchId == Guid.Parse(idBranch) && x.Id == deptIdOfUser).FirstOrDefault().ManagerId;
            return managerIdOfUser;
        }

        //Lấy ra AccountantManagerId của Dept Accountant
        //Đang gán cứng BrandId của Branch ITL HCM (27d26acb-e247-47b7-961e-afa7b3d7e11e)
        private string GetAccountantId(string idBranch = "27d26acb-e247-47b7-961e-afa7b3d7e11e")
        {
            var accountantManagerId = catDepartmentRepo.Get(x => x.BranchId == Guid.Parse(idBranch) && x.Code == Constants.DEPT_CODE_ACCOUNTANT).FirstOrDefault().ManagerId;
            return accountantManagerId;
        }

        //Lấy ra BUHeadId của BUHead
        //Đang gán cứng BrandId của Branch ITL HCM (27d26acb-e247-47b7-961e-afa7b3d7e11e)
        private string GetBUHeadId(string idBranch = "27d26acb-e247-47b7-961e-afa7b3d7e11e")
        {
            var buHeadId = sysBranchRepo.Get(x => x.Id == Guid.Parse(idBranch)).FirstOrDefault().ManagerId;
            return buHeadId;
        }

        //Lấy ra employeeId của User
        private string GetEmployeeIdOfUser(string UserId)
        {
            return sysUserRepo.Get(x => x.Id == UserId).FirstOrDefault().EmployeeId;
        }

        //Lấy info Employee của User dựa vào employeeId
        private SysEmployee GetEmployeeByEmployeeId(string employeeId)
        {
            return sysEmployeeRepo.Get(x => x.Id == employeeId).FirstOrDefault();
        }

        //Lấy info Employee của User dựa vào userId
        private SysEmployee GetEmployeeByUserId(string userId)
        {
            var employeeId = GetEmployeeIdOfUser(userId);
            return sysEmployeeRepo.Get(x => x.Id == employeeId).FirstOrDefault();
        }

        //Lấy ra ds các user được ủy quyền theo nhóm leader, manager department, accountant manager, BUHead dựa vào dept
        //Đang gán cứng BrandId của Branch ITL HCM (27d26acb-e247-47b7-961e-afa7b3d7e11e)
        private List<string> GetListUserDeputyByDept(string dept, string idBranch = "27d26acb-e247-47b7-961e-afa7b3d7e11e")
        {
            Dictionary<string, string> listUsers = new Dictionary<string, string> {
                 { "william.hiep", Constants.DEPT_CODE_OPS },//User ủy quyền cho dept OPS
                 { "linda.linh", Constants.DEPT_CODE_ACCOUNTANT },//User ủy quyền cho dept Accountant
                 { "christina.my", Constants.DEPT_CODE_ACCOUNTANT }//User ủy quyền cho dept Accountant
            };
            var list = listUsers.ToList();
            var deputy = listUsers.Where(x => x.Value == dept).Select(x => x.Key).ToList();
            return deputy;
        }

        //Check group trước đó đã được approve hay chưa? Nếu group trước đó đã approve thì group hiện tại mới được Approve
        //Nếu group hiện tại đã được approve thì không cho approve nữa
        private HandleState CheckApprovedOfDeptPrevAndDeptCurrent(string settlementNo, string userId, string deptOfUser)
        {
            HandleState result = new HandleState("Not Found");

            //Lấy ra Settlement Approval dựa vào settlementNo
            var acctApprove = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementNo && x.IsDeputy == false).FirstOrDefault();
            if (acctApprove == null)
            {
                result = new HandleState("Not Found Settlement Approval by SettlementNo is " + settlementNo);
                return result;
            }

            //Lấy ra Settlement Payment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            if (settlement == null)
            {
                result = new HandleState("Not Found Settlement Payment by SettlementNo is" + settlementNo);
                return result;
            }


            //Trường hợp không có Leader
            if (string.IsNullOrEmpty(acctApprove.Leader))
            {
                //Manager Department Approve
                //Kiểm tra user có phải là dept manager hoặc có phải là user được ủy quyền duyệt hay không
                if (userId == GetManagerIdOfUser(settlement.Requester) || GetListUserDeputyByDept(deptOfUser).Contains(userId))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (GetInfoGroupOfUser(userId).DepartmentId != GetInfoGroupOfUser(settlement.Requester).DepartmentId)
                    {
                        result = new HandleState("Not in the same department");
                    }
                    else
                    {
                        result = new HandleState();
                    }

                    //Requester đã approve thì Manager mới được phép Approve
                    if (!string.IsNullOrEmpty(acctApprove.Requester) && acctApprove.RequesterAprDate != null)
                    {
                        result = new HandleState();
                        //Check group CSManager đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                            && acctApprove.ManagerAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                        {
                            result = new HandleState("Manager Department Approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Requester or Requester not approve");
                    }
                }

                //Accountant Approve
                //Kiểm tra user có phải là Accountant Manager hoặc có phải là user được ủy quyền duyệt hay không
                if (userId == GetAccountantId() || GetListUserDeputyByDept(deptOfUser).Contains(userId))
                {
                    //Check group DepartmentManager đã được Approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Manager)
                        && settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                        && acctApprove.ManagerAprDate != null
                        && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                    {
                        result = new HandleState();
                        //Check group Accountant đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_DONE)
                            && acctApprove.AccountantAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.AccountantApr))
                        {
                            result = new HandleState("Accountant Approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Manager or Manager not approve");
                    }
                }
            }
            else //Trường hợp có leader
            {
                //Leader Approve
                if (userId == GetLeaderIdOfUser(settlement.Requester) || GetListUserDeputyByDept(deptOfUser).Contains(userId))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (GetInfoGroupOfUser(userId).DepartmentId != GetInfoGroupOfUser(settlement.Requester).DepartmentId)
                    {
                        result = new HandleState("Not in the same department");
                    }
                    else
                    {
                        result = new HandleState();
                    }

                    //Check Requester đã approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Requester)
                        && acctApprove.RequesterAprDate != null)
                    {
                        result = new HandleState();
                        //Check group Leader đã được approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_LEADERAPPROVED)
                            && acctApprove.LeaderAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.Leader))
                        {
                            result = new HandleState("Leader Approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Requester or Requester not approve");
                    }
                }

                //Manager Department Approve
                if (userId == GetManagerIdOfUser(settlement.Requester) || GetListUserDeputyByDept(deptOfUser).Contains(userId))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (GetInfoGroupOfUser(userId).DepartmentId != GetInfoGroupOfUser(settlement.Requester).DepartmentId)
                    {
                        result = new HandleState("Not in the same department");
                    }
                    else
                    {
                        result = new HandleState();
                    }

                    //Check group Leader đã được approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Leader)
                        && settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_LEADERAPPROVED)
                        && acctApprove.LeaderAprDate != null)
                    {
                        result = new HandleState();
                        //Check group Manager Department đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                            && acctApprove.ManagerAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                        {
                            result = new HandleState("Manager Department Approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Leader or Leader not approve");
                    }

                }

                //Accountant Approve
                //Kiểm tra user có phải là Accountant Manager hoặc có phải là user được ủy quyền duyệt hay không
                if (userId == GetAccountantId() || GetListUserDeputyByDept(deptOfUser).Contains(userId))
                {
                    //Check group DepartmentManager đã được Approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Manager)
                        && settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                        && acctApprove.ManagerAprDate != null
                        && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                    {
                        result = new HandleState();
                        //Check group Accountant đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (settlement.StatusApproval.Equals(Constants.STATUS_APPROVAL_DONE)
                            && acctApprove.AccountantAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.AccountantApr))
                        {
                            result = new HandleState("Accountant Approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Manager or Manager not approve");
                    }
                }
            }

            return result;
        }

        //Send Mail đề nghị Approve
        private bool SendMailSuggestApproval(string settlementNo, string userReciver, string emailUserReciver)
        {
            var surcharge = csShipmentSurchargeRepo.Get();

            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            //Lấy ra SettlementPayment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();

            //Lấy ra tên & email của user Requester
            var requesterId = GetEmployeeIdOfUser(settlement.Requester);
            var requesterName = GetEmployeeByEmployeeId(requesterId).EmployeeNameVn;
            var emailRequester = GetEmployeeByEmployeeId(requesterId).Email;

            //Lấy ra thông tin JobId dựa vào SettlementNo
            var listJobId = GetJobIdBySettlementNo(settlementNo);
            string jobIds = "";
            foreach (var jobId in listJobId)
            {
                jobIds += !string.IsNullOrEmpty(jobId) ? jobId + "; " : "";
            }
            jobIds += ")";
            jobIds = jobIds.Replace("; )", "").Replace(")", "");

            var totalAmount = surcharge
                .Where(x => x.SettlementCode == settlementNo)
                .Sum(x => x.Total * GetRateCurrencyExchange(currencyExchange, x.CurrencyId, settlement.SettlementCurrency));
            totalAmount = Math.Round(totalAmount, 2);

            //Lấy ra list shipment(JobId,MBL,HBL) dựa vào SettlementNo
            var shipments = GetShipmentBySettlementNo(settlementNo);
            //Lấy ra list AdvanceNo dựa vào Shipment(JobId,MBL,HBL)
            string advanceNos = "";
            if (shipments != null && shipments.Count() > 0)
            {
                foreach (var shipment in shipments)
                {
                    var listAdvanceNo = GetAdvanceNoByShipment(shipment.JobId, shipment.MBL, shipment.HBL);
                    foreach (var advanceNo in listAdvanceNo)
                    {
                        advanceNos += !string.IsNullOrEmpty(advanceNo) ? advanceNo + "; " : "";
                    }
                }
                advanceNos += ")";
                advanceNos = advanceNos.Replace("; )", "").Replace(")", "");
            }

            //Mail Info
            string subject = "eFMS - Settlement Payment Approval Request from [RequesterName]";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p> <i> <b>Dear Mr/Mrs [UserName],</b> </i></p><p>You have new Settlement Payment Approval Request from <b>[RequesterName]</b> as below info:</p><p> <i>Anh/ Chị có một yêu cầu duyệt thanh toán từ <b>[RequesterName]</b> với thông tin như sau: </i></p><ul><li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li><li>Settlement Amount/ <i>Số tiền thanh toán</i> : <b>[TotalAmount] [CurrencySettlement]</b></li><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li></ul><p>You click here to check more detail and approve: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]/approve' target='_blank'>Detail Payment Request</a> </span></p><p> <i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]/approve' target='_blank'>Chi tiết phiếu đề nghị thanh toán</a> </span> </i></p><p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='{0}'/></p></div>", logoeFMSBase64());
            body = body.Replace("[UserName]", userReciver);
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[SettlementNo]", settlementNo);
            body = body.Replace("[TotalAmount]", String.Format("{0:n}", totalAmount));
            body = body.Replace("[CurrencySettlement]", settlement.SettlementCurrency);
            body = body.Replace("[AdvanceNos]", advanceNos);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[RequestDate]", settlement.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/accounting/settlement-payment");
            body = body.Replace("[SettlementId]", settlement.Id.ToString());
            List<string> toEmails = new List<string> {
                emailUserReciver
            };
            List<string> attachments = null;

            //CC cho User Requester để biết được quá trình Approve đã đến bước nào
            //Và các User thuộc group của User Approve được ủy quyền
            List<string> emailCCs = new List<string> {
                emailRequester
            };

            //Lấy ra email của các User được ủy quyền của group của User Approve
            var deptCodeOfUserReciver = GetInfoDeptOfUser(userReciver).Code;
            var usersDeputy = GetListUserDeputyByDept(deptCodeOfUserReciver);
            if (usersDeputy.Count > 0)
            {
                foreach (var userId in usersDeputy)
                {
                    //Lấy ra employeeId của user
                    var employeeIdOfUser = GetEmployeeIdOfUser(userId);
                    //Lấy ra email của user
                    var emailUser = GetEmployeeByEmployeeId(employeeIdOfUser).Email;
                    emailCCs.Add(emailUser);
                }
            }

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        //Send Mail Approved
        private bool SendMailApproved(string settlementNo, DateTime approvedDate)
        {
            var surcharge = csShipmentSurchargeRepo.Get();

            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            //Lấy ra SettlementPayment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();

            //Lấy ra tên & email của user Requester
            var requesterId = GetEmployeeIdOfUser(settlement.Requester);
            var requesterName = GetEmployeeByEmployeeId(requesterId).EmployeeNameVn;
            var emailRequester = GetEmployeeByEmployeeId(requesterId).Email;

            //Lấy ra thông tin JobId dựa vào SettlementNo
            var listJobId = GetJobIdBySettlementNo(settlementNo);
            string jobIds = "";
            foreach (var jobId in listJobId)
            {
                jobIds += !string.IsNullOrEmpty(jobId) ? jobId + "; " : "";
            }
            jobIds += ")";
            jobIds = jobIds.Replace("; )", "").Replace(")", "");

            var totalAmount = surcharge
                .Where(x => x.SettlementCode == settlementNo)
                .Sum(x => x.Total * GetRateCurrencyExchange(currencyExchange, x.CurrencyId, settlement.SettlementCurrency));
            totalAmount = Math.Round(totalAmount, 2);

            //Lấy ra list shipment(JobId,MBL,HBL) dựa vào SettlementNo
            var shipments = GetShipmentBySettlementNo(settlementNo);
            //Lấy ra list AdvanceNo dựa vào Shipment(JobId,MBL,HBL)
            string advanceNos = "";
            if (shipments != null && shipments.Count() > 0)
            {
                foreach (var shipment in shipments)
                {
                    var listAdvanceNo = GetAdvanceNoByShipment(shipment.JobId, shipment.MBL, shipment.HBL);
                    foreach (var advanceNo in listAdvanceNo)
                    {
                        advanceNos += !string.IsNullOrEmpty(advanceNo) ? advanceNo + "; " : "";
                    }
                }
                advanceNos += ")";
                advanceNos = advanceNos.Replace("; )", "").Replace(")", "");
            }

            //Mail Info
            string subject = "eFMS - Settlement Payment from [RequesterName] is approved";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p> <i> <b>Dear Mr/Mrs [RequesterName],</b> </i></p><p>You have an Settlement Payment is approved at <b>[ApprovedDate]</b> as below info:</p><p> <i>Anh/ Chị có một đề nghị thanh toán đã được phê duyệt vào lúc <b>[ApprovedDate]</b> với thông tin như sau: </i></p><ul><li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li><li>Settlement Amount/ <i>Số tiền thanh toán</i> : <b>[TotalAmount] [CurrencySettlement]</b></li><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li></ul><p>You can click here to check more detail: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Detail Payment Request</a> </span></p><p> <i>Anh/ Chị có thể chọn vào đây để biết thêm thông tin chi tiết: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Chi tiết đề nghị thanh toán</a> </span> </i></p><p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='{0}'/></p></div>", logoeFMSBase64());
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[ApprovedDate]", approvedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[SettlementNo]", settlementNo);
            body = body.Replace("[TotalAmount]", String.Format("{0:n}", totalAmount));
            body = body.Replace("[CurrencySettlement]", settlement.SettlementCurrency);
            body = body.Replace("[AdvanceNos]", advanceNos);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[RequestDate]", settlement.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/accounting/settlement-payment");
            body = body.Replace("[SettlementId]", settlement.Id.ToString());
            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;
            List<string> emailCCs = new List<string>
            {
                //Không cần email cc
            };

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        //Send Mail Deny Approve
        private bool SendMailDeniedApproval(string settlementNo, string comment, DateTime DeniedDate)
        {
            var surcharge = csShipmentSurchargeRepo.Get();

            //Lấy danh sách Currency Exchange của ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            //Lấy ra SettlementPayment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();

            //Lấy ra tên & email của user Requester
            var requesterId = GetEmployeeIdOfUser(settlement.Requester);
            var requesterName = GetEmployeeByEmployeeId(requesterId).EmployeeNameVn;
            var emailRequester = GetEmployeeByEmployeeId(requesterId).Email;

            //Lấy ra thông tin JobId dựa vào SettlementNo
            var listJobId = GetJobIdBySettlementNo(settlementNo);
            string jobIds = "";
            foreach (var jobId in listJobId)
            {
                jobIds += !string.IsNullOrEmpty(jobId) ? jobId + "; " : "";
            }
            jobIds += ")";
            jobIds = jobIds.Replace("; )", "").Replace(")", "");

            var totalAmount = surcharge
                .Where(x => x.SettlementCode == settlementNo)
                .Sum(x => x.Total * GetRateCurrencyExchange(currencyExchange, x.CurrencyId, settlement.SettlementCurrency));
            totalAmount = Math.Round(totalAmount, 2);

            //Lấy ra list shipment(JobId,MBL,HBL) dựa vào SettlementNo
            var shipments = GetShipmentBySettlementNo(settlementNo);
            //Lấy ra list AdvanceNo dựa vào Shipment(JobId,MBL,HBL)
            string advanceNos = "";
            if (shipments != null && shipments.Count() > 0)
            {
                foreach (var shipment in shipments)
                {
                    var listAdvanceNo = GetAdvanceNoByShipment(shipment.JobId, shipment.MBL, shipment.HBL);
                    foreach (var advanceNo in listAdvanceNo)
                    {
                        advanceNos += !string.IsNullOrEmpty(advanceNo) ? advanceNo + "; " : "";
                    }
                }
                advanceNos += ")";
                advanceNos = advanceNos.Replace("; )", "").Replace(")", "");
            }

            //Mail Info
            string subject = "eFMS - Settlement Payment from [RequesterName] is denied";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p> <i> <b>Dear Mr/Mrs [RequesterName],</b> </i></p><p>You have an Settlement Payment is denied at <b>[DeniedDate]</b> by as below info:</p><p> <i>Anh/ Chị có một yêu cầu đề nghị thanh toán đã bị từ chối vào lúc <b>[DeniedDate]</b> by với thông tin như sau: </i></p><ul><li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li><li>Settlement Amount/ <i>Số tiền tạm ứng</i> : <b>[TotalAmount] [CurrencySettlement]</b></li><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li><li>Comment/ <i>Lý do từ chối</i> : <b>[Comment]</b></li></ul><p>You click here to recheck detail: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Detail Payment Request</a> </span></p><p> <i>Anh/ Chị chọn vào đây để kiểm tra lại thông tin chi tiết: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Chi tiết đề nghị thanh toán</a> </span> </i></p><p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='{0}'/></p></div>", logoeFMSBase64());
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[DeniedDate]", DeniedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[SettlementNo]", settlementNo);
            body = body.Replace("[TotalAmount]", String.Format("{0:n}", totalAmount));
            body = body.Replace("[CurrencySettlement]", settlement.SettlementCurrency);
            body = body.Replace("[AdvanceNos]", advanceNos);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[RequestDate]", settlement.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("[Comment]", comment);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/accounting/settlement-payment");
            body = body.Replace("[SettlementId]", settlement.Id.ToString());
            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;
            List<string> emailCCs = new List<string>
            {
                //Add các email của các user đã approve trước đó (không cần thiết)
            };

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        //Logo eFMS
        private string logoeFMSBase64()
        {
            return "data:image/png;base64,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";
        }

        //Kiểm tra User đăng nhập vào có thuộc các user Approve Settlement không, nếu không thuộc bất kỳ 1 user nào thì gán cờ IsApproved bằng true
        //Kiểm tra xem dept đã approve chưa, nếu dept của user đó đã approve thì gán cờ IsApproved bằng true
        private bool CheckUserInApproveSettlementAndDeptApproved(string userCurrent, AcctApproveSettlementModel approveSettlement)
        {
            var isApproved = false;
            if (userCurrent == approveSettlement.Requester) //Requester
            {
                isApproved = true;
                if (approveSettlement.RequesterAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent == approveSettlement.Leader) //Leader
            {
                isApproved = true;
                if (approveSettlement.LeaderAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent == approveSettlement.Manager || userCurrent == approveSettlement.ManagerApr) //Dept Manager
            {
                isApproved = true;
                if (string.IsNullOrEmpty(approveSettlement.ManagerApr) && approveSettlement.ManagerAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent == approveSettlement.Accountant || userCurrent == approveSettlement.AccountantApr) //Accountant Manager
            {
                isApproved = true;
                if (string.IsNullOrEmpty(approveSettlement.AccountantApr) && approveSettlement.AccountantAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent == approveSettlement.Buhead || userCurrent == approveSettlement.BuheadApr) //BUHead
            {
                isApproved = true;
                if (string.IsNullOrEmpty(approveSettlement.BuheadApr) && approveSettlement.BuheadAprDate == null)
                {
                    isApproved = false;
                }
            }
            else
            {
                //Đây là trường hợp các User không thuộc Approve Settlement
                isApproved = true;
            }
            return isApproved;
        }


        #endregion
    }
}
