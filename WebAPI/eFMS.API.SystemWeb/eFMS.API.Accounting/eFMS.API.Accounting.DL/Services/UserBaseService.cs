﻿using AutoMapper;
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
        public UserBaseService(
            IContextBase<SysUser> repository, 
            IMapper mapper,
            IContextBase<SysUserLevel> sysUserLevel,
            IContextBase<SysGroup> sysGroup,
            IContextBase<CatDepartment> catDepartment,
            IContextBase<SysEmployee> sysEmployee,
            IContextBase<SysOffice> sysOffice) : base(repository, mapper)
        {
            sysUserLevelRepo = sysUserLevel;
            sysGroupRepo = sysGroup;
            catDepartmentRepo = catDepartment;
            sysEmployeeRepo = sysEmployee;
            sysOfficeRepo = sysOffice;
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

        public List<string> GetDeptManager(Guid? companyId, Guid? officeId, int? departmentId)
        {
            var managers = sysUserLevelRepo.Get(x => x.GroupId == AccountingConstants.SpecialGroup
                                                    && x.Position == AccountingConstants.PositionManager
                                                    && x.DepartmentId == departmentId
                                                    && x.DepartmentId != null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId).Select(s => s.UserId).ToList();
            return managers;
        }

        public List<string> GetAccoutantManager(Guid? companyId, Guid? officeId)
        {
            var deptAccountants = catDepartmentRepo.Get(s => s.DeptType == AccountingConstants.DeptTypeAccountant).Select(s => s.Id).ToList();
            var accountants = sysUserLevelRepo.Get(x => x.GroupId == AccountingConstants.SpecialGroup
                                                    && x.Position == AccountingConstants.PositionManager
                                                    && x.OfficeId == officeId
                                                    && x.DepartmentId != null
                                                    && x.CompanyId == companyId)
                                                    .Where(x => deptAccountants.Contains(x.DepartmentId.Value))
                                                    .Select(s => s.UserId).ToList();
            return accountants;
        }

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
                 { "william.hiep", AccountingConstants.DEPT_CODE_OPS },//User ủy quyền cho dept OPS
                 { "linda.linh", AccountingConstants.DEPT_CODE_ACCOUNTANT },//User ủy quyền cho dept Accountant
                 { "christina.my", AccountingConstants.DEPT_CODE_ACCOUNTANT }//User ủy quyền cho dept Accountant
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
    }
}
