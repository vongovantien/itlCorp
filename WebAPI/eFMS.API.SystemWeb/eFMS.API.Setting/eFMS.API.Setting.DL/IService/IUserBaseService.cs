using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.IService
{
    public interface IUserBaseService : IRepositoryBase<SysUser, SysUserModel>
    {
        CatDepartment GetInfoDeptOfUser(int? departmentId);
        List<string> GetLeaderGroup(Guid? companyId, Guid? officeId, int? departmentId, int? groupId);
        List<string> GetDeptManager(Guid? companyId, Guid? officeId, int? departmentId);
        List<string> GetAccoutantManager(Guid? companyId, Guid? officeId);
        List<string> GetBUHead(Guid? companyId, Guid? officeId);
        string GetEmployeeIdOfUser(string userId);
        SysEmployee GetEmployeeByEmployeeId(string employeeId);
        SysEmployee GetEmployeeByUserId(string userId);
        bool CheckIsAccountantDept(int? deptId);
        List<string> GetListUserDeputyByDept(string dept);
        bool CheckDeputyManagerByUser(int? departmentId, string userId);
        bool CheckDeputyAccountantByUser(int? departmentId, string userId);
    }
}
