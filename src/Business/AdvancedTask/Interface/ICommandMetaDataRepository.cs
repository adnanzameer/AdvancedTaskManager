using AdvancedTask.Business.AdvancedTask.Command;

namespace AdvancedTask.Business.AdvancedTask.Interface
{
    public interface ICommandMetaDataRepository
    {
        CommandMetaData GetByApprovalId(int approvalId);
    }
}

