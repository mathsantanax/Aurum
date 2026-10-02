using Aurum.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Infrastructure.Persistence.Seed
{
    public static class IdentityRoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<AurumRoles>>();

            string[] rolesNames =
            {
                "Admin",
                "User"
            };

            foreach (var roleName in rolesNames)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                    continue;
                var result = await roleManager.CreateAsync(
                            new AurumRoles
                            {
                                Name = roleName
                            });

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(x => x.Description));

                    throw new InvalidOperationException(
                        $"Não foi possível criar a role '{roleName}': {errors}");
                }
            }
        }
    }
}
