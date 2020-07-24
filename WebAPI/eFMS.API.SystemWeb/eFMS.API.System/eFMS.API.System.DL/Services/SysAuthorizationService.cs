using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.System.DL.Services
{
    public class SysAuthorizationService : RepositoryBase<SysAuthorization, SysAuthorizationModel>, ISysAuthorizationService
    {
        private readonly ICurrentUser currentUser;
        private IContextBase<SysUser> userRepository;
        public SysAuthorizationService(IContextBase<SysAuthorization> repository, IMapper mapper, ICurrentUser user,
            IContextBase<SysUser> userRepo) : base(repository, mapper)
        {
            currentUser = user;
            userRepository = userRepo;
        }

        public IQueryable<SysAuthorization> GetAuthorPermission()
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.sysAuthorize);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            var authorizeds = DataContext.Get();
            if (authorizeds == null) return null;
            switch (rangeSearch)
            {
                case PermissionRange.None:
                    authorizeds = null;
                    break;
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    authorizeds = authorizeds.Where(x => x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Group:
                    authorizeds = authorizeds.Where(x => (x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Department:
                    authorizeds = authorizeds.Where(x => (x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Office:
                    authorizeds = authorizeds.Where(x => (x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                                                || x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Company:
                    authorizeds = authorizeds.Where(x => x.CompanyId == currentUser.CompanyID
                                                || x.UserCreated == currentUser.UserID);
                    break;
            }
            return authorizeds;
        }

        public IQueryable<SysAuthorizationModel> QueryData(SysAuthorizationCriteria criteria)
        {
            Expression<Func<SysAuthorization, bool>> query = null;

            if (!string.IsNullOrEmpty(criteria.All))
            {
                query = x =>
                           x.Services.IndexOf(criteria.Service ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        || x.UserId.IndexOf(criteria.UserID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        || x.AssignTo.IndexOf(criteria.AssignTo ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
                if (criteria.StartDate != null)
                {
                    query = query.Or(x => x.StartDate.Date == criteria.StartDate.Value.Date);
                }
                if (criteria.EndDate != null)
                {
                    query = query.Or(x => x.EndDate.Value.Date == criteria.EndDate.Value.Date);
                }
                if (criteria.Active != null)
                {
                    query = query.Or(x => x.Active == criteria.Active);
                }
            }
            else
            {
                query = x =>
                       x.Services.IndexOf(criteria.Service ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && x.UserId.IndexOf(criteria.UserID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && x.AssignTo.IndexOf(criteria.AssignTo ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
                if (criteria.StartDate != null)
                {
                    query = query.And(x => x.StartDate.Date == criteria.StartDate.Value.Date);
                }
                if (criteria.EndDate != null)
                {
                    query = query.And(x => x.EndDate.Value.Date == criteria.EndDate.Value.Date);
                }
                if (criteria.Active != null)
                {
                    query = query.And(x => x.Active == criteria.Active);
                }
            }
            var authorizeds = DataContext.Get(query);
            var userAssigns = userRepository.Get();
            var userAssignTos = userRepository.Get();
            var queryData = (from authorize in authorizeds
                             join userAssign in userAssigns on authorize.UserId equals userAssign.Id into grpUserAssigneds
                             from assign in grpUserAssigneds.DefaultIfEmpty()
                             join userAssignTo in userAssignTos on authorize.AssignTo equals userAssignTo.Id into grpUserAssignedTos
                             from assignedTo in grpUserAssignedTos.DefaultIfEmpty()
                             select new SysAuthorizationModel
                             {
                                 Id = authorize.Id,
                                 Name = authorize.Name,
                                 ServicesName = GetServiceNameOfAuthorization(authorize.Services),
                                 Description = authorize.Description,
                                 UserId = authorize.UserId,
                                 AssignTo = authorize.AssignTo,
                                 StartDate = authorize.StartDate,
                                 EndDate = authorize.EndDate,
                                 UserCreated = authorize.UserCreated,
                                 DatetimeCreated = authorize.DatetimeCreated,
                                 UserModified = authorize.UserModified,
                                 DatetimeModified = authorize.DatetimeModified,
                                 Active = authorize.Active,
                                 InactiveOn = authorize.InactiveOn,
                                 UserNameAssign = assign.Username,
                                 UserNameAssignTo = assignedTo.Username
                             });

            var result = queryData.ToArray().OrderByDescending(x => x.DatetimeModified).AsQueryable();
            return result;
        }

        public IQueryable<SysAuthorizationModel> QueryDataPermission(SysAuthorizationCriteria criteria)
        {
            var authorPermission = GetAuthorPermission();
            if (authorPermission == null || authorPermission.Any() == false) return null;

            Expression<Func<SysAuthorization, bool>> query = null;

            if (!string.IsNullOrEmpty(criteria.All))
            {
                query = x =>
                           x.Services.IndexOf(criteria.Service ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        || x.UserId.IndexOf(criteria.UserID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                        || x.AssignTo.IndexOf(criteria.AssignTo ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
                if (criteria.StartDate != null)
                {
                    query = query.Or(x => x.StartDate.Date == criteria.StartDate.Value.Date);
                }
                if (criteria.EndDate != null)
                {
                    query = query.Or(x => x.EndDate.HasValue ? x.EndDate.Value.Date == criteria.EndDate.Value.Date : true);
                }
                if (criteria.Active != null)
                {
                    query = query.Or(x => x.Active == criteria.Active);
                }
            }
            else
            {
                query = x =>
                       x.Services.IndexOf(criteria.Service ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && x.UserId.IndexOf(criteria.UserID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && x.AssignTo.IndexOf(criteria.AssignTo ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
                if (criteria.StartDate != null)
                {
                    query = query.And(x => x.StartDate.Date == criteria.StartDate.Value.Date);
                }
                if (criteria.EndDate != null)
                {
                    query = query.And(x => x.EndDate.HasValue ? x.EndDate.Value.Date == criteria.EndDate.Value.Date : true);
                }
                if (criteria.Active != null)
                {
                    query = query.And(x => x.Active == criteria.Active);
                }
            }
            var authorizeds = authorPermission.Where(query);
            var userAssigns = userRepository.Get();
            var userAssignTos = userRepository.Get();
            var queryData = (from authorize in authorizeds
                             join userAssign in userAssigns on authorize.UserId equals userAssign.Id into grpUserAssigneds
                             from assign in grpUserAssigneds.DefaultIfEmpty()
                             join userAssignTo in userAssignTos on authorize.AssignTo equals userAssignTo.Id into grpUserAssignedTos
                             from assignedTo in grpUserAssignedTos.DefaultIfEmpty()
                             select new SysAuthorizationModel
                             {
                                 Id = authorize.Id,
                                 Name = authorize.Name,
                                 ServicesName = GetServiceNameOfAuthorization(authorize.Services),
                                 Description = authorize.Description,
                                 UserId = authorize.UserId,
                                 AssignTo = authorize.AssignTo,
                                 StartDate = authorize.StartDate,
                                 EndDate = authorize.EndDate,
                                 UserCreated = authorize.UserCreated,
                                 DatetimeCreated = authorize.DatetimeCreated,
                                 UserModified = authorize.UserModified,
                                 DatetimeModified = authorize.DatetimeModified,
                                 Active = authorize.Active,
                                 InactiveOn = authorize.InactiveOn,
                                 UserNameAssign = assign.Username,
                                 UserNameAssignTo = assignedTo.Username
                             });

            var result = queryData.ToArray().OrderByDescending(x => x.DatetimeModified).AsQueryable();
            return result;
        }

        private string GetServiceNameOfAuthorization(string services)
        {
            var serviceName = string.Empty;
            if (!string.IsNullOrEmpty(services))
            {
                //Tách chuỗi services thành mảng
                string[] arrayStrServices = services.Split(';').Where(x => x.ToString() != string.Empty).ToArray();
                //Xóa các services trùng
                string[] arrayGrpServices = arrayStrServices.Distinct<string>().ToArray();
                serviceName = string.Join("; ", arrayGrpServices.Select(s => CustomData.Services.Where(x => x.Value == s).FirstOrDefault()?.DisplayName));
            }
            return serviceName;
        }

        public IQueryable<SysAuthorizationModel> Paging(SysAuthorizationCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = QueryDataPermission(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }
            rowsCount = (data.Count() > 0) ? data.Count() : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            return data;
        }

        public SysAuthorizationModel GetById(int id)
        {
            SysAuthorizationModel data = new SysAuthorizationModel();
            SysAuthorization authoriza = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (authoriza != null)
            {
                data = mapper.Map<SysAuthorizationModel>(authoriza);
                data.ServicesName = GetServiceNameOfAuthorization(data.Services);
                data.UserCreatedName = userRepository.Where(x => x.Id == data.UserCreated)?.FirstOrDefault().Username;
                data.UserModifiedName = userRepository.Where(x => x.Id == data.UserModified)?.FirstOrDefault().Username;

            }
            return data;
        }

        public SysAuthorizationModel GetAuthorizationById(int id)
        {
            var detail = GetById(id);
            if (detail == null) return null;
            ICurrentUser _currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.sysAuthorize);

            var permissionRangeWrite = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Write);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Delete);
            detail.Permission = new PermissionAllowBase
            {
                AllowUpdate = GetPermissionToAction(detail, permissionRangeWrite, _currentUser) != 403 ? true : false,
                AllowDelete = GetPermissionToAction(detail, permissionRangeDelete, _currentUser) != 403 ? true : false
            };
            return detail;
        }

        private int GetPermissionToAction(SysAuthorizationModel model, PermissionRange permissionRange, ICurrentUser currentUser)
        {
            int code = 403;
            switch (permissionRange)
            {
                case PermissionRange.All:
                    code = 200;
                    break;
                case PermissionRange.Owner:
                    if (model.UserCreated == currentUser.UserID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId == currentUser.GroupId
                        && model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
            }
            return code;
        }

        public int CheckDetailPermission(int id)
        {
            var detail = GetById(id);
            ICurrentUser _currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.sysAuthorize);
            var permissionRangeDetail = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Detail);
            int code = GetPermissionToAction(detail, permissionRangeDetail, _currentUser);
            return code;
        }

        public int CheckDeletePermission(int id)
        {
            var detail = GetById(id);
            ICurrentUser _currentUser = PermissionExtention.GetUserMenuPermission(currentUser, Menu.sysAuthorize);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(_currentUser.UserMenuPermission.Delete);
            int code = GetPermissionToAction(detail, permissionRangeDelete, _currentUser);
            return code;
        }

        public HandleState Insert(SysAuthorizationModel model)
        {
            try
            {
                var userCurrent = currentUser.UserID;
                var today = DateTime.Now;
                var modelAdd = mapper.Map<SysAuthorization>(model);
                modelAdd.UserCreated = modelAdd.UserModified = userCurrent;
                modelAdd.DatetimeCreated = modelAdd.DatetimeModified = today;
                modelAdd.GroupId = currentUser.GroupId;
                modelAdd.DepartmentId = currentUser.DepartmentId;
                modelAdd.OfficeId = currentUser.OfficeID;
                modelAdd.CompanyId = currentUser.CompanyID;
                if (modelAdd.Active == false)
                {
                    modelAdd.InactiveOn = today;
                }
                var hs = DataContext.Add(modelAdd);
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
    }
}
