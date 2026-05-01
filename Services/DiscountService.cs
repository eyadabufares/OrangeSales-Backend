using Microsoft.Data.SqlClient;
using Orange.Training.SecondTask.Models;

namespace Orange.Training.SecondTask.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly SqlConnection _connection;

        public DiscountService(SqlConnection connection)
        {
            _connection = connection;
        }

        public OrderResponse CalculateDiscount(DiscountRequest request)
        {
            decimal subtotal = 0;
            decimal totalTax = 0;

            _connection.Open();
            try
            {
                foreach (var item in request.Items)
                {
                    decimal itemTotal = item.Price * item.Qty;
                    subtotal += itemTotal;

                    string taxQuery = "SELECT TaxRate FROM Categories WHERE Name = @CategoryName";
                    using (SqlCommand cmd = new SqlCommand(taxQuery, _connection))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", item.Category);
                        var rate = cmd.ExecuteScalar();
                        decimal taxRate = (rate != null) ? Convert.ToDecimal(rate) : 0.16m; 
                        totalTax += itemTotal * taxRate;
                    }
                }

                decimal couponDiscount = 0;
                if (!string.IsNullOrEmpty(request.CouponCode))
                {
                    string couponQuery = "SELECT DiscountValue FROM Coupons WHERE Code = @Code AND IsActive = 1";
                    using (SqlCommand cmd = new SqlCommand(couponQuery, _connection))
                    {
                        cmd.Parameters.AddWithValue("@Code", request.CouponCode);
                        var disc = cmd.ExecuteScalar();
                        couponDiscount = (disc != null) ? Convert.ToDecimal(disc) : 0;
                    }
                }

                decimal discountAmount = couponDiscount;
                if (request.UserType == "VIP") discountAmount += subtotal * 0.10m;
                if (subtotal > 1000) discountAmount += subtotal * 0.05m;

                decimal finalTotal = (subtotal + totalTax) - discountAmount;

                return new OrderResponse
                {
                    OrderId = 777,
                    Subtotal = subtotal,
                    Tax = totalTax,
                    Total = finalTotal
                };
            }
            finally
            {
                _connection.Close(); 
            }
        }
    }
}