using eFMS.API.Log.DL.Common;
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
        public List<LogModel> Paging(CategoryTable tableType, string query, int page, int size, out long rowsCount)
        {
            IEnumerable<LogModel> data = null;
            rowsCount = 0;
            switch (tableType)
            {
                case CategoryTable.CatCharge:
                    data = PagingCatCharge(query, page, size, out rowsCount);
                    break;
                case CategoryTable.CatChargeDefaultAccount:
                    data = PagingCatChargeDefaultAccount(query, page, size, out rowsCount);
                    break;
                case CategoryTable.CatCommonityGroup:
                    data = PagingCatCommonityGroup(query, page, size, out rowsCount);
                    break;
                case CategoryTable.CatCommodity:
                    data = PagingCatCommodity(query, page, size, out rowsCount);
                    break;
                case CategoryTable.CatCountry:
                    data = PagingCatCountry(query, page, size, out rowsCount);
                    break;
                case CategoryTable.CatCurrency:
                    data = PagingCatCurrency(query, page, size, out rowsCount);
                    break;
                //case CategoryTable.CatCurrencyExchange:
                //    data = PagingCatCurrencyExchange(query, page, size, out rowsCount);
                //    break;
                case CategoryTable.CatPartner:
                    data = PagingCatPartner(query, page, size, out rowsCount);
                    break;
                case CategoryTable.CatPlace:
                    data = PagingCatPlace(query, page, size, out rowsCount);
                    break;
                case CategoryTable.CatStage:
                    break;
                case CategoryTable.CatUnit:
                    data = PagingCatUnit(query, page, size, out rowsCount);
                    break;
            }
            var result = (from s in data
                          join user in ((eFMSDataContext)sysUserContext.DC).SysUser on s.UserUpdated equals user.Id
                          select new LogModel
                          {
                              Id = s.Id,
                              UserUpdated = user.Username,
                              Action = s.Action,
                              DatetimeUpdated = s.DatetimeUpdated,
                              PropertyChange = s.PropertyChange
                          }).ToList();
            return result;
        }

        private IEnumerable<LogModel> PagingCatCommonityGroup(string query, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCommodityGroup, bool>> groupCommodityEx = x => x.NewObject.GroupNameEn.Contains(query ?? "");
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
                Name = x.NewObject.GroupNameEn
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatPlace(string query, int page, int size, out long rowsCount)
        {
            Expression<Func<CatPlace, bool>> placeEx = x => x.NewObject.Code.Contains(query ?? "") && x.NewObject.NameEn.Contains(query ?? "");
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
                Name = x.NewObject.Code ?? x.NewObject.NameEn
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatPartner(string query, int page, int size, out long rowsCount)
        {
            Expression<Func<CatPartner, bool>> partnerEx = x => x.NewObject.Id.Contains(query ?? "") && x.NewObject.PartnerNameEn.Contains(query ?? "");
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
                Name = x.NewObject.PartnerNameEn ?? x.NewObject.Id
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCurrencyExchange(string query, int page, int size, out long rowsCount)
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

        private IEnumerable<LogModel> PagingCatCountry(string query, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCountry, bool>> countryEx = x => x.NewObject.NameEn.Contains(query ?? "") && x.NewObject.Code.Contains(query ?? "");
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
                Name = x.NewObject.Code??x.NewObject.NameEn
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCommodity(string query, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCommodity, bool>> commodityEx = x => x.NewObject.CommodityNameEn.Contains(query ?? "");
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
                Name = x.NewObject?.CommodityNameEn
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatChargeDefaultAccount(string query, int page, int size, out long rowsCount)
        {
            Expression<Func<CatChargeDefaultAccount, bool>> changeDefaultEx = x => x.NewObject.DebitAccountNo.Contains(query ?? "")
                      && x.NewObject.CreditAccountNo.Contains(query ?? "");
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

        private IEnumerable<LogModel> PagingCatUnit(string query, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCurrency, bool>> currencyEx = x => x.NewObject.CurrencyName.Contains(query ?? "")
                   && x.NewObject.Id.Contains(query ?? "");
            var filterCurrency = Builders<CatCurrency>.Filter.Where(currencyEx);
            var queryCurrencyResult = mongoContext.CatCurrencies.Find(filterCurrency);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject.Id ?? x.NewObject.CurrencyName
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCurrency(string query, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCurrency, bool>> currencyEx = x => x.NewObject.CurrencyName.Contains(query ?? "")
                && x.NewObject.Id.Contains(query ?? "");
            var filterCurrency = Builders<CatCurrency>.Filter.Where(currencyEx);
            var queryCurrencyResult = mongoContext.CatCurrencies.Find(filterCurrency);
            rowsCount = queryCurrencyResult.CountDocuments();
            var data = queryCurrencyResult.Skip(page).Limit(size).ToList().Select(x => new LogModel
            {
                Id = x.Id,
                UserUpdated = x.PropertyCommon.UserModified,
                Action = ConvertAction.ConvertLinqAction(x.PropertyCommon.ActionType),
                DatetimeUpdated = x.PropertyCommon.DatetimeModified,
                PropertyChange = x.PropertyCommon.PropertyChange,
                ObjectId = x.NewObject.Id.ToString(),
                Name = x.NewObject.Id ?? x.NewObject.CurrencyName
            });
            return data;
        }

        private IEnumerable<LogModel> PagingCatCharge(string query, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCharge, bool>> chargeEx = x => x.NewObject.ChargeNameEn.Contains(query ?? "")
                && x.NewObject.Code.Contains(query ?? "");
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
                Code = x.NewObject.Code,
                Name = x.NewObject.ChargeNameEn
            });
            return data;
        }
    }
}
