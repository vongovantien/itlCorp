using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Service.Models;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eFMS.IdentityServer
{
    public class ProfileService : IProfileService
    {
        readonly IAuthenUserService authenUserService;
        readonly ISysEmployeeService employeeService;
        public ProfileService(IAuthenUserService service, ISysEmployeeService emService)
        {
            authenUserService = service;
            employeeService = emService;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
            var user = authenUserService.GetUserById(subjectId);
            var employee = employeeService.First(x => x.Id == user.EmployeeId);

            var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Id, user.Id.ToString()),
                    new Claim(JwtClaimTypes.Email, user.Email),                   
                    new Claim(JwtClaimTypes.PreferredUserName,user.Username),
                    new Claim(JwtClaimTypes.PhoneNumber,user.Tel??""),
                    //new Claim("workplaceId",user.WorkPlaceId?.ToString()),
                    new Claim("userName", user.Username),
                    new Claim("employeeId", employee.Id)
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
