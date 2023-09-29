using API.Data.Repositories.Interfaces;
using Models;
using Models.Dtos;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Models;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    // [Route("api/v{version:ApiVersion}/[controller]")]
    public class VillaNumbersController : ControllerBase
    {
        private readonly IVillaRepository _villaRepo;
        private readonly IVillaNumberRepository _villaNumberRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<VillaNumbersController> _logger;

        protected ApiResponse _response;

        public VillaNumbersController(
            IVillaRepository villaRepo,
            IVillaNumberRepository villaNumberRepo,
            IMapper mapper,
            ILogger<VillaNumbersController> logger
            )
        {
            _villaRepo = villaRepo;
            _villaNumberRepo = villaNumberRepo;
            _mapper = mapper;
            _logger = logger;

            _response = new();
        }


        [HttpGet]
        [Authorize]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> GetVillaNumbers() 
        {
            try
            {
                _logger.LogInformation("Get all villas");

                var villaNumbers = await _villaNumberRepo.GetAllAsync(includedProperties: "Villa");

                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<IEnumerable<VillaNumberDto>>(villaNumbers);

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

        [HttpGet]
        [MapToApiVersion("2.0")]
        public IEnumerable<string> Get()
        {
            return new string[] { "yes", "no" };
        }

        [HttpGet("{number:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VillaNumberDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetVillaNumber(int number)
        {
            try
            {
                if (number == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;

                    return BadRequest(_response);
                }

                var villaNumber = await _villaNumberRepo.GetAsync(v => v.Number == number, includedProperties: "Villa");

                if (villaNumber == null)
                {
                    _logger.LogError($"Villa Number with number {number} is not found");

                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;

                    return NotFound(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = _mapper.Map<VillaNumberDto>(villaNumber);

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDto dto)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(ModelState);
                //}

                if(await _villaRepo.GetAsync(v => v.Id == dto.VillaId) == null) 
                {
                    ModelState.AddModelError("ErrorMessages", "Invalid villa id");

                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.Result = ModelState;

                    return BadRequest(_response);
                }

                if (await _villaNumberRepo.GetAsync(v => v.Number == dto.Number) != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Villa number already exists");

                    _response.StatusCode=HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.Result = ModelState;

                    return BadRequest(_response);
                }

                if (dto == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.Result = ModelState;

                    return BadRequest(_response);
                }

                VillaNumber villaNumber = _mapper.Map<VillaNumber>(dto);
                villaNumber.CreatedAt = DateTime.Now;

                await _villaNumberRepo.CreateAsync(villaNumber);

                _response.StatusCode = HttpStatusCode.Created;
                _response.Result = villaNumber;

                // return CreatedAtRoute("GetVilla", new { id = villaNumber.Number }, _response);
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

        [HttpDelete("{number:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteVillaNumber(int number)
        {
            try
            {
                if (number == 0) return BadRequest();

                var villaNumber = await _villaNumberRepo.GetAsync(v => v.Number == number);

                if (villaNumber == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;

                    return NotFound(_response);
                }

                await _villaNumberRepo.DeleteAsync(villaNumber);

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

        [HttpPut("{number:int}", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateVillaNumber(int number, [FromBody] VillaNumberUpdateDto dto)
        {
            try
            {
                if (dto == null || number != dto.Number)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;

                    return BadRequest(_response);
                }

                if (await _villaNumberRepo.GetAsync(v => v.Number == dto.Number) == null)
                {
                    ModelState.AddModelError("DuplicateNumberError", "Villa number already exists");

                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.Result = ModelState;

                    return BadRequest(_response);
                }

                var villaNumber = await _villaNumberRepo.GetAsync(v => v.Number == number, false);

                if (villaNumber == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;

                    return NotFound(_response);
                }

                villaNumber = _mapper.Map<VillaNumber>(dto);

                await _villaNumberRepo.UpdateAsync(villaNumber);

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

        [HttpPatch("{number:int}", Name = "PatchVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PatchVilla(int number, JsonPatchDocument<VillaNumberUpdateDto> dto)
        {
            if (dto == null || number == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;

                return BadRequest(_response);
            }

            var villaNumber = await _villaNumberRepo.GetAsync(v => v.Number == number, false);

            if (villaNumber == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound; 
                _response.IsSuccess = false;

                return NotFound(_response);
            }

            VillaNumberUpdateDto villaNumberDto = _mapper.Map<VillaNumberUpdateDto>(villaNumber);

            dto.ApplyTo(villaNumberDto, ModelState);

            if (!ModelState.IsValid)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.Result = ModelState;

                return BadRequest(_response);
            }

            VillaNumber model = _mapper.Map<VillaNumber>(villaNumberDto);
            model.UpdatedAt = DateTime.Now;

            await _villaNumberRepo.UpdateAsync(model);

            _response.StatusCode = HttpStatusCode.NoContent;

            return Ok(_response);
        }
    }
}
