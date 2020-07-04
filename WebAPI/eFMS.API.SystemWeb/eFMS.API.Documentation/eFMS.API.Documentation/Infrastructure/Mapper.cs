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
            //CreateMap<sp_GetOpsTransaction, OpsTransactionModel>();
            CreateMap<CsArrivalFrieghtCharge, CsArrivalFrieghtChargeModel>();
            CreateMap<CsTransactionDetailAddManifest, CsTransactionDetail>();
            CreateMap<CsTransactionDetail, CsTransactionDetailAddManifest>();
            CreateMap<CsDimensionDetail, CsDimensionDetailModel>();
            CreateMap<SysImage, SysImageModel>();
            CreateMap<CsAirWayBill, CsAirWayBillModel>().ForMember(x => x.DimensionDetails, opt => opt.Ignore())
                .ForMember(x => x.OtherCharges, opt => opt.Ignore());
            CreateMap<CsAirWayBillModel, CsAirWayBill>();


            CreateMap<CsTransactionEditModel, CsTransaction>();
            CreateMap<CsTransactionDetailModel, CsTransactionDetail>();
            CreateMap<CsMawbcontainerModel, CsMawbcontainer>();
            CreateMap<CsShipmentSurcharge, CsShipmentSurchargeDetailsModel>();
            CreateMap<CsManifestEditModel, CsManifest>();
            CreateMap<CsShippingInstructionModel, CsShippingInstruction>();
            CreateMap<CsShipmentSurchargeModel, CsShipmentSurcharge>();
            CreateMap<CsBookingNoteEditModel, CsBookingNote>();
            CreateMap<CsBookingNote, CsBookingNoteEditModel>();
            CreateMap<CsBookingNote, CsBookingNoteModel>();

            CreateMap<CsShippingInstruction, CsShippingInstructionReportConstModel>();


            CreateMap<spc_GetListChargeShipmentMaster, ChargeShipmentModel>();

            CreateMap<CsShipmentSurchargeModel, CsShipmentSurcharge>();

            CreateMap<CsMawbcontainerImportModel, CsMawbcontainer>();

            CreateMap<spc_GetSurchargeByHouseBill, CsShipmentSurchargeDetailsModel>();

            CreateMap<AcctCdnote, AcctCdnoteModel>();

            CreateMap<CsDimensionDetailModel, CsDimensionDetail>();
            
            CreateMap<CatCurrencyExchange, CatCurrencyExchangeModel>();
        }
    }
}
