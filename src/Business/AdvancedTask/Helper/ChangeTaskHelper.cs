using AdvancedTask.Business.AdvancedTask.Mapper;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Business.AdvancedTask.Helper
{
    public class ChangeTaskHelper
    {
        private Injected<ApprovalCommandService> _generalCommandService;
        private Injected<ApprovalCommandMapper> _approvalCommandMapper;

        public ChangeTaskViewModel GetData(int id)
        {
            var commandByApprovalId = _generalCommandService.Service.GetCommandByApprovalId(id);
            return commandByApprovalId != null ? _approvalCommandMapper.Service.Map(commandByApprovalId, PrincipalInfo.CurrentPrincipal) : null;
        }
    }
}
