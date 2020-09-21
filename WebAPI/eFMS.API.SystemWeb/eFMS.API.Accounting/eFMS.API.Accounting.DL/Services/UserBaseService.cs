using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.Services
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

        //Lấy ra ds các user được ủy quyền theo nhóm leader, manager department, accountant manager, BUHead dựa vào dept
        public List<string> GetListUserDeputyByDept(string dept)
        {
            Dictionary<string, string> listUsers = new Dictionary<string, string> {
                 { "7569a3ec-7d1c-41a6-9b02-79f7d13f0dc8", AccountingConstants.DEPT_CODE_OPS },//User ủy quyền cho dept OPS
                 { "bc34e764-2fc3-4d7c-9d5e-7c6dc56a208f", AccountingConstants.DEPT_CODE_ACCOUNTANT },//User ủy quyền cho dept Accountant
                 { "5983f430-dc42-4276-8997-4c1fec1fa739", AccountingConstants.DEPT_CODE_ACCOUNTANT },//User ủy quyền cho dept Accountant

                 { "9945b553-1461-4cdd-8686-be4894736f58", AccountingConstants.DEPT_CODE_ACCOUNTANT },//User ủy quyền cho dept Accountant
                 { "3b46e144-469f-46b0-b514-0efd181ed840", AccountingConstants.DEPT_CODE_ACCOUNTANT },//User ủy quyền cho dept Accountant
                 { "81a2abcf-3b1a-4892-909d-604c23667b7d", AccountingConstants.DEPT_CODE_ACCOUNTANT },//User ủy quyền cho dept Accountant
                 { "197f66bf-f0a1-4449-9c25-a202ae50e8f9", AccountingConstants.DEPT_CODE_ACCOUNTANT },//User ủy quyền cho dept Accountant
                 { "ce7a4c55-a52c-4d55-8fed-e60ba68d75e5", AccountingConstants.DEPT_CODE_ACCOUNTANT },//User ủy quyền cho dept Accountant
                 { "850e6069-b5ea-4ab4-8864-a2332237b148", AccountingConstants.DEPT_CODE_ACCOUNTANT }, //User ủy quyền cho dept Accountant
                 { "5a9f3b02-a105-4189-ab9e-0606bd089314", AccountingConstants.DEPT_CODE_ACCOUNTANT } //User ủy quyền cho dept Accountant
            };
            var list = listUsers.ToList();
            var deputy = listUsers.Where(x => x.Value == dept).Select(x => x.Key).ToList();
            return deputy;
        }

        public bool CheckDeputyManagerByUser(int? departmentId, string userId)
        {
            var result = false;
            //Lấy ra dept code của user dựa vào user
            var deptCodeOfUser = GetInfoDeptOfUser(departmentId)?.Code;
            if (!string.IsNullOrEmpty(deptCodeOfUser) && deptCodeOfUser != AccountingConstants.DEPT_CODE_ACCOUNTANT)
            {
                result = GetListUserDeputyByDept(deptCodeOfUser).Contains(userId) ? true : false;
            }
            return result;
        }

        public bool CheckDeputyAccountantByUser(int? departmentId, string userId)
        {
            var result = false;
            //Lấy ra dept code của user dựa vào user
            var deptCodeOfUser = GetInfoDeptOfUser(departmentId)?.Code;
            var deptTypeOfDept = GetInfoDeptOfUser(departmentId)?.DeptType;
            if (!string.IsNullOrEmpty(deptCodeOfUser) && deptTypeOfDept == AccountingConstants.DeptTypeAccountant)
            {
                result = GetListUserDeputyByDept(deptCodeOfUser).Contains(userId) ? true : false;
            }
            return result;
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
        public List<string> GetAuthorizedApprovalByTypeAndAuthorizer(string type, string authorizer)
        {
            var userAuthorizedApprovals = authourizedApprovalRepo.Get(x => x.Type == type && x.Authorizer == authorizer && x.Active == true && (x.ExpirationDate ?? DateTime.Now.Date) >= DateTime.Now.Date).Select(x => x.Commissioner).ToList();
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
            //Get list user authorized of user
            var userAuthorizedApprovals = GetAuthorizedApprovalByTypeAndAuthorizer(type, userId);
            foreach (var userAuth in userAuthorizedApprovals)
            {
                //var isSame = CheckUserSameLevel(userAuth, groupId, departmentId, officeId, companyId);
                //if (isSame)
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

        public bool CheckIsUserDeputy(string type, string commissioner, string userId, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var deputies = GetUsersDeputyByCondition(type, userId, groupId, departmentId, officeId, companyId).Where(x => x == commissioner);
            return deputies.Any();
        }

        #endregion -- DEPUTY USER --
    }
}
