using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;

namespace Advanced.Task.Manager.Infrastructure.Cms.ChangeApproval
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
    public class ChangeApprovalDynamicDataStoreFactory
    {
        public DynamicDataStore GetStore(string name)
        {
            return DynamicDataStoreFactory.Instance.GetStore(name);
        }
    }
}
