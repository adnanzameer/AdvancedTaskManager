using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Hybrid)]
    internal class ChangeApprovalDynamicDataStoreFactory
    {
        public DynamicDataStore GetStore(string name)
        {
            return DynamicDataStoreFactory.Instance.GetStore(name);
        }
    }
}
