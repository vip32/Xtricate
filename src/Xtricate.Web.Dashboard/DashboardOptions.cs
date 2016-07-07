using System.Collections.Generic;

namespace Xtricate.Web.Dashboard
{
    public class DashboardOptions
    {
        public DashboardOptions()
        {
            Name = "Dashboard";
            AppPath = "/";
            //AuthorizationFilters = new[] { new LocalRequestsOnlyAuthorizationFilter() };
            AuthorizationFilters = new[] {new NoAuthorizationFilter()};
            MenuRoutes = new List<string>();
        }

        public string Name { get; set; }

        public string AppPath { get; set; }

        public IEnumerable<IAuthorizationFilter> AuthorizationFilters { get; set; }

        public IEnumerable<string> MenuRoutes { get; set; }
    }
}