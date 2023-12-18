using System;
using AdvancedTask.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Dynamic;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class MovingContentCommand : ApprovalCommandBase
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(MovingContentCommand));
        private Injected<IContentLoader> _contentLoader;

        public override bool IsValid()
        {
            try
            {
                return _contentLoader.Service.Get<IContent>(JsonConvert.DeserializeObject<MovingPayLoad>(this.NewSettingsJson).Destination) != null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return false;
            }
        }
    }
}