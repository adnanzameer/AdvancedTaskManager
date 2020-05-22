using System;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.DataAnnotations;

namespace AdvancedTask.Business.AdvancedTask.Command
{
    public abstract class ChangeApprovalCommandBase : IDynamicData
    {
        private bool _isReadOnly;

        public virtual Identity Id { get; set; }

        public virtual int ApprovalID { get; set; }

        public virtual int LastApprovalStepIndex { get; set; }

        [Ignore]
        public ContentReference AppliedOnContentLink
        {
            get
            {
                ContentReference result;
                return ContentReference.TryParse(this.AppliedOnContent, out result) ? result : ContentReference.EmptyReference;
            }
            set
            {
                this.AppliedOnContent = ContentReference.IsNullOrEmpty(value) ? string.Empty : value.ToReferenceWithoutVersion().ToString();
            }
        }

        public virtual string AppliedOnContent { get; set; }

        public virtual string CurrentSettingsJson { get; set; }

        public virtual string NewSettingsJson { get; set; }

        public virtual CommandMetaData.ChangeTaskApprovalStatus CommandStatus { get; set; }

        public virtual string CreatedBy { get; set; }

        public virtual string ChangedBy { get; set; }

        public virtual DateTime CreatedTime { get; set; }

        public virtual DateTime Saved { get; set; }

        public virtual bool IsReadOnly
        {
            get
            {
                return this._isReadOnly;
            }
            protected set
            {
            }
        }

        public virtual bool IsValid()
        {
            return true;
        }
    }
}
