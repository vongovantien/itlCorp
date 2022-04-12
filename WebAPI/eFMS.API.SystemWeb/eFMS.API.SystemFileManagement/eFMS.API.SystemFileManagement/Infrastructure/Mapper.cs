using AutoMapper;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Service.Models;

namespace eFMS.API.SystemFileManagement.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Map to entity model
            CreateMap<SysActionFuncLogModel, SysActionFuncLog>();
            CreateMap<SysImage, SysImageModel>().ReverseMap();
            //CreateMap<AccAccountReceivable, ReceivableTable>();
        }
    }
}
