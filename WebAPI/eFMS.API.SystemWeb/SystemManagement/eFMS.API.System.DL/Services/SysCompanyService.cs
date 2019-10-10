using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Common;
using eFMS.API.Common.NoSql;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.System.DL.Services
{
    public class SysCompanyService : RepositoryBase<SysCompany, SysCompanyModel>, ISysCompanyService
    {
        //private readonly ICurrentUser currentUser;
        public SysCompanyService(IContextBase<SysCompany> repository, IMapper mapper) : base(repository, mapper)
        {
            //currentUser = user;
        }

        public IQueryable<SysCompanyModel> GetAll()
        {
            var bu = DataContext.Get();
            return bu.ProjectTo<SysCompanyModel>(mapper.ConfigurationProvider);
        }

        public List<SysCompanyModel> Paging(SysCompanyCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);

            rowsCount = (data.Count() > 0) ? data.Count() : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size).ToList();
            }
            return data;
        }

        public List<SysCompanyModel> Query(SysCompanyCriteria criteria)
        {

            var bu = DataContext.Get(); // tạo đối tượng context cho table SysCompany.
            var result = bu.Where(x => x.Active == true);

            if (criteria.Type == "All")
            {
                result = bu.Where(x => ((x.Code ?? "").IndexOf(criteria.Keyword ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               && ((x.BunameEn ?? "").IndexOf(criteria.Keyword ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               && ((x.BunameVn ?? "").IndexOf(criteria.Keyword ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            else if (criteria.Type == "Code")
            {
                bu = bu.Where(x => x.Code == criteria.Keyword);
            }
            else if (criteria.Type == "NameAbbr")
            {
                bu = bu.Where(x => x.BunameAbbr == criteria.Keyword);
            }
            else if (criteria.Type == "NameEn")
            {
                bu = bu.Where(x => ((x.BunameEn ?? "").IndexOf(criteria.Keyword ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            else
            {
                bu = bu.Where(x => x.BunameVn == criteria.Keyword);
            }

            var responseData = mapper.Map<List<SysCompanyModel>>(bu).ToList(); // maping BU sang SysCompanyModel ( hoặc object # => define trong Mapper.cs);

            return responseData;
        }

        public HandleState Update(Guid id, SysCompanyAddModel model)
        {
            var userCurrent = "admin";

            try
            {
                var SysCompanyCurrent = DataContext.First(x => x.Id == id);
                SysCompanyCurrent.DatetimeCreated = SysCompanyCurrent.DatetimeCreated;
                SysCompanyCurrent.UserCreated = SysCompanyCurrent.UserCreated;
                if (SysCompanyCurrent.Code != model.CompanyCode && DataContext.Any(item => item.Code == model.CompanyCode))
                {
                    return new HandleState("Code existed");
                }

                SysCompanyCurrent.DatetimeModified = DateTime.Now;
                SysCompanyCurrent.UserModified = userCurrent;
                SysCompanyCurrent.LogoPath = model.PhotoUrl;
                SysCompanyCurrent.Active = model.Status;
                SysCompanyCurrent.Code = model.CompanyCode;
                SysCompanyCurrent.Website = model.Website;
                SysCompanyCurrent.BunameAbbr = model.CompanyNameAbbr;
                SysCompanyCurrent.BunameEn = model.CompanyNameEn;
                SysCompanyCurrent.BunameVn = model.CompanyNameVn;

                DataContext.Update(SysCompanyCurrent, x => x.Id == id);

                var hs = new HandleState();
                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public HandleState Delete(Guid id)
        {
            try
            {
                ChangeTrackerHelper.currentUser = "admin";

                var hs = DataContext.Delete(x => x.Id == id);
                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public HandleState Add(SysCompanyAddModel model)
        {
            try
            {
                var userCurrent = "admin";

                var SysCompany = new SysCompanyModel
                {
                    Code = model.CompanyCode,
                    BunameAbbr = model.CompanyNameVn,
                    BunameVn = model.CompanyNameVn,
                    BunameEn = model.CompanyNameEn,
                    Website = model.Website,
                    Active = model.Status,
                    LogoPath = model.PhotoUrl,
                    Id = Guid.NewGuid()
                };

                SysCompany.DatetimeCreated = SysCompany.DatetimeModified = DateTime.Now;
                SysCompany.UserCreated = SysCompany.UserModified = userCurrent;

                DataContext.Add(SysCompany);

                var hs = new HandleState();
                return hs;

            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex);
                return hs;
            }
        }
    }


}
