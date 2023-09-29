using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Dtos;

namespace Web.Models.ViewModels
{
    public class VillaNumberCreateVM
    {
        public VillaNumberCreateDto VillaNumber { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> VillaList { get; set; }

        public VillaNumberCreateVM()
        {
            VillaNumber = new VillaNumberCreateDto();
        }
    }
}
