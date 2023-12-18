using EPiServer.Data.Dynamic;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class LanguageSettingCommand : ApprovalCommandBase
    {
    }
}
