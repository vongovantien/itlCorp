using AutoMapper;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Models;

namespace eFMS.API.Accounting.Infrastructure
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
        }
    }
}
