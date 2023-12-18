using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Notification;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Profile.Internal;

namespace AdvancedTask.Helper
{
    public interface IUIHelper
    {
        string GetDisplayNameForUser(string senderUsername);
        Task<bool> CanUserPublish<T>(T content) where T : IContent;
    }

    public class UIHelper : IUIHelper
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
            var result = _queryableNotificationUserService.GetAsync(senderUsername).ConfigureAwait(false).GetAwaiter().GetResult();

            if (result == null)
                return null;

            if (PrincipalInfo.CurrentPrincipal.Identity != null)
            {
                var name = PrincipalInfo.CurrentPrincipal.Identity.Name;
                var culture = _currentUiCulture.Get(name);
                if (result.UserName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return _localizationService.GetStringByCulture("/gadget/tasks/yousubject", culture);
            }

            return !string.IsNullOrEmpty(result.DisplayName) ? result.DisplayName : result.UserName;
        }

        public async Task<bool> CanUserPublish<T>(T content) where T : IContent
        {
            IList<string> roles = new List<string>();
            //using (var store = new UserStore<ApplicationUser>(new ApplicationDbContext<ApplicationUser>("EPiServerDB")))
            //{
            // if (PrincipalInfo.CurrentPrincipal.Identity != null)
            // {
            //  var user = await store.FindByNameAsync(PrincipalInfo.CurrentPrincipal.Identity.Name);
            //  roles = await GetUserToRoles(store, user);
            // }
            //}
            var accessor = ServiceLocator.Current.GetInstance<IPrincipalAccessor>();
            
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


    }
}