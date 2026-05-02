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
                    Console.WriteLine(ex.Message);
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
                    Console.WriteLine(ex.Message);
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

        public async Task<UpdateProfileResponse> UpdateUserProfile(int userId, UpdateProfileRequest request)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string checkQuery = "SELECT passwordhash FROM \"users\" WHERE id = @Id";
                string storedHash = "";
                using (var checkCmd = new NpgsqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Id", userId);
                    var result = await checkCmd.ExecuteScalarAsync();
                    if (result == null) return new UpdateProfileResponse { Success = false, Message = "User not found" };
                    storedHash = result.ToString();
                }

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, storedHash))
                {
                    return new UpdateProfileResponse { Success = false, Message = "Current password is incorrect" };
                }

                string passwordUpdate = "";
                if (!string.IsNullOrEmpty(request.NewPassword))
                {
                    passwordUpdate = ", passwordhash = @NewPasswordHash";
                }

                string updateQuery = $@"UPDATE ""users"" 
                                       SET fullname = @FullName, 
                                           email = @Email, 
                                           profileimageurl = @Img 
                                           {passwordUpdate} 
                                       WHERE id = @Id";

                using (var cmd = new NpgsqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", request.FullName);
                    cmd.Parameters.AddWithValue("@Email", request.Email.Trim().ToLower());
                    cmd.Parameters.AddWithValue("@Img", (object)request.ProfileImageUrl ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", userId);

                    if (!string.IsNullOrEmpty(request.NewPassword))
                        cmd.Parameters.AddWithValue("@NewPasswordHash", BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

                    int rows = await cmd.ExecuteNonQueryAsync();

                    if (rows > 0)
                    {
                        return new UpdateProfileResponse
                        {
                            Success = true,
                            Message = "Profile updated successfully",
                            UpdatedFullName = request.FullName,
                            UpdatedEmail = request.Email,
                            UpdatedProfileImageUrl = request.ProfileImageUrl
                        };
                    }
                }
            }
            return new UpdateProfileResponse { Success = false, Message = "An error occurred during update" };
        }
    }
}