using System;
using AdvancedTask.Infrastructure.Cms.ChangeApproval;

namespace AdvancedTask.Features.Interface
{
    public interface ICommandMetaDataRepository
    {
        CommandMetaData GetByApprovalId(int approvalId);
        CommandMetaData GetByCommandId(Guid commandId);
        Guid Save(CommandMetaData commandMetaData);
    }
}

