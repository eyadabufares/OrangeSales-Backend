namespace Orange.Training.SecondTask.Models
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
    }
}