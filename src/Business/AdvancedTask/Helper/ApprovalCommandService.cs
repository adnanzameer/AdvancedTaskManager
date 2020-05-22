using System;
using AdvancedTask.Business.AdvancedTask.Command;
using AdvancedTask.Business.AdvancedTask.Interface;

namespace AdvancedTask.Business.AdvancedTask.Helper
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
            ChangeApprovalCommandBase byCommandId = null;

            if (commandTypeName.EndsWith("MovingContentCommand"))
            {
                byCommandId = ApprovalCommandRepositoryBase<MovingContentCommand>.GetByCommandId(commandId, commandTypeName);
            }
            else
            if (commandTypeName.EndsWith("ExpirationDateSettingCommand"))
            {
                byCommandId = ApprovalCommandRepositoryBase<ExpirationDateSettingCommand>.GetByCommandId(commandId, commandTypeName);
            }
            else
            if (commandTypeName.EndsWith("LanguageSettingCommand"))
            {
                byCommandId = ApprovalCommandRepositoryBase<LanguageSettingCommand>.GetByCommandId(commandId, commandTypeName);
            }
            else
            if (commandTypeName.EndsWith("SecuritySettingCommand"))
            {
                byCommandId = ApprovalCommandRepositoryBase<SecuritySettingCommand>.GetByCommandId(commandId, commandTypeName);
            }
            return byCommandId;
        }
    }
}
