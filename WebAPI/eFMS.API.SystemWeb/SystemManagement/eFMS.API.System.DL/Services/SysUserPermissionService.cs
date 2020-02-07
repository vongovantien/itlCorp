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
using System.Diagnostics.Contracts;

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

        private IContextBase<SysPermissionSampleGeneral> permissionSampleGeneralRepository;
        private IContextBase<SysPermissionSampleSpecial> permissionSampleSpecialRepository;
        private IContextBase<SysUserPermissionSpecial> userPermissionSpecialRepository;
        private IContextBase<SysUserPermissionGeneral> userPermissionGeneralRepository;

        public SysUserPermissionService(IContextBase<SysUserPermission> repository, IMapper mapper,
            ISysUserPermissionGeneralService userPermissionGeneral,
            ISysUserPermissionSpecialService userPermissionSpecial,
            IContextBase<SysUser> userRepo,
            IContextBase<SysEmployee> employeeRepo,
            IContextBase<SysOffice> officeRepo,
            IContextBase<SysPermissionSample> permissionSampleRepo,
            IContextBase<SysPermissionSampleGeneral> permissionSampleGeneralRepo,
            IContextBase<SysUserPermissionSpecial> userPermissionSpecialRepo,
            IContextBase<SysUserPermissionGeneral> userPermissionGeneralRepo) : base(repository, mapper)
        {
            userPermissionGeneralService = userPermissionGeneral;
            userPermissionSpecialService = userPermissionSpecial;
            userRepository = userRepo;
            employeeRepository = employeeRepo;
            officeRepository = officeRepo;
            permissionSampleRepository = permissionSampleRepo;
            permissionSampleGeneralRepository = permissionSampleGeneralRepo;
            userPermissionSpecialRepository = userPermissionSpecialRepo;
            userPermissionGeneralRepository = userPermissionGeneralRepo;
        }

        public SysUserPermissionModel GetBy(string userId, Guid officeId)
        {
            var permission = Get(x => x.UserId == userId && x.OfficeId == officeId)?.FirstOrDefault();
            if (permission == null) return permission;
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
        public HandleState Add(List<SysUserPermissionEditModel> list)
        {
            Contract.Ensures(Contract.Result<HandleState>() != null);
            List<SysUserPermission> userPermissions = new List<SysUserPermission>();
            List<SysUserPermissionGeneral> permissionGenerals = null;
            List<SysUserPermissionSpecial> permissionSpecials = null;
            foreach(var item in list)
            {
                var userPermission = mapper.Map<SysUserPermission>(item);
                userPermission.Id = Guid.NewGuid();
                userPermission.UserCreated = userPermission.UserModified = "admin";
                userPermission.DatetimeCreated = userPermission.DatetimeModified = DateTime.Now;
                userPermissions.Add(userPermission);
                permissionGenerals = GetPermissionGeneralDefault(item.PermissionSampleId);
                permissionSpecials = GetPermissionSpecilaDefault(item.PermissionSampleId);
            }
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Add(userPermissions);
                    if (hs.Success)
                    {
                        if(permissionGenerals.Count > 0)
                        {
                            var hsGeneral = userPermissionGeneralRepository.Add(permissionGenerals);
                        }
                        if(permissionSpecials.Count > 0)
                        {
                            var hsSpecial = userPermissionSpecialRepository.Add(permissionSpecials);
                        }
                    }
                    DataContext.SubmitChanges();
                    trans.Commit();
                    return hs;

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        private List<SysUserPermissionSpecial> GetPermissionSpecilaDefault(Guid permissionSampleId)
        {
            List<SysUserPermissionSpecial> permissionSpecials = null;
            var specialDefaults = permissionSampleSpecialRepository.Get(x => x.PermissionId == permissionSampleId);
            if (specialDefaults != null)
            {
                permissionSpecials = new List<SysUserPermissionSpecial>();
                foreach (var special in specialDefaults)
                {
                    var userSpecial = new SysUserPermissionSpecial
                    {
                        UserPermissionId = permissionSampleId,
                        ModuleId = special.ModuleId,
                        MenuId = special.MenuId,
                        ActionName = special.ActionName,
                        IsAllow = special.IsAllow,
                        UserModified = "admin",
                        DatetimeModified = DateTime.Now
                    };
                    permissionSpecials.Add(userSpecial);
                }
            }
            return permissionSpecials;
        }

        private List<SysUserPermissionGeneral> GetPermissionGeneralDefault(Guid permissionSampleId)
        {
            List<SysUserPermissionGeneral> permissionGenerals = null;
            var generalDefaults = permissionSampleGeneralRepository.Get(x => x.PermissionId == permissionSampleId);
            if (generalDefaults != null)
            {
                permissionGenerals = new List<SysUserPermissionGeneral>();
                foreach (var general in generalDefaults)
                {
                    var userGeneral = new SysUserPermissionGeneral
                    {
                        UserPermissionId = permissionSampleId,
                        MenuId = general.MenuId,
                        Access = general.Access,
                        Detail = general.Detail,
                        Write = general.Write,
                        Delete = general.Delete,
                        List = general.List,
                        Import = general.Import,
                        Export = general.Export,
                        DatetimeModified = DateTime.Now,
                        UserModified = "admin"
                    };
                    permissionGenerals.Add(userGeneral);
                }
            }
            return permissionGenerals;
        }

        public HandleState Delete(Guid id)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Delete(x => x.Id == id);
                    if (hs.Success)
                    {
                        var hsGeneral = userPermissionGeneralRepository.Delete(x => x.UserPermissionId == id);
                        var hsspecial = userPermissionSpecialRepository.Delete(x => x.UserPermissionId == id);
                    }
                    DataContext.SubmitChanges();
                    trans.Commit();
                    return hs;

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
    }
}
