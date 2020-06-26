using System;
using System.Threading.Tasks;
using AdvancedTask.Business.AdvancedTask.Command;
using AdvancedTask.Business.AdvancedTask.Interface;
using AdvancedTask.Helper;
using EPiServer.Approvals;
using EPiServer.Security;

namespace AdvancedTask.Business.AdvancedTask
{

    internal class ChangeApprovalActions
    {
        private readonly ICommandMetaDataRepository _commandMetaDataRepository;
        private readonly IApprovalEngine _approvalEngine;
        private readonly ApprovalCommandService _approvalCommandService;

        public ChangeApprovalActions(ICommandMetaDataRepository cmdRepository, IApprovalEngine approvalEngine, ApprovalCommandService approvalCommandService)
        {
            _commandMetaDataRepository = cmdRepository;
            _approvalEngine = approvalEngine;
            _approvalCommandService = approvalCommandService;
        }

        public async Task ForceComplete(int approvalId, string forceReason)
        {
            var commandMetaData = _approvalCommandService.GetCommandMetaDataByApprovalId(approvalId);
            var byCommandId = _approvalCommandService.GetApprovalCommand(commandMetaData.Type, commandMetaData.CommandId);

            if (commandMetaData.Type.EndsWith("MovingContentCommand"))
                await ForceAccept(byCommandId as MovingContentCommand, forceReason);
            else
            if (commandMetaData.Type.EndsWith("ExpirationDateSettingCommand"))
                await ForceAccept(byCommandId as ExpirationDateSettingCommand, forceReason);
            else
            if (commandMetaData.Type.EndsWith("LanguageSettingCommand"))
                await ForceAccept(byCommandId as LanguageSettingCommand, forceReason);
            else
            if (commandMetaData.Type.EndsWith("SecuritySettingCommand"))
                await ForceAccept(byCommandId as SecuritySettingCommand, forceReason);

        }


        private async Task ForceAccept<T>(T approvalCommand, string forceReason) where T : ApprovalCommandBase
        {
            await _approvalEngine.ForceApproveAsync(approvalCommand.ApprovalID, PrincipalInfo.CurrentPrincipal.Identity.Name, forceReason);
            PostChangeApprovalTransition(approvalCommand, CommandMetaData.ChangeTaskApprovalStatus.Approved);
        }


        private void PostChangeApprovalTransition<T>(T approvalCommand, CommandMetaData.ChangeTaskApprovalStatus approvalStatus) where T : ApprovalCommandBase
        {

            //if (approvalCommand.IsReadOnly)
            //    approvalCommand = (T)approvalCommand.CreateWritableClone();

            approvalCommand.ChangedBy = PrincipalInfo.CurrentPrincipal.Identity.Name;
            approvalCommand.CommandStatus = approvalStatus;
            approvalCommand.Saved = DateTime.Now;
            var byCommandId = this._commandMetaDataRepository.GetByCommandId(approvalCommand.Id.ExternalId);
            if (byCommandId == null)
                return;
            byCommandId.CommandStatus = approvalStatus;
            _commandMetaDataRepository.Save(byCommandId);
        }
    }
}
