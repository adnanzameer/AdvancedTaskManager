using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using Microsoft.Extensions.Configuration;

namespace AdvancedTask.Business
{
    [EditorDescriptorRegistration(TargetType = typeof(ContentData))]
    public class HideCategoryEditorDescriptor : EditorDescriptor
    {
        private Injected<IConfiguration> _configuration;
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            var enableContentApprovalDeadline = _configuration.Service.GetValue<bool>("ATM:EnableContentApprovalDeadline:True");

            if (enableContentApprovalDeadline)
                return;

            foreach (var property in metadata.Properties)
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