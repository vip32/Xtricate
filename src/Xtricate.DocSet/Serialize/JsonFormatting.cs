using System;

namespace Xtricate.DocSet.Serialize
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class JsonFormatting : Attribute
    {
        public bool CamelCase { get; set; } = true;
    }
}
