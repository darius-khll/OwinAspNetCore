using Autofac;
using Autofac.Integration.Owin;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OwinAspNetCore.Middlewares
{
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
}
