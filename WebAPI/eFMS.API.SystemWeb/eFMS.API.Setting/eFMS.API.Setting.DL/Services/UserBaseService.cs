using AutoMapper;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Setting.DL.Services
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

        public UserBaseService(
            IContextBase<SysUser> repository,
            IMapper mapper,
            IContextBase<SysUserLevel> sysUserLevel,
            IContextBase<SysGroup> sysGroup,
            IContextBase<CatDepartment> catDepartment,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<SysOffice> sysOffice,
            IContextBase<SysSettingFlow> settingFlow,
            IContextBase<SysAuthorizedApproval> authourizedApproval) : base(repository, mapper)
        {
            sysUserLevelRepo = sysUserLevel;
            sysGroupRepo = sysGroup;
            catDepartmentRepo = catDepartment;
            sysEmployeeRepo = sysEmployee;
            sysOfficeRepo = sysOffice;
            settingFlowRepo = settingFlow;
            authourizedApprovalRepo = authourizedApproval;
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

        //Lấy Info Dept của User
        public CatDepartment GetInfoDeptOfUser(int? departmentId)
        {
            var deptOfUser = catDepartmentRepo.Get(x => x.Id == departmentId).FirstOrDefault();
            return deptOfUser;
        }

        #region -- LEADER, MANAGER, ACCOUNTANT, BOD --
        public List<string> GetLeaderGroup(Guid? companyId, Guid? officeId, int? departmentId, int? groupId)
        {
            var leaders = sysUserLevelRepo.Get(x => x.Position == SettingConstants.PositionManager
                                                    && x.GroupId == groupId
                                                    && x.DepartmentId == departmentId
                                                    && x.DepartmentId != null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId).Select(s => s.UserId).ToList();
            return leaders;
        }

        public List<string> GetDeptManager(Guid? companyId, Guid? officeId, int? departmentId)
        {
            var managers = sysUserLevelRepo.Get(x => x.GroupId == SettingConstants.SpecialGroup
                                                    && x.Position == SettingConstants.PositionManager
                                                    && x.DepartmentId == departmentId
                                                    && x.DepartmentId != null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId).Select(s => s.UserId).ToList();
            return managers;
        }

        public List<string> GetAccoutantManager(Guid? companyId, Guid? officeId)
        {
            var deptAccountants = catDepartmentRepo.Get(s => s.DeptType == SettingConstants.DeptTypeAccountant).Select(s => s.Id).ToList();
            var accountants = sysUserLevelRepo.Get(x => x.GroupId == SettingConstants.SpecialGroup
                                                    && x.Position == SettingConstants.PositionManager
                                                    && x.OfficeId == officeId
                                                    && x.DepartmentId != null
                                                    && x.CompanyId == companyId)
                                                    .Where(x => deptAccountants.Contains(x.DepartmentId.Value))
                                                    .Select(s => s.UserId).ToList();
            return accountants;
        }

        public List<string> GetBUHead(Guid? companyId, Guid? officeId)
        {
            var buHeads = sysUserLevelRepo.Get(x => x.GroupId == SettingConstants.SpecialGroup
                                                    && x.Position == SettingConstants.PositionManager
                                                    && x.DepartmentId == null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId).Select(s => s.UserId).ToList();
            return buHeads;
        }
        #endregion -- LEADER, MANAGER, ACCOUNTANT, BOD --

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
            var isAccountantDept = catDepartmentRepo.Get(x => x.DeptType == SettingConstants.DeptTypeAccountant && x.Id == deptId).Any();
            return isAccountantDept;
        }

        public bool CheckIsBOD(int? departmentId, Guid? officeId, Guid? companyId)
        {
            var isBod = sysUserLevelRepo.Get(x => x.GroupId == SettingConstants.SpecialGroup
                                                    && x.DepartmentId == null
                                                    && x.OfficeId != null
                                                    && x.CompanyId != null
                                                    && x.DepartmentId == departmentId
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId
                                                    ).Select(s => s.UserId).Any();
            return isBod;
        }

        #region --- SETTING FLOW UNLOCK ---
        public SysSettingFlow GetSettingFlowUnlock(string type, Guid? officeId)
        {
            type = (type == "Change Service Date") ? "Shipment" : type;
            var settingFlow = settingFlowRepo.Get(x => x.Flow == "Unlock" && x.Type == type && x.OfficeId == officeId).FirstOrDefault();
            return settingFlow;
        }

        public string GetRoleByLevel(string level, string type, Guid? officeId)
        {
            var role = string.Empty;
            switch (level)
            {
                case "Leader":
                    role = GetSettingFlowUnlock(type, officeId)?.Leader;
                    break;
                case "Manager":
                    role = GetSettingFlowUnlock(type, officeId)?.Manager;
                    break;
                case "Accountant":
                    role = GetSettingFlowUnlock(type, officeId)?.Accountant;
                    break;
                case "BOD":
                    role = GetSettingFlowUnlock(type, officeId)?.Bod;
                    break;
                default:
                    break;
            }
            return role;
        }
        #endregion --- SETTING FLOW UNLOCK ---

        #region --- AUTHOIRIZED APPROVAL ---
        public List<string> GetAuthorizedApprovalByTypeAndAuthorizer(string type, string authorizer)
        {
            var userAuthorizedApprovals = authourizedApprovalRepo.Get(x => x.Type == type && x.Authorizer == authorizer && x.Active == true && (x.ExpirationDate ?? DateTime.Now.Date) >= DateTime.Now.Date).Select(x => x.Commissioner).ToList();
            return userAuthorizedApprovals;
        }
        #endregion  --- AUTHOIRIZED APPROVAL ---

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
            var _typeAuthApr = (type == "Change Service Date") ? "Shipment" : type;
            //Get list user authorized of user
            var userAuthorizedApprovals = GetAuthorizedApprovalByTypeAndAuthorizer(_typeAuthApr, userId);

            foreach (var userAuth in userAuthorizedApprovals)
            {
                var isSame = CheckUserSameLevel(userAuth, groupId, departmentId, officeId, companyId);
                if (isSame)
                {
                    userDeputies.Add(userAuth);
                }
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

        public bool CheckIsUserDeputy(string type, string userId, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var deputies = GetUsersDeputyByCondition(type, userId, groupId, departmentId, officeId, companyId);
            return deputies.Any();
        }

        #endregion -- DEPUTY USER --
    }
}
