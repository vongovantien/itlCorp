using AutoMapper;
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

        public List<CatCharge> GetCharges(CatChargeCriteria criteria, int page, int size, out int rowsCount)
        {
            var list = Query(criteria);
            rowsCount = list.Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                list = list.Skip((page - 1) * size).Take(size).ToList();
            }
            return list;
        }

        public List<CatCharge> Query(CatChargeCriteria criteria)
        {
            var list = DataContext.Get();
            if(criteria.All == null)
            {
                list = list.Where(x => ((x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.ChargeNameEn ?? "").IndexOf(criteria.ChargeNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.ChargeNameVn ?? "").IndexOf(criteria.ChargeNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            else
            {
               list = list.Where(x => ((x.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.ChargeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.ChargeNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            return list.ToList(); ;
        }
        
    }
}
