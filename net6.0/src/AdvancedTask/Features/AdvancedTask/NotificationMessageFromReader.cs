using System;
using System.Data.Common;
using EPiServer.Notification.Internal;

namespace AdvancedTask.Features.AdvancedTask
{
    public static class NotificationMessageFromReader
    {
        public static InternalNotificationMessage Create(DbDataReader reader)
        {
            var notificationMessage = new InternalNotificationMessage()
            {
                Recipient = Convert.ToString(reader["Recipient"]),
                Sender = Convert.ToString(reader["Sender"]),
                ChannelName = Convert.ToString(reader["Channel"]),
                TypeName = Convert.ToString(reader["Type"]),
                Subject = Convert.ToString(reader["Subject"]),
                Content = Convert.ToString(reader["Content"]),
                ID = Convert.ToInt32(reader["ID"]),
                Saved = Convert.ToDateTime(reader["Saved"]).ToLocalTime()
            };
            var obj1 = reader["Sent"];
            if (obj1 != DBNull.Value)
                notificationMessage.Sent = Convert.ToDateTime(obj1).ToLocalTime();

            var obj2 = reader["SendAt"];
            if (obj2 != DBNull.Value)
                notificationMessage.SendAt = Convert.ToDateTime(obj2).ToLocalTime();

            var obj3 = reader["Read"];
            if (obj3 != DBNull.Value)
                notificationMessage.Read = Convert.ToDateTime(obj3).ToLocalTime();

            var obj4 = reader["Category"];
            if (obj4 != DBNull.Value)
                notificationMessage.Category = new Uri(Convert.ToString(obj4) ?? string.Empty);

            return notificationMessage;
        }
    }
}
