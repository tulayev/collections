using Microsoft.AspNetCore.Mvc;

namespace Collections.Controllers
{
    public class LocaleController : Controller
    {
        public IActionResult ChangeLocale(string page, string culture, int? id)
        {
            Response.Cookies.Append("Locale", culture, 
                new CookieOptions
                {
                    Expires = DateTime.Now.AddMonths(1)
                });

            if (id != null)
            {
                return RedirectToAction(page, "Home", new { id, culture });
            }
            
            return RedirectToAction(page, "Home", new { culture });
        }
    }
}
