using eFMS.API.Provider.Infrasture;
using eFMS.API.Provider.Infrasture.API.Catalogue;
using eFMS.API.Provider.Models;
using eFMS.API.Provider.Models.Criteria;
using eFMS.API.Provider.Services.IService;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static eFMS.API.Provider.Infrasture.Settings;

namespace eFMS.API.Provider.Services.ServiceImpl
{
    public class CatStageApiService : BaseApiService, ICatStageApiService
    {
        public CatStageApiService(HttpClient httpClient, IOptions<Settings.APIUrls> settings) : base(httpClient, settings, 1, nameof(APIUrls.CatelogueUrl))
        {
        }

        public async Task<List<CatAreaApiModel>> GetStages(CatStageCriteria criteria)
        {
            string strUri = CatStageAPI.GetAll(baseUrl);
            var results = await PostApi<List<CatAreaApiModel>, CatStageCriteria>(strUri, criteria);
            return results;
        }
    }
}
