using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Common.NoSql;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.System.DL.Services
{
    public class SysBuService : RepositoryBase<SysBu, SysBuModel>, ISysBuService
    {
        //private readonly ICurrentUser currentUser;
        public SysBuService(IContextBase<SysBu> repository, IMapper mapper) : base(repository, mapper)
        {
            //currentUser = user;
        }

        public IQueryable<SysBuModel> GetAll()
        {
            var bu = DataContext.Get();
            return bu.ProjectTo<SysBuModel>(mapper.ConfigurationProvider);
        }

        public List<SysBuModel> Paging(SysBuCriteria criteria, int page, int size, out int rowsCount)
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

        public List<SysBuModel> Query(SysBuCriteria criteria)
        {

            var bu = DataContext.Get(); // tạo đối tượng context cho table SysBu.
            var result = bu.Where(x => x.Inactive == false);

            if (criteria.Type == "All")
            {
                result = bu.Where(x => ((x.Code ?? "").IndexOf(criteria.Keyword ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               && ((x.BunameEn ?? "").IndexOf(criteria.Keyword ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               && ((x.BunameVn ?? "").IndexOf(criteria.Keyword ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            else if (criteria.Type == "Code" )
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
            } else
            {
                bu = bu.Where(x => x.BunameVn == criteria.Keyword);
            }

            var responseData = mapper.Map<List<SysBuModel>>(bu).ToList(); // maping BU sang SysBuModel ( hoặc object # => define trong Mapper.cs);

            return responseData;
        }

        public HandleState Update(SysBuAddModel model)
        {
            var userCurrent = "admin";

            try
            {
                var sysBuCurrent = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                sysBuCurrent.DatetimeCreated = sysBuCurrent.DatetimeCreated;
                sysBuCurrent.UserCreated = sysBuCurrent.UserCreated;

                sysBuCurrent.DatetimeModified = DateTime.Now;
                sysBuCurrent.UserModified = userCurrent;
                sysBuCurrent.LogoPath = model.PhotoUrl;
                sysBuCurrent.Inactive = model.Status;
                sysBuCurrent.Code = model.CompanyCode; 
                sysBuCurrent.Website = model.Website;
                sysBuCurrent.BunameAbbr = model.CompanyNameAbbr;
                sysBuCurrent.BunameEn = model.CompanyNameEn;
                sysBuCurrent.BunameVn = model.CompanyNameVn;

                DataContext.Update(sysBuCurrent, x => x.Id == model.Id);

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

        public HandleState Add(SysBuAddModel model)
        {
            try
            {
                var userCurrent = "admin";
                
                var SysBu = new SysBuModel
                {
                    Code = model.CompanyCode,
                    BunameAbbr = model.CompanyNameVn,
                    BunameVn = model.CompanyNameVn,
                    BunameEn = model.CompanyNameEn,
                    Website = model.Website,
                    Inactive = model.Status,
                    LogoPath = model.PhotoUrl,
                    Id = Guid.NewGuid()
                };

                SysBu.DatetimeCreated = SysBu.DatetimeModified = DateTime.Now;
                SysBu.UserCreated = SysBu.UserModified = userCurrent;

                DataContext.Add(SysBu);

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
