﻿using AutoMapper;
using eFMS.API.Common.Globals;
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
    public class SysPermissionSampleGeneralService : RepositoryBase<SysPermissionSampleGeneral, SysPermissionSampleGeneralModel>, 
        ISysPermissionSampleGeneralService
    {
        private readonly IContextBase<SysMenu> menuRepository;
        public SysPermissionSampleGeneralService(IContextBase<SysPermissionSampleGeneral> repository, 
            IMapper mapper,
            IContextBase<SysMenu> menuRepo) : base(repository, mapper)
        {
            menuRepository = menuRepo;
        }

        public List<SysPermissionSampleGeneralViewModel> GetBy(Guid? permissionId)
        {
            List<SysPermissionSampleGeneralViewModel> results = new List<SysPermissionSampleGeneralViewModel>();
            var menus = menuRepository.Get().ToList();
            var modules = menus.Where(x => string.IsNullOrEmpty(x.ParentId)).OrderBy(x => x.NameEn);
            var permissionDetails = DataContext.Get(x => x.PermissionId == permissionId).ToList();
            if (modules == null) return results;
            foreach(var module in modules)
            {
                var item = new SysPermissionSampleGeneralViewModel
                {
                    ModuleName = module.NameEn,
                    ModuleID = module.Id,
                    PermissionID = permissionId == null ? Guid.Empty : (Guid)permissionId
                };
                var functions = menus.Where(x => x.ParentId == module.Id);
                var listPerDetails = new List<SysPermissionSampleGeneralModel>();
                if(functions != null)
                {
                    foreach (var function in functions)
                    {
                        var detail = permissionDetails.FirstOrDefault(x => x.MenuId == function.Id);
                        var perDetail = new SysPermissionSampleGeneralModel {
                                    MenuId = function.Id,
                                    MenuName = function.NameEn,
                                    PermissionId = permissionId == null ? Guid.Empty : (Guid)permissionId
                        };
                        if(detail != null)
                        {
                            perDetail.Id = detail.Id;
                            perDetail.Access = detail.Access;
                            perDetail.Detail = detail.Detail;
                            perDetail.Write = detail.Write;
                            perDetail.Delete = detail.Delete;
                            perDetail.List = detail.List;
                            perDetail.Import = detail.Import;
                            perDetail.Export = detail.Export;
                        }
                        else
                        {
                            perDetail.Access = false;
                            perDetail.Detail = Constants.PERMISSION_RANGE_OWNER;
                            perDetail.Write = Constants.PERMISSION_RANGE_OWNER;
                            perDetail.Delete = Constants.PERMISSION_RANGE_OWNER;
                            perDetail.List = Constants.PERMISSION_RANGE_OWNER;
                            perDetail.Import = false;
                            perDetail.Export = false;
                        }
                        listPerDetails.Add(perDetail);
                    }
                }
                item.SysPermissionGenerals = listPerDetails;
                if(item.SysPermissionGenerals.Count() > 0)
                {
                    results.Add(item);
                }
            }
            return results;
        }
    }
}
