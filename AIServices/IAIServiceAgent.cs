using System.Threading.Tasks;

namespace DBTransferProject.AIServices
{
    public interface IAIServiceAgent
    {
        Task<string> ProcessAsync(string input);
    }

}
