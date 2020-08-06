using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Models;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Provider.Services.IService;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatIncotermService : RepositoryBase<CatIncoterm, CatIncotermModel>, ICatIncotermService, IPermissionBaseService<CatIncoterm, CatIncotermModel>
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatChargeIncoterm> catChargeIncotermRepository;
        private readonly IContextBase<SysUser> sysUserRepository;

        public CatIncotermService(IContextBase<CatIncoterm> repository,
            IMapper mapper,
            IStringLocalizer<CatalogueLanguageSub> localizer,
            IContextBase<CatChargeIncoterm> catChargeIncotermRepo,
            ICurrentUser curUser,
            IContextBase<SysUser> sysUserRepo
            ) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = curUser;
            catChargeIncotermRepository = catChargeIncotermRepo;
            sysUserRepository = sysUserRepo;

            SetChildren<CatChargeIncoterm>("Id", "IncotermId");
        }

        public HandleState AddNew(CatIncotermEditModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {

                model.Incoterm.Id = Guid.NewGuid();
                model.Incoterm.UserCreated = model.Incoterm.UserModified = currentUser.UserID;
                model.Incoterm.DatetimeCreated = model.Incoterm.DatetimeModified = DateTime.Now;
                model.Incoterm.GroupId = currentUser.GroupId;
                model.Incoterm.DepartmentId = currentUser.DepartmentId;
                model.Incoterm.OfficeId = currentUser.OfficeID;
                model.Incoterm.CompanyId = currentUser.CompanyID;
                try
                {
                    HandleState hs = DataContext.Add(model.Incoterm, false);
                    if(hs.Success)
                    {
                        List<CatChargeIncoterm> listCharge = new List<CatChargeIncoterm>();

                        if (model.Buyings.Count() > 0 )
                        {
                            listCharge.AddRange(model.Buyings);
                        }
                        if (model.Sellings.Count() > 0)
                        {
                           listCharge.AddRange(model.Sellings);
                        }

                        if(listCharge.Count() > 0)
                        {
                            foreach (CatChargeIncoterm item in listCharge)
                            {
                                item.Id = Guid.NewGuid();
                                item.IncotermId = model.Incoterm.Id;
                                item.UserCreated = item.UserModified = currentUser.UserID;
                                item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                                catChargeIncotermRepository.Add(item, false);
                            }
                        }
                        DataContext.SubmitChanges();
                        catChargeIncotermRepository.SubmitChanges();

                        trans.Commit();
                    }
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public HandleState Update(CatIncotermEditModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    CatIncoterm catIncoterm = DataContext.Get(x => x.Id == model.Incoterm.Id)?.FirstOrDefault();
                    HandleState hs = null;
                    if(catIncoterm == null)
                    {
                        hs = new HandleState("Not found incoterm");
                    }
                    model.Incoterm.GroupId = catIncoterm.GroupId;
                    model.Incoterm.DepartmentId = catIncoterm.DepartmentId;
                    model.Incoterm.OfficeId = catIncoterm.OfficeId;
                    model.Incoterm.CompanyId = catIncoterm.CompanyId;

                    model.Incoterm.UserModified = currentUser.UserID;
                    model.Incoterm.DatetimeModified = DateTime.Now;

                    hs = DataContext.Update(model.Incoterm, x => x.Id == model.Incoterm.Id, false);
                    if (hs.Success == false)
                    {
                        return hs;
                    }

                    List<CatChargeIncoterm> listCharge = new List<CatChargeIncoterm>();

                    if (model.Buyings.Count() > 0)
                    {
                        listCharge.AddRange(model.Buyings);
                    }
                    if (model.Sellings.Count() > 0)
                    {
                        listCharge.AddRange(model.Sellings);
                    }

                    if (listCharge.Count() > 0)
                    {
                        List<Guid> lstCatChargeIncotermNeedRemove = catChargeIncotermRepository.Where(x => x.IncotermId == model.Incoterm.Id).Select(t => t.Id).ToList();

                        foreach (Guid id in lstCatChargeIncotermNeedRemove)
                        {
                            catChargeIncotermRepository.Delete(x => x.Id == id, false);
                        }

                        foreach (CatChargeIncoterm item in listCharge)
                        {
                            item.Id = Guid.NewGuid();
                            item.IncotermId = model.Incoterm.Id;
                            item.UserCreated = item.UserModified = currentUser.UserID;
                            item.DatetimeCreated = item.DatetimeModified = DateTime.Now;

                            catChargeIncotermRepository.Add(item, false);
                        }
                    }
                    else
                    {
                        catChargeIncotermRepository.Delete(x => x.IncotermId == model.Incoterm.Id, false);
                    }
                    DataContext.SubmitChanges();
                    catChargeIncotermRepository.SubmitChanges();
                    trans.Commit();
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public bool CheckAllowDelete(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool CheckAllowViewDetail(Guid id)
        {

            throw new NotImplementedException();
        }

        public HandleState Delete(Guid Id)
        {
            throw new NotImplementedException();
        }

        public CatIncotermEditModel GetDetail(Guid id)
        {
            CatIncotermEditModel incotermDataViewModel = new CatIncotermEditModel();

            CatIncoterm incoterm = DataContext.Get(x => x.Id == id).FirstOrDefault();
            CatIncotermModel incotermModel = mapper.Map<CatIncotermModel>(incoterm);

            List<CatChargeIncoterm> listChargeDefault = catChargeIncotermRepository.Get(x => x.IncotermId == id).ToList();
            if(listChargeDefault.Count > 0)
            {
                incotermDataViewModel.Buyings = listChargeDefault.Where(x => x.Type.ToLower() == "BUY".ToLower()).ToList();
                incotermDataViewModel.Sellings = listChargeDefault.Where(x => x.Type.ToLower() == "SELL".ToLower()).ToList();
            }

            // Update permission
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialIncoterm);
            PermissionRange permissionRangeWrite = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = incoterm.UserCreated,
                CompanyId = incoterm.CompanyId,
                DepartmentId = incoterm.DepartmentId,
                OfficeId = incoterm.OfficeId,
                GroupId = incoterm.GroupId
            };
            incotermDataViewModel.Permission = new PermissionAllowBase
            {
                AllowUpdate = PermissionExtention.GetPermissionDetail(permissionRangeWrite, baseModel, currentUser),
            };
            SysUser userCreated = sysUserRepository.Get(u => u.Id == incotermModel.UserCreated).FirstOrDefault();
            SysUser userModified = sysUserRepository.Get(u => u.Id == incotermModel.UserModified).FirstOrDefault();

            incotermDataViewModel.Incoterm = new CatIncotermModel();
            incotermDataViewModel.Incoterm = incotermModel;
            incotermDataViewModel.Incoterm.UserCreatedName = userCreated != null ? userCreated.Username : "Admin";
            incotermDataViewModel.Incoterm.UserModified = userModified != null ? userModified.Username : "Admin";

            return incotermDataViewModel;
        }

        public IQueryable<CatIncotermModel> Paging(CatIncotermCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetQueryBy(criteria);
            //
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialIncoterm);
            PermissionRange permissionRangeList = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);

            data = QueryByPermission(data, permissionRangeList, _user);
            //format
            var result = FormatIncoterm(data);
            rowsCount = result.Count();
            if(page == 0)
            {
                page = 1;
                size = rowsCount;
            }
            return result.Skip((page - 1)*size).Take(size);
        }
        

        public IQueryable<CatIncotermModel> Query(CatIncotermCriteria criteria)
        {
            return GetQueryBy(criteria);
        }
        private IQueryable<CatIncotermModel> FormatIncoterm(IQueryable<CatIncotermModel> dataQuery)
        {
            List<CatIncotermModel> listIncoterm = new List<CatIncotermModel>();

            if (dataQuery != null && dataQuery.Count() > 0)
            {
                foreach (var item in dataQuery)
                {
                    
                    item.UserCreatedName = item.UserCreated == null ? null : sysUserRepository.Get(u => u.Id == item.UserCreated).FirstOrDefault().Username;
                    item.UserModifiedName = item.UserModified == null ? null : sysUserRepository.Get(u => u.Id == item.UserModified).FirstOrDefault().Username;
                    listIncoterm.Add(item);
                }

                IEnumerable<CatIncotermModel> d = listIncoterm.Select(x => new CatIncotermModel
                {
                    UserCreatedName = x.UserCreatedName,
                    UserModified = x.UserModified,
                    UserModifiedName = x.UserModifiedName,
                    UserCreated = x.UserCreated,
                    DatetimeCreated = x.DatetimeCreated,
                    DatetimeModified = x.DatetimeModified,
                    Id = x.Id,
                    Code = x.Code,
                    NameEn = x.NameEn,
                    Active = x.Active,
                    DescriptionEn = x.DescriptionEn,
                    DescriptionLocal = x.DescriptionLocal,
                    Service = x.Service,
                    NameLocal = x.NameLocal
                });
                return d.AsQueryable();
            }
            return Enumerable.Empty<CatIncotermModel>().AsQueryable();
        }
        private IQueryable<CatIncotermModel> GetQueryBy(CatIncotermCriteria criteria)
        {
            Expression<Func<CatIncotermModel, bool>> query;
            if (criteria.Service == null)
            {
                if(criteria.Active == null)
                {
                    query = (x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (criteria.Service != null && !string.IsNullOrEmpty(x.Service) ? criteria.Service.Any(val => x.Service.Contains(val.Trim(), StringComparison.OrdinalIgnoreCase))
                                        : "" == "")
                                        && (x.Active == true || x.Active == false));
                }
                else
                {
                    query = (x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (criteria.Service != null && !string.IsNullOrEmpty(x.Service) ? criteria.Service.Any(val => x.Service.Contains(val.Trim(), StringComparison.OrdinalIgnoreCase))
                                        : "" == "")
                                        && (x.Active == criteria.Active));
                }
            }
            else
            {
                if (criteria.Active == null)
                {
                    query = (x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (criteria.Service != null && !string.IsNullOrEmpty(x.Service) ? criteria.Service.Any(val => x.Service.Contains(val.Trim(), StringComparison.OrdinalIgnoreCase))
                                        : "" == "")
                                        && (x.Active == true || x.Active == false));
                }
                else
                {
                    query = (x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                        && (criteria.Service != null && !string.IsNullOrEmpty(x.Service) ? criteria.Service.Any(val => x.Service.Contains(val.Trim(), StringComparison.OrdinalIgnoreCase))
                                        : "" == "")
                                        && (x.Active == criteria.Active));
                }
            }

            IQueryable<CatIncotermModel> dataQuery = Get(query);
            dataQuery = dataQuery?.OrderByDescending(x => x.DatetimeModified);

            return dataQuery;
        }

        public IQueryable<CatIncotermModel> QueryByPermission(IQueryable<CatIncotermModel> data, PermissionRange range, ICurrentUser currentUser)
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
    }
}
