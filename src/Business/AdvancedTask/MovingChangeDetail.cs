using System;
using System.Collections.Generic;
using AdvancedTask.Business.AdvancedTask.Interface;
using AdvancedTask.Business.AdvancedTask.Mapper;
using AdvancedTask.Models;
using EPiServer;
using EPiServer.Cms.Shell.Service.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Security;
using Newtonsoft.Json;

namespace AdvancedTask.Business.AdvancedTask
{
    public class MovingChangeDetail
    {
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
                return null;
            var contentChangeDetailsList = new List<IContentChangeDetails>();
            try
            {
                var movingPayLoad1 = JsonConvert.DeserializeObject<MovingPayLoad>(byCommandId.CurrentSettingsJson);
                var movingPayLoad2 = JsonConvert.DeserializeObject<MovingPayLoad>(byCommandId.NewSettingsJson);
                var content1 = _contentLoaderService.Get<IContent>(movingPayLoad1.Destination, AccessLevel.Read);
                var content2 = _contentLoaderService.Get<IContent>(movingPayLoad2.Destination, AccessLevel.Read);
                contentChangeDetailsList.Add((IContentChangeDetails)new ContentChangeDetails()
                {
                    Name = _localizationService.GetString(string.Format("{0}/path", (object)_baseLanguagePath)),
                    OldValue = (object)content1?.ContentLink,
                    NewValue = (object)content2?.ContentLink
                });
            }
            catch (Exception ex)
            {

            }
            return contentChangeDetailsList;
        }

        #endregion
    }
}
