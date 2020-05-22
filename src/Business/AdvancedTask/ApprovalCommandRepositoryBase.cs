using System;
using System.Linq;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Business.AdvancedTask
{
    public static class ApprovalCommandRepositoryBaseTest<T> where T : ChangeApprovalCommandBase
    {
        public static  ChangeApprovalCommandBase GetByCommandId(Guid id, string type)
        {
            var store = ServiceLocator.Current.GetInstance<ChangeApprovalDynamicDataStoreFactory>().GetStore(type);

            var obj = store?.Items<T>().FirstOrDefault(x => x.Id.ExternalId == id);

            return obj;
        }
    }
}