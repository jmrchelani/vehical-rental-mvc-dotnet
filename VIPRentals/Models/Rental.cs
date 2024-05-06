using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VIPRentals.Models
{
    public class Rental
    {
        // Rental Model, Vehicle, Start Date, End Date, and Total Price
        [Key]
        public int Id { get; set; }

        [Required]
        public string Vehicle { get; set; }

        [Required]
        public string User { get; set; }

        [Required]
        public string StartDate { get; set; }

        [Required]
        public string EndDate { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }
    }
}
