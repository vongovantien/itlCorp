﻿using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Helpers;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
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

        public List<CatPartnerImportModel> CheckValidImport(List<CatPartnerImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var partners = dc.CatPartner.Where(x => x.Inactive == false).ToList();
            var partnerGroups = DataEnums.CatPartnerGroups;
            var users = dc.SysUser.Where(x => x.Inactive == false).ToList();
            var countries = dc.CatCountry.Where(x => x.Inactive == false);
            var provinces = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province) && x.Inactive == false);
            var branchs = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Branch) && x.Inactive == false);
            var salemans = dc.SysUser.Where(x => x.Inactive == false);
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.TaxCode))
                {
                    item.TaxCode = string.Format("Tax code is not allow empty!|wrong");
                    item.IsValid = false;
                }
                else
                {
                    if(partners.Any(x => (x.TaxCode??"").ToLower() == item.TaxCode.ToLower())){
                        item.TaxCode = string.Format("Tax code {0} has been existed!|wrong", item.TaxCode);
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.PartnerGroup))
                {
                    item.PartnerGroup = string.Format("Partner group is not allow empty!|wrong");
                    item.IsValid = false;
                }
                else 
                {
                    if (item.PartnerGroup.ToLower() == DataEnums.AllPartner.ToLower())
                    {
                        item.PartnerGroup = "AGENT;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER";
                    }
                    else
                    {
                        var group = partnerGroups.FirstOrDefault(x => x.Id.ToLower() == item.PartnerGroup.ToLower());
                        if (group == null)
                        {
                            item.PartnerGroup = string.Format("Partner group {0} is not found!|wrong", item.PartnerGroup);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.PartnerGroup = group.Id;
                            if (group.Id == DataEnums.CustomerPartner)
                            {
                                if (string.IsNullOrEmpty(item.SalePersonId))
                                {
                                    item.PartnerGroup = string.Format("Saleman is not allow empty!|wrong");
                                    item.IsValid = false;
                                }
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(item.PartnerNameEn))
                {
                    item.PartnerNameEn = string.Format("Partner name EN is not allow empty!|wrong");
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.PartnerNameVn))
                {
                    item.PartnerNameVn = string.Format("Partner name VN is not allow empty!|wrong");
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ShortName))
                {
                    item.ShortName = string.Format("Short name is not allow empty!|wrong");
                    item.IsValid = false;
                }
                if (!string.IsNullOrEmpty(item.CountryBilling))
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryBilling.ToLower());
                    if (country == null)
                    {
                        item.CountryBilling = string.Format("Country '{0}' is not found!|wrong", item.CountryBilling);
                        item.CityBilling = string.Format("Country '{0}' is not found!|wrong", item.CountryBilling);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.CountryId = country.Id;
                        if (!string.IsNullOrEmpty(item.CityBilling))
                        {
                            var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.CityBilling.ToLower() && i.CountryId == country.Id);
                            if(province == null)
                            {
                                item.CityBilling = string.Format("City billing '{0}' is not found!|wrong", item.CityBilling);
                                item.IsValid = false;
                            }
                        }
                    }
                }
                else
                {
                    item.CountryBilling = "Country is empty. Please check again!|wrong";
                    item.Inactive = false;
                }
                if (!string.IsNullOrEmpty(item.CountryShipping))
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryShipping.ToLower());
                    if (country == null)
                    {
                        item.CountryShipping = string.Format("Country '{0}' is not found!|wrong", item.CountryShipping);
                        item.CityShipping = string.Format("Country '{0}' is not found!|wrong", item.CountryShipping);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.CountryId = country.Id;
                        if (!string.IsNullOrEmpty(item.CityShipping))
                        {
                            var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.CityShipping.ToLower() && i.CountryId == country.Id);
                            if (province == null)
                            {
                                item.CityBilling = string.Format("City shipping '{0}' is not found!|wrong", item.CityShipping);
                                item.IsValid = false;
                            }
                        }
                    }
                }
                else
                {
                    item.CountryShipping = "Country is empty. Please check again!| wrong";
                }
                if (!string.IsNullOrEmpty(item.Profile)){
                    var workplace = branchs.FirstOrDefault(i => i.NameEn.ToLower() == item.Profile);
                    if(workplace == null)
                    {
                        item.CityBilling = string.Format("Workplace '{0}' is not found!|wrong", item.Profile);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.WorkPlaceId = workplace.Id;
                    }
                }
                if (!string.IsNullOrEmpty(item.SaleManName)) {
                    var salePerson = salemans.FirstOrDefault(i => i.Username == item.SaleManName);
                    if(salePerson == null)
                    {
                        item.SaleManName = string.Format("Sale man '{0}' is not found!|wrong", item.SaleManName);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.SalePersonId = salePerson.Id;
                    }
                }
            });
            return list;
        }

        public List<DepartmentPartner> GetDepartments()
        {
            return DataEnums.Departments;
        }

        public HandleState Import(List<CatPartnerImportModel> data)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach (var item in data)
                {
                    var partner = mapper.Map<CatPartner>(item);
                    partner.UserCreated = ChangeTrackerHelper.currentUser;
                    partner.DatetimeCreated = DateTime.Now;
                    partner.Id = partner.AccountNo = partner.TaxCode;
                    //partner.Inactive = item.Status.ToString().ToLower() == "active" ? false : true;
                    dc.CatPartner.Add(partner);
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public IQueryable<CatPartnerViewModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount)
        {
            //var query = GetQueryExpression(criteria);
            //return Paging(query, page, size, out rowsCount);
            List<CatPartnerViewModel> results = null;
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

        public IQueryable<CatPartnerViewModel> Query(CatPartnerCriteria criteria)
        {
            var query = (from partner in ((eFMSDataContext)DataContext.DC).CatPartner
                         join user in ((eFMSDataContext)DataContext.DC).SysUser on partner.UserCreated equals user.Id into userPartners
                         from y in userPartners.DefaultIfEmpty()
                         join saleman in ((eFMSDataContext)DataContext.DC).SysUser on partner.SalePersonId equals saleman.Id into prods
                         from x in prods.DefaultIfEmpty()
                         select new { user = y, partner, saleman = x }
                          );
            IQueryable<CatPartnerViewModel> results = null;
            string partnerGroup = PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup);
            if (criteria.All == null)
            {
                results = query.Where(x => ((x.partner.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.user.Username ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           && (x.partner.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           )).Select(x => new CatPartnerViewModel {
                               Id = x.partner.Id,
                               PartnerGroup = x.partner.PartnerGroup,
                               PartnerNameVn = x.partner.PartnerNameVn,
                               PartnerNameEn = x.partner.PartnerNameEn,
                               AddressVn = x.partner.AddressVn,
                               AddressEn = x.partner.AddressEn,
                               ShortName = x.partner.ShortName,
                               CountryId = x.partner.CountryId,
                               AccountNo = x.partner.AccountNo,
                               Tel = x.partner.Tel,
                               Fax = x.partner.Fax,
                               TaxCode = x.partner.TaxCode,
                               Email = x.partner.Email,
                               Website = x.partner.Website,
                               BankAccountNo = x.partner.BankAccountNo,
                               BankAccountName = x.partner.BankAccountName,
                               BankAccountAddress = x.partner.BankAccountAddress,
                               Note = x.partner.Note,
                               SalePersonId = x.partner.SalePersonId,
                               Public = x.partner.Public,
                               CreditAmount = x.partner.CreditAmount,
                               DebitAmount = x.partner.DebitAmount,
                               RefuseEmail = x.partner.RefuseEmail,
                               ReceiveAttachedWaybill = x.partner.ReceiveAttachedWaybill,
                               RoundedSoamethod = x.partner.RoundedSoamethod,
                               TaxExemption = x.partner.TaxExemption,
                               ReceiveEtaemail = x.partner.ReceiveEtaemail,
                               ShowInDashboard = x.partner.ShowInDashboard,
                               ProvinceId = x.partner.ProvinceId,
                               ParentId = x.partner.ParentId,
                               PercentCredit = x.partner.PercentCredit,
                               AlertPercentCreditEmail = x.partner.AlertPercentCreditEmail,
                               PaymentBeneficiary = x.partner.PaymentBeneficiary,
                               UsingParrentRateCard = x.partner.UsingParrentRateCard,
                               SugarId = x.partner.SugarId,
                               BookingOverdueDay = x.partner.BookingOverdueDay,
                               FixRevenueByProject = x.partner.FixRevenueByProject,
                               UserCreated = x.partner.UserCreated,
                               DatetimeCreated = x.partner.DatetimeCreated,
                               UserModified = x.partner.UserModified,
                               DatetimeModified = x.partner.DatetimeModified,
                               Inactive = x.partner.Inactive,
                               InactiveOn = x.partner.InactiveOn,
                               UserCreatedName = x.user.Username,
                               SalePersonName = x.saleman.Username
                            }).OrderBy(x => x.AccountNo);
            }
            else
            {
                results = query.Where(x => ((x.partner.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.AddressVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.TaxCode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.Tel ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.partner.Fax ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           || (x.user.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                           ) && ((x.partner.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0))
                           .Select(x => new CatPartnerViewModel {
                               Id = x.partner.Id,
                               PartnerGroup = x.partner.PartnerGroup,
                               PartnerNameVn = x.partner.PartnerNameVn,
                               PartnerNameEn = x.partner.PartnerNameEn,
                               AddressVn = x.partner.AddressVn,
                               AddressEn = x.partner.AddressEn,
                               ShortName = x.partner.ShortName,
                               CountryId = x.partner.CountryId,
                               AccountNo = x.partner.AccountNo,
                               Tel = x.partner.Tel,
                               Fax = x.partner.Fax,
                               TaxCode = x.partner.TaxCode,
                               Email = x.partner.Email,
                               Website = x.partner.Website,
                               BankAccountNo = x.partner.BankAccountNo,
                               BankAccountName = x.partner.BankAccountName,
                               BankAccountAddress = x.partner.BankAccountAddress,
                               Note = x.partner.Note,
                               SalePersonId = x.partner.SalePersonId,
                               Public = x.partner.Public,
                               CreditAmount = x.partner.CreditAmount,
                               DebitAmount = x.partner.DebitAmount,
                               RefuseEmail = x.partner.RefuseEmail,
                               ReceiveAttachedWaybill = x.partner.ReceiveAttachedWaybill,
                               RoundedSoamethod = x.partner.RoundedSoamethod,
                               TaxExemption = x.partner.TaxExemption,
                               ReceiveEtaemail = x.partner.ReceiveEtaemail,
                               ShowInDashboard = x.partner.ShowInDashboard,
                               ProvinceId = x.partner.ProvinceId,
                               ParentId = x.partner.ParentId,
                               PercentCredit = x.partner.PercentCredit,
                               AlertPercentCreditEmail = x.partner.AlertPercentCreditEmail,
                               PaymentBeneficiary = x.partner.PaymentBeneficiary,
                               UsingParrentRateCard = x.partner.UsingParrentRateCard,
                               SugarId = x.partner.SugarId,
                               BookingOverdueDay = x.partner.BookingOverdueDay,
                               FixRevenueByProject = x.partner.FixRevenueByProject,
                               UserCreated = x.partner.UserCreated,
                               DatetimeCreated = x.partner.DatetimeCreated,
                               UserModified = x.partner.UserModified,
                               DatetimeModified = x.partner.DatetimeModified,
                               Inactive = x.partner.Inactive,
                               InactiveOn = x.partner.InactiveOn,
                               UserCreatedName = x.user.Username,
                               SalePersonName = x.saleman.Username
                           }).OrderBy(x => x.AccountNo);
            }
            return results;
            //var data = Get();
            //IQueryable<CatPartnerModel> results = null;
            //string partnerGroup = PlaceTypeEx.GetPartnerGroup(criteria.PartnerGroup);
            //if (criteria.All == null)
            //{
            //    results = data.Where(x => ((x.Id ?? "").IndexOf(criteria.Id ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.ShortName ?? "").IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.AddressVn ?? "").IndexOf(criteria.AddressVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.TaxCode ?? "").IndexOf(criteria.TaxCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.Fax ?? "").IndexOf(criteria.Fax ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.UserCreated ?? "").IndexOf(criteria.UserCreated ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               && (x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               ));
            //}
            //else
            //{
            //    results = data.Where(x => ((x.Id ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.ShortName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.AddressVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.TaxCode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.Tel ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.Fax ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               || (x.UserCreated ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            //               ) && ((x.PartnerGroup ?? "").IndexOf(partnerGroup ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            //}
            //return results;
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
