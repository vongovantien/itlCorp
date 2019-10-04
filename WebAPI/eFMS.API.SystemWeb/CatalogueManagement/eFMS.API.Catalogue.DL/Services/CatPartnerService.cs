using AutoMapper;
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
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPartnerService : RepositoryBase<CatPartner, CatPartnerModel>, ICatPartnerService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IDistributedCache cache;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatSaleman> salemanRepository;
        public CatPartnerService(IContextBase<CatPartner> repository, 
            IMapper mapper, 
            IStringLocalizer<LanguageSub> localizer, 
            IDistributedCache distributedCache, 
            ICurrentUser user,
            IContextBase<CatSaleman> salemanRepo) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            cache = distributedCache;
            currentUser = user;
            salemanRepository = salemanRepo;
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

        public IQueryable<CatPartner> GetPartners()
        {
            var lstPartner = RedisCacheHelper.GetObject<List<CatPartner>>(cache, Templates.CatPartner.NameCaching.ListName);
            IQueryable<CatPartner> data = null;
            if (lstPartner != null)
            {
                data = lstPartner.AsQueryable();
            }
            else
            {
                data = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.CatPartner.NameCaching.ListName, data);
            }
            var results = data?.Select(x => mapper.Map<CatPartnerModel>(x));
            return results;
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
            partner.Inactive = false;
            if(partner.InternalReferenceNo != null)
            {
                partner.Id = partner.InternalReferenceNo + "." + partner.TaxCode;
            }
            else
            {
                partner.Id = partner.TaxCode;
            }
            var hs = DataContext.Add(partner, false);
            if (hs.Success)
            {
                cache.Remove(Templates.CatPartner.NameCaching.ListName);
                //RedisCacheHelper.SetObject(cache, Templates.CatPartner.NameCaching.ListName, DataContext.Get().ToList());
                var salemans = mapper.Map<List<CatSaleman>>(entity.SaleMans);
                salemans.ForEach(x => {
                    x.PartnerId = partner.Id;
                    x.CreateDate = DateTime.Now;
                    x.UserCreated = currentUser.UserID;
                });
                salemanRepository.Add(salemans, false);
            }
            DataContext.SubmitChanges();
            salemanRepository.SubmitChanges();
            return hs;
        }
        public HandleState Update(CatPartnerModel model)
        {
            var entity = mapper.Map<CatPartner>(model);
            entity.DatetimeModified = DateTime.Now;
            entity.UserModified = currentUser.UserID;
            if (entity.Inactive == true)
            {
                entity.InactiveOn = DateTime.Now;
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                cache.Remove(Templates.CatPartner.NameCaching.ListName);
            }
            return hs;
        }
        public HandleState Delete(string id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                cache.Remove(Templates.CatPartner.NameCaching.ListName);
            }
            return hs;
        }
        #endregion
        
        public List<CatPartnerViewModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CatPartnerViewModel> results = null;
            var list = Query(criteria);
            if (list == null)
            {
                rowsCount = 0;
                return results;
            }
            list = list.OrderByDescending(x => x.DatetimeModified).ToList();
            rowsCount = list.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = list.Skip((page - 1) * size).Take(size).ToList();
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
                    SalePersonName = item.Key != null ? ((eFMSDataContext)DataContext.DC).SysUser.First(x => x.Id == item.Key).Username : null,
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
            var partners = GetPartners().Where(x => (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
            var query = (from partner in partners
                         join user in ((eFMSDataContext)DataContext.DC).SysUser on partner.UserCreated equals user.Id into userPartners
                         from y in userPartners.DefaultIfEmpty()
                         join saleman in ((eFMSDataContext)DataContext.DC).SysUser on partner.SalePersonId equals saleman.Id into prods
                         from x in prods.DefaultIfEmpty()
                         select new { user = y, partner, saleman = x }
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
                           //&& (x.partner.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.AccountNo ?? "").IndexOf(criteria.AccountNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Inactive == criteria.Inactive || criteria.Inactive == null)
                           ));
            }
            else
            {
                query = query.Where(x =>
                           ((x.partner.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.PartnerNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.PartnerNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.AddressVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.TaxCode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.Tel ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.Fax ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.user.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.AccountNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           )
                           //&& (x.partner.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Inactive == criteria.Inactive || criteria.Inactive == null));
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
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach (var item in data)
                {
                    var partner = mapper.Map<CatPartner>(item);
                    partner.UserCreated = currentUser.UserID;
                    partner.DatetimeModified = DateTime.Now;
                    partner.DatetimeCreated = DateTime.Now;
                    partner.Id = partner.AccountNo = partner.TaxCode;
                    partner.Inactive = false;
                    dc.CatPartner.Add(partner);
                }
                dc.SaveChanges();
                cache.Remove(Templates.CatPartner.NameCaching.ListName);
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        public List<CatPartnerImportModel> CheckValidImport(List<CatPartnerImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var partners = GetPartners()?.ToList();
            var users = dc.SysUser.ToList();
            var countries = RedisCacheHelper.GetObject<List<CatCountry>>(cache, Templates.CatCountry.NameCaching.ListName)?.AsQueryable();
            if(countries == null)
            {
                countries = dc.CatCountry;
            }
            var provinces = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province));
            var branchs = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Branch));
            var salemans = dc.SysUser;

            var allGroup = DataEnums.PARTNER_GROUP;
            var partnerGroups = allGroup.Split(";");
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.TaxCode))
                {
                    item.TaxCode = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_TAXCODE_EMPTY]);
                    item.IsValid = false;
                }
                else
                {
                    //bool isNumeric = int.TryParse(item.TaxCode, out int n);
                    if (list.Count(x => x.TaxCode.ToLower() == item.TaxCode.ToLower()) > 1)
                    {
                        item.TaxCode = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_TAXCODE_DUPLICATED]);
                        item.IsValid = false;
                    }
                    //if (isNumeric == false || n <0)
                    //{
                    //    item.TaxCode = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_TAXCODE_NOT_NUMBER]);
                    //    item.IsValid = false;
                    //}
                    else
                    {
                        if (partners.Any(x => x.TaxCode.ToLower() == item.TaxCode.ToLower()))
                        {
                            item.TaxCode = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_TAXCODE_EXISTED], item.TaxCode);
                            item.IsValid = false;
                        }
                    }
                }
                if (string.IsNullOrEmpty(item.PartnerGroup))
                {
                    item.PartnerGroup = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_GROUP_EMPTY]);
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
                            item.PartnerGroup = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_GROUP_NOT_FOUND], item.PartnerGroup);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.PartnerGroup = String.Join(";", groups);
                            if (item.PartnerGroup.Contains(DataEnums.CustomerPartner))
                            {
                                if (string.IsNullOrEmpty(item.SaleManName))
                                {
                                    item.SaleManName = stringLocalizer[LanguageSub.MSG_PARTNER_SALEMAN_EMPTY];
                                    item.IsValid = false;
                                }
                                else
                                {
                                    var salePerson = salemans.FirstOrDefault(i => i.Username == item.SaleManName);
                                    if (salePerson == null)
                                    {
                                        item.SaleManName = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_SALEMAN_NOT_FOUND], item.SaleManName);
                                        item.IsValid = false;
                                    }
                                    else
                                    {
                                        item.SalePersonId = salePerson.Id;
                                    }
                                }
                                //}
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(item.PartnerNameEn))
                {
                    item.PartnerNameEn = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_NAME_EN_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.PartnerNameVn))
                {
                    item.PartnerNameVn = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_NAME_VN_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ShortName))
                {
                    item.ShortName = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_SHORT_NAME_EMPTY]);
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CountryBilling)) {
                    item.CountryBilling = stringLocalizer[LanguageSub.MSG_PARTNER_COUNTRY_BILLING_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CityBilling))
                {
                    item.CityBilling = stringLocalizer[LanguageSub.MSG_PARTNER_PROVINCE_BILLING_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CountryShipping))
                {
                    item.CountryShipping = stringLocalizer[LanguageSub.MSG_PARTNER_COUNTRY_SHIPPING_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CityShipping))
                {
                    item.CityShipping = stringLocalizer[LanguageSub.MSG_PARTNER_PROVINCE_SHIPPING_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.AddressEn))
                {
                    item.AddressEn = stringLocalizer[LanguageSub.MSG_PARTNER_ADDRESS_BILLING_EN_NOT_FOUND];
                    item.IsValid = false;

                }
                if (string.IsNullOrEmpty(item.AddressVn))
                {
                    item.AddressVn = stringLocalizer[LanguageSub.MSG_PARTNER_ADDRESS_BILLING_VN_NOT_FOUND];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.AddressShippingEn))
                {
                    item.AddressShippingEn = stringLocalizer[LanguageSub.MSG_PARTNER_ADDRESS_SHIPPING_EN_NOT_FOUND];
                    item.IsValid = false;
                }
                //if (!string.IsNullOrEmpty(item.Profile))
                //{
                //    var workplace = branchs.FirstOrDefault(i => i.NameEn.ToLower() == item.Profile);
                //    if (workplace == null)
                //    {
                //        item.CityBilling = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_WORKPLACE_NOT_FOUND], item.Profile);
                //        item.IsValid = false;
                //    }
                //    else
                //    {
                //        item.WorkPlaceId = workplace.Id;
                //    }
                //}
                if (string.IsNullOrEmpty(item.AddressShippingVn))
                {
                    item.AddressShippingVn = stringLocalizer[LanguageSub.MSG_PARTNER_ADDRESS_SHIPPING_VN_NOT_FOUND];
                    item.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryBilling.ToLower());
                    if (country == null)
                    {
                        item.CountryBilling = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_COUNTRY_BILLING_NOT_FOUND], item.CountryBilling);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.CountryId = country.Id;
                        var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.CityBilling.ToLower() && i.CountryId == country.Id);
                        if (province == null)
                        {
                            item.CityBilling = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_PROVINCE_BILLING_NOT_FOUND], item.CityBilling);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.ProvinceId = province.Id;
                        }
                    }
                    var countryShipping = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryShipping.ToLower());
                    if (countryShipping == null)
                    {
                        item.CountryShipping = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_COUNTRY_SHIPPING_NOT_FOUND], item.CountryShipping);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.CountryShippingId = countryShipping.Id;
                        var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.CityShipping.ToLower() && i.CountryId == item.CountryId);
                        if (province == null)
                        {
                            item.CityShipping = string.Format(stringLocalizer[LanguageSub.MSG_PARTNER_PROVINCE_SHIPPING_NOT_FOUND], item.CityShipping);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.ProvinceShippingId = province.Id;
                        }
                    }
                    if(item.DepartmentId == null)
                    {
                        item.DepartmentId = "Head Office";
                    }
                }
            });
            return list;
        }
        #endregion

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
