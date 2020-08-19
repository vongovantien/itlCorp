using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Setting.DL.Services
{
    public class UnlockRequestService : RepositoryBase<SetUnlockRequest, SetUnlockRequestModel>, IUnlockRequestService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SetUnlockRequestJob> setUnlockRequestJobRepo;
        private readonly IContextBase<SetUnlockRequestApprove> setUnlockRequestApproveRepo;
        private readonly IContextBase<AcctAdvancePayment> advancePaymentRepo;
        private readonly IContextBase<AcctSettlementPayment> settlementPaymentRepo;
        private readonly IContextBase<OpsTransaction> opsTransactionRepo;
        private readonly IContextBase<CsTransaction> transRepo;
        private readonly IContextBase<CsTransactionDetail> transDetailRepo;
        private readonly IContextBase<CustomsDeclaration> customsRepo;
        private readonly IContextBase<SysUser> userRepo;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepo;
        private readonly IUnlockRequestApproveService unlockRequestApproveService;
        readonly IUserBaseService userBaseService;

        public UnlockRequestService(
            IContextBase<SetUnlockRequest> repository,
            IMapper mapper,
            ICurrentUser user,
            IContextBase<SetUnlockRequestJob> setUnlockRequestJob,
            IContextBase<SetUnlockRequestApprove> setUnlockRequestApprove,
            IContextBase<AcctAdvancePayment> advancePayment,
            IContextBase<AcctSettlementPayment> settlementPayment,
            IContextBase<OpsTransaction> opsTransaction,
            IContextBase<CsTransaction> trans,
            IContextBase<CsTransactionDetail> transDetail,
            IContextBase<CustomsDeclaration> customs,
            IContextBase<SysUser> sysUser,
            IContextBase<CsShipmentSurcharge> surcharge,
            IUnlockRequestApproveService unlockRequestApprove,
            IUserBaseService userBase) : base(repository, mapper)
        {
            currentUser = user;
            setUnlockRequestJobRepo = setUnlockRequestJob;
            setUnlockRequestApproveRepo = setUnlockRequestApprove;
            advancePaymentRepo = advancePayment;
            settlementPaymentRepo = settlementPayment;
            opsTransactionRepo = opsTransaction;
            transRepo = trans;
            transDetailRepo = transDetail;
            customsRepo = customs;
            userRepo = sysUser;
            surchargeRepo = surcharge;
            unlockRequestApproveService = unlockRequestApprove;
            userBaseService = userBase;
        }

        #region --- GET SHIPMENT, ADVANCE, SETTLEMENT TO UNLOCK REQUEST ---
        private List<SetUnlockRequestJobModel> GetAdvance(UnlockJobCriteria criteria)
        {
            //Only Advance Payment has Status Approval is DONE
            var result = new List<SetUnlockRequestJobModel>();
            if (criteria.Advances == null || criteria.Advances.Count == 0) return result;
            var data = advancePaymentRepo.Get(x => criteria.Advances.Where(w => x.StatusApproval == SettingConstants.STATUS_APPROVAL_DONE && !string.IsNullOrEmpty(w)).Contains(x.AdvanceNo)).Select(s => new SetUnlockRequestJobModel()
            {
                UnlockName = s.AdvanceNo,
                Job = s.AdvanceNo,
                UnlockType = UnlockTypeEx.GetUnlockType(criteria.UnlockTypeNum)
            });
            return data.ToList();
        }

        private List<SetUnlockRequestJobModel> GetSettlement(UnlockJobCriteria criteria)
        {
            //Only Settlement Payment has Status Approval is DONE
            var result = new List<SetUnlockRequestJobModel>();
            if (criteria.Settlements == null || criteria.Settlements.Count == 0) return result;
            var data = settlementPaymentRepo.Get(x => criteria.Settlements.Where(w => x.StatusApproval == SettingConstants.STATUS_APPROVAL_DONE && !string.IsNullOrEmpty(w)).Contains(x.SettlementNo)).Select(s => new SetUnlockRequestJobModel()
            {
                UnlockName = s.SettlementNo,
                Job = s.SettlementNo,
                UnlockType = UnlockTypeEx.GetUnlockType(criteria.UnlockTypeNum)
            });
            return data.ToList();
        }

        private List<SetUnlockRequestJobModel> GetJobNo(UnlockJobCriteria criteria)
        {
            string _unlockType = UnlockTypeEx.GetUnlockType(criteria.UnlockTypeNum);
            var result = new List<SetUnlockRequestJobModel>();
            if (criteria.JobIds != null && criteria.JobIds.Count > 0)
            {
                var dataOps = opsTransactionRepo.Get(x => criteria.JobIds.Where(w => !string.IsNullOrEmpty(w)).Contains(x.JobNo)).Select(s => new SetUnlockRequestJobModel()
                {
                    UnlockName = s.JobNo,
                    Job = s.JobNo,
                    UnlockType = _unlockType
                });
                var dataDoc = transRepo.Get(x => criteria.JobIds.Where(w => !string.IsNullOrEmpty(w)).Contains(x.JobNo)).Select(s => new SetUnlockRequestJobModel()
                {
                    UnlockName = s.JobNo,
                    Job = s.JobNo,
                    UnlockType = _unlockType
                });
                var dataMerge = dataOps.Union(dataDoc);
                return dataMerge.ToList();
            }

            if (criteria.Mbls != null && criteria.Mbls.Count > 0)
            {
                var dataOps = opsTransactionRepo.Get(x => criteria.Mbls.Where(w => !string.IsNullOrEmpty(w)).Contains(x.Mblno)).Select(s => new SetUnlockRequestJobModel()
                {
                    UnlockName = s.JobNo + " - " + s.Mblno,
                    Job = s.JobNo,
                    UnlockType = _unlockType
                });
                var dataDoc = transRepo.Get(x => criteria.Mbls.Where(w => !string.IsNullOrEmpty(w)).Contains(x.Mawb)).Select(s => new SetUnlockRequestJobModel()
                {
                    UnlockName = s.JobNo + " - " + s.Mawb,
                    Job = s.JobNo,
                    UnlockType = _unlockType
                });
                var dataMerge = dataOps.Union(dataDoc);
                return dataMerge.ToList();
            }

            if (criteria.CustomNos != null && criteria.CustomNos.Count > 0)
            {
                var dataOps = customsRepo.Get(x => criteria.CustomNos.Where(w => !string.IsNullOrEmpty(w)).Contains(x.ClearanceNo)).Select(s => new CustomsDeclaration() { ClearanceNo = s.ClearanceNo, JobNo = s.JobNo }).ToList();

                var dataOpsGroup = dataOps.GroupBy(g => new { JobNo = g.JobNo }).Where(w => !string.IsNullOrEmpty(w.Key.JobNo)).Select(s => new SetUnlockRequestJobModel()
                {
                    UnlockName = s.Key.JobNo + " - " + string.Join(", ", s.Select(l => l.ClearanceNo)),
                    Job = s.Key.JobNo,
                    UnlockType = _unlockType
                });
                return dataOpsGroup.ToList();
            }
            return result;
        }

        public List<SetUnlockRequestJobModel> GetJobToUnlockRequest(UnlockJobCriteria criteria)
        {
            var result = new List<SetUnlockRequestJobModel>();
            switch (criteria.UnlockTypeNum)
            {
                case UnlockTypeEnum.SHIPMENT:
                    result = GetJobNo(criteria);
                    break;
                case UnlockTypeEnum.ADVANCE:
                    result = GetAdvance(criteria);
                    break;
                case UnlockTypeEnum.SETTLEMENT:
                    result = GetSettlement(criteria);
                    break;
                case UnlockTypeEnum.CHANGESERVICEDATE:
                    result = GetJobNo(criteria);
                    break;
                default:
                    break;
            }
            return result;
        }

        public HandleState CheckExistVoucherNoOfAdvance(UnlockJobCriteria criteria)
        {
            var hs = new HandleState();
            if (criteria.Advances == null || criteria.Advances.Count == 0) return hs;
            foreach (var adv in criteria.Advances)
            {
                var advance = advancePaymentRepo.Get(x => x.AdvanceNo == adv && !string.IsNullOrEmpty(x.VoucherNo)).FirstOrDefault();
                if (advance != null)
                {
                    object message = "You cant's unlock " + advance.AdvanceNo + ", because it has  existed in " + advance.VoucherNo + ", please recheck!";
                    return new HandleState(message);
                }
            }
            return hs;
        }
        public HandleState CheckExistInvoiceNoOfSettlement(UnlockJobCriteria criteria)
        {
            var hs = new HandleState();
            if (criteria.Settlements == null || criteria.Settlements.Count == 0) return hs;
            foreach (var settle in criteria.Settlements)
            {
                var charge = surchargeRepo.Get(x => x.SettlementCode == settle && (!string.IsNullOrEmpty(x.InvoiceNo) || !string.IsNullOrEmpty(x.VoucherId))).FirstOrDefault();
                if (charge != null)
                {
                    object message = "You cant's unlock " + settle + ", because it has  existed in " + charge.InvoiceNo + " " + charge.VoucherId + ", please recheck!";
                    return new HandleState(message);
                }
            }
            return hs;
        }

        #endregion --- GET SHIPMENT, ADVANCE, SETTLEMENT TO UNLOCK REQUEST ---

        #region --- CRUD ---
        public HandleState AddUnlockRequest(SetUnlockRequestModel model)
        {
            try
            {
                model.Id = Guid.NewGuid();
                model.Requester = model.UserCreated = model.UserModified = currentUser.UserID;
                model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                model.GroupId = currentUser.GroupId;
                model.DepartmentId = currentUser.DepartmentId;
                model.OfficeId = currentUser.OfficeID;
                model.CompanyId = currentUser.CompanyID;
                model.StatusApproval = model.StatusApproval = string.IsNullOrEmpty(model.StatusApproval) ? SettingConstants.STATUS_APPROVAL_NEW : model.StatusApproval;
                var unlockRequest = mapper.Map<SetUnlockRequest>(model);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Add(unlockRequest);
                        if (hs.Success)
                        {
                            var jobs = mapper.Map<List<SetUnlockRequestJob>>(model.Jobs);
                            if (jobs != null && jobs.Count > 0)
                            {
                                foreach (var job in model.Jobs)
                                {
                                    job.Id = Guid.NewGuid();
                                    job.UnlockRequestId = model.Id;
                                    job.UnlockType = model.UnlockType;
                                    job.UserCreated = currentUser.UserID;
                                    job.DatetimeCreated = DateTime.Now;
                                    var hsAddJob = setUnlockRequestJobRepo.Add(job, false);
                                }
                                setUnlockRequestJobRepo.SubmitChanges();
                            }

                            DataContext.SubmitChanges();
                            trans.Commit();
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

        public HandleState DeleteUnlockRequest(Guid id)
        {
            var hs = new HandleState();
            try
            {
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var unlockRequest = DataContext.Get(x => x.Id == id).FirstOrDefault();
                        if (unlockRequest == null) return new HandleState((object)"Not found Unlock Request");
                        if (unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_NEW
                            && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DENIED)
                        {
                            return new HandleState((object)"Not allow delete. Unlock request are awaiting approval.");
                        }
                        hs = DataContext.Delete(x => x.Id == id, false);
                        if (hs.Success)
                        {
                            var unlockRequestJobs = setUnlockRequestJobRepo.Get(x => x.UnlockRequestId == id);
                            //Xóa các Unlock Request  Job có UnlockRequestId = Id truyền vào
                            if (unlockRequestJobs != null)
                            {
                                foreach (var item in unlockRequestJobs)
                                {
                                    var unlockRequestJobDelete = setUnlockRequestJobRepo.Delete(x => x.Id == item.Id, false);
                                }
                                setUnlockRequestJobRepo.SubmitChanges();
                            }

                            DataContext.SubmitChanges();
                            trans.Commit();
                        }
                        return hs;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        hs = new HandleState((object)ex.Message);
                        return hs;
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                hs = new HandleState((object)ex.Message);
                return hs;
            }
        }

        public HandleState UpdateUnlockRequest(SetUnlockRequestModel model)
        {
            try
            {
                var unlockRequest = mapper.Map<SetUnlockRequest>(model);
                var unlockRequestCurrent = DataContext.Get(x => x.Id == unlockRequest.Id).FirstOrDefault();
                if (unlockRequestCurrent == null) return new HandleState("Not found unlock request");
                unlockRequest.DatetimeModified = DateTime.Now;
                unlockRequest.UserModified = currentUser.UserID;

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var hs = DataContext.Update(unlockRequest, x => x.Id == unlockRequest.Id);
                        if (hs.Success)
                        {
                            var jobs = mapper.Map<List<SetUnlockRequestJob>>(model.Jobs);
                            if (jobs != null && jobs.Count > 0)
                            {
                                var listJobNeedDelete = setUnlockRequestJobRepo.Get(x => x.UnlockRequestId == unlockRequest.Id).Where(x => !jobs.Select(s => s.Id).Contains(x.Id));
                                foreach (var job in listJobNeedDelete)
                                {
                                    var hsDeleteJob = setUnlockRequestJobRepo.Delete(x => x.Id == job.Id, false);
                                }
                                foreach (var job in jobs)
                                {
                                    if (job.Id == Guid.Empty)
                                    {
                                        job.Id = Guid.NewGuid();
                                        job.UnlockRequestId = model.Id;
                                        job.UnlockType = model.UnlockType;
                                        job.UserCreated = currentUser.UserID;
                                        job.DatetimeCreated = DateTime.Now;
                                        var hsAddJob = setUnlockRequestJobRepo.Add(job, false);
                                    }
                                    else
                                    {
                                        var hsUpdateJob = setUnlockRequestJobRepo.Update(job, x => x.Id == job.Id, false);
                                    }
                                }

                                setUnlockRequestJobRepo.SubmitChanges();
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

        #region --- LIST & PAGING ---
        private PermissionRange GetPermissionRangeOfRequester()
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingUnlockRequest);
            PermissionRange _permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            return _permissionRange;
        }

        private Expression<Func<SetUnlockRequest, bool>> ExpressionQuery(UnlockRequestCriteria criteria)
        {
            var unlockType = UnlockTypeEx.GetUnlockType(criteria.UnlockTypeNum);
            Expression<Func<SetUnlockRequest, bool>> query = q => true;

            if (!string.IsNullOrEmpty(unlockType))
            {
                query = query.And(x => x.UnlockType == unlockType);
            }

            //if (!string.IsNullOrEmpty(criteria.Requester))
            //{
            //    query = query.And(x => x.UserCreated == criteria.Requester);
            //}

            if (criteria.CreatedDate != null)
            {
                query = query.And(x => x.DatetimeCreated.Value.Date == criteria.CreatedDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(criteria.StatusApproval))
            {
                query = query.And(x => x.StatusApproval == criteria.StatusApproval);
            }
            return query;
        }

        private IQueryable<SetUnlockRequest> GetDataUnlockRequest(UnlockRequestCriteria criteria)
        {
            var permissionRangeRequester = GetPermissionRangeOfRequester();
            var unlockRequests = DataContext.Get();
            var unlockRequestAprs = setUnlockRequestApproveRepo.Get(x => x.IsDeny == false);
            var data = from unlockRequest in unlockRequests
                       join unlockRequestApr in unlockRequestAprs on unlockRequest.Id equals unlockRequestApr.UnlockRequestId into unlockRequestApr2
                       from unlockRequestApr in unlockRequestApr2.DefaultIfEmpty()
                       select new { unlockRequest, unlockRequestApr };
            var result = data.Where(x =>
                (
                    permissionRangeRequester == PermissionRange.All ? (criteria.Requester == currentUser.UserID ? x.unlockRequest.UserCreated == criteria.Requester : false) : true
                    &&
                    permissionRangeRequester == PermissionRange.None ? false : true
                    &&
                    permissionRangeRequester == PermissionRange.Owner ? x.unlockRequest.UserCreated == criteria.Requester : true
                    &&
                    permissionRangeRequester == PermissionRange.Group ? (x.unlockRequest.GroupId == currentUser.GroupId
                                                                        && x.unlockRequest.DepartmentId == currentUser.DepartmentId
                                                                        && x.unlockRequest.OfficeId == currentUser.OfficeID
                                                                        && x.unlockRequest.CompanyId == currentUser.CompanyID
                                                                        && (criteria.Requester == currentUser.UserID ? x.unlockRequest.UserCreated == criteria.Requester : false)) : true
                    &&
                    permissionRangeRequester == PermissionRange.Department ? (x.unlockRequest.DepartmentId == currentUser.DepartmentId
                                                                              && x.unlockRequest.OfficeId == currentUser.OfficeID
                                                                              && x.unlockRequest.CompanyId == currentUser.CompanyID
                                                                              && (criteria.Requester == currentUser.UserID ? x.unlockRequest.UserCreated == criteria.Requester : false)) : true
                    &&
                    permissionRangeRequester == PermissionRange.Office ? (x.unlockRequest.OfficeId == currentUser.OfficeID
                                                                          && x.unlockRequest.CompanyId == currentUser.CompanyID
                                                                          && (criteria.Requester == currentUser.UserID ? x.unlockRequest.UserCreated == criteria.Requester : false)) : true
                    &&
                    permissionRangeRequester == PermissionRange.Company ? x.unlockRequest.CompanyId == currentUser.CompanyID && (criteria.Requester == currentUser.UserID ? x.unlockRequest.UserCreated == criteria.Requester : false) : true
                )
                ||
                (x.unlockRequestApr != null && (x.unlockRequestApr.Leader == currentUser.UserID
                  || x.unlockRequestApr.LeaderApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(x.unlockRequest.UnlockType, x.unlockRequestApr.Leader, x.unlockRequest.GroupId, x.unlockRequest.DepartmentId, x.unlockRequest.OfficeId, x.unlockRequest.CompanyId)
                )
                && x.unlockRequest.GroupId == currentUser.GroupId
                && x.unlockRequest.DepartmentId == currentUser.DepartmentId
                && x.unlockRequest.OfficeId == currentUser.OfficeID
                && x.unlockRequest.CompanyId == currentUser.CompanyID
                && x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_NEW
                && x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DENIED
                && (x.unlockRequest.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.unlockRequest.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) //LEADER AND DEPUTY OF LEADER
                ||
                (x.unlockRequestApr != null && (x.unlockRequestApr.Manager == currentUser.UserID
                  || x.unlockRequestApr.ManagerApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(x.unlockRequest.UnlockType, x.unlockRequestApr.Manager, null, x.unlockRequest.DepartmentId, x.unlockRequest.OfficeId, x.unlockRequest.CompanyId)
                  )
                // && x.unlockRequest.GroupId == currentUser.GroupId
                && x.unlockRequest.DepartmentId == currentUser.DepartmentId
                && x.unlockRequest.OfficeId == currentUser.OfficeID
                && x.unlockRequest.CompanyId == currentUser.CompanyID
                && x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_NEW
                && x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DENIED
                && (!string.IsNullOrEmpty(x.unlockRequestApr.Leader) ? x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_REQUESTAPPROVAL : true)
                && (x.unlockRequest.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.unlockRequest.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) //MANANER AND DEPUTY OF MANAGER
                ||
                (x.unlockRequestApr != null && (x.unlockRequestApr.Accountant == currentUser.UserID
                  || x.unlockRequestApr.AccountantApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(x.unlockRequest.UnlockType, x.unlockRequestApr.Accountant, null, null, x.unlockRequest.OfficeId, x.unlockRequest.CompanyId)
                  )
                && x.unlockRequest.OfficeId == currentUser.OfficeID
                && x.unlockRequest.CompanyId == currentUser.CompanyID
                && x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_NEW
                && x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DENIED
                //&& (!string.IsNullOrEmpty(x.unlockRequestApr.Leader) ? x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_REQUESTAPPROVAL : true)
                //&& (!string.IsNullOrEmpty(x.unlockRequestApr.Manager) ? x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_LEADERAPPROVED : true)
                && (x.unlockRequest.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.unlockRequest.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) // ACCOUTANT AND DEPUTY OF ACCOUNTANT
                ||
                (x.unlockRequestApr != null && (x.unlockRequestApr.Buhead == currentUser.UserID
                  || x.unlockRequestApr.BuheadApr == currentUser.UserID
                  || userBaseService.CheckIsUserDeputy(x.unlockRequest.UnlockType, x.unlockRequestApr.Buhead ?? null, null, null, x.unlockRequest.OfficeId, x.unlockRequest.CompanyId)
                  )
                // && x.unlockRequest.GroupId == currentUser.GroupId
                // && x.unlockRequest.DepartmentId == currentUser.DepartmentId
                && x.unlockRequest.OfficeId == currentUser.OfficeID
                && x.unlockRequest.CompanyId == currentUser.CompanyID
                && x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_NEW
                && x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DENIED
                //&& (!string.IsNullOrEmpty(x.unlockRequestApr.Leader) ? x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_REQUESTAPPROVAL : true)
                //&& (!string.IsNullOrEmpty(x.unlockRequestApr.Manager) ? x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_LEADERAPPROVED : true)
                //&& (!string.IsNullOrEmpty(x.unlockRequestApr.Accountant) ? x.unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_MANAGERAPPROVED : true)
                && (x.unlockRequest.Requester == criteria.Requester && currentUser.UserID != criteria.Requester ? x.unlockRequest.Requester == criteria.Requester : (currentUser.UserID == criteria.Requester ? true : false))
                ) //BOD AND DEPUTY OF BOD                
            ).Select(s => s.unlockRequest);

            return result;
        }

        public IQueryable<UnlockRequestResult> GetData(UnlockRequestCriteria criteria)
        {
            var queryUnlockRequest = ExpressionQuery(criteria);
            var dataUnlockRequests = GetDataUnlockRequest(criteria);
            if (dataUnlockRequests == null) return null;
            var unlockRequests = dataUnlockRequests.Where(queryUnlockRequest);

            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                var unlockRequestJobs = setUnlockRequestJobRepo.Get(x => criteria.ReferenceNos.Contains(x.UnlockName));
                unlockRequests = unlockRequests.Join(unlockRequestJobs, u => u.Id, j => j.UnlockRequestId, (u, j) => u);
            }

            var users = userRepo.Get();
            var data = from ur in unlockRequests
                       join user in users on ur.Requester equals user.Id into user2
                       from user in user2.DefaultIfEmpty()
                       select new UnlockRequestResult()
                       {
                           Id = ur.Id,
                           Subject = ur.Subject,
                           RequestDate = ur.RequestDate,
                           Requester = ur.Requester,
                           RequesterName = user.Username,
                           UnlockType = ur.UnlockType,
                           StatusApproval = ur.StatusApproval,
                           DatetimeCreated = ur.DatetimeCreated,
                           DatetimeModified = ur.DatetimeModified
                       };

            return data.ToArray().OrderByDescending(o => o.DatetimeModified).AsQueryable();
        }

        public List<UnlockRequestResult> Paging(UnlockRequestCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetData(criteria);
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
        #endregion --- LIST & PAGING ---

        #region --- DETAIL ---
        public SetUnlockRequestModel GetDetailUnlockRequest(Guid id)
        {
            var detail = new SetUnlockRequestModel();
            var unlockRequest = DataContext.Get(x => x.Id == id).FirstOrDefault();
            detail = mapper.Map<SetUnlockRequestModel>(unlockRequest);
            if (unlockRequest != null)
            {
                var unlockJob = setUnlockRequestJobRepo.Get(x => x.UnlockRequestId == id).ToList();
                detail.Jobs = mapper.Map<List<SetUnlockRequestJobModel>>(unlockJob);
                detail.RequesterName = userRepo.Where(x => x.Id == unlockRequest.Requester).FirstOrDefault()?.Username;
                detail.UserNameCreated = userRepo.Where(x => x.Id == unlockRequest.UserCreated).FirstOrDefault()?.Username;
                detail.UserNameModified = userRepo.Where(x => x.Id == unlockRequest.UserModified).FirstOrDefault()?.Username;
                var unlockApprove = setUnlockRequestApproveRepo.Get(x => x.UnlockRequestId == id && x.IsDeny == false).FirstOrDefault();

                detail.IsRequester = (currentUser.UserID == unlockRequest.Requester
                    && currentUser.GroupId == unlockRequest.GroupId
                    && currentUser.DepartmentId == unlockRequest.DepartmentId
                    && currentUser.OfficeID == unlockRequest.OfficeId
                    && currentUser.CompanyID == unlockRequest.CompanyId) ? true : false;
                detail.IsManager = unlockRequestApproveService.CheckUserIsManager(currentUser, unlockRequest, unlockApprove);
                detail.IsApproved = unlockRequestApproveService.CheckUserIsApproved(currentUser, unlockRequest, unlockApprove);
                detail.IsShowBtnDeny = unlockRequestApproveService.CheckIsShowBtnDeny(currentUser, unlockRequest, unlockApprove);
            }
            return detail;
        }
        #endregion --- DETAIL ---
    }
}
