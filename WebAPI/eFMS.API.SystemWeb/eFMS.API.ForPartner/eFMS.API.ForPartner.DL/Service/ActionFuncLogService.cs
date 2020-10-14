using AutoMapper;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;

namespace eFMS.API.ForPartner.DL.Service
{
    public class ActionFuncLogService : RepositoryBase<SysActionFuncLog, SysActionFuncLogModel>, IActionFuncLogService
    {
        private readonly ICurrentUser currentUser;

        public ActionFuncLogService(IContextBase<SysActionFuncLog> repository,
            IMapper mapper,
            ICurrentUser currUser) : base(repository, mapper)
        {
            currentUser = currUser;
        }

        public HandleState AddActionFuncLog(SysActionFuncLogModel model)
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

                var log = mapper.Map<SysActionFuncLog>(model);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Add(log, false);
                        if (hs.Success)
                        {
                            var sm = DataContext.SubmitChanges();
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
                return new HandleState(ex.Message);
            }
        }

        public HandleState UpdateActionFuncLog(SysActionFuncLogModel model)
        {
            try
            {
                var log = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                model.UserModified = currentUser.UserID;
                model.DatetimeModified = DateTime.Now;
                model.GroupId = currentUser.GroupId;
                model.DepartmentId = currentUser.DepartmentId;
                model.OfficeId = currentUser.OfficeID;
                model.CompanyId = currentUser.CompanyID;

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Update(log, x => x.Id == log.Id, false);
                        if (hs.Success)
                        {
                            var sm = DataContext.SubmitChanges();
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
                return new HandleState(ex.Message);
            }
        }
    }
}
