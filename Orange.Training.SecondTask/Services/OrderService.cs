using Microsoft.Data.SqlClient;
using Orange.Training.SecondTask.Models;
using System.Data;

namespace Orange.Training.SecondTask.Services
{
    public class OrderService : IOrderService
    {
        private readonly SqlConnection _connection;
        private readonly IAiService _aiService;

        public OrderService(SqlConnection connection, IAiService aiService)
        {
            _connection = connection;
            _aiService = aiService;
        }

        public async Task<OrderResponse> PlaceOrder(OrderRequest request)
        {
            decimal subtotal = 0;
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            SqlTransaction transaction = _connection.BeginTransaction();

            try
            {
                foreach (var item in request.Items)
                {
                    string priceQuery = "SELECT Price FROM Products WHERE Id = @ProductId";
                    using (SqlCommand cmd = new SqlCommand(priceQuery, _connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            subtotal += Convert.ToDecimal(result) * item.Qty;
                        }
                    }
                }

                decimal tax = subtotal * 0.16m;
                decimal total = subtotal + tax;

                string insertOrderQuery = @"INSERT INTO Orders (UserId, Total, Subtotal, Tax, OrderDate, AiStatus) 
                                           VALUES (@UserId, @Total, @Subtotal, @Tax, GETDATE(), 'Pending');
                                           SELECT SCOPE_IDENTITY();";

                int newOrderId;
                using (SqlCommand cmd = new SqlCommand(insertOrderQuery, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@UserId", request.UserId); 
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@Tax", tax);
                    newOrderId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (var item in request.Items)
                {
                    string insertItemsQuery = "INSERT INTO OrderItems (OrderId, ProductId, Quantity) VALUES (@OrderId, @ProductId, @Qty)";
                    using (SqlCommand cmd = new SqlCommand(insertItemsQuery, _connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", newOrderId);
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@Qty", item.Qty);
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                _connection.Close();

                string realDescription = $"Order ID {newOrderId} with total {total} JOD.";
                var aiResult = await _aiService.ValidateOrderAsync(newOrderId, total, realDescription);
                UpdateOrderAiStatus(newOrderId, aiResult.Status, aiResult.Reason);

                return new OrderResponse
                {
                    OrderId = newOrderId,
                    Subtotal = subtotal,
                    Tax = tax,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                if (_connection.State == ConnectionState.Open && transaction != null)
                    transaction.Rollback();
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
            }
        }

        private void UpdateOrderAiStatus(int orderId, string status, string reason)
        {
            if (_connection.State != ConnectionState.Open) _connection.Open();

            string updateQuery = "UPDATE Orders SET AiStatus = @status, AiReason = @reason WHERE Id = @id";
            using (SqlCommand cmd = new SqlCommand(updateQuery, _connection))
            {
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@reason", (object)reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", orderId);
                cmd.ExecuteNonQuery();
            }
            _connection.Close();
        }

        public List<OrderHistoryResponse> GetOrdersByUserId(int userId)
        {
            var orders = new List<OrderHistoryResponse>();
            string query = "SELECT Id, Subtotal, Tax, Total, AiStatus, AiReason FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC";

            if (_connection.State != ConnectionState.Open) _connection.Open();
            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new OrderHistoryResponse
                        {
                            OrderId = reader.GetInt32(0),
                            Subtotal = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                            Tax = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                            Total = reader.GetDecimal(3),
                            AiStatus = reader.IsDBNull(4) ? "Pending" : reader.GetString(4),
                            AiReason = reader.IsDBNull(5) ? "" : reader.GetString(5)
                        });
                    }
                }
            }
            _connection.Close();
            return orders;
        }

        public bool DeleteOrder(int orderId)
        {
            if (_connection.State != ConnectionState.Open) _connection.Open();
            SqlTransaction transaction = _connection.BeginTransaction();

            try
            {
                string deleteLogsQuery = "DELETE FROM OrderAiLogs WHERE OrderId = @OrderId";
                using (SqlCommand cmd = new SqlCommand(deleteLogsQuery, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.ExecuteNonQuery();
                }

                string deleteItemsQuery = "DELETE FROM OrderItems WHERE OrderId = @OrderId";
                using (SqlCommand cmd = new SqlCommand(deleteItemsQuery, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.ExecuteNonQuery();
                }

                string deleteOrderQuery = "DELETE FROM Orders WHERE Id = @OrderId";
                using (SqlCommand cmd = new SqlCommand(deleteOrderQuery, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    transaction.Commit();
                    return rowsAffected > 0;
                }
            }
            catch (Exception)
            {
                if (transaction != null) transaction.Rollback();
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}