using AutoMapper;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.DL.Models.ExportResults;
using eFMS.API.SystemFileManagement.DL.Models.SettlementPayment;
using eFMS.API.SystemFileManagement.DL.ViewModel;
using eFMS.API.SystemFileManagement.Service.Models;

namespace eFMS.API.SystemFileManagement.Infrastructure
{
    public class MappingProfile : Profile
    {
        public  MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<AcctSoa, AcctSoaModel>();

            CreateMap<AcctAdvancePayment, AcctAdvancePaymentModel>();
            CreateMap<AcctAdvancePaymentModel, AcctAdvancePayment>();
            CreateMap<AcctApproveAdvance, AcctApproveAdvanceModel>();
            CreateMap<ShipmentChargeSettlement, CsShipmentSurcharge>();
            CreateMap<AcctSettlementPayment, AcctSettlementPaymentModel>();
            CreateMap<AcctApproveSettlement, AcctApproveSettlementModel>();
            CreateMap<AcctAdvanceRequest, AcctAdvanceRequestModel>();
            CreateMap<CatCurrencyExchange, CatCurrencyExchangeModel>();
            CreateMap<AccAccountingManagement, AccAccountingManagementModel>();
            CreateMap<ChargeOfAccountingManagementModel, AccountingManagementExport>();
            CreateMap<CatContract, CatContractModel>();
            CreateMap<AcctCombineBilling, AcctCombineBillingModel>();
            CreateMap<CsShipmentSurcharge, CsShipmentSurchargeDetailsModel>();


            // Map to entity model
            CreateMap<AcctSoaModel, AcctSoa>();
            CreateMap<CsShipmentSurchargeModel, CsShipmentSurcharge>();
            CreateMap<AcctAdvanceRequestModel, AcctAdvanceRequest>();
            CreateMap<AcctApproveAdvanceModel, AcctApproveAdvance>();
            CreateMap<AcctApproveSettlementModel, AcctApproveSettlement>();
            CreateMap<AcctSettlementPaymentModel, AcctSettlementPayment>();
            CreateMap<AccAccountingManagementModel, AccAccountingManagement>();
            CreateMap<AccAccountReceivableModel, AccAccountReceivable>();
            CreateMap<SysActionFuncLogModel, SysActionFuncLog>();
            CreateMap<CatContractModel, CatContract>();
            CreateMap<AcctReceipt, AcctReceiptModel>().ReverseMap();
            CreateMap<SysImage, SysImageModel>().ReverseMap();
            CreateMap<AccAccountReceivable, ReceivableTable>();
            CreateMap<AcctReceiptSyncModel, AcctReceiptSync>();
            CreateMap<AcctCombineBillingModel, AcctCombineBilling>();
        }
    }
}
