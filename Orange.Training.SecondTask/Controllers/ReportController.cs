using Microsoft.AspNetCore.Mvc;
using Orange.Training.SecondTask.Models;
using Orange.Training.SecondTask.Services;

namespace Orange.Training.SecondTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("sales-summary/{orderId}")]
        public IActionResult GetSalesSummary(int orderId)
        {
            try
            {
                var response = _reportService.GetSalesSummary(orderId);

                if (response == null || response.Orders == null)
                {
                    return NotFound(new { message = "Report data not found for this order." });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReportController: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error while generating report." });
            }
        }
    }
}