using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Ploeh.AutoFixture;
using Serilog;

namespace Xtricate.Service.Dashboard.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        private static readonly IEnumerable<Product> Products;
        private static readonly ILogger Logger;

        static ProductsController()
        {
            Logger = Log.ForContext<ProductsController>();
            Products = new Fixture().CreateMany<Product>(500).OrderBy(p => p.Id).ToList();
            Logger.Debug("created the products {ProductCount}", Products.Count());
        }

        [Route("")]
        // GET: api/Products
        public IEnumerable<Product> Get()
        {
            Logger.Debug("amount of products {ProductCount}, {CorrelationId}, {Unk}", Products.Count(), Guid.NewGuid().ToString(), 123);
            return Products;
        }

        [Route("{id:int}")]
        // GET: api/Products/5
        public Product Get(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            Logger.Debug("product with id {id} {@Product}", id, product);
            return product;
        }

        // POST: api/Products
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Products/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Products/5
        public void Delete(int id)
        {
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Sku { get; set; }
    }
}