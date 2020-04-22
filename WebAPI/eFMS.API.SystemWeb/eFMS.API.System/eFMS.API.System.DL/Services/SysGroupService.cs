﻿using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysGroupService : RepositoryBase<SysGroup, SysGroupModel>, ISysGroupService
    {
        private readonly IContextBase<CatDepartment> departmentRepository;
        private readonly ICatDepartmentService departmentService;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUserLevel> sysLevelRepository;
        private readonly IContextBase<SysUser> userRepository;

        public SysGroupService(IContextBase<SysGroup> repository,
            IMapper mapper,
            IContextBase<CatDepartment> departmentRepo,
            ICatDepartmentService deptService,
            IContextBase<SysUserLevel> userLevelRepo,
            ICurrentUser currUser,
            IContextBase<SysUser> userRepo) : base(repository, mapper)
        {
            SetChildren<SysUserLevel>("Id", "GroupId");
            departmentRepository = departmentRepo;
            departmentService = deptService;
            currentUser = currUser;
            sysLevelRepository = userLevelRepo;
            userRepository = userRepo;
        }

        public SysGroupModel GetById(short id)
        {
            var group = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (group == null) return null;
            var result = mapper.Map<SysGroupModel>(group);
            if (group.DepartmentId != null)
            {
                var department = departmentService.GetDepartmentById((int)group.DepartmentId);
                result.DepartmentName = department.DeptNameEn;
                result.CompanyName = department.CompanyName;
                result.OfficeName = department.OfficeName;
                result.CompanyId = department.CompanyId;
                result.OfficeId = department.BranchId;
                result.NameUserCreated = userRepository.Get(x => x.Id == department.UserCreated).FirstOrDefault()?.Username;
                result.NameUserModified = userRepository.Get(x => x.Id == department.UserModified).FirstOrDefault()?.Username;
            }
            return result;
        }

        public IQueryable<SysGroupModel> Paging(SysGroupCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);

            rowsCount = (data.Count() > 0) ? data.Count() : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            return data;
        }

        public IQueryable<SysGroupModel> Query(SysGroupCriteria criteria)
        {
            IQueryable<SysGroup> groups = null;
            IQueryable<CatDepartment> departments = null;
            if (criteria.All == null)
            {
                groups = DataContext.Get(x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && (x.NameEn ?? "").IndexOf(criteria.NameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && (x.NameVn ?? "").IndexOf(criteria.NameVN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && (x.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && (x.DepartmentId == criteria.DepartmentId || criteria.DepartmentId == 0)
                                               && (x.Id == criteria.Id || criteria.Id == 0)
                                        );
                departments = departmentRepository.Get(x => (x.DeptNameEn ?? "").IndexOf(criteria.DepartmentName ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }
            else
            {
                groups = DataContext.Get(x => (x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               || (x.NameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               || (x.NameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               || (x.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        //|| (x.DepartmentId == criteria.DepartmentId || criteria.DepartmentId == 0)
                                        //|| (x.Id == criteria.Id || criteria.Id == 0)
                                        );
                departments = departmentRepository.Get(x => (x.DeptNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1);
                if (departments.Count() == 0)
                {
                    departments = departmentRepository.Get();
                }
            }
            if (groups == null) return null;
            groups = groups.Where(x => x.IsSpecial == false || x.IsSpecial == null);
            var results = groups.Join(departments, x => x.DepartmentId, y => y.Id, (x, y) => new SysGroupModel
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameVn = x.NameVn,
                ShortName = x.ShortName,
                DepartmentId = x.DepartmentId,
                ParentId = x.ParentId,
                ManagerId = x.ManagerId,
                UserCreated = x.UserCreated,
                UserModified = x.UserModified,
                DatetimeCreated = x.DatetimeCreated,
                DatetimeModified = x.DatetimeModified,
                Active = x.Active,
                InactiveOn = x.InactiveOn,
                DepartmentName = y.DeptNameEn
            }).OrderByDescending(x => x.DatetimeModified);
            return results;
        }

        public IQueryable<SysGroupModel> GetGroupByDepartment(int id)
        {
            try
            {
                IQueryable<SysGroupModel> results = null;

                var group = DataContext.Get(x => x.DepartmentId == id);
                var department = departmentService.GetDepartmentById(id);

                results = group.Select(item => new SysGroupModel
                {
                    Id = item.Id,
                    Code = item.Code,
                    NameEn = item.NameEn, // Group name.
                    NameVn = item.NameVn,
                    DepartmentId = item.DepartmentId,
                    DepartmentName = department.DeptNameEn,
                    CompanyName = department.CompanyName,
                    OfficeName = department.OfficeName
                });

                return results;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IQueryable<CatDepartmentGroupCriteria> GetGroupDepartmentPermission(string userId, Guid officeId)
        {
            try
            {
                IQueryable<CatDepartmentGroupCriteria> results = null;
                // Các department user đc phân.
                var currentUserDepartments = sysLevelRepository.Get(lv => lv.UserId == userId && lv.OfficeId == officeId && lv != null)?.Select(l => l.DepartmentId).ToList();
                if (currentUserDepartments.Count() > 0)
                {
                    // các groups user đc phân
                    var currentUserGroups = sysLevelRepository.Get(lv => lv.UserId == userId && lv.DepartmentId == currentUserDepartments.First())?.Select(l => l.GroupId).ToList(); 
                    if (currentUserGroups.Count() > 0)
                    {
                        var query = from lv in sysLevelRepository.Get(lv => lv.UserId == userId && lv.OfficeId == officeId)
                                    join dp in departmentRepository.Get() on lv.DepartmentId equals dp.Id
                                    join sg in DataContext.Get() on lv.GroupId equals sg.Id
                                    select new CatDepartmentGroupCriteria
                                    {
                                        UserId = lv.UserId,
                                        DepartmentId = (int)lv.DepartmentId,
                                        DepartmentName = dp.DeptNameAbbr,
                                        GroupId = lv.GroupId,
                                        GroupName = sg.ShortName,
                                    };
                        results =  query;
                    }
                }
                else // user k có department | group.
                {
                    return null;
                }

                return results;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override HandleState Add(SysGroupModel entity)
        {
            var item = mapper.Map<SysGroup>(entity);
            item.UserCreated = item.UserModified = currentUser.UserID;
            item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
            var hs = DataContext.Add(item, true);
            if (hs.Success)
            {
                entity.Id = (short)DataContext.Get(x => x.Code == entity.Code)?.FirstOrDefault()?.Id;
            }
            DataContext.SubmitChanges();
            return hs;
        }
    }
}
