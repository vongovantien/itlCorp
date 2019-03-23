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

        public List<CatPartner> GetAllParnerByJob(Guid JobId)
        {
            try
            {
                List<CatPartner> listPartners = new List<CatPartner>();
                List<Guid> lst_Hbid = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == JobId).ToList().Select(x => x.Id).ToList();
                foreach (var id in lst_Hbid)
                {
                    var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == id).FirstOrDefault();
                    List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                    if (houseBill != null)
                    {
                        listCharges = Query(houseBill.Id, null);
                        
                        foreach (var c in listCharges)
                        {
                            if (c.PaymentObjectId != null)
                            {
                                var partner = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.PaymentObjectId).FirstOrDefault();
                                if (partner != null) listPartners.Add(partner);
                            }
                            if (c.ReceiverId != null)
                            {
                                var partner = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.ReceiverId).FirstOrDefault();
                                if (partner != null) listPartners.Add(partner);
                            }
                            if (c.PayerId != null)
                            {
                                var partner = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.PayerId).FirstOrDefault();
                                if (partner != null) listPartners.Add(partner);
                            }
                        }
                    }

                }
                listPartners = listPartners.Distinct().ToList();
                return listPartners;
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public List<CsShipmentSurchargeDetailsModel> GetByHB(Guid HbID,string type)
        {
            return Query(HbID,type);
        }

        public List<object> GetByListHB(List<Guid> lst_Hbid)
        {
           List<object> returnList = new List<object>();
           foreach(var id in lst_Hbid)
            {
                var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == id).FirstOrDefault();
                List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                if (houseBill != null)
                {
                    listCharges = Query(houseBill.Id, null);
                }

                var returnObj = new { houseBill.Hwbno, houseBill.Hbltype, houseBill.Id, listCharges };
                returnList.Add(returnObj);
            }
            return returnList;
        }

        public List<object> GroupChargeByHB(Guid JobId, string PartnerId)
        {
            List<object> returnList = new List<object>();
            List<Guid> lst_Hbid = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == JobId).ToList().Select(x => x.Id).ToList();
            foreach (var id in lst_Hbid)
            {
                var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == id).FirstOrDefault();
                List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                if (houseBill != null)
                {
                    listCharges = Query(houseBill.Id, null);
                    listCharges = listCharges.Where(x => (x.PayerId == PartnerId || x.ReceiverId == PartnerId || x.PaymentObjectId == PartnerId)).ToList();
                }
                if (listCharges.Count > 0)
                {
                    var returnObj = new { houseBill.Hwbno, houseBill.Hbltype, houseBill.Id, listCharges };
                    returnList.Add(returnObj);
                }            
            }
            return returnList;

        }

        private List<CsShipmentSurchargeDetailsModel> Query(Guid HbID, string type)
        {
            List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
            var charges = DataContext.Get();
            if (!string.IsNullOrEmpty(type))
            {
                charges = charges.Where(x => x.Type.ToLower() == type.ToLower());
            }
            var query = (from charge in charges
                         where charge.Hblid == HbID 
                         join chargeDetail in ((eFMSDataContext)DataContext.DC).CatCharge on charge.ChargeId equals chargeDetail.Id

                         join partner in ((eFMSDataContext)DataContext.DC).CatPartner on charge.PaymentObjectId equals partner.Id into partnerGroup
                         from p in partnerGroup.DefaultIfEmpty()

                         join receiver in ((eFMSDataContext)DataContext.DC).CatPartner on charge.ReceiverId equals receiver.Id into receiverGroup
                         from r in receiverGroup.DefaultIfEmpty()

                         join payer in ((eFMSDataContext)DataContext.DC).CatPartner on charge.PayerId equals payer.Id into payerGroup
                         from pay in payerGroup.DefaultIfEmpty()

                         join unit in ((eFMSDataContext)DataContext.DC).CatUnit on charge.UnitId equals unit.Id
                         join currency in ((eFMSDataContext)DataContext.DC).CatCurrency on charge.CurrencyId equals currency.Id
                         select new { charge, p, r, pay, unit.UnitNameEn, currency.CurrencyName, chargeDetail.ChargeNameEn, chargeDetail.Code }
                        ).ToList();
            foreach (var item in query)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item.charge);
                charge.PartnerName = item.p?.PartnerNameEn;
                charge.NameEn = item?.ChargeNameEn;
                charge.ReceiverName = item.r?.PartnerNameEn;
                charge.PayerName = item.pay?.PartnerNameEn;
                charge.Unit = item.UnitNameEn;
                charge.Currency = item.CurrencyName;
                charge.ChargeCode = item.Code;
                listCharges.Add(charge);
            }
            return listCharges;
        }
    }
}
