﻿using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
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
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        public HandleState AddNew(CatPotential model)
        {
            try
            {
                model.Active = true;
                model.Id = Guid.NewGuid();

                model.UserCreated = currentUser.UserID;
                model.DatetimeCreated = DateTime.Now;
                model.UserModified = currentUser.UserID;
                model.DatetimeModified = DateTime.Now;

                model.GroupId = currentUser.GroupId;
                model.DepartmentId = currentUser.DepartmentId;
                model.OfficeId = currentUser.OfficeID;
                model.CompanyId = currentUser.CompanyID;

                HandleState hs = DataContext.Add(model);
                if (hs.Success)
                {
                    return hs;
                }
                else
                {
                    return new HandleState("Add Potential Failed!");
                }
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        public HandleState Delete(Guid Id)
        {
            try
            {
                var hs = new HandleState();
                hs = DataContext.Delete(x => x.Id == Id);
                if (!hs.Success)
                {
                    return new HandleState("Delete Potential Failed!");
                }
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }

        }
        public CatPotentialEditModel GetDetail(Guid id)
        {
            CatPotentialEditModel potentialDataViewModel = new CatPotentialEditModel();
            CatPotential potential = DataContext.Get(x => x.Id == id).FirstOrDefault();
            CatPotentialModel potentialModel = mapper.Map<CatPotentialModel>(potential);

            // Update permission
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialIncoterm);
            PermissionRange permissionRangeWrite = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Detail);

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = potential.UserCreated,
                CompanyId = potential.CompanyId,
                DepartmentId = potential.DepartmentId,
                OfficeId = potential.OfficeId,
                GroupId = potential.GroupId
            };
            potentialDataViewModel.Permission = new PermissionAllowBase
            {
                AllowUpdate = PermissionExtention.GetPermissionDetail(permissionRangeWrite, baseModel, currentUser),
            };
            SysUser userCreated = sysUserRepository.Get(u => u.Id == potentialModel.UserCreated).FirstOrDefault();
            SysUser userModified = sysUserRepository.Get(u => u.Id == potentialModel.UserModified).FirstOrDefault();

            potentialDataViewModel.Potential = new CatPotentialModel();
            potentialDataViewModel.Potential = potentialModel;
            potentialDataViewModel.Potential.UserCreatedName = userCreated != null ? userCreated.Username : "Admin";
            potentialDataViewModel.Potential.UserModifiedName = userModified != null ? userModified.Username : "Admin";

            return potentialDataViewModel;
        }
        public HandleState Update(CatPotential model)
        {
            try
            {
                CatPotential catPotential = DataContext.Get(x => x.Id == model.Id)?.FirstOrDefault();
                HandleState hs = null;
                if (catPotential == null)
                {
                    hs = new HandleState("Not found Potential Customer");
                }
                model.GroupId = catPotential.GroupId;
                model.DepartmentId = catPotential.DepartmentId;
                model.OfficeId = catPotential.OfficeId;
                model.CompanyId = catPotential.CompanyId;
                //
                model.UserCreated = catPotential.UserCreated;
                model.DatetimeCreated = catPotential.DatetimeCreated;


                model.UserModified = currentUser.UserID;
                model.DatetimeModified = DateTime.Now;

                hs = DataContext.Update(model, x => x.Id == model.Id);
               
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }

        }

        public IQueryable<CatPotentialModel> Paging(CatPotentialCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);
            //
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialPotential);
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
        public IQueryable<CatPotentialModel> Query(CatPotentialCriteria criteria)
        {
            Expression<Func<CatPotentialModel, bool>> query = q => true;

            if (!string.IsNullOrEmpty(criteria.All))
            {
                var listUserIdByUserName = sysUserRepository.Get(u => u.Username.Contains(criteria.All))
                    .Select(r => r.Id).ToList();
                query = (x => x.NameEn.Contains(criteria.All) || x.NameLocal.Contains(criteria.All)
                || x.Address.Contains(criteria.All) || x.Taxcode.Contains(criteria.Taxcode) || x.Tel.Contains(criteria.All)
                || x.Email.Contains(criteria.All) || x.PotentialType.Contains(criteria.All)
                || (x.Active.Value == true ? "Active" : "Inactive").Equals(criteria.All) || x.Margin.Value.ToString().Contains(criteria.All)
                || x.Quotation.Value.ToString().Contains(criteria.All)
                || listUserIdByUserName.Any(val => val == x.UserCreated));
            }
            else
            {
                query = (x => (x.NameEn ?? "").IndexOf(criteria.NameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0 &&
                               (x.NameLocal ?? "").IndexOf(criteria.NameLocal ?? "", StringComparison.OrdinalIgnoreCase) >= 0 &&
                               (x.Taxcode ?? "").IndexOf(criteria.Taxcode ?? "", StringComparison.OrdinalIgnoreCase) >= 0 &&
                               (x.Tel ?? "").IndexOf(criteria.Tel ?? "", StringComparison.OrdinalIgnoreCase) >= 0);
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
                foreach (var item in dataQuery)
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
        public IQueryable<CatPotentialModel> QueryByPermission(IQueryable<CatPotentialModel> data, PermissionRange range, ICurrentUser currentUser)
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
        public IQueryable<CatPotentialModel> QueryExport(CatPotentialCriteria criteria)
        {
            var data = Query(criteria);
            //
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialPotential);
            PermissionRange permissionRangeList = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);

            data = QueryByPermission(data, permissionRangeList, _user);
            //format
            var result = FormatPotential(data);
            return result;
        }
       
        public bool CheckAllowPermissionAction(Guid id, PermissionRange range)
        {
            CatPotential potential = DataContext.Get(o => o.Id == id).FirstOrDefault();
            if (potential == null)
            {
                return false;
            }

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = potential.UserCreated,
                CompanyId = potential.CompanyId,
                DepartmentId = potential.DepartmentId,
                OfficeId = potential.OfficeId,
                GroupId = potential.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, range, currentUser);

            if (code == 403) return false;

            return true;
        }
    }
}
