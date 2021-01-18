using System;
using System.Configuration;
using System.Linq;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Security;

namespace AdvancedTask.Business
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    internal class AdvancedTaskInitialization : IInitializableModule
    {
        private const string ContentApprovalDeadlinePropertyName = "ATM_ContentApprovalDeadline";

        private static readonly object _lock = new object();
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ITabDefinitionRepository _tabDefinitionRepository;
        private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;
        private readonly IPropertyDefinitionTypeRepository _propertyDefinitionTypeRepository;

        public AdvancedTaskInitialization(IContentTypeRepository contentTypeRepository, ITabDefinitionRepository tabDefinitionRepository, IPropertyDefinitionRepository propertyDefinitionRepository, IPropertyDefinitionTypeRepository propertyDefinitionTypeRepository)
        {
            _contentTypeRepository = contentTypeRepository;
            _tabDefinitionRepository = tabDefinitionRepository;
            _propertyDefinitionRepository = propertyDefinitionRepository;
            _propertyDefinitionTypeRepository = propertyDefinitionTypeRepository;
        }

        public void Initialize(InitializationEngine context)
        {
            var enableContentApprovalDeadline = bool.Parse(ConfigurationManager.AppSettings["ATM:EnableContentApprovalDeadline"] ?? "false");

            if (enableContentApprovalDeadline)
            {
                SetupMappingProperties();
            }
            else
            {
                var deleteContentApprovalDeadlineProperty = bool.Parse(ConfigurationManager.AppSettings["ATM:DeleteContentApprovalDeadlineProperty"] ?? "false");
                if (deleteContentApprovalDeadlineProperty)
                    DeleteMappingProperties();
            }

        }

        private void SetupMappingProperties()
        {
            CreateOrDeleteTab("Content Approval", true);

            foreach (var contentType in _contentTypeRepository.List().Where(x => x.IsAvailable))
                CreateUpdatePropertyDefinition(
                    contentType,
                    ContentApprovalDeadlinePropertyName,
                    "Content approval deadline",
                    typeof(PropertyDate),
                    "Content Approval", 10);
        }

        private void DeleteMappingProperties()
        {
            foreach (var contentType in _contentTypeRepository.List().Where(x => x.IsAvailable))
                DeletePropertyDefinition(
                    contentType,
                    ContentApprovalDeadlinePropertyName);
        }

        private void CreateOrDeleteTab(string tabName, bool createNew)
        {
            var obj2 = _lock;
            lock (obj2)
            {
                var tabDefinition = this._tabDefinitionRepository.Load(tabName);
                if (createNew)
                {
                    if (tabDefinition != null)
                        return;

                    tabDefinition = new TabDefinition
                    {
                        Name = tabName,
                        SortIndex = 300,
                        RequiredAccess = AccessLevel.Edit
                    };
                    _tabDefinitionRepository.Save(tabDefinition);
                }
                else if (tabDefinition != null)
                {
                    _tabDefinitionRepository.Delete(tabDefinition);
                }
            }
        }

        private void CreateUpdatePropertyDefinition(ContentType contentType, string propertyDefinitionName, string editCaption = null, Type propertyDefinitionType = null, string tabName = null, int? propertyOrder = new int?())
        {
            var propertyDefinition =
                GetPropertyDefinition(contentType, propertyDefinitionName);
            if (propertyDefinition == null)
            {
                if (propertyDefinitionType == null) return;
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
                propertyDefinition.Name = propertyDefinitionName;
            if (editCaption != null)
                propertyDefinition.EditCaption = editCaption;
            if (propertyDefinitionType != null)
                propertyDefinition.Type = this._propertyDefinitionTypeRepository.Load(propertyDefinitionType);
            if (tabName != null)
            {
                var obj2 = _lock;
                lock (obj2)
                {
                    propertyDefinition.Tab = _tabDefinitionRepository.Load(tabName);
                }
            }

            if (propertyOrder.HasValue)
                propertyDefinition.FieldOrder = propertyOrder.Value;
            _propertyDefinitionRepository.Save(propertyDefinition);
        }

        private void DeletePropertyDefinition(ContentType contentType, string propertyDefinitionName)
        {
            var propertyDefinition = GetPropertyDefinition(contentType, propertyDefinitionName);

            if (propertyDefinition != null) _propertyDefinitionRepository.Delete(propertyDefinition);
        }

        private PropertyDefinition GetPropertyDefinition(ContentType contentType, string propertyName, Type propertyDefinitionType = null)
        {
            var source = from pd in contentType.PropertyDefinitions
                         where pd.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)
                         select pd;
            if (propertyDefinitionType != null)
                source = from pd in source
                         where pd.Type.DefinitionType == propertyDefinitionType
                         select pd;
            return source.FirstOrDefault();
        }


        public void Uninitialize(InitializationEngine context)
        {
            // required
        }
    }
}