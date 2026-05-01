using Microsoft.AspNetCore.Mvc;
using Orange.Training.SecondTask.Models;
using Orange.Training.SecondTask.Services;

namespace Orange.Training.SecondTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequest request)
        {
            var response = await _orderService.PlaceOrder(request);
            return Ok(response);
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetUserOrders(int userId)
        {
            var orders = _orderService.GetOrdersByUserId(userId);
            return Ok(orders);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var result = _orderService.DeleteOrder(id);
            if (result)
            {
                return Ok(new { message = "Order deleted successfully" });
            }
            return NotFound(new { message = "Order not found" });
        }
    }
}