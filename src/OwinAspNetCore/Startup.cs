using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Owin;
using OwinAspNetCore.Middlewares;
using OwinAspNetCore.Models;
using System;
using System.Reflection;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

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
            autofacContainerBuilder.RegisterHubs(Assembly.GetExecutingAssembly());
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

                owinApp.Map("/odata", innerOwinApp => // you can use owin middleware branching as like as asp.net core middlewares branching
                {
                    HttpConfiguration webApiConfig = new HttpConfiguration();

                    webApiConfig.DependencyResolver = new AutofacWebApiDependencyResolver(AutofacContainer);

                    ODataModelBuilder odataMetadataBuilder = new ODataConventionModelBuilder();

                    odataMetadataBuilder.EntitySet<Product>("Products");

                    webApiConfig.MapODataServiceRoute(
                        routeName: "ODataRoute",
                        routePrefix: "", /*no odata anymore because we're in odata branch of owin requests, see owinApp.Map method*/
                        model: odataMetadataBuilder.GetEdmModel());

                    innerOwinApp.UseAutofacWebApi(webApiConfig);
                    innerOwinApp.UseWebApi(webApiConfig);
                });

                HubConfiguration signalRConfiguration = new HubConfiguration();
                signalRConfiguration.Resolver = new AutofacDependencyResolver(AutofacContainer);
                owinApp.MapSignalR("/signalr", signalRConfiguration); // MapSignalR uses owin branching internally.

            });

            appLifetime.ApplicationStopped.Register(() => AutofacContainer.Dispose());
        }
    }
}
