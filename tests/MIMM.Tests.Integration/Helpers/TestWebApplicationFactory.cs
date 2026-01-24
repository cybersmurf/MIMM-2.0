using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MIMM.Backend.Data;

namespace MIMM.Tests.Integration.Helpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                // 64+ char secret to satisfy HS256 minimum key size (>= 256 bits)
                ["Jwt:Key"] = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                ["Jwt:Issuer"] = "MIMM.Tests",
                ["Jwt:Audience"] = "MIMM.Tests",
                ["Cors:AllowedOrigins:0"] = "http://localhost:5000"
            };
            config.AddInMemoryCollection(settings!);
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext registrations
            var dbContextOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextOptionsDescriptor != null)
            {
                services.Remove(dbContextOptionsDescriptor);
            }

            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Register InMemory EF Core with isolated internal service provider to avoid provider conflicts
            var efProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("MimmTestDb");
                options.UseInternalServiceProvider(efProvider);
            });
            // Override authentication to use test scheme reading X-Test-User-Id
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}
