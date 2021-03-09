using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public class SysCompanyService : RepositoryBase<SysCompany, SysCompanyModel>, ISysCompanyService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUserLevel> sysLevelRepository;
        private readonly IContextBase<SysUser> userRepository;

        //private readonly ICurrentUser currentUser;
        public SysCompanyService(
            IContextBase<SysCompany> repository, 
            IMapper mapper,
            IContextBase<SysUserLevel> userLevelRepo,
            ICurrentUser icurrentUser,
            IContextBase<SysUser> userRepo) : base(repository, mapper)
        {
            currentUser = icurrentUser;
            //currentUser = user;
            sysLevelRepository = userLevelRepo;
            userRepository = userRepo;
            SetChildren<SysCompany>("ID", "BUID");
            SetChildren<SysOffice>("Id", "BuId");
            SetChildren<SysUserLevel>("Id", "CompanyId");
        }

        public IQueryable<SysCompanyModel> GetAll()
        {
            var bu = DataContext.Get();
            return bu.ProjectTo<SysCompanyModel>(mapper.ConfigurationProvider);
        }

        public SysCompanyModel Get(Guid id)
        {
            var result = Get(x => x.Id == id)?.FirstOrDefault();
            if (result == null) return null;
            else
            {
                result.NameUserCreated = userRepository.Get(x => x.Id == result.UserCreated).FirstOrDefault()?.Username;
                result.NameUserModified = userRepository.Get(x => x.Id == result.UserModified).FirstOrDefault()?.Username;
                return result;
            }
        }

        public IQueryable<SysCompany> GetByUserId(string id)
        {
            var userLevels = sysLevelRepository.Get(x => x.UserId == id).ToList();
            var lstSysCompanies = DataContext.Get(x => x.Active == true).ToList();
            var lsts = lstSysCompanies.Where(item => userLevels.Any(uslv => uslv.CompanyId.Equals(item.Id)));
            return lsts.AsQueryable();
        }

        public List<SysCompanyModel> Paging(SysCompanyCriteria criteria, int page, int size, out int rowsCount)
        {
            List<SysCompanyModel> results = null;
            var data = Query(criteria);
            if (data == null) {
                rowsCount = 0;
                results = null;
            }
            rowsCount = data.Select(x => x.Id).Count();
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = data.OrderByDescending(x => x.DatetimeModified).Skip((page - 1) * size).Take(size).ToList();
            }
            return results;
        }

        public IQueryable<SysCompanyModel> Query(SysCompanyCriteria criteria)
        {
            Expression<Func<SysCompany, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.BunameEn ?? "").IndexOf(criteria.BuNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.BunameVn ?? "").IndexOf(criteria.BuNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            && (x.BunameAbbr ?? "").IndexOf(criteria.BuNameAbbr ?? "", StringComparison.OrdinalIgnoreCase) > -1;
            }
            else
            {
                query = x => (x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.BunameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.BunameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.BunameAbbr ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1;
            }

            var bus = DataContext.Get(query);
            if (bus == null) return null;
            var results = bus.ProjectTo<SysCompanyModel>(mapper.ConfigurationProvider); // maping BU sang SysCompanyModel ( hoặc object # => define trong Mapper.cs);
            return results;
        }

        public HandleState Update(Guid id, SysCompanyAddModel model)
        {
            var hs = new HandleState();
            var userCurrent = currentUser.UserID;

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
                SysCompanyCurrent.KbExchangeRate = model.KbExchangeRate;

                hs = DataContext.Update(SysCompanyCurrent, x => x.Id == id);

               
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState Delete(Guid id)
        {
            try
            {

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
                var userCurrent = currentUser.UserID;

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

                var hs = DataContext.Add(SysCompany);
               
                if (hs.Success)
                {
                    model.Id = DataContext.Get(x => x.Code == model.CompanyCode).FirstOrDefault().Id; // lấy ra company vừa tạo.
                }
                return hs;

            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex);
                return hs;
            }
        }

        public List<SysCompanyModel> GetCompanyPermissionLevel()
        {
            List<SysCompanyModel> results = new List<SysCompanyModel>();
            try
            {
                var companyIds = sysLevelRepository.Get(c => c.CompanyId != null).Select(c => c.CompanyId).ToList();
                if(companyIds.Count() > 0)
                {
                    var companies = DataContext.Get(c => companyIds.Contains(c.Id)).ToList();

                    foreach (var item in companies)
                    {
                        var c = mapper.Map<SysCompanyModel>(item);
                        results.Add(c);
                    }

                }
                return results;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }


}
