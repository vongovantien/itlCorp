using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
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

        public CatPartnerBankService(IContextBase<CatPartnerBank> repository,
            IMapper mapper,
            IContextBase<SysUser> sysUserRepo,
            IContextBase<CatBank> catBankRepo,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser currUser) : base(repository, mapper)
        {
            currentUser = currUser;
            sysUserRepository = sysUserRepo;
            stringLocalizer = localizer;
            catBankRepository = catBankRepo;
        }

        #region CRUD
        public async Task<HandleState> AddNew(CatPartnerBankModel entity)
        {
            var result = new HandleState();
            var newItem = mapper.Map<CatPartnerBank>(entity);
            newItem.Id = Guid.NewGuid();
            newItem.DatetimeCreated = newItem.DatetimeModified = DateTime.Now;
            newItem.UserCreated = newItem.UserModified = currentUser.UserID;
            newItem.Active = true;
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

        public async Task<CatPartnerBankModel> GetDetail(Guid Id)
        {
            var dataReturn = from partner in DataContext.Where(x => x.Id == Id)
                             join catBank in catBankRepository.Get() on partner.BankId equals catBank.Id
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
            var dataReturn = from partner in DataContext.Where(x => x.PartnerId == partnerId)
                             join catBank in catBankRepository.Get() on partner.BankId equals catBank.Id
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
                    return new HandleState("Bank Account Not Found !!");
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
    }
}
