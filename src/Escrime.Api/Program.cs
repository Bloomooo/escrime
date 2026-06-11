using System.Text.Json.Serialization;
using Escrime.Api.Data;
using Escrime.Api.Services;
using Escrime.Domain;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<EscrimeDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Escrime")));

// Domaine : sans état → singletons ; repository lié au DbContext → scoped
builder.Services.AddSingleton<ScoreCalculator>();
builder.Services.AddSingleton<TournamentRanking>();
builder.Services.AddSingleton<INotificationService, LoggingNotificationService>();
builder.Services.AddScoped<IPlayerRepository, EfPlayerRepository>();
builder.Services.AddScoped<TournamentService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlDoc = Path.Combine(AppContext.BaseDirectory, "Escrime.Api.xml");
    if (File.Exists(xmlDoc))
        options.IncludeXmlComments(xmlDoc);
});

// CORS ouvert pour le développement du front (à restreindre en production)
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Migration automatique au démarrage (sauf en tests : SQLite in-memory + EnsureCreated)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<EscrimeDbContext>().Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.MapControllers();

app.Run();

// Requis par WebApplicationFactory<Program> pour les tests d'intégration
public partial class Program { }
