using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ITL.NetCore.Common;

namespace eFMS.API.System.DL.Services
{
    public class SysUserPermissionService : RepositoryBase<SysUserPermission, SysUserPermissionModel>, ISysUserPermissionService
    {
        private ISysUserPermissionGeneralService userPermissionGeneralService;
        private ISysUserPermissionSpecialService userPermissionSpecialService;
        private IContextBase<SysUser> userRepository;
        private IContextBase<SysEmployee> employeeRepository;
        private IContextBase<SysOffice> officeRepository;
        private IContextBase<SysPermissionSample> permissionSampleRepository;

        public SysUserPermissionService(IContextBase<SysUserPermission> repository, IMapper mapper,
            ISysUserPermissionGeneralService userPermissionGeneral,
            ISysUserPermissionSpecialService userPermissionSpecial,
            IContextBase<SysUser> userRepo,
            IContextBase<SysEmployee> employeeRepo,
            IContextBase<SysOffice> officeRepo,
            IContextBase<SysPermissionSample> permissionSampleRepo) : base(repository, mapper)
        {
            userPermissionGeneralService = userPermissionGeneral;
            userPermissionSpecialService = userPermissionSpecial;
            userRepository = userRepo;
            employeeRepository = employeeRepo;
            officeRepository = officeRepo;
            permissionSampleRepository = permissionSampleRepo;
        }

        public SysUserPermissionModel GetBy(string userId, Guid officeId)
        {
            var permission = Get(x => x.UserId == userId && x.OfficeId == officeId)?.FirstOrDefault();
            var employeeId = userRepository.Get(x => x.Id == userId)?.FirstOrDefault()?.EmployeeId;
            permission.UserTitle = employeeId == null ? null : employeeRepository.Get(x => x.Id == employeeId)?.FirstOrDefault()?.Title;
            permission.OfficeName = officeRepository.Get(x => x.Id == officeId)?.FirstOrDefault()?.BranchNameEn;
            permission.PermissionName = permissionSampleRepository.Get(x => x.Id == permission.PermissionSampleId)?.FirstOrDefault()?.Name;
            if (permission == null) return permission;
            permission.SysUserPermissionGenerals = userPermissionGeneralService.GetBy(permission.Id);
            permission.SysUserPermissionSpecials = userPermissionSpecialService.GetBy(permission.Id);
            return permission;
        }

        public SysUserPermissionModel Get(Guid id)
        {
            var permission = Get(x => x.Id == id)?.FirstOrDefault();
            var employeeId = userRepository.Get(x => x.Id == permission.UserId)?.FirstOrDefault()?.EmployeeId;
            permission.UserTitle = employeeId == null ? null : employeeRepository.Get(x => x.Id == employeeId)?.FirstOrDefault()?.Title;
            permission.OfficeName = officeRepository.Get(x => x.Id == permission.OfficeId)?.FirstOrDefault()?.BranchNameEn;
            permission.PermissionName = permissionSampleRepository.Get(x => x.Id == permission.PermissionSampleId)?.FirstOrDefault()?.Name;
            if (permission == null) return permission;
            permission.SysUserPermissionGenerals = userPermissionGeneralService.GetBy(permission.Id);
            permission.SysUserPermissionSpecials = userPermissionSpecialService.GetBy(permission.Id);
            return permission;
        }

        private SysUserPermissionModel GetUserPermissionDefault(Guid permissionSampleId)
        {
            var permissionSample = permissionSampleRepository.Get(x => x.Id == permissionSampleId)?.FirstOrDefault();
            SysUserPermissionModel result = new SysUserPermissionModel();
            if (permissionSample != null)
            {
                result = mapper.Map<SysUserPermissionModel>(permissionSample);
                result.PermissionName = permissionSample.Name;
                result.PermissionSampleId = permissionSampleId;
            }
            result.SysUserPermissionGenerals = userPermissionGeneralService.GetUserGeneralPermissionDefault(permissionSampleId);
            result.SysUserPermissionSpecials = userPermissionSpecialService.GetUserGeneralSpecialDefault(permissionSampleId);
            return result;
        }
    }
}
