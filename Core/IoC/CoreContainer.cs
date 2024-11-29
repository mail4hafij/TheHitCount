using Autofac;
using Common;
using Common.Contract.Messaging;
using Core.Handler;
using Core.Integrations;
using Core.LIB;

namespace Core.IoC
{
    public class CoreContainer
    {
        public static void Bind(ContainerBuilder builder)
        {
            // LIB (request handler library)
            builder.RegisterType<ResponseFactory>().As<IResponseFactory>();
            builder.RegisterType<HandlerCaller>().As<IHandlerCaller>();
            builder.RegisterType<RequestHandlerFactory>().As<IRequestHandlerFactory>();

            // HitCountService
            builder.RegisterType<HitCountService>().As<IHitCountService>();

            // All integrations
            builder.RegisterType<SerpService>().As<ISerpService>();

            // All handlers
            builder.RegisterType<GetHitCountHandler>().As<RequestHandler<GetHitCountReq, GetHitCountResp>>();
        }
    }
}
