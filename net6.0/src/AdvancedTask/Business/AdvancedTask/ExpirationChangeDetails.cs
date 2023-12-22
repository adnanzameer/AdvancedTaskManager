using System;
using System.Collections.Generic;
using AdvancedTask.Business.AdvancedTask.Interface;
using AdvancedTask.Models;
using EPiServer.Logging;
using Newtonsoft.Json;

namespace AdvancedTask.Business.AdvancedTask
{
    public interface IExpirationChangeDetails
    {
        IEnumerable<IContentChangeDetails> GetExpirationCommandChangeDetails(ChangeTaskViewModel model);
    }

    public class ExpirationChangeDetails : IExpirationChangeDetails
    {
        private readonly ILogger _logger;

        public ExpirationChangeDetails()
        {
            _logger = LogManager.GetLogger(typeof(ExpirationChangeDetails));
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
                            Name = GetExpirationDateSettingCommand(interceptProperty.ToLowerInvariant()),
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

        static string GetExpirationDateSettingCommand(string interceptProperty)
        {
            switch (interceptProperty.ToLowerInvariant())
            {
                case "pagearchivelink":
                    return "Archive to";
                case "pagestoppublish":
                    return "Expiration date";
                default:
                    return "[Location is no longer available]";
            }
        }
    }
}
