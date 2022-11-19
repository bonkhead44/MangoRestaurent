namespace Mango.Web.Services.IServices
{
    public interface ICouponService
    {
        Task<T> GetCoupon<T>(string code, string? token = null);
    }
}
