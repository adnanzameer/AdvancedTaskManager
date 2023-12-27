using System.ComponentModel.DataAnnotations;

namespace FoundationCore.Web.Business.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MaxItemsAttribute : ValidationAttribute
    {
        private readonly int _maxAllowed;

        public MaxItemsAttribute(int maxItemsAllowed)
        {
            _maxAllowed = maxItemsAllowed;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var contentArea = value as ContentArea;

            // Get all items or none if null
            var allItems = contentArea?.Items ?? Enumerable.Empty<ContentAreaItem>();

            // Count the unique personalisation group names, replacing empty ones (items which aren't personalised) with a unique name
            var i = 0;
            var maxNumberOfItemsShown = allItems.Select(x => string.IsNullOrEmpty(x.ContentGroup) ? i++.ToString() : x.ContentGroup).Distinct().Count();

            return maxNumberOfItemsShown > _maxAllowed ? new ValidationResult($"The property \"{validationContext.DisplayName}\" is limited to {_maxAllowed} items") : null;
        }

    }
}
