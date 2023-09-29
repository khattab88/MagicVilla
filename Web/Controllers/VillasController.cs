using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Dtos;
using Newtonsoft.Json;
using Utilities;
using Web.Services.Interfaces;

namespace Web.Controllers
{
    public class VillasController : Controller
    {
        private readonly IVillaService _villaSvc;
        private readonly IMapper _mapper;

        public VillasController(
            IVillaService villaSvc,
            IMapper mapper
            )
        {
            _villaSvc = villaSvc;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            List<VillaDto> villas = new();

            string token = HttpContext.Session.GetString(Constants.AccessToken);
            var response = await _villaSvc.GetAllAsync<ApiResponse>();

            if (response != null && response.IsSuccess)
            {
                villas = JsonConvert.DeserializeObject<List<VillaDto>>(Convert.ToString(response.Result));
            }

            return View(villas);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VillaCreateDto createDto)
        {
            if (ModelState.IsValid)
            {
                string token = HttpContext.Session.GetString(Constants.AccessToken);
                var response = await _villaSvc.CreateAsync<ApiResponse>(createDto);

                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(createDto);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id)
        {
            string token = HttpContext.Session.GetString(Constants.AccessToken);
            var response = await _villaSvc.GetAsync<ApiResponse>(id);

            if (response != null && response.IsSuccess)
            {
                VillaDto villa = JsonConvert.DeserializeObject<VillaDto>(Convert.ToString(response.Result));

                return View(_mapper.Map<VillaUpdateDto>(villa));
            }

            return NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(VillaUpdateDto updateDto)
        {
            if (ModelState.IsValid)
            {
                string token = HttpContext.Session.GetString(Constants.AccessToken);
                var response = await _villaSvc.UpdateAsync<ApiResponse>(updateDto);

                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(updateDto);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            string token = HttpContext.Session.GetString(Constants.AccessToken);
            var response = await _villaSvc.GetAsync<ApiResponse>(id);

            if (response != null && response.IsSuccess)
            {
                VillaDto villa = JsonConvert.DeserializeObject<VillaDto>(Convert.ToString(response.Result));

                return View(villa);
            }

            return NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(VillaDto villa)
        {
            string token = HttpContext.Session.GetString(Constants.AccessToken);
            var response = await _villaSvc.DeleteAsync<ApiResponse>(villa.Id);

            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }
    }
}
