using EPiServer.Data.Dynamic;

namespace AdvancedTask.Business.AdvancedTask
{
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class LanguageSettingCommand : ChangeApprovalCommandBase
    {
    }
}
