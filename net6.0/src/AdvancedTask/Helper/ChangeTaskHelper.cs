﻿using System;
using AdvancedTask.Business.AdvancedTask;
using AdvancedTask.Business.AdvancedTask.Mapper;
using AdvancedTask.Models;
using EPiServer.Security;

namespace AdvancedTask.Helper
{
    public interface IChangeTaskHelper
    {
        ChangeTaskDetail GetData(int id);
    }

    public class ChangeTaskHelper: IChangeTaskHelper
    {
        private readonly IApprovalCommandService _generalCommandService;
        private readonly IApprovalCommandMapper _approvalCommandMapper;
        private readonly ILanguageChangeDetails _languageChangeDetails;
        private readonly IMovingChangeDetail _movingChangeDetail;
        private readonly ISecurityChangeDetail _securityChangeDetail;
        private readonly IExpirationChangeDetails _expirationChangeDetails;
        private readonly IPrincipalAccessor _principalAccessor;

        public ChangeTaskHelper(IApprovalCommandService generalCommandService, IApprovalCommandMapper approvalCommandMapper, ILanguageChangeDetails languageChangeDetails, IMovingChangeDetail movingChangeDetail, ISecurityChangeDetail securityChangeDetail, IExpirationChangeDetails expirationChangeDetails, IPrincipalAccessor principalAccessor)
        {
            _generalCommandService = generalCommandService;
            _approvalCommandMapper = approvalCommandMapper;
            _languageChangeDetails = languageChangeDetails;
            _movingChangeDetail = movingChangeDetail;
            _securityChangeDetail = securityChangeDetail;
            _expirationChangeDetails = expirationChangeDetails;
            _principalAccessor = principalAccessor;
        }


        public ChangeTaskDetail GetData(int id)
        {
            var commandByApprovalId = _generalCommandService.GetCommandByApprovalId(id);
            var taskDetails = commandByApprovalId != null ? _approvalCommandMapper.Map(commandByApprovalId, _principalAccessor.Principal) : null;
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
                    customTask.Details = _expirationChangeDetails.GetExpirationCommandChangeDetails(taskDetails);
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

