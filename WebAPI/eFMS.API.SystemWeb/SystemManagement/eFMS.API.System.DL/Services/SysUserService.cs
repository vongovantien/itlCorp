﻿using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using eFMS.API.System.Service.ViewModels;
using ITL.NetCore.Connection;

namespace eFMS.API.System.DL.Services
{
    public class SysUserService : RepositoryBase<SysUser, SysUserModel>, ISysUserService
    {
        public SysUserService(IContextBase<SysUser> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<SysUserViewModel> GetAll()
        {
            var data = Get().ToList();
            var results = mapper.Map<List<SysUserViewModel>>(data);
            return results;
        }

        public List<vw_sysUser> GetUserWorkplace()
        {
            List<vw_sysUser> lvWorkspace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_sysUser>();
            return lvWorkspace;
        }
    }
}
