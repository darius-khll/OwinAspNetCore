using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace OwinAspNetCore.Middlewares
{
    public class SampleAspNetCoreMiddleware
    {
        private readonly RequestDelegate Next;

        public SampleAspNetCoreMiddleware(RequestDelegate next)
        {
            Next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            ISomeDependency getDependencyFromAspNetCoreHttpContext = context.RequestServices.GetService<ISomeDependency>();

            // do what ever you want using context.Request & context.Response
            await Next.Invoke(context);
        }
    }
}
