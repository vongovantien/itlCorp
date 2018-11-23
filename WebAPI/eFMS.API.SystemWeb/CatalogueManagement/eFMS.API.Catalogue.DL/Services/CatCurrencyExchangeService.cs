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
using System.Globalization;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCurrencyExchangeService : RepositoryBase<CatCurrencyExchange, CatCurrencyExchangeModel>, ICatCurrencyExchangeService
    {
        public CatCurrencyExchangeService(IContextBase<CatCurrencyExchange> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public CurrencyExchangeNewestViewModel GetCurrencyExchangeNewest()
        {
            var lastRate = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.OrderByDescending(x => x.DatetimeModified).ThenBy(x => x.DatetimeCreated).FirstOrDefault();
            if (lastRate == null) return null;
            List<vw_catCurrencyExchangeNewest> lvCatPlace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_catCurrencyExchangeNewest>();

            var result = new CurrencyExchangeNewestViewModel
            {
                DatetimeModified = lastRate.DatetimeModified ?? lastRate.DatetimeCreated,
                ExchangeRates = lvCatPlace
            };
            return result;
        }

        public CurrencyExchangeNewestViewModel GetExchangeRates(DateTime date, string localCurrency)
        {
            var users = ((eFMSDataContext)DataContext.DC).GetViewData<vw_sysUser>();
            var data = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => x.DatetimeCreated.Value.Date == date.Date
                && x.CurrencyToId == localCurrency);

            var result = new CurrencyExchangeNewestViewModel();
            if (data.Count() == 0) return result;
            var lastRate = data.OrderBy(x => x.DatetimeModified).ThenBy(x => x.DatetimeCreated).FirstOrDefault();
            result.LocalCurrency = localCurrency;
            result.DatetimeCreated = lastRate.DatetimeCreated;
            result.DatetimeModified = lastRate.DatetimeModified?? lastRate.DatetimeCreated;
            result.UserModifield = lastRate != null ? (lastRate.UserModified!= null ?(users.FirstOrDefault(x => x.ID == lastRate.UserModified).Username): null) ?? (users.FirstOrDefault(x => x.ID == lastRate.UserCreated).Username) : null;

            result.ExchangeRates = new List<vw_catCurrencyExchangeNewest>();
            foreach (var item in data)
            {
                var rate = new vw_catCurrencyExchangeNewest
                {
                    CurrencyFromId = item.CurrencyFromId,
                    Rate = item.Rate,
                    //DatetimeModifield = item.DatetimeModified ==null?item.DatetimeCreated : item.DatetimeModified,
                };
                result.ExchangeRates.Add(rate);
            }
            return result;
        }

        public List<CatCurrencyExchangeHistory> Paging(CatCurrencyExchangeCriteria criteria, int page, int size, out int rowsCount)
        {
            var users = ((eFMSDataContext)DataContext.DC).GetViewData<vw_sysUser>();
            var data = Get(x => (x.CurrencyToId ?? "").IndexOf(criteria.LocalCurrencyId ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                && (x.DatetimeCreated >= criteria.FromDate || criteria.FromDate == null)
                                && (x.DatetimeCreated <= criteria.ToDate || criteria.ToDate == null))
                                .Join(users, x => x.UserCreated, y => y.ID, (x, y) => new { x, y });
            var dateCreateds = data.GroupBy(x => x.x.DatetimeCreated.Value.Date)
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
                var date = data.Where(x => x.x.DatetimeCreated.Value.Date == item.Key).OrderBy(x => x.x.DatetimeCreated == item.Key).First();
                var rate = new CatCurrencyExchangeHistory
                {
                    DatetimeCreated = item.Key,
                    UserModifield = date.x.UserModified==null? date.y.Username: (users.FirstOrDefault(x => x.ID == date.x.UserModified)?.Username),
                    LocalCurrency = date.x.CurrencyToId,
                    DatetimeUpdated = date.x.DatetimeModified ?? date.x.DatetimeCreated
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
                ).ToList();
            return list;
        }

        public HandleState UpdateRate(CatCurrencyExchangeEditModel model)
        {
            var rates = model.CatCurrencyExchangeRates;
            try
            {
                foreach (var item in rates)
                {
                    var rate = new CatCurrencyExchange();
                    rate = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.FirstOrDefault(x => x.DatetimeCreated.Value.Date == DateTime.Now.Date && x.CurrencyFromId == item.CurrencyFromId && x.CurrencyToId == model.CurrencyToId);
                    if (rate != null)
                    {
                        rate.Rate = item.Rate;
                        rate.UserModified = model.UserModified;
                        rate.DatetimeModified = DateTime.Now;
                        ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Update(rate);
                    }
                    else
                    {
                        rate = new CatCurrencyExchange
                        {
                            CurrencyFromId = item.CurrencyFromId,
                            CurrencyToId = model.CurrencyToId,
                            Rate = item.Rate,
                            UserModified = model.UserModified,
                            DatetimeCreated = DateTime.Now
                        };
                        ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Add(rate);
                    }
                }
                DataContext.DC.SaveChanges();
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
