using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AdvancedTask.Business.AdvancedTask;
using AdvancedTask.Business.AdvancedTask.Command;
using AdvancedTask.Business.AdvancedTask.Interface;
using AdvancedTask.Business.AdvancedTask.Mapper;
using AdvancedTask.Helper;
using AdvancedTask.Models;
using EPiServer;
using EPiServer.Cms.Shell.Service.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Security;
using EPiServer.Shell.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdvancedTask.Business.AdvancedTask
{
    public class SecurityChangeDetail
    {
        private readonly ApprovalCommandService _generalCommandService;
        private readonly LocalizationService _localizationService;

        private static readonly List<AccessLevel> KnownAccessLevels = new List<AccessLevel>()
        {
            AccessLevel.Read,
            AccessLevel.Create,
            AccessLevel.Edit,
            AccessLevel.Delete,
            AccessLevel.Publish,
            AccessLevel.Administer
        };

        public SecurityChangeDetail(LocalizationService localizationService, ApprovalCommandService generalCommandService)
        {
            _localizationService = localizationService;
            _generalCommandService = generalCommandService;
        }


        #region security

        public IEnumerable<IContentChangeDetails> GetSecurityChangeDetails(Guid command)
        {
            var byCommandId = _generalCommandService.GetCommandById(command);
            var contentChangeDetailsList = new List<IContentChangeDetails>();
            var str1 = _localizationService.GetString("/gadget/changeapproval/securitysettingcommand/yes");
            var str2 = _localizationService.GetString("/gadget/changeapproval/securitysettingcommand/no");
            if (byCommandId == null)
                return null;
            var accessControlList1 = GetContentAccessControlList(byCommandId.CurrentSettingsJson);
            var accessControlList2 = GetContentAccessControlList(byCommandId.NewSettingsJson);
            if (accessControlList1 == null || accessControlList2 == null)
                return null;
            if (accessControlList1.IsInherited != accessControlList2.IsInherited)
            {
                var contentChangeDetails = (IContentChangeDetails)new ContentChangeDetails();
                contentChangeDetails.Name = _localizationService.GetString("/gadget/changeapproval/securitysettingcommand/inheritsettingsfromtheparentpage");
                contentChangeDetails.OldValue = accessControlList1.IsInherited ? str1 : str2;
                contentChangeDetails.NewValue = accessControlList2.IsInherited ? str1 : str2;
                contentChangeDetailsList.Add(contentChangeDetails);
            }

            var secCommand = byCommandId as SecuritySettingCommand;
            if (secCommand != null && (secCommand.SecuritySaveType == SecuritySaveType.MergeChildPermissions || secCommand.SecuritySaveType == SecuritySaveType.ReplaceChildPermissions))
            {
                var contentChangeDetails = (IContentChangeDetails)new ContentChangeDetails();
                contentChangeDetails.Name = _localizationService.GetString("/gadget/changeapproval/securitysettingcommand/applytosubitems");
                contentChangeDetails.OldValue = string.Empty;
                contentChangeDetails.NewValue = str1;
                contentChangeDetailsList.Add(contentChangeDetails);
            }
            var contentChangeDetails1 = (IContentChangeDetails)new ContentChangeDetails();
            contentChangeDetails1.NewValue ="";
            contentChangeDetails1.OldValue = "";

            contentChangeDetails1.Name = _localizationService.GetString("/gadget/changeapproval/securitysettingcommand/accessrolecontrol");
            var source1 = accessControlList1.Entries.OrderBy<AccessControlEntry, string>((Func<AccessControlEntry, string>)(e => e.Name));
            var source2 = accessControlList2.Entries.OrderBy<AccessControlEntry, string>((Func<AccessControlEntry, string>)(e => e.Name));

            foreach (var accessControlEntry1 in source1)
            {
                var currentAccessControlItem = accessControlEntry1;
                var accessControlEntry2 = source2.FirstOrDefault<AccessControlEntry>((Func<AccessControlEntry, bool>)(e => e.Name.Equals(currentAccessControlItem.Name, StringComparison.OrdinalIgnoreCase))) ?? new AccessControlEntry(currentAccessControlItem.Name, AccessLevel.Undefined);
                contentChangeDetails1.OldValue = contentChangeDetails1.OldValue + LocalizeCurrentAccessLevel(WebUtility.HtmlEncode(currentAccessControlItem.Name), currentAccessControlItem.Access, accessControlEntry2.Access) + "</br>";
            }

            foreach (var accessControlEntry1 in (IEnumerable<AccessControlEntry>)source2)
            {
                var newAccessControlItem = accessControlEntry1;
                var accessControlEntry2 = source1.FirstOrDefault<AccessControlEntry>((Func<AccessControlEntry, bool>)(e => e.Name.Equals(newAccessControlItem.Name, StringComparison.OrdinalIgnoreCase))) ?? new AccessControlEntry(newAccessControlItem.Name, AccessLevel.Undefined);
                contentChangeDetails1.NewValue = contentChangeDetails1.NewValue + LocalizeNewAccessLevel(WebUtility.HtmlEncode(newAccessControlItem.Name), accessControlEntry2.Access, newAccessControlItem.Access) + "</br>";
            }
            contentChangeDetails1.OldValue = contentChangeDetails1.OldValue == null ? string.Empty : contentChangeDetails1.OldValue.ToString().TrimEnd("</br>");
            contentChangeDetails1.NewValue = contentChangeDetails1.NewValue == null ? string.Empty : contentChangeDetails1.NewValue.ToString().TrimEnd("</br>");
            contentChangeDetailsList.Add(contentChangeDetails1);
            return contentChangeDetailsList;
        }

        private string LocalizeCurrentAccessLevel(string accessLevelName, AccessLevel currentAccessLevel, AccessLevel newAccessLevel)
        {
            var str = string.Format("{0}: ", accessLevelName);
            string text;
            if (currentAccessLevel == AccessLevel.NoAccess || currentAccessLevel == AccessLevel.Undefined)
            {
                text = _localizationService.GetString("/gadget/changeapproval/accesslevels/" + currentAccessLevel);
            }
            else
            {
                foreach (var knownAccessLevel in KnownAccessLevels)
                {
                    if (currentAccessLevel.HasFlag((System.Enum)knownAccessLevel))
                        str = str + _localizationService.GetString("/gadget/changeapproval/accesslevels/" + knownAccessLevel) + ", ";
                }
                text = str.TrimEnd(", ");
            }
            if (currentAccessLevel == newAccessLevel)
                text = text.Fade();
            return text;
        }

        private string LocalizeNewAccessLevel(string accessLevelName, AccessLevel currentAccessLevel, AccessLevel newAccessLevel)
        {
            var str = string.Format("{0}: ", accessLevelName);
            string text1;
            if ((newAccessLevel == AccessLevel.NoAccess || newAccessLevel == AccessLevel.Undefined) && newAccessLevel == currentAccessLevel)
            {
                text1 = _localizationService.GetString("/gadget/changeapproval/accesslevels/" + newAccessLevel);
            }
            else
            {
                foreach (var knownAccessLevel in KnownAccessLevels)
                {
                    if (currentAccessLevel.HasFlag((System.Enum)knownAccessLevel) || newAccessLevel.HasFlag((System.Enum)knownAccessLevel))
                    {
                        var text2 = _localizationService.GetString("/gadget/changeapproval/accesslevels/" + knownAccessLevel);
                        if (currentAccessLevel.HasFlag((System.Enum)knownAccessLevel) && !newAccessLevel.HasFlag((System.Enum)knownAccessLevel))
                            text2 = text2.Strikethrough();
                        else if (!currentAccessLevel.HasFlag((System.Enum)knownAccessLevel) && newAccessLevel.HasFlag((System.Enum)knownAccessLevel))
                            text2 = text2.Bold();
                        str = str + text2 + ", ";
                    }
                }
                text1 = str.TrimEnd(", ");
            }
            if (currentAccessLevel == newAccessLevel)
                text1 = text1.Fade();
            return text1;
        }
        private ContentACL GetContentAccessControlList(string json)
        {
            if (string.IsNullOrEmpty(json))
                return (ContentACL)null;
            var jobject = JsonConvert.DeserializeObject<JObject>(json);
            return new ContentACL()
            {
                IsInherited = jobject["IsInherited"].ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase),
                Entries = (IEnumerable<AccessControlEntry>)GetAccessControlListFromJsonArray(jobject["Entries"])
            };
        }
        private List<AccessControlEntry> GetAccessControlListFromJsonArray(JToken entriesArray)
        {
            var accessControlEntryList = new List<AccessControlEntry>();
            try
            {
                foreach (var entries in (IEnumerable<JToken>)entriesArray)
                {
                    var accessControlEntry = new AccessControlEntry(entries["Name"].ToString(), (AccessLevel)int.Parse(entries["Access"].ToString()), (SecurityEntityType)int.Parse(entries["EntityType"].ToString()));
                    accessControlEntryList.Add(accessControlEntry);
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
            }
            return accessControlEntryList;
        }
        #endregion
    }
}
