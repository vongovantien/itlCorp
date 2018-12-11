﻿using eFMS.API.Log.DL.Common;
using eFMS.API.Log.DL.IService;
using eFMS.API.Log.DL.Models;
using eFMS.API.Log.Service;
using eFMS.API.Log.Service.Contexts;
using eFMS.API.Log.Service.Models;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Collections;
using eFMS.API.Log.Models;

namespace eFMS.API.Log.DL.Services
{
    public class CategoryLogService : ICategoryLogService
    {
        private readonly ChangeLogContext mongoContext = null;
        private readonly IContextBase<SysUser> sysUserContext;

        public CategoryLogService(IOptions<Settings> settings, IContextBase<SysUser> userContext)
        {
            mongoContext = new ChangeLogContext(settings);
            sysUserContext = userContext;
        }
        public List<LogModel> Paging(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            IEnumerable<LogModel> data = null;
            rowsCount = 0;
            switch (criteria.TableType)
            {
                case CategoryTable.CatCharge:
                    data = PagingCatCharge(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.CatChargeDefaultAccount:
                    data = PagingCatChargeDefaultAccount(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.CatCommonityGroup:
                    data = PagingCatCommonityGroup(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.CatCommodity:
                    data = PagingCatCommodity(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.CatCountry:
                    data = PagingCatCountry(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.CatCurrency:
                    data = PagingCatCurrency(criteria, page, size, out rowsCount);
                    break;
                //case CategoryTable.CatCurrencyExchange:
                //    data = PagingCatCurrencyExchange(query, page, size, out rowsCount);
                //    break;
                case CategoryTable.CatPartner:
                    data = PagingCatPartner(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.CatPlace:
                    data = PagingCatPlace(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.CatStage:
                    data = PagingCatStage(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.CatUnit:
                    data = PagingCatUnit(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.Warehouse:
                    data = PagingCatPlace(criteria, page, size, out rowsCount);
                    break;

            }
            if (data == null) return null;
            var result = (from s in data
                          join user in ((eFMSDataContext)sysUserContext.DC).SysUser on s.UserUpdated equals user.Id
                          select new LogModel
                          {
                              Id = s.Id,
                              UserUpdated = user.Username,
                              Action = s.Action,
                              DatetimeUpdated = s.DatetimeUpdated,
                              PropertyChange = s.PropertyChange,
                              Name = s.Name,
                              Code = s.Code,
                              ObjectId = s.ObjectId
                          }).ToList();
            return result;
        }
        public List<CategoryCollectionModel> GetCollectionName()
        {
            List<CategoryCollectionModel> collections = new List<CategoryCollectionModel>
            {
                new CategoryCollectionModel { Id = (int)CategoryTable.CatCharge, Name = "Charge" },
                new CategoryCollectionModel { Id = (int)CategoryTable.CatChargeDefaultAccount, Name = "Charge Default Account" },
                new CategoryCollectionModel { Id = (int)CategoryTable.CatCommonityGroup, Name = "Commonity Group" },
                new CategoryCollectionModel { Id = (int)CategoryTable.CatCommodity, Name = "Commodity"},
                new CategoryCollectionModel { Id = (int)CategoryTable.CatCountry, Name = "Country" },
                new CategoryCollectionModel { Id = (int)CategoryTable.CatCurrency, Name = "Currency" },
                new CategoryCollectionModel { Id = (int)CategoryTable.CatPartner, Name = "Partner" },
                //new CategoryCollectionModel { Id = (int)CategoryTable.CatPlace, Name = "Place" },
                new CategoryCollectionModel { Id = (int)CategoryTable.Warehouse, Name = "Warehouse" },
                new CategoryCollectionModel { Id = (int)CategoryTable.PortIndex, Name = "Port Index" },
                new CategoryCollectionModel { Id = (int)CategoryTable.Province, Name = "Province" },
                new CategoryCollectionModel { Id = (int)CategoryTable.District, Name = "District" },
                new CategoryCollectionModel { Id = (int)CategoryTable.Ward, Name = "Ward" },
                new CategoryCollectionModel { Id = (int)CategoryTable.CatStage, Name = "Stage" },
                new CategoryCollectionModel { Id = (int)CategoryTable.CatUnit, Name = "Unit" }
            };
            return collections;
        }

        private IEnumerable<LogModel> PagingCatStage(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            Expression<Func<CatStage, bool>> stageEx = x => x.NewObject.Code.Contains(criteria.Query ?? "")
                            && x.NewObject.StageNameEn.Contains(criteria.Query ?? "")
                            && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                            && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterStage = Builders<CatStage>.Filter.Where(stageEx);
            var queryCurrencyResult = mongoContext.CatStages.Find(filterStage);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject.StageNameEn,
                Code = null
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCommonityGroup(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCommodityGroup, bool>> groupCommodityEx = x => x.NewObject.GroupNameEn.Contains(criteria.Query ?? "")
                            && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                            && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterCommodityGroup = Builders<CatCommodityGroup>.Filter.Where(groupCommodityEx);
            var queryCurrencyResult = mongoContext.CatCommodityGroups.Find(filterCommodityGroup);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject.GroupNameEn,
                Code = null
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatPlace(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            string type = string.Empty;
            switch (criteria.TableType)
            {
                case CategoryTable.Warehouse:
                    type = CatPlaceConstant.Warehouse;
                    break;
                case CategoryTable.PortIndex:
                    type = CatPlaceConstant.Port;
                    break;
                case CategoryTable.Province:
                    type = CatPlaceConstant.Province;
                    break;
                case CategoryTable.District:
                    type = CatPlaceConstant.District;
                    break;
                case CategoryTable.Ward:
                    type = CatPlaceConstant.Ward;
                    break;
            }
            Expression<Func<CatPlace, bool>> placeEx = x => x.NewObject.Code.Contains(criteria.Query ?? "") 
                && x.NewObject.NameEn.Contains(criteria.Query ?? "") 
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null)
                && (x.NewObject.PlaceTypeId.Contains(type ?? ""));
            var filterPlace = Builders<CatPlace>.Filter.Where(placeEx);
            var queryCurrencyResult = mongoContext.CatPlaces.Find(filterPlace);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject?.NameEn,
                Code = x.NewObject?.Code
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatPartner(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            Expression<Func<CatPartner, bool>> partnerEx = x => x.NewObject.Id.Contains(criteria.Query ?? "") 
                && x.NewObject.PartnerNameEn.Contains(criteria.Query ?? "")
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterPartner = Builders<CatPartner>.Filter.Where(partnerEx);
            var queryCurrencyResult = mongoContext.CatPartners.Find(filterPartner);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject?.PartnerNameEn,
                Code = x.NewObject?.Id
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCurrencyExchange(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            var filterCurrencyExchange = Builders<CatCurrencyExchange>.Filter.Where(_ => true);
            var queryCurrencyResult = mongoContext.CatCurrencyExchanges.Find(filterCurrencyExchange);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = ""
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCountry(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCountry, bool>> countryEx = x => x.NewObject.NameEn.Contains(criteria.Query ?? "") 
                && x.NewObject.Code.Contains(criteria.Query ?? "")
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterCountry = Builders<CatCountry>.Filter.Where(countryEx);
            var queryCurrencyResult = mongoContext.CatCountries.Find(filterCountry);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject.Code??x.NewObject.NameEn,
                Code = x.NewObject?.Id.ToString()
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCommodity(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCommodity, bool>> commodityEx = x => x.NewObject.CommodityNameEn.Contains(criteria.Query ?? "")
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterCommodity = Builders<CatCommodity>.Filter.Where(commodityEx);
            var queryCurrencyResult = mongoContext.CatCommodities.Find(filterCommodity);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject?.CommodityNameEn,
                Code = x.NewObject?.Id.ToString()
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatChargeDefaultAccount(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            Expression<Func<CatChargeDefaultAccount, bool>> changeDefaultEx = x => x.NewObject.DebitAccountNo.Contains(criteria.Query ?? "")
                      && x.NewObject.CreditAccountNo.Contains(criteria.Query ?? "")
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterChargeDefaultAccount = Builders<CatChargeDefaultAccount>.Filter.Where(changeDefaultEx);
            var queryCurrencyResult = mongoContext.CatChargeDefaultAccounts.Find(filterChargeDefaultAccount);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject.CreditAccountNo ?? x.NewObject.DebitAccountNo
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatUnit(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            Expression<Func<CatUnit, bool>> unitEx = x => x.NewObject.UnitNameEn.Contains(criteria.Query ?? "")
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterUnit = Builders<CatUnit>.Filter.Where(unitEx);
            var queryUnitResult = mongoContext.CatUnits.Find(filterUnit);
            rowsCount = queryUnitResult.CountDocuments();
            var data = queryUnitResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Code = null,
                Name = x.NewObject?.UnitNameEn
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCurrency(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCurrency, bool>> currencyEx = x => x.NewObject.CurrencyName.Contains(criteria.Query ?? "")
                && x.NewObject.Id.Contains(criteria.Query ?? "")
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterCurrency = Builders<CatCurrency>.Filter.Where(currencyEx);
            var queryCurrencyResult = mongoContext.CatCurrencies.Find(filterCurrency).SortByDescending(x => x.PropertyCommon.DatetimeModified);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList();
            return data.Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Code = x.NewObject?.Id,
                Name = x.NewObject?.CurrencyName
            });
        }

        private IEnumerable<LogModel> PagingCatCharge(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCharge, bool>> chargeEx = x => x.NewObject.ChargeNameEn.Contains(criteria.Query ?? "")
                && x.NewObject.Code.Contains(criteria.Query ?? "")
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterCharge = Builders<CatCharge>.Filter.Where(chargeEx);
            var queryResult = mongoContext.CatCatCharges.Find(filterCharge);
            rowsCount = queryResult.CountDocuments();
            var data = queryResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Code = x.NewObject?.Code,
                Name = x.NewObject?.ChargeNameEn
            });
            return data;
        }

    }
}
