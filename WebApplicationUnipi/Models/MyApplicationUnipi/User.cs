using System.ComponentModel.DataAnnotations;

namespace WebApplicationUnipi.Models.MyApplicationUnipi
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Enter your email")]
        [Display(Name = "Email Address")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Enter your password")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}