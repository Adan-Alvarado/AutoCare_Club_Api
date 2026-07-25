using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Services.ServicesCatalog;
using AutoCare_Club_Api.Services.Users;
using Microsoft.EntityFrameworkCore;
using AutoCare_Club.Api.Services.Vehicle;
using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AutoCareDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

builder.Services
    .AddIdentity<UserEntity, RoleEntity>()
    .AddEntityFrameworkStores<AutoCareDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<
    IServiceCatalogService,
    ServiceCatalogService>();
builder.Services.AddTransient<IUserService, UserService>();


builder.Services.AddScoped<
    IVehicleService,
    VehicleService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
