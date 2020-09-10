//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using AdvancedTask.Business.AdvancedTask;
//using EPiServer.Approvals;

//namespace AdvancedTask.Helper
//{
//    internal static class ChangeApprovalExtensions
//    {
//        public static async Task<IEnumerable<ChangeApproval>> GetChangeApprovalItemsAsync(this IApprovalRepository repository, IEnumerable<int> ids)
//        {
//            return (await repository.GetItemsAsync(ids).ConfigureAwait(false)).OfType<ChangeApproval>();
//        }

//        public static async Task<ChangeApproval> GetChangeApprovalAsync(this IApprovalRepository repository, int approvalId)
//        {
//            return (await repository.GetChangeApprovalItemsAsync((IEnumerable<int>)new[] {approvalId}).ConfigureAwait(false)).SingleOrDefault();
//        }
//    }
//}
