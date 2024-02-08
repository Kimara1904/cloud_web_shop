using Common.DTOs;
using Common.Interfaces;
using Common.Messages;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Newtonsoft.Json;
using SoCreate.ServiceFabric.PubSub.Subscriber;
using System.Text;
using System.Text.Json.Nodes;

namespace Client.Controllers
{
    [IgnoreAntiforgeryToken]
    public class ChartController : Controller
    {
        public string PaypalClientId { get; set; } = null!;
        private string PaypalSecret { get; set; } = null!;
        public string PaypalUrl { get; set; } = null!;

        public ChartController(IConfiguration configuration)
        {
            PaypalClientId = configuration["PaypalSettings:ClientId"];
            PaypalSecret = configuration["PaypalSettings:Secret"];
            PaypalUrl = configuration["PaypalSettings:Url"];
        }

        public async Task<IActionResult> Index(string payment = "checkout")
        {
            var chart = JsonConvert.DeserializeObject<Chart>(HttpContext.Session.GetString("ActiveChart")!)!;
            var user = JsonConvert.DeserializeObject<UserDTO>(HttpContext.Session.GetString("CurrentUser")!)!;
            double totalPrice = 0.0;

            foreach (var item in chart.Items)
            {
                totalPrice += item.Amount * Double.Parse(item.ArticlePrice.ToString("R"));
            }

            var serviceUri = new Uri("fabric:/Web-Shop/Validator");
            IValidator proxy = ServiceProxy.Create<IValidator>(serviceUri);

            Tuple<string, List<Chart>?>? result = null!;
            try
            {
                result = await proxy.ChartViewValidator(user.Id)!;

                if (result.Item2 == null)
                {
                    ViewBag.ErrorView = result.Item1;
                }
                chart.TotalPrice = Double.Parse(totalPrice.ToString("R"));
                ViewBag.Chart = chart;
                ViewBag.SelectedPayment = payment;
                return View(result.Item2);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorView = ex.Message;
                chart.TotalPrice = Double.Parse(totalPrice.ToString("R"));
                ViewBag.Chart = chart;
                ViewBag.SelectedPayment = payment;
                return View(result.Item2);
            }
        }

        [HttpPost]
        public IActionResult AddArticle(ChartItem article, string category)
        {
            Chart chart = JsonConvert.DeserializeObject<Chart>(HttpContext.Session.GetString("ActiveChart")!)!;

            chart.Items.Add(article);

            HttpContext.Session.SetString("ActiveChart", JsonConvert.SerializeObject(chart));
            return RedirectToAction("Index", "Article", category);
        }

        [HttpPost]
        public IActionResult DeleteArticle(long articleId)
        {
            Chart chart = JsonConvert.DeserializeObject<Chart>(HttpContext.Session.GetString("ActiveChart")!)!;

            var itemToRemove = chart.Items.FirstOrDefault(item => item.ArticleId == articleId);
            if (itemToRemove != null)
            {
                chart.Items.Remove(itemToRemove);
            }

            HttpContext.Session.SetString("ActiveChart", JsonConvert.SerializeObject(chart));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ModifyAmount(long articleId, int amount)
        {
            Chart chart = JsonConvert.DeserializeObject<Chart>(HttpContext.Session.GetString("ActiveChart")!)!;

            var itemToChange = chart.Items.FirstOrDefault(item => item.ArticleId == articleId);
            if (itemToChange != null)
            {
                itemToChange.Amount = amount;
            }

            HttpContext.Session.SetString("ActiveChart", JsonConvert.SerializeObject(chart));
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            Chart chart = JsonConvert.DeserializeObject<Chart>(HttpContext.Session.GetString("ActiveChart")!)!;

            double totalPrice = 0.0;

            foreach (var item in chart.Items)
            {
                totalPrice += item.Amount * Double.Parse(item.ArticlePrice.ToString("R"));
            }
            chart.TotalPrice = Double.Parse(totalPrice.ToString("R"));

            var serviceUri = new Uri("fabric:/Web-Shop/Validator");
            IValidator proxy = ServiceProxy.Create<IValidator>(serviceUri);

            try
            {
                var result = await proxy.CheckoutValidator(chart);
                if (!result.Equals("Successfully check out"))
                {
                    ViewBag.Error = result;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [Subscribe]
        private Task HandlePubMessage(PubMessage message)
        {
            ViewBag.Message = message;
            return Task.CompletedTask;
        }


        public async Task<JsonResult> CreateOrder()
        {
            Chart chart = JsonConvert.DeserializeObject<Chart>(HttpContext.Session.GetString("ActiveChart")!)!;

            double totalPrice = 0.0;

            foreach (var item in chart.Items)
            {
                totalPrice += item.Amount * Double.Parse(item.ArticlePrice.ToString("R"));
            }
            chart.TotalPrice = Double.Parse(totalPrice.ToString("R"));

            if (chart.Items.Count == 0)
            {
                return new JsonResult("");
            }

            var createOrderRequest = new JsonObject
            {
                { "intent", "CAPTURE" }
            };

            var purchaseUnits = new JsonArray
            {
                new JsonObject
                {
                    { "amount", new JsonObject
                        {
                            { "currency_code", "USD" },
                            { "value", chart.TotalPrice }
                        }
                    }
                }
            };

            createOrderRequest.Add("purchase_units", purchaseUnits);

            string accessToken = await GetPayPalAccessToken();

            string url = PaypalUrl + "/v2/checkout/orders";

            string orderID = "";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent(createOrderRequest.ToString(), null, "application/json");

                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var read = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(read);
                    if (jsonResponse is not null)
                    {
                        orderID = jsonResponse["id"]?.ToString() ?? "";
                    }
                }
            }

            return new JsonResult(new { id = orderID });
        }

        public async Task<JsonResult> CompleteOrder([FromBody] JsonObject data)
        {
            if (data is null || data["orderID"] is null) return new JsonResult("");

            var orderID = data["orderID"]!.ToString();

            string accessToken = await GetPayPalAccessToken();

            string url = PaypalUrl + $"/v2/checkout/orders/{orderID}/capture";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("", null, "application/json");

                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var read = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(read);
                    if (jsonResponse is not null)
                    {
                        orderID = jsonResponse["id"]?.ToString() ?? "";
                        string payPalOrderStatus = jsonResponse["status"]?.ToString() ?? "";

                        if (payPalOrderStatus.Equals("COMPLETED"))
                        {
                            return new JsonResult("success");
                        }
                    }
                }
            }

            return new JsonResult("");
        }

        private async Task<string> GetPayPalAccessToken()
        {
            string accessToken = "";

            string url = PaypalUrl + "/v1/oauth2/token";

            using (var client = new HttpClient())
            {
                string credentials64 =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(PaypalClientId + ":" + PaypalSecret));
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials64);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("grant_type=client_credentials", null,
                    "application/x-www-form-urlencoded");

                var response = await client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var read = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(read);
                    if (jsonResponse is not null)
                    {
                        accessToken = jsonResponse["access_token"]?.ToString() ?? "";
                    }
                }
            }

            return accessToken;
        }
    }
}
