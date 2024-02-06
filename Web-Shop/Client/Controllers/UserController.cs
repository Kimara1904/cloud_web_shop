using Common.DTOs;
using Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Newtonsoft.Json;
using System.Fabric;

namespace Client.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            UserDTO user = JsonConvert.DeserializeObject<UserDTO>(HttpContext.Session.GetString("CurrentUser")!)!;
            ViewBag.User = user;
            ViewBag.Result = TempData["Result"] as string ?? "";
            return View("Modify");
        }

        public async Task<IActionResult> Modify(ModifyDTO newUserInfo)
        {
            UserDTO user = JsonConvert.DeserializeObject<UserDTO>(HttpContext.Session.GetString("CurrentUser")!)!;
            newUserInfo.Id = user.Id;

            var serviceUri = new Uri("fabric:/Web-Shop/Validator");
            IValidator proxy = ServiceProxy.Create<IValidator>(serviceUri);

            try
            {
                var result = await proxy.ModifyValidator(newUserInfo);

                if (result.Item2 != null)
                {
                    HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(result.Item2));
                }

                TempData["Result"] = result.Item1;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Result"] = "Error in communication with service " + ex.Message;
                return RedirectToAction("ToLogin");
            }
        }
    }
}
