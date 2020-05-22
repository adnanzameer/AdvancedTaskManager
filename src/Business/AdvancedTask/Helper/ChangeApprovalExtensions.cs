using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Approvals;
using EPiServer.Core;
using EPiServer.Framework;

namespace AdvancedTask.Business.AdvancedTask.Helper
{
    public static class ChangeApprovalExtensions
    {
        public static async Task<IEnumerable<ChangeApproval>> GetChangeApprovalItemsAsync(
          this IApprovalRepository repository,
          IEnumerable<ContentReference> contentLinks)
        {
            Validator.ThrowIfNull("references", (object)contentLinks);
            var source = await repository.GetItemsAsync(contentLinks.Select<ContentReference, Uri>((Func<ContentReference, Uri>)(cr => ChangeApprovalReferenceHelper.GetUri(cr, false)))).ConfigureAwait(false);
            return source == null ? (IEnumerable<ChangeApproval>)null : source.OfType<ChangeApproval>();
        }

        public static async Task<IEnumerable<ChangeApproval>> GetChangeApprovalItemsAsync(
          this IApprovalRepository repository,
          IEnumerable<int> ids)
        {
            return (await repository.GetItemsAsync(ids).ConfigureAwait(false)).OfType<ChangeApproval>();
        }

        public static async Task<ChangeApproval> GetChangeApprovalAsync(
          this IApprovalRepository repository,
          int approvalId)
        {
            return (await repository.GetChangeApprovalItemsAsync((IEnumerable<int>)new int[1]
            {
        approvalId
            }).ConfigureAwait(false)).SingleOrDefault<ChangeApproval>();
        }

        public static async Task<ChangeApproval> GetChangeApprovalAsync(
          this IApprovalRepository repository,
          ContentReference contentLink)
        {
            var source = await repository.GetChangeApprovalItemsAsync((IEnumerable<ContentReference>)new ContentReference[1]
            {
        contentLink
            }).ConfigureAwait(false);
            return source != null ? source.SingleOrDefault<ChangeApproval>() : (ChangeApproval)null;
        }

        public static bool IsChangeApproval(this ApprovalEventArgs eventArgs)
        {
            return eventArgs?.ApprovalReference != (Uri)null && eventArgs.ApprovalReference.Scheme.Equals(ChangeApprovalTypeFactory.ChangeApprovalType);
        }
    }
}
