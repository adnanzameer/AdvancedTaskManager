using EPiServer.Notification.Internal;
using System;
using System.Data.Common;

namespace AdvancedTask.Business
{
    public static class NotificationMessageFromReader
    {
        public static InternalNotificationMessage Create(DbDataReader reader)
        {
            InternalNotificationMessage notificationMessage = new InternalNotificationMessage()
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
            object obj1 = reader["Sent"];
            if (obj1 != DBNull.Value)
            {
                notificationMessage.Sent = new DateTime?(Convert.ToDateTime(obj1).ToLocalTime());
            }

            object obj2 = reader["SendAt"];
            if (obj2 != DBNull.Value)
            {
                notificationMessage.SendAt = new DateTime?(Convert.ToDateTime(obj2).ToLocalTime());
            }

            object obj3 = reader["Read"];
            if (obj3 != DBNull.Value)
            {
                notificationMessage.Read = new DateTime?(Convert.ToDateTime(obj3).ToLocalTime());
            }

            object obj4 = reader["Category"];
            if (obj4 != DBNull.Value)
            {
                notificationMessage.Category = new Uri(Convert.ToString(obj4));
            }

            return notificationMessage;
        }
    }
}
