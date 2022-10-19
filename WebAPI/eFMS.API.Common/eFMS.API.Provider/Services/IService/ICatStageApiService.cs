﻿using eFMS.API.Provider.Models;
using eFMS.API.Provider.Models.Criteria;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.Provider.Services.IService
{
    public interface ICatStageApiService
    {
        Task<List<CatStageApiModel>> GetStages(CatStageCriteria criteria);
        Task<List<CatStageApiModel>> GetAll();
    }
}
