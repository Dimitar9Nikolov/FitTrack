using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FitTrack.Controllers;

public class ErrorsController : Controller
{
    [Route("errors/{statusCode:int}")]
    public IActionResult HandleStatusCode(int statusCode)
    {
        return statusCode switch
        {
            404 => View("NotFound"),
            403 => View("Forbidden"),
            _   => View("ServerError")
        };
    }

    [Route("errors/exception")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Exception()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        // Log the exception here in a real app — omitted for brevity
        return View("ServerError");
    }
}
