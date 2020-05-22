using EPiServer.Data.Dynamic;
using EPiServer.Security;

namespace AdvancedTask.Business.AdvancedTask
{
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class SecuritySettingCommand : ChangeApprovalCommandBase
    {
        public virtual SecuritySaveType SecuritySaveType { get; set; }
    }
}
