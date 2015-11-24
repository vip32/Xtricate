using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    using BuildFunc = Action<
        Func<
            IDictionary<string, object>,
            Func<
                Func<IDictionary<string, object>, Task>,
                Func<IDictionary<string, object>, Task>
        >>>;
    using MidFunc = Func<
            Func<IDictionary<string, object>, Task>,
            Func<IDictionary<string, object>, Task>
            >;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MiddlewareExtensions
    {
        public static BuildFunc UseDashboard(
            this BuildFunc builder,
            DashboardOptions options,
            RouteCollection routes)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (routes == null) throw new ArgumentNullException(nameof(routes));

            builder(_ => UseDashboard(options, routes));

            return builder;
        }

        public static MidFunc UseDashboard(
            DashboardOptions options,
            RouteCollection routes)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (routes == null) throw new ArgumentNullException(nameof(routes));

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    var dispatcher = routes.FindDispatcher(context.Request.Path.Value);

                    if (dispatcher == null)
                    {
                        return next(env);
                    }

                    if (options.AuthorizationFilters.Any(filter => !filter.Authorize(context.Environment)))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return Task.FromResult(false);
                    }

                    var dispatcherContext = new RequestDispatcherContext(
                        options.Name,
                        options.AppPath,
                        context.Environment,
                        dispatcher.Item2);

                    return dispatcher.Item1.Dispatch(dispatcherContext);
                };
        }
    }
}