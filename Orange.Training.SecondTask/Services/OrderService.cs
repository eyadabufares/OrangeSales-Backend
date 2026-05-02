using Npgsql;
using Orange.Training.SecondTask.Models;
using System.Data;

namespace Orange.Training.SecondTask.Services
{
    public class OrderService : IOrderService
    {
        private readonly NpgsqlConnection _connection;
        private readonly IAiService _aiService;

        public OrderService(NpgsqlConnection connection, IAiService aiService)
        {
            _connection = connection;
            _aiService = aiService;
        }

        public async Task<OrderResponse> PlaceOrder(OrderRequest request)
        {
            decimal subtotal = 0;
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();
            using var transaction = await _connection.BeginTransactionAsync();

            try
            {
                foreach (var item in request.Items)
                {
                    string priceQuery = "SELECT \"Price\" FROM \"Products\" WHERE \"Id\" = @ProductId";
                    using (var cmd = new NpgsqlCommand(priceQuery, _connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            subtotal += Convert.ToDecimal(result) * item.Qty;
                        }
                    }
                }

                decimal tax = subtotal * 0.16m;
                decimal total = subtotal + tax;

                string insertOrderQuery = @"INSERT INTO ""Orders"" (""UserId"", ""Total"", ""Subtotal"", ""Tax"", ""OrderDate"", ""AiStatus"") 
                                           VALUES (@UserId, @Total, @Subtotal, @Tax, NOW(), 'Pending') 
                                           RETURNING ""Id"";";

                int newOrderId;
                using (var cmd = new NpgsqlCommand(insertOrderQuery, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@UserId", request.UserId);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmd.Parameters.AddWithValue("@Tax", tax);
                    newOrderId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                foreach (var item in request.Items)
                {
                    string insertItemsQuery = "INSERT INTO \"OrderItems\" (\"OrderId\", \"ProductId\", \"Quantity\") VALUES (@OrderId, @ProductId, @Qty)";
                    using (var cmd = new NpgsqlCommand(insertItemsQuery, _connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", newOrderId);
                        cmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                        cmd.Parameters.AddWithValue("@Qty", item.Qty);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();

                string realDescription = $"Order ID {newOrderId} with total {total} JOD.";
                var aiResult = await _aiService.ValidateOrderAsync(newOrderId, total, realDescription);
                await UpdateOrderAiStatus(newOrderId, aiResult.Status, aiResult.Reason);

                return new OrderResponse
                {
                    OrderId = newOrderId,
                    Subtotal = subtotal,
                    Tax = tax,
                    Total = total
                };
            }
            catch (Exception)
            {
                if (transaction != null) await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task UpdateOrderAiStatus(int orderId, string status, string reason)
        {
            if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();

            string updateQuery = "UPDATE \"Orders\" SET \"AiStatus\" = @status, \"AiReason\" = @reason WHERE \"Id\" = @id";
            using (var cmd = new NpgsqlCommand(updateQuery, _connection))
            {
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@reason", (object)reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", orderId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public List<OrderHistoryResponse> GetOrdersByUserId(int userId)
        {
            var orders = new List<OrderHistoryResponse>();
            string query = "SELECT \"Id\", \"Subtotal\", \"Tax\", \"Total\", \"AiStatus\", \"AiReason\" FROM \"Orders\" WHERE \"UserId\" = @UserId ORDER BY \"OrderDate\" DESC";

            if (_connection.State != ConnectionState.Open) _connection.Open();
            using (var cmd = new NpgsqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using (var reader = cmd.ExecuteReader())
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
            return orders;
        }

        public bool DeleteOrder(int orderId)
        {
            if (_connection.State != ConnectionState.Open) _connection.Open();
            using var transaction = _connection.BeginTransaction();

            try
            {
                string deleteLogsQuery = "DELETE FROM \"OrderAiLogs\" WHERE \"OrderId\" = @OrderId";
                using (var cmd = new NpgsqlCommand(deleteLogsQuery, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.ExecuteNonQuery();
                }

                string deleteItemsQuery = "DELETE FROM \"OrderItems\" WHERE \"OrderId\" = @OrderId";
                using (var cmd = new NpgsqlCommand(deleteItemsQuery, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.ExecuteNonQuery();
                }

                string deleteOrderQuery = "DELETE FROM \"Orders\" WHERE \"Id\" = @OrderId";
                using (var cmd = new NpgsqlCommand(deleteOrderQuery, _connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    transaction.Commit();
                    return rowsAffected > 0;
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}