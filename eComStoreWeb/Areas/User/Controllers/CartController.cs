using Microsoft.AspNetCore.Mvc;

namespace eComStore.Web.Areas.User.Controllers
{
    [Area("User")]
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
