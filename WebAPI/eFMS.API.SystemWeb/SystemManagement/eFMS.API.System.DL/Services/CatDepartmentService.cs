using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class CatDepartmentService : RepositoryBase<CatDepartment, CatDepartmentModel>, ICatDepartmentService
    {
        public CatDepartmentService(IContextBase<CatDepartment> repository, IMapper mapper) : base(repository, mapper)
        {

        }

        public IQueryable<CatDepartmentModel> QueryData(CatDepartmentCriteria criteria)
        {
            var dept = DataContext.Get();
            var query = dept;
            if (criteria.Type == "All" || string.IsNullOrEmpty(criteria.Type))
            {
                if (!string.IsNullOrEmpty(criteria.Keyword))
                {
                    query = query.Where(x =>
                           x.Code == criteria.Keyword
                        || x.DeptName == criteria.Keyword
                        || x.DeptNameEn == criteria.Keyword
                        || x.DeptNameAbbr == criteria.Keyword
                        || x.BranchId.ToString() == criteria.Keyword
                    );
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(criteria.Keyword))
                {
                    query = query.Where(x =>
                           criteria.Type == "Code" ? x.Code == criteria.Keyword : 1 == 1
                        && criteria.Type == "NameLocal" ? x.DeptName == criteria.Keyword : 1 == 1
                        && criteria.Type == "NameEN" ? x.DeptNameEn == criteria.Keyword : 1 == 1
                        && criteria.Type == "NameAbbr" ? x.DeptNameAbbr == criteria.Keyword : 1 == 1
                        && criteria.Type == "Office" ? x.DeptNameEn == criteria.Keyword : 1 == 1
                    );
                }
            }
            var result = query.Select(x => new CatDepartmentModel
            {
                Id = x.Id,
                Code = x.Code,
                DeptName = x.DeptName,
                DeptNameEn = x.DeptNameEn,
                DeptNameAbbr = x.DeptNameAbbr,
                Description = x.Description,
                BranchId = x.BranchId,
                OfficeName = "",
                ManagerId = x.ManagerId,
                UserCreated = x.UserCreated,
                DatetimeCreated = x.DatetimeCreated,
                UserModified = x.UserModified,
                DatetimeModified = x.DatetimeModified,
                Active = x.Active,
                InactiveOn = x.InactiveOn
            });
            result = result.OrderByDescending(x => x.DatetimeModified);
            return result;
        }

        public IQueryable<CatDepartmentModel> Paging(CatDepartmentCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = QueryData(criteria);
            rowsCount = (data.Count() > 0) ? data.Count() : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            return data;
        }

        public CatDepartmentModel GetDepartmentById(int id)
        {
            var data = new CatDepartmentModel();
            var dept = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (dept != null)
            {
                data = mapper.Map<CatDepartmentModel>(dept);
            }
            return data;
        }

    }

}
