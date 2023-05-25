using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Catalogue;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPartnerBankService : RepositoryBase<CatPartnerBank, CatPartnerBankModel>, ICatPartnerBankService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IContextBase<CatBank> catBankRepository;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<SysImage> sysImageRepository;
        private readonly IOptions<ApiServiceUrl> _serviceUrl;
        public CatPartnerBankService(IContextBase<CatPartnerBank> repository,
            IMapper mapper,
            IContextBase<SysUser> sysUserRepo,
            IContextBase<CatBank> catBankRepo,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser currUser,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<SysImage> sysImageRepo, IOptions<ApiServiceUrl> serviceUrl) : base(repository, mapper)
        {
            currentUser = currUser;
            sysUserRepository = sysUserRepo;
            stringLocalizer = localizer;
            catBankRepository = catBankRepo;
            catPartnerRepository = catPartnerRepo;
            sysImageRepository = sysImageRepo;
            _serviceUrl = serviceUrl;
        }

        #region CRUD
        public async Task<HandleState> AddNew(CatPartnerBankModel entity)
        {
            var result = new HandleState();
            var newItem = mapper.Map<CatPartnerBank>(entity);
            newItem.DatetimeCreated = newItem.DatetimeModified = DateTime.Now;
            newItem.UserCreated = newItem.UserModified = currentUser.UserID;
            newItem.Active = true;
            newItem.ApproveStatus = "New";
            try
            {
                result = await DataContext.AddAsync(newItem);

            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }

        public async Task<string> CheckExistedPartnerBank(CatPartnerBankModel model)
        {
            var partner = await catPartnerRepository.Where(x => x.Id == model.PartnerId.ToString()).FirstOrDefaultAsync();
            var existingBankAccounts = await DataContext.WhereAsync(x => x.PartnerId.ToString() == partner.Id);

            if (model.Id != Guid.Empty)
            {
                var currentBankAccount = existingBankAccounts.FirstOrDefault(x => x.Id == model.Id);

                if (currentBankAccount != null && model.BankAccountNo == currentBankAccount.BankAccountNo)
                {
                    return null;
                }
            }

            if (existingBankAccounts.Any(account => account.BankAccountNo == model.BankAccountNo))
            {
                return "Bank account is existed";
            }

            return null;
        }

        public async Task<CatPartnerBankModel> GetDetail(Guid Id)
        {
            var dataReturn = from partner in DataContext.Where(x => x.Id == Id)
                             join catBank in catBankRepository.Get() on partner.BankId equals catBank.Id.ToString()
                             select new CatPartnerBankModel
                             {
                                 Id = partner.Id,
                                 PartnerId = partner.PartnerId,
                                 BankAccountName = partner.BankAccountName,
                                 BankAccountNo = partner.BankAccountNo,
                                 BankAddress = partner.BankAddress,
                                 SwiftCode = partner.SwiftCode,
                                 Note = partner.Note,
                                 BankId = partner.BankId,
                                 BankName = catBank.BankNameEn,
                                 BankCode = catBank.Code,
                                 Source = catBank.Source,
                                 ApproveStatus = partner.ApproveStatus,
                                 BeneficiaryAddress = partner.BeneficiaryAddress,
                                 UserCreated = partner.UserCreated,
                                 UserModified = partner.UserModified,
                                 DatetimeCreated = partner.DatetimeCreated,
                                 DatetimeModified = partner.DatetimeModified,
                                 UserCreatedName = sysUserRepository.First(x => x.Id == partner.UserCreated).Username,
                                 UserModifiedName = sysUserRepository.First(x => x.Id == partner.UserModified).Username,
                             };

            return await dataReturn.FirstOrDefaultAsync();
        }

        public IQueryable<CatPartnerBankModel> GetByPartner(Guid partnerId)
        {
            var dataReturn = from partner in DataContext.Where(x => x.PartnerId == partnerId.ToString())
                             join catBank in catBankRepository.Get() on partner.BankId equals catBank.Id.ToString()
                             orderby partner.DatetimeCreated descending
                             select new CatPartnerBankModel
                             {
                                 Id = partner.Id,
                                 PartnerId = partner.PartnerId,
                                 BankAccountName = partner.BankAccountName,
                                 BankAccountNo = partner.BankAccountNo,
                                 BankAddress = partner.BankAddress,
                                 SwiftCode = partner.SwiftCode,
                                 Note = partner.Note,
                                 BankId = partner.BankId,
                                 BankName = catBank.BankNameEn,
                                 BankCode = catBank.Code,
                                 Source = catBank.Source,
                                 Active = catBank.Active,
                                 ApproveStatus = partner.ApproveStatus,
                                 BeneficiaryAddress = partner.BeneficiaryAddress,
                                 UserCreated = partner.UserCreated,
                                 UserModified = partner.UserModified,
                                 DatetimeCreated = partner.DatetimeCreated,
                                 DatetimeModified = partner.DatetimeModified,
                                 UserCreatedName = sysUserRepository.First(x => x.Id == partner.UserCreated).Username,
                                 UserModifiedName = sysUserRepository.First(x => x.Id == partner.UserModified).Username,
                             };

            return dataReturn;
        }

        public async Task<HandleState> Update(CatPartnerBankModel model)
        {
            var result = new HandleState();
            try
            {
                var existedItem = await DataContext.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                if (existedItem == null)
                {
                    return new HandleState(stringLocalizer["MSG_PARTNER_BANK_EXISTED"].Value);
                }
                model.DatetimeCreated = existedItem.DatetimeCreated;
                model.DatetimeModified = DateTime.Now;
                model.UserCreated = existedItem.UserCreated;
                model.UserModified = currentUser.UserID;
                model.Active = existedItem.Active;

                result = DataContext.Update(model, x => x.Id == model.Id, true);
                return result;
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }

        public async Task<HandleState> Delete(Guid Id)
        {
            var hs = await DataContext.DeleteAsync(x => x.Id == Id);
            return hs;
        }
        #endregion

        public async Task<HandleState> ReviseBankInformation(Guid bankId)
        {
            var bankDetail = await DataContext.Where(x => x.Id == bankId).FirstOrDefaultAsync();
            bankDetail.ApproveStatus = "Revise";
            var hs = DataContext.Update(bankDetail, x => x.Id == bankId);

            return hs;
        }

        public async Task<List<BankSyncModel>> GetListPartnerBankInfoToSync(List<Guid> partnerBankIds)
        {
            var hs = new HandleState();
            var dataReturn = new List<BankSyncModel>();
            var bankDetails = await DataContext.WhereAsync(x => partnerBankIds.Contains(x.Id));
            foreach (var item in bankDetails)
            {
                var partner = await catPartnerRepository.Get(x => x.Id == item.PartnerId.ToString()).FirstOrDefaultAsync();
                var bank = await catBankRepository.Get(x => x.Id.ToString() == item.BankId).FirstOrDefaultAsync();

                var lstbankDetail = new List<BankDetail>
                {
                    new BankDetail
                    {
                        BankName = bank.BankNameEn,
                        BankCode = bank.Code,
                        BankAccountNo = item.BankAccountNo,
                        SwiftCode = item.SwiftCode,
                        Address = item.BankAddress
                    }
                };

                var test = _serviceUrl.Value.ApiUrlFileSystem.ToString();
                var lstAttachedFile = await sysImageRepository.Get(x => x.ObjectId == item.Id.ToString())
                .Select(x => new AttachedDocument
                {
                    AttachDocDate = x.DateTimeCreated,
                    AttachDocName = x.Name,
                    AttachDocPath = _serviceUrl.Value.ApiUrlFileSystem.ToString() + "/api/v1/en-Us/AWSS3/DownloadFile/" + x.KeyS3,
                    AttachDocRowId = x.Id,
                }).ToListAsync();

                var dataSync = new BankSyncModel
                {
                    Details = lstbankDetail,
                    CustomerCode = partner.AccountNo,
                    AtchDocInfo = lstAttachedFile
                };
                dataReturn.Add(dataSync);
            }


            return dataReturn;
        }

        public async Task<HandleState> UpdateByStatus(List<Guid> Ids, string status)
        {
            var hs = new HandleState();
            var partnerBanks = DataContext.Get(x => Ids.Contains(x.Id));

            foreach (var item in partnerBanks)
            {
                item.ApproveStatus = status;
                hs = await DataContext.UpdateAsync(item, x => x.Id == item.Id, false);
            }

            hs = DataContext.SubmitChanges();
            return hs;
        }

        public async Task<HandleState> ImportPartnerBank(List<CatPartnerBankImportModel> data)
        {
            try
            {
                foreach (var item in data)
                {
                    var catBank = catBankRepository.First(x => x.Code == item.BankCode.Trim())?.Id.ToString() ?? null;
                    var charge = new CatPartnerBank
                    {
                        Id = Guid.NewGuid(),
                        PartnerId = item.PartnerId,
                        BankId = catBank,
                        BankAccountName = item.BankAccountName,
                        BankAccountNo = item.BankAccountNo,
                        BankAddress = item.BankAddress,
                        BeneficiaryAddress = item.BeneficiaryAddress,
                        SwiftCode = item.SwiftCode,
                        ApproveStatus = "Approved",
                        DatetimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        UserCreated = item.UserCreated,
                        UserModified = item.UserModified,
                        Active = true
                    };
                    DataContext.Add(charge, false);
                }
                DataContext.SubmitChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public async Task<List<CatPartnerBankImportModel>> CheckValidImport(List<CatPartnerBankImportModel> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var bankImport = list[i];

                var partner = await catPartnerRepository.Where(x => x.AccountNo == bankImport.CustomerCode.ToString()).FirstOrDefaultAsync();
                var existingBankAccounts = await DataContext.WhereAsync(x => x.PartnerId.ToString() == partner.Id);

                if (existingBankAccounts.Any(account => account.BankAccountNo == bankImport.BankAccountNo))
                {
                    bankImport.ErrorMessage = stringLocalizer["MSG_PARTNER_BANK_EXISTED"].Value;
                    bankImport.IsValid = false;
                }
            }
            return list;
        }

        public IQueryable<CatPartnerBankModel> GetApprovedBanksByPartner(Guid partnerId)
        {
            var dataReturn = from partner in DataContext.Where(x => x.PartnerId == partnerId.ToString() && x.ApproveStatus == "Approved")
                             join catBank in catBankRepository.Get() on partner.BankId equals catBank.Id.ToString()
                             orderby partner.DatetimeCreated descending
                             select new CatPartnerBankModel
                             {
                                 Id = partner.Id,
                                 PartnerId = partner.PartnerId,
                                 BankAccountName = partner.BankAccountName,
                                 BankAccountNo = partner.BankAccountNo,
                                 BankAddress = partner.BankAddress,
                                 SwiftCode = partner.SwiftCode,
                                 Note = partner.Note,
                                 BankId = partner.BankId,
                                 BankName = catBank.BankNameEn,
                                 BankCode = catBank.Code,
                                 Source = catBank.Source,
                                 Active = catBank.Active,
                                 ApproveStatus = partner.ApproveStatus,
                                 BeneficiaryAddress = partner.BeneficiaryAddress,
                                 UserCreated = partner.UserCreated,
                                 UserModified = partner.UserModified,
                                 DatetimeCreated = partner.DatetimeCreated,
                                 DatetimeModified = partner.DatetimeModified,
                                 UserCreatedName = sysUserRepository.First(x => x.Id == partner.UserCreated).Username,
                                 UserModifiedName = sysUserRepository.First(x => x.Id == partner.UserModified).Username,
                             };

            return dataReturn;
        }
    }
}
