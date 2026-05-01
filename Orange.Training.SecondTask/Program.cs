using Npgsql;
using Orange.Training.SecondTask.Services;

namespace Orange.Training.SecondTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                      ?? builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;

            builder.Services.AddScoped(sp => new NpgsqlConnection(connectionString));
            builder.Services.AddScoped<IDiscountService, DiscountService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddHttpClient<IAiService, AiService>();
            builder.Services.AddScoped<IAiService, AiService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader());
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowReactApp");
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}