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
using eFMS.API.Common.Helpers;
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
        readonly IContextBase<SysSentEmailHistory> sentEmailHistoryRepo;
        readonly IContextBase<SysOffice> sysOfficeRepo;
        readonly IAcctAdvancePaymentService acctAdvancePaymentService;
        readonly ICurrencyExchangeService currencyExchangeService;
        readonly IUserBaseService userBaseService;
        private string typeApproval = "Settlement";
        private decimal _decimalNumber = Constants.DecimalNumber;

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
            IContextBase<SysSentEmailHistory> sentEmailHistory,
            IContextBase<SysOffice> sysOffice,
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
            sentEmailHistoryRepo = sentEmailHistory;
            sysOfficeRepo = sysOffice;
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
                  || userBaseService.CheckIsUserDeputy(typeApproval, currentUser.UserID, x.settlementPaymentApr.Leader, x.settlementPayment.GroupId, x.settlementPayment.DepartmentId, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
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
                  || userBaseService.CheckIsUserDeputy(typeApproval, currentUser.UserID, x.settlementPaymentApr.Manager, null, x.settlementPayment.DepartmentId, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
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
                  || userBaseService.CheckIsUserDeputy(typeApproval, currentUser.UserID, x.settlementPaymentApr.Accountant, null, null, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
                  || userBaseService.CheckIsAccountantByOfficeDept(currentUser.OfficeID, currentUser.DepartmentId)
                  )
                && x.settlementPayment.OfficeId == currentUser.OfficeID
                && x.settlementPayment.CompanyId == currentUser.CompanyID
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                && (x.settlementPayment.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.settlementPayment.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) // ACCOUTANT AND DEPUTY OF ACCOUNTANT
                ||
                (x.settlementPaymentApr != null && (x.settlementPaymentApr.Buhead == currentUser.UserID
                  || x.settlementPaymentApr.BuheadApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(typeApproval, currentUser.UserID, x.settlementPaymentApr.Buhead ?? null, null, null, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
                  )
                && x.settlementPayment.OfficeId == currentUser.OfficeID
                && x.settlementPayment.CompanyId == currentUser.CompanyID
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                && (x.settlementPayment.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.settlementPayment.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) //BOD AND DEPUTY OF BOD   
                ||
                (
                 userBaseService.CheckIsUserAdmin(currentUser.UserID, currentUser.OfficeID, currentUser.CompanyID, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId) // Is User Admin
                 &&
                 (x.settlementPayment.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.settlementPayment.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) //[CR: 09/01/2021]
            ).Select(s => s.settlementPayment);
            return result;
        }

        private IQueryable<AcctSettlementPayment> QueryWithShipment(IQueryable<AcctSettlementPayment> settlementPayments, AcctSettlementPaymentCriteria criteria)
        {
            var surcharge = csShipmentSurchargeRepo.Get();
            var opst = opsTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            var custom = customsDeclarationRepo.Get();
            var advPayment = acctAdvancePaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);
            List<string> refNo = new List<string>();
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                refNo = (from set in settlementPayments
                         join sur in surcharge on set.SettlementNo equals sur.SettlementCode into grpSur
                         from sur in grpSur.DefaultIfEmpty()
                         join ops in opst on sur.Hblid equals ops.Hblid into grpOps
                         from ops in grpOps.DefaultIfEmpty()
                         join cus in custom on ops.JobNo equals cus.JobNo into grpCus
                         from cus in grpCus.DefaultIfEmpty()
                         join adv in advPayment on sur.AdvanceNo equals adv.AdvanceNo into grpAdv
                         from adv in grpAdv.DefaultIfEmpty()
                         where
                         (
                              criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ?
                              (
                                  (
                                         (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(set.SettlementNo, StringComparer.OrdinalIgnoreCase) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(sur.Hblno, StringComparer.OrdinalIgnoreCase) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(sur.Mblno, StringComparer.OrdinalIgnoreCase) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(sur.JobNo, StringComparer.OrdinalIgnoreCase) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(cus.ClearanceNo, StringComparer.OrdinalIgnoreCase) : true)
                                      || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(sur.AdvanceNo, StringComparer.OrdinalIgnoreCase) : true)
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
            IQueryable<CatPartner> partners = catPartnerRepo.Get();

            var data = from settlePayment in settlementPayments
                       join p in partners on settlePayment.Payee equals p.Id into partnerGrps
                       from partnerGrp in partnerGrps.DefaultIfEmpty()
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

                           // CR 14484
                           VoucherDate = settlePayment.VoucherDate,
                           VoucherNo = settlePayment.VoucherNo,
                           LastSyncDate = settlePayment.LastSyncDate,
                           SyncStatus = settlePayment.SyncStatus,
                           ReasonReject = settlePayment.ReasonReject,
                           PayeeName = partnerGrp.ShortName
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
                if (settlement == null) return new HandleState((object)"Not found Settlement Payment");
                if (!settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_NEW)
                    && !settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED))
                {
                    return new HandleState((object)"Not allow delete. Settlements are awaiting approval.");
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        //Phí chừng từ (cập nhật lại SettlementCode, AdvanceNo bằng null)
                        var surchargeShipment = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo && x.IsFromShipment == true).ToList();
                        if (surchargeShipment != null && surchargeShipment.Count > 0)
                        {
                            foreach (var item in surchargeShipment)
                            {
                                //Cập nhật status payment of Advance Request = NotSettled (Nếu có)
                                var advanceRequest = acctAdvanceRequestRepo.Get(x => x.Hblid == item.Hblid && x.AdvanceNo == item.AdvanceNo && x.StatusPayment != AccountingConstants.STATUS_PAYMENT_NOTSETTLED).FirstOrDefault();
                                if (advanceRequest != null)
                                {
                                    advanceRequest.StatusPayment = AccountingConstants.STATUS_PAYMENT_NOTSETTLED;
                                    advanceRequest.DatetimeModified = DateTime.Now;
                                    advanceRequest.UserModified = userCurrenct;
                                    var hsUpdateAdvRequest = acctAdvanceRequestRepo.Update(advanceRequest, x => x.Id == advanceRequest.Id);
                                }

                                item.SettlementCode = null;
                                item.AdvanceNo = null;
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
                        return new HandleState((object)ex.Message);
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
                                                           join adv in advanceRequests on sur.AdvanceNo equals adv.AdvanceNo into advGrps
                                                           from advGrp in advGrps.DefaultIfEmpty()
                                                           where sur.SettlementCode == settlementNo
                                                           select new ShipmentSettlement
                                                           {
                                                               SettlementNo = sur.SettlementCode,
                                                               JobId = opst.JobNo,
                                                               HBL = opst.Hwbno,
                                                               MBL = opst.Mblno,
                                                               HblId = opst.Hblid,
                                                               CurrencyShipment = settle.SettlementCurrency,
                                                               // TotalAmount = sur.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settle.SettlementCurrency),
                                                               ShipmentId = opst.Id,
                                                               Type = "OPS",
                                                               AdvanceNo = advGrp.AdvanceNo
                                                           };

            IQueryable<ShipmentSettlement> dataDocument = from sur in surcharge
                                                          join cstd in csTransD on sur.Hblid equals cstd.Id
                                                          join cst in csTrans on cstd.JobId equals cst.Id into cst2
                                                          from cst in cst2.DefaultIfEmpty()
                                                          join settle in settlement on sur.SettlementCode equals settle.SettlementNo into settle2
                                                          from settle in settle2.DefaultIfEmpty()
                                                          join adv in advanceRequests on sur.AdvanceNo equals adv.AdvanceNo into advGrps
                                                          from advGrp in advGrps.DefaultIfEmpty()
                                                          where sur.SettlementCode == settlementNo
                                                          select new ShipmentSettlement
                                                          {
                                                              SettlementNo = sur.SettlementCode,
                                                              JobId = cst.JobNo,
                                                              HBL = cstd.Hwbno,
                                                              MBL = cst.Mawb,
                                                              CurrencyShipment = settle.SettlementCurrency,
                                                              // TotalAmount = sur.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, sur.CurrencyId, settle.SettlementCurrency),
                                                              HblId = cstd.Id,
                                                              ShipmentId = cst.Id,
                                                              Type = "DOC",
                                                              AdvanceNo = advGrp.AdvanceNo
                                                          };
            IQueryable<ShipmentSettlement> dataQueryUnionService = dataOperation.Union(dataDocument);

            var dataGroups = dataQueryUnionService.ToList()
                                        .GroupBy(x => new { x.SettlementNo, x.JobId, x.HBL, x.MBL, x.CurrencyShipment, x.HblId, x.Type, x.ShipmentId, x.AdvanceNo })
                .Select(x => new ShipmentSettlement
                {
                    SettlementNo = x.Key.SettlementNo,
                    JobId = x.Key.JobId,
                    HBL = x.Key.HBL,
                    MBL = x.Key.MBL,
                    CurrencyShipment = x.Key.CurrencyShipment,
                    //TotalAmount = x.Sum(t => t.TotalAmount),
                    HblId = x.Key.HblId,
                    Type = x.Key.Type,
                    ShipmentId = x.Key.ShipmentId,
                    AdvanceNo = x.Key.AdvanceNo
                });

            List<ShipmentSettlement> shipmentSettlement = new List<ShipmentSettlement>();
            foreach (ShipmentSettlement item in dataGroups)
            {
                // Lấy thông tin advance theo group settlement.
                AdvanceInfo advInfo = GetAdvanceInfo(item.SettlementNo, item.MBL, item.HblId, item.CurrencyShipment, item.AdvanceNo, currencyExchange);

                int roundDecimal = 0;
                if (item.CurrencyShipment != AccountingConstants.CURRENCY_LOCAL)
                {
                    roundDecimal = 3;
                }

                shipmentSettlement.Add(new ShipmentSettlement
                {
                    SettlementNo = item.SettlementNo,
                    JobId = item.JobId,
                    MBL = item.MBL,
                    HBL = item.HBL,
                    // TotalAmount = item.TotalAmount,
                    CurrencyShipment = item.CurrencyShipment,
                    ChargeSettlements = GetChargesSettlementBySettlementNoAndShipment(item.SettlementNo, item.JobId, item.MBL, item.HBL, item.AdvanceNo),
                    HblId = item.HblId,
                    ShipmentId = item.ShipmentId,
                    Type = item.Type,

                    TotalAmount = advInfo.TotalAmount ?? 0,
                    AdvanceNo = advInfo.AdvanceNo,
                    AdvanceAmount = advInfo.AdvanceAmount,
                    Balance = NumberHelper.RoundNumber((advInfo.TotalAmount - advInfo.AdvanceAmount) ?? 0, roundDecimal),
                    CustomNo = advInfo.CustomNo
                });
            }

            return shipmentSettlement.OrderByDescending(x => x.JobId).ToList();
        }

        public AdvanceInfo GetAdvanceInfo(string _settlementNo, string _mbl, Guid _hbl, string _settleCurrency, string _advanceNo, List<CatCurrencyExchange> currencyExchange)
        {
            AdvanceInfo result = new AdvanceInfo();
            string advNo = null, customNo = null;
            IQueryable<CsShipmentSurcharge> surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == _settlementNo);
            var surchargeGrpBy = surcharges.GroupBy(x => new { x.Hblid, x.Mblno, x.Hblno, x.AdvanceNo, x.ClearanceNo }).ToList();

            var surchargeGrp = surchargeGrpBy.Where(x => x.Key.Hblid == _hbl && x.Key.Mblno == _mbl);
            if (surchargeGrp != null && surchargeGrp.Count() > 0)
            {
                var advDataMatch = surchargeGrp.Where(x => x.Key.AdvanceNo == _advanceNo);
                advNo = advDataMatch?.FirstOrDefault()?.Key?.AdvanceNo;
                customNo = surchargeGrp?.FirstOrDefault()?.Key?.ClearanceNo;
            }

            // Trường hợp settle cho 1 phiếu advance
            if (!string.IsNullOrEmpty(advNo))
            {
                var advData = from advP in acctAdvancePaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                              join advR in acctAdvanceRequestRepo.Get() on advP.AdvanceNo equals advR.AdvanceNo
                              where advR.Mbl == _mbl && advR.Hblid == _hbl && advR.AdvanceNo == advNo
                              select new
                              {
                                  AdvAmount = advR.Amount * currencyExchangeService.CurrencyExchangeRateConvert(null, advP.RequestDate, advR.RequestCurrency, _settleCurrency), // tính theo tỷ giá ngày request adv và currency settlement
                              };
                result.AdvanceNo = advNo;
                result.AdvanceAmount = advData.ToList().Sum(x => x.AdvAmount);


                // Tính total amount của settlement theo adv đó.
                IQueryable<CsShipmentSurcharge> surChargeToCalculateAmount = csShipmentSurchargeRepo.Get(x => x.AdvanceNo == advNo && x.Mblno == _mbl && x.Hblid == _hbl);
                result.TotalAmount = surChargeToCalculateAmount.Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, _settleCurrency));

            }
            else
            {
                if (surchargeGrp != null && surchargeGrp.Count() > 0)
                {
                    result.TotalAmount = surchargeGrp?.FirstOrDefault().Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, _settleCurrency));
                }
                else
                {
                    result.TotalAmount = surcharges.Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, _settleCurrency));
                }
            }


            if (!string.IsNullOrEmpty(customNo))
            {
                result.CustomNo = customNo;
            }

            return result;

        }

        public List<ShipmentChargeSettlement> GetChargesSettlementBySettlementNoAndShipment(string settlementNo, string JobId, string MBL, string HBL, string AdvNo)
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
                                     && sur.AdvanceNo == AdvNo
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
                                    AdvanceNo = AdvNo
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
                                    && sur.AdvanceNo == AdvNo
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
                                   AdvanceNo = AdvNo
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
                opsTrans = opsTrans.Where(x => criteria.jobIds.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase));
                csTrans = csTrans.Where(x => criteria.jobIds.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase));
            }
            if (criteria.mbls != null)
            {
                opsTrans = opsTrans.Where(x => criteria.mbls.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase));
                csTrans = csTrans.Where(x => criteria.mbls.Contains(x.Mawb, StringComparer.OrdinalIgnoreCase));
            }
            if (criteria.hbls != null)
            {
                opsTrans = opsTrans.Where(x => criteria.hbls.Contains(x.Hwbno, StringComparer.OrdinalIgnoreCase));
                csTransD = csTransD.Where(x => criteria.hbls.Contains(x.Hwbno, StringComparer.OrdinalIgnoreCase));
            }
            if (criteria.customNos != null)
            {
                var join = from ops in opsTrans
                           join cd in customsDeclarationRepo.Get(x => criteria.customNos.Contains(x.ClearanceNo, StringComparer.OrdinalIgnoreCase)) on ops.JobNo equals cd.JobNo
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
        public ResultModel CheckDuplicateShipmentSettlement(CheckDuplicateShipmentSettlementCriteria criteria, out List<DuplicateShipmentSettlementResultModel> modelResult)
        {
            var result = new ResultModel();
            modelResult = new List<DuplicateShipmentSettlementResultModel>();
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
                            && x.Notes == criteria.Notes

                    );

                    var isExists = surChargeExists.Select(s => s.Id).Any();
                    result.Status = isExists;
                    if (isExists)
                    {
                        var charge = catChargeRepo.Get();
                        var data = from sur in surChargeExists
                                   join chg in charge on sur.ChargeId equals chg.Id
                                   select new { criteria.JobNo, criteria.HBLNo, criteria.MBLNo, ChargeName = chg.ChargeNameEn, sur.SettlementCode, sur.ChargeId };
                        string msg = string.Join("<br/>", data.ToList()
                            .Select(s => !string.IsNullOrEmpty(s.JobNo)
                            && !string.IsNullOrEmpty(s.HBLNo)
                            && !string.IsNullOrEmpty(s.MBLNo)
                            ? string.Format(@"Shipment: [{0}-{1}-{2}] Charge [{3}] has already existed in settlement: {4}", s.JobNo, s.HBLNo, s.MBLNo, s.ChargeName, s.SettlementCode)
                            : string.Format(@"Charge [{0}] has already existed in settlement: {1}.", s.ChargeName, s.SettlementCode)));
                        result.Message = msg;

                        modelResult = data.Select(x => new DuplicateShipmentSettlementResultModel
                        {
                            JobNo = x.JobNo,
                            MBLNo = x.MBLNo,
                            HBLNo = x.HBLNo,
                            ChargeId = x.ChargeId
                        }).ToList();
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
                            && x.Notes == criteria.Notes
                    );

                    var isExists = surChargeExists.Select(s => s.Id).Any();
                    result.Status = isExists;
                    if (isExists)
                    {
                        var charge = catChargeRepo.Get();
                        var data = from sur in surChargeExists
                                   join chg in charge on sur.ChargeId equals chg.Id
                                   select new { criteria.JobNo, criteria.HBLNo, criteria.MBLNo, ChargeName = chg.ChargeNameEn, sur.SettlementCode, sur.ChargeId };
                        string msg = string.Join("<br/>", data.ToList()
                            .Select(s => !string.IsNullOrEmpty(s.JobNo)
                            && !string.IsNullOrEmpty(s.HBLNo)
                            && !string.IsNullOrEmpty(s.MBLNo)
                            ? string.Format(@"Shipment: [{0}-{1}-{2}] Charge [{3}] has already existed in settlement: {4}", s.JobNo, s.HBLNo, s.MBLNo, s.ChargeName, s.SettlementCode)
                            : string.Format(@"Charge [{0}] has already existed in settlement: {1}.", s.ChargeName, s.SettlementCode)));
                        result.Message = msg;

                        modelResult = data.Select(x => new DuplicateShipmentSettlementResultModel
                        {
                            JobNo = x.JobNo,
                            MBLNo = x.MBLNo,
                            HBLNo = x.HBLNo,
                            ChargeId = x.ChargeId
                        }).ToList();
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
                settlement.GroupId = currentUser.GroupId;
                settlement.DepartmentId = currentUser.DepartmentId;
                settlement.OfficeId = currentUser.OfficeID;
                settlement.CompanyId = currentUser.CompanyID;

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        decimal _totalAmount = 0;
                        //Lấy các phí chứng từ IsFromShipment = true
                        var chargeShipment = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && x.IsFromShipment == true).Select(s => s.Id).ToList();
                        if (chargeShipment.Count > 0)
                        {
                            var listChargeShipment = csShipmentSurchargeRepo.Get(x => chargeShipment.Contains(x.Id)).ToList();
                            foreach (var charge in listChargeShipment)
                            {
                                // Phí Chứng từ cho phép cập nhật lại số HD, Ngày HD, Số SerieNo, Note.
                                var chargeSettlementCurrentToAddCsShipmentSurcharge = model.ShipmentCharge.First(x => x.Id == charge.Id);
                                if (chargeSettlementCurrentToAddCsShipmentSurcharge != null)
                                {
                                    charge.Notes = chargeSettlementCurrentToAddCsShipmentSurcharge.Notes;
                                    charge.SeriesNo = chargeSettlementCurrentToAddCsShipmentSurcharge.SeriesNo;
                                    charge.InvoiceNo = chargeSettlementCurrentToAddCsShipmentSurcharge.InvoiceNo;
                                    charge.InvoiceDate = chargeSettlementCurrentToAddCsShipmentSurcharge.InvoiceDate;
                                }

                                charge.SettlementCode = settlement.SettlementNo;
                                charge.UserModified = userCurrent;
                                charge.DatetimeModified = DateTime.Now;
                                
                                _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, settlement.SettlementCurrency);

                                csShipmentSurchargeRepo.Update(charge, x => x.Id == charge.Id);
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

                            foreach (var charge in listChargeSceneAdd)
                            {
                                charge.Id = Guid.NewGuid();
                                charge.SettlementCode = settlement.SettlementNo;
                                charge.DatetimeCreated = charge.DatetimeModified = DateTime.Now;
                                charge.UserCreated = charge.UserModified = userCurrent;
                                charge.ExchangeDate = DateTime.Now;

                                #region -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                                var amountOriginal = currencyExchangeService.CalculatorAmountAccountingByCurrency(charge, charge.CurrencyId);
                                charge.NetAmount = amountOriginal.NetAmount; //Thành tiền trước thuế (Original)
                                charge.Total = amountOriginal.NetAmount + amountOriginal.VatAmount; //Thành tiền sau thuế (Original)
                                charge.FinalExchangeRate = amountOriginal.ExchangeRate; //Tỉ giá so với Local

                                if (charge.CurrencyId == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    charge.AmountVnd = amountOriginal.NetAmount;
                                    charge.VatAmountVnd = amountOriginal.VatAmount;
                                }
                                else
                                {
                                    var amountLocal = currencyExchangeService.CalculatorAmountAccountingByCurrency(charge, AccountingConstants.CURRENCY_LOCAL);
                                    charge.AmountVnd = amountLocal.NetAmount; //Thành tiền trước thuế (Local)
                                    charge.VatAmountVnd = amountLocal.VatAmount; //Tiền thuế (Local)
                                }

                                if (charge.CurrencyId == AccountingConstants.CURRENCY_USD)
                                {
                                    charge.AmountUsd = amountOriginal.NetAmount;
                                    charge.VatAmountUsd = amountOriginal.VatAmount;
                                }
                                else
                                {
                                    var amountUsd = currencyExchangeService.CalculatorAmountAccountingByCurrency(charge, AccountingConstants.CURRENCY_USD);
                                    charge.AmountUsd = amountUsd.NetAmount; //Thành tiền trước thuế (USD)
                                    charge.VatAmountUsd = amountUsd.VatAmount; //Tiền thuế (USD)
                                }
                                #endregion -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

                                _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, settlement.SettlementCurrency);

                                charge.TransactionType = GetTransactionTypeOfChargeByHblId(charge.Hblid);
                                charge.OfficeId = currentUser.OfficeID;
                                charge.CompanyId = currentUser.CompanyID;
                                csShipmentSurchargeRepo.Add(charge);
                            }
                        }

                        settlement.Amount = _totalAmount;
                        var hs = DataContext.Add(settlement);
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

            int roundDecimal = model.Settlement.SettlementCurrency != AccountingConstants.CURRENCY_LOCAL ? 3 : 0;
            amount = NumberHelper.RoundNumber(amount, roundDecimal);
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
                settlement.GroupId = settlementCurrent.GroupId;
                settlement.DepartmentId = settlementCurrent.DepartmentId;
                settlement.OfficeId = settlementCurrent.OfficeId;
                settlement.CompanyId = settlementCurrent.CompanyId;
                settlement.LastSyncDate = settlementCurrent.LastSyncDate;
                settlement.SyncStatus = settlementCurrent.SyncStatus;
                settlement.ReasonReject = settlementCurrent.ReasonReject;
                settlement.LockedLog = settlementCurrent.LockedLog;

                //Cập nhật lại Status Approval là NEW nếu Status Approval hiện tại là DENIED
                if (model.Settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED) && settlementCurrent.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED))
                {
                    settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_NEW;
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        decimal _totalAmount = 0;

                        //Start --Phí chứng từ (IsFromShipment = true)--
                        //Cập nhật SettlementCode = null cho các SettlementNo
                        var chargeShipmentOld = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlement.SettlementNo && x.IsFromShipment == true).ToList();
                        if (chargeShipmentOld.Count > 0)
                        {
                            foreach (var item in chargeShipmentOld)
                            {
                                #region -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]
                                acctAdvancePaymentService.UpdateStatusPaymentNotSettledOfAdvanceRequest(item.Hblid, item.AdvanceNo);
                                #endregion -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]

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
                            foreach (var charge in listChargeShipmentUpdate)
                            {
                                // Phí Chứng từ cho phép cập nhật lại số HD, Ngày HD, Số SerieNo, Note.
                                var chargeSettlementCurrentToUpdateCsShipmentSurcharge = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && x.IsFromShipment == true && x.Id == charge.Id)?.FirstOrDefault();
                                charge.Notes = chargeSettlementCurrentToUpdateCsShipmentSurcharge.Notes;
                                charge.SeriesNo = chargeSettlementCurrentToUpdateCsShipmentSurcharge.SeriesNo;
                                charge.InvoiceNo = chargeSettlementCurrentToUpdateCsShipmentSurcharge.InvoiceNo;
                                charge.InvoiceDate = chargeSettlementCurrentToUpdateCsShipmentSurcharge.InvoiceDate;

                                charge.SettlementCode = settlement.SettlementNo;
                                charge.UserModified = userCurrent;
                                charge.DatetimeModified = DateTime.Now;
                                
                                _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, settlement.SettlementCurrency);

                                csShipmentSurchargeRepo.Update(charge, x => x.Id == charge.Id);
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
                            foreach (var charge in listChargeSceneAdd)
                            {
                                charge.Id = Guid.NewGuid();
                                charge.SettlementCode = settlement.SettlementNo;
                                charge.DatetimeCreated = charge.DatetimeModified = DateTime.Now;
                                charge.UserCreated = charge.UserModified = userCurrent;
                                charge.ExchangeDate = DateTime.Now;
                                charge.TransactionType = GetTransactionTypeOfChargeByHblId(charge.Hblid);
                                charge.OfficeId = currentUser.OfficeID;
                                charge.CompanyId = currentUser.CompanyID;

                                #region -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                                var amountOriginal = currencyExchangeService.CalculatorAmountAccountingByCurrency(charge, charge.CurrencyId);
                                charge.NetAmount = amountOriginal.NetAmount; //Thành tiền trước thuế (Original)
                                charge.Total = amountOriginal.NetAmount + amountOriginal.VatAmount; //Thành tiền sau thuế (Original)
                                charge.FinalExchangeRate = amountOriginal.ExchangeRate; //Tỉ giá so với Local

                                if (charge.CurrencyId == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    charge.AmountVnd = amountOriginal.NetAmount;
                                    charge.VatAmountVnd = amountOriginal.VatAmount;
                                }
                                else
                                {
                                    var amountLocal = currencyExchangeService.CalculatorAmountAccountingByCurrency(charge, AccountingConstants.CURRENCY_LOCAL);
                                    charge.AmountVnd = amountLocal.NetAmount; //Thành tiền trước thuế (Local)
                                    charge.VatAmountVnd = amountLocal.VatAmount; //Tiền thuế (Local)
                                }

                                if (charge.CurrencyId == AccountingConstants.CURRENCY_USD)
                                {
                                    charge.AmountUsd = amountOriginal.NetAmount;
                                    charge.VatAmountUsd = amountOriginal.VatAmount;
                                }
                                else
                                {
                                    var amountUsd = currencyExchangeService.CalculatorAmountAccountingByCurrency(charge, AccountingConstants.CURRENCY_USD);
                                    charge.AmountUsd = amountUsd.NetAmount; //Thành tiền trước thuế (USD)
                                    charge.VatAmountUsd = amountUsd.VatAmount; //Tiền thuế (USD)
                                }
                                #endregion -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

                                _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, settlement.SettlementCurrency);

                                csShipmentSurchargeRepo.Add(charge);
                            }
                        }

                        //Cập nhật lại các thông tin của phí hiện trường (nếu có edit chỉnh sửa phí hiện trường)
                        var chargeSceneUpdate = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && idsChargeScene.Contains(x.Id) && x.IsFromShipment == false);
                        var idChargeSceneUpdate = chargeSceneUpdate.Select(s => s.Id).ToList();
                        if (chargeSceneUpdate.Count() > 0)
                        {
                            var listChargeExists = csShipmentSurchargeRepo.Get(x => idChargeSceneUpdate.Contains(x.Id));

                            #region -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]
                            foreach (var chargeExist in listChargeExists)
                            {
                                acctAdvancePaymentService.UpdateStatusPaymentNotSettledOfAdvanceRequest(chargeExist.Hblid, chargeExist.AdvanceNo);
                            }
                            #endregion -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]

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

                                if (sceneCharge != null)
                                {
                                    sceneCharge.UnitId = item.UnitId;
                                    sceneCharge.UnitPrice = item.UnitPrice;
                                    sceneCharge.ChargeId = item.ChargeId;
                                    sceneCharge.Quantity = item.Quantity;
                                    sceneCharge.CurrencyId = item.CurrencyId;
                                    sceneCharge.Vatrate = item.Vatrate;
                                    sceneCharge.ContNo = item.ContNo;
                                    sceneCharge.InvoiceNo = item.InvoiceNo;
                                    sceneCharge.InvoiceDate = item.InvoiceDate;
                                    sceneCharge.SeriesNo = item.SeriesNo;
                                    sceneCharge.Notes = item.Notes;
                                    sceneCharge.PayerId = item.PayerId;
                                    sceneCharge.PaymentObjectId = item.PaymentObjectId;
                                    sceneCharge.Type = item.Type;
                                    sceneCharge.ChargeGroup = item.ChargeGroup;

                                    sceneCharge.ClearanceNo = item.ClearanceNo;
                                    sceneCharge.AdvanceNo = item.AdvanceNo;
                                    sceneCharge.JobNo = item.JobNo;
                                    sceneCharge.Mblno = item.Mblno;
                                    sceneCharge.Hblno = item.Hblno;

                                    sceneCharge.UserModified = userCurrent;
                                    sceneCharge.DatetimeModified = DateTime.Now;

                                    #region -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                                    var amountOriginal = currencyExchangeService.CalculatorAmountAccountingByCurrency(sceneCharge, sceneCharge.CurrencyId);
                                    sceneCharge.NetAmount = amountOriginal.NetAmount; //Thành tiền trước thuế (Original)
                                    sceneCharge.Total = amountOriginal.NetAmount + amountOriginal.VatAmount; //Thành tiền sau thuế (Original)
                                    sceneCharge.FinalExchangeRate = amountOriginal.ExchangeRate; //Tỉ giá so với Local

                                    if (sceneCharge.CurrencyId == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        sceneCharge.AmountVnd = amountOriginal.NetAmount;
                                        sceneCharge.VatAmountVnd = amountOriginal.VatAmount;
                                    }
                                    else
                                    {
                                        var amountLocal = currencyExchangeService.CalculatorAmountAccountingByCurrency(sceneCharge, AccountingConstants.CURRENCY_LOCAL);
                                        sceneCharge.AmountVnd = amountLocal.NetAmount; //Thành tiền trước thuế (Local)
                                        sceneCharge.VatAmountVnd = amountLocal.VatAmount; //Tiền thuế (Local)
                                    }

                                    if (sceneCharge.CurrencyId == AccountingConstants.CURRENCY_USD)
                                    {
                                        sceneCharge.AmountUsd = amountOriginal.NetAmount;
                                        sceneCharge.VatAmountUsd = amountOriginal.VatAmount;
                                    }
                                    else
                                    {
                                        var amountUsd = currencyExchangeService.CalculatorAmountAccountingByCurrency(sceneCharge, AccountingConstants.CURRENCY_USD);
                                        sceneCharge.AmountUsd = amountUsd.NetAmount; //Thành tiền trước thuế (USD)
                                        sceneCharge.VatAmountUsd = amountUsd.VatAmount; //Tiền thuế (USD)
                                    }
                                    #endregion -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

                                    _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(sceneCharge, settlement.SettlementCurrency);

                                    csShipmentSurchargeRepo.Update(sceneCharge, x => x.Id == sceneCharge.Id);
                                }
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

                        settlement.Amount = _totalAmount;
                        var hs = DataContext.Update(settlement, x => x.Id == settlement.Id);

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
        public decimal GetAdvanceAmountByShipmentAndCurrency(string advanceNo, Guid hblId, string Currency)
        {
            //Chỉ lấy ra các charge có status advance là done
            //Quy đổi tỉ giá theo ngày hiện tại
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == DateTime.Now.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }
            var advanceNoDone = acctAdvancePaymentRepo.Get(x => x.AdvanceNo == advanceNo && x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE).Select(s => s.AdvanceNo).FirstOrDefault();
            var request = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo
            && x.AdvanceNo == advanceNoDone
            && x.Hblid == hblId);
            var advanceAmount = request.Sum(x => x.Amount * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.RequestCurrency, Currency));
            return advanceAmount.Value;
        }

        public AscSettlementPaymentRequestReportParams GetFirstShipmentOfSettlement(string settlementNo)
        {
            //Order giảm dần theo JobNo
            var surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo).OrderByDescending(x => x.JobNo);
            var firstCharge = surcharges.FirstOrDefault(); //Get charge đầu tiên

            string _jobId = string.Empty;
            string _mbl = string.Empty;
            string _hbl = string.Empty;
            string _containerQty = string.Empty;
            string _customerName = string.Empty;
            string _consigneeName = string.Empty;
            string _consignerName = string.Empty;
            string _advDate = string.Empty;
            decimal? _gw = 0;
            decimal? _nw = 0;
            int? _psc = 0;
            decimal? _cbm = 0;

            var opsTran = opsTransactionRepo.Get(x => x.Hblid == firstCharge.Hblid).FirstOrDefault();
            if (opsTran != null)
            {
                _jobId = opsTran?.JobNo;
                _mbl = opsTran?.Mblno;
                _hbl = opsTran?.Hwbno;
                _containerQty = opsTran.SumContainers != null ? opsTran.SumContainers.Value.ToString() + "/" : string.Empty;
                _customerName = catPartnerRepo.Get(x => x.Id == opsTran.CustomerId).FirstOrDefault()?.PartnerNameVn;
            }
            else
            {
                var csTranDetail = csTransactionDetailRepo.Get(x => x.Id == firstCharge.Hblid).FirstOrDefault();
                if (csTranDetail != null)
                {
                    var csTran = csTransactionRepo.Get(x => x.Id == csTranDetail.JobId).FirstOrDefault();
                    _jobId = csTran?.JobNo;
                    _mbl = csTran?.Mawb;
                    _hbl = csTranDetail?.Hwbno;
                }
                _customerName = catPartnerRepo.Get(x => x.Id == csTranDetail.CustomerId).FirstOrDefault()?.PartnerNameVn;
                _consigneeName = catPartnerRepo.Get(x => x.Id == csTranDetail.ConsigneeId).FirstOrDefault()?.PartnerNameVn;
                _consignerName = catPartnerRepo.Get(x => x.Id == csTranDetail.ShipperId).FirstOrDefault()?.PartnerNameVn;
            }

            var advanceRequest = acctAdvanceRequestRepo.Get(x => x.Hblid == firstCharge.Hblid && x.AdvanceNo == firstCharge.AdvanceNo).FirstOrDefault();
            if (advanceRequest != null)
            {
                var advancePayment = acctAdvancePaymentRepo.Get(x => x.AdvanceNo == advanceRequest.AdvanceNo && x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE).FirstOrDefault();
                if (advancePayment != null)
                {
                    _advDate = advancePayment.DatetimeCreated.Value.ToString("dd/MM/yyyy");
                }
            }

            var result = new AscSettlementPaymentRequestReportParams();
            result.JobId = _jobId;
            result.AdvDate = _advDate;
            result.Customer = _customerName;
            result.Consignee = _consigneeName;
            result.Consigner = _consignerName;
            result.ContainerQty = _containerQty;
            result.CustomsId = !string.IsNullOrEmpty(firstCharge.ClearanceNo) ? firstCharge.ClearanceNo : GetCustomNoOldOfShipment(_jobId);
            result.HBL = _hbl;
            result.MBL = _mbl;
            result.StlCSName = string.Empty;

            //CR: Sum _gw, _nw, _psc, _cbm theo Masterbill [28/12/2020 - Alex]
            //Settlement có nhiều Job thì sum all các job đó
            foreach (var surcharge in surcharges)
            {
                var _opsTrans = opsTransactionRepo.Where(x => x.Hblid == surcharge.Hblid).FirstOrDefault();
                if (_opsTrans != null)
                {
                    _gw += _opsTrans.SumGrossWeight;
                    _nw += _opsTrans.SumNetWeight;
                    _psc += _opsTrans.SumPackages;
                    _cbm += _opsTrans.SumCbm;
                }
                else
                {
                    var csTranDetail = csTransactionDetailRepo.Get(x => x.Id == surcharge.Hblid).FirstOrDefault();
                    //_gw += csTransDetail?.GrossWeight;
                    //_nw += csTransDetail?.NetWeight;
                    //_psc += csTransDetail?.PackageQty;
                    //_cbm += csTransDetail?.Cbm;
                    if (csTranDetail != null)
                    {
                        var _csTrans = csTransactionRepo.Where(x => x.Id == csTranDetail.JobId).FirstOrDefault();
                        _gw += _csTrans?.GrossWeight;
                        _nw += _csTrans?.NetWeight;
                        _psc += _csTrans?.PackageQty;
                        _cbm += _csTrans?.Cbm;
                    }
                }
            }

            result.GW = _gw;
            result.NW = _nw;
            result.PSC = _psc;
            result.CBM = _cbm;
            return result;
        }

        private string GetCustomNoOldOfShipment(string jobNo)
        {
            var customNos = customsDeclarationRepo.Get(x => x.JobNo == jobNo).OrderBy(o => o.DatetimeModified).Select(s => s.ClearanceNo);
            return customNos.FirstOrDefault() ?? string.Empty;
        }

        public List<AscSettlementPaymentRequestReport> GetSettlementPaymentRequestReportBySettlementNo(string settlementNo)
        {
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            var listSettlementPayment = new List<AscSettlementPaymentRequestReport>();

            listSettlementPayment = GetChargeOfSettlement(settlementNo, settlement.SettlementCurrency).ToList();
            if (listSettlementPayment.Count == 0)
            {
                return null;
            }
            // First shipment of Settlement
            var firstShipment = GetFirstShipmentOfSettlement(settlementNo);
            //Lấy thông tin các User Approve Settlement
            var infoSettleAprove = GetInfoApproveSettlementNoCheckBySettlementNo(settlementNo);
            listSettlementPayment.ForEach(fe =>
            {
                fe.JobIdFirst = firstShipment.JobId;
                fe.AdvDate = firstShipment.AdvDate;
                fe.SettlementNo = settlementNo;
                fe.Customer = firstShipment.Customer;
                fe.Consignee = firstShipment.Consignee;
                fe.Consigner = firstShipment.Consigner;
                fe.ContainerQty = firstShipment.ContainerQty;
                fe.GW = firstShipment.GW;
                fe.NW = firstShipment.NW;
                fe.CustomsId = firstShipment.CustomsId;
                fe.PSC = firstShipment.PSC;
                fe.CBM = firstShipment.CBM;
                fe.HBL = firstShipment.HBL;
                fe.MBL = firstShipment.MBL;
                fe.StlCSName = firstShipment.StlCSName;

                fe.SettleRequester = (settlement != null && !string.IsNullOrEmpty(settlement.Requester)) ? userBaseService.GetEmployeeByUserId(settlement.Requester)?.EmployeeNameVn : string.Empty;
                fe.SettleRequestDate = settlement.RequestDate != null ? settlement.RequestDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                fe.StlDpManagerName = infoSettleAprove != null ? infoSettleAprove.ManagerName : string.Empty;
                fe.StlDpManagerSignDate = infoSettleAprove != null && infoSettleAprove.ManagerAprDate.HasValue ? infoSettleAprove.ManagerAprDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                fe.StlAscDpManagerName = infoSettleAprove != null ? infoSettleAprove.AccountantName : string.Empty;
                fe.StlAscDpManagerSignDate = infoSettleAprove != null && infoSettleAprove.AccountantAprDate.HasValue ? infoSettleAprove.AccountantAprDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                fe.StlBODSignDate = infoSettleAprove != null && infoSettleAprove.BuheadAprDate.HasValue ? infoSettleAprove.BuheadAprDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                fe.StlRequesterSignDate = infoSettleAprove != null && infoSettleAprove.RequesterAprDate.HasValue ? infoSettleAprove.RequesterAprDate.Value.ToString("dd/MM/yyyy") : string.Empty;

                //Lấy ra tổng Advance Amount của các charge thuộc Settlement
                decimal advanceAmount = 0;

                var chargeOfSettlements = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo).Where(w => !string.IsNullOrEmpty(w.AdvanceNo)).Select(s => new { s.Hblid, s.AdvanceNo }).Distinct().ToList();
                foreach (var charge in chargeOfSettlements)
                {
                    advanceAmount += GetAdvanceAmountByShipmentAndCurrency(charge.AdvanceNo, charge.Hblid, settlement.SettlementCurrency);
                }
                fe.AdvValue = advanceAmount > 0 ? String.Format("{0:n}", advanceAmount) : string.Empty;
                fe.AdvCurrency = advanceAmount > 0 ? settlement.SettlementCurrency : string.Empty;

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
                fe.Inword = _inword;
            });
            return listSettlementPayment;
        }

        public IQueryable<AscSettlementPaymentRequestReport> GetChargeOfSettlement(string settlementNo, string currency)
        {
            var settle = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            //Quy đổi tỉ giá theo ngày Request Date, nếu không có thì lấy exchange rate mới nhất
            var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settle.RequestDate.Value.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

            var surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo);
            var data = new List<AscSettlementPaymentRequestReport>();
            foreach (var surcharge in surcharges)
            {
                var item = new AscSettlementPaymentRequestReport();
                item.AdvID = settlementNo;
                string _jobId = string.Empty;
                string _hbl = string.Empty;
                var opsTran = opsTransactionRepo.Get(x => x.Hblid == surcharge.Hblid).FirstOrDefault();
                if (opsTran != null)
                {
                    _jobId = opsTran?.JobNo;
                    _hbl = opsTran?.Hwbno;
                }
                else
                {
                    var csTranDetail = csTransactionDetailRepo.Get(x => x.Id == surcharge.Hblid).FirstOrDefault();
                    if (csTranDetail != null)
                    {
                        var csTrans = csTransactionRepo.Get(x => x.Id == csTranDetail.JobId).FirstOrDefault();
                        _jobId = csTrans?.JobNo;
                        _hbl = csTranDetail?.Hwbno;
                    }
                }
                item.JobId = _jobId;
                item.HBL = _hbl;
                item.Description = catChargeRepo.Get(x => x.Id == surcharge.ChargeId).FirstOrDefault()?.ChargeNameEn;
                item.InvoiceNo = surcharge.InvoiceNo ?? string.Empty;
                item.Amount = surcharge.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, surcharge.CurrencyId, currency) + _decimalNumber;
                item.Debt = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? true : false;
                item.Currency = string.Empty;
                item.Note = surcharge.Notes;
                item.CompanyName = AccountingConstants.COMPANY_NAME;
                item.CompanyAddress = AccountingConstants.COMPANY_ADDRESS1;
                item.Website = AccountingConstants.COMPANY_WEBSITE;
                item.Tel = AccountingConstants.COMPANY_CONTACT;
                item.Contact = currentUser.UserName;

                data.Add(item);
            }
            return data.ToArray().OrderByDescending(x => x.JobId).AsQueryable();
        }

        public Crystal Preview(string settlementNo)
        {
            Crystal result = null;
            var listSettlementPayment = GetSettlementPaymentRequestReportBySettlementNo(settlementNo);
            result = new Crystal
            {
                ReportName = "AcsSettlementPaymentRequestSingle.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listSettlementPayment);
            result.FormatType = ExportFormatType.PortableDocFormat;
            return result;
        }

        public Crystal PreviewMultipleSettlement(List<string> settlementNos)
        {
            Crystal result = null;
            List<SettlementPaymentMulti> _settlementNos = new List<SettlementPaymentMulti>();
            var listSettlement = new List<AscSettlementPaymentRequestReport>();
            for (var i = 0; i < settlementNos.Count; i++)
            {
                var settlements = GetSettlementPaymentRequestReportBySettlementNo(settlementNos[i]);
                foreach (var settlement in settlements)
                {
                    listSettlement.Add(settlement);
                }

                var _settlementNo = new SettlementPaymentMulti();
                _settlementNo.SettlementNo = settlementNos[i];
                _settlementNos.Add(_settlementNo);
            }
            result = new Crystal
            {
                ReportName = "AcsSettlementPaymentRequestMulti.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(_settlementNos);
            result.AddSubReport("AcsSettlementPaymentRequestSingle.rpt", listSettlement);
            result.FormatType = ExportFormatType.PortableDocFormat;
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
                            //Update Status Payment of Advance Request by Settlement Code [17-11-2020]
                            acctAdvancePaymentService.UpdateStatusPaymentOfAdvanceRequest(settlementPayment.SettlementNo);
                        }
                        else
                        {
                            //Send Mail Suggest
                            sendMailSuggest = SendMailSuggestApproval(settlementPayment.SettlementNo, userLeaderOrManager, mailLeaderOrManager, mailUsersDeputy);
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

                        // Send mail là Option nên send mail có thất bại vẫn cập nhật data Approve Settlement [23/12/2020]
                        if (!sendMailSuggest)
                        {
                            return new HandleState("Send mail suggest approval failed");
                        }
                        if (!sendMailApproved)
                        {
                            return new HandleState("Send mail approved approval failed");
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
                        //Update Status Payment of Advance Request by Settlement Code [17-11-2020]
                        acctAdvancePaymentService.UpdateStatusPaymentOfAdvanceRequest(settlementPayment.SettlementNo);
                    }
                    else
                    {
                        //Send Mail Suggest
                        sendMailSuggest = SendMailSuggestApproval(settlementPayment.SettlementNo, userApproveNext, mailUserApproveNext, mailUsersDeputy);
                    }

                    settlementPayment.UserModified = approve.UserModified = userCurrent;
                    settlementPayment.DatetimeModified = approve.DateModified = DateTime.Now;

                    var hsUpdateSettlementPayment = DataContext.Update(settlementPayment, x => x.Id == settlementPayment.Id, false);
                    var hsUpdateApprove = acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id, false);

                    acctApproveSettlementRepo.SubmitChanges();
                    DataContext.SubmitChanges();
                    trans.Commit();

                    // Send mail là Option nên send mail có thất bại vẫn cập nhật data Approve Settlement [23/12/2020]
                    if (!sendMailSuggest)
                    {
                        return new HandleState("Send mail suggest approval failed");
                    }
                    if (!sendMailApproved)
                    {
                        return new HandleState("Send mail approved approval failed");
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
                    if ((leaderLevel.Role == AccountingConstants.ROLE_APPROVAL || leaderLevel.Role == AccountingConstants.ROLE_AUTO)
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
                    if ((managerLevel.Role == AccountingConstants.ROLE_APPROVAL || managerLevel.Role == AccountingConstants.ROLE_AUTO) && !isDeptAccountant
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
                    if ((accountantLevel.Role == AccountingConstants.ROLE_APPROVAL || accountantLevel.Role == AccountingConstants.ROLE_AUTO) && isDeptAccountant
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
                    if ((buHeadLevel.Role == AccountingConstants.ROLE_APPROVAL || buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL || buHeadLevel.Role == AccountingConstants.ROLE_AUTO) && isBod
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

                    // Send mail là Option nên send mail có thất bại vẫn cập nhật data Approve Settlement [23/12/2020]
                    var sendMailDeny = SendMailDeniedApproval(settlementPayment.SettlementNo, comment, DateTime.Now);
                    if (!sendMailDeny)
                    {
                        return new HandleState("Send mail denied failed");
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
                item.NameAndTimeDeny = userBaseService.GetEmployeeByUserId(approve.UserModified)?.EmployeeNameVn + " - " + approve.DateModified?.ToString("dd/MM/yyyy HH:mm:ss");
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

        public HandleState CheckValidateMailByUserId(string userId)
        {
            var requesterId = userBaseService.GetEmployeeIdOfUser(userId);
            var _requester = userBaseService.GetEmployeeByEmployeeId(requesterId);
            var emailRequester = _requester?.Email;
            if (string.IsNullOrEmpty(emailRequester))
            {
                return new HandleState("Not found email of [name]");
            }
            if (!SendMail.IsValidEmail(emailRequester))
            {
                return new HandleState("The [name] email address is incorrect");
            }
            return new HandleState();
        }

        /// <summary>
        /// Check exist leader/manager/accountant/BOD, email, valid email
        /// </summary>
        /// <param name="type"></param>
        /// <param name="groupId"></param>
        /// <param name="departmentId"></param>
        /// <param name="officeId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public HandleState CheckExistUserApproval(string type, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var infoLevelApprove = LeaderLevel(type, groupId, departmentId, officeId, companyId);
            if (infoLevelApprove.Role == AccountingConstants.ROLE_AUTO || infoLevelApprove.Role == AccountingConstants.ROLE_APPROVAL)
            {
                if (infoLevelApprove.LevelApprove == AccountingConstants.LEVEL_LEADER)
                {
                    if (string.IsNullOrEmpty(infoLevelApprove.UserId)) return new HandleState("Not found leader");
                    if (string.IsNullOrEmpty(infoLevelApprove.EmailUser)) return new HandleState("Not found email of leader");
                    if (!SendMail.IsValidEmail(infoLevelApprove.EmailUser)) return new HandleState("The leader email address is incorrect");
                }
            }

            var managerLevel = ManagerLevel(type, departmentId, officeId, companyId);
            if (managerLevel.Role == AccountingConstants.ROLE_AUTO || managerLevel.Role == AccountingConstants.ROLE_APPROVAL)
            {
                if (string.IsNullOrEmpty(managerLevel.UserId)) return new HandleState("Not found manager");
                if (string.IsNullOrEmpty(managerLevel.EmailUser)) return new HandleState("Not found email of manager");
                if (!SendMail.IsValidEmail(managerLevel.EmailUser)) return new HandleState("The manager email address is incorrect");
            }

            var accountantLevel = AccountantLevel(type, officeId, companyId);
            if (accountantLevel.Role == AccountingConstants.ROLE_AUTO || accountantLevel.Role == AccountingConstants.ROLE_APPROVAL)
            {
                if (string.IsNullOrEmpty(accountantLevel.UserId)) return new HandleState("Not found accountant");
                if (string.IsNullOrEmpty(accountantLevel.EmailUser)) return new HandleState("Not found email of accountant");
                if (!SendMail.IsValidEmail(accountantLevel.EmailUser)) return new HandleState("The accountant email address is incorrect");
            }

            var buHeadLevel = BuHeadLevel(type, officeId, companyId);
            if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO || buHeadLevel.Role == AccountingConstants.ROLE_APPROVAL || buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL)
            {
                if (string.IsNullOrEmpty(buHeadLevel.UserId)) return new HandleState("Not found BOD");
                if (string.IsNullOrEmpty(buHeadLevel.EmailUser)) return new HandleState("Not found email of BOD");
                if (!SendMail.IsValidEmail(buHeadLevel.EmailUser)) return new HandleState("The BOD email address is incorrect");
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

            var isDeptAccountant = userBaseService.CheckIsAccountantDept(currentUser.DepartmentId);

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
                else if (((isAccountant && userCurrent.GroupId == AccountingConstants.SpecialGroup) || accountantLevel.UserDeputies.Contains(currentUser.UserID)) && isDeptAccountant) //Accountant Manager or Deputy Accountant thuộc Dept Accountant
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
                            ((userCurrent.GroupId == AccountingConstants.SpecialGroup && isAccountant && (userCurrent.UserID == approve.Accountant || userCurrent.UserID == approve.AccountantApr))
                          ||
                            accountantLevel.UserDeputies.Contains(currentUser.UserID))
                          && isDeptAccountant
                        ) //Accountant Manager or Deputy Accountant thuộc Dept Accountant
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
                       (((isAccountant && userCurrent.GroupId == AccountingConstants.SpecialGroup && (userCurrent.UserID == approve.Accountant || userCurrent.UserID == approve.AccountantApr))
                      ||
                        accountantLevel.UserDeputies.Contains(currentUser.UserID)))
                      && isDeptAccountant
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
                    chargeCopy.ClearanceNo = shipment.CustomNo; //Lấy customNo của Shipment
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

        //Send Mail đề nghị Approve
        private bool SendMailSuggestApproval(string settlementNo, string userReciver, string emailUserReciver, List<string> emailUsersDeputy)
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
            totalAmount = NumberHelper.RoundNumber(totalAmount, 2);

            //Lấy ra list AdvanceNo dựa vào Shipment(JobId,MBL,HBL)
            string advanceNos = string.Empty;
            var listAdvanceNo = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo).Select(s => s.AdvanceNo).Distinct();
            advanceNos = string.Join("; ", listAdvanceNo);

            var userReciverId = userBaseService.GetEmployeeIdOfUser(userReciver);
            var userReciverName = userBaseService.GetEmployeeByEmployeeId(userReciverId)?.EmployeeNameEn;

            //Mail Info
            var numberOfRequest = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo).Select(s => s.Id).Count();
            numberOfRequest = numberOfRequest == 0 ? 1 : (numberOfRequest + 1);
            string subject = "eFMS - Settlement Payment Approval Request from [RequesterName] - [NumberOfRequest] " + (numberOfRequest > 1 ? "times" : "time");
            subject = subject.Replace("[RequesterName]", requesterName);
            subject = subject.Replace("[NumberOfRequest]", numberOfRequest.ToString());
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color: #004080'>" +
                                            "<p><i><b>Dear Mr/Mrs [UserName],</b> </i></p>" +
                                            "<p>" +
                                                "<div>You have new Settlement Payment Approval Request from <b>[RequesterName]</b> as below info:</div>" +
                                                "<div><i>Anh/ Chị có một yêu cầu duyệt thanh toán từ <b>[RequesterName]</b> với thông tin như sau: </i></div>" +
                                            "</p>" +
                                            "<ul>" +
                                                "<li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li>" +
                                                "<li>Settlement Amount/ <i>Số tiền thanh toán</i> : <b>[TotalAmount] [CurrencySettlement]</b></li>" +
                                                "<li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li>" +
                                                "<li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li>" +
                                                "<li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li>" +
                                                "<li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li>" +
                                            "</ul>" +
                                            "<p>" +
                                                "<div>You click here to check more detail and approve: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]/approve' target='_blank'>Detail Payment Request</a> </span></div>" +
                                                "<div><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]/approve' target='_blank'>Chi tiết phiếu đề nghị thanh toán</a> </span> </i></div>" +
                                            "</p>" +
                                            "<p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='[logoEFMS]'/></p>" +
                                         "</div>");
            body = body.Replace("[UserName]", userReciverName);
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[SettlementNo]", settlementNo);
            body = body.Replace("[TotalAmount]", string.Format("{0:n}", totalAmount));
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

            if (emailUsersDeputy.Count > 0)
            {
                foreach (var email in emailUsersDeputy)
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        emailCCs.Add(email);
                    }
                }
            }

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);

            #region --- Ghi Log Send Mail ---
            var logSendMail = new SysSentEmailHistory
            {
                SentUser = SendMail._emailFrom,
                Receivers = string.Join("; ", toEmails),
                Ccs = string.Join("; ", emailCCs),
                Subject = subject,
                Sent = sendMailResult,
                SentDateTime = DateTime.Now,
                Body = body
            };
            var hsLogSendMail = sentEmailHistoryRepo.Add(logSendMail);
            var hsSm = sentEmailHistoryRepo.SubmitChanges();
            #endregion --- Ghi Log Send Mail ---

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
            totalAmount = NumberHelper.RoundNumber(totalAmount, 2);

            //Lấy ra list AdvanceNo dựa vào Shipment(JobId,MBL,HBL)
            string advanceNos = string.Empty;
            var listAdvanceNo = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo).Select(s => s.AdvanceNo).Distinct();
            advanceNos = string.Join("; ", listAdvanceNo);

            //Mail Info
            string subject = "eFMS - Settlement Payment from [RequesterName] is approved";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color: #004080'>" +
                                            "<p> <i> <b>Dear Mr/Mrs [RequesterName],</b> </i></p>" +
                                            "<p>" +
                                                "<div>You have an Settlement Payment is approved at <b>[ApprovedDate]</b> as below info:</div>" +
                                                "<div><i>Anh/ Chị có một đề nghị thanh toán đã được phê duyệt vào lúc <b>[ApprovedDate]</b> với thông tin như sau: </i></div>" +
                                            "</p>" +
                                            "<ul>" +
                                                "<li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li>" +
                                                "<li>Settlement Amount/ <i>Số tiền thanh toán</i> : <b>[TotalAmount] [CurrencySettlement]</b></li>" +
                                                "<li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li>" +
                                                "<li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li>" +
                                                "<li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li>" +
                                                "<li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li>" +
                                            "</ul>" +
                                            "<p>" +
                                                "<div>You can click here to check more detail: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Detail Payment Request</a> </span></div>" +
                                                "<div> <i>Anh/ Chị có thể chọn vào đây để biết thêm thông tin chi tiết: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Chi tiết đề nghị thanh toán</a> </span> </i></div>" +
                                            "</p>" +
                                            "<p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='[logoEFMS]'/></p>" +
                                         "</div>");
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[ApprovedDate]", approvedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[SettlementNo]", settlementNo);
            body = body.Replace("[TotalAmount]", string.Format("{0:n}", totalAmount));
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

            #region --- Ghi Log Send Mail ---
            var logSendMail = new SysSentEmailHistory
            {
                SentUser = SendMail._emailFrom,
                Receivers = string.Join("; ", toEmails),
                Ccs = string.Join("; ", emailCCs),
                Subject = subject,
                Sent = sendMailResult,
                SentDateTime = DateTime.Now,
                Body = body
            };
            var hsLogSendMail = sentEmailHistoryRepo.Add(logSendMail);
            var hsSm = sentEmailHistoryRepo.SubmitChanges();
            #endregion --- Ghi Log Send Mail ---

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
            jobIds = string.Join("; ", listJobId.ToList());

            var totalAmount = surcharge
                .Where(x => x.SettlementCode == settlementNo)
                .Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, settlement.SettlementCurrency));
            totalAmount = NumberHelper.RoundNumber(totalAmount, 2);

            //Lấy ra list AdvanceNo dựa vào Shipment(JobId,MBL,HBL)
            string advanceNos = string.Empty;
            var listAdvanceNo = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo).Select(s => s.AdvanceNo).Distinct();
            advanceNos = string.Join("; ", listAdvanceNo);

            //Mail Info
            string subject = "eFMS - Settlement Payment from [RequesterName] is denied";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color: #004080'>" +
                                            "<p> <i> <b>Dear Mr/Mrs [RequesterName],</b> </i></p>" +
                                            "<p>" +
                                                "<div>You have an Settlement Payment is denied at <b>[DeniedDate]</b> by as below info:</div>" +
                                                "<div> <i>Anh/ Chị có một yêu cầu đề nghị thanh toán đã bị từ chối vào lúc <b>[DeniedDate]</b> by với thông tin như sau: </i></div>" +
                                            "</p>" +
                                            "<ul>" +
                                                "<li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li>" +
                                                "<li>Settlement Amount/ <i>Số tiền tạm ứng</i> : <b>[TotalAmount] [CurrencySettlement]</b></li>" +
                                                "<li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li>" +
                                                "<li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li>" +
                                                "<li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li>" +
                                                "<li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li>" +
                                                "<li>Comment/ <i>Lý do từ chối</i> : <b>[Comment]</b></li>" +
                                            "</ul>" +
                                            "<p>" +
                                                "<div>You click here to recheck detail: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Detail Payment Request</a> </span></div>" +
                                                "<div> <i>Anh/ Chị chọn vào đây để kiểm tra lại thông tin chi tiết: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]' target='_blank'>Chi tiết đề nghị thanh toán</a> </span> </i></div>" +
                                            "</p>" +
                                            "<p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='[logoEFMS]'/></p>" +
                                         "</div>");
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[DeniedDate]", DeniedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[SettlementNo]", settlementNo);
            body = body.Replace("[TotalAmount]", string.Format("{0:n}", totalAmount));
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

            #region --- Ghi Log Send Mail ---
            var logSendMail = new SysSentEmailHistory
            {
                SentUser = SendMail._emailFrom,
                Receivers = string.Join("; ", toEmails),
                Ccs = string.Join("; ", emailCCs),
                Subject = subject,
                Sent = sendMailResult,
                SentDateTime = DateTime.Now,
                Body = body
            };
            var hsLogSendMail = sentEmailHistoryRepo.Add(logSendMail);
            var hsSm = sentEmailHistoryRepo.SubmitChanges();
            #endregion --- Ghi Log Send Mail ---

            return sendMailResult;
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
            if (settlesToUnLock.Count() < keyWords.Distinct().Count()) return result;
            if (settlesToUnLock.Where(x => x.SyncStatus == "Synced").Any())
            {
                result.Logs = settlesToUnLock.Where(x => x.SyncStatus == "Synced").Select(x => x.SettlementNo).ToList();
            }
            else
            {
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

            //Nếu User là Admin thì sẽ cho phép xem detail [CR: 09/01/2021]
            var isAdmin = userBaseService.CheckIsUserAdmin(currentUser.UserID, currentUser.OfficeID, currentUser.CompanyID, detail.OfficeId, detail.CompanyId);
            if (isAdmin) return true;

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

            var office = sysOfficeRepo.Get(x => x.Id == settlementPayment.OfficeId).FirstOrDefault();
            var _contactOffice = string.Format("{0}\nTel: {1}  Fax: {2}\nE-mail: {3}\nWebsite: www.itlvn.com", office?.AddressEn, office?.Tel, office?.Fax, office?.Email);

            var infoSettlement = new InfoSettlementExport
            {
                Requester = _requester,
                RequestDate = settlementPayment.RequestDate,
                Department = _department,
                SettlementNo = settlementPayment.SettlementNo,
                Manager = _manager,
                Accountant = _accountant,
                IsRequesterApproved = _settlementApprove?.RequesterAprDate != null,
                IsManagerApproved = _settlementApprove?.ManagerAprDate != null,
                IsAccountantApproved = _settlementApprove?.AccountantAprDate != null,
                IsBODApproved = _settlementApprove?.BuheadAprDate != null,
                ContactOffice = _contactOffice
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

                string _personInCharge = string.Empty;
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

                    var employeeId = sysUserRepo.Get(x => x.Id == ops.BillingOpsId).FirstOrDefault()?.EmployeeId;
                    _personInCharge = sysEmployeeRepo.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameEn;
                    shipmentSettlement.PersonInCharge = _personInCharge;

                    listData.Add(shipmentSettlement);
                }
                else
                {
                    var tranDetail = csTransactionDetailRepo.Get(x => x.Id == houseBillId.hblId).FirstOrDefault();
                    if (tranDetail != null)
                    {
                        var trans = csTransactionRepo.Get(x => x.Id == tranDetail.JobId).FirstOrDefault();
                        shipmentSettlement.JobNo = trans?.JobNo;
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

                        if (trans != null)
                        {
                            var employeeId = sysUserRepo.Get(x => x.Id == trans.PersonIncharge).FirstOrDefault()?.EmployeeId;
                            _personInCharge = sysEmployeeRepo.Get(x => x.Id == employeeId).FirstOrDefault()?.EmployeeNameEn;
                            shipmentSettlement.PersonInCharge = _personInCharge;
                        }
                        listData.Add(shipmentSettlement);
                    }
                }
            }
            var result = listData.ToArray().OrderBy(x => x.JobNo); //Sắp xếp tăng dần theo JobNo [05-01-2021]
            return result.ToList();
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
            var groupAdvanceNoAndHblID = surChargeBySettleCode.GroupBy(g => new { g.AdvanceNo, g.Hblid }).ToList().Where(x => x.Key.Hblid == hblId);
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
                                                //join ar in advRequest on sur.JobNo equals ar.JobId
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
                                              Description = sur.Notes
                                          };

                        var data = dataOperation.Union(dataService).ToList();

                        decimal? advTotalAmount;
                        var group = data.GroupBy(d => new { d.SettleNo, d.JobID, d.HBL, d.MBL, d.CustomNo })
                            .Select(s => new SettlementExportGroupDefault
                            {
                                JobID = s.Key.JobID,
                                MBL = s.Key.MBL,
                                HBL = s.Key.HBL,
                                CustomNo = s.Key.CustomNo,
                                SettlementTotalAmount = s.Sum(d => d.SettlementAmount),
                                requestList = getRequestList(data, s.Key.JobID, s.Key.HBL, s.Key.MBL, s.Key.SettleNo, out advTotalAmount),
                                AdvanceTotalAmount = advTotalAmount,
                                BalanceTotalAmount = advTotalAmount - s.Sum(d => d.SettlementAmount)
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

        private List<SettlementExportDefault> getRequestList(List<SettlementExportDefault> data, string JobID,
            string HBL, string MBL, string SettleNo, out decimal? advTotalAmount)
        {
            var advRequest = acctAdvanceRequestRepo.Get();
            var advPayment = acctAdvancePaymentRepo.Get();
            var settleDesc = DataContext.Get().Where(x => x.SettlementNo == SettleNo).FirstOrDefault().Note;
            //
            var groupAdvReq = advRequest.GroupBy(x => new { x.JobId, x.AdvanceNo, x.Hbl, x.Mbl })
                            .Select(y => new { y.Key.JobId, y.Key.Hbl, y.Key.Mbl, y.Key.AdvanceNo, AdvanceAmount = y.Sum(z => z.Amount) });
            var groupAdvReqByStatus = from advReq in groupAdvReq
                                      join advPay in advPayment on advReq.AdvanceNo equals advPay.AdvanceNo
                                      where advPay.StatusApproval == "Done"
                                      select new
                                      {
                                          advReq.AdvanceNo,
                                          advReq.JobId,
                                          advReq.Hbl,
                                          advReq.Mbl,
                                          advReq.AdvanceAmount
                                      };
            //
            var groupData = data
                .GroupBy(d => new
                {
                    d.JobID,
                    d.HBL,
                    d.MBL,
                    d.AdvanceNo,
                    d.SettleNo,
                    d.Currency,
                    d.ApproveDate,
                    d.CustomNo,
                    d.RequestDate,
                    d.Requester
                })
                .Where(x => x.Key.JobID == JobID && x.Key.HBL == HBL && x.Key.MBL == MBL && x.Key.SettleNo == SettleNo)
                .Select(y => new SettlementExportDefault
                {
                    JobID = y.Key.JobID,
                    HBL = y.Key.HBL,
                    MBL = y.Key.MBL,
                    SettleNo = y.Key.SettleNo,
                    AdvanceNo = y.Key.AdvanceNo,
                    SettlementAmount = y.Sum(z => z.SettlementAmount),
                    ApproveDate = y.Key.ApproveDate,
                    Currency = y.Key.Currency,
                    CustomNo = y.Key.CustomNo,
                    Description = settleDesc,
                    RequestDate = y.Key.RequestDate,
                    Requester = y.Key.Requester
                });
            var groupDataAdvanceNoIsNull = groupData.Where(x => x.AdvanceNo == null);
            groupData = groupData.Where(x => x.AdvanceNo != null);
            var result = from gd in groupData
                         join gar in /*groupAdvReq*/ groupAdvReqByStatus on new { gd.AdvanceNo, gd.JobID, gd.HBL, gd.MBL } equals
                         new { gar.AdvanceNo, JobID = gar.JobId, HBL = gar.Hbl, MBL = gar.Mbl } into res
                         from gar in res.DefaultIfEmpty()
                         select new SettlementExportDefault
                         {
                             JobID = gd.JobID,
                             HBL = gd.HBL,
                             MBL = gd.MBL,
                             SettleNo = gd.SettleNo,
                             AdvanceNo = gd.AdvanceNo,

                             SettlementAmount = gd.SettlementAmount,
                             ApproveDate = gd.ApproveDate,
                             Currency = gd.Currency,
                             CustomNo = gd.CustomNo,
                             Description = gd.Description,
                             RequestDate = gd.RequestDate,
                             Requester = gd.Requester,
                             AdvanceAmount = gar == null ? 0 : gar.AdvanceAmount,
                         };
            var output = result.ToList();
            if (groupDataAdvanceNoIsNull.Count() > 0)
            {
                foreach (var item in groupDataAdvanceNoIsNull)
                {
                    item.AdvanceAmount = 0;
                }
                output.AddRange(groupDataAdvanceNoIsNull);
            }

            advTotalAmount = result.Sum(z => z.AdvanceAmount).Value;

            //;
            return output;

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
                        var hsUpdateApproveSettlement = acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id);
                    }

                    settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_NEW;
                    settlement.UserModified = currentUser.UserID;
                    settlement.DatetimeModified = DateTime.Now;
                    var hsUpdateSettlementPayment = DataContext.Update(settlement, x => x.Id == settlement.Id);
                    trans.Commit();
                    return hsUpdateSettlementPayment;
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

        private string GetTransactionTypeOfChargeByHblId(Guid? hblId)
        {
            string transactionType = string.Empty;
            var ops = opsTransactionRepo.Get(x => x.Hblid == hblId).FirstOrDefault();
            if (ops != null)
            {
                transactionType = "CL";
            }
            else
            {
                var tranDetail = csTransactionDetailRepo.Get(x => x.Id == hblId).FirstOrDefault();
                if (tranDetail != null)
                {
                    var tran = csTransactionRepo.Get(x => x.Id == tranDetail.JobId).FirstOrDefault();
                    transactionType = tran?.TransactionType;
                }
            }
            return transactionType;
        }

        public HandleState DenyAdvancePayments(List<Guid> Ids)
        {
            HandleState result = new HandleState();
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (Guid Id in Ids)
                    {
                        {
                            AcctSettlementPayment settle = DataContext.First(x => x.Id == Id);
                            if (settle != null && settle.SyncStatus == AccountingConstants.STATUS_REJECTED)
                            {
                                settle.StatusApproval = AccountingConstants.STATUS_APPROVAL_DENIED;
                                settle.UserModified = currentUser.UserID;
                                settle.DatetimeModified = DateTime.Now;

                                // ghi log
                                string log = String.Format("{0} has been opened at {1} on {2} by {3}", settle.SettlementNo, string.Format("{0:HH:mm:ss tt}", DateTime.Now), DateTime.Now.ToString("dd/MM/yyyy"), currentUser.UserName);

                                settle.LockedLog = settle.LockedLog + log + ";";

                                result = DataContext.Update(settle, x => x.Id == Id, false);

                                if (result.Success)
                                {
                                    IQueryable<AcctApproveSettlement> approveSettles = acctApproveSettlementRepo.Get(x => x.SettlementNo == settle.SettlementNo);
                                    foreach (var approve in approveSettles)
                                    {
                                        approve.IsDeny = true;
                                        approve.UserModified = currentUser.UserID;
                                        approve.DateModified = DateTime.Now;

                                        acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id, false);
                                    }
                                }
                            }
                        }
                    }
                    DataContext.SubmitChanges();
                    acctApproveSettlementRepo.SubmitChanges();
                    trans.Commit();
                    return result;
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
    }
}

