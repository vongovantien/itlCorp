using System;
using System.Linq;
using SystemManagement.DL.Models;

using System.Threading.Tasks;
using System.Collections.Generic;
using SystemManagement.DL.Helpers.PagingPrams;
using SystemManagement.DL.Helpers.PageList;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;
using SystemManagement.DL.Models.Views;

namespace SystemManagement.DL.Services
{
    public interface ISysEmployeeService: IRepositoryBase<SysEmployee, SysEmployeeModel>
    {
        SysEmployeeModel GetByID(string id);
        IQueryable<SysEmployeeModel> GetFollowWorkPlace(Guid WorkPlaceId);
        object GenerateID(Guid WorkPlaceId);
        IQueryable<SysEmployeeModel> GetFollowRole(int RoleId, Guid WorkPlaceId);
        PagedList<SysEmployeeModel> GetMAllSysEmployeeModel(PagingParams sysEmployeeFilter);

        List<vw_sysEmployee> GetViewEmployees();

    }
}
