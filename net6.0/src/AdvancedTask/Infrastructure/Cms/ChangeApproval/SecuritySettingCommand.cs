using EPiServer.Data.Dynamic;
using EPiServer.Security;

namespace AdvancedTask.Infrastructure.Cms.ChangeApproval
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class SecuritySettingCommand : ApprovalCommandBase
    {
        public virtual SecuritySaveType SecuritySaveType { get; set; }
    }
}
