using Escrime.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Escrime.IntegrationTests;

/// <summary>
/// Héberge l'API en mémoire sur une base SQLite in-memory isolée.
/// La connexion doit rester ouverte : la base disparaît à sa fermeture.
/// Une factory par test = une base vierge par test (pas d'état partagé).
/// </summary>
public class EscrimeApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<EscrimeDbContext>));
            services.Remove(descriptor);

            _connection.Open();
            services.AddDbContext<EscrimeDbContext>(options => options.UseSqlite(_connection));

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<EscrimeDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
