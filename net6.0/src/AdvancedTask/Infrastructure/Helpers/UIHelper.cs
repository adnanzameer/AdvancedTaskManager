using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.Notification;
using EPiServer.Security;
using EPiServer.Shell.Profile.Internal;

namespace AdvancedTask.Infrastructure.Helpers
{
    public interface IUIHelper
    {
        string GetDisplayNameForUser(string senderUsername);
        bool CanUserPublish<T>(T content) where T : IContent;
        Task<List<string>> GetUserRoles();
    }

    public class UIHelper : IUIHelper
    {
        private readonly QueryableNotificationUserService _queryableNotificationUserService;
        private readonly ICurrentUiCulture _currentUiCulture;
        private readonly IPrincipalAccessor _principalAccessor;
        private readonly SecurityEntityProvider _securityEntityProvider;
        public UIHelper(QueryableNotificationUserService queryableNotificationUserService, ICurrentUiCulture currentUiCulture, IPrincipalAccessor principalAccessor, SecurityEntityProvider securityEntityProvider)
        {
            _queryableNotificationUserService = queryableNotificationUserService;
            _currentUiCulture = currentUiCulture;
            _principalAccessor = principalAccessor;
            _securityEntityProvider = securityEntityProvider;
        }

        public string GetDisplayNameForUser(string senderUsername)
        {
            if (string.IsNullOrEmpty(senderUsername))
                return null;
            var result = _queryableNotificationUserService.GetAsync(senderUsername).ConfigureAwait(false).GetAwaiter().GetResult();

            if (result == null)
                return null;

            if (_principalAccessor.Principal.Identity != null)
            {
                var name = _principalAccessor.Principal.Identity.Name;
                var culture = _currentUiCulture.Get(name);
                if (result.UserName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return "You";
            }

            return !string.IsNullOrEmpty(result.DisplayName) ? result.DisplayName : result.UserName;
        }

        public async Task<List<string>> GetUserRoles()
        {
            if (_principalAccessor.Principal.Identity != null)
            {
                var name = _principalAccessor.Principal.Identity.Name;
                var roles = await _securityEntityProvider.GetRolesForUserAsync(name);
                return roles.ToList();
            }

            return new List<string>();
        }


        public bool CanUserPublish<T>(T content) where T : IContent
        {
            var securedContent = content as ISecurable;
            if (securedContent != null)
            {
                var descriptor = securedContent.GetSecurityDescriptor();

                return descriptor.HasAccess(_principalAccessor.Principal, AccessLevel.Publish);
            }
            return false;
        }
    }
}