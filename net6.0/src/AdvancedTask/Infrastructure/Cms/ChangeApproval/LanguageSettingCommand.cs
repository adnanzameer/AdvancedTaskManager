using EPiServer.Data.Dynamic;

namespace AdvancedTask.Infrastructure.Cms.ChangeApproval
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class LanguageSettingCommand : ApprovalCommandBase
    {
    }
}
