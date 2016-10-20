using System;
using System.Collections.Generic;

namespace Xtricate.Core.Templ
{
    public class TemplateDefinition
    {
        public string Key { get; set; }

        public IEnumerable<string> Tags { get; set; }

        //public CultureInfo Culture { get; set; }

        public Type TemplateType { get; set; }

        public Type ModelType { get; set; }
    }
}