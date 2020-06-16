using AutoMapper;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.Service.Models;
using eFMS.API.Operation.Service.ViewModels;

namespace eFMS.API.Operation.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            //map entity to view model
            CreateMap<OpsStageAssigned, OpsStageAssignedModel>();
            CreateMap<OpsStageAssignedEditModel, OpsStageAssigned>();
            CreateMap<OpsStageAssignedEditModel, OpsStageAssignedModel>();
            CreateMap<SetEcusconnection, SetEcusConnectionModel>();
            CreateMap<CustomsDeclaration, CustomsDeclarationModel>();
            CreateMap<CustomsDeclarationModel, CustomsDeclaration>();
            CreateMap<sp_GetCustomDeclaration, CustomsDeclarationModel>();
            CreateMap<OpsTransaction, OpsTransactionModel>();

            // map to entity model
            CreateMap<CustomsDeclarationModel, CustomsDeclaration>();
            CreateMap<SetEcusConnectionModel, SetEcusconnection>();
        }
    }
}
