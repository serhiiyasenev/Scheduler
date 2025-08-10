using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Scheduler.BLL.Models;

namespace Scheduler.BLL.Services;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is EntityNotFoundException ex)
        {
            context.Result = new NotFoundObjectResult(new { ex.Message });
            context.ExceptionHandled = true;
        }
    }
}