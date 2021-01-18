using System;
using System.Collections.Generic;
using AdvancedTask.Business.AdvancedTask.Interface;
using AdvancedTask.Models;
using EPiServer.Cms.Shell.Service.Internal;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.Security;
using Newtonsoft.Json;

namespace AdvancedTask.Business.AdvancedTask
{
    internal class MovingChangeDetail
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(MovingChangeDetail));
        private readonly LocalizationService _localizationService;
        private readonly string _baseLanguagePath = "/gadget/changeapproval/movingcontentcommand";

        private readonly ContentLoaderService _contentLoaderService;

        public MovingChangeDetail(ContentLoaderService contentLoaderService, LocalizationService localizationService)
        {
            _contentLoaderService = contentLoaderService;
            _localizationService = localizationService;
        }
        
        #region moving

        public IEnumerable<IContentChangeDetails> GetMovingChangeDetails(ChangeTaskViewModel byCommandId)
        {

            if (byCommandId == null)
                return new List<IContentChangeDetails>();

            var contentChangeDetailsList = new List<IContentChangeDetails>();
            try
            {
                var movingPayLoad1 = JsonConvert.DeserializeObject<MovingPayLoad>(byCommandId.CurrentSettingsJson);
                var movingPayLoad2 = JsonConvert.DeserializeObject<MovingPayLoad>(byCommandId.NewSettingsJson);
                var content1 = _contentLoaderService.Get<IContent>(movingPayLoad1.Destination, AccessLevel.Read);
                var content2 = _contentLoaderService.Get<IContent>(movingPayLoad2.Destination, AccessLevel.Read);
                contentChangeDetailsList.Add(new ContentChangeDetails()
                {
                    Name = _localizationService.GetString(string.Format("{0}/path", _baseLanguagePath)),
                    OldValue = content1?.Name,
                    NewValue = content2?.Name
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
            }
            return contentChangeDetailsList;
        }

        #endregion
    }
}
