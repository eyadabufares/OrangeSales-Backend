using Orange.Training.SecondTask.Models;

namespace Orange.Training.SecondTask.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> PlaceOrder(OrderRequest request);

        List<OrderHistoryResponse> GetOrdersByUserId(int userId);

        bool DeleteOrder(int orderId);
    }
}