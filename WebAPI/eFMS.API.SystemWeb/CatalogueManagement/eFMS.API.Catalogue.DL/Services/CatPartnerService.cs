using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using eFMS.API.Common;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPartnerService : RepositoryBaseCache<CatPartner, CatPartnerModel>, ICatPartnerService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IContextBase<CatSaleman> salemanRepository;
        private readonly ICatPlaceService placeService;
        private readonly ICatCountryService countryService;
        private readonly IOptions<WebUrl> webUrl;

        public CatPartnerService(IContextBase<CatPartner> repository, 
            ICacheServiceBase<CatPartner> cacheService, 
            IMapper mapper,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user,
            IContextBase<SysUser> sysUserRepo,
            ICatPlaceService place,
            ICatCountryService country,
            IContextBase<CatSaleman> salemanRepo, IOptions<WebUrl> url) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            placeService = place;
            salemanRepository = salemanRepo;
            sysUserRepository = sysUserRepo;
            countryService = country;
            webUrl = url;
            SetChildren<CsTransaction>("Id", "ColoaderId");
            SetChildren<CsTransaction>("Id", "AgentId");
            SetChildren<SysUser>("Id", "PersonIncharge");
            SetChildren<OpsTransaction>("Id", "CustomerId");
            SetChildren<OpsTransaction>("Id", "SupplierId");
            SetChildren<OpsTransaction>("Id", "AgentId");
            SetChildren<CsShippingInstruction>("Id", "Shipper");
            SetChildren<CsShippingInstruction>("Id", "Supplier");
            SetChildren<CsShippingInstruction>("Id", "ConsigneeId");
            SetChildren<CsShippingInstruction>("Id", "ActualShipperId");
            SetChildren<CsShippingInstruction>("Id", "ActualConsigneeId");
            SetChildren<CsManifest>("Id", "Supplier");
        }

        public IQueryable<CatPartnerModel> GetPartners()
        {
            return Get();
        }
        public List<DepartmentPartner> GetDepartments()
        {
            return DataEnums.Departments;
        }

        #region CRUD
        public override HandleState Add(CatPartnerModel entity)
        {
            var partner = mapper.Map<CatPartner>(entity);
            partner.DatetimeCreated = DateTime.Now;
            partner.DatetimeModified = DateTime.Now;
            partner.UserCreated = partner.UserModified = currentUser.UserID;
            partner.Active = true;
           
            var hs = DataContext.Add(partner);
            if (hs.Success)
            {
                if(entity.SaleMans.Count() > 0)
                {
                    var salemans = mapper.Map<List<CatSaleman>>(entity.SaleMans);
                    salemans.ForEach(x => {
                        x.Id = Guid.NewGuid();
                        x.PartnerId = partner.Id;
                        x.CreateDate = DateTime.Now;
                        x.UserCreated = currentUser.UserID;
                    });
                    partner.SalePersonId = salemans.FirstOrDefault().SaleManId.ToString();
                    DataContext.Update(partner, x => x.Id == partner.Id);
                   salemanRepository.Add(salemans);
                }
                DataContext.SubmitChanges();
                salemanRepository.SubmitChanges();
                ClearCache();
                Get();
                SendMail.Send("Confirm Add Partner", webUrl.Value.Url.ToString() + "/en/#/home/catalogue/partner-data/detail/" + entity.Id, "samuel.an@logtechub.com", null, null);
            }
            return hs;
        }
        public HandleState Update(CatPartnerModel model)
        {
            var entity = mapper.Map<CatPartner>(model);
            entity.DatetimeModified = DateTime.Now;
            entity.UserModified = currentUser.UserID;
            if (entity.Active == false)
            {
                entity.InactiveOn = DateTime.Now;
            } 
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                var hsoldman = salemanRepository.Delete(x => x.PartnerId == model.Id && !model.SaleMans.Any(sale => sale.Id == x.Id));
                var salemans = mapper.Map<List<CatSaleman>>(model.SaleMans);

                foreach (var item in model.SaleMans)
                {
                    if(item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.PartnerId = entity.Id;
                        item.CreateDate = DateTime.Now;
                        item.UserCreated = currentUser.UserID;
                        salemanRepository.Add(item);
                    }
                    else
                    {
                        item.ModifiedDate = DateTime.Now;
                        item.UserModified = currentUser.UserID;
                        salemanRepository.Update(item, x=> x.Id == item.Id);
                    }
                }
                salemanRepository.SubmitChanges();
                ClearCache();
                Get();
            }
            return hs;
        }
        public HandleState Delete(string id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion
        
        public IQueryable<CatPartnerViewModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }
            rowsCount = data.Count();
            IQueryable<CatPartnerViewModel> results = null;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = data.OrderByDescending(x => x.DatetimeModified).Skip((page - 1) * size).Take(size).AsQueryable();
            }
            return results;
        }

        public List<CustomerPartnerViewModel> PagingCustomer(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CustomerPartnerViewModel> results = null;
            var data = Query(criteria)?.GroupBy(x => x.SalePersonId);
            if(data == null)
            {
                rowsCount = 0;
                return results;
            }
            rowsCount = data.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            results = new List<CustomerPartnerViewModel>();
            foreach (var item in data)
            {
                var partner = new CustomerPartnerViewModel
                {
                    SalePersonId = item.Key,
                    SalePersonName = item.Key != null ? sysUserRepository.First(x => x.Id == item.Key).Username : null,
                    CatPartnerModels = item.ToList(),
                    SumNumberPartner = item.Count()
                };
                results.Add(partner);
            }
            return results;
        }
        public List<CatPartnerViewModel> Query(CatPartnerCriteria criteria)
        {
            string partnerGroup = criteria != null? PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup): null;
            var sysUsers = sysUserRepository.Get();
            var partners = Get(x => (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            var query = (from partner in partners
                         join user in sysUsers on partner.UserCreated equals user.Id
                         join saleman in sysUsers on partner.SalePersonId equals saleman.Id into prods
                         from x in prods.DefaultIfEmpty()
                         select new { user, partner, saleman = x }
                          );
            if (criteria.All == null)
            {
                query = query.Where(x => ((x.partner.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.PartnerNameEn ?? "").IndexOf(criteria.PartnerNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.PartnerNameVn ?? "").IndexOf(criteria.PartnerNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.user.Username ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.AccountNo ?? "").IndexOf(criteria.AccountNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Active == criteria.Active || criteria.Active == null)
                           ));
            }
            else
            {
                query = query.Where(x =>
                           (
                           (x.partner.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.PartnerNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.PartnerNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.AddressVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.TaxCode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.Tel ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.Fax ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.user.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           || (x.partner.AccountNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                           )
                           && (x.partner.Active == criteria.Active || criteria.Active == null));
            }
            if (query.Count() == 0) return null;
            List<CatPartnerViewModel> results = new List<CatPartnerViewModel>();
            foreach (var item in query)
            {
                var partner = mapper.Map<CatPartnerViewModel>(item.partner);
                partner.UserCreatedName = item.user?.Username;
                partner.SalePersonName = item.saleman?.Username;
                results.Add(partner);
            }
            return results;
        }

        #region import
        public HandleState Import(List<CatPartnerImportModel> data)
        {
            try
            {
                foreach (var item in data)
                {
                    bool active = string.IsNullOrEmpty(item.Status) || (item.Status.ToLower() == "active");
                    DateTime? inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    var partner = mapper.Map<CatPartner>(item);
                    partner.UserCreated = currentUser.UserID;
                    partner.DatetimeModified = DateTime.Now;
                    partner.DatetimeCreated = DateTime.Now;
                    partner.Id = Guid.NewGuid().ToString();
                    partner.AccountNo = partner.TaxCode;
                    partner.Active = active;
                    partner.InactiveOn = inactiveDate;
                    DataContext.Add(partner, false);
                }
                DataContext.SubmitChanges();
                ClearCache();
                Get();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        public List<CatPartnerImportModel> CheckValidImport(List<CatPartnerImportModel> list)
        {
            var partners = Get().ToList();
            var users = sysUserRepository.Get().ToList();
            var countries = countryService.Get().ToList();
            var provinces = placeService.Get(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province)).ToList();
            var branchs = placeService.Get(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Branch)).ToList();
            var salemans = sysUserRepository.Get().ToList();

            var allGroup = DataEnums.PARTNER_GROUP;
            var partnerGroups = allGroup.Split(";");
            list.ForEach(item =>
            {
                if (item.TaxCode.Length == 0)
                {
                    item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    string taxCode = item.TaxCode.ToLower();
                    if (list.Count(x => x.TaxCode.ToLower() == taxCode) > 1)
                    {
                        item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_DUPLICATED]);
                        item.IsValid = false;
                    }
                    else
                    {
                        if (partners.Any(x => x.TaxCode.ToLower() == taxCode))
                        {
                            item.TaxCodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_TAXCODE_EXISTED], item.TaxCode);
                            item.IsValid = false;
                        }
                    }
                }
                if (item.PartnerGroup.Length == 0)
                {
                    item.PartnerGroupError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_GROUP_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    item.PartnerGroup = item.PartnerGroup.ToUpper();
                    if (item.PartnerGroup == DataEnums.AllPartner)
                    {
                        item.PartnerGroup = allGroup;
                    }
                    else
                    {
                        var groups = item.PartnerGroup.Split(";").Select(x => x.Trim());
                        var group = partnerGroups.Intersect(groups);
                        if (group == null)
                        {
                            item.PartnerGroupError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_GROUP_NOT_FOUND], item.PartnerGroup);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.PartnerGroup = String.Join(";", groups);
                            if (item.PartnerGroup.Contains(DataEnums.CustomerPartner))
                            {
                                if (item.SaleManName.Length == 0)
                                {
                                    item.SaleManNameError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SALEMAN_EMPTY];
                                    item.IsValid = false;
                                }
                                else
                                {
                                    var salePerson = salemans.FirstOrDefault(i => i.Username == item.SaleManName);
                                    if (salePerson == null)
                                    {
                                        item.SaleManNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SALEMAN_NOT_FOUND], item.SaleManName);
                                        item.IsValid = false;
                                    }
                                    else
                                    {
                                        item.SalePersonId = salePerson.Id;
                                    }
                                }
                            }
                        }
                    }
                }
                if (item.PartnerNameEn.Length == 0)
                {
                    item.PartnerNameEnError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_NAME_EN_EMPTY]);
                    item.IsValid = false;
                }
                if (item.PartnerNameVn.Length == 0)
                {
                    item.PartnerNameVnError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_NAME_VN_EMPTY]);
                    item.IsValid = false;
                }
                if (item.ShortName.Length == 0)
                {
                    item.ShortNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_SHORT_NAME_EMPTY]);
                    item.IsValid = false;
                }
                if (item.AddressEn.Length == 0)
                {
                    item.AddressEnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_BILLING_EN_NOT_FOUND];
                    item.IsValid = false;

                }
                if (item.AddressVn.Length == 0)
                {
                    item.AddressVnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_BILLING_VN_NOT_FOUND];
                    item.IsValid = false;
                }
                if (item.AddressShippingEn.Length == 0)
                {
                    item.AddressShippingEnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_SHIPPING_EN_NOT_FOUND];
                    item.IsValid = false;
                }
                if (item.AddressShippingVn.Length == 0)
                {
                    item.AddressShippingVnError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_ADDRESS_SHIPPING_VN_NOT_FOUND];
                    item.IsValid = false;
                }
                if (item.DepartmentId == null)
                {
                    item.DepartmentId = "Head Office";
                }
                if (item.CountryBilling.Length == 0)
                {
                    item.CountryBillingError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_BILLING_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    string countryBilling = item.CountryBilling.ToLower();
                    if(countryBilling.Length == 0)
                    {
                        item.CountryBillingError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_BILLING_EMPTY];
                        item.IsValid = false;
                    }
                    else
                    {
                        var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == countryBilling);
                        if (country == null)
                        {
                            item.CountryBillingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_BILLING_NOT_FOUND], item.CountryBilling);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.CountryId = country.Id;
                            if (item.CityBilling.Length == 0)
                            {
                                item.CityBillingError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_BILLING_EMPTY];
                                item.IsValid = false;
                            }
                            else
                            {
                                string cityBilling = item.CityBilling.ToLower();
                                var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == cityBilling && i.CountryId == country.Id);
                                if (province == null)
                                {
                                    item.CityBillingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_BILLING_NOT_FOUND], item.CityBilling);
                                    item.IsValid = false;
                                }
                                else
                                {
                                    item.ProvinceId = province.Id;
                                }
                            }
                        }
                    }
                }
                if (item.CountryShipping.Length == 0)
                {
                    item.CountryShippingError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_SHIPPING_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    string countShipping = item.CountryShipping.ToLower();
                    var countryShipping = countries.FirstOrDefault(i => i.NameEn.ToLower() == countShipping);
                    if (countryShipping == null)
                    {
                        item.CountryShippingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_SHIPPING_NOT_FOUND], item.CountryShipping);
                        item.IsValid = false;
                    }
                    else
                    {
                        if(countShipping.Length == 0)
                        {
                            item.CountryShippingError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_COUNTRY_SHIPPING_EMPTY];
                            item.IsValid = false;
                        }
                        else
                        {
                            item.CountryShippingId = countryShipping.Id;

                            if (item.CityShipping.Length == 0)
                            {
                                item.CityShippingError = stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_SHIPPING_EMPTY];
                                item.IsValid = false;
                            }
                            else
                            {
                                var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.CityShipping.ToLower() && i.CountryId == item.CountryId);
                                if (province == null)
                                {
                                    item.CityShippingError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PARTNER_PROVINCE_SHIPPING_NOT_FOUND], item.CityShipping);
                                    item.IsValid = false;
                                }
                                else
                                {
                                    item.ProvinceShippingId = province.Id;
                                }
                            }
                        }
                    }
                }
            });
            return list;
        }
        #endregion

        public IQueryable<CatPartnerModel> GetBy(CatPartnerGroupEnum partnerGroup)
        {
            string group = PlaceTypeEx.GetPartnerGroup(partnerGroup);
            IQueryable<CatPartnerModel> data = Get().Where(x => (x.PartnerGroup ?? "").IndexOf(group ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            return data;
        }

        public IQueryable<CatPartnerModel> GetMultiplePartnerGroup(PartnerMultiCriteria criteria)
        {
            IQueryable<CatPartnerModel> data = Get();
            if (criteria == null) return data;
            List<string> grpCodes = new List<string>();
            if (criteria.PartnerGroups != null)
            {
                foreach (var grp in criteria.PartnerGroups)
                {
                    string group = PlaceTypeEx.GetPartnerGroup(grp);
                    grpCodes.Add(group);
                }
                Expression<Func<CatPartnerModel, bool>> query = null;
                foreach (var group in grpCodes.Distinct())
                {
                    if (query == null)
                    {
                        query = x => (x.PartnerGroup ?? "").IndexOf(group ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
                    }
                    else
                    {
                        query = query.Or(x => (x.PartnerGroup ?? "").IndexOf(group ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                }
                query = criteria.Active != null ? query.And(x => x.Active == criteria.Active) : query;
                data =  data.Where(query);
            }
            else
            {
                data = data.Where(x => x.Active == criteria.Active || criteria.Active == null);
            }           
            return data;
        }
    }
}
