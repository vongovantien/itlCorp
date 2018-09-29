using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using SystemManagementAPI.Service.Contexts;
using SystemManagementAPI.Service.Models;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using ITL.NetCore.Connection;
using SystemManagement.DL.Helpers.Framework;
using LinqKit;
using Newtonsoft.Json;
using SystemManagement.DL.Helpers.Extensions;
using SystemManagement.DL.Helpers.PageList;
using SystemManagement.DL.Helpers.PagingPrams;
using SystemManagement.DL.Models.Views;

namespace SystemManagement.DL.Services
{
    public class SysEmployeeService : RepositoryBase<SysEmployee, SysEmployeeModel>, ISysEmployeeService
    {
        public SysEmployeeService(IContextBase<SysEmployee> repository, IMapper mapper) : base(repository, mapper)
        {
        }
        public SysEmployeeModel GetByID(string id)
        {
            return First(t => t.Id == id);
        }
        public IQueryable<SysEmployeeModel> GetFollowWorkPlace(Guid WorkPlaceId)
        {
            return Get(t => t.WorkPlaceId == WorkPlaceId);
        }
        public IQueryable<SysEmployeeModel> GetFollowRole(int RoleId, Guid WorkPlaceId)
        {
            var lEmployee = from emp in DataContext.Get()
                            join u in ((eTMSDataContext)DataContext.DC).SysUser on emp.Id equals u.EmployeeId
                            join ur in ((eTMSDataContext)DataContext.DC).SysUserRole on u.Id equals ur.UserId
                            where ur.WorkPlaceId == WorkPlaceId && !(ur.Inactive ?? false)
                            select emp;
            var returnedList = new List<SysEmployeeModel>();
            lEmployee.ToList().ForEach(u =>
            {
                returnedList.Add(mapper.Map<SysEmployee, SysEmployeeModel>(u));
            });
            return returnedList.AsQueryable();
        }
        public object GenerateID(Guid WorkPlaceId)
        {
            string funcName = "dbo.fn_GenerateEmployeeID";
            SqlParameter pa = new SqlParameter("@WorkPlaceID", WorkPlaceId);
            var result = ((eTMSDataContext)DataContext.DC).ExecuteFuncScalar(funcName, pa);

            return new { Id = result.ToString() };
        }
        public PagedList<SysEmployeeModel> GetMAllSysEmployeeModel(PagingParams pagingParams)
        {
            try
            {
                var returnedList = new List<SysEmployeeModel>();
                DataContext.Get().ToList().ForEach(u =>
                {
                    returnedList.Add(mapper.Map<SysEmployee, SysEmployeeModel>(u));
                });
                SysEmployeeModel filterValues = JsonConvert.DeserializeObject<SysEmployeeModel>(pagingParams.Filter);
                IQueryable<SysEmployeeModel> query = returnedList.AsQueryable();
                PredicateParser predicateParser = new PredicateParser();
                ExpressionStarter<SysEmployeeModel> predicate = PredicateBuilder.New(ExpressionExtensions.BuildPredicate<SysEmployeeModel>(filterValues, predicateParser, true));
                query = query.AsExpandable().Where(predicate);
                return new PagedList<SysEmployeeModel>(
                    query, pagingParams.PageNumber, pagingParams.PageSize);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<vw_sysEmployee> GetViewEmployees()
        {
            return ((eTMSDataContext)DataContext.DC).GetViewData<vw_sysEmployee>();
        }
    }
}
