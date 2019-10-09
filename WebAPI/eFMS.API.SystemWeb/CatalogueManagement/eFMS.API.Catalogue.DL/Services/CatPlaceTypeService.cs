using AutoMapper;
using eFMS.API.Catalogue.DL.Infrastructure;
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
    public class CatPlaceTypeService : RepositoryBase<CatPlaceType, CatPlaceTypeModel>, ICatPlaceTypeService
    {
        public CatPlaceTypeService(IContextBase<CatPlaceType> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public IQueryable<CatPlaceTypeModel> Query(CatPlaceTypeCriteria criteria, string orderByProperty, bool isAscendingOrder)
        {
            Expression<Func<CatPlaceTypeModel, bool>> query = x => (x.NameVn ?? "").Contains(criteria.NameVn ?? "")
                                             && (x.NameEn ?? "").Contains(criteria.NameEn ?? "")
                                             && (x.Active == criteria.Active || criteria.Active == null);
            var results = base.Get(query);
            if (!string.IsNullOrEmpty(orderByProperty) && (isAscendingOrder || !isAscendingOrder))
            {
                var orderBy = ExpressionExtension.CreateExpression<CatPlaceTypeModel, object>(orderByProperty);
                results = isAscendingOrder ? results.OrderBy(orderBy) : results.OrderByDescending(orderBy);
            }
            return results;
        }
    }
}
