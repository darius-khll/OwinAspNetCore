using Microsoft.AspNetCore.Mvc;

namespace OwinAspNetCore
{
    [Route("api/[controller]")]
    public class SampleAspNetCoreController : Controller
    {
        [HttpGet("GetValues")]
        public int[] GetValues()
        {
            return new[] { 1, 2, 3 };
        }
    }
}
