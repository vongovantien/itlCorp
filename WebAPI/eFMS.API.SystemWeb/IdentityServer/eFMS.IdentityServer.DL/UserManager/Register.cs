using AutoMapper;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Contexts;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.DL.Services;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.DL.UserManager
{
    public static class Register
    {
        public static IServiceCollection AddUserManager(this IServiceCollection services)
        {
            return services
                    .AddAutoMapper()

                    .AddScoped<IHttpContextAccessor, HttpContextAccessor>()
                    .AddScoped<ICurrentUser, CurrentUser>()
                    .AddTransient<IContextBase<SysUser>, Base<SysUser>>().
                    AddTransient<IUserPermissionService, SysUserPermissionService>()
                    .AddTransient<IContextBase<SysUserPermission>, Base<SysUserPermission>>()
                    .AddTransient<IContextBase<SysUserPermissionGeneral>, Base<SysUserPermissionGeneral>>()
                    .AddTransient<IContextBase<SysUserPermissionSpecial>, Base<SysUserPermissionSpecial>>();

        }
    }

    public class MappingProfileService : Profile
    {
        public MappingProfileService()
        {
            CreateMap<SysUserLogModel, SysUserLog>();
            CreateMap<SysEmployee, EmployeeModel>();
            CreateMap<SysUser, UserModel>();
            CreateMap<SysUserLog, SysUserLogModel>();
        }
    }
}
