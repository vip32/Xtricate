using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
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

        public MailTemplate<TModel> Create<TModel>(string name, CultureInfo culture)
        {
            var definition = _templatesDefinitions.FirstOrDefault(t =>
                         t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase ) &&
                         t.Culture.Equals(culture));
            if (definition == null) throw new Exception($"no template found for name={name} and culture={culture.Name}");
            var instance = (MailTemplate<TModel>)Activator.CreateInstance(definition.Type, new object[] { });
            if (instance == null) throw new Exception($"template should be of type 'MailTemplate<{typeof(TModel).Name}>'");
            instance.Culture = culture;
            // TODO: set texts
            return instance;
        }
    }

    public class MailTemplateDefinition
    {
        public string Name { get; set; }
        public CultureInfo Culture { get; set; }
        public Type Type { get; set; }
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
            var factory = new MailTemplateFactory(
                new List<MailTemplateDefinition>
                {
                    new MailTemplateDefinition
                    {
                        Name = "OrderConfirmation",
                        Culture = CultureInfo.GetCultureInfo("de-DE"),
                        Type = typeof (TestEmailTemplate1)
                    }
                });

            var templ = factory.Create<EmailModel>("OrderConfirmation", CultureInfo.GetCultureInfo("de-DE"));
            Assert.That(templ, Is.Not.Null);
            templ.Model = new EmailModel();

            var body = templ.ToString();
            Assert.That(body, Is.Not.Null.Or.Empty);
            Trace.WriteLine(body);
        }
    }

    public class EmailModel : Expando
    {
        public DateTime Created => new DateTime(2000, 10, 10);
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Subject { get; set; }
        public string To { get; set; }
        public string From { get; set; }
    }
}
