using Microsoft.AspNetCore.Mvc;

namespace OwinAspNetCore
{
    [Route("api/[controller]")]
    public class SampleAspNetCoreController : Controller
    {
        public SampleAspNetCoreController(ISomeDependency someDependency)
        {

        }

        [HttpGet("GetValues")]
        public int[] GetValues()
        {
            return new[] { 1, 2, 3 };
        }
    }
}
