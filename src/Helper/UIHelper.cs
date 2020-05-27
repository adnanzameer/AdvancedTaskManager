using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Notification;
using EPiServer.Security;
using EPiServer.Shell.Profile.Internal;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AdvancedTask.Helper
{
    public class UIHelper
    {
        private readonly QueryableNotificationUserService _queryableNotificationUserService;
        private readonly ICurrentUiCulture _currentUiCulture;
        private readonly LocalizationService _localizationService;

        public UIHelper(
          QueryableNotificationUserService queryableNotificationUserService,
          ICurrentUiCulture currentUiCulture,
          LocalizationService localizationService)
        {
            _queryableNotificationUserService = queryableNotificationUserService;
            _currentUiCulture = currentUiCulture;
            _localizationService = localizationService;
        }

        public string GetDisplayNameForUser(string senderUsername)
        {
            if (string.IsNullOrEmpty(senderUsername))
                return null;
            var result = this._queryableNotificationUserService.GetAsync(senderUsername).ConfigureAwait(false).GetAwaiter().GetResult();
            if (result == null)
                return null;
            var name = PrincipalInfo.CurrentPrincipal.Identity.Name;
            var culture = _currentUiCulture.Get(name);
            if (result.UserName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return _localizationService.GetStringByCulture("/gadget/tasks/yousubject", culture);
            return !string.IsNullOrEmpty(result.DisplayName) ? result.DisplayName : result.UserName;
        }

        public async Task<bool> CanUserPublish<T>(T content) where T : IContent
        {
            IList<string> roles;
            using (var store = new UserStore<ApplicationUser>(new ApplicationDbContext<ApplicationUser>("EPiServerDB")))
            {
                var user = await store.FindByNameAsync(PrincipalInfo.CurrentPrincipal.Identity.Name);
                roles = await GetUserToRoles(store, user);
            }
            return RoleHasAccess(content, roles.ToArray(), AccessLevel.Publish);
        }

        private bool RoleHasAccess<T>(T content, string[] roles, AccessLevel accessLevel) where T : IContent
        {
            var securedContent = content as ISecurable;
            if (securedContent != null)
            {
                var descriptor = securedContent.GetSecurityDescriptor();
                var identity = new GenericIdentity("doesn't matter");
                var principal = new GenericPrincipal(identity, roles);
                return descriptor.HasAccess(principal, accessLevel);
            }
            return false;
        }

        private async Task<IList<string>> GetUserToRoles(UserStore<ApplicationUser> store, ApplicationUser user)
        {
            IUserRoleStore<ApplicationUser, string> userRoleStore = store;
            using (new RoleStore<IdentityRole>(new ApplicationDbContext<ApplicationUser>("EPiServerDB")))
            {
                return await userRoleStore.GetRolesAsync(user);
            }
        }
    }
}