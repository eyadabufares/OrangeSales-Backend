using Microsoft.Data.SqlClient;
using Orange.Training.SecondTask.Services;

namespace Orange.Training.SecondTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. تعريف الـ Connection String
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                      ?? builder.Configuration.GetSection("ConnectionStrings:DefaultConnection").Value;

            // 2. إضافة الـ Services
            builder.Services.AddScoped(sp => new SqlConnection(connectionString));
            builder.Services.AddScoped<IDiscountService, DiscountService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddHttpClient<IAiService, AiService>();
            builder.Services.AddScoped<IAiService, AiService>();

            // --- [إضافة كود الـ CORS هون] ---
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy => policy.AllowAnyOrigin() // بسمح لأي موقع (زي localhost:3000) يطلب بيانات
                                    .AllowAnyMethod() // بسمح بكل أنواع الطلبات (GET, POST, etc.)
                                    .AllowAnyHeader()); // بسمح بكل أنواع الهيدرز
            });
            // ------------------------------

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 3. إعدادات الـ Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // --- [تفعيل الـ CORS هون - لازم يكون قبل الـ Authorization] ---
            app.UseCors("AllowReactApp");
            // ----------------------------------------------------------

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}