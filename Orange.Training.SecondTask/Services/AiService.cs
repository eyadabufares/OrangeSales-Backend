using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Orange.Training.SecondTask.Models;
using System.Text;

namespace Orange.Training.SecondTask.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly SqlConnection _connection;

        public AiService(HttpClient httpClient, SqlConnection connection)
        {
            _httpClient = httpClient;
            _connection = connection;
        }

        private string GetPromptFromDb()
        {
            string dbPrompt = "";
            try
            {
                if (_connection.State != System.Data.ConnectionState.Open)
                    _connection.Open();

                string query = "SELECT TOP 1 PromptTemplate FROM AiSettings ORDER BY Id DESC";
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    var result = cmd.ExecuteScalar();
                    dbPrompt = result != null ? result.ToString() : "";
                }
            }
            catch (Exception)
            {
                dbPrompt = "Validate this order. Return JSON.";
            }
            finally
            {
                _connection.Close();
            }
            return dbPrompt;
        }

        public async Task<AiValidationResponse> ValidateOrderAsync(int orderId, decimal price, string description)
        {
            string basePrompt = GetPromptFromDb();

            string finalPrompt = $"{basePrompt} | Data to check: OrderId={orderId}, Price={price}, Description='{description}'. " +
                                 "Strict Rule: Your response must be ONLY a valid JSON object.";

            string url = $"https://text.pollinations.ai/{Uri.EscapeDataString(finalPrompt)}";

            AiValidationResponse aiResponse = new AiValidationResponse();
            string responseBody = "";
            int statusCode = 0;

            try
            {
                var response = await _httpClient.GetAsync(url);
                statusCode = (int)response.StatusCode;
                responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    aiResponse = JsonConvert.DeserializeObject<AiValidationResponse>(responseBody)
                                 ?? new AiValidationResponse { Status = "not valid", Reason = "Invalid AI response format" };
                }
                else if (statusCode == 429)
                {
                    aiResponse = new AiValidationResponse { Status = "System Busy", Reason = "AI service capacity reached." };
                }
                else
                {
                    aiResponse = new AiValidationResponse { Status = "Error", Reason = $"AI Error: {statusCode}" };
                }
            }
            catch (Exception ex)
            {
                statusCode = 500;
                responseBody = ex.Message;
                aiResponse = new AiValidationResponse { Status = "Error", Reason = "Connection failed" };
            }

            SaveAiLog(orderId, finalPrompt, responseBody, statusCode);

            return aiResponse;
        }

        private void SaveAiLog(int orderId, string requestBody, string responseJson, int statusCode)
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            string query = "INSERT INTO OrderAiLogs (OrderId, RequestBody, ResponseJson, ResponseCode) VALUES (@OrderId, @Req, @Res, @Code)";
            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.Parameters.AddWithValue("@Req", requestBody);
                cmd.Parameters.AddWithValue("@Res", responseJson);
                cmd.Parameters.AddWithValue("@Code", statusCode);
                cmd.ExecuteNonQuery();
            }
            _connection.Close();
        }
    }
}