using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xtricate.Web.Dashboard
{
    public class RouteCollection
    {
        public readonly List<Tuple<string, IRequestDispatcher>> Dispatchers
            = new List<Tuple<string, IRequestDispatcher>>();

        public void Add(string pathTemplate, IRequestDispatcher dispatcher)
        {
            if (pathTemplate == null) throw new ArgumentNullException(nameof(pathTemplate));
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));

            Dispatchers.Add(new Tuple<string, IRequestDispatcher>(pathTemplate, dispatcher));
        }

        public Tuple<IRequestDispatcher, Match> FindDispatcher(string path)
        {
            if (path.Length == 0) path = "/";

            foreach (var dispatcher in Dispatchers)
            {
                var pattern = dispatcher.Item1;

                if (!pattern.StartsWith("^", StringComparison.OrdinalIgnoreCase))
                    pattern = "^" + pattern;
                if (!pattern.EndsWith("$", StringComparison.OrdinalIgnoreCase))
                    pattern += "$";

                var match = Regex.Match(
                    path,
                    pattern,
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (match.Success)
                {
                    return new Tuple<IRequestDispatcher, Match>(dispatcher.Item2, match);
                }
            }

            return null;
        }
    }
}
