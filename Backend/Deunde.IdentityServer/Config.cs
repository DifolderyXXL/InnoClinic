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
            new IdentityResources.Phone(),
            new IdentityResources.Address(),
        ];

    public static IEnumerable<ApiScope> ApiScopes => [
        new ApiScope("api")
    ];

    public static IEnumerable<Client> Clients => [
        new Client
        {
            ClientId = "interactive.confidential",
            ClientSecrets = { new Secret("secret".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,
            AllowOfflineAccess = true,
            AlwaysIncludeUserClaimsInIdToken = true,
            RedirectUris = { "https://localhost:5001/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },
            AllowedScopes = { "openid", "profile", "email", "api", "offline_access" }
        }
    ];
}
