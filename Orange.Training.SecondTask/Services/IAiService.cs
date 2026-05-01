using Orange.Training.SecondTask.Models;

namespace Orange.Training.SecondTask.Services
{
    public interface IAiService
    {
        Task<AiValidationResponse> ValidateOrderAsync(int orderId, decimal price, string description);
    }
}