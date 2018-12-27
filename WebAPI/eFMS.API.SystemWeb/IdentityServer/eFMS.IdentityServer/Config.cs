using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer
{
    public class Config
    {
        public static IEnumerable<Client> GetClients(string[] redirectUris, int tokenLifetime, int slientRefreshToken)
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "eFMS",
                    ClientName = "eFMS Services",
                    AccessTokenLifetime =  tokenLifetime,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    RequireClientSecret = false,
                    RequireConsent = false,
                    AlwaysSendClientClaims = true,

                    AllowAccessTokensViaBrowser = true,
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowOfflineAccess = true,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    SlidingRefreshTokenLifetime = slientRefreshToken,
                    //RedirectUris = redirectUris,
                    RedirectUris = new List<string> {"http://localhost:4200/auth-callback", "http://localhost:4200/silent-refresh.html"},
                    PostLogoutRedirectUris = new List<string> {"http://localhost:4200/"},
                    AllowedCorsOrigins = new List<string> {"http://localhost:4200", "http://localhost:4300","http://test.efms.itlvn.com", "http://test.efms.itlvn.com/en/", "http://test.efms.itlvn.com/vi/", "http://test.api-efms.itlvn.com/Catalogue", "http://localhost:44361"},
                    AlwaysIncludeUserClaimsInIdToken = true,
                    
                    
                    AllowedScopes =
                    {
                        "openid", "profile", "offline_access", "efms_scope", "dnt_api"
                    },
                }
            };
        }

        //Defining the InMemory API's
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("dnt_api", "eFMS D&T API")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) }
                }
            };
        }

        //Support for OpenId connectivity scopes
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile() { Required = true },
                new IdentityResource()
                {
                    Name = "efms_scope",
                    Description = "eFMS D&T API",
                    DisplayName = "eFMS D&T API",
                    UserClaims =
                    {
                        "userId","workplaceId","userName","email"
                    }
                }
            };
        }
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "password"
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password"
                }
            };
        }
    }
}
