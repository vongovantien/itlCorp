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
using eFMS.API.System.DL.ViewModels;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.DL.UserManager;

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
        private IContextBase<SysMenu> sysMenuRepository;

        private readonly ICurrentUser currentUser;

        public SysUserPermissionService(IContextBase<SysUserPermission> repository, IMapper mapper,
            ISysUserPermissionGeneralService userPermissionGeneral,
            ISysUserPermissionSpecialService userPermissionSpecial,
            IContextBase<SysUser> userRepo,
            IContextBase<SysEmployee> employeeRepo,
            IContextBase<SysOffice> officeRepo,
            IContextBase<SysPermissionSample> permissionSampleRepo,
            IContextBase<SysPermissionSampleGeneral> permissionSampleGeneralRepo,
            IContextBase<SysUserPermissionSpecial> userPermissionSpecialRepo,
            IContextBase<SysMenu> sysMenuRepo,
            IContextBase<SysUserPermissionGeneral> userPermissionGeneralRepo, ICurrentUser icurrentUser) : base(repository, mapper)
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
            currentUser = icurrentUser;
            sysMenuRepository = sysMenuRepo;

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
            permission.SysPermissionSampleGenerals = userPermissionGeneralService.GetBy(permission.Id);
            permission.SysPermissionSampleSpecials = userPermissionSpecialService.GetBy(permission.Id);
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
            permission.SysPermissionSampleGenerals = userPermissionGeneralService.GetBy(permission.Id);
            permission.SysPermissionSampleSpecials = userPermissionSpecialService.GetBy(permission.Id);
            return permission;
        }
        public HandleState Add(List<SysUserPermissionEditModel> list)
        {
            Contract.Ensures(Contract.Result<HandleState>() != null);
            List<SysUserPermission> userPermissions = new List<SysUserPermission>();
            List<SysUserPermissionGeneral> permissionGenerals = null;
            List<SysUserPermissionSpecial> permissionSpecials = null;
            foreach (var item in list)
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
                        if (permissionGenerals.Count > 0)
                        {
                            var hsGeneral = userPermissionGeneralRepository.Add(permissionGenerals);
                        }
                        if (permissionSpecials.Count > 0)
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


        public HandleState Update(SysUserPermissionModel entity)
        {
            var permission = mapper.Map<SysUserPermission>(entity);
            permission.UserModified = currentUser.UserID;
            permission.DatetimeModified = DateTime.Now;
            var result = DataContext.Update(permission, x => x.Id == entity.Id, false);
            if (result.Success)
            {
                foreach (var item in entity.SysPermissionSampleGenerals)
                {
                    var list = mapper.Map<List<SysUserPermissionGeneralModel>>(item.SysPermissionGenerals);
                    foreach (var general in list)
                    {
                        general.UserModified = currentUser.UserID;
                        userPermissionGeneralService.Update(general, x => x.Id == general.Id, false);
                    }
                }
                foreach (var item in entity.SysPermissionSampleSpecials)
                {
                    foreach (var per in item.SysPermissionSpecials)
                    {
                        foreach (var s in per.PermissionSpecialActions)
                        {
                            if (s.Id == Guid.Empty)
                            {
                                var peritem = mapper.Map<SysUserPermissionSpecial>(s);
                                peritem.Id = s.Id;
                                peritem.IsAllow = s.IsAllow;
                                peritem.MenuId = s.MenuId;
                                peritem.ModuleId = s.ModuleId;
                                peritem.ActionName = s.NameEn;
                                peritem.UserPermissionId = entity.Id;
                                userPermissionSpecialRepository.Add(peritem, false);
                            }
                            else
                            {
                                var peritem = userPermissionSpecialRepository.First(x => x.Id == s.Id);
                                peritem.IsAllow = s.IsAllow;
                                peritem.UserModified = currentUser.UserID;
                                var t = userPermissionSpecialRepository.Update(peritem, x => x.Id == s.Id, false);
                            }
                        }
                    }
                }
                DataContext.SubmitChanges();
                userPermissionGeneralService.SubmitChanges();
                userPermissionSpecialRepository.SubmitChanges();
            }
            return result;
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

        public UserPermissionModel GetPermission(string userId, Guid officeId, string route)
        {
            var menu = sysMenuRepository.Get(m => m.Route == route).ToList();
            if(menu.Count() == 0)
            {
                return new UserPermissionModel
                {
                    Access = false
                };
            }
            else
            {
                string menuId = menu.Select(m => m.Id).First();

                var userPermissionId = DataContext.Get(x => x.UserId == userId && x.OfficeId == officeId)?.FirstOrDefault()?.Id;
                if (userPermissionId == null) return null;
                var generalPermission = userPermissionGeneralRepository.Get(x => x.MenuId == menuId && x.UserPermissionId == userPermissionId)?.FirstOrDefault();
                if (generalPermission == null) return null;
                var specialPermissions = userPermissionSpecialRepository.Get(x => x.MenuId == menuId && x.UserPermissionId == userPermissionId).ToList();
                var result = new UserPermissionModel
                {
                    MenuId = menuId,
                    Access = generalPermission.Access,
                    Detail = generalPermission.Detail,
                    Write = generalPermission.Write,
                    Delete = generalPermission.Delete,
                    List = generalPermission.List,
                    Import = generalPermission.Import,
                    Export = generalPermission.Export,
                    SpecialActions = specialPermissions?.Select(x => new SpecialAction
                    {
                        Action = x.ActionName,
                        IsAllow = x.IsAllow
                    }).ToList()
                };
                return result;
            }

            
        }
    }
}
