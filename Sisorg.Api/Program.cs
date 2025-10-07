using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Sisorg.Api.Infrastructure;
using Sisorg.Api.Middleware;
using Sisorg.Api.Repositories;
using Sisorg.Api.Services;
using Pomelo.EntityFrameworkCore.MySql.Storage; 
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.PropertyNamingPolicy = null; 
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Default");
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 43));

    opt.UseMySql(cs, serverVersion, mySql =>
    {
        mySql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null); 
    });
});

builder.Services.AddCors(opt =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    opt.AddPolicy("Front", p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddScoped<IRegistryRepository, RegistryRepository>();
builder.Services.AddScoped<IRegistryService, RegistryService>();
builder.Services.AddSingleton<IFileParser, FileParser>(); 

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Front");
app.MapControllers();

app.Run();
