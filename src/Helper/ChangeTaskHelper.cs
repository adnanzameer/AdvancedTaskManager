using AdvancedTask.Business.AdvancedTask.Mapper;
using AdvancedTask.Models;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Helper
{
    public class ChangeTaskHelper
    {
        private readonly ApprovalCommandService _generalCommandService;
        private readonly ApprovalCommandMapper _approvalCommandMapper;

        public ChangeTaskHelper(ApprovalCommandService generalCommandService, ApprovalCommandMapper approvalCommandMapper)
        {
            _generalCommandService = generalCommandService;
            _approvalCommandMapper = approvalCommandMapper;
        }

        public ChangeTaskViewModel GetData(int id)
        {
            var commandByApprovalId = _generalCommandService.GetCommandByApprovalId(id);
            return commandByApprovalId != null ? _approvalCommandMapper.Map(commandByApprovalId, PrincipalInfo.CurrentPrincipal) : null;
        }
    }
}
