using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatAddressPartnerService : RepositoryBaseCache<CatAddressPartner, CatAddressPartnerModel>, ICatAddressPartnerService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IMapper mapper;

        public CatAddressPartnerService(IContextBase<CatAddressPartner> repository,
            ICacheServiceBase<CatAddressPartner> cacheService,
            IMapper imapper,
            IContextBase<SysUser> sysUserRepo,
            IStringLocalizer<LanguageSub> localizer,
        ICurrentUser currUser) : base(repository, cacheService, imapper)
        {
            currentUser = currUser;
            sysUserRepository = sysUserRepo;
            stringLocalizer = localizer;
            mapper = imapper;
        }

        #region CRUD
        public override HandleState Add(CatAddressPartnerModel entity)
        {
            var address = mapper.Map<CatAddressPartner>(entity);
            address.Id = Guid.NewGuid();
            address.DatetimeCreated = address.DatetimeModified = DateTime.Now;
            address.Active = true;
            address.UserCreated = address.UserModified = currentUser.UserID;
            var result = DataContext.Add(address, false);
            DataContext.SubmitChanges();
            if (result.Success)
            {
                ClearCache();
                Get();
            }
            return result;
        }
        public HandleState Update(CatAddressPartnerModel model)
        {
            HandleState result = new HandleState();
            try
            {
                var address = GetDetail(model.Id);
                var entity = mapper.Map<CatAddressPartner>(address);
                entity.UserModified = currentUser.UserID;
                entity.DatetimeModified = DateTime.Now;
                entity.ShortNameAddress = model.ShortNameAddress;
                entity.StreetAddress = model.StreetAddress;
                entity.AddressType = model.AddressType;
                entity.Location = model.Location;
                entity.Active = model.Active;

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


        //    public IQueryable<CatAddressPartnerModel> Paging(CatBankCriteria criteria, int pageNumber, int pageSize, out int rowsCount)
        //    {
        //        IQueryable<CatAddressPartnerModel> results = QueryPaging(criteria);
        //        if (results == null)
        //        {
        //            rowsCount = 0;
        //            return null;
        //        }

        //        rowsCount = results.Select(x => x.Id).Count();
        //        if (rowsCount == 0)
        //            return null;

        //        if (pageSize > 1)
        //        {
        //            if (pageNumber < 1) { pageNumber = 1; }
        //            results = results.OrderByDescending(x => x.DatetimeModified).Skip((pageNumber - 1) * pageSize).Take(pageSize).AsQueryable();
        //        }
        //        return results;
        //    }

        //    private IQueryable<CatAddressPartnerModel> QueryPaging(CatAddressPartnerCriteria criteria)
        //    {
        //        //var sysUsers = sysUserRepository.Get();
        //        //var sysBanks = GetAll();
        //        //var query = (from bank in sysBanks
        //        //             join user in sysUsers on bank.UserCreated equals user.Id into userCreate
        //        //             join user2 in sysUsers on bank.UserModified equals user2.Id into userModifi
        //        //             from modifi in userModifi.DefaultIfEmpty()
        //        //             from create in userCreate.DefaultIfEmpty()
        //        //             select new { bank, create, modifi }
        //        //    );

        //        //if (criteria.ShortNameAddress != null)
        //        //    query = query.Where(x => (x.bank.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1);
        //        //else if (criteria.BankNameVn != null)
        //        //    query = query.Where(x => (x.bank.BankNameVn ?? "").IndexOf(criteria.BankNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1);
        //        //else if (criteria.BankNameEn != null)
        //        //    query = query.Where(x => (x.bank.BankNameEn ?? "").IndexOf(criteria.BankNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1);
        //        //else if (criteria.Active != null)
        //        //{
        //        //    query = query.Where(x => x.bank.Active == criteria.Active);
        //        //}
        //        //else
        //        //{
        //        //    query = query.Where(x => (x.bank.Code.ToString() ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
        //        //                       || (x.bank.BankNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
        //        //                       || (x.bank.BankNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1);
        //        //}

        //        //var res = query.Select(x => new CatAddressPartnerModel
        //        //{
        //        //    Id = x.bank.Id,
        //        //    Code = x.bank.Code,
        //        //    BankNameEn = x.bank.BankNameEn,
        //        //    BankNameVn = x.bank.BankNameVn,
        //        //    UserCreated = x.bank.UserCreated,
        //        //    DatetimeCreated = x.bank.DatetimeCreated,
        //        //    UserModified = x.bank.UserModified,
        //        //    DatetimeModified = x.bank.DatetimeModified,
        //        //    Active = x.bank.Active,
        //        //    InactiveOn = x.bank.InactiveOn,
        //        //    UserCreatedName = x.create.Username,
        //        //    UserModifiedName = x.modifi.Username
        //        //});

        //        //return res;
        //    }

        //    public IQueryable<CatAddressPartnerModel> Query(CatAddressPartnerCriteria criteria)
        //    {
        //        return GetBy(criteria);
        //    }

        //    private IQueryable<CatAddressPartnerModel> GetBy(CatAddressPartnerCriteria criteria)
        //    {
        //        ClearCache();
        //        Expression<Func<CatAddressPartnerModel, bool>> query;
        //        if (criteria.Code != null)
        //            query = (x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1);
        //        else if (criteria.BankNameVn != null)
        //            query = (x => (x.BankNameVn ?? "").IndexOf(criteria.BankNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1);
        //        else if (criteria.BankNameEn != null)
        //            query = (x => (x.BankNameEn ?? "").IndexOf(criteria.BankNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1);
        //        else if (criteria.Active != null)
        //        {
        //            query = (x => x.Active == criteria.Active);
        //        }
        //        else
        //        {
        //            query = (x => (x.Code.ToString() ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
        //                               || (x.BankNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
        //                               || (x.BankNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1);
        //        }
        //        return Get(query);
        //    }

        public IQueryable<CatAddressPartnerModel> GetAll()
        {
            ClearCache();
            return Get();
        }

        public CatAddressPartnerModel GetDetail(Guid id)
        {
            ClearCache();
            CatAddressPartnerModel queryDetail = Get(x => x.Id == id).FirstOrDefault();
            // Get usercreate name
            if (queryDetail.UserCreated != null)
                queryDetail.UserCreatedName = sysUserRepository.Get(x => x.Id == queryDetail.UserCreated)?.FirstOrDefault()?.Username;
            // Get usermodified name
            if (queryDetail.UserCreated != null)
                queryDetail.UserModifiedName = sysUserRepository.Get(x => x.Id == queryDetail.UserModified)?.FirstOrDefault()?.Username;

            return queryDetail;

        }

        //    public List<CatBankImportModel> CheckValidImport(List<CatBankImportModel> list)
        //    {
        //        var banks = Get();
        //        for (int i = 0; i < list.Count; i++)
        //        {
        //            var bankImport = list[i];

        //            if (string.IsNullOrEmpty(bankImport.BankNameVn))
        //            {
        //                bankImport.BankName_VN_Error = stringLocalizer[CatalogueLanguageSub.MSG_BANK_NAME_VN_EMPTY];
        //                bankImport.IsValid = false;
        //            }
        //            if (string.IsNullOrEmpty(bankImport.BankNameEn))
        //            {
        //                bankImport.BankName_EN_Error = stringLocalizer[CatalogueLanguageSub.MSG_BANK_NAME_EN_EMPTY];
        //                bankImport.IsValid = false;
        //            }
        //            if (string.IsNullOrEmpty(bankImport.Code))
        //            {
        //                bankImport.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_BANK_CODE_EMPTY];
        //                bankImport.IsValid = false;
        //            }
        //            else
        //            {
        //                var check = banks.FirstOrDefault(x => x.Code.ToLower() == bankImport.Code.ToLower());
        //                if (check != null)
        //                {
        //                    bankImport.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_CODE_EXISTED, bankImport.Code];
        //                    bankImport.IsValid = false;
        //                }
        //                if (list.Count(x => x.Code?.ToLower() == bankImport.Code?.ToLower()) > 1)
        //                {
        //                    bankImport.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_CHARGE_CODE_DUPLICATED, bankImport.Code];
        //                    bankImport.IsValid = false;
        //                }
        //            }

        //        }
        //        return list;
        //    }

        //    public HandleState Import(List<CatBankImportModel> data)
        //    {
        //        try
        //        {
        //            foreach (var item in data)
        //            {
        //                var charge = new CatBank
        //                {
        //                    Id = Guid.NewGuid(),
        //                    Code = item.Code,
        //                    BankNameVn = item.BankNameVn,
        //                    BankNameEn = item.BankNameEn,
        //                    Active = item.Status.Trim().ToLower() == "active",
        //                    DatetimeCreated = DateTime.Now,
        //                    UserCreated = currentUser.UserID,
        //                };
        //                DataContext.Add(charge, false);
        //            }
        //            DataContext.SubmitChanges();
        //            ClearCache();
        //            Get();
        //            return new HandleState();
        //        }
        //        catch (Exception ex)
        //        {
        //            return new HandleState(ex.Message);
        //        }
        //    }

        public async Task<IQueryable<CatAddressPartnerModel>> GetAddressByPartnerId(Guid id)
        {
            var data = await DataContext.WhereAsync(x => x.PartnerId == id);
            if (data.Count() == 0)
            {
                return Enumerable.Empty<CatAddressPartnerModel>().AsQueryable();
            }

            var result = data.Select(x => new CatAddressPartnerModel
            {
                Id = x.Id,
                UserCreated = x.UserCreated,
                DatetimeCreated = x.DatetimeCreated,
                UserModified = x.UserModified,
                DatetimeModified = x.DatetimeModified,
                Active = x.Active,
                InactiveOn = x.InactiveOn,
            });

            return result.AsQueryable();
        }
    }
}

