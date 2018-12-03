using AutoMapper;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.ViewModels;
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
            CreateMap<CatPartnerGroup, CatPartnerGroupModel>();
            CreateMap<CatPlaceEditModel, CatPlaceModel>();
            CreateMap<CatCommodityGroupEditModel, CatCommodityGroupModel>();
            CreateMap<CatCommodityEditModel, CatCommodityModel>();
            CreateMap<CatPartnerEditModel, CatPartnerModel>();

            CreateMap<CatCurrencyModel, CatCurrency>();
            CreateMap<CatCommodityGroupModel, CatCommodityGroup>();
        }
    }
}
