﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Villa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public string Details { get; set; }
        public double Rate { get; set; }
        public int SqFt { get; set; }
        public int Occupancy { get; set; }
        public string Amenity { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual List<VillaNumber> VillaNumbers { get; set; }
    }
}