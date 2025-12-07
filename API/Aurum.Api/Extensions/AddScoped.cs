<<<<<<< HEAD
﻿using Aurum.Api.Service;
using Aurum.Application.AuthServices;
using Aurum.Application.Interfaces;
using Aurum.Application.Services;
=======
﻿using Aurum.Application.Services;
>>>>>>> 361a6b5a70ac4ce95e8202e8bab960055fb7d478
using Aurum.Domain.Interfaces;
using Aurum.Infrastructure.Repositories;

namespace Aurum.Api.Extensions
{
    public static class AddScoped
    {
        public static WebApplicationBuilder UseScoped(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IAuthService, TokenService>();

            //Registros

<<<<<<< HEAD
            // Usuário
=======
>>>>>>> 361a6b5a70ac4ce95e8202e8bab960055fb7d478
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddScoped<PrivateWalletService>();
            builder.Services.AddScoped<IPrivateWalletRepositories, PrivateWalletRepository>();

            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

            builder.Services.AddScoped<SharedWalletService>();
            builder.Services.AddScoped<ISharedWalletRepository, SharedWalletRepository>();

            return builder;
        }
    }
}
