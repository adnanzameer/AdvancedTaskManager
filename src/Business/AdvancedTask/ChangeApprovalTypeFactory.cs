
using System;
using AdvancedTask.Helper;
using EPiServer.Approvals;

namespace AdvancedTask.Business.AdvancedTask
{
    internal class ChangeApprovalTypeFactory : IApprovalTypeFactory
    {
        public const string ChangeApprovalType = "changeapproval";
        private readonly IApprovalDefinitionReferenceResolver _approvalReferenceResolver;

        public ChangeApprovalTypeFactory(IApprovalDefinitionReferenceResolver approvalReferenceResolver)
        {
            _approvalReferenceResolver = approvalReferenceResolver;
        }

        public string ApprovalType
        {
            get
            {
                return ChangeApprovalType;
            }
        }

        public IApprovalDefinitionReferenceResolver DefinitionReferenceResolver
        {
            get
            {
                return _approvalReferenceResolver;
            }
        }

        public IApprovalLanguageResolver LanguageResolver
        {
            get
            {
                return null;
            }
        }

        public Approval CreateApproval(Uri reference)
        {
            return new ChangeApproval()
            {
                ContentLink = ChangeApprovalReferenceHelper.GetContentReference(reference)
            };
        }

        public ApprovalDefinition CreateApprovalDefinition(Uri reference)
        {
            throw new NotSupportedException("Change Approval does not support creating an approval definition.");
        }
    }
}