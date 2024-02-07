using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult Index()
        {
            return ToLogin();
        }

        [HttpPost]
        public async Task<IActionResult> LoginUser(string username, string password)
        {
            try
            {
                var serviceUri = new Uri("fabric:/Web-Shop/Validator");

                IValidator proxy = ServiceProxy.Create<IValidator>(serviceUri);
                var result = await proxy.LoginValidator(username, password);

                if (result.Item2 != null)
                {
                    HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(result.Item2));
                    HttpContext.Session.SetString("ActiveChart", JsonConvert.SerializeObject(new Chart()
                    {
                        BuyerId = result.Item2.Id,
                        Address = result.Item2.Address,
                        Items = new List<ChartItem>()
                    }));
                    return RedirectToAction("Index", "Article");
                }
                else
                {
                    TempData["Error"] = result.Item1;
                    return RedirectToAction("ToLogin");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error in communication with service " + ex.Message;
                return RedirectToAction("ToLogin");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO newUser)
        {
            try
            {
                var serviceUri = new Uri("fabric:/Web-Shop/Validator");

                IValidator proxy = ServiceProxy.Create<IValidator>(serviceUri);
                string result = await proxy.RegisterValidator(newUser);

                if (result.Equals("Successfully created new user"))
                {
                    return RedirectToAction("ToLogin");
                }
                else
                {
                    TempData["Error"] = result;
                    return RedirectToAction("ToRegister");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error in communication with service " + ex.Message;
                return RedirectToAction("ToRegister");
            }
        }

        public IActionResult ToLogin()
        {
            ViewBag.Error = TempData["Error"] as string ?? "";
            return View("Login");
        }

        public IActionResult ToRegister()
        {
            ViewBag.Error = TempData["Error"] as string ?? "";
            return View("Register");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("ToLogin");
        }
    }
}
