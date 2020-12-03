using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;

namespace eFMS.API.Documentation.DL.Services
{
    public class ReportLogService : RepositoryBase<SysReportLog, SysReportLogModel>, IReportLogService
    {
        private readonly ICurrentUser currentUser;
        public ReportLogService(IContextBase<SysReportLog> repository,
            IMapper mapper,
            ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
        }

        public HandleState AddNew(SysReportLogModel model)
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
                SysReportLog reportLog = mapper.Map<SysReportLog>(model);
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        HandleState hs = DataContext.Add(reportLog, false);
                        if (hs.Success)
                        {                            
                            DataContext.SubmitChanges();
                            trans.Commit();
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
                return new HandleState((object)ex.Message);
            }
        }
    }
}
