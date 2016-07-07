using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using NUnit.Framework;
using Xtricate.Dynamic;

namespace Xtricate.UnitTests.Dynamic
{
    [TestFixture]
    public class ExpandoTests : AssertionHelper
    {
        [Test]
        public void AddAndReadDynamicIndexersTest()
        {
            var ex = new ExpandoInstance
            {
                Name = "Name",
                Entered = DateTime.Now
            };

            ex["Address"] = "Address 123";
            ex["Contacted"] = true;

            dynamic exd = ex;

            Assert.AreEqual(exd.Address, ex["Address"]);
            Assert.AreEqual(exd.Contacted, true);
        }

        [Test]
        public void AddAndReadDynamicPropertiesTest()
        {
            // strong typing first
            var ex = new User
            {
                Name = "Name",
                Email = "Name@whatsa.com"
            };

            // create dynamic and create new props
            dynamic exd = ex;
            exd.entered = DateTime.Now;
            exd.Company = "Company";
            exd.Accesses = 10;

            Assert.AreEqual(exd.Company, "Company");
            Assert.AreEqual(exd.Accesses, 10);
        }

        [Test]
        public void ChildObjectTest()
        {
            var user = new User();

            dynamic duser = user;

            // Set properties on dynamic object
            duser.Address = new Address();
            duser.Address.FullAddress = "Address 123";
            duser.Address.Phone = "111 222 333 444";

            Assert.AreEqual(duser.Address.Phone, "111 222 333 444");
        }

        [Test]
        public void DynamicAccessToPropertyTest()
        {
            // Set standard properties
            var ex = new ExpandoInstance
            {
                Name = "Name",
                Entered = DateTime.Now
            };

            // turn into dynamic
            dynamic exd = ex;

            // Dynamic can access
            Assert.AreEqual(ex.Name, exd.Name);
            Assert.AreEqual(ex.Entered, exd.Entered);
        }

        /// <summary>
        ///     Summary method that demonstrates some
        ///     of the basic behaviors.
        ///     More specific tests are provided below
        /// </summary>
        [Test]
        public void ExandoBasicTests()
        {
            // Set standard properties
            var ex = new User
            {
                Name = "Name",
                Email = "Name@email.com",
                Active = true
            };

            // set dynamic properties that don't exist on type
            dynamic exd = ex;
            exd.Entered = DateTime.Now;
            exd.Company = "Company";
            exd.Accesses = 10;

            // set dynamic properties as dictionary
            ex["Address"] = "Address 123";
            ex["Email"] = "Name@email.com";
            ex["TotalOrderAmounts"] = 51233.99M;

            // iterate over all properties dynamic and native
            foreach (var prop in ex.GetProperties(true))
            {
                Trace.WriteLine(prop.Key + " " + prop.Value);
            }

            // you can access plain properties both as explicit or dynamic
            Expect(ex.Name, EqualTo(exd.Name), "Name doesn't match");

            // You can access dynamic properties either as dynamic or via IDictionary
            Expect(exd.Company, EqualTo(ex["Company"] as string), "Company doesn't match");
            Expect(exd.Address, EqualTo(ex["Address"] as string), "Name doesn't match");

            // You can access strong type properties via the collection as well (inefficient though)
            Expect(ex.Name, EqualTo(ex["Name"] as string));

            // dynamic can access everything
            Expect(ex.Name, EqualTo(exd.Name)); // native property
            Expect(ex["TotalOrderAmounts"], EqualTo(exd.TotalOrderAmounts)); // dictionary property
        }

        [Test]
        public void ExpandoInstanceCanAcceptExtensionMetods()
        {
            var user = new User
            {
                Email = "Name@email.com",
                Password = "Password1",
                Name = "Name1",
                Active = true
            };

            dynamic duser = user;

            duser.Phone = "111 222 333 444";

            var json = JsonConvert.SerializeObject(user);
            Trace.WriteLine(json);
            Assert.IsTrue(!string.IsNullOrEmpty(json));

            Assert.Throws<RuntimeBinderException>(() => duser.Dump()); // does not contain a definition for 'Dump'
        }

        [Test]
        public void ExpandoMixinTest1()
        {
            // have Expando work on Addresses
            var user = new User(new Address());

            // cast to dynamicAccessToPropertyTest
            dynamic duser = user;

            // Set strongly typed properties
            duser.Email = "Name@email.com";
            user.Password = "Password1";

            // Set properties on address object
            duser.Address = "Address 123";
            duser.Phone = "111 222 333 444";

            // set dynamic properties
            duser.NonExistantProperty = "NonExistantProperty1";

            // shows default value Address.Phone value
            Trace.WriteLine(string.Format("phone:{0}", duser.Phone as string));
            Assert.AreEqual(user["Phone"], "111 222 333 444");
            Assert.AreEqual(duser.Phone, "111 222 333 444");
        }

        [Test]
        public void InheritedExpandoInstanceSerializesEverything()
        {
            var user = new SpecialUser
            {
                Name = "name",
                Country = "country" // special user property
            };

            var json = JsonConvert.SerializeObject(user);

            Trace.WriteLine(json);
            Assert.IsTrue(!string.IsNullOrEmpty(json));
            //Assert.IsTrue(json.Contains("Name") || json.Contains("name"));
            Assert.IsTrue(json.Contains("Country") || json.Contains("country"));
        }

        [Test]
        public void IterateOverDynamicPropertiesTest()
        {
            var ex = new ExpandoInstance
            {
                Name = "Name",
                Entered = DateTime.Now
            };

            dynamic exd = ex;
            exd.Company = "Company";
            exd.Accesses = 10;

            // Dictionary pseudo implementation
            ex["Count"] = 10;
            ex["Type"] = "MyType";

            // Dictionary Count - 2 dynamic props added
            Assert.IsTrue(ex.Properties.Count == 4);

            // iterate over all properties
            foreach (KeyValuePair<string, object> prop in exd.GetProperties(true))
            {
                Trace.WriteLine(prop.Key + " " + prop.Value);
            }
        }

        [Test]
        public void MixInObjectInstanceTest()
        {
            // Create expando an mix-in second objectInstanceTest
            var ex = new ExpandoInstance(new Address())
            {
                Name = "Name",
                Entered = DateTime.Now
            };

            // create dynamic
            dynamic exd = ex;

            // values should show Addresses initialized values (not null)
            Trace.WriteLine(string.Format("address:{0}", exd.FullAddress as string));
            Trace.WriteLine(string.Format("email:{0}", exd.Email as string));
            Trace.WriteLine(string.Format("phone:{0}", exd.Phone as string));
        }

        [Test]
        public void PropertyAsIndexerTest()
        {
            // Set standard properties
            var ex = new ExpandoInstance
            {
                Name = "Name",
                Entered = DateTime.Now
            };

            Assert.AreEqual(ex.Name, ex["Name"]);
            Assert.AreEqual(ex.Entered, ex["Entered"]);
        }

        [Test]
        public void ToExandoBasicTests()
        {
            // Set standard properties
            var ex = new User
            {
                Name = "Name",
                Email = "Name@email.com",
                Active = true
            };

            // set dynamic properties that don't exist on type
            ex.ToExpando().Entered = DateTime.Now;
            ex.ToExpando().Company = "Company";
            ex.ToExpando().Accesses = 10;

            // you can access plain properties both as explicit or dynamic
            Assert.AreEqual(ex.Name, ex.ToExpando().Name, "Name doesn't match");

            // You can access dynamic properties either as dynamic or via IDictionary
            Assert.AreEqual(ex.ToExpando().Company, ex["Company"] as string, "Company doesn't match");

            // You can access strong type properties via the collection as well (inefficient though)
            Assert.AreEqual(ex.Name, ex["Name"] as string);

            // dynamic can access everything
            Assert.AreEqual(ex.Name, ex.ToExpando().Name); // native property
        }

        [Test]
        public void TwoWayJsonSerializeExpandoTyped()
        {
            // Set standard properties
            var ex = new User
            {
                Name = "Name",
                Email = "Name@email.com",
                Password = "Password1",
                Active = true
            };

            // set dynamic properties
            dynamic exd = ex;
            exd.Entered = DateTime.Now;
            exd.Company = "Company";
            exd.Accesses = 10;

            // set dynamic properties as dictionary
            ex["Address"] = "Address 123";
            ex["Email"] = "Name@email.com";
            ex["TotalOrderAmounts"] = 51233.99M;

            // *** Should serialize both standard properties and dynamic properties
            var json = JsonConvert.SerializeObject(ex);
            Assert.IsTrue(!string.IsNullOrEmpty(json));
            Trace.WriteLine("*** Serialized Native object:");
            Trace.WriteLine(json);

            Assert.IsTrue(json.Contains("Name")); // standard
            Assert.IsTrue(json.Contains("Company")); // dynamic

            // *** Now deserialize the JSON back into object to
            // *** check for two-way serialization
            var user2 = JsonConvert.DeserializeObject<User>(json);
            Assert.IsNotNull(user2);
            json = JsonConvert.SerializeObject(user2);
            Assert.IsTrue(!string.IsNullOrEmpty(json));
            Trace.WriteLine("*** De-Serialized User2 object:");
            Trace.WriteLine(json);

            Assert.IsTrue(json.Contains("Name")); // standard
            Assert.IsTrue(json.Contains("Company")); // dynamic
        }

        [Test]
        public void TwoWayXmlSerializeExpandoTyped()
        {
            // Set standard properties
            var ex = new User {Name = "Name", Active = true};

            // set dynamic properties
            dynamic exd = ex;
            exd.Entered = DateTime.Now;
            exd.Company = "Company";
            exd.Accesses = 10;

            // set dynamic properties as dictionary
            ex["Address"] = "Address 123";
            ex["Email"] = "Name@email.com";
            ex["TotalOrderAmounts"] = 51233.99M;

            // Serialize creates both static and dynamic properties
            // dynamic properties are serialized as a 'collection'
            string xml;
            SerializationHelper.SerializeObject(exd, out xml);
            Trace.WriteLine("*** Serialized Dynamic object:");
            Trace.WriteLine(xml);

            Assert.IsTrue(xml.Contains("Name")); // static
            Assert.IsTrue(xml.Contains("Company")); // dynamic

            // Serialize
            var user2 = SerializationHelper.DeSerializeObject(xml, typeof (User));
            SerializationHelper.SerializeObject(exd, out xml);
            Trace.WriteLine(xml);

            Assert.IsTrue(xml.Contains("Name")); // static
            Assert.IsTrue(xml.Contains("Company")); // dynamic
        }

        //[Test]
        //public void ExpandoObjectJsonTest()
        //{
        //    dynamic ex = new ExpandoObject();
        //    ex.Name = "Name";
        //    ex.Entered = DateTime.Now;
        //    ex.Address = "Address 123";
        //    ex.Contacted = true;
        //    ex.Count = 10;
        //    ex.Completed = DateTime.Now.AddHours(2);

        //    string json = JsonConvert.SerializeObject(ex, Formatting.Indented);
        //    Assert.IsTrue(!string.IsNullOrEmpty(json));
        //    System.Diagnostics.Trace.WriteLine(json);
        //}

        [Test]
        public void UserExampleTest()
        {
            // Set strongly typed properties
            var user = new User
            {
                Email = "Name@email.com",
                Password = "Password1",
                Name = "Name1",
                Active = true
            };

            // Now add dynamic properties
            dynamic duser = user;
            duser.Entered = DateTime.Now;
            duser.Accesses = 1;

            // you can also add dynamic props via indexer
            user["NickName"] = "NickName1";
            duser["WebSite"] = "http://www.example.com";

            // Access strong type through dynamic ref
            Assert.AreEqual(user.Name, duser.Name);
            // Access strong type through indexer
            Assert.AreEqual(user.Password, user["Password"]);
            // access dyanmically added value through indexer
            Assert.AreEqual(duser.Entered, user["Entered"]);
            // access index added value through dynamic
            Assert.AreEqual(user["NickName"], duser.NickName);

            // loop through all properties dynamic AND strong type properties (true)
            foreach (var prop in user.GetProperties(true))
            {
                var val = prop.Value;
                if (val == null)
                    val = "null";

                Trace.WriteLine(prop.Key + ": " + val);
            }
        }
    }

    public class ExpandoInstance : Expando
    {
        public ExpandoInstance()
        {
        }

        /// <summary>
        ///     Allow passing in of an instance
        /// </summary>
        /// <param name="instance"></param>
        public ExpandoInstance(object instance)
            : base(instance)
        {
        }

        public string Name { get; set; }
        public DateTime Entered { get; set; }
    }

    public class Address
    {
        public Address()
        {
            FullAddress = "Address 123";
            Phone = "123 456 7890";
            Email = "Name@email.com";
        }

        public string FullAddress { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class User : Expando
    {
        public User()
        {
        }

        // only required if you want to mix in seperate instance
        public User(object instance) : base(instance)
        {
        }

        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public DateTime? ExpiresOn { get; set; }
    }

    public class SpecialUser : User
    {
        public string Country { get; set; }
    }

    public class VerySpecialUser : SpecialUser
    {
        public string Title { get; set; }
    }
}