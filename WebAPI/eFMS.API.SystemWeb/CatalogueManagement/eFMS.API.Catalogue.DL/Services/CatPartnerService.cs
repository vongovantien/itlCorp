using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
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

        public List<DepartmentPartner> GetDepartments()
        {
            return DataEnums.Departments;
        }

        public IQueryable<CatPartnerModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {
            //var query = GetQueryExpression(criteria);
            //return Paging(query, page, size, out rowsCount);
            List<CatPartnerModel> results = null;
            var list = Query(criteria);
            rowsCount = list.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = list.Skip((page - 1) * size).Take(size).ToList();
            }
            return results.AsQueryable();
        }

        public List<CustomerPartnerViewModel> PagingCustomer(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria).GroupBy(x => x.SalePersonId);
            rowsCount = data.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            List<CustomerPartnerViewModel> results = new List<CustomerPartnerViewModel>();
            foreach (var item in data)
            {
                var partner = new CustomerPartnerViewModel();
                partner.SalePersonId = item.Key;
                partner.SalePersonName = ((eFMSDataContext)DataContext.DC).SysUser.First(x => x.Id == item.Key).Username;
                partner.CatPartnerModels = item.ToList();
                partner.SumNumberPartner = item.Count();
                results.Add(partner);
            }
            return results;
        }

        public IQueryable<CatPartnerModel> Query(CatPartnerCriteria criteria)
        {
            //var query = GetQueryExpression(criteria);
            //var results = Get(query);
            var data = Get();
            IQueryable<CatPartnerModel> results = null;
            string partnerGroup = PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup);
            if (criteria.All == null)
            {
                results = data.Where(x => ((x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.UserCreated ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           ));
            }
            else
            {
                results = data.Where(x => ((x.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.AddressVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.TaxCode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.Tel ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.Fax ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.UserCreated ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           ) && ((x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
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
                           && (x.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.UserCreated ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           );
            }
            else
            {
                query = x => ((x.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.UserCreated ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           ) && ((x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            return query;
        }
    }
}
