using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Open.Server
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var clients = await context.CZ_Clients.AsQueryable().ToListAsync();

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();



             foreach ( var client in clients)
            {
                if (await manager.FindByClientIdAsync(client.ClientId) is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = client.ClientId,
                        ClientSecret = client.ClientSecret,
                        DisplayName = "My client application",
                        Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.Password,
                        Permissions.Endpoints.Authorization,
                        Permissions.GrantTypes.ClientCredentials
                    }
                    });
                }
            }




            //if (await manager.FindByClientIdAsync("console") is null)
            //{
            //    await manager.CreateAsync(new OpenIddictApplicationDescriptor
            //    {
            //        ClientId = "console",
            //        ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207",
            //        DisplayName = "My client application",
            //        Permissions =
            //    {
            //        Permissions.Endpoints.Token,
            //        Permissions.GrantTypes.Password,
            //        Permissions.Endpoints.Authorization,
            //        Permissions.GrantTypes.ClientCredentials
            //    }
            //    });
            //}
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
