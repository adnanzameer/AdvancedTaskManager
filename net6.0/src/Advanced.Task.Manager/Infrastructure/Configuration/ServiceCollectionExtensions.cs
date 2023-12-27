using System;
using System.Linq;
using Advanced.Task.Manager.Infrastructure;
using Advanced.Task.Manager.Infrastructure.Cms;
using Advanced.Task.Manager.Infrastructure.Cms.ChangeApproval;
using Advanced.Task.Manager.Infrastructure.Helpers;
using Advanced.Task.Manager.Infrastructure.Mapper;
using EPiServer.Authorization;
using EPiServer.Shell.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Advanced.Task.Manager.Infrastructure.Configuration
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Action<AuthorizationPolicyBuilder> DefaultPolicy = p => p.RequireRole(Roles.Administrators, Roles.WebAdmins, Roles.CmsAdmins);

        public static IServiceCollection AddAdvancedTask(this IServiceCollection services, Action<AdvancedTaskManagerOptions> setupAction)
        {
            return services.AddAdvancedTask(setupAction, DefaultPolicy);
        }

        public static IServiceCollection AddAdvancedTask(this IServiceCollection services, Action<AdvancedTaskManagerOptions> setupAction, Action<AuthorizationPolicyBuilder> configurePolicy)
        {
            services.AddTransient<IUIHelper, UIHelper>();
            services.AddTransient<IChangeTaskHelper, ChangeTaskHelper>();
            services.AddTransient<IExpirationChangeDetails, ExpirationChangeDetails>();
            services.AddTransient<ICommandMetaDataRepository, DefaultCommandMetaDataRepository>();
            services.AddTransient<ILanguageChangeDetails, LanguageChangeDetails>();
            services.AddTransient<IApprovalCommandMapper, ApprovalCommandMapper>();
            services.AddTransient<IMovingChangeDetail, MovingChangeDetail>();
            services.AddTransient<ISecurityChangeDetail, SecurityChangeDetail>();
            services.AddTransient<IApprovalCommandService, ApprovalCommandService>();
            services.AddTransient<IApprovalCommandRepositoryBase, ApprovalCommandRepositoryBase>();
            services.AddTransient<INotificationHandler, NotificationHandler>();

            services.Configure<ProtectedModuleOptions>(
                pm =>
                {
                    if (!pm.Items.Any(i => i.Name.Equals(Constants.ModuleName, StringComparison.OrdinalIgnoreCase)))
                    {
                        pm.Items.Add(new() { Name = Constants.ModuleName });
                    }
                });

            var providerOptions = new AdvancedTaskManagerOptions();
            setupAction(providerOptions);

            services.AddOptions<AdvancedTaskManagerOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                setupAction(options);
                configuration.GetSection("AdvancedTaskManager").Bind(options);
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.PolicyName, configurePolicy);
            });

            return services;
        }
    }
}