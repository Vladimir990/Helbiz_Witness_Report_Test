using System.ComponentModel.DataAnnotations;

namespace Helbiz_Witness_Reports_Test.Models
{
    public class Report
    {
        [Required]
        public string? ReportContent { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }

        public string? Country { get; set; }
    }
}
