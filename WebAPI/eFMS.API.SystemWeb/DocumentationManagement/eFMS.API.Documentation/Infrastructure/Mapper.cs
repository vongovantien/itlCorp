using AutoMapper;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using System.Collections.Generic;

namespace eFMS.API.Shipment.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            CreateMap<CsTransaction, CsTransactionEditModel>();
            CreateMap<CsTransactionDetail, CsTransactionDetailModel>();
            CreateMap<CsMawbcontainer, CsMawbcontainerModel>();
            CreateMap<CsTransaction, CsTransactionModel>();
            //CreateMap<CsTransactionDetail, CsTransactionDetailReport>();
            CreateMap<CsManifest, CsManifestModel>();
            CreateMap<CsShippingInstruction, CsShippingInstructionModel>();
            CreateMap<OpsTransaction, OpsTransactionModel>();
            CreateMap<sp_GetOpsTransaction, OpsTransactionModel>();
            CreateMap<CsArrivalFrieghtCharge, CsArrivalFrieghtChargeModel>();


            CreateMap<CsTransactionEditModel, CsTransaction>();
            CreateMap<CsTransactionDetailModel, CsTransactionDetail>();
            CreateMap<CsMawbcontainerModel, CsMawbcontainer>();
            CreateMap<CsShipmentSurcharge, CsShipmentSurchargeDetailsModel>();
            CreateMap<CsManifestEditModel, CsManifest>();
            CreateMap<CsShippingInstructionModel, CsShippingInstruction>();
            CreateMap<CsShipmentSurchargeModel, CsShipmentSurcharge>();

            CreateMap<spc_GetListChargeShipmentMaster, ChargeShipmentModel>();

            CreateMap<CsShipmentSurchargeModel, CsShipmentSurcharge>();

            CreateMap<CsMawbcontainerImportModel, CsMawbcontainer>();

            CreateMap<spc_GetSurchargeByHouseBill, CsShipmentSurchargeDetailsModel>();
        }
    }
}
