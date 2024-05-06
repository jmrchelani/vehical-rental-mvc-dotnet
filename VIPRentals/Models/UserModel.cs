using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VIPRentals.Models
{
    public class UserModel : IdentityUser 
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string FullName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "City")]
        public string City { get; set; }
    }
}
