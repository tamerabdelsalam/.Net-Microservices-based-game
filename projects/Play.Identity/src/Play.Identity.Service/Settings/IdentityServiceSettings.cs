using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;

namespace Play.Identity.Service.Settings;

public class IdentityServiceSettings
{
     public IReadOnlyCollection<ApiScope> ApiScopes { get; set; } = Array.Empty<ApiScope>();
     public IReadOnlyCollection<Client> Clients { get; init; }
     public IReadOnlyCollection<IdentityResource> IdentityResources =>
            [
                 new IdentityResources.OpenId(),
                 new IdentityResources.Profile()
            ];
}
