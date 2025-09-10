using Helpio.Ir.Application;
using Helpio.Ir.Infrastructure;
using Helpio.Ir.API.Services;
using Helpio.Ir.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application and Infrastructure services
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(Helpio.Ir.Application.Mappings.MappingProfile).Assembly);

// Add API-specific services
builder.Services.AddHttpContextAccessor(); // Required for OrganizationContext
builder.Services.AddScoped<IOrganizationContext, OrganizationContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add API Key authentication middleware BEFORE authorization
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
