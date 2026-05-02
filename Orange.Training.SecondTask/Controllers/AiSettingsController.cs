using Microsoft.AspNetCore.Mvc;
using Npgsql; 
using Orange.Training.SecondTask.Models;

namespace Orange.Training.SecondTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiSettingsController : ControllerBase
    {
        private readonly NpgsqlConnection _connection; 

        public AiSettingsController(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        [HttpGet]
        public IActionResult GetCurrentPrompt()
        {
            string currentPrompt = "";
            if (_connection.State != System.Data.ConnectionState.Open) _connection.Open();

            string query = "SELECT \"PromptTemplate\" FROM \"AiSettings\" ORDER BY \"Id\" DESC LIMIT 1";

            using (var cmd = new NpgsqlCommand(query, _connection))
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

            if (_connection.State != System.Data.ConnectionState.Open) _connection.Open();

            string query = "INSERT INTO \"AiSettings\" (\"PromptTemplate\", \"LastUpdated\") VALUES (@NewPrompt, NOW())";

            using (var cmd = new NpgsqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@NewPrompt", request.NewPrompt);
                cmd.ExecuteNonQuery();
            }
            _connection.Close();

            return Ok(new { message = "AI Settings updated successfully!" });
        }
    }
}