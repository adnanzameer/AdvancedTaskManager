using System;
using System.Globalization;
using EPiServer.Framework.Localization;
using EPiServer.Notification;
using EPiServer.Security;
using EPiServer.Shell.Profile.Internal;

namespace AdvancedTask.Business.AdvancedTask.Helper
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

        internal string GetDisplayNameForUser(string senderUsername)
        {
            if (string.IsNullOrEmpty(senderUsername))
                return (string)null;
            INotificationUser result = this._queryableNotificationUserService.GetAsync(senderUsername).ConfigureAwait(false).GetAwaiter().GetResult();
            if (result == null)
                return (string)null;
            string name = PrincipalInfo.CurrentPrincipal.Identity.Name;
            CultureInfo culture = this._currentUiCulture.Get(name);
            if (result.UserName.Equals(name, StringComparison.OrdinalIgnoreCase))
                return this._localizationService.GetStringByCulture("/episerver/shared/text/yousubject", culture);
            return !string.IsNullOrEmpty(result.DisplayName) ? result.DisplayName : result.UserName;
        }
    }
}