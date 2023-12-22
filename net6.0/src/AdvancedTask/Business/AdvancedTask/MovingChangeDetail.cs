using System;
using System.Collections.Generic;
using AdvancedTask.Business.AdvancedTask.Interface;
using AdvancedTask.Models;
using EPiServer.Cms.Shell.Service.Internal;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Security;
using Newtonsoft.Json;

namespace AdvancedTask.Business.AdvancedTask
{
    public interface IMovingChangeDetail
    {
        IEnumerable<IContentChangeDetails> GetMovingChangeDetails(ChangeTaskViewModel byCommandId);
    }

    public class MovingChangeDetail : IMovingChangeDetail
    {
        private readonly ILogger _logger;

        private readonly ContentLoaderService _contentLoaderService;

        public MovingChangeDetail(ContentLoaderService contentLoaderService)
        {
            _contentLoaderService = contentLoaderService;
            _logger = LogManager.GetLogger(typeof(MovingChangeDetail));
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
                contentChangeDetailsList.Add(new ContentChangeDetails()
                {
                    Name = "Path",
                    OldValue = content1?.Name,
                    NewValue = content2?.Name
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
            return contentChangeDetailsList;
        }

        #endregion
    }
}
