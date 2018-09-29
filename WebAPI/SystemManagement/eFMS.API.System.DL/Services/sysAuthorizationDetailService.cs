using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AutoMapper;
using ITL.NetCore.Connection.EF;

using Microsoft.EntityFrameworkCore;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class SysAuthorizationDetailService : RepositoryBase<SysAuthorizationDetail, SysAuthorizationDetailModel>, ISysAuthorizationDetailService
    {
        public SysAuthorizationDetailService(IContextBase<SysAuthorizationDetail> repository, IMapper mapper) : base(repository, mapper)
        {
        }        
    }
}
