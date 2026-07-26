using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Services.ServicesCatalog;
using AutoCare_Club_Api.Services.Auth;
using AutoCare_Club_Api.Services.Roles;
using AutoCare_Club_Api.Services.Users;
using Microsoft.EntityFrameworkCore;
using AutoCare_Club.Api.Services.Vehicle;
using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Entities;
using Microsoft.AspNetCore.Identity;
using AutoCare_Club.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AutoCareDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));


builder.Services.AddScoped<
    IServiceCatalogService,
    ServiceCatalogService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IRoleService, RoleService>();


builder.Services.AddScoped<
    IVehicleService,
    VehicleService>();

builder.Services.AddCorsConfiguration(builder.Configuration);

builder.Services.AddAuthenticationConfig(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
