using System;

namespace AdvancedTask.Business.Interface
{
    public interface IAdvancedTask
    {
        DateTime? ContentApprovalDeadline { get; set; }
    }
}
