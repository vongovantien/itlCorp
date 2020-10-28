using AutoMapper;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Models;
using eFMS.API.System.Service.Models;

namespace eFMS.API.Catalogue.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects

            //map entity model to view model
            CreateMap<SysUser, SysUserViewModel>();
            CreateMap<CatDepartment, CatDepartmentModel>();
            CreateMap<SysOffice, SysOfficeViewModel>();
            CreateMap<SysOffice, SysOfficeModel>();
            CreateMap<SysGroup, SysGroupModel>();
            CreateMap<SysImage, SysImageModel>();
            CreateMap<SysUserLevel, SysUserLevelModel>();
            CreateMap<SysPermissionSample, SysPermissionSampleModel>();
            CreateMap<SysPermissionSampleGeneral, SysPermissionSampleGeneralModel>();
            CreateMap<SysPermissionSampleSpecial, SysPermissionSampleSpecialModel>();
            CreateMap<SysAuthorization, SysAuthorizationModel>();
            CreateMap<SysAuthorizedApproval, SysAuthorizedApprovalModel>();
            CreateMap<SysMenu, MenuUserModel>();
            CreateMap<SysUserPermission, SysUserPermissionModel>();
            CreateMap<SysPermissionSample, SysUserPermissionModel>();
            CreateMap<SysUserPermissionGeneral, SysUserPermissionGeneralModel>();
            CreateMap<SysCompany, SysCompanyModel>();
            CreateMap<SysUser, SysUserModel>();
            CreateMap<SysSettingFlow, SysSettingFlowModel>();
            CreateMap<SetLockingDateShipment, SetLockingDateShipmentModel>();
            CreateMap<SysPartnerApi, SysPartnerAPIModel>();
            CreateMap<SysNotifications, SysNotificationsModel>();
            CreateMap<SysUserNotification, SysUserNotificationModel>();


            //map view model to entity model
            CreateMap<SysOfficeEditModel, SysOfficeModel>();
            CreateMap<SysGroupModel, SysGroup>();
            CreateMap<SysEmployeeModel, SysEmployee>();
            CreateMap<SysPermissionSampleModel, SysPermissionSample>();
            CreateMap<SysPermissionSampleGeneralModel, SysPermissionSampleGeneral>();
            CreateMap<SysEmployee, SysEmployeeModel>();
            CreateMap<SysPermissionSampleGeneralViewModel, SysPermissionSampleGeneral>();
            CreateMap<SysPermissionSampleSpecialModel, SysPermissionSampleSpecial>();
            CreateMap<UserPermissionSpecialAction, SysUserPermissionSpecial>();
            CreateMap<PermissionSpecialAction, SysPermissionSampleSpecial>();
            CreateMap<SysUserPermissionEditModel, SysUserPermission>();
            CreateMap<CatDepartmentModel, CatDepartment>();
            CreateMap<SysAuthorizationModel, SysAuthorization>();
            CreateMap<SysEmployeeModel, SysEmployee>();
            CreateMap<SysUserLevelModel, SysUserLevel>();
            CreateMap<SysUserModel, SysUser>();
            CreateMap<SysUserPermissionGeneralModel, SysUserPermissionGeneral>();
            CreateMap<SysSettingFlowModel,SysSettingFlow >();
            CreateMap<SetLockingDateShipmentModel, SetLockingDateShipment>();
            CreateMap<SysPartnerAPIModel, SysPartnerApi>();
            CreateMap<SysNotificationsModel, SysNotifications>();
            CreateMap<SysUserNotificationModel, SysUserNotification>();




        }
    }
}
