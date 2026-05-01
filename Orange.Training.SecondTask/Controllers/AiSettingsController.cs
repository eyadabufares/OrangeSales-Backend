using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Orange.Training.SecondTask.Models;

namespace Orange.Training.SecondTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiSettingsController : ControllerBase
    {
        private readonly SqlConnection _connection;

        public AiSettingsController(SqlConnection connection)
        {
            _connection = connection;
        }

        [HttpGet]
        public IActionResult GetCurrentPrompt()
        {
            string currentPrompt = "";
            _connection.Open();
            string query = "SELECT TOP 1 PromptTemplate FROM AiSettings ORDER BY Id DESC";
            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                var result = cmd.ExecuteScalar();
                currentPrompt = result != null ? result.ToString() : "No prompt found.";
            }
            _connection.Close();
            return Ok(new { prompt = currentPrompt });
        }

        [HttpPost("update")]
        public IActionResult UpdatePrompt([FromBody] AiSettingUpdate request)
        {
            if (string.IsNullOrEmpty(request.NewPrompt))
                return BadRequest("Prompt cannot be empty");

            _connection.Open();
            string query = "INSERT INTO AiSettings (PromptTemplate, LastUpdated) VALUES (@NewPrompt, GETDATE())";
            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@NewPrompt", request.NewPrompt);
                cmd.ExecuteNonQuery();
            }
            _connection.Close();

            return Ok(new { message = "AI Settings updated successfully!" });
        }
    }
}