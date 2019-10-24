using AutoMapper;
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
    public class SysPermissionGeneralDetailService : RepositoryBase<SysPermissionGeneralDetail, SysPermissionGeneralDetailModel>, 
        ISysPermissionGeneralDetailService
    {
        private IContextBase<SysMenu> menuRepository;
        public SysPermissionGeneralDetailService(IContextBase<SysPermissionGeneralDetail> repository, 
            IMapper mapper,
            IContextBase<SysMenu> menuRepo) : base(repository, mapper)
        {
            menuRepository = menuRepo;
        }

        public List<SysPermissionGeneralDetailViewModel> GetBy(short permissionId)
        {
            List<SysPermissionGeneralDetailViewModel> results = new List<SysPermissionGeneralDetailViewModel>();
            var menus = menuRepository.Get().ToList();
            var modules = menus.Where(x => x.ParentId == null);
            if (modules == null) return results;
            foreach(var module in modules)
            {
                var item = new SysPermissionGeneralDetailViewModel
                {
                    ModuleName = module.NameEn,
                    ModuleID = module.Id
                };
                var functions = menus.Where(x => x.ParentId == module.Id);
            }
            return null;
        }
    }
}
