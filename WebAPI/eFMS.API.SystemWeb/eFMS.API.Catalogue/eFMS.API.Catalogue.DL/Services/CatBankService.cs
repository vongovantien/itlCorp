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

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatBankService : RepositoryBaseCache<CatBank, CatBankModel>, ICatBankService
    {
        private readonly ICurrentUser currentUser;

        public CatBankService(IContextBase<CatBank> repository, 
            ICacheServiceBase<CatBank> cacheService, 
            IMapper mapper,
            ICurrentUser currUser) : base(repository, cacheService, mapper)
        {
            currentUser = currUser;
            //SetChildren<CatCharge>("Id", "CurrencyId");
            //SetChildren<CatBankExchange>("Id", "CurrencyFromId");
            //SetChildren<CatBankExchange>("Id", "CurrencyToId");
            //SetChildren<AcctSoa>("Id", "CurrencyId");
            //SetChildren<CatCharge>("Id", "CurrencyId");
            //SetChildren<CsShipmentSurcharge>("Id", "CurrencyId");
            //SetChildren<AcctCdnote>("Id", "CurrencyId");
            //SetChildren<AcctSoa>("Id", "Currency");
            //SetChildren<CatCharge>("Id", "Currency");
        }

        #region CRUD
        public override HandleState Add(CatBankModel entity)
        {
            var bank = mapper.Map<CatBank>(entity);
            bank.DatetimeCreated = bank.DatetimeModified = DateTime.Now;
            bank.Active = true;
            bank.UserCreated = currentUser.UserID;
            var result = DataContext.Add(bank,false);
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
                var entity = mapper.Map<CatBank>(model);
                entity.UserModified = currentUser.UserID;
                entity.DatetimeModified = DateTime.Now;
                if (entity.Active == false)
                {
                    entity.InactiveOn = DateTime.Now;
                }
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
            if(hs.Success == true)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion


        public IQueryable<CatBankModel> Paging(CatBankCriteria criteria, int pageNumber, int pageSize, out int rowsCount)
        {
            Expression<Func<CatBankModel, bool>> query;

            if (criteria.Code != null)
                query = (x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            else if(criteria.BankNameVn != null)
                query = (x => (x.BankNameVn ?? "").IndexOf(criteria.BankNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            else if (criteria.BankNameEn != null)
                query = (x => (x.BankNameEn ?? "").IndexOf(criteria.BankNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            else if (criteria.Active != null)
            {
                query = (x => x.Active == criteria.Active);
            }
            else
            {
                query = (x => (x.Code.ToString() ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.BankNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.BankNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }

            var data = Paging(query, pageNumber, pageSize, x => x.DatetimeModified, false, out rowsCount);
            return data;
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
    }
}
