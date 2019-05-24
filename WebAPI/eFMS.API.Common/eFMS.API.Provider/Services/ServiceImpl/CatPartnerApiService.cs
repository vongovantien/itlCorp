using eFMS.API.Provider.Infrasture;
using eFMS.API.Provider.Infrasture.API.Catalogue;
using eFMS.API.Provider.Models;
using eFMS.API.Provider.Services.IService;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Provider.Services.ServiceImpl
{
    public class CatPartnerApiService : BaseApiService, ICatPartnerApiService
    {
        public CatPartnerApiService(HttpClient httpClient, IOptions<Settings.APIUrls> settings, int versionApi, string nameUrlBase) : base(httpClient, settings, versionApi, nameUrlBase)
        {
        }

        public async Task<List<CatPartnerApiModel>> GetPartners()
        {
            string strUri = CatPartnerAPI.GetAll(baseUrl);
            return await GetApi<List<CatPartnerApiModel>>(strUri);
        }
    }
}
