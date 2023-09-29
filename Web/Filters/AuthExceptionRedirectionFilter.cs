using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Web.Services.Exceptions;

namespace Web.Filters
{
    public class AuthExceptionRedirectionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if(context.Exception is AuthException)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
