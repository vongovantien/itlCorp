using eFMS.API.Log.DL.IService;
using eFMS.API.Log.Service;
using eFMS.API.Log.Service.Contexts;
using eFMS.API.Log.Service.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Log.DL.Services
{
    public class CatCurrencyService: ICatCurrencyService
    {
        private readonly ChangeLogContext mongoContext = null;

        public CatCurrencyService(IOptions<Settings> settings)
        {
            mongoContext = new ChangeLogContext(settings);
        }

        public Task Add(CatCurrency item)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CatCurrency> Get()
        {
            var filter = Builders<CatCurrency>.Filter.Where(_ => true);
            return mongoContext.CatCurrencies.Find(filter).ToList();
        }

        public async Task<IEnumerable<CatCurrency>> GetAll()
        {
            var filter = Builders<CatCurrency>.Filter.Where(_ => true);
            return await mongoContext.CatCurrencies.Find(filter).ToListAsync();
        }

        public async Task<CatCurrency> GetAsync(Guid id)
        {
            var filter = Builders<CatCurrency>.Filter.Where(x => x.Id == id);
            return await mongoContext.CatCurrencies.Find(filter).FirstOrDefaultAsync<CatCurrency>();
        }

        public async Task<bool> Remove(Guid id)
        {
            var result = false;
            var filter = Builders<CatCurrency>.Filter.Where(x => x.Id == id);
            var resultDelete = await mongoContext.CatCurrencies.FindOneAndDeleteAsync(filter);
            if(resultDelete.Id == id)
            {
                result = true;
            }
            return result;
        }

        public Task<bool> Update(CatCurrency item)
        {
            throw new NotImplementedException();
        }
    }
}
