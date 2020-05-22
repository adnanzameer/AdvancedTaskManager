using System;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Dynamic;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class MovingContentCommand : ChangeApprovalCommandBase
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(MovingContentCommand));

        public override bool IsValid()
        {
            try
            {
                return ServiceLocator.Current.GetInstance<IContentLoader>().Get<IContent>(JsonConvert.DeserializeObject<MovingPayLoad>(this.NewSettingsJson).Destination) != null;
            }
            catch (Exception ex)
            {
                MovingContentCommand._logger.Error(ex.Message);
                return false;
            }
        }
    }
}