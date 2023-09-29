using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class VillaNumberDto
    {
        [Required]
        public int Number { get; set; }
        public string SpecialDetails { get; set; }

        [Required]
        public int VillaId { get; set; }

        public VillaDto Villa { get; set; }
    }
}
