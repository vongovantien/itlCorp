using eFMS.API.Provider.Infrasture;
using eFMS.API.Provider.Infrasture.API.System;
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
    public class CatDepartmentApiService : BaseApiService, ICatDepartmentApiService
    {
        public CatDepartmentApiService(HttpClient httpClient, IOptions<APIUrls> settings, IMemoryCache memoryCache) : base(httpClient, settings, 1, nameof(APIUrls.SystemUrl), memoryCache)
        {
        }

        public async Task<List<CatDepartmentApiModel>> GetAll()
        {
            string strUri = CatDepartmentAPI.GetAll(baseUrl);
            return await GetApi<List<CatDepartmentApiModel>>(strUri, true);
        }
    }
}
