using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DesignerCloset.Models
{
    public class SneakerImage
    {

        [Required]
        public string? CustomerName { get; set; }

        [Required]
        public string? ProductName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total must be greater than 0")]
        public double Total { get; set; }


        [Required]
        [Display(Name = "Sneaker Image")]
        public IFormFile? SneakerPicture { get; set; }


    }


}

