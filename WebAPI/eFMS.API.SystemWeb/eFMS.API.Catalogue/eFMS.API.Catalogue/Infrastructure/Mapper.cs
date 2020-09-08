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
            CreateMap<CatContract, CatContractModel>();
            CreateMap<CatContract, CatContractViewModel>();

            CreateMap<CatContractEditModel, CatContractModel>();
            CreateMap<CatChartOfAccounts, CatChartOfAccountsModel>();
            CreateMap<CatChartOfAccountsImportModel, CatChartOfAccounts>();
            CreateMap<CatIncotermModel, CatIncoterm>();
            CreateMap<CatChargeIncoterm, CatChargeIncotermModel>();
            CreateMap<CatPotentialModel, CatPotential>();

            //map to entity model
            CreateMap<CatPartnerChargeModel, CatPartnerCharge>();
            CreateMap<CatContractModel, CatSaleman>();
            CreateMap<CatCurrencyModel, CatCurrency>();
            CreateMap<CatUnitModel, CatUnit>();
            CreateMap<CatCountryModel, CatCountry>();
            CreateMap<CatCommodityModel, CatCommodity>();
            CreateMap<CatPlaceModel, CatPlace>();
            CreateMap<CatPartnerImportModel, CatPartner>();
            CreateMap<CatStageModel, CatStage>();
            CreateMap<CatPartnerModel, CatPartner>();
            CreateMap<CatIncoterm, CatIncotermModel>();
            CreateMap<CatChargeIncotermModel, CatChargeIncoterm>();
            CreateMap<CatPotential, CatPotentialModel>();

        }
    }
}
