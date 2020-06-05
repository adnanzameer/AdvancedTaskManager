using System;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    [EPiServerDataStore(AutomaticallyRemapStore = true)]
    public class CommandMetaData : IDynamicData
    {
        public Identity Id { get; set; }

        [EPiServerDataIndex]
        public int ApprovalId { get; set; }

        public string Type { get; set; }

        [EPiServerDataIndex]
        public Guid CommandId { get; set; }

        [EPiServerDataIndex]
        public ContentReference AppliedOnContent { get; set; }

        public ChangeTaskApprovalStatus CommandStatus { get; set; }


        public enum ChangeTaskApprovalStatus
        {
            InReview,
            Approved
        }
    }
}