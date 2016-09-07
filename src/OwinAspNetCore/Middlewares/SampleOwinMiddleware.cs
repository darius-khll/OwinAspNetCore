using Autofac;
using Autofac.Integration.Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using System.Threading.Tasks;

namespace OwinAspNetCore.Middlewares
{
    public class SampleOwinMiddleware : OwinMiddleware
    {
        public SampleOwinMiddleware(OwinMiddleware next)
            : base(next)
        {

        }

        public async override Task Invoke(IOwinContext context)
        {
            // You've access to asp.net core http context in your owin middlewares, asp.net web api odata controllers, signalr hubs, etc.

            HttpContext aspNetCoreContext = (HttpContext)context.Environment["Microsoft.AspNetCore.Http.HttpContext"];

            // do what ever you want using context.Request & context.Response

            ISomeDependency getDependencyFromAspNetCoreHttpContext = aspNetCoreContext.RequestServices.GetService<ISomeDependency>();
            ISomeDependency getDependencyFromOwinContext = context.GetAutofacLifetimeScope().Resolve<ISomeDependency>();

            await Next.Invoke(context);
        }
    }
}
