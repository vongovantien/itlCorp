using AutoMapper;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;

namespace eFMS.API.Setting.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            // map entity to view model
            CreateMap<SetTariff, SetTariffModel>();
            CreateMap<SetTariffDetail, SetTariffDetailModel>();
            CreateMap<SetTariff, TariffViewModel>();
            CreateMap<SetTariffModel, TariffViewModel>();

            // map to entity model
            CreateMap<SetTariffModel, SetTariff>();
            CreateMap<SetTariffDetailModel, SetTariffDetail>();

            CreateMap<SetUnlockRequest, SetUnlockRequestModel>();
            CreateMap<SetUnlockRequestJob, SetUnlockRequestJobModel>();
        }
    }
}
