using eFMS.API.Provider.Models;
using eFMS.API.Provider.Models.Criteria;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Provider.Services.IService
{
    public interface ICatStageApiService
    {
        Task<List<CatAreaApiModel>> GetStages(CatStageCriteria criteria);
    }
}
