using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Ploeh.AutoFixture;

namespace Xtricate.Service.Dashboard.Controllers
{
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        private static readonly IEnumerable<Product> Products;

        static ProductsController()
        {
            Products = new Fixture().CreateMany<Product>(2500).OrderBy(p => p.Id);
        }

        [Route("")]
        // GET: api/Products
        public IEnumerable<Product> Get()
        {
            return Products;
        }

        [Route("{id:int}")]
        // GET: api/Products/5
        public Product Get(int id)
        {
            return Products.FirstOrDefault(p => p.Id == id);
        }

        // POST: api/Products
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Products/5
        public void Put(int id, [FromBody]string value)
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
