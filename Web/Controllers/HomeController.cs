using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Models.Dtos;
using Models;
using Newtonsoft.Json;
using System.Diagnostics;
using Web.Models;
using Web.Services.Interfaces;
using Utilities;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
		private readonly IVillaService _villaSvc;
		private readonly IMapper _mapper;

        public HomeController(
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
	}
}