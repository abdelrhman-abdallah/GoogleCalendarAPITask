using System.ComponentModel.DataAnnotations;

namespace CalenderAPITask.DTO
{
    public class GoogleCalendarReqDTO
    {
        [Required]
        public string Summary { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
    }
}
