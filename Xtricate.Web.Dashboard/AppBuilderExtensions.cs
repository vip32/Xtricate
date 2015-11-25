using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Owin.Infrastructure;
using Owin;

namespace Xtricate.Web.Dashboard
{
    using BuildFunc = Action<
                    Func<
                        IDictionary<string, object>,
                        Func<
                            Func<IDictionary<string, object>, Task>,
                            Func<IDictionary<string, object>, Task>
                    >>>;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseDashboard(
            this IAppBuilder builder,
            string pathMatch = null,
            DashboardOptions options = null,
            RouteCollection routes = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (string.IsNullOrEmpty(pathMatch)) pathMatch = "/dashboard";
            if (options == null) options = new DashboardOptions();
            if (routes == null) routes = new RouteCollectionBuilder().Routes;

            SignatureConversions.AddConversions(builder);

            builder.Map(pathMatch, subApp => subApp
                .UseOwin()
                .UseDashboard(options, routes));

            return builder;
        }

        private static BuildFunc UseOwin(this IAppBuilder builder)
        {
            return middleware => builder.Use(middleware(builder.Properties));
        }
    }
}