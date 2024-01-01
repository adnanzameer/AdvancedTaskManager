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
    internal class MovingContentCommand : ApprovalCommandBase
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(MovingContentCommand));
        private Injected<IContentLoader> _contentLoader;

        public override bool IsValid()
        {
            try
            {
                return _contentLoader.Service.Get<IContent>(JsonConvert.DeserializeObject<MovingPayLoad>(this.NewSettingsJson).Destination) != null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message,ex);
                return false;
            }
        }
    }
}