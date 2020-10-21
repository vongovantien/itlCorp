using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.IdentityServer
{
    public class Config
    {
        public static IEnumerable<Client> GetClients(string[] originUris, string[] redirectUris, int tokenLifetime, int slientRefreshToken)
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
                    RedirectUris = redirectUris,
                    AllowedCorsOrigins= originUris,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes =
                    {
                        "openid", "profile", "offline_access", "efms_api"
                    },
                },
                new Client
                {
                    ClientId = "Bravo",
                    ClientName = "Bravo Client API",
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
                    RedirectUris = redirectUris,
                    AllowedCorsOrigins= originUris,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes =
                    {
                        "openid", "profile", "offline_access", "efms_api"
                    },
                },

            };
        }

        //Defining the InMemory API's
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("efms_api", "eFMS D&T API")
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
                    Description = "eFMS API",
                    DisplayName = "eFMS API",
                    UserClaims =
                    {
                        "userId","workplaceId","userName","email"
                    }
                }
            };
        }
    }
}
