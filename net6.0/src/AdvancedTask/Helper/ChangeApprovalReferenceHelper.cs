using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedTask.Business.AdvancedTask;
using EPiServer.Core;

namespace AdvancedTask.Helper
{
    internal static class ChangeApprovalReferenceHelper
    {
        public static Uri GetUri(ContentReference contentLink, bool ignoreVersion)
        {
            if (contentLink == null)
                return null;
            var uriString = string.Format("{0}:{1}/{2}/", ChangeApprovalTypeFactory.ChangeApprovalType, contentLink.ProviderName ?? "", contentLink.ID);
            if (!ignoreVersion && contentLink.WorkID != 0)
                uriString = uriString + contentLink.WorkID.ToString() + "/";
            return new Uri(uriString);
        }

        public static ContentReference GetContentReference(Uri key)
        {
            if (key == null || !key.Scheme.Equals(ChangeApprovalTypeFactory.ChangeApprovalType))
                return null;
            var list = key.Segments.Select(x => x.Replace("/", "")).ToList();
            if (list.Count() < 2 || list.Count > 3)
                return null;
            var providerName = string.IsNullOrEmpty(list[0]) ? null : list[0];
            int result1;
            if (!int.TryParse(list[1], out result1))
                return null;
            var result2 = 0;
            return list.Count > 2 && list[2] != string.Empty && !int.TryParse(list[2], out result2) ? null : new ContentReference(result1, result2, providerName);
        }
    }
}