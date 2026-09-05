using Microsoft.AspNetCore.Mvc;

namespace ShopNext.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode:int}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewBag.StatusCode = statusCode;
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "The requested resource or page could not be found.";
                    return View("404");
                case 403:
                    return RedirectToAction("AccessDenied", "Account");
                case 500:
                default:
                    ViewBag.ErrorMessage = "An unexpected server error occurred. Our engineering team has been notified.";
                    return View("500");
            }
        }

        [Route("Error/500")]
        public IActionResult ServerError()
        {
            ViewBag.StatusCode = 500;
            ViewBag.ErrorMessage = "An internal server error occurred while processing your request.";
            return View("500");
        }
    }
}
