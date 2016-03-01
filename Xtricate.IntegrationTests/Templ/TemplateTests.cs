using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Xtricate.Dynamic;
using Xtricate.IntegrationTests.Templ.Templates;

namespace Xtricate.IntegrationTests.Templ
{
    [TestFixture]
    public class TemplateTests
    {
        [Test]
        public void Can_Template_With_Empty_Template()
        {
            var templ = new EmptyBody();
            var html = templ.ToString();
            Trace.WriteLine(html);

            Assert.That(html, Is.Not.Null);
            Assert.That(html, Is.Not.Empty);
        }

        [Test]
        public void Can_Template_With_Model_Und_Culture_Texts()
        {
            var template = new TestBody
            {
                Culture = "de-DE",
                Model = new PersonModel
                {
                    FirstName = "John",
                    LastName = "Doe"
                },
                Texts = new PropertyBag<IDictionary<string, string>>
                {
                    {
                        "de-DE", new Dictionary<string, string>()
                        {
                            {"welcome", "Wilkommen"}
                        }
                    },
                    {
                        "nl-NL", new Dictionary<string, string>()
                        {
                            {"welcome", "Welkom"}
                        }
                    }
                }
            };
            template.Model.ToExpando().DynamicPropery = "DYNAMIC";

            var html = template.ToString();
            Trace.WriteLine(html);

            Assert.That(html, Is.Not.Null);
            Assert.That(html, Is.Not.Empty);
            Assert.That(html.Contains("BODY"));
            Assert.That(html.Contains("LAYOUT"));
            Assert.That(html.Contains("HEADER"));
            Assert.That(html.Contains("FOOTER"));
            Assert.That(html.Contains(template.Model.FirstName));
            Assert.That(html.Contains(template.Model.LastName));
            Assert.That(html.Contains(template.Model.ToExpando().DynamicPropery));
            Assert.That(html.Contains("10.10.2000")); // de-DE
            Assert.That(html.Contains("Wilkommen")); // de-DE
            Assert.That(template.OutProperties.ContainsKey("subject"));
            Trace.WriteLine("outproperty[key] = " + template.OutProperties["subject"]);
        }
    }
    public class PersonModel : Expando
    {
        public DateTime Created => new DateTime(2000, 10, 10);
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Subject { get; set; }
    }
}