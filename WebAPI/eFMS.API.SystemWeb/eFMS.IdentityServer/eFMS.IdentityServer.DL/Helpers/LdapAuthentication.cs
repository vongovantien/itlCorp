using System;
using System.DirectoryServices;

namespace eFMS.IdentityServer.DL.Helpers
{
    public class LdapAuthentication
    {
        public string Path { get; set; }

        public LdapAuthentication()
        {

        }
        public LdapAuthentication(string path)
        {
            Path = path;
        }
        public bool IsAuthenticated(string domain, string username, string password)
        {
            try
            {
                string Individual = domain + @"\" + username;
                DirectoryEntry entry = new DirectoryEntry(Path, Individual, password);

                object obj = entry.NativeObject;

                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.AddRange(new string[] { "objectguid", "objectsid","samaccountname", "cn", "name", "displayname", "mail","mobile"
                ,"department", "telephonenumber", "title", "extensionattribute1" });
                SearchResult result = search.FindOne();

                if (result == null)
                {
                    return false;
                }
                //
                Path = result.Path;
                object _filterAttribute = (string)result.Properties["cn"][0];

                return true;
            }
            catch(Exception ex)
            {
            }
            return false;
        }

        public SearchResult GetNodeInfomation(string domain, string username, string password)
        {
            try
            {
                string Individual = domain + @"\" + username;
                DirectoryEntry entry = new DirectoryEntry(Path, Individual, password, AuthenticationTypes.Secure);

                object obj = entry.NativeObject;
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.AddRange(new string[] { "objectguid", "objectsid","samaccountname", "cn", "name", "displayname", "mail","mobile"
                ,"department", "telephonenumber", "title", "extensionattribute1" });
                SearchResult result = search.FindOne();

                return result;
            }
            catch(Exception ex)
            {
            }
            return null;
        }

        public SearchResult GetLDAPInfo(string domain, string username)
        {
            try
            {
                string Individual = domain + @"\" + username;
                DirectoryEntry entry = new DirectoryEntry(Path);

                object obj = entry.NativeObject;
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + username + ")";
                search.PropertiesToLoad.AddRange(new string[] { "objectguid", "objectsid","samaccountname", "cn", "name", "displayname", "mail","mobile"
                ,"department", "telephonenumber", "title", "extensionattribute1" });
                SearchResult result = search.FindOne();

                return result;
            }
            catch (Exception ex)
            {
            }
            return null;
        }
    }
}
