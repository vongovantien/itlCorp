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
    public class ClaimsExtender : IClaimsTransformation
    {
        private IUserPermissionService _userpermissionService;
        public ClaimsExtender(IUserPermissionService userpermissionService)
        {
            _userpermissionService = userpermissionService;
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
                var lstPermissions = _userpermissionService.GetPermission(userID, officeId);
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
    }
}