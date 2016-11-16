using System;
using System.Collections.Generic;
using Microsoft.Owin;

namespace Xtricate.Web.Dashboard
{
    /// <summary>
    ///     Owin Request extensions.
    /// </summary>
    public static class OwinRequestExtensions
    {
        public static string GetFullPath(this IOwinRequest request)
        {
            return $"{request.Scheme}://{request.Uri.Host}{(request.Uri.IsDefaultPort ? string.Empty : $":{request.Uri.Port}")}{request.Uri.AbsolutePath}";
        }

        public static bool IsPost(this IOwinRequest request)
        {
            return request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Gets the combined request parameters from the form body, query string, and request headers.
        /// </summary>
        /// <param name="request">Owin request.</param>
        /// <returns>Dictionary of combined form body, query string, and request headers.</returns>
        public static Dictionary<string, string> GetParameters(this IOwinRequest request)
        {
            var bodyParameters = request.GetBodyParameters();
            var queryParameters = request.GetQueryParameters();
            var headerParameters = request.GetHeaderParameters();

            foreach(var item in queryParameters)
                bodyParameters.Add(item.Key, item.Value);
            foreach (var item in headerParameters)
                bodyParameters.Add(item.Key, item.Value);

            return bodyParameters;
        }

        /// <summary>
        ///     Gets the query string request parameters.
        /// </summary>
        /// <param name="request">Owin Request.</param>
        /// <returns>Dictionary of query string parameters.</returns>
        public static Dictionary<string, string> GetQueryParameters(this IOwinRequest request)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

            foreach (var pair in request.Query)
            {
                var value = GetJoinedValue(pair.Value);
                dictionary.Add(pair.Key, value);
            }

            return dictionary;
        }

        /// <summary>
        ///     Gets the form body request parameters.
        /// </summary>
        /// <param name="request">Owin Request.</param>
        /// <returns>Dictionary of form body parameters.</returns>
        public static Dictionary<string, string> GetBodyParameters(this IOwinRequest request)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            var formCollectionTask = request.ReadFormAsync();

            formCollectionTask.Wait();
            foreach (var pair in formCollectionTask.Result)
            {
                var value = GetJoinedValue(pair.Value);
                dictionary.Add(pair.Key, value);
            }

            return dictionary;
        }

        /// <summary>
        ///     Gets the header request parameters.
        /// </summary>
        /// <param name="request">Owin Request.</param>
        /// <returns>Dictionary of header parameters.</returns>
        public static Dictionary<string, string> GetHeaderParameters(this IOwinRequest request)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in request.Headers)
            {
                var value = GetJoinedValue(pair.Value);
                dictionary.Add(pair.Key, value);
            }

            return dictionary;
        }

        private static string GetJoinedValue(string[] value)
        {
            if (value != null)
                return string.Join(",", value);

            return null;
        }
    }
}