﻿using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Common.Globals;
namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeService  :RepositoryBase<CatCharge,CatChargeModel>,ICatChargeService
    {
        public CatChargeService(IContextBase<CatCharge> repository,IMapper mapper):base(repository,mapper)
        {

        }

        public HandleState AddCharge(CatChargeAddOrUpdateModel model)
        {
            Guid chargeId = Guid.NewGuid();
            model.Charge.Id = chargeId;
            model.Charge.Inactive = false;
            model.Charge.UserCreated = "Thor.The";
            model.Charge.DatetimeCreated = DateTime.Now;

            try
            {
                DataContext.Add(model.Charge);

                foreach (var x in model.ListChargeDefaultAccount)
                {
                    x.ChargeId = chargeId;
                    x.Inactive = false;
                    x.UserCreated = "Thor.The";
                    x.DatetimeCreated = DateTime.Now;
                    ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Add(x);
                    ((eFMSDataContext)DataContext.DC).SaveChanges();
                }
                var hs = new HandleState();
                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
            
        }

        public HandleState UpdateCharge(CatChargeAddOrUpdateModel model)
        {
            model.Charge.UserModified = "Thor.The";
            model.Charge.DatetimeModified = DateTime.Now;
            try
            {
                DataContext.Update(model.Charge, x => x.Id == model.Charge.Id);
                foreach(var x in model.ListChargeDefaultAccount)
                {
                    x.UserModified = "Thor.The";
                    x.DatetimeModified = DateTime.Now;
                    ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Update(x);
                    ((eFMSDataContext)DataContext.DC).SaveChanges();
                }
                var hs = new HandleState();
                return hs;
            }
            catch(Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
            
        }

        public CatChargeAddOrUpdateModel GetChargeById(Guid id)
        {
            CatChargeAddOrUpdateModel returnCharge = new CatChargeAddOrUpdateModel();
            var charge = DataContext.Get(x => x.Id == id).FirstOrDefault();
            var listChargeDefault = ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Where(x => x.ChargeId == id).ToList();
            returnCharge.Charge = charge;
            returnCharge.ListChargeDefaultAccount = listChargeDefault;
            return returnCharge;
        }



        public List<Object> GetCharges(CatChargeCriteria criteria, int page, int size, out int rowsCount)
        {
            var list = Query(criteria);
            var listReturn = new List<Object>();

            rowsCount = list.Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                list = list.Skip((page - 1) * size).Take(size).ToList();
            }
            foreach(var charge in list)
            {
                var currency = ((eFMSDataContext)DataContext.DC).CatCurrency.Where(x => x.Id == charge.CurrencyId).FirstOrDefault();
                var unit = ((eFMSDataContext)DataContext.DC).CatUnit.Where(x => x.Id == charge.UnitId).FirstOrDefault();
                //var listServices = charge.ServiceTypeId.Split(";");
                var chargeDefaultAccounts = ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Where(x => x.ChargeId == charge.Id).ToList();
                var obj = new { currency = currency.Id, unit = unit.Code, charge, chargeDefaultAccounts };
                listReturn.Add(obj);
            }
            
            return listReturn;
        }

        public List<CatCharge> Query(CatChargeCriteria criteria)
        {
            var list = DataContext.Get();
            if(criteria.All == null)
            {
                list = list.Where(x => ((x.ChargeNameEn ?? "").IndexOf(criteria.ChargeNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.ChargeNameVn ?? "").IndexOf(criteria.ChargeNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            else
            {
               list = list.Where(x => ((x.ChargeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.ChargeNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            return list.ToList(); ;
        }

        public HandleState DeleteCharge(Guid id)
        {
            DataContext.Delete(x => x.Id == id);
            try
            {
                DataContext.Delete(x => x.Id == id);
                var listChargeDefaultAccount = ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Where(x => x.ChargeId == id).ToList();
                foreach(var item in listChargeDefaultAccount)
                {
                    ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Remove(item);
                }
                ((eFMSDataContext)DataContext.DC).SaveChanges();
                var hs = new HandleState();
                return hs;

            }
            catch(Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }

        }

  
    }
}
