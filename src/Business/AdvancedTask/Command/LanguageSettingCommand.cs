﻿using EPiServer.Data.Dynamic;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    internal class LanguageSettingCommand : ApprovalCommandBase
    {
    }
}
