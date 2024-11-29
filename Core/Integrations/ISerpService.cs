
namespace Core.Integrations
{
    public interface ISerpService
    {
        Task<long> GetHitCount(string queryKey, string keyword, string engine);
    }
}
