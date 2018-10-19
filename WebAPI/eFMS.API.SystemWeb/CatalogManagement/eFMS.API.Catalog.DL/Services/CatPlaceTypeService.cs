using AutoMapper;
using eFMS.API.Catalog.DL.Infrastructure;
using eFMS.API.Catalog.DL.IService;
using eFMS.API.Catalog.DL.Models;
using eFMS.API.Catalog.DL.Models.Criteria;
using eFMS.API.Catalog.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.Catalog.DL.Services
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
                                             && (x.Inactive == criteria.Inactive || criteria.Inactive == null);
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
