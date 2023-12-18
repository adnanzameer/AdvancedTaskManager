using System;
using System.Linq;
using EPiServer.Authorization;
using EPiServer.Shell.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AdvancedTask.Infrastructure.Configuration
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Action<AuthorizationPolicyBuilder> DefaultPolicy = p =>
            p.RequireRole(Roles.Administrators, Roles.WebAdmins, Roles.CmsAdmins);

        public static IServiceCollection AddAdvancedTask(this IServiceCollection services)
        {
            return services.AddAdvancedTask(DefaultPolicy);
        }

        public static IServiceCollection AddAdvancedTask(
            this IServiceCollection services,
            Action<AuthorizationPolicyBuilder> configurePolicy)
        {
            //services.AddTransient<ITagService, TagService>();
            //services.AddTransient<ITagRepository, TagRepository>();
            //services.AddTransient<ITagEngine, TagEngine>();
            //services.AddSingleton<Func<ITagService>>(x => x.GetRequiredService<ITagService>);
            //services.AddSingleton<TagsInitializer>();
            //services.AddTransient<TagsExporter>();
            //services.AddSingleton<Func<TagsExporter>>(x => x.GetRequiredService<TagsExporter>);

            services.Configure<ProtectedModuleOptions>(
                pm =>
                {
                    if (!pm.Items.Any(i => i.Name.Equals(Constants.ModuleName, StringComparison.OrdinalIgnoreCase)))
                    {
                        pm.Items.Add(new() { Name = Constants.ModuleName });
                    }
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.PolicyName, configurePolicy);
            });

            return services;
        }
    }
}