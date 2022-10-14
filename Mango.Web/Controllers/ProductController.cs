using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _iProductService;
        public ProductController(IProductService iProductService)
        {
            _iProductService = iProductService;
        }
        public async Task<IActionResult> ProductIndex()
        {
            var response = await _iProductService.GetAllProductsAsync<ResponseDto>();
            List<ProductDto> productList = new();
            if (response.Result != null && response.IsSuccess)
            {
                productList = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }
            return View(productList);
        }

        public async Task<IActionResult> ProductCreate()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductCreate(ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                var response = await _iProductService.CreateProductAsync<ResponseDto>(productDto);
                List<ProductDto> productList = new();
                if (response.Result != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(ProductIndex));
                }
            }
            return View(productDto);
        }

        public async Task<IActionResult> ProductDelete(int productId)
        {
            var response = await _iProductService.GetProductByIdAsync<ResponseDto>(productId);
            if (response.Result != null && response.IsSuccess)
            {
                ProductDto product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                return View(product);
            }
            return NotFound();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductDelete(ProductDto productDto)
        {
            //if (ModelState.IsValid)
            //{
            var response = await _iProductService.DeleteProductAsync<ResponseDto>(productDto.ProductId);
            if (response.IsSuccess)
            {
                return RedirectToAction(nameof(ProductIndex));
            }
            //}
            return View(productDto);
        }

        public async Task<IActionResult> ProductEdit(int productId)
        {
            var response = await _iProductService.GetProductByIdAsync<ResponseDto>(productId);
            if (response.Result != null && response.IsSuccess)
            {
                ProductDto product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                return View(product);
            }
            return NotFound();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductEdit(ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                var response = await _iProductService.UpdateProductAsync<ResponseDto>(productDto);
                List<ProductDto> productList = new();
                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(ProductIndex));
                }
            }
            return View(productDto);
        }
    }
}
