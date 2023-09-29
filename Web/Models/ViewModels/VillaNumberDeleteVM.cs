using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Dtos;

namespace Web.Models.ViewModels
{
    public class VillaNumberDeleteVM
    {
        public VillaNumberDto VillaNumber { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> VillaList { get; set; }

        public VillaNumberDeleteVM()
        {
            VillaNumber = new VillaNumberDto();
        }
    }
}
