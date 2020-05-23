using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedTask.Business.AdvancedTask.Interface;

namespace AdvancedTask.Models
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
