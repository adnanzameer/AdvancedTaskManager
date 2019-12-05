using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using System;
using System.Linq;

namespace AdvancedTask.Business
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class AdvancedTaskInitialization : IInitializableModule
    {
        private const string ContentApprovalDeadlinePropertyName = "ATM_ContentApprovalDeadline";

        private static readonly object _lock = new object();
        private Injected<IContentTypeRepository> _contentTypeRepository;
        private Injected<ITabDefinitionRepository> _tabDefinitionRepository;
        private Injected<IPropertyDefinitionRepository> _propertyDefinitionRepository;
        private Injected<IPropertyDefinitionTypeRepository> _propertyDefinitionTypeRepository;
        private Injected<LocalizationService> _localizationService;

        public void Initialize(InitializationEngine context)
        {
            SetupInsightFormMappingProperties();
        }

        private void SetupInsightFormMappingProperties()
        {
            CreateOrDeleteTab("Content Approval", true);

            foreach (ContentType contentType in _contentTypeRepository.Service.List().Where(x => x.IsAvailable))
            {
                CreateUpdatePropertyDefinition(
                    contentType,
                    ContentApprovalDeadlinePropertyName,
                    "Content approval deadline",
                    typeof(PropertyDate),
                    "Content Approval", 10);
            }
        }

        private void CreateOrDeleteTab(string tabName, bool createNew)
        {
            object obj2 = _lock;
            lock (obj2)
            {
                TabDefinition tabDefinition = this._tabDefinitionRepository.Service.Load(tabName);
                if (createNew)
                {
                    if (tabDefinition != null)
                    {
                        return;
                    }

                    tabDefinition = new TabDefinition
                    {
                        Name = tabName,
                        SortIndex = 300,
                        RequiredAccess = AccessLevel.Edit
                    };
                    _tabDefinitionRepository.Service.Save(tabDefinition);
                }
                else if (tabDefinition != null)
                {
                    _tabDefinitionRepository.Service.Delete(tabDefinition);
                }
            }
        }

        private void CreateUpdatePropertyDefinition(ContentType contentType, string propertyDefinitionName, string editCaption = null, Type propertyDefinitionType = null, string tabName = null, int? propertyOrder = new int?())
        {
            PropertyDefinition propertyDefinition =
                GetPropertyDefinition(contentType, propertyDefinitionName);
            if (propertyDefinition == null)
            {
                if (propertyDefinitionType == null)
                {
                    return;
                }
                propertyDefinition = new PropertyDefinition();
            }
            else
            {
                propertyDefinition = propertyDefinition.CreateWritableClone();
            }
            propertyDefinition.ContentTypeID = contentType.ID;
            propertyDefinition.DisplayEditUI = true;
            propertyDefinition.DefaultValueType = DefaultValueType.None;
            if (propertyDefinitionName != null)
            {
                propertyDefinition.Name = propertyDefinitionName;
            }
            if (editCaption != null)
            {
                propertyDefinition.EditCaption = editCaption;
            }
            if (propertyDefinitionType != null)
            {
                propertyDefinition.Type = this._propertyDefinitionTypeRepository.Service.Load(propertyDefinitionType);
            }
            if (tabName != null)
            {
                propertyDefinition.Tab = _tabDefinitionRepository.Service.Load(tabName);
            }
            if (propertyOrder.HasValue)
            {
                propertyDefinition.FieldOrder = propertyOrder.Value;
            }
            _propertyDefinitionRepository.Service.Save(propertyDefinition);
        }

        private PropertyDefinition GetPropertyDefinition(ContentType contentType, string propertyName, Type propertyDefinitionType = null)
        {
            System.Collections.Generic.IEnumerable<PropertyDefinition> source = from pd in contentType.PropertyDefinitions
                                                                                where pd.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)
                                                                                select pd;
            if (propertyDefinitionType != null)
            {
                source = from pd in source
                         where pd.Type.DefinitionType == propertyDefinitionType
                         select pd;
            }
            return source.FirstOrDefault();
        }


        public void Uninitialize(InitializationEngine context) { }
    }
}