using AutoMapper;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

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
                var reportLog = new List<SysReportLogModel>();
                reportLog.Add(model);
                var result = UpdateCreditManagement(reportLog);
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState((object)ex.Message);
            }
        }

        /// <summary>
        /// Use store to insert data report log
        /// </summary>
        /// <param name="reportInsert"></param>
        /// <returns></returns>
        private sp_InsertReportLog UpdateCreditManagement(List<SysReportLogModel> reportInsert)
        {
            var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@ReportData",
                    Value = DataHelper.ToDataTable(reportInsert),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[ReportLog]"
                }
            };
            var result = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_InsertReportLog>(parameters);
            return result.FirstOrDefault();
        }
    }
}
