using AutoCare_Club.Api.Constants;
using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace AutoCare_Club_Api.Database
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider services,
            IConfiguration configuration)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<RoleEntity>>();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<UserEntity>>();

            await CreateRolesAsync(roleManager);
            await CreateAdminAsync(
                userManager,
                configuration);
        }

        private static async Task CreateRolesAsync(
            RoleManager<RoleEntity> roleManager)
        {
            var roles = new Dictionary<string, string>
            {
                {
                    RolesConstant.Admin,
                    "Administrador del sistema"
                },
                {
                    RolesConstant.Customer,
                    "Cliente de AutoCare Club"
                },
                {
                    RolesConstant.Technician,
                    "Técnico de AutoCare Club"
                }
            };

            foreach (var role in roles)
            {
                if (await roleManager.RoleExistsAsync(role.Key))
                {
                    continue;
                }

                var result = await roleManager.CreateAsync(
                    new RoleEntity
                    {
                        Name = role.Key,
                        Descripcion = role.Value
                    });

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(
                            ", ",
                            result.Errors.Select(
                                error => error.Description)));
                }
            }
        }

        private static async Task CreateAdminAsync(
            UserManager<UserEntity> userManager,
            IConfiguration configuration)
        {
            var email =
                configuration["InitialAdmin:Email"];

            var password =
                configuration["InitialAdmin:Password"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "Falta configurar InitialAdmin.");
            }

            var admin =
                await userManager.FindByEmailAsync(email);

            if (admin is null)
            {
                admin = new UserEntity
                {
                    FirstName = "Administrador",
                    LastName = "AutoCare",
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult =
                    await userManager.CreateAsync(
                        admin,
                        password);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(
                            ", ",
                            createResult.Errors.Select(
                                error => error.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(
                admin,
                RolesConstant.Admin))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(
                        admin,
                        RolesConstant.Admin);

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(
                            ", ",
                            roleResult.Errors.Select(
                                error => error.Description)));
                }
            }
        }
    }
}