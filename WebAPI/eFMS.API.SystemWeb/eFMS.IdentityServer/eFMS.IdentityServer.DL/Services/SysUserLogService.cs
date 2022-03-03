using AutoMapper;
using eFMS.API.System.DL.Models;
using eFMS.IdentityServer.DL.Helpers;
using eFMS.IdentityServer.DL.Infrastructure;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text;

namespace eFMS.IdentityServer.DL.Services
{
    public class SysUserLogService : RepositoryBase<SysUserLog, SysUserLogModel>, ISysUserLogService
    {
        private readonly LDAPConfig ldap;

        public SysUserLogService(IContextBase<SysUserLog> repository,
            IMapper mapper,
            IOptions<LDAPConfig> ldapConfig
            ) : base(repository, mapper)
        {
            ldap = ldapConfig.Value;
        }


        public SearchResult GetLDAPInfo(List<string> usernames, out List<string> userInactive)
        {
            SearchResult ldapInfo = null;
            LdapAuthentication Ldap = new LdapAuthentication();
            Ldap.Path = ldap.LdapPaths[0];
            userInactive = new List<string>();
            foreach (var username in usernames)
            {
                ldapInfo = Ldap.GetLDAPInfo(ldap.Domain, username);

                if(ldapInfo == null)
                {
                    userInactive.Add(username);
                }
            }

            return ldapInfo;
        }
    }
}
