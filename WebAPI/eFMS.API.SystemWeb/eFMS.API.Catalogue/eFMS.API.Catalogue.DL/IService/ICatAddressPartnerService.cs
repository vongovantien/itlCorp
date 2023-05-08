using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
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
        HandleState Update(CatAddressPartnerModel model);
        HandleState Delete(Guid id);
        CatAddressPartnerModel GetDetail(Guid id);
        IQueryable<CatAddressPartnerModel> GetAddressByPartnerId(Guid id);
    }
}
