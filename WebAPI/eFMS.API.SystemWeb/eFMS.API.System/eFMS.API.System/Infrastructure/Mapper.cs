﻿using AutoMapper;
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

            //map view model to entity model
            CreateMap<SysOfficeEditModel, SysOfficeModel>();
            CreateMap<SysGroupModel, SysGroup>();
            CreateMap<SysEmployeeModel, SysEmployee>();
            CreateMap<SysPermissionSampleModel, SysPermissionSample>();
            CreateMap<SysPermissionSampleGeneralModel, SysPermissionSampleGeneral>();
            CreateMap<SysUser, SysUserModel>();
            CreateMap<SysEmployee, SysEmployeeModel>();
            CreateMap<SysPermissionSampleGeneralViewModel, SysPermissionSampleGeneral>();
            CreateMap<SysPermissionSampleSpecialModel, SysPermissionSampleSpecial>();
            CreateMap<SysMenu, MenuUserModel>();
            CreateMap<SysUserPermission, SysUserPermissionModel>();
            CreateMap<SysPermissionSample, SysUserPermissionModel>();
            CreateMap<SysUserPermissionGeneral, SysUserPermissionGeneralModel>();
            CreateMap<SysCompany, SysCompanyModel>();

            CreateMap<UserPermissionSpecialAction, SysUserPermissionSpecial>();

            CreateMap<PermissionSpecialAction, SysPermissionSampleSpecial>();


            CreateMap<SysUserPermissionEditModel, SysUserPermission>();


        }
    }
}
