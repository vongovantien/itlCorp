using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatAddressPartnerService : IRepositoryBaseCache<CatAddressPartner, CatAddressPartnerModel>
    {
        IQueryable<CatAddressPartnerModel> GetAll();
        //IQueryable<CatAddressPartnerModel> Paging(CatBankCriteria criteria, int pageNumber, int pageSize, out int rowsCount);
        HandleState Update(CatAddressPartnerModel model);
        HandleState Delete(Guid id);
        //IQueryable<CatAddressPartnerModel> Query(CatBankCriteria criteria);
        CatAddressPartnerModel GetDetail(Guid id);
        //List<CatAddressPartnerModel> CheckValidImport(List<CatAddressPartnerModel> list);
        //HandleState Import(List<CatAddressPartnerModel> data);
        Task<IQueryable<CatAddressPartnerModel>> GetAddressByPartnerId(Guid id);
    }
}
