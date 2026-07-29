using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Deunde.IdentityServer;

public static class Config
{
    public static class Policies
    {
        public const string Admin = "admin";
    }

    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource("role", "User role", new[] { JwtClaimTypes.Role })
        ];

    public static IEnumerable<ApiScope> ApiScopes => [
        new ApiScope("api"),
        new ApiScope("identity")
    ];

    public static IEnumerable<ApiResource> ApiResources => [
        new ApiResource("identity")
        {
            Scopes = { "identity" }
        },
        new ApiResource("api")
        {
            Scopes = { "api" },
            UserClaims = { "email", "role", JwtClaimTypes.Role }
        }
    ];

    public static IEnumerable<Client> Clients => [
        new Client
        {
            ClientId = "interactive.confidential",
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,
            AllowOfflineAccess = true,
            AlwaysIncludeUserClaimsInIdToken = true,
            UpdateAccessTokenClaimsOnRefresh = true,
            RedirectUris = { "https://localhost:5001/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },
            AllowedScopes = { "openid", "profile", "email", "api", "offline_access", "role" },
            
            RefreshTokenExpiration = TokenExpiration.Absolute,
            AbsoluteRefreshTokenLifetime = 2592000,
            AccessTokenLifetime = 3600
        },
        new Client
        {
            ClientId = "m2m",
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            AllowedScopes = {  "identity" }
        },
        new Client
        {
            ClientId = "postman-admin",
            ClientSecrets = { new Secret("secret".Sha256()) },
            
            RefreshTokenExpiration = TokenExpiration.Absolute,
            AbsoluteRefreshTokenLifetime = 2592000,
            AccessTokenLifetime = 3600,
            
            RefreshTokenUsage = TokenUsage.ReUse,
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
            
            AllowOfflineAccess = true,
            AlwaysIncludeUserClaimsInIdToken = true,
            UpdateAccessTokenClaimsOnRefresh = true,
            AllowedScopes = { "openid", "profile", "email", "api", "offline_access", "role" }
        },
        new Client
        {
            ClientId = "swagger-interactive",
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,
            AllowOfflineAccess = true,
            AlwaysIncludeUserClaimsInIdToken = true,
            UpdateAccessTokenClaimsOnRefresh = true,
            RequirePkce = true,
            RequireClientSecret = false,
            AllowedScopes = { "openid", "profile", "email", "api", "offline_access" },

            RedirectUris = { "https://localhost:5001/swagger/oauth2-redirect.html", "https://localhost:5001/swagger/oauth2-silent.html" },
            PostLogoutRedirectUris = { "https://localhost:5001/swagger/index.html" },
            AllowedCorsOrigins = { "https://localhost:5001", "https://localhost:7076" }
        },
    ];
}
