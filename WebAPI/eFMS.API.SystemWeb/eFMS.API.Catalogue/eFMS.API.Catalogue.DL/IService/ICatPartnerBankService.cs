using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatPartnerBankService : IRepositoryBase<CatPartnerBank, CatPartnerBankModel>
    {
        Task<HandleState> AddNew(CatPartnerBankModel model);
        Task<HandleState> Update(CatPartnerBankModel model);
        Task<CatPartnerBankModel> GetDetail(Guid Id);
        IQueryable<CatPartnerBankModel> GetByPartner(Guid partnerId);
        Task<HandleState> Delete(Guid Id);
    }
}
