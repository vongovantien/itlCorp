using AutoMapper;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
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

        public UnlockRequestService(
            IContextBase<SetUnlockRequest> repository,
            IMapper mapper,
            ICurrentUser user,
            IContextBase<SetUnlockRequestJob> setUnlockRequestJob,
            IContextBase<SetUnlockRequestApprove> setUnlockRequestApprove) : base(repository, mapper)
        {
            currentUser = user;
            setUnlockRequestJobRepo = setUnlockRequestJob;
            setUnlockRequestApproveRepo = setUnlockRequestApprove;
        }

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
                                foreach(var job in jobs)
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
