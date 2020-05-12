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
            ICurrencyExchangeService currencyExchange,
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
        }

        public List<AcctAdvancePaymentResult> Paging(AcctAdvancePaymentCriteria criteria, int page, int size, out int rowsCount)
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

        private IQueryable<AcctAdvancePayment> GetAdvancesPermission()
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctAP);
            PermissionRange _permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (_permissionRange == PermissionRange.None) return null;

            IQueryable<AcctAdvancePayment> advances = null;
            switch (_permissionRange)
            {
                case PermissionRange.None:
                    break;
                case PermissionRange.All:
                    advances = DataContext.Get();
                    break;
                case PermissionRange.Owner:
                    advances = DataContext.Get(x => x.UserCreated == _user.UserID);
                    break;
                case PermissionRange.Group:
                    advances = DataContext.Get(x => x.GroupId == _user.GroupId
                                                 && x.DepartmentId == _user.DepartmentId
                                                 && x.OfficeId == _user.OfficeID
                                                 && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Department:
                    advances = DataContext.Get(x => x.DepartmentId == _user.DepartmentId
                                                 && x.OfficeId == _user.OfficeID
                                                 && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Office:
                    advances = DataContext.Get(x => x.OfficeId == _user.OfficeID
                                                 && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Company:
                    advances = DataContext.Get(x => x.CompanyId == _user.CompanyID);
                    break;
            }
            return advances;
        }

        private IQueryable<AcctAdvancePaymentResult> GetDatas(AcctAdvancePaymentCriteria criteria, IQueryable<AcctAdvancePayment> advances)
        {
            if (advances == null) return null;
            var request = acctAdvanceRequestRepo.Get();
            var approveAdvance = acctApproveAdvanceRepo.Get(x => x.IsDeputy == false);
            var user = sysUserRepo.Get();

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
                refNo = (from ad in advances
                         join re in request on ad.AdvanceNo equals re.AdvanceNo into re2
                         from re in re2.DefaultIfEmpty()
                         where
                         (
                             criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ?
                             (
                                 (
                                        (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.AdvanceNo) : true)
                                     || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.Hbl) : true)
                                     || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.Mbl) : true)
                                     || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.CustomNo) : true)
                                     || (criteria.ReferenceNos != null ? criteria.ReferenceNos.Contains(re.JobId) : true)
                                 )
                             )
                             :
                             (
                                 true
                             )
                          )
                         select ad.AdvanceNo).ToList();
            }

            var data = from ad in advances
                       join u in user on ad.Requester equals u.Id into u2
                       from u in u2.DefaultIfEmpty()
                       join re in request on ad.AdvanceNo equals re.AdvanceNo into re2
                       from re in re2.DefaultIfEmpty()
                       join apr in approveAdvance on ad.AdvanceNo equals apr.AdvanceNo into apr2
                       from apr in apr2.DefaultIfEmpty()
                       where
                         (
                            criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0 ? refNo.Contains(ad.AdvanceNo) : true
                         )
                         &&
                         (
                            !string.IsNullOrEmpty(criteria.Requester) ?
                            (
                                    (ad.Requester == criteria.Requester && currentUser.GroupId != 11)
                                || (currentUser.GroupId == 11
                                    && userBaseService.CheckIsAccountantDept(currentUser.DepartmentId) == false
                                    && apr.Manager == criteria.Requester
                                    && (ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED))
                                || (currentUser.GroupId == 11
                                    && userBaseService.CheckIsAccountantDept(currentUser.DepartmentId) == true
                                    && apr.Accountant == criteria.Requester
                                    && (ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED && ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL))
                                || (currentUser.GroupId == 11
                                    && apr.ManagerApr == criteria.Requester
                                    && (ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED))
                                || (apr.AccountantApr == criteria.Requester
                                    && (ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED && ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL))
                                || (isManagerDeputy && (ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED))
                                || (isAccountantDeputy && (ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED && ad.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL))
                            )
                            :
                                true
                         )
                         &&
                         (
                            criteria.RequestDateFrom.HasValue && criteria.RequestDateTo.HasValue ?
                                //Convert RequestDate về date nếu RequestDate có value
                                ad.RequestDate.Value.Date >= (criteria.RequestDateFrom.HasValue ? criteria.RequestDateFrom.Value.Date : criteria.RequestDateFrom)
                                && ad.RequestDate.Value.Date <= (criteria.RequestDateTo.HasValue ? criteria.RequestDateTo.Value.Date : criteria.RequestDateTo)
                            :
                                true
                         )
                         &&
                         (
                            !string.IsNullOrEmpty(criteria.StatusApproval) && !criteria.StatusApproval.Equals("All") ?
                                ad.StatusApproval == criteria.StatusApproval
                            :
                                true
                         )
                         &&
                         (
                            criteria.AdvanceModifiedDateFrom.HasValue && criteria.AdvanceModifiedDateTo.HasValue ?
                                //Convert DatetimeModified về date nếu DatetimeModified có value
                                ad.DatetimeModified.Value.Date >= (criteria.AdvanceModifiedDateFrom.HasValue ? criteria.AdvanceModifiedDateFrom.Value.Date : criteria.AdvanceModifiedDateFrom)
                                && ad.DatetimeModified.Value.Date <= (criteria.AdvanceModifiedDateTo.HasValue ? criteria.AdvanceModifiedDateTo.Value.Date : criteria.AdvanceModifiedDateTo)
                            :
                                true
                         )
                         &&
                         (
                           !string.IsNullOrEmpty(criteria.StatusPayment) && !criteria.StatusPayment.Equals("All") ?
                                re.StatusPayment == criteria.StatusPayment
                           :
                                true
                         )
                         &&
                         (
                           !string.IsNullOrEmpty(criteria.PaymentMethod) && !criteria.PaymentMethod.Equals("All") ?
                                ad.PaymentMethod == criteria.PaymentMethod
                           :
                                true
                          )
                          &&
                         (
                           !string.IsNullOrEmpty(criteria.CurrencyID) && !criteria.CurrencyID.Equals("All") ?
                                ad.AdvanceCurrency == criteria.CurrencyID
                           :
                                true
                          )

                       select new AcctAdvancePaymentResult
                       {
                           Id = ad.Id,
                           AdvanceNo = ad.AdvanceNo,
                           AdvanceNote = ad.AdvanceNote,
                           AdvanceCurrency = ad.AdvanceCurrency,
                           Requester = ad.Requester,
                           RequesterName = u.Username,
                           RequestDate = ad.RequestDate,
                           DeadlinePayment = ad.DeadlinePayment,
                           UserCreated = ad.UserCreated,
                           DatetimeCreated = ad.DatetimeCreated,
                           UserModified = ad.UserModified,
                           DatetimeModified = ad.DatetimeModified,
                           StatusApproval = ad.StatusApproval,
                           PaymentMethod = ad.PaymentMethod,
                           Amount = re.Amount
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
                x.PaymentMethod
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

        private IQueryable<AcctAdvancePaymentResult> QueryDataPermission(AcctAdvancePaymentCriteria criteria)
        {
            var advances = GetAdvancesPermission();
            var data = GetDatas(criteria, advances);
            return data;
        }

        public IQueryable<AcctAdvancePaymentResult> QueryData(AcctAdvancePaymentCriteria criteria)
        {
            var advances = GetAdvancesPermission();
            var data = GetDatas(criteria, advances);
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
                                var advRequestDelete = acctAdvanceRequestRepo.Delete(x => x.Id == item.Id);
                            }
                        }
                        var advance = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault();
                        if (advance == null) return new HandleState("Not found Advance Payment No: " + advanceNo);
                        if (advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                            && advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED)
                        {
                            return new HandleState("Not allow delete. Advance Payment are awaiting approval.");
                        }
                        var hs = DataContext.Delete(x => x.Id == advance.Id);
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

            return advanceModel;
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

        #region PREVIEW ADVANCE PAYMENT
        public Crystal Preview(Guid advanceId)
        {
            Crystal result = null;
            string strJobId = string.Empty;
            string strHbl = string.Empty;
            string strCustomNo = string.Empty;
            int contQty = 0;
            decimal nw = 0;
            int psc = 0;
            decimal cbm = 0;

            var advance = GetAdvancePaymentByAdvanceId(advanceId);

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
            var approveAdvance = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeputy == false).FirstOrDefault();
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
                AdvContact = requesterName,//cần lấy ra username
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
            var approveAdvance = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeputy == false).FirstOrDefault();
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
        public HandleState CheckExistsInfoManagerOfRequester(AcctApproveAdvanceModel approve)
        {
            var userCurrent = currentUser.UserID;

            var acctApprove = mapper.Map<AcctApproveAdvance>(approve);
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

        //Insert Or Update AcctApproveAdvance by AdvanceNo
        public HandleState InsertOrUpdateApprovalAdvance(AcctApproveAdvanceModel approve)
        {
            try
            {
                var userCurrent = currentUser.UserID;
                var acctApprove = mapper.Map<AcctApproveAdvance>(approve);

                if (!string.IsNullOrEmpty(approve.AdvanceNo))
                {
                    var advance = DataContext.Get(x => x.AdvanceNo == approve.AdvanceNo).FirstOrDefault();
                    if (advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW
                        && advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED
                        && advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_DONE
                        && advance.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)
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
                        acctApprove.Buhead = userBaseService.GetBUHeadId(brandOfUser.ToString());//Lấy ra BuHead dựa vào Brand của userCurrent

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

                        if (string.IsNullOrEmpty(emailLeaderOrManager)) return new HandleState("Not found email Leader or Manager of user");

                        var sendMailResult = SendMailSuggestApproval(acctApprove.AdvanceNo, userLeaderOrManager, emailLeaderOrManager, usersDeputy);

                        if (sendMailResult)
                        {
                            var checkExistsApproveByAdvanceNo = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == acctApprove.AdvanceNo && x.IsDeputy == false).FirstOrDefault();
                            if (checkExistsApproveByAdvanceNo == null) //Insert AcctApproveAdvance
                            {
                                acctApprove.Id = Guid.NewGuid();
                                acctApprove.RequesterAprDate = DateTime.Now;
                                acctApprove.UserCreated = acctApprove.UserModified = userCurrent;
                                acctApprove.DateCreated = acctApprove.DateModified = DateTime.Now;
                                acctApprove.IsDeputy = false;
                                var hsAddApproveAdvance = acctApproveAdvanceRepo.Add(acctApprove);
                            }
                            else //Update AcctApproveAdvance by AdvanceNo
                            {
                                checkExistsApproveByAdvanceNo.RequesterAprDate = DateTime.Now;
                                checkExistsApproveByAdvanceNo.UserModified = userCurrent;
                                checkExistsApproveByAdvanceNo.DateModified = DateTime.Now;
                                var hsUpdateApproveAdvance = acctApproveAdvanceRepo.Update(checkExistsApproveByAdvanceNo, x => x.Id == checkExistsApproveByAdvanceNo.Id);
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

        //Check group trước đó đã được approve hay chưa? Nếu group trước đó đã approve thì group hiện tại mới được Approve
        //Nếu group hiện tại đã được approve thì không cho approve nữa
        private HandleState CheckApprovedOfDeptPrevAndDeptCurrent(string advanceNo, ICurrentUser _userCurrent, string deptOfUser)
        {
            HandleState result = new HandleState("Not allow approve/deny");

            //Lấy ra Advance Approval dựa vào advanceNo
            var acctApprove = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advanceNo && x.IsDeputy == false).FirstOrDefault();
            if (acctApprove == null)
            {
                result = new HandleState("Not found advance approval by AdvanceNo is " + advanceNo);
                return result;
            }

            //Lấy ra Advance Payment dựa vào advanceNo
            var advance = DataContext.Get(x => x.AdvanceNo == advanceNo).FirstOrDefault();
            if (advance == null)
            {
                result = new HandleState("Not found advance payment by AdvanceNo is" + advanceNo);
                return result;
            }

            //Lấy ra brandId của user requester
            var brandOfUserRequest = advance.OfficeId;
            if (brandOfUserRequest == Guid.Empty || brandOfUserRequest == null) return new HandleState("Not found office of user requester");

            //Lấy ra brandId của userId
            var brandOfUserId = _userCurrent.OfficeID;
            if (brandOfUserId == Guid.Empty || brandOfUserId == null) return new HandleState("Not found office of user");

            //Trường hợp không có Leader
            if (string.IsNullOrEmpty(acctApprove.Leader))
            {
                //Manager Department Approve
                var managerOfUserRequester = userBaseService.GetDeptManager(advance.CompanyId, advance.OfficeId, advance.DepartmentId).FirstOrDefault();
                //Kiểm tra user có phải là dept manager hoặc có phải là user được ủy quyền duyệt (Manager Dept) hay không
                if (_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId) == false
                    && (_userCurrent.UserID == managerOfUserRequester
                        || userBaseService.CheckDeputyManagerByUser(_userCurrent.DepartmentId, _userCurrent.UserID)))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (_userCurrent.CompanyID == advance.CompanyId
                       && _userCurrent.OfficeID == advance.OfficeId
                       && _userCurrent.DepartmentId != advance.DepartmentId)
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
                        if (advance.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
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
                var accountantOfUser = userBaseService.GetAccoutantManager(advance.CompanyId, advance.OfficeId).FirstOrDefault();
                //Kiểm tra user có phải là Accountant Manager hoặc có phải là user được ủy quyền duyệt (Accoutant) hay không
                if (_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId)
                    && (_userCurrent.UserID == accountantOfUser
                        || userBaseService.CheckDeputyAccountantByUser(_userCurrent.DepartmentId, _userCurrent.UserID)))
                {
                    //Check group DepartmentManager đã được Approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Manager)
                        && advance.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                        && acctApprove.ManagerAprDate != null
                        && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                    {
                        result = new HandleState();
                        //Check group Accountant đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (advance.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DONE)
                            && acctApprove.AccountantAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.AccountantApr))
                        {
                            result = new HandleState("Accountant approved");
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
                if (_userCurrent.GroupId != AccountingConstants.SpecialGroup
                    && _userCurrent.UserID == userBaseService.GetLeaderIdOfUser(advance.Requester))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (_userCurrent.CompanyID == advance.CompanyId
                       && _userCurrent.OfficeID == advance.OfficeId
                       && _userCurrent.DepartmentId != advance.DepartmentId)
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
                        if (advance.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED)
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
                var managerOfUserRequester = userBaseService.GetDeptManager(advance.CompanyId, advance.OfficeId, advance.DepartmentId).FirstOrDefault();
                //Kiểm tra user có phải là dept manager hoặc có phải là user được ủy quyền duyệt (Manager Dept) hay không
                if (_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId) == false
                    && (_userCurrent.UserID == managerOfUserRequester
                        || userBaseService.CheckDeputyManagerByUser(_userCurrent.DepartmentId, _userCurrent.UserID)))
                {
                    //Kiểm tra User Approve có thuộc cùng dept với User Requester hay
                    //Nếu không cùng thì không cho phép Approve (đối với Dept Manager)
                    if (_userCurrent.CompanyID == advance.CompanyId
                       && _userCurrent.OfficeID == advance.OfficeId
                       && _userCurrent.DepartmentId != advance.DepartmentId)
                    {
                        result = new HandleState("Not in the same department");
                    }
                    else
                    {
                        result = new HandleState();
                    }

                    //Check group Leader đã được approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Leader)
                        && advance.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED)
                        && acctApprove.LeaderAprDate != null)
                    {
                        result = new HandleState();
                        //Check group Manager Department đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (advance.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                            && acctApprove.ManagerAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                        {
                            result = new HandleState("Manager Department approved");
                        }
                    }
                    else
                    {
                        result = new HandleState("Not found Leader or Leader not approve");
                    }

                }

                //Accountant Approve
                var accountantOfUser = userBaseService.GetAccoutantManager(advance.CompanyId, advance.OfficeId).FirstOrDefault();
                //Kiểm tra user có phải là Accountant Manager hoặc có phải là user được ủy quyền duyệt (Accoutant) hay không
                if (_userCurrent.GroupId == AccountingConstants.SpecialGroup
                    && userBaseService.CheckIsAccountantDept(_userCurrent.DepartmentId)
                    && (_userCurrent.UserID == accountantOfUser
                        || userBaseService.CheckDeputyAccountantByUser(_userCurrent.DepartmentId, _userCurrent.UserID)))
                {
                    //Check group DepartmentManager đã được Approve chưa
                    if (!string.IsNullOrEmpty(acctApprove.Manager)
                        && advance.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED)
                        && acctApprove.ManagerAprDate != null
                        && !string.IsNullOrEmpty(acctApprove.ManagerApr))
                    {
                        result = new HandleState();
                        //Check group Accountant đã approve chưa
                        //Nếu đã approve thì không được approve nữa
                        if (advance.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DONE)
                            && acctApprove.AccountantAprDate != null
                            && !string.IsNullOrEmpty(acctApprove.AccountantApr))
                        {
                            result = new HandleState("Accountant approved");
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

        //Update Approval cho từng group
        public HandleState UpdateApproval(Guid advanceId)
        {
            var userCurrent = currentUser.UserID;

            var userAprNext = string.Empty;
            var emailUserAprNext = string.Empty;
            List<string> usersDeputy = new List<string>();

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var advance = DataContext.Get(x => x.Id == advanceId).FirstOrDefault();

                    if (advance == null) return new HandleState("Not found Advance Payment");

                    var approve = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeputy == false).FirstOrDefault();

                    if (approve == null)
                    {
                        if (acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo).Select(s => s.Id).Any() == false)
                        {
                            return new HandleState("Not found Advance Approval by AdvanceNo is " + advance.AdvanceNo);
                        }
                        else
                        {
                            return new HandleState("Not allow approve");
                        }
                    }

                    //Lấy ra brandId của user requester
                    var brandOfUserRequest = advance.OfficeId;
                    if (brandOfUserRequest == Guid.Empty || brandOfUserRequest == null) return new HandleState("Not found office of user requester");

                    //Lấy ra brandId của userId
                    var brandOfUserId = currentUser.OfficeID;
                    if (brandOfUserId == Guid.Empty || brandOfUserId == null) return new HandleState("Not found office of user");

                    //Lấy ra dept code của userApprove dựa vào userApprove
                    var deptCodeOfUser = userBaseService.GetInfoDeptOfUser(currentUser.DepartmentId)?.Code;
                    if (string.IsNullOrEmpty(deptCodeOfUser)) return new HandleState("Not found department of user");

                    //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
                    var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(advance.AdvanceNo, currentUser, deptCodeOfUser);
                    if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
                    if (approve != null && advance != null)
                    {
                        if (userCurrent == userBaseService.GetLeaderIdOfUser(advance.Requester))
                        {
                            if (advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED
                                || advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED
                                || advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                                || advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                            {
                                return new HandleState("Leader approved");
                            }
                            advance.StatusApproval = AccountingConstants.STATUS_APPROVAL_LEADERAPPROVED;
                            approve.LeaderAprDate = DateTime.Now;//Cập nhật ngày Approve của Leader

                            //Lấy email của Department Manager
                            userAprNext = userBaseService.GetDeptManager(advance.CompanyId, advance.OfficeId, advance.DepartmentId).FirstOrDefault();
                            var userAprNextId = userBaseService.GetEmployeeIdOfUser(userAprNext);
                            emailUserAprNext = userBaseService.GetEmployeeByEmployeeId(userAprNextId)?.Email;

                            var deptCodeRequester = userBaseService.GetInfoDeptOfUser(advance.DepartmentId)?.Code;
                            usersDeputy = userBaseService.GetListUserDeputyByDept(deptCodeRequester);
                        }
                        else if (userBaseService.CheckIsAccountantDept(currentUser.DepartmentId) == false
                                && (userCurrent == userBaseService.GetDeptManager(advance.CompanyId, advance.OfficeId, advance.DepartmentId).FirstOrDefault()
                                    || userBaseService.GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent)))
                        {
                            if (advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED
                                || advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                                || advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                            {
                                return new HandleState("Manager department approved");
                            }
                            advance.StatusApproval = AccountingConstants.STATUS_APPROVAL_DEPARTMENTAPPROVED;
                            approve.ManagerAprDate = DateTime.Now;//Cập nhật ngày Approve của Manager
                            approve.ManagerApr = userCurrent;

                            //Lấy email của Accountant Manager
                            userAprNext = userBaseService.GetAccoutantManager(advance.CompanyId, advance.OfficeId).FirstOrDefault();
                            var userAprNextId = userBaseService.GetEmployeeIdOfUser(userAprNext);
                            emailUserAprNext = userBaseService.GetEmployeeByEmployeeId(userAprNextId)?.Email;

                            //var deptCodeAccountant = GetInfoDeptOfUser(AccountingConstants.AccountantDeptId)?.Code;
                            //usersDeputy = GetListUserDeputyByDept(deptCodeAccountant);
                        }
                        else if (
                            userBaseService.CheckIsAccountantDept(currentUser.DepartmentId)
                            && (userCurrent == userBaseService.GetAccoutantManager(advance.CompanyId, advance.OfficeId).FirstOrDefault()//GetAccountantId(brandOfUserId.ToString()) 
                                || userBaseService.GetListUserDeputyByDept(deptCodeOfUser).Contains(userCurrent)))
                        {
                            if (advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                            {
                                return new HandleState("Chief accountant approved");
                            }
                            advance.StatusApproval = AccountingConstants.STATUS_APPROVAL_DONE;
                            approve.AccountantAprDate = approve.BuheadAprDate = DateTime.Now;//Cập nhật ngày Approve của Accountant & BUHead
                            approve.AccountantApr = userCurrent;
                            approve.BuheadApr = approve.Buhead;

                            //Send mail approval success when Accountant approved, mail send to requester
                            SendMailApproved(advance.AdvanceNo, DateTime.Now);
                        }

                        advance.UserModified = approve.UserModified = userCurrent;
                        advance.DatetimeModified = approve.DateModified = DateTime.Now;

                        var hsUpdateAdvance = DataContext.Update(advance, x => x.Id == advance.Id);
                        var hsUpdateApprove = acctApproveAdvanceRepo.Update(approve, x => x.Id == approve.Id);
                        trans.Commit();
                    }

                    //Nếu currentUser là Accoutant of Requester thì return
                    if (userBaseService.CheckIsAccountantDept(currentUser.DepartmentId)
                        && userCurrent == userBaseService.GetAccoutantManager(advance.CompanyId, advance.OfficeId).FirstOrDefault())
                    {
                        return new HandleState();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(emailUserAprNext)) return new HandleState("Not found email of user " + userAprNext);

                        //Send mail đề nghị approve
                        var sendMailResult = SendMailSuggestApproval(advance.AdvanceNo, userAprNext, emailUserAprNext, usersDeputy);

                        return sendMailResult ? new HandleState() : new HandleState("Send mail suggest Approval failed");
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

        public HandleState DeniedApprove(Guid advanceId, string comment)
        {
            var userCurrent = currentUser.UserID;

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var advance = DataContext.Get(x => x.Id == advanceId).FirstOrDefault();

                    if (advance == null) return new HandleState("Not found Advance Payment");

                    var approve = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo && x.IsDeputy == false).FirstOrDefault();
                    if (approve == null)
                    {
                        if (acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advance.AdvanceNo).Select(s => s.Id).Any() == false)
                        {
                            return new HandleState("Not found Approve Advance by advanceNo " + advance.AdvanceNo);
                        }
                        else
                        {
                            return new HandleState("Not allow deny");
                        }
                    }

                    //Lấy ra brandId của user requester
                    var brandOfUserRequest = advance.OfficeId;
                    if (brandOfUserRequest == Guid.Empty || brandOfUserRequest == null) return new HandleState("Not found office of user requester");

                    //Lấy ra brandId của userId
                    var brandOfUserId = currentUser.OfficeID;
                    if (brandOfUserId == Guid.Empty || brandOfUserId == null) return new HandleState("Not found office of user");

                    //Lấy ra dept code của userApprove dựa vào userApprove
                    var deptCodeOfUser = userBaseService.GetInfoDeptOfUser(currentUser.DepartmentId)?.Code;
                    if (string.IsNullOrEmpty(deptCodeOfUser)) return new HandleState("Not found department of user");

                    //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
                    //var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(advance.AdvanceNo, userCurrent, deptCodeOfUser);
                    //if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
                    if (approve != null && advance != null)
                    {
                        if (advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_DENIED)
                        {
                            return new HandleState("Advance payment has been denied");
                        }

                        if (userCurrent == userBaseService.GetLeaderIdOfUser(advance.Requester))
                        {
                            //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
                            var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(advance.AdvanceNo, currentUser, deptCodeOfUser);
                            if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
                            approve.LeaderAprDate = DateTime.Now;//Cập nhật ngày Denie của Leader
                        }
                        else if (currentUser.GroupId == AccountingConstants.SpecialGroup
                            && userBaseService.CheckIsAccountantDept(currentUser.DepartmentId) == false
                            && (userCurrent == userBaseService.GetDeptManager(advance.CompanyId, advance.OfficeId, advance.DepartmentId).FirstOrDefault()
                                || userBaseService.CheckDeputyManagerByUser(currentUser.DepartmentId, currentUser.UserID)))
                        {
                            //Cho phép User Manager thực hiện deny khi user Manager đã Approved, 
                            //nếu Chief Accountant đã Approved thì User Manager ko được phép deny
                            if (advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED || advance.StatusApproval == AccountingConstants.STATUS_APPROVAL_DONE)
                            {
                                return new HandleState("Not allow deny. Advance payment has been approved");
                            }

                            approve.ManagerAprDate = DateTime.Now;//Cập nhật ngày Denie của Manager
                            approve.ManagerApr = userCurrent; //Cập nhật user manager denie                   
                        }
                        else if (currentUser.GroupId == AccountingConstants.SpecialGroup
                            && userBaseService.CheckIsAccountantDept(currentUser.DepartmentId)
                            && (userCurrent == userBaseService.GetAccoutantManager(advance.CompanyId, advance.OfficeId).FirstOrDefault()
                                || userBaseService.CheckDeputyAccountantByUser(currentUser.DepartmentId, currentUser.UserID)))
                        {
                            //Kiểm tra group trước đó đã được approve chưa và group của userApprove đã được approve chưa
                            var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(advance.AdvanceNo, currentUser, deptCodeOfUser);
                            if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
                            approve.AccountantAprDate = DateTime.Now;//Cập nhật ngày Denie của Accountant
                            approve.AccountantApr = userCurrent; //Cập nhật user accountant denie
                        }
                        else
                        {
                            var checkApr = CheckApprovedOfDeptPrevAndDeptCurrent(advance.AdvanceNo, currentUser, deptCodeOfUser);
                            if (checkApr.Success == false) return new HandleState(checkApr.Exception.Message);
                        }

                        approve.UserModified = userCurrent;
                        approve.DateModified = DateTime.Now;
                        approve.Comment = comment;
                        approve.IsDeputy = true;
                        var hsUpdateApproveAdvance = acctApproveAdvanceRepo.Update(approve, x => x.Id == approve.Id);

                        //Cập nhật lại advance status của Advance Payment
                        advance.StatusApproval = AccountingConstants.STATUS_APPROVAL_DENIED;
                        advance.UserModified = userCurrent;
                        advance.DatetimeModified = DateTime.Now;
                        var hsUpdateAdvancePayment = DataContext.Update(advance, x => x.Id == advance.Id);

                        trans.Commit();
                    }

                    //Send mail denied approval
                    var sendMailResult = SendMailDeniedApproval(advance.AdvanceNo, comment, DateTime.Now);
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

        //Kiểm tra User đăng nhập vào có thuộc các user Approve Advance không, nếu không thuộc bất kỳ 1 user nào thì gán cờ IsApproved bằng true
        //Kiểm tra xem dept đã approve chưa, nếu dept của user đó đã approve thì gán cờ IsApproved bằng true
        private bool CheckUserInApproveAdvanceAndDeptApproved(ICurrentUser userCurrent, AcctApproveAdvanceModel approveAdvance)
        {
            var isApproved = false;
            var isDeputyManage = userBaseService.CheckDeputyManagerByUser(userCurrent.DepartmentId, userCurrent.UserID);
            var isDeputyAccoutant = userBaseService.CheckDeputyAccountantByUser(userCurrent.DepartmentId, userCurrent.UserID);

            // 1 user vừa có thể là Requester, Manager Dept, Accountant Dept nên khi check Approved cần phải dựa vào group
            // Group 11 chính là group Manager

            if (userCurrent.GroupId != AccountingConstants.SpecialGroup
                && userCurrent.UserID == approveAdvance.Requester) //Requester
            {
                isApproved = true;
                if (approveAdvance.RequesterAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.GroupId != AccountingConstants.SpecialGroup
                && userCurrent.UserID == approveAdvance.Leader) //Leader
            {
                isApproved = true;
                if (approveAdvance.LeaderAprDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.GroupId == AccountingConstants.SpecialGroup
                && userBaseService.CheckIsAccountantDept(userCurrent.DepartmentId) == false
                && (userCurrent.UserID == approveAdvance.Manager
                    || userCurrent.UserID == approveAdvance.ManagerApr
                    || isDeputyManage)) //Dept Manager
            {
                isApproved = true;
                var isDeptWaitingApprove = DataContext.Get(x => x.AdvanceNo == approveAdvance.AdvanceNo && (x.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && x.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED)).Any();
                if (string.IsNullOrEmpty(approveAdvance.ManagerApr) && approveAdvance.ManagerAprDate == null && isDeptWaitingApprove)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.GroupId == AccountingConstants.SpecialGroup
                && userBaseService.CheckIsAccountantDept(userCurrent.DepartmentId)
                && (userCurrent.UserID == approveAdvance.Accountant
                    || userCurrent.UserID == approveAdvance.AccountantApr
                    || isDeputyAccoutant)) //Accountant Manager
            {
                isApproved = true;
                var isDeptWaitingApprove = DataContext.Get(x => x.AdvanceNo == approveAdvance.AdvanceNo && (x.StatusApproval != AccountingConstants.STATUS_APPROVAL_NEW && x.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED && x.StatusApproval != AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)).Any();
                if (string.IsNullOrEmpty(approveAdvance.AccountantApr) && approveAdvance.AccountantAprDate == null && isDeptWaitingApprove)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.UserID == approveAdvance.Buhead || userCurrent.UserID == approveAdvance.BuheadApr) //BUHead
            {
                isApproved = true;
                if (string.IsNullOrEmpty(approveAdvance.BuheadApr) && approveAdvance.BuheadAprDate == null)
                {
                    isApproved = false;
                }
            }
            else
            {
                //Đây là trường hợp các User không thuộc Approve Advance
                isApproved = true;
            }
            return isApproved;
        }

        public AcctApproveAdvanceModel GetInfoApproveAdvanceByAdvanceNo(string advanceNo)
        {
            var userCurrent = currentUser.UserID;

            var approveAdvance = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advanceNo && x.IsDeputy == false).FirstOrDefault();
            var aprAdvanceMap = new AcctApproveAdvanceModel();

            if (approveAdvance != null)
            {
                aprAdvanceMap = mapper.Map<AcctApproveAdvanceModel>(approveAdvance);

                //Kiểm tra User đăng nhập vào có thuộc các user Approve Advance không, nếu không thuộc bất kỳ 1 user nào thì gán cờ IsApproved bằng true
                //Kiểm tra xem dept đã approve chưa, nếu dept của user đó đã approve thì gán cờ IsApproved bằng true           
                aprAdvanceMap.IsApproved = CheckUserInApproveAdvanceAndDeptApproved(currentUser, aprAdvanceMap);
                aprAdvanceMap.RequesterName = string.IsNullOrEmpty(aprAdvanceMap.Requester) ? null : userBaseService.GetEmployeeByUserId(aprAdvanceMap.Requester)?.EmployeeNameVn;
                aprAdvanceMap.LeaderName = string.IsNullOrEmpty(aprAdvanceMap.Leader) ? null : userBaseService.GetEmployeeByUserId(aprAdvanceMap.Leader)?.EmployeeNameVn;
                aprAdvanceMap.ManagerName = string.IsNullOrEmpty(aprAdvanceMap.Manager) ? null : userBaseService.GetEmployeeByUserId(aprAdvanceMap.Manager)?.EmployeeNameVn;
                aprAdvanceMap.AccountantName = string.IsNullOrEmpty(aprAdvanceMap.Accountant) ? null : userBaseService.GetEmployeeByUserId(aprAdvanceMap.Accountant)?.EmployeeNameVn;
                aprAdvanceMap.BUHeadName = string.IsNullOrEmpty(aprAdvanceMap.Buhead) ? null : userBaseService.GetEmployeeByUserId(aprAdvanceMap.Buhead)?.EmployeeNameVn;
                aprAdvanceMap.StatusApproval = DataContext.Get(x => x.AdvanceNo == approveAdvance.AdvanceNo).FirstOrDefault()?.StatusApproval;

                var isManagerDeputy = userBaseService.CheckDeputyManagerByUser(currentUser.DepartmentId, currentUser.UserID);
                aprAdvanceMap.IsManager = currentUser.GroupId == AccountingConstants.SpecialGroup && (userCurrent == approveAdvance.Manager || userCurrent == approveAdvance.ManagerApr || isManagerDeputy) ? true : false;
            }
            else
            {
                //Mặc định nếu chưa send request thì gán IsApproved bằng true (nhằm để disable button Approve & Deny)
                aprAdvanceMap.IsApproved = true;
            }
            return aprAdvanceMap;
        }

        #endregion APPROVAL ADVANCE PAYMENT

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

            //Get advance theo shipment và advance chưa làm settlement
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
                        if (payment.StatusApproval != AccountingConstants.STATUS_APPROVAL_DENIED)
                        {
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
                                    approve.IsDeputy = true;
                                    approve.UserModified = currentUser.UserID;
                                    approve.DateModified = DateTime.Now;
                                    acctApproveAdvanceRepo.Update(approve, x => x.Id == approve.Id);
                                }
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
            var _advanceApprove = acctApproveAdvanceRepo.Get(x => x.AdvanceNo == advancePayment.AdvanceNo && x.IsDeputy == false).FirstOrDefault();
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
    }
}
