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

            if (!pathTemplate.StartsWith("/"))
                pathTemplate = "/" + pathTemplate;

            if (Exists(pathTemplate))
                Remove(pathTemplate);
            Dispatchers.Add(new Tuple<string, IRequestDispatcher>(pathTemplate, dispatcher));
        }

        public void Remove(string pathTemplate)
        {
            if (pathTemplate == null) throw new ArgumentNullException(nameof(pathTemplate));

            Dispatchers.RemoveAll(d => d.Item1.Equals(pathTemplate));
        }

        public bool Exists(string pathTemplate)
        {
            if (pathTemplate == null) throw new ArgumentNullException(nameof(pathTemplate));

            return Dispatchers.Exists(d => d.Item1.Equals(pathTemplate));
        }

        public Tuple<IRequestDispatcher, Match> FindDispatcher(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
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