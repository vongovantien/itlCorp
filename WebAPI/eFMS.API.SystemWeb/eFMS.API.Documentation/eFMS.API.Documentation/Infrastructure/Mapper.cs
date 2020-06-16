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
            CreateMap<CsManifest, CsManifestModel>();
            CreateMap<CsShippingInstruction, CsShippingInstructionModel>();
            CreateMap<OpsTransaction, OpsTransactionModel>();
            CreateMap<CsArrivalFrieghtCharge, CsArrivalFrieghtChargeModel>();
            CreateMap<CsTransactionDetailAddManifest, CsTransactionDetail>();
            CreateMap<CsTransactionDetail, CsTransactionDetailAddManifest>();
            CreateMap<CsDimensionDetail, CsDimensionDetailModel>();
            CreateMap<SysImage, SysImageModel>();
            CreateMap<CsAirWayBill, CsAirWayBillModel>().ForMember(x => x.DimensionDetails, opt => opt.Ignore())
                .ForMember(x => x.OtherCharges, opt => opt.Ignore());
            CreateMap<CsShipmentSurcharge, CsShipmentSurchargeDetailsModel>();
            CreateMap<CsBookingNote, CsBookingNoteEditModel>();
            CreateMap<CsBookingNote, CsBookingNoteModel>();
            CreateMap<spc_GetListChargeShipmentMaster, ChargeShipmentModel>();
            CreateMap<spc_GetSurchargeByHouseBill, CsShipmentSurchargeDetailsModel>();
            CreateMap<AcctCdnote, AcctCdnoteModel>();
            CreateMap<CatCurrencyExchange, CatCurrencyExchangeModel>();

            //map to entity model
            CreateMap<CsAirWayBillModel, CsAirWayBill>();
            CreateMap<CsTransactionEditModel, CsTransaction>();
            CreateMap<CsTransactionDetailModel, CsTransactionDetail>();
            CreateMap<CsMawbcontainerModel, CsMawbcontainer>();
            CreateMap<CsManifestEditModel, CsManifest>();
            CreateMap<CsShippingInstructionModel, CsShippingInstruction>();
            CreateMap<CsShipmentSurchargeModel, CsShipmentSurcharge>();
            CreateMap<CsBookingNoteEditModel, CsBookingNote>();
            CreateMap<CsShipmentSurchargeModel, CsShipmentSurcharge>();
            CreateMap<CsMawbcontainerImportModel, CsMawbcontainer>();
            CreateMap<CsDimensionDetailModel, CsDimensionDetail>();
            CreateMap<AcctCdnoteModel, AcctCdnote>();
            CreateMap<CsArrivalFrieghtChargeModel, CsArrivalFrieghtCharge>();
            CreateMap<CsMawbcontainerModel, CsMawbcontainer>();
            CreateMap<CsShipmentOtherChargeModel, CsShipmentOtherCharge>();
            CreateMap<OpsTransactionModel, OpsTransaction>();
        }
    }
}
