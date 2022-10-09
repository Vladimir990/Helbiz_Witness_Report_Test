using Helbiz_Witness_Reports_Test.Models;
using System.Threading.Tasks;

namespace Helbiz_Witness_Reports_Test.Services
{
    public interface IReportsService
    {
        Task<bool> GetReportsByTitleAsync(string title, string phoneNumber);
    }
}
