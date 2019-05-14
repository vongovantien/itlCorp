using eFMS.API.Log.DL.IService;
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
using System.Threading.Tasks;
using System.Linq;
using eFMS.API.Log.DL.Models;
using eFMS.API.Log.DL.Common;

namespace eFMS.API.Log.DL.Services
{
    public class CatCurrencyService: ICatCurrencyService
    {
        private readonly ChangeLogContext mongoContext = null;
        private readonly IContextBase<SysUser> sysUserContext;

        public CatCurrencyService(IOptions<Settings> settings, IContextBase<SysUser> userContext)
        {
            mongoContext = new ChangeLogContext(settings);
            sysUserContext = userContext;
        }
        public IEnumerable<CatCurrency> Get()
        {
            var filter = Builders<CatCurrency>.Filter.Where(_ => true);
            return mongoContext.CatCurrencies.Find(filter).ToList();
        }

        public CatCurrency Get(Guid id)
        {
            //var filter = Builders<CatCurrency>.Filter.Where(x => x.Id == id);
            //return await mongoContext.CatCurrencies.Find(filter).FirstOrDefaultAsync<CatCurrency>();
            var filter = Builders<CatCurrency>.Filter.Where(x => x.Id == id);
            return mongoContext.CatCurrencies.Find(filter).FirstOrDefault<CatCurrency>();
        }

        public List<LogModel> Paging(string query, int page, int size, out long rowsCount)
        {
            Expression<Func<CatCurrency, bool>> p = x => x.NewObject.CurrencyName.Contains(query ?? "")
                  && x.NewObject.Id.Contains(query ?? "");
            var filter = Builders<CatCurrency>.Filter.Where(p);
            rowsCount = mongoContext.CatCurrencies.Find(filter).CountDocuments();
            //var result = mongoContext.CatCurrencies.Find(filter).ToList();
            //var filter = Builders<CatCurrency>.Filter.Where(_ => true);
            var data = mongoContext.CatCurrencies.Find(filter).Skip(page).Limit(size).ToList();
            var result = (from s in data
             join user in ((eFMSDataContext)sysUserContext.DC).SysUser on s.PropertyCommon.UserModified equals user.Id
             select new LogModel {
                 Id = s.Id,
                 UserUpdated = user.Username,
                 Action = ConvertAction.ConvertLinqAction(s.PropertyCommon.ActionType),
                 DatetimeUpdated = s.PropertyCommon.DatetimeModified,
                 PropertyChange = s.PropertyCommon.PropertyChange
             }).ToList();
            return result;
        }

        public bool Remove(Guid id)
        {
            var result = false;
            var filter = Builders<CatCurrency>.Filter.Where(x => x.Id == id);
            var resultDelete = mongoContext.CatCurrencies.FindOneAndDelete(filter);
            if(resultDelete.Id == id)
            {
                result = true;
            }
            return result;
        }
    }
}
