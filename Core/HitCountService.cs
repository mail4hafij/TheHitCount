using Common;
using Common.Contract.Messaging;
using Core.LIB;

namespace Core
{
    public class HitCountService : HandlerBase, IHitCountService
    {
        public HitCountService(IHandlerCaller handlerCaller) : base(handlerCaller) { }

        public GetHitCountResp GetHitCount(GetHitCountReq req) => Process<GetHitCountReq, GetHitCountResp>(req);
    }
}
