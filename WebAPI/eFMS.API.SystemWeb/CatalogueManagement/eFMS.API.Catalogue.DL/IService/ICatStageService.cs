using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatStageService : IRepositoryBase<CatStage, CatStageModel>
    {
        IQueryable<CatStageModel> GetAll();
        IQueryable<CatStageModel> Paging(CatStageCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<CatStageModel> Query(CatStageCriteria criteria);
        HandleState AddStage(CatStageModel catStage);
        HandleState UpdateStage(CatStageModel catStage);
        HandleState DeleteStage(int id);
        List<CatStageImportModel> CheckValidImport(List<CatStageImportModel> list);
        HandleState Import(List<CatStageImportModel> data);
    }


}
