using System;
using System.Collections.Generic;
using System.Globalization;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace AdvancedTask.Business.AdvancedTask
{
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class ExpirationDateSettingCommand : ChangeApprovalCommandBase, ICultureSpecificApprovalCommand
    {
        public virtual string AppliedOnLanguageBranch { get; set; }

        public virtual List<int> AffectedVersions { get; set; }

        public override bool IsValid()
        {
            try
            {
                IDictionary<string, object> dictionary = JsonConvert.DeserializeObject<IDictionary<string, object>>(this.NewSettingsJson);
                IVersionable versionable = (string.IsNullOrEmpty(this.AppliedOnLanguageBranch) ? ServiceLocator.Current.GetInstance<IContentLoader>().Get<IContent>(this.AppliedOnContentLink) : ServiceLocator.Current.GetInstance<IContentLoader>().Get<IContent>(this.AppliedOnContentLink, new CultureInfo(this.AppliedOnLanguageBranch))) as IVersionable;
                object obj;
                if (dictionary.TryGetValue("PageStopPublish", out obj))
                {
                    DateTime? nullable = obj as DateTime?;
                    if (nullable.HasValue)
                    {
                        DateTime? startPublish = versionable.StartPublish;
                        if (startPublish.HasValue)
                        {
                            DateTime dateTime1 = nullable.Value;
                            ref DateTime local2 = ref dateTime1;
                            startPublish = versionable.StartPublish;
                            DateTime dateTime2 = startPublish.Value;
                            if (local2.CompareTo(dateTime2) < 0)
                                return false;
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
