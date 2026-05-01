using Orange.Training.SecondTask.Models;

namespace Orange.Training.SecondTask.Services
{
    public interface IDiscountService
    {
        OrderResponse CalculateDiscount(DiscountRequest request);
    }
}