using AutoMapper;
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
using System.Linq.Expressions;

namespace eFMS.API.Accounting.DL.Services
{
    public class AcctSettlementPaymentService : RepositoryBase<AcctSettlementPayment, AcctSettlementPaymentModel>, IAcctSettlementPaymentService
    {
        private readonly ICurrentUser currentUser;
        private readonly IOptions<WebUrl> webUrl;
        private readonly IOptions<ApiUrl> apiUrl;
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
        private string typeApproval = "Settlement";

        public AcctSettlementPaymentService(IContextBase<AcctSettlementPayment> repository,
            IMapper mapper,
            ICurrentUser user,
            IOptions<WebUrl> wUrl,
            IOptions<ApiUrl> aUrl,
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
            webUrl = wUrl;
            apiUrl = aUrl;
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

        #region --- LIST & PAGING SETTLEMENT PAYMENT ---
        public List<AcctSettlementPaymentResult> Paging(AcctSettlementPaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetDatas(criteria);
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

        private PermissionRange GetPermissionRangeOfRequester()
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            PermissionRange _permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            return _permissionRange;
        }

        private Expression<Func<AcctSettlementPayment, bool>> ExpressionQuery(AcctSettlementPaymentCriteria criteria)
        {
            Expression<Func<AcctSettlementPayment, bool>> query = q => true;
            if (criteria.RequestDateFrom.HasValue && criteria.RequestDateTo.HasValue)
            {
                //Convert RequestDate về date nếu RequestDate có value
                query = query.And(x =>
                                   x.RequestDate.Value.Date >= (criteria.RequestDateFrom.HasValue ? criteria.RequestDateFrom.Value.Date : criteria.RequestDateFrom)
                                && x.RequestDate.Value.Date <= (criteria.RequestDateTo.HasValue ? criteria.RequestDateTo.Value.Date : criteria.RequestDateTo));
            }

            if (!string.IsNullOrEmpty(criteria.StatusApproval) && !criteria.StatusApproval.Equals("All"))
            {
                query = query.And(x => x.StatusApproval == criteria.StatusApproval);
            }

            if (!string.IsNullOrEmpty(criteria.PaymentMethod) && !criteria.PaymentMethod.Equals("All"))
            {
                query = query.And(x => x.PaymentMethod == criteria.PaymentMethod);
            }

            if (!string.IsNullOrEmpty(criteria.CurrencyID) && !criteria.CurrencyID.Equals("All"))
            {
                query = query.And(x => x.SettlementCurrency == criteria.CurrencyID);
            }
            return query;
        }

        private IQueryable<AcctSettlementPayment> GetDataSettlementPayment(AcctSettlementPaymentCriteria criteria)
        {
            var permissionRangeRequester = GetPermissionRangeOfRequester();
            var settlementPayments = DataContext.Get();
            var settlementPaymentAprs = acctApproveSettlementRepo.Get(x => x.IsDeny == false);
            var data = from settlementPayment in settlementPayments
                       join settlementPaymentApr in settlementPaymentAprs on settlementPayment.SettlementNo equals settlementPaymentApr.SettlementNo into settlementPaymentApr2
                       from settlementPaymentApr in settlementPaymentApr2.DefaultIfEmpty()
                       select new { settlementPayment, settlementPaymentApr };
            var result = data.Where(x =>
                (
                    permissionRangeRequester == PermissionRange.All ? (criteria.Requester == currentUser.UserID ? x.settlementPayment.UserCreated == criteria.Requester : false) : true
                    &&
                    permissionRangeRequester == PermissionRange.None ? false : true
                    &&
                    permissionRangeRequester == PermissionRange.Owner ? x.settlementPayment.UserCreated == criteria.Requester : true
                    &&
                    permissionRangeRequester == PermissionRange.Group ? (x.settlementPayment.GroupId == currentUser.GroupId
                                                                        && x.settlementPayment.DepartmentId == currentUser.DepartmentId
                                                                        && x.settlementPayment.OfficeId == currentUser.OfficeID
                                                                        && x.settlementPayment.CompanyId == currentUser.CompanyID
                                                                        && (criteria.Requester == currentUser.UserID ? x.settlementPayment.UserCreated == criteria.Requester : false)) : true
                    &&
                    permissionRangeRequester == PermissionRange.Department ? (x.settlementPayment.DepartmentId == currentUser.DepartmentId
                                                                              && x.settlementPayment.OfficeId == currentUser.OfficeID
                                                                              && x.settlementPayment.CompanyId == currentUser.CompanyID
                                                                              && (criteria.Requester == currentUser.UserID ? x.settlementPayment.UserCreated == criteria.Requester : false)) : true
                    &&
                    permissionRangeRequester == PermissionRange.Office ? (x.settlementPayment.OfficeId == currentUser.OfficeID
                                                                          && x.settlementPayment.CompanyId == currentUser.CompanyID
                                                                          && (criteria.Requester == currentUser.UserID ? x.settlementPayment.UserCreated == criteria.Requester : false)) : true
                    &&
                    permissionRangeRequester == PermissionRange.Company ? x.settlementPayment.CompanyId == currentUser.CompanyID && (criteria.Requester == currentUser.UserID ? x.settlementPayment.UserCreated == criteria.Requester : false) : true
                )
                ||
                (x.settlementPaymentApr != null && (x.settlementPaymentApr.Leader == currentUser.UserID
                  || x.settlementPaymentApr.LeaderApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(typeApproval, x.settlementPaymentApr.Leader, x.settlementPayment.GroupId, x.settlementPayment.DepartmentId, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
                )
                && x.settlementPayment.GroupId == currentUser.GroupId
                && x.settlementPayment.DepartmentId == currentUser.DepartmentId
                && x.settlementPayment.OfficeId == currentUser.OfficeID
                && x.settlementPayment.CompanyId == currentUser.CompanyID
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                && (x.settlementPayment.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.settlementPayment.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) //LEADER AND DEPUTY OF LEADER
                ||
                (x.settlementPaymentApr != null && (x.settlementPaymentApr.Manager == currentUser.UserID
                  || x.settlementPaymentApr.ManagerApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(typeApproval, x.settlementPaymentApr.Manager, null, x.settlementPayment.DepartmentId, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
                  )
                && x.settlementPayment.DepartmentId == currentUser.DepartmentId
                && x.settlementPayment.OfficeId == currentUser.OfficeID
                && x.settlementPayment.CompanyId == currentUser.CompanyID
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                && (!string.IsNullOrEmpty(x.settlementPaymentApr.Leader) ? x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL : true)
                && (x.settlementPayment.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.settlementPayment.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) //MANANER AND DEPUTY OF MANAGER
                ||
                (x.settlementPaymentApr != null && (x.settlementPaymentApr.Accountant == currentUser.UserID
                  || x.settlementPaymentApr.AccountantApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(typeApproval, x.settlementPaymentApr.Accountant, null, null, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
                  )
                && x.settlementPayment.OfficeId == currentUser.OfficeID
                && x.settlementPayment.CompanyId == currentUser.CompanyID
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                //&& (!string.IsNullOrEmpty(x.settlementPaymentApr.Leader) ? x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL : true)
                //&& (!string.IsNullOrEmpty(x.settlementPaymentApr.Manager) ? x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED : true)
                && (x.settlementPayment.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.settlementPayment.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) // ACCOUTANT AND DEPUTY OF ACCOUNTANT
                ||
                (x.settlementPaymentApr != null && (x.settlementPaymentApr.Buhead == currentUser.UserID
                  || x.settlementPaymentApr.BuheadApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(typeApproval, x.settlementPaymentApr.Buhead ?? null, null, null, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
                  )
                && x.settlementPayment.OfficeId == currentUser.OfficeID
                && x.settlementPayment.CompanyId == currentUser.CompanyID
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                //&& (!string.IsNullOrEmpty(x.settlementPaymentApr.Leader) ? x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL : true)
                //&& (!string.IsNullOrEmpty(x.settlementPaymentApr.Manager) ? x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED : true)
                //&& (!string.IsNullOrEmpty(x.settlementPaymentApr.Accountant) ? x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED : true)
                && (x.settlementPayment.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.settlementPayment.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) //BOD AND DEPUTY OF BOD                
            ).Select(s => s.settlementPayment);
            return result;
        }

        private IQueryable<AcctSettlementPayment> QueryWithShipment(IQueryable<AcctSettlementPayment> settlementPayments, AcctSettlementPaymentCriteria criteria)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var opst = opsTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var csTransDe = csTransactionDetailRepo.Get();
            var custom = customsDeclarationRepo.Get();
            var advRequest = acctAdvanceRequestRepo.Get();
            List<string> refNo = new List<string>();
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                refNo = (from set in settlementPayments
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

            if (refNo.Count() > 0)
            {
                settlementPayments = settlementPayments.Where(x => refNo.Contains(x.SettlementNo));
            }
            else
            {
                if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
                {
                    settlementPayments = null;
                }
            }
            return settlementPayments;
        }

        public IQueryable<AcctSettlementPaymentResult> GetDatas(AcctSettlementPaymentCriteria criteria)
        {
            var querySettlementPayment = ExpressionQuery(criteria);
            var dataSettlementPayments = GetDataSettlementPayment(criteria);
            if (dataSettlementPayments == null) return null;
            var settlementPayments = dataSettlementPayments.Where(querySettlementPayment);
            settlementPayments = QueryWithShipment(settlementPayments, criteria);
            if (settlementPayments == null) return null;

            var approveSettlePayment = acctApproveSettlementRepo.Get(x => x.IsDeny == false);
            var users = sysUserRepo.Get();

            var data = from settlePayment in settlementPayments
                       join user in users on settlePayment.Requester equals user.Id into user2
                       from user in user2.DefaultIfEmpty()
                       join aproveSettlement in approveSettlePayment on settlePayment.SettlementNo equals aproveSettlement.SettlementNo into aproveSettlement2
                       from aproveSettlement in aproveSettlement2.DefaultIfEmpty()
                       select new AcctSettlementPaymentResult
                       {
                           Id = settlePayment.Id,
                           Amount = settlePayment.Amount ?? 0,
                           SettlementNo = settlePayment.SettlementNo,
                           SettlementCurrency = settlePayment.SettlementCurrency,
                           Requester = settlePayment.Requester,
                           RequesterName = user.Username,
                           RequestDate = settlePayment.RequestDate,
                           StatusApproval = settlePayment.StatusApproval,
                           PaymentMethod = settlePayment.PaymentMethod,
                           Note = settlePayment.Note,
                           DatetimeModified = settlePayment.DatetimeModified,
                           StatusApprovalName = Common.CustomData.StatusApproveAdvance.Where(x => x.Value == settlePayment.StatusApproval).Select(x => x.DisplayName).FirstOrDefault(),
                           PaymentMethodName = Common.CustomData.PaymentMethod.Where(x => x.Value == settlePayment.PaymentMethod).Select(x => x.DisplayName).FirstOrDefault(),
                       };

            //Sort Array sẽ nhanh hơn
            data = data.ToArray().OrderByDescending(orb => orb.DatetimeModified).AsQueryable();
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
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settle.RequestDate.Value.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

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
        #endregion --- LIST & PAGING SETTLEMENT PAYMENT ---

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
            if (settlement == null) return null;
            var settlementMap = mapper.Map<AcctSettlementPaymentModel>(settlement);
            settlementMap.NumberOfRequests = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo).Select(s => s.Id).Count();
            settlementMap.UserNameCreated = sysUserRepo.Get(x => x.Id == settlement.UserCreated).FirstOrDefault()?.Username;
            settlementMap.UserNameModified = sysUserRepo.Get(x => x.Id == settlement.UserModified).FirstOrDefault()?.Username;

            var settlementApprove = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo && x.IsDeny == false).FirstOrDefault();
            settlementMap.IsRequester = (currentUser.UserID == settlement.Requester
                && currentUser.GroupId == settlement.GroupId
                && currentUser.DepartmentId == settlement.DepartmentId
                && currentUser.OfficeID == settlement.OfficeId
                && currentUser.CompanyID == settlement.CompanyId) ? true : false;
            settlementMap.IsManager = CheckUserIsManager(currentUser, settlement, settlementApprove);
            settlementMap.IsApproved = CheckUserIsApproved(currentUser, settlement, settlementApprove);
            settlementMap.IsShowBtnDeny = CheckIsShowBtnDeny(currentUser, settlement, settlementApprove);

            return settlementMap;
        }

        public List<ShipmentSettlement> GetListShipmentSettlementBySettlementNo(string settlementNo)
        {
            IQueryable<CsShipmentSurcharge> surcharge = csShipmentSurchargeRepo.Get();
            IQueryable<AcctSettlementPayment> settlement = DataContext.Get();
            IQueryable<OpsTransaction> opsTrans = opsTransactionRepo.Get();
            IQueryable<CsTransactionDetail> csTransD = csTransactionDetailRepo.Get();
            IQueryable<CsTransaction> csTrans = csTransactionRepo.Get();
            IQueryable<CustomsDeclaration> cdNos = customsDeclarationRepo.Get();
            IQueryable<AcctAdvanceRequest> advanceRequests = acctAdvanceRequestRepo.Get();
            IQueryable<AcctAdvancePayment> advances = acctAdvancePaymentRepo.Get(a => a.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);


            AcctSettlementPayment settleCurrent = settlement.Where(x => x.SettlementNo == settlementNo).FirstOrDefault();
            if (settlement == null) return null;
            //Quy đổi tỉ giá theo ngày Request Date, nếu exchange rate của ngày Request date không có giá trị thì lấy excharge rate mới nhất
            List<CatCurrencyExchange> currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settleCurrent.RequestDate.Value.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

            IQueryable<ShipmentSettlement> dataOperation = from sur in surcharge
                                join opst in opsTrans on sur.Hblid equals opst.Hblid
                                join cd in cdNos on opst.Hblid.ToString() equals cd.Hblid into cdNoGroups // list các tờ khai theo job
                                from cdNoGroup in cdNoGroups.DefaultIfEmpty()
                                join settle in settlement on sur.SettlementCode equals settle.SettlementNo into settle2
                                from settle in settle2.DefaultIfEmpty()
                                where sur.SettlementCode == settlementNo
                                select new ShipmentSettlement
                                {
                                    SettlementNo = sur.SettlementCode,
                                    JobId = opst.JobNo,
                                    HBL = opst.Hwbno,
                                    MBL = opst.Mblno,
                                    HblId = opst.Hblid,
                                    CurrencyShipment = settle.SettlementCurrency,
                                    TotalAmount = sur.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settle.SettlementCurrency),
                                    ShipmentId = opst.Id,
                                    Type = "OPS",
                                };

            IQueryable<ShipmentSettlement> dataDocument = from sur in surcharge
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
                                   TotalAmount = sur.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settle.SettlementCurrency),
                                   HblId = cstd.Id,
                                   ShipmentId = cst.Id,
                                   Type = "DOC",
                               };
            IQueryable<ShipmentSettlement> dataQueryUnionService = dataOperation.Union(dataDocument);

            var dataGroups = dataQueryUnionService.ToList()
                                        .GroupBy(x => new { x.SettlementNo, x.JobId, x.HBL, x.MBL, x.CurrencyShipment, x.HblId, x.Type, x.ShipmentId})
                .Select(x => new ShipmentSettlement
            {
                SettlementNo = x.Key.SettlementNo,
                JobId = x.Key.JobId,
                HBL = x.Key.HBL,
                MBL = x.Key.MBL,
                CurrencyShipment = x.Key.CurrencyShipment,
                TotalAmount = x.Sum(t => t.TotalAmount),
                HblId = x.Key.HblId,
                Type = x.Key.Type,
                ShipmentId = x.Key.ShipmentId,
            });

            List<ShipmentSettlement> shipmentSettlement = new List<ShipmentSettlement>();
            foreach (ShipmentSettlement item in dataGroups)
            {
                // Lấy thông tin advance theo group settlement.
                AdvanceInfo advInfo = GetAdvanceInfo(item.SettlementNo, item.MBL, item.HBL, item.CurrencyShipment);

                shipmentSettlement.Add(new ShipmentSettlement
                {
                    SettlementNo = item.SettlementNo,
                    JobId = item.JobId,
                    MBL = item.MBL,
                    HBL = item.HBL,
                    TotalAmount = item.TotalAmount,
                    CurrencyShipment = item.CurrencyShipment,
                    ChargeSettlements = GetChargesSettlementBySettlementNoAndShipment(item.SettlementNo, item.JobId, item.MBL, item.HBL),
                    HblId = item.HblId,
                    ShipmentId = item.ShipmentId,
                    Type = item.Type,

                    AdvanceNo = advInfo.AdvanceNo,
                    AdvanceAmount = advInfo.AdvanceAmount,
                    Balance = item.TotalAmount - advInfo.AdvanceAmount,
                    CustomNo = advInfo.CustomNo
                });
            }

            return shipmentSettlement.OrderByDescending(x => x.JobId).ToList();
        }

        public AdvanceInfo GetAdvanceInfo(string _settlementNo, string _mbl, string _hbl, string _SettleCurrency)
        {
            AdvanceInfo result = new AdvanceInfo();
            string advNo = null, customNo = null;
            IQueryable<CsShipmentSurcharge> surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == _settlementNo);
            var surchargeGrpBy = surcharges.GroupBy(x => new { x.Hblid, x.Mblno, x.Hblno, x.AdvanceNo, x.ClearanceNo }).ToList();

            var surchargeGrp = surchargeGrpBy.Where(x => x.Key.Hblno == _hbl && x.Key.Mblno == _mbl);
            if(surchargeGrp != null && surchargeGrp.Count() > 0)
            {
                 advNo = surchargeGrp?.FirstOrDefault().Key.AdvanceNo;
                 customNo = surchargeGrp?.FirstOrDefault().Key.ClearanceNo;
            }
          

            if (!string.IsNullOrEmpty(advNo))
            {
                var advData = from advP in acctAdvancePaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                              join advR in acctAdvanceRequestRepo.Get() on advP.AdvanceNo equals advR.AdvanceNo
                              where advR.Mbl == _mbl && advR.Hbl == _hbl && advR.AdvanceNo == advNo
                              select new
                              {
                                  AdvAmount = advR.Amount * currencyExchangeService.CurrencyExchangeRateConvert(null, advP.RequestDate, advR.RequestCurrency, _SettleCurrency), // tính theo tỷ giá ngày request adv và currency settlement
                              };
                result.AdvanceNo = advNo;
                result.AdvanceAmount = advData.ToList().Sum(x => x.AdvAmount);
            }

            if (!string.IsNullOrEmpty(customNo))
            {
                result.CustomNo = customNo;
            }

            return result;

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
                                    AdvanceNo = sur.AdvanceNo,
                                    ShipmentId = opst.Id,
                                    TypeService = "OPS"
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
                                   AdvanceNo = sur.AdvanceNo,
                                   ShipmentId = cst.Id,
                                   TypeService = "DOC"
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
            //Quy đổi tỉ giá theo ngày hiện tại, nếu tỉ giá ngày hiện tại không có thì lấy ngày mới nhất
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == DateTime.Now.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

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
            if (criteria.customNos != null)
            {
                var join = from ops in opsTrans
                           join cd in customsDeclarationRepo.Get(x => criteria.customNos.Contains(x.ClearanceNo)) on ops.JobNo equals cd.JobNo
                           select ops;
                opsTrans = join;
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
                                    ChargeCode = cc.Code,
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

            if (criteria.customNos != null)
            {
                return dataOperation.ToList();
            }

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
                                   ChargeCode = cc.Code,
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
                            //&& (string.IsNullOrEmpty(criteria.CustomNo) ? true : x.ClearanceNo == criteria.CustomNo)
                            //&& (string.IsNullOrEmpty(criteria.InvoiceNo) ? true : x.InvoiceNo == criteria.InvoiceNo)
                            //&& (string.IsNullOrEmpty(criteria.ContNo) ? true : x.ContNo == criteria.ContNo)
                            && x.ClearanceNo == criteria.CustomNo
                            && x.InvoiceNo == criteria.InvoiceNo
                            && x.ContNo == criteria.ContNo
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
                            //&& (string.IsNullOrEmpty(criteria.CustomNo) ? true : x.ClearanceNo == criteria.CustomNo)
                            //&& (string.IsNullOrEmpty(criteria.InvoiceNo) ? true : x.InvoiceNo == criteria.InvoiceNo)
                            //&& (string.IsNullOrEmpty(criteria.ContNo) ? true : x.ContNo == criteria.ContNo)
                            && x.ClearanceNo == criteria.CustomNo
                            && x.InvoiceNo == criteria.InvoiceNo
                            && x.ContNo == criteria.ContNo
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
                    var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == model.Settlement.RequestDate.Value.Date).ToList();
                    if (currencyExchange.Count == 0)
                    {
                        var maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                        currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                    }
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
                    return new HandleState("Only allowed to edit the settlement payment status is New or Deny");
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
                                    // Phí Chứng từ cho phép cập nhật lại số HD, Ngày HD, Số SerieNo, Note.
                                    var chargeSettlementCurrentToUpdateCsShipmentSurcharge = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && x.IsFromShipment == true && x.Id == item.Id)?.FirstOrDefault();
                                    item.Notes = chargeSettlementCurrentToUpdateCsShipmentSurcharge.Notes;
                                    item.SeriesNo = chargeSettlementCurrentToUpdateCsShipmentSurcharge.SeriesNo;
                                    item.InvoiceNo = chargeSettlementCurrentToUpdateCsShipmentSurcharge.InvoiceNo;
                                    item.InvoiceDate = chargeSettlementCurrentToUpdateCsShipmentSurcharge.InvoiceDate;

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
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == DateTime.Now.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }
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
            var settleApprove = acctApproveSettlementRepo.Get(x => x.IsDeny == false);

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
                           GW = (opst.SumGrossWeight ?? cstd.GrossWeight),
                           NW = (opst.SumNetWeight ?? cstd.NetWeight),
                           CustomsId = (sur.ClearanceNo == null ? string.Empty : sur.ClearanceNo),
                           PSC = (opst.SumPackages ?? cstd.PackageQty),
                           CBM = (opst.SumCbm ?? cstd.Cbm),
                           HBL = (opst.Hwbno == null ? cstd.Hwbno : opst.Hwbno),
                           MBL = (opst.Mblno == null ? (cst.Mawb ?? string.Empty) : opst.Mblno),
                           StlCSName = string.Empty
                       };
            data = data.OrderByDescending(x => x.JobId);
            var result = new AscSettlementPaymentRequestReportParams();
            result.JobId = data.First().JobId;
            result.AdvDate = data.First().AdvDate;
            result.SettlementNo = data.First().SettlementNo;
            result.Customer = data.First().Customer;
            result.Consignee = data.First().Consignee;
            result.Consigner = data.First().Consigner;
            result.ContainerQty = data.First().ContainerQty;
            result.GW = data.Sum(sum => sum.GW);
            result.NW = data.Sum(sum => sum.NW);
            result.CustomsId = data.First().CustomsId;
            result.PSC = data.Sum(sum => sum.PSC);
            result.CBM = data.Sum(sum => sum.CBM);
            result.HBL = data.First().HBL;
            result.MBL = data.First().MBL;
            result.StlCSName = data.First().StlCSName;
            return result;
        }

        public IQueryable<AscSettlementPaymentRequestReport> GetChargeOfSettlement(string settlementNo, string currency)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var charge = catChargeRepo.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransDe = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();

            var settle = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();

            //Quy đổi tỉ giá theo ngày Request Date, nếu không có thì lấy exchange rate mới nhất
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settle.RequestDate.Value.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

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
                           Debt = sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? true : false,
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
                           MBLNO = (opst.Mblno == null ? (cst.Mawb ?? string.Empty) : opst.Mblno),
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

        public HandleState InsertOrUpdateApprovalSettlement(AcctApproveSettlementModel approve)
        {
            try
            {
                var userCurrent = currentUser.UserID;

                var settlementApprove = mapper.Map<AcctApproveSettlement>(approve);
                var settlementPayment = DataContext.Get(x => x.SettlementNo == approve.SettlementNo).FirstOrDefault();

                if (!string.IsNullOrEmpty(approve.SettlementNo))
                {
                    if (settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                        && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                        && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE
                        && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)
                    {
                        return new HandleState("Awaiting approval");
                    }
                }

                // Check existing Settling Flow
                var settingFlow = userBaseService.GetSettingFlowApproval(typeApproval, settlementPayment.OfficeId);
                if (settingFlow == null)
                {
                    return new HandleState("No setting flow yet");
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        string _leader = null;
                        string _manager = null;
                        string _accountant = null;
                        string _bhHead = null;

                        var isAllLevelAutoOrNone = CheckAllLevelIsAutoOrNone(typeApproval, settlementPayment.OfficeId);
                        if (isAllLevelAutoOrNone)
                        {
                            //Cập nhật Status Approval là Done cho Settlement Payment
                            settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            var hsUpdateAdvancePayment = DataContext.Update(settlementPayment, x => x.Id == settlementPayment.Id, false);
                        }

                        var leaderLevel = LeaderLevel(typeApproval, settlementPayment.GroupId, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
                        var managerLevel = ManagerLevel(typeApproval, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
                        var accountantLevel = AccountantLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);
                        var buHeadLevel = BuHeadLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);

                        var userLeaderOrManager = string.Empty;
                        var mailLeaderOrManager = string.Empty;
                        List<string> mailUsersDeputy = new List<string>();

                        if (leaderLevel.Role == AccountingConstants.ROLE_AUTO || leaderLevel.Role == AccountingConstants.ROLE_APPROVAL)
                        {
                            _leader = leaderLevel.UserId;
                            if (string.IsNullOrEmpty(_leader)) return new HandleState("Not found leader");
                            if (leaderLevel.Role == AccountingConstants.ROLE_AUTO)
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED;
                                settlementApprove.LeaderApr = userCurrent;
                                settlementApprove.LeaderAprDate = DateTime.Now;
                                settlementApprove.LevelApprove = AccountingConstants.LEVEL_LEADER;
                            }
                            if (leaderLevel.Role == AccountingConstants.ROLE_APPROVAL)
                            {
                                userLeaderOrManager = leaderLevel.UserId;
                                mailLeaderOrManager = leaderLevel.EmailUser;
                                mailUsersDeputy = leaderLevel.EmailDeputies;
                            }
                        }
                        else
                        {
                            if (
                                (buHeadLevel.Role == AccountingConstants.ROLE_NONE || buHeadLevel.Role == AccountingConstants.ROLE_AUTO)
                                &&
                                (accountantLevel.Role == AccountingConstants.ROLE_NONE || accountantLevel.Role == AccountingConstants.ROLE_AUTO)
                                &&
                                (managerLevel.Role == AccountingConstants.ROLE_NONE || managerLevel.Role == AccountingConstants.ROLE_AUTO)
                               )
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            }
                        }

                        if (managerLevel.Role == AccountingConstants.ROLE_AUTO || managerLevel.Role == AccountingConstants.ROLE_APPROVAL)
                        {
                            _manager = managerLevel.UserId;
                            if (string.IsNullOrEmpty(_manager)) return new HandleState("Not found manager");
                            if (managerLevel.Role == AccountingConstants.ROLE_AUTO && leaderLevel.Role != AccountingConstants.ROLE_APPROVAL)
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED;
                                settlementApprove.ManagerApr = userCurrent;
                                settlementApprove.ManagerAprDate = DateTime.Now;
                                settlementApprove.LevelApprove = AccountingConstants.LEVEL_MANAGER;
                            }
                            if (managerLevel.Role == AccountingConstants.ROLE_APPROVAL && leaderLevel.Role != AccountingConstants.ROLE_APPROVAL)
                            {
                                userLeaderOrManager = managerLevel.UserId;
                                mailLeaderOrManager = managerLevel.EmailUser;
                                mailUsersDeputy = managerLevel.EmailDeputies;
                            }
                        }
                        else
                        {
                            if (
                                (buHeadLevel.Role == AccountingConstants.ROLE_NONE || buHeadLevel.Role == AccountingConstants.ROLE_AUTO)
                                &&
                                (accountantLevel.Role == AccountingConstants.ROLE_NONE || accountantLevel.Role == AccountingConstants.ROLE_AUTO)
                                &&
                                (leaderLevel.Role == AccountingConstants.ROLE_NONE || leaderLevel.Role == AccountingConstants.ROLE_AUTO)
                               )
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            }
                        }

                        if (accountantLevel.Role == AccountingConstants.ROLE_AUTO || accountantLevel.Role == AccountingConstants.ROLE_APPROVAL)
                        {
                            _accountant = accountantLevel.UserId;
                            if (string.IsNullOrEmpty(_accountant)) return new HandleState("Not found accountant");
                            if (accountantLevel.Role == AccountingConstants.ROLE_AUTO
                                && managerLevel.Role != AccountingConstants.ROLE_APPROVAL
                                && leaderLevel.Role != AccountingConstants.ROLE_APPROVAL)
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                settlementApprove.AccountantApr = userCurrent;
                                settlementApprove.AccountantAprDate = DateTime.Now;
                                settlementApprove.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
                                //if (buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL)
                                //{
                                //    settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                //    settlementApprove.BuheadApr = settlementApprove.AccountantApr = userCurrent;
                                //    settlementApprove.BuheadAprDate = settlementApprove.AccountantAprDate = DateTime.Now;
                                //    settlementApprove.LevelApprove = AccountingConstants.LEVEL_BOD;
                                //}
                            }
                            if (accountantLevel.Role == AccountingConstants.ROLE_APPROVAL
                                && managerLevel.Role != AccountingConstants.ROLE_APPROVAL
                                && leaderLevel.Role != AccountingConstants.ROLE_APPROVAL)
                            {
                                userLeaderOrManager = accountantLevel.UserId;
                                mailLeaderOrManager = accountantLevel.EmailUser;
                                mailUsersDeputy = accountantLevel.EmailDeputies;
                            }
                        }
                        else
                        {
                            if (
                                (buHeadLevel.Role == AccountingConstants.ROLE_NONE || buHeadLevel.Role == AccountingConstants.ROLE_AUTO)
                                &&
                                (managerLevel.Role == AccountingConstants.ROLE_NONE || managerLevel.Role == AccountingConstants.ROLE_AUTO)
                                &&
                                (leaderLevel.Role == AccountingConstants.ROLE_NONE || leaderLevel.Role == AccountingConstants.ROLE_AUTO)
                               )
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            }
                        }

                        if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO || buHeadLevel.Role == AccountingConstants.ROLE_APPROVAL || buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL)
                        {
                            _bhHead = buHeadLevel.UserId;
                            if (string.IsNullOrEmpty(_bhHead)) return new HandleState("Not found BOD");
                            if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO
                                && accountantLevel.Role != AccountingConstants.ROLE_APPROVAL
                                && managerLevel.Role != AccountingConstants.ROLE_APPROVAL
                                && leaderLevel.Role != AccountingConstants.ROLE_APPROVAL)
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                settlementApprove.BuheadApr = userCurrent;
                                settlementApprove.BuheadAprDate = DateTime.Now;
                                settlementApprove.LevelApprove = AccountingConstants.LEVEL_BOD;
                            }
                            if ((buHeadLevel.Role == AccountingConstants.ROLE_APPROVAL || buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL)
                                && accountantLevel.Role != AccountingConstants.ROLE_APPROVAL
                                && managerLevel.Role != AccountingConstants.ROLE_APPROVAL
                                && leaderLevel.Role != AccountingConstants.ROLE_APPROVAL)
                            {
                                userLeaderOrManager = buHeadLevel.UserId;
                                mailLeaderOrManager = buHeadLevel.EmailUser;
                                mailUsersDeputy = buHeadLevel.EmailDeputies;
                            }
                            //if (buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL && (leaderLevel.Role == AccountingConstants.ROLE_NONE || leaderLevel.Role == AccountingConstants.ROLE_AUTO) && (managerLevel.Role == AccountingConstants.ROLE_NONE || managerLevel.Role == AccountingConstants.ROLE_AUTO) && (accountantLevel.Role == AccountingConstants.ROLE_NONE || accountantLevel.Role == AccountingConstants.ROLE_AUTO))
                            //{
                            //    settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            //    settlementApprove.BuheadApr = userCurrent;
                            //    settlementApprove.BuheadAprDate = DateTime.Now;
                            //    settlementApprove.LevelApprove = AccountingConstants.LEVEL_BOD;
                            //    if (leaderLevel.Role != AccountingConstants.ROLE_NONE)
                            //    {
                            //        settlementApprove.LeaderApr = userCurrent;
                            //        settlementApprove.LeaderAprDate = DateTime.Now;
                            //    }
                            //    if (managerLevel.Role != AccountingConstants.ROLE_NONE)
                            //    {
                            //        settlementApprove.ManagerApr = userCurrent;
                            //        settlementApprove.ManagerAprDate = DateTime.Now;
                            //    }
                            //    if (accountantLevel.Role != AccountingConstants.ROLE_NONE)
                            //    {
                            //        settlementApprove.AccountantApr = userCurrent;
                            //        settlementApprove.AccountantAprDate = DateTime.Now;
                            //    }
                            //}
                        }
                        else
                        {
                            if (
                                (accountantLevel.Role == AccountingConstants.ROLE_NONE || accountantLevel.Role == AccountingConstants.ROLE_AUTO)
                                &&
                                (managerLevel.Role == AccountingConstants.ROLE_NONE || managerLevel.Role == AccountingConstants.ROLE_AUTO)
                                &&
                                (leaderLevel.Role == AccountingConstants.ROLE_NONE || leaderLevel.Role == AccountingConstants.ROLE_AUTO)
                               )
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            }
                        }

                        var sendMailApproved = true;
                        var sendMailSuggest = true;
                        if (settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                        {
                            //Send Mail Approved
                            sendMailApproved = SendMailApproved(settlementPayment.SettlementNo, DateTime.Now);
                        }
                        else
                        {
                            //Send Mail Suggest
                            sendMailSuggest = SendMailSuggestApproval(settlementPayment.SettlementNo, userLeaderOrManager, mailLeaderOrManager, mailUsersDeputy);
                        }

                        if (!sendMailSuggest)
                        {
                            return new HandleState("Send mail suggest approval failed");
                        }
                        if (!sendMailApproved)
                        {
                            return new HandleState("Send mail approved approval failed");
                        }

                        var checkExistsApproveBySettlementNo = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementApprove.SettlementNo && x.IsDeny == false).FirstOrDefault();
                        if (checkExistsApproveBySettlementNo == null) //Insert ApproveSettlement
                        {
                            settlementApprove.Id = Guid.NewGuid();
                            settlementApprove.Leader = _leader;
                            settlementApprove.Manager = _manager;
                            settlementApprove.Accountant = _accountant;
                            settlementApprove.Buhead = _bhHead;
                            settlementApprove.UserCreated = settlementApprove.UserModified = userCurrent;
                            settlementApprove.DateCreated = settlementApprove.DateModified = DateTime.Now;
                            settlementApprove.IsDeny = false;
                            var hsAddApprove = acctApproveSettlementRepo.Add(settlementApprove, false);
                        }
                        else //Update ApproveSettlement by Settlement No
                        {
                            checkExistsApproveBySettlementNo.UserModified = userCurrent;
                            checkExistsApproveBySettlementNo.DateModified = DateTime.Now;
                            var hsUpdateApprove = acctApproveSettlementRepo.Update(checkExistsApproveBySettlementNo, x => x.Id == checkExistsApproveBySettlementNo.Id, false);
                        }

                        acctApproveSettlementRepo.SubmitChanges();
                        DataContext.SubmitChanges();
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
                    var settlementPayment = DataContext.Get(x => x.Id == settlementId).FirstOrDefault();

                    if (settlementPayment == null) return new HandleState("Not found settlement payment");

                    var approve = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementPayment.SettlementNo && x.IsDeny == false).FirstOrDefault();
                    if (approve == null)
                    {
                        return new HandleState("Not found settlement payment approval");
                    }

                    var isAllLevelAutoOrNone = CheckAllLevelIsAutoOrNone(typeApproval, settlementPayment.OfficeId);
                    if (isAllLevelAutoOrNone)
                    {
                        //Cập nhật Status Approval là Done cho Settlement Payment
                        settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                    }

                    var leaderLevel = LeaderLevel(typeApproval, settlementPayment.GroupId, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
                    var managerLevel = ManagerLevel(typeApproval, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
                    var accountantLevel = AccountantLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);
                    var buHeadLevel = BuHeadLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);

                    var userApproveNext = string.Empty;
                    var mailUserApproveNext = string.Empty;
                    List<string> mailUsersDeputy = new List<string>();

                    var isDeptAccountant = userBaseService.CheckIsAccountantDept(currentUser.DepartmentId);

                    var isLeader = userBaseService.GetLeaderGroup(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId, currentUser.GroupId).FirstOrDefault() == currentUser.UserID;
                    if (leaderLevel.Role == AccountingConstants.ROLE_APPROVAL
                         &&
                         (
                           (isLeader && currentUser.GroupId != AccountingConstants.SpecialGroup && userCurrent == leaderLevel.UserId)
                           ||
                           leaderLevel.UserDeputies.Contains(userCurrent)
                          )
                        )
                    {
                        if (string.IsNullOrEmpty(approve.LeaderApr))
                        {
                            if (!string.IsNullOrEmpty(approve.Requester))
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED;
                                approve.LeaderApr = userCurrent;
                                approve.LeaderAprDate = DateTime.Now;
                                approve.LevelApprove = AccountingConstants.LEVEL_LEADER;
                                userApproveNext = managerLevel.UserId;
                                mailUserApproveNext = managerLevel.EmailUser;
                                mailUsersDeputy = managerLevel.EmailDeputies;
                                if (managerLevel.Role == AccountingConstants.ROLE_AUTO || managerLevel.Role == AccountingConstants.ROLE_NONE)
                                {
                                    if (managerLevel.Role == AccountingConstants.ROLE_AUTO)
                                    {
                                        settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED;
                                        approve.ManagerApr = managerLevel.UserId;
                                        approve.ManagerAprDate = DateTime.Now;
                                        approve.LevelApprove = AccountingConstants.LEVEL_MANAGER;
                                        userApproveNext = accountantLevel.UserId;
                                        mailUserApproveNext = accountantLevel.EmailUser;
                                        mailUsersDeputy = accountantLevel.EmailDeputies;
                                    }
                                    if (accountantLevel.Role == AccountingConstants.ROLE_AUTO || accountantLevel.Role == AccountingConstants.ROLE_NONE)
                                    {
                                        if (accountantLevel.Role == AccountingConstants.ROLE_AUTO)
                                        {
                                            settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                            approve.AccountantApr = accountantLevel.UserId;
                                            approve.AccountantAprDate = DateTime.Now;
                                            approve.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
                                            userApproveNext = buHeadLevel.UserId;
                                            mailUserApproveNext = buHeadLevel.EmailUser;
                                            mailUsersDeputy = buHeadLevel.EmailDeputies;
                                        }
                                        if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO)
                                        {
                                            settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                            approve.BuheadApr = buHeadLevel.UserId;
                                            approve.BuheadAprDate = DateTime.Now;
                                            approve.LevelApprove = AccountingConstants.LEVEL_BOD;
                                        }
                                        if (buHeadLevel.Role == AccountingConstants.ROLE_NONE)
                                        {
                                            settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                            approve.LevelApprove = AccountingConstants.LEVEL_LEADER;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return new HandleState("Not allow approve");
                            }
                        }
                        else
                        {
                            return new HandleState("Leader approved");
                        }
                    }

                    var isManager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault() == currentUser.UserID;
                    if (managerLevel.Role == AccountingConstants.ROLE_APPROVAL && !isDeptAccountant
                        &&
                        (
                           (isManager && currentUser.GroupId == AccountingConstants.SpecialGroup && userCurrent == managerLevel.UserId)
                           ||
                           managerLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (leaderLevel.Role == AccountingConstants.ROLE_APPROVAL && !string.IsNullOrEmpty(approve.Leader) && string.IsNullOrEmpty(approve.LeaderApr))
                        {
                            return new HandleState("Leader has not approved it yet");
                        }
                        if (string.IsNullOrEmpty(approve.ManagerApr))
                        {
                            if ((!string.IsNullOrEmpty(approve.Leader) && !string.IsNullOrEmpty(approve.LeaderApr)) || string.IsNullOrEmpty(approve.Leader) || leaderLevel.Role == AccountingConstants.ROLE_NONE || leaderLevel.Role == AccountingConstants.ROLE_AUTO)
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED;
                                approve.ManagerApr = userCurrent;
                                approve.ManagerAprDate = DateTime.Now;
                                approve.LevelApprove = AccountingConstants.LEVEL_MANAGER;
                                userApproveNext = accountantLevel.UserId;
                                mailUserApproveNext = accountantLevel.EmailUser;
                                mailUsersDeputy = accountantLevel.EmailDeputies;
                                if (accountantLevel.Role == AccountingConstants.ROLE_AUTO || accountantLevel.Role == AccountingConstants.ROLE_NONE)
                                {
                                    if (accountantLevel.Role == AccountingConstants.ROLE_AUTO)
                                    {
                                        settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                        approve.AccountantApr = accountantLevel.UserId;
                                        approve.AccountantAprDate = DateTime.Now;
                                        approve.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
                                        userApproveNext = buHeadLevel.UserId;
                                        mailUserApproveNext = buHeadLevel.EmailUser;
                                        mailUsersDeputy = buHeadLevel.EmailDeputies;
                                    }
                                    if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO)
                                    {
                                        settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                        approve.BuheadApr = buHeadLevel.UserId;
                                        approve.BuheadAprDate = DateTime.Now;
                                        approve.LevelApprove = AccountingConstants.LEVEL_BOD;
                                    }
                                    if (buHeadLevel.Role == AccountingConstants.ROLE_NONE)
                                    {
                                        settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                        approve.LevelApprove = AccountingConstants.LEVEL_MANAGER;
                                    }
                                }
                            }
                            else
                            {
                                return new HandleState("Not allow approve");
                            }
                        }
                        else
                        {
                            return new HandleState("Manager approved");
                        }
                    }

                    var isAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
                    if (accountantLevel.Role == AccountingConstants.ROLE_APPROVAL && isDeptAccountant
                        &&
                        (
                           (isAccountant && currentUser.GroupId == AccountingConstants.SpecialGroup && userCurrent == accountantLevel.UserId)
                           ||
                           accountantLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (leaderLevel.Role == AccountingConstants.ROLE_APPROVAL && !string.IsNullOrEmpty(approve.Leader) && string.IsNullOrEmpty(approve.LeaderApr))
                        {
                            return new HandleState("Leader has not approved it yet");
                        }
                        if (managerLevel.Role == AccountingConstants.ROLE_APPROVAL && !string.IsNullOrEmpty(approve.Manager) && string.IsNullOrEmpty(approve.ManagerApr))
                        {
                            return new HandleState("The manager has not approved it yet");
                        }
                        if (string.IsNullOrEmpty(approve.AccountantApr))
                        {
                            if ((!string.IsNullOrEmpty(approve.Manager) && !string.IsNullOrEmpty(approve.ManagerApr)) || string.IsNullOrEmpty(approve.Manager) || managerLevel.Role == AccountingConstants.ROLE_NONE || managerLevel.Role == AccountingConstants.ROLE_AUTO)
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                approve.AccountantApr = userCurrent;
                                approve.AccountantAprDate = DateTime.Now;
                                approve.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
                                userApproveNext = buHeadLevel.UserId;
                                mailUserApproveNext = buHeadLevel.EmailUser;
                                mailUsersDeputy = buHeadLevel.EmailDeputies;
                                if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO)
                                {
                                    settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                    approve.BuheadApr = buHeadLevel.UserId;
                                    approve.BuheadAprDate = DateTime.Now;
                                    approve.LevelApprove = AccountingConstants.LEVEL_BOD;
                                }
                                if (buHeadLevel.Role == AccountingConstants.ROLE_NONE)
                                {
                                    settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                    approve.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
                                }
                            }
                            else
                            {
                                return new HandleState("Not allow approve");
                            }
                        }
                        else
                        {
                            return new HandleState("Accountant approved");
                        }
                    }

                    var isBuHead = userBaseService.GetBUHead(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
                    var isBod = userBaseService.CheckIsBOD(currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);
                    if ((buHeadLevel.Role == AccountingConstants.ROLE_APPROVAL || buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL) && isBod
                        &&
                        (
                          (isBuHead && currentUser.GroupId == AccountingConstants.SpecialGroup && userCurrent == buHeadLevel.UserId)
                          ||
                          buHeadLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (buHeadLevel.Role != AccountingConstants.ROLE_SPECIAL)
                        {
                            if (leaderLevel.Role == AccountingConstants.ROLE_APPROVAL && !string.IsNullOrEmpty(approve.Leader) && string.IsNullOrEmpty(approve.LeaderApr))
                            {
                                return new HandleState("Leader has not approved it yet");
                            }
                            if (managerLevel.Role == AccountingConstants.ROLE_APPROVAL && !string.IsNullOrEmpty(approve.Manager) && string.IsNullOrEmpty(approve.ManagerApr))
                            {
                                return new HandleState("The manager has not approved it yet");
                            }
                            if (accountantLevel.Role == AccountingConstants.ROLE_APPROVAL && !string.IsNullOrEmpty(approve.Accountant) && string.IsNullOrEmpty(approve.AccountantApr))
                            {
                                return new HandleState("The accountant has not approved it yet");
                            }
                        }
                        else //buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL
                        {
                            if (!string.IsNullOrEmpty(approve.Leader) && string.IsNullOrEmpty(approve.LeaderApr))
                            {
                                approve.LeaderApr = userCurrent;
                                approve.LeaderAprDate = DateTime.Now;
                            }
                            if (!string.IsNullOrEmpty(approve.Manager) && string.IsNullOrEmpty(approve.ManagerApr))
                            {
                                approve.ManagerApr = userCurrent;
                                approve.ManagerAprDate = DateTime.Now;
                            }
                            if (!string.IsNullOrEmpty(approve.Accountant) && string.IsNullOrEmpty(approve.AccountantApr))
                            {
                                approve.AccountantApr = userCurrent;
                                approve.AccountantAprDate = DateTime.Now;
                            }
                        }
                        if (string.IsNullOrEmpty(approve.BuheadApr))
                        {
                            if ((!string.IsNullOrEmpty(approve.Accountant) && !string.IsNullOrEmpty(approve.AccountantApr)) || string.IsNullOrEmpty(approve.Accountant) || accountantLevel.Role == AccountingConstants.ROLE_NONE || accountantLevel.Role == AccountingConstants.ROLE_AUTO || buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL)
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                approve.BuheadApr = userCurrent;
                                approve.BuheadAprDate = DateTime.Now;
                                approve.LevelApprove = AccountingConstants.LEVEL_BOD;
                            }
                            else
                            {
                                return new HandleState("Not allow approve");
                            }
                        }
                        else
                        {
                            return new HandleState("BOD approved");
                        }
                    }

                    var sendMailApproved = true;
                    var sendMailSuggest = true;

                    if (settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                    {
                        //Send Mail Approved
                        sendMailApproved = SendMailApproved(settlementPayment.SettlementNo, DateTime.Now);
                    }
                    else
                    {
                        //Send Mail Suggest
                        sendMailSuggest = SendMailSuggestApproval(settlementPayment.SettlementNo, userApproveNext, mailUserApproveNext, mailUsersDeputy);
                    }

                    if (!sendMailSuggest)
                    {
                        return new HandleState("Send mail suggest approval failed");
                    }
                    if (!sendMailApproved)
                    {
                        return new HandleState("Send mail approved approval failed");
                    }

                    settlementPayment.UserModified = approve.UserModified = userCurrent;
                    settlementPayment.DatetimeModified = approve.DateModified = DateTime.Now;

                    var hsUpdateSettlementPayment = DataContext.Update(settlementPayment, x => x.Id == settlementPayment.Id, false);
                    var hsUpdateApprove = acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id, false);

                    acctApproveSettlementRepo.SubmitChanges();
                    DataContext.SubmitChanges();
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

        public HandleState DeniedApprove(Guid settlementId, string comment)
        {
            var userCurrent = currentUser.UserID;
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var settlementPayment = DataContext.Get(x => x.Id == settlementId).FirstOrDefault();
                    if (settlementPayment == null) return new HandleState("Not found settlement payment");

                    var approve = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementPayment.SettlementNo && x.IsDeny == false).FirstOrDefault();
                    if (approve == null)
                    {
                        return new HandleState("Not found settlement payment approval");
                    }

                    if (settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DENIED)
                    {
                        return new HandleState("Settlement payment has been denied");
                    }

                    var leaderLevel = LeaderLevel(typeApproval, settlementPayment.GroupId, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
                    var managerLevel = ManagerLevel(typeApproval, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
                    var accountantLevel = AccountantLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);
                    var buHeadLevel = BuHeadLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);

                    var isApprover = false;
                    var isDeptAccountant = userBaseService.CheckIsAccountantDept(currentUser.DepartmentId);

                    var isLeader = userBaseService.GetLeaderGroup(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId, currentUser.GroupId).FirstOrDefault() == currentUser.UserID;
                    if (leaderLevel.Role == AccountingConstants.ROLE_APPROVAL
                        &&
                        (
                            (isLeader && currentUser.GroupId != AccountingConstants.SpecialGroup && userCurrent == leaderLevel.UserId)
                            ||
                            leaderLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED
                            || settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                            || settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                        {
                            return new HandleState("Not allow deny. Settlement payment has been approved");
                        }

                        if (string.IsNullOrEmpty(approve.Requester))
                        {
                            return new HandleState("The requester has not send the request yet");
                        }
                        approve.LeaderApr = userCurrent;
                        approve.LeaderAprDate = DateTime.Now;
                        approve.LevelApprove = AccountingConstants.LEVEL_LEADER;

                        isApprover = true;
                    }

                    var isManager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault() == currentUser.UserID;
                    if (managerLevel.Role == AccountingConstants.ROLE_APPROVAL && !isDeptAccountant
                        &&
                        (
                            (isManager && currentUser.GroupId == AccountingConstants.SpecialGroup && userCurrent == managerLevel.UserId)
                            ||
                            managerLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                            || settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                        {
                            return new HandleState("Not allow deny. Settlement payment has been approved");
                        }

                        if (leaderLevel.Role == AccountingConstants.ROLE_APPROVAL && string.IsNullOrEmpty(approve.LeaderApr))
                        {
                            return new HandleState("Leader not approve");
                        }
                        approve.ManagerApr = userCurrent;
                        approve.ManagerAprDate = DateTime.Now;
                        approve.LevelApprove = AccountingConstants.LEVEL_MANAGER;

                        isApprover = true;
                    }

                    var isAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
                    if (accountantLevel.Role == AccountingConstants.ROLE_APPROVAL && isDeptAccountant
                        &&
                        (
                            (isAccountant && currentUser.GroupId == AccountingConstants.SpecialGroup && userCurrent == accountantLevel.UserId)
                            ||
                            accountantLevel.UserDeputies.Contains(userCurrent)
                         )
                        )
                    {
                        if (settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                        {
                            return new HandleState("Not allow deny. Settlement payment has been approved");
                        }

                        if (managerLevel.Role == AccountingConstants.ROLE_APPROVAL && string.IsNullOrEmpty(approve.ManagerApr))
                        {
                            return new HandleState("The manager has not approved it yet");
                        }
                        approve.AccountantApr = userCurrent;
                        approve.AccountantAprDate = DateTime.Now;
                        approve.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;

                        isApprover = true;
                    }

                    var isBuHead = userBaseService.GetBUHead(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
                    var isBod = userBaseService.CheckIsBOD(currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);
                    if ((buHeadLevel.Role == AccountingConstants.ROLE_APPROVAL || buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL) && isBod
                        &&
                        (
                          (isBuHead && currentUser.GroupId == AccountingConstants.SpecialGroup && userCurrent == buHeadLevel.UserId)
                          ||
                          buHeadLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (buHeadLevel.Role == AccountingConstants.ROLE_APPROVAL && accountantLevel.Role == AccountingConstants.ROLE_APPROVAL && string.IsNullOrEmpty(approve.AccountantApr))
                        {
                            return new HandleState("The accountant has not approved it yet");
                        }
                        approve.BuheadApr = userCurrent;
                        approve.BuheadAprDate = DateTime.Now;
                        approve.LevelApprove = AccountingConstants.LEVEL_BOD;

                        isApprover = true;
                    }

                    if (!isApprover)
                    {
                        return new HandleState("Not allow deny. You are not in the approval process.");
                    }

                    var sendMailDeny = SendMailDeniedApproval(settlementPayment.SettlementNo, comment, DateTime.Now);
                    if (!sendMailDeny)
                    {
                        return new HandleState("Send mail denied failed");
                    }

                    settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DENIED;
                    approve.IsDeny = true;
                    approve.Comment = comment;
                    settlementPayment.UserModified = approve.UserModified = userCurrent;
                    settlementPayment.DatetimeModified = approve.DateModified = DateTime.Now;

                    var hsUpdateSettlementPayment = DataContext.Update(settlementPayment, x => x.Id == settlementPayment.Id, false);
                    var hsUpdateApprove = acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id, false);

                    acctApproveSettlementRepo.SubmitChanges();
                    DataContext.SubmitChanges();
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

        public AcctApproveSettlementModel GetInfoApproveSettlementBySettlementNo(string settlementNo)
        {
            var userCurrent = currentUser.UserID;
            var approveSettlement = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementNo && x.IsDeny == false).FirstOrDefault();
            var aprSettlementMap = new AcctApproveSettlementModel();

            if (approveSettlement != null)
            {
                aprSettlementMap = mapper.Map<AcctApproveSettlementModel>(approveSettlement);
                aprSettlementMap.RequesterName = userBaseService.GetEmployeeByUserId(aprSettlementMap.Requester)?.EmployeeNameVn;
                aprSettlementMap.LeaderName = userBaseService.GetEmployeeByUserId(aprSettlementMap.Leader)?.EmployeeNameVn;
                aprSettlementMap.ManagerName = userBaseService.GetEmployeeByUserId(aprSettlementMap.Manager)?.EmployeeNameVn;
                aprSettlementMap.AccountantName = userBaseService.GetEmployeeByUserId(aprSettlementMap.Accountant)?.EmployeeNameVn;
                aprSettlementMap.BUHeadName = userBaseService.GetEmployeeByUserId(aprSettlementMap.Buhead)?.EmployeeNameVn;
                aprSettlementMap.StatusApproval = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault()?.StatusApproval;
                aprSettlementMap.NumOfDeny = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementNo && x.IsDeny == true && !(x.Comment ?? string.Empty).Contains("RECALL")).Select(s => s.Id).Count();
                aprSettlementMap.IsShowLeader = !string.IsNullOrEmpty(approveSettlement.Leader);
                aprSettlementMap.IsShowManager = !string.IsNullOrEmpty(approveSettlement.Manager);
                aprSettlementMap.IsShowAccountant = !string.IsNullOrEmpty(approveSettlement.Accountant);
                aprSettlementMap.IsShowBuHead = !string.IsNullOrEmpty(approveSettlement.Buhead);
            }
            else
            {
                aprSettlementMap.StatusApproval = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault()?.StatusApproval;
                aprSettlementMap.NumOfDeny = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementNo && x.IsDeny == true && !(x.Comment ?? string.Empty).Contains("RECALL")).Select(s => s.Id).Count();
            }
            return aprSettlementMap;
        }

        public AcctApproveSettlementModel GetInfoApproveSettlementNoCheckBySettlementNo(string settlementNo)
        {
            var settleApprove = acctApproveSettlementRepo.Get(x => x.IsDeny == false && x.SettlementNo == settlementNo);

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

        public List<DeniedInfoResult> GetHistoryDeniedSettlement(string settlementNo)
        {
            var approves = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementNo && x.IsDeny == true && !(x.Comment ?? string.Empty).Contains("RECALL")).OrderByDescending(x => x.DateCreated).ToList();
            var data = new List<DeniedInfoResult>();
            int i = 1;
            foreach (var approve in approves)
            {
                var item = new DeniedInfoResult();
                item.No = i;
                item.NameAndTimeDeny = userBaseService.GetEmployeeByUserId(approve.UserModified)?.EmployeeNameVn + "\r\n" + approve.DateModified?.ToString("dd/MM/yyyy HH:mm");
                item.LevelApprove = approve.LevelApprove;
                item.Comment = approve.Comment;
                data.Add(item);
                i = i + 1;
            }
            return data;
        }
        #endregion --- APPROVAL SETTLEMENT PAYMENT ---

        #region -- Info Level Approve --        
        public bool CheckAllLevelIsAutoOrNone(string type, Guid? officeId)
        {
            var roleLeader = userBaseService.GetRoleByLevel(AccountingConstants.LEVEL_LEADER, type, officeId);
            var roleManager = userBaseService.GetRoleByLevel(AccountingConstants.LEVEL_MANAGER, type, officeId);
            var roleAccountant = userBaseService.GetRoleByLevel(AccountingConstants.LEVEL_ACCOUNTANT, type, officeId);
            var roleBuHead = userBaseService.GetRoleByLevel(AccountingConstants.LEVEL_BOD, type, officeId);
            if (
                (roleLeader == AccountingConstants.ROLE_AUTO && roleManager == AccountingConstants.ROLE_AUTO && roleAccountant == AccountingConstants.ROLE_AUTO && roleBuHead == AccountingConstants.ROLE_AUTO)
                ||
                (roleLeader == AccountingConstants.ROLE_NONE && roleManager == AccountingConstants.ROLE_NONE && roleAccountant == AccountingConstants.ROLE_NONE && roleBuHead == AccountingConstants.ROLE_NONE)
               )
            {
                return true;
            }
            return false;
        }

        public InfoLevelApproveResult LeaderLevel(string type, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleLeader = userBaseService.GetRoleByLevel(AccountingConstants.LEVEL_LEADER, type, officeId);
            var userLeader = userBaseService.GetLeaderGroup(companyId, officeId, departmentId, groupId).FirstOrDefault();
            var employeeIdOfLeader = userBaseService.GetEmployeeIdOfUser(userLeader);

            var userDeputies = userBaseService.GetUsersDeputyByCondition(type, userLeader, groupId, departmentId, officeId, companyId);
            var emailDeputies = userBaseService.GetEmailUsersDeputyByCondition(type, userLeader, groupId, departmentId, officeId, companyId);

            result.LevelApprove = AccountingConstants.LEVEL_LEADER;
            result.Role = roleLeader;
            result.UserId = userLeader;
            result.UserDeputies = userDeputies;
            result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfLeader)?.Email;
            result.EmailDeputies = emailDeputies;

            return result;
        }

        public InfoLevelApproveResult ManagerLevel(string type, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleManager = userBaseService.GetRoleByLevel(AccountingConstants.LEVEL_MANAGER, type, officeId);
            var userManager = userBaseService.GetDeptManager(companyId, officeId, departmentId).FirstOrDefault();
            var employeeIdOfManager = userBaseService.GetEmployeeIdOfUser(userManager);

            var userDeputies = userBaseService.GetUsersDeputyByCondition(type, userManager, null, departmentId, officeId, companyId);
            var emailDeputies = userBaseService.GetEmailUsersDeputyByCondition(type, userManager, null, departmentId, officeId, companyId);

            result.LevelApprove = AccountingConstants.LEVEL_MANAGER;
            result.Role = roleManager;
            result.UserId = userManager;
            result.UserDeputies = userDeputies;
            result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfManager)?.Email;
            result.EmailDeputies = emailDeputies;

            return result;
        }

        public InfoLevelApproveResult AccountantLevel(string type, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleAccountant = userBaseService.GetRoleByLevel(AccountingConstants.LEVEL_ACCOUNTANT, type, officeId);
            var userAccountant = userBaseService.GetAccoutantManager(companyId, officeId).FirstOrDefault();
            var employeeIdOfAccountant = userBaseService.GetEmployeeIdOfUser(userAccountant);

            var userDeputies = userBaseService.GetUsersDeputyByCondition(type, userAccountant, null, null, officeId, companyId);
            var emailDeputies = userBaseService.GetEmailUsersDeputyByCondition(type, userAccountant, null, null, officeId, companyId);

            result.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
            result.Role = roleAccountant;
            result.UserId = userAccountant;
            result.UserDeputies = userDeputies;
            result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfAccountant)?.Email;
            result.EmailDeputies = emailDeputies;

            return result;
        }

        public InfoLevelApproveResult BuHeadLevel(string type, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleBuHead = userBaseService.GetRoleByLevel(AccountingConstants.LEVEL_BOD, type, officeId);
            var userBuHead = userBaseService.GetBUHead(companyId, officeId).FirstOrDefault();
            var employeeIdOfBuHead = userBaseService.GetEmployeeIdOfUser(userBuHead);

            var userDeputies = userBaseService.GetUsersDeputyByCondition(type, userBuHead, null, null, officeId, companyId);
            var emailDeputies = userBaseService.GetEmailUsersDeputyByCondition(type, userBuHead, null, null, officeId, companyId);

            result.LevelApprove = AccountingConstants.LEVEL_BOD;
            result.Role = roleBuHead;
            result.UserId = userBuHead;
            result.UserDeputies = userDeputies;
            result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfBuHead)?.Email;
            result.EmailDeputies = emailDeputies;

            return result;
        }
        #endregion -- Info Level Approve --

        #region -- Check Exist, Check Is Manager, Is Approved --
        public HandleState CheckExistSettingFlow(string type, Guid? officeId)
        {
            // Check existing Settling Flow
            var settingFlow = userBaseService.GetSettingFlowApproval(type, officeId);
            if (settingFlow == null)
            {
                return new HandleState("No setting flow yet");
            }
            return new HandleState();
        }

        public HandleState CheckExistUserApproval(string type, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var infoLevelApprove = LeaderLevel(type, groupId, departmentId, officeId, companyId);
            if (infoLevelApprove.Role == AccountingConstants.ROLE_AUTO || infoLevelApprove.Role == AccountingConstants.ROLE_APPROVAL)
            {
                if (infoLevelApprove.LevelApprove == AccountingConstants.LEVEL_LEADER)
                {
                    if (string.IsNullOrEmpty(infoLevelApprove.UserId)) return new HandleState("Not found leader");
                    if (string.IsNullOrEmpty(infoLevelApprove.EmailUser)) return new HandleState("Not found email of leader");
                }
            }

            var managerLevel = ManagerLevel(type, departmentId, officeId, companyId);
            if (managerLevel.Role == AccountingConstants.ROLE_AUTO || managerLevel.Role == AccountingConstants.ROLE_APPROVAL)
            {
                if (string.IsNullOrEmpty(managerLevel.UserId)) return new HandleState("Not found manager");
                if (string.IsNullOrEmpty(managerLevel.EmailUser)) return new HandleState("Not found email of manager");
            }

            var accountantLevel = AccountantLevel(type, officeId, companyId);
            if (accountantLevel.Role == AccountingConstants.ROLE_AUTO || accountantLevel.Role == AccountingConstants.ROLE_APPROVAL)
            {
                if (string.IsNullOrEmpty(accountantLevel.UserId)) return new HandleState("Not found accountant");
                if (string.IsNullOrEmpty(accountantLevel.EmailUser)) return new HandleState("Not found email of accountant");
            }

            var buHeadLevel = BuHeadLevel(type, officeId, companyId);
            if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO || buHeadLevel.Role == AccountingConstants.ROLE_APPROVAL)
            {
                if (string.IsNullOrEmpty(buHeadLevel.UserId)) return new HandleState("Not found BOD");
                if (string.IsNullOrEmpty(buHeadLevel.EmailUser)) return new HandleState("Not found email of BOD");
            }
            return new HandleState();
        }

        public bool CheckUserIsApproved(ICurrentUser userCurrent, AcctSettlementPayment settlementPayment, AcctApproveSettlement approve)
        {
            var isApproved = false;
            if (approve == null) return true;
            var leaderLevel = LeaderLevel(typeApproval, settlementPayment.GroupId, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
            var managerLevel = ManagerLevel(typeApproval, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
            var accountantLevel = AccountantLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);
            var buHeadLevel = BuHeadLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);

            var isLeader = userBaseService.GetLeaderGroup(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId, currentUser.GroupId).FirstOrDefault() == currentUser.UserID;
            var isManager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault() == currentUser.UserID;
            var isAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
            var isBuHead = userBaseService.GetBUHead(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;

            var isDeptAccountant = userBaseService.CheckIsAccountantDept(currentUser.DepartmentId);
            var isBod = userBaseService.CheckIsBOD(currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);

            if (!isLeader && userCurrent.GroupId != AccountingConstants.SpecialGroup && userCurrent.UserID == approve.Requester) //Requester
            {
                isApproved = true;
                if (approve.RequesterAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (
                        (isLeader && userCurrent.GroupId != AccountingConstants.SpecialGroup && (userCurrent.UserID == approve.Leader || userCurrent.UserID == approve.LeaderApr))
                      ||
                        leaderLevel.UserDeputies.Contains(userCurrent.UserID)
                    ) //Leader
            {
                isApproved = true;
                if (string.IsNullOrEmpty(approve.LeaderApr) && leaderLevel.Role != AccountingConstants.ROLE_NONE)
                {
                    isApproved = false;
                }
            }
            else if (
                        (isManager && !isDeptAccountant && userCurrent.GroupId == AccountingConstants.SpecialGroup && (userCurrent.UserID == approve.Manager || userCurrent.UserID == approve.ManagerApr))
                      ||
                        managerLevel.UserDeputies.Contains(currentUser.UserID)
                    ) //Dept Manager
            {
                isApproved = true;
                if (!string.IsNullOrEmpty(approve.Leader) && string.IsNullOrEmpty(approve.LeaderApr))
                {
                    return true;
                }
                if (string.IsNullOrEmpty(approve.ManagerApr) && managerLevel.Role != AccountingConstants.ROLE_NONE)
                {
                    isApproved = false;
                }

            }
            else if (
                        (isAccountant && isDeptAccountant && userCurrent.GroupId == AccountingConstants.SpecialGroup && (userCurrent.UserID == approve.Accountant || userCurrent.UserID == approve.AccountantApr))
                      ||
                        accountantLevel.UserDeputies.Contains(currentUser.UserID)
                    ) //Accountant Manager
            {
                isApproved = true;
                if (!string.IsNullOrEmpty(approve.Leader) && string.IsNullOrEmpty(approve.LeaderApr))
                {
                    return true;
                }
                if (!string.IsNullOrEmpty(approve.Manager) && string.IsNullOrEmpty(approve.ManagerApr))
                {
                    return true;
                }
                if (string.IsNullOrEmpty(approve.AccountantApr) && accountantLevel.Role != AccountingConstants.ROLE_NONE)
                {
                    isApproved = false;
                }
            }
            else if (
                        (userCurrent.GroupId == AccountingConstants.SpecialGroup && isBuHead && isBod && (userCurrent.UserID == approve.Buhead || userCurrent.UserID == approve.BuheadApr))
                      ||
                        buHeadLevel.UserDeputies.Contains(userCurrent.UserID)
                    ) //BUHead
            {
                isApproved = true;
                if (buHeadLevel.Role != AccountingConstants.ROLE_SPECIAL)
                {
                    if (!string.IsNullOrEmpty(approve.Leader) && string.IsNullOrEmpty(approve.LeaderApr))
                    {
                        return true;
                    }
                    if (!string.IsNullOrEmpty(approve.Manager) && string.IsNullOrEmpty(approve.ManagerApr))
                    {
                        return true;
                    }
                    if (!string.IsNullOrEmpty(approve.Accountant) && string.IsNullOrEmpty(approve.AccountantApr))
                    {
                        return true;
                    }
                }
                if (
                    (string.IsNullOrEmpty(approve.BuheadApr) && buHeadLevel.Role != AccountingConstants.ROLE_NONE)
                    ||
                    (buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL && !string.IsNullOrEmpty(approve.Requester))
                   )
                {
                    isApproved = false;
                }
            }
            else
            {
                //Đây là trường hợp các User không thuộc Approve
                isApproved = true;
            }
            return isApproved;
        }

        public bool CheckUserIsManager(ICurrentUser userCurrent, AcctSettlementPayment settlementPayment, AcctApproveSettlement approve)
        {
            var isManagerOrLeader = false;

            var leaderLevel = LeaderLevel(typeApproval, settlementPayment.GroupId, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
            var managerLevel = ManagerLevel(typeApproval, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
            var accountantLevel = AccountantLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);
            var buHeadLevel = BuHeadLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);

            var isLeader = userBaseService.GetLeaderGroup(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId, currentUser.GroupId).FirstOrDefault() == currentUser.UserID;
            var isManager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault() == currentUser.UserID;
            var isAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
            var isBuHead = userBaseService.GetBUHead(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;

            if (approve == null)
            {
                if ((isLeader && userCurrent.GroupId != AccountingConstants.SpecialGroup) || leaderLevel.UserDeputies.Contains(userCurrent.UserID)) //Leader
                {
                    isManagerOrLeader = true;
                }
                else if ((isManager && userCurrent.GroupId == AccountingConstants.SpecialGroup) || managerLevel.UserDeputies.Contains(currentUser.UserID)) //Dept Manager
                {
                    isManagerOrLeader = true;
                }
                else if ((isAccountant && userCurrent.GroupId == AccountingConstants.SpecialGroup) || accountantLevel.UserDeputies.Contains(currentUser.UserID)) //Accountant Manager
                {
                    isManagerOrLeader = true;
                }
                else if ((isBuHead && userCurrent.GroupId == AccountingConstants.SpecialGroup) || buHeadLevel.UserDeputies.Contains(userCurrent.UserID)) //BUHead
                {
                    isManagerOrLeader = true;
                }
            }
            else
            {
                if (
                     (isLeader && userCurrent.GroupId != AccountingConstants.SpecialGroup && (userCurrent.UserID == approve.Leader || userCurrent.UserID == approve.LeaderApr))
                     ||
                     leaderLevel.UserDeputies.Contains(userCurrent.UserID)
                   ) //Leader
                {
                    isManagerOrLeader = true;
                }
                else if (
                            (isManager && userCurrent.GroupId == AccountingConstants.SpecialGroup && (userCurrent.UserID == approve.Manager || userCurrent.UserID == approve.ManagerApr))
                          ||
                            managerLevel.UserDeputies.Contains(currentUser.UserID)
                        ) //Dept Manager
                {
                    isManagerOrLeader = true;
                }
                else if (
                            (userCurrent.GroupId == AccountingConstants.SpecialGroup && isAccountant && (userCurrent.UserID == approve.Accountant || userCurrent.UserID == approve.AccountantApr))
                          ||
                            accountantLevel.UserDeputies.Contains(currentUser.UserID)
                        ) //Accountant Manager
                {
                    isManagerOrLeader = true;
                }
                else if (
                            (userCurrent.GroupId == AccountingConstants.SpecialGroup && isBuHead && (userCurrent.UserID == approve.Buhead || userCurrent.UserID == approve.BuheadApr))
                          ||
                            buHeadLevel.UserDeputies.Contains(userCurrent.UserID)
                        ) //BUHead
                {
                    isManagerOrLeader = true;
                }
            }
            return isManagerOrLeader;
        }

        public bool CheckIsShowBtnDeny(ICurrentUser userCurrent, AcctSettlementPayment settlementPayment, AcctApproveSettlement approve)
        {
            if (approve == null) return false;

            var leaderLevel = LeaderLevel(typeApproval, settlementPayment.GroupId, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
            var managerLevel = ManagerLevel(typeApproval, settlementPayment.DepartmentId, settlementPayment.OfficeId, settlementPayment.CompanyId);
            var accountantLevel = AccountantLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);
            var buHeadLevel = BuHeadLevel(typeApproval, settlementPayment.OfficeId, settlementPayment.CompanyId);

            var isLeader = userBaseService.GetLeaderGroup(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId, currentUser.GroupId).FirstOrDefault() == currentUser.UserID;
            var isManager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault() == currentUser.UserID;
            var isAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
            var isBuHead = userBaseService.GetBUHead(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;

            var isDeptAccountant = userBaseService.CheckIsAccountantDept(currentUser.DepartmentId);
            var isBod = userBaseService.CheckIsBOD(currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);

            var isShowBtnDeny = false;

            if (
                 ((isLeader && userCurrent.GroupId != AccountingConstants.SpecialGroup && (userCurrent.UserID == approve.Leader || userCurrent.UserID == approve.LeaderApr))
                 ||
                 leaderLevel.UserDeputies.Contains(userCurrent.UserID))
               ) //Leader
            {
                if (settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL || settlementPayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED)
                {
                    isShowBtnDeny = true;
                }
                else
                {
                    isShowBtnDeny = false;
                }
            }
            else if (
                        ((isManager && !isDeptAccountant && userCurrent.GroupId == AccountingConstants.SpecialGroup && (userCurrent.UserID == approve.Manager || userCurrent.UserID == approve.ManagerApr))
                      ||
                        managerLevel.UserDeputies.Contains(currentUser.UserID))

                    ) //Dept Manager
            {
                isShowBtnDeny = false;
                if (!string.IsNullOrEmpty(approve.Manager)
                    && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                    && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                    && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                    && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE)
                {
                    isShowBtnDeny = true;
                }

                if (!string.IsNullOrEmpty(approve.Leader) && string.IsNullOrEmpty(approve.LeaderApr))
                {
                    return false;
                }
            }
            else if (
                       ((isAccountant && isDeptAccountant && userCurrent.GroupId == AccountingConstants.SpecialGroup && (userCurrent.UserID == approve.Accountant || userCurrent.UserID == approve.AccountantApr))
                      ||
                        accountantLevel.UserDeputies.Contains(currentUser.UserID))
                    ) //Accountant Manager
            {
                isShowBtnDeny = false;
                if (!string.IsNullOrEmpty(approve.Accountant)
                    && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                    && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                    && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE)
                {
                    isShowBtnDeny = true;
                }

                if (!string.IsNullOrEmpty(approve.Leader) && string.IsNullOrEmpty(approve.LeaderApr))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(approve.Manager) && string.IsNullOrEmpty(approve.ManagerApr))
                {
                    return false;
                }
            }
            else if (isBod
                &&
                (
                  (isBuHead && currentUser.GroupId == AccountingConstants.SpecialGroup && userCurrent.UserID == buHeadLevel.UserId)
                  ||
                  buHeadLevel.UserDeputies.Contains(userCurrent.UserID)
                )
               )
            {
                isShowBtnDeny = false;
                if (settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW 
                    && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED 
                    && settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE)
                {
                    isShowBtnDeny = true;
                }

                if (buHeadLevel.Role != AccountingConstants.ROLE_SPECIAL)
                {
                    if (!string.IsNullOrEmpty(approve.Leader) && string.IsNullOrEmpty(approve.LeaderApr))
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(approve.Manager) && string.IsNullOrEmpty(approve.ManagerApr))
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(approve.Accountant) && string.IsNullOrEmpty(approve.AccountantApr))
                    {
                        return false;
                    }
                }
            }
            else
            {
                isShowBtnDeny = false;
            }
            return isShowBtnDeny;
        }
        #endregion -- Check Exist, Check Is Manager, Is Approved --

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
            var acctApprove = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementNo && x.IsDeny == false).FirstOrDefault();
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

            if (settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
            {
                result = new HandleState("Settlement payment approved");
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
                var IsDenyManager = userBaseService.CheckDeputyManagerByUser(_userCurrent.DepartmentId, _userCurrent.UserID);
                //Kiểm tra user có phải là dept manager hoặc có phải là user được ủy quyền duyệt (Manager Dept) hay không
                if ((_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId) == false
                    && _userCurrent.UserID == managerOfUserRequester)
                        || IsDenyManager
                   )
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
                var IsDenyAccountant = userBaseService.CheckDeputyAccountantByUser(_userCurrent.DepartmentId, _userCurrent.UserID);
                //Kiểm tra user có phải là Accountant Manager hoặc có phải là user được ủy quyền duyệt hay không
                if ((_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId)
                    && _userCurrent.UserID == accountantOfUser)
                    || IsDenyAccountant)
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
                var IsDenyManager = userBaseService.CheckDeputyManagerByUser(_userCurrent.DepartmentId, _userCurrent.UserID);
                //Kiểm tra user có phải là dept manager hoặc có phải là user được ủy quyền duyệt (Manager Dept) hay không
                if ((_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId) == false
                    && _userCurrent.UserID == managerOfUserRequester)
                        || IsDenyManager)
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
                var IsDenyAccountant = userBaseService.CheckDeputyAccountantByUser(_userCurrent.DepartmentId, _userCurrent.UserID);
                //Kiểm tra user có phải là Accountant Manager hoặc có phải là user được ủy quyền duyệt (Accoutant) hay không
                if ((_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId)
                    && _userCurrent.UserID == accountantOfUser)
                    || IsDenyAccountant)
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
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settlement.RequestDate.Value.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

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
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p> <i> <b>Dear Mr/Mrs [UserName],</b> </i></p><p>You have new Settlement Payment Approval Request from <b>[RequesterName]</b> as below info:</p><p> <i>Anh/ Chị có một yêu cầu duyệt thanh toán từ <b>[RequesterName]</b> với thông tin như sau: </i></p><ul><li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li><li>Settlement Amount/ <i>Số tiền thanh toán</i> : <b>[TotalAmount] [CurrencySettlement]</b></li><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li></ul><p>You click here to check more detail and approve: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]/approve' target='_blank'>Detail Payment Request</a> </span></p><p> <i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]/approve' target='_blank'>Chi tiết phiếu đề nghị thanh toán</a> </span> </i></p><p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='[logoEFMS]'/></p></div>");
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
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");
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
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settlement.RequestDate.Value.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

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
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p> <i> <b>Dear Mr/Mrs [RequesterName],</b> </i></p><p>You have an Settlement Payment is approved at <b>[ApprovedDate]</b> as below info:</p><p> <i>Anh/ Chị có một đề nghị thanh toán đã được phê duyệt vào lúc <b>[ApprovedDate]</b> với thông tin như sau: </i></p><ul><li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li><li>Settlement Amount/ <i>Số tiền thanh toán</i> : <b>[TotalAmount] [CurrencySettlement]</b></li><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li></ul><p>You can click here to check more detail: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Detail Payment Request</a> </span></p><p> <i>Anh/ Chị có thể chọn vào đây để biết thêm thông tin chi tiết: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Chi tiết đề nghị thanh toán</a> </span> </i></p><p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='[logoEFMS]'/></p></div>");
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
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");
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
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settlement.RequestDate.Value.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

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
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p> <i> <b>Dear Mr/Mrs [RequesterName],</b> </i></p><p>You have an Settlement Payment is denied at <b>[DeniedDate]</b> by as below info:</p><p> <i>Anh/ Chị có một yêu cầu đề nghị thanh toán đã bị từ chối vào lúc <b>[DeniedDate]</b> by với thông tin như sau: </i></p><ul><li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li><li>Settlement Amount/ <i>Số tiền tạm ứng</i> : <b>[TotalAmount] [CurrencySettlement]</b></li><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li><li>Comment/ <i>Lý do từ chối</i> : <b>[Comment]</b></li></ul><p>You click here to recheck detail: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Detail Payment Request</a> </span></p><p> <i>Anh/ Chị chọn vào đây để kiểm tra lại thông tin chi tiết: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Chi tiết đề nghị thanh toán</a> </span> </i></p><p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='[logoEFMS]'/></p></div>");
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
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");
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
            var IsDenyManage = userBaseService.CheckDeputyManagerByUser(userCurrent.DepartmentId, userCurrent.UserID);
            var IsDenyAccoutant = userBaseService.CheckDeputyAccountantByUser(userCurrent.DepartmentId, userCurrent.UserID);

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
            else if (
                (userCurrent.GroupId == AccountingConstants.SpecialGroup
                && userBaseService.CheckIsAccountantDept(userCurrent.DepartmentId) == false
                && (userCurrent.UserID == approveSettlement.Manager
                    || userCurrent.UserID == approveSettlement.ManagerApr))

                    || IsDenyManage) //Dept Manager
            {
                isApproved = true;
                var isDeptWaitingApprove = DataContext.Get(x => x.SettlementNo == approveSettlement.SettlementNo && (x.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && x.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED)).Any();
                if (string.IsNullOrEmpty(approveSettlement.ManagerApr) && approveSettlement.ManagerAprDate == null && isDeptWaitingApprove)
                {
                    isApproved = false;
                }
            }
            else if (
                (userCurrent.GroupId == AccountingConstants.SpecialGroup
                && userBaseService.CheckIsAccountantDept(userCurrent.DepartmentId)
                && (userCurrent.UserID == approveSettlement.Accountant
                    || userCurrent.UserID == approveSettlement.AccountantApr))

                    || IsDenyAccoutant)//Accountant Manager
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
                        settle.StatusApproval = AccountingConstants.STATUS_APPROVAL_DENIED;
                        settle.UserModified = currentUser.UserName;
                        settle.DatetimeModified = DateTime.Now;
                        var log = item.SettlementNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + "admin";
                        settle.LockedLog = item.LockedLog + log + ";";
                        var hs = DataContext.Update(settle, x => x.Id == item.Id);
                        var approveSettles = acctApproveSettlementRepo.Get(x => x.SettlementNo == item.SettlementNo);
                        foreach (var approve in approveSettles)
                        {
                            approve.IsDeny = true;
                            approve.UserModified = currentUser.UserID;
                            approve.DateModified = DateTime.Now;
                            acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id);
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
            var _settlementApprove = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlementPayment.SettlementNo && x.IsDeny == false).FirstOrDefault();
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

            //Quy đổi tỉ giá theo ngày Request Date, nếu exchange rate của ngày Request date không có giá trị thì lấy excharge rate mới nhất
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settlementPayment.RequestDate.Value.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

            var surChargeBySettleCode = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementPayment.SettlementNo);

            var houseBillIds = surChargeBySettleCode.Select(s => new { hblId = s.Hblid, customNo = s.ClearanceNo }).Distinct();
            foreach (var houseBillId in houseBillIds)
            {
                var shipmentSettlement = new InfoShipmentSettlementExport();

                #region -- CHANRGE AND ADVANCE OF SETTELEMENT --
                var _shipmentCharges = GetChargeOfShipmentSettlementExport(houseBillId.hblId, settlementPayment.SettlementCurrency, surChargeBySettleCode, currencyExchange);
                var _infoAdvanceExports = GetAdvanceOfShipmentSettlementExport(houseBillId.hblId, settlementPayment.SettlementCurrency, surChargeBySettleCode, currencyExchange);
                shipmentSettlement.ShipmentCharges = _shipmentCharges;
                shipmentSettlement.InfoAdvanceExports = _infoAdvanceExports;
                #endregion -- CHANRGE AND ADVANCE OF SETTELEMENT --

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

                    listData.Add(shipmentSettlement);
                }
                else
                {
                    var tranDetail = csTransactionDetailRepo.Get(x => x.Id == houseBillId.hblId).FirstOrDefault();
                    if (tranDetail != null)
                    {
                        shipmentSettlement.JobNo = csTransactionRepo.Get(x => x.Id == tranDetail.JobId).FirstOrDefault()?.JobNo;
                        shipmentSettlement.CustomNo = string.Empty; //Hàng Documentation không có CustomNo
                        shipmentSettlement.HBL = tranDetail.Hwbno;
                        shipmentSettlement.MBL = csTransactionRepo.Get(x => x.Id == tranDetail.JobId).FirstOrDefault()?.Mawb;
                        shipmentSettlement.Customer = catPartnerRepo.Get(x => x.Id == tranDetail.CustomerId).FirstOrDefault()?.PartnerNameVn;
                        shipmentSettlement.Shipper = catPartnerRepo.Get(x => x.Id == tranDetail.ShipperId).FirstOrDefault()?.PartnerNameVn;
                        shipmentSettlement.Consignee = catPartnerRepo.Get(x => x.Id == tranDetail.ConsigneeId).FirstOrDefault()?.PartnerNameVn;
                        shipmentSettlement.Container = tranDetail.PackageContainer;
                        shipmentSettlement.Cw = tranDetail.ChargeWeight;
                        shipmentSettlement.Pcs = tranDetail.PackageQty;
                        shipmentSettlement.Cbm = tranDetail.Cbm;
                        
                        listData.Add(shipmentSettlement);
                    }
                }
            }
            return listData;
        }

        private List<InfoShipmentChargeSettlementExport> GetChargeOfShipmentSettlementExport(Guid hblId, string settlementCurrency, IQueryable<CsShipmentSurcharge> surChargeBySettleCode, List<CatCurrencyExchange> currencyExchange)
        {
            var shipmentSettlement = new InfoShipmentSettlementExport();
            var listCharge = new List<InfoShipmentChargeSettlementExport>();
            var surChargeByHblId = surChargeBySettleCode.Where(x => x.Hblid == hblId);
            foreach (var sur in surChargeByHblId)
            {
                var infoShipmentCharge = new InfoShipmentChargeSettlementExport();
                infoShipmentCharge.ChargeName = catChargeRepo.Get(x => x.Id == sur.ChargeId).FirstOrDefault()?.ChargeNameEn;
                //Quy đổi theo currency của Settlement
                infoShipmentCharge.ChargeAmount = sur.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settlementCurrency);
                infoShipmentCharge.InvoiceNo = sur.InvoiceNo;
                infoShipmentCharge.ChargeNote = sur.Notes;
                string _chargeType = string.Empty;
                if (sur.Type == AccountingConstants.TYPE_CHARGE_OBH)
                {
                    _chargeType = AccountingConstants.TYPE_CHARGE_OBH;
                }
                else if (!string.IsNullOrEmpty(sur.InvoiceNo) && sur.Type != AccountingConstants.TYPE_CHARGE_OBH)
                {
                    _chargeType = "INVOICE";
                }
                else
                {
                    _chargeType = "NO_INVOICE";
                }
                infoShipmentCharge.ChargeType = _chargeType;

                listCharge.Add(infoShipmentCharge);
            }            
            return listCharge;
        }

        private List<InfoAdvanceExport> GetAdvanceOfShipmentSettlementExport(Guid hblId, string settlementCurrency, IQueryable<CsShipmentSurcharge> surChargeBySettleCode, List<CatCurrencyExchange> currencyExchange)
        {
            var listAdvance = new List<InfoAdvanceExport>();
            // Gom surcharge theo AdvanceNo & HBLID
            var groupAdvanceNoAndHblID = surChargeBySettleCode.GroupBy(g => new { g.AdvanceNo, g.Hblid }).ToList();
            foreach (var item in groupAdvanceNoAndHblID)
            {
                //Advance Payment có Status Approve là Done
                var advanceIsDone = acctAdvancePaymentRepo.Get(x => x.AdvanceNo == item.Key.AdvanceNo
                && x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE).FirstOrDefault();
                if (advanceIsDone != null)
                {
                    var advance = new InfoAdvanceExport();
                    var advanceAmountByHbl = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceIsDone.AdvanceNo && x.Hblid == item.Key.Hblid)
                        .Select(s => s.Amount * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, s.RequestCurrency, settlementCurrency)).Sum();
                    advance.AdvanceAmount = advanceAmountByHbl;
                    advance.RequestDate = advanceIsDone.RequestDate;
                    listAdvance.Add(advance);
                }
            }
            return listAdvance;
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

        public HandleState RecallRequest(Guid settlementId)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    AcctSettlementPayment settlement = DataContext.Get(x => x.Id == settlementId).FirstOrDefault();
                    if (settlement == null)
                    {
                        return new HandleState("Not found Settlement Payment");
                    }
                    if (settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_DENIED
                           || settlement.StatusApproval == AccountingConstants.STATUS_APPROVAL_NEW)
                    {
                        return new HandleState("Settlement payment not yet send the request");
                    }
                    if (settlement.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                            && settlement.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                            && settlement.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)
                    {
                        return new HandleState("Settlement payment approving");
                    }

                    AcctApproveSettlement approve = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo && x.IsDeny == false).FirstOrDefault();
                    //Update Approve Setttlement.
                    if (approve != null)
                    {
                        approve.UserModified = currentUser.UserID;
                        approve.DateModified = DateTime.Now;
                        approve.Comment = "RECALL BY " + currentUser.UserName;
                        approve.IsDeny = true;

                        HandleState hsUpdateApproveSettlement = acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id);
                        if (!hsUpdateApproveSettlement.Success)
                        {
                            return new HandleState("Cannot Update Approve Settlement");
                        }
                        settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_NEW;
                        settlement.UserModified = currentUser.UserID;
                        settlement.DatetimeModified = DateTime.Now;

                        HandleState hsUpdateSettlementPayment = DataContext.Update(settlement, x => x.Id == settlement.Id);

                        trans.Commit();
                    }

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

        public bool CheckIsLockedShipment(string jobNo)
        {
            var isLocked = false;
            var opsJobs = opsTransactionRepo.Get(x => x.JobNo == jobNo && x.IsLocked == true).FirstOrDefault();
            if (opsJobs != null)
            {
                isLocked = true;
            }
            else
            {
                var docJobs = csTransactionRepo.Get(x => x.JobNo == jobNo && x.IsLocked == true).FirstOrDefault();
                if (docJobs != null)
                {
                    isLocked = true;
                }
            }
            return isLocked;
        }
    }
}

