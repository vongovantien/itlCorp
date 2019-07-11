using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using eFMS.API.Setting.Models;
using eFMS.API.Setting.Service;
using eFMS.API.Common;
using eFMS.API.Setting.Service.Contexts;
using eFMS.API.Setting.Service.Models;
using eFMS.API.Setting.Service.ViewModels;

namespace eFMS.API.Setting.DL.Services
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
                case CategoryTable.PortIndex:
                    data = PagingCatPlace(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.Province:
                    data = PagingCatPlace(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.District:
                    data = PagingCatPlace(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.Ward:
                    data = PagingCatPlace(criteria, page, size, out rowsCount);
                    break;
                case CategoryTable.ExchangeRate:
                    data = PagingCatCurrencyExchange(criteria, page, size, out rowsCount);
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
                new CategoryCollectionModel { Id = (int)CategoryTable.CatUnit, Name = "Unit" },
                //new CategoryCollectionModel { Id = (int)CategoryTable.ExchangeRate, Name = "Exchange Rate" }
            };
            return collections.OrderBy(x => x.Name).ToList();
        }

        private IEnumerable<LogModel> PagingCatStage(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            Expression<Func<CatStageLog, bool>> stageEx = x => x.NewObject.Code.Contains(criteria.Query ?? "")
                            && x.NewObject.StageNameEn.Contains(criteria.Query ?? "")
                            && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                            && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterStage = Builders<CatStageLog>.Filter.Where(stageEx);
            var queryCurrencyResult = mongoContext.CatStages.Find(filterStage);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page*size).Limit(size).ToList().Select(x => new LogModel
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
            Expression<Func<CatCommodityGroupLog, bool>> groupCommodityEx = x => x.NewObject.GroupNameEn.Contains(criteria.Query ?? "")
                            && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                            && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterCommodityGroup = Builders<CatCommodityGroupLog>.Filter.Where(groupCommodityEx);
            var queryCurrencyResult = mongoContext.CatCommodityGroups.Find(filterCommodityGroup);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page*size).Limit(size).ToList().Select(x => new LogModel
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
            Expression<Func<CatPlaceLog, bool>> placeEx = x => x.NewObject.Code.Contains(criteria.Query ?? "") 
                && x.NewObject.NameEn.Contains(criteria.Query ?? "") 
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null)
                && (x.NewObject.PlaceTypeId.Contains(type ?? ""));
            var filterPlace = Builders<CatPlaceLog>.Filter.Where(placeEx);
            var queryCurrencyResult = mongoContext.CatPlaces.Find(filterPlace);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page*size).Limit(size).ToList().Select(x => new LogModel
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
            string valueId, valueName;
            valueId = valueName = criteria.Query != null ? criteria.Query.ToLower() : "";
            Expression<Func<CatPartnerLog, bool>> partnerEx = x => (x.NewObject.Id.ToLower().Contains(valueId) 
                || x.NewObject.PartnerNameEn.ToLower().Contains(valueName))
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterPartner = Builders<CatPartnerLog>.Filter.Where(partnerEx);
            var queryCurrencyResult = mongoContext.CatPartners.Find(filterPartner);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page*size).Limit(size).ToList().Select(x => new LogModel
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
            var filterCurrencyExchange = Builders<CatCurrencyExchangeLog>.Filter.Where(_ => true);
            var queryCurrencyResult = mongoContext.CatCurrencyExchanges.Find(filterCurrencyExchange);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page*size).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject.CurrencyFromId
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCountry(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            string valueName, valueCode;
            valueName = valueCode = criteria.Query != null ? criteria.Query.ToLower() : "";
            Expression<Func<CatCountryLog, bool>> countryEx = x => (x.NewObject.NameEn.ToLower().Contains(valueName) 
                || x.NewObject.Code.ToLower().Contains(valueCode))
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterCountry = Builders<CatCountryLog>.Filter.Where(countryEx);
            var queryCurrencyResult = mongoContext.CatCountries.Find(filterCountry);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page*size).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject.Code??x.NewObject.NameEn,
                Code = int.TryParse(x.NewObject?.Id.ToString(), out int n) == true ? null : x.NewObject?.Id.ToString()
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCommodity(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            var value = criteria.Query != null ? criteria.Query.ToLower() : "";
            Expression<Func<CatCommodityLog, bool>> commodityEx = x => x.NewObject.CommodityNameEn.ToLower().Contains(value)
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterCommodity = Builders<CatCommodityLog>.Filter.Where(commodityEx);
            var queryCurrencyResult = mongoContext.CatCommodities.Find(filterCommodity);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page*size).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject?.CommodityNameEn,
                Code = int.TryParse(x.NewObject?.Id.ToString(), out int n) == true?null: x.NewObject?.Id.ToString()
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatChargeDefaultAccount(CategoryCriteria criteria, int page, int size, out long rowsCount)
        {
            string valueDebit, valueCredit;
            valueDebit = valueCredit = criteria.Query != null ? criteria.Query.ToLower() : "";
            Expression<Func<CatChargeDefaultAccountLog, bool>> changeDefaultEx = x => (x.NewObject.DebitAccountNo.ToLower().Contains(valueDebit)
                      || x.NewObject.CreditAccountNo.ToLower().Contains(valueCredit))
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterChargeDefaultAccount = Builders<CatChargeDefaultAccountLog>.Filter.Where(changeDefaultEx);
            var queryCurrencyResult = mongoContext.CatChargeDefaultAccounts.Find(filterChargeDefaultAccount);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page*size).Limit(size).ToList().Select(x => new LogModel
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
            string value = criteria.Query != null ? criteria.Query.ToLower() : "";
            Expression<Func<CatUnitLog, bool>> unitEx = x => x.NewObject.UnitNameEn.ToLower().Contains(value)
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterUnit = Builders<CatUnitLog>.Filter.Where(unitEx);
            var queryUnitResult = mongoContext.CatUnits.Find(filterUnit);
            rowsCount = queryUnitResult.CountDocuments();
            var data = queryUnitResult.Skip(page*size).Limit(size).ToList().Select(x => new LogModel
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
            string valueName, valueId;
            valueName = valueId = criteria.Query != null ? criteria.Query.ToLower() : "";
            Expression<Func<CatCurrencyLog, bool>> currencyEx = x => (x.NewObject.CurrencyName.ToLower().Contains(valueName)
                || x.NewObject.Id.ToLower().Contains(valueId))
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterCurrency = Builders<CatCurrencyLog>.Filter.Where(currencyEx);
            var queryCurrencyResult = mongoContext.CatCurrencies.Find(filterCurrency).SortByDescending(x => x.PropertyCommon.DatetimeModified);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page*size).Limit(size).ToList();
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
            string valueName, valueCode;
           valueName = valueCode = criteria.Query != null ? criteria.Query.ToLower() : "";
            Expression<Func<CatChargeLog, bool>> chargeEx = x => (x.NewObject.ChargeNameEn.ToLower().Contains(valueName)
                || x.NewObject.Code.ToLower().Contains(valueCode))
                && (x.PropertyCommon.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                && (x.PropertyCommon.DatetimeModified <= criteria.ToDate || criteria.ToDate == null);
            var filterCharge = Builders<CatChargeLog>.Filter.Where(chargeEx);
            var queryResult = mongoContext.CatCatCharges.Find(filterCharge);
            rowsCount = queryResult.CountDocuments();
            var data = queryResult.Skip(page*size).Limit(size).ToList().Select(x => new LogModel
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
