using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

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

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.MapControllers();

app.Run();

// Requis par WebApplicationFactory<Program> pour les tests d'intégration
public partial class Program { }
