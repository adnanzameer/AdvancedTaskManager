using System;
using System.Collections.Generic;
using System.Globalization;
using AdvancedTask.Business.AdvancedTask.Interface;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class ExpirationDateSettingCommand : ChangeApprovalCommandBase, ICultureSpecificApprovalCommand
    {
        private Injected<IContentLoader> _contentLoader;
        public virtual string AppliedOnLanguageBranch { get; set; }

        public virtual List<int> AffectedVersions { get; set; }

        public override bool IsValid()
        {
            try
            {
                var dictionary = JsonConvert.DeserializeObject<IDictionary<string, object>>(this.NewSettingsJson);
                var versionable = (string.IsNullOrEmpty(this.AppliedOnLanguageBranch) ? _contentLoader.Service.Get<IContent>(this.AppliedOnContentLink) : _contentLoader.Service.Get<IContent>(this.AppliedOnContentLink, new CultureInfo(this.AppliedOnLanguageBranch))) as IVersionable;
                object obj;
                if (dictionary.TryGetValue("PageStopPublish", out obj))
                {
                    var nullable = obj as DateTime?;
                    if (nullable.HasValue)
                    {
                        var startPublish = versionable.StartPublish;
                        if (startPublish.HasValue)
                        {
                            var dateTime1 = nullable.Value;
                            ref var local2 = ref dateTime1;
                            startPublish = versionable.StartPublish;
                            if (startPublish != null)
                            {
                                var dateTime2 = startPublish.Value;
                                if (local2.CompareTo(dateTime2) < 0)
                                    return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
