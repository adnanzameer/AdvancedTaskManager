using System.Collections.Generic;
using AdvancedTask.Business.AdvancedTask.Interface;
using EPiServer.Core;

namespace AdvancedTask.Models
{
    public class ContentTask
    {
        public ContentTask()
        {
            ContentReference = ContentReference.EmptyReference;
            ContentName = "";
            ContentType = "";
            Type = "";
            DateTime = "";
            Deadline = "";
            StartedBy = "";
            WarningColor = "";
            Details= new List<IContentChangeDetails>();
        }

        public int ApprovalId { get; set; }
        public ContentReference ContentReference { get; set; }
        public string ContentName { get; set; }
        public string ContentType { get; set; }
        public string Type { get; set; }
        public string DateTime { get; set; }
        public string Deadline { get; set; }
        public string StartedBy { get; set; }
        public string WarningColor { get; set; }
        public bool NotificationUnread { get; set; }
        public bool CanUserPublish { get; set; }
        public IEnumerable<IContentChangeDetails> Details { get; set; }
    }
}

