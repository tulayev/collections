using Microsoft.AspNetCore.Mvc;

namespace Collections.Controllers
{
    public class LocaleController : Controller
    {
        public IActionResult ChangeLocale(string page, string segment, string culture, int? id)
        {
            Response.Cookies.Append("Locale", culture, 
                new CookieOptions
                {
                    Expires = DateTime.Now.AddMonths(1)
                });

            if (id != null)
            {
                return RedirectToAction(segment, page, new { id, culture });
            }
            
            return RedirectToAction(segment, page, new { culture });
        }
    }
}
