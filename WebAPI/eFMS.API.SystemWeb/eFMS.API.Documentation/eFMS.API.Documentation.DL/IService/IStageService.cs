using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IStageService: IRepositoryBase<CatStage, CatStageModel>
    {
        Task<CatStage> GetStageByType(string stageType);
    }
}
