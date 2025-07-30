using Collections.Data.Repositories;
using Collections.Data;
using Collections.Helpers;
using Collections.Services.Account;
using Collections.Services.Elastic;
using Collections.Services.Image;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Collections.Services.Admin.UserManagement;
using Collections.Services.Admin.Items;
using Collections.Services.Admin.Likes;
using Collections.Services.Admin.ProfileManagement;
using Collections.Services.Admin.Roles;
using Collections.Services.Admin.Tags;
using Collections.Services.Admin.FieldGroup;
using Collections.Services.Admin.Collections;

namespace Collections.Extensions
{
    public static class AppServicesExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

            services.AddControllersWithViews()
                .AddViewLocalization()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(ConnectionStringResolver.GetConnectionString(config)));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IElasticClientService, ElasticClientService>();
            services.AddScoped<IAccountService, AccountService>();

            services.AddScoped<ICollectionService, CollectionService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<ILikeService, LikeService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFieldGroupService, FieldGroupService>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            return services;
        }
    }
}
