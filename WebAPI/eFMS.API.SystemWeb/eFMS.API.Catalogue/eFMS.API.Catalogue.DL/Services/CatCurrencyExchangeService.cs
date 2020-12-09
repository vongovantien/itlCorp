using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using eFMS.API.Catalogue.Service.ViewModels;
using ITL.NetCore.Connection;
using System.Linq;
using ITL.NetCore.Common;
using eFMS.API.Catalogue.Service.Contexts;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCurrencyExchangeService : RepositoryBase<CatCurrencyExchange, CatCurrencyExchangeModel>, ICatCurrencyExchangeService
    {
        private readonly IContextBase<SysUser> userRepository;
        public CatCurrencyExchangeService(IContextBase<CatCurrencyExchange> repository, IMapper mapper,
            IContextBase<SysUser> userRepo) : base(repository, mapper)
        {
            userRepository = userRepo;
        }

        public vw_catCurrencyExchangeNewest ConvertRate(DateTime date, string localCurrency, string fromCurrency)
        {
            DateTime dateString = date.Date;
            var data = DataContext.Get(x => x.CurrencyFromId == fromCurrency
                                         && x.CurrencyToId == localCurrency
                                         && x.DatetimeCreated.Value.Month == date.Month
                                         && x.DatetimeCreated.Value.Year == date.Year
                                         && x.DatetimeCreated.Value.Day == date.Day
                                         && x.DatetimeModified.Value.Month == date.Month
                                         && x.DatetimeModified.Value.Year == date.Year
                                         && x.DatetimeModified.Value.Day == date.Day
                                         ).OrderBy(x => x.DatetimeCreated).ThenBy(x =>x.DatetimeModified).LastOrDefault(); 
            if (data == null)
            {
                var newestExchanges = ((eFMSDataContext)DataContext.DC).GetViewData<vw_catCurrencyExchangeNewest>();
                var newList = new List<CatCurrencyExchange>();
                foreach (var item in newestExchanges)
                {
                    if (item.DatetimeCreated.Value.Date < DateTime.Now.Date)
                    {
                        var exchange = new CatCurrencyExchange
                        {
                            CurrencyFromId = item.CurrencyFromID,
                            DatetimeCreated = new DateTime(date.Year, date.Month, date.Day),
                            DatetimeModified = new DateTime(date.Year, date.Month, date.Day),
                            UserCreated = "system",
                            UserModified = "system",
                            Rate = item.Rate,
                            Active = true,
                            CurrencyToId = item.CurrencyToID
                        };
                        newList.Add(exchange);
                    }
                }
                if(newList.Count > 0)
                {
                    var hs = DataContext.Add(newList);
                    if (hs.Success == false) return null;
                    data = newList.Where(x => x.CurrencyFromId == fromCurrency
                                             && x.CurrencyToId == localCurrency
                                             ).OrderBy(x => x.DatetimeCreated).ThenBy(x => x.DatetimeModified).LastOrDefault();
                }
            }
            return new vw_catCurrencyExchangeNewest
            {
                CurrencyFromID = data != null ? data.CurrencyFromId : null,
                Rate = data != null ? data.Rate : 0,
                DatetimeCreated = data != null ? data?.DatetimeModified : null
            };
        }

        public object GetCurrency()
        {
            var fromCurrencies = DataContext.Get().GroupBy(x => x.CurrencyFromId).OrderBy(x => x.Key).Select(x => x.Key).ToList();
            var toCurrencies = DataContext.Get().GroupBy(x => x.CurrencyToId).OrderBy(x => x.Key).Select(x => x.Key).ToList();
            return new { fromCurrencies, toCurrencies };
        }

        public CurrencyExchangeNewestViewModel GetCurrencyExchangeNewest(string currencyToId)
        {
            var lastExchanges = GetExchangeRateNewest(currencyToId);
            if (lastExchanges == null) return null;
            var result = new CurrencyExchangeNewestViewModel
            {
                DatetimeModified = lastExchanges.FirstOrDefault().DatetimeCreated,
                ExchangeRates = lastExchanges
            };
            return result;
        }

        private List<vw_catCurrencyExchangeNewest> GetExchangeRateNewest(string currencyToId)
        {
            List<vw_catCurrencyExchangeNewest> exchangeRates = ((eFMSDataContext)DataContext.DC).GetViewData<vw_catCurrencyExchangeNewest>();
            if (exchangeRates.Count == 0) return null;

            if (!string.IsNullOrEmpty(currencyToId) && exchangeRates.Count > 0) exchangeRates = exchangeRates.Where(x => x.CurrencyToID == currencyToId && (x.Active == true || false)).ToList();

            if (exchangeRates.Count == 0) return null;
            return exchangeRates;
        }

        public CurrencyExchangeNewestViewModel GetExchangeRates(DateTime date, string localCurrency, string fromCurrency)
        {
            var users = userRepository.Get();
            var data = DataContext.Get(x => x.DatetimeCreated.Value.Day == date.Day
                && x.DatetimeCreated.Value.Month == date.Month
                && x.DatetimeCreated.Value.Year == date.Year
                && x.CurrencyToId == localCurrency 
                && (x.CurrencyFromId == fromCurrency || string.IsNullOrEmpty(fromCurrency)));

            var result = new CurrencyExchangeNewestViewModel();
            if (data.Count() == 0) return result;
            var lastRate = data.OrderBy(x => x.DatetimeModified).ThenBy(x => x.DatetimeCreated).LastOrDefault();
            result.LocalCurrency = localCurrency;
            result.DatetimeCreated = date;
            result.DatetimeModified = date;
            string userName = string.Empty;
            if(lastRate.UserModified != null)
            {
                var userModified = users.FirstOrDefault(x => x.Id == lastRate.UserModified);
                userName = userModified != null ? userModified.Username : "system";
            }
            result.UserModifield = userName;

            result.ExchangeRates = new List<vw_catCurrencyExchangeNewest>();
            foreach (var item in data)
            {
                var rate = new vw_catCurrencyExchangeNewest
                {
                    CurrencyFromID = item.CurrencyFromId,
                    Rate = item.Rate,
                    DatetimeCreated = item.DatetimeModified
                };
                result.ExchangeRates.Add(rate);
            }
            result.ExchangeRates = result.ExchangeRates.OrderByDescending(x => x.DatetimeCreated).GroupBy(x => new { x.CurrencyFromID, x.Rate }).Select(x => new vw_catCurrencyExchangeNewest { CurrencyFromID = x.Key.CurrencyFromID, Rate = x.Key.Rate }).ToList();
            return result;
        }

        public List<CatCurrencyExchangeHistory> Paging(CatCurrencyExchangeCriteria criteria, int page, int size, out int rowsCount)
        {
            var users = userRepository.Get();
            var exchanges = DataContext.Get(x => (x.CurrencyToId ?? "").IndexOf(criteria.LocalCurrencyId ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                && (x.DatetimeCreated >= criteria.FromDate || criteria.FromDate == null)
                                && (x.DatetimeCreated <= criteria.ToDate || criteria.ToDate == null)
                                && (x.Active == criteria.Active || criteria.Active == null));
            var data = (from ex in exchanges
                        join u in users on ex.UserCreated equals u.Id into grpUsers
                        from user in grpUsers.DefaultIfEmpty()
                        select new { ex, user }).OrderByDescending(x => x.ex.DatetimeCreated);
            var dateCreateds = data.GroupBy(x => x.ex.DatetimeCreated.Value.Date)
                .Select(x => x);
            rowsCount = dateCreateds.Count();
            if (rowsCount == 0) return null;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                dateCreateds = dateCreateds.Skip((page - 1) * size).Take(size);
            }
            List<CatCurrencyExchangeHistory> results = new List<CatCurrencyExchangeHistory>();
            foreach (var item in dateCreateds)
            {
                var date = data.Where(x => x.ex.DatetimeCreated.Value.Date == item.Key)
                                .OrderBy(x => x.ex.DatetimeCreated == item.Key).First();
                var userName = "system";
                if(date.ex.UserModified != null)
                {
                    if(date.ex.UserModified != "system")
                    {
                        userName = date.user?.Username;
                    }
                }
                var rate = new CatCurrencyExchangeHistory
                {
                    DatetimeCreated = item.Key,
                    UserModifield = userName,
                    LocalCurrency = date.ex.CurrencyToId,
                    DatetimeUpdated = date.ex.DatetimeModified ?? date.ex.DatetimeCreated
                };
                results.Add(rate);
            }
            return results;
        }

        public List<vw_catCurrencyExchange> Query(CatCurrencyExchangeCriteria criteria)
        {
            var list = GetView();
            list = list.Where(x => (x.CurrencyToId ?? "").IndexOf(criteria.LocalCurrencyId ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                && (x.DatetimeCreated >= criteria.FromDate || criteria.FromDate == null)
                                && (x.DatetimeCreated <= criteria.ToDate || criteria.ToDate ==  null)
                                && (x.Active == criteria.Active || criteria.Active == null)
                ).ToList();
            return list;
        }

        public HandleState RemoveExchangeCurrency(string currencyFrom, string currentUser)
        {
            try
            {
                var rates = DataContext.Get(x => x.CurrencyFromId == currencyFrom && x.Active == true);
                foreach (var item in rates)
                {
                    item.UserModified = currentUser;
                    item.DatetimeModified = DateTime.Now;
                    item.Active = false;
                    item.InactiveOn = DateTime.Now;
                    DataContext.Update(item, x => x.Id == item.Id, false);
                }
                DataContext.SubmitChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState UpdateRate(CatCurrencyExchangeEditModel model)
        {
            var rates = model.CatCurrencyExchangeRates;
            try
            {
                foreach (var item in rates)
                {
                    var rate = new CatCurrencyExchange();
                    rate = DataContext.Get(x => x.DatetimeCreated.Value.Date == DateTime.Now.Date 
                                && x.CurrencyFromId == item.CurrencyFromId 
                                && x.CurrencyToId == model.CurrencyToId
                                && x.Active == true).FirstOrDefault();
                    if (rate != null)
                    {
                        if(item.IsUpdate == true)
                        {
                            rate.UserModified = model.UserModified;
                            rate.DatetimeModified = DateTime.Now;
                            rate.Active = false;
                            DataContext.Update(rate, x => x.Id == rate.Id, false);
                        }
                        else { continue; }


                        var newrate = new CatCurrencyExchange
                        {
                            CurrencyFromId = item.CurrencyFromId,
                            CurrencyToId = model.CurrencyToId,
                            Rate = item.Rate,
                            Active = true,
                            UserCreated = model.UserModified,
                            DatetimeCreated = DateTime.Now,
                            UserModified = model.UserModified,
                            DatetimeModified = DateTime.Now
                        };
                        DataContext.Add(newrate, false);
                    }
                    else
                    {
                        rate = new CatCurrencyExchange
                        {
                            CurrencyFromId = item.CurrencyFromId,
                            CurrencyToId = model.CurrencyToId,
                            Rate = item.Rate,
                            Active = true,
                            UserCreated = model.UserModified,
                            DatetimeCreated = DateTime.Now,
                            UserModified = model.UserModified,
                            DatetimeModified = DateTime.Now
                        };
                        DataContext.Add(rate, false);
                    }
                }
                DataContext.SubmitChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        private List<vw_catCurrencyExchange> GetView()
        {
            List<vw_catCurrencyExchange> lvCatPlace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_catCurrencyExchange>();
            return lvCatPlace;
        }
    }
}
