using EPiServer.Data.Dynamic;
using EPiServer.Security;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    internal class SecuritySettingCommand : ApprovalCommandBase
    {
        public virtual SecuritySaveType SecuritySaveType { get; set; }
    }
}
