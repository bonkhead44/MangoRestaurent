using Mango.Services.CouponAPI.Models.Dtos;
using Mango.Services.CouponAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/coupon")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        protected ResponseDto _response;
        private readonly ICouponRepository _iCouponRepository;
        public CouponController(ICouponRepository iCouponRepository)
        {
            _iCouponRepository = iCouponRepository;
            _response = new ResponseDto();
        }

        [HttpGet("{code}")]
        public async Task<object> GetDiscountForCode(string code)
        {
            try
            {
                var coupon = await _iCouponRepository.GetCouponByCode(code);
                _response.Result = coupon;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
    }
}
