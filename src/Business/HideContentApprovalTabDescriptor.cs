using System;
using System.Collections.Generic;
using System.Configuration;
using EPiServer.Core;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;

namespace AdvancedTask.Business
{
    [EditorDescriptorRegistration(TargetType = typeof(ContentData))]
    public class HideCategoryEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            bool enableContentApprovalDeadline = bool.Parse(ConfigurationManager.AppSettings["ATM:EnableContentApprovalDeadline"] ?? "false");

            if (enableContentApprovalDeadline)
            {
                return;
            }

            foreach (ExtendedMetadata property in metadata.Properties)
            {
                if (property.GroupSettings != null && string.Equals(property.GroupSettings.Name, "Content Approval", StringComparison.OrdinalIgnoreCase))
                {
                    property.GroupSettings.DisplayUI = false;
                    return;

                }
            }
        }
    }
}