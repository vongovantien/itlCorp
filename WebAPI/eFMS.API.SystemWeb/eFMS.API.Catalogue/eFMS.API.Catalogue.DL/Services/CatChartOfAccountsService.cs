using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChartOfAccountsService : RepositoryBaseCache<CatChartOfAccounts, CatChartOfAccountsModel>, ICatChartOfAccountsService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatChartOfAccounts> chartRepository;
        public CatChartOfAccountsService(IContextBase<CatChartOfAccounts> repository,
        ICacheServiceBase<CatChartOfAccounts> cacheService,
        IMapper mapper,
        IStringLocalizer<LanguageSub> localizer,
        ICurrentUser user,
        IContextBase<CatChartOfAccounts> chartRepo) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            chartRepository = chartRepo;
        }

        #region CRUD
        public object Add(CatChartOfAccounts model)
        {
            model.Id = Guid.NewGuid();
            model.UserCreated = model.UserModified = currentUser.UserID;
            model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
            model.GroupId = currentUser.GroupId;
            model.DepartmentId = currentUser.DepartmentId;
            model.OfficeId = currentUser.OfficeID;
            model.CompanyId = currentUser.CompanyID;
            using (var bk = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Add(model);
                    var result = hs;
                    bk.Commit();
                    if (hs.Success)
                    {
                        ClearCache();
                        Get();
                    }
                    return new { model, result };

                }
                catch (Exception ex)
                {
                    bk.Rollback();
                    var result = new HandleState(ex.Message);
                    return new { model = new object { }, result };
                }
                finally
                {
                    bk.Dispose();
                }
            }
        }

        public HandleState Update(CatChartOfAccounts model)
        {

            // check permission
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            var accObj = DataContext.First(x => x.Id == model.Id);

            model.UserCreated = accObj.UserCreated;
            model.OfficeId = accObj.OfficeId;
            model.CompanyId = accObj.CompanyId;
            model.DepartmentId = accObj.DepartmentId;
            model.GroupId = accObj.GroupId;
            var resultToUpdate = GetPermissionDetail(permissionRange, model);
            if (!resultToUpdate) return new HandleState(403, "");
            // en check permission

            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            model.DatetimeCreated = accObj.DatetimeCreated;
            if (accObj == null)
            {
                return new HandleState("Chart not found !");
            }

            using (var accTrans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Update(model, x => x.Id == model.Id);
                    accTrans.Commit();
                    if (hs.Success)
                    {
                        ClearCache();
                        Get();

                    }
                    return hs;

                }
                catch (Exception ex)
                {
                    accTrans.Rollback();
                    var result = new HandleState(ex.Message);
                    return result;
                }
                finally
                {
                    accTrans.Dispose();
                }
            }

        }
        public HandleState Delete(Guid accId)
        {
            using (var accTrans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Delete(x => x.Id == accId);
                    if (hs.Success)
                    {
                        accTrans.Commit();
                        ClearCache();
                        Get();
                    }
                    else
                    {
                        accTrans.Rollback();
                    }
                    return hs;
                }
                catch (Exception ex)
                {
                    accTrans.Rollback();
                    var result = new HandleState(ex.Message);
                    return result;
                }
                finally
                {
                    accTrans.Dispose();
                }
            }
        }

        public CatChartOfAccountsModel GetDetail(Guid id)
        {
            var queryDetail = Get(x => x.Id == id).FirstOrDefault();
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPartnerdata);//Set default
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            bool resultDelete = false;
            switch (permissionRangeDelete)
            {
                case PermissionRange.None:
                    break;
                case PermissionRange.Owner:

                    if (queryDetail.UserCreated == currentUser.UserID)
                    {
                        resultDelete = true;
                    }
                    break;
                case PermissionRange.Group:
                    if (queryDetail.GroupId == currentUser.GroupId && queryDetail.DepartmentId == currentUser.DepartmentId)
                    {
                        resultDelete = true;
                    }
                    break;
                case PermissionRange.Department:
                    if (queryDetail.DepartmentId != currentUser.DepartmentId)
                    {
                        resultDelete = true;
                    }
                    break;
                case PermissionRange.Office:
                    if (queryDetail.OfficeId == currentUser.OfficeID)
                    {
                        resultDelete = true;
                    }
                    break;
                case PermissionRange.Company:
                    if (queryDetail.CompanyId == currentUser.CompanyID)
                    {
                        resultDelete = true;
                    }
                    break;
            }

            queryDetail.Permission = new PermissionAllowBase
            {
                AllowUpdate = GetPermissionDetail(permissionRangeWrite, queryDetail),
                AllowDelete = resultDelete
            };
            return queryDetail;
        }

        private bool GetPermissionDetail(PermissionRange permissionRangeWrite, CatChartOfAccounts model)
        {
            bool result = false;
            switch (permissionRangeWrite)
            {
                case PermissionRange.All:
                    result = true;
                    break;
                case PermissionRange.Owner:
                    if (model.UserCreated == currentUser.UserID)
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId == currentUser.GroupId
                     && model.DepartmentId == currentUser.DepartmentId
                     && model.OfficeId == currentUser.OfficeID
                     && model.CompanyId == currentUser.CompanyID)
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId == currentUser.DepartmentId
                      && model.OfficeId == currentUser.OfficeID
                      && model.CompanyId == currentUser.CompanyID)
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        result = true;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId == currentUser.CompanyID)
                    {
                        result = true;
                    }
                    break;
            }
            return result;
        }

        #endregion

        public IQueryable<CatChartOfAccounts> Paging(CatChartOfAccountsCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catChartOfAccounts);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            switch (rangeSearch)
            {
                case PermissionRange.None:
                    data = null;
                    break;
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    data = data.Where(x => x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Group:
                    data = data.Where(x => x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Department:
                    data = data.Where(x => x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Office:
                    data = data.Where(x => x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Company:
                    data = data.Where(x => x.CompanyId == currentUser.CompanyID);
                    break;
            }
            rowsCount = data.Select(x => x.Id).Count();
            if (rowsCount == 0)
            {
                return null;
            }
            IQueryable<CatChartOfAccounts> results = null;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = data.OrderByDescending(x => x.DatetimeModified).Skip((page - 1) * size).Take(size).AsQueryable();
            }
            return results;
        }

        public IQueryable<CatChartOfAccounts> Query(CatChartOfAccountsCriteria criteria)
        {
            var data = chartRepository.Get();
            bool? active = null;
  
            if (criteria.All == null)
            {
                if (criteria.Active?.ToLower() == "active")
                {
                    active = true;
                }
                else if (criteria.Active?.ToLower() == "inactive")
                {
                    active = false;
                }
                data = data.Where(x => (x.AccountCode ?? "").IndexOf(criteria.AccountCode ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (x.AccountNameLocal ?? "").IndexOf(criteria.AccountNameLocal ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (x.AccountNameEn ?? "").IndexOf(criteria.AccountNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (x.Active == active || criteria.Active == null));
            }
            else
            {
                {
                    if (criteria.All?.ToLower() == "active")
                    {
                        active = true;
                    }
                    else if (criteria.All?.ToLower() == "inactive")
                    {
                        active = false;
                    }

                    data = data.Where(x => (x.AccountCode ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.AccountNameLocal ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.AccountNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                     || (x.Active == active || criteria.All == null));
                }

            }
            return data;
        }


    }
}
