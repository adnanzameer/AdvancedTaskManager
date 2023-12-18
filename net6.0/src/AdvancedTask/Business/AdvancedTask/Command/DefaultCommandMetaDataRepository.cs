using System;
using System.Linq;
using System.Linq.Expressions;
using AdvancedTask.Business.AdvancedTask.Interface;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [ServiceConfiguration(ServiceType = typeof(ICommandMetaDataRepository))]
    public class DefaultCommandMetaDataRepository : ICommandMetaDataRepository
    {
        private static readonly object _lock = new object();
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
                return (CommandMetaData)null;
            lock (_lock)
            {
                return store.Items<CommandMetaData>().SingleOrDefault<CommandMetaData>((Expression<Func<CommandMetaData, bool>>)(command => command.ApprovalId == approvalId));
            }
        }

        public CommandMetaData GetByCommandId(Guid commandId)
        {
            var store = this._changeApprovalDynamicDataStoreFactory.GetStore("EPiServer.ChangeApproval.Core.Internal.CommandMetaData");
            if (store == null)
                return (CommandMetaData)null;
            lock (_lock)
            {
                CommandMetaData commandMetaData = store.Items<CommandMetaData>().SingleOrDefault<CommandMetaData>((Expression<Func<CommandMetaData, bool>>)(command => command.CommandId == commandId));
                return commandMetaData;
            }
        }


        public Guid Save(CommandMetaData commandMetaData)
        {
            DynamicDataStore store = this._changeApprovalDynamicDataStoreFactory.GetStore("EPiServer.ChangeApproval.Core.Internal.CommandMetaData");
            if (store == null)
                return Guid.Empty;
            lock (_lock)
            {
                var identity = store.Save(commandMetaData);
                return identity == null ? Guid.Empty : identity.ExternalId;
            }
        }
    }
}
