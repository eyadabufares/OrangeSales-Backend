namespace Orange.Training.SecondTask.Models
{
    public class DiscountRequest
    {
        public string UserType { get; set; } 
        public string CouponCode { get; set; } 
        public List<DiscountItemDto> Items { get; set; }
    }

    public class DiscountItemDto
    {
        public string Category { get; set; } 
        public decimal Price { get; set; }
        public int Qty { get; set; }
    }
}