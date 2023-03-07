﻿using AutoMapper;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;

namespace eFMS.API.Shipment.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
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
            CreateMap<CsBookingNote, CsBookingNoteEditModel>();
            CreateMap<CsBookingNote, CsBookingNoteModel>();
            CreateMap<CsShippingInstruction, CsShippingInstructionReportConstModel>();
            CreateMap<spc_GetListChargeShipmentMaster, ChargeShipmentModel>();
            CreateMap<CsShipmentSurcharge, CsShipmentSurchargeModel>();
            CreateMap<CsMawbcontainerImportModel, CsMawbcontainer>();
            CreateMap<CsDimensionDetailModel, CsDimensionDetail>();
            CreateMap<AcctCdnoteModel, AcctCdnote>();
            CreateMap<CsArrivalFrieghtChargeModel, CsArrivalFrieghtCharge>();
            CreateMap<CsMawbcontainerModel, CsMawbcontainer>();
            CreateMap<CsShipmentOtherChargeModel, CsShipmentOtherCharge>();
            CreateMap<OpsTransactionModel, OpsTransaction>();
            CreateMap<DataSurchargeResult, CsShipmentSurcharge>();
            CreateMap<sp_GetDataExportAccountant, CsShipmentSurcharge>();
            CreateMap<SysReportLogModel, SysReportLog>();
            CreateMap<CatStageModel, CatStage>();
            CreateMap<SysTrackInfo, SysTrackInfoModel>();
            CreateMap<CsShipmentSurchargeImportModel, CsShipmentSurcharge>();
            CreateMap<sp_GetShipmentAssignPIC, Shipments>();
        }
    }
}
