﻿using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysRoleService : RepositoryBase<SysRole, SysRoleModel>, ISysRoleService
    {
        public SysRoleService(IContextBase<SysRole> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
