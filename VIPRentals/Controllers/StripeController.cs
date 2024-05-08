using Microsoft.AspNetCore.Mvc;

namespace VIPRentals.Controllers
{

    [Route("api/stripe")]
    public class StripeController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            Console.WriteLine(json);

            return Ok();
        }

   
    }
}
