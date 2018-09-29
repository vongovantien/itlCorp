using AutoMapper;
using SystemManagementAPI.Service.Models;
using SystemManagement.DL.Models;

namespace SystemManagementAPI.API.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<SysEmployeeModel, SysEmployee>();
            CreateMap<SysEmployee, SysEmployeeModel>();
            CreateMap<SysMenuModel, SysMenu>();
            CreateMap<SysMenu, SysMenuModel>();
            CreateMap<CatShipmentTypeModel, CatShipmentType>();
            CreateMap<CatShipmentType, CatShipmentTypeModel>();
        }
    }
}
