using Orange.Training.SecondTask.Models;

namespace Orange.Training.SecondTask.Services
{
    public interface IReportService
    {
        SalesReportResponse GetSalesSummary(int orderId);
    }
}