using EPiServer.Shell.ObjectEditing;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Newtonsoft.Json;

namespace FoundationCore.Web.Business.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HtmlViewerExtenderAttribute : Attribute, IDisplayMetadataProvider
    {
        public HtmlViewerExtenderAttribute(params string[] properties)
        {
            Properties = properties;
        }

        public ICollection<string> Properties { get; set; }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var extendedMetadata = context.DisplayMetadata.AdditionalValues[ExtendedMetadata.ExtendedMetadataDisplayKey] as ExtendedMetadata;
            if (extendedMetadata == null)
            {
                return;
            }

            if (Properties.Any())
                extendedMetadata.EditorConfiguration.Add("mappedHtmlProperties", GetCamelCaseFieldNames(Properties));
        }

        private static ICollection<string> GetCamelCaseFieldNames(IEnumerable<string> fieldNames)
        {
            IDictionary<string, int> dict = new Dictionary<string, int>();

            foreach (var fieldName in fieldNames)
                dict.Add(fieldName, 0);

            var serializedObject = JsonConvert.SerializeObject(dict, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });

            var result = JsonConvert.DeserializeObject<Dictionary<string, int>>(serializedObject);

            return result.Keys.ToList();
        }
    }
}