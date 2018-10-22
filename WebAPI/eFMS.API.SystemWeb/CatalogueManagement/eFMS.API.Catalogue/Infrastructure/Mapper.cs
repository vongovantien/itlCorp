using AutoMapper;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Models;
using eFMS.API.Catalogue.Service.Models;

namespace eFMS.API.Catalogue.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            //CreateMap<SysUserGroup, SysUserGroupModel>();
            //CreateMap<SysUserGroupModel, SysUserGroup>();
            CreateMap<CatPlace, CatPlaceModel>();
            CreateMap<CatPlaceEditModel, CatPlaceModel>();
        }
    }
}
