using Microsoft.AspNetCore.Mvc;
using Orange.Training.SecondTask.Models;
using Orange.Training.SecondTask.Services;

namespace Orange.Training.SecondTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpPost("apply-discounts")]
        public OrderResponse ApplyDiscounts([FromBody] DiscountRequest request)
        {
            var result = _discountService.CalculateDiscount(request);

            return result;
        }
    }
}