﻿using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface ICatPartnerBankService : IRepositoryBase<CatPartnerBank, CatPartnerBankModel>, IForPartnerApiService
    {
        Task<HandleState> UpdatePartnerBankInfoSyncStatus(BankStatusUpdateModel model);
        Task<ICurrentUser> SetCurrentUserPartner(ICurrentUser currentUser, string apiKey);
    }
}
