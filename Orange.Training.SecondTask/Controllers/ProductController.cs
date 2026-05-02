using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;

namespace Orange.Training.SecondTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly NpgsqlConnection _connection;

        public ProductController(NpgsqlConnection connection)
        {
            _connection = connection;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = new List<object>();
            try
            {
                if (_connection.State != ConnectionState.Open) await _connection.OpenAsync();

                string query = "SELECT id, name, price FROM products ORDER BY name ASC";
                using var cmd = new NpgsqlCommand(query, _connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    products.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Price = reader.GetDecimal(2)
                    });
                }
            }
            finally
            {
                await _connection.CloseAsync();
            }
            return Ok(products);
        }
    }
}