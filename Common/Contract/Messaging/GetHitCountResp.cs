
namespace Common.Contract.Messaging
{
    public class GetHitCountResp : Resp
    {
        public Dictionary<string, List<long>> Hits { get; set; }
    }
}
