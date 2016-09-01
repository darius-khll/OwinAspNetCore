using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Owin;
using System;
using Microsoft.Owin.Diagnostics;

namespace OwinAspNetCore
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

        }

        public void Configure(IApplicationBuilder aspNetCoreApp, IHostingEnvironment env)
        {
            aspNetCoreApp.UseOwinApp(owinApp =>
            {
                if (env.IsDevelopment())
                {
                    owinApp.UseErrorPage(new ErrorPageOptions()
                    {
                        ShowCookies = true,
                        ShowEnvironment = true,
                        ShowExceptionDetails = true,
                        ShowHeaders = true,
                        ShowQuery = true,
                        ShowSourceCode = true
                    });
                }
                // owinApp.UseFileServer(); as like as asp.net core static files middleware
                // owinApp.UseStaticFiles(); as like as asp.net core static files middleware
                // owinApp.UseWebApi(); asp.net web api / odata / web hooks
                // owinApp.MapSignalR();

                owinApp.Use<AddSampleHeaderToResponseHeadersOwinMiddleware>();
            });
        }
    }

    public class AddSampleHeaderToResponseHeadersOwinMiddleware : OwinMiddleware
    {
        public AddSampleHeaderToResponseHeadersOwinMiddleware(OwinMiddleware next)
            : base(next)
        {

        }

        public async override Task Invoke(IOwinContext context)
        {
            //throw new InvalidOperationException("ErrorTest");

            //context.Response.Headers.Add("Test", new[] { context.Request.Uri.ToString() });

            await Next.Invoke(context);
        }
    }
}
