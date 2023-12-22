﻿using System;
using System.Collections.Generic;
using System.Globalization;
using AdvancedTask.Business.AdvancedTask.Interface;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Dynamic;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class ExpirationDateSettingCommand : ApprovalCommandBase, ICultureSpecificApprovalCommand
    {
        private Injected<IContentLoader> _contentLoader;
        public virtual string AppliedOnLanguageBranch { get; set; }

        public virtual List<int> AffectedVersions { get; set; }

        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ExpirationDateSettingCommand));
        public override bool IsValid()
        {
            try
            {
                var dictionary = JsonConvert.DeserializeObject<IDictionary<string, object>>(NewSettingsJson);
                var versionable = (string.IsNullOrEmpty(this.AppliedOnLanguageBranch) ? _contentLoader.Service.Get<IContent>(AppliedOnContentLink) : _contentLoader.Service.Get<IContent>(AppliedOnContentLink, new CultureInfo(AppliedOnLanguageBranch))) as IVersionable;
                if (dictionary.TryGetValue("PageStopPublish", out var obj))
                {
                    if (obj is DateTime nullable && versionable != null)
                    {
                        var startPublish = versionable.StartPublish;
                        if (startPublish.HasValue)
                        {
                            ref var local2 = ref nullable;
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
                Logger.Error(ex.Message, ex);
                return false;
            }
        }
    }
}
