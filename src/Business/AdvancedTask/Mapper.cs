using System.Globalization;
using System.Security.Principal;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Business.AdvancedTask
{
    [ServiceConfiguration(typeof(ApprovalCommandMapper), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ApprovalCommandMapper
    {
        private readonly ViewModelMapper _mapper = new ViewModelMapper();
        private IContentRepository _contentRepository;
        private UIHelper _uiHelper;
        private ChangeApprovalHelper _changeApprovalHelper;

        public ApprovalCommandMapper(IContentRepository contentRepository, UIHelper uiHelper, ChangeApprovalHelper changeApprovalHelper)
        {
            _contentRepository = contentRepository;
            _uiHelper = uiHelper;
            _changeApprovalHelper = changeApprovalHelper;
            _mapper.Add<SecuritySettingCommand, ChangeTaskViewModel>();
            _mapper.Add<LanguageSettingCommand, ChangeTaskViewModel>();
            _mapper.Add<ExpirationDateSettingCommand, ChangeTaskViewModel>();
            _mapper.Add<MovingContentCommand, ChangeTaskViewModel>();
        }

        public virtual ChangeTaskViewModel Map(ChangeApprovalCommandBase approvalCommand, IPrincipal principal)
        {
            //var result = _changeApprovalHelper.GetChangeApprovalAsync(approvalCommand.ApprovalID).GetAwaiter().GetResult();
            //var approvalDefinition = result == null ? null : _changeApprovalHelper.GetApprovalDefinitionVersionAsync(result.DefinitionVersionID).GetAwaiter().GetResult();
            var commandViewModel1 = _mapper.Map(approvalCommand) as ChangeTaskViewModel;
            var name1 = approvalCommand is ICultureSpecificApprovalCommand specificApprovalCommand ? specificApprovalCommand.AppliedOnLanguageBranch : null;
            var cultureInfo = string.IsNullOrEmpty(name1) ? null : new CultureInfo(name1);
            var commandViewModel2 = commandViewModel1;
            var contentRepository = _contentRepository;
            var appliedOnContentLink = approvalCommand.AppliedOnContentLink;
            var settings = new LoaderOptions
            {
                new LanguageLoaderOption()
                {
                    Language = cultureInfo, FallbackBehaviour = LanguageBehaviour.FallbackWithMaster
                }
            };

            var name2 = contentRepository.Get<IContent>(appliedOnContentLink, settings)?.Name;

            if (commandViewModel2 != null)
                commandViewModel2.Name = name2;

            if (commandViewModel1 != null)
            {
                commandViewModel1.Status = (int) approvalCommand.CommandStatus;
                commandViewModel1.CanExecute = approvalCommand.CommandStatus == CommandMetaData.ChangeTaskApprovalStatus.InReview;
                var fullName = approvalCommand.GetType().FullName;
                if (fullName != null)
                    commandViewModel1.TypeIdentifier = fullName.ToLower();

                commandViewModel1.Id = approvalCommand.Id.ExternalId.ToString();
                commandViewModel1.IsCommandDataValid = approvalCommand.IsValid();
                commandViewModel1.CreatedBy = _uiHelper.GetDisplayNameForUser(commandViewModel1.CreatedBy);
                commandViewModel1.ChangedBy = _uiHelper.GetDisplayNameForUser(commandViewModel1.ChangedBy);
                //commandViewModel1.CanUserActOnHisOwnChanges = !string.Equals(approvalCommand.CreatedBy, principal.Identity.Name, StringComparison.OrdinalIgnoreCase) || approvalDefinition != null && approvalDefinition.SelfApprove;
                return commandViewModel1;
            }

            return null;
        }
    }
}
