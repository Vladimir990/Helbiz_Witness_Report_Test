using Helbiz_Witness_Reports_Test.Models;
using Helbiz_Witness_Reports_Test.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Helbiz_Witness_Reports_Test.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : Controller
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [Route("report/{title}")]
        [HttpGet]
        public IActionResult GetReportsByTitle(string title, string phoneNumber)
        {
            var report = _reportsService.GetReportsByTitleAsync(title, phoneNumber);

            if (!report.Result) return NotFound("The record with the specified title was not found or the phone number is invalid");

            return Ok("Record saved successfully");
        }

    }
}
