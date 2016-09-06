using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNetCore.Http;
using System.Web.OData.Builder;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.AspNetCore.Hosting;
using Autofac;
using System;
using Autofac.Extensions.DependencyInjection;

namespace OwinAspNetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {

        }

        public IContainer AutofacContainer { get; private set; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            ContainerBuilder autofacContainerBuilder = new ContainerBuilder();

            autofacContainerBuilder.RegisterType<SomeDependency>().As<ISomeDependency>();
            autofacContainerBuilder.Populate(services);
            AutofacContainer = autofacContainerBuilder.Build();

            return new AutofacServiceProvider(AutofacContainer);
        }

        public void Configure(IApplicationBuilder aspNetCoreApp, IApplicationLifetime appLifetime)
        {
            aspNetCoreApp.UseMvc();

            aspNetCoreApp.UseOwinApp(owinApp =>
            {
                // owinApp.UseWebApi(); asp.net web api / odata / web hooks

                HttpConfiguration webApiConfig = new HttpConfiguration();

                ODataModelBuilder odataMetadataBuilder = new ODataConventionModelBuilder();

                odataMetadataBuilder.EntitySet<Product>("Products");

                webApiConfig.MapODataServiceRoute(
                    routeName: "ODataRoute",
                    routePrefix: "odata",
                    model: odataMetadataBuilder.GetEdmModel());

                owinApp.UseWebApi(webApiConfig);

                //owinApp.MapSignalR();

                owinApp.Use<SampleOwinMiddleware>();
            });

            aspNetCoreApp.UseMiddleware<SampleAspNetCoreMiddleware>();

            appLifetime.ApplicationStopped.Register(() => this.AutofacContainer.Dispose());
        }
    }

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

            await Next.Invoke(context);
        }
    }

    public class SampleAspNetCoreMiddleware
    {
        private readonly RequestDelegate Next;

        public SampleAspNetCoreMiddleware(RequestDelegate next)
        {
            Next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // do what ever you want using context.Request & context.Response
            await Next.Invoke(context);
        }
    }
}
