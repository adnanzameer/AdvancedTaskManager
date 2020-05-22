using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Approvals;
using EPiServer.Approvals.ContentApprovals;
using EPiServer.Cms.Shell.Service.Internal;
using EPiServer.Core;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Business.AdvancedTask.Helper
{
    [ServiceConfiguration(typeof(ChangeApprovalHelper))]
    public class ChangeApprovalHelper
    {
        private readonly IApprovalRepository _approvalRepository;
        private readonly IApprovalDefinitionRepository _approvalDefinitionRepository;
        private readonly ContentLoaderService _contentLoaderService;
        private readonly IApprovalDefinitionVersionRepository _approvalDefinitionVersionRepository;
        private readonly SecurityEntityProvider _securityEntityProvider;

        public ChangeApprovalHelper(
          IApprovalRepository approvalRepository,
          IApprovalDefinitionRepository approvalDefinitionRepository,
          IApprovalDefinitionVersionRepository approvalDefinitionVersionRepository,
          ContentLoaderService contentLoaderService,
          SecurityEntityProvider securityEntityProvider)
        {
            this._approvalRepository = approvalRepository;
            this._approvalDefinitionRepository = approvalDefinitionRepository;
            this._approvalDefinitionVersionRepository = approvalDefinitionVersionRepository;
            this._contentLoaderService = contentLoaderService;
            this._securityEntityProvider = securityEntityProvider;
        }

        public virtual async Task<ApprovalDefinitionResolveResult> ResolveApprovalDefinitionAsync(
          ContentReference contentLink)
        {
            return await this._approvalDefinitionRepository.ResolveAsync(contentLink).ConfigureAwait(false);
        }

        public virtual async Task<ContentApprovalDefinition> GetApprovalRelatedDefinitionAsync(
          ContentReference contentLink)
        {
            var changeApprovalAsync = await this.GetChangeApprovalAsync(contentLink);
            if (changeApprovalAsync == null)
                throw new MissingApprovalException("The approval definition related to the given contentLink doesn't exist.");
            this.ValidateContent(contentLink, AccessLevel.Read);
            return await this.GetApprovalDefinitionVersionAsync(changeApprovalAsync.DefinitionVersionID);
        }

        public virtual async Task<ChangeApproval> GetChangeApprovalAsync(
          ContentReference contentLink)
        {
            return await this._approvalRepository.GetChangeApprovalAsync(contentLink.ToReferenceWithoutVersion()).ConfigureAwait(false);
        }

        public virtual async Task<ChangeApproval> GetChangeApprovalAsync(
          int id)
        {
            return await this._approvalRepository.GetChangeApprovalAsync(id).ConfigureAwait(false);
        }

        public virtual async Task<ContentApprovalDefinition> GetApprovalDefinitionVersionAsync(
          int definitionVersionId)
        {
            return await this._approvalDefinitionVersionRepository.GetAsync(definitionVersionId).ConfigureAwait(false) as ContentApprovalDefinition;
        }

        public virtual IEnumerable<string> GetUsersFromApprovalDefinitionReviewers(
          IEnumerable<ApprovalDefinitionReviewer> reviewers)
        {
            var source = new List<string>();
            foreach (var reviewer in reviewers)
            {
                IEnumerable<string> strings;
                if (reviewer.ReviewerType != ApprovalDefinitionReviewerType.User)
                    strings = this._securityEntityProvider.GetUsersInRole(reviewer.Name);
                else
                    strings = ((IEnumerable<string>)new string[1]
                    {
            reviewer.Name
                    }).AsEnumerable<string>();
                var collection = strings;
                source.AddRange(collection);
            }
            return source.Distinct<string>();
        }

        private void ValidateContent(ContentReference contentLink, AccessLevel accessLevel)
        {
            if (this._contentLoaderService.Get<IContent>(contentLink, accessLevel) is ContentAssetFolder)
                throw new ContentNotFoundException();
        }
    }
}
