using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
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
using System.Text;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPotentialService : RepositoryBase<CatPotential, CatPotentialModel>, ICatPotentialService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUser> sysUserRepository;
        public CatPotentialService(IContextBase<CatPotential> repository,
            IMapper mapper,
            IStringLocalizer<CatalogueLanguageSub> localizer,
            ICurrentUser curUser,
            IContextBase<SysUser> sysUserRepo
            ) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = curUser;
            sysUserRepository = sysUserRepo;
        }
        public HandleState AddNew(CatPotentialEditModel model)
        {
            try
            {
                model.Potential.Id = Guid.NewGuid();
                model.Potential.UserCreated = currentUser.UserID;
                model.Potential.DatetimeCreated = DateTime.Now;
                model.Potential.GroupId = currentUser.GroupId;
                model.Potential.DepartmentId = currentUser.DepartmentId;
                model.Potential.OfficeId = currentUser.OfficeID;
                model.Potential.CompanyId = currentUser.CompanyID;
                HandleState hs = DataContext.Add(model.Potential, false);
                if (hs.Success)
                {
                    DataContext.SubmitChanges();
                    return hs;
                }
                else
                {
                    return new HandleState("Add Potential Failed!");
                }
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState Delete(Guid Id)
        {
            throw new NotImplementedException();
        }

        public CatPotentialModel GetDetail(Guid id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<CatPotentialModel> Paging(CatPotentialCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetQueryBy(criteria);
            //
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialIncoterm);
            PermissionRange permissionRangeList = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);

            data = QueryByPermission(data, permissionRangeList, _user);
            //format
            var result = FormatPotential(data);
            rowsCount = result.Count();
            if (page == 0)
            {
                page = 1;
                size = rowsCount;
            }
            return result.Skip((page - 1) * size).Take(size);
        }
        private IQueryable<CatPotentialModel> GetQueryBy(CatPotentialCriteria criteria)
        {
            Expression<Func<CatPotentialModel, bool>> query = q => true;
            if (!string.IsNullOrEmpty(criteria.NameEn))
            {
                query = (x => x.NameEn.Contains(criteria.NameEn));
            }
            if (!string.IsNullOrEmpty(criteria.NameLocal))
            {
                query = (x => x.NameLocal.Contains(criteria.NameLocal));
            }
            if (!string.IsNullOrEmpty(criteria.TaxCode))
            {
                query = (x => x.Taxcode.Contains(criteria.TaxCode));
            }
            if (!string.IsNullOrEmpty(criteria.Tel))
            {
                query = (x => x.Tel.Contains(criteria.Tel));
            }
            if (!string.IsNullOrEmpty(criteria.Email))
            {
                query = (x => x.Email.Contains(criteria.Email));
            }
            if (!string.IsNullOrEmpty(criteria.Address))
            {
                query = (x => x.Address.Contains(criteria.Address));
            }
            if (criteria.Active.HasValue)
            {
                query = (x => x.Active.Value == criteria.Active.Value);
            }
            if (!string.IsNullOrEmpty(criteria.Creator))
            {
                var listUserIdByUserName = sysUserRepository.Get(u => u.Username.Contains(criteria.Creator))
                    .Select(r => r.Id).ToList();
                query = (x => listUserIdByUserName.Any(val => val == x.UserCreated));
            }
            IQueryable<CatPotentialModel> dataQuery = Get(query);
            dataQuery = dataQuery?.OrderByDescending(x => x.DatetimeModified);
            return dataQuery;
        }
        private IQueryable<CatPotentialModel> FormatPotential(IQueryable<CatPotentialModel> dataQuery)
        {
            List<CatPotentialModel> listPotential = new List<CatPotentialModel>();
            if (dataQuery != null && dataQuery.Count() > 0)
            {
                foreach(var item in dataQuery)
                {
                    item.UserCreatedName = item.UserCreated == null ? null : sysUserRepository.Get(u => u.Id == item.UserCreated)
                        .FirstOrDefault().Username;
                    item.UserModifiedName = item.UserModified == null ? null : sysUserRepository.Get(u => u.Id == item.UserModified)
                    .FirstOrDefault().Username;
                    listPotential.Add(item);
                }
                IEnumerable<CatPotentialModel> potentials = listPotential.Select(r => new CatPotentialModel
                {
                    Id = r.Id,
                    Active = r.Active,
                    Address = r.Address,
                    CompanyId = r.CompanyId,
                    DatetimeModified = r.DatetimeModified,
                    DepartmentId = r.DepartmentId,
                    Email = r.Email,
                    GroupId = r.GroupId,
                    Margin = r.Margin,
                    NameEn = r.NameEn,
                    NameLocal = r.NameLocal,
                    OfficeId = r.OfficeId,
                    PotentialType = r.PotentialType,
                    Quotation = r.Quotation,
                    Taxcode = r.Taxcode,
                    Tel = r.Tel,
                    UserCreated = r.UserCreated,
                    UserCreatedName = r.UserCreatedName,
                    DatetimeCreated = r.DatetimeCreated,
                    UserModified = r.UserModified,
                    UserModifiedName = r.UserModifiedName,
                    
                });
                return potentials.AsQueryable();
            }
            return Enumerable.Empty<CatPotentialModel>().AsQueryable();
        }
        private IQueryable<CatPotentialModel> QueryByPermission(IQueryable<CatPotentialModel> data, 
            PermissionRange range, ICurrentUser currentUser)
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
        
        public IQueryable<CatPotentialModel> Query(CatPotentialCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public HandleState Update(CatPotentialEditModel model)
        {
            /*try
            {
                CatPotential catPotential = DataContext.Get(x => x.Id == model.Potential.Id)?.FirstOrDefault();
                HandleState hs = null;
                if (catPotential == null)
                {
                    hs = new HandleState("Not found Potential Customer");
                }
                model.Incoterm.GroupId = catIncoterm.GroupId;
                model.Incoterm.DepartmentId = catIncoterm.DepartmentId;
                model.Incoterm.OfficeId = catIncoterm.OfficeId;
                model.Incoterm.CompanyId = catIncoterm.CompanyId;


            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }*/
            throw new NotImplementedException();
        }

        IQueryable<CatPotentialModel> IPermissionBaseService<CatPotential, CatPotentialModel>.QueryByPermission(IQueryable<CatPotentialModel> data, PermissionRange range, ICurrentUser currentUser)
        {
            throw new NotImplementedException();
        }

        public bool CheckAllowPermissionAction(Guid id, PermissionRange range)
        {
            throw new NotImplementedException();
        }
    }
}
