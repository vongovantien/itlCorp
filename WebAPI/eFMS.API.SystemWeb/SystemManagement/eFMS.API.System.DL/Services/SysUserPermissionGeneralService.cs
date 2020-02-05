﻿using AutoMapper;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysUserPermissionGeneralService : RepositoryBase<SysUserPermissionGeneral, SysUserPermissionGeneralModel>, ISysUserPermissionGeneralService
    {
        private readonly IContextBase<SysMenu> menuRepository;
        public SysUserPermissionGeneralService(IContextBase<SysUserPermissionGeneral> repository, IMapper mapper,
            IContextBase<SysMenu> menuRepo) : base(repository, mapper)
        {
            menuRepository = menuRepo;
        }

        public List<SysUserPermissionGeneralViewModel> GetBy(Guid id)
        {
            List<SysUserPermissionGeneralViewModel> results = new List<SysUserPermissionGeneralViewModel>();
            var menus = menuRepository.Get().ToList();
            var modules = menus.Where(x => x.ParentId == null).OrderBy(x => x.NameEn);
            var permissionDetails = DataContext.Get(x => x.UserPermissionId == id).ToList();
            if (modules == null) return results;
            foreach (var module in modules)
            {
                var item = new SysUserPermissionGeneralViewModel
                {
                    ModuleName = module.NameEn,
                    ModuleID = module.Id,
                    UserPermissionId = id
                };
                var functions = menus.Where(x => x.ParentId == module.Id);
                var listPerDetails = new List<SysUserPermissionGeneralModel>();
                if (functions != null)
                {
                    foreach (var function in functions)
                    {
                        var detail = permissionDetails.FirstOrDefault(x => x.MenuId == function.Id);
                        var perDetail = new SysUserPermissionGeneralModel
                        {
                            MenuId = function.Id,
                            MenuName = function.NameEn,
                            UserPermissionId = id
                        };
                        if (detail != null)
                        {
                            perDetail.Id = detail.Id;
                            perDetail.Access = detail.Access;
                            perDetail.Detail = detail.Delete;
                            perDetail.Write = detail.Write;
                            perDetail.Delete = detail.Delete;
                            perDetail.List = detail.List;
                            perDetail.Import = detail.Import;
                            perDetail.Export = detail.Export;
                        }
                        else
                        {
                            perDetail.Access = false;
                            perDetail.Detail = Constants.Owner;
                            perDetail.Write = Constants.Owner;
                            perDetail.Delete = Constants.Owner;
                            perDetail.List = Constants.Owner;
                            perDetail.Import = false;
                            perDetail.Export = false;
                        }
                        listPerDetails.Add(perDetail);
                    }
                }
                item.SysUserPermissionGenerals = listPerDetails;
                results.Add(item);
            }
            return results;
        }
    }
}
