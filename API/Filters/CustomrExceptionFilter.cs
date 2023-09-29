using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters
{
    public class CustomrExceptionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if(context.Exception is FileNotFoundException fileNotFoundException)
            {
                context.Result = new ObjectResult("file not found (handled by exception filter)") 
                {
                    StatusCode = 503
                };

                context.ExceptionHandled = false;
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
