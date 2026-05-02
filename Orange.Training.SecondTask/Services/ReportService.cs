using Npgsql;
using Orange.Training.SecondTask.Models;
using System.Data;

namespace Orange.Training.SecondTask.Services
{
    public class ReportService : IReportService
    {
        private readonly NpgsqlConnection _connection;

        public ReportService(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public SalesReportResponse GetSalesSummary(int orderId)
        {
            var items = new List<ItemReportDto>();

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                string query = @"SELECT p.name, p.price, oi.quantity 
                                 FROM orderitems oi 
                                 JOIN products p ON oi.productid = p.id 
                                 WHERE oi.orderid = @OrderId";

                using (var cmd = new NpgsqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new ItemReportDto
                            {
                                Product = reader["name"].ToString(),
                                Price = Convert.ToDecimal(reader["price"]),
                                Qty = Convert.ToInt32(reader["quantity"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Error in ReportService: {ex.Message}");
                throw; 
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
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