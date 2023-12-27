using EPiServer.Shell.ObjectEditing;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Newtonsoft.Json;

namespace FoundationCore.Web.Business.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FriendlyPropertyListUrlAttribute : Attribute, IDisplayMetadataProvider
    {
        public FriendlyPropertyListUrlAttribute(params string[] fieldNames)
        {
            FieldNames = fieldNames;
        }

        public ICollection<string> FieldNames { get; }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var extendedMetadata = context.DisplayMetadata.AdditionalValues[ExtendedMetadata.ExtendedMetadataDisplayKey] as ExtendedMetadata;

            if (extendedMetadata == null)
            {
                return;
            }

            var model = extendedMetadata.InitialValue as dynamic;

            if (model == null)
            {
                return;
            }

            // Add our field names to the metadata.
            extendedMetadata.EditorConfiguration.Add("fieldNames", GetCamelCaseFieldNames(FieldNames));

            // A dictionary which will contain our mappings for both our ContentReferences
            // and URLs to friendly URLs.
            IDictionary<string, string> urls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in model)
            {
                var properties = item.GetType().GetProperties();

                foreach (var property in properties)
                {
                    var url = GetValue<Url>(property, item);
                    if (url != null)
                    {
                        if (!url.IsEmpty() && !urls.ContainsKey(url.ToString()))
                        {
                            var friendlyUrl = UrlResolver.Current.GetUrl(new UrlBuilder(url), ContextMode.Default);

                            if (IsImageUrl(friendlyUrl))
                            {
                                urls.Add(url.ToString(), "<img src=\"" + friendlyUrl + "\"/>");
                            }
                            else
                            {
                                urls.Add(url.ToString(), friendlyUrl);
                            }
                        }
                    }
                    else
                    {
                        var contentLink = GetValue<ContentReference>(property, item);

                        if (!ContentReference.IsNullOrEmpty(contentLink) && !urls.ContainsKey(contentLink.ID.ToString()))
                        {
                            var friendlyUrl = UrlResolver.Current.GetUrl(contentLink);
                            if (IsImageUrl(friendlyUrl))
                            {
                                urls.Add(contentLink.ID.ToString(), "<img src=\"" + friendlyUrl + "\"/>");
                            }
                            else
                            {
                                urls.Add(contentLink.ID.ToString(), friendlyUrl);
                            }
                        }
                    }
                }
            }

            // Add our URL mappings to the metadata.
            extendedMetadata.EditorConfiguration.Add("urlMappings", urls);
        }

        /// <summary>
        ///     Gets the value of a property as the correct type.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="item">The parent object.</param>
        /// <returns>The property value.</returns>
        private static T GetValue<T>(dynamic property, dynamic item)
        {
            if (typeof(T).IsAssignableFrom(property.PropertyType))
            {
                return (T)property.GetValue(item, null);
            }

            return default;
        }

        /// <summary>
        ///     Converts Pascal Case field names into Camel Case via Newtonsoft.Json.
        /// </summary>
        /// <param name="fieldNames">The field names.</param>
        /// <returns>The field names in Camel Case.</returns>
        private static ICollection<string> GetCamelCaseFieldNames(IEnumerable<string> fieldNames)
        {
            IDictionary<string, int> dict = new Dictionary<string, int>();

            foreach (var fieldName in fieldNames)
            {
                dict.Add(fieldName, 0);
            }

            var serializedObject = JsonConvert.SerializeObject(dict, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });

            var result = JsonConvert.DeserializeObject<Dictionary<string, int>>(serializedObject);

            return result.Keys.ToList();
        }

        private static bool IsImageUrl(string url)
        {
            var imageExtensions = new List<string> { ".JPG", ".JPEG", ".JPE", ".BMP", ".GIF", ".PNG" };

            return imageExtensions.Contains(Path.GetExtension(url).ToUpperInvariant());
        }
    }
}