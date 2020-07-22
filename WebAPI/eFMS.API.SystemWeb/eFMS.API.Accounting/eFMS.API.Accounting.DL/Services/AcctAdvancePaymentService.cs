using AutoMapper;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Common;
using Microsoft.Extensions.Options;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.Models.ReportResults;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Common.Models;
using eFMS.API.Accounting.DL.Models.ExportResults;
using Newtonsoft.Json;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Linq.Expressions;

namespace eFMS.API.Accounting.DL.Services
{
    public class AcctAdvancePaymentService : RepositoryBase<AcctAdvancePayment, AcctAdvancePaymentModel>, IAcctAdvancePaymentService
    {
        private readonly ICurrentUser currentUser;
        private readonly IOptions<WebUrl> webUrl;
        private readonly IOptions<ApiUrl> apiUrl;
        readonly IContextBase<AcctAdvanceRequest> acctAdvanceRequestRepo;
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<OpsTransaction> opsTransactionRepo;
        readonly IContextBase<CsTransaction> csTransactionRepo;
        readonly IContextBase<CsTransactionDetail> csTransactionDetailRepo;
        readonly IContextBase<SysEmployee> sysEmployeeRepo;
        readonly IContextBase<AcctApproveAdvance> acctApproveAdvanceRepo;
        readonly IContextBase<CatDepartment> catDepartmentRepo;
        readonly IContextBase<OpsStageAssigned> opsStageAssignedRepo;
        readonly IContextBase<CsShipmentSurcharge> csShipmentSurchargeRepo;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo;
        readonly ICurrencyExchangeService currencyExchangeService;
        readonly IContextBase<AcctApproveSettlement> acctApproveSettlementRepo;
        readonly IUserBaseService userBaseService;
        private readonly IStringLocalizer stringLocalizer;
        private string typeApproval = "Advance";

        public AcctAdvancePaymentService(IContextBase<AcctAdvancePayment> repository,
            IMapper mapper,
            ICurrentUser user,
            IOptions<WebUrl> wUrl,
            IOptions<ApiUrl> aUrl,
            IContextBase<AcctAdvanceRequest> acctAdvanceRequest,
            IContextBase<SysUser> sysUser,
            IContextBase<OpsTransaction> opsTransaction,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CsTransactionDetail> csTransactionDetail,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<AcctApproveAdvance> acctApproveAdvance,
            IContextBase<CatDepartment> catDepartment,
            IContextBase<OpsStageAssigned> opsStageAssigned,
            IContextBase<CsShipmentSurcharge> csShipmentSurcharge,
            IContextBase<CatPartner> catPartner,
            IContextBase<AcctApproveSettlement> acctApproveSettlementRepository,
            IContextBase<CatCurrencyExchange> catCurrencyExchange,
            ICurrencyExchangeService currencyExchange, IStringLocalizer<LanguageSub> localizer,
            IUserBaseService userBase) : base(repository, mapper)
        {
            currentUser = user;
            webUrl = wUrl;
            apiUrl = aUrl;
            acctAdvanceRequestRepo = acctAdvanceRequest;
            sysUserRepo = sysUser;
            opsTransactionRepo = opsTransaction;
            csTransactionRepo = csTransaction;
            csTransactionDetailRepo = csTransactionDetail;
            sysEmployeeRepo = sysEmployee;
            acctApproveAdvanceRepo = acctApproveAdvance;
            catDepartmentRepo = catDepartment;
            opsStageAssignedRepo = opsStageAssigned;
            csShipmentSurchargeRepo = csShipmentSurcharge;
            catPartnerRepo = catPartner;
            acctApproveSettlementRepo = acctApproveSettlementRepository;
            catCurrencyExchangeRepo = catCurrencyExchange;
            currencyExchangeService = currencyExchange;
            userBaseService = userBase;
            stringLocalizer = localizer;
        }

        #region --- LIST & PAGING ---

        public List<AcctAdvancePaymentResult> Paging(AcctAdvancePaymentCriteria criteria, int page, int size, out int rowsCount)
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
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctAP);
            PermissionRange _permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            return _permissionRange;
        }

        private Expression<Func<AcctAdvancePayment, bool>> ExpressionQuery(AcctAdvancePaymentCriteria criteria)
        {
            Expression<Func<AcctAdvancePayment, bool>> query = q => true;
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

            if (criteria.AdvanceModifiedDateFrom.HasValue && criteria.AdvanceModifiedDateTo.HasValue)
            {
                //Convert DatetimeModified về date nếu DatetimeModified có value
                query = query.And(x =>
                                   x.DatetimeModified.Value.Date >= (criteria.AdvanceModifiedDateFrom.HasValue ? criteria.AdvanceModifiedDateFrom.Value.Date : criteria.AdvanceModifiedDateFrom)
                                && x.DatetimeModified.Value.Date <= (criteria.AdvanceModifiedDateTo.HasValue ? criteria.AdvanceModifiedDateTo.Value.Date : criteria.AdvanceModifiedDateTo));
            }

            if (!string.IsNullOrEmpty(criteria.PaymentMethod) && !criteria.PaymentMethod.Equals("All"))
            {
                query = query.And(x => x.PaymentMethod == criteria.PaymentMethod);
            }

            if (!string.IsNullOrEmpty(criteria.CurrencyID) && !criteria.CurrencyID.Equals("All"))
            {
                query = query.And(x => x.AdvanceCurrency == criteria.CurrencyID);
            }
            return query;
        }

        private IQueryable<AcctAdvancePayment> GetDataAdvancePayment()
        {
            var permissionRangeRequester = GetPermissionRangeOfRequester();
            var advancePayments = DataContext.Get();
            var advancePaymentAprs = acctApproveAdvanceRepo.Get(x => x.IsDeny == false);
            var data = from advancePayment in advancePayments
                       join advancePaymentApr in advancePaymentAprs on advancePayment.AdvanceNo equals advancePaymentApr.AdvanceNo into advancePaymentApr2
                       from advancePaymentApr in advancePaymentApr2.DefaultIfEmpty()
                       select new { advancePayment, advancePaymentApr };
            var result = data.Where(x =>
                (
                    permissionRangeRequester == PermissionRange.All ? x.advancePayment.Requester == currentUser.UserID : true
                    &&
                    permissionRangeRequester == PermissionRange.None ? false : true
                    &&
                    permissionRangeRequester == PermissionRange.Owner ? x.advancePayment.UserCreated == currentUser.UserID && x.advancePayment.Requester == currentUser.UserID : true
                    &&
                    permissionRangeRequester == PermissionRange.Group ? (x.advancePayment.GroupId == currentUser.GroupId
                                                                        && x.advancePayment.DepartmentId == currentUser.DepartmentId
                                                                        && x.advancePayment.OfficeId == currentUser.OfficeID
                                                                        && x.advancePayment.CompanyId == currentUser.CompanyID
                                                                        && x.advancePayment.Requester == currentUser.UserID) : true
                    &&
                    permissionRangeRequester == PermissionRange.Department ? (x.advancePayment.DepartmentId == currentUser.DepartmentId
                                                                              && x.advancePayment.OfficeId == currentUser.OfficeID
                                                                              && x.advancePayment.CompanyId == currentUser.CompanyID
                                                                              && x.advancePayment.Requester == currentUser.UserID) : true
                    &&
                    permissionRangeRequester == PermissionRange.Office ? (x.advancePayment.OfficeId == currentUser.OfficeID
                                                                          && x.advancePayment.CompanyId == currentUser.CompanyID
                                                                          && x.advancePayment.Requester == currentUser.UserID) : true
                    &&
                    permissionRangeRequester == PermissionRange.Company ? x.advancePayment.CompanyId == currentUser.CompanyID && x.advancePayment.Requester == currentUser.UserID : true
                )
                ||
                (x.advancePaymentApr != null && (x.advancePaymentApr.Leader == currentUser.UserID
                  || x.advancePaymentApr.LeaderApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(typeApproval, x.advancePaymentApr.Leader, x.advancePayment.GroupId, x.advancePayment.DepartmentId, x.advancePayment.OfficeId, x.advancePayment.CompanyId)
                )
                && x.advancePayment.GroupId == currentUser.GroupId
                && x.advancePayment.DepartmentId == currentUser.DepartmentId
                && x.advancePayment.OfficeId == currentUser.OfficeID
                && x.advancePayment.CompanyId == currentUser.CompanyID
                && x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                ) //LEADER AND DEPUTY OF LEADER
                ||
                (x.advancePaymentApr != null && (x.advancePaymentApr.Manager == currentUser.UserID
                  || x.advancePaymentApr.ManagerApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(typeApproval, x.advancePaymentApr.Manager, null, x.advancePayment.DepartmentId, x.advancePayment.OfficeId, x.advancePayment.CompanyId)
                  )
                && x.advancePayment.DepartmentId == currentUser.DepartmentId
                && x.advancePayment.OfficeId == currentUser.OfficeID
                && x.advancePayment.CompanyId == currentUser.CompanyID
                && x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                && (!string.IsNullOrEmpty(x.advancePaymentApr.Leader) ? x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL : true)
                ) //MANANER AND DEPUTY OF MANAGER
                ||
                (x.advancePaymentApr != null && (x.advancePaymentApr.Accountant == currentUser.UserID
                  || x.advancePaymentApr.AccountantApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(typeApproval, x.advancePaymentApr.Accountant, null, null, x.advancePayment.OfficeId, x.advancePayment.CompanyId)
                  )
                && x.advancePayment.OfficeId == currentUser.OfficeID
                && x.advancePayment.CompanyId == currentUser.CompanyID
                && x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                //&& (!string.IsNullOrEmpty(x.advancePaymentApr.Leader) ? x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL : true)
                //&& (!string.IsNullOrEmpty(x.advancePaymentApr.Manager) ? x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED : true)
                ) // ACCOUTANT AND DEPUTY OF ACCOUNTANT
                ||
                (x.advancePaymentApr != null && (x.advancePaymentApr.Buhead == currentUser.UserID
                  || x.advancePaymentApr.BuheadApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(typeApproval, x.advancePaymentApr.Buhead ?? null, null, null, x.advancePayment.OfficeId, x.advancePayment.CompanyId)
                  )
                && x.advancePayment.OfficeId == currentUser.OfficeID
                && x.advancePayment.CompanyId == currentUser.CompanyID
                && x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                && x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                //&& (!string.IsNullOrEmpty(x.advancePaymentApr.Leader) ? x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL : true)
                //&& (!string.IsNullOrEmpty(x.advancePaymentApr.Manager) ? x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED : true)
                //&& (!string.IsNullOrEmpty(x.advancePaymentApr.Accountant) ? x.advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED : true)
                ) //BOD AND DEPUTY OF BOD                
            ).Select(s => s.advancePayment);
            return result;
        }

        private IQueryable<AcctAdvancePayment> QueryWithAdvanceRequest(IQueryable<AcctAdvancePayment> advancePayments, AcctAdvancePaymentCriteria criteria)
        {
            IQueryable<AcctAdvanceRequest> advanceRequests = null;
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                advanceRequests = acctAdvanceRequestRepo.Get(x =>
                                        criteria.ReferenceNos.Contains(x.AdvanceNo)
                                     || criteria.ReferenceNos.Contains(x.Hbl)
                                     || criteria.ReferenceNos.Contains(x.Mbl)
                                     || criteria.ReferenceNos.Contains(x.CustomNo)
                                     || criteria.ReferenceNos.Contains(x.JobId)
                                     );

            }

            if (!string.IsNullOrEmpty(criteria.StatusPayment) && !criteria.StatusPayment.Equals("All"))
            {
                advanceRequests = acctAdvanceRequestRepo.Get(x => x.StatusPayment == criteria.StatusPayment);
            }

            if (advanceRequests != null)
            {
                advancePayments = advancePayments.Join(advanceRequests, u => u.AdvanceNo, j => j.AdvanceNo, (u, j) => u);
            }

            return advancePayments;
        }

        private IQueryable<AcctAdvancePaymentResult> GetDatas(AcctAdvancePaymentCriteria criteria)
        {
            var queryAdvancePayment = ExpressionQuery(criteria);
            var dataAdvancePayments = GetDataAdvancePayment();
            if (dataAdvancePayments == null) return null;
            var advancePayments = dataAdvancePayments.Where(queryAdvancePayment);
            advancePayments = QueryWithAdvanceRequest(advancePayments, criteria);

            if (advancePayments == null) return null;
            var requestAdvances = acctAdvanceRequestRepo.Get();
            var users = sysUserRepo.Get();

            var data = from advancePayment in advancePayments
                       join user in users on advancePayment.Requester equals user.Id into user2
                       from user in user2.DefaultIfEmpty()
                       join requestAdvance in requestAdvances on advancePayment.AdvanceNo equals requestAdvance.AdvanceNo into requestAdvances2
                       from requestAdvance in requestAdvances2.DefaultIfEmpty()
                       select new AcctAdvancePaymentResult
                       {
                           Id = advancePayment.Id,
                           AdvanceNo = advancePayment.AdvanceNo,
                           AdvanceNote = advancePayment.AdvanceNote,
                           AdvanceCurrency = advancePayment.AdvanceCurrency,
                           Requester = advancePayment.Requester,
                           RequesterName = user.Username,
                           RequestDate = advancePayment.RequestDate,
                           DeadlinePayment = advancePayment.DeadlinePayment,
                           UserCreated = advancePayment.UserCreated,
                           DatetimeCreated = advancePayment.DatetimeCreated,
                           UserModified = advancePayment.UserModified,
                           DatetimeModified = advancePayment.DatetimeModified,
                           StatusApproval = advancePayment.StatusApproval,
                           PaymentMethod = advancePayment.PaymentMethod,
                           Amount = requestAdvance.Amount,
                           VoucherNo = advancePayment.VoucherNo,
                           VoucherDate = advancePayment.VoucherDate
                       };

            //Gom nhóm và Sắp xếp giảm dần theo Advance DatetimeModified
            data = data.GroupBy(x => new
            {
                x.Id,
                x.AdvanceNo,
                x.AdvanceNote,
                x.AdvanceCurrency,
                x.Requester,
                x.RequesterName,
                x.RequestDate,
                x.DeadlinePayment,
                x.UserCreated,
                x.DatetimeCreated,
                x.UserModified,
                x.DatetimeModified,
                x.StatusApproval,
                x.PaymentMethod,
                x.VoucherNo,
                x.VoucherDate
            }).Select(s => new AcctAdvancePaymentResult
            {
                Id = s.Key.Id,
                AdvanceNo = s.Key.AdvanceNo,
                AdvanceNote = s.Key.AdvanceNote,
                AdvanceCurrency = s.Key.AdvanceCurrency,
                Requester = s.Key.Requester,
                RequesterName = s.Key.RequesterName,
                RequestDate = s.Key.RequestDate,
                DeadlinePayment = s.Key.DeadlinePayment,
                UserCreated = s.Key.UserCreated,
                DatetimeCreated = s.Key.DatetimeCreated,
                UserModified = s.Key.UserModified,
                DatetimeModified = s.Key.DatetimeModified,
                StatusApproval = s.Key.StatusApproval,
                VoucherNo = s.Key.VoucherNo,
                VoucherDate = s.Key.VoucherDate,
                AdvanceStatusPayment = GetAdvanceStatusPayment(s.Key.AdvanceNo),
                PaymentMethod = s.Key.PaymentMethod,
                PaymentMethodName = Common.CustomData.PaymentMethod.Where(x => x.Value == s.Key.PaymentMethod).Select(x => x.DisplayName).FirstOrDefault(),
                Amount = s.Sum(su => su.Amount),
                StatusApprovalName = Common.CustomData.StatusApproveAdvance.Where(x => x.Value == s.Key.StatusApproval).Select(x => x.DisplayName).FirstOrDefault()
            });
            //Sort Array sẽ nhanh hơn
            data = data.ToArray().OrderByDescending(orb => orb.DatetimeModified).AsQueryable();
            return data;
        }

        public string GetAdvanceStatusPayment(string advanceNo)
        {
            var requestTmp = acctAdvanceRequestRepo.Get();
            var result = requestTmp.Where(x => x.StatusPayment == AccountingConstants.STATUS_PAYMENT_NOTSETTLED && x.AdvanceNo == advanceNo).Select(s => s.Id).Count() == requestTmp.Where(x => x.AdvanceNo == advanceNo).Select(s => s.Id).Count()
                            ?
                                AccountingConstants.STATUS_PAYMENT_NOTSETTLED
                            :
                                requestTmp.Where(x => x.StatusPayment == AccountingConstants.STATUS_PAYMENT_SETTLED && x.AdvanceNo == advanceNo).Select(s => s.Id).Count() == requestTmp.Where(x => x.AdvanceNo == advanceNo).Select(s => s.Id).Count() ?
                                    AccountingConstants.STATUS_PAYMENT_SETTLED
                                :
                                    AccountingConstants.STATUS_PAYMENT_PARTIALSETTLEMENT;

            return result;
        }

        #endregion --- LIST & PAGING ---

        public List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceNo(string advanceNo)
        {
            //Sum(Amount) theo lô hàng (JobId, HBL)
            var list = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo)
                .GroupBy(g => new { g.JobId, g.Hbl, g.CustomNo })
                .Select(se => new AcctAdvanceRequest
                {
                    JobId = se.First().JobId,
                    Hbl = se.First().Hbl,
                    CustomNo = se.First().CustomNo,
                    Amount = se.Sum(s => s.Amount),
                    RequestCurrency = se.First().RequestCurrency,
                    StatusPayment = se.First().StatusPayment
                }).ToList();
            var datamap = mapper.Map<List<AcctAdvanceRequestModel>>(list);
            return datamap;
        }

        public List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceNoList(string[] advanceNoList)
        {
            var list = acctAdvanceRequestRepo.Where(x => advanceNoList.Contains(x.AdvanceNo))
                .GroupBy(g => new { g.JobId, g.Hbl, g.CustomNo, g.Mbl, g.AdvanceNo })
                .Select(se => new AcctAdvanceRequest
                {
                    JobId = se.First().JobId,
                    Hbl = se.First().Hbl,
                    CustomNo = se.First().CustomNo,
                    Amount = se.Sum(s => s.Amount),
                    RequestCurrency = se.First().RequestCurrency,
                    StatusPayment = se.First().StatusPayment,
                    AdvanceNo = se.FirstOrDefault().AdvanceNo,
                    Mbl = se.First().Mbl,
                    Description = se.FirstOrDefault().Description,
                }).ToList();
            var datamap = mapper.Map<List<AcctAdvanceRequestModel>>(list);
            var surcharge = csShipmentSurchargeRepo.Get(); // lấy ds surcharge đã có advanceNo.

            foreach (var item in datamap)
            {
                string requesterID = DataContext.First(x => x.AdvanceNo == item.AdvanceNo).Requester;
                if (!string.IsNullOrEmpty(requesterID))
                {
                    string employeeID = sysUserRepo.Get(x => x.Id == requesterID).FirstOrDefault()?.EmployeeId;
                    item.Requester = sysEmployeeRepo.Get(x => x.Id == employeeID).FirstOrDefault()?.EmployeeNameVn;
                }

                item.RequestDate = DataContext.First(x => x.AdvanceNo == item.AdvanceNo).RequestDate;
                item.ApproveDate = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == item.AdvanceNo).FirstOrDefault()?.BuheadAprDate;

                var surchargeAdvanceNo = surcharge.Where(x => x.AdvanceNo == item.AdvanceNo)?.FirstOrDefault();
                if (surchargeAdvanceNo != null && surchargeAdvanceNo.SettlementCode != null)
                {
                    string _settleCode = surchargeAdvanceNo.SettlementCode;
                    var data = acctApproveSettlementRepo.Get(x => x.SettlementNo == _settleCode)?.FirstOrDefault();
                    if (data != null)
                    {
                        item.SettleDate = data.RequesterAprDate;
                    }
                    else
                    {
                        item.SettleDate = null;
                    }
                }
                else
                {
                    item.SettleDate = null;
                }

            }
            return datamap;
        }

        public List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceId(Guid advanceId)
        {
            //Sum(Amount) theo lô hàng (JobId, HBL)
            var list = acctAdvanceRequestRepo.Get(x => x.Id == advanceId)
                .GroupBy(g => new { g.JobId, g.Hbl, g.CustomNo })
                .Select(se => new AcctAdvanceRequest
                {
                    JobId = se.First().JobId,
                    Hbl = se.First().Hbl,
                    CustomNo = se.First().CustomNo,
                    Amount = se.Sum(s => s.Amount),
                    RequestCurrency = se.First().RequestCurrency,
                    StatusPayment = se.First().StatusPayment
                }).ToList();
            var datamap = mapper.Map<List<AcctAdvanceRequestModel>>(list);
            return datamap;
        }

        #region --- DETAIL ---
        public AcctAdvancePaymentModel GetAdvancePaymentByAdvanceNo(string advanceNo)
        {
            var advanceModel = new AcctAdvancePaymentModel();

            //Lấy ra Advance Payment dựa vào Advance No
            var advance = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            //Không tìm thấy Advance Payment thì trả về null
            if (advance == null) return null;

            //Lấy ra danh sách Advance Request dựa vào Advance No và sắp xếp giảm dần theo DatetimeModified Advance Request
            var request = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo).OrderByDescending(x => x.DatetimeModified).ToList();
            //Không tìm thấy Advance Request thì trả về null
            if (request == null) return null;

            //Mapper AcctAdvancePayment thành AcctAdvancePaymentModel
            advanceModel = mapper.Map<AcctAdvancePaymentModel>(advance);
            //Mapper List<AcctAdvanceRequest> thành List<AcctAdvanceRequestModel>
            advanceModel.AdvanceRequests = mapper.Map<List<AcctAdvanceRequestModel>>(request);
            advanceModel.NumberOfRequests = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo).Select(s => s.Id).Count();
            advanceModel.UserNameCreated = sysUserRepo.Get(x => x.Id == advance.UserCreated).FirstOrDefault()?.Username;
            advanceModel.UserNameModified = sysUserRepo.Get(x => x.Id == advance.UserModified).FirstOrDefault()?.Username;

            var advanceApprove = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeny == false).FirstOrDefault();

            advanceModel.IsRequester = (currentUser.UserID == advance.Requester
                && currentUser.GroupId == advance.GroupId
                && currentUser.DepartmentId == advance.DepartmentId
                && currentUser.OfficeID == advance.OfficeId
                && currentUser.CompanyID == advance.CompanyId) ? true : false;
            advanceModel.IsManager = CheckUserIsManager(currentUser, advance, advanceApprove);
            advanceModel.IsApproved = CheckUserIsApproved(currentUser, advance, advanceApprove);
            advanceModel.IsShowBtnDeny = CheckIsShowBtnDeny(currentUser, advance, advanceApprove);
            return advanceModel;
        }

        public AcctAdvancePaymentModel GetAdvancePaymentByAdvanceId(Guid advanceId)
        {
            var advanceModel = new AcctAdvancePaymentModel();

            //Lấy ra Advance Payment dựa vào Advance Id
            var advance = DataContext.Get(x => x.Id == advanceId).FirstOrDefault();
            //Không tìm thấy Advance Payment thì trả về null
            if (advance == null) return null;

            //Lấy ra danh sách Advance Request dựa vào Advance No và sắp xếp giảm dần theo DatetimeModified Advance Request
            var request = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advance.AdvanceNo).OrderByDescending(x => x.DatetimeModified).ToList();
            //Không tìm thấy Advance Request thì trả về null
            if (request == null) return null;

            //Mapper AcctAdvancePayment thành AcctAdvancePaymentModel
            advanceModel = mapper.Map<AcctAdvancePaymentModel>(advance);
            //Mapper List<AcctAdvanceRequest> thành List<AcctAdvanceRequestModel>
            advanceModel.AdvanceRequests = mapper.Map<List<AcctAdvanceRequestModel>>(request);
            advanceModel.NumberOfRequests = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo).Select(s => s.Id).Count();
            advanceModel.UserNameCreated = sysUserRepo.Get(x => x.Id == advance.UserCreated).FirstOrDefault()?.Username;
            advanceModel.UserNameModified = sysUserRepo.Get(x => x.Id == advance.UserModified).FirstOrDefault()?.Username;
            advanceModel.RequesterName = sysUserRepo.Get(x => x.Id == advance.Requester).FirstOrDefault()?.Username;

            var advanceApprove = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeny == false).FirstOrDefault();

            advanceModel.IsRequester = (currentUser.UserID == advance.Requester
                && currentUser.GroupId == advance.GroupId
                && currentUser.DepartmentId == advance.DepartmentId
                && currentUser.OfficeID == advance.OfficeId
                && currentUser.CompanyID == advance.CompanyId) ? true : false;
            advanceModel.IsManager = CheckUserIsManager(currentUser, advance, advanceApprove);
            advanceModel.IsApproved = CheckUserIsApproved(currentUser, advance, advanceApprove);
            advanceModel.IsShowBtnDeny = CheckIsShowBtnDeny(currentUser, advance, advanceApprove);
            return advanceModel;
        }
        #endregion --- DETAIL ---

        /// <summary>
        /// Get shipments (JobId, HBL, MBL) from shipment documentation and shipment operation
        /// </summary>
        /// <returns></returns>
        public List<Shipments> GetShipments()
        {
            var userCurrent = currentUser.UserID;

            //Start change request Modified 14/10/2019 by Andy.Hoa
            //Get list shipment operation theo user current
            var shipmentsOperation = from ops in opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED)
                                     join osa in opsStageAssignedRepo.Get() on ops.Id equals osa.JobId
                                     where osa.MainPersonInCharge == userCurrent
                                     select new Shipments
                                     {
                                         JobId = ops.JobNo,
                                         HBL = ops.Hwbno,
                                         MBL = ops.Mblno
                                     };
            shipmentsOperation = shipmentsOperation.GroupBy(x => new { x.JobId, x.HBL, x.MBL }).Select(s => new Shipments
            {
                JobId = s.Key.JobId,
                HBL = s.Key.HBL,
                MBL = s.Key.MBL
            });
            shipmentsOperation = shipmentsOperation.Distinct();
            //End change request
            var transactions = from cst in csTransactionRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED)
                               join osa in opsStageAssignedRepo.Get() on cst.Id equals osa.JobId
                               where osa.MainPersonInCharge == userCurrent
                               select cst;
            var shipmentsDocumention = (from t in transactions
                                        join td in csTransactionDetailRepo.Get() on t.Id equals td.JobId
                                        select new Shipments
                                        {
                                            JobId = t.JobNo,
                                            HBL = td.Hwbno,
                                            MBL = t.Mawb,
                                        });
            shipmentsDocumention = shipmentsDocumention.Distinct();
            var shipments = shipmentsOperation.Union(shipmentsDocumention).ToList();
            return shipments;
        }

        #region --- CRUD ---
        private string CreateAdvanceNo()
        {
            string year = (DateTime.Now.Year.ToString()).Substring(2, 2);
            string month = DateTime.Now.ToString("DDMMYYYY").Substring(2, 2);
            string prefix = "AD" + year + month + "/";
            string stt;

            //Lấy ra dòng cuối cùng của table acctAdvancePayment
            var rowlast = DataContext.Get().OrderByDescending(x => x.AdvanceNo).FirstOrDefault();

            if (rowlast == null)
            {
                stt = "0001";
            }
            else
            {
                var advanceCurrent = rowlast.AdvanceNo;
                var prefixCurrent = advanceCurrent.Substring(2, 4);
                //Reset về 1 khi qua tháng tiếp theo
                if (prefixCurrent != (year + month))
                {
                    stt = "0001";
                }
                else
                {
                    stt = (Convert.ToInt32(advanceCurrent.Substring(7, 4)) + 1).ToString();
                    stt = stt.PadLeft(4, '0');
                }
            }

            return prefix + stt;
        }

        public HandleState AddAdvancePayment(AcctAdvancePaymentModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctAP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var userCurrent = currentUser.UserID;
                var advance = mapper.Map<AcctAdvancePayment>(model);
                advance.Id = model.Id = Guid.NewGuid();
                advance.AdvanceNo = model.AdvanceNo = CreateAdvanceNo();
                advance.StatusApproval = model.StatusApproval = string.IsNullOrEmpty(model.StatusApproval) ? AccountingConstants.STATUS_APPROVAL_NEW : model.StatusApproval;

                advance.DatetimeCreated = advance.DatetimeModified = DateTime.Now;
                advance.UserCreated = advance.UserModified = userCurrent;
                advance.GroupId = currentUser.GroupId;
                advance.DepartmentId = currentUser.DepartmentId;
                advance.OfficeId = currentUser.OfficeID;
                advance.CompanyId = currentUser.CompanyID;

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Add(advance);
                        if (hs.Success)
                        {
                            var request = mapper.Map<List<AcctAdvanceRequest>>(model.AdvanceRequests);
                            foreach (var item in request)
                            {
                                item.Id = Guid.NewGuid();
                                item.AdvanceNo = advance.AdvanceNo;
                                item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                                item.UserCreated = item.UserModified = userCurrent;
                                item.StatusPayment = AccountingConstants.STATUS_PAYMENT_NOTSETTLED;
                                var hsAddRequest = acctAdvanceRequestRepo.Add(item);
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
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        /// <summary>
        /// Kiểm tra lô hàng (JobId, HBL, MBL) đã được add trong advance payment nào hay chưa?
        /// </summary>
        /// <param name="JobId"></param>
        /// <param name="HBL"></param>
        /// <param name="MBL"></param>
        /// <returns>true: đã tồn tại; false: chưa tồn tại</returns>
        public bool CheckShipmentsExistInAdvancePayment(ShipmentAdvancePaymentCriteria criteria)
        {
            try
            {
                var result = false;
                //Check trường hợp Add new advance payment
                if (string.IsNullOrEmpty(criteria.AdvanceNo))
                {
                    result = acctAdvanceRequestRepo.Get().Any(x =>
                      x.JobId == criteria.JobId
                   && x.Hbl == criteria.HBL
                   && x.Mbl == criteria.MBL);
                }
                else //Check trường hợp Update advance payment
                {
                    result = acctAdvanceRequestRepo.Get().Any(x =>
                      x.JobId == criteria.JobId
                   && x.Hbl == criteria.HBL
                   && x.Mbl == criteria.MBL
                   && x.AdvanceNo != criteria.AdvanceNo);
                }
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CheckDeletePermissionByAdvanceNo(string advanceNo)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctAP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.AdvanceNo == advanceNo)?.FirstOrDefault();
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

        public bool CheckDeletePermissionByAdvanceId(Guid advanceId)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctAP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.Id == advanceId)?.FirstOrDefault();
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

        public HandleState DeleteAdvancePayment(string advanceNo)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctAP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var requests = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo);
                        //Xóa các Advance Request có AdvanceNo = AdvanceNo truyền vào
                        if (requests != null)
                        {
                            foreach (var item in requests)
                            {
                                var advRequestDelete = acctAdvanceRequestRepo.Delete(x => x.Id == item.Id, false);
                            }
                        }
                        //Xóa các Approve Advance có AdvanceNo = AdvanceNo truyền vào
                        var approveAdvance = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advanceNo);
                        if (approveAdvance != null)
                        {
                            foreach(var approve in approveAdvance)
                            {
                                var approveAdvanceDelete = acctApproveAdvanceRepo.Delete(x => x.Id == approve.Id, false);
                            }
                        }

                        var advance = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault();
                        if (advance == null) return new HandleState("Not found Advance Payment No: " + advanceNo);
                        if (advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                            && advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED)
                        {
                            return new HandleState("Not allow delete. Advance Payment are awaiting approval.");
                        }
                        var hs = DataContext.Delete(x => x.Id == advance.Id, false);
                        if (hs.Success)
                        {
                            acctAdvanceRequestRepo.SubmitChanges();
                            acctApproveAdvanceRepo.SubmitChanges();
                            DataContext.SubmitChanges();
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

        public bool CheckDetailPermissionByAdvanceNo(string advanceNo)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctAP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.AdvanceNo == advanceNo)?.FirstOrDefault();
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

        public bool CheckDetailPermissionByAdvanceId(Guid advanceId)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctAP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.Id == advanceId)?.FirstOrDefault();
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

        public bool CheckUpdatePermissionByAdvanceId(Guid advanceId)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctAP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.Id == advanceId)?.FirstOrDefault();
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

        public HandleState UpdateAdvancePayment(AcctAdvancePaymentModel model)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctAP);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None) return new HandleState(403, "");

            try
            {
                var userCurrent = currentUser.UserID;
                var today = DateTime.Now;
                var advance = mapper.Map<AcctAdvancePayment>(model);

                var advanceCurrent = DataContext.Get(x => x.Id == advance.Id).FirstOrDefault();
                if (advanceCurrent == null) return new HandleState("Not found advance payment");

                //Get Advance current from Database
                if (!advanceCurrent.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_NEW) && !advanceCurrent.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED))
                {
                    return new HandleState("Only allowed to edit the advance payment status is New or Deny");
                }

                advance.DatetimeCreated = advanceCurrent.DatetimeCreated;
                advance.UserCreated = advanceCurrent.UserCreated;

                advance.DatetimeModified = today;
                advance.UserModified = userCurrent;
                advance.GroupId = advanceCurrent.GroupId;
                advance.DepartmentId = advanceCurrent.DepartmentId;
                advance.OfficeId = advanceCurrent.OfficeId;
                advance.CompanyId = advanceCurrent.CompanyId;

                //Cập nhật lại Status Approval là NEW nếu Status Approval hiện tại là DENIED
                if (model.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED) && advanceCurrent.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED))
                {
                    advance.StatusApproval = AccountingConstants.STATUS_APPROVAL_NEW;
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Update(advance, x => x.Id == advance.Id);

                        if (hs.Success)
                        {
                            var request = mapper.Map<List<AcctAdvanceRequest>>(model.AdvanceRequests);
                            //Lấy ra các Request cũ cần update
                            var requestUpdate = request.Where(x => x.Id != Guid.Empty).ToList();

                            //Lấy ra các Request có cùng AdvanceNo và không tồn tại trong requestUpdate
                            var requestNeedRemove = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && !requestUpdate.Contains(x)).ToList();
                            //Xóa các requestNeedRemove
                            foreach (var item in requestNeedRemove)
                            {
                                var hsRequestNeedRemove = acctAdvanceRequestRepo.Delete(x => x.Id == item.Id);
                            }

                            //Lấy ra những request mới (có Id là Empty)
                            var requestNew = request.Where(x => x.Id == Guid.Empty).ToList();
                            if (requestNew != null && requestNew.Count > 0)
                            {
                                foreach (var item in requestNew)
                                {
                                    item.Id = Guid.NewGuid();
                                    item.AdvanceNo = advance.AdvanceNo;
                                    item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                                    item.UserCreated = item.UserModified = userCurrent;
                                    item.StatusPayment = AccountingConstants.STATUS_PAYMENT_NOTSETTLED;
                                    var hsRequestNew = acctAdvanceRequestRepo.Add(item);
                                }
                            }

                            if (requestUpdate != null && requestUpdate.Count > 0)
                            {
                                //Cập nhật những request cũ cần update
                                foreach (var item in requestUpdate)
                                {
                                    item.DatetimeModified = today;
                                    item.UserModified = userCurrent;
                                    var hsRequestUpdate = acctAdvanceRequestRepo.Update(item, x => x.Id == item.Id);
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
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        #endregion --- CRUD ---

        #region PREVIEW ADVANCE PAYMENT
        public Crystal Preview(Guid advanceId)
        {
            Crystal result = null;
            string strJobId = string.Empty;
            string strHbl = string.Empty;
            string strCustomNo = string.Empty;
            int contQty = 0;
            decimal nw = 0;
            decimal gw = 0;
            int psc = 0;
            decimal cbm = 0;
            string shipper = string.Empty;
            string consignee = string.Empty;
            string customer = string.Empty;

            var advance = GetAdvancePaymentByAdvanceId(advanceId);

            if (advance == null) return null;

            if (advance.AdvanceRequests.Count > 0)
            {
                var groupJobByHbl = advance.AdvanceRequests
                .GroupBy(g => new { g.Hbl })
                .Select(s => new AcctAdvanceRequestModel
                {
                    JobId = s.First().JobId,
                    Hbl = s.Key.Hbl,
                    Mbl = s.First().Mbl,
                    CustomNo = s.First().CustomNo
                });

                foreach (var request in groupJobByHbl)
                {
                    //Lấy ra NW, CBM, PSC, Container Qty
                    var ops = opsTransactionRepo.Get(x => x.JobNo == request.JobId).FirstOrDefault();
                    if (ops != null)
                    {
                        contQty += ops.SumContainers.HasValue ? ops.SumContainers.Value : 0;
                        nw += ops.SumNetWeight ?? 0;
                        psc += ops.SumPackages ?? 0;
                        cbm += ops.SumCbm ?? 0;
                        gw += ops.SumGrossWeight ?? 0;
                        string customerNameAbbr = catPartnerRepo.Get(x => x.Id == ops.CustomerId).FirstOrDefault()?.ShortName;
                        customer += !string.IsNullOrEmpty(customerNameAbbr) && !customer.Contains(customerNameAbbr) ? customerNameAbbr + ", " : string.Empty;
                    }
                    else
                    {
                        var job = csTransactionRepo.Get(x => x.JobNo == request.JobId).FirstOrDefault();
                        nw += job.NetWeight ?? 0;
                        psc += job.PackageQty ?? 0;
                        cbm += job.Cbm ?? 0;
                        gw += job.GrossWeight ?? 0;

                        var house = csTransactionDetailRepo.Get(x => x.Hwbno == request.Hbl).FirstOrDefault();
                        string customerNameAbbr = catPartnerRepo.Get(x => x.Id == house.CustomerId).FirstOrDefault()?.ShortName;
                        customer += !string.IsNullOrEmpty(customerNameAbbr) && !customer.Contains(customerNameAbbr) ? customerNameAbbr + ", " : string.Empty;
                        string shipperNameAbbr = catPartnerRepo.Get(x => x.Id == house.ShipperId).FirstOrDefault()?.ShortName;
                        shipper += !string.IsNullOrEmpty(shipperNameAbbr) && !shipper.Contains(shipperNameAbbr) ? shipperNameAbbr + ", " : string.Empty;
                        string consigneeNameAbbr = catPartnerRepo.Get(x => x.Id == house.ConsigneeId).FirstOrDefault()?.ShortName;
                        consignee += !string.IsNullOrEmpty(consigneeNameAbbr) && !consignee.Contains(consigneeNameAbbr) ? consigneeNameAbbr + ", " : string.Empty;
                    }
                }

                customer = !string.IsNullOrEmpty(customer) ? customer.Substring(0, customer.Length - 2) : string.Empty;
                shipper = !string.IsNullOrEmpty(shipper) ? shipper.Substring(0, shipper.Length - 2) : string.Empty;
                consignee = !string.IsNullOrEmpty(consignee) ? consignee.Substring(0, consignee.Length - 2) : string.Empty;

                //Lấy ra chuỗi JobId
                strJobId = string.Join(", ", groupJobByHbl.Where(x => !string.IsNullOrEmpty(x.JobId)).Select(s => s.JobId));
                //Lấy ra chuỗi HBL
                strHbl = string.Join(", ", groupJobByHbl.Where(x => !string.IsNullOrEmpty(x.Hbl)).Select(s => s.Hbl));
                //Lấy ra chuỗi CustomNo
                strCustomNo = string.Join(", ", groupJobByHbl.Where(x => !string.IsNullOrEmpty(x.CustomNo)).Select(s => s.CustomNo));
            }

            //Lấy ra tên requester
            var employeeId = sysUserRepo.Get(x => x.Id == advance.Requester).Select(x => x.EmployeeId).FirstOrDefault();
            var requesterName = sysEmployeeRepo.Get(x => x.Id == employeeId).Select(x => x.EmployeeNameVn).FirstOrDefault();

            string managerName = string.Empty;
            string accountantName = string.Empty;
            var approveAdvance = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeny == false).FirstOrDefault();
            if (approveAdvance != null)
            {
                managerName = string.IsNullOrEmpty(approveAdvance.Manager) ? null : userBaseService.GetEmployeeByUserId(approveAdvance.Manager)?.EmployeeNameVn;
                accountantName = string.IsNullOrEmpty(approveAdvance.Accountant) ? null : userBaseService.GetEmployeeByUserId(approveAdvance.Accountant)?.EmployeeNameVn;
            }

            var deptRequester = catDepartmentRepo.Get(x => x.Id == advance.DepartmentId).FirstOrDefault();
            var acctAdvance = new AdvancePaymentRequestReport
            {
                AdvID = advance.AdvanceNo,
                RefNo = "N/A",
                AdvDate = advance.RequestDate.Value.Date,
                AdvTo = "N/A",
                AdvContactID = "N/A",
                AdvContact = requesterName,//cần lấy ra username
                AdvAddress = deptRequester?.DeptNameAbbr ?? string.Empty, //Department của Requester
                AdvValue = advance.AdvanceRequests.Select(s => s.Amount).Sum(),
                AdvCurrency = advance.AdvanceCurrency,
                AdvCondition = advance.AdvanceNote,
                AdvRef = strJobId,
                AdvHBL = strHbl,
                AdvPaymentDate = null,
                AdvPaymentNote = "N/A",
                AdvDpManagerID = "N/A",
                AdvDpManagerStickDeny = null,
                AdvDpManagerStickApp = null,
                AdvDpManagerName = managerName,
                AdvDpSignDate = null,
                AdvAcsDpManagerID = "N/A",
                AdvAcsDpManagerStickDeny = null,
                AdvAcsDpManagerStickApp = null,
                AdvAcsDpManagerName = accountantName,
                AdvAcsSignDate = null,
                AdvBODID = "N/A",
                AdvBODStickDeny = null,
                AdvBODStickApp = null,
                AdvBODName = "N/A",
                AdvBODSignDate = null,
                AdvCashier = "N/A",
                AdvCashierName = "N/A",
                CashedDate = null,
                Saved = null,
                SettleNo = "N/A",
                PaidDate = null,
                AmountSettle = 0,
                SettleCurrency = "N/A",
                ClearStatus = null,
                Status = "N/A",
                AcsApproval = null,
                Description = "N/A",
                JobNo = "N/A",
                MAWB = "N/A",
                Amount = 0,
                Currency = "N/A",
                ExchangeRate = 0,
                TotalAmount = 0,
                PaymentDate = advance.DeadlinePayment.Value.Date,
                InvoiceNo = "N/A",
                CustomID = strCustomNo,
                HBLNO = "N/A",
                Norm = null,
                Validfee = null,
                Others = null,
                CSApp = null,
                CSDecline = null,
                CSUser = "N/A",
                CSAppDate = null,
                Customer = customer,
                Shipper = shipper,
                Consignee = consignee,
                ContQty = contQty.ToString(),
                Noofpieces = psc,
                UnitPieaces = "N/A",
                GW = gw,
                NW = nw,
                CBM = cbm,
                ServiceType = "N/A",
                AdvCSName = string.Empty,
                AdvCSSignDate = null,
                AdvCSStickApp = null,
                AdvCSStickDeny = null,
                TotalNorm = advance.AdvanceRequests.Where(x => x.AdvanceType == AccountingConstants.ADVANCE_TYPE_NORM).Select(s => s.Amount).Sum(),
                TotalInvoice = advance.AdvanceRequests.Where(x => x.AdvanceType == AccountingConstants.ADVANCE_TYPE_INVOICE).Select(s => s.Amount).Sum(),
                TotalOrther = advance.AdvanceRequests.Where(x => x.AdvanceType == AccountingConstants.ADVANCE_TYPE_OTHER).Select(s => s.Amount).Sum()
            };

            acctAdvance.TotalNorm = acctAdvance.TotalNorm != 0 ? acctAdvance.TotalNorm : null;
            acctAdvance.TotalInvoice = acctAdvance.TotalInvoice != 0 ? acctAdvance.TotalInvoice : null;
            acctAdvance.TotalOrther = acctAdvance.TotalOrther != 0 ? acctAdvance.TotalOrther : null;

            var listAdvance = new List<AdvancePaymentRequestReport>
            {
                acctAdvance
            };

            //Chuyển tiền Amount thành chữ
            decimal _amount = acctAdvance.AdvValue.HasValue ? acctAdvance.AdvValue.Value : 0;
            //decimal _amount2 = 3992.123M;

            var _currency = advance.AdvanceCurrency == AccountingConstants.CURRENCY_LOCAL ?
                       (_amount % 1 > 0 ? "đồng lẻ" : "đồng chẵn")
                    :
                    advance.AdvanceCurrency;

            var _inword = InWordCurrency.ConvertNumberCurrencyToString(_amount, _currency);

            var parameter = new AdvancePaymentRequestReportParams
            {
                CompanyName = AccountingConstants.COMPANY_NAME,
                CompanyAddress1 = AccountingConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = AccountingConstants.COMPANY_ADDRESS2,
                Website = AccountingConstants.COMPANY_WEBSITE,
                Contact = AccountingConstants.COMPANY_CONTACT,
                Inword = _inword ?? string.Empty
            };

            result = new Crystal
            {
                ReportName = "AdvancePaymentRequest.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listAdvance);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal Preview(AcctAdvancePaymentModel advance)
        {
            Crystal result = null;
            string strJobId = string.Empty;
            string strHbl = string.Empty;
            string strCustomNo = string.Empty;
            int contQty = 0;
            decimal nw = 0;
            int psc = 0;
            decimal cbm = 0;

            if (advance == null) return null;

            if (advance.AdvanceRequests.Count > 0)
            {
                foreach (var jobId in advance.AdvanceRequests.GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId))
                {
                    //Lấy ra NW, CBM, PSC, Container Qty
                    var ops = opsTransactionRepo.Get(x => x.JobNo == jobId).FirstOrDefault();
                    if (ops != null)
                    {
                        contQty += ops.SumContainers.HasValue ? ops.SumContainers.Value : 0;
                        nw += ops.SumNetWeight.HasValue ? ops.SumNetWeight.Value : 0;
                        psc += ops.SumPackages.HasValue ? ops.SumPackages.Value : 0;
                        cbm += ops.SumCbm.HasValue ? ops.SumCbm.Value : 0;
                    }

                    //Lấy ra chuỗi JobId
                    strJobId += !string.IsNullOrEmpty(jobId) ? jobId + "," : string.Empty;
                }

                //Lấy ra chuỗi HBL
                foreach (var hbl in advance.AdvanceRequests.GroupBy(x => x.Hbl).Select(x => x.FirstOrDefault().Hbl))
                {
                    strHbl += !string.IsNullOrEmpty(hbl) ? hbl + "," : string.Empty;
                }

                //Lấy ra chuỗi CustomNo
                foreach (var customNo in advance.AdvanceRequests.GroupBy(x => x.CustomNo).Select(x => x.FirstOrDefault().CustomNo))
                {
                    strCustomNo += !string.IsNullOrEmpty(customNo) ? customNo + "," : string.Empty;
                }

                strJobId += ")";
                strJobId = strJobId.Replace(",)", string.Empty).Replace(")", string.Empty);
                strHbl += ")";
                strHbl = strHbl.Replace(",)", string.Empty).Replace(")", string.Empty);
                strCustomNo += ")";
                strCustomNo = strCustomNo.Replace(",)", string.Empty).Replace(")", string.Empty);
            }

            //Lấy ra tên requester
            var employeeId = sysUserRepo.Get(x => x.Id == advance.Requester).Select(x => x.EmployeeId).FirstOrDefault();
            var requesterName = sysEmployeeRepo.Get(x => x.Id == employeeId).Select(x => x.EmployeeNameVn).FirstOrDefault();

            string managerName = string.Empty;
            string accountantName = string.Empty;
            var approveAdvance = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeny == false).FirstOrDefault();
            if (approveAdvance != null)
            {
                managerName = string.IsNullOrEmpty(approveAdvance.Manager) ? null : userBaseService.GetEmployeeByUserId(approveAdvance.Manager)?.EmployeeNameVn;
                accountantName = string.IsNullOrEmpty(approveAdvance.Accountant) ? null : userBaseService.GetEmployeeByUserId(approveAdvance.Accountant)?.EmployeeNameVn;
            }

            var acctAdvance = new AdvancePaymentRequestReport
            {
                AdvID = advance.AdvanceNo,
                RefNo = "N/A",
                AdvDate = advance.RequestDate.Value.Date,
                AdvTo = "N/A",
                AdvContactID = "N/A",
                AdvContact = requesterName,
                AdvAddress = string.Empty,
                AdvValue = advance.AdvanceRequests.Select(s => s.Amount).Sum(),
                AdvCurrency = advance.AdvanceCurrency,
                AdvCondition = advance.AdvanceNote,
                AdvRef = strJobId,
                AdvHBL = strHbl,
                AdvPaymentDate = null,
                AdvPaymentNote = "N/A",
                AdvDpManagerID = "N/A",
                AdvDpManagerStickDeny = null,
                AdvDpManagerStickApp = null,
                AdvDpManagerName = managerName,
                AdvDpSignDate = null,
                AdvAcsDpManagerID = "N/A",
                AdvAcsDpManagerStickDeny = null,
                AdvAcsDpManagerStickApp = null,
                AdvAcsDpManagerName = accountantName,
                AdvAcsSignDate = null,
                AdvBODID = "N/A",
                AdvBODStickDeny = null,
                AdvBODStickApp = null,
                AdvBODName = "N/A",
                AdvBODSignDate = null,
                AdvCashier = "N/A",
                AdvCashierName = "N/A",
                CashedDate = null,
                Saved = null,
                SettleNo = "N/A",
                PaidDate = null,
                AmountSettle = 0,
                SettleCurrency = "N/A",
                ClearStatus = null,
                Status = "N/A",
                AcsApproval = null,
                Description = "N/A",
                JobNo = "N/A",
                MAWB = "N/A",
                Amount = 0,
                Currency = "N/A",
                ExchangeRate = 0,
                TotalAmount = 0,
                PaymentDate = advance.DeadlinePayment.Value.Date,
                InvoiceNo = "N/A",
                CustomID = strCustomNo,
                HBLNO = "N/A",
                Norm = null,
                Validfee = null,
                Others = null,
                CSApp = null,
                CSDecline = null,
                CSUser = "N/A",
                CSAppDate = null,
                Customer = string.Empty,
                Shipper = string.Empty,
                Consignee = string.Empty,
                ContQty = contQty.ToString(),
                Noofpieces = psc,
                UnitPieaces = "N/A",
                GW = 0,
                NW = nw,
                CBM = cbm,
                ServiceType = "N/A",
                AdvCSName = string.Empty,
                AdvCSSignDate = null,
                AdvCSStickApp = null,
                AdvCSStickDeny = null,
                TotalNorm = advance.AdvanceRequests.Where(x => x.AdvanceType == AccountingConstants.ADVANCE_TYPE_NORM).Select(s => s.Amount).Sum(),
                TotalInvoice = advance.AdvanceRequests.Where(x => x.AdvanceType == AccountingConstants.ADVANCE_TYPE_INVOICE).Select(s => s.Amount).Sum(),
                TotalOrther = advance.AdvanceRequests.Where(x => x.AdvanceType == AccountingConstants.ADVANCE_TYPE_OTHER).Select(s => s.Amount).Sum()
            };

            acctAdvance.TotalNorm = acctAdvance.TotalNorm != 0 ? acctAdvance.TotalNorm : null;
            acctAdvance.TotalInvoice = acctAdvance.TotalInvoice != 0 ? acctAdvance.TotalInvoice : null;
            acctAdvance.TotalOrther = acctAdvance.TotalOrther != 0 ? acctAdvance.TotalOrther : null;

            var listAdvance = new List<AdvancePaymentRequestReport>
            {
                acctAdvance
            };

            //Chuyển tiền Amount thành chữ
            decimal _amount = acctAdvance.AdvValue ?? 0; //acctAdvance.AdvValue.HasValue ? acctAdvance.AdvValue.Value : 0;

            var _currency = advance.AdvanceCurrency == AccountingConstants.CURRENCY_LOCAL ?
                       (_amount % 1 > 0 ? "đồng lẻ" : "đồng chẵn")
                    :
                    advance.AdvanceCurrency;
            var _inword = InWordCurrency.ConvertNumberCurrencyToString(_amount, _currency);

            var parameter = new AdvancePaymentRequestReportParams
            {
                CompanyName = AccountingConstants.COMPANY_NAME,
                CompanyAddress1 = AccountingConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = AccountingConstants.COMPANY_ADDRESS2,
                Website = AccountingConstants.COMPANY_WEBSITE,
                Contact = AccountingConstants.COMPANY_CONTACT,
                Inword = _inword
            };

            result = new Crystal
            {
                ReportName = "AdvancePaymentRequest.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listAdvance);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
        #endregion PREVIEW ADVANCE PAYMENT

        #region APPROVAL ADVANCE PAYMENT        
        //Insert Or Update AcctApproveAdvance by AdvanceNo
        public HandleState InsertOrUpdateApprovalAdvance(AcctApproveAdvanceModel approve)
        {
            try
            {
                var userCurrent = currentUser.UserID;
                var advanceApprove = mapper.Map<AcctApproveAdvance>(approve);
                var advancePayment = DataContext.Get(x => x.AdvanceNo == approve.AdvanceNo).FirstOrDefault();

                if (!string.IsNullOrEmpty(approve.AdvanceNo))
                {
                    if (advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                        && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                        && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE
                        && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)
                    {
                        return new HandleState("Awaiting approval");
                    }
                }

                // Check existing Settling Flow
                var settingFlow = userBaseService.GetSettingFlowApproval(typeApproval, advancePayment.OfficeId);
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

                        var isAllLevelAutoOrNone = CheckAllLevelIsAutoOrNone(typeApproval, advancePayment.OfficeId);
                        if (isAllLevelAutoOrNone)
                        {
                            //Cập nhật Status Approval là Done cho Advance Payment
                            advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            var hsUpdateAdvancePayment = DataContext.Update(advancePayment, x => x.Id == advancePayment.Id, false);
                        }

                        var leaderLevel = LeaderLevel(typeApproval, advancePayment.GroupId, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
                        var managerLevel = ManagerLevel(typeApproval, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
                        var accountantLevel = AccountantLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);
                        var buHeadLevel = BuHeadLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);

                        var userLeaderOrManager = string.Empty;
                        var mailLeaderOrManager = string.Empty;
                        List<string> mailUsersDeputy = new List<string>();

                        if (leaderLevel.Role == AccountingConstants.ROLE_AUTO || leaderLevel.Role == AccountingConstants.ROLE_APPROVAL)
                        {
                            _leader = leaderLevel.UserId;
                            if (string.IsNullOrEmpty(_leader)) return new HandleState("Not found leader");
                            if (leaderLevel.Role == AccountingConstants.ROLE_AUTO)
                            {
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED;
                                advanceApprove.LeaderApr = userCurrent;
                                advanceApprove.LeaderAprDate = DateTime.Now;
                                advanceApprove.LevelApprove = AccountingConstants.LEVEL_LEADER;
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
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            }
                        }

                        if (managerLevel.Role == AccountingConstants.ROLE_AUTO || managerLevel.Role == AccountingConstants.ROLE_APPROVAL)
                        {
                            _manager = managerLevel.UserId;
                            if (string.IsNullOrEmpty(_manager)) return new HandleState("Not found manager");
                            if (managerLevel.Role == AccountingConstants.ROLE_AUTO && leaderLevel.Role != AccountingConstants.ROLE_APPROVAL)
                            {
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED;
                                advanceApprove.ManagerApr = userCurrent;
                                advanceApprove.ManagerAprDate = DateTime.Now;
                                advanceApprove.LevelApprove = AccountingConstants.LEVEL_MANAGER;
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
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
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
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                advanceApprove.AccountantApr = userCurrent;
                                advanceApprove.AccountantAprDate = DateTime.Now;
                                advanceApprove.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
                                if (buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL)
                                {
                                    advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                    advanceApprove.BuheadApr = advanceApprove.AccountantApr = userCurrent;
                                    advanceApprove.BuheadAprDate = advanceApprove.AccountantAprDate = DateTime.Now;
                                    advanceApprove.LevelApprove = AccountingConstants.LEVEL_BOD;
                                }
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
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
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
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                advanceApprove.BuheadApr = userCurrent;
                                advanceApprove.BuheadAprDate = DateTime.Now;
                                advanceApprove.LevelApprove = AccountingConstants.LEVEL_BOD;
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
                            if (buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL && (leaderLevel.Role == AccountingConstants.ROLE_NONE || leaderLevel.Role == AccountingConstants.ROLE_AUTO) && (managerLevel.Role == AccountingConstants.ROLE_NONE || managerLevel.Role == AccountingConstants.ROLE_AUTO) && (accountantLevel.Role == AccountingConstants.ROLE_NONE || accountantLevel.Role == AccountingConstants.ROLE_AUTO))
                            {
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                advanceApprove.BuheadApr = userCurrent;
                                advanceApprove.BuheadAprDate = DateTime.Now;
                                advanceApprove.LevelApprove = AccountingConstants.LEVEL_BOD;
                                if (leaderLevel.Role != AccountingConstants.ROLE_NONE)
                                {
                                    advanceApprove.LeaderApr = userCurrent;
                                    advanceApprove.LeaderAprDate = DateTime.Now;
                                }
                                if (managerLevel.Role != AccountingConstants.ROLE_NONE)
                                {
                                    advanceApprove.ManagerApr = userCurrent;
                                    advanceApprove.ManagerAprDate = DateTime.Now;
                                }
                                if (accountantLevel.Role != AccountingConstants.ROLE_NONE)
                                {
                                    advanceApprove.AccountantApr = userCurrent;
                                    advanceApprove.AccountantAprDate = DateTime.Now;
                                }
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
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            }
                        }

                        var sendMailApproved = true;
                        var sendMailSuggest = true;
                        if (advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                        {
                            //Send Mail Approved
                            sendMailApproved = SendMailApproved(advancePayment.AdvanceNo, DateTime.Now);
                        }
                        else
                        {
                            //Send Mail Suggest
                            sendMailSuggest = SendMailSuggestApproval(advancePayment.AdvanceNo, userLeaderOrManager, mailLeaderOrManager, mailUsersDeputy);
                        }

                        if (!sendMailSuggest)
                        {
                            return new HandleState("Send mail suggest approval failed");
                        }
                        if (!sendMailApproved)
                        {
                            return new HandleState("Send mail approved approval failed");
                        }

                        var checkExistsApproveByAdvanceNo = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advanceApprove.AdvanceNo && x.IsDeny == false).FirstOrDefault();
                        if (checkExistsApproveByAdvanceNo == null) //Insert ApproveAdvance
                        {
                            advanceApprove.Id = Guid.NewGuid();
                            advanceApprove.Leader = _leader;
                            advanceApprove.Manager = _manager;
                            advanceApprove.Accountant = _accountant;
                            advanceApprove.Buhead = _bhHead;
                            advanceApprove.UserCreated = advanceApprove.UserModified = userCurrent;
                            advanceApprove.DateCreated = advanceApprove.DateModified = DateTime.Now;
                            advanceApprove.IsDeny = false;
                            var hsAddApprove = acctApproveAdvanceRepo.Add(advanceApprove, false);
                        }
                        else //Update ApproveAdvance by Advance No
                        {
                            checkExistsApproveByAdvanceNo.UserModified = userCurrent;
                            checkExistsApproveByAdvanceNo.DateModified = DateTime.Now;
                            var hsUpdateApprove = acctApproveAdvanceRepo.Update(checkExistsApproveByAdvanceNo, x => x.Id == checkExistsApproveByAdvanceNo.Id, false);
                        }

                        acctApproveAdvanceRepo.SubmitChanges();
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

        public HandleState UpdateApproval(Guid advanceId)
        {
            var userCurrent = currentUser.UserID;
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var advancePayment = DataContext.Get(x => x.Id == advanceId).FirstOrDefault();
                    if (advancePayment == null) return new HandleState("Not found Advance Payment");

                    var approve = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advancePayment.AdvanceNo && x.IsDeny == false).FirstOrDefault();
                    if (approve == null)
                    {
                        return new HandleState("Not found advance payment approval");
                    }

                    var isAllLevelAutoOrNone = CheckAllLevelIsAutoOrNone(typeApproval, advancePayment.OfficeId);
                    if (isAllLevelAutoOrNone)
                    {
                        //Cập nhật Status Approval là Done cho Advance Payment
                        advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                    }

                    var leaderLevel = LeaderLevel(typeApproval, advancePayment.GroupId, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
                    var managerLevel = ManagerLevel(typeApproval, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
                    var accountantLevel = AccountantLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);
                    var buHeadLevel = BuHeadLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);

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
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED;
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
                                        advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED;
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
                                            advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                            approve.AccountantApr = accountantLevel.UserId;
                                            approve.AccountantAprDate = DateTime.Now;
                                            approve.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
                                            userApproveNext = buHeadLevel.UserId;
                                            mailUserApproveNext = buHeadLevel.EmailUser;
                                            mailUsersDeputy = buHeadLevel.EmailDeputies;
                                        }
                                        if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO)
                                        {
                                            advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                            approve.BuheadApr = buHeadLevel.UserId;
                                            approve.BuheadAprDate = DateTime.Now;
                                            approve.LevelApprove = AccountingConstants.LEVEL_BOD;
                                        }
                                        if (buHeadLevel.Role == AccountingConstants.ROLE_NONE)
                                        {
                                            advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
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
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED;
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
                                        advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                        approve.AccountantApr = accountantLevel.UserId;
                                        approve.AccountantAprDate = DateTime.Now;
                                        approve.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
                                        userApproveNext = buHeadLevel.UserId;
                                        mailUserApproveNext = buHeadLevel.EmailUser;
                                        mailUsersDeputy = buHeadLevel.EmailDeputies;
                                    }
                                    if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO)
                                    {
                                        advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                        approve.BuheadApr = buHeadLevel.UserId;
                                        approve.BuheadAprDate = DateTime.Now;
                                        approve.LevelApprove = AccountingConstants.LEVEL_BOD;
                                    }
                                    if (buHeadLevel.Role == AccountingConstants.ROLE_NONE)
                                    {
                                        advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
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
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                approve.AccountantApr = userCurrent;
                                approve.AccountantAprDate = DateTime.Now;
                                approve.LevelApprove = AccountingConstants.LEVEL_ACCOUNTANT;
                                userApproveNext = buHeadLevel.UserId;
                                mailUserApproveNext = buHeadLevel.EmailUser;
                                mailUsersDeputy = buHeadLevel.EmailDeputies;
                                if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO)
                                {
                                    advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                                    approve.BuheadApr = buHeadLevel.UserId;
                                    approve.BuheadAprDate = DateTime.Now;
                                    approve.LevelApprove = AccountingConstants.LEVEL_BOD;
                                }
                                if (buHeadLevel.Role == AccountingConstants.ROLE_NONE)
                                {
                                    advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
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
                        if (string.IsNullOrEmpty(approve.BuheadApr))
                        {
                            if ((!string.IsNullOrEmpty(approve.Accountant) && !string.IsNullOrEmpty(approve.AccountantApr)) || string.IsNullOrEmpty(approve.Accountant) || accountantLevel.Role == AccountingConstants.ROLE_NONE || accountantLevel.Role == AccountingConstants.ROLE_AUTO || buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL)
                            {
                                advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
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

                    if (advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                    {
                        //Send Mail Approved
                        sendMailApproved = SendMailApproved(advancePayment.AdvanceNo, DateTime.Now);
                    }
                    else
                    {
                        //Send Mail Suggest
                        sendMailSuggest = SendMailSuggestApproval(advancePayment.AdvanceNo, userApproveNext, mailUserApproveNext, mailUsersDeputy);
                    }

                    if (!sendMailSuggest)
                    {
                        return new HandleState("Send mail suggest Approval failed");
                    }
                    if (!sendMailApproved)
                    {
                        return new HandleState("Send mail Approved Approval failed");
                    }

                    advancePayment.UserModified = approve.UserModified = userCurrent;
                    advancePayment.DatetimeModified = approve.DateModified = DateTime.Now;

                    var hsUpdateadvancePayment = DataContext.Update(advancePayment, x => x.Id == advancePayment.Id, false);
                    var hsUpdateApprove = acctApproveAdvanceRepo.Update(approve, x => x.Id == approve.Id, false);

                    acctApproveAdvanceRepo.SubmitChanges();
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

        public HandleState DeniedApprove(Guid advanceId, string comment)
        {
            var userCurrent = currentUser.UserID;

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var advancePayment = DataContext.Get(x => x.Id == advanceId).FirstOrDefault();
                    if (advancePayment == null) return new HandleState("Not found Advance Payment");

                    var approve = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advancePayment.AdvanceNo && x.IsDeny == false).FirstOrDefault();
                    if (approve == null)
                    {
                        return new HandleState("Not found advance payment approval");
                    }

                    if (advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DENIED)
                    {
                        return new HandleState("Advance payment has been denied");
                    }

                    var leaderLevel = LeaderLevel(typeApproval, advancePayment.GroupId, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
                    var managerLevel = ManagerLevel(typeApproval, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
                    var accountantLevel = AccountantLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);
                    var buHeadLevel = BuHeadLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);

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
                        if (advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED
                            || advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                            || advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                        {
                            return new HandleState("Not allow deny. Advance payment has been approved");
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
                        if (advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                            || advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                        {
                            return new HandleState("Not allow deny. Advance payment has been approved");
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
                        if (advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                        {
                            return new HandleState("Not allow deny. Advance payment has been approved");
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
                    if (buHeadLevel.Role == AccountingConstants.ROLE_APPROVAL && isBod
                        &&
                        (
                          (isBuHead && currentUser.GroupId == AccountingConstants.SpecialGroup && userCurrent == buHeadLevel.UserId)
                          ||
                          buHeadLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (accountantLevel.Role == AccountingConstants.ROLE_APPROVAL && string.IsNullOrEmpty(approve.AccountantApr))
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

                    var sendMailDeny = SendMailDeniedApproval(advancePayment.AdvanceNo, comment, DateTime.Now);
                    if (!sendMailDeny)
                    {
                        return new HandleState("Send mail denied failed");
                    }

                    advancePayment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DENIED;
                    approve.IsDeny = true;
                    approve.Comment = comment;
                    advancePayment.UserModified = approve.UserModified = userCurrent;
                    advancePayment.DatetimeModified = approve.DateModified = DateTime.Now;

                    var hsUpdateadvancePayment = DataContext.Update(advancePayment, x => x.Id == advancePayment.Id, false);
                    var hsUpdateApprove = acctApproveAdvanceRepo.Update(approve, x => x.Id == approve.Id, false);

                    acctApproveAdvanceRepo.SubmitChanges();
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

        public HandleState RecallRequest(Guid advanceId)
        {
            var userCurrent = currentUser.UserID;
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var advance = DataContext.Get(x => x.Id == advanceId).FirstOrDefault();
                    if (advance == null)
                    {
                        return new HandleState("Not found Advance Payment");
                    }
                    else
                    {
                        if (advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_DENIED
                            || advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_NEW)
                        {
                            return new HandleState("Advance payment not yet send the request");
                        }
                        if (advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                            && advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                            && advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)
                        {
                            return new HandleState("Advance payment approving");
                        }

                        var approve = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeny == false).FirstOrDefault();
                        //Cập nhật lại approve advance
                        if (approve != null)
                        {
                            approve.UserModified = userCurrent;
                            approve.DateModified = DateTime.Now;
                            approve.Comment = "RECALL";
                            approve.IsDeny = true;
                            var hsUpdateApproveAdvance = acctApproveAdvanceRepo.Update(approve, x => x.Id == approve.Id);
                        }

                        //Cập nhật lại advance status của Advance Payment
                        advance.StatusApproval = AccountingConstants.STATUS_APPROVAL_NEW;
                        advance.UserModified = userCurrent;
                        advance.DatetimeModified = DateTime.Now;
                        var hsUpdateAdvancePayment = DataContext.Update(advance, x => x.Id == advance.Id);
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

        //Send Mail đề nghị Approve
        private bool SendMailSuggestApproval(string advanceNo, string userReciver, string emailUserReciver, List<string> usersDeputy)
        {
            //Lấy ra AdvancePayment dựa vào AdvanceNo
            var advance = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            if (advance == null) return false;

            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(advance.Requester);
            var requesterName = userBaseService.GetEmployeeByEmployeeId(requesterId)?.EmployeeNameEn;
            var emailRequester = userBaseService.GetEmployeeByEmployeeId(requesterId)?.Email;

            //Lấy ra thông tin của Advance Request dựa vào AdvanceNo
            var requests = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo).GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId);
            string jobIds = string.Empty;
            jobIds = String.Join("; ", requests.ToList());

            var totalAmount = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo).Select(s => s.Amount).Sum();
            if (totalAmount != null)
            {
                totalAmount = Math.Round(totalAmount.Value, 2);
            }

            var userReciverId = userBaseService.GetEmployeeIdOfUser(userReciver);
            var userReciverName = userBaseService.GetEmployeeByEmployeeId(userReciverId)?.EmployeeNameEn;

            //Mail Info
            var numberOfRequest = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo).Select(s => s.Id).Count();
            numberOfRequest = numberOfRequest == 0 ? 1 : (numberOfRequest + 1);
            string subject = "eFMS - Advance Payment Approval Request from [RequesterName] - [NumberOfRequest] " + (numberOfRequest > 1 ? "times" : "time");
            subject = subject.Replace("[RequesterName]", requesterName);
            subject = subject.Replace("[NumberOfRequest]", numberOfRequest.ToString());
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [UserName],</b></i></p><p>You have new Advance Payment Approval Request from <b>[RequesterName]</b> as below info:</p><p><i>Anh/ Chị có một yêu cầu duyệt tạm ứng từ <b>[RequesterName]</b> với thông tin như sau:</i></p><ul><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNo]</b></li><li>Advance Amount/ <i>Số tiền tạm ứng</i> : <b>[TotalAmount] [CurrencyAdvance]</b><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li></ul><p>You click here to check more detail and approve: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]/approve' target='_blank'>Detail Advance Request</a></span></p><p><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]/approve' target='_blank'>Chi tiết phiếu tạm ứng</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[UserName]", userReciverName);
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[AdvanceNo]", advanceNo);
            body = body.Replace("[TotalAmount]", String.Format("{0:n}", totalAmount));
            body = body.Replace("[CurrencyAdvance]", advance.AdvanceCurrency);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[RequestDate]", advance.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/accounting/advance-payment");
            body = body.Replace("[AdvanceId]", advance.Id.ToString());
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
        private bool SendMailApproved(string advanceNo, DateTime approvedDate)
        {
            //Lấy ra AdvancePayment dựa vào AdvanceNo
            var advance = DataContext.Where(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            if (advance == null) return false;

            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(advance.Requester);
            var _requester = userBaseService.GetEmployeeByEmployeeId(requesterId);
            var requesterName = _requester?.EmployeeNameEn;
            var emailRequester = _requester?.Email;

            //Lấy ra thông tin của Advance Request dựa vào AdvanceNo
            var requests = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo).GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId);
            string jobIds = string.Empty;
            jobIds = String.Join("; ", requests.ToList());

            var totalAmount = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo).Select(s => s.Amount).Sum();
            if (totalAmount != null)
            {
                totalAmount = Math.Round(totalAmount.Value, 2);
            }

            //Mail Info
            string subject = "eFMS - Advance Payment from [RequesterName] is approved";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have an Advance Payment is approved at <b>[ApprovedDate]</b> as below info:</p><p><i>Anh/ Chị có một yêu cầu tạm ứng đã được phê duyệt vào lúc <b>[ApprovedDate]</b> với thông tin như sau:</i></p><ul><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNo]</b></li><li>Advance Amount/ <i>Số tiền tạm ứng</i> : <b>[TotalAmount] [CurrencyAdvance]</b><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li></ul><p>You can click here to check more detail: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]' target='_blank'>Detail Advance Request</a></span></p><p><i>Anh/ Chị có thể chọn vào đây để biết thêm thông tin chi tiết: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]' target='_blank'>Chi tiết tạm ứng</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[ApprovedDate]", approvedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[AdvanceNo]", advanceNo);
            body = body.Replace("[TotalAmount]", String.Format("{0:n}", totalAmount));
            body = body.Replace("[CurrencyAdvance]", advance.AdvanceCurrency);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[RequestDate]", advance.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/accounting/advance-payment");
            body = body.Replace("[AdvanceId]", advance.Id.ToString());
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

        //Send Mail Deny Approve (Gửi đến Requester và các Leader, Manager, Accountant, BUHead đã approve trước đó)
        private bool SendMailDeniedApproval(string advanceNo, string comment, DateTime DeniedDate)
        {
            //Lấy ra AdvancePayment dựa vào AdvanceNo
            var advance = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            if (advance == null) return false;

            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(advance.Requester);
            var _requester = userBaseService.GetEmployeeByEmployeeId(requesterId);
            var requesterName = _requester?.EmployeeNameEn;
            var emailRequester = _requester?.Email;

            //Lấy ra thông tin của Advance Request dựa vào AdvanceNo
            var requests = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo).GroupBy(x => x.JobId).Select(x => x.FirstOrDefault().JobId);
            string jobIds = string.Empty;
            jobIds = String.Join("; ", requests.ToList());

            var totalAmount = acctAdvanceRequestRepo.Get(x => x.AdvanceNo == advanceNo).Select(s => s.Amount).Sum();
            if (totalAmount != null)
            {
                totalAmount = Math.Round(totalAmount.Value, 2);
            }

            //Mail Info
            string subject = "eFMS - Advance Payment from [RequesterName] is denied";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have an Advance Payment is denied at <b>[DeniedDate]</b> by as below info:</p><p><i>Anh/ Chị có một yêu cầu tạm ứng đã bị từ chối vào lúc <b>[DeniedDate]</b> by với thông tin như sau:</i></p><ul><li>Advance No / <i>Mã tạm ứng</i> : <b>[AdvanceNo]</b></li><li>Advance Amount/ <i>Số tiền tạm ứng</i> : <b>[TotalAmount] [CurrencyAdvance]</b><li>Shipments/ <i>Lô hàng</i> : <b>[JobIds]</b></li><li>Requester/ <i>Người đề nghị</i> : <b>[RequesterName]</b></li><li>Request date/ <i>Thời gian đề nghị</i> : <b>[RequestDate]</b></li><li>Comment/ <i>Lý do từ chối</i> : <b>[Comment]</b></li></ul><p>You click here to recheck detail: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]' target='_blank'>Detail Advance Request</a></span></p><p><i>Anh/ Chị chọn vào đây để kiểm tra lại thông tin chi tiết: <span><a href='[Url]/[lang]/[UrlFunc]/[AdvanceId]' target='_blank'>Chi tiết tạm ứng</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[DeniedDate]", DeniedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[AdvanceNo]", advanceNo);
            body = body.Replace("[TotalAmount]", String.Format("{0:n}", totalAmount));
            body = body.Replace("[CurrencyAdvance]", advance.AdvanceCurrency);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[RequestDate]", advance.RequestDate.Value.ToString("dd/MM/yyyy"));
            body = body.Replace("[Comment]", comment);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/accounting/advance-payment");
            body = body.Replace("[AdvanceId]", advance.Id.ToString());
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
        
        public AcctApproveAdvanceModel GetInfoApproveAdvanceByAdvanceNo(string advanceNo)
        {
            var userCurrent = currentUser.UserID;

            var advanceApprove = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advanceNo && x.IsDeny == false).FirstOrDefault();
            var advanceApproveModel = new AcctApproveAdvanceModel();

            if (advanceApprove != null)
            {
                advanceApproveModel = mapper.Map<AcctApproveAdvanceModel>(advanceApprove);
                advanceApproveModel.RequesterName = userBaseService.GetEmployeeByUserId(advanceApproveModel.Requester)?.EmployeeNameVn;
                advanceApproveModel.LeaderName = userBaseService.GetEmployeeByUserId(advanceApproveModel.Leader)?.EmployeeNameVn;
                advanceApproveModel.ManagerName = userBaseService.GetEmployeeByUserId(advanceApproveModel.Manager)?.EmployeeNameVn;
                advanceApproveModel.AccountantName = userBaseService.GetEmployeeByUserId(advanceApproveModel.Accountant)?.EmployeeNameVn;
                advanceApproveModel.BUHeadName = userBaseService.GetEmployeeByUserId(advanceApproveModel.Buhead)?.EmployeeNameVn;
                advanceApproveModel.StatusApproval = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault()?.StatusApproval;
                advanceApproveModel.NumOfDeny = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advanceNo && x.IsDeny == true && x.Comment != "RECALL").Select(s => s.Id).Count();
                advanceApproveModel.IsShowLeader = !string.IsNullOrEmpty(advanceApprove.Leader);
                advanceApproveModel.IsShowManager = !string.IsNullOrEmpty(advanceApprove.Manager);
                advanceApproveModel.IsShowAccountant = !string.IsNullOrEmpty(advanceApprove.Accountant);
                advanceApproveModel.IsShowBuHead = !string.IsNullOrEmpty(advanceApprove.Buhead);
            }
            else
            {
                advanceApproveModel.StatusApproval = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault()?.StatusApproval;
                advanceApproveModel.NumOfDeny = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advanceNo && x.IsDeny == true && x.Comment != "RECALL").Select(s => s.Id).Count();
            }
            return advanceApproveModel;
        }

        public List<DeniedInfoResult> GetHistoryDeniedAdvance(string advanceNo)
        {
            var approves = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advanceNo && x.IsDeny == true && x.Comment != "RECALL").OrderByDescending(x => x.DateCreated).ToList();
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

        #endregion APPROVAL ADVANCE PAYMENT

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
                }
            }

            var managerLevel = ManagerLevel(type, departmentId, officeId, companyId);
            if (managerLevel.Role == AccountingConstants.ROLE_AUTO || managerLevel.Role == AccountingConstants.ROLE_APPROVAL)
            {
                if (string.IsNullOrEmpty(managerLevel.UserId)) return new HandleState("Not found manager");
            }

            var accountantLevel = AccountantLevel(type, officeId, companyId);
            if (accountantLevel.Role == AccountingConstants.ROLE_AUTO || accountantLevel.Role == AccountingConstants.ROLE_APPROVAL)
            {
                if (string.IsNullOrEmpty(accountantLevel.UserId)) return new HandleState("Not found accountant");
            }

            var buHeadLevel = BuHeadLevel(type, officeId, companyId);
            if (buHeadLevel.Role == AccountingConstants.ROLE_AUTO || buHeadLevel.Role == AccountingConstants.ROLE_APPROVAL)
            {
                if (string.IsNullOrEmpty(buHeadLevel.UserId)) return new HandleState("Not found BOD");
            }
            return new HandleState();
        }

        public bool CheckUserIsApproved(ICurrentUser userCurrent, AcctAdvancePayment advancePayment, AcctApproveAdvance approve)
        {
            var isApproved = false;
            if (approve == null) return true;
            var leaderLevel = LeaderLevel(typeApproval, advancePayment.GroupId, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
            var managerLevel = ManagerLevel(typeApproval, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
            var accountantLevel = AccountantLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);
            var buHeadLevel = BuHeadLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);

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

        public bool CheckUserIsManager(ICurrentUser userCurrent, AcctAdvancePayment advancePayment, AcctApproveAdvance approve)
        {
            var isManagerOrLeader = false;

            var leaderLevel = LeaderLevel(typeApproval, advancePayment.GroupId, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
            var managerLevel = ManagerLevel(typeApproval, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
            var accountantLevel = AccountantLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);
            var buHeadLevel = BuHeadLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);

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

        public bool CheckIsShowBtnDeny(ICurrentUser userCurrent, AcctAdvancePayment advancePayment, AcctApproveAdvance approve)
        {
            if (approve == null) return false;

            var leaderLevel = LeaderLevel(typeApproval, advancePayment.GroupId, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
            var managerLevel = ManagerLevel(typeApproval, advancePayment.DepartmentId, advancePayment.OfficeId, advancePayment.CompanyId);
            var accountantLevel = AccountantLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);
            var buHeadLevel = BuHeadLevel(typeApproval, advancePayment.OfficeId, advancePayment.CompanyId);

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
                if (advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL || advancePayment.StatusApproval == AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED)
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
                if (   !string.IsNullOrEmpty(approve.Manager)
                    && advancePayment.StatusApproval != AccountingConstants.ACCOUNTING_INVOICE_STATUS_NEW
                    && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                    && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                    && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE)
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
                    && advancePayment.StatusApproval != AccountingConstants.ACCOUNTING_INVOICE_STATUS_NEW
                    && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                    && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE)
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
            else if (buHeadLevel.Role == AccountingConstants.ROLE_SPECIAL && isBod
                &&
                (
                  (isBuHead && currentUser.GroupId == AccountingConstants.SpecialGroup && userCurrent.UserID == buHeadLevel.UserId)
                  ||
                  buHeadLevel.UserDeputies.Contains(userCurrent.UserID)
                )
                && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED && advancePayment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE
               )
            {
                isShowBtnDeny = true;
            }
            else
            {
                isShowBtnDeny = false;
            }
            return isShowBtnDeny;
        }
        #endregion -- Check Exist, Check Is Manager, Is Approved --

        public List<AcctAdvanceRequestModel> GetAdvancesOfShipment()
        {
            var request = acctAdvanceRequestRepo.Get();
            var opsShipment = opsTransactionRepo.Get(x => x.Hblid != Guid.Empty && x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false);
            var docShipment = csTransactionRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED && x.IsLocked == false);
            var surcharge = csShipmentSurchargeRepo.Get();

            var requestOrder = request.GroupBy(g => new { g.AdvanceNo, g.Hbl }).Select(s => new AcctAdvanceRequest
            {
                Amount = s.Sum(a => a.Amount),
                AdvanceNo = s.First().AdvanceNo,
                RequestCurrency = s.First().RequestCurrency,
                JobId = s.First().JobId,
                Hbl = s.First().Hbl
            });

            //So sánh bằng
            var queryOps = from req in requestOrder
                           join ops in opsShipment on req.JobId equals ops.JobNo into ops2
                           from ops in ops2
                           join adv in DataContext.Get() on req.AdvanceNo equals adv.AdvanceNo into adv2
                           from adv in adv2
                           select new AcctAdvanceRequestModel
                           {
                               Id = req.Id,
                               JobId = req.JobId,
                               Hbl = req.Hbl,
                               RequestDate = adv.RequestDate,
                               Amount = req.Amount,
                               AdvanceNo = req.AdvanceNo,
                               RequestCurrency = req.RequestCurrency
                           };

            //So sánh bằng
            var queryDoc = from req in requestOrder
                           join doc in docShipment on req.JobId equals doc.JobNo into doc2
                           from doc in doc2
                           join adv in DataContext.Get() on req.AdvanceNo equals adv.AdvanceNo into adv2
                           from adv in adv2
                           select new AcctAdvanceRequestModel
                           {
                               Id = req.Id,
                               JobId = req.JobId,
                               Hbl = req.Hbl,
                               RequestDate = adv.RequestDate,
                               Amount = req.Amount,
                               AdvanceNo = req.AdvanceNo,
                               RequestCurrency = req.RequestCurrency
                           };
            var mergeAdvRequest = queryOps.Union(queryDoc);

            //Get advance theo shipment và advance chưa làm settlement; order giảm dần theo ngày request date
            var data = mergeAdvRequest.ToList().Where(x => !surcharge.Any(a => a.AdvanceNo == x.AdvanceNo));
            return data.ToList();
        }

        public LockedLogResultModel GetAdvanceToUnlock(List<string> keyWords)
        {
            var result = new LockedLogResultModel();
            var advancesToUnLock = DataContext.Get(x => keyWords.Contains(x.AdvanceNo));
            if (advancesToUnLock == null) return result;
            result.LockedLogs = advancesToUnLock.Select(x => new LockedLogModel
            {
                Id = x.Id,
                AdvanceNo = x.AdvanceNo,
                LockedLog = x.LockedLog
            });
            if (result.LockedLogs != null)
            {
                result.Logs = new List<string>();
                foreach (var item in advancesToUnLock)
                {
                    var logs = item.LockedLog != null ? item.LockedLog.Split(';').Where(x => x.Length > 0).ToList() : new List<string>();
                    result.Logs.AddRange(logs);
                }
            }
            return result;
        }

        public HandleState UnLock(List<LockedLogModel> advancePayments)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in advancePayments)
                    {
                        var payment = DataContext.Get(x => x.Id == item.Id)?.FirstOrDefault();

                        payment.StatusApproval = AccountingConstants.STATUS_APPROVAL_DENIED;
                        payment.UserModified = currentUser.UserID;
                        payment.DatetimeModified = DateTime.Now;
                        var log = item.AdvanceNo + " has been opened at " + string.Format("{0:HH:mm:ss tt}", DateTime.Now) + " on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + "admin";
                        payment.LockedLog = item.LockedLog + log + ";";
                        var hs = DataContext.Update(payment, x => x.Id == item.Id);
                        if (hs.Success)
                        {
                            var approveAdvances = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == item.AdvanceNo);
                            foreach (var approve in approveAdvances)
                            {
                                approve.IsDeny = true;
                                approve.UserModified = currentUser.UserID;
                                approve.DateModified = DateTime.Now;
                                acctApproveAdvanceRepo.Update(approve, x => x.Id == approve.Id);
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

        #region --- EXPORT ADVANCE ---
        public AdvanceExport AdvancePaymentExport(Guid advanceId, string language)
        {
            AdvanceExport dataExport = new AdvanceExport();
            var advancePayment = GetAdvancePaymentByAdvanceId(advanceId);
            if (advancePayment == null) return null;

            dataExport.InfoAdvance = GetInfoAdvanceExport(advancePayment, language);
            dataExport.ShipmentsAdvance = GetListShipmentAdvanceExport(advancePayment);
            return dataExport;
        }

        private InfoAdvanceExport GetInfoAdvanceExport(AcctAdvancePaymentModel advancePayment, string language)
        {
            string _requester = string.IsNullOrEmpty(advancePayment.Requester) ? string.Empty : userBaseService.GetEmployeeByUserId(advancePayment.Requester)?.EmployeeNameVn;

            #region -- Advance Amount & Sayword --           
            var _advanceAmount = advancePayment.AdvanceRequests.Select(s => s.Amount).Sum();
            if (advancePayment.AdvanceCurrency != AccountingConstants.CURRENCY_LOCAL)
            {
                //Tỉ giá quy đổi theo ngày đề nghị tạm ứng (RequestDate)
                var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == advancePayment.RequestDate.Value.Date).ToList();
                var _rate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, advancePayment.AdvanceCurrency, AccountingConstants.CURRENCY_LOCAL);
                _advanceAmount = _advanceAmount * _rate;
            }
            var _sayWordAmount = string.Empty;
            var _currencyAdvance = (language == "VN" && _advanceAmount >= 1) ?
                       (_advanceAmount % 1 > 0 ? "đồng lẻ" : "đồng chẵn")
                    :
                    advancePayment.AdvanceCurrency;
            _sayWordAmount = (language == "VN" && _advanceAmount >= 1) ?
                        InWordCurrency.ConvertNumberCurrencyToString(_advanceAmount.Value, _currencyAdvance)
                    :
                        InWordCurrency.ConvertNumberCurrencyToStringUSD(_advanceAmount.Value, "") + " " + AccountingConstants.CURRENCY_LOCAL;
            #endregion -- Advance Amount & Sayword --

            #region -- Info Manager, Accoutant & Department --
            var _advanceApprove = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advancePayment.AdvanceNo && x.IsDeny == false).FirstOrDefault();
            string _manager = string.Empty;
            string _accountant = string.Empty;
            if (_advanceApprove != null)
            {
                _manager = string.IsNullOrEmpty(_advanceApprove.Manager) ? string.Empty : userBaseService.GetEmployeeByUserId(_advanceApprove.Manager)?.EmployeeNameVn;
                _accountant = string.IsNullOrEmpty(_advanceApprove.Accountant) ? string.Empty : userBaseService.GetEmployeeByUserId(_advanceApprove.Accountant)?.EmployeeNameVn;
            }

            var _department = catDepartmentRepo.Get(x => x.Id == advancePayment.DepartmentId).FirstOrDefault()?.DeptNameAbbr;
            #endregion -- Info Manager, Accoutant & Department --

            var infoAdvance = new InfoAdvanceExport
            {
                Requester = _requester,
                RequestDate = advancePayment.RequestDate,
                Department = _department,
                AdvanceNo = advancePayment.AdvanceNo,
                AdvanceAmount = _advanceAmount,
                AdvanceAmountWord = _sayWordAmount,
                AdvanceReason = advancePayment.AdvanceNote,
                DealinePayment = advancePayment.DeadlinePayment,
                Manager = _manager,
                Accountant = _accountant
            };
            return infoAdvance;
        }

        private List<InfoShipmentAdvanceExport> GetListShipmentAdvanceExport(AcctAdvancePaymentModel advancePayment)
        {
            var shipmentsAdvance = new List<InfoShipmentAdvanceExport>();
            var groupJobByHbl = advancePayment.AdvanceRequests
                .GroupBy(g => new { g.Hbl })
                .Select(s => new AcctAdvanceRequestModel
                {
                    JobId = s.First().JobId,
                    Hbl = s.Key.Hbl,
                    Mbl = s.First().Mbl,
                    CustomNo = s.First().CustomNo
                });
            foreach (var request in groupJobByHbl)
            {
                string _customer = string.Empty;
                string _shipper = string.Empty;
                string _consignee = string.Empty;
                string _container = string.Empty;
                decimal? _cw = 0;
                decimal? _pcs = 0;
                decimal? _cbm = 0;
                if (request.JobId.Contains("LOG"))
                {
                    var ops = opsTransactionRepo.Get(x => x.JobNo == request.JobId).FirstOrDefault();
                    if (ops != null)
                    {
                        _customer = catPartnerRepo.Get(x => x.Id == ops.CustomerId).FirstOrDefault()?.PartnerNameVn;
                        _container = ops.ContainerDescription;
                        _cw = ops.SumChargeWeight ?? 0;
                        _pcs = ops.SumPackages ?? 0;
                        _cbm = ops.SumCbm ?? 0;
                    }
                }
                else
                {
                    var trans = csTransactionRepo.Get(x => x.JobNo == request.JobId).FirstOrDefault();
                    if (trans != null)
                    {
                        var tranDetail = csTransactionDetailRepo.Get(x => x.JobId == trans.Id && x.Hwbno == request.Hbl).FirstOrDefault();
                        if (tranDetail != null)
                        {
                            _customer = catPartnerRepo.Get(x => x.Id == tranDetail.CustomerId).FirstOrDefault()?.PartnerNameVn;
                            _shipper = catPartnerRepo.Get(x => x.Id == tranDetail.ShipperId).FirstOrDefault()?.PartnerNameVn;
                            _consignee = catPartnerRepo.Get(x => x.Id == tranDetail.ConsigneeId).FirstOrDefault()?.PartnerNameVn;
                            _container = tranDetail.PackageContainer;
                            _cw = tranDetail.ChargeWeight;
                            _pcs = tranDetail.PackageQty;
                            _cbm = tranDetail.Cbm;
                        }
                    }
                }
                var shipmentAdvance = new InfoShipmentAdvanceExport
                {
                    JobNo = request.JobId,
                    CustomNo = request.CustomNo,
                    HBL = request.Hbl,
                    MBL = request.Mbl,
                    Customer = _customer,
                    Shipper = _shipper,
                    Consignee = _consignee,
                    Container = _container,
                    Cw = _cw,
                    Pcs = _pcs,
                    Cbm = _cbm,
                    NormAmount = advancePayment.AdvanceRequests
                                            .Where(x => x.JobId == request.JobId
                                                    && x.Hbl == request.Hbl
                                                    && x.AdvanceType == AccountingConstants.ADVANCE_TYPE_NORM)
                                            .Select(s => s.Amount).Sum() ?? 0,
                    InvoiceAmount = advancePayment.AdvanceRequests
                                            .Where(x => x.JobId == request.JobId
                                            && x.Hbl == request.Hbl
                                            && x.AdvanceType == AccountingConstants.ADVANCE_TYPE_INVOICE)
                                            .Select(s => s.Amount).Sum() ?? 0,
                    OtherAmount = advancePayment.AdvanceRequests
                                            .Where(x => x.JobId == request.JobId
                                            && x.Hbl == request.Hbl
                                            && x.AdvanceType == AccountingConstants.ADVANCE_TYPE_OTHER)
                                            .Select(s => s.Amount).Sum() ?? 0,
                };
                if (advancePayment.AdvanceCurrency != AccountingConstants.CURRENCY_LOCAL)
                {
                    //Tỉ giá quy đổi theo ngày đề nghị tạm ứng (RequestDate)
                    var currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeModified.Value.Date == advancePayment.RequestDate.Value.Date).ToList();
                    var _rate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, advancePayment.AdvanceCurrency, AccountingConstants.CURRENCY_LOCAL);
                    shipmentAdvance.NormAmount = shipmentAdvance.NormAmount * _rate;
                    shipmentAdvance.InvoiceAmount = shipmentAdvance.InvoiceAmount * _rate;
                    shipmentAdvance.OtherAmount = shipmentAdvance.OtherAmount * _rate;
                }
                shipmentsAdvance.Add(shipmentAdvance);
            }
            return shipmentsAdvance;
        }
        #endregion --- EXPORT ADVANCE ---

        public void UpdateStatusPaymentOfAdvanceRequest(string settlementCode)
        {
            var hblIdList = csShipmentSurchargeRepo.Get(x => x.SettlementCode == settlementCode).Select(s => s.Hblid).ToList();
            var hblNoList = new List<string>();
            //Find hblNo in operation
            hblNoList = opsTransactionRepo.Get(x => hblIdList.Contains(x.Hblid)).Select(s => s.Hwbno).ToList();
            if (hblNoList.Count == 0)
            {
                //Find hblNo documentation 
                hblNoList = csTransactionDetailRepo.Get(x => hblIdList.Contains(x.Id)).Select(s => s.Hwbno).ToList();
            }

            foreach (var hblNo in hblNoList)
            {
                var avdReq = acctAdvanceRequestRepo.Get(x => x.Hbl == hblNo).FirstOrDefault();
                if (avdReq != null)
                {
                    avdReq.StatusPayment = AccountingConstants.STATUS_PAYMENT_SETTLED;
                    var hsUpdateAdvReq = acctAdvanceRequestRepo.Update(avdReq, x => x.Id == avdReq.Id);
                }
            }
        }

        public HandleState UpdatePaymentVoucher(AcctAdvancePaymentModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = new HandleState();
                    foreach (var id in model.advancePaymentIds)
                    {
                        var advance = DataContext.Get(x => x.Id == new Guid(id)).FirstOrDefault();
                        advance.VoucherNo = model.VoucherNo;
                        advance.VoucherDate = model.VoucherDate;
                        advance.DeadlinePayment = model.VoucherDate.HasValue ? model.VoucherDate.Value.AddDays(14) : advance.DeadlinePayment;
                        hs = DataContext.Update(advance, x => x.Id == advance.Id);
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
        public HandleState Import(List<AccAdvancePaymentVoucherImportModel> data)
        {
            try
            {
                var lstAdvance = new List<AcctAdvancePayment>();
                foreach (var item in data)
                {
                    var advance = DataContext.Get(x => x.AdvanceNo == item.AdvanceNo).FirstOrDefault();
                    advance.VoucherNo = item.VoucherNo;
                    advance.VoucherDate = item.VoucherDate;
                    lstAdvance.Add(advance);
                }
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in lstAdvance)
                        {
                            item.DeadlinePayment = item.VoucherDate.Value.AddDays(14);
                            DataContext.Update(item, x => x.Id == item.Id);
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
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public List<AccAdvancePaymentVoucherImportModel> CheckValidImport(List<AccAdvancePaymentVoucherImportModel> list, bool validDate)
        {
            DateTime dt;
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.AdvanceNo))
                {
                    item.AdvanceNoError = stringLocalizer[AccountingLanguageSub.MSG_ADVANCE_NO_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    if (!DataContext.Any(x => x.AdvanceNo == item.AdvanceNo))
                    {
                        item.AdvanceNoError = stringLocalizer[AccountingLanguageSub.MSG_ADVANCE_NO_NOT_EXIST, item.AdvanceNo];
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.AdvanceNo?.ToLower() == item.AdvanceNo?.ToLower()) > 1)
                    {
                        item.AdvanceNoError = string.Format(stringLocalizer[AccountingLanguageSub.MSG_ADVANCE_NO_DUPLICATE], item.AdvanceNo);
                        item.IsValid = false;
                    }
                    if (DataContext.Any(x => x.AdvanceNo == item.AdvanceNo && x.StatusApproval != "Done"))
                    {
                        item.AdvanceNoError = string.Format(stringLocalizer[AccountingLanguageSub.MSG_ADVANCE_NO_NOT_APPROVAL], item.AdvanceNo);
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.VoucherNo))
                {
                    item.VoucherNoError = stringLocalizer[AccountingLanguageSub.MSG_VOUCHER_NO_EMPTY];
                    item.IsValid = false;
                }

                if (!item.VoucherDate.HasValue)
                {
                    item.VoucherDateError = stringLocalizer[AccountingLanguageSub.MSG_VOUCHER_DATE_EMPTY];
                    item.IsValid = false;
                }

            });
            return list;
        }
    }
}
