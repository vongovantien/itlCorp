using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;


namespace eFMS.API.System.DL.Services
{
    public class SysPartnerAPIService : RepositoryBase<SysPartnerApi, SysPartnerAPIModel>, ISysPartnerAPIService
    {
       private readonly ICurrentUser currentUser;


        public SysPartnerAPIService(
            IMapper mapper,
            IContextBase<SysPartnerApi> repository,
            ICurrentUser ICurrentUser): base(repository, mapper)
        {
            currentUser = ICurrentUser;
        }

        public string GenerateAPIKey()
        {
            return "api12312312_!@31@_#-123";
        }
    }
}
