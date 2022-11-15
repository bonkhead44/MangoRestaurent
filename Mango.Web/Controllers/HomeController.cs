using Mango.Web.Models;
using Mango.Web.Services;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Mango.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _iCartService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService iCartService)
        {
            _logger = logger;
            _productService = productService;
            _iCartService = iCartService;
        }

        public async Task<IActionResult> Index()
        {
            List<ProductDto> list = new();
            var response = await _productService.GetAllProductsAsync<ResponseDto>("");
            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }
            return View(list);
        }

        [Authorize]
        public async Task<IActionResult> Details(int productId)
        {
            var userId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
            ProductDto model = new();
            var response = await _productService.GetProductByIdAsync<ResponseDto>(productId, "");
            if (response != null && response.IsSuccess)
            {
                model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            }
            return View(model);
        }

        [HttpPost]
        [ActionName("Details")]
        [Authorize]
        public async Task<IActionResult> DetailsPost(ProductDto productDto)
        {

            CartDto cartDto = new();

            CartHeaderDto cartHeaderDto = new CartHeaderDto();
            cartHeaderDto.UserId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
            cartDto.CartHeader = cartHeaderDto;

            CartDetailsDto cartDetailsDto = new();
            cartDetailsDto.ProductId = productDto.ProductId;
            cartDetailsDto.Count = productDto.Count;
            //cartDetailsDto.CartHeader = cartHeaderDto;


            var response = await _productService.GetProductByIdAsync<ResponseDto>(productDto.ProductId, "");
            if (response != null && response.IsSuccess)
            {
                cartDetailsDto.Product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            }

            List<CartDetailsDto> list = new();
            list.Add(cartDetailsDto);

            cartDto.CartDetails = list;

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            //if (ModelState.IsValid)
            //{
            var addToCartResponse = await _iCartService.AddToCartAsync<ResponseDto>(cartDto, accessToken);
            //}



            if (addToCartResponse != null && addToCartResponse.IsSuccess)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(productDto);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Login()
        {
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Logout()
        {
            //SignOut("Cookies", "oidc");
            //return RedirectToAction(nameof(Index));
            await HttpContext.SignOutAsync();
            SignOut("Cookies", "oidc");
            return RedirectToAction("Index", "Home");
        }
    }
}