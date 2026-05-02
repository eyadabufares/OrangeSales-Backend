using Npgsql;
using Orange.Training.SecondTask.Models;
using BCrypt.Net;
using System.Data;
using System.Threading.Tasks;

namespace Orange.Training.SecondTask.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString;

        public AuthService(IConfiguration configuration)
        {
            _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                                ?? configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> Register(RegisterRequest request)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                try
                {
                    string query = "INSERT INTO \"users\" (fullname, email, passwordhash, createdat) VALUES (@FullName, @Email, @PasswordHash, @CreatedAt)";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", request.FullName);
                        cmd.Parameters.AddWithValue("@Email", request.Email.Trim().ToLower());
                        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during register: " + ex.Message);
                    return false;
                }
            }
        }

        public async Task<bool> Login(LoginRequest request)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                try
                {
                    string query = "SELECT email, passwordhash FROM \"users\" WHERE email = @Email";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", request.Email.Trim().ToLower());

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string storedHash = reader["passwordhash"].ToString();
                                return BCrypt.Net.BCrypt.Verify(request.Password, storedHash);
                            }
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during login: " + ex.Message);
                    return false;
                }
            }
        }

        public async Task<User> GetUserByEmail(string email)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT id, email, fullname FROM \"users\" WHERE email = @Email";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email.Trim().ToLower());

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = reader.GetInt32(0),
                                Email = reader.GetString(1),
                                FullName = reader.GetString(2)
                            };
                        }
                    }
                }
                return null;
            }
        }
    }
}