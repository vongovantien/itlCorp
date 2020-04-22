using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.DL.Infrastructure
{
    public class LDAPConfig
    {
        public string[] LdapPaths { get; set; }
        public string Domain { get; set; }
    }
}
