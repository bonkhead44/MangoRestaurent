using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class CouponService : BaseService, ICouponService
    {

        private readonly IHttpClientFactory _httpClientFactory;
        public CouponService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<T> GetCoupon<T>(string code, string? token = null)
        {
            return await this.SendAsync<T>(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.CouponAPIBase + "/api/coupon/" + code,
                AccessToken = token
            });
        }
    }
}
