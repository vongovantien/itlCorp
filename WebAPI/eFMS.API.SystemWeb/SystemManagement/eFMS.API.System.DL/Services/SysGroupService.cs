using AutoMapper;
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
    public class SysGroupService : RepositoryBase<SysGroup, SysGroupModel>, ISysGroupService
    {
        public SysGroupService(IContextBase<SysGroup> repository, IMapper mapper) : base(repository, mapper)
        {
            SetChildren<SysUserGroup>("Id", "GroupId");
        }
    }
}
