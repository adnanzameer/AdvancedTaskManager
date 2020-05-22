using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedTask.Business.AdvancedTask
{
    public interface ICommandMetaDataRepository
    {
        CommandMetaData GetByApprovalId(int approvalId);
    }
}

