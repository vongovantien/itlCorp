﻿using eFMS.API.Provider.Infrasture;
using eFMS.API.Provider.Infrasture.API.Catalogue;
using eFMS.API.Provider.Models;
using eFMS.API.Provider.Services.IService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static eFMS.API.Provider.Infrasture.Settings;

namespace eFMS.API.Provider.Services.ServiceImpl
{
    public class CatPartnerApiService : BaseApiService, ICatPartnerApiService
    {
        public CatPartnerApiService(HttpClient httpClient, IOptions<Settings.APIUrls> settings, IMemoryCache memoryCache) : base(httpClient, settings, 1, nameof(APIUrls.CatelogueUrl), memoryCache)
        {
        }

        public async Task<List<CatPartnerApiModel>> GetPartners()
        {
            string strUri = CatPartnerAPI.GetAll(baseUrl);
            return await GetApi<List<CatPartnerApiModel>>(strUri, true);
        }
    }
}
