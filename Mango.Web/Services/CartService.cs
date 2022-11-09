using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class CartService : BaseService, ICartService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CartService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public Task<T> AddToCartAsync<T>(CartDto cartDto, string? token = null)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetCartByUserIdAsnyc<T>(string userId, string? token = null)
        {
            throw new NotImplementedException();
        }

        public Task<T> RemoveFromCartAsync<T>(int cartId, string? token = null)
        {
            throw new NotImplementedException();
        }

        public Task<T> UpdateCartAsync<T>(CartDto cartDto, string? token = null)
        {
            throw new NotImplementedException();
        }
    }
}
