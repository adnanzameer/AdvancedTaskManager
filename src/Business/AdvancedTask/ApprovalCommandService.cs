using System;

namespace AdvancedTask.Business.AdvancedTask
{
    public class ApprovalCommandService
    {
        private readonly ICommandMetaDataRepository _commandMetaDataRepository;

        public ApprovalCommandService(
            ICommandMetaDataRepository cmdRepository)
        {
            _commandMetaDataRepository = cmdRepository;
        }

        public virtual ChangeApprovalCommandBase GetCommandByApprovalId(int approvalId)
        {
            var byApprovalId = _commandMetaDataRepository.GetByApprovalId(approvalId);
            return byApprovalId == null ? null : GetApprovalCommand(byApprovalId.Type, byApprovalId.CommandId);
        }

        private ChangeApprovalCommandBase GetApprovalCommand(string commandTypeName, Guid commandId)
        {
            var byCommandId = ApprovalCommandRepositoryBaseTest<MovingContentCommand>.GetByCommandId(commandId, commandTypeName);
            return byCommandId;
        }
    }
}
