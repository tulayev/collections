using Microsoft.AspNetCore.Mvc;

namespace Collections.Controllers
{
    public class LocaleController : Controller
    {
        public IActionResult ChangeLocale(string page, string segment, string culture, string? slug)
        {
            Response.Cookies.Append("Locale", culture, 
                new CookieOptions
                {
                    Expires = DateTime.Now.AddMonths(1)
                });

            if (slug != null)
            {
                return RedirectToAction(segment, page, new { slug, culture });
            }
            
            return RedirectToAction(segment, page, new { culture });
        }
    }
}
