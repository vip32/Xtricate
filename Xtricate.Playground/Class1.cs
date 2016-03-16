using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Xtricate.Dynamic;
using Xtricate.Playground.Templates;
using Xtricate.Templ;

namespace Xtricate.Playground
{
    public class CommunicationService
    {
    }

    public class MailTemplateFactory
    {
        private readonly IEnumerable<MailTemplateDefinition> _templatesDefinitions;

        public MailTemplateFactory(IEnumerable<MailTemplateDefinition> templatesDefinitions)
        {
            _templatesDefinitions = templatesDefinitions;
        }

        public MailTemplate<TModel> Create<TModel>(string key, IEnumerable<string> tags, CultureInfo culture)
        {
            var definition = _templatesDefinitions.FirstOrDefault(t =>
                         t.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase ) &&
                         t.Culture.Equals(culture));
            if (definition == null) throw new Exception($"no template found for name={key} and culture={culture.Name}");
            var instance = (MailTemplate<TModel>)Activator.CreateInstance(definition.TemplateType, new object[] { });
            if (instance == null) throw new Exception($"template should be of type 'MailTemplate<{typeof(TModel).Name}>'");
            instance.Culture = culture;
            // TODO: set texts here
            return instance;
        }
    }

    public class MailTemplateDefinition
    {
        public string Key { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public CultureInfo Culture { get; set; }
        public Type TemplateType { get; set; }
        public Type ModelType { get; set; }
    }

    public abstract class MailTemplate<TModel> : MailTemplate
    {
        public TModel Model { get; set; }
    }

    public abstract class MailTemplate : Template
    {
    }

    [TestFixture]
    public class MailTests
    {
        [Test]
        public void Test1()
        {
            var templ = new TestEmailTemplate1();
            Assert.That(templ, Is.Not.Null);

            var templ2 = (TestEmailTemplate1)Activator.CreateInstance(
                        typeof(TestEmailTemplate1), new object[] { });
            Assert.That(templ2, Is.Not.Null);

            //var ctor = typeof(TestEmailTemplate1).GetConstructors()[0];
            //var o = FormatterServices.GetUninitializedObject(typeof(TestEmailTemplate1));
            ////return (MailTemplate<TModel>) ctor.Invoke(o, new object[] {});
            //var templ3 = ctor.Invoke(o, new object[] { });
            //Assert.That(templ3, Is.Not.Null);
        }

        [Test]
        public void Test2()
        {
            // all templates registered in kernel
            var factory = new MailTemplateFactory(
                new List<MailTemplateDefinition>
                {
                    new MailTemplateDefinition
                    {
                        Key = "OrderConfirmation",
                        Tags = null,
                        Culture = CultureInfo.GetCultureInfo("de-DE"),
                        TemplateType = typeof (TestEmailTemplate1),
                        ModelType = typeof (EmailTemplate1Model),
                    }
                });

            // called from > controller > service > factory
            var templ = factory.Create<EmailTemplate1Model>("OrderConfirmation", null, CultureInfo.GetCultureInfo("de-DE"));
            Assert.That(templ, Is.Not.Null);
            templ.Model = new EmailTemplate1Model
            {
                FirstName = "John",
                LastName = "Doe"
            };
            templ.Texts = new PropertyBag<IDictionary<string, string>>
            {
                {
                    CultureInfo.GetCultureInfo("de-DE").Name, new Dictionary<string, string>()
                    {
                        {"welcome", "Wilkommen"}
                    }
                },
                {
                    CultureInfo.GetCultureInfo("nl-NL").Name, new Dictionary<string, string>()
                    {
                        {"welcome", "Welkom"}
                    }
                }
            };

            var body = templ.ToString();
            Assert.That(body, Is.Not.Null.Or.Empty);
            Assert.That(templ.OutProperties.Any());
            Assert.That(templ.OutProperties.ContainsKey("subject"));
            Trace.WriteLine(body);
            Trace.WriteLine(templ.OutProperties["subject"]);
        }
    }

    public class EmailTemplate1Model : Expando
    {
        public DateTime Created => new DateTime(2000, 10, 10);
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    //public class MailMessage
    //{

    //}

    //public class MailAttachment
    //{
    //    public string Name { get; set; }
    //    byte[] Data { get; set; }
    //}
}
