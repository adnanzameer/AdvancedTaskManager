using System.Collections.Generic;
using EPiServer.Security;

namespace AdvancedTask.Business.AdvancedTask
{
    internal class ContentACL
    {
        public IEnumerable<AccessControlEntry> Entries { get; set; }

        public bool IsInherited { get; set; }
    }
}