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

    //public abstract class MailTemplate<TModel> : MailTemplate
    //{
    //    public TModel Model { get; set; }
    //}

    //public abstract class MailTemplate : Template
    //{
    //}

    [TestFixture]
    public class MailTests
    {
        [Test]
        public void Test1()
        {
            var templ = new OrderConfirmationTemplate();
            Assert.That(templ, Is.Not.Null);

            var templ2 = (OrderConfirmationTemplate)Activator.CreateInstance(
                        typeof(OrderConfirmationTemplate), new object[] { });
            Assert.That(templ2, Is.Not.Null);

            //var ctor = typeof(OrderConfirmationTemplate).GetConstructors()[0];
            //var o = FormatterServices.GetUninitializedObject(typeof(OrderConfirmationTemplate));
            ////return (MailTemplate<TModel>) ctor.Invoke(o, new object[] {});
            //var templ3 = ctor.Invoke(o, new object[] { });
            //Assert.That(templ3, Is.Not.Null);
        }

        [Test]
        public void Test2()
        {
            // all templates registered in kernel
            var engine = new TemplateEngine(
                new TemplateFactory(
                    new[]
                    {
                        new TemplateDefinition
                        {
                            Key = "OrderConfirmation",
                            Tags = new[] {"shop", "de-DE"},
                            //Culture = CultureInfo.GetCultureInfo("de-DE"),
                            TemplateType = typeof (OrderConfirmationTemplate),
                            ModelType = typeof (OrderModel),
                        }
                    }),
                new PropertyBag<IDictionary<string, string>>
                {
                    {
                        CultureInfo.GetCultureInfo("de-DE").Name, new Dictionary<string, string>
                        {
                            {"welcome", "Wilkommen"}
                        }
                    },
                    {
                        CultureInfo.GetCultureInfo("nl-NL").Name, new Dictionary<string, string>
                        {
                            {"welcome", "Welkom"}
                        }
                    }
                });

            // called from > controller > service > factory
            var model = new OrderModel
            {
                FirstName = "John",
                LastName = "Doe",
                Items = new List<OrderItemModel>
                {
                    new OrderItemModel {Name = "product1", Price = 9.99m, Quantity = 2, Sku = "sku1"},
                    new OrderItemModel {Name = "product2", Price = 3.99m, Quantity = 4, Sku = "sku2"},
                }
            };
            var templ = engine.GetTemplate(model, "OrderConfirmation", new[] {"shop"}, "de-DE");
            Assert.That(templ, Is.Not.Null);
            Assert.That(templ.Model, Is.Not.Null);

            var body = templ.ToString();
            Assert.That(body, Is.Not.Null.Or.Empty);
            Assert.That(templ.OutProperties.Any());
            Assert.That(templ.OutProperties.ContainsKey("subject"));
            Trace.WriteLine(body);
            Trace.WriteLine(templ.OutProperties["subject"]);
        }
    }

    public class OrderModel : Expando
    {
        public DateTime Created => new DateTime(2000, 10, 10);
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<OrderItemModel> Items { get; set; }
    }

    public class OrderItemModel
    {
        public DateTime Created => new DateTime(2000, 10, 10);
        public string Sku { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal Total
        {
            get { return Quantity*Price; }
        }
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
