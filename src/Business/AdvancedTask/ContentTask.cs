using EPiServer.Core;

namespace AdvancedTask.Business.AdvancedTask
{
    public class ContentTask
    {
        public int ApprovalId { get; set; }
        public ContentReference ContentReference { get; set; }
        public string ContentName { get; set; }
        public string ContentType { get; set; }
        public string Type { get; set; }
        public string DateTime { get; set; }
        public string ApprovalType { get; set; }
        public string Deadline { get; set; }
        public string StartedBy { get; set; }
        public string WarningColor { get; set; }
        public bool NotificationUnread { get; set; }
        public bool CanUserPublish { get; set; }
    }
}

