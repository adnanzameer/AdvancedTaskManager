﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.Core;
using EPiServer.Framework.Modules.Internal;
using Newtonsoft.Json;

namespace AdvancedTaskManager.Infrastructure.Helpers
{
    public static class Extensions
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

        public static string TryGetValidDateFormat(string customFormat)
        {
            if (string.IsNullOrEmpty(customFormat))
                return null;

            var currentUtcDateTimeString = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

            return !string.IsNullOrEmpty(customFormat) &&
                   DateTime.TryParseExact(currentUtcDateTimeString, customFormat,
                       CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                ? customFormat
                : null;
        }
    }
}
