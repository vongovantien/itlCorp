using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.IService;
using IdentityModel;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace eFMS.API.Shipment.Infrastructure
{
    public interface IClaimsExtender {
        List<string> GetPermission(string userId, Guid officeId);
    }
    public class ClaimsExtender : IClaimsExtender, IClaimsTransformation
    {
        private IContextBase<SysUserPermission> userPermissionRepository;
        private IContextBase<SysUserPermissionGeneral> userPermissionGeneralRepository;
        private IContextBase<SysUserPermissionSpecial> userPermissionSpecialRepository;
        public ClaimsExtender(IContextBase<SysUserPermission> userPermissionRepo,
            IContextBase<SysUserPermissionGeneral> userPermissionGeneralRepo,
            IContextBase<SysUserPermissionSpecial> userPermissionSpecialRepo)
        {
            userPermissionRepository = userPermissionRepo;
            userPermissionGeneralRepository = userPermissionGeneralRepo;
            userPermissionSpecialRepository = userPermissionSpecialRepo;
        }

        Task<ClaimsPrincipal> IClaimsTransformation.TransformAsync(ClaimsPrincipal principal)
        {
            try
            {
                //string userID = principal.FindFirstValue("id");
                string userID = "admin";
                Guid officeId = new Guid("2FDCA3AC-6C54-434F-9D71-12F8F50B857B");
                List<Claim> lstClaim = new List<Claim>();
                // var s = userPermissionService.Get();
                var lstPermissions = GetPermission(userID, officeId);
                if (lstPermissions != null)
                {
                    lstPermissions.ForEach(x => lstClaim.Add(new Claim(JwtClaimTypes.Role, x)));
                }
                principal.AddIdentity(new ClaimsIdentity(lstClaim, JwtBearerDefaults.AuthenticationScheme, "name", "role"));
            }
            catch(Exception ex) {
                string s = ex.Message;
            }
            //Add additional claims here.
            return Task.FromResult(principal);
        }
        public List<string> GetPermission(string userId, Guid officeId)
        {
            List<string> results = new List<string>();
            var userPermissionId = userPermissionRepository.Get(x => x.UserId == userId && x.OfficeId == officeId)?.FirstOrDefault()?.Id;
            if (userPermissionId != null)
            {
                var generalPermissions = userPermissionGeneralRepository.Get(x => x.UserPermissionId == userPermissionId);
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

                var specialPermissions = userPermissionSpecialRepository.Get(x => x.UserPermissionId == userPermissionId);
                if (specialPermissions != null)
                {
                }
            }
            return results;
        }
    }
}