using System;
public class OrderHistoryResponse
{
    public int OrderId { get; set; }
    public decimal Total { get; set; }
    public decimal Tax { get; set; }     
    public decimal Subtotal { get; set; } 
    public string AiStatus { get; set; }  
    public string AiReason { get; set; }
    public DateTime OrderDate { get; set; }
}