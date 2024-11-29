using Common.Contract.Messaging;

namespace Common
{
    public interface IHitCountService
    {
        GetHitCountResp GetHitCount(GetHitCountReq req);
    }
}
