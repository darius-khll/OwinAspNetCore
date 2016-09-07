using OwinAspNetCore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.OData;

namespace OwinAspNetCore.Controllers
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
