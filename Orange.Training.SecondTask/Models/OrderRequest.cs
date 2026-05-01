namespace Orange.Training.SecondTask.Models
{
    public class OrderRequest
    {
        public int UserId { get; set; } 
        public List<OrderItemRequest> Items { get; set; }
    }

    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Qty { get; set; }
    }
}