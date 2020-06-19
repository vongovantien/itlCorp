using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatContractService : IRepositoryBaseCache<CatContract, CatContractModel>
    {
        IQueryable<CatContract> GetContracts();
        IQueryable<CatContractViewModel> Query(CatContractCriteria criteria);

        List<CatContractViewModel> Paging(CatContractCriteria criteria, int page, int size, out int rowsCount);
        HandleState Delete(Guid id);
        HandleState Update(CatContractModel model);
        List<CatContractModel> GetBy(string partnerId);
        Guid? GetContractIdByPartnerId(string partnerId);
        Task<ResultHandle> UploadContractFile(ContractFileUploadModel model);



    }
}
