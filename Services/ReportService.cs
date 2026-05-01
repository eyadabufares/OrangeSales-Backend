using Microsoft.Data.SqlClient;
using Orange.Training.SecondTask.Models;

namespace Orange.Training.SecondTask.Services
{
    public class ReportService : IReportService
    {
        private readonly SqlConnection _connection;

        public ReportService(SqlConnection connection)
        {
            _connection = connection;
        }

        public SalesReportResponse GetSalesSummary(int orderId)
        {
            var items = new List<ItemReportDto>();

            _connection.Open();
            try
            {
                string query = @"SELECT p.Name, p.Price, oi.Quantity 
                                 FROM OrderItems oi 
                                 JOIN Products p ON oi.ProductId = p.Id 
                                 WHERE oi.OrderId = @OrderId";

                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new ItemReportDto
                            {
                                Product = reader["Name"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Qty = Convert.ToInt32(reader["Quantity"])
                            });
                        }
                    }
                }
            }
            finally
            {
                _connection.Close();
            }

            return new SalesReportResponse
            {
                Orders = new List<OrderReportDto>
                {
                    new OrderReportDto
                    {
                        Id = orderId,
                        Items = items
                    }
                }
            };
        }
    }
}