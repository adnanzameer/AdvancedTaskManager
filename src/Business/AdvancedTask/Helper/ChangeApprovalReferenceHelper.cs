using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;

namespace AdvancedTask.Business.AdvancedTask.Helper
{
    public static class ChangeApprovalReferenceHelper
    {
        public static Uri GetUri(ContentReference contentLink, bool ignoreVersion)
        {
            if (contentLink == (ContentReference)null)
                return (Uri)null;
            string uriString = string.Format("{0}:{1}/{2}/", (object)ChangeApprovalTypeFactory.ChangeApprovalType, (object)(contentLink.ProviderName ?? ""), (object)contentLink.ID);
            if (!ignoreVersion && contentLink.WorkID != 0)
                uriString = uriString + contentLink.WorkID.ToString() + "/";
            return new Uri(uriString);
        }

        public static ContentReference GetContentReference(Uri key)
        {
            if (key == (Uri)null || !key.Scheme.Equals(ChangeApprovalTypeFactory.ChangeApprovalType))
                return (ContentReference)null;
            List<string> list = ((IEnumerable<string>)key.Segments).Select<string, string>((Func<string, string>)(x => x.Replace("/", ""))).ToList<string>();
            if (list.Count<string>() < 2 || list.Count > 3)
                return (ContentReference)null;
            string providerName = string.IsNullOrEmpty(list[0]) ? (string)null : list[0];
            int result1;
            if (!int.TryParse(list[1], out result1))
                return (ContentReference)null;
            int result2 = 0;
            return list.Count > 2 && list[2] != string.Empty && !int.TryParse(list[2], out result2) ? (ContentReference)null : new ContentReference(result1, result2, providerName);
        }
    }
}