using Microsoft.Data.SqlClient;
using Orange.Training.SecondTask.Models;
using BCrypt.Net;

namespace Orange.Training.SecondTask.Services
{
    public class AuthService : IAuthService
    {
        private readonly SqlConnection _connection;

        public AuthService(SqlConnection connection)
        {
            _connection = connection;
        }

        public bool Register(RegisterRequest request)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            _connection.Open();
            try
            {
                string query = "INSERT INTO Users (FullName, Email, PasswordHash) VALUES (@FullName, @Email, @PasswordHash)";
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@FullName", request.FullName);
                    cmd.Parameters.AddWithValue("@Email", request.Email.Trim());
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during register: " + ex.Message);
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }

        public bool Login(LoginRequest request)
        {
            User user = null;

            _connection.Open();
            try
            {
                string query = "SELECT Email, PasswordHash FROM Users WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Email", request.Email.Trim());
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Email = reader["Email"].ToString().Trim(),
                                PasswordHash = reader["PasswordHash"].ToString().Trim()
                            };
                        }
                    }
                }

                if (user != null)
                {
                    Console.WriteLine($"Found User: {user.Email}");
                    Console.WriteLine($"Hash Length: {user.PasswordHash.Length}");

                    return BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                }

                return false;
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}