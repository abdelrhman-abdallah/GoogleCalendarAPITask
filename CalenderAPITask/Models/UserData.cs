using System.ComponentModel.DataAnnotations;

namespace CalenderAPITask.Models
{
    public class UserData
    {
        [Required]
        [EmailAddress]
        public string Gmail { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
