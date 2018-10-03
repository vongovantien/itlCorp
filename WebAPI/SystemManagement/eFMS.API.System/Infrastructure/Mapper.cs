using AutoMapper;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;

namespace eFMS.API.System.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<SysUserGroup, SysUserGroupModel>();
            CreateMap<SysUserGroupModel, SysUserGroup>();
        }
    }
}
