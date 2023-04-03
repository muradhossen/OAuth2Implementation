using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Open.Server;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace AuthorizationServer.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private Dictionary<string, string[]> CZ_Client_Roles;
        private Dictionary<string, string> CZ_Users;

        public AuthorizationController(ApplicationDbContext db)
        {
            _db = db;

            CZ_Client_Roles = new Dictionary<string, string[]>();
            SetClientRoles(CZ_Client_Roles);

            CZ_Users = new Dictionary<string, string>();
            SetUsers(CZ_Users);
        }



        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
            
            // Retrieve the user principal stored in the authentication cookie.
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // If the user principal can't be extracted, redirect the user to the login page.
            if (!result.Succeeded)
            {
                return Challenge(
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    });
            }

            // Create a new claims principal
            var claims = new List<Claim>
            {
                // 'subject' claim which is required
                new Claim(OpenIddictConstants.Claims.Subject, result.Principal.Identity.Name),
                new Claim("some claim", "some value").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                new Claim(OpenIddictConstants.Claims.Email, "some@email").SetDestinations(OpenIddictConstants.Destinations.IdentityToken)
            };

            var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Set requested scopes (this is not done automatically)
            claimsPrincipal.SetScopes(request.GetScopes());

            // Signing in with the OpenIddict authentiction scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        
        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            ClaimsPrincipal claimsPrincipal;

            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Subject (sub) is a required field, we use the client id as the subject identifier here.
                identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

                var clientRoles = CZ_Client_Roles.FirstOrDefault(x => x.Key == request.ClientId).Value;

                foreach (var role in clientRoles)
                {
                    // Add some claim, don't forget to add destination otherwise it won't be added to the access token.
                    identity.AddClaim(OpenIddictConstants.Claims.Role, role, OpenIddictConstants.Destinations.AccessToken);
                }

                claimsPrincipal = new ClaimsPrincipal(identity);


                var scopes = request.GetScopes();

                claimsPrincipal.SetScopes(request.GetScopes());
            }

            else if (request.IsPasswordGrantType())
            {

                var userPassword = request.Password;
                var username = request.Username;

                if (!IsExistUser(username))
                {
                    return NotFound("User dosen't exist!");
                }


                if (!IsUserValid(username, userPassword))
                {
                    return BadRequest("Invalid username or password!");
                }


                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Subject (sub) is a required field, we use the client id as the subject identifier here.
                identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

                // Add some claim, don't forget to add destination otherwise it won't be added to the access token.
                identity.AddClaim(OpenIddictConstants.Claims.Role, "some-value", OpenIddictConstants.Destinations.AccessToken);

                claimsPrincipal = new ClaimsPrincipal(identity);

                claimsPrincipal.SetScopes(request.GetScopes());
            }

            //else if (request.IsAuthorizationCodeGrantType())
            //{
            //    // Retrieve the claims principal stored in the authorization code
            //    claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            //}
            
            else if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            }

            else
            {
                throw new InvalidOperationException("The specified grant type is not supported.");
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        
        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo")]
        public async Task<IActionResult> Userinfo()
        {
            var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;


            var scopes = claimsPrincipal.GetScopes();

            return Ok(new
            {
                Name = claimsPrincipal.GetClaim(OpenIddictConstants.Claims.Subject),
                Roles = claimsPrincipal.GetClaims("Role")               
            });
        }

        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("/api/users/validate")]
        public async Task<IActionResult> CheckIsUserAuthorization()
        {
            var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;


            return Ok(claimsPrincipal);
        }


        private void SetClientRoles(Dictionary<string, string[]> CZ_Client_Roles)
        {
            foreach (var client in _db.CZ_Clients)
            {
                CZ_Client_Roles.Add(client.ClientId, new[] { "Admin", "Hr", "Empoyee" });
            }
        }

        private void SetUsers(Dictionary<string, string> CZ_Users)
        {

            for (int i = 1; i < 10; i++)
            {
                CZ_Users.Add($"user{i}@example.com", "P@ssw0rd!");
            }
        }

        private bool IsExistUser(string username)
        {
            if (CZ_Users.FirstOrDefault(c=> c.Key == username).Value == null)
            {
                return false;
            }

            return true;
        }

        private bool IsUserValid(string username, string password)
        {
            if (CZ_Users.FirstOrDefault(c => c.Key == username && c.Value == password).Value == null)
            {
                return false;
            }

            return true;
        }

    }
}