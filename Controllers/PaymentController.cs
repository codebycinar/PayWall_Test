using Microsoft.AspNetCore.Mvc;

namespace PayWallDemo.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
