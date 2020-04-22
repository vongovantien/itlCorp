﻿using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IUserBaseService : IRepositoryBase<SysUser, SysUserModel>
    {
        string GetLeaderIdOfUser(string userId);
        CatDepartment GetInfoDeptOfUser(int? departmentId);
        List<string> GetDeptManager(Guid? companyId, Guid? officeId, int? departmentId);
        List<string> GetAccoutantManager(Guid? companyId, Guid? officeId);
        string GetBUHeadId(string idBranch);
        string GetEmployeeIdOfUser(string userId);
        SysEmployee GetEmployeeByEmployeeId(string employeeId);
        SysEmployee GetEmployeeByUserId(string userId);
        bool CheckIsAccountantDept(int? deptId);
        List<string> GetListUserDeputyByDept(string dept);
        bool CheckDeputyManagerByUser(int? departmentId, string userId);
        bool CheckDeputyAccountantByUser(int? departmentId, string userId);
    }
}
