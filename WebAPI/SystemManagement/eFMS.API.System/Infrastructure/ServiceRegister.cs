using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using SystemManagement.DL.Infrastructure.ErrorHandler;
using SystemManagement.DL.IService;
using SystemManagement.DL.Services;
using SystemManagementAPI.Service.Contexts;
using SystemManagementAPI.Service.Models;

namespace SystemManagementAPI.API.Infrastructure
{
     public static class ServiceRegister
    {

        public static void Register(IServiceCollection services)
        {
            services.AddTransient<ISysEmployeeService, SysEmployeeService>();
            services.AddTransient<IErrorHandler, ErrorHandler>();
            services.AddTransient<IContextBase<SysEmployee>, Base<SysEmployee>>();
            //Mon.Trung Start
            services.AddTransient<ISysTemplateService, SysTemplateService>();
            services.AddTransient<IContextBase<SysTemplate>, Base<SysTemplate>>();

            services.AddTransient<ISysTemplateDetailService, SysTemplateDetailService>();
            services.AddTransient<IContextBase<SysTemplateDetail>, Base<SysTemplateDetail>>();
            //SysAuthorization
            services.AddTransient<ISysAuthorizationService, SysAuthorizationService>();
            services.AddTransient<IContextBase<SysAuthorization>, Base<SysAuthorization>>();
            //SysAuthorizationDetail
            services.AddTransient<ISysAuthorizationDetailService, SysAuthorizationDetailService>();
            services.AddTransient<IContextBase<SysAuthorizationDetail>, Base<SysAuthorizationDetail>>();
            //CatBranch
            services.AddTransient<ICatBranchService, CatBranchService>();
            services.AddTransient<IContextBase<CatBranch>, Base<CatBranch>>();
            //CatHub
            services.AddTransient<ICatHubService, CatHubService>();
            services.AddTransient<IContextBase<CatHub>, Base<CatHub>>();
            
            //ICatPartnerService
            services.AddTransient<ICatPartnerService, CatPartnerService>();
            services.AddTransient<IContextBase<CatPartner>, Base<CatPartner>>();
            //ICatVehicleTypeService
            services.AddTransient<ICatVehicleTypeService, CatVehicleTypeService>();
            services.AddTransient<IContextBase<CatVehicleType>, Base<CatVehicleType>>();
            //ICatPlaceService
            services.AddTransient<ICatPlaceService, CatPlaceService>();
            services.AddTransient<IContextBase<CatPlace>, Base<CatPlace>>();
            //ICatDepartment
            services.AddTransient<ICatDepartmentService, CatDepartmentService>();
            services.AddTransient<IContextBase<CatDepartment>, Base<CatDepartment>>();
            //ICatPosition
            services.AddTransient<ICatPositionService, CatPositionService>();
            services.AddTransient<IContextBase<CatPosition>, Base<CatPosition>>();
            //ICatSaleResource
            services.AddTransient<ICatSaleResourceService, CatSaleResourceService>();
            services.AddTransient<IContextBase<CatSaleResource>, Base<CatSaleResource>>();
            //SysBaseEnum
            services.AddTransient<ISysBaseEnumService, SysBaseEnumService>();
            services.AddTransient<IContextBase<SysBaseEnum>, Base<SysBaseEnum>>();
            //SysBaseEnumDetail
            services.AddTransient<ISysBaseEnumDetailService, SysBaseEnumDetailService>();
            services.AddTransient<IContextBase<SysBaseEnumDetail>, Base<SysBaseEnumDetail>>();
            //CatCustomerGoodsDescription
            services.AddTransient<ICatCustomerGoodsDescriptionService, CatCustomerGoodsDescriptionService>();
            services.AddTransient<IContextBase<CatCustomerGoodsDescription>, Base<CatCustomerGoodsDescription>>();
            //CatCustomerPlace
            services.AddTransient<ICatCustomerPlaceService, CatCustomerPlaceService>();
            services.AddTransient<IContextBase<CatCustomerPlace>, Base<CatCustomerPlace>>();
            //CatCustomerShipmentNote
            services.AddTransient<ICatCustomerShipmentNoteService, CatCustomerShipmentNoteService>();
            services.AddTransient<IContextBase<CatCustomerShipmentNote>, Base<CatCustomerShipmentNote>>();
            //Mon.Trung End

            services.AddScoped<ISysMenuService, SysMenuService>();
            services.AddScoped<IContextBase<SysMenu>, Base<SysMenu>>();

            services.AddScoped<ISysUserRoleService, SysUserRoleService>();
            services.AddScoped<IContextBase<SysUserRole>, Base<SysUserRole>>();


            services.AddScoped<ISysUserService, SysUserService>();
            services.AddScoped<IContextBase<SysUser>, Base<SysUser>>();

            services.AddScoped<ICatShipmentTypeService, CatShipmentTypeService>();
            services.AddScoped<IContextBase<CatShipmentType>, Base<CatShipmentType>>();
        }
    }
}
