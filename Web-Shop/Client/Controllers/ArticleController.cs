using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public class ArticleController : Controller
    {
        public async Task<IActionResult> Index(string category = "All")
        {
            UserDTO user = JsonConvert.DeserializeObject<UserDTO>(HttpContext.Session.GetString("CurrentUser")!)!;
            ViewBag.SelectedCategory = category;
            ViewBag.Chart = JsonConvert.DeserializeObject<Chart>(HttpContext.Session.GetString("ActiveChart")!)!;

            var serviceUri = new Uri("fabric:/Web-Shop/Validator");
            IValidator proxy = ServiceProxy.Create<IValidator>(serviceUri);

            try
            {
                var result = await proxy.ArticleViewValidator(user, category);

                if (result.Item2 == null)
                {
                    ViewBag.Error = result.Item1;
                }
                return View(result.Item2);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error in communication with service " + ex.Message;
                return View(null);
            }
        }
    }
}
