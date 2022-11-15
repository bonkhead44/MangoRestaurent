using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _iCartService;
        public CartController(ICartService iCartService)
        {
            _iCartService = iCartService;
        }
        public IActionResult CartIndex()
        {
            return View();
        }
    }
}
