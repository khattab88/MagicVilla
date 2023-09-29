using API.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Dtos.Identity;
using System.Net;

namespace API.Controllers
{
    [Route("api/UsersAuth")]
    [ApiController]
    // [ApiVersion("1.0")]
    //[Route("api/v{version:ApiVersion}/UsersAuth")]
    [ApiVersionNeutral]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepo;
        protected ApiResponse _response;

        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _response = new();
        }

        [HttpGet("error")]
        public async Task<IActionResult> Error()
        {
            throw new FileNotFoundException();
        }

        [HttpGet("ImageError")]
        public async Task<IActionResult> ImageError()
        {
            throw new BadImageFormatException("Bad Image Exception");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request) 
        {
            var loginResponse = await _userRepo.Login(request);

            if(loginResponse.User == null || string.IsNullOrEmpty(loginResponse.Token.AccessToken)) 
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Username or password are incorrect");

                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            _response.Result = loginResponse;

            return Ok(_response);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            bool isUniqueUser = _userRepo.IsUnique(request.UserName);

            if (!isUniqueUser) 
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.ErrorMessages.Add("Username already exists");

                return BadRequest(_response);
            }

            var user = await _userRepo.Register(request);

            if(user == null) 
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _response.ErrorMessages.Add("error while registering");

                return BadRequest(_response);
            }

            _response.StatusCode = System.Net.HttpStatusCode.OK;
            _response.Result = user;

            return Ok(_response);
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenDto token) 
        {
            if (ModelState.IsValid)
            {
                var newToken = await _userRepo.RefreshAccessToken(token);

                if(newToken == null || string.IsNullOrEmpty(newToken.AccessToken)) 
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.Result = "Invalid Token";
                    return BadRequest(_response);
                }

                _response.Result = newToken;
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            else
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.Result = "Invalid Input";
                return BadRequest(_response);
            }
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeAccessToken(TokenDto token)
        {
            if (ModelState.IsValid)
            {
                await _userRepo.RevokeRefreshToken(token);
                
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }

            _response.IsSuccess = false;
            _response.Result = "Invalid Input";
            return BadRequest(_response);
        }
    }
}
