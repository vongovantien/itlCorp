using AutoMapper;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;

namespace eFMS.API.Catalogue.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            //CreateMap<SysUserGroup, SysUserGroupModel>();
            //CreateMap<SysUserGroupModel, SysUserGroup>();

            CreateMap<SysUser, SysUserViewModel>();
            CreateMap<SysBranch, SysBranchViewModel>();
            CreateMap<SysBranchEditModel, SysBranchModel>();



        }
    }
}
