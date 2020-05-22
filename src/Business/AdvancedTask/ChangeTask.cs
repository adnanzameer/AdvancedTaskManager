using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Business.AdvancedTask
{
    class ChangeTask
    {
        private Injected<ApprovalCommandService> _generalCommandService;
        private Injected<ApprovalCommandMapper> _approvalCommandMapper;

        public ChangeTaskViewModel GetData(int id)
        {
            var commandByApprovalId = _generalCommandService.Service.GetCommandByApprovalId(id);
            if (commandByApprovalId != null)
                return _approvalCommandMapper.Service.Map(commandByApprovalId, PrincipalInfo.CurrentPrincipal);

            return null;
        }
    }
}
