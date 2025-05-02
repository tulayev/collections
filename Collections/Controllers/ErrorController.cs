using Microsoft.AspNetCore.Mvc;

namespace Collections.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public ViewResult Index()
        {
            return View();
        }
    }
}
