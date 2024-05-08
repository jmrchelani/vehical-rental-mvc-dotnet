using System.ComponentModel.DataAnnotations;

namespace VIPRentals.Models
{
    public class Vehicle
    {
        // Vehicle Model, Make, Year, and Per-day-rent price, image

        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Vehicle Model")]
        public string Model { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Vehicle Make")]
        public string Make { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Vehicle Year")]
        public string Year { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Per-hour-rent Price")]
        public decimal Price { get; set; }

        [DataType(DataType.ImageUrl)]
        [Display(Name = "Vehicle Image")]
        public string? Image { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Vehicle Owner")]
        public string? Owner { get; set; }

    }
}
