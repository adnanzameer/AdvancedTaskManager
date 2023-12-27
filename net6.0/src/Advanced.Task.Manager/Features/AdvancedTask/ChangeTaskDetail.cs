using System.Collections.Generic;
using Advanced.Task.Manager.Infrastructure.Cms.ChangeApproval;

namespace Advanced.Task.Manager.Features.AdvancedTask
{
    public class ChangeTaskDetail
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
