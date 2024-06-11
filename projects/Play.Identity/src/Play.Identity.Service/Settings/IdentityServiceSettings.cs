using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;

namespace Play.Identity.Service.Settings;

public class IdentityServiceSettings
{
    public IReadOnlyCollection<ApiScope> ApiScopes { get; init; } = Array.Empty<ApiScope>();
    public IReadOnlyCollection<Client> Clients { get; init; } = Array.Empty<Client>();
    public IReadOnlyCollection<IdentityResource> IdentityResources =>
           new IdentityResource[]
           {
                new IdentityResources.OpenId()
           };
}
