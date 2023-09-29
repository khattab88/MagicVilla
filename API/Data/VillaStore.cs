using Models.Dtos;

namespace API.Data
{
    public static class VillaStore
    {
        public static List<VillaDto> Villas = new List<VillaDto> 
        {
            new VillaDto { Id = 1, Name = "Beach Villa", SqFt = 100, Occupancy = 4 },
            new VillaDto { Id = 2, Name = "Garden Villa", SqFt = 150, Occupancy = 5 },
        };
    }
}
