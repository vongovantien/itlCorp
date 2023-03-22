﻿using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System.Linq;
using System;
using System.Collections.Generic;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.Common;
using Microsoft.Extensions.Localization;
using eFMS.IdentityServer.DL.UserManager;
using AutoMapper.QueryableExtensions;
using ITL.NetCore.Connection.Caching;
using System.Linq.Expressions;
using eFMS.API.Common.Globals;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Common.Models;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeService : RepositoryBaseCache<CatCharge, CatChargeModel>, ICatChargeService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatChargeDefaultAccount> chargeDefaultRepository;
        private readonly ICatCurrencyService currencyService;
        private readonly ICatUnitService catUnitService;
        private readonly IContextBase<SysOffice> sysOfficeRepository;
        private readonly IContextBase<CatChargeGroup> catChargeGroupRepository;




        public CatChargeService(IContextBase<CatCharge> repository,
            ICacheServiceBase<CatCharge> cacheService,
            IMapper mapper,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user,
            IContextBase<CatChargeDefaultAccount> chargeDefaultRepo,
            ICatCurrencyService currService,
            IContextBase<SysOffice> sysOfficeRepo,
            IContextBase<CatChargeGroup> catChargeGroupRepo,
            ICatUnitService unitService) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            chargeDefaultRepository = chargeDefaultRepo;
            currencyService = currService;
            currentUser = user;
            catUnitService = unitService;
            sysOfficeRepository = sysOfficeRepo;
            catChargeGroupRepository = catChargeGroupRepo;

            SetChildren<CsShipmentSurcharge>("Id", "ChargeId");
            SetChildren<CatPartnerCharge>("Id", "ChargeId");
        }

        public HandleState AddCharge(CatChargeAddOrUpdateModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                Guid chargeId = Guid.NewGuid();
                model.Charge.Id = chargeId;
                //model.Charge.Active = true;
                model.Charge.UserCreated = model.Charge.UserModified = currentUser.UserID;
                model.Charge.DatetimeCreated = DateTime.Now;

                // Update permission
                model.Charge.GroupId = currentUser.GroupId;
                model.Charge.DepartmentId = currentUser.DepartmentId;
                model.Charge.OfficeId = currentUser.OfficeID;
                model.Charge.CompanyId = currentUser.CompanyID;

                try
                {
                    var hs = DataContext.Add(model.Charge, false);
                    if (hs.Success == false) return hs;
                    foreach (var x in model.ListChargeDefaultAccount)
                    {
                        x.ChargeId = chargeId;
                        x.Active = true;
                        x.UserCreated = x.UserModified = currentUser.UserID;
                        x.DatetimeCreated = DateTime.Now;
                        chargeDefaultRepository.Add(x, false);
                    }
                    chargeDefaultRepository.SubmitChanges();
                    DataContext.SubmitChanges();
                    ClearCache();
                    Get();
                    trans.Commit();
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public HandleState UpdateCharge(CatChargeAddOrUpdateModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    model.Charge = SetDefaultUpdateData(model.Charge);
                    var hs = DataContext.Update(model.Charge, x => x.Id == model.Charge.Id, false);
                    if (hs.Success == false) return hs;

                    foreach (var item in model.ListChargeDefaultAccount)
                    {
                        if (item.Id == 0)
                        {
                            item.ChargeId = model.Charge.Id;
                            item.UserCreated = item.UserModified = currentUser.UserID;
                            item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                            chargeDefaultRepository.Add(item, false);
                        }
                        else
                        {
                            chargeDefaultRepository.Delete(x => x.ChargeId == item.ChargeId);
                            item.UserModified = currentUser.UserID;
                            item.DatetimeModified = DateTime.Now;
                            item.Id = 0;
                            chargeDefaultRepository.Add(item, false);
                        }
                    }
                    if(model.ListChargeDefaultAccount.Count() ==0)
                    {
                        var objChargeDefaultCurrent = chargeDefaultRepository.Get(x => x.ChargeId == model.Charge.Id).FirstOrDefault();
                        if(objChargeDefaultCurrent != null)
                        {
                            chargeDefaultRepository.Delete(x => x.ChargeId == objChargeDefaultCurrent.ChargeId);
                        }
                    }
                    chargeDefaultRepository.SubmitChanges();
                    DataContext.SubmitChanges();
                    trans.Commit();
                    ClearCache();
                    Get();
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        private CatCharge SetDefaultUpdateData(CatCharge charge)
        {
            charge.UserModified = currentUser.UserID;
            charge.DatetimeModified = DateTime.Now;
            var oldCharge = DataContext.First(x => x.Id == charge.Id);
            charge.UserCreated = oldCharge.UserCreated;
            charge.DatetimeCreated = oldCharge.DatetimeCreated;
            charge.GroupId = oldCharge.GroupId;
            charge.DepartmentId = oldCharge.DepartmentId;
            charge.OfficeId = oldCharge.OfficeId;
            charge.CompanyId = oldCharge.CompanyId;
            return charge;
        }

        public CatChargeAddOrUpdateModel GetChargeById(Guid id)
        {
            CatChargeAddOrUpdateModel returnCharge = new CatChargeAddOrUpdateModel();
            var charge = DataContext.Get(x => x.Id == id).FirstOrDefault();
            var listChargeDefault = chargeDefaultRepository.Get(x => x.ChargeId == id).ToList();
            returnCharge.Charge = charge;
           // Update permission
           ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catCharge);
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = returnCharge.Charge.UserCreated,
                CompanyId = returnCharge.Charge.CompanyId,
                DepartmentId = returnCharge.Charge.DepartmentId,
                OfficeId = returnCharge.Charge.OfficeId,
                GroupId = returnCharge.Charge.GroupId
            };
            returnCharge.Permission = new PermissionAllowBase
            {
                AllowUpdate = PermissionExtention.GetPermissionDetail(permissionRangeWrite, baseModel, currentUser),
            };

            returnCharge.ListChargeDefaultAccount = listChargeDefault;

            return returnCharge;
        }
        
        public IQueryable<CatChargeModel> Paging(CatChargeCriteria criteria, int page, int size, out int rowsCount)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catCharge);
            var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None)
            {
                rowsCount = 0;
                return null;
            }

            Expression<Func<CatCharge, bool>> query = null;
            if (string.IsNullOrEmpty(criteria.All))
            {
                query = x => (x.ChargeNameEn ?? "").IndexOf(criteria.ChargeNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.ChargeNameVn ?? "").IndexOf(criteria.ChargeNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Type ?? "").IndexOf(criteria.Type ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.ServiceTypeId ?? "").IndexOf(criteria.ServiceTypeId ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Active == criteria.Active || criteria.Active == null)
                                    ;
            }
            else
            {
                query = x => ((x.ChargeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ChargeNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Type ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ServiceTypeId ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   && (x.Active == criteria.Active || criteria.Active == null);
            }

            // Query with Permission Range.
            switch (rangeSearch)
            {
                case PermissionRange.Owner:
                    query = query.And(x => x.UserCreated == currentUser.UserID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Group:
                    query = query.And(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                        || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Department:
                    query = query.And(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                        || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Office:
                    query = query.And(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID) || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Company:
                    query = query.And(x => x.CompanyId == currentUser.CompanyID || x.UserCreated == currentUser.UserID);
                    break;
                default:
                    break;
            }
            var data = DataContext.Paging(query, page, size, x => x.DatetimeModified, false, out rowsCount);

            var datamap = data.ProjectTo<CatChargeModel>(mapper.ConfigurationProvider);
            return datamap;
        }

        public IQueryable<CatChargeModel> Query(CatChargeCriteria criteria)
        {
            return QueryCriteria(criteria);
        }

        public HandleState DeleteCharge(Guid id)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    //ChangeTrackerHelper.currentUser = currentUser.UserID;
                    var hs = DataContext.Delete(x => x.Id == id, false);
                    if (hs.Success)
                    {
                        var listChargeDefaultAccount = chargeDefaultRepository.Get(x => x.ChargeId == id).ToList();
                        foreach (var item in listChargeDefaultAccount)
                        {
                            chargeDefaultRepository.Delete(x => x.Id == item.Id, false);
                        }
                        chargeDefaultRepository.SubmitChanges();
                        DataContext.SubmitChanges();
                        ClearCache();
                        Get();
                        trans.Commit();
                    }
                    return hs;

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public List<CatChargeImportModel> CheckValidImport(List<CatChargeImportModel> list)
        {
            var charges = Get();
            var units = catUnitService.Get();
            var currencies = currencyService.Get();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.ServiceName))
                {
                    item.ServiceNameError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var services = item.ServiceName.Split(";").Where(x => !string.IsNullOrEmpty(x));
                    string serviceToAdd = string.Empty;
                    foreach (var service in services)
                    {
                        var dataService = CustomData.Services.FirstOrDefault(x => x.Value.ToLower() == service.ToLower().Trim());
                        if (dataService == null)
                        {
                            item.ServiceNameError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_SERVICE_TYPE_NOT_FOUND, service];
                            item.IsValid = false;
                            break;
                        }
                        serviceToAdd = serviceToAdd + dataService.Value + ";";
                    }
                    if (serviceToAdd.Length > 0)
                    {
                        serviceToAdd = serviceToAdd.Substring(0, serviceToAdd.Length - 1);
                        item.ServiceTypeId = serviceToAdd;
                    }
                }
                if (string.IsNullOrEmpty(item.ChargeNameEn))
                {
                    item.ChargeNameEnError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ChargeNameVn))
                {
                    item.ChargeNameVnError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (item.UnitPrice < 0)
                {
                    item.IsValid = false;
                    item.UnitPriceError = "Price is not allow empty and must be a decimal number";
                }
                if (item.Vatrate > 99)
                {
                    item.IsValid = false;
                    item.VatrateError = "VAT is must be lower than 100";
                }
                
                if (string.IsNullOrEmpty(item.Type))
                {
                    item.TypeError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_TYPE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    string type = CustomData.ChargeTypes.FirstOrDefault(x => x == item.Type.ToUpper());
                    if(string.IsNullOrEmpty(type))
                    {
                        item.TypeError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_TYPE_NOT_FOUND, item.Type];
                        item.IsValid = false;
                    }
                    else
                    {
                        item.Type = item.Type.ToUpper();
                    }
                }
                if (!string.IsNullOrEmpty(item.Code))
                {
                    var charge = charges.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower());
                    if (charge != null)
                    {
                        item.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_CODE_EXISTED], item.Code);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.Code?.ToLower() == item.Code?.ToLower()) > 1)
                    {
                        item.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_CODE_DUPLICATED], item.Code);
                        item.IsValid = false;
                    }
                }
                else
                {
                    item.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_CODE_EMPTY];
                    item.IsValid = false;
                }

            });
            return list;
        }

        public HandleState Import(List<CatChargeImportModel> data)
        {
            try
            {
                foreach (var item in data)
                {
                    var charge = new CatCharge
                    {
                        Id = Guid.NewGuid(),
                        Code = item.Code,
                        ChargeNameEn = item.ChargeNameEn,
                        ChargeNameVn = item.ChargeNameVn,
                        UnitId = item.UnitId,
                        UnitPrice = item.UnitPrice,
                        CurrencyId = item.CurrencyId,
                        Type = item.Type,
                        ServiceTypeId = item.ServiceTypeId,
                        Vatrate = item.Vatrate,
                        Active = item.Status.Trim().ToLower() == "active",
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID,
                        CompanyId = currentUser.CompanyID,
                        OfficeId = currentUser.OfficeID,
                        GroupId = currentUser.GroupId,
                        DepartmentId = currentUser.DepartmentId
                };
                    DataContext.Add(charge, false);
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

        public IQueryable<CatChargeModel> GetSettlePaymentCharges(string keySearch, bool? active, int? size)
        {
            IQueryable<CatChargeModel> list = null;
            if (size != null)
            {
                int pageSize = (int)size;
                list = Paging(x => x.Type != "DEBIT" && (x.Active == active || active == null)
                                                     && (x.Code.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                            || x.ChargeNameEn.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                            || x.ChargeNameVn.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                            , 0, pageSize);
            }
            else
            {
                list = Get(x => x.Type != "DEBIT" && (x.Active == active || active == null)
                                                  && (x.Code.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                            || x.ChargeNameEn.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                            || x.ChargeNameVn.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                            );
            }

            return list;
        }

        public object GetListService()
        {
            return CustomData.Services;
        }

        public IQueryable<CatChargeModel> GetBy(string type)
        {
            var data = DataContext.Get(x => x.Type == type);
            if (data == null) return null;
            return data.ProjectTo<CatChargeModel>(mapper.ConfigurationProvider);
        }

        public bool CheckAllowPermissionAction(Guid id, PermissionRange range)
        {
            CatCharge charge = DataContext.Get(o => o.Id == id).FirstOrDefault();
            if (charge == null)
            {
                return false;
            }

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = charge.UserCreated,
                CompanyId = charge.CompanyId,
                DepartmentId = charge.DepartmentId,
                OfficeId = charge.OfficeId,
                GroupId = charge.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, range, currentUser);

            if (code == 403) return false;

            return true;

        }

        private IQueryable<CatChargeModel> QueryCriteria(CatChargeCriteria criteria)
        {
            Expression<Func<CatCharge, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.ChargeNameEn ?? "").IndexOf(criteria.ChargeNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.ChargeNameVn ?? "").IndexOf(criteria.ChargeNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Type ?? "").IndexOf(criteria.Type ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.ServiceTypeId ?? "").Contains(criteria.ServiceTypeId ?? "", StringComparison.OrdinalIgnoreCase)
                                    && (x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                query = x => ((x.ChargeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ChargeNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Type ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ServiceTypeId ?? "").Contains(criteria.All ?? "", StringComparison.OrdinalIgnoreCase))
                                   && (x.Active == criteria.Active || criteria.Active == null);
            }
            if(!string.IsNullOrEmpty(criteria.OfficeId))
            {
                query = query.And(x => !string.IsNullOrEmpty(x.Offices) && x.Offices.ToLower().Contains(criteria.OfficeId.ToLower()));
            } 
            var list = DataContext.Get(query);
            var chargeGroup = catChargeGroupRepository.Get();
            var catChargeLst = (from charge in list
                                join chgroup in chargeGroup on charge.ChargeGroup equals chgroup.Id into chargeGrp
                                from chgroup in chargeGrp.DefaultIfEmpty()
                                select new CatChargeModel
                                {
                                    Id = charge.Id,
                                    Code = charge.Code,
                                    ChargeNameVn = charge.ChargeNameVn,
                                    ChargeNameEn = charge.ChargeNameEn,
                                    ServiceTypeId = charge.ServiceTypeId,
                                    Type = charge.Type,
                                    CurrencyId = charge.CurrencyId,
                                    UnitPrice = charge.UnitPrice,
                                    UnitId = charge.UnitId,
                                    Vatrate = charge.Vatrate,
                                    IncludedVat = charge.IncludedVat,
                                    UserCreated = charge.UserCreated,
                                    DatetimeCreated = charge.DatetimeCreated,
                                    UserModified = charge.UserModified,
                                    DatetimeModified = charge.DatetimeModified,
                                    Active = charge.Active,
                                    InactiveOn = charge.InactiveOn,
                                    GroupId = charge.GroupId,
                                    DepartmentId = charge.DepartmentId,
                                    OfficeId = charge.OfficeId,
                                    CompanyId = charge.CompanyId,
                                    ChargeGroup = charge.ChargeGroup,
                                    ChargeGroupName = chgroup != null ? chgroup.Name : null,
                                    CreditCharge = charge.CreditCharge != null ? charge.CreditCharge : null,
                                    DebitCharge = charge.DebitCharge != null ? charge.DebitCharge : null,
                                    Offices = charge.Offices

                                })?.OrderBy(x => x.DatetimeModified);
            List<CatChargeModel> lst = new List<CatChargeModel>();
            lst = catChargeLst.ToList();
            if (lst.Count() > 0)
            {
                foreach (var item in lst)
                {
                    if (!String.IsNullOrEmpty(item.CreditCharge.ToString()))
                    {
                        var chargeCredit = DataContext.Get(x => x.Id == item.CreditCharge).FirstOrDefault();

                        item.BuyingCode = chargeCredit.Code;
                        item.BuyingName = chargeCredit.ChargeNameEn;
                    }
                    if (!String.IsNullOrEmpty(item.DebitCharge.ToString()))
                    {
                        var chargeDebit = DataContext.Get(x => x.Id == item.DebitCharge).FirstOrDefault();

                        item.SellingCode = chargeDebit.Code;
                        item.SellingName = chargeDebit.ChargeNameEn;
                    }
                    var arrOffice = item.Offices.Split(",").ToArray();
                    string officeStr = string.Empty;
                    if (arrOffice.Length > 0)
                    {
                        foreach (var o in arrOffice)
                        {
                            if (!String.IsNullOrEmpty(o))
                            {
                                var office = sysOfficeRepository.Where(x => x.Id.ToString().ToLower() == o.ToLower()).FirstOrDefault().Code.ToUpper();
                                officeStr += office + ";";
                            }
                        }
                    }
                    item.OfficesName = officeStr.TrimEnd(';');
                    item.ServiceTypeId = Common.CommonData.GetServicesName(item.ServiceTypeId);
                }
            }
            return lst.AsQueryable();

        }
        public IQueryable<CatChargeModel> QueryByPermission(CatChargeCriteria criteria, PermissionRange range)
        {
            IQueryable<CatChargeModel> data = null;
            var list = QueryCriteria(criteria);

            switch (range)
            {
                case PermissionRange.All:
                    data = list;
                    break;
                case PermissionRange.Owner:
                    data = list.Where(x => x.UserCreated == currentUser.UserID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Group:
                    data = list.Where(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                        || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Department:
                    data = list.Where(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                        || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Office:
                    data = list.Where(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID) || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Company:
                    data = list.Where(x => x.CompanyId == currentUser.CompanyID || x.UserCreated == currentUser.UserID);
                    break;
                default:
                    break;
            }

            return data;
        }

        public IQueryable<CatChargeModel> QueryExport(CatChargeCriteria criteria)
        {
            IQueryable<CatChargeModel> data = null;

            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catCharge);
            var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None)
            {
                return data;
            }
            data = QueryByPermission(criteria, rangeSearch);
            return data;
        }

        /// <summary>
        /// Get charge data with current user service and charge type
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IQueryable<CatChargeModel> GetChargesWithCurrentUserService(List<string> serviceType, string type)
        {
            //ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catCharge);
            //var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
            //if (rangeSearch == PermissionRange.None)
            //{
            //    return null;
            //}

            Expression<Func<CatCharge, bool>> query = null;

            if (serviceType.Count > 0)
            {
                query = x => (serviceType.Any(z => x.ServiceTypeId.Contains(z)));
            }
            if (!string.IsNullOrEmpty(type))
            {
                if (query == null)
                {
                    query = x => (x.Type ?? "").IndexOf(type ?? "", StringComparison.OrdinalIgnoreCase) > -1;
                }
                else
                {
                    query = query.And(x => (x.Type ?? "").IndexOf(type ?? "", StringComparison.OrdinalIgnoreCase) > -1);
                }
            }
            // Query with Permission Range.
            //switch (rangeSearch)
            //{
            //    case PermissionRange.Owner:
            //        query = query.And(x => x.UserCreated == currentUser.UserID && x.CompanyId == currentUser.CompanyID);
            //        break;
            //    case PermissionRange.Group:
            //        query = query.And(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
            //                            || x.UserCreated == currentUser.UserID);
            //        break;
            //    case PermissionRange.Department:
            //        query = query.And(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
            //                            || x.UserCreated == currentUser.UserID);
            //        break;
            //    case PermissionRange.Office:
            //        query = query.And(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID) || x.UserCreated == currentUser.UserID);
            //        break;
            //    case PermissionRange.Company:
            //        query = query.And(x => x.CompanyId == currentUser.CompanyID || x.UserCreated == currentUser.UserID);
            //        break;
            //    default:
            //        break;
            //}
            var data = DataContext.Get(query);

            var datamap = data.ProjectTo<CatChargeModel>(mapper.ConfigurationProvider);
            return datamap;
        }
    }
}
