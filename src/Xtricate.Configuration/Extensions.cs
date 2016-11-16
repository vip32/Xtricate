using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xtricate.Configuration
{
    public static class Extensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
        {
            return new HashSet<T>(items);
        }

        public static string Fmt(this string text, params object[] args)
        {
            return string.Format(text, args);
        }

        public static void Each<T>(this IEnumerable<T> values, Action<T> action)
        {
            if (values == null) return;

            foreach (var value in values)
            {
                action(value);
            }
        }

        public static void Each<T>(this IEnumerable<T> values, Action<int, T> action)
        {
            if (values == null) return;

            var i = 0;
            foreach (var value in values)
            {
                action(i++, value);
            }
        }

        public static List<To> Map<To, From>(this IEnumerable<From> items, Func<From, To> converter)
        {
            if (items == null)
                return new List<To>();

            var list = new List<To>();
            foreach (var item in items)
            {
                list.Add(converter(item));
            }

            return list;
        }

        public static List<To> Map<To>(this IEnumerable items, Func<object, To> converter)
        {
            if (items == null)
                return new List<To>();

            var list = new List<To>();
            foreach (var item in items)
            {
                list.Add(converter(item));
            }

            return list;
        }

        public static string GetOperationName(this Type type)
        {
            var typeName = type.FullName != null //can be null, e.g. generic types
                ? type.FullName.SplitOnFirst("[[").First() //Generic Fullname
                    .Replace(type.Namespace + ".", "") //Trim Namespaces
                    .Replace("+", ".") //Convert nested into normal type
                : type.Name;

            return type.IsGenericParameter ? "'" + typeName : typeName;
        }

        public static string[] SplitOnFirst(this string strVal, string needle)
        {
            if (strVal == null)
                return new string[0];
            var length = strVal.IndexOf(needle);
            if (length != -1)
            {
                var strArray = new string[2];
                var index1 = 0;
                var str1 = strVal.Substring(0, length);
                strArray[index1] = str1;
                var index2 = 1;
                var str2 = strVal.Substring(length + needle.Length);
                strArray[index2] = str2;
                return strArray;
            }

            var strArray1 = new string[1];
            var index = 0;
            var str = strVal;
            strArray1[index] = str;
            return strArray1;
        }

        public static string[] SplitOnLast(this string strVal, char needle)
        {
            if (strVal == null)
                return new string[0];
            var length = strVal.LastIndexOf(needle);
            if (length != -1)
            {
                var strArray = new string[2];
                var index1 = 0;
                var str1 = strVal.Substring(0, length);
                strArray[index1] = str1;
                var index2 = 1;
                var str2 = strVal.Substring(length + 1);
                strArray[index2] = str2;
                return strArray;
            }

            var strArray1 = new string[1];
            var index = 0;
            var str = strVal;
            strArray1[index] = str;
            return strArray1;
        }

        public static Dictionary<string, string> ParseKeyValueText(this string text, string delimiter = " ")
        {
            var dictionary = new Dictionary<string, string>();
            if (text == null)
                return dictionary;
            var source = ReadLines(text);
            foreach (var strArray in source.Select(line => SplitOnFirst(line, delimiter)))
            {
                var index = strArray[0].Trim();
                if (index.Length != 0 && !index.StartsWith("#"))
                    dictionary[index] = strArray.Length == 2 ? strArray[1].Trim() : null;
            }

            return dictionary;
        }

        private static IEnumerable<string> ReadLines(this string text)
        {
            var reader = new StringReader(text ?? "");
            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }
    }
}