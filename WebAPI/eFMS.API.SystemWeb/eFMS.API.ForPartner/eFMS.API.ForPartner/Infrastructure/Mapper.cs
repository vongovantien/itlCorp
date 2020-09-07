using AutoMapper;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;

namespace eFMS.API.ForPartner.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<AccAccountingManagement, AccAccountingManagementModel>();


            // Map to entity model
            CreateMap<AccAccountingManagementModel, AccAccountingManagement>();
        }
    }
}
