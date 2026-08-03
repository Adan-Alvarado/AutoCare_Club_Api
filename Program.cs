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
using AutoCare_Club_Api.Database;
using AutoCare_Club.Api.Services.Orders;
using Scalar.AspNetCore;
using AutoCare_Club_Api.Services.Schedules;
using AutoCare_Club_Api.Services.Appointments;
using AutoCare_Club.Api.Services.Payments;
using AutoCare_Club_Api.Services.Technicians;

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

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();

builder.Services.AddScoped<IAppointmentService, AppointmentService>();


builder.Services.AddScoped<
    IVehicleService,
    VehicleService>();

builder.Services.AddScoped<
    IOrderService,
    OrderService>();

builder.Services.AddScoped<
    IPaymentService,
    PaymentService>();

builder.Services.AddScoped<
    ITechnicianService,
    TechnicianService>();

builder.Services.AddCorsConfiguration(builder.Configuration);

builder.Services.AddAuthenticationConfig(builder.Configuration);


var app = builder.Build();

await DbInitializer.InitializeAsync(
    app.Services,
    app.Configuration);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
