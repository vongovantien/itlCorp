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
            CreateMap<SysGroup, SysGroupModel>();
            CreateMap<SysImage, SysImageModel>();
            CreateMap<SysUserGroup, SysUserGroupModel>();

            //map view model to entity model
            CreateMap<SysOfficeEditModel, SysOfficeModel>();

        }
    }
}
