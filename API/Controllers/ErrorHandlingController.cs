using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorHandlingController : ControllerBase
    {
        [HttpGet("ProcessError")]
        public ActionResult ProcessError([FromServices] IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();

                return Problem(
                    detail: feature.Error.StackTrace,
                    title: feature.Error.Message,
                    instance: env.EnvironmentName
                    );
            }
            else
            {
                return Problem();
            }
        }
    }
}
