using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.Dtos.Identity;
using Models.Enums;
using Newtonsoft.Json;
using System.Security.Claims;
using Utilities;
using Web.Services.Interfaces;

namespace Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authSvc;
        private readonly ITokenProvider _tokenProvider;

        

        public AuthController(IAuthService authSvc, ITokenProvider tokenProvider)
        {
            _authSvc = authSvc;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDto loginRequest = new();

            return View(loginRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto loginRequest) 
        {
            var response = await _authSvc.LoginAsync<ApiResponse>(loginRequest);

            if(response != null && response.IsSuccess)
            {
                LoginResponseDto loginResponse = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(response.Result));

                // sign in
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.Name, loginResponse.User.UserName));
                identity.AddClaim(new Claim(ClaimTypes.Role, loginResponse.Role));

                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(principal);

                // save token into session
                // HttpContext.Session.SetString(Constants.AccessToken, loginResponse.Token.AccessToken);
                _tokenProvider.Set(new TokenDto { AccessToken = loginResponse.Token.AccessToken });

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("ErrorMessages", response.ErrorMessages.FirstOrDefault());
                return View(loginRequest);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>() 
            {
                new SelectListItem{ Text = Role.Admin.ToString(), Value = Role.Admin.ToString() },
                new SelectListItem{ Text = Role.Customer.ToString(), Value = Role.Customer.ToString() }
            };

            ViewBag.RoleList = roleList;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequestDto registerRequest)
        {
            if (string.IsNullOrEmpty(registerRequest.Role))
            {
                registerRequest.Role = Role.Admin.ToString();
            }

            var response = await _authSvc.RegisterAsync<ApiResponse>(registerRequest);

            if(response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(Login));
            }

            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{ Text = Role.Admin.ToString(), Value = Role.Admin.ToString() },
                new SelectListItem{ Text = Role.Customer.ToString(), Value = Role.Customer.ToString() }
            };

            ViewBag.RoleList = roleList;

            return View(registerRequest);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            var token = _tokenProvider.Get();
            await _authSvc.LogoutAsync<ApiResponse>(token);
            _tokenProvider.Clear();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
