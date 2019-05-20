using eFMS.API.Provider.Infrasture;
using eFMS.API.Provider.Infrasture.API.Catalogue;
using eFMS.API.Provider.Models;
using eFMS.API.Provider.Services.IService;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static eFMS.API.Provider.Infrasture.Settings;

namespace eFMS.API.Provider.Services.ServiceImpl
{
    public class CatAreaApiService: BaseApiService, ICatAreaApiService
    {
        public CatAreaApiService(HttpClient httpClient, IOptions<APIUrls> settings) : base(httpClient, settings, 1, nameof(APIUrls.CatelogueUrl))
        {
        }

        public async Task<List<CatAreaApiModel>> GetAreas()
        {
            string strUri = CatAreaAPI.GetAll(baseUrl);
            return await GetApi<List<CatAreaApiModel>>(strUri);
        }
    }
}
