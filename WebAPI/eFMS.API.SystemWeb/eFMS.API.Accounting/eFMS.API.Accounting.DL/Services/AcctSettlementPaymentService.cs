﻿using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Accounting.DL.Models.ReportResults;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Contexts;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

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
        readonly IContextBase<SysAuthorizedApproval> authourizedApprovalRepo;
        private readonly IContextBase<AcctSoa> acctSoaRepo;
        private readonly IContextBase<CustomsDeclaration> customClearanceRepo;
        private readonly IContextBase<AcctCdnote> acctCdnoteRepo;
        readonly IAcctAdvancePaymentService acctAdvancePaymentService;
        readonly ICurrencyExchangeService currencyExchangeService;
        readonly IUserBaseService userBaseService;
        private readonly IContextBase<SysImage> sysImageRepository;
        private readonly IAccAccountReceivableService accAccountReceivableService;
        private readonly IContextBase<SysNotifications> sysNotificationRepository;
        private readonly IContextBase<SysUserNotification> sysUserNotificationRepository;
        private readonly IContextBase<SysEmailTemplate> sysEmailTemplateRepository;
        private readonly IContextBase<SysEmailSetting> sysEmailSettingRepository;
        private readonly IContextBase<OpsStageAssigned> opsStageAssignedRepository;
        private readonly IContextBase<CsLinkCharge> csLinkChargeRepository;
        private readonly IContextBase<SysSettingFlow> settingflowRepository;
        private readonly IContextBase<SysImageDetail> imageDetailRepository;
        private readonly IContextBase<CatContract> contractRepository;
        private string typeApproval = "Settlement";
        private decimal _decimalNumber = Constants.DecimalNumber;
        private IDatabaseUpdateService databaseUpdateService;
        private readonly IStringLocalizer stringLocalizer;

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
            IContextBase<SysAuthorizedApproval> authourizedApproval,
            IContextBase<AcctSoa> acctSoa,
            IContextBase<CustomsDeclaration> customClearance,
            IContextBase<AcctCdnote> acctCdnote,
            IAcctAdvancePaymentService advance,
            ICurrencyExchangeService currencyExchange,
            IContextBase<SysImage> sysImageRepo,
            IAccAccountReceivableService accAccountReceivable,
            IContextBase<SysNotifications> sysNotificationRepo,
            IContextBase<SysUserNotification> sysUserNotificationRepo,
            IContextBase<SysEmailTemplate> sysEmailTemplateRepo,
            IContextBase<SysEmailSetting> sysEmailSettingRepo,
            IUserBaseService userBase,
            IDatabaseUpdateService _databaseUpdateService,
            IContextBase<CsLinkCharge> csLinkChargeRepo,
            IContextBase<SysSettingFlow> settingflowRepo,
            IContextBase<SysImageDetail> imageDetailRepo,
            IContextBase<OpsStageAssigned> opsStageAssignedRepo,
            IContextBase<CatContract> contractRepo,
            IStringLocalizer<AccountingLanguageSub> localizer) : base(repository, mapper)
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
            authourizedApprovalRepo = authourizedApproval;
            acctSoaRepo = acctSoa;
            customClearanceRepo = customClearance;
            acctCdnoteRepo = acctCdnote;
            sysImageRepository = sysImageRepo;
            accAccountReceivableService = accAccountReceivable;
            sysNotificationRepository = sysNotificationRepo;
            sysUserNotificationRepository = sysUserNotificationRepo;
            sysEmailTemplateRepository = sysEmailTemplateRepo;
            sysEmailSettingRepository = sysEmailSettingRepo;
            opsStageAssignedRepository = opsStageAssignedRepo;
            databaseUpdateService = _databaseUpdateService;
            csLinkChargeRepository = csLinkChargeRepo;
            settingflowRepository = settingflowRepo;
            imageDetailRepository = imageDetailRepo;
            contractRepository = contractRepo;
            stringLocalizer = localizer;
        }

        #region --- LIST & PAGING SETTLEMENT PAYMENT ---
        public List<AcctSettlementPaymentResult> Paging(AcctSettlementPaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetSettlementsByCriteria(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            var result = new List<AcctSettlementPaymentResult>();

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

                result = TakeSettlements(data).ToList();
            }

            return result;
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

            if (!string.IsNullOrEmpty(criteria.PayeeId))
            {
                query = query.And(x => x.Payee == criteria.PayeeId);
            }

            if (criteria.DepartmentId != null)
            {
                query = query.And(x => x.DepartmentId == criteria.DepartmentId);
            }
            return query;
        }

        private IQueryable<AcctSettlementPayment> GetSettlementByPermission(AcctSettlementPaymentCriteria criteria)
        {
            var permissionRangeRequester = GetPermissionRangeOfRequester();

            //Nếu không có điều kiện search thì load 3 tháng kể từ ngày modified mới nhất
            var queryDefault = ExpressionQueryDefault(criteria);
            var settlementPayments = DataContext.Get().Where(queryDefault);

            var settlementPaymentAprs = acctApproveSettlementRepo.Get(x => x.IsDeny == false);
            var authorizedApvList = authourizedApprovalRepo.Get(x => x.Type == typeApproval && x.Active == true && (x.ExpirationDate ?? DateTime.Now.Date) >= DateTime.Now.Date).ToList();
            var isAccountantDept = userBaseService.CheckIsAccountantByOfficeDept(currentUser.OfficeID, currentUser.DepartmentId);

            var userCurrent = sysUserRepo.Get(x => x.Id == currentUser.UserID).FirstOrDefault();

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
                    permissionRangeRequester == PermissionRange.Owner ? x.settlementPayment.UserCreated == criteria.Requester && x.settlementPayment.OfficeId == currentUser.OfficeID : true
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
                  //|| userBaseService.CheckIsUserDeputy(typeApproval, currentUser.UserID, x.settlementPaymentApr.Leader, x.settlementPayment.GroupId, x.settlementPayment.DepartmentId, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
                  || authorizedApvList.Where(w => w.Commissioner == currentUser.UserID && w.Authorizer == x.settlementPaymentApr.Leader && w.OfficeCommissioner == x.settlementPayment.OfficeId).Any()
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
                  //|| userBaseService.CheckIsUserDeputy(typeApproval, currentUser.UserID, x.settlementPaymentApr.Manager, null, x.settlementPayment.DepartmentId, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
                  || authorizedApvList.Where(w => w.Commissioner == currentUser.UserID && w.Authorizer == x.settlementPaymentApr.Manager && w.OfficeCommissioner == x.settlementPayment.OfficeId).Any()
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
                  //|| userBaseService.CheckIsUserDeputy(typeApproval, currentUser.UserID, x.settlementPaymentApr.Accountant, null, null, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
                  || authorizedApvList.Where(w => w.Commissioner == currentUser.UserID && w.Authorizer == x.settlementPaymentApr.Accountant && w.OfficeCommissioner == x.settlementPayment.OfficeId).Any()
                  || isAccountantDept
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
                  //|| userBaseService.CheckIsUserDeputy(typeApproval, currentUser.UserID, x.settlementPaymentApr.Buhead ?? null, null, null, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId)
                  || authorizedApvList.Where(w => w.Commissioner == currentUser.UserID && w.Authorizer == x.settlementPaymentApr.Buhead && w.OfficeCommissioner == x.settlementPayment.OfficeId).Any()
                  )
                && x.settlementPayment.OfficeId == currentUser.OfficeID
                && x.settlementPayment.CompanyId == currentUser.CompanyID
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.settlementPayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                && (x.settlementPayment.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.settlementPayment.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) //BOD AND DEPUTY OF BOD   
                ||
                (
                 //userBaseService.CheckIsUserAdmin(currentUser.UserID, currentUser.OfficeID, currentUser.CompanyID, x.settlementPayment.OfficeId, x.settlementPayment.CompanyId) // Is User Admin
                 (userCurrent.UserType == "Super Admin" || (userCurrent.UserType == "Local Admin" && currentUser.OfficeID == x.settlementPayment.OfficeId && currentUser.CompanyID == x.settlementPayment.CompanyId))
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
                Regex regex = new Regex("SM");
                if (criteria.ReferenceNos.Any(x => regex.IsMatch(x)))
                {
                    refNo = criteria.ReferenceNos.Where(x => regex.IsMatch(x)).ToList();

                    settlementPayments = settlementPayments.Where(x => refNo.Contains(x.SettlementNo));
                }
                else
                {
                    refNo = (from set in settlementPayments
                             join sur in surcharge on set.SettlementNo equals sur.SettlementCode into grpSur
                             from sur in grpSur.DefaultIfEmpty()
                             join ops in opst.AsParallel() on sur.Hblid equals ops.Hblid into grpOps
                             from ops in grpOps.DefaultIfEmpty()
                             join cus in custom.AsParallel() on ops.JobNo equals cus.JobNo into grpCus
                             from cus in grpCus.DefaultIfEmpty()
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
                }

            }

            return settlementPayments;
        }

        /// <summary>
        /// Nếu không có điều kiện search (ngoại trừ param requester) thì load list Advance 3 tháng kể từ ngày modified mới nhất trở về trước
        /// </summary>
        /// <returns></returns>
        private Expression<Func<AcctSettlementPayment, bool>> ExpressionQueryDefault(AcctSettlementPaymentCriteria criteria)
        {
            Expression<Func<AcctSettlementPayment, bool>> query = q => true;
            if ((criteria.ReferenceNos == null || criteria.ReferenceNos.Count == 0)
                && criteria.RequestDateFrom == null
                && criteria.RequestDateTo == null
                && (string.IsNullOrEmpty(criteria.PaymentMethod) || criteria.PaymentMethod == "All")
                && (string.IsNullOrEmpty(criteria.StatusApproval) || criteria.StatusApproval == "All")
                && (string.IsNullOrEmpty(criteria.CurrencyID) || criteria.CurrencyID == "All"))
            {
                var maxDate = (DataContext.Get().Max(x => x.DatetimeModified) ?? DateTime.Now).AddDays(1).Date;
                var minDate = maxDate.AddMonths(-3).AddDays(-1).Date; //Bắt đầu từ ngày MaxDate trở về trước 3 tháng
                query = query.And(x => x.DatetimeModified.Value > minDate && x.DatetimeModified.Value < maxDate);
            }
            return query;
        }

        public IQueryable<AcctSettlementPayment> GetSettlementsByCriteria(AcctSettlementPaymentCriteria criteria)
        {
            var querySettlementPayment = ExpressionQuery(criteria);
            var dataSettlementPayments = GetSettlementByPermission(criteria);
            if (dataSettlementPayments == null) return null;
            var settlementPayments = dataSettlementPayments.Where(querySettlementPayment);
            settlementPayments = QueryWithShipment(settlementPayments, criteria);
            if (settlementPayments != null)
            {
                settlementPayments = settlementPayments.OrderByDescending(orb => orb.DatetimeModified).AsQueryable();
            }
            return settlementPayments;
        }

        private IQueryable<AcctSettlementPaymentResult> TakeSettlements(IQueryable<AcctSettlementPayment> settlementPayments)
        {
            if (settlementPayments == null) return null;

            var users = sysUserRepo.Get();
            IQueryable<CatPartner> partners = catPartnerRepo.Get();
            IQueryable<CatDepartment> departments = catDepartmentRepo.Get();
            var settleNos = settlementPayments.Select(x => x.SettlementNo).ToList();
            var surchargesIssued = csShipmentSurchargeRepo.Get(x => settleNos.Any(z => z == x.SettlementCode) && x.IsFromShipment == false &&
                    !(string.IsNullOrEmpty(x.DebitNo) && string.IsNullOrEmpty(x.CreditNo) && string.IsNullOrEmpty(x.Soano) && string.IsNullOrEmpty(x.PaySoano) && string.IsNullOrEmpty(x.VoucherId) && string.IsNullOrEmpty(x.VoucherIdre)))
                    .Select(x => x.SettlementCode).Distinct().ToList();

            var data = from settlePayment in settlementPayments
                       join p in partners on settlePayment.Payee equals p.Id into partnerGrps
                       from partnerGrp in partnerGrps.DefaultIfEmpty()
                       join user in users on settlePayment.Requester equals user.Id into user2
                       from user in user2.DefaultIfEmpty()
                       join dept in departments on settlePayment.DepartmentId equals dept.Id into deptGrps
                       from deptGrp in deptGrps.DefaultIfEmpty()
                       select new AcctSettlementPaymentResult
                       {
                           Id = settlePayment.Id,
                           Amount = settlePayment.Amount ?? 0,
                           SettlementNo = settlePayment.SettlementNo,
                           SettlementCurrency = settlePayment.SettlementCurrency,
                           Requester = settlePayment.Requester,
                           RequesterName = user.Username,
                           RequestDate = settlePayment.RequestDate,
                           DueDate = settlePayment.DueDate,
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
                           PayeeName = partnerGrp.ShortName,
                           DepartmentName = deptGrp.DeptNameAbbr,

                           IssuedSoa = surchargesIssued.Any(z => z == settlePayment.SettlementNo)
                       };
            return data;
        }

        public IQueryable<AcctSettlementPaymentResult> QueryData(AcctSettlementPaymentCriteria criteria)
        {
            var settlementPayments = GetSettlementsByCriteria(criteria);
            var result = TakeSettlements(settlementPayments);
            return result;
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
                                    SettlementCurrency = set.SettlementCurrency,
                                    IsLocked = ops.IsLocked
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
                                   SettlementCurrency = set.SettlementCurrency,
                                   IsLocked = cst.IsLocked
                               };
            var data = dataOperation.Union(dataDocument);
            var dataGrp = data.ToList().GroupBy(x => new
            {
                x.JobId,
                x.HBL,
                x.MBL,
                x.SettlementCurrency,
                x.IsLocked
            }
            ).Select(s => new ShipmentOfSettlementResult
            {
                JobId = s.Key.JobId,
                Amount = s.Sum(su => su.Amount * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, su.ChargeCurrency, su.SettlementCurrency)),
                HBL = s.Key.HBL,
                MBL = s.Key.MBL,
                SettlementCurrency = s.Key.SettlementCurrency,
                IsLocked = s.Key.IsLocked
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

            if (!string.IsNullOrEmpty(settlement.Payee))
            {
                settlementMap.PayeeName = catPartnerRepo.Get(x => x.Id == settlement.Payee)?.FirstOrDefault().ShortName;
            }

            var surchargesSm = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlement.SettlementNo);
            settlementMap.TotalCharge = surchargesSm.Count();
            settlementMap.TotalGroup = surchargesSm.GroupBy(x => new { x.SettlementCode, x.Hblid, x.AdvanceNo, CustomNo = (string.IsNullOrEmpty(x.ClearanceNo) ? null : x.ClearanceNo) }).Count();

            return settlementMap;
        }

        public List<ShipmentSettlement> GetListShipmentSettlementBySettlementNo(string settlementNo, int page = - 1, int size = 5)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@settlementNo", Value = settlementNo },
            };
            if (page > 0)
            {
                parameters = parameters.Concat(new[] { new SqlParameter("@Page", page), new SqlParameter("@Size", size) }).ToArray();
            }
            var dataSp = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetListJobGroupSurchargeDetailSettlement>(parameters);
            var data = mapper.Map<List<ShipmentSettlement>>(dataSp);

            foreach (var item in data)
            {
                item.ChargeSettlements = GetSurchargeDetailSettlement(item.SettlementNo, item.HblId, item.AdvanceNo, item.CustomNo);
            }
            return data;

            //IQueryable<CsShipmentSurcharge> surcharge = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo);
            //IQueryable<AcctSettlementPayment> settlement = DataContext.Get();
            //IQueryable<OpsTransaction> opsTrans = opsTransactionRepo.Get();
            //IQueryable<CsTransactionDetail> csTransD = csTransactionDetailRepo.Get();
            //IQueryable<CsTransaction> csTrans = csTransactionRepo.Get();
            //IQueryable<CustomsDeclaration> cdNos = customsDeclarationRepo.Get();
            //IQueryable<AcctAdvanceRequest> advanceRequests = acctAdvanceRequestRepo.Get();
            ////IQueryable<AcctAdvancePayment> advances = acctAdvancePaymentRepo.Get(a => a.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE);


            //AcctSettlementPayment settleCurrent = settlement.Where(x => x.SettlementNo == settlementNo).FirstOrDefault();
            //if (settlement == null) return null;
            ////Quy đổi tỉ giá theo ngày Request Date, nếu exchange rate của ngày Request date không có giá trị thì lấy excharge rate mới nhất
            ////List<CatCurrencyExchange> currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settleCurrent.RequestDate.Value.Date).ToList();
            ////if (currencyExchange.Count == 0)
            ////{
            ////    DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
            ////    currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            ////}

            //var result = surcharge
            //            .GroupBy(s => s.SettlementCode)
            //            //.Where(g => g.Select(s => s.TransactionType).Distinct())
            //            .Select(g => new
            //            {
            //                SettlementCode = g.Key,
            //                TransactionTypes = g.Select(s => s.TransactionType).Distinct().ToList()
            //            }).FirstOrDefault();
            //IQueryable<ShipmentSettlement> dataOperation = Enumerable.Empty<ShipmentSettlement>().AsQueryable();
            //IQueryable<ShipmentSettlement> dataDocument = Enumerable.Empty<ShipmentSettlement>().AsQueryable();
            //bool isHasCl = result?.TransactionTypes.Where(transactionType => transactionType == "CL").FirstOrDefault() != null;
            //bool isHasDoc = result?.TransactionTypes.Where(transactionType => transactionType != "CL").FirstOrDefault() != null;
            //if (isHasCl)
            //{
            //    dataOperation = from sur in surcharge
            //                    join opst in opsTrans on sur.Hblid equals opst.Hblid
            //                    //join cd in cdNos on opst.Hblid.ToString() equals cd.Hblid into cdNoGroups // list các tờ khai theo job
            //                    //from cdNoGroup in cdNoGroups.DefaultIfEmpty()
            //                    //join settle in settlement on sur.SettlementCode equals settle.SettlementNo into settle2
            //                    //from settle in settle2.DefaultIfEmpty()
            //                    //join adv in advanceRequests on sur.AdvanceNo equals adv.AdvanceNo into advGrps
            //                    //from advGrp in advGrps.DefaultIfEmpty()
            //                    // where sur.SettlementCode == settlementNo
            //                    select new ShipmentSettlement
            //                    {
            //                        SettlementNo = sur.SettlementCode,
            //                        JobId = sur.JobNo,
            //                        HBL = sur.Hblno,
            //                        MBL = sur.Mblno,
            //                        HblId = sur.Hblid,
            //                        CurrencyShipment = sur.CurrencyId,
            //                        ShipmentId = opst.Id,
            //                        Type = "OPS",
            //                        AdvanceNo = sur.AdvanceNo,
            //                        IsLocked = opst.IsLocked,
            //                        CustomNo = sur.ClearanceNo
            //                    };
            //}
            //if (isHasDoc)
            //{
            //    dataDocument = from sur in surcharge
            //                   join cstd in csTransD on sur.Hblid equals cstd.Id
            //                   join cst in csTrans on cstd.JobId equals cst.Id into cst2
            //                   from cst in cst2.DefaultIfEmpty()
            //                   //join settle in settlement on sur.SettlementCode equals settle.SettlementNo into settle2
            //                   //from settle in settle2.DefaultIfEmpty()
            //                   //join adv in advanceRequests on sur.AdvanceNo equals adv.AdvanceNo into advGrps
            //                   //from advGrp in advGrps.DefaultIfEmpty()
            //                   where sur.SettlementCode == settlementNo
            //                   select new ShipmentSettlement
            //                   {
            //                       SettlementNo = sur.SettlementCode,
            //                       JobId = sur.JobNo,
            //                       HBL = cstd.Hwbno,
            //                       MBL = cst.Mawb,
            //                       CurrencyShipment = sur.CurrencyId,
            //                       HblId = cstd.Id,
            //                       ShipmentId = cst.Id,
            //                       Type = "DOC",
            //                       AdvanceNo = sur.AdvanceNo,
            //                       IsLocked = cst.IsLocked,
            //                       CustomNo = sur.ClearanceNo
            //                   };
            //}

            //IQueryable<ShipmentSettlement> dataQueryUnionService = dataOperation.Union(dataDocument);

            //var dataGroups = dataQueryUnionService.ToList()
            //                            .GroupBy(x => new { x.SettlementNo, x.HblId, x.AdvanceNo, CustomNo = (string.IsNullOrEmpty(x.CustomNo) ? null : x.CustomNo) }) /* case đặc biệt
            //                            1. Có tạm ứng - không có tk 
            //                            2. Có tạm ứng - Có tờ khai
            //                            */
            //    .Select(x => new ShipmentSettlement
            //    {
            //        SettlementNo = x.Key.SettlementNo,
            //        JobId = x.FirstOrDefault().JobId,
            //        HBL = x.FirstOrDefault().HBL,
            //        MBL = x.FirstOrDefault().MBL,
            //        CurrencyShipment = x.FirstOrDefault().CurrencyShipment,
            //        //TotalAmount = x.Sum(t => t.TotalAmount),
            //        HblId = x.Key.HblId,
            //        Type = x.FirstOrDefault().Type,
            //        ShipmentId = x.FirstOrDefault().ShipmentId,
            //        AdvanceNo = x.Key.AdvanceNo,
            //        IsLocked = x.FirstOrDefault().IsLocked,
            //        CustomNo = x.Key.CustomNo
            //    });

            //List<ShipmentSettlement> shipmentSettlement = new List<ShipmentSettlement>();
            //foreach (ShipmentSettlement item in dataGroups)
            //{
            //    // Lấy thông tin advance theo group settlement.
            //    AdvanceInfo advInfo = GetAdvanceBalanceInfo(item.SettlementNo, item.HblId.ToString(), item.CurrencyShipment, item.AdvanceNo, item.CustomNo);

            //    int roundDecimal = 0;
            //    if (item.CurrencyShipment != AccountingConstants.CURRENCY_LOCAL)
            //    {
            //        roundDecimal = 3;
            //    }

            //    shipmentSettlement.Add(new ShipmentSettlement
            //    {
            //        SettlementNo = item.SettlementNo,
            //        JobId = item.JobId,
            //        MBL = item.MBL,
            //        HBL = item.HBL,
            //        // TotalAmount = item.TotalAmount,
            //        CurrencyShipment = item.CurrencyShipment,
            //        // ChargeSettlements = GetChargesSettlementBySettlementNoAndShipment(item.SettlementNo, item.JobId, item.MBL, item.HBL, item.AdvanceNo, item.CustomNo),
            //        // ChargeSettlements = GetSurchargeDetailSettlement(item.SettlementNo, item.HblId, item.AdvanceNo, item.CustomNo),
            //        HblId = item.HblId,
            //        ShipmentId = item.ShipmentId,
            //        Type = item.Type,
            //        TotalAmount = advInfo.TotalAmount ?? 0,
            //        AdvanceNo = advInfo.AdvanceNo,
            //        AdvanceAmount = advInfo.AdvanceAmount,
            //        Balance = NumberHelper.RoundNumber((advInfo.TotalAmount - advInfo.AdvanceAmount) ?? 0, roundDecimal),
            //        CustomNo = advInfo.CustomNo,
            //        IsLocked = item.IsLocked,
            //        Files = new List<SysImage>()
            //    });
            //}

            //IQueryable<SysImage> FileInShipmentSettlement = sysImageRepository.Get(x => x.Folder == "Settlement"
            //&& x.ObjectId == settleCurrent.Id.ToString()
            //&& !string.IsNullOrEmpty(x.ChildId));

            //if (FileInShipmentSettlement.Count() > 0)
            //{
            //    List<string> hblIds = FileInShipmentSettlement.Select(x => x.ChildId.Substring(0, x.ChildId.IndexOf("/"))).ToList();
            //    var shipmentSettlementHasFile = shipmentSettlement.Where(x => hblIds.Contains(x.HblId.ToString()));

            //    if (shipmentSettlementHasFile.Count() == 0)
            //    {
            //        return shipmentSettlement.OrderByDescending(x => x.JobId).ToList();
            //    }
            //    foreach (ShipmentSettlement item in shipmentSettlementHasFile)
            //    {
            //        string folderChild = string.Format("{0}/", item.HblId.ToString());
            //        if (!string.IsNullOrEmpty(item.AdvanceNo))
            //        {
            //            folderChild += item.AdvanceNo + "/";
            //        }
            //        if (!string.IsNullOrEmpty(item.CustomNo))
            //        {
            //            folderChild += item.CustomNo + "/";
            //        }

            //        item.Files = FileInShipmentSettlement.Where(x => x.ChildId == folderChild).ToList();
            //    }
            //}
            // return shipmentSettlement.OrderByDescending(x => x.JobId).ToList();
        }

        private List<SysImage> GetShipmentAttachFile(string settleCode, Guid hblId, string advanceNo, string customNo)
        {
            List<SysImage> files = new List<SysImage>();

            var id = DataContext.Get(x => x.SettlementNo == settleCode)?.FirstOrDefault().Id;

            string folderChild = string.Format("{0}/", hblId.ToString());
            if (!string.IsNullOrEmpty(advanceNo))
            {
                folderChild += advanceNo + "/";
            }
            if (!string.IsNullOrEmpty(customNo))
            {
                folderChild += customNo + "/";
            }

            files = sysImageRepository.Get(x => x.Folder == "Settlement" && x.ObjectId == id.ToString() && x.ChildId == folderChild).ToList();
            return files;
        }

        public AdvanceInfo GetAdvanceBalanceInfo(string _settlementNo, string _hbl, string _settleCurrency, string _advanceNo, string _clearanceNo = null)
        {
            AdvanceInfo result = new AdvanceInfo();
            string advNo = null, customNo = null;
            IQueryable<CsShipmentSurcharge> surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == _settlementNo);
            var surchargeGrpBy = surcharges.GroupBy(x => new { x.Hblid, x.Mblno, x.Hblno, x.AdvanceNo, ClearanceNo = (string.IsNullOrEmpty(x.ClearanceNo) ? null : x.ClearanceNo) }).ToList();

            var surchargeGrp = surchargeGrpBy.Where(x => x.Key.Hblid.ToString() == _hbl && x.Key.ClearanceNo == _clearanceNo);
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
                              where advR.Hblid.ToString() == _hbl && advR.AdvanceNo == advNo
                              select new
                              {
                                  AdvAmount = advR.Amount * currencyExchangeService.CurrencyExchangeRateConvert(null, advP.RequestDate, advR.RequestCurrency, _settleCurrency), // tính theo tỷ giá ngày request adv và currency settlement
                              };
                result.AdvanceNo = advNo;
                result.AdvanceAmount = advData.ToList().Sum(x => x.AdvAmount);


                // Tính total amount của settlement theo adv đó.
                IQueryable<CsShipmentSurcharge> surChargeToCalculateAmount = csShipmentSurchargeRepo.Get(x => x.SettlementCode == _settlementNo && x.AdvanceNo == advNo && x.Hblid.ToString() == _hbl);
                //result.TotalAmount = surChargeToCalculateAmount.Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, _settleCurrency));
                if (_settleCurrency == AccountingConstants.CURRENCY_LOCAL)
                {
                    result.TotalAmount = surChargeToCalculateAmount.Sum(x => (x.AmountVnd ?? 0) + (x.VatAmountVnd ?? 0));
                }
                else
                {
                    result.TotalAmount = surChargeToCalculateAmount.Sum(x => (x.AmountUsd ?? 0) + (x.VatAmountUsd ?? 0));
                }
            }
            else
            {
                if (surchargeGrp != null && surchargeGrp.Count() > 0)
                {
                    // result.TotalAmount = surchargeGrp?.FirstOrDefault().Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, _settleCurrency));
                    if (_settleCurrency == AccountingConstants.CURRENCY_LOCAL)
                    {
                        result.TotalAmount = surchargeGrp?.FirstOrDefault().Sum(x => (x.AmountVnd ?? 0) + (x.VatAmountVnd ?? 0));
                    }
                    else
                    {
                        result.TotalAmount = surchargeGrp?.FirstOrDefault().Sum(x => (x.AmountUsd ?? 0) + (x.VatAmountUsd ?? 0));
                    }
                }
                else
                {
                    if (_settleCurrency == AccountingConstants.CURRENCY_LOCAL)
                    {
                        result.TotalAmount = surcharges.Sum(x => (x.AmountVnd ?? 0) + (x.VatAmountVnd ?? 0));
                    }
                    else
                    {
                        result.TotalAmount = surcharges.Sum(x => (x.AmountUsd ?? 0) + (x.VatAmountUsd ?? 0));
                    }
                    // result.TotalAmount = surcharges.Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, _settleCurrency));
                }
            }


            if (!string.IsNullOrEmpty(customNo))
            {
                result.CustomNo = customNo;
            }

            return result;

        }

        public List<ShipmentChargeSettlement> GetChargesSettlementBySettlementNoAndShipment(string settlementNo, string JobId, string MBL, string HBL, string AdvNo, string clearanceNo)
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
                                join vatP in payee on sur.VatPartnerId equals vatP.Id into vatPgrps
                                from vatPgrp in vatPgrps.DefaultIfEmpty()
                                join opst in opsTrans on sur.Hblid equals opst.Hblid //into opst2
                                //from opst in opst2.DefaultIfEmpty()
                                where
                                     sur.SettlementCode == settlementNo
                                     && opst.JobNo == JobId
                                     && opst.Hwbno == HBL
                                     && opst.Mblno == MBL
                                     && (string.IsNullOrEmpty(sur.AdvanceNo) ? null : sur.AdvanceNo) == AdvNo
                                     && (!string.IsNullOrEmpty(sur.ClearanceNo) ? (sur.ClearanceNo == clearanceNo) : string.IsNullOrEmpty(clearanceNo))
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
                                    FinalExchangeRate = sur.FinalExchangeRate,
                                    NetAmount = sur.NetAmount,
                                    Vatrate = sur.Vatrate,
                                    Total = sur.Total,
                                    AmountVnd = sur.AmountVnd,
                                    VatAmountVnd = sur.VatAmountVnd,
                                    AmountUSD = sur.AmountUsd,
                                    VatAmountUSD = sur.VatAmountUsd,
                                    PayerId = sur.PayerId,
                                    Payer = ((sur.Type == AccountingConstants.TYPE_CHARGE_BUY || sur.Type == AccountingConstants.TYPE_CHARGE_OTHER) ? pae.ShortName : par.ShortName),//par.ShortName,
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
                                    AdvanceNo = AdvNo,
                                    IsLocked = opst.IsLocked,
                                    KickBack = sur.KickBack,
                                    VatPartnerId = sur.VatPartnerId,
                                    VatPartnerShortName = vatPgrp.ShortName,
                                    SyncedFrom = sur.SyncedFrom,
                                    Soano = sur.Soano,
                                    PaySyncedFrom = sur.PaySyncedFrom,
                                    PaySoano = sur.PaySoano,
                                    DebitNo = sur.DebitNo,
                                    CreditNo = sur.CreditNo,
                                    SyncedFromBy = GetSyncedFrom(sur),
                                    LinkChargeId = sur.LinkChargeId,

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
                               join vatP in payee on sur.VatPartnerId equals vatP.Id into vatPgrps
                               from vatPgrp in vatPgrps.DefaultIfEmpty()
                               join cstd in csTransD on sur.Hblid equals cstd.Id //into cstd2
                               //from cstd in cstd2.DefaultIfEmpty()
                               join cst in csTrans on cstd.JobId equals cst.Id into cst2
                               from cst in cst2.DefaultIfEmpty()
                               where
                                    sur.SettlementCode == settlementNo
                                    && cst.JobNo == JobId
                                    && cstd.Hwbno == HBL
                                    && cst.Mawb == MBL
                                    && (string.IsNullOrEmpty(sur.AdvanceNo) ? null : sur.AdvanceNo) == AdvNo
                                    && (!string.IsNullOrEmpty(sur.ClearanceNo) ? (sur.ClearanceNo == clearanceNo) : string.IsNullOrEmpty(clearanceNo))
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
                                   FinalExchangeRate = sur.FinalExchangeRate,
                                   NetAmount = sur.NetAmount,
                                   Vatrate = sur.Vatrate,
                                   Total = sur.Total,
                                   AmountVnd = sur.AmountVnd,
                                   VatAmountVnd = sur.VatAmountVnd,
                                   AmountUSD = sur.AmountUsd,
                                   VatAmountUSD = sur.VatAmountUsd,
                                   PayerId = sur.PayerId,
                                   Payer = ((sur.Type == AccountingConstants.TYPE_CHARGE_BUY || sur.Type == AccountingConstants.TYPE_CHARGE_OTHER) ? pae.ShortName : par.ShortName),//par.ShortName,
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
                                   AdvanceNo = AdvNo,
                                   IsLocked = cst.IsLocked,
                                   KickBack = sur.KickBack,
                                   VatPartnerId = sur.VatPartnerId,
                                   VatPartnerShortName = vatPgrp.ShortName,
                                   SyncedFrom = sur.SyncedFrom,
                                   Soano = sur.Soano,
                                   PaySyncedFrom = sur.PaySyncedFrom,
                                   PaySoano = sur.PaySoano,
                                   DebitNo = sur.DebitNo,
                                   CreditNo = sur.CreditNo,
                                   SyncedFromBy = GetSyncedFrom(sur),
                                   LinkChargeId = sur.LinkChargeId
                               };
            var data = dataOperation.Union(dataDocument).ToList();

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settlementNo"></param>
        /// <param name="getCopyCharge">true: function copy charge in settle, false: get detail charge no group in settle</param>
        /// <returns></returns>
        public IQueryable<ShipmentChargeSettlement> GetListShipmentChargeSettlementNoGroup(string settlementNo, bool getCopyCharge = false)
        {
            var surcharge = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo);
            var charge = catChargeRepo.Get();
            var unit = catUnitRepo.Get();
            var payer = catPartnerRepo.Get();
            var payee = catPartnerRepo.Get();
            var vatPartners = catPartnerRepo.Get();
            var opsTrans = opsTransactionRepo.Get(x => getCopyCharge ? (x.OfficeId == currentUser.OfficeID) : true);  // Lấy theo office current user
            var csTransD = csTransactionDetailRepo.Get(x => getCopyCharge ? (x.OfficeId == currentUser.OfficeID) : true);  // Lấy theo office current user
            var csTrans = csTransactionRepo.Get();
            var userRepo = sysUserRepo.Get();

            var dataOperation = from sur in surcharge
                                join cc in charge on sur.ChargeId equals cc.Id into cc2
                                from cc in cc2.DefaultIfEmpty()
                                join u in unit on sur.UnitId equals u.Id into u2
                                from u in u2.DefaultIfEmpty()
                                join par in payer on sur.PayerId equals par.Id into par2
                                from par in par2.DefaultIfEmpty()
                                join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                                from pae in pae2.DefaultIfEmpty()
                                join vatP in payee on sur.VatPartnerId equals vatP.Id into vatPgrps
                                from vatPgrp in vatPgrps.DefaultIfEmpty()
                                join opst in opsTrans on sur.Hblid equals opst.Hblid
                                join user in userRepo on opst.UserCreated equals user.Id into sysUser
                                from user in sysUser.DefaultIfEmpty()
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
                                    FinalExchangeRate = sur.FinalExchangeRate,
                                    Quantity = sur.Quantity,
                                    UnitId = sur.UnitId,
                                    UnitName = u.UnitNameEn,
                                    UnitPrice = sur.UnitPrice,
                                    CurrencyId = sur.CurrencyId,
                                    NetAmount = sur.NetAmount,
                                    Vatrate = sur.Vatrate,
                                    Total = sur.Total,
                                    VatAmountVnd = sur.VatAmountVnd,
                                    AmountVnd = sur.AmountVnd,
                                    VatAmountUSD = sur.VatAmountUsd,
                                    AmountUSD = sur.AmountUsd,
                                    TotalAmountVnd = sur.VatAmountVnd + sur.AmountVnd,
                                    PayerId = sur.PayerId,
                                    Payer = ((sur.Type == AccountingConstants.TYPE_CHARGE_BUY || sur.Type == AccountingConstants.TYPE_CHARGE_OTHER) ? pae.ShortName : par.ShortName),//par.ShortName,
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
                                    AdvanceNo = !getCopyCharge ? sur.AdvanceNo : GetAdvanceNoSettle(sur.AdvanceNo, opst.JobNo),
                                    OriginAdvanceNo = !getCopyCharge ? sur.AdvanceNo : GetAdvanceNoSettle(sur.AdvanceNo, opst.JobNo),
                                    ShipmentId = opst.Id,
                                    TypeService = "OPS",
                                    IsLocked = opst.IsLocked,
                                    PICName = user.Username,
                                    KickBack = sur.KickBack,
                                    VatPartnerId = sur.VatPartnerId,
                                    VatPartnerShortName = vatPgrp.ShortName,
                                    SyncedFrom = sur.SyncedFrom,
                                    Soano = sur.Soano,
                                    PaySyncedFrom = sur.PaySyncedFrom,
                                    PaySoano = sur.PaySoano,
                                    DebitNo = sur.DebitNo,
                                    CreditNo = sur.CreditNo,
                                    SyncedFromBy = GetSyncedFrom(sur),
                                    LinkChargeId = sur.LinkChargeId

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
                               join vatP in payee on sur.VatPartnerId equals vatP.Id into vatPgrps
                               from vatPgrp in vatPgrps.DefaultIfEmpty()
                               join cstd in csTransD on sur.Hblid equals cstd.Id //into cstd2
                               //from cstd in cstd2.DefaultIfEmpty()
                               join cst in csTrans on cstd.JobId equals cst.Id into cst2
                               from cst in cst2.DefaultIfEmpty()
                               join user in userRepo on cst.UserCreated equals user.Id into sysUser
                               from user in sysUser.DefaultIfEmpty()
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
                                   FinalExchangeRate = sur.FinalExchangeRate,
                                   Quantity = sur.Quantity,
                                   UnitId = sur.UnitId,
                                   UnitName = u.UnitNameEn,
                                   UnitPrice = sur.UnitPrice,
                                   CurrencyId = sur.CurrencyId,
                                   NetAmount = sur.NetAmount,
                                   Vatrate = sur.Vatrate,
                                   Total = sur.Total,
                                   VatAmountVnd = sur.VatAmountVnd,
                                   AmountVnd = sur.AmountVnd,
                                   VatAmountUSD = sur.VatAmountUsd,
                                   AmountUSD = sur.AmountUsd,
                                   TotalAmountVnd = sur.VatAmountVnd + sur.AmountVnd,
                                   PayerId = sur.PayerId,
                                   Payer = ((sur.Type == AccountingConstants.TYPE_CHARGE_BUY || sur.Type == AccountingConstants.TYPE_CHARGE_OTHER) ? pae.ShortName : par.ShortName),//par.ShortName,
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
                                   AdvanceNo = !getCopyCharge ? sur.AdvanceNo : GetAdvanceNoSettle(sur.AdvanceNo, cst.JobNo),
                                   OriginAdvanceNo = !getCopyCharge ? sur.AdvanceNo : GetAdvanceNoSettle(sur.AdvanceNo, cst.JobNo),
                                   ShipmentId = cst.Id,
                                   TypeService = "DOC",
                                   IsLocked = cst.IsLocked,
                                   PICName = user.Username,
                                   KickBack = sur.KickBack,
                                   VatPartnerId = sur.VatPartnerId,
                                   VatPartnerShortName = vatPgrp.ShortName,
                                   SyncedFrom = sur.SyncedFrom,
                                   Soano = sur.Soano,
                                   PaySyncedFrom = sur.PaySyncedFrom,
                                   PaySoano = sur.PaySoano,
                                   DebitNo = sur.DebitNo,
                                   CreditNo = sur.CreditNo,
                                   SyncedFromBy = GetSyncedFrom(sur),
                                   LinkChargeId = sur.LinkChargeId

                               };
            var data = dataOperation.Union(dataDocument);
            return data.OrderByDescending(x => x.JobId);

        }

        private string GetSyncedFrom(CsShipmentSurcharge surcharge)
        {
            string _syncedFromBy = string.Empty;

            if (surcharge.IsFromShipment == false && surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH)
            {
                switch (surcharge.SyncedFrom)
                {
                    case "SOA":
                        _syncedFromBy = surcharge.Soano;
                        break;
                    case "CDNOTE":
                        _syncedFromBy = surcharge.DebitNo;
                        break;
                    default:
                        break;
                }
            }

            return _syncedFromBy;
        }

        /// <summary>
        /// Get advance no from adv no in surcharge with COPY CHARGE
        /// </summary>
        /// <param name="advanceNoCharge"></param>
        /// <param name="requester"></param>
        /// <returns></returns>
        private string GetAdvanceNoSettle(string advanceNoCharge, string jobNo)
        {
            var jobOpsDetail = opsTransactionRepo.Get(x => x.JobNo == jobNo).FirstOrDefault();
            var jobTransDetail = csTransactionRepo.Get(x => x.JobNo == jobNo).FirstOrDefault();
            if (jobOpsDetail != null)
            {
                var personAssign = opsStageAssignedRepository.Get(x => x.JobId == jobOpsDetail.Id).Select(x => x.MainPersonInCharge).ToList();
                if (acctAdvancePaymentRepo.Any(x => x.AdvanceNo == advanceNoCharge && (x.Requester == jobOpsDetail.UserCreated || x.Requester == jobOpsDetail.BillingOpsId || personAssign.Any(z => z == x.Requester))))
                {
                    return advanceNoCharge;
                }
            }
            if (jobTransDetail != null)
            {
                var personAssign = opsStageAssignedRepository.Get(x => x.JobId == jobTransDetail.Id).Select(x => x.MainPersonInCharge).ToList();
                if (acctAdvancePaymentRepo.Any(x => x.AdvanceNo == advanceNoCharge && (x.Requester == jobTransDetail.UserCreated || x.Requester == jobTransDetail.PersonIncharge || personAssign.Any(z => z == x.Requester))))
                {
                    return advanceNoCharge;
                }
            }
            return string.Empty;
        }

        #endregion --- DETAILS SETTLEMENT PAYMENT ---

        #region --- PAYMENT MANAGEMENT ---
        public List<AdvancePaymentMngt> GetAdvancePaymentMngts(string jobId, string mbl, string hbl, string requester)
        {
            var advance = acctAdvancePaymentRepo.Get();
            var request = acctAdvanceRequestRepo.Get();
            //Chỉ lấy những advance có status là Done => update sprint22: lấy tất cả
            var data = from req in request
                       join ad in advance on req.AdvanceNo equals ad.AdvanceNo into ad2
                       from ad in ad2.DefaultIfEmpty()
                       where
                       //ad.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE
                       req.JobId == jobId
                       && req.Mbl == mbl
                       && req.Hbl == hbl
                       && ad.Requester == requester
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
                    TotalAmount = NumberHelper.RoundNumber(item.TotalAmount, item.AdvanceCurrency == AccountingConstants.CURRENCY_LOCAL ? 0 : 2),
                    AdvanceCurrency = item.AdvanceCurrency,
                    AdvanceDate = item.AdvanceDate,
                    ChargeAdvancePaymentMngts = request.Where(x => x.AdvanceNo == item.AdvanceNo && x.JobId == jobId && x.Mbl == mbl && x.Hbl == hbl)
                    .Select(x => new ChargeAdvancePaymentMngt { AdvanceNo = x.AdvanceNo, TotalAmount = x.Amount.Value, AdvanceCurrency = x.RequestCurrency, Description = x.Description }).ToList()
                });
            }
            return dataResult;
        }

        public List<SettlementPaymentMngt> GetSettlementPaymentMngts(string jobId, string mbl, string hbl, string requester)
        {
            var settlement = DataContext.Get();
            var surcharge = csShipmentSurchargeRepo.Get();
            var charge = catChargeRepo.Get();
            var payee = catPartnerRepo.Get();
            var payer = catPartnerRepo.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();
            // [CR] Lấy theo exchange rate of charge
            //Quy đổi tỉ giá theo ngày hiện tại, nếu tỉ giá ngày hiện tại không có thì lấy ngày mới nhất
            //var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == DateTime.Now.Date).ToList();
            //if (currencyExchange.Count == 0)
            //{
            //    DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
            //    currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            //}

            // Chỉ lấy ra những settlement có status là done => update sprint22: lấy tất cả
            var dataOperation = from settle in settlement
                                join sur in surcharge on settle.SettlementNo equals sur.SettlementCode into sur2
                                from sur in sur2.DefaultIfEmpty()
                                join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                                from pae in pae2.DefaultIfEmpty()
                                join opst in opsTrans on sur.Hblid equals opst.Hblid
                                where
                                     //settle.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE
                                     opst.JobNo == jobId
                                     && opst.Hwbno == hbl
                                     && opst.Mblno == mbl
                                     && settle.Requester == requester
                                select new SettlementPaymentMngt
                                {
                                    SettlementNo = settle.SettlementNo,
                                    TotalAmount = settle.SettlementCurrency == AccountingConstants.CURRENCY_LOCAL ? ((sur.AmountVnd ?? 0) + (sur.VatAmountVnd ?? 0)) :
                                                                                                                    ((sur.AmountUsd ?? 0) + (sur.VatAmountUsd ?? 0)),
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
                                    //settle.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE
                                    cst.JobNo == jobId
                                    && cstd.Hwbno == hbl
                                    && cst.Mawb == mbl
                                    && settle.Requester == requester
                               select new SettlementPaymentMngt
                               {
                                   SettlementNo = settle.SettlementNo,
                                   TotalAmount = settle.SettlementCurrency == AccountingConstants.CURRENCY_LOCAL ? ((sur.AmountVnd ?? 0) + (sur.VatAmountVnd ?? 0)) :
                                                                                                                    ((sur.AmountUsd ?? 0) + (sur.VatAmountUsd ?? 0)),
                                   SettlementCurrency = settle.SettlementCurrency,
                                   ChargeCurrency = sur.CurrencyId,
                                   SettlementDate = settle.DatetimeCreated
                               };
            var data = dataOperation.Union(dataDocument);

            var dataGrp = data.ToList().GroupBy(x => new { x.SettlementNo, x.SettlementCurrency, x.SettlementDate })
                .Select(s => new SettlementPaymentMngt
                {
                    SettlementNo = s.Key.SettlementNo,
                    //TotalAmount = s.Sum(su => su.TotalAmount * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, su.ChargeCurrency, su.SettlementCurrency)),
                    TotalAmount = s.Sum(su => su.TotalAmount),
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
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            var surcharge = csShipmentSurchargeRepo.Get();
            var charge = catChargeRepo.Get();
            var payee = catPartnerRepo.Get();
            var payer = catPartnerRepo.Get();
            var opsTrans = opsTransactionRepo.Get();
            var csTransD = csTransactionDetailRepo.Get();
            var csTrans = csTransactionRepo.Get();
            // get settlement status
            var settleStatus = settlement.StatusApproval;
            // get requester name
            var userRequest = sysUserRepo.Get(x => x.Id == settlement.Requester).FirstOrDefault().EmployeeId;
            var requester = sysEmployeeRepo.Get(em => em.Id == userRequest).FirstOrDefault()?.EmployeeNameVn;

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
                                    AdvanceNo = sur.AdvanceNo,
                                    ChargeCode = cc.Code,
                                    ChargeName = cc.ChargeNameEn,
                                    TotalAmount = NumberHelper.RoundNumber(sur.Total, sur.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? 0 : 2),
                                    SettlementCurrency = sur.CurrencyId,
                                    OBHPartner = (sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),
                                    Payer = (sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName),
                                    InvoiceNo = sur.InvoiceNo,
                                    SettlementStatus = settleStatus,
                                    Requester = requester
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
                                   AdvanceNo = sur.AdvanceNo,
                                   ChargeCode = cc.Code,
                                   ChargeName = cc.ChargeNameEn,
                                   TotalAmount = NumberHelper.RoundNumber(sur.Total, sur.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? 0 : 2),
                                   SettlementCurrency = sur.CurrencyId,
                                   OBHPartner = (sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? pae.ShortName : par.ShortName),
                                   Payer = (sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? pae.ShortName : par.ShortName),
                                   InvoiceNo = sur.InvoiceNo,
                                   SettlementStatus = settleStatus,
                                   Requester = requester
                               };
            var data = dataOperation.Union(dataDocument);
            return data;
        }
        #endregion --- PAYMENT MANAGEMENT ---

        #region -- GET EXISITS CHARGE --
        private List<sp_GetDataExistsCharge> GetExistingChargeData(ExistsChargeCriteria criteria)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@serviceDateFrom", Value = criteria.serviceDateFrom },
                new SqlParameter(){ ParameterName = "@serviceDateTo", Value = criteria.serviceDateTo },
                new SqlParameter(){ ParameterName = "@partnerId", Value = criteria.partnerId },
                new SqlParameter(){ ParameterName = "@jobIds", Value = string.Join(';',criteria.jobIds) },
                new SqlParameter(){ ParameterName = "@mbls", Value = string.Join(';',criteria.mbls) },
                new SqlParameter(){ ParameterName = "@hbls", Value = string.Join(';',criteria.hbls) },
                new SqlParameter(){ ParameterName = "@customNos", Value = string.Join(';',criteria.customNos) },
                new SqlParameter(){ ParameterName = "@soaNo", Value = string.Join(';',criteria.soaNo) },
                new SqlParameter(){ ParameterName = "@creditNo", Value = string.Join(';',criteria.creditNo) },
                new SqlParameter(){ ParameterName = "@servicesType", Value = criteria.servicesType },
                new SqlParameter(){ ParameterName = "@personInCharge", Value = criteria.personInCharge },
                new SqlParameter(){ ParameterName = "@requester", Value = criteria.requester },
                new SqlParameter(){ ParameterName = "@office", Value = currentUser.OfficeID }
            };
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDataExistsCharge>(parameters);
            return list;
        }

        public List<ShipmentChargeSettlement> GetExistsCharge(ExistsChargeCriteria criteria)
        {
            #region soure old
            //Chỉ lấy ra những phí chứng từ (thuộc phí credit + partner hay những phí thuộc đối tượng payer + partner)
            //var surcharge = csShipmentSurchargeRepo
            //    .Get(x =>
            //            x.IsFromShipment == true
            //         && ((x.Type == AccountingConstants.TYPE_CHARGE_BUY && x.PaymentObjectId == criteria.partnerId && string.IsNullOrEmpty(x.SyncedFrom))
            //         || (x.Type == AccountingConstants.TYPE_CHARGE_OBH && x.PayerId == criteria.partnerId && string.IsNullOrEmpty(x.PaySyncedFrom)))
            //         && string.IsNullOrEmpty(x.SettlementCode)
            //    );
            // Get data source
            //var charge = catChargeRepo.Get();
            //var payer = catPartnerRepo.Get();
            //var payee = catPartnerRepo.Get();
            //var opsTrans = opsTransactionRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED);
            //var csTrans = csTransactionRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED);

            // Data search = jobNo
            //criteria.jobIds = criteria.jobIds.Where(x => !string.IsNullOrEmpty(x)).ToList();
            //if (criteria.jobIds != null && criteria.jobIds.Count() > 0)
            //{
            //    surcharge = surcharge.Where(x => criteria.jobIds.Any(job => job == x.JobNo) && x.JobNo != null);
            //}
            // Data search = mblNo
            //criteria.mbls = criteria.mbls.Where(x => !string.IsNullOrEmpty(x)).ToList();
            //if (criteria.mbls != null && criteria.mbls.Count() > 0)
            //{
            //    surcharge = surcharge.Where(x => criteria.mbls.Any(mbl => mbl == x.Mblno) && x.Mblno != null);
            //}
            // Data search = hblNo
            //criteria.hbls = criteria.hbls.Where(x => !string.IsNullOrEmpty(x)).ToList();
            //if (criteria.hbls != null && criteria.hbls.Count() > 0)
            //{
            //    surcharge = surcharge.Where(x => criteria.hbls.Any(hbl => hbl == x.Hblno) && x.Hblno != null);
            //}
            // Data search = soaNo
            //criteria.soaNo = criteria.soaNo.Where(x => !string.IsNullOrEmpty(x)).ToList();
            //if (criteria.soaNo != null && criteria.soaNo.Count() > 0)
            //{
            //    surcharge = surcharge.Where(x => criteria.soaNo.IndexOf(x.PaySoano ?? "") >= 0);
            //}
            // Data search = customNo
            //criteria.customNos = criteria.customNos.Where(x => !string.IsNullOrEmpty(x)).ToList();
            //var clearanceData = customClearanceRepo.Get();
            //if (criteria.customNos != null && criteria.customNos.Count() > 0)
            //{
            //    clearanceData = customClearanceRepo.Get(x => criteria.customNos.Any(cus => cus == x.ClearanceNo)).OrderBy(x => x.ClearanceDate);
            //    var clearanceDataGroup = clearanceData.GroupBy(x => x.JobNo).Select(x => x.Key).ToList();
            //    opsTrans = opsTrans.Where(x => clearanceDataGroup.Any(cl => cl == x.JobNo));
            //}
            // Data search = creditNo
            //criteria.creditNo = criteria.creditNo.Where(x => !string.IsNullOrEmpty(x)).ToList();
            //if (criteria.creditNo != null && criteria.creditNo.Count() > 0)
            //{
            //    surcharge = surcharge.Where(x => criteria.creditNo.IndexOf(x.CreditNo ?? "") >= 0);
            //}
            // Data search = ServiceDate
            //if (criteria.serviceDateFrom != null || criteria.serviceDateTo != null)
            //{
            //    opsTrans = opsTrans.Where(x => x.ServiceDate.HasValue ? criteria.serviceDateFrom.Value.Date <= x.ServiceDate.Value.Date && x.ServiceDate.Value.Date <= criteria.serviceDateTo.Value.Date : false);
            //    csTrans = csTrans.Where(x => x.ServiceDate.HasValue ? (criteria.serviceDateFrom.Value.Date <= x.ServiceDate.Value.Date && x.ServiceDate.Value.Date <= criteria.serviceDateTo.Value.Date) : false);
            //}
            // Data search = serviceType
            //if (!string.IsNullOrEmpty(criteria.servicesType))
            //{
            //    surcharge = surcharge.Where(x => criteria.servicesType.Contains(x.TransactionType));
            //}
            // Data search = PIC (PIC = UserCreated of job)
            //if (!string.IsNullOrEmpty(criteria.personInCharge))
            //{
            //    opsTrans = opsTrans.Where(x => criteria.personInCharge.ToLower().Contains(x.UserCreated.ToLower()));
            //    csTrans = csTrans.Where(x => criteria.personInCharge.ToLower().Contains(x.UserCreated.ToLower()));
            //}

            //var operationLst = (from sur in surcharge
            //                     join cc in charge on sur.ChargeId equals cc.Id into cc2
            //                     from cc in cc2.DefaultIfEmpty()
            //                     join u in unit on sur.UnitId equals u.Id into u2
            //                     from u in u2.DefaultIfEmpty()
            //                     join par in payer on sur.PayerId equals par.Id into par2
            //                     from par in par2.DefaultIfEmpty()
            //                     join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
            //                     from pae in pae2.DefaultIfEmpty()
            //                     join opst in opsTrans on sur.Hblid equals opst.Hblid
            //                     join vatP in payee on sur.VatPartnerId equals vatP.Id into vatPgrps
            //                     from vatPgrp in vatPgrps.DefaultIfEmpty()
            //                         //join adv in advanceRequests on sur.AdvanceNo equals adv.AdvanceNo into advGrps
            //                         //from advGrp in advGrps.DefaultIfEmpty()
            //                     join user in userRepo on opst.UserCreated equals user.Id into sysUser
            //                     from user in sysUser.DefaultIfEmpty()
            //                     select new ShipmentChargeSettlement
            //                     {
            //                         Id = sur.Id,
            //                         JobId = sur.JobNo,
            //                         MBL = sur.Mblno,
            //                         HBL = sur.Hblno,
            //                         Hblid = sur.Hblid,
            //                         Type = sur.Type,
            //                         //SettlementCode = sur.SettlementCode,
            //                         ChargeId = sur.ChargeId,
            //                         ChargeCode = cc.Code,
            //                         ChargeName = cc.ChargeNameEn,
            //                         Quantity = sur.Quantity,
            //                         UnitId = sur.UnitId,
            //                         UnitName = u.UnitNameEn,
            //                         UnitPrice = sur.UnitPrice,
            //                         CurrencyId = sur.CurrencyId,
            //                         FinalExchangeRate = sur.FinalExchangeRate,
            //                         NetAmount = sur.NetAmount,
            //                         Vatrate = sur.Vatrate,
            //                         Total = sur.Total,
            //                         AmountVnd = sur.AmountVnd,
            //                         VatAmountVnd = sur.VatAmountVnd,
            //                         TotalAmountVnd = sur.AmountVnd + sur.VatAmountVnd,
            //                         AmountUSD = sur.AmountUsd,
            //                         VatAmountUSD = sur.VatAmountUsd,
            //                         PayerId = sur.PayerId,
            //                         Payer = pae.ShortName,
            //                         PaymentObjectId = sur.PaymentObjectId,
            //                         OBHPartnerName = sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? par.ShortName : string.Empty,
            //                         InvoiceNo = sur.InvoiceNo,
            //                         SeriesNo = sur.SeriesNo,
            //                         InvoiceDate = sur.InvoiceDate,
            //                         ContNo = sur.ContNo,
            //                         Notes = sur.Notes,
            //                         IsFromShipment = sur.IsFromShipment,
            //                         PICName = user.Username,
            //                         //PICName = opst.UserCreated,
            //                         KickBack = sur.KickBack,
            //                         VatPartnerId = sur.VatPartnerId,
            //                         VatPartnerShortName = vatPgrp.ShortName,
            //                     });
            //var dataOperation = operationLst.ToList();
            //foreach (var item in dataOperation)
            //{
            //    var jobId = item.JobId;
            //    var clearanceDataList = clearanceData.Where(x => x.JobNo == jobId);
            //    if (clearanceDataList.Count() > 0)
            //    {
            //        item.ClearanceNo = clearanceDataList.FirstOrDefault() == null ? string.Empty : clearanceDataList.OrderBy(x => x.ClearanceDate).First().ClearanceNo;
            //    }
            //}
            //if (criteria.customNos != null && criteria.customNos.Count() > 0)
            //{
            //    return dataOperation;
            //}

            //var documentLst = (from sur in surcharge
            //                    join cc in charge on sur.ChargeId equals cc.Id into cc2
            //                    from cc in cc2.DefaultIfEmpty()
            //                    join u in unit on sur.UnitId equals u.Id into u2
            //                    from u in u2.DefaultIfEmpty()
            //                    join par in payer on sur.PayerId equals par.Id into par2
            //                    from par in par2.DefaultIfEmpty()
            //                    join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
            //                    from pae in pae2.DefaultIfEmpty()
            //                    join cst in csTrans on sur.JobNo equals cst.JobNo
            //                    join vatP in payee on sur.VatPartnerId equals vatP.Id into vatPgrps
            //                    from vatPgrp in vatPgrps.DefaultIfEmpty()
            //                        //join adv in advanceRequests on sur.AdvanceNo equals adv.AdvanceNo into advGrps
            //                        //from advGrp in advGrps.DefaultIfEmpty()
            //                    join user in userRepo on cst.UserCreated equals user.Id into sysUser
            //                    from user in sysUser.DefaultIfEmpty()
            //                    select new ShipmentChargeSettlement
            //                    {
            //                        Id = sur.Id,
            //                        JobId = cst.JobNo,
            //                        MBL = cst.Mawb,
            //                        HBL = sur.Hblno,
            //                        Hblid = sur.Hblid,
            //                        Type = sur.Type,
            //                        //SettlementCode = sur.SettlementCode,
            //                        ChargeId = sur.ChargeId,
            //                        ChargeCode = cc.Code,
            //                        ChargeName = cc.ChargeNameEn,
            //                        Quantity = sur.Quantity,
            //                        UnitId = sur.UnitId,
            //                        UnitName = u.UnitNameEn,
            //                        UnitPrice = sur.UnitPrice,
            //                        CurrencyId = sur.CurrencyId,
            //                        FinalExchangeRate = sur.FinalExchangeRate,
            //                        NetAmount = sur.NetAmount,
            //                        Vatrate = sur.Vatrate,
            //                        Total = sur.Total,
            //                        AmountVnd = sur.AmountVnd,
            //                        VatAmountVnd = sur.VatAmountVnd,
            //                        TotalAmountVnd = sur.AmountVnd + sur.VatAmountVnd,
            //                        AmountUSD = sur.AmountUsd,
            //                        VatAmountUSD = sur.VatAmountUsd,
            //                        PayerId = sur.PayerId,
            //                        Payer = pae.ShortName,
            //                        PaymentObjectId = sur.PaymentObjectId,
            //                        OBHPartnerName = sur.Type == AccountingConsp_GetDebitDetailByArgIdstants.TYPE_CHARGE_OBH ? par.ShortName : string.Empty,
            //                        InvoiceNo = sur.InvoiceNo,
            //                        SeriesNo = sur.SeriesNo,
            //                        InvoiceDate = sur.InvoiceDate,
            //                        ClearanceNo = sur.ClearanceNo,
            //                        ContNo = sur.ContNo,
            //                        Notes = sur.Notes,
            //                        IsFromShipment = sur.IsFromShipment,
            //                        //AdvanceNo = advGrp.AdvanceNo,
            //                        PICName = user.Username,
            //                        KickBack = sur.KickBack,
            //                        VatPartnerId = sur.VatPartnerId,
            //                        VatPartnerShortName = vatPgrp.ShortName,
            //                    });
            //var dataDocument = documentLst.ToList();
            //var data = dataDocument.Union(dataOperation);
            #endregion
            var dataShiment = GetExistingChargeData(criteria).Select(x => new ShipmentChargeSettlement
            {
                Id = x.Id,
                JobId = x.JobId,
                MBL = x.MBL,
                HBL = x.HBL,
                Hblid = x.Hblid,
                Type = x.Type,
                //SettlementCode = x.SettlementCode,
                ChargeId = x.ChargeId,
                ChargeCode = x.ChargeCode,
                ChargeName = x.ChargeName,
                Quantity = x.Quantity,
                UnitId = x.UnitId,
                UnitName = x.UnitName,
                UnitPrice = x.UnitPrice,
                CurrencyId = x.CurrencyId,
                FinalExchangeRate = x.FinalExchangeRate,
                NetAmount = x.NetAmount,
                Vatrate = x.Vatrate,
                Total = x.Total,
                AmountVnd = x.AmountVnd,
                VatAmountVnd = x.VatAmountVnd,
                TotalAmountVnd = x.AmountVnd + x.VatAmountVnd,
                AmountUSD = x.AmountUSD,
                VatAmountUSD = x.VatAmountUSD,
                PayerId = x.PayerId,
                Payer = x.Payer,
                PaymentObjectId = x.PaymentObjectId,
                OBHPartnerName = x.OBHPartnerName,
                InvoiceNo = x.InvoiceNo,
                SeriesNo = x.SeriesNo,
                InvoiceDate = x.InvoiceDate,
                ClearanceNo = x.ClearanceNo,
                ContNo = x.ContNo,
                Notes = x.Notes,
                IsFromShipment = x.IsFromShipment,
                //AdvanceNo = advGrp.AdvanceNo,
                PICName = x.PICName,
                KickBack = x.KickBack,
                VatPartnerId = x.VatPartnerId,
                VatPartnerShortName = x.VatPartnerShortName
            });
            return dataShiment.ToList();
        }
        #endregion -- GET EXISITS CHARGE --

        #region -- INSERT & UPDATE SETTLEMENT PAYMENT --
        public ResultModel CheckDuplicateShipmentSettlement(CheckDuplicateShipmentSettlementCriteria criteria, out List<DuplicateShipmentSettlementResultModel> modelResult)
        {
            var result = new ResultModel();
            modelResult = new List<DuplicateShipmentSettlementResultModel>();
            if (criteria.TypeService != "OPS" || (criteria.TypeService == "OPS" && (!string.IsNullOrEmpty(criteria.CustomNo) || !string.IsNullOrEmpty(criteria.InvoiceNo) || !string.IsNullOrEmpty(criteria.ContNo))))
            {
                var surChargeExists = csShipmentSurchargeRepo.Get(x =>
                            (criteria.SurchargeID == Guid.Empty ? true : x.Id != criteria.SurchargeID)
                            && x.SettlementCode != criteria.SettlementNo
                            && x.ChargeId == criteria.ChargeID
                            && x.Hblid == criteria.HBLID
                            && (criteria.TypeCharge == AccountingConstants.TYPE_CHARGE_BUY ? x.PaymentObjectId == criteria.Partner : (criteria.TypeCharge == AccountingConstants.TYPE_CHARGE_OBH ? x.PayerId == criteria.Partner : true))
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
                    string msg = string.Join("</br>", data.ToList()
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
            return result;
        }

        public ResultHandle CheckDuplicateListShipmentsSettlement(List<CheckDuplicateShipmentSettlementCriteria> charges)
        {
            var modelResult = new List<DuplicateShipmentSettlementResultModel>();
            var charge = catChargeRepo.Get();
            foreach (var criteria in charges)
            {
                if (!string.IsNullOrEmpty(criteria.CustomNo) || !string.IsNullOrEmpty(criteria.InvoiceNo) || !string.IsNullOrEmpty(criteria.ContNo))
                {
                    var surChargeExists = csShipmentSurchargeRepo.Get(x =>
                                (criteria.SurchargeID == Guid.Empty ? true : (x.Id != criteria.SurchargeID))
                                && x.SettlementCode != criteria.SettlementNo
                                && x.ChargeId == criteria.ChargeID
                                && x.Hblid == criteria.HBLID
                                && (criteria.TypeCharge == AccountingConstants.TYPE_CHARGE_BUY ? x.PaymentObjectId == criteria.Partner : (criteria.TypeCharge == AccountingConstants.TYPE_CHARGE_OBH ? x.PayerId == criteria.Partner : true))
                                && x.ClearanceNo == criteria.CustomNo
                                && x.InvoiceNo == criteria.InvoiceNo
                                && x.ContNo == criteria.ContNo
                                && x.Notes == criteria.Notes
                        );

                    if (surChargeExists.Select(s => s.Id).Any())
                    {
                        var data = from sur in surChargeExists
                                   join chg in charge on sur.ChargeId equals chg.Id
                                   select new { criteria.JobNo, criteria.HBLNo, criteria.MBLNo, ChargeName = chg.ChargeNameEn, sur.SettlementCode, sur.ChargeId };
                        string msg = string.Join("</br>", data.ToList()
                            .Select(s => !string.IsNullOrEmpty(s.JobNo)
                            && !string.IsNullOrEmpty(s.HBLNo)
                            && !string.IsNullOrEmpty(s.MBLNo)
                            ? string.Format(@"Shipment: [{0}-{1}-{2}] Charge [{3}] has already existed in settlement: {4}", s.JobNo, s.HBLNo, s.MBLNo, s.ChargeName, s.SettlementCode)
                            : string.Format(@"Charge [{0}] has already existed in settlement: {1}.", s.ChargeName, s.SettlementCode)));

                        modelResult = data.Select(x => new DuplicateShipmentSettlementResultModel
                        {
                            JobNo = x.JobNo,
                            MBLNo = x.MBLNo,
                            HBLNo = x.HBLNo,
                            ChargeId = x.ChargeId
                        }).ToList();
                        return new ResultHandle() { Status = false, Message = msg, Data = modelResult };
                    }
                }
            }
            return new ResultHandle() { Status = true };
        }

        public HandleState AddSettlementPayment(CreateUpdateSettlementModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var userCurrent = currentUser.UserID;
                var entity = mapper.Map<AcctSettlementPayment>(model.Settlement);
                entity.Id = model.Settlement.Id = Guid.NewGuid();
                entity.SettlementNo = string.Empty;
                entity.StatusApproval = model.Settlement.StatusApproval = string.IsNullOrEmpty(model.Settlement.StatusApproval) ? AccountingConstants.STATUS_APPROVAL_NEW : model.Settlement.StatusApproval;
                entity.UserCreated = entity.UserModified = userCurrent;
                entity.DatetimeCreated = entity.DatetimeModified = DateTime.Now;
                entity.GroupId = currentUser.GroupId;
                entity.DepartmentId = currentUser.DepartmentId;
                entity.OfficeId = currentUser.OfficeID;
                entity.CompanyId = currentUser.CompanyID;
                entity.BankAccountNo = StringHelper.RemoveSpecialChars(entity.BankAccountNo, Constants.spaceCharacter);
                entity.BankAccountName = entity.BankName = entity.Note = null;

                var addResult = databaseUpdateService.InsertDataToDB(entity);
                if (!addResult.Status)
                {
                    return new HandleState((object)"Fail to create settlment. Please try again.");
                }
                var settlement = mapper.Map<AcctSettlementPayment>(DataContext.Get(x => x.Id == entity.Id).FirstOrDefault());
                databaseUpdateService.LogAddEntity(settlement);
                model.Settlement.SettlementNo = settlement.SettlementNo;
                model.Settlement.Requester = settlement.Requester;
                decimal kickBackExcRate = currentUser.KbExchangeRate ?? 20000;
                HandleState hs = new HandleState();
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        // Tính Balance trong settle
                        decimal? advanceAmount = GetAdvanceAmountSettle(model.ShipmentCharge, settlement.SettlementCurrency);
                        if (advanceAmount != null)
                        {
                            settlement.AdvanceAmount = advanceAmount;
                            settlement.BalanceAmount = settlement.AdvanceAmount - settlement.Amount;

                            if (settlement.BalanceAmount == 0)
                            {
                                settlement.PaymentMethod = AccountingConstants.PAYMENT_METHOD_OTHER;
                            }
                        }
                        settlement.BankAccountName = model.Settlement.BankAccountName;
                        settlement.BankName = model.Settlement.BankName;
                        settlement.Note = model.Settlement.Note;

                        hs = DataContext.Update(settlement, x => x.Id == settlement.Id);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        new LogHelper("eFMS_LOG_AcctSettlePayment", ex.ToString());
                        hs = new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_LOG_AcctSettlePayment", ex.ToString());
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

        private decimal? CalculateBalanceSettle(List<ShipmentChargeSettlement> charges, string settlementNo, string currency)
        {
            decimal? _advanceAmount = null;

            IEnumerable<ShipmentSettlement> dataGroups = charges.GroupBy(x => new { x.JobId, x.HBL, x.MBL, x.Hblid, x.AdvanceNo, ClearanceNo = (string.IsNullOrEmpty(x.ClearanceNo) ? null : x.ClearanceNo) })
                                    .Select(x => new ShipmentSettlement
                                    {
                                        JobId = x.Key.JobId,
                                        HBL = x.Key.HBL,
                                        MBL = x.Key.MBL,
                                        HblId = x.Key.Hblid,
                                        AdvanceNo = x.Key.AdvanceNo,
                                        CustomNo = x.Key.ClearanceNo
                                    });

            if (dataGroups != null && dataGroups.Count() > 0)
            {
                decimal? _totalAdvanceAmount = 0;

                foreach (ShipmentSettlement item in dataGroups)
                {
                    if (!string.IsNullOrEmpty(item.AdvanceNo))
                    {
                        AdvanceInfo advInfo = GetAdvanceBalanceInfo(settlementNo, item.HblId.ToString(), currency, item.AdvanceNo, item.CustomNo);
                        _totalAdvanceAmount += advInfo.AdvanceAmount ?? 0;
                    }
                }
                if (_totalAdvanceAmount == 0)
                {
                    _advanceAmount = null;
                }
                else
                {
                    _advanceAmount = _totalAdvanceAmount;
                }
            }

            return _advanceAmount;

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
                settlement.SettlementType = settlementCurrent.SettlementType;
                settlement.BankAccountNo = StringHelper.RemoveSpecialChars(settlement.BankAccountNo, Constants.spaceCharacter);

                decimal kickBackExcRate = currentUser.KbExchangeRate ?? 20000;

                //Cập nhật lại Status Approval là NEW nếu Status Approval hiện tại là DENIED
                if (model.Settlement.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED) && settlementCurrent.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED))
                {
                    settlement.StatusApproval = AccountingConstants.STATUS_APPROVAL_NEW;
                }
                HandleState hs = new HandleState();
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        // Tính Balance trong settle
                        decimal? advanceAmount = GetAdvanceAmountSettle(model.ShipmentCharge, settlement.SettlementCurrency);
                        if (advanceAmount != null)
                        {
                            settlement.AdvanceAmount = advanceAmount;
                            settlement.BalanceAmount = settlement.AdvanceAmount - settlement.Amount;

                            if (settlement.BalanceAmount == 0)
                            {
                                settlement.PaymentMethod = AccountingConstants.PAYMENT_METHOD_OTHER;
                            }
                        }

                        hs = DataContext.Update(settlement, x => x.Id == settlement.Id);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        new LogHelper("eFMS_LOG_AcctSettlePayment", ex.ToString());
                        hs = new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("eFMS_LOG_AcctSettlePayment", ex.ToString());
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
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
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
            //string _paymentMethod = settlement.PaymentMethod;
            //string _beneficiary = settlement.BankAccountName;
            //string _accNo = settlement.BankAccountNo;
            //string _bank = settlement.BankName;
            //string _bankCode = settlement.BankCode;
            DateTime? _serviceDate = null;
            //DateTime? _dueDate = settlement.DueDate;

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
                _serviceDate = opsTran?.ServiceDate;
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
                    _serviceDate = csTran?.ServiceDate;
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
            result.CustomsId = firstCharge.ClearanceNo;
            result.HBL = _hbl;
            result.MBL = _mbl;
            result.StlCSName = string.Empty;
            result.PaymentMethod = settlement.PaymentMethod;
            result.Beneficiary = settlement.BankAccountName;
            result.AccNo = settlement.BankAccountNo;
            result.Bank = settlement.BankName;
            result.BankCode = settlement.BankCode;
            result.ServiceDate = _serviceDate;
            result.DueDate = settlement.DueDate;

            //CR: Sum _gw, _nw, _psc, _cbm theo Masterbill [28/12/2020 - Alex]
            //Settlement có nhiều Job thì sum all các job đó
            //Groupby HBLID
            var hblIds = surcharges.GroupBy(g => g.Hblid).Select(s => s.Key).ToList();
            foreach (var hblId in hblIds)
            {
                var _opsTrans = opsTransactionRepo.Where(x => x.Hblid == hblId).FirstOrDefault();
                if (_opsTrans != null)
                {
                    _gw += _opsTrans.SumGrossWeight;
                    _nw += _opsTrans.SumNetWeight;
                    _psc += _opsTrans.SumPackages;
                    _cbm += _opsTrans.SumCbm;
                }
                else
                {
                    var csTranDetail = csTransactionDetailRepo.Get(x => x.Id == hblId).FirstOrDefault();
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
            // Check valid office
            var isCommonOffice = sysOfficeRepo.Any(x => x.Id == currentUser.OfficeID && DataTypeEx.IsCommonOffice(x.Code));
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
                fe.DueDate = firstShipment.DueDate;
                fe.ServiceDate = firstShipment.ServiceDate;
                fe.PaymentMethod = firstShipment.PaymentMethod;
                fe.Bank = firstShipment.Bank;
                fe.BankCode = firstShipment.BankCode;
                fe.AccNo = firstShipment.AccNo;
                fe.Beneficiary = firstShipment.Beneficiary;


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
                fe.IsDisplayLogo = isCommonOffice;
            });
            return listSettlementPayment;
        }

        public IQueryable<AscSettlementPaymentRequestReport> GetChargeOfSettlement(string settlementNo, string currency)
        {
            var settle = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            //Quy đổi tỉ giá theo ngày Request Date, nếu không có thì lấy exchange rate mới nhất
            //var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settle.RequestDate.Value.Date).ToList();
            //if (currencyExchange.Count == 0)
            //{
            //    DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
            //    currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            //}

            var surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo);
            //.GroupBy(x => new { x.SettlementCode, x.JobNo, x.Hblno, x.Mblno, x.CurrencyId, x.Hblid, x.Type, x.AdvanceNo });
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
                // item.Amount = surcharge.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, surcharge.CurrencyId, currency) + _decimalNumber;
                if (currency == AccountingConstants.CURRENCY_LOCAL)
                {
                    item.Amount = (surcharge.AmountVnd ?? 0) + (surcharge.VatAmountVnd ?? 0);
                }
                else
                {
                    item.Amount = (surcharge.AmountUsd ?? 0) + (surcharge.VatAmountUsd ?? 0);
                }
                item.Debt = surcharge.Type == AccountingConstants.TYPE_CHARGE_OBH ? true : false;
                item.Currency = string.Empty;
                item.Note = surcharge.Notes;
                // Get Office Info
                var officeInfo = sysOfficeRepo.Get(x => x.Id == currentUser.OfficeID).FirstOrDefault();
                item.CompanyName = officeInfo?.BranchNameEn?.ToUpper();
                item.CompanyAddress = officeInfo?.AddressEn;
                item.Website = string.IsNullOrEmpty(officeInfo?.Website) ? " www.itlvn.com" : officeInfo.Website;
                item.Tel = "Tel‎: " + officeInfo?.Tel + "  Fax‎: " + officeInfo?.Fax;
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
                settlementApprove.RequesterAprDate = DateTime.Now;
                var settlementPayment = DataContext.Get(x => x.SettlementNo == approve.SettlementNo).FirstOrDefault();
                approve.Id = settlementPayment.Id;

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
                var settingFlow = CheckExistSettingFlow(typeApproval, settlementPayment.OfficeId);
                if (!string.IsNullOrEmpty(settingFlow.Message?.ToString()))
                {
                    return settingFlow;
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

                        var hs = acctApproveSettlementRepo.SubmitChanges();
                        var surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementPayment.SettlementNo);
                        if (settlementPayment.SettlementCurrency == AccountingConstants.CURRENCY_LOCAL)
                        {
                            settlementPayment.Amount = surcharges.Sum(surcharge => (surcharge.AmountVnd ?? 0) + (surcharge.VatAmountVnd ?? 0));
                        }
                        else if (settlementPayment.SettlementCurrency == AccountingConstants.CURRENCY_USD)
                        {
                            settlementPayment.Amount = surcharges.Sum(surcharge => (surcharge.AmountUsd ?? 0) + (surcharge.VatAmountUsd ?? 0));
                        }
                        else
                        {
                            settlementPayment.Amount = surcharges.Sum(surcharge => surcharge.Total);
                        }
                        var hsUpdateAdvancePayment = DataContext.Update(settlementPayment, x => x.Id == settlementPayment.Id);
                        trans.Commit();

                        var sendMailApproved = true;
                        var sendMailSuggest = true;
                        if (hs.Success) // Send mail when success update
                        {
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

                            if (!sendMailSuggest)
                            {
                                return new HandleState("Send mail suggest approval failed");
                            }
                            if (!sendMailApproved)
                            {
                                return new HandleState("Send mail approved approval failed");
                            }
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
                                approve.LeaderApr = approve.Leader = userCurrent;
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
                                approve.ManagerApr = approve.Manager = userCurrent;
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
                                approve.AccountantApr = approve.Accountant = userCurrent;
                                approve.AccountantAprDate = DateTime.Now;
                                approve.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
                                userApproveNext = buHeadLevel.UserId;
                                mailUserApproveNext = buHeadLevel.EmailUser;
                                mailUsersDeputy = buHeadLevel.EmailDeputies;
                                //Nếu Role BUHead là Auto or Special thì chuyển trạng thái Done
                                if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO || buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL)
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
                                approve.LeaderApr = approve.Leader = userCurrent;
                                approve.LeaderAprDate = DateTime.Now;
                            }
                            if (!string.IsNullOrEmpty(approve.Manager) && string.IsNullOrEmpty(approve.ManagerApr))
                            {
                                approve.ManagerApr = approve.Manager = userCurrent;
                                approve.ManagerAprDate = DateTime.Now;
                            }
                            if (!string.IsNullOrEmpty(approve.Accountant) && string.IsNullOrEmpty(approve.AccountantApr))
                            {
                                approve.AccountantApr = approve.Accountant = userCurrent;
                                approve.AccountantAprDate = DateTime.Now;
                            }
                        }
                        if (string.IsNullOrEmpty(approve.BuheadApr))
                        {
                            if ((!string.IsNullOrEmpty(approve.Accountant) && !string.IsNullOrEmpty(approve.AccountantApr)) || string.IsNullOrEmpty(approve.Accountant) || accountantLevel.Role == AccountingConstants.ROLE_NONE || accountantLevel.Role == AccountingConstants.ROLE_AUTO || buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL)
                            {
                                settlementPayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                approve.BuheadApr = approve.Buhead = userCurrent;
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



                    settlementPayment.UserModified = approve.UserModified = userCurrent;
                    settlementPayment.DatetimeModified = approve.DateModified = DateTime.Now;

                    var hsUpdateSettlementPayment = DataContext.Update(settlementPayment, x => x.Id == settlementPayment.Id, false);
                    var hsUpdateApprove = acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id, false);

                    var hs = acctApproveSettlementRepo.SubmitChanges();
                    DataContext.SubmitChanges();
                    trans.Commit();

                    var sendMailApproved = true;
                    var sendMailSuggest = true;
                    if (hs.Success) // Send mail when success update
                    {
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
                        // Send mail là Option nên send mail có thất bại vẫn cập nhật data Approve Settlement [23/12/2020]
                        if (!sendMailSuggest)
                        {
                            return new HandleState("Send mail suggest approval failed");
                        }
                        if (!sendMailApproved)
                        {
                            return new HandleState("Send mail approved approval failed");
                        }
                    }
                    return new HandleState();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    new LogHelper("eFMS_LOG_AcctSettlePayment", ex.ToString());
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

                    var hs = acctApproveSettlementRepo.SubmitChanges();
                    DataContext.SubmitChanges();
                    trans.Commit();

                    if (hs.Success) // Send mail when success update
                    {
                        var sendMailDeny = SendMailDeniedApproval(settlementPayment.SettlementNo, comment, DateTime.Now);
                        if (!sendMailDeny)
                        {
                            return new HandleState("Send mail denied failed");
                        }
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
            //var emailDeputies = userBaseService.GetEmailUsersDeputyByCondition(type, userAccountant, null, null, officeId, companyId);

            result.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
            result.Role = roleAccountant;
            result.UserId = userAccountant;
            result.UserDeputies = userDeputies;

            // Get email setting accountant
            var deptAccountants = userBaseService.GetDepartmentUser(companyId, officeId, userAccountant).FirstOrDefault();
            var emailSetting = deptAccountants == null ? null : sysEmailSettingRepository.Get(x => x.EmailType == "Approve Settlement" && deptAccountants == x.DeptId).FirstOrDefault()?.EmailInfo;
            var emailDeputies = new List<string>();
            if (emailSetting != null)
            {
                emailDeputies = emailSetting.Split(";").ToList();
            }
            else
            {
                emailDeputies = catDepartmentRepo.Get(x => x.Id == deptAccountants).FirstOrDefault()?.Email?.Split(";").ToList();
            }
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
                //User vừa là Requester và vừa là User Deputy 
                if (approve.RequesterAprDate == null
                    || (leaderLevel.UserDeputies.Contains(userCurrent.UserID) && string.IsNullOrEmpty(approve.LeaderApr) && leaderLevel.Role != AccountingConstants.ROLE_NONE)
                    || (managerLevel.UserDeputies.Contains(currentUser.UserID) && string.IsNullOrEmpty(approve.ManagerApr) && managerLevel.Role != AccountingConstants.ROLE_NONE)
                    || (accountantLevel.UserDeputies.Contains(currentUser.UserID) && string.IsNullOrEmpty(approve.AccountantApr) && accountantLevel.Role != AccountingConstants.ROLE_NONE)
                    || (buHeadLevel.UserDeputies.Contains(userCurrent.UserID) && (string.IsNullOrEmpty(approve.BuheadApr) && buHeadLevel.Role != AccountingConstants.ROLE_NONE) || (buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL && !string.IsNullOrEmpty(approve.Requester)))
                    )
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
                var advance = GetListAdvanceNoForShipment(shipment.HBLID, null, currentUser.UserID, null, true)?.FirstOrDefault();
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
                    #region comment: copy charge without copy cdnote, soa
                    //chargeCopy.Soaclosed = charge.Soaclosed;
                    //chargeCopy.Cdclosed = charge.Cdclosed;
                    //chargeCopy.CreditNo = charge.CreditNo;
                    //chargeCopy.DebitNo = charge.DebitNo;
                    //chargeCopy.Soano = charge.Soano;
                    //chargeCopy.PaySoano = charge.PaySoano;
                    #endregion
                    chargeCopy.IsFromShipment = charge.IsFromShipment;
                    chargeCopy.TypeOfFee = charge.TypeOfFee;
                    chargeCopy.AdvanceNo = advance;
                    chargeCopy.TypeService = charge.TypeService;
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
            //Lấy ra SettlementPayment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            if (settlement == null) return false;

            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(settlement.Requester);
            var _requester = userBaseService.GetEmployeeByEmployeeId(requesterId);
            var requesterName = _requester?.EmployeeNameEn;
            var emailRequester = _requester?.Email;

            //Lấy ra thông tin JobId dựa vào SettlementNo
            var listJobId = GetJobIdBySettlementNo(settlementNo);
            string jobIds = string.Empty;
            jobIds = String.Join("; ", listJobId.ToList());

            decimal totalAmount = settlement.Amount ?? 0; //19-04-2021 - Andy

            //Lấy ra list AdvanceNo dựa vào Shipment(JobId,MBL,HBL)
            string advanceNos = string.Empty;
            var listAdvanceNo = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo).Select(s => s.AdvanceNo).Distinct();
            advanceNos = string.Join("; ", listAdvanceNo);

            var userReciverId = userBaseService.GetEmployeeIdOfUser(userReciver);
            var userReciverName = userBaseService.GetEmployeeByEmployeeId(userReciverId)?.EmployeeNameEn;

            //Mail Info
            var numberOfRequest = acctApproveSettlementRepo.Get(x => x.SettlementNo == settlement.SettlementNo).Select(s => s.Id).Count();
            numberOfRequest = numberOfRequest == 0 ? 1 : (numberOfRequest + 1);
            #region Old Template
            //string subject = "eFMS - Settlement Payment Approval Request from [RequesterName] - [NumberOfRequest] " + (numberOfRequest > 1 ? "times" : "time");
            //subject = subject.Replace("[RequesterName]", requesterName);
            //subject = subject.Replace("[NumberOfRequest]", numberOfRequest.ToString());
            //string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt; color: #004080'>" +
            //                                "<p><i><b>Dear Mr/Mrs [UserName],</b> </i></p>" +
            //                                "<p>" +
            //                                    "<div>You have new Settlement Payment Approval Request from <b>[RequesterName]</b> as below info:</div>" +
            //                                    "<div><i>Anh/ Chị có một yêu cầu duyệt thanh toán từ <b>[RequesterName]</b> với thông tin như sau: </i></div>" +
            //                                "</p>" +
            //                                "<ul>" +
            //                                    "<li>Settlement No / <i>Mã đề nghị thanh toán</i> : <b>[SettlementNo]</b></li>" +
            //                                    "<li>Settlement Amount/ <i>Số tiền thanh toán</i> : <b>[TotalAmount] [CurrencySettlement]</b></li>" +
            //                                    "<li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNos]</b></li>" +
            //                                    "<li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li>" +
            //                                    "<li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li>" +
            //                                    "<li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li>" +
            //                                "</ul>" +
            //                                "<p>" +
            //                                    "<div>You click here to check more detail and approve: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]/approve' target='_blank'>Detail Payment Request</a> </span></div>" +
            //                                    "<div><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span> <a href='[Url]/[lang]/[UrlFunc]/[SettlementId]/approve' target='_blank'>Chi tiết phiếu đề nghị thanh toán</a> </span> </i></div>" +
            //                                "</p>" +
            //                                "<p>Thanks and Regards,<p><p> <b>eFMS System,</b></p><p> <img src='[logoEFMS]'/></p>" +
            //                             "</div>");
            //body = body.Replace("[UserName]", userReciverName);
            //body = body.Replace("[RequesterName]", requesterName);
            //body = body.Replace("[SettlementNo]", settlementNo);
            //body = body.Replace("[TotalAmount]", string.Format("{0:n}", totalAmount));
            //body = body.Replace("[CurrencySettlement]", settlement.SettlementCurrency);
            //body = body.Replace("[AdvanceNos]", advanceNos);
            //body = body.Replace("[JobIds]", jobIds);
            //body = body.Replace("[RequestDate]", settlement.RequestDate.Value.ToString("dd/MM/yyyy"));
            //body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            //body = body.Replace("[lang]", "en");
            //body = body.Replace("[UrlFunc]", "#/home/accounting/settlement-payment");
            //body = body.Replace("[SettlementId]", settlement.Id.ToString());
            //body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");
            #endregion

            // Filling email with template
            var emailTemplate = sysEmailTemplateRepository.Get(x => x.Code == "SETTLE-SUGGEST-APPROVE")?.FirstOrDefault();
            // Subject
            var subject = new StringBuilder(emailTemplate.Subject);
            subject.Replace("{{RequesterName}}", requesterName);
            subject.Replace("{{NumberOfRequest}}", numberOfRequest.ToString() + (numberOfRequest > 1 ? " times" : " time"));

            // Body
            var body = new StringBuilder(emailTemplate.Body);
            body = body.Replace("{{UserName}}", userReciverName);
            body = body.Replace("{{RequesterName}}", requesterName);
            body = body.Replace("{{SettlementNo}}", settlementNo);
            body = body.Replace("{{TotalAmount}}", string.Format("{0:n}", totalAmount));
            body = body.Replace("{{CurrencySettlement}}", settlement.SettlementCurrency);
            body = body.Replace("{{AdvanceNos}}", advanceNos);
            body = body.Replace("{{JobIds}}", jobIds);
            body = body.Replace("{{RequestDate}}", settlement.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("{{Address}}", webUrl.Value.Url.ToString() + "/en/#/home/accounting/settlement-payment/" + settlement.Id.ToString() + "/approve");
            body = body.Replace("{{LogoEFMS}}", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");


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

            var sendMailResult = SendMail.Send(subject.ToString(), body.ToString(), toEmails, attachments, emailCCs);

            #region --- Ghi Log Send Mail ---
            var logSendMail = new SysSentEmailHistory
            {
                SentUser = SendMail._emailFrom,
                Receivers = string.Join("; ", toEmails),
                Ccs = string.Join("; ", emailCCs),
                Subject = subject.ToString(),
                Sent = sendMailResult,
                SentDateTime = DateTime.Now,
                Body = body.ToString()
            };
            var hsLogSendMail = sentEmailHistoryRepo.Add(logSendMail);
            var hsSm = sentEmailHistoryRepo.SubmitChanges();
            #endregion --- Ghi Log Send Mail ---

            return sendMailResult;
        }

        //Send Mail Approved
        private bool SendMailApproved(string settlementNo, DateTime approvedDate)
        {
            //var surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo);

            //Lấy ra SettlementPayment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            if (settlement == null) return false;

            //Quy đổi tỉ giá theo ngày Request Date
            /*var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settlement.RequestDate.Value.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }*/

            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(settlement.Requester);
            var _requester = userBaseService.GetEmployeeByEmployeeId(requesterId);
            var requesterName = _requester?.EmployeeNameEn;
            var emailRequester = _requester?.Email;
            var partnerName = catPartnerRepo.Get(x => x.Id == settlement.Payee).FirstOrDefault()?.PartnerNameVn;

            //Lấy ra thông tin JobId dựa vào SettlementNo
            var listJobId = GetJobIdBySettlementNo(settlementNo);
            string jobIds = string.Empty;
            jobIds = String.Join("; ", listJobId.ToList());

            /*var totalAmount = surcharges
                .Where(x => x.SettlementCode == settlementNo)
                .Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, settlement.SettlementCurrency));
            totalAmount = NumberHelper.RoundNumber(totalAmount, 2);*/

            decimal totalAmount = settlement.Amount ?? 0; //19-04-2021 - Andy

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
                                                "<li>Payee/ <i>Đối tượng thanh toán</i> : <b>[Payee]</b></li>" +
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
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[Payee]", partnerName);
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
            //var surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementNo);

            //Lấy ra SettlementPayment dựa vào SettlementNo
            var settlement = DataContext.Get(x => x.SettlementNo == settlementNo).FirstOrDefault();
            if (settlement == null) return false;

            //Quy đổi tỉ giá theo ngày Request Date
            /*var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == settlement.RequestDate.Value.Date).ToList();
            if (currencyExchange.Count == 0)
            {
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }*/

            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(settlement.Requester);
            var _requester = userBaseService.GetEmployeeByEmployeeId(requesterId);
            var requesterName = _requester?.EmployeeNameEn;
            var emailRequester = _requester?.Email;

            //Lấy ra thông tin JobId dựa vào SettlementNo
            var listJobId = GetJobIdBySettlementNo(settlementNo);
            string jobIds = string.Empty;
            jobIds = string.Join("; ", listJobId.ToList());

            /*var totalAmount = surcharges
                .Where(x => x.SettlementCode == settlementNo)
                .Sum(x => x.Total * currencyExchangeService.GetRateCurrencyExchange(currencyExchange, x.CurrencyId, settlement.SettlementCurrency));
            totalAmount = NumberHelper.RoundNumber(totalAmount, 2);*/

            decimal totalAmount = settlement.Amount ?? 0; //19-04-2021 - Andy

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

        /// <summary>
        /// Check before delete settlement
        /// </summary>
        /// <param name="settlementId"></param>
        /// <returns>0 : delete</returns>
        /// <returns>403 : dont have permission</returns>
        /// <returns>-1 : add charge have synced charges</returns>
        public int CheckDeletePermissionBySettlementId(Guid settlementId)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None)
                return 403;

            var detail = DataContext.Get(x => x.Id == settlementId)?.FirstOrDefault();
            if (detail == null) return 403;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, permissionRange, _user);

            if (code == 403) return code;

            if (detail.SettlementType == "DIRECT")
            {
                var isExistedChargeSynced = csShipmentSurchargeRepo.Any(x => x.SettlementCode == detail.SettlementNo && (!string.IsNullOrEmpty(x.SyncedFrom) || !string.IsNullOrEmpty(x.PaySyncedFrom)));
                return isExistedChargeSynced ? -1 : 0;
            }
            return 0;
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
            string _bod = string.Empty;
            if (_settlementApprove != null)
            {
                _manager = string.IsNullOrEmpty(_settlementApprove.Manager) ? string.Empty : userBaseService.GetEmployeeByUserId(_settlementApprove.Manager)?.EmployeeNameVn;
                _accountant = string.IsNullOrEmpty(_settlementApprove.Accountant) ? string.Empty : userBaseService.GetEmployeeByUserId(_settlementApprove.Accountant)?.EmployeeNameVn;
                _bod = string.IsNullOrEmpty(_settlementApprove.Buhead) ? string.Empty : userBaseService.GetEmployeeByUserId(_settlementApprove.Buhead)?.EmployeeNameVn;
            }

            var _department = catDepartmentRepo.Get(x => x.Id == settlementPayment.DepartmentId).FirstOrDefault()?.DeptNameAbbr;
            #endregion -- Info Manager, Accoutant & Department --

            var office = sysOfficeRepo.Get(x => x.Id == currentUser.OfficeID).FirstOrDefault();
            var officeName = office?.BranchNameEn?.ToUpper();
            var _contactOffice = string.Format("{0}\nTel: {1}  Fax: {2}\nE-mail: {3}", office?.AddressEn, office?.Tel, office?.Fax, office?.Email);
            var isCommonOffice = DataTypeEx.IsCommonOffice(office?.Code);

            var surcharge = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementPayment.SettlementNo).ToList();
            var soapayNo = surcharge.Select(x => x.PaySoano).ToList();
            var soa = acctSoaRepo.Get(x => soapayNo.Contains(x.Soano)).ToList();

            var ops = opsTransactionRepo.Get(x => x.JobNo == surcharge.FirstOrDefault().JobNo).FirstOrDefault();
            var partner = new CatPartner();
            if (ops != null)
            {
                partner = catPartnerRepo.Get(x => x.Id == ops.SupplierId).FirstOrDefault();
            }

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
                OfficeName = officeName,
                ContactOffice = _contactOffice,
                PaymentMethod = settlementPayment.PaymentMethod,
                BankAccountName = settlementPayment.BankAccountName,
                BankAccountNo = settlementPayment.BankAccountNo,
                BankName = settlementPayment.BankName,
                BankCode = settlementPayment.BankCode,
                DueDate = settlementPayment.DueDate,
                SOADate = soa.FirstOrDefault()?.SoaformDate,
                SOANo = string.Join(";", soa?.Select(x => x.Soano)),
                Supplier = partner?.ShortName,
                Note = settlementPayment.Note,
                SettlementCurrency = settlementPayment.SettlementCurrency,
                BOD = _bod,
                IsDisplayLogo = isCommonOffice
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

            var houseBillIds = surChargeBySettleCode.GroupBy(s => new { s.Hblid, s.AdvanceNo, ClearanceNo = (string.IsNullOrEmpty(s.ClearanceNo) ? null : s.ClearanceNo) }).Select(s => new { hblId = s.Key.Hblid, customNo = s.Key.ClearanceNo, s.Key.AdvanceNo });
            foreach (var houseBillId in houseBillIds)
            {
                var shipmentSettlement = new InfoShipmentSettlementExport();

                #region -- CHANRGE AND ADVANCE OF SETTELEMENT --
                var _shipmentCharges = GetChargeOfShipmentSettlementExport(houseBillId.hblId, houseBillId.customNo, settlementPayment.SettlementCurrency, surChargeBySettleCode, currencyExchange, houseBillId.AdvanceNo);
                var _infoAdvanceExports = GetAdvanceOfShipmentSettlementExport(houseBillId.hblId, settlementPayment.SettlementCurrency, surChargeBySettleCode, currencyExchange, houseBillId.AdvanceNo);
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
                    shipmentSettlement.ServiceDate = ops.ServiceDate;
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
                            shipmentSettlement.ServiceDate = trans.ServiceDate;
                        }
                        listData.Add(shipmentSettlement);
                    }
                }
            }
            var result = listData.ToArray().OrderBy(x => x.JobNo); //Sắp xếp tăng dần theo JobNo [05-01-2021]
            return result.ToList();
        }

        private List<InfoShipmentChargeSettlementExport> GetChargeOfShipmentSettlementExport(Guid hblId, string customNo, string settlementCurrency, IQueryable<CsShipmentSurcharge> surChargeBySettleCode, List<CatCurrencyExchange> currencyExchange, string advanceNo)
        {
            var shipmentSettlement = new InfoShipmentSettlementExport();
            var listCharge = new List<InfoShipmentChargeSettlementExport>();
            var surChargeByHblId = surChargeBySettleCode.Where(x => x.Hblid == hblId && x.AdvanceNo == advanceNo && x.ClearanceNo == customNo); // Trường hợp cùng 1 lô nhưng tạm ứng nhiều lần
            foreach (var sur in surChargeByHblId)
            {
                var infoShipmentCharge = new InfoShipmentChargeSettlementExport();
                infoShipmentCharge.ChargeName = catChargeRepo.Get(x => x.Id == sur.ChargeId).FirstOrDefault()?.ChargeNameEn;
                //Quy đổi theo currency của Settlement
                if (settlementCurrency == AccountingConstants.CURRENCY_LOCAL)
                {
                    infoShipmentCharge.ChargeNetAmount = sur.AmountVnd;
                    infoShipmentCharge.ChargeVatAmount = (sur.VatAmountVnd ?? 0);
                    infoShipmentCharge.ChargeAmount = (sur.AmountVnd ?? 0) + (sur.VatAmountVnd ?? 0);
                }
                else
                {
                    infoShipmentCharge.ChargeNetAmount = sur.AmountUsd;
                    infoShipmentCharge.ChargeVatAmount = (sur.VatAmountUsd ?? 0);
                    infoShipmentCharge.ChargeAmount = (sur.AmountUsd ?? 0) + (sur.VatAmountUsd ?? 0);
                }

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
                infoShipmentCharge.SurType = sur.Type;
                listCharge.Add(infoShipmentCharge);
            }
            return listCharge;
        }

        private List<InfoAdvanceExport> GetAdvanceOfShipmentSettlementExport(Guid hblId, string settlementCurrency, IQueryable<CsShipmentSurcharge> surChargeBySettleCode, List<CatCurrencyExchange> currencyExchange, string advanceNo)
        {
            var listAdvance = new List<InfoAdvanceExport>();
            // Gom surcharge theo AdvanceNo & HBLID
            var groupAdvanceNoAndHblID = surChargeBySettleCode.GroupBy(g => new { g.AdvanceNo, g.Hblid }).ToList().Where(x => x.Key.Hblid == hblId && x.Key.AdvanceNo == advanceNo);  // Trường hợp cùng 1 lô nhưng tạm ứng nhiều lần
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
                    advance.AdvanceNo = advanceIsDone.AdvanceNo;
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
                                            //join cus in custom on new { JobNo = (ops.JobNo != null ? ops.JobNo : ops.JobNo), HBL = (ops.Hwbno != null ? ops.Hwbno : ops.Hwbno), MBL = (ops.Mblno != null ? ops.Mblno : ops.Mblno) } equals new { JobNo = cus.JobNo, HBL = cus.Hblid, MBL = cus.Mblid } into cus1
                                            join cus in custom on ops.JobNo equals cus.JobNo into cus1
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
                                              //join cus in custom on new { JobNo = (cst.JobNo != null ? cst.JobNo : cst.JobNo), HBL = (cstd.Hwbno != null ? cstd.Hwbno : cstd.Hwbno), MBL = (cstd.Mawb != null ? cstd.Mawb : cstd.Mawb) } equals new { JobNo = cus.JobNo, HBL = cus.Hblid, MBL = cus.Mblid } into cus1
                                              //from cus in cus1.DefaultIfEmpty()

                                          where sur.SettlementCode == settleCode
                                          select new SettlementExportDefault
                                          {
                                              JobID = cst.JobNo,
                                              HBL = cstd.Hwbno,
                                              MBL = cst.Mawb,

                                              CustomNo = string.Empty,
                                              SettleNo = currentSettlement.SettlementNo,
                                              Currency = currentSettlement.SettlementCurrency,
                                              AdvanceNo = sur.AdvanceNo,
                                              Requester = requesterName,
                                              RequestDate = currentSettlement.RequestDate,
                                              ApproveDate = approveDate,
                                              Description = sur.Notes,
                                              SettlementAmount = sur.Total,
                                          };

                        var data = dataOperation.Union(dataService).ToList();

                        decimal? advTotalAmount;
                        var group = data.GroupBy(d => new { d.SettleNo, d.JobID, d.HBL, d.MBL, CustomNo = (string.IsNullOrEmpty(d.CustomNo) ? null : d.CustomNo) })
                            .Select(s => new SettlementExportGroupDefault
                            {
                                JobID = s.Key.JobID,
                                MBL = s.Key.MBL,
                                HBL = s.Key.HBL,
                                CustomNo = s.Key.CustomNo,
                                SettlementTotalAmount = s.Sum(d => d.SettlementAmount),
                                requestList = getRequestList(data, s.Key.JobID, s.Key.HBL, s.Key.MBL, s.Key.SettleNo, s.FirstOrDefault().Currency, out advTotalAmount),
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
            string HBL, string MBL, string SettleNo, string Currency, out decimal? advTotalAmount)
        {
            var advRequest = acctAdvanceRequestRepo.Get();
            var advPayment = acctAdvancePaymentRepo.Get();
            var settleDesc = DataContext.Get().Where(x => x.SettlementNo == SettleNo).FirstOrDefault().Note;
            //
            var groupAdvReq = advRequest.GroupBy(x => new { x.JobId, x.AdvanceNo, x.Hbl, x.Mbl, x.RequestCurrency })
                            .Select(y => new { y.Key.JobId, y.Key.Hbl, y.Key.Mbl, y.Key.AdvanceNo, AdvanceAmount = y.Sum(z => z.Amount * currencyExchangeService.CurrencyExchangeRateConvert(null, null, z.RequestCurrency, Currency)) });
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
                    CustomNo = (string.IsNullOrEmpty(d.CustomNo) ? null : d.CustomNo),
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
                    SettlementAmount = y.Key.Currency == "VND" ? Math.Round(y.Sum(z => (decimal)z.SettlementAmount), 0) : Math.Round(y.Sum(z => (decimal)z.SettlementAmount), 2),
                    ApproveDate = y.Key.ApproveDate,
                    Currency = y.Key.Currency,
                    CustomNo = y.Key.CustomNo,
                    Description = settleDesc,
                    RequestDate = y.Key.RequestDate,
                    Requester = y.Key.Requester
                }); ;
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


        /// <summary>
        /// Get data for General Preview
        /// </summary>
        /// <param name="settlementId"></param>
        /// <returns></returns>
        public InfoSettlementExport GetGeneralSettlementExport(Guid settlementId)
        {
            var settlementPayment = GetSettlementPaymentById(settlementId);
            if (settlementPayment == null) return null;

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

            string _payeeName = string.Empty;

            if (!string.IsNullOrEmpty(settlementPayment.Payee))
            {
                var payeeInfo = catPartnerRepo.Get(x => x.Id == settlementPayment.Payee).FirstOrDefault();
                _payeeName = payeeInfo?.PartnerNameVn;
            }
            string _inWords = settlementPayment.SettlementCurrency == AccountingConstants.CURRENCY_LOCAL ? InWordCurrency.ConvertNumberCurrencyToString(settlementPayment.Amount ?? 0, settlementPayment.SettlementCurrency)
                    :
                        InWordCurrency.ConvertNumberCurrencyToStringUSD(settlementPayment.Amount ?? 0, "") + " " + settlementPayment.SettlementCurrency;
            var office = sysOfficeRepo.Get(x => x.Id == currentUser.OfficeID).FirstOrDefault();
            var officeName = office?.BranchNameEn?.ToUpper();
            var _contactOffice = string.Format("{0}\nTel: {1}  Fax: {2}\nE-mail: {3}", office?.AddressEn, office?.Tel, office?.Fax, office?.Email);
            var isCommonOffice = DataTypeEx.IsCommonOffice(office.Code);

            var infoSettlement = new InfoSettlementExport
            {
                Requester = _requester,
                RequestDate = settlementPayment.RequestDate,
                Department = _department,
                SettlementNo = settlementPayment.SettlementNo,
                SettlementAmount = settlementPayment.Amount,
                SettlementCurrency = settlementPayment.SettlementCurrency,
                PaymentMethod = Common.CustomData.PaymentMethod.Where(x => x.Value == settlementPayment.PaymentMethod).Select(x => x.DisplayName).FirstOrDefault(),
                AmountInWords = _inWords,
                Manager = _manager,
                Accountant = _accountant,
                IsRequesterApproved = _settlementApprove?.RequesterAprDate != null,
                IsManagerApproved = _settlementApprove?.ManagerAprDate != null,
                IsAccountantApproved = _settlementApprove?.AccountantAprDate != null,
                IsBODApproved = _settlementApprove?.BuheadAprDate != null,
                BankAccountNo = settlementPayment.BankAccountNo,
                BankName = settlementPayment.BankName,
                BankAccountName = settlementPayment.BankAccountName,
                PayeeName = _payeeName,
                Note = settlementPayment.Note,
                IsDisplayLogo = isCommonOffice,
                OfficeName = officeName,
                ContactOffice = _contactOffice
            };
            return infoSettlement;
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

        public HandleState DenySettlePayments(List<Guid> Ids, string comment)
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
                            if (settle != null && settle.SyncStatus != AccountingConstants.STATUS_SYNCED && settle.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW)
                            {
                                settle.StatusApproval = AccountingConstants.STATUS_APPROVAL_DENIED;
                                settle.UserModified = currentUser.UserID;
                                settle.DatetimeModified = DateTime.Now;

                                // write log unlock
                                string log = String.Format("{0} has been opened at {1} on {2} by {3} from denied", settle.SettlementNo, string.Format("{0:HH:mm:ss tt}", DateTime.Now), DateTime.Now.ToString("dd/MM/yyyy"), currentUser.UserName);

                                settle.LockedLog = settle.LockedLog + log + ";";

                                result = DataContext.Update(settle, x => x.Id == Id);

                                if (result.Success)
                                {
                                    IQueryable<AcctApproveSettlement> approveSettles = acctApproveSettlementRepo.Get(x => x.SettlementNo == settle.SettlementNo);
                                    foreach (var approve in approveSettles)
                                    {
                                        approve.IsDeny = true;
                                        approve.UserModified = currentUser.UserID;
                                        approve.DateModified = DateTime.Now;
                                        approve.Comment = comment;
                                        acctApproveSettlementRepo.Update(approve, x => x.Id == approve.Id, false);
                                    }

                                    HandleState hsUpdateApproval = acctApproveSettlementRepo.SubmitChanges();
                                    // push user notification
                                    if (hsUpdateApproval.Success)
                                    {
                                        //Cập nhật status payment of Advance Request = NotSettled (Nếu có)
                                        var surchargeShipment = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settle.SettlementNo && !string.IsNullOrEmpty(x.AdvanceNo)).ToList();
                                        if (surchargeShipment != null && surchargeShipment.Count > 0)
                                        {
                                            var surchargeGroup = surchargeShipment.GroupBy(x => new { x.Hblid, x.AdvanceNo }).Select(x => x.Key);
                                            foreach (var shipment in surchargeGroup)
                                            {
                                                var advanceRequest = acctAdvanceRequestRepo.Get(x => x.Hblid == shipment.Hblid && x.AdvanceNo == shipment.AdvanceNo && x.StatusPayment != AccountingConstants.STATUS_PAYMENT_NOTSETTLED);
                                                foreach (var advRq in advanceRequest)
                                                {
                                                    advRq.StatusPayment = AccountingConstants.STATUS_PAYMENT_NOTSETTLED;
                                                    advRq.DatetimeModified = DateTime.Now;
                                                    advRq.UserModified = currentUser.UserID;
                                                    var hsUpdateAdvRequest = acctAdvanceRequestRepo.Update(advRq, x => x.Id == advRq.Id, false);
                                                }
                                            }
                                            acctAdvanceRequestRepo.SubmitChanges();
                                        }

                                        string title = string.Format(@"Accountant Denied Data Settlement {0}", settle.SettlementNo);
                                        string desc = string.Format(@"Settlement {0} has denied by {1}, Click to view", settle.SettlementNo, currentUser.UserName);
                                        SysNotifications sysNotify = new SysNotifications
                                        {
                                            Id = Guid.NewGuid(),
                                            DatetimeCreated = DateTime.Now,
                                            DatetimeModified = DateTime.Now,
                                            Type = "User",
                                            Title = title,
                                            Description = desc,
                                            IsClosed = false,
                                            IsRead = false,
                                            UserCreated = currentUser.UserID,
                                            UserModified = currentUser.UserID,
                                            Action = "Detail",
                                            ActionLink = string.Format(@"home/accounting/settlement-payment/{0}", settle.Id),
                                            UserIds = settle.UserCreated
                                        };

                                        sysNotificationRepository.Add(sysNotify);

                                        SysUserNotification sysUserNotify = new SysUserNotification
                                        {
                                            Id = Guid.NewGuid(),
                                            UserId = settle.UserCreated,
                                            Status = "New",
                                            NotitficationId = sysNotify.Id,
                                            DatetimeCreated = DateTime.Now,
                                            DatetimeModified = DateTime.Now,
                                            UserCreated = currentUser.UserID,
                                            UserModified = currentUser.UserID,
                                        };

                                        sysUserNotificationRepository.Add(sysUserNotify);
                                    }
                                }
                            }
                        }
                    }
                    trans.Commit();
                    foreach (Guid Id in Ids)
                    {
                        var settleNo = DataContext.Where(x => x.Id == Id).FirstOrDefault().SettlementNo;
                        var sendMailDeny = SendMailDeniedApproval(settleNo, comment, DateTime.Now);
                    }
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

        public bool CheckValidateDeleteSettle(string settlementNo)
        {
            bool isValidate = true;

            if (csShipmentSurchargeRepo.Any(x => x.SettlementCode == settlementNo
             && x.IsFromShipment == false &&
             (!string.IsNullOrEmpty(x.Soano)
             || !string.IsNullOrEmpty(x.PaySoano)
             || !string.IsNullOrEmpty(x.VoucherId)
             || !string.IsNullOrEmpty(x.VoucherIdre)
             || !string.IsNullOrEmpty(x.CreditNo)
             || !string.IsNullOrEmpty(x.DebitNo)
             || !string.IsNullOrEmpty(x.LinkChargeId)
             || x.AcctManagementId != null
             || x.PayerAcctManagementId != null
             )
            ))
            {
                isValidate = false;
            }

            return isValidate;
        }

        public bool CheckSettlementHaveShipmentLock(string settlementNo)
        {
            bool result = false;


            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IQueryable<CatPartner> GetPartnerForSettlement(ExistsChargeCriteria criteria)
        {
            var surcharge = csShipmentSurchargeRepo.Get(x => x.IsFromShipment == true);
            var partner = catPartnerRepo.Get();
            if (criteria.soaNo != null && criteria.soaNo.Count() > 0)
            {
                var soa = acctSoaRepo.Get(x => criteria.soaNo.Contains(x.Soano));
                var result = from pner in partner
                             join ss in soa on pner.Id equals ss.Customer
                             select pner;
                return result;
            }
            if (criteria.creditNo != null && criteria.creditNo.Count() > 0)
            {
                var cdNote = acctCdnoteRepo.Get(x => criteria.creditNo.Contains(x.Code) && x.Type == AccountingConstants.ACCOUNTANT_TYPE_CREDIT);
                var result = from pner in partner
                             join cd in cdNote on pner.Id equals cd.PartnerId
                             select pner;

                return result;
            }
            return partner.AsQueryable();
        }

        public string CheckSoaCDNoteIsSynced(ExistsChargeCriteria criteria)
        {
            var surcharges = csShipmentSurchargeRepo
                .Get(x =>
                        x.IsFromShipment == true
                     && ((x.Type == AccountingConstants.TYPE_CHARGE_BUY && x.PaymentObjectId == criteria.partnerId)
                     || (x.Type == AccountingConstants.TYPE_CHARGE_OBH && x.PayerId == criteria.partnerId))
                );
            string message = string.Empty;
            if (criteria.soaNo?.Count() > 0)
            {
                var surchargesFilter = surcharges.Where(x => criteria.soaNo.Contains(x.PaySoano) && x.Type == AccountingConstants.TYPE_CHARGE_OBH
                                                    && !string.IsNullOrEmpty(x.PaySyncedFrom));
                if (!surchargesFilter.Any())
                {
                    surchargesFilter = surcharges.Where(x => criteria.soaNo.Contains(x.PaySoano) && x.Type != AccountingConstants.TYPE_CHARGE_OBH
                                                    && !string.IsNullOrEmpty(x.SyncedFrom));
                }
                if (surchargesFilter.Any())
                {
                    message = "SOA: " + string.Join(',', surchargesFilter.Select(x => x.PaySoano).Distinct()) + " exist charges that synced to Accountant or made settlement, Please you check again!";
                }

            }
            if (criteria.creditNo?.Count() > 0)
            {
                var surchargesFilter = surcharges.Where(x => criteria.creditNo.Contains(x.CreditNo) && x.Type == AccountingConstants.TYPE_CHARGE_OBH
                                                    && !string.IsNullOrEmpty(x.PaySyncedFrom));
                if (!surchargesFilter.Any())
                {
                    surchargesFilter = surcharges.Where(x => criteria.creditNo.Contains(x.CreditNo) && x.Type != AccountingConstants.TYPE_CHARGE_OBH
                                                    && !string.IsNullOrEmpty(x.SyncedFrom));
                }
                if (surchargesFilter.Any())
                {
                    message = "CDNote: " + string.Join(',', surchargesFilter.Select(x => x.CreditNo).Distinct()) + " exist charges that synced to Accountant or made settlement, Please you check again!";
                }
            }
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="mbl"></param>
        /// <param name="hbl"></param>
        /// <returns></returns>
        public List<string> GetListAdvanceNoForShipment(Guid hblId, string payeeId = null, string requester = null, string settlementNo = null, bool isCopyCharge = false)
        {
            var advanceNoLst = acctAdvanceRequestRepo.Get(x => x.StatusPayment == AccountingConstants.STATUS_PAYMENT_NOTSETTLED && x.Hblid == hblId).Select(x => new { x.JobId, x.AdvanceNo }).Distinct().ToList();
            IQueryable<AcctAdvancePayment> advancePayments = null;
            var advanceNo = new List<string>();
            // Check if adv existed in another settlement
            foreach (var item in advanceNoLst)
            {
                var advanceExp = csShipmentSurchargeRepo.Any(x => item.AdvanceNo == x.AdvanceNo && item.JobId == x.JobNo && !string.IsNullOrEmpty(x.SettlementCode) && (x.SettlementCode != settlementNo || string.IsNullOrEmpty(settlementNo)));
                if (!advanceExp)
                {
                    advanceNo.Add(item.AdvanceNo);
                }
            }

            if (string.IsNullOrEmpty(payeeId))
            {
                // TH copy charge không dùng adv no của adv carrier
                advancePayments = acctAdvancePaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE && advanceNo.Any(ad => ad == (x.AdvanceNo)) && (isCopyCharge ? string.IsNullOrEmpty(x.AdvanceFor) : true)).OrderBy(x => x.RequestDate).ThenBy(x => x.DatetimeCreated);
            }
            else
            {
                if (!string.IsNullOrEmpty(payeeId))
                {
                    advancePayments = acctAdvancePaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE && advanceNo.Any(ad => ad == x.AdvanceNo) && (x.Payee == payeeId) && (isCopyCharge ? string.IsNullOrEmpty(x.AdvanceFor) : true)).OrderBy(x => x.RequestDate).ThenBy(x => x.DatetimeCreated);
                }
                // [CR]: tam thoi bo search theo requester
                //if (!string.IsNullOrEmpty(requester) && (advancePayments == null || advancePayments.Count() == 0))
                //{
                //    advancePayments = acctAdvancePaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE && advanceNo.Any(ad => ad == x.AdvanceNo) && x.Requester == requester);
                //}
            }
            if (!string.IsNullOrEmpty(requester)) // only for case copy charge get requester != null
            {
                advancePayments = advancePayments.Where(x => x.Requester == requester).OrderBy(x => x.RequestDate).ThenBy(x => x.DatetimeCreated);
            }
            return advancePayments == null ? new List<string>() : advancePayments.Select(x => x.AdvanceNo).ToList();
        }

        #region --- Calculator Receivable Settlement ---
        /// <summary>
        /// Tính công nợ dựa vào Settlement Code của Settlement
        /// </summary>
        /// <param name="settlementCode"></param>
        /// <returns></returns>
        public List<ObjectReceivableModel> CalculatorReceivableSettlement(string settlementCode)
        {
            var surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementCode).Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL);
            if (surcharges.Count() > 0)
            {
                return new List<ObjectReceivableModel>();
            }
            var objectReceivablesModel = accAccountReceivableService.GetObjectReceivableBySurcharges(surcharges);
            return objectReceivablesModel;
        }
        #endregion --- Calculator Receivable Settlement ---

        public HandleState CalculateBalanceSettle(List<string> settlementNo)
        {
            HandleState rs = new HandleState();
            if (settlementNo.Count() == 0)
            {
                return rs;
            }
            foreach (var item in settlementNo)
            {
                AcctSettlementPayment currentSettle = DataContext.Get(x => x.SettlementNo == item)?.FirstOrDefault();

                if (currentSettle != null)
                {
                    var charges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == item).ToList();

                    if (charges.Count < 0)
                    {
                        return rs;
                    }
                    List<ShipmentChargeSettlement> shipmentSettleCharges = new List<ShipmentChargeSettlement>();

                    foreach (var charge in charges)
                    {
                        ShipmentChargeSettlement shipmentSettleCharge = new ShipmentChargeSettlement
                        {
                            HBL = charge.Hblno,
                            Hblid = charge.Hblid,
                            MBL = charge.Mblno,
                            ClearanceNo = charge.ClearanceNo,
                            AdvanceNo = charge.AdvanceNo,
                            JobId = charge.JobNo
                        };
                        shipmentSettleCharges.Add(shipmentSettleCharge);
                    }

                    decimal? advanceAmount = CalculateBalanceSettle(shipmentSettleCharges, currentSettle.SettlementNo, currentSettle.SettlementCurrency);
                    if (advanceAmount != null)
                    {
                        currentSettle.AdvanceAmount = advanceAmount;
                        currentSettle.BalanceAmount = currentSettle.AdvanceAmount - currentSettle.Amount;

                        if (currentSettle.BalanceAmount == 0)
                        {
                            currentSettle.PaymentMethod = AccountingConstants.PAYMENT_METHOD_OTHER;
                        }
                    }

                    rs = DataContext.Update(currentSettle, x => x.Id == currentSettle.Id);
                }
            }

            return rs;
        }

        /// <summary>
        /// Get detail shipment of settle to export
        /// </summary>
        /// <param name="settlementNo"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        private List<ShipmentSettlementExportGroup> GetDataShipmentOfSettlement(IQueryable<AcctSettlementPayment> settlements)
        {
            var surcharges = csShipmentSurchargeRepo.Get();
            var custom = customsDeclarationRepo.Get();
            var sysUser = sysUserRepo.Get();
            var sysEmployee = sysEmployeeRepo.Get();
            var chargeDatas = catChargeRepo.Get();
            var UnitDatas = catUnitRepo.Get();
            var payer = catPartnerRepo.Get();
            var payee = catPartnerRepo.Get();
            var advanceReqData = acctAdvanceRequestRepo.Get();
            var approveSettlement = acctApproveSettlementRepo.Get();

            var data = from settle in settlements
                       join sur in surcharges on settle.SettlementNo equals sur.SettlementCode
                       join par in payer on sur.PayerId equals par.Id into par2
                       from par in par2.DefaultIfEmpty()
                       join pae in payee on sur.PaymentObjectId equals pae.Id into pae2
                       from pae in pae2.DefaultIfEmpty()
                       join vatPartner in payee on sur.VatPartnerId equals vatPartner.Id into vatPgrps
                       from vatPartner in vatPgrps.DefaultIfEmpty()
                       join charges in chargeDatas on sur.ChargeId equals charges.Id into grpCharge
                       from charge in grpCharge.DefaultIfEmpty()
                       join units in UnitDatas on sur.UnitId equals units.Id into grpUnit
                       from unit in grpUnit.DefaultIfEmpty()
                       join advanceReq in advanceReqData on new { sur.AdvanceNo, sur.JobNo, sur.Hblno } equals new { advanceReq.AdvanceNo, JobNo = advanceReq.JobId, Hblno = advanceReq.Hbl } into grpAdv
                       from adv in grpAdv.GroupBy(x => new { x.JobId, x.Hblid, x.AdvanceNo }).Select(x => new { AmountVnd = x.Sum(z => z.AmountVnd ?? 0), AmountUsd = x.Sum(z => z.AmountUsd ?? 0) }).Take(1).DefaultIfEmpty()
                       join cus in custom on sur.JobNo equals cus.JobNo into cus1
                       from cus in cus1.OrderBy(c => c.DatetimeCreated).Take(1).DefaultIfEmpty()
                       select new ShipmentSettlementExportGroup
                       {
                           SettleNo = settle.SettlementNo,
                           JobID = sur.JobNo,
                           HBL = sur.Hblno,
                           MBL = sur.Mblno,
                           CustomNo = cus == null ? string.Empty : cus.ClearanceNo,
                           ChargeCode = charge == null ? string.Empty : charge.Code,
                           ChargeName = charge == null ? string.Empty : charge.ChargeNameEn,
                           Quantity = sur.Quantity,
                           ChargeUnit = unit == null ? string.Empty : unit.UnitNameEn,
                           UnitPrice = sur.UnitPrice,
                           CurrencyId = sur.CurrencyId,
                           NetAmount = sur.NetAmount ?? 0,
                           Vatrate = sur.Vatrate ?? 0,
                           VatAmount = sur.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (sur.VatAmountVnd ?? 0) : (sur.VatAmountUsd ?? 0),
                           TotalAmount = sur.Total,
                           TotalAmountVnd = (sur.VatAmountVnd ?? 0) + (sur.AmountVnd ?? 0),
                           TotalAmountUsd = (sur.VatAmountUsd ?? 0) + (sur.AmountUsd ?? 0),
                           Payee = sur.Type == AccountingConstants.TYPE_CHARGE_BUY ? (pae == null ? string.Empty : pae.ShortName) : (par == null ? string.Empty : par.ShortName),
                           OBHPartnerName = sur.Type == AccountingConstants.TYPE_CHARGE_OBH ? (pae == null ? string.Empty : pae.ShortName) : (par == null ? string.Empty : par.ShortName),
                           InvoiceNo = sur.InvoiceNo,
                           SeriesNo = sur.SeriesNo,
                           InvoiceDate = sur.InvoiceDate,
                           VatPartner = vatPartner == null ? string.Empty : vatPartner.ShortName,
                           AdvanceNo = sur.AdvanceNo,
                           AdvanceAmount = adv == null ? 0m : (settle.SettlementCurrency == AccountingConstants.CURRENCY_LOCAL ? adv.AmountVnd : adv.AmountUsd)
                       };
            var groupData = data.OrderBy(x => x.SettleNo).ThenBy(x => x.MBL).ThenBy(x => x.HBL);
            return groupData.ToList();
        }

        /// <summary>
        /// Get data to export settlement list detail
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public List<AccountingSettlementExportGroup> GetDataExportSettlementDetail(AcctSettlementPaymentCriteria criteria)
        {
            var results = new List<AccountingSettlementExportGroup>();
            var settlementPayments = GetSettlementsByCriteria(criteria);
            if (settlementPayments == null || settlementPayments.Count() == 0)
            {
                return results;
            }

            var sysUser = sysUserRepo.Get();
            var sysEmployee = sysEmployeeRepo.Get();
            var approveSettlement = acctApproveSettlementRepo.Get();
            // Get data
            var detailShipments = GetDataShipmentOfSettlement(settlementPayments);
            var data = new List<AccountingSettlementExportGroup>();
            foreach (var settlement in settlementPayments)
            {
                var shipments = detailShipments.Where(x => x.SettleNo == settlement.SettlementNo).ToList();
                var shipmentGroup = shipments.GroupBy(x => new { x.JobID, x.MBL, x.HBL });
                var settle = new AccountingSettlementExportGroup();
                settle.SettlementNo = settlement.SettlementNo;
                if (!string.IsNullOrEmpty(settlement.Requester))
                {
                    var requesterId = sysUser.Where(x => x.Id == settlement.Requester).FirstOrDefault()?.EmployeeId;
                    settle.Requester = requesterId == null ? null : sysEmployee.Where(x => x.Id == requesterId).FirstOrDefault()?.EmployeeNameEn;
                }
                settle.RequestDate = settlement.RequestDate;
                settle.SettlementAmount = settlement.Amount ?? 0;
                settle.Currency = settlement.SettlementCurrency;
                settle.ApproveDate = approveSettlement.Where(x => x.SettlementNo == settlement.SettlementNo).FirstOrDefault()?.BuheadAprDate;
                settle.PaymentMethod = settlement.PaymentMethod;
                settle.DueDate = settlement.DueDate;
                settle.BankAccountNo = settlement.BankAccountNo;
                settle.BankAccountName = settlement.BankAccountName;
                settle.BankName = settlement.BankName;

                settle.ShipmentDetail = new List<ShipmentSettlementExportGroup>();
                settle.TotalNetAmount = 0;
                settle.TotalVatAmount = 0;
                settle.TotalAmount = 0;
                settle.TotalAmountVnd = 0;
                settle.TotalAdvanceAmount = 0;
                foreach (var item in shipmentGroup)
                {
                    var jobDetail = new ShipmentSettlementExportGroup();
                    var totalShipment = settle.Currency == "VND" ? item.Sum(x => x.TotalAmountVnd ?? 0) : item.Sum(x => x.TotalAmountUsd ?? 0);
                    var customNo = string.Join(',', item.Select(x => x.CustomNo).Distinct());
                    jobDetail.JobID = item.Key.JobID;
                    jobDetail.MBL = item.Key.MBL;
                    jobDetail.HBL = item.Key.HBL;
                    jobDetail.CustomNo = customNo;
                    jobDetail.AdvanceNo = string.Join(";", item.GroupBy(x => x.AdvanceNo).Select(x => x.Key));
                    jobDetail.NetAmount = item.Sum(x => x.NetAmount ?? 0);
                    jobDetail.VatAmount = item.Sum(x => x.VatAmount ?? 0);
                    jobDetail.TotalAmount = item.Sum(x => x.TotalAmount ?? 0);
                    jobDetail.TotalAmountVnd = item.Sum(x => x.TotalAmountVnd ?? 0);
                    jobDetail.AdvanceAmount = item.GroupBy(x => x.AdvanceNo).Sum(x => x.FirstOrDefault().AdvanceAmount ?? 0);
                    jobDetail.Balance = jobDetail.AdvanceAmount - totalShipment;

                    settle.TotalNetAmount += jobDetail.NetAmount;
                    settle.TotalVatAmount += jobDetail.VatAmount;
                    settle.TotalAmount += jobDetail.TotalAmount;
                    settle.TotalAmountVnd += jobDetail.TotalAmountVnd;
                    settle.TotalAdvanceAmount += jobDetail.AdvanceAmount;
                    var surcharges = from charge in item
                                     select new SurchargesShipmentSettlementExportGroup
                                     {
                                         ChargeCode = charge.ChargeCode,
                                         ChargeName = charge.ChargeName,
                                         Quantity = charge.Quantity,
                                         ChargeUnit = charge.ChargeUnit,
                                         UnitPrice = charge.UnitPrice,
                                         CurrencyId = charge.CurrencyId,
                                         NetAmount = charge.NetAmount,
                                         Vatrate = charge.Vatrate,
                                         VatAmount = charge.VatAmount,
                                         TotalAmount = charge.TotalAmount,
                                         TotalAmountVnd = charge.TotalAmountVnd,
                                         Payee = charge.Payee,
                                         OBHPartnerName = charge.OBHPartnerName,
                                         InvoiceNo = charge.InvoiceNo,
                                         SeriesNo = charge.SeriesNo,
                                         InvoiceDate = charge.InvoiceDate,
                                         VatPartner = charge.VatPartner
                                     };

                    jobDetail.surchargesDetail = new List<SurchargesShipmentSettlementExportGroup>();
                    jobDetail.surchargesDetail.AddRange(surcharges.OrderBy(x => x.ChargeCode).AsQueryable());
                    settle.ShipmentDetail.Add(jobDetail);
                }
                data.Add(settle);
            }
            var result = data.OrderBy(x => x.SettlementNo).ThenBy(x => x.ShipmentDetail.OrderBy(z => z.JobID)).AsQueryable();
            return result.ToList();
        }

        /// Get total Advance amount of Settlement
        /// </summary>
        /// <param name="charges"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        private decimal? GetAdvanceAmountSettle(List<ShipmentChargeSettlement> charges, string currency)
        {
            decimal? _advanceAmount = null;

            IEnumerable<ShipmentSettlement> dataGroups = charges.Where(x => !string.IsNullOrEmpty(x.AdvanceNo)).GroupBy(x => new { x.JobId, x.HBL, x.MBL, x.Hblid, x.AdvanceNo, ClearanceNo = (string.IsNullOrEmpty(x.ClearanceNo) ? null : x.ClearanceNo) })
                                    .Select(x => new ShipmentSettlement
                                    {
                                        JobId = x.Key.JobId,
                                        HBL = x.Key.HBL,
                                        MBL = x.Key.MBL,
                                        HblId = x.Key.Hblid,
                                        AdvanceNo = x.Key.AdvanceNo,
                                        CustomNo = x.Key.ClearanceNo
                                    });

            if (dataGroups != null && dataGroups.Count() > 0)
            {
                decimal? _totalAdvanceAmount = 0;
                var advData = from advP in acctAdvancePaymentRepo.Get(x => x.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                              join advR in acctAdvanceRequestRepo.Get() on advP.AdvanceNo equals advR.AdvanceNo
                              where dataGroups.Any(x => x.HblId == advR.Hblid && x.AdvanceNo == advR.AdvanceNo)
                              select new
                              {
                                  AdvAmount = advR.Amount * currencyExchangeService.CurrencyExchangeRateConvert(null, advP.RequestDate, advR.RequestCurrency, currency), // tính theo tỷ giá ngày request adv và currency settlement
                              };
                _totalAdvanceAmount = advData.ToList().Sum(x => x.AdvAmount ?? 0);
                if (_totalAdvanceAmount == 0)
                {
                    _advanceAmount = null;
                }
                else
                {
                    _advanceAmount = _totalAdvanceAmount;
                }
            }

            return _advanceAmount;

        }

        public List<ShipmentChargeSettlement> GetSurchargeDetailSettlement(string settlementNo, Guid? HblId = null, string advanceNo = null, string clearanceNo = null, int page = -1, int size = 30)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@SettlementNo", Value = settlementNo },
            };
            if (HblId != null)
            {
                parameters = parameters.Concat(new[] { new SqlParameter("@HblId", HblId) }).ToArray();
            }
            if (advanceNo != null)
            {
                parameters = parameters.Concat(new[] { new SqlParameter("@AdvanceNo", advanceNo) }).ToArray();
            }
            if (clearanceNo != null)
            {
                parameters = parameters.Concat(new[] { new SqlParameter("@ClearanceNo", clearanceNo) }).ToArray();
            }
            if (page > 0)
            {
                parameters = parameters.Concat(new[] { new SqlParameter("@Page", page), new SqlParameter("@Size", size) }).ToArray();
            }
            // List<sp_GetSurchargeDetailSettlement> listSurcharges = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetSurchargeDetailSettlement>(parameters);
            // var data = mapper.Map<List<ShipmentChargeSettlement>>(listSurcharges);

            var data = new List<ShipmentChargeSettlement>();
            using (SqlConnection connection = new SqlConnection(DbHelper.DbHelper.ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("sp_GetSurchargeDetailSettlement", connection))
                    {
                        command.CommandTimeout = 300;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddRange(parameters);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ShipmentChargeSettlement
                                {
                                    Id = Guid.Parse(reader["Id"].ToString()),
                                    Type = reader["Type"].ToString(),
                                    KickBack = reader["KickBack"] == DBNull.Value ? false : (bool?)reader["KickBack"],
                                    ChargeId = Guid.Parse(reader["ChargeId"].ToString()),
                                    ChargeName = reader["ChargeName"].ToString(),
                                    ChargeCode = reader["ChargeCode"].ToString(),
                                    ChargeGroup = reader["ChargeGroup"] == DBNull.Value ? Guid.Empty : Guid.Parse(reader["ChargeGroup"].ToString()),
                                    UnitId = reader["UnitId"] == DBNull.Value ? (short)0 : (short)reader["UnitId"],
                                    UnitName = reader["UnitName"].ToString(),
                                    Hblid = Guid.Parse(reader["HBLID"].ToString()),
                                    JobId = reader["JobId"].ToString(),
                                    MBL = reader["MBL"].ToString(),
                                    HBL = reader["HBL"].ToString(),
                                    SettlementCode = reader["SettlementCode"].ToString(),
                                    AdvanceNo = reader["AdvanceNo"] == DBNull.Value ? null : reader["AdvanceNo"].ToString(),
                                    OriginAdvanceNo = reader["OriginAdvanceNo"] == DBNull.Value ? null : reader["OriginAdvanceNo"].ToString(),
                                    FinalExchangeRate = reader["FinalExchangeRate"] == DBNull.Value ? 1 : (decimal)reader["FinalExchangeRate"],
                                    Quantity = reader["Quantity"] == DBNull.Value ? 0 : (decimal)reader["Quantity"],
                                    UnitPrice = reader["UnitPrice"] == DBNull.Value ? 0 : (decimal)reader["UnitPrice"],
                                    CurrencyId = reader["CurrencyId"].ToString(),
                                    Vatrate = reader["Vatrate"] == DBNull.Value ? 0 : (decimal)reader["Vatrate"],
                                    NetAmount = reader["NetAmount"] == DBNull.Value ? 0 : (decimal)reader["NetAmount"],
                                    Total = reader["Total"] == DBNull.Value ? 0 : (decimal)reader["Total"],
                                    AmountVnd = reader["AmountVND"] == DBNull.Value ? 0 : (decimal)reader["AmountVND"],
                                    AmountUSD = reader["AmountUSD"] == DBNull.Value ? 0 : (decimal)reader["AmountUSD"],
                                    VatAmountVnd = reader["VatAmountVnd"] == DBNull.Value ? 0 : (decimal)reader["VatAmountVnd"],
                                    VatAmountUSD = reader["VatAmountUSD"] == DBNull.Value ? 0 : (decimal)reader["VatAmountUSD"],
                                    TotalAmountVnd = reader["TotalAmountVnd"] == DBNull.Value ? 0 : (decimal)reader["TotalAmountVnd"],
                                    PayerId = reader["PayerId"] == DBNull.Value ? null : reader["PayerId"].ToString(),
                                    VatPartnerId = reader["VatPartnerId"] == DBNull.Value ? string.Empty : reader["VatPartnerId"].ToString(),
                                    PaymentObjectId = reader["PaymentObjectId"] == DBNull.Value ? string.Empty : reader["PaymentObjectId"].ToString(),
                                    VatPartnerShortName = reader["VatPartnerShortName"] == DBNull.Value ? string.Empty : reader["VatPartnerShortName"].ToString(),
                                    InvoiceNo = reader["InvoiceNo"] == DBNull.Value ? string.Empty : reader["InvoiceNo"].ToString(),
                                    SeriesNo = reader["SeriesNo"] == DBNull.Value ? string.Empty : reader["SeriesNo"].ToString(),
                                    InvoiceDate = reader["InvoiceDate"] == DBNull.Value ? null : (DateTime?)reader["InvoiceDate"],
                                    ClearanceNo = reader["clearanceNo"] == DBNull.Value ? null : reader["clearanceNo"].ToString(),
                                    ContNo = reader["ContNo"] == DBNull.Value ? string.Empty : reader["ContNo"].ToString(),
                                    Notes = reader["Notes"] == DBNull.Value ? string.Empty : reader["Notes"].ToString(),
                                    IsFromShipment = reader["IsFromShipment"] == DBNull.Value ? false : (bool?)reader["IsFromShipment"],
                                    TypeOfFee = reader["TypeOfFee"] == DBNull.Value ? string.Empty : reader["TypeOfFee"].ToString(),
                                    SyncedFrom = reader["SyncedFrom"] == DBNull.Value ? string.Empty : reader["SyncedFrom"].ToString(),
                                    PaySyncedFrom = reader["PaySyncedFrom"] == DBNull.Value ? null : reader["PaySyncedFrom"].ToString(),
                                    Soano = reader["SOANo"] == DBNull.Value ? string.Empty : reader["SOANo"].ToString(),
                                    PaySoano = reader["PaySOANo"] == DBNull.Value ? string.Empty : reader["PaySOANo"].ToString(),
                                    CreditNo = reader["CreditNo"] == DBNull.Value ? string.Empty : reader["CreditNo"].ToString(),
                                    DebitNo = reader["DebitNo"] == DBNull.Value ? string.Empty : reader["DebitNo"].ToString(),
                                    LinkChargeId = reader["LinkChargeId"] == DBNull.Value ? null : reader["LinkChargeId"].ToString(),
                                    SyncedFromBy = reader["SyncedFromBy"] == DBNull.Value ? string.Empty : reader["SyncedFromBy"].ToString(),
                                    PICName = reader["PICName"] == DBNull.Value ? string.Empty : reader["PICName"].ToString(),
                                    ShipmentId = Guid.Parse(reader["ShipmentId"].ToString()),
                                    IsLocked = reader["IsLocked"] == DBNull.Value ? false : (bool?)reader["IsLocked"],
                                    TypeService = reader["TypeService"] == DBNull.Value ? string.Empty : reader["TypeService"].ToString(),
                                    Payer = reader["Payer"] == DBNull.Value ? string.Empty : reader["Payer"].ToString(),
                                    HasNotSynce = (bool?)reader["HasNotSynce"],
                                    HadIssued = (bool?)reader["HadIssued"],
                                    PayeeIssued = (bool?)reader["PayeeIssued"],
                                    OBHPartnerIssued = (bool?)reader["OBHPartnerIssued"],
                                    ChargeAutoRated = (bool?)reader["ChargeAutoRated"],
                                    OBHPartnerName = reader["OBHPartnerName"] == DBNull.Value ? string.Empty : reader["OBHPartnerName"].ToString(),
                                };

                                data.Add(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    new LogHelper("GetDataExportAccountantError", ex.Message?.ToString());
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
            return data;
        }

        /// <summary>
        /// Check allow update direct charges in list detail settle
        /// </summary>
        /// <param name="shipmentCharges"></param>
        /// <returns></returns>
        public ResultHandle CheckAllowUpdateDirectCharges(List<ShipmentChargeSettlement> shipmentCharges)
        {
            var chargeIds = shipmentCharges.Select(x => x.Id).ToList();
            var surcharges = csShipmentSurchargeRepo.Get(x => chargeIds.Any(z => z == x.Id) && x.IsFromShipment == false);
            var hs = new ResultHandle();
            if (surcharges.Any(x => !string.IsNullOrEmpty(x.CreditNo) || !string.IsNullOrEmpty(x.DebitNo)))
            {
                return new ResultHandle { Status = false, Message = "You can't update charges have been issued CDNOTE", Data = surcharges };
            }
            if (surcharges.Any(x => !string.IsNullOrEmpty(x.Soano) || !string.IsNullOrEmpty(x.PaySoano)))
            {
                return new ResultHandle { Status = false, Message = "You can't update charges have been issued SOA", Data = surcharges };
            }
            if (surcharges.Any(x => !string.IsNullOrEmpty(x.SyncedFrom) || !string.IsNullOrEmpty(x.PaySyncedFrom)))
            {
                return new ResultHandle { Status = false, Message = "You can't update charges have been synced", Data = surcharges };
            }
            if (surcharges.Any(x => !string.IsNullOrEmpty(x.VoucherId) || !string.IsNullOrEmpty(x.VoucherIdre)))
            {
                return new ResultHandle { Status = false, Message = "You can't update charges have been issued voucher", Data = surcharges };
            }
            return new ResultHandle();
        }

        /// <summary>
        /// Check allow deby settlement
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ResultHandle CheckAllowDenySettle(List<Guid> ids)
        {
            if (false)
            {
                #region Check allow deny direct settlement nếu có charge obh đã issue debit/soa
                var invalidSettles = new List<Guid>();
                var invalidCodeSettles = new List<string>();

                var csLinkCharges = csLinkChargeRepository.Get(x => x.LinkChargeType == AccountingConstants.LINK_TYPE_AUTO_RATE);
                var settlementData = DataContext.Get(x => ids.Any(z => z == x.Id));
                var surcharges = csShipmentSurchargeRepo.Get(x => x.Type != AccountingConstants.TYPE_CHARGE_OBH);
                var chargesSelling = surcharges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL);
                foreach (var settlementId in ids)
                {
                    var detail = settlementData.First(x => x.Id == settlementId);
                    if (detail == null) return null;

                    #region // Bỏ rule => Check Settlements had OBH Partner issue Debit/Soa. Please re-check.
                    //if (detail.SettlementType == "DIRECT")
                    //{
                    //    var obhDebitSurcharges = csShipmentSurchargeRepo.Get(x => x.IsFromShipment == false && x.SettlementCode == detail.SettlementNo && x.Type == AccountingConstants.TYPE_CHARGE_OBH && (!string.IsNullOrEmpty(x.DebitNo) || !string.IsNullOrEmpty(x.Soano))).ToList();
                    //    var isDebit = acctCdnoteRepo.Any(x => obhDebitSurcharges.Any(z => z.DebitNo == x.Code && z.PaymentObjectId == x.PartnerId));
                    //    var isSoa = acctSoaRepo.Any(x => obhDebitSurcharges.Any(z => z.Soano == x.Soano && z.PaymentObjectId == x.Customer));
                    //    if (isDebit || isSoa)
                    //    {
                    //        invalidSettles.Add(settlementId);
                    //        invalidCodeSettles.Add(detail.SettlementNo);
                    //    }
                    //}
                    //if (invalidSettles.Count > 0)
                    //            {
                    //                return new ResultHandle { Status = false, Message = string.Format("Settlements : {0} had OBH Partner issue Debit/Soa. Please re-check.", invalidCodeSettles.Join(",")), Data = invalidSettles };
                    //            }
                    #endregion

                    // [CR:17807]: Không cho phép DENY bất khì phiếu Settlement nào có phí đã Autorate
                    var surchargesSettle = surcharges.Where(x => x.SettlementCode == detail.SettlementNo);
                    var chargesLinked = from sur in surchargesSettle
                                        join linkChg in csLinkCharges on sur.Id.ToString() equals linkChg.ChargeOrgId
                                        join sell in chargesSelling on linkChg.ChargeLinkId equals sell.ToString()
                                        select sell.Id;
                    if (chargesLinked != null && chargesLinked.Count() > 0)
                    {
                        invalidSettles.Add(settlementId);
                        invalidCodeSettles.Add(detail.SettlementNo);
                    }
                }
                if (invalidSettles.Count > 0)
                {
                    return new ResultHandle { Status = false, Message = string.Format("Settlements : {0} had auto rate fees. You can not deny.", invalidCodeSettles.Join(",")), Data = invalidSettles };
                }
                #endregion
            }
            return new ResultHandle();
        }

        /// <summary>
        /// Check if settlement has autorate charges
        /// </summary>
        /// <param name="settlementNo"></param>
        /// <returns></returns>
        public bool CheckSettleHasAutoRateCharges(string settlementNo)
        {
            var csLinkCharges = csLinkChargeRepository.Get(x => x.LinkChargeType == AccountingConstants.LINK_TYPE_AUTO_RATE);
            var surcharges = csShipmentSurchargeRepo.Get(x => x.Type != AccountingConstants.TYPE_CHARGE_OBH);
            var chargesSelling = surcharges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL);
            var surchargesSettle = surcharges.Where(x => x.SettlementCode == settlementNo);
            var chargesLinked = from sur in surchargesSettle
                                join linkChg in csLinkCharges on sur.Id.ToString() equals linkChg.ChargeOrgId
                                join sell in chargesSelling on linkChg.ChargeLinkId equals sell.ToString()
                                select sell.Id;
            if (chargesLinked != null && chargesLinked.Any())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update surcharge settlement
        /// </summary>
        /// <param name="newSurcharges"></param>
        /// <param name="settleCode"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public void UpdateSurchargeSettle(List<ShipmentChargeSettlement> newSurcharges, string settleCode, string action)
        {
            decimal kickBackExcRate = currentUser.KbExchangeRate ?? 20000;
            new LogHelper("EFMS_LOG_UPD_SETTLEMENT", "Settle :" + settleCode + " - Action: " + action + " User: " + currentUser.UserName + " - " + currentUser.UserID + " - Department: " + currentUser.DepartmentId);
            if (action == "Add")
            {
                #region Add
                //Lấy các phí chứng từ IsFromShipment = true
                var chargeShipment = newSurcharges.Where(x => x.Id != Guid.Empty && x.IsFromShipment == true).ToList();
                if (chargeShipment.Count > 0)
                {
                    //var listChargeShipment = csShipmentSurchargeRepo.Get(x => chargeShipment.Contains(x.Id)).ToList();
                    var newSurchargeList = mapper.Map<List<CsShipmentSurcharge>>(chargeShipment);
                    databaseUpdateService.UpdateSurchargeSettleDataToDB(newSurchargeList, settleCode, kickBackExcRate, action);
                    #region del old
                    //var listChargeShipment = csShipmentSurchargeRepo.Get(x => chargeShipment.Contains(x.Id)).ToList();
                    //foreach (var charge in listChargeShipment)
                    //{
                    //    // Phí Chứng từ cho phép cập nhật lại số HD, Ngày HD, Số SerieNo, Note.
                    //    var chargeSettlementCurrentToAddCsShipmentSurcharge = model.ShipmentCharge.First(x => x.Id == charge.Id);
                    //    if (chargeSettlementCurrentToAddCsShipmentSurcharge != null)
                    //    {
                    //        var exchangeRate = chargeSettlementCurrentToAddCsShipmentSurcharge.FinalExchangeRate;
                    //        charge.AdvanceNo = chargeSettlementCurrentToAddCsShipmentSurcharge.AdvanceNo;
                    //        charge.Notes = chargeSettlementCurrentToAddCsShipmentSurcharge.Notes;
                    //        charge.SeriesNo = chargeSettlementCurrentToAddCsShipmentSurcharge.SeriesNo;
                    //        charge.InvoiceNo = chargeSettlementCurrentToAddCsShipmentSurcharge.InvoiceNo;
                    //        charge.InvoiceDate = chargeSettlementCurrentToAddCsShipmentSurcharge.InvoiceDate;
                    //        charge.FinalExchangeRate = charge.FinalExchangeRate == exchangeRate ? charge.FinalExchangeRate
                    //                                                                            : (charge.Type == AccountingConstants.TYPE_CHARGE_BUY && charge.KickBack == true) ? kickBackExcRate : exchangeRate;
                    //        charge.AmountVnd = chargeSettlementCurrentToAddCsShipmentSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                    //        charge.VatAmountVnd = chargeSettlementCurrentToAddCsShipmentSurcharge.VatAmountVnd; //Tiền thuế (Local)
                    //        charge.VatPartnerId = chargeSettlementCurrentToAddCsShipmentSurcharge.VatPartnerId; // Đối tượng trên đầu hóa đơn
                    //    }

                    //    charge.SettlementCode = settlement.SettlementNo;
                    //    charge.UserModified = userCurrent;
                    //    charge.DatetimeModified = DateTime.Now;

                    //    _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, settlement.SettlementCurrency);

                    //    csShipmentSurchargeRepo.Update(charge, x => x.Id == charge.Id);
                    //}
                    #endregion
                }

                //Lấy các phí hiện trường IsFromShipment = false & thực hiện insert các charge mới
                var chargeScene = newSurcharges.Where(x => x.Id == Guid.Empty && x.IsFromShipment == false).ToList();
                if (chargeScene.Count > 0)
                {
                    var listChargeSceneAdd = mapper.Map<List<CsShipmentSurcharge>>(chargeScene);
                    foreach (ShipmentChargeSettlement itemScene in chargeScene)
                    {
                        foreach (CsShipmentSurcharge itemSceneAdd in listChargeSceneAdd)
                        {
                            if (itemSceneAdd.Id == itemScene.Id && itemSceneAdd.Hblid == itemScene.Hblid)
                            {
                                itemSceneAdd.JobNo = itemScene.JobId;
                                itemSceneAdd.Mblno = itemScene.MBL;
                                itemSceneAdd.Hblno = itemScene.HBL;
                            }
                        }
                        #region del old
                        //foreach (var charge in listChargeSceneAdd)
                        //{
                        //    charge.Id = Guid.NewGuid();
                        //    charge.SettlementCode = settlement.SettlementNo;
                        //    charge.DatetimeCreated = charge.DatetimeModified = DateTime.Now;
                        //    charge.UserCreated = charge.UserModified = userCurrent;
                        //    charge.ExchangeDate = DateTime.Now;

                        //    #region -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                        //    var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(charge, kickBackExcRate);
                        //    charge.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                        //    charge.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                        //    charge.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                        //    charge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                        //    charge.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                        //    charge.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                        //    charge.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                        //    #endregion -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

                        //    _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, settlement.SettlementCurrency);

                        //    charge.TransactionType = GetTransactionTypeOfChargeByHblId(charge.Hblid);
                        //    charge.OfficeId = currentUser.OfficeID;
                        //    charge.CompanyId = currentUser.CompanyID;

                        //    csShipmentSurchargeRepo.Add(charge);
                        //}
                        #endregion
                    }
                    databaseUpdateService.UpdateSurchargeSettleDataToDB(listChargeSceneAdd, settleCode, kickBackExcRate, action);
                }
                #endregion
            }
            else if (action == "Update")
            {
                #region Update
                //Start --Phí chứng từ (IsFromShipment = true)--
                var settlement = DataContext.Get(x => x.SettlementNo == settleCode).FirstOrDefault();
                if (settlement.SettlementType == "EXISTING")
                {
                    //Cập nhật SettlementCode = null cho các SettlementNo
                    var chargeShipmentOld = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlement.SettlementNo && x.IsFromShipment == true);
                    if (chargeShipmentOld.Count() > 0)
                    {
                        var advUpdGrp = chargeShipmentOld.Where(x => !string.IsNullOrEmpty(x.AdvanceNo)).GroupBy(x => new { x.Hblid, x.AdvanceNo }).Select(x => new
                        {
                            x.Key.Hblid,
                            x.Key.AdvanceNo
                        }).ToList();
                        var advRequestList = acctAdvanceRequestRepo.Get(x => advUpdGrp.Any(z => z.Hblid == x.Hblid && z.AdvanceNo == x.AdvanceNo));
                        #region -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]
                        acctAdvancePaymentService.UpdateStatusPaymentNotSettledOfAdvanceRequest(advRequestList);
                        #endregion -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]
                    }
                    var newSurchargeList = mapper.Map<List<CsShipmentSurcharge>>(newSurcharges);
                    databaseUpdateService.UpdateSurchargeSettleDataToDB(newSurchargeList, settlement.SettlementNo, kickBackExcRate, action);
                    #region
                    //var listChargeShipmentUpdate = csShipmentSurchargeRepo.Get(x => chargeShipmentUpdate.Contains(x.Id)).ToList();
                    //foreach (var charge in listChargeShipmentUpdate)
                    //{
                    //    // Phí Chứng từ cho phép cập nhật lại số HD, Ngày HD, Số SerieNo, Note.
                    //    var chargeSettlementCurrentToUpdateCsShipmentSurcharge = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && x.IsFromShipment == true && x.Id == charge.Id)?.FirstOrDefault();
                    //    var exchangeRate = chargeSettlementCurrentToUpdateCsShipmentSurcharge.FinalExchangeRate;
                    //    charge.AdvanceNo = chargeSettlementCurrentToUpdateCsShipmentSurcharge.AdvanceNo;
                    //    charge.Notes = chargeSettlementCurrentToUpdateCsShipmentSurcharge.Notes;
                    //    charge.SeriesNo = chargeSettlementCurrentToUpdateCsShipmentSurcharge.SeriesNo;
                    //    charge.InvoiceNo = chargeSettlementCurrentToUpdateCsShipmentSurcharge.InvoiceNo;
                    //    charge.InvoiceDate = chargeSettlementCurrentToUpdateCsShipmentSurcharge.InvoiceDate;
                    //    charge.VatPartnerId = chargeSettlementCurrentToUpdateCsShipmentSurcharge.VatPartnerId;
                    //    charge.FinalExchangeRate = charge.FinalExchangeRate == exchangeRate ? charge.FinalExchangeRate
                    //                                                                            : (charge.Type == AccountingConstants.TYPE_CHARGE_BUY && charge.KickBack == true) ? kickBackExcRate : exchangeRate;
                    //    charge.AmountVnd = chargeSettlementCurrentToUpdateCsShipmentSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                    //    charge.VatAmountVnd = chargeSettlementCurrentToUpdateCsShipmentSurcharge.VatAmountVnd; //Tiền thuế (Local)

                    //    charge.SettlementCode = settlement.SettlementNo;
                    //    charge.UserModified = userCurrent;
                    //    charge.DatetimeModified = DateTime.Now;

                    //    _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, settlement.SettlementCurrency);

                    //    csShipmentSurchargeRepo.Update(charge, x => x.Id == charge.Id);
                    //}
                    #endregion
                }
                //End --Phí chứng từ (IsFromShipment = true)--

                //Start --Phí hiện trường (IsFromShipment = false)--
                else
                {
                    var chargeScene = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlement.SettlementNo && x.IsFromShipment == false).ToList();
                    var listChargeSceneUpdate = mapper.Map<List<CsShipmentSurcharge>>(newSurcharges);
                    foreach (var itemScene in newSurcharges)
                    {
                        foreach (var itemSceneAdd in listChargeSceneUpdate)
                        {
                            if (itemSceneAdd.Id == itemScene.Id && itemSceneAdd.Hblid == itemScene.Hblid)
                            {
                                itemSceneAdd.JobNo = itemScene.JobId;
                                itemSceneAdd.Mblno = itemScene.MBL;
                                itemSceneAdd.Hblno = itemScene.HBL;
                            }
                        }
                    }
                    databaseUpdateService.UpdateSurchargeSettleDataToDB(listChargeSceneUpdate, settlement.SettlementNo, kickBackExcRate, action);

                    var idsChargeScene = chargeScene.Select(x => x.Id);
                    //Cập nhật lại các thông tin của phí hiện trường (nếu có edit chỉnh sửa phí hiện trường)
                    var chargeSceneUpdate = newSurcharges.Where(x => x.Id != Guid.Empty && idsChargeScene.Contains(x.Id) && x.IsFromShipment == false);

                    var idChargeSceneUpdate = chargeSceneUpdate.Select(s => s.Id).ToList();

                    if (chargeSceneUpdate.Count() > 0)
                    {
                        var listChargeExists = csShipmentSurchargeRepo.Get(x => idChargeSceneUpdate.Contains(x.Id));

                        #region -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]
                        var advUpdGrp = listChargeExists.Where(x => !string.IsNullOrEmpty(x.AdvanceNo)).GroupBy(x => new { x.Hblid, x.AdvanceNo }).Select(x => new
                        {
                            x.Key.Hblid,
                            x.Key.AdvanceNo
                        }).ToList();
                        var advRequestList = acctAdvanceRequestRepo.Get(x => advUpdGrp.Any(z => z.Hblid == x.Hblid && z.AdvanceNo == x.AdvanceNo));
                        acctAdvancePaymentService.UpdateStatusPaymentNotSettledOfAdvanceRequest(advRequestList);
                        #endregion -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]
                    }

                    #region Delete Old
                    //if (chargeSceneAdd.Count > 0)
                    //{
                    //    var listChargeSceneAdd = mapper.Map<List<CsShipmentSurcharge>>(chargeSceneAdd);
                    //    foreach (ShipmentChargeSettlement itemScene in chargeSceneAdd)
                    //    {
                    //        foreach (CsShipmentSurcharge itemSceneAdd in listChargeSceneAdd)
                    //        {
                    //            if (itemSceneAdd.Id == itemScene.Id && itemSceneAdd.Hblid == itemScene.Hblid)
                    //            {
                    //                itemSceneAdd.JobNo = itemScene.JobId;
                    //                itemSceneAdd.Mblno = itemScene.MBL;
                    //                itemSceneAdd.Hblno = itemScene.HBL;
                    //                // itemSceneAdd.Hblid = itemScene.Hblid;

                    //            }
                    //        }
                    //    }
                    //    foreach (var charge in listChargeSceneAdd)
                    //    {
                    //        charge.Id = Guid.NewGuid();
                    //        charge.SettlementCode = settlement.SettlementNo;
                    //        charge.DatetimeCreated = charge.DatetimeModified = DateTime.Now;
                    //        charge.UserCreated = charge.UserModified = userCurrent;
                    //        charge.ExchangeDate = DateTime.Now;
                    //        charge.TransactionType = GetTransactionTypeOfChargeByHblId(charge.Hblid);
                    //        charge.OfficeId = currentUser.OfficeID;
                    //        charge.CompanyId = currentUser.CompanyID;
                    //        charge.CreditNo = charge.DebitNo = charge.Soano = charge.PaySoano = null;  // refresh các hđ trước đó

                    //        #region -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                    //        var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(charge, kickBackExcRate);
                    //        charge.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                    //        charge.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                    //        charge.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                    //        charge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                    //        charge.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                    //        charge.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                    //        charge.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                    //        #endregion -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

                    //        _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, settlement.SettlementCurrency);

                    //        csShipmentSurchargeRepo.Add(charge);
                    //    }
                    //}

                    ////Cập nhật lại các thông tin của phí hiện trường (nếu có edit chỉnh sửa phí hiện trường)
                    //var chargeSceneUpdate = model.ShipmentCharge.Where(x => x.Id != Guid.Empty && idsChargeScene.Contains(x.Id) && x.IsFromShipment == false);

                    //var idChargeSceneUpdate = chargeSceneUpdate.Select(s => s.Id).ToList();
                    //if (chargeSceneUpdate.Count() > 0)
                    //{
                    //    var listChargeExists = csShipmentSurchargeRepo.Get(x => idChargeSceneUpdate.Contains(x.Id));

                    //    #region -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]
                    //    foreach (var chargeExist in listChargeExists)
                    //    {
                    //        acctAdvancePaymentService.UpdateStatusPaymentNotSettledOfAdvanceRequest(chargeExist.Hblid, chargeExist.AdvanceNo);
                    //    }
                    //    #endregion -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]

                    //    var listChargeSceneUpdate = mapper.Map<List<CsShipmentSurcharge>>(chargeSceneUpdate);
                    //    foreach (ShipmentChargeSettlement itemScene in chargeSceneUpdate)
                    //    {

                    //        foreach (CsShipmentSurcharge itemSceneUpdate in listChargeSceneUpdate)
                    //        {
                    //            if (string.IsNullOrEmpty(itemScene.LinkChargeId))
                    //            {
                    //                if (itemSceneUpdate.Id == itemScene.Id)
                    //                {
                    //                    itemSceneUpdate.JobNo = itemScene.JobId;
                    //                    itemSceneUpdate.Mblno = itemScene.MBL;
                    //                    itemSceneUpdate.Hblno = itemScene.HBL;
                    //                    itemSceneUpdate.Hblid = itemScene.Hblid;
                    //                }
                    //            }
                    //        }
                    //    }
                    //    foreach (var item in listChargeSceneUpdate)
                    //    {
                    //        var sceneCharge = listChargeExists.Where(x => x.Id == item.Id).FirstOrDefault();

                    //        if (sceneCharge != null)
                    //        {
                    //            if (string.IsNullOrEmpty(item.LinkChargeId) && string.IsNullOrEmpty(item.SyncedFrom) && string.IsNullOrEmpty(item.PaySyncedFrom))
                    //            {
                    //                sceneCharge.UnitId = item.UnitId;
                    //                sceneCharge.UnitPrice = item.UnitPrice;
                    //                sceneCharge.ChargeId = item.ChargeId;
                    //                sceneCharge.Quantity = item.Quantity;
                    //                sceneCharge.CurrencyId = item.CurrencyId;
                    //                sceneCharge.Vatrate = item.Vatrate;
                    //                sceneCharge.ContNo = item.ContNo;
                    //                sceneCharge.InvoiceNo = item.InvoiceNo;
                    //                sceneCharge.InvoiceDate = item.InvoiceDate;
                    //                sceneCharge.SeriesNo = item.SeriesNo;
                    //                sceneCharge.Notes = item.Notes;
                    //                // không cho cập nhật payee hoặc obh partner nếu charges đã issued
                    //                if (sceneCharge.Type == AccountingConstants.TYPE_CHARGE_OBH && string.IsNullOrEmpty(sceneCharge.PaySoano) && string.IsNullOrEmpty(sceneCharge.CreditNo))
                    //                {
                    //                    sceneCharge.PayerId = item.PayerId;
                    //                }
                    //                if (string.IsNullOrEmpty(sceneCharge.Soano) && string.IsNullOrEmpty(sceneCharge.DebitNo) && string.IsNullOrEmpty(sceneCharge.PaySoano) && string.IsNullOrEmpty(sceneCharge.CreditNo))
                    //                {
                    //                    sceneCharge.PaymentObjectId = item.PaymentObjectId;
                    //                }
                    //                sceneCharge.Type = item.Type;
                    //                sceneCharge.ChargeGroup = item.ChargeGroup;
                    //                sceneCharge.VatPartnerId = item.VatPartnerId;

                    //                sceneCharge.ClearanceNo = item.ClearanceNo;
                    //                sceneCharge.AdvanceNo = item.AdvanceNo;
                    //                sceneCharge.JobNo = item.JobNo;
                    //                sceneCharge.Mblno = item.Mblno;
                    //                sceneCharge.Hblno = item.Hblno;
                    //                sceneCharge.Hblid = item.Hblid;

                    //                sceneCharge.UserModified = userCurrent;
                    //                sceneCharge.DatetimeModified = DateTime.Now;
                    //            }


                    //            #region -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --
                    //            var amountSurcharge = currencyExchangeService.CalculatorAmountSurcharge(sceneCharge, kickBackExcRate);
                    //            sceneCharge.NetAmount = amountSurcharge.NetAmountOrig; //Thành tiền trước thuế (Original)
                    //            sceneCharge.Total = amountSurcharge.GrossAmountOrig; //Thành tiền sau thuế (Original)
                    //            sceneCharge.FinalExchangeRate = amountSurcharge.FinalExchangeRate; //Tỉ giá so với Local
                    //            sceneCharge.AmountVnd = amountSurcharge.AmountVnd; //Thành tiền trước thuế (Local)
                    //            sceneCharge.VatAmountVnd = amountSurcharge.VatAmountVnd; //Tiền thuế (Local)
                    //            sceneCharge.AmountUsd = amountSurcharge.AmountUsd; //Thành tiền trước thuế (USD)
                    //            sceneCharge.VatAmountUsd = amountSurcharge.VatAmountUsd; //Tiền thuế (USD)
                    //            #endregion -- Tính giá trị các field cho phí hiện trường: FinalExchangeRate, NetAmount, Total, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd --

                    //            _totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(sceneCharge, settlement.SettlementCurrency);

                    //            csShipmentSurchargeRepo.Update(sceneCharge, x => x.Id == sceneCharge.Id);
                    //        }
                    //    }
                    //}

                    ////Xóa các phí hiện trường đã chọn xóa của user
                    //var chargeSceneRemove = chargeScene.Where(x => !model.ShipmentCharge.Select(s => s.Id).Contains(x.Id)).ToList();
                    //if (chargeSceneRemove.Count > 0)
                    //{
                    //    foreach (var item in chargeSceneRemove)
                    //    {
                    //        if (string.IsNullOrEmpty(item.LinkChargeId))
                    //        {
                    //            csShipmentSurchargeRepo.Delete(x => x.Id == item.Id);
                    //        }
                    //    }
                    //}
                    ////End --Phí hiện trường (IsFromShipment = false)--
                    #endregion
                }
                //End --Phí hiện trường (IsFromShipment = false)--
                #endregion
            }
            else if (action == "Delete")
            {
                //Phí chừng từ (cập nhật lại SettlementCode, AdvanceNo bằng null)
                var surchargeShipment = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settleCode && x.IsFromShipment == true).ToList();
                if (surchargeShipment != null && surchargeShipment.Count > 0)
                {
                    var advUpdGrp = surchargeShipment.Where(x => !string.IsNullOrEmpty(x.AdvanceNo)).GroupBy(x => new { x.Hblid, x.AdvanceNo }).Select(x => new
                    {
                        x.Key.Hblid,
                        x.Key.AdvanceNo
                    }).ToList(); ;
                    var advRequestList = acctAdvanceRequestRepo.Get(x => advUpdGrp.Any(z => z.Hblid == x.Hblid && z.AdvanceNo == x.AdvanceNo) && x.StatusPayment != AccountingConstants.STATUS_PAYMENT_NOTSETTLED);
                    #region -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]
                    acctAdvancePaymentService.UpdateStatusPaymentNotSettledOfAdvanceRequest(advRequestList);
                    #endregion -- Cập nhật Status Payment = 'NotSettled' của Advance Request cho các phí của Settlement (nếu có) -- [15/01/2021]
                }
                // Update/Delete surcharge of settlement
                databaseUpdateService.UpdateSurchargeSettleDataToDB(surchargeShipment, settleCode, kickBackExcRate, action);
            }
        }

        /// <summary>
        /// Call Auto replicate after done settle
        /// </summary>
        /// <param name="settleNo"></param>
        /// <returns></returns>
        public async Task<ResultHandle> AutoRateReplicateFromSettle(Guid settleId)
        {
            var settlement = DataContext.Get(x => x.Id == settleId).FirstOrDefault();
            if (settlement.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE) // Get Auto rate fees when settlement Done
            {
                return new ResultHandle();
            }
            var surcharges = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlement.SettlementNo);
            try
            {
                var jobNos = surcharges.Select(x => x.JobNo).Distinct().ToList();
                var validAutorate = opsTransactionRepo.Any(x => jobNos.Any(z => z == x.JobNo) && x.LinkSource == AccountingConstants.TYPE_LINK_SOURCE_SHIPMENT && x.ReplicatedId == null);
                if (!validAutorate)
                {
                    var outSourceOffice = sysOfficeRepo.Get(x => x.OfficeType == AccountingConstants.OFFICE_TYPE_OUTSOURCE).Select(x => x.Id).ToList();
                    validAutorate = opsTransactionRepo.Any(x => jobNos.Any(z => z == x.JobNo) && outSourceOffice.Any(ofi => ofi == x.OfficeId));
                }
                var response = new ResultHandle();
                if (validAutorate)
                {
                    Uri urlDocumentation = new Uri(apiUrl.Value.Url);
                    string urlAutorate = new Uri(urlDocumentation, "Documentation/api/v1/en-US/OpsTransaction/AutoRateReplicate?settleNo=" + settlement.SettlementNo + "&&jobNo=null").ToString();
                    HttpResponseMessage resquest = await HttpClientService.GetApi(urlAutorate, null);
                    response = await resquest.Content.ReadAsAsync<ResultHandle>();
                }
                new LogHelper("EFMS_AutoRateReplicate_Called", "Settle :" + settlement.SettlementNo + " - Total of fees: " + surcharges.Count() + " Status: " + response?.Status);
                return response;
            }
            catch (Exception ex)
            {
                new LogHelper("EFMS_AutoRateReplicate_Error", "Settle :" + settlement.SettlementNo + " - Total of fees: " + surcharges.Count() + " Error: " + ex.Message);
                return new ResultHandle();
            }
        }

        private bool checkSettleSettingFlow()
        {
            if (settingflowRepository.Get(x => x.OfficeId == currentUser.OfficeID && x.Type == "AccountPayable").FirstOrDefault()?.ApprovalSettlement == true)
            {
                return true;
            }
            return false;
        }

        /// Check if payee not staff then not accept input cost > sell
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string CheckValidFeesOnShipment(CreateUpdateSettlementModel model)
        {
            var invalidShipment = string.Empty;
            if (!checkSettleSettingFlow())
            {
                return invalidShipment;
            }
            if (!string.IsNullOrEmpty(model.Settlement.Payee))
            {
                if (catPartnerRepo.Any(x => x.Id == model.Settlement.Payee && !x.PartnerGroup.ToLower().Contains("staff")))
                {
                    var jobIds = model.ShipmentCharge.Select(x => x.JobId).ToList();
                    var validJobNo = new List<string>();
                    var opsNoProfit = opsTransactionRepo.Get(x => jobIds.Any(z => z == x.JobNo) && x.NoProfit != true).Select(x => x.JobNo);
                    var serviceNoProfit = csTransactionRepo.Get(x => jobIds.Any(z => z == x.JobNo) && x.NoProfit != true).Select(x => x.JobNo);
                    validJobNo.AddRange(opsNoProfit);
                    validJobNo.AddRange(serviceNoProfit);

                    var listSipment = new List<string>();
                    var surcharges = csShipmentSurchargeRepo.Get(x => x.Type != AccountingConstants.TYPE_CHARGE_OBH && validJobNo.Any(z => z == x.JobNo));
                    foreach (var jobNo in validJobNo)
                    {
                        //[CR:10042023] shipment not checked no profit with no selling charges => not allow to settle
                        var sellingCharges = surcharges.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL && x.JobNo == jobNo);
                        if (sellingCharges.Count() <= 0)
                        {
                            listSipment.Add(jobNo);
                        }
                        else
                        {
                            // [CR:09/05/2022]: so sánh profit trên tổng của lô hàng
                            var shipment = surcharges.Where(x=>x.JobNo == jobNo);
                            var buyAmount = shipment.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_BUY).Sum(x => x.AmountVnd ?? 0);
                            var sellAmount = shipment.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL).Sum(x => x.AmountVnd ?? 0);
                            if (buyAmount > sellAmount)
                            {
                                listSipment.Add(shipment.FirstOrDefault().JobNo);
                            }
                            if (model.Settlement.SettlementType == "DIRECT" && buyAmount == sellAmount)
                            {
                                listSipment.Add(shipment.FirstOrDefault().JobNo);
                            }
                        }
                    }

                    // [CR:09/05/2022]: so sánh profit trên tổng của lô hàng
                    //var shipmentGrp = surcharges.GroupBy(x => x.JobNo);
                    //foreach (var shipment in shipmentGrp)
                    //{
                    //    var buyAmount = shipment.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_BUY).Sum(x => x.AmountVnd ?? 0);
                    //    var sellAmount = shipment.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL).Sum(x => x.AmountVnd ?? 0);
                    //    if (buyAmount > sellAmount)
                    //    {
                    //        listSipment.Add(shipment.FirstOrDefault().JobNo);
                    //    }
                    //}
                    if (listSipment.Count > 0)
                    {
                        listSipment = listSipment.Distinct().ToList();
                        invalidShipment = string.Join("\n", listSipment);
                    }
                }
            }

            return invalidShipment;
        }

        public List<ShipmentChargeSettlement> GetSurchargePagingSettlementPayment(string settlementNo, int page, int size)
        {
            var data = GetSurchargeDetailSettlement(settlementNo, null, null, null, page, size);
            return data;
        }

        /// <summary>
        /// Check prepaid shipment was confirmed with AR or not
        /// </summary>
        /// <param name="ShipmentCharges"></param>
        /// <returns></returns>
        public ResultHandle CheckConfirmPrepaidShipment(List<ShipmentChargeSettlement> ShipmentCharges)
        {
            ResultHandle result = new ResultHandle() { Status = true };
            var hblIds = ShipmentCharges.Select(x => x.Hblid).ToList();
            var office = sysOfficeRepo.First(x => x.Id == currentUser.OfficeID);
            if (office.OfficeType == AccountingConstants.OFFICE_TYPE_OUTSOURCE)
            {
                return result;
            }
            var opsTransaction = opsTransactionRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED && hblIds.Contains(x.Hblid));
            var transactionDetail = csTransactionDetailRepo.Get(x => hblIds.Contains(x.Id));
            var partners = catPartnerRepo.Get(x => x.Active == true);
            var contracts = contractRepository.Get(x => x.Active == true && x.ContractType == AccountingConstants.ARGEEMENT_TYPE_PREPAID && (x.IsExpired == false || x.IsExpired == null));
            string salemanBOD = sysUserRepo.Get(x => x.Username == AccountingConstants.ITL_BOD)?.FirstOrDefault()?.Id;
            var surcharges = csShipmentSurchargeRepo.Get(x => x.Type != AccountingConstants.TYPE_CHARGE_BUY && hblIds.Contains(x.Hblid));
            string messError = string.Empty;
            foreach (var hbl in hblIds)
            {
                var opsDetail = opsTransaction.Where(x => x.Hblid == hbl).FirstOrDefault();
                var transDetail = transactionDetail.Where(x => x.Id == hbl).FirstOrDefault();
                {
                    var existPrepaid = (from partner in partners
                                        join contract in contracts on partner.Id.ToLower() equals contract.PartnerId.ToLower()
                                        where partner.Id == (opsDetail == null ? transDetail.CustomerId : opsDetail.CustomerId) && contract.SaleManId == (opsDetail == null ? transDetail.SaleManId : opsDetail.SalemanId)
                                        && contract.SaleManId != salemanBOD
                                        select new
                                        {
                                            contract.SaleManId
                                        }).FirstOrDefault();
                    if (existPrepaid == null)
                    {
                        continue;
                    }
                    var chargesHbl = surcharges.Where(x => x.Hblid == hbl);
                    if (chargesHbl.Count() > 0)
                    {
                        bool hasIssuedDebit = chargesHbl.All(x => !string.IsNullOrEmpty(x.DebitNo) || !string.IsNullOrEmpty(x.Soano));
                        if (!hasIssuedDebit)
                        {
                            messError = stringLocalizer[AccountingLanguageSub.MSG_SETTLEMENT_HAD_SHIPMENT_PREPAID_NOT_ISSUED_DEBIT, chargesHbl.FirstOrDefault().JobNo];
                            return new ResultHandle() { Status = false, Message = messError };
                        }
                        var debitCodes = chargesHbl.Select(x => x.DebitNo).ToList();
                        var soaNos = chargesHbl.Select(x => x.Soano).ToList();
                        // Lấy data debit note và soa ngoại trừ phiếu đã sync có hđ <> prepaid trước đó
                        var debitNotes = acctCdnoteRepo.Get(x => debitCodes.Contains(x.Code) && x.Type != AccountingConstants.ACCOUNTANT_TYPE_CREDIT && (x.Status == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID || x.Status == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID));
                        var accSoas = acctSoaRepo.Get(x => soaNos.Contains(x.Soano) && x.Type != AccountingConstants.TYPE_SOA_CREDIT && !(x.SyncStatus == AccountingConstants.STATUS_SYNCED && x.Status == AccountingConstants.STATUS_SOA_NEW));
                        var hasConfirm = debitNotes.All(x => x.Status == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID) && accSoas.Any();
                        if (!hasConfirm)
                        {
                            messError = stringLocalizer[AccountingLanguageSub.MSG_SETTLEMENT_HAD_SHIPMENT_PREPAID_NOT_ISSUED_DEBIT, chargesHbl.FirstOrDefault().JobNo];
                            return new ResultHandle() { Status = false, Message = messError };
                        }
                    }
                }
            }
            return result;
        }
    }
}

