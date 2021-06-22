using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;
using System.Linq.Expressions;
using ITL.NetCore.Connection.Caching;
using System.Collections.Generic;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Common.Globals;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatBankService : RepositoryBaseCache<CatBank, CatBankModel>, ICatBankService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IStringLocalizer stringLocalizer;

        public CatBankService(IContextBase<CatBank> repository,
            ICacheServiceBase<CatBank> cacheService,
            IMapper mapper,
            IContextBase<SysUser> sysUserRepo,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser currUser) : base(repository, cacheService, mapper)
        {
            currentUser = currUser;
            sysUserRepository = sysUserRepo;
            stringLocalizer = localizer;
        }

        #region CRUD
        public override HandleState Add(CatBankModel entity)
        {
            var bank = mapper.Map<CatBank>(entity);
            bank.Id = Guid.NewGuid();
            bank.DatetimeCreated = bank.DatetimeModified = DateTime.Now;
            bank.Active = true;
            bank.UserCreated = currentUser.UserID;
            var result = DataContext.Add(bank, false);
            DataContext.SubmitChanges();
            if (result.Success)
            {
                ClearCache();
                Get();
            }
            return result;
        }
        public HandleState Update(CatBankModel model)
        {
            HandleState result = new HandleState();
            try
            {
                var bank = GetDetail(model.Id);
                var entity = mapper.Map<CatBank>(bank);
                entity.UserModified = currentUser.UserID;
                entity.DatetimeModified = DateTime.Now;
                entity.BankNameEn = model.BankNameEn;
                entity.BankNameVn = model.BankNameVn;

                if (entity.Active == false)
                    entity.InactiveOn = DateTime.Now;

                result = DataContext.Update(entity, x => x.Id == model.Id, false);
                if (result.Success)
                {
                    DataContext.SubmitChanges();
                    ClearCache();
                    Get();
                }
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }
        public HandleState Delete(Guid id)
        {
            //ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success == true)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion


        public IQueryable<CatBankModel> Paging(CatBankCriteria criteria, int pageNumber, int pageSize, out int rowsCount)
        {
            IQueryable<CatBankModel> results = QueryPaging(criteria);
            if (results == null)
            {
                rowsCount = 0;
                return null;
            }

            rowsCount = results.Select(x => x.Id).Count();
            if (rowsCount == 0)
                return null;

            if (pageSize > 1)
            {
                if (pageNumber < 1) { pageNumber = 1; }
                results.OrderByDescending(x => x.DatetimeModified).Skip((pageNumber - 1) * pageSize).Take(pageSize).AsQueryable();
            }
            return results;
        }

        private IQueryable<CatBankModel> QueryPaging(CatBankCriteria criteria)
        {
            var sysUsers = sysUserRepository.Get();
            var sysBanks = GetAll();
            var query = (from bank in sysBanks
                         join user in sysUsers on bank.UserCreated equals user.Id into userCreate
                         join user2 in sysUsers on bank.UserModified equals user2.Id into userModifi
                         from modifi in userModifi.DefaultIfEmpty()
                         from create in userCreate.DefaultIfEmpty()
                         select new { bank, create, modifi }
                );

            if (criteria.Code != null)
                query = query.Where(x => (x.bank.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            else if (criteria.BankNameVn != null)
                query = query.Where(x => (x.bank.BankNameVn ?? "").IndexOf(criteria.BankNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            else if (criteria.BankNameEn != null)
                query = query.Where(x => (x.bank.BankNameEn ?? "").IndexOf(criteria.BankNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            else
            {
                query = query.Where(x => (x.bank.Code.ToString() ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.bank.BankNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.bank.BankNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }

            var res = query.Select(x => new CatBankModel
            {
                Id = x.bank.Id,
                Code = x.bank.Code,
                BankNameEn = x.bank.BankNameEn,
                BankNameVn = x.bank.BankNameVn,
                UserCreated = x.bank.UserCreated,
                DatetimeCreated = x.bank.DatetimeCreated,
                UserModified = x.bank.UserModified,
                DatetimeModified = x.bank.DatetimeModified,
                Active = x.bank.Active,
                InactiveOn = x.bank.InactiveOn,
                UserCreatedName = x.create.Username,
                UserModifiedName = x.modifi.Username
            });

            return res;
        }

        public IQueryable<CatBankModel> Query(CatBankCriteria criteria)
        {
            return GetBy(criteria);
        }

        private IQueryable<CatBankModel> GetBy(CatBankCriteria criteria)
        {
            ClearCache();
            Expression<Func<CatBankModel, bool>> query;
            if (criteria.Code != null)
                query = (x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            else if (criteria.BankNameVn != null)
                query = (x => (x.BankNameVn ?? "").IndexOf(criteria.BankNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            else if (criteria.BankNameEn != null)
                query = (x => (x.BankNameEn ?? "").IndexOf(criteria.BankNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            else if(criteria.Active != null)
            {
                query = (x => x.Active == criteria.Active);
            }
            else
            {
                query = (x => (x.Code.ToString() ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.BankNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.BankNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }
            return Get(query);
        }

        public IQueryable<CatBankModel> GetAll()
        {
            ClearCache();
            return Get();
        }

        public CatBankModel GetDetail(Guid id)
        {
            ClearCache();
            CatBankModel queryDetail = Get(x => x.Id == id).FirstOrDefault();

            // Get usercreate name
            if (queryDetail.UserCreated != null)
                queryDetail.UserCreatedName = sysUserRepository.Get(x => x.Id == queryDetail.UserCreated)?.FirstOrDefault()?.Username;
            // Get usermodified name
            if (queryDetail.UserCreated != null)
                queryDetail.UserModifiedName = sysUserRepository.Get(x => x.Id == queryDetail.UserModified)?.FirstOrDefault()?.Username;

            return queryDetail;

        }

        public List<CatBankImportModel> CheckValidImport(List<CatBankImportModel> list)
        {
            var banks = Get();
            for (int i = 0; i < list.Count; i++)
            {
                var bankImport = list[i];

                if (string.IsNullOrEmpty(bankImport.BankNameVn))
                {
                    bankImport.BankName_VN_Error = stringLocalizer[CatalogueLanguageSub.MSG_BANK_NAME_VN_EMPTY];
                    bankImport.IsValid = false;
                }
                if (string.IsNullOrEmpty(bankImport.BankNameEn))
                {
                    bankImport.BankName_EN_Error = stringLocalizer[CatalogueLanguageSub.MSG_BANK_NAME_EN_EMPTY];
                    bankImport.IsValid = false;
                }
                if (string.IsNullOrEmpty(bankImport.Code))
                {
                    bankImport.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_BANK_CODE_EMPTY];
                    bankImport.IsValid = false;
                }
                else
                {
                    var check = banks.FirstOrDefault(x => x.Code.ToLower() == bankImport.Code.ToLower());
                    if (check != null)
                    {
                        bankImport.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_CODE_EXISTED, bankImport.Code];
                        bankImport.IsValid = false;
                    }
                    if (list.Count(x => x.Code?.ToLower() == bankImport.Code?.ToLower()) > 1)
                    {
                        bankImport.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_CODE_DUPLICATED, bankImport.Code];
                        bankImport.IsValid = false;
                    }
                }

            }
            return list;
        }

        public HandleState Import(List<CatBankImportModel> data)
        {
            try
            {
                foreach (var item in data)
                {
                    var charge = new CatBank
                    {
                        Id = Guid.NewGuid(),
                        Code = item.Code,
                        BankNameVn = item.BankNameVn,
                        BankNameEn = item.BankNameEn,
                        Active = item.Status.Trim().ToLower() == "active",
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                    };
                    DataContext.Add(charge, false);
                }
                DataContext.SubmitChanges();
                ClearCache();
                Get();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
    }
}
