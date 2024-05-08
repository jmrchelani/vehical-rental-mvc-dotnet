using System.ComponentModel.DataAnnotations;

namespace VIPRentals.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Vehicle { get; set; }

        [Required]
        public string Rental { get; set; }

        [Required]
        public string User { get; set; }

        [Required]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required]
        public string Date { get; set; }
    }
}
