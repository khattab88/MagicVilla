using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Models;
using Newtonsoft.Json;
using Web.Services.Interfaces;
using Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Reflection.Metadata;
using Utilities;

namespace Web.Controllers
{
    public class VillaNumbersController : Controller
    {
        private readonly IVillaNumberService _villaNumberSvc;
        private readonly IVillaService _villaSvc;
        private readonly IMapper _mapper;

        public VillaNumbersController(
            IVillaNumberService villaNumberSvc,
            IVillaService villaSvc,
            IMapper mapper
            )
        {
            _villaNumberSvc = villaNumberSvc;
            _villaSvc = villaSvc;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            List<VillaNumberDto> villaNumbers = new();

            string token = HttpContext.Session.GetString(Constants.AccessToken);
            var response = await _villaNumberSvc.GetAllAsync<ApiResponse>();

            if (response != null && response.IsSuccess)
            {
                villaNumbers = JsonConvert.DeserializeObject<List<VillaNumberDto>>(Convert.ToString(response.Result));
            }

            return View(villaNumbers);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            VillaNumberCreateVM vm = new();

            string token = HttpContext.Session.GetString(Constants.AccessToken);
            var response = await _villaSvc.GetAllAsync<ApiResponse>();

            if (response != null && response.IsSuccess)
            {
                var villas = JsonConvert.DeserializeObject<List<VillaDto>>(Convert.ToString(response.Result));
                vm.VillaList = villas.Select(v => new SelectListItem { Text = v.Name, Value = v.Id.ToString() });
            }

            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VillaNumberCreateVM createVm)
        {
            if (ModelState.IsValid)
            {
                string token = HttpContext.Session.GetString(Constants.AccessToken);
                var response = await _villaNumberSvc.CreateAsync<ApiResponse>(createVm.VillaNumber);

                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
                //else
                //{
                //    if (response.ErrorMessages.Count > 0)
                //    {
                //        ModelState.AddModelError("ErrorMessages", response.ErrorMessages.FirstOrDefault());
                //    }
                //}
            }

            return View(createVm);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int number)
        {
            VillaNumberUpdateVM vm = new();

            string token = HttpContext.Session.GetString(Constants.AccessToken);
            var villasResponse = await _villaSvc.GetAllAsync<ApiResponse>();

            if (villasResponse != null && villasResponse.IsSuccess)
            {
                var villas = JsonConvert.DeserializeObject<List<VillaDto>>(Convert.ToString(villasResponse.Result));
                vm.VillaList = villas.Select(v => new SelectListItem { Text = v.Name, Value = v.Id.ToString() });
            }

            var response = await _villaNumberSvc.GetAsync<ApiResponse>(number);

            if (response != null && response.IsSuccess)
            {
                VillaNumberDto villa = JsonConvert.DeserializeObject<VillaNumberDto>(Convert.ToString(response.Result));
                vm.VillaNumber = _mapper.Map<VillaNumberUpdateDto>(villa);

                return View(vm);
            }

            return NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(VillaNumberUpdateVM updateVm)
        {
            if (ModelState.IsValid)
            {
                string token = HttpContext.Session.GetString(Constants.AccessToken);
                var response = await _villaNumberSvc.UpdateAsync<ApiResponse>(updateVm.VillaNumber);

                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(updateVm);
        }
    }
}
