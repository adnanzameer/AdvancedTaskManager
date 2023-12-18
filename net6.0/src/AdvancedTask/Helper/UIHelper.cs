using System;
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
        bool CanUserPublish<T>(T content) where T : IContent;
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

        public bool CanUserPublish<T>(T content) where T : IContent
        {
            var securedContent = content as ISecurable;
            if (securedContent != null)
            {
                var descriptor = securedContent.GetSecurityDescriptor();
                var accessor = ServiceLocator.Current.GetInstance<IPrincipalAccessor>();
                return descriptor.HasAccess(accessor.Principal, AccessLevel.Publish);
            }
            return false;
        }
    }
}