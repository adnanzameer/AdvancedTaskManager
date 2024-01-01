using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Notification;
using EPiServer.Security;
using EPiServer.Shell.Profile.Internal;

namespace AdvancedTask.Helper
{
    internal class UIHelper
    {
        private readonly QueryableNotificationUserService _queryableNotificationUserService;
        private readonly ICurrentUiCulture _currentUiCulture;
        private readonly LocalizationService _localizationService;
        private readonly SecurityEntityProvider _securityEntityProvider;

        public UIHelper(
          QueryableNotificationUserService queryableNotificationUserService,
          ICurrentUiCulture currentUiCulture,
          LocalizationService localizationService, SecurityEntityProvider securityEntityProvider)
        {
            _queryableNotificationUserService = queryableNotificationUserService;
            _currentUiCulture = currentUiCulture;
            _localizationService = localizationService;
            _securityEntityProvider = securityEntityProvider;
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

        public List<string> GetUserRoles()
        {
            if (PrincipalAccessor.Current.Identity != null)
            {
                var name = PrincipalAccessor.Current.Identity.Name;
                var roles = _securityEntityProvider.GetRolesForUser(name);
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

                return descriptor.HasAccess(PrincipalAccessor.Current, AccessLevel.Publish);
            }
            return false;
        }
    }
}