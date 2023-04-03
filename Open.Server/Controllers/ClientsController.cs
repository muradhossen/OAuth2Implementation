using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using System.Threading.Tasks;
using System;
using Open.Server.DTO;

namespace Open.Server.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientsController : Controller
    {
        private readonly IOpenIddictApplicationManager _applicationManager;

        public ClientsController(IOpenIddictApplicationManager applicationManager)
        {
            _applicationManager = applicationManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                DisplayName = request.DisplayName,
                //RedirectUris = { new Uri(request.RedirectUri) },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                }
            };

            await _applicationManager.CreateAsync(descriptor);

            return Ok();
        }
    }
}
