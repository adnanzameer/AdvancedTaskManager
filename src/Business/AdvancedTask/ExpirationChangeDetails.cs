using System;
using System.Collections.Generic;
using AdvancedTask.Business.AdvancedTask.Interface;
using AdvancedTask.Models;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using Newtonsoft.Json;

namespace AdvancedTask.Business.AdvancedTask
{
    internal class ExpirationChangeDetails
    {
        private readonly LocalizationService _localizationService;
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(ExpirationChangeDetails));

        public ExpirationChangeDetails(LocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public IEnumerable<IContentChangeDetails> GetExpirationCommandChangeDetails(ChangeTaskViewModel model)
        {
            var interceptPropertyList = new List<string>() { "PageStopPublish", "PageArchiveLink" };
            var contentChangeDetailsList = new List<IContentChangeDetails>();

            try
            {
                var dictionary1 = JsonConvert.DeserializeObject<IDictionary<string, object>>(model.CurrentSettingsJson);
                var dictionary2 = JsonConvert.DeserializeObject<IDictionary<string, object>>(model.NewSettingsJson);
                foreach (var interceptProperty in interceptPropertyList)
                {
                    if (dictionary1.ContainsKey(interceptProperty))
                        contentChangeDetailsList.Add(new ContentChangeDetails()
                        {
                            Name = _localizationService.GetString("/gadget/changeapproval/expirationdatesettingcommand/" + interceptProperty.ToLowerInvariant()),
                            OldValue = dictionary1[interceptProperty],
                            NewValue = dictionary2[interceptProperty]
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            return contentChangeDetailsList;
        }
    }
}
