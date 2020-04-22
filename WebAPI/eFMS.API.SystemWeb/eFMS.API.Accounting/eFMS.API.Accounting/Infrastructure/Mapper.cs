﻿using AutoMapper;
using eFMS.API.Accounting.DL.Models;
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
            CreateMap<AcctSoaModel, AcctSoa>();
            CreateMap<CsShipmentSurchargeModel, CsShipmentSurcharge>();

            CreateMap<AcctAdvancePayment, AcctAdvancePaymentModel>();
            CreateMap<AcctAdvancePaymentModel, AcctAdvancePayment>();

            CreateMap<AcctApproveAdvance, AcctApproveAdvanceModel>();

            CreateMap<ShipmentChargeSettlement, CsShipmentSurcharge>();
            CreateMap<AcctSettlementPayment, AcctSettlementPaymentModel>();
            CreateMap<AcctSettlementPaymentModel, AcctSettlementPayment>();
            CreateMap<AcctApproveSettlement, AcctApproveSettlementModel>();

            CreateMap<AcctAdvanceRequest, AcctAdvanceRequestModel>();
            CreateMap<CatCurrencyExchange, CatCurrencyExchangeModel>();
        }
    }
}
