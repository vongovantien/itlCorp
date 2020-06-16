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
            // map to view model
            CreateMap<CatPlace, CatPlaceModel>();
            CreateMap<CatStage, CatStageModel>();
            CreateMap<CatPartnerGroup, CatPartnerGroupModel>();
            CreateMap<CatPlaceEditModel, CatPlaceModel>();
            CreateMap<CatCommodityGroupEditModel, CatCommodityGroupModel>();
            CreateMap<CatCommodityEditModel, CatCommodityModel>();
            CreateMap<CatPartnerEditModel, CatPartnerModel>();
            CreateMap<CatPartner, CatPartnerModel>();
            CreateMap<CatPartner, CatPartnerViewModel>();
            CreateMap<CatPartnerModel, CatPartnerViewModel>();
            CreateMap<CatPartner, CatPartnerModel>();
            CreateMap<CatPartnerCharge, CatPartnerChargeModel>();
            CreateMap<CatPlace, CatPlaceViewModel>();
            CreateMap<CatCommodity, CatCommodityModel>();
            CreateMap<CatUnit, CatUnitModel>();
            CreateMap<CatCommodityGroupModel, CatCommodityGroup>();
            CreateMap<CatSaleman, CatSaleManModel>();
            CreateMap<CatSaleman, CatSaleManViewModel>();

            CreateMap<CatSaleManEditModel, CatSaleManModel>();
            CreateMap<CatChartOfAccounts, CatChartOfAccountsModel>();
            CreateMap<CatChartOfAccountsImportModel, CatChartOfAccounts>();

            //map to entity model
            CreateMap<CatPartnerChargeModel, CatPartnerCharge>();
            CreateMap<CatSaleManModel, CatSaleman>();
            CreateMap<CatCurrencyModel, CatCurrency>();
            CreateMap<CatUnitModel, CatUnit>();
            CreateMap<CatCountryModel, CatCountry>();
            CreateMap<CatCommodityModel, CatCommodity>();
            CreateMap<CatPlaceModel, CatPlace>();
            CreateMap<CatPartnerImportModel, CatPartner>();
            CreateMap<CatStageModel, CatStage>();
            CreateMap<CatPartnerModel, CatPartner>();


        }
    }
}
