using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Models;
using Models.Dtos.Identity;
using Models.Enums;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Utilities.Enums;
using Web.Services.Exceptions;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ITokenProvider _tokenProvider;
        private readonly IHttpContextAccessor _contextAccessor;

        private string _baseUrl;

        public ApiResponse Response { get; set; }

        public BaseService(
            IHttpClientFactory httpClient, 
            ITokenProvider tokenProvider, 
            IConfiguration configuration,
            IHttpContextAccessor contextAccessor)
        {
            _httpClient = httpClient;

            this.Response = new();
            _tokenProvider = tokenProvider;
            _contextAccessor = contextAccessor;

            _baseUrl = configuration.GetValue<string>("Services:VillaAPI");
        }

        public async Task<T> SendRequestAsync<T>(ApiRequest request)
        {
            try
            {
                var client = _httpClient.CreateClient("MagicVillaAPI");

                var messageFactory = () => 
                {
                    HttpRequestMessage message = new();

                    if (request.ContentType == ContentType.MultiPartFormData)
                    {
                        message.Headers.Add("Accept", "*/*");
                    }
                    else
                    {
                        message.Headers.Add("Accept", "application/json");
                    }

                    message.RequestUri = new Uri(request.Url);


                    if (request.ContentType == ContentType.MultiPartFormData)
                    {
                        var formContent = new MultipartFormDataContent();

                        foreach (var prop in request.Data.GetType().GetProperties())
                        {
                            var value = prop.GetValue(request.Data);

                            if (value is FormFile)
                            {
                                var file = (FormFile)value;
                                if (file != null)
                                {
                                    formContent.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                                }
                            }
                            else
                            {
                                formContent.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);
                            }
                        }

                        message.Content = formContent;
                    }
                    else
                    {
                        if (request.Data != null)
                        {
                            message.Content = new StringContent(JsonConvert.SerializeObject(request.Data), Encoding.UTF8, "application/json");
                        }
                    }


                    switch (request.RequestType)
                    {
                        case RequestType.POST:
                            message.Method = HttpMethod.Post;
                            break;
                        case RequestType.PUT:
                            message.Method = HttpMethod.Put;
                            break;
                        case RequestType.DELETE:
                            message.Method = HttpMethod.Delete;
                            break;
                        case RequestType.PATCH:
                            message.Method = HttpMethod.Patch;
                            break;
                        default:
                            message.Method = HttpMethod.Get;
                            break;
                    }

                    return message;
                };

                HttpResponseMessage response = null;

                // response = await client.SendAsync(messageFactory());
                response = await SendWithRefreshTokenAsync(client, messageFactory);

                var content = await response.Content.ReadAsStringAsync();

                var apiReposne = JsonConvert.DeserializeObject<T>(content);

                return apiReposne;
            }
            catch (AuthException authException) 
            {
                throw;
            }
            catch (Exception ex)
            {
                var errorReposne = new ApiResponse
                {
                    ErrorMessages = new List<string> { Convert.ToString(ex.Message) },
                    IsSuccess = false
                };

                var res = JsonConvert.SerializeObject(errorReposne);

                var apiReponse = JsonConvert.DeserializeObject<T>(res);

                return apiReponse;
            }
        }

        private async Task<HttpResponseMessage> SendWithRefreshTokenAsync(HttpClient httpClient,
            Func<HttpRequestMessage> httpRequestMessageFactory)
        {
            TokenDto token = _tokenProvider.Get();

            if(token != null && !string.IsNullOrEmpty(token.AccessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            }

            try
            {
                var response = await httpClient.SendAsync(httpRequestMessageFactory());

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                // if it fails we can pass refresh token
                if(!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // generate new refresh token & sign in with new token and then retry
                    await InvokeRefreshTokenEndpoint(httpClient, token.AccessToken, token.RefreshToken);
                    response = await httpClient.SendAsync(httpRequestMessageFactory());
                    return response;
                }

                return response;
            }
            catch (AuthException authException) 
            {
                throw;
            }
            catch (HttpRequestException httpRequestException)
            {
                if(httpRequestException.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // refresh token and retry
                    await InvokeRefreshTokenEndpoint(httpClient, token.AccessToken, token.RefreshToken);
                    return await httpClient.SendAsync(httpRequestMessageFactory());
                }
                throw;
            }
        }

        private async Task InvokeRefreshTokenEndpoint(HttpClient httpClient, string accessToken, string refreshToken)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add("Accept", "application/json");
            request.RequestUri = new Uri($"{_baseUrl}/api/UsersAuth/refresh");
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(JsonConvert.SerializeObject(new TokenDto() 
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(content);

            if(apiResponse?.IsSuccess != true)
            {
                await _contextAccessor.HttpContext.SignOutAsync();
                _tokenProvider.Clear();

                throw new AuthException();
            }
            else
            {
                var tokenDataStr = JsonConvert.SerializeObject(apiResponse.Result);
                var tokenDto = JsonConvert.DeserializeObject<TokenDto>(tokenDataStr);

                if(tokenDto != null && !string.IsNullOrEmpty(tokenDto.AccessToken))
                {
                    // sign user in 
                    await SignInWithNewToken(tokenDto);

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDto.AccessToken);
                }
            }
        }

        private async Task SignInWithNewToken(TokenDto token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token.AccessToken);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Claims.FirstOrDefault(c => c.Type == "unique_name").Value));
            identity.AddClaim(new Claim(ClaimTypes.Role, jwt.Claims.FirstOrDefault(c => c.Type == "role").Value));

            var principal = new ClaimsPrincipal(identity);
            await _contextAccessor.HttpContext.SignInAsync(principal);

            _tokenProvider.Set(token);
        }
    }
}
