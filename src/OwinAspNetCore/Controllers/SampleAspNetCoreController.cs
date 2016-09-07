using Microsoft.AspNetCore.Mvc;

namespace OwinAspNetCore.Controllers
{
    [Route("api/[controller]")]
    public class SampleAspNetCoreController : Controller
    {
        private ISomeDependency _someDependency;

        public SampleAspNetCoreController(ISomeDependency someDependency)
        {
            _someDependency = someDependency;
        }

        [HttpGet("GetValues")]
        public int[] GetValues()
        {
            return new[] { 1, 2, 3 };
        }
    }
}
