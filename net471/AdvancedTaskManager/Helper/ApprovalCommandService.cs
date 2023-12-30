using System;
using AdvancedTask.Business.AdvancedTask;
using AdvancedTask.Business.AdvancedTask.Command;
using AdvancedTask.Business.AdvancedTask.Interface;

namespace AdvancedTask.Helper
{
    internal class ApprovalCommandService
    {
        private readonly ICommandMetaDataRepository _commandMetaDataRepository;
        private readonly ApprovalCommandRepositoryBase _approvalCommandRepositoryBase;

        public ApprovalCommandService(ICommandMetaDataRepository cmdRepository, ApprovalCommandRepositoryBase approvalCommandRepositoryBase)
        {
            _commandMetaDataRepository = cmdRepository;
            _approvalCommandRepositoryBase = approvalCommandRepositoryBase;
        }

        public virtual ApprovalCommandBase GetCommandById(Guid commandId)
        {
            var byCommandId = _commandMetaDataRepository.GetByCommandId(commandId);
            return byCommandId == null ? null : GetApprovalCommand(byCommandId.Type, byCommandId.CommandId);
        }


        public virtual ApprovalCommandBase GetCommandByApprovalId(int approvalId)
        {
            var byApprovalId = _commandMetaDataRepository.GetByApprovalId(approvalId);
            return byApprovalId == null ? null : GetApprovalCommand(byApprovalId.Type, byApprovalId.CommandId);
        }


        public virtual CommandMetaData GetCommandMetaDataByApprovalId(int approvalId)
        {
            var byApprovalId = _commandMetaDataRepository.GetByApprovalId(approvalId);
            return byApprovalId;
        }

        public ApprovalCommandBase GetApprovalCommand(string commandTypeName, Guid commandId)
        {
            ApprovalCommandBase byCommandId = null;

            if (commandTypeName.EndsWith("MovingContentCommand"))
                byCommandId = _approvalCommandRepositoryBase.GetByCommandId<MovingContentCommand>(commandId, commandTypeName);
            else
            if (commandTypeName.EndsWith("ExpirationDateSettingCommand"))
                byCommandId = _approvalCommandRepositoryBase.GetByCommandId<ExpirationDateSettingCommand>(commandId, commandTypeName);
            else
            if (commandTypeName.EndsWith("LanguageSettingCommand"))
                byCommandId = _approvalCommandRepositoryBase.GetByCommandId<LanguageSettingCommand>(commandId, commandTypeName);
            else
            if (commandTypeName.EndsWith("SecuritySettingCommand")) byCommandId = _approvalCommandRepositoryBase.GetByCommandId<SecuritySettingCommand>(commandId, commandTypeName);
            return byCommandId;
        }

    }
}
