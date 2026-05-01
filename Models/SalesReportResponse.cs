namespace Orange.Training.SecondTask.Models
{
    public class SalesReportResponse
    {
        public List<OrderReportDto> Orders { get; set; }
    }

    public class OrderReportDto
    {
        public int Id { get; set; }
        public List<ItemReportDto> Items { get; set; }
    }

    public class ItemReportDto
    {
        public string Product { get; set; } 
        public decimal Price { get; set; }
        public int Qty { get; set; }
    }
}