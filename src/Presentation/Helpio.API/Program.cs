using Helpio.Ir.Application;
using Helpio.Ir.Infrastructure;
using Helpio.Ir.Infrastructure.Data;
using Helpio.Ir.Infrastructure.Data.Seeders;
using Helpio.Ir.API.Services;
using Helpio.Ir.API.Middleware;
using Helpio.Ir.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with API Key support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Helpio API", 
        Version = "v1",
        Description = "A comprehensive helpdesk and ticketing system API"
    });

    // Add API Key authentication to Swagger
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key needed to access the endpoints. X-API-Key: My_API_Key",
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "ApiKey",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Application and Infrastructure services
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(Helpio.Ir.Application.Mappings.MappingProfile).Assembly);

// Add API-specific services
builder.Services.AddHttpContextAccessor(); // Required for OrganizationContext
builder.Services.AddScoped<IOrganizationContext, OrganizationContext>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Helpio API V1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();

// Add API Key authentication middleware BEFORE authorization
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Seed sample data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        await SampleDataSeeder.SeedSampleDataAsync(context);
        
        // ????? API Keys ???? ???
        var apiKeys = await context.ApiKeys.ToListAsync();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("?? Available API Keys for testing:");
        foreach (var key in apiKeys)
        {
            logger.LogInformation("Organization: {OrganizationName} | API Key: {ApiKey}", 
                key.KeyName, key.KeyValue);
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

app.Run();
