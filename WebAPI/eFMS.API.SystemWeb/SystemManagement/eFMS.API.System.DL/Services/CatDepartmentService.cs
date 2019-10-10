using AutoMapper;
using eFMS.API.Common.NoSql;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class CatDepartmentService : RepositoryBase<CatDepartment, CatDepartmentModel>, ICatDepartmentService
    {
        private readonly IContextBase<SysCompany> sysCompanyRepo;
        private readonly IContextBase<SysOffice> sysOfficeRepo;
        private readonly IStringLocalizer stringLocalizer;
        public CatDepartmentService(IStringLocalizer<LanguageSub> localizer, IContextBase<CatDepartment> repository, IMapper mapper, IContextBase<SysOffice> sysOffice, IContextBase<SysCompany> sysCompany) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            sysOfficeRepo = sysOffice;
            sysCompanyRepo = sysCompany;
            //SetChildren<CatDepartment>("","","Used",false);
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
            var result = from d in query
                         join off in sysOfficeRepo.Get() on d.BranchId equals off.Id into off2
                         from off in off2.DefaultIfEmpty()
                         select new CatDepartmentModel
                         {
                             Id = d.Id,
                             Code = d.Code,
                             DeptName = d.DeptName,
                             DeptNameEn = d.DeptNameEn,
                             DeptNameAbbr = d.DeptNameAbbr,
                             Description = d.Description,
                             BranchId = d.BranchId,
                             OfficeName = off.BranchNameEn,
                             ManagerId = d.ManagerId,
                             UserCreated = d.UserCreated,
                             DatetimeCreated = d.DatetimeCreated,
                             UserModified = d.UserModified,
                             DatetimeModified = d.DatetimeModified,
                             Active = d.Active,
                             InactiveOn = d.InactiveOn
                         };
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
                var off = sysOfficeRepo.Get(x => x.Id == dept.BranchId).FirstOrDefault();
                var com = sysCompanyRepo.Get(x => off != null && x.Id == off.Buid).FirstOrDefault();
                data = mapper.Map<CatDepartmentModel>(dept);
                data.OfficeName = off != null ? off.BranchNameEn : "";
                data.CompanyName = com != null ? com.BunameEn : "";
            }
            return data;
        }

        public HandleState Insert(CatDepartmentModel model)
        {
            try
            {
                var userCurrent = "admin";
                var today = DateTime.Now;
                var modelAdd = mapper.Map<CatDepartment>(model);
                var deptCurrent = DataContext.Get(x => x.Code == model.Code).FirstOrDefault();
                if(deptCurrent != null)
                {
                    return new HandleState("Code has already existed");
                }
                modelAdd.UserCreated = modelAdd.UserModified = userCurrent;
                modelAdd.DatetimeCreated = modelAdd.DatetimeModified = today;
                if (modelAdd.Active == false)
                {
                    modelAdd.InactiveOn = today;
                }
                var hs = DataContext.Add(modelAdd);

                if (hs.Success)
                {
                    model.Id = DataContext.Get(x => x.Code == model.Code).Any() ? DataContext.Get(x => x.Code == model.Code).FirstOrDefault().Id : 0;
                }
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState Update(CatDepartmentModel model)
        {
            try
            {
                var userCurrent = "admin";
                var today = DateTime.Now;
                var modelUpdate = mapper.Map<CatDepartment>(model);
                var deptCurrent = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                modelUpdate.UserCreated = deptCurrent.UserCreated;
                modelUpdate.DatetimeCreated = deptCurrent.DatetimeCreated;
                modelUpdate.Code = deptCurrent.Code;
                modelUpdate.Description = deptCurrent.Description;
                modelUpdate.ManagerId = deptCurrent.ManagerId;
                modelUpdate.UserModified = userCurrent;
                modelUpdate.DatetimeModified = today;
                if (modelUpdate.Active == false)
                {
                    modelUpdate.InactiveOn = today;
                }
                var hs = DataContext.Update(modelUpdate, x => x.Id == model.Id);
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState Delete(int id)
        {
            try
            {
                ChangeTrackerHelper.currentUser = "admin";
                var hs = DataContext.Delete(x => x.Id == id);
                return hs;
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
    }

}
