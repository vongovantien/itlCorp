using AutoMapper;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Report.DL.Services
{
    public class UserBaseService : RepositoryBase<SysUser, SysUserModel>, IUserBaseService
    {
        readonly IContextBase<SysUserLevel> sysUserLevelRepo;
        private readonly IContextBase<SysGroup> sysGroupRepo;
        private readonly IContextBase<CatDepartment> catDepartmentRepo;
        private readonly IContextBase<SysEmployee> sysEmployeeRepo;
        private readonly IContextBase<SysOffice> sysOfficeRepo;
        private readonly IContextBase<SysUser> sysUserRepo;

        public UserBaseService(
            IContextBase<SysUser> repository, 
            IMapper mapper,
            IContextBase<SysUserLevel> sysUserLevelRepo, 
            IContextBase<SysGroup> sysGroupRepo, 
            IContextBase<CatDepartment> catDepartmentRepo, 
            IContextBase<SysEmployee> sysEmployeeRepo, 
            IContextBase<SysOffice> sysOfficeRepo
            ) : base(repository, mapper)
        {
            this.sysUserLevelRepo = sysUserLevelRepo;
            this.sysGroupRepo = sysGroupRepo;
            this.catDepartmentRepo = catDepartmentRepo;
            this.sysEmployeeRepo = sysEmployeeRepo;
            this.sysOfficeRepo = sysOfficeRepo;
        }

        public List<string> GetAccoutantManager(Guid? companyId, Guid? officeId)
        {
            var deptAccountants = catDepartmentRepo.Get(s => s.DeptType == "ACCOUNTANT").Select(s => s.Id).ToList();
            var accountants = sysUserLevelRepo.Get(x => x.GroupId == 11
                                                    && x.OfficeId == officeId
                                                    && x.DepartmentId != null
                                                    && x.CompanyId == companyId
                                                    && x.Position == "Manager-Leader")
                                                    .Where(x => deptAccountants.Contains(x.DepartmentId.Value))
                                                    .Select(s => s.UserId).ToList();
            return accountants;
        }

        public List<string> GetCompanyManager(Guid? companyId)
        {
            var companyManager = sysUserLevelRepo.Get(x => x.GroupId == 11
                                                    && x.DepartmentId == null
                                                    && x.OfficeId == null
                                                    && x.CompanyId == companyId
                                                    && x.Position == "Manager-Leader")
                                                    .Select(s => s.UserId).ToList();
            return companyManager;
        }

        public List<string> GetDeptManager(Guid? companyId, Guid? officeId, int? departmentId)
        {
            var managers = sysUserLevelRepo.Get(x => x.GroupId == 11
                                                    && x.Position == "Manager-Leader"
                                                    && x.DepartmentId == departmentId
                                                    && x.DepartmentId != null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId).Select(s => s.UserId).ToList();
            return managers;
        }

        public List<string> GetOfficeManager(Guid? companyId, Guid? officeId)
        {
            var officeManager = sysUserLevelRepo.Get(x => x.GroupId == 11
                                                    && x.DepartmentId == null
                                                    && x.OfficeId == officeId
                                                    && x.CompanyId == companyId
                                                    && x.Position == "Manager-Leader")
                                                    .Select(s => s.UserId).ToList();
            return officeManager;
        }
    }
}
