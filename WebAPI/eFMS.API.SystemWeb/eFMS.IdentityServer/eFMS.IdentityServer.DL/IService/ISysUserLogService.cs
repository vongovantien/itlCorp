﻿using eFMS.API.System.DL.Models;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.DL.IService
{
    public interface ISysUserLogService : IRepositoryBase<SysUserLog, SysUserLogModel>
    {
    }
}
