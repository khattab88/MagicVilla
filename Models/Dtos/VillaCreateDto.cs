using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class VillaCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public int Occupancy { get; set; }
        public int SqFt { get; set; }
        public string Details { get; set; }
        [Required]
        public double Rate { get; set; }
        public string Amenity { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; }
        public IFormFile? Image { get; set; }
    }
}
