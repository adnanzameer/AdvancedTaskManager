using System;
using System.Linq;
using AdvancedTask.Business.AdvancedTask.Interface;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [ServiceConfiguration(ServiceType = typeof(ICommandMetaDataRepository))]
    internal class DefaultCommandMetaDataRepository : ICommandMetaDataRepository
    {
        private static readonly object Lock = new object();
        private readonly ChangeApprovalDynamicDataStoreFactory _changeApprovalDynamicDataStoreFactory;

        public DefaultCommandMetaDataRepository(
          ChangeApprovalDynamicDataStoreFactory changeApprovalDynamicDataStoreFactory)
        {
            _changeApprovalDynamicDataStoreFactory = changeApprovalDynamicDataStoreFactory;
        }

        public CommandMetaData GetByApprovalId(int approvalId)
        {
            var store = this._changeApprovalDynamicDataStoreFactory.GetStore("EPiServer.ChangeApproval.Core.Internal.CommandMetaData");

            if (store == null)
                return null;
            lock (Lock)
            {
                return store.Items<CommandMetaData>().SingleOrDefault(command => command.ApprovalId == approvalId);
            }
        }

        public CommandMetaData GetByCommandId(Guid commandId)
        {
            var store = _changeApprovalDynamicDataStoreFactory.GetStore("EPiServer.ChangeApproval.Core.Internal.CommandMetaData");
            if (store == null)
                return null;
            lock (Lock)
            {
                var commandMetaData = store.Items<CommandMetaData>().SingleOrDefault(command => command.CommandId == commandId);
                return commandMetaData;
            }
        }


        public Guid Save(CommandMetaData commandMetaData)
        {
            var store = _changeApprovalDynamicDataStoreFactory.GetStore("EPiServer.ChangeApproval.Core.Internal.CommandMetaData");
            if (store == null)
                return Guid.Empty;
            lock (Lock)
            {
                var identity = store.Save(commandMetaData);
                return identity == null ? Guid.Empty : identity.ExternalId;
            }
        }
    }
}
