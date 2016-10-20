using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Serilog;
//using Xunit;
//using Xtricate.Templ;

namespace Xtricate.Core.IntegrationTests.Net46
{
    public class TemplateTests
    {
        public TemplateTests()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Trace(outputTemplate: "{Timestamp:u} [{Level}] {SourceContext}:: {CorrelationId} {Message}{NewLine}{Exception}")
                .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:u} [{Level}] {SourceContext}:: {CorrelationId} {Message}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        //[Fact]
        public void Can_Template_With_Model_Und_Culture_Texts()
        {
            //var t = new TemplateDefinition();

            //var template = new TestBody
            //{
            //    Culture = new CultureInfo("de-DE"),
            //    Model = new PersonModel
            //    {
            //        FirstName = "John",
            //        LastName = "Doe"
            //    },
            //    //Texts = new PropertyBag<IDictionary<string, string>>
            //    //{
            //    //    {
            //    //        new CultureInfo("de-DE").Name, new Dictionary<string, string>()
            //    //        {
            //    //            {"welcome", "Wilkommen"}
            //    //        }
            //    //    },
            //    //    {
            //    //        new CultureInfo("nl-NL").Name, new Dictionary<string, string>()
            //    //        {
            //    //            {"welcome", "Welkom"}
            //    //        }
            //    //    }
            //    //}
            //};
            ////template.Model.ToExpando().DynamicPropery = "DYNAMIC";

            //var html = template.ToString();
            //Trace.WriteLine(html);

            //Assert.NotNull(html);
            //Assert.NotEmpty(html);
            //Assert.Contains(html, "BODY");
            //Assert.Contains(html, "LAYOUT");
            //Assert.Contains(html, "HEADER");
            //Assert.Contains(html, "FOOTER");
            //Assert.Contains(html, template.Model.FirstName);
            //Assert.Contains(html, template.Model.LastName);
            ////Assert.Contains(html, template.Model.ToExpando().DynamicPropery);
            //Assert.Contains(html, "10.10.2000"); // de-DE
            ////Assert.Contains(html, "Wilkommen"); // de-DE
            //Assert.Contains(template.OutProperties, "subject");
            //Trace.WriteLine("outproperty[key] = " + template.OutProperties["subject"]);
        }
    }

    public class PersonModel //: Expando
    {
        public DateTime Created => new DateTime(2000, 10, 10);
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Subject { get; set; }
    }
}