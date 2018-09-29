using AutoMapper;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using SystemManagement.DL.Models;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class SysBaseEnumDetailService : RepositoryBase<SysBaseEnumDetail, SysBaseEnumDetailModel>, ISysBaseEnumDetailService
    {
        public SysBaseEnumDetailService(IContextBase<SysBaseEnumDetail> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
