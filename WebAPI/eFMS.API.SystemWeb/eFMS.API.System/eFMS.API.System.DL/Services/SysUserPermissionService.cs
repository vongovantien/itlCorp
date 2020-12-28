using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using ITL.NetCore.Common;
using System.Diagnostics.Contracts;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Common.Models;

namespace eFMS.API.System.DL.Services
{
    public class SysUserPermissionService : RepositoryBase<SysUserPermission, SysUserPermissionModel>, ISysUserPermissionService
    {
        private ISysUserPermissionGeneralService userPermissionGeneralService;
        private ISysUserPermissionSpecialService userPermissionSpecialService;
        private IContextBase<SysUser> userRepository;
        private IContextBase<SysEmployee> employeeRepository;
        private IContextBase<SysOffice> officeRepository;
        private IContextBase<SysCompany> companyRepository;

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
            IContextBase<SysUserPermissionGeneral> userPermissionGeneralRepo, ICurrentUser icurrentUser, IContextBase<SysCompany> companyRepo, IContextBase<SysPermissionSampleSpecial> permissionSampleSpecialRepo, IContextBase<SysMenu> sysMenuRepo) : base(repository, mapper)

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
            companyRepository = companyRepo;
            permissionSampleSpecialRepository = permissionSampleSpecialRepo;
            sysMenuRepository = sysMenuRepo;
        }

        public IQueryable<SysUserPermissionModel> GetByUserId(string id)
        {
            var dataOffice = officeRepository.Get();
            var dataCompany = companyRepository.Get();
            var data = DataContext.Get(x => x.UserId == id.ToString());
            var permissionSample = permissionSampleRepository.Get();
            var results = from d in data
                          join p in permissionSample on d.PermissionSampleId equals p.Id into grpSample
                          from sample in grpSample.DefaultIfEmpty()
                          join o in dataOffice on d.OfficeId equals o.Id
                          join c in dataCompany on o.Buid equals c.Id
                          select new SysUserPermissionModel
                          {
                              Id = d.Id,
                              Name = sample.Name,
                              OfficeId = d.OfficeId,
                              PermissionSampleId = d.PermissionSampleId,
                              Buid = c.Id,
                              CompanyName = c.BunameVn,
                              CompanyAbbrName = c.BunameAbbr,
                              OfficeName = o.BranchNameVn,
                              OfficeAbbrName = o.ShortName,
                          };

            return results;

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
            permission.UserName = userRepository.Get(x => x.Id == permission.UserId).Select(t => t.Username).FirstOrDefault();
            var userCreated = userRepository.Get(x => x.Id == permission.UserCreated).FirstOrDefault();
            var userModified = userRepository.Get(x => x.Id == permission.UserModified).FirstOrDefault();
            permission.NameUserCreated = userCreated?.Username;
            permission.NameUserModified = userModified?.Username;
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

            permission.UserName = userRepository.Get(x => x.Id == permission.UserId).Select(t => t.Username).FirstOrDefault();
            var userCreated = userRepository.Get(x => x.Id == permission.UserCreated).FirstOrDefault();
            var userModified = userRepository.Get(x => x.Id == permission.UserModified).FirstOrDefault();

            permission.NameUserCreated = userCreated?.Username;
            permission.NameUserModified = userModified?.Username;


            return permission;
        }
        public HandleState Add(List<SysUserPermissionEditModel> list)
        {
            HandleState result = new HandleState();
            List<SysUserPermissionEditModel> listAdd = new List<SysUserPermissionEditModel>();

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (SysUserPermissionEditModel item in list)
                    {
                        if (item.Id == Guid.Empty)
                        {
                            // Trong một office user chỉ có 1 bộ quyền
                            List<SysUserPermission> sysUserPerInOffice = DataContext.Get(x => x.UserId == item.UserId && x.OfficeId == item.OfficeId).ToList();
                            if (sysUserPerInOffice.Count == 0)
                            {
                                listAdd.Add(item);
                            }
                        }
                    }

                    if (listAdd.Count > 0)
                    {
                        List<SysUserPermissionGeneral> permissionGenerals = null;
                        List<SysUserPermissionSpecial> permissionSpecials = null;
                        foreach (var item in listAdd)
                        {
                            SysUserPermission userPermission = mapper.Map<SysUserPermission>(item);
                            userPermission.Id = Guid.NewGuid();
                            userPermission.DatetimeCreated = userPermission.DatetimeModified = DateTime.Now;

                            DataContext.Add(userPermission, false);

                            // Lấy bộ quyền General và special từ bộ chuẩn
                            permissionGenerals = GetPermissionGeneralDefault(item.PermissionSampleId, userPermission.Id);
                            permissionSpecials = GetPermissionSpecilaDefault(item.PermissionSampleId, userPermission.Id);

                            if (permissionGenerals.Count > 0)
                            {
                                HandleState hsGeneral = userPermissionGeneralRepository.Add(permissionGenerals, false);
                            }
                            if (permissionSpecials.Count > 0)
                            {
                                HandleState hsSpecial = userPermissionSpecialRepository.Add(permissionSpecials, false);
                            }
                        }

                        result = DataContext.SubmitChanges();

                        userPermissionGeneralRepository.SubmitChanges();
                        userPermissionSpecialRepository.SubmitChanges();

                        trans.Commit();
                    }

                    return result;
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
                        general.DatetimeModified = DateTime.Now;
                        if (general.Id != Guid.Empty)
                        {
                            var hs = userPermissionGeneralService.Update(general, x => x.Id == general.Id, false);
                        }
                        else
                        {
                            var hs = userPermissionGeneralService.Add(general, false);
                        }
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
                                peritem.Id = Guid.NewGuid();
                                peritem.IsAllow = s.IsAllow;
                                peritem.MenuId = s.MenuId;
                                peritem.ModuleId = s.ModuleId;
                                peritem.ActionName = s.ActionName;
                                peritem.UserPermissionId = entity.Id;
                                peritem.UserModified = currentUser.UserID;
                                peritem.DatetimeModified = DateTime.Now;
                                var hs = userPermissionSpecialRepository.Add(peritem, false);
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

        private List<SysUserPermissionSpecial> GetPermissionSpecilaDefault(Guid permissionSampleId, Guid userPermissionId)
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
                        UserPermissionId = userPermissionId,
                        ModuleId = special.ModuleId,
                        MenuId = special.MenuId,
                        ActionName = special.ActionName,
                        IsAllow = special.IsAllow,
                        UserModified = currentUser.UserID,
                        DatetimeModified = DateTime.Now
                    };
                    permissionSpecials.Add(userSpecial);
                }
            }
            return permissionSpecials;
        }

        private List<SysUserPermissionGeneral> GetPermissionGeneralDefault(Guid permissionSampleId, Guid userPermissionId)
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
                        UserPermissionId = userPermissionId,
                        MenuId = general.MenuId,
                        Access = general.Access,
                        Detail = general.Detail,
                        Write = general.Write,
                        Delete = general.Delete,
                        List = general.List,
                        Import = general.Import,
                        Export = general.Export,
                        DatetimeModified = DateTime.Now,
                        UserModified = currentUser.UserID
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
                    userPermissionGeneralRepository.SubmitChanges();
                    userPermissionSpecialRepository.SubmitChanges();
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
            if (menu.Count() == 0)
            {
                return new UserPermissionModel
                {
                    Access = false
                };
            }
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
                AllowAdd = generalPermission.Write == "None" ? false : true,
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
