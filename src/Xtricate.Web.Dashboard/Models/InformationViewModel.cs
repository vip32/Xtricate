using System;
using System.Collections.Generic;
using System.Linq;

namespace Xtricate.Web.Dashboard.Models
{
    public class InformationViewModel
    {
        public InformationViewModel(
            RouteCollection routes, IEnumerable<string> javascripts = null, IEnumerable<string> stylesheets = null)
        {
            //if (routes == null) throw new ArgumentNullException(nameof(routes));
            //if (routes.Dispatchers == null) throw new ArgumentNullException("routes.Dispatchers");

            if(routes != null && routes.Dispatchers != null)
                Routes = routes.Dispatchers.Select(route => route.Item1);

            Javascripts = javascripts ?? new List<string>();
            Stylesheets = stylesheets ?? new List<string>();
        }

        public IEnumerable<string> Routes { get; set; }

        public IEnumerable<string> Javascripts { get; set; }

        public IEnumerable<string> Stylesheets { get; set; }
    }
}