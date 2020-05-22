using System;
using System.Linq;
using System.Linq.Expressions;
using AdvancedTask.Business.AdvancedTask.Interface;
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
            this._changeApprovalDynamicDataStoreFactory = changeApprovalDynamicDataStoreFactory;
        }

        public CommandMetaData GetByApprovalId(int approvalId)
        {
            var store = this._changeApprovalDynamicDataStoreFactory.GetStore("EPiServer.ChangeApproval.Core.Internal.CommandMetaData");

            if (store == null)
                return (CommandMetaData)null;
            lock (DefaultCommandMetaDataRepository._lock)
            {
                return store.Items<CommandMetaData>().SingleOrDefault<CommandMetaData>((Expression<Func<CommandMetaData, bool>>)(command => command.ApprovalId == approvalId));
            }
        }
    }
}
