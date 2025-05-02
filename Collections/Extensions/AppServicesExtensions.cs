using Collections.Data.Repositories;
using Collections.Data;
using Collections.Helpers;
using Collections.Services.Account;
using Collections.Services.Elastic;
using Collections.Services.Image;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Collections.Services.Admin.UserManagement;

namespace Collections.Extensions
{
    public static class AppServicesExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddControllersWithViews()
                .AddViewLocalization()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(ConnectionStringResolver.GetConnectionString(config)));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddSingleton<IImageService, ImageService>();

            services.AddSingleton<IElasticClientService, ElasticClientService>();

            services.AddSingleton<IAccountService, AccountService>();

            services.AddSingleton<IUserService, UserService>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            return services;
        }
    }
}
