using AdvancedTask.Business.AdvancedTask.Mapper;
using AdvancedTask.Models;
using EPiServer;
using EPiServer.Cms.Shell.Service.Internal;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Security;

namespace AdvancedTask.Helper
{
    public class ChangeTaskHelper
    {
        private readonly ApprovalCommandService _generalCommandService;
        private readonly ApprovalCommandMapper _approvalCommandMapper;

        public ChangeTaskHelper(ApprovalCommandService generalCommandService, ApprovalCommandMapper approvalCommandMapper, LocalizationService localizationService, ILanguageBranchRepository languageBranchRepository, ContentLanguageSettingRepository contentLanguageSettingRepository, IContentLanguageSettingsHandler contentLanguageSettingsHandler, IContentRepository contentRepository, ContentLoaderService contentLoaderService)
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

