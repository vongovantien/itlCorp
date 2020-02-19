using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.IdentityServer.DL.Services
{
    public class SysUserPermissionService: IUserPermissionService
    {
        readonly IContextBase<SysUserPermission> userPermissionRepository;
        readonly IContextBase<SysUserPermissionGeneral> permissionGeneralRepository;
        readonly IContextBase<SysUserPermissionSpecial> permissionSpecialRepository;
        readonly IContextBase<SysAuthorization> authorizationRepository;
        
        public SysUserPermissionService(IContextBase<SysUserPermission> userPermissionRepo,
            IContextBase<SysUserPermissionGeneral> permissionGeneralRepo,
            IContextBase<SysUserPermissionSpecial> permissionSpecialRepo,
            IContextBase<SysAuthorization> authorizeRepo
            )
        {
            userPermissionRepository = userPermissionRepo;
            permissionGeneralRepository = permissionGeneralRepo;
            permissionSpecialRepository = permissionSpecialRepo;
            authorizationRepository = authorizeRepo;
        }

        public List<UserPermissionModel> Get(string userId, Guid officeId)
        {
            List<UserPermissionModel> results = new List<UserPermissionModel>();
            var userPermissionId = userPermissionRepository.Get(x => x.UserId == userId && x.OfficeId == officeId)?.FirstOrDefault()?.Id;
            if (userPermissionId != null)
            {
                var generalPermissions = permissionGeneralRepository.Get(x => x.UserPermissionId == userPermissionId);
                if (generalPermissions != null)
                {
                    foreach (var item in generalPermissions)
                    {
                        var general = new UserPermissionModel
                        {
                            MenuId = item.MenuId,
                            Access = item.Access,
                            Detail = item.Detail,
                            Write = item.Write,
                            Delete = item.Delete,
                            List = item.List,
                            Import = item.Import,
                            Export = item.Export,
                            AllowAdd = item.Write == "None"?false: true,
                            SpecialActions = permissionSpecialRepository.Get(x => x.UserPermissionId == userPermissionId && x.MenuId == item.MenuId)?
                                .Select(x => new SpecialAction
                                {
                                    Action = x.ActionName,
                                    IsAllow = x.IsAllow
                                }).ToList()
                        };
                        results.Add(general);
                    }
                }
            } 
            return results;
        }

        public List<string> GetAuthorizedIds(string transactionType, ICurrentUser currentUser)
        {
            List<string> authorizeUserIds = authorizationRepository.Get(x => x.Active == true
                                                                    && x.AssignTo == currentUser.UserID
                                                                    && (x.EndDate ?? DateTime.Now.Date) >= DateTime.Now.Date
                                                                    && x.Services.Contains(transactionType)
                                                                    )?.Select(x => x.UserId).ToList();
            return authorizeUserIds;
        }

        public async Task<List<string>> GetPermission(string userId, Guid officeId)
        {
            List<string> results = new List<string>();
            var userPermissionId = userPermissionRepository.Get(x => x.UserId == userId && x.OfficeId == officeId)?.FirstOrDefault()?.Id;
            if (userPermissionId != null)
            {
                var generalPermissions = permissionGeneralRepository.Get(x => x.UserPermissionId == userPermissionId);
                if (generalPermissions != null)
                {
                    foreach (var item in generalPermissions)
                    {
                        if (item.Access == true)
                        {
                            string role = string.Format("{0}.{1}", item.MenuId, UserPermission.AllowAccess);
                            results.Add(role);
                            role = string.Format("{0}.{1}", item.MenuId, UserPermission.Add);
                            results.Add(role);
                            role = string.Format("{0}.{1}", item.MenuId, UserPermission.Update);
                            results.Add(role);
                            role = string.Format("{0}.{1}", item.MenuId, UserPermission.Delete);
                            results.Add(role);
                            role = string.Format("{0}.{1}", item.MenuId, UserPermission.List);
                            results.Add(role);
                            role = string.Format("{0}.{1}", item.MenuId, UserPermission.Detail);
                            results.Add(role);
                        }
                        if (item.Import == true)
                        {
                            string role = string.Format("{0}.{1}", item.MenuId, UserPermission.Import);
                            results.Add(role);
                        }
                        if (item.Export == true)
                        {
                            string role = string.Format("{0}.{1}", item.MenuId, UserPermission.Export);
                            results.Add(role);
                        }
                    }
                }

                var specialPermissions = permissionSpecialRepository.Get(x => x.UserPermissionId == userPermissionId);
                if (specialPermissions != null)
                {
                }
            }
            return await Task.FromResult(results);
        }
    }
}
