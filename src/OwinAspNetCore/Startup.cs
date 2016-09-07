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
using Autofac.Integration.Owin;
using System.Reflection;
using Autofac.Integration.WebApi;

namespace OwinAspNetCore
{
    public class Startup
    {
        public IContainer AutofacContainer { get; private set; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            ContainerBuilder autofacContainerBuilder = new ContainerBuilder();
            autofacContainerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            autofacContainerBuilder.RegisterType<SomeDependency>().As<ISomeDependency>().InstancePerLifetimeScope();
            autofacContainerBuilder.Populate(services);
            AutofacContainer = autofacContainerBuilder.Build();

            return new AutofacServiceProvider(AutofacContainer);
        }

        public void Configure(IApplicationBuilder aspNetCoreApp, IApplicationLifetime appLifetime)
        {
            aspNetCoreApp.UseMvc();

            aspNetCoreApp.UseMiddleware<SampleAspNetCoreMiddleware>();

            aspNetCoreApp.UseOwinApp(owinApp =>
            {
                owinApp.Use<ExtendAspNetCoreAutofacLifetimeToOwinMiddleware>();
                // Use ExtendAspNetCoreAutofacLifetimeToOwinMiddleware instead of owinApp.UseAutofacMiddleware(AutofacContainer); because that middleware will create autofac lifetime scope from scratch,
                // But our middleware will get lifetime scope from asp net core http context object, and after that, signalr & asp.net web api odata and other owin middlewares can use that lifetime scope.

                owinApp.Use<SampleOwinMiddleware>();

                // owinApp.UseWebApi(); asp.net web api / odata / web hooks

                HttpConfiguration webApiConfig = new HttpConfiguration();

                webApiConfig.DependencyResolver = new AutofacWebApiDependencyResolver(AutofacContainer);

                ODataModelBuilder odataMetadataBuilder = new ODataConventionModelBuilder();

                odataMetadataBuilder.EntitySet<Product>("Products");

                webApiConfig.MapODataServiceRoute(
                    routeName: "ODataRoute",
                    routePrefix: "odata",
                    model: odataMetadataBuilder.GetEdmModel());

                owinApp.UseAutofacWebApi(webApiConfig);
                owinApp.UseWebApi(webApiConfig);

                //owinApp.MapSignalR();
            });

            appLifetime.ApplicationStopped.Register(() => AutofacContainer.Dispose());
        }
    }

    public class ExtendAspNetCoreAutofacLifetimeToOwinMiddleware : OwinMiddleware
    {
        public ExtendAspNetCoreAutofacLifetimeToOwinMiddleware(OwinMiddleware next)
            : base(next)
        {

        }

        static ExtendAspNetCoreAutofacLifetimeToOwinMiddleware()
        {
            Type autofacConstantsType = typeof(OwinContextExtensions).Assembly.GetType("Autofac.Integration.Owin.Constants");

            FieldInfo owinLifetimeScopeKeyField = autofacConstantsType.GetField("OwinLifetimeScopeKey", BindingFlags.Static | BindingFlags.NonPublic);

            owinLifetimeScopeKey = (string)owinLifetimeScopeKeyField.GetValue(null);
        }

        private static readonly string owinLifetimeScopeKey;

        public async override Task Invoke(IOwinContext context)
        {
            // You've access to asp.net core http context in your owin middlewares, asp.net web api odata controllers, signalr hubs, etc.

            HttpContext aspNetCoreContext = (HttpContext)context.Environment["Microsoft.AspNetCore.Http.HttpContext"];

            // do what ever you want using context.Request & context.Response

            ILifetimeScope autofacScope = aspNetCoreContext.RequestServices.GetService<ILifetimeScope>();

            context.Set(owinLifetimeScopeKey, autofacScope);

            await Next.Invoke(context);
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

            ISomeDependency getDependencyFromAspNetCoreHttpContext = aspNetCoreContext.RequestServices.GetService<ISomeDependency>();
            ISomeDependency getDependencyFromOwinContext = context.GetAutofacLifetimeScope().Resolve<ISomeDependency>();

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
            ISomeDependency getDependencyFromAspNetCoreHttpContext = context.RequestServices.GetService<ISomeDependency>();

            // do what ever you want using context.Request & context.Response
            await Next.Invoke(context);
        }
    }
}
