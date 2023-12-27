using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;
using EPiServer.Framework.Modules.Internal;
using Newtonsoft.Json;

namespace Advanced.Task.Manager.Infrastructure.Helpers
{
    public static class Extentions
    {
        public static IEnumerable<DateTime> GetDaysInRange(this DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return Enumerable.Empty<DateTime>();

            return Enumerable.Range(0, 1 + endDate.Subtract(startDate).Days)
                .Select(offset => startDate.AddDays(offset))
                .ToArray();
        }


        public static int CountDaysInRange(this DateTime startDate, DateTime endDate)
        {
            var dates = startDate.GetDaysInRange(endDate);

            return dates.Count();
        }

        public static T ToObject<T>(this string value) where T : class
        {
            if (string.IsNullOrEmpty(value))
                return default;
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
        }

        public static string GetEditUrl(this ContentReference contentLink)
        {
            return $"{ModuleResourceResolver.Instance.ResolvePath("CMS", null)}#context=epi.cms.contentdata:///{contentLink}";
        }

        public static string GetEditUrl(this ContentReference contentLink, string language)
        {
            return $"{ModuleResourceResolver.Instance.ResolvePath("CMS", null).TrimEnd('/')}/?language={language}#context=epi.cms.contentdata:///{contentLink}";
        }
    }
}
