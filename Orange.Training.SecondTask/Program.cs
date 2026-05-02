using Npgsql;
using Orange.Training.SecondTask.Services;

namespace Orange.Training.SecondTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                                   ?? builder.Configuration.GetConnectionString("DefaultConnection");

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddScoped(sp => new NpgsqlConnection(connectionString));
            builder.Services.AddScoped<IDiscountService, DiscountService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddHttpClient<IAiService, AiService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVercelAndOthers",
                    policy => policy.SetIsOriginAllowed(origin => true)
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials());
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowVercelAndOthers");

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}