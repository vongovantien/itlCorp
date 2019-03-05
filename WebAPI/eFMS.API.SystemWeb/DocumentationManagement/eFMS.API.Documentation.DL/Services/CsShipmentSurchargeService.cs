using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsShipmentSurchargeService : RepositoryBase<CsShipmentSurcharge, CsShipmentSurchargeModel>, ICsShipmentSurchargeService
    {
        public CsShipmentSurchargeService(IContextBase<CsShipmentSurcharge> repository, IMapper mapper) : base(repository, mapper)
        {

        }

        public List<CsShipmentSurchargeDetailsModel> GetByHB(Guid HbID)
        {

            List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
            var query = (from charge in ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge where charge.Hblid==HbID
                         join chargeDetail in ((eFMSDataContext)DataContext.DC).CatCharge on charge.ChargeId equals chargeDetail.Id
                         join partner in ((eFMSDataContext)DataContext.DC).CatPartner on charge.PaymentObjectId equals partner.Id
                         join unit in ((eFMSDataContext)DataContext.DC).CatUnit on charge.UnitId equals unit.Id
                         join currency in ((eFMSDataContext)DataContext.DC).CatCurrency on charge.CurrencyId equals currency.Id
                         select new { charge, partner.PartnerNameEn, unit.UnitNameEn, currency.CurrencyName,chargeDetail.ChargeNameEn }
                         ).ToList() ;
            foreach(var item in query)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item.charge);
                charge.PartnerName = item.PartnerNameEn;
                charge.NameEn = item.ChargeNameEn;
                charge.Unit = item.UnitNameEn;
                charge.Currency = item.CurrencyName;
                listCharges.Add(charge);
            }
            return listCharges;

        }
    }
}
