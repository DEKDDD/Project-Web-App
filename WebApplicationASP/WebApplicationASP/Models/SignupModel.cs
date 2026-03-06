using System.ComponentModel.DataAnnotations;

namespace WebApplicationASP.Models
{
    public class Signup
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}