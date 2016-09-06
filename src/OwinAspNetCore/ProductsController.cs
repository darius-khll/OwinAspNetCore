using System.Collections.Generic;
using System.Linq;
using System.Web.OData;

namespace OwinAspNetCore
{
    public class ProductsController : ODataController
    {
        public ProductsController(ISomeDependency someDependency)
        {

        }

        [EnableQuery]
        public IQueryable<Product> Get()
        {
            return new List<Product>
            {
                 new Product { Id = 1, Name = "Test" , Price = 10 }
            }.AsQueryable();
        }
    }
}
