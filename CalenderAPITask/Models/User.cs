using System.ComponentModel.DataAnnotations;

namespace CalenderAPITask.Models
{
    public class User
    {
        [Required]
        [EmailAddress]
        public string Gmail { get; set; }
        [Required]
        public string SaltedPassword { get; set; } 
        public string? Salt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
