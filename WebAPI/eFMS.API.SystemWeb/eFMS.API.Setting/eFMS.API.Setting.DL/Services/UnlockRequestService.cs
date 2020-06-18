using AutoMapper;
using eFMS.API.Common.Globals;
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
using System.Text;

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
            IContextBase<CustomsDeclaration> customs) : base(repository, mapper)
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
        }
        
        #region --- GET SHIPMENT, ADVANCE, SETTLEMENT TO UNLOCK REQUEST ---
        public List<SetUnlockRequestJobModel> GetAdvance(UnlockJobCriteria criteria)
        {
            var result = new List<SetUnlockRequestJobModel>();
            if (criteria.Advances == null || criteria.Advances.Count == 0) return result;
            var data = advancePaymentRepo.Get(x => criteria.Advances.Where(w => !string.IsNullOrEmpty(w)).Contains(x.AdvanceNo)).Select(s => new SetUnlockRequestJobModel()
            {
                UnlockName = s.AdvanceNo,
                Job = s.AdvanceNo,
                UnlockType = UnlockTypeEx.GetUnlockType(criteria.UnlockTypeNum)
            });
            return data.ToList();
        }

        public List<SetUnlockRequestJobModel> GetSettlement(UnlockJobCriteria criteria)
        {
            var result = new List<SetUnlockRequestJobModel>();
            if (criteria.Settlements == null || criteria.Settlements.Count == 0) return result;
            var data = settlementPaymentRepo.Get(x => criteria.Settlements.Where(w => !string.IsNullOrEmpty(w)).Contains(x.SettlementNo)).Select(s => new SetUnlockRequestJobModel()
            {
                UnlockName = s.SettlementNo,
                Job = s.SettlementNo,
                UnlockType = UnlockTypeEx.GetUnlockType(criteria.UnlockTypeNum)
            });
            return data.ToList();
        }

        public List<SetUnlockRequestJobModel> GetJobNo(UnlockJobCriteria criteria)
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

        #endregion --- GET SHIPMENT, ADVANCE, SETTLEMENT TO UNLOCK REQUEST ---
        public HandleState AddUnlockRequest(SetUnlockRequestModel model)
        {
            try
            {
                model.Id = Guid.NewGuid();
                model.UserCreated = model.UserModified = currentUser.UserID;
                model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                model.GroupId = currentUser.GroupId;
                model.DepartmentId = currentUser.DepartmentId;
                model.OfficeId = currentUser.OfficeID;
                model.CompanyId = currentUser.CompanyID;
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
            throw new NotImplementedException();
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
                                        var isExist = setUnlockRequestJobRepo.Get(x => x.Id == job.Id).Any();
                                        if (isExist)
                                        {
                                            var hsUpdateJob = setUnlockRequestJobRepo.Update(job, x => x.Id == job.Id, false);
                                        }
                                        else
                                        {
                                            var hsDeleteJob = setUnlockRequestJobRepo.Delete(x => x.Id == job.Id, false);
                                        }

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
    }
}
