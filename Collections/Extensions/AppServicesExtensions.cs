using Collections.Services.Account;
using Collections.Services.Elastic;
using Collections.Services.Image;
using System.Text.Json.Serialization;

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
            
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            
            services.AddSingleton<IElasticClientService, ElasticClientService>();
            
            services.AddSingleton<IImageService, ImageService>();

            services.AddSingleton<IUserService, UserService>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            return services;
        }
    }
}
