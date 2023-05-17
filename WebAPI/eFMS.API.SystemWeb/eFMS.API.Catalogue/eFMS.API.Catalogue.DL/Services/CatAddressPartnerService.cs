using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatAddressPartnerService : RepositoryBaseCache<CatAddressPartner, CatAddressPartnerModel>, ICatAddressPartnerService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<CatCountry> catCountryRepository;
        private readonly IContextBase<CatCity> catCityRepository;
        private readonly IContextBase<CatDistrict> catDistrictRepository;
        private readonly IContextBase<CatWard> catWardRepository;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IMapper mapper;

        public CatAddressPartnerService(IContextBase<CatAddressPartner> repository,
            ICacheServiceBase<CatAddressPartner> cacheService,
            IMapper imapper,
            IContextBase<SysUser> sysUserRepo,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<CatCountry> catCountryRepo,
            IContextBase<CatCity> catCityRepo,
            IContextBase<CatDistrict> catDistrictRepo,
            IContextBase<CatWard> catWardRepo,
            IStringLocalizer<LanguageSub> localizer,
        ICurrentUser currUser) : base(repository, cacheService, imapper)
        {
            currentUser = currUser;
            sysUserRepository = sysUserRepo;
            stringLocalizer = localizer;
            catPartnerRepository = catPartnerRepo;
            catCountryRepository = catCountryRepo;
            catDistrictRepository = catDistrictRepo;
            catCityRepository = catCityRepo;
            catWardRepository = catWardRepo;
            mapper = imapper;
        }

        #region CRUD
        public override HandleState Add(CatAddressPartnerModel entity)
        {
            var address = mapper.Map<CatAddressPartner>(entity);
            var nameCountry = catCountryRepository.Get(x => x.Id == address.CountryId)?.FirstOrDefault()?.NameVn;
            var nameCity = catCityRepository.Get(x => x.Id == address.CityId)?.FirstOrDefault()?.NameVn;
            var nameDistrict = catDistrictRepository.Get(x => x.Id == address.DistrictId)?.FirstOrDefault()?.NameVn;
            var nameWard = catWardRepository.Get(x => x.Id == address.WardId)?.FirstOrDefault()?.NameVn;
            address.Location = address.StreetAddress + ", " + nameWard + ", " + nameDistrict + ", " + nameCity + ", " + nameCountry;
            address.Id = Guid.NewGuid();
            address.DatetimeCreated = address.DatetimeModified = DateTime.Now;
            address.Active = true;
            address.UserCreated = address.UserModified = currentUser.UserID;
            var result = DataContext.Add(address, false);
            DataContext.SubmitChanges();
            if (result.Success)
            {
                ClearCache();
                Get();
            }
            return result;
        }
        public HandleState Update(CatAddressPartnerModel model)
        {
            HandleState result = new HandleState();
            try
            {
                var address = GetDetail(model.Id);
                var entity = mapper.Map<CatAddressPartner>(address);
                entity.UserModified = currentUser.UserID;
                entity.DatetimeModified = DateTime.Now;
                entity.ShortNameAddress = model.ShortNameAddress;
                entity.CountryId = model.CountryId;
                entity.CityId = model.CityId;
                entity.DistrictId = model.DistrictId;
                entity.WardId = model.WardId;
                entity.StreetAddress = model.StreetAddress;
                entity.AddressType = model.AddressType;
                entity.ContactPerson = model.ContactPerson;
                entity.Tel = model.Tel;
                entity.Active = model.Active;

                var nameCountry = catCountryRepository.Get(x => x.Id == entity.CountryId)?.FirstOrDefault()?.NameVn;
                var nameCity = catCityRepository.Get(x => x.Id == entity.CityId)?.FirstOrDefault()?.NameVn;
                var nameDistrict = catDistrictRepository.Get(x => x.Id == entity.DistrictId)?.FirstOrDefault()?.NameVn;
                var nameWard = catWardRepository.Get(x => x.Id == entity.WardId)?.FirstOrDefault()?.NameVn;
                entity.Location = entity.StreetAddress + ", " + nameWard + ", " + nameDistrict + ", " + nameCity + ", " + nameCountry;


                if (entity.Active == false)
                    entity.InactiveOn = DateTime.Now;

                result = DataContext.Update(entity, x => x.Id == model.Id, false);

                if (result.Success)
                {
                    DataContext.SubmitChanges();
                    ClearCache();
                    Get();
                }
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }
        public HandleState Delete(Guid id)
        {
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success == true)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion

        public IQueryable<CatAddressPartnerModel> GetAll()
        {
            ClearCache();
            return Get();
        }

        public CatAddressPartnerModel GetDetail(Guid id)
        {
            ClearCache();
            CatAddressPartnerModel queryDetail = Get(x => x.Id == id).FirstOrDefault();
            //get infor partner
            var partner = catPartnerRepository.Get(x => x.Id.ToLower() == queryDetail.PartnerId.ToString().ToLower())?.FirstOrDefault();
            queryDetail.AccountNo = partner?.AccountNo;
            queryDetail.ShortName = partner?.ShortName;
            queryDetail.TaxCode = partner?.TaxCode;
            // Get usercreate name
            if (queryDetail.UserCreated != null)
                queryDetail.UserCreatedName = sysUserRepository.Get(x => x.Id == queryDetail.UserCreated)?.FirstOrDefault()?.Username;
            // Get usermodified name
            if (queryDetail.UserCreated != null)
                queryDetail.UserModifiedName = sysUserRepository.Get(x => x.Id == queryDetail.UserModified)?.FirstOrDefault()?.Username;

            return queryDetail;

        }


        public IQueryable<CatAddressPartnerModel> GetAddressByPartnerId(Guid partnerId)
        {
            var data = DataContext.Get(x => x.PartnerId == partnerId);
            if (data == null) return null;
            var results = data.ProjectTo<CatAddressPartnerModel>(mapper.ConfigurationProvider).ToList();
            results.ForEach(x =>
            {
                var partner = catPartnerRepository.Get(y => y.Id.ToLower() == x.PartnerId.ToString().ToLower())?.FirstOrDefault();
                x.ShortName = partner?.ShortName;
                x.TaxCode = partner?.TaxCode;
                x.AccountNo = partner?.AccountNo;
                x.CountryName = catCountryRepository.Get(y => y.Id == x.CountryId).Select(t => t.NameEn).FirstOrDefault();
                x.CityName = catCityRepository.Get(y => y.Id == x.CityId).Select(t => t.NameEn).FirstOrDefault();
                x.DistrictName = catDistrictRepository.Get(y => y.Id == x.DistrictId).Select(t => t.NameEn).FirstOrDefault();
                x.WardName = catWardRepository.Get(y => y.Id == x.WardId).Select(t => t.NameEn).FirstOrDefault();
            });
            return results?.OrderByDescending(x => x.DatetimeModified).AsQueryable();
        }

    }
}

