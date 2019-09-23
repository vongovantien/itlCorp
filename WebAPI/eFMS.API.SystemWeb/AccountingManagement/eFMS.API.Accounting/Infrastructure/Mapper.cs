using AutoMapper;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;

namespace eFMS.API.Accounting.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<AcctSoa, AcctSoaModel>();
            CreateMap<AcctSoaModel, AcctSoa>();
            CreateMap<spc_GetListAcctSOAByMaster, AcctSOAResult>();
            CreateMap<spc_GetListAcctSOAByMaster, AcctSOADetailResult>();
            CreateMap<spc_GetListChargeShipmentMasterBySOANo, ChargeShipmentModel>();
            CreateMap<spc_GetListMoreChargeMasterByCondition, ChargeShipmentModel>();
            CreateMap<spc_GetDataExportSOABySOANo, ExportSOAModel>();
            CreateMap<CsShipmentSurchargeModel, CsShipmentSurcharge>();

            CreateMap<AcctAdvancePayment, AcctAdvancePaymentModel>();
            CreateMap<AcctAdvancePaymentModel, AcctAdvancePayment>();

            CreateMap<AcctApproveAdvance, AcctApproveAdvanceModel>();

            CreateMap<ShipmentChargeSettlement, CsShipmentSurcharge>();
            CreateMap<AcctSettlementPayment, AcctSettlementPaymentModel>();
            CreateMap<AcctSettlementPaymentModel, AcctSettlementPayment>();
            CreateMap<AcctApproveSettlement, AcctApproveSettlementModel>();
        }
    }
}
