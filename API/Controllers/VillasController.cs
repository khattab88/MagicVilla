using API.Data;
using API.Data.Repositories.Interfaces;
using API.Logging;
using Models;
using Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Controllers
{
    [Route("api/[controller]")]
    // [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class VillasController : ControllerBase
    {
        private readonly IVillaRepository _repo;
        private readonly IMapper _mapper;
        private readonly IBasicLogger _logger;

        protected ApiResponse _response;

        public VillasController(
            IVillaRepository repo,
            IMapper mapper,
            IBasicLogger logger
            )
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;

            this._response = new();
        }


        [HttpGet]
        // [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        // [ResponseCache(Duration = 30, Location = ResponseCacheLocation.None, NoStore = true)]
        // [ResponseCache(CacheProfileName = "Default30")]
        public async Task<ActionResult<ApiResponse>> GetVillas([FromQuery] int? occupancy, [FromQuery] string? search,
            int pageSize = 0, int pageNumber = 1)
        {
            try
            {
                List<Villa> villas = null;
                _logger.Log("Get all villas", "INFO");

                if (occupancy > 0)
                {
                    villas = await _repo.GetAllAsync(v => v.Occupancy == occupancy, pageSize: pageSize, pageNumber: pageNumber);
                }
                else
                {
                    villas = await _repo.GetAllAsync(pageSize: pageSize, pageNumber: pageNumber);
                }

                if(!string.IsNullOrEmpty(search))
                {
                    villas = villas.Where(v => v.Name.ToLower() ==  search.ToLower() ||
                        v.Amenity.ToLower() == search.ToLower())
                        .ToList();
                }

                Pagination paging = new Pagination() { PageNumber = pageNumber, PageSize = pageSize };
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paging));

                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<IEnumerable<VillaDto>>(villas);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Result = null;
                _response.ErrorMessages = new List<string>() { ex.Message };
            }

            return _response;
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VillaDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetVilla(int id)
        {
            try
            {
                if (id == 0) 
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;

                    return BadRequest(_response); 
                }

                var villa = await _repo.GetAsync(v => v.Id == id);

                if (villa == null)
                {
                    _logger.Log($"Villa with id {id} is not found", "ERROR");

                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;

                    return NotFound(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<VillaDto>(villa);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Result = null;
                _response.ErrorMessages = new List<string>() { ex.Message };
            }

            return _response;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CreateVilla([FromForm] VillaCreateDto dto)
        {
            try
            {
                if (await _repo.GetAsync(v => v.Name.ToLower() == dto.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("DuplicateNameError", "Villa name already exists");
                    return BadRequest(ModelState);
                }

                if (dto == null)
                {
                    return BadRequest(dto);
                }

                Villa villa = _mapper.Map<Villa>(dto);

                await _repo.CreateAsync(villa);

                if(dto.Image != null)
                {
                    string fileName = villa.Id + Path.GetExtension(dto.Image.FileName);
                    string filePath = @"wwwroot\ProductImages\" + fileName;

                    var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    FileInfo file = new FileInfo(directoryLocation);

                    if (file.Exists)
                    {
                        file.Delete();
                    }

                    using (var fileStream = new FileStream(directoryLocation, FileMode.Create))
                    {
                        dto.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    villa.ImageUrl = baseUrl + "/ProductImages/" + fileName;
                    villa.ImageLocalPath = filePath;
                }
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }

                await _repo.UpdateAsync(villa);

                _response.StatusCode = HttpStatusCode.Created;
                _response.Result = _mapper.Map<VillaDto>(villa);

                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Result = null;
                _response.ErrorMessages = new List<string>() { ex.Message };
            }

            return _response;
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "NAN")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteVilla(int id) 
        {
            try
            {
                if (id == 0) return BadRequest();

                var villa = await _repo.GetAsync(v => v.Id == id);

                if (villa == null)
                {
                    return NotFound();
                }

                await _repo.DeleteAsync(villa);

                _response.StatusCode = HttpStatusCode.NoContent;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Result = null;
                _response.ErrorMessages = new List<string>() { ex.Message };
            }

            return _response;
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDto dto) 
        {
            try
            {
                if (dto == null || id != dto.Id)
                {
                    return BadRequest();
                }

                // var villa = await _context.Villas.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
                var villa = await _repo.GetAsync(v => v.Id == id, false);

                if (villa == null)
                {
                    return NotFound();
                }

                villa = _mapper.Map<Villa>(dto);

                await _repo.UpdateAsync(villa);

                _response.StatusCode = HttpStatusCode.NoContent;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.Result = null;
                _response.ErrorMessages = new List<string>() { ex.Message };
            }

            return _response;
        }

        /* PATCH REQUEST (update name property)
         [
          {
            "path": "/name",
            "op": "replace",
            "value": "New Villa"
          }
         ]
         */
        [HttpPatch("{id:int}", Name = "PatchVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchVilla(int id, JsonPatchDocument<VillaUpdateDto> dto) 
        {
            if (dto == null || id == 0)
            {
                return BadRequest();
            }

            var villa = await _repo.GetAsync(v => v.Id == id, false);

            if (villa == null)
            {
                return NotFound();
            }

            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);

            dto.ApplyTo(villaDto, ModelState);

            if(!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            Villa model = _mapper.Map<Villa>(villaDto);

            await _repo.UpdateAsync(model);

            return NoContent();
        }
    }
}
