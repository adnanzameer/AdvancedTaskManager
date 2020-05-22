using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Business.AdvancedTask
{
    [ServiceConfiguration(Lifecycle = ServiceInstanceScope.Hybrid)]
    public class ChangeApprovalDynamicDataStoreFactory
    {
        public DynamicDataStore GetStore(string name)
        {
            return DynamicDataStoreFactory.Instance.GetStore(name);
        }
    }
}
