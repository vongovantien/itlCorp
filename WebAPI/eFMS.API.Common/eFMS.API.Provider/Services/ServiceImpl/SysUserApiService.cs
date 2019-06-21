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
    public class SysUserApiService : BaseApiService, ISysUserApiService
    {
        public SysUserApiService(HttpClient httpClient, IOptions<Settings.APIUrls> settings, IMemoryCache memoryCache) : base(httpClient, settings, 1, nameof(APIUrls.SystemUrl), memoryCache)
        {
        }

        public async Task<List<SysUserApiModel>> GetUsers()
        {
            string strUri = SysUserAPI.GetAll(baseUrl);
            var results = await GetApi<List<SysUserApiModel>>(strUri, true);
            return results;
        }
    }
}
