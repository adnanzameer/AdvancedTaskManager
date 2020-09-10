using System.Linq;
using System.Threading.Tasks;
using AdvancedTask.Business.AdvancedTask;
using EPiServer.Approvals;
using EPiServer.Cms.Shell.Service.Internal;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace AdvancedTask.Helper
{
    [ServiceConfiguration(typeof(ChangeApprovalHelper))]
    internal class ChangeApprovalHelper
    {
        private readonly IApprovalRepository _approvalRepository;
     

        public ChangeApprovalHelper(IApprovalRepository approvalRepository)
        {
            this._approvalRepository = approvalRepository;

        }

        public virtual async Task<Approval> GetChangeApprovalAsync(int id)
        {
            var list = await _approvalRepository.GetItemsAsync(new[]{id}).ConfigureAwait(false);
            return list.ToList().FirstOrDefault();
        }
    }
}
