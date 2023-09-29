using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Dtos;

namespace Web.Models.ViewModels
{
    public class VillaNumberUpdateVM
    {
        public VillaNumberUpdateDto VillaNumber { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> VillaList { get; set; }

        public VillaNumberUpdateVM()
        {
            VillaNumber = new VillaNumberUpdateDto();
        }
    }
}
