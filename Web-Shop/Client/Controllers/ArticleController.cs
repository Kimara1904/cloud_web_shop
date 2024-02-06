using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class ArticleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
