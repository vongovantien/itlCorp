using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Catalogue;
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
        Task<string> CheckExistedPartnerBank(CatPartnerBankModel model);
        Task<HandleState> Delete(Guid Id);
        Task<HandleState> ReviseBankInformation(Guid bankId);
        Task<List<BankSyncModel>> GetListPartnerBankInfoToSync(List<Guid> partnerBankIds);
        Task<HandleState> UpdateByStatus(List<Guid> ids, string status);
        Task<List<CatPartnerBankImportModel>> CheckValidImport(List<CatPartnerBankImportModel> list);
        Task<HandleState> ImportPartnerBank(List<CatPartnerBankImportModel> data);
        IQueryable<CatPartnerBankModel> GetApprovedBanksByPartner(Guid partnerId);
    }
}
