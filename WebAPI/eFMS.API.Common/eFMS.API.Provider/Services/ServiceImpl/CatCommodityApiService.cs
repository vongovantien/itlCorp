using eFMS.API.Provider.Infrasture.API.Catalogue;
using eFMS.API.Provider.Models;
using eFMS.API.Provider.Services.IService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static eFMS.API.Provider.Infrasture.Settings;

namespace eFMS.API.Provider.Services.ServiceImpl
{
    public class CatCommodityApiService : BaseApiService, ICatCommodityApiService
    {
        public CatCommodityApiService(HttpClient httpClient, IOptions<APIUrls> settings, IMemoryCache memoryCache) : base(httpClient, settings, 1, nameof(APIUrls.CatelogueUrl), memoryCache)
        {
        }

        public async Task<List<CatCommodityApiModel>> GetCommodities()
        {
            string strUri = CatCommodityAPI.GetAll(baseUrl);
            return await GetApi<List<CatCommodityApiModel>>(strUri, true);
        }
    }
}
