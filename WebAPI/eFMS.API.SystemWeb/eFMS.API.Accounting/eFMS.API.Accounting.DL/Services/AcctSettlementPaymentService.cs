﻿using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Accounting.DL.Models.ReportResults;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
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
    public class AcctSettlementPaymentService : RepositoryBase<AcctSettlementPayment, AcctSettlementPaymentModel>, IAcctSettlementPaymentService
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
        readonly IContextBase<CatDepartment> catDepartmentRepo;
        readonly IContextBase<SysEmployee> sysEmployeeRepo;
        readonly IContextBase<CatCharge> catChargeRepo;
        readonly IContextBase<CatUnit> catUnitRepo;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IAcctAdvancePaymentService acctAdvancePaymentService;
        readonly ICurrencyExchangeService currencyExchangeService;
        readonly IUserBaseService userBaseService;

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
            IContextBase<CatDepartment> catDepartment,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<CatCharge> catCharge,
            IContextBase<CatUnit> catUnit,
            IContextBase<CatPartner> catPartner,
            IAcctAdvancePaymentService advance,
            ICurrencyExchangeService currencyExchange,
            IUserBaseService userBase) : base(repository, mapper)
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
            catDepartmentRepo = catDepartment;
            sysEmployeeRepo = sysEmployee;
            catChargeRepo = catCharge;
            catUnitRepo = catUnit;
            catPartnerRepo = catPartner;
            acctAdvancePaymentService = advance;
            currencyExchangeService = currencyExchange;
            userBaseService = userBase;
        }

        #region --- LIST SETTLEMENT PAYMENT ---
        public List<AcctSettlementPaymentResult> Paging(AcctSettlementPaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = QueryDataPermission(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            //Phân trang
            var _totalItem = data.Select(s => s.Id).Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
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

        private IQueryable<AcctSettlementPayment> GetSettlementsPermission()
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            PermissionRange _permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (_permissionRange == PermissionRange.None) return null;

            IQueryable<AcctSettlementPayment> settlements = null;
            switch (_permissionRange)
            {
                case PermissionRange.None:
                    break;
                case PermissionRange.All:
                    settlements = DataContext.Get();
                    break;
                case PermissionRange.Owner:
                    settlements = DataContext.Get(x => x.UserCreated == _user.UserID);
                    break;
                case PermissionRange.Group:
                    settlements = DataContext.Get(x => x.GroupId == _user.GroupId
                                                    && x.DepartmentId == _user.DepartmentId
                                                    && x.OfficeId == _user.OfficeID
                                                    && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Department:
                    settlements = DataContext.Get(x => x.DepartmentId == _user.DepartmentId
                                                    && x.OfficeId == _user.OfficeID
                                                    && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Office:
                    settlements = DataContext.Get(x => x.OfficeId == _user.OfficeID
                                                    && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Company:
                    settlements = DataContext.Get(x => x.CompanyId == _user.CompanyID);
                    break;
            }
            return settlements;
        }

        private IQueryable<AcctSettlementPaymentResult> GetDatas(AcctSettlementPaymentCriteria criteria, IQueryable<AcctSettlementPayment> settlements)
        {
            if (settlements == null) return null;

            var approveSettle = acctApproveSettlementRepo.Get(x => x.IsDeputy == false);
            var user = sysUserRepo.Get();
            var surcharge = csShipmentSurchargeRepo.Get();
            var opst = opsTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = csTransactionDetailRepo.Get();
            var custom = customsDeclarationRepo.Get();
            var advRequest = acctAdvanceRequestRepo.Get();

            var isManagerDeputy = false;
            var isAccountantDeputy = false;
            if (!string.IsNullOrEmpty(criteria.Requester))
            {
                isManagerDeputy = userBaseService.CheckDeputyManagerByUser(currentUser.DepartmentId, criteria.Requester);
                isAccountantDeputy = userBaseService.CheckDeputyAccountantByUser(currentUser.DepartmentId, criteria.Requester);
            }

            List<string> refNo = new List<string>();
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                refNo = (from set in settlements
                         join sur in surcharge on set.SettlementNo equals sur.SettlementCode into sc
                         from sur in sc.DefaultIfEmpty()
                         join ops in opst on sur.Hblid equals ops.Hblid into op
                         from ops in op.DefaultIfEmpty()
                         join cstd in csTransDe on sur.Hblid equals cstd.Id into csd
                         from cstd in csd.DefaultIfEmpty()
                         join cst in csTrans on cstd.JobId equals cst.Id into cs
                         from cst in cs.DefaultIfEmpty()
                         join cus in custom on new { JobNo = (cst.JobNo != null ? cst.JobNo : ops.JobNo), HBL = (cstd.Hwbno != null ? cstd.Hwbno : ops.Hwbno), MBL = (cst.Mawb != null ? cst.Mawb : ops.Mblno) } equals new { JobNo = cus.JobNo, HBL = cus.Hblid, MBL = cus.Mblid } into cus1
                         from cus in cus1.DefaultIfEmpty()
                         join req in advRequest on new { JobNo = (cst.JobNo != null ? cst.JobNo : ops.JobNo), HBL = (cstd.Hwbno != null ? cstd.Hwbno : ops.Hwbno), MBL = (cst.Mawb != null ? cst.Mawb : ops.Mblno) } equals new { JobNo = req.JobId, HBL = req.Hbl, MBL = req.Mbl } into req1
                         from req in req1.DefaultIfEmpty()
                         where
                         (
                              criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ?
                              (
                                  (
                                         (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(set.SettlementNo) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(ops.Hwbno) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(ops.Mblno) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(ops.JobNo) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(cstd.Hwbno) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(cst.Mawb) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(cst.JobNo) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(cus.ClearanceNo) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(req.AdvanceNo) : true)
                                  )
                              )
                              :
                              (
                                  true
                              )
                         )
                         select set.SettlementNo).ToList();
            }

            var data = from set in settlements
                       join u in user on set.Requester equals u.Id into u2
                       from u in u2.DefaultIfEmpty()
                       join apr in approveSettle on set.SettlementNo equals apr.SettlementNo into apr1
                       from apr in apr1.DefaultIfEmpty()
                       where
                       (
                            criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ? refNo.Contains(set.SettlementNo) : true
                       )
                       &&
                       (
                            !string.IsNullOrEmpty(criteria.Requester) ?
                            (
                                    (set.Requester == criteria.Requester && currentUser.GroupId != 11)
                                || (currentUser.GroupId == 11
                                    && userBaseService.CheckIsAccountantDept(currentUser.DepartmentId) == false 
                                    && apr.Manager == criteria.Requester 
                                    && (set.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && set.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED))
                                || (currentUser.GroupId == 11
                                    && userBaseService.CheckIsAccountantDept(currentUser.DepartmentId) == true
                                    && apr.Accountant == criteria.Requester 
                                    && (set.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && set.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED && set.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL))
                                || (apr.ManagerApr == criteria.Requester 
                                    && (set.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && set.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED))
                                || (apr.AccountantApr == criteria.Requester 
                                    && (set.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && set.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED && set.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL))
                                || (isManagerDeputy && (set.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && set.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED))
                                || (isAccountantDeputy && (set.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && set.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED && set.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL))
                            )
                            :
                                true
                       )
                       &&
                       (
                            criteria.RequestDateFrom.HasValue && criteria.RequestDateTo.HasValue ?
                                //Convert RequestDate về date nếu RequestDate có value
                                set.RequestDate.Value.Date >= (criteria.RequestDateFrom.HasValue ? criteria.RequestDateFrom.Value.Date : criteria.RequestDateFrom)
                                && set.RequestDate.Value.Date <= (criteria.RequestDateTo.HasValue ? criteria.RequestDateTo.Value.Date : criteria.RequestDateTo)
                            :
                                true
                       )
                       &&
                       (
                            !string.IsNullOrEmpty(criteria.StatusApproval) && !criteria.StatusApproval.Equals("All") ?
                                set.StatusApproval == criteria.StatusApproval
                            :
                                true
                       )
                       &&
                       (
                           !string.IsNullOrEmpty(criteria.PaymentMethod) && !criteria.PaymentMethod.Equals("All") ?
                                set.PaymentMethod == criteria.PaymentMethod
                           :
                                true
                       )
                       &&
                       (
                           !string.IsNullOrEmpty(criteria.CurrencyID) && !criteria.CurrencyID.Equals("All") ?
                                set.SettlementCurrency == criteria.CurrencyID
                           :
                                true
                       )
                       select new AcctSettlementPaymentResult
                       {
                           Id = set.Id,
                           Amount = set.Amount ?? 0,
                           SettlementNo = set.SettlementNo,
                           SettlementCurrency = set.SettlementCurrency,
                           Requester = set.Requester,
                           RequesterName = u.Username,
                           RequestDate = set.RequestDate,
                           StatusApproval = set.StatusApproval,
                           PaymentMethod = set.PaymentMethod,
                           Note = set.Note,
                           DatetimeModified = set.DatetimeModified,
                           StatusApprovalName = Common.CustomData.StatusApproveAdvance.Where(x => x.Value == set.StatusApproval).Select(x => x.DisplayName).FirstOrDefault(),
                           PaymentMethodName = Common.CustomData.PaymentMethod.Where(x => x.Value == set.PaymentMethod).Select(x => x.DisplayName).FirstOrDefault(),
                       };

            //Sort Array sẽ nhanh hơn
            data = data.ToArray().OrderByDescending(orb => orb.DatetimeModified).AsQueryable();
            return data;
        }

        private IQueryable<AcctSettlementPaymentResult> QueryDataPermission(AcctSettlementPaymentCriteria criteria)
        {
            var settlements = GetSettlementsPermission();
            var data = GetDatas(criteria, settlements);
            return data;
        }

        public IQueryable<AcctSettlementPaymentResult> QueryData(AcctSettlementPaymentCriteria criteria)
        {
            var settlements = GetSettlementsPermission();
            var data = GetDatas(criteria, settlements);
            return data;
        }

        public List<ShipmentOfSettlementResult> GetShipmentOfSettlements(string settlementNo)
        {
            var settlement = DataContext.Get();
            var surcharge = csShipmentSurchargeRepo.Get();
            var opst = opsTransactionRepo.Get();
            var csTrans = csTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            //Quy đổi tỉ giá theo ngày Request Date
            var settle = settlement.Where(x => x.SettlementNo == settlementNo).FirstOrDefault();
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == settle.RequestDate.Value.Date).ToList();

            var dataOperation = from set in settlement
                                join sur in surcharge on set.SettlementNo equals sur.SettlementCode into sc
                                from sur in sc.DefaultIfEmpty()
                                join ops in opst on sur.Hblid equals ops.Hblid
                                where
                                     sur.SettlementCode == settlementNo
                                select new ShipmentOfSettlementResult
                                {
                                    JobId = ops.JobNo,
                                    HBL = ops.Hwbno,
                                    MBL = ops.Mblno,
                                    Amount = sur.Total,
                                    ChargeCurrency = sur.CurrencyId,
                                    SettlementCurrency = set.SettlementCurrency
                                };
            var dataDocument = from set in settlement
                               join sur in surcharge on set.SettlementNo equals sur.SettlementCode into sc
                               from sur in sc.DefaultIfEmpty()
                               join cstd in csTransDe on sur.Hblid equals cstd.Id //into csd
                               //from cstd in csd.DefaultIfEmpty()
                               join cst in csTrans on cstd.JobId equals cst.Id into cs
                               from cst in cs.DefaultIfEmpty()
                               where
                                    sur.SettlementCode == settlementNo
                               select new ShipmentOfSettlementResult
                               {
                                   JobId = cst.JobNo,
                                   HBL = cstd.Hwbno,
                                   MBL = cst.Mawb,
                                   Amount = sur.Total,
                                   ChargeCurrency = sur.CurrencyId,
                                   SettlementCurrency = set.SettlementCurrency
                               };
            var data = dataOperation.Union(dataDocument);
            var dataGrp = data.ToList().GroupBy(x => new
            {
                x.JobId,
                x.HBL,
                x.MBL,
                x.SettlementCurrency
            }
            ).Select(s => new ShipmentOfSettlementResult
            {
                JobId = s.Key.JobId,
                Amount = s.Sum(su => su.Amount * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, su.ChargeCurrency, su.SettlementCurrency)),
                HBL = s.Key.HBL,
                MBL = s.Key.MBL,
                SettlementCurrency = s.Key.SettlementCurrency
            }
            );
            return dataGrp.ToList();
        }
        #endregion --- LIST SETTLEMENT PAYMENT ---

        public HandleState DeleteSettlementPayment(string settlementNo)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var userCurrenct = currentUser.UserID;

                var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
                if (settlement == null) return new HandleState("Not found Settlement Payment");
                if (!settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_NEW)
                    && !settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED))
                {
                    return new HandleState("Not allow delete. Settlements are awaiting approval.");
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        //Phí chừng từ (chỉ cập nhật lại SettlementCode bằng null)
                        var surchargeShipment = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo && x.IsFromShipment == true).ToList();
                        if (surchargeShipment != null && surchargeShipment.Count > 0)
                        {
                            foreach (var item in surchargeShipment)
                            {
                                item.SettlementCode = null;
                                item.UserModified = userCurrenct;
                                item.DatetimeModified = DateTime.Now;
                                var hsUpdateSurcharge = csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id);
                            }
                        }
                        //Phí hiện trường (Xóa khỏi surcharge)
                        var surchargeScene = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo && x.IsFromShipment == false).ToList();
                        if (surchargeScene != null && surchargeScene.Count > 0)
                        {
                            foreach (var item in surchargeScene)
                            {
                                var hsRemoveSurchargeScene = csShipmentSurchargeRepo.Delete(x => x.Id == item.Id);
                            }
                        }

                        var hs = DataContext.Delete(x => x.Id == settlement.Id);
                        if (hs.Success)
                        {
                            trans.Commit();
                        }
                        else
                        {
                            trans.Rollback();
                        }
                        return hs;

                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
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
            settlementMap.NumberOfRequests = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo).Select(s => s.Id).Count();
            settlementMap.UserNameCreated = sysUserRepo.Get(x => x.Id == settlement.UserCreated).FirstOrDefault()?.Username;
            settlementMap.UserNameModified = sysUserRepo.Get(x => x.Id == settlement.UserModified).FirstOrDefault()?.Username;
            return settlementMap;
        }

        public List<ShipmentSettlement> GetListShipmentSettlementBySettlementNo(string settlementNo)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var settlement = DataContext.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();

            var settleCurrent = settlement.Where(x => x.SettlementNo == settlementNo).FirstOrDefault();
            if (settlement == null) return null;
            //Quy đổi tỉ giá theo ngày Request Date
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == settleCurrent.RequestDate.Value.Date).ToList();

            var dataOperation = from sur in surcharge
                                join opst in opsTrans on sur.Hblid equals opst.Hblid
                                join settle in settlement on sur.SettlementCode equals settle.SettlementNo into settle2
                                from settle in settle2.DefaultIfEmpty()
                                where sur.SettlementCode == settlementNo
                                select new ShipmentSettlement
                                {
                                    SettlementNo = sur.SettlementCode,
                                    JobId = opst.JobNo,
                                    HBL = opst.Hwbno,
                                    MBL = opst.Mblno,
                                    CurrencyShipment = settle.SettlementCurrency,
                                    TotalAmount = sur.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settle.SettlementCurrency)
                                };
            var dataDocument = from sur in surcharge
                               join cstd in csTransD on sur.Hblid equals cstd.Id
                               join cst in csTrans on cstd.JobId equals cst.Id into cst2
                               from cst in cst2.DefaultIfEmpty()
                               join settle in settlement on sur.SettlementCode equals settle.SettlementNo into settle2
                               from settle in settle2.DefaultIfEmpty()
                               where sur.SettlementCode == settlementNo
                               select new ShipmentSettlement
                               {
                                   SettlementNo = sur.SettlementCode,
                                   JobId = cst.JobNo,
                                   HBL = cstd.Hwbno,
                                   MBL = cst.Mawb,
                                   CurrencyShipment = settle.SettlementCurrency,
                                   TotalAmount = sur.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settle.SettlementCurrency)
                               };
            var dataQuery = dataOperation.Union(dataDocument);

            var dataGroup = dataQuery.ToList()
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
            foreach (var item in dataGroup)
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

            var dataOperation = from sur in surcharge
                                join cc in charge on sur.ChargeId equals cc.Id into cc2
                                from cc in cc2.DefaultIfEmpty()
                                join u in unit on sur.UnitId equals u.Id into u2
                                from u in u2.DefaultIfEmpty()
                                join par in payer on sur.PayerId equals par.Id into par2
                                from par in par2.DefaultIfEmpty()
                                join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                                from pae in pae2.DefaultIfEmpty()
                                join opst in opsTrans on sur.Hblid equals opst.Hblid //into opst2
                                //from opst in opst2.DefaultIfEmpty()
                                where
                                        sur.SettlementCode == settlementNo
                                     && opst.JobNo == JobId
                                     && opst.Hwbno == HBL
                                     && opst.Mblno == MBL
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
                                    Total = sur.Total,
                                    PayerId = sur.PayerId,
                                    Payer = (sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName),//par.ShortName,
                                    PaymentObjectId = sur.PaymentObjectId,
                                    OBHPartnerName = (sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),//pae.ShortName,
                                    InvoiceNo = sur.InvoiceNo,
                                    SeriesNo = sur.SeriesNo,
                                    InvoiceDate = sur.InvoiceDate,
                                    ClearanceNo = sur.ClearanceNo,
                                    ContNo = sur.ContNo,
                                    Notes = sur.Notes,
                                    IsFromShipment = sur.IsFromShipment,
                                    TypeOfFee = sur.TypeOfFee,
                                    AdvanceNo = sur.AdvanceNo
                                };
            var dataDocument = from sur in surcharge
                               join cc in charge on sur.ChargeId equals cc.Id into cc2
                               from cc in cc2.DefaultIfEmpty()
                               join u in unit on sur.UnitId equals u.Id into u2
                               from u in u2.DefaultIfEmpty()
                               join par in payer on sur.PayerId equals par.Id into par2
                               from par in par2.DefaultIfEmpty()
                               join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                               from pae in pae2.DefaultIfEmpty()
                               join cstd in csTransD on sur.Hblid equals cstd.Id //into cstd2
                               //from cstd in cstd2.DefaultIfEmpty()
                               join cst in csTrans on cstd.JobId equals cst.Id into cst2
                               from cst in cst2.DefaultIfEmpty()
                               where
                                       sur.SettlementCode == settlementNo
                                    && cst.JobNo == JobId
                                    && cstd.Hwbno == HBL
                                    && cst.Mawb == MBL
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
                                   Total = sur.Total,
                                   PayerId = sur.PayerId,
                                   Payer = (sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName),//par.ShortName,
                                   PaymentObjectId = sur.PaymentObjectId,
                                   OBHPartnerName = (sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),//pae.ShortName,
                                   InvoiceNo = sur.InvoiceNo,
                                   SeriesNo = sur.SeriesNo,
                                   InvoiceDate = sur.InvoiceDate,
                                   ClearanceNo = sur.ClearanceNo,
                                   ContNo = sur.ContNo,
                                   Notes = sur.Notes,
                                   IsFromShipment = sur.IsFromShipment,
                                   TypeOfFee = sur.TypeOfFee,
                                   AdvanceNo = sur.AdvanceNo
                               };
            var data = dataOperation.Union(dataDocument);
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

            var dataOperation = from sur in surcharge
                                join cc in charge on sur.ChargeId equals cc.Id into cc2
                                from cc in cc2.DefaultIfEmpty()
                                join u in unit on sur.UnitId equals u.Id into u2
                                from u in u2.DefaultIfEmpty()
                                join par in payer on sur.PayerId equals par.Id into par2
                                from par in par2.DefaultIfEmpty()
                                join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                                from pae in pae2.DefaultIfEmpty()
                                join opst in opsTrans on sur.Hblid equals opst.Hblid
                                where
                                     sur.SettlementCode == settlementNo
                                select new ShipmentChargeSettlement
                                {
                                    Id = sur.Id,
                                    JobId = opst.JobNo,
                                    MBL = opst.Mblno,
                                    HBL = opst.Hwbno,
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
                                    Total = sur.Total,
                                    PayerId = sur.PayerId,
                                    Payer = (sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName),//par.ShortName,
                                    PaymentObjectId = sur.PaymentObjectId,
                                    OBHPartnerName = (sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),//pae.ShortName,
                                    InvoiceNo = sur.InvoiceNo,
                                    SeriesNo = sur.SeriesNo,
                                    InvoiceDate = sur.InvoiceDate,
                                    ClearanceNo = sur.ClearanceNo,
                                    ContNo = sur.ContNo,
                                    Notes = sur.Notes,
                                    IsFromShipment = sur.IsFromShipment,
                                    TypeOfFee = sur.TypeOfFee,
                                    AdvanceNo = sur.AdvanceNo
                                };
            var dataDocument = from sur in surcharge
                               join cc in charge on sur.ChargeId equals cc.Id into cc2
                               from cc in cc2.DefaultIfEmpty()
                               join u in unit on sur.UnitId equals u.Id into u2
                               from u in u2.DefaultIfEmpty()
                               join par in payer on sur.PayerId equals par.Id into par2
                               from par in par2.DefaultIfEmpty()
                               join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                               from pae in pae2.DefaultIfEmpty()
                               join cstd in csTransD on sur.Hblid equals cstd.Id //into cstd2
                               //from cstd in cstd2.DefaultIfEmpty()
                               join cst in csTrans on cstd.JobId equals cst.Id into cst2
                               from cst in cst2.DefaultIfEmpty()
                               where
                                    sur.SettlementCode == settlementNo
                               select new ShipmentChargeSettlement
                               {
                                   Id = sur.Id,
                                   JobId = cst.JobNo,
                                   MBL = cst.Mawb,
                                   HBL = cstd.Hwbno,
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
                                   Total = sur.Total,
                                   PayerId = sur.PayerId,
                                   Payer = (sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName),//par.ShortName,
                                   PaymentObjectId = sur.PaymentObjectId,
                                   OBHPartnerName = (sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),//pae.ShortName,
                                   InvoiceNo = sur.InvoiceNo,
                                   SeriesNo = sur.SeriesNo,
                                   InvoiceDate = sur.InvoiceDate,
                                   ClearanceNo = sur.ClearanceNo,
                                   ContNo = sur.ContNo,
                                   Notes = sur.Notes,
                                   IsFromShipment = sur.IsFromShipment,
                                   TypeOfFee = sur.TypeOfFee,
                                   AdvanceNo = sur.AdvanceNo
                               };
            var data = dataOperation.Union(dataDocument);
            data = data.ToArray().OrderByDescending(x => x.JobId).AsQueryable();
            return data;
        }

        #endregion --- DETAILS SETTLEMENT PAYMENT ---

        #region --- PAYMENT MANAGEMENT ---
        public List<AdvancePaymentMngt> GetAdvancePaymentMngts(string jobId, string mbl, string hbl)
        {
            var advance = acctAdvancePaymentRepo.Get();
            var request = acctAdvanceRequestRepo.Get();
            //Chỉ lấy những advance có status là Done
            var data = from req in request
                       join ad in advance on req.AdvanceNo equals ad.AdvanceNo into ad2
                       from ad in ad2.DefaultIfEmpty()
                       where
                            ad.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE
                       && req.JobId == jobId
                       && req.Mbl == mbl
                       && req.Hbl == hbl
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
                    ChargeAdvancePaymentMngts = request.Where(x => x.AdvanceNo == item.AdvanceNo && x.JobId == jobId && x.Mbl == mbl && x.Hbl == hbl)
                    .Select(x => new ChargeAdvancePaymentMngt { AdvanceNo = x.AdvanceNo, TotalAmount = x.Amount.Value, AdvanceCurrency = x.RequestCurrency, Description = x.Description }).ToList()
                });
            }
            return dataResult;
        }

        public List<SettlementPaymentMngt> GetSettlementPaymentMngts(string jobId, string mbl, string hbl)
        {
            var settlement = DataContext.Get();
            var surcharge = csShipmentSurchargeRepo.Get();
            var charge = catChargeRepo.Get();
            var payee = catPartnerRepo.Get();
            var payer = catPartnerRepo.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();
            //Quy đổi tỉ giá theo ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();

            //Chỉ lấy ra những settlement có status là done     
            var dataOperation = from settle in settlement
                                join sur in surcharge on settle.SettlementNo equals sur.SettlementCode into sur2
                                from sur in sur2.DefaultIfEmpty()
                                join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                                from pae in pae2.DefaultIfEmpty()
                                join opst in opsTrans on sur.Hblid equals opst.Hblid
                                where
                                        settle.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE
                                     && opst.JobNo == jobId
                                     && opst.Hwbno == hbl
                                     && opst.Mblno == mbl
                                select new SettlementPaymentMngt
                                {
                                    SettlementNo = settle.SettlementNo,
                                    TotalAmount = sur.Total,
                                    SettlementCurrency = settle.SettlementCurrency,
                                    ChargeCurrency = sur.CurrencyId,
                                    SettlementDate = settle.DatetimeCreated
                                };
            var dataDocument = from settle in settlement
                               join sur in surcharge on settle.SettlementNo equals sur.SettlementCode into sur2
                               from sur in sur2.DefaultIfEmpty()
                               join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                               from pae in pae2.DefaultIfEmpty()
                               join cstd in csTransD on sur.Hblid equals cstd.Id
                               join cst in csTrans on cstd.JobId equals cst.Id into cst2
                               from cst in cst2.DefaultIfEmpty()
                               where
                                       settle.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE
                                    && cst.JobNo == jobId
                                    && cstd.Hwbno == hbl
                                    && cst.Mawb == mbl
                               select new SettlementPaymentMngt
                               {
                                   SettlementNo = settle.SettlementNo,
                                   TotalAmount = sur.Total,
                                   SettlementCurrency = settle.SettlementCurrency,
                                   ChargeCurrency = sur.CurrencyId,
                                   SettlementDate = settle.DatetimeCreated
                               };
            var data = dataOperation.Union(dataDocument);

            var dataGrp = data.ToList().GroupBy(x => new { x.SettlementNo, x.SettlementCurrency, x.SettlementDate })
                .Select(s => new SettlementPaymentMngt
                {
                    SettlementNo = s.Key.SettlementNo,
                    TotalAmount = s.Sum(su => su.TotalAmount * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, su.ChargeCurrency, su.SettlementCurrency)),
                    SettlementCurrency = s.Key.SettlementCurrency,
                    SettlementDate = s.Key.SettlementDate
                });

            var dataResult = new List<SettlementPaymentMngt>();
            foreach (var item in dataGrp)
            {
                dataResult.Add(new SettlementPaymentMngt
                {
                    SettlementNo = item.SettlementNo,
                    TotalAmount = item.TotalAmount,
                    SettlementCurrency = item.SettlementCurrency,
                    SettlementDate = item.SettlementDate,
                    ChargeSettlementPaymentMngts = GetListChargeSettlementPaymentMngt(item.SettlementNo, jobId, hbl, mbl).ToList()
                });
            }
            return dataResult;
        }
        public IQueryable<ChargeSettlementPaymentMngt> GetListChargeSettlementPaymentMngt(string settlementNo, string jobId, string hbl, string mbl)
        {
            var settlement = DataContext.Get();
            var surcharge = csShipmentSurchargeRepo.Get();
            var charge = catChargeRepo.Get();
            var payee = catPartnerRepo.Get();
            var payer = catPartnerRepo.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();

            var dataOperation = from sur in surcharge
                                join cc in charge on sur.ChargeId equals cc.Id into cc2
                                from cc in cc2.DefaultIfEmpty()
                                join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                                from pae in pae2.DefaultIfEmpty()
                                join par in payer on sur.PayerId equals par.Id into par2
                                from par in par2.DefaultIfEmpty()
                                join opst in opsTrans on sur.Hblid equals opst.Hblid //into opst2
                                //from opst in opst2.DefaultIfEmpty()
                                where
                                    sur.SettlementCode == settlementNo
                                 && opst.JobNo == jobId
                                 && opst.Hwbno == hbl
                                 && opst.Mblno == mbl
                                select new ChargeSettlementPaymentMngt
                                {
                                    SettlementNo = settlementNo,
                                    ChargeName = cc.ChargeNameEn,
                                    TotalAmount = sur.Total,
                                    SettlementCurrency = sur.CurrencyId,
                                    OBHPartner = (sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),
                                    Payer = (sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName)
                                };
            var dataDocument = from sur in surcharge
                               join cc in charge on sur.ChargeId equals cc.Id into cc2
                               from cc in cc2.DefaultIfEmpty()
                               join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                               from pae in pae2.DefaultIfEmpty()
                               join par in payer on sur.PayerId equals par.Id into par2
                               from par in par2.DefaultIfEmpty()
                               join cstd in csTransD on sur.Hblid equals cstd.Id //into cstd2
                               //from cstd in cstd2.DefaultIfEmpty()
                               join cst in csTrans on cstd.JobId equals cst.Id into cst2
                               from cst in cst2.DefaultIfEmpty()
                               where
                                   sur.SettlementCode == settlementNo
                                && cst.JobNo == jobId
                                && cstd.Hwbno == hbl
                                && cst.Mawb == mbl
                               select new ChargeSettlementPaymentMngt
                               {
                                   SettlementNo = settlementNo,
                                   ChargeName = cc.ChargeNameEn,
                                   TotalAmount = sur.Total,
                                   SettlementCurrency = sur.CurrencyId,
                                   OBHPartner = (sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),
                                   Payer = (sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName)
                               };
            var data = dataOperation.Union(dataDocument);
            return data;
        }
        #endregion --- PAYMENT MANAGEMENT ---

        #region -- GET EXISITS CHARGE --
        public List<ShipmentChargeSettlement> GetExistsCharge(ExistsChargeCriteria criteria)
        {
            //Chỉ lấy ra những phí chứng từ (thuộc phí credit + partner hay những phí thuộc đối tượng payer + partner)
            var surcharge = csShipmentSurchargeRepo
                .Get(x =>
                        x.IsFromShipment == true
                     //&& (x.Type == Constants.TYPE_CHARGE_BUY || (x.PayerId != null && x.CreditNo != null))
                     //&& (x.Type == AccountingConstants.TYPE_CHARGE_BUY || (x.PayerId == criteria.partnerId && x.CreditNo != null))
                     && ((x.Type == AccountingConstants.TYPE_CHARGE_BUY && x.PaymentObjectId == criteria.partnerId) 
                     || (x.Type == AccountingConstants.TYPE_CHARGE_OBH && x.PayerId == criteria.partnerId))
                );
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();
            var payer = catPartnerRepo.Get();
            var payee = catPartnerRepo.Get();
            var opsTrans = opsTransactionRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED);
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();

            if (criteria.jobIds != null)
            {
                opsTrans = opsTrans.Where(x => criteria.jobIds.Contains(x.JobNo));
                csTrans = csTrans.Where(x => criteria.jobIds.Contains(x.JobNo));
            }
            if (criteria.mbls != null)
            {
                opsTrans = opsTrans.Where(x => criteria.mbls.Contains(x.Mblno));
                csTrans = csTrans.Where(x => criteria.mbls.Contains(x.Mawb));
            }
            if (criteria.hbls != null)
            {
                opsTrans = opsTrans.Where(x => criteria.hbls.Contains(x.Hwbno));
                csTransD = csTransD.Where(x => criteria.hbls.Contains(x.Hwbno));
            }
            var dataOperation = from sur in surcharge
                                join cc in charge on sur.ChargeId equals cc.Id into cc2
                                from cc in cc2.DefaultIfEmpty()
                                join u in unit on sur.UnitId equals u.Id into u2
                                from u in u2.DefaultIfEmpty()
                                join par in payer on sur.PayerId equals par.Id into par2
                                from par in par2.DefaultIfEmpty()
                                join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                                from pae in pae2.DefaultIfEmpty()
                                join opst in opsTrans on sur.Hblid equals opst.Hblid
                                select new ShipmentChargeSettlement
                                {
                                    Id = sur.Id,
                                    JobId = opst.JobNo,
                                    MBL = opst.Mblno,
                                    HBL = opst.Hwbno,
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
                                    Total = sur.Total,
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

            var dataDocument = from sur in surcharge
                               join cc in charge on sur.ChargeId equals cc.Id into cc2
                               from cc in cc2.DefaultIfEmpty()
                               join u in unit on sur.UnitId equals u.Id into u2
                               from u in u2.DefaultIfEmpty()
                               join par in payer on sur.PayerId equals par.Id into par2
                               from par in par2.DefaultIfEmpty()
                               join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                               from pae in pae2.DefaultIfEmpty()
                               join cstd in csTransD on sur.Hblid equals cstd.Id
                               join cst in csTrans on cstd.JobId equals cst.Id
                               select new ShipmentChargeSettlement
                               {
                                   Id = sur.Id,
                                   JobId = cst.JobNo,
                                   MBL = cst.Mawb,
                                   HBL = cstd.Hwbno,
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
                                   Total = sur.Total,
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

            var data = dataDocument.Union(dataOperation);
            return data.ToList();
        }
        #endregion -- GET EXISITS CHARGE --

        #region -- INSERT & UPDATE SETTLEMENT PAYMENT --
        public ResultModel CheckDuplicateShipmentSettlement(CheckDuplicateShipmentSettlementCriteria criteria)
        {
            var result = new ResultModel();
            if (criteria.SurchargeID == Guid.Empty)
            {
                if (!string.IsNullOrEmpty(criteria.CustomNo) || !string.IsNullOrEmpty(criteria.InvoiceNo) || !string.IsNullOrEmpty(criteria.ContNo))
                {
                    var surChargeExists = csShipmentSurchargeRepo.Get(x =>
                               x.ChargeId == criteria.ChargeID
                            && x.Hblid == criteria.HBLID
                            && (criteria.TypeCharge == AccountingConstants.TYPE_CHARGE_BUY ? x.PaymentObjectId == criteria.Partner : (criteria.TypeCharge == AccountingConstants.TYPE_CHARGE_OBH ? x.PayerId == criteria.Partner : true))
                            && (string.IsNullOrEmpty(criteria.CustomNo) ? true : x.ClearanceNo == criteria.CustomNo)
                            && (string.IsNullOrEmpty(criteria.InvoiceNo) ? true : x.InvoiceNo == criteria.InvoiceNo)
                            && (string.IsNullOrEmpty(criteria.ContNo) ? true : x.ContNo == criteria.ContNo)
                    );

                    var isExists = surChargeExists.Select(s => s.Id).Any();
                    result.Status = isExists;
                    if (isExists)
                    {
                        var charge = catChargeRepo.Get();
                        var data = from sur in surChargeExists
                                   join chg in charge on sur.ChargeId equals chg.Id
                                   select new { JobNo = criteria.JobNo, HBLNo = criteria.HBLNo, MBLNo = criteria.MBLNo, ChargeName = chg.ChargeNameEn, SettlementCode = sur.SettlementCode };
                        string msg = string.Join("<br/>", data.ToList()
                            .Select(s => !string.IsNullOrEmpty(s.JobNo)
                            && !string.IsNullOrEmpty(s.HBLNo)
                            && !string.IsNullOrEmpty(s.MBLNo)
                            ? string.Format(@"Shipment: [{0}-{1}-{2}] Charge [{3}] has already existed in settlement: {4}", s.JobNo, s.HBLNo, s.MBLNo, s.ChargeName, s.SettlementCode)
                            : string.Format(@"Charge [{0}] has already existed in settlement: {1}.", s.ChargeName, s.SettlementCode)));
                        result.Message = msg;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(criteria.CustomNo) || !string.IsNullOrEmpty(criteria.InvoiceNo) || !string.IsNullOrEmpty(criteria.ContNo))
                {
                    var surChargeExists = csShipmentSurchargeRepo.Get(x =>
                               x.Id != criteria.SurchargeID
                            && x.ChargeId == criteria.ChargeID
                            && x.Hblid == criteria.HBLID
                            && (criteria.TypeCharge == AccountingConstants.TYPE_CHARGE_BUY ? x.PaymentObjectId == criteria.Partner : (criteria.TypeCharge == AccountingConstants.TYPE_CHARGE_OBH ? x.PayerId == criteria.Partner : true))
                            && (string.IsNullOrEmpty(criteria.CustomNo) ? true : x.ClearanceNo == criteria.CustomNo)
                            && (string.IsNullOrEmpty(criteria.InvoiceNo) ? true : x.InvoiceNo == criteria.InvoiceNo)
                            && (string.IsNullOrEmpty(criteria.ContNo) ? true : x.ContNo == criteria.ContNo)
                    );

                    var isExists = surChargeExists.Select(s => s.Id).Any();
                    result.Status = isExists;
                    if (isExists)
                    {
                        var charge = catChargeRepo.Get();
                        var data = from sur in surChargeExists
                                   join chg in charge on sur.ChargeId equals chg.Id
                                   select new { JobNo = criteria.JobNo, HBLNo = criteria.HBLNo, MBLNo = criteria.MBLNo, ChargeName = chg.ChargeNameEn, SettlementCode = sur.SettlementCode };
                        string msg = string.Join("<br/>", data.ToList()
                            .Select(s => !string.IsNullOrEmpty(s.JobNo)
                            && !string.IsNullOrEmpty(s.HBLNo)
                            && !string.IsNullOrEmpty(s.MBLNo)
                            ? string.Format(@"Shipment: [{0}-{1}-{2}] Charge [{3}] has already existed in settlement: {4}", s.JobNo, s.HBLNo, s.MBLNo, s.ChargeName, s.SettlementCode)
                            : string.Format(@"Charge [{0}] has already existed in settlement: {1}.", s.ChargeName, s.SettlementCode)));
                        result.Message = msg;
                    }
                }
            }
            return result;
        }

        public HandleState AddSettlementPayment(CreateUpdateSettlementModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var userCurrent = currentUser.UserID;
                var settlement = mapper.Map<AcctSettlementPayment>(model.Settlement);
                settlement.Id = model.Settlement.Id = Guid.NewGuid();
                settlement.SettlementNo = model.Settlement.SettlementNo = CreateSettlementNo();
                settlement.StatusApproval = model.Settlement.StatusApproval = string.IsNullOrEmpty(model.Settlement.StatusApproval) ? AccountingConstants.STATUS_APPROVAL_NEW : model.Settlement.StatusApproval;
                settlement.UserCreated = settlement.UserModified = userCurrent;
                settlement.DatetimeCreated = settlement.DatetimeModified = DateTime.Now;
                settlement.Amount = CaculatorAmountSettlement(model);
                settlement.GroupId = currentUser.GroupId;
                settlement.DepartmentId = currentUser.DepartmentId;
                settlement.OfficeId = currentUser.OfficeID;
                settlement.CompanyId = currentUser.CompanyID;

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Add(settlement);
                        if (hs.Success)
                        {
                            //Lấy các phí chứng từ IsFromShipment = true
                            var chargeShipment = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && x.IsFromShipment == true).Select(s => s.Id).ToList();
                            if (chargeShipment.Count > 0)
                            {
                                var listChargeShipment = csShipmentSurchargeRepo.Get(x => chargeShipment.Contains(x.Id)).ToList();
                                foreach (var item in listChargeShipment)
                                {
                                    item.SettlementCode = settlement.SettlementNo;
                                    item.UserModified = userCurrent;
                                    item.DatetimeModified = DateTime.Now;
                                    csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id);
                                }
                            }

                            //Lấy các phí hiện trường IsFromShipment = false & thực hiện insert các charge mới
                            var chargeScene = model.ShipmentCharge.Where(x => x.Id == Guid.Empty && x.IsFromShipment == false).ToList();
                            if (chargeScene.Count > 0)
                            {
                                var listChargeSceneAdd = mapper.Map<List<CsShipmentSurcharge>>(chargeScene);
                                foreach (ShipmentChargeSettlement itemScene in chargeScene)
                                {
                                    foreach (CsShipmentSurcharge itemSceneAdd in listChargeSceneAdd)
                                    {
                                        if (itemSceneAdd.Id == itemScene.Id)
                                        {
                                            itemSceneAdd.JobNo = itemScene.JobId;
                                            itemSceneAdd.Mblno = itemScene.MBL;
                                            itemSceneAdd.Hblno = itemScene.HBL;
                                        }
                                    }
                                }
                                foreach (var item in listChargeSceneAdd)
                                {
                                    item.Id = Guid.NewGuid();
                                    item.SettlementCode = settlement.SettlementNo;
                                    item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                                    item.UserCreated = item.UserModified = userCurrent;
                                    item.ExchangeDate = DateTime.Now;
                                    csShipmentSurchargeRepo.Add(item);
                                }
                            }
                        }
                        trans.Commit();
                        return hs;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        private decimal CaculatorAmountSettlement(CreateUpdateSettlementModel model)
        {
            decimal amount = 0;
            if (model.ShipmentCharge.Count > 0)
            {
                foreach (var charge in model.ShipmentCharge)
                {
                    var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == model.Settlement.RequestDate.Value.Date).ToList();
                    var rate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, charge.CurrencyId, model.Settlement.SettlementCurrency);
                    amount += charge.Total * rate;
                }
            }
            return amount;
        }

        public HandleState UpdateSettlementPayment(CreateUpdateSettlementModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var userCurrent = currentUser.UserID;

                var settlement = mapper.Map<AcctSettlementPayment>(model.Settlement);

                var settlementCurrent = DataContext.Get(x => x.Id == settlement.Id).FirstOrDefault();
                if (settlementCurrent == null) return new HandleState("Not found settlement payment");

                //Get Advance current from Database
                if (!settlementCurrent.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_NEW) && !settlementCurrent.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED))
                {
                    return new HandleState("Only allowed to edit the advance payment status is New or Deny");
                }

                settlement.DatetimeCreated = settlementCurrent.DatetimeCreated;
                settlement.UserCreated = settlementCurrent.UserCreated;

                settlement.DatetimeModified = DateTime.Now;
                settlement.UserModified = userCurrent;
                settlement.Amount = CaculatorAmountSettlement(model);
                settlement.GroupId = settlementCurrent.GroupId;
                settlement.DepartmentId = settlementCurrent.DepartmentId;
                settlement.OfficeId = settlementCurrent.OfficeId;
                settlement.CompanyId = settlementCurrent.CompanyId;

                //Cập nhật lại Status Approval là NEW nếu Status Approval hiện tại là DENIED
                if (model.Settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED) && settlementCurrent.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED))
                {
                    settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_NEW;
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Update(settlement, x => x.Id == settlement.Id);
                        if (hs.Success)
                        {
                            //Start --Phí chứng từ (IsFromShipment = true)--
                            //Cập nhật SettlementCode = null cho các SettlementNo
                            var chargeShipmentOld = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlement.SettlementNo && x.IsFromShipment == true).ToList();
                            if (chargeShipmentOld.Count > 0)
                            {
                                foreach (var item in chargeShipmentOld)
                                {
                                    item.SettlementCode = null;
                                    item.UserModified = userCurrent;
                                    item.DatetimeModified = DateTime.Now;
                                    csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id);
                                }
                            }
                            //Cập nhật SettlementCode = SettlementNo cho các SettlementNo
                            var chargeShipmentUpdate = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && x.IsFromShipment == true).Select(s => s.Id).ToList();
                            if (chargeShipmentUpdate.Count > 0)
                            {
                                var listChargeShipmentUpdate = csShipmentSurchargeRepo.Get(x => chargeShipmentUpdate.Contains(x.Id)).ToList();
                                foreach (var item in listChargeShipmentUpdate)
                                {
                                    item.SettlementCode = settlement.SettlementNo;
                                    item.UserModified = userCurrent;
                                    item.DatetimeModified = DateTime.Now;
                                    csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id);
                                }
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
                                foreach (ShipmentChargeSettlement itemScene in chargeSceneAdd)
                                {
                                    foreach (CsShipmentSurcharge itemSceneAdd in listChargeSceneAdd)
                                    {
                                        if (itemSceneAdd.Id == itemScene.Id)
                                        {
                                            itemSceneAdd.JobNo = itemScene.JobId;
                                            itemSceneAdd.Mblno = itemScene.MBL;
                                            itemSceneAdd.Hblno = itemScene.HBL;
                                        }
                                    }
                                }
                                foreach (var item in listChargeSceneAdd)
                                {
                                    item.Id = Guid.NewGuid();
                                    item.SettlementCode = settlement.SettlementNo;
                                    item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                                    item.UserCreated = item.UserModified = userCurrent;
                                    item.ExchangeDate = DateTime.Now;
                                    csShipmentSurchargeRepo.Add(item);
                                }
                            }

                            //Cập nhật lại các thông tin của phí hiện trường (nếu có edit chỉnh sửa phí hiện trường)
                            var chargeSceneUpdate = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && idsChargeScene.Contains(x.Id) && x.IsFromShipment == false);
                            var idChargeSceneUpdate = chargeSceneUpdate.Select(s => s.Id).ToList();
                            if (chargeSceneUpdate.Count() > 0)
                            {
                                var listChargeExists = csShipmentSurchargeRepo.Get(x => idChargeSceneUpdate.Contains(x.Id));
                                var listChargeSceneUpdate = mapper.Map<List<CsShipmentSurcharge>>(chargeSceneUpdate);
                                foreach (ShipmentChargeSettlement itemScene in chargeSceneUpdate)
                                {
                                    foreach (CsShipmentSurcharge itemSceneUpdate in listChargeSceneUpdate)
                                    {
                                        if (itemSceneUpdate.Id == itemScene.Id)
                                        {
                                            itemSceneUpdate.JobNo = itemScene.JobId;
                                            itemSceneUpdate.Mblno = itemScene.MBL;
                                            itemSceneUpdate.Hblno = itemScene.HBL;
                                        }
                                    }
                                }
                                foreach (var item in listChargeSceneUpdate)
                                {
                                    var sceneCharge = listChargeExists.Where(x => x.Id == item.Id).FirstOrDefault();
                                    item.UserCreated = sceneCharge?.UserCreated;
                                    item.DatetimeCreated = sceneCharge?.DatetimeCreated;
                                    item.UserModified = userCurrent;
                                    item.DatetimeModified = DateTime.Now;
                                    item.ExchangeDate = sceneCharge?.ExchangeDate;
                                    item.FinalExchangeRate = sceneCharge?.FinalExchangeRate;
                                    item.CreditNo = sceneCharge?.CreditNo;
                                    item.DebitNo = sceneCharge?.DebitNo;
                                    item.PaySoano = sceneCharge?.PaySoano;
                                    item.Soano = sceneCharge?.Soano;
                                    item.KickBack = sceneCharge?.KickBack;
                                    item.QuantityType = sceneCharge?.QuantityType;
                                    item.IncludedVat = sceneCharge?.IncludedVat;
                                    item.PaymentRefNo = sceneCharge?.PaymentRefNo;
                                    item.Status = sceneCharge?.Status;
                                    item.VoucherId = sceneCharge?.VoucherId;
                                    item.VoucherIddate = sceneCharge?.VoucherIddate;
                                    item.VoucherIdre = sceneCharge?.VoucherIdre;
                                    item.VoucherIdredate = sceneCharge?.VoucherIdredate;
                                    csShipmentSurchargeRepo.Update(item, x => x.Id == item.Id);
                                }
                            }

                            //Xóa các phí hiện trường đã chọn xóa của user
                            var chargeSceneRemove = chargeScene.Where(x => !model.ShipmentCharge.Select(s => s.Id).Contains(x.Id)).ToList();
                            if (chargeSceneRemove.Count > 0)
                            {
                                foreach (var item in chargeSceneRemove)
                                {
                                    csShipmentSurchargeRepo.Delete(x => x.Id == item.Id);
                                }
                            }
                            //End --Phí hiện trường (IsFromShipment = false)--
                        }
                        trans.Commit();
                        return hs;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
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
            //Quy đổi tỉ giá theo ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == DateTime.Now.Date).ToList();
            var advance = acctAdvancePaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);
            var request = acctAdvanceRequestRepo.Get(x => x.JobId == JobId && x.Mbl == MBL && x.Hbl == HBL);
            var query = from adv in advance
                        join req in request on adv.AdvanceNo equals req.AdvanceNo into req1
                        from req in req1.DefaultIfEmpty()
                        select req;
            var advanceAmount = query.Sum(x => x.Amount * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.RequestCurrency, Currency));
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
            var settlement = DataContext.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);
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
                       join request in advRequest on new { JobId = (opst.JobNo == null ? cst.JobNo : opst.JobNo), HBL = (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno), MBL = (opst.Mblno == null ? cst.Mawb : opst.Mblno) } equals new { JobId = request.JobId, HBL = request.Hbl, MBL = request.Mbl } into request1
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
                           AdvDate = (!string.IsNullOrEmpty(advance.StatusApproval) && advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE ? advance.DatetimeModified.Value.ToString("dd/MM/yyyy") : string.Empty),
                           SettlementNo = settlementNo,
                           Customer = cus.PartnerNameVn != null ? cus.PartnerNameVn.ToUpper() : string.Empty,
                           Consignee = cnee.PartnerNameVn != null ? cnee.PartnerNameVn.ToUpper() : string.Empty,
                           Consigner = cner.PartnerNameVn != null ? cner.PartnerNameVn.ToUpper() : string.Empty,
                           ContainerQty = opst.SumContainers.HasValue ? opst.SumContainers.Value.ToString() + "/" : string.Empty,
                           GW = opst.SumGrossWeight.HasValue ? opst.SumGrossWeight.Value : 0,
                           NW = opst.SumNetWeight.HasValue ? opst.SumNetWeight.Value : 0,
                           CustomsId = (sur.ClearanceNo == null ? string.Empty : sur.ClearanceNo),
                           PSC = opst.SumPackages.HasValue ? opst.SumPackages.Value : 0,
                           CBM = opst.SumCbm.HasValue ? opst.SumCbm.Value : 0,
                           HBL = (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno),
                           MBL = (opst.Mblno == null ? cst.Mawb : opst.Mblno),
                           StlCSName = string.Empty
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

            var settle = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();

            //Quy đổi tỉ giá theo ngày Request Date
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == settle.RequestDate.Value.Date).ToList();

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
                           AdvContact = string.Empty,
                           AdvAddress = string.Empty,
                           StlDescription = string.Empty,
                           AdvanceNo = string.Empty,
                           AdvValue = 0,
                           AdvCurrency = string.Empty,
                           Remains = 0,
                           AdvanceDate = DateTime.Now,
                           No = 0,
                           CustomsID = string.Empty,
                           JobID = (opst.JobNo == null ? cst.JobNo : opst.JobNo),
                           HBL = (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno),
                           Description = cat.ChargeNameEn,
                           InvoiceNo = sur.InvoiceNo == null ? string.Empty : sur.InvoiceNo,
                           Amount = sur.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, currency),
                           Debt = false,
                           Currency = string.Empty,
                           Note = sur.Notes,
                           AdvDpManagerID = string.Empty,
                           AdvDpManagerStickDeny = true,
                           AdvDpManagerStickApp = true,
                           AdvDpManagerName = string.Empty,
                           AdvDpSignDate = DateTime.Now,
                           AdvAcsDpManagerID = string.Empty,
                           AdvAcsDpManagerStickDeny = true,
                           AdvAcsDpManagerStickApp = true,
                           AdvAcsDpManagerName = string.Empty,
                           AdvAcsSignDate = DateTime.Now,
                           AdvBODID = string.Empty,
                           AdvBODStickDeny = true,
                           AdvBODStickApp = true,
                           AdvBODName = string.Empty,
                           AdvBODSignDate = DateTime.Now,
                           SltAcsCashierName = string.Empty,
                           SltCashierDate = DateTime.Now,
                           Saved = true,
                           ClearStatus = true,
                           Status = string.Empty,
                           AcsApproval = true,
                           SltDpComment = string.Empty,
                           Shipper = string.Empty,
                           ShipmentInfo = string.Empty,
                           MBLNO = (opst.Mblno == null ? cst.Mawb : opst.Mblno),
                           VAT = 0,
                           BFVATAmount = 0,
                           ContainerQty = string.Empty,
                           Noofpieces = 0,
                           UnitPieaces = string.Empty,
                           GrossWeight = 0,
                           NW = 0,
                           CBM = 0,
                           ShipperHBL = string.Empty,
                           ConsigneeHBL = string.Empty,
                           ModeSettle = string.Empty,
                           STT = 0,
                           Series = string.Empty,
                           InvoiceDate = DateTime.Now,
                           Inword = string.Empty,
                           InvoiceID = string.Empty,
                           Commodity = string.Empty,
                           ServiceType = string.Empty,
                           SltCSName = string.Empty,
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
            if (listSettlementPayment.Count == 0)
            {
                return null;
            }

            var parameter = new AscSettlementPaymentRequestReportParams();
            parameter = GetFirstShipmentOfSettlement(settlementNo);
            if (parameter != null)
            {
                parameter.SettleRequester = (settlement != null && !string.IsNullOrEmpty(settlement.Requester)) ? userBaseService.GetEmployeeByUserId(settlement.Requester)?.EmployeeNameVn : string.Empty;

                parameter.SettleRequestDate = settlement.RequestDate != null ? settlement.RequestDate.Value.ToString("dd/MM/yyyy") : string.Empty;

                //Lấy thông tin các User Approve Settlement
                var infoSettleAprove = GetInfoApproveSettlementNoCheckBySettlementNo(settlementNo);
                parameter.StlDpManagerName = infoSettleAprove != null ? infoSettleAprove.ManagerName : string.Empty;
                parameter.StlDpManagerSignDate = infoSettleAprove != null && infoSettleAprove.ManagerAprDate.HasValue ? infoSettleAprove.ManagerAprDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                parameter.StlAscDpManagerName = infoSettleAprove != null ? infoSettleAprove.AccountantName : string.Empty;
                parameter.StlAscDpManagerSignDate = infoSettleAprove != null && infoSettleAprove.AccountantAprDate.HasValue ? infoSettleAprove.AccountantAprDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                parameter.StlBODSignDate = infoSettleAprove != null && infoSettleAprove.BuheadAprDate.HasValue ? infoSettleAprove.BuheadAprDate.Value.ToString("dd/MM/yyyy") : string.Empty;

                parameter.CompanyName = AccountingConstants.COMPANY_NAME;
                parameter.CompanyAddress1 = AccountingConstants.COMPANY_ADDRESS1;
                parameter.CompanyAddress2 = AccountingConstants.COMPANY_CONTACT;
                parameter.Website = AccountingConstants.COMPANY_WEBSITE;
                parameter.Contact = currentUser.UserName;//Get user login

                //Lấy ra tổng Advance Amount của các charge thuộc Settlement
                decimal advanceAmount = 0;
                var shipments = listSettlementPayment.GroupBy(x => new { x.JobID, x.MBLNO, x.HBL }).Select(x => new { JobID = x.Key.JobID, MBLNO = x.Key.MBLNO, HBL = x.Key.HBL });
                foreach (var item in shipments)
                {
                    advanceAmount += GetAdvanceAmountByShipmentAndCurrency(item.JobID, item.MBLNO, item.HBL, settlement.SettlementCurrency);
                }
                parameter.AdvValue = advanceAmount > 0 ? String.Format("{0:n}", advanceAmount) : string.Empty;
                parameter.AdvCurrency = advanceAmount > 0 ? settlement.SettlementCurrency : string.Empty;

                //Chuyển tiền Amount thành chữ
                decimal _amount = advanceAmount > 0 ? advanceAmount : 0;
                var _inword = string.Empty;
                if (_amount > 0)
                {
                    var _currency = settlement.SettlementCurrency == AccountingConstants.CURRENCY_LOCAL ?
                               (_amount % 1 > 0 ? "đồng lẻ" : "đồng chẵn")
                            :
                            settlement.SettlementCurrency;

                    _inword = InWordCurrency.ConvertNumberCurrencyToString(_amount, _currency);
                }
                parameter.Inword = _inword;
            }

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

        public HandleState CheckExistsInfoManagerOfRequester(AcctApproveSettlementModel settlement)
        {
            var userCurrent = currentUser.UserID;
            if (settlement.Requester != userCurrent) return new HandleState("Error");

            var acctApprove = mapper.Map<AcctApproveSettlement>(settlement);
            //Lấy ra brandId của user 
            var brandOfUser = currentUser.OfficeID;
            if (brandOfUser == Guid.Empty || brandOfUser == null) return new HandleState("Not found office of user");

            //Lấy ra các user Leader, Manager Dept của user requester, user Accountant, BUHead(nếu có) của user requester
            acctApprove.Leader = userBaseService.GetLeaderIdOfUser(userCurrent);
            acctApprove.Manager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault();
            if (string.IsNullOrEmpty(acctApprove.Manager)) return new HandleState("Not found department manager of user");
            acctApprove.Accountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
            if (string.IsNullOrEmpty(acctApprove.Accountant)) return new HandleState("Not found accountant manager");
            acctApprove.Buhead = userBaseService.GetBUHeadId(brandOfUser.ToString());

            var emailLeaderOrManager = string.Empty;
            var userLeaderOrManager = string.Empty;
            //Lấy ra Leader của User & Manager Dept của User Requester
            if (string.IsNullOrEmpty(acctApprove.Leader))
            {
                userLeaderOrManager = acctApprove.Manager;
                //Lấy ra employeeId của managerIdOfUser
                var employeeIdOfUserManager = userBaseService.GetEmployeeIdOfUser(userLeaderOrManager);
                //Lấy ra email của Manager
                emailLeaderOrManager = userBaseService.GetEmployeeByEmployeeId(employeeIdOfUserManager)?.Email;
            }
            else
            {
                userLeaderOrManager = acctApprove.Leader;
                //Lấy ra employeeId của managerIdOfUser
                var employeeIdOfUserLeader = userBaseService.GetEmployeeIdOfUser(userLeaderOrManager);
                //Lấy ra email của Leader (hiện tại chưa có nên gán rỗng)
                emailLeaderOrManager = userBaseService.GetEmployeeByEmployeeId(employeeIdOfUserLeader)?.Email;
            }

            if (string.IsNullOrEmpty(emailLeaderOrManager)) return new HandleState("Not found email Leader or Manager");
            return new HandleState();
        }

        public HandleState InsertOrUpdateApprovalSettlement(AcctApproveSettlementModel settlement)
        {
            try
            {
                var userCurrent = currentUser.UserID;

                var acctApprove = mapper.Map<AcctApproveSettlement>(settlement);

                if (!string.IsNullOrEmpty(settlement.SettlementNo))
                {
                    var settle = DataContext.Get(x => x.SettlementNo == settlement.SettlementNo).FirstOrDefault();
                    if (settle.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                        && settle.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                        && settle.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE
                        && settle.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)
                    {
                        return new HandleState("Awaiting approval");
                    }
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        //Lấy ra brandId của user 
                        var brandOfUser = currentUser.OfficeID;
                        if (brandOfUser == Guid.Empty || brandOfUser == null) return new HandleState("Not found office of user");

                        //Lấy ra các user Leader, Manager Dept của user requester, user Accountant, BUHead(nếu có) của user requester
                        acctApprove.Leader = userBaseService.GetLeaderIdOfUser(userCurrent);
                        acctApprove.Manager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault();
                        if (string.IsNullOrEmpty(acctApprove.Manager)) return new HandleState("Not found department manager of user");
                        acctApprove.Accountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault();
                        if (string.IsNullOrEmpty(acctApprove.Accountant)) return new HandleState("Not found accountant manager");
                        acctApprove.Buhead = userBaseService.GetBUHeadId(brandOfUser.ToString());

                        var emailLeaderOrManager = string.Empty;
                        var userLeaderOrManager = string.Empty;
                        List<string> usersDeputy = new List<string>();

                        //Send mail đề nghị approve đến Leader(Nếu có) nếu không có thì send tới Manager Dept
                        //Lấy ra Leader của User & Manager Dept của User Requester
                        if (string.IsNullOrEmpty(acctApprove.Leader))
                        {
                            userLeaderOrManager = acctApprove.Manager;
                            //Lấy ra employeeId của managerIdOfUser
                            var employeeIdOfUserManager = userBaseService.GetEmployeeIdOfUser(userLeaderOrManager);
                            //Lấy ra email của Manager
                            emailLeaderOrManager = userBaseService.GetEmployeeByEmployeeId(employeeIdOfUserManager)?.Email;

                            var deptCodeRequester = userBaseService.GetInfoDeptOfUser(currentUser.DepartmentId)?.Code;
                            usersDeputy = userBaseService.GetListUserDeputyByDept(deptCodeRequester);
                        }
                        else
                        {
                            userLeaderOrManager = acctApprove.Leader;
                            //Lấy ra employeeId của managerIdOfUser
                            var employeeIdOfUserLeader = userBaseService.GetEmployeeIdOfUser(userLeaderOrManager);
                            //Lấy ra email của Leader (hiện tại chưa có nên gán rỗng)
                            emailLeaderOrManager = userBaseService.GetEmployeeByEmployeeId(employeeIdOfUserLeader)?.Email;
                        }

                        if (string.IsNullOrEmpty(emailLeaderOrManager)) return new HandleState("Not found email Leader or Manager");

                        var sendMailResult = SendMailSuggestApproval(acctApprove.SettlementNo, userLeaderOrManager, emailLeaderOrManager, usersDeputy);

                        if (sendMailResult)
                        {
                            var checkExistsApproveBySettlementNo = acctApproveSettlementRepo.Get(x => x.SettlementNo == acctApprove.SettlementNo && x.IsDeputy == false).FirstOrDefault();
                            if (checkExistsApproveBySettlementNo == null) //Insert AcctApproveSettlement
                            {
                                acctApprove.Id = Guid.NewGuid();
                                acctApprove.RequesterAprDate = DateTime.Now;
                                acctApprove.UserCreated = acctApprove.UserModified = userCurrent;
                                acctApprove.DateCreated = acctApprove.DateModified = DateTime.Now;
                                acctApprove.IsDeputy = false;
                                var hsAddApproveSettlement = acctApproveSettlementRepo.Add(acctApprove);
                            }
                            else //Update AcctApproveSettlement by SettlementNo
                            {
                                checkExistsApproveBySettlementNo.RequesterAprDate = DateTime.Now;
                                checkExistsApproveBySettlementNo.UserModified = userCurrent;
                                checkExistsApproveBySettlementNo.DateModified = DateTime.Now;
                                var hsUpdateApproveSettlement = acctApproveSettlementRepo.Update(checkExistsApproveBySettlementNo, x => x.Id == checkExistsApproveBySettlementNo.Id);
                            }
                            trans.Commit();
                        }

                        return !sendMailResult ? new HandleState("Send mail suggest approval failed") : new HandleState();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message.ToString());
            }
        }

        public HandleState UpdateApproval(Guid settlementId)
        {
            var userCurrent = currentUser.UserID;

            var userAprNext = string.Empty;
            var emailUserAprNext = string.Empty;
            List<string> usersDeputy = new List<string>();

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var settlement = DataContext.Get(x => x.Id == settlementId).FirstOrDefault();

                    if (settlement == null) return new HandleState("Not found settlement payment");

                    var approve = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo && x.IsDeputy == false).FirstOrDefault();

                    if (approve == null)
                    {
                        if (acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo).Select(s => s.Id).Any() == false)
                        {
                            return new HandleState("Not found Settlement Approval by SettlementNo is " + settlement.SettlementNo);
                        }
                        else
                        {
                            return new HandleState("Not allow approve");
                        }
                    }

                    //Lấy ra brandId của user requester
                    var brandOfUserRequest = settlement.OfficeId;
                    if (brandOfUserRequest == Guid.Empty || brandOfUserRequest == null) return new HandleState("Not found office of user requester");

                    //Lấy ra brandId của userId
                    var brandOfUserId = currentUser.OfficeID;
                    if (brandOfUserId == Guid.Empty) return new HandleState("Not found office of user");

                    //Lấy ra dept code của userApprove dựa vào userApprove
                    var deptCodeOfUser = userBaseService.GetInfoDeptOfUser(currentUser.DepartmentId)?.Code;
                    if (string.IsNullOrEmpty(deptCodeOfUser)) return new HandleState("Not found department of user");

                    //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
                    var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(settlement.SettlementNo, currentUser, deptCodeOfUser);
                    if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
                    if (approve != null && settlement != null)
                    {
                        if (userCurrent == userBaseService.GetLeaderIdOfUser(settlement.Requester))
                        {
                            if (settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED
                                || settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED
                                || settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                                || settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                            {
                                return new HandleState("Leader approved");
                            }
                            settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED;
                            approve.LeaderAprDate = DateTime.Now;//Cập nhật ngày Approve của Leader

                            //Lấy email của Department Manager
                            userAprNext = userBaseService.GetDeptManager(settlement.CompanyId, settlement.OfficeId, settlement.DepartmentId).FirstOrDefault();
                            var userAprNextId = userBaseService.GetEmployeeIdOfUser(userAprNext);
                            emailUserAprNext = userBaseService.GetEmployeeByEmployeeId(userAprNextId)?.Email;

                            var deptCodeRequester = userBaseService.GetInfoDeptOfUser(settlement.DepartmentId)?.Code;
                            usersDeputy = userBaseService.GetListUserDeputyByDept(deptCodeRequester);
                        }
                        else if (currentUser.GroupId == AccountingConstants.SpecialGroup 
                            && userBaseService.CheckIsAccountantDept(currentUser.DepartmentId) == false 
                            && (userCurrent == userBaseService.GetDeptManager(settlement.CompanyId, settlement.OfficeId, settlement.DepartmentId).FirstOrDefault()
                                || userBaseService.CheckDeputyManagerByUser(currentUser.DepartmentId, currentUser.UserID)))
                        {
                            if (settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED
                                || settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                                || settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                            {
                                return new HandleState("Manager department approved");
                            }
                            settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED;
                            approve.ManagerAprDate = DateTime.Now;//Cập nhật ngày Approve của Manager
                            approve.ManagerApr = userCurrent;

                            //Lấy email của Accountant Manager
                            userAprNext = userBaseService.GetAccoutantManager(settlement.CompanyId, settlement.OfficeId).FirstOrDefault();
                            var userAprNextId = userBaseService.GetEmployeeIdOfUser(userAprNext);
                            emailUserAprNext = userBaseService.GetEmployeeByEmployeeId(userAprNextId)?.Email;

                            //var deptCodeAccountant = GetInfoDeptOfUser(AccountingConstants.AccountantDeptId)?.Code;
                            //usersDeputy = GetListUserDeputyByDept(deptCodeAccountant);
                        }
                        else if (currentUser.GroupId == AccountingConstants.SpecialGroup
                                && userBaseService.CheckIsAccountantDept(currentUser.DepartmentId)
                                && (userCurrent == userBaseService.GetAccoutantManager(settlement.CompanyId, settlement.OfficeId).FirstOrDefault()
                                    || userBaseService.CheckDeputyAccountantByUser(currentUser.DepartmentId, currentUser.UserID)))
                        {
                            if (settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                            {
                                return new HandleState("Chief accountant approved");
                            }
                            settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            approve.AccountantAprDate = approve.BuheadAprDate = DateTime.Now;//Cập nhật ngày Approve của Accountant & BUHead
                            approve.AccountantApr = userCurrent;
                            approve.BuheadApr = approve.Buhead;

                            //Cập nhật Status Payment cho Advance Request dựa vào HBL
                            acctAdvancePaymentService.UpdateStatusPaymentOfAdvanceRequest(settlement.SettlementNo);

                            //Send mail approval success when Accountant approved, mail send to requester
                            SendMailApproved(settlement.SettlementNo, DateTime.Now);
                        }

                        settlement.UserModified = approve.UserModified = userCurrent;
                        settlement.DatetimeModified = approve.DateModified = DateTime.Now;

                        var hsUpdateSettle = DataContext.Update(settlement, x => x.Id == settlement.Id);
                        var hsUpdateApprove = acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id);
                        trans.Commit();
                    }

                    //Nếu currentUser là Accoutant of Requester thì return
                    if (userBaseService.CheckIsAccountantDept(currentUser.DepartmentId) 
                        && userCurrent == userBaseService.GetAccoutantManager(settlement.CompanyId, settlement.OfficeId).FirstOrDefault())
                    {
                        return new HandleState();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(emailUserAprNext)) return new HandleState("Not found email of user " + userAprNext);

                        //Send mail đề nghị approve
                        var sendMailResult = SendMailSuggestApproval(settlement.SettlementNo, userAprNext, emailUserAprNext, usersDeputy);

                        return sendMailResult ? new HandleState() : new HandleState("Send mail suggest approval failed");
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public HandleState DeniedApprove(Guid settlementId, string comment)
        {
            var userCurrent = currentUser.UserID;

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var settlement = DataContext.Get(x => x.Id == settlementId).FirstOrDefault();

                    if (settlement == null) return new HandleState("Not found settlement payment");

                    var approve = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo && x.IsDeputy == false).FirstOrDefault();
                    if (approve == null)
                    {
                        if (acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo).Select(s => s.Id).Any() == false)
                        {
                            return new HandleState("Not found approve settlement by SettlementNo " + settlement.SettlementNo);
                        }
                        else
                        {
                            return new HandleState("Not allow deny");
                        }
                    }

                    //Lấy ra brandId của user requester
                    var brandOfUserRequest = settlement.OfficeId;
                    if (brandOfUserRequest == Guid.Empty || brandOfUserRequest == null) return new HandleState("Not found office of user requester");

                    //Lấy ra brandId của userId
                    var brandOfUserId = currentUser.OfficeID;
                    if (brandOfUserId == Guid.Empty || brandOfUserId == null) return new HandleState("Not found office of user");

                    //Lấy ra dept code của userApprove dựa vào userApprove
                    var deptCodeOfUser = userBaseService.GetInfoDeptOfUser(currentUser.DepartmentId)?.Code;
                    if (string.IsNullOrEmpty(deptCodeOfUser)) return new HandleState("Not found department of user");

                    //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
                    //var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(settlement.SettlementNo, userCurrent, deptCodeOfUser);
                    //if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
                    if (approve != null && settlement != null)
                    {
                        if (settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_DENIED)
                        {
                            return new HandleState("Settlement payment denied");
                        }

                        if (userCurrent == userBaseService.GetLeaderIdOfUser(settlement.Requester))
                        {
                            var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(settlement.SettlementNo, currentUser, deptCodeOfUser);
                            if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
                            approve.LeaderAprDate = DateTime.Now;//Cập nhật ngày Denie của Leader
                        }
                        else if (currentUser.GroupId == AccountingConstants.SpecialGroup
                            && userBaseService.CheckIsAccountantDept(currentUser.DepartmentId) == false
                            && (userCurrent == userBaseService.GetDeptManager(settlement.CompanyId, settlement.OfficeId, settlement.DepartmentId).FirstOrDefault()
                                || userBaseService.CheckDeputyManagerByUser(currentUser.DepartmentId, currentUser.UserID)))
                        {
                            //Cho phép User Manager thực hiện deny khi user Manager đã Approved, 
                            //nếu Chief Accountant đã Approved thì User Manager ko được phép deny
                            if (settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                                || settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                            {
                                return new HandleState("Not allow deny. Advance payment has been approved");
                            }

                            approve.ManagerAprDate = DateTime.Now;//Cập nhật ngày Denie của Manager
                            approve.ManagerApr = userCurrent; //Cập nhật user manager denie                   
                        }
                        else if (currentUser.GroupId == AccountingConstants.SpecialGroup
                            && userBaseService.CheckIsAccountantDept(currentUser.DepartmentId)
                            && (userCurrent == userBaseService.GetAccoutantManager(settlement.CompanyId, settlement.OfficeId).FirstOrDefault()
                                || userBaseService.CheckDeputyAccountantByUser(currentUser.DepartmentId, currentUser.UserID)))
                        {
                            var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(settlement.SettlementNo, currentUser, deptCodeOfUser);
                            if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
                            approve.AccountantAprDate = DateTime.Now;//Cập nhật ngày Denie của Accountant
                            approve.AccountantApr = userCurrent; //Cập nhật user accountant denie
                        }
                        else
                        {
                            var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(settlement.SettlementNo, currentUser, deptCodeOfUser);
                            if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
                        }

                        approve.UserModified = userCurrent;
                        approve.DateModified = DateTime.Now;
                        approve.Comment = comment;
                        approve.IsDeputy = true;
                        var hsUpdateApprove = acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id);

                        //Cập nhật lại advance status của Settlement Payment
                        settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_DENIED;
                        settlement.UserModified = userCurrent;
                        settlement.DatetimeModified = DateTime.Now;
                        var hsUpdateSettle = DataContext.Update(settlement, x => x.Id == settlement.Id);

                        trans.Commit();
                    }

                    //Send mail denied approval
                    var sendMailResult = SendMailDeniedApproval(settlement.SettlementNo, comment, DateTime.Now);
                    return sendMailResult ? new HandleState() : new HandleState("Send mail deny approval failed");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
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
                aprSettlementMap.IsApproved = CheckUserInApproveSettlementAndDeptApproved(currentUser, aprSettlementMap);
                aprSettlementMap.RequesterName = string.IsNullOrEmpty(aprSettlementMap.Requester) ? null : userBaseService.GetEmployeeByUserId(aprSettlementMap.Requester)?.EmployeeNameVn;
                aprSettlementMap.LeaderName = string.IsNullOrEmpty(aprSettlementMap.Leader) ? null : userBaseService.GetEmployeeByUserId(aprSettlementMap.Leader)?.EmployeeNameVn;
                aprSettlementMap.ManagerName = string.IsNullOrEmpty(aprSettlementMap.Manager) ? null : userBaseService.GetEmployeeByUserId(aprSettlementMap.Manager)?.EmployeeNameVn;
                aprSettlementMap.AccountantName = string.IsNullOrEmpty(aprSettlementMap.Accountant) ? null : userBaseService.GetEmployeeByUserId(aprSettlementMap.Accountant)?.EmployeeNameVn;
                aprSettlementMap.BUHeadName = string.IsNullOrEmpty(aprSettlementMap.Buhead) ? null : userBaseService.GetEmployeeByUserId(aprSettlementMap.Buhead)?.EmployeeNameVn;
                aprSettlementMap.StatusApproval = DataContext.Get(x => x.SettlementNo == aprSettlementMap.SettlementNo).FirstOrDefault()?.StatusApproval;

                var isManagerDeputy = userBaseService.CheckDeputyManagerByUser(currentUser.DepartmentId, currentUser.UserID);
                aprSettlementMap.IsManager = currentUser.GroupId == AccountingConstants.SpecialGroup && (userCurrent == approveSettlement.Manager || userCurrent == approveSettlement.ManagerApr || isManagerDeputy) ? true : false;
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
                //Lấy ra advance cũ nhất chưa có settlement của shipment(JobId)
                var advance = acctAdvancePaymentService.GetAdvancesOfShipment().Where(x => x.JobId == shipment.JobId).FirstOrDefault()?.AdvanceNo;
                foreach (var charge in criteria.charges)
                {
                    var chargeCopy = new ShipmentChargeSettlement();
                    chargeCopy.Id = Guid.Empty;
                    chargeCopy.JobId = shipment.JobId;
                    chargeCopy.HBL = shipment.HBL;
                    chargeCopy.MBL = shipment.MBL;
                    chargeCopy.ChargeCode = charge.ChargeCode;
                    chargeCopy.Hblid = shipment.HBLID;//Lấy HBLID của shipment gán cho surcharge
                    chargeCopy.Type = charge.Type;
                    chargeCopy.ChargeId = charge.ChargeId;
                    chargeCopy.ChargeName = charge.ChargeName;
                    chargeCopy.Quantity = charge.Quantity;
                    chargeCopy.UnitId = charge.UnitId;
                    chargeCopy.UnitName = charge.UnitName;
                    chargeCopy.UnitPrice = charge.UnitPrice;
                    chargeCopy.CurrencyId = charge.CurrencyId;
                    chargeCopy.Vatrate = charge.Vatrate;
                    chargeCopy.Total = charge.Total;
                    chargeCopy.PayerId = charge.PayerId;
                    chargeCopy.Payer = charge.Payer;
                    chargeCopy.ObjectBePaid = charge.ObjectBePaid;
                    chargeCopy.PaymentObjectId = charge.PaymentObjectId;
                    chargeCopy.OBHPartnerName = charge.OBHPartnerName;
                    chargeCopy.Notes = charge.Notes;
                    chargeCopy.SettlementCode = null;
                    chargeCopy.InvoiceNo = charge.InvoiceNo;
                    chargeCopy.InvoiceDate = charge.InvoiceDate;
                    chargeCopy.SeriesNo = charge.SeriesNo;
                    chargeCopy.PaymentRequestType = charge.PaymentRequestType;
                    chargeCopy.ClearanceNo = charge.ClearanceNo;
                    chargeCopy.ContNo = charge.ContNo;
                    chargeCopy.Soaclosed = charge.Soaclosed;
                    chargeCopy.Cdclosed = charge.Cdclosed;
                    chargeCopy.CreditNo = charge.CreditNo;
                    chargeCopy.DebitNo = charge.DebitNo;
                    chargeCopy.Soano = charge.Soano;
                    chargeCopy.IsFromShipment = charge.IsFromShipment;
                    chargeCopy.PaySoano = charge.PaySoano;
                    chargeCopy.TypeOfFee = charge.TypeOfFee;
                    chargeCopy.AdvanceNo = advance;

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
                        && (ops.Mblno == null ? cst.Mawb : ops.Mblno) != null
                        select new Shipments
                        {
                            JobId = (ops.JobNo == null ? cst.JobNo : ops.JobNo),
                            HBL = (ops.Hwbno == null ? cstd.Hwbno : ops.Hwbno),
                            MBL = (ops.Mblno == null ? cst.Mawb : ops.Mblno)
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
                               ad.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE
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
        
        //Check group trước đó đã được approve hay chưa? Nếu group trước đó đã approve thì group hiện tại mới được Approve
        //Nếu group hiện tại đã được approve thì không cho approve nữa
        private HandleState CheckApprovedOfDeptPrevAndDeptCurrent(string settlementNo, ICurrentUser _userCurrent, string deptOfUser)
        {
            HandleState result = new HandleState("Not allow approve/deny");

            //Lấy ra Settlement Approval dựa vào settlementNo
            var acctApprove = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementNo && x.IsDeputy == false).FirstOrDefault();
            if (acctApprove == null)
            {
                result = new HandleState("Not found settlement approval by SettlementNo is " + settlementNo);
                return result;
            }

            //Lấy ra Settlement Payment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            if (settlement == null)
            {
                result = new HandleState("Not found settlement payment by SettlementNo is" + settlementNo);
                return result;
            }

            //Lấy ra brandId của user requester
            var brandOfUserRequest = settlement.OfficeId;
            if (brandOfUserRequest == Guid.Empty || brandOfUserRequest == null) return new HandleState("Not found office of user requester");

            //Lấy ra brandId của userId
            var brandOfUserId = _userCurrent.OfficeID;
            if (brandOfUserId == Guid.Empty || brandOfUserId == null) return new HandleState("Not found office of user");

            //Trường hợp không có Leader
            if (string.IsNullOrEmpty(acctApprove.Leader))
            {
                //Manager Department Approve
                var managerOfUserRequester = userBaseService.GetDeptManager(settlement.CompanyId, settlement.OfficeId, settlement.DepartmentId).FirstOrDefault();
                //Kiểm tra user có phải là dept manager hoặc có phải là user được ủy quyền duyệt (Manager Dept) hay không
                if (_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId) == false
                    && (_userCurrent.UserID == managerOfUserRequester
                        || userBaseService.CheckDeputyManagerByUser(_userCurrent.DepartmentId, _userCurrent.UserID)))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay ko
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (_userCurrent.CompanyID == settlement.CompanyId
                       && _userCurrent.OfficeID == settlement.OfficeId
                       && _userCurrent.DepartmentId != settlement.DepartmentId)
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
                        if (settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                            && acctApprove.ManagerAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                        {
                            result = new HandleState("Manager department approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Requester or Requester not approve");
                    }
                }

                //Accountant Approve
                var accountantOfUser = userBaseService.GetAccoutantManager(settlement.CompanyId, settlement.OfficeId).FirstOrDefault();
                //Kiểm tra user có phải là Accountant Manager hoặc có phải là user được ủy quyền duyệt hay không
                if (_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId)
                    && (_userCurrent.UserID == accountantOfUser
                        || userBaseService.CheckDeputyAccountantByUser(_userCurrent.DepartmentId, _userCurrent.UserID)))
                {
                    //Check group DepartmentManager đã được Approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Manager)
                        && settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                        && acctApprove.ManagerAprDate != null
                        && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                    {
                        result = new HandleState();
                        //Check group Accountant đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DONE)
                            && acctApprove.AccountantAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.AccountantApr))
                        {
                            result = new HandleState("Chief accountant approved");
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
                if (_userCurrent.GroupId != AccountingConstants.SpecialGroup && _userCurrent.UserID == userBaseService.GetLeaderIdOfUser(settlement.Requester))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (_userCurrent.CompanyID == settlement.CompanyId
                       && _userCurrent.OfficeID == settlement.OfficeId
                       && _userCurrent.DepartmentId != settlement.DepartmentId)
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
                        if (settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED)
                            && acctApprove.LeaderAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.Leader))
                        {
                            result = new HandleState("Leader approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Requester or Requester not approve");
                    }
                }

                //Manager Department Approve
                var managerOfUserRequester = userBaseService.GetDeptManager(settlement.CompanyId, settlement.OfficeId, settlement.DepartmentId).FirstOrDefault();
                //Kiểm tra user có phải là dept manager hoặc có phải là user được ủy quyền duyệt (Manager Dept) hay không
                if (_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId) == false
                    && (_userCurrent.UserID == managerOfUserRequester
                        || userBaseService.CheckDeputyManagerByUser(_userCurrent.DepartmentId, _userCurrent.UserID)))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (_userCurrent.CompanyID == settlement.CompanyId
                       && _userCurrent.OfficeID == settlement.OfficeId
                       && _userCurrent.DepartmentId != settlement.DepartmentId)
                    {
                        result = new HandleState("Not in the same department");
                    }
                    else
                    {
                        result = new HandleState();
                    }

                    //Check group Leader đã được approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Leader)
                        && settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED)
                        && acctApprove.LeaderAprDate != null)
                    {
                        result = new HandleState();
                        //Check group Manager Department đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                            && acctApprove.ManagerAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                        {
                            result = new HandleState("Manager department approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Leader or Leader not approve");
                    }
                }

                //Accountant Approve
                var accountantOfUser = userBaseService.GetAccoutantManager(settlement.CompanyId, settlement.OfficeId).FirstOrDefault();
                //Kiểm tra user có phải là Accountant Manager hoặc có phải là user được ủy quyền duyệt (Accoutant) hay không
                if (_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId)
                    && (_userCurrent.UserID == accountantOfUser
                        || userBaseService.CheckDeputyAccountantByUser(_userCurrent.DepartmentId, _userCurrent.UserID)))
                {
                    //Check group DepartmentManager đã được Approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Manager)
                        && settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                        && acctApprove.ManagerAprDate != null
                        && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                    {
                        result = new HandleState();
                        //Check group Accountant đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DONE)
                            && acctApprove.AccountantAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.AccountantApr))
                        {
                            result = new HandleState("Chief accountant approved");
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
        private bool SendMailSuggestApproval(string settlementNo, string userReciver, string emailUserReciver, List<string> usersDeputy)
        {
            var surcharge = csShipmentSurchargeRepo.Get();

            //Lấy ra SettlementPayment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            if (settlement == null) return false;

            //Quy đổi tỉ giá theo ngày Request Date
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == settlement.RequestDate.Value.Date).ToList();

            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(settlement.Requester);
            var _requester = userBaseService.GetEmployeeByEmployeeId(requesterId);
            var requesterName = _requester?.EmployeeNameEn;
            var emailRequester = _requester?.Email;

            //Lấy ra thông tin JobId dựa vào SettlementNo
            var listJobId = GetJobIdBySettlementNo(settlementNo);
            string jobIds = string.Empty;
            jobIds = String.Join("; ", listJobId.ToList());

            var totalAmount = surcharge
                .Where(x => x.SettlementCode == settlementNo)
                .Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, settlement.SettlementCurrency));
            totalAmount = Math.Round(totalAmount, 2);

            //Lấy ra list shipment(JobId,MBL,HBL) dựa vào SettlementNo
            var shipments = GetShipmentBySettlementNo(settlementNo);
            //Lấy ra list AdvanceNo dựa vào Shipment(JobId,MBL,HBL)
            string advanceNos = string.Empty;
            if (shipments != null && shipments.Count() > 0)
            {
                foreach (var shipment in shipments)
                {
                    var listAdvanceNo = GetAdvanceNoByShipment(shipment.JobId, shipment.MBL, shipment.HBL);
                    foreach (var advanceNo in listAdvanceNo)
                    {
                        advanceNos += !string.IsNullOrEmpty(advanceNo) ? advanceNo + "; " : string.Empty;
                    }
                }
                advanceNos += ")";
                advanceNos = advanceNos != ")" ? advanceNos.Replace("; )", string.Empty) : string.Empty;
            }

            var userReciverId = userBaseService.GetEmployeeIdOfUser(userReciver);
            var userReciverName = userBaseService.GetEmployeeByEmployeeId(userReciverId)?.EmployeeNameEn;

            //Mail Info
            var numberOfRequest = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo).Select(s => s.Id).Count();
            numberOfRequest = numberOfRequest == 0 ? 1 : (numberOfRequest + 1);
            string subject = "eFMS - Settlement Payment Approval Request from [RequesterName] - [NumberOfRequest] " + (numberOfRequest > 1 ? "times" : "time");
            subject = subject.Replace("[RequesterName]", requesterName);
            subject = subject.Replace("[NumberOfRequest]", numberOfRequest.ToString());
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p> <i> <b>Dear Mr/Mrs [UserName],</b> </i></p><p>You have new Settlement Payment Approval Request from <b>[RequesterName]</b> as below info:</p><p> <i>Anh/ Chị có một yêu cầu duyệt thanh toán từ <b>[RequesterName]</b> với thông tin như sau: </i></p><ul><li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li><li>Settlement Amount/ <i>Số tiền thanh toán</i> : <b>[TotalAmount] [CurrencySettlement]</b></li><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li></ul><p>You click here to check more detail and approve: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]/approve' target='_blank'>Detail Payment Request</a> </span></p><p> <i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]/approve' target='_blank'>Chi tiết phiếu đề nghị thanh toán</a> </span> </i></p><p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='{0}'/></p></div>", CrystalEx.GetLogoEFMS());
            body = body.Replace("[UserName]", userReciverName);
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

            if (usersDeputy.Count > 0)
            {
                foreach (var userName in usersDeputy)
                {
                    //Lấy ra userId by userName
                    var userId = sysUserRepo.Get(x => x.Username == userName).FirstOrDefault()?.Id;
                    //Lấy ra employeeId của user
                    var employeeIdOfUser = userBaseService.GetEmployeeIdOfUser(userId);
                    //Lấy ra email của user
                    var emailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfUser)?.Email;
                    if (!string.IsNullOrEmpty(emailUser))
                    {
                        emailCCs.Add(emailUser);
                    }
                }
            }

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        //Send Mail Approved
        private bool SendMailApproved(string settlementNo, DateTime approvedDate)
        {
            var surcharge = csShipmentSurchargeRepo.Get();

            //Lấy ra SettlementPayment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            if (settlement == null) return false;

            //Quy đổi tỉ giá theo ngày Request Date
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == settlement.RequestDate.Value.Date).ToList();

            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(settlement.Requester);
            var _requester = userBaseService.GetEmployeeByEmployeeId(requesterId);
            var requesterName = _requester?.EmployeeNameEn;
            var emailRequester = _requester?.Email;

            //Lấy ra thông tin JobId dựa vào SettlementNo
            var listJobId = GetJobIdBySettlementNo(settlementNo);
            string jobIds = string.Empty;
            jobIds = String.Join("; ", listJobId.ToList());

            var totalAmount = surcharge
                .Where(x => x.SettlementCode == settlementNo)
                .Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, settlement.SettlementCurrency));
            totalAmount = Math.Round(totalAmount, 2);

            //Lấy ra list shipment(JobId,MBL,HBL) dựa vào SettlementNo
            var shipments = GetShipmentBySettlementNo(settlementNo);
            //Lấy ra list AdvanceNo dựa vào Shipment(JobId,MBL,HBL)
            string advanceNos = string.Empty;
            if (shipments != null && shipments.Count() > 0)
            {
                foreach (var shipment in shipments)
                {
                    var listAdvanceNo = GetAdvanceNoByShipment(shipment.JobId, shipment.MBL, shipment.HBL);
                    foreach (var advanceNo in listAdvanceNo)
                    {
                        advanceNos += !string.IsNullOrEmpty(advanceNo) ? advanceNo + "; " : string.Empty;
                    }
                }
                advanceNos += ")";
                advanceNos = advanceNos != ")" ? advanceNos.Replace("; )", string.Empty) : string.Empty;
            }

            //Mail Info
            string subject = "eFMS - Settlement Payment from [RequesterName] is approved";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p> <i> <b>Dear Mr/Mrs [RequesterName],</b> </i></p><p>You have an Settlement Payment is approved at <b>[ApprovedDate]</b> as below info:</p><p> <i>Anh/ Chị có một đề nghị thanh toán đã được phê duyệt vào lúc <b>[ApprovedDate]</b> với thông tin như sau: </i></p><ul><li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li><li>Settlement Amount/ <i>Số tiền thanh toán</i> : <b>[TotalAmount] [CurrencySettlement]</b></li><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li></ul><p>You can click here to check more detail: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Detail Payment Request</a> </span></p><p> <i>Anh/ Chị có thể chọn vào đây để biết thêm thông tin chi tiết: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Chi tiết đề nghị thanh toán</a> </span> </i></p><p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='{0}'/></p></div>", CrystalEx.GetLogoEFMS());
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

            //Lấy ra SettlementPayment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            if (settlement == null) return false;

            //Quy đổi tỉ giá theo ngày Request Date
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == settlement.RequestDate.Value.Date).ToList();

            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(settlement.Requester);
            var _requester = userBaseService.GetEmployeeByEmployeeId(requesterId);
            var requesterName = _requester?.EmployeeNameEn;
            var emailRequester = _requester?.Email;

            //Lấy ra thông tin JobId dựa vào SettlementNo
            var listJobId = GetJobIdBySettlementNo(settlementNo);
            string jobIds = string.Empty;
            jobIds = String.Join("; ", listJobId.ToList());

            var totalAmount = surcharge
                .Where(x => x.SettlementCode == settlementNo)
                .Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, settlement.SettlementCurrency));
            totalAmount = Math.Round(totalAmount, 2);

            //Lấy ra list shipment(JobId,MBL,HBL) dựa vào SettlementNo
            var shipments = GetShipmentBySettlementNo(settlementNo);
            //Lấy ra list AdvanceNo dựa vào Shipment(JobId,MBL,HBL)
            string advanceNos = string.Empty;
            if (shipments != null && shipments.Count() > 0)
            {
                foreach (var shipment in shipments)
                {
                    var listAdvanceNo = GetAdvanceNoByShipment(shipment.JobId, shipment.MBL, shipment.HBL);
                    foreach (var advanceNo in listAdvanceNo)
                    {
                        advanceNos += !string.IsNullOrEmpty(advanceNo) ? advanceNo + "; " : string.Empty;
                    }
                }
                advanceNos += ")";
                advanceNos = advanceNos != ")" ? advanceNos.Replace("; )", string.Empty) : string.Empty;
            }

            //Mail Info
            string subject = "eFMS - Settlement Payment from [RequesterName] is denied";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p> <i> <b>Dear Mr/Mrs [RequesterName],</b> </i></p><p>You have an Settlement Payment is denied at <b>[DeniedDate]</b> by as below info:</p><p> <i>Anh/ Chị có một yêu cầu đề nghị thanh toán đã bị từ chối vào lúc <b>[DeniedDate]</b> by với thông tin như sau: </i></p><ul><li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li><li>Settlement Amount/ <i>Số tiền tạm ứng</i> : <b>[TotalAmount] [CurrencySettlement]</b></li><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li><li>Comment/ <i>Lý do từ chối</i> : <b>[Comment]</b></li></ul><p>You click here to recheck detail: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Detail Payment Request</a> </span></p><p> <i>Anh/ Chị chọn vào đây để kiểm tra lại thông tin chi tiết: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Chi tiết đề nghị thanh toán</a> </span> </i></p><p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='{0}'/></p></div>", CrystalEx.GetLogoEFMS());
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

        //Kiểm tra User đăng nhập vào có thuộc các user Approve Settlement không, nếu không thuộc bất kỳ 1 user nào thì gán cờ IsApproved bằng true
        //Kiểm tra xem dept đã approve chưa, nếu dept của user đó đã approve thì gán cờ IsApproved bằng true
        private bool CheckUserInApproveSettlementAndDeptApproved(ICurrentUser userCurrent, AcctApproveSettlementModel approveSettlement)
        {
            var isApproved = false;
            var isDeputyManage = userBaseService.CheckDeputyManagerByUser(userCurrent.DepartmentId, userCurrent.UserID);
            var isDeputyAccoutant = userBaseService.CheckDeputyAccountantByUser(userCurrent.DepartmentId, userCurrent.UserID);

            // 1 user vừa có thể là Requester, Manager Dept, Accountant Dept nên khi check Approved cần phải dựa vào group
            // Group 11 chính là group Manager

            if (userCurrent.GroupId != AccountingConstants.SpecialGroup 
                && userCurrent.UserID == approveSettlement.Requester) //Requester
            {
                isApproved = true;
                if (approveSettlement.RequesterAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.GroupId != AccountingConstants.SpecialGroup 
                && userCurrent.UserID == approveSettlement.Leader) //Leader
            {
                isApproved = true;
                if (approveSettlement.LeaderAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.GroupId == AccountingConstants.SpecialGroup
                && userBaseService.CheckIsAccountantDept(userCurrent.DepartmentId) == false
                && (userCurrent.UserID == approveSettlement.Manager 
                    || userCurrent.UserID == approveSettlement.ManagerApr 
                    || isDeputyManage)) //Dept Manager
            {
                isApproved = true;
                var isDeptWaitingApprove = DataContext.Get(x => x.SettlementNo == approveSettlement.SettlementNo && (x.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && x.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED)).Any();
                if (string.IsNullOrEmpty(approveSettlement.ManagerApr) && approveSettlement.ManagerAprDate == null && isDeptWaitingApprove)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.GroupId == AccountingConstants.SpecialGroup
                && userBaseService.CheckIsAccountantDept(userCurrent.DepartmentId)
                && (userCurrent.UserID == approveSettlement.Accountant 
                    || userCurrent.UserID == approveSettlement.AccountantApr 
                    || isDeputyAccoutant)) //Accountant Manager
            {
                isApproved = true;
                var isDeptWaitingApprove = DataContext.Get(x => x.SettlementNo == approveSettlement.SettlementNo && (x.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && x.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED && x.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)).Any();
                if (string.IsNullOrEmpty(approveSettlement.AccountantApr) && approveSettlement.AccountantAprDate == null && isDeptWaitingApprove)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.UserID == approveSettlement.Buhead || userCurrent.UserID == approveSettlement.BuheadApr) //BUHead
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

        public ResultHandle UnLock(List<string> keyWords)
        {
            var settleToUnLocks = DataContext.Get(x => keyWords.Contains(x.SettlementNo));
            try
            {
                List<string> results = new List<string>();
                foreach (var item in settleToUnLocks)
                {
                    string log = string.Empty;
                    var logs = item.LockedLog != null ? item.LockedLog.Split(';').Where(x => x.Length > 0).ToList() : new List<string>();
                    if (item.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED)
                    {
                        item.StatusApproval = AccountingConstants.STATUS_APPROVAL_DENIED;
                        log = item.SettlementNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + "admin";
                        item.LockedLog = item.LockedLog + log + ";";
                        item.UserModified = currentUser.UserName;
                        item.DatetimeModified = DateTime.Now;
                        var hs = DataContext.Update(item, x => x.Id == item.Id);
                        if (hs.Success == false)
                        {
                            log = item.SettlementNo + " unlock failed " + hs.Message;
                        }
                        if (log.Length > 0) logs.Add(log);
                    }
                    if (logs.Count > 0) results.AddRange(logs);
                }
                return new ResultHandle { Status = true, Message = "Done", Data = results };
            }
            catch (Exception)
            {

                throw;
            }
        }

        public LockedLogResultModel GetSettlePaymentsToUnlock(List<string> keyWords)
        {
            var result = new LockedLogResultModel();
            var settlesToUnLock = DataContext.Get(x => keyWords.Contains(x.SettlementNo));
            if (settlesToUnLock == null) return result;
            result.LockedLogs = settlesToUnLock.Select(x => new LockedLogModel
            {
                Id = x.Id,
                SettlementNo = x.SettlementNo,
                LockedLog = x.LockedLog
            });
            if (result.LockedLogs != null)
            {
                result.Logs = new List<string>();
                foreach (var item in settlesToUnLock)
                {
                    var logs = item.LockedLog != null ? item.LockedLog.Split(';').Where(x => x.Length > 0).ToList() : new List<string>();
                    result.Logs.AddRange(logs);
                }
            }
            return result;
        }

        public HandleState UnLock(List<LockedLogModel> settlePayments)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in settlePayments)
                    {
                        var settle = DataContext.Get(x => x.Id == item.Id)?.FirstOrDefault();
                        if (settle.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED)
                        {
                            settle.StatusApproval = AccountingConstants.STATUS_APPROVAL_DENIED;
                            settle.UserModified = currentUser.UserName;
                            settle.DatetimeModified = DateTime.Now;
                            var log = item.SettlementNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + "admin";
                            settle.LockedLog = item.LockedLog + log + ";";
                            var hs = DataContext.Update(settle, x => x.Id == item.Id);
                            var approveSettles = acctApproveSettlementRepo.Get(x => x.SettlementNo == item.SettlementNo);
                            foreach (var approve in approveSettles)
                            {
                                approve.IsDeputy = true;
                                approve.UserModified = currentUser.UserID;
                                approve.DateModified = DateTime.Now;
                                acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id);
                            }
                        }
                    }
                    trans.Commit();
                    return new HandleState();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
        
        #endregion

        public bool CheckDetailPermissionBySettlementNo(string settlementNo)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.SettlementNo == settlementNo)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, permissionRange, _user);

            if (code == 403) return false;

            return true;
        }

        public bool CheckDetailPermissionBySettlementId(Guid settlementId)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.Id == settlementId)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, permissionRange, _user);

            if (code == 403) return false;

            return true;
        }

        public bool CheckDeletePermissionBySettlementNo(string settlementNo)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.SettlementNo == settlementNo)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, permissionRange, _user);

            if (code == 403) return false;

            return true;
        }

        public bool CheckDeletePermissionBySettlementId(Guid settlementId)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.Id == settlementId)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, permissionRange, _user);

            if (code == 403) return false;

            return true;
        }

        public bool CheckUpdatePermissionBySettlementId(Guid settlementId)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.Id == settlementId)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, permissionRange, _user);

            if (code == 403) return false;

            return true;
        }

        #region --- EXPORT SETTLEMENT ---
        public SettlementExport SettlementExport(Guid settlementId)
        {
            SettlementExport dataExport = new SettlementExport();
            var settlementPayment = GetSettlementPaymentById(settlementId);
            if (settlementPayment == null) return null;

            dataExport.InfoSettlement = GetInfoSettlementExport(settlementPayment);
            dataExport.ShipmentsSettlement = GetListShipmentSettlementExport(settlementPayment);
            return dataExport;
        }

        public InfoSettlementExport GetInfoSettlementExport(AcctSettlementPaymentModel settlementPayment)
        {
            string _requester = string.IsNullOrEmpty(settlementPayment.Requester) ? string.Empty : userBaseService.GetEmployeeByUserId(settlementPayment.Requester)?.EmployeeNameVn;

            #region -- Info Manager, Accoutant & Department --
            var _settlementApprove = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementPayment.SettlementNo && x.IsDeputy == false).FirstOrDefault();
            string _manager = string.Empty;
            string _accountant = string.Empty;
            if (_settlementApprove != null)
            {
                _manager = string.IsNullOrEmpty(_settlementApprove.Manager) ? string.Empty : userBaseService.GetEmployeeByUserId(_settlementApprove.Manager)?.EmployeeNameVn;
                _accountant = string.IsNullOrEmpty(_settlementApprove.Accountant) ? string.Empty : userBaseService.GetEmployeeByUserId(_settlementApprove.Accountant)?.EmployeeNameVn;
            }

            var _department = catDepartmentRepo.Get(x => x.Id == settlementPayment.DepartmentId).FirstOrDefault()?.DeptNameAbbr;
            #endregion -- Info Manager, Accoutant & Department --

            var infoSettlement = new InfoSettlementExport
            {
                Requester = _requester,
                RequestDate = settlementPayment.RequestDate,
                Department = _department,
                SettlementNo = settlementPayment.SettlementNo,
                Manager = _manager,
                Accountant = _accountant
            };
            return infoSettlement;
        }

        public List<InfoShipmentSettlementExport> GetListShipmentSettlementExport(AcctSettlementPaymentModel settlementPayment)
        {
            var listData = new List<InfoShipmentSettlementExport>();

            //Quy đổi tỉ giá theo ngày Request Date
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == settlementPayment.RequestDate.Value.Date).ToList();

            var surChargeBySettleCode = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementPayment.SettlementNo);

            var houseBillIds = surChargeBySettleCode.Select(s => new { hblId = s.Hblid, customNo = s.ClearanceNo }).Distinct();
            foreach (var houseBillId in houseBillIds)
            {
                var shipmentSettlement = new InfoShipmentSettlementExport();
                var ops = opsTransactionRepo.Get(x => x.Hblid == houseBillId.hblId).FirstOrDefault();
                if (ops != null)
                {
                    shipmentSettlement.JobNo = ops.JobNo;
                    shipmentSettlement.CustomNo = houseBillId.customNo;
                    shipmentSettlement.HBL = ops.Hwbno;
                    shipmentSettlement.MBL = ops.Mblno;
                    shipmentSettlement.Customer = catPartnerRepo.Get(x => x.Id == ops.CustomerId).FirstOrDefault()?.PartnerNameVn;
                    shipmentSettlement.Container = ops.ContainerDescription;
                    shipmentSettlement.Cw = ops.SumChargeWeight;
                    shipmentSettlement.Pcs = ops.SumPackages;
                    shipmentSettlement.Cbm = ops.SumCbm;
                    var advanceRequests = acctAdvanceRequestRepo.Get(x => x.JobId == shipmentSettlement.JobNo
                                                                       && x.Mbl == shipmentSettlement.MBL
                                                                       && x.Hbl == shipmentSettlement.HBL);

                    var advanceRequest = advanceRequests.FirstOrDefault();

                    //Chỉ lấy những advance đã được phê duyệt
                    if (advanceRequest != null)
                    {
                        var advancePayment = acctAdvancePaymentRepo.Get(x => x.AdvanceNo == advanceRequest.AdvanceNo).FirstOrDefault();
                        if (advancePayment != null && advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                        {
                            shipmentSettlement.AdvanceAmount = advanceRequests.Select(s => s.Amount * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, s.RequestCurrency, settlementPayment.SettlementCurrency)).Sum();
                            shipmentSettlement.AdvanceRequestDate = advancePayment.RequestDate;
                        }
                    }

                    var list = new List<InfoShipmentChargeSettlementExport>();
                    var surChargeByHblId = surChargeBySettleCode.Where(x => x.Hblid == houseBillId.hblId);
                    foreach (var sur in surChargeByHblId)
                    {
                        var infoShipmentCharge = new InfoShipmentChargeSettlementExport();
                        infoShipmentCharge.ChargeName = catChargeRepo.Get(x => x.Id == sur.ChargeId).FirstOrDefault()?.ChargeNameEn;
                        //Quy đổi theo currency của Settlement
                        infoShipmentCharge.ChargeAmount = sur.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settlementPayment.SettlementCurrency);
                        infoShipmentCharge.InvoiceNo = sur.InvoiceNo;
                        infoShipmentCharge.ChargeNote = sur.Notes;
                        string _chargeType = string.Empty;
                        if (sur.Type == "OBH" || (sur.IsFromShipment == false && sur.TypeOfFee == "OBH"))
                        {
                            _chargeType = "OBH";
                        }
                        else if (sur.TypeOfFee == "Invoice" || (sur.IsFromShipment == true && sur.Type != "OBH"))
                        {
                            _chargeType = "INVOICE";
                        }
                        else
                        {
                            _chargeType = "NO_INVOICE";
                        }
                        infoShipmentCharge.ChargeType = _chargeType;

                        list.Add(infoShipmentCharge);
                    }
                    shipmentSettlement.ShipmentCharges = list;

                    listData.Add(shipmentSettlement);
                }
                else
                {
                    var tranDetail = csTransactionDetailRepo.Get(x => x.Id == houseBillId.hblId).FirstOrDefault();
                    if (tranDetail != null)
                    {
                        shipmentSettlement.JobNo = csTransactionRepo.Get(x => x.Id == tranDetail.JobId).FirstOrDefault().JobNo;
                        shipmentSettlement.CustomNo = houseBillId.customNo;
                        shipmentSettlement.HBL = tranDetail.Hwbno;
                        shipmentSettlement.MBL = csTransactionRepo.Get(x => x.Id == tranDetail.JobId).FirstOrDefault().Mawb;
                        shipmentSettlement.Customer = catPartnerRepo.Get(x => x.Id == tranDetail.CustomerId).FirstOrDefault()?.PartnerNameVn;
                        shipmentSettlement.Shipper = catPartnerRepo.Get(x => x.Id == tranDetail.ShipperId).FirstOrDefault()?.PartnerNameVn;
                        shipmentSettlement.Consignee = catPartnerRepo.Get(x => x.Id == tranDetail.ConsigneeId).FirstOrDefault()?.PartnerNameVn;
                        shipmentSettlement.Container = tranDetail.PackageContainer;
                        shipmentSettlement.Cw = tranDetail.ChargeWeight;
                        shipmentSettlement.Pcs = tranDetail.PackageQty;
                        shipmentSettlement.Cbm = tranDetail.Cbm;
                        var advanceRequests = acctAdvanceRequestRepo.Get(x => x.JobId == shipmentSettlement.JobNo
                                                                           && x.Mbl == shipmentSettlement.MBL
                                                                           && x.Hbl == shipmentSettlement.HBL);

                        var advanceRequest = advanceRequests.FirstOrDefault();

                        //Chỉ lấy những advance đã được phê duyệt
                        if (advanceRequest != null)
                        {
                            var advancePayment = acctAdvancePaymentRepo.Get(x => x.AdvanceNo == advanceRequest.AdvanceNo).FirstOrDefault();
                            if (advancePayment != null && advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                            {
                                shipmentSettlement.AdvanceAmount = advanceRequests.Select(s => s.Amount * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, s.RequestCurrency, settlementPayment.SettlementCurrency)).Sum();
                                shipmentSettlement.AdvanceRequestDate = advancePayment.RequestDate;
                            }
                        }

                        var list = new List<InfoShipmentChargeSettlementExport>();
                        var surChargeByHblId = surChargeBySettleCode.Where(x => x.Hblid == houseBillId.hblId);
                        foreach (var sur in surChargeByHblId)
                        {
                            var infoShipmentCharge = new InfoShipmentChargeSettlementExport();
                            infoShipmentCharge.ChargeName = catChargeRepo.Get(x => x.Id == sur.ChargeId).FirstOrDefault()?.ChargeNameEn;
                            //Quy đổi theo currency của Settlement
                            infoShipmentCharge.ChargeAmount = sur.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settlementPayment.SettlementCurrency);
                            infoShipmentCharge.InvoiceNo = sur.InvoiceNo;
                            infoShipmentCharge.ChargeNote = sur.Notes;
                            string _chargeType = string.Empty;
                            if (sur.Type == "OBH" || (sur.IsFromShipment == false && sur.TypeOfFee == "OBH"))
                            {
                                _chargeType = "OBH";
                            }
                            else if (sur.TypeOfFee == "Invoice" || (sur.IsFromShipment == true && sur.Type != "OBH"))
                            {
                                _chargeType = "INVOICE";
                            }
                            else
                            {
                                _chargeType = "NO_INVOICE";
                            }
                            infoShipmentCharge.ChargeType = _chargeType;

                            list.Add(infoShipmentCharge);
                        }
                        shipmentSettlement.ShipmentCharges = list;

                        listData.Add(shipmentSettlement);
                    }
                }
            }
            return listData;
        }

        public List<SettlementExportGroupDefault> QueryDataSettlementExport(string[] settlementCode)
        {
            var results = new List<SettlementExportGroupDefault>();
            var settlements = DataContext.Get();
            var surcharges = csShipmentSurchargeRepo.Get();
            var opsTransations = opsTransactionRepo.Get();
            var csTransations = csTransactionRepo.Get();
            var csTranstionDetails = csTransactionDetailRepo.Get();  // HBL
            var custom = customsDeclarationRepo.Get();
            var advRequest = acctAdvanceRequestRepo.Get();

            try
            {
                if (settlementCode.Count() > 0)
                {
                    foreach (var settleCode in settlementCode)
                    {
                        var currentSettlement = settlements.Where(x => x.SettlementNo == settleCode).FirstOrDefault();

                        string requesterID = DataContext.First(x => x.SettlementNo == settleCode).Requester;
                        string employeeID = "";
                        if (!string.IsNullOrEmpty(requesterID))
                        {
                            employeeID = sysUserRepo.Get(x => x.Id == requesterID).FirstOrDefault()?.EmployeeId;
                        }

                        var approveDate = acctApproveSettlementRepo.Get(x => x.SettlementNo == settleCode).FirstOrDefault()?.BuheadAprDate;
                        string requesterName = sysEmployeeRepo.Get(x => x.Id == employeeID).FirstOrDefault()?.EmployeeNameVn;

                        // Get Operation
                        var dataOperation = from set in settlements
                                            join sur in surcharges on set.SettlementNo equals sur.SettlementCode into sc // Join Surcharge.
                                            from sur in sc.DefaultIfEmpty()
                                            join ops in opsTransations on sur.Hblid equals ops.Hblid // Join OpsTranstion
                                            join cus in custom on new { JobNo = (ops.JobNo != null ? ops.JobNo : ops.JobNo), HBL = (ops.Hwbno != null ? ops.Hwbno : ops.Hwbno), MBL = (ops.Mblno != null ? ops.Mblno : ops.Mblno) } equals new { JobNo = cus.JobNo, HBL = cus.Hblid, MBL = cus.Mblid } into cus1
                                            from cus in cus1.DefaultIfEmpty()
                                            where sur.SettlementCode == settleCode
                                            select new SettlementExportDefault
                                            {
                                                JobID = ops.JobNo,
                                                HBL = ops.Hwbno,
                                                MBL = ops.Mblno,

                                                CustomNo = cus.ClearanceNo,
                                                SettleNo = currentSettlement.SettlementNo,
                                                Currency = currentSettlement.SettlementCurrency,
                                                AdvanceNo = sur.AdvanceNo,
                                                Requester = requesterName,
                                                RequestDate = currentSettlement.RequestDate,
                                                ApproveDate = approveDate,
                                                Description = sur.Notes,
                                                SettlementAmount = sur.Total,

                                            };

                        // Get Document
                        var dataService = from set in settlements
                                          join sur in surcharges on set.SettlementNo equals sur.SettlementCode into sc  // Join Surcharge.
                                          from sur in sc.DefaultIfEmpty()
                                          join cstd in csTranstionDetails on sur.Hblid equals cstd.Id // Join HBL
                                          join cst in csTransations on cstd.JobId equals cst.Id into cs // join Cs Transation
                                          from cst in cs.DefaultIfEmpty()
                                          join cus in custom on new { JobNo = (cst.JobNo != null ? cst.JobNo : cst.JobNo), HBL = (cstd.Hwbno != null ? cstd.Hwbno : cstd.Hwbno), MBL = (cstd.Mawb != null ? cstd.Mawb : cstd.Mawb) } equals new { JobNo = cus.JobNo, HBL = cus.Hblid, MBL = cus.Mblid } into cus1
                                          from cus in cus1.DefaultIfEmpty()
                                          where sur.SettlementCode == settleCode
                                          select new SettlementExportDefault
                                          {
                                              JobID = cst.JobNo,
                                              HBL = cstd.Hwbno,
                                              MBL = cst.Mawb,
                                              SettlementAmount = sur.Total,
                                              CustomNo = cus.ClearanceNo,
                                              SettleNo = currentSettlement.SettlementNo,
                                              Currency = currentSettlement.SettlementCurrency,
                                              AdvanceNo = sur.AdvanceNo,
                                              Requester = requesterName,
                                              RequestDate = currentSettlement.RequestDate,
                                              ApproveDate = approveDate,
                                              Description = sur.Notes,

                                          };

                        var data = dataOperation.Union(dataService).ToList();

                        var group = data.GroupBy(d => new { d.JobID, d.HBL, d.MBL, d.CustomNo }).Select(s => new SettlementExportGroupDefault
                        {
                            JobID = s.Key.JobID,
                            MBL = s.Key.MBL,
                            HBL = s.Key.HBL,
                            CustomNo = s.Key.CustomNo,
                            AdvanceTotalAmount = s.Sum(su => su.AdvanceAmount),
                            SettlementTotalAmount = s.Sum(d => d.SettlementAmount),
                            BalanceTotalAmount = s.Sum(d => d.SettlementAmount) - s.Sum(su => su.AdvanceAmount),
                            requestList = data.Where(w => w.JobID == s.Key.JobID && w.MBL == s.Key.MBL && w.HBL == s.Key.HBL && w.CustomNo == s.Key.CustomNo).ToList()
                        });

                        // data = data.o.OrderByDescending(x => x.JobID).AsQueryable();
                        foreach (var item in group)
                        {
                            results.Add(item);
                        }
                    }
                }
                else
                {
                    return new List<SettlementExportGroupDefault>();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return results;

        }
        #endregion --- EXPORT SETTLEMENT ---
    }
}
