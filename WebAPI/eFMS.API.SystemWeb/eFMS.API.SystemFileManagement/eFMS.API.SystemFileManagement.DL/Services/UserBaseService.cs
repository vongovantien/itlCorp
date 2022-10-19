﻿using AutoMapper;
using eFMS.API.SystemFileManagement.DL.Common;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.SystemFileManagement.DL.Services
{
    public class UserBaseService : RepositoryBase<SysUser, SysUserModel>, IUserBaseService
    {
        readonly IContextBase<SysUserLevel> sysUserLevelRepo;
        readonly IContextBase<SysGroup> sysGroupRepo;
        readonly IContextBase<CatDepartment> catDepartmentRepo;
        readonly IContextBase<SysEmployee> sysEmployeeRepo;
        readonly IContextBase<SysOffice> sysOfficeRepo;
        readonly IContextBase<SysSettingFlow> settingFlowRepo;
        readonly IContextBase<SysAuthorizedApproval> authourizedApprovalRepo;
        readonly IContextBase<SysUser> sysUserRepo;

        public UserBaseService(
            IContextBase<SysUser> repository,
            IMapper mapper,
            IContextBase<SysUserLevel> sysUserLevel,
            IContextBase<SysGroup> sysGroup,
            IContextBase<CatDepartment> catDepartment,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<SysOffice> sysOffice,
            IContextBase<SysSettingFlow> settingFlow,
            IContextBase<SysAuthorizedApproval> authourizedApproval,
            IContextBase<SysUser> sysUser) : base(repository, mapper)
        {
            sysUserLevelRepo = sysUserLevel;
            sysGroupRepo = sysGroup;
            catDepartmentRepo = catDepartment;
            sysEmployeeRepo = sysEmployee;
            sysOfficeRepo = sysOffice;
            settingFlowRepo = settingFlow;
            authourizedApprovalRepo = authourizedApproval;
            sysUserRepo = sysUser;
        }

        private int? GetGroupIdOfUser(string userId)
        {
            //Lấy ra groupId của user
            var grpIdOfUser = sysUserLevelRepo.Get(x => x.UserId == userId).FirstOrDefault()?.GroupId;
            return grpIdOfUser;
        }

        private SysGroup GetInfoGroupOfUser(string userId)
        {
            var grpIdOfUser = GetGroupIdOfUser(userId);
            var infoGrpOfUser = sysGroupRepo.Get(x => x.Id == grpIdOfUser).FirstOrDefault();
            return infoGrpOfUser;
        }

        //Lấy ra Leader của User
        //Leader đây chính là ManagerID của Group
        public string GetLeaderIdOfUser(string userId)
        {
            var leaderIdOfUser = GetInfoGroupOfUser(userId)?.ManagerId;
            return leaderIdOfUser;
        }

        //Lấy Info Dept của User
        public CatDepartment GetInfoDeptOfUser(int? departmentId)
        {
            var deptOfUser = catDepartmentRepo.Get(x => x.Id == departmentId).FirstOrDefault();
            return deptOfUser;
        }

        #region -- LEADER, MANAGER, ACCOUNTANT, BOD --
        public List<string> GetLeaderGroup(Guid? companyId, Guid? officeId, int? departmentId, int? groupId)
        {
            var userIds = sysUserRepo.Get().Select(s => s.Id).ToList();
            var leaders = sysUserLevelRepo.Get(x => x.Position == AccountingConstants.PositionManager
                                                    && x.GroupId == groupId
                                                    && x.DepartmentId == departmentId
                                                    && x.DepartmentId != null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId)
                                                    .Where(w => userIds.Contains(w.UserId))
                                                    .Select(s => s.UserId).ToList();
            return leaders;
        }

        public List<string> GetDeptManager(Guid? companyId, Guid? officeId, int? departmentId)
        {
            var userIds = sysUserRepo.Get().Select(s => s.Id).ToList();
            var managers = sysUserLevelRepo.Get(x => x.GroupId == AccountingConstants.SpecialGroup
                                                    && x.Position == AccountingConstants.PositionManager
                                                    && x.DepartmentId == departmentId
                                                    && x.DepartmentId != null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId)
                                                    .Where(w => userIds.Contains(w.UserId))
                                                    .Select(s => s.UserId).ToList();
            return managers;
        }

        public List<string> GetAccoutantManager(Guid? companyId, Guid? officeId)
        {
            var userIds = sysUserRepo.Get().Select(s => s.Id).ToList();
            var deptAccountants = catDepartmentRepo.Get(s => s.DeptType == AccountingConstants.DeptTypeAccountant).Select(s => s.Id).ToList();
            var accountants = sysUserLevelRepo.Get(x => x.GroupId == AccountingConstants.SpecialGroup
                                                    && x.Position == AccountingConstants.PositionManager
                                                    && x.OfficeId == officeId
                                                    && x.DepartmentId != null
                                                    && x.CompanyId == companyId)
                                                    .Where(x => deptAccountants.Contains(x.DepartmentId.Value) && userIds.Contains(x.UserId))
                                                    .Select(s => s.UserId).ToList();
            return accountants;
        }

        public List<string> GetBUHead(Guid? companyId, Guid? officeId)
        {
            var userIds = sysUserRepo.Get().Select(s => s.Id).ToList();
            var buHeads = sysUserLevelRepo.Get(x => x.GroupId == AccountingConstants.SpecialGroup
                                                    && x.Position == AccountingConstants.PositionManager
                                                    && x.DepartmentId == null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId)
                                                    .Where(w => userIds.Contains(w.UserId))
                                                    .Select(s => s.UserId).ToList();
            return buHeads;
        }
        #endregion -- LEADER, MANAGER, ACCOUNTANT, BOD --

        //Lấy ra BUHeadId của BUHead
        public string GetBUHeadId(string idBranch)
        {
            var buHeadId = "BU Head";
            return buHeadId;
        }

        //Lấy ra employeeId của User
        public string GetEmployeeIdOfUser(string userId)
        {
            return DataContext.Get(x => x.Id == userId).FirstOrDefault()?.EmployeeId;
        }

        //Lấy info Employee của User dựa vào employeeId
        public SysEmployee GetEmployeeByEmployeeId(string employeeId)
        {
            return sysEmployeeRepo.Get(x => x.Id == employeeId).FirstOrDefault();
        }

        //Lấy info Employee của User dựa vào userId
        public SysEmployee GetEmployeeByUserId(string userId)
        {
            var employeeId = GetEmployeeIdOfUser(userId);
            var data = sysEmployeeRepo.Get(x => x.Id == employeeId).FirstOrDefault();
            return data;
        }

        //Kiểm tra department có phải là department accountant hay không
        public bool CheckIsAccountantDept(int? deptId)
        {
            var isAccountantDept = catDepartmentRepo.Get(x => x.DeptType == AccountingConstants.DeptTypeAccountant && x.Id == deptId).Any();
            return isAccountantDept;
        }

        public bool CheckIsBOD(int? departmentId, Guid? officeId, Guid? companyId)
        {
            var isBod = sysUserLevelRepo.Get(x => x.GroupId == AccountingConstants.SpecialGroup
                                                    && x.DepartmentId == null
                                                    && x.OfficeId != null
                                                    && x.CompanyId != null
                                                    && x.DepartmentId == departmentId
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId
                                                    ).Select(s => s.UserId).Any();
            return isBod;
        }


        #region --- SETTING FLOW APPROVAL ---
        public SysSettingFlow GetSettingFlowApproval(string type, Guid? officeId)
        {
            var settingFlow = settingFlowRepo.Get(x => x.Flow == "Approval" && x.Type == type && x.OfficeId == officeId).FirstOrDefault();
            return settingFlow;
        }

        public string GetRoleByLevel(string level, string type, Guid? officeId)
        {
            var role = string.Empty;
            switch (level)
            {
                case "Leader":
                    role = GetSettingFlowApproval(type, officeId)?.Leader;
                    break;
                case "Manager":
                    role = GetSettingFlowApproval(type, officeId)?.Manager;
                    break;
                case "Accountant":
                    role = GetSettingFlowApproval(type, officeId)?.Accountant;
                    break;
                case "BOD":
                    role = GetSettingFlowApproval(type, officeId)?.Bod;
                    break;
                default:
                    break;
            }
            return role;
        }
        #endregion --- SETTING FLOW APPROVAL ---

        #region --- AUTHORIZED APPROVAL ---
        public List<string> GetAuthorizedApprovalByTypeAndAuthorizer(string type, string authorizer, Guid? officeCommissioner)
        {
            var userAuthorizedApprovals = authourizedApprovalRepo.Get(x => x.Type == type && x.Authorizer == authorizer && x.Active == true && x.OfficeCommissioner == officeCommissioner && (x.ExpirationDate ?? DateTime.Now.Date) >= DateTime.Now.Date).Select(x => x.Commissioner).ToList();
            return userAuthorizedApprovals;
        }
        #endregion  --- AUTHORIZED APPROVAL ---

        #region -- DEPUTY USER --
        public bool CheckUserSameLevel(string userId, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var result = false;
            if (groupId != null && departmentId != null && officeId != null && companyId != null)
            {
                return sysUserLevelRepo.Get(x => x.UserId == userId && x.GroupId == groupId && x.DepartmentId == departmentId && x.OfficeId == officeId && x.CompanyId == companyId).Any();
            }
            if (departmentId != null && officeId != null && companyId != null)
            {
                return sysUserLevelRepo.Get(x => x.UserId == userId && x.DepartmentId == departmentId && x.OfficeId == officeId && x.CompanyId == companyId).Any();
            }
            if (officeId != null && companyId != null)
            {
                return sysUserLevelRepo.Get(x => x.UserId == userId && x.OfficeId == officeId && x.CompanyId == companyId).Any();
            }
            return result;
        }

        public List<string> GetUsersDeputyByCondition(string type, string userId, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var userDeputies = new List<string>();
            if (string.IsNullOrEmpty(userId)) return userDeputies;
            //Get list user authorized of user by type & office
            var userAuthorizedApprovals = GetAuthorizedApprovalByTypeAndAuthorizer(type, userId, officeId);
            foreach (var userAuth in userAuthorizedApprovals)
            {
                userDeputies.Add(userAuth);
            }
            return userDeputies;
        }

        public List<string> GetEmailUsersDeputyByCondition(string type, string userId, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var users = GetUsersDeputyByCondition(type, userId, groupId, departmentId, officeId, companyId);
            var emailUserDeputies = new List<string>();
            foreach (var user in users)
            {
                var employeeIdOfUser = GetEmployeeIdOfUser(user);
                var email = GetEmployeeByEmployeeId(employeeIdOfUser)?.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    emailUserDeputies.Add(email);
                }
            }
            return emailUserDeputies;
        }

        public bool CheckIsUserDeputy(string type, string commissioner, string userId, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var deputies = GetUsersDeputyByCondition(type, userId, groupId, departmentId, officeId, companyId).Any(x => x == commissioner);
            return deputies;
        }

        #endregion -- DEPUTY USER --
        /// <summary>
        /// Kiểm tra Department của user có phải là department Accountant hay không theo office ID & department ID
        /// </summary>
        /// <param name="officeId">Office ID của User</param>
        /// <param name="deptId">Department ID của User</param>
        /// <returns></returns>
        public bool CheckIsAccountantByOfficeDept(Guid? officeId, int? deptId)
        {
            var isDeptAccountant = catDepartmentRepo.Get(x => x.DeptType == AccountingConstants.DeptTypeAccountant
                                                           && x.Id == deptId
                                                           && x.BranchId == officeId).Any();
            return isDeptAccountant;
        }

        /// <summary>
        /// Check is User Admin
        /// </summary>
        /// <param name="currUserId">ID of Current User</param>
        /// <param name="currOfficeId">Office ID of Current User</param>
        /// <param name="currCompanyId">Company ID of Current User</param>
        /// <param name="objOfficeId">Office ID of Object</param>
        /// <param name="objCompanyId">Company ID of Object</param>
        /// <returns></returns>
        public bool CheckIsUserAdmin(string currUserId, Guid currOfficeId, Guid currCompanyId, Guid? objOfficeId, Guid? objCompanyId)
        {
            var result = false;
            var user = DataContext.Get(x => x.Id == currUserId).FirstOrDefault();
            if (user != null)
            {
                if (user.UserType == "Super Admin")
                {
                    result = true;
                }
                if (objOfficeId != null && objCompanyId != null)
                {
                    if (user.UserType == "Local Admin" && currOfficeId == objOfficeId && currCompanyId == objCompanyId)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Get Department of user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<int?> GetDepartmentUser(Guid? companyId, Guid? officeId, string userId)
        {
            var deptAccountants = catDepartmentRepo.Get(s => s.DeptType == AccountingConstants.DeptTypeAccountant).Select(s => s.Id).ToList();
            var result = sysUserLevelRepo.Get(x => x.UserId == userId && x.CompanyId == companyId && x.OfficeId == officeId && deptAccountants.Any(z => z == x.DepartmentId))
                                                    .Select(s => s.DepartmentId).ToList();
            return result;
        }
    }
}
