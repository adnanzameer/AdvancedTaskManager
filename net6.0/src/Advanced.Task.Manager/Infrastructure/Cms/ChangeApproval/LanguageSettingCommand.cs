using EPiServer.Data.Dynamic;

namespace Advanced.Task.Manager.Infrastructure.Cms.ChangeApproval
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class LanguageSettingCommand : ApprovalCommandBase
    {
    }
}
