using System.Collections.Generic;
using AdvancedTask.Business.AdvancedTask.Interface;

namespace AdvancedTask.Models
{
    internal class ChangeTaskDetail
    {
        public ChangeTaskDetail()
        {
            Details = new List<IContentChangeDetails>();
        }

        public IEnumerable<IContentChangeDetails> Details { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

    }
}
