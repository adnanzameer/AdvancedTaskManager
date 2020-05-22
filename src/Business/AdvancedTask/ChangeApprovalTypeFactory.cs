
using System;
using EPiServer.Approvals;

namespace AdvancedTask.Business.AdvancedTask
{
    internal class ChangeApprovalTypeFactory : IApprovalTypeFactory
    {
        public static string ChangeApprovalType = "changeapproval";
        private readonly IApprovalDefinitionReferenceResolver _approvalReferenceResolver;

        public ChangeApprovalTypeFactory(IApprovalDefinitionReferenceResolver approvalReferenceResolver)
        {
            _approvalReferenceResolver = approvalReferenceResolver;
        }

        public string ApprovalType
        {
            get
            {
                return ChangeApprovalTypeFactory.ChangeApprovalType;
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
                return (IApprovalLanguageResolver)null;
            }
        }

        public Approval CreateApproval(Uri reference)
        {
            return (Approval)new ChangeApproval()
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