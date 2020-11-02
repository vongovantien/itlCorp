using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysAuthorizedApprovalService : RepositoryBaseCache<SysAuthorizedApproval, SysAuthorizedApprovalModel>, ISysAuthorizedApprovalService
    {
        private readonly ICurrentUser currentUser;
        private IContextBase<SysUser> userRepository;

        public SysAuthorizedApprovalService(IContextBase<SysAuthorizedApproval> repository, IMapper mapper, ICurrentUser user,
            IContextBase<SysUser> userRepo, ICacheServiceBase<SysAuthorizedApproval> cacheService) : base(repository, cacheService, mapper)
        {
            currentUser = user;
            userRepository = userRepo;
        }
        #region CRUD
        public override HandleState Add(SysAuthorizedApprovalModel model)
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
                    return result;

                }
                catch (Exception ex)
                {
                    bk.Rollback();
                    var result = new HandleState(ex.Message);
                    return result;
                }
                finally
                {
                    bk.Dispose();
                }
            }
        }

        public HandleState Update(SysAuthorizedApprovalModel model)
        {

            // check permission
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.sysAuthorize);//Set default
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            var obj = DataContext.First(x => x.Id == model.Id);

            model.UserCreated = obj.UserCreated;
            model.OfficeId = obj.OfficeId;
            model.CompanyId = obj.CompanyId;
            model.DepartmentId = obj.DepartmentId;
            model.GroupId = obj.GroupId;
            var resultToUpdate = GetPermission(permissionRange, model);
            if (!resultToUpdate) return new HandleState(403, "");
            // en check permission

            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            model.DatetimeCreated = obj.DatetimeCreated;
            if (obj == null)
            {
                return new HandleState("Authorized not found !");
            }

            using (var authorTrans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Update(model, x => x.Id == model.Id);
                    authorTrans.Commit();
                    return hs;

                }
                catch (Exception ex)
                {
                    authorTrans.Rollback();
                    var result = new HandleState(ex.Message);
                    return result;
                }
                finally
                {
                    ClearCache();
                    Get();
                    authorTrans.Dispose();
                }
            }

        }

        public HandleState Delete(Guid authorId)
        {
            using (var authorTrans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Delete(x => x.Id == authorId);
                    if (hs.Success)
                    {
                        authorTrans.Commit();
                    }
                    else
                    {
                        authorTrans.Rollback();
                    }
                    return hs;
                }
                catch (Exception ex)
                {
                    authorTrans.Rollback();
                    var result = new HandleState(ex.Message);
                    return result;
                }
                finally
                {
                    ClearCache();
                    Get();
                    authorTrans.Dispose();
                }
            }
        }


        #endregion

        public IQueryable<SysAuthorizedApprovalModel> Query(SysAuthorizedApprovalCriteria criteria)
        {
            Expression<Func<SysAuthorizedApproval, bool>> query = null;
            if (!string.IsNullOrEmpty(criteria.All))
            {
                query = x =>
                       x.Type.IndexOf(criteria.Type ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                   || x.Authorizer.IndexOf(criteria.Authorizer ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                   || x.Commissioner.IndexOf(criteria.Commissioner ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
                if (criteria.EffectiveDate != null)
                {
                    query = query.Or(x => x.EffectiveDate.HasValue ? x.EffectiveDate.Value.Date == criteria.EffectiveDate.Value.Date : true);
                }
                if (criteria.ExpirationDate != null)
                {
                    query = query.Or(x => x.ExpirationDate.HasValue ? x.ExpirationDate.Value.Date == criteria.ExpirationDate.Value.Date : true);
                }

                if (criteria.Active != null)
                {
                    query = query.Or(x => x.Active == criteria.Active);
                }

            }
            else
            {
                query = x =>
                     x.Type.IndexOf(criteria.Type ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                     && x.Authorizer.IndexOf(criteria.Authorizer ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                     && x.Commissioner.IndexOf(criteria.Commissioner ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
                if (criteria.EffectiveDate != null)
                {
                    query = query.And(x => x.EffectiveDate.HasValue ? x.EffectiveDate.Value.Date == criteria.EffectiveDate.Value.Date : true);
                }
                if (criteria.ExpirationDate != null)
                {
                    query = query.And(x => x.ExpirationDate.HasValue ? x.ExpirationDate.Value.Date == criteria.ExpirationDate.Value.Date : true);
                }

                if (criteria.Active != null)
                {
                    query = query.And(x => x.Active == criteria.Active);
                }

            }
            var authorizeds = DataContext.Get(query);
            var users = userRepository.Get();
            var data = from author in authorizeds
                       join userCom in users on author.Commissioner equals userCom.Id into grpUserCom
                       from userCom in grpUserCom.DefaultIfEmpty()
                       join userAutho in users on author.Authorizer equals userAutho.Id into grpUserAutho
                       from userAutho in grpUserAutho.DefaultIfEmpty()
                       select new SysAuthorizedApprovalModel
                       {
                           Id = author.Id,
                           AuthorizerName = userAutho.Username,
                           Authorizer = author.Authorizer,
                           Commissioner = author.Commissioner,
                           CommissionerName = userCom.Username,
                           Type = author.Type,
                           EffectiveDate = author.EffectiveDate,
                           ExpirationDate = author.ExpirationDate,
                           Active = author.Active,
                           UserCreated = author.UserCreated,
                           UserModified = author.UserModified,
                           DatetimeCreated = author.DatetimeCreated,
                           DatetimeModified = author.DatetimeModified,
                           Description = author.Description,
                           CompanyId = author.CompanyId,
                           OfficeId = author.OfficeId,
                           DepartmentId = author.DepartmentId,
                           GroupId = author.GroupId

                       };
            var dataFor = data.ToList();

            dataFor.ForEach(x => { x.NameUserCreated = userRepository.Get(t => t.Id == x.UserCreated).FirstOrDefault()?.Username; x.NameUserModified = userRepository.Get(t => t.Id == x.UserModified).FirstOrDefault()?.Username; });
            var result = dataFor.ToArray().OrderByDescending(x => x.DatetimeModified).AsQueryable();
            return result;
        }

        public IQueryable<SysAuthorizedApprovalModel> Paging(SysAuthorizedApprovalCriteria criteria, int page, int size, out int rowsCount)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.sysAuthorize);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == 0)
            {
                rowsCount = 0;
                return null;
            }
            var data = Query(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }
            IQueryable<SysAuthorizedApprovalModel> dataPermission = QueryByPermission(data, rangeSearch);
            rowsCount = dataPermission.Select(x => x.Id).Count();
            if (rowsCount == 0)
            {
                return null;
            }
            IQueryable<SysAuthorizedApprovalModel> results = null;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = dataPermission.OrderByDescending(x => x.DatetimeModified).Skip((page - 1) * size).Take(size).AsQueryable();
            }
            return results;

        }

        #region permission
        public bool CheckDetailPermission(Guid id)
        {
            var detail = DataContext.Get(x => x.Id == id).FirstOrDefault();
            ICurrentUser _currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.sysAuthorize);
            var permissionRangeDetail = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Detail);
            bool code = GetPermission(permissionRangeDetail, detail);
            return code;
        }

        public bool CheckDeletePermission(Guid id)
        {
            var detail = DataContext.Get(x => x.Id == id).FirstOrDefault();
            ICurrentUser _currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.sysAuthorize);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Delete);
            bool code = GetPermission(permissionRangeDelete, detail);
            return code;
        }

        private IQueryable<SysAuthorizedApprovalModel> QueryByPermission(IQueryable<SysAuthorizedApprovalModel> data, PermissionRange range)
        {
            switch (range)
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
            return data;
        }

        private bool GetPermission(PermissionRange permissionRange, SysAuthorizedApproval model)
        {
            bool result = false;
            switch (permissionRange)
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


    }
}
