using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPartnerService : RepositoryBase<CatPartner, CatPartnerModel>, ICatPartnerService
    {
        public CatPartnerService(IContextBase<CatPartner> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public IQueryable<CatPartnerModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {
            var query = GetQueryExpression(criteria);
            return Paging(query, page, size, out rowsCount);
        }

        public IQueryable<CatPartnerModel> Query(CatPartnerCriteria criteria)
        {
            var query = GetQueryExpression(criteria);
            var results = Get(query);
            return results;
        }

        private Expression<Func<CatPartnerModel, bool>> GetQueryExpression(CatPartnerCriteria criteria)
        {
            Expression<Func<CatPartnerModel, bool>> query = null;
            string partnerGroup = PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup);
            if (criteria.All == null)
            {
                query = x => ((x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           );
            }
            else
            {
                query = x => ((x.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           )&&((x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return query;
        }
    }
}
