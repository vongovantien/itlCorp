using AutoMapper;
using eFMS.API.Catalog.DL.Models;
using eFMS.API.Catalog.Models;
using eFMS.API.Catalog.Service.Models;

namespace eFMS.API.Catalog.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            //CreateMap<SysUserGroup, SysUserGroupModel>();
            //CreateMap<SysUserGroupModel, SysUserGroup>();
            CreateMap<SysUserGroupEditModel, SysUserGroupModel>();
        }
    }
}
