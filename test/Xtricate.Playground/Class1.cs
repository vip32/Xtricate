using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NUnit.Framework;
using Xtricate.Configuration;
using Xtricate.DocSet;
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
            var options = new StorageOptions(new ConnectionStrings().Get("XtricateTestSqlDb"), schemaName: "StorageTests") { BufferedLoad = false };
            var connectionFactory = new SqlConnectionFactory();
            var storage = new DocStorage<MimeMessage>(connectionFactory, options, new SqlBuilder(), new JsonNetSerializer(), new Md5Hasher());

            //  REGISTER templates - all templates registered in kernel
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
                            ModelType = typeof (OrderConfirmationModel),
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

            // CREATE the template based on a model (MAILSERVICE)
            var model = new OrderConfirmationModel
            {
                FirstName = "John",
                LastName = "Doe",
                DeliveryDate = DateTime.Now.AddDays(2),
                Items = new List<OrderItemModel>
                {
                    new OrderItemModel {Name = "product1", Price = 9.99m, Quantity = 2, Sku = "sku1"},
                    new OrderItemModel {Name = "product2", Price = 3.99m, Quantity = 4, Sku = "sku2"},
                }
            };

            var templ = engine.GetTemplate(model, "OrderConfirmation", new[] { "shop", "de-DE" }, "de-DE");
            Assert.That(templ, Is.Not.Null);
            Assert.That(templ.Model, Is.Not.Null);

            var body = templ.ToString();
            Assert.That(body, Is.Not.Null.Or.Empty);
            Assert.That(templ.OutProperties.Any());
            Assert.That(templ.OutProperties.ContainsKey("subject"));
            Trace.WriteLine(body);
            Trace.WriteLine(templ.OutProperties["subject"]);

            // CREATE the message (MAILSERVICE)
            var message = new MimeMessage();
            message.Subject = templ.OutProperties["subject"];
            message.From.Add(new MailboxAddress("Joey", "joey@friends.com"));
            message.To.Add(new MailboxAddress("Alice", "alice@wonderland.com"));
            var builder = new BodyBuilder {HtmlBody = body};
            message.Body = builder.ToMessageBody();
            //message.Attachments

            // STORE message in docset (MAILSERVICE)
            var key = new Random().Next(10000, 99999);
            var messageStream = new MemoryStream();
            message.WriteTo(messageStream);
            Assert.That(messageStream, Is.Not.Null);
            messageStream.Position = 0;
            storage.Upsert(key, messageStream, new[] {"en-US"});

            // LOAD message from docst (JOB)
            message = MimeMessage.Load(storage.LoadData(key, new[] { "en-US" }).FirstOrDefault());
            Assert.That(message, Is.Not.Null);

            // SEND the docset message (JOB)
            using (var client = new SmtpClient(new ProtocolLogger("smtp.log")))
            {
                client.Connect("localhost", 25, SecureSocketOptions.None);
                //client.Authenticate("username", "password");
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }

    public class OrderConfirmationModel : Expando
    {
        public DateTime Created => new DateTime(2000, 10, 10);
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<OrderItemModel> Items { get; set; }
        public DateTime DeliveryDate { get; set; }
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
            get { return Quantity * Price; }
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