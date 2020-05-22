using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Business.AdvancedTask
{
    [ServiceConfiguration(ServiceType = typeof(ICommandMetaDataRepository))]
    public class DefaultCommandMetaDataRepository : ICommandMetaDataRepository
    {
        private static object _lock = new object();
        private ChangeApprovalDynamicDataStoreFactory _changeApprovalDynamicDataStoreFactory;

        public DefaultCommandMetaDataRepository(
          ChangeApprovalDynamicDataStoreFactory changeApprovalDynamicDataStoreFactory)
        {
            this._changeApprovalDynamicDataStoreFactory = changeApprovalDynamicDataStoreFactory;
        }

        public CommandMetaData GetByApprovalId(int approvalId)
        {
            DynamicDataStore store = this._changeApprovalDynamicDataStoreFactory.GetStore("EPiServer.ChangeApproval.Core.Internal.CommandMetaData");

            if (store == null)
                return (CommandMetaData)null;
            lock (DefaultCommandMetaDataRepository._lock)
                return store.Items<CommandMetaData>().SingleOrDefault<CommandMetaData>((Expression<Func<CommandMetaData, bool>>)(command => command.ApprovalId == approvalId));
        }

        public CommandMetaData GetByCommandId(Guid commandId)
        {
            //DynamicDataStore store = this._changeApprovalDynamicDataStoreFactory.GetStore(typeof(CommandMetaData));
            DynamicDataStore store = this._changeApprovalDynamicDataStoreFactory.GetStore("EPiServer.ChangeApproval.Core.Internal.CommandMetaData");
            if (store == null)
                return (CommandMetaData)null;
            lock (DefaultCommandMetaDataRepository._lock)
            {
                CommandMetaData commandMetaData = store.Items<CommandMetaData>().SingleOrDefault<CommandMetaData>((Expression<Func<CommandMetaData, bool>>)(command => command.CommandId == commandId));
                return commandMetaData;
            }
        }

        public CommandMetaData GetByContentReference(ContentReference contentReference)
        {
            DynamicDataStore store = this._changeApprovalDynamicDataStoreFactory.GetStore("EPiServer.ChangeApproval.Core.Internal.CommandMetaData");
            if (store == null)
                return (CommandMetaData)null;
            lock (DefaultCommandMetaDataRepository._lock)
            {
                CommandMetaData commandMetaData = store.Items<CommandMetaData>().SingleOrDefault<CommandMetaData>((Expression<Func<CommandMetaData, bool>>)(command => command.AppliedOnContent == contentReference && (int)command.CommandStatus == 0));
                return commandMetaData;
            }
        }

        public IEnumerable<CommandMetaData> GetAllMetadataByContentReference(
          ContentReference contentReference)
        {
            DynamicDataStore store = this._changeApprovalDynamicDataStoreFactory.GetStore("EPiServer.ChangeApproval.Core.Internal.CommandMetaData");
            if (store == null)
                return Enumerable.Empty<CommandMetaData>();
            lock (DefaultCommandMetaDataRepository._lock)
                return (IEnumerable<CommandMetaData>)store.Items<CommandMetaData>().Where<CommandMetaData>((Expression<Func<CommandMetaData, bool>>)(command => command.AppliedOnContent == contentReference));
        }

    }
}
