﻿using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.DL.IService
{
    public interface IUserBaseService : IRepositoryBase<SysUser, SysUserModel>
    {
        string GetLeaderIdOfUser(string userId);
        CatDepartment GetInfoDeptOfUser(int? departmentId);
        List<string> GetLeaderGroup(Guid? companyId, Guid? officeId, int? departmentId, int? groupId);
        List<string> GetDeptManager(Guid? companyId, Guid? officeId, int? departmentId);
        List<string> GetAccoutantManager(Guid? companyId, Guid? officeId);
        List<string> GetBUHead(Guid? companyId, Guid? officeId);
        string GetBUHeadId(string idBranch);
        string GetEmployeeIdOfUser(string userId);
        SysEmployee GetEmployeeByEmployeeId(string employeeId);
        SysEmployee GetEmployeeByUserId(string userId);
        bool CheckIsAccountantDept(int? deptId);
        bool CheckIsBOD(int? departmentId, Guid? officeId, Guid? companyId);
        SysSettingFlow GetSettingFlowApproval(string type, Guid? officeId);
        string GetRoleByLevel(string level, string type, Guid? officeId);
        List<string> GetAuthorizedApprovalByTypeAndAuthorizer(string type, string authorizer, Guid? officeCommissioner);
        bool CheckUserSameLevel(string userId, int? groupId, int? departmentId, Guid? officeId, Guid? companyId);
        List<string> GetUsersDeputyByCondition(string type, string userId, int? groupId, int? departmentId, Guid? officeId, Guid? companyId);
        List<string> GetEmailUsersDeputyByCondition(string type, string userId, int? groupId, int? departmentId, Guid? officeId, Guid? companyId);
        bool CheckIsUserDeputy(string type, string commissioner, string userId, int? groupId, int? departmentId, Guid? officeId, Guid? companyId);
        bool CheckIsAccountantByOfficeDept(Guid? officeId, int? deptId);
        bool CheckIsUserAdmin(string currUserId, Guid currOfficeId, Guid currCompanyId, Guid? objOfficeId, Guid? objCompanyId);
        List<int?> GetDepartmentUser(Guid? companyId, Guid? officeId, string userId);
    }
}
