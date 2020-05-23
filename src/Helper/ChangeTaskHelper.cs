using System;
using AdvancedTask.Business.AdvancedTask;
using AdvancedTask.Business.AdvancedTask.Mapper;
using AdvancedTask.Models;
using EPiServer.Security;

namespace AdvancedTask.Helper
{
    public class ChangeTaskHelper
    {
        private readonly ApprovalCommandService _generalCommandService;
        private readonly ApprovalCommandMapper _approvalCommandMapper;
        private readonly LanguageChangeDetails _languageChangeDetails;
        private readonly MovingChangeDetail _movingChangeDetail;
        private readonly SecurityChangeDetail _securityChangeDetail;

        public ChangeTaskHelper(ApprovalCommandService generalCommandService, ApprovalCommandMapper approvalCommandMapper, LanguageChangeDetails languageChangeDetails, MovingChangeDetail movingChangeDetail, SecurityChangeDetail securityChangeDetail)
        {
            _generalCommandService = generalCommandService;
            _approvalCommandMapper = approvalCommandMapper;
            _languageChangeDetails = languageChangeDetails;
            _movingChangeDetail = movingChangeDetail;
            _securityChangeDetail = securityChangeDetail;
        }


        public ChangeTaskDetail GetData(int id)
        {
            var commandByApprovalId = _generalCommandService.GetCommandByApprovalId(id);
            var taskDetails = commandByApprovalId != null ? _approvalCommandMapper.Map(commandByApprovalId, PrincipalInfo.CurrentPrincipal) : null;
            if (taskDetails != null)
            {
                var customTask = new ChangeTaskDetail();
                if (taskDetails.TypeIdentifier.ToLower().EndsWith("movingcontentcommand"))
                {
                    customTask.Type = "Moving Content";
                    customTask.Details = _movingChangeDetail.GetMovingChangeDetails(taskDetails);
                }
                else if (taskDetails.TypeIdentifier.ToLower().EndsWith("expirationdatesettingcommand"))
                {
                    customTask.Type = "Expiration Date Setting";
                    var helper = new ExpirationChangeDetails();
                    customTask.Details = helper.GetExpirationCommandChangeDetails(taskDetails);
                }
                else if (taskDetails.TypeIdentifier.ToLower().EndsWith("languagesettingcommand"))
                {
                    customTask.Type = "Language Setting";
                    customTask.Details = _languageChangeDetails.GetLanguageChangeDetails(taskDetails);
                }
                else if (taskDetails.TypeIdentifier.ToLower().EndsWith("securitysettingcommand"))
                {
                    customTask.Type = "Security Setting";
                    customTask.Details = _securityChangeDetail.GetSecurityChangeDetails(new Guid(taskDetails.Id));
                }

                customTask.Name = taskDetails.Name;

                return customTask;
            }

            return null;
        }

    }
}

