using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Service.Models;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eFMS.IdentityServer
{
    public class ProfileService : IProfileService
    {
        readonly IAuthenUserService authenUserService;
        protected ISysUserLogService userLogService;
        public ProfileService(IAuthenUserService service,ISysUserLogService _userLogService)
        {
            authenUserService = service;
            userLogService = _userLogService;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
            var user = authenUserService.GetUserById(subjectId);
            var userInfo = new SysUserLogModel
            {
                LoggedInOn = DateTime.Now,
                UserId = user.Id,
                WorkPlaceId = user.WorkPlaceId
            };
            userLogService.Add(userInfo);

            var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Id, user.Id.ToString()),
                    new Claim(JwtClaimTypes.Email, user.Email),                   
                    new Claim(JwtClaimTypes.PreferredUserName,user.Username),
                    new Claim(JwtClaimTypes.PhoneNumber,user.Tel??""),
                    new Claim("workplaceId",user.WorkPlaceId.ToString()??"")
                };

            context.IssuedClaims = claims;

     
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var user = authenUserService.GetUserById(context.Subject.GetSubjectId());
            context.IsActive = (user != null);// && !user.InActive;
            return Task.FromResult(0);
        }
    }
}
