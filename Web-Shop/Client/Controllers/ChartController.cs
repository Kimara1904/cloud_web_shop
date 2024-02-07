using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Client.Controllers
{
    public class ChartController : Controller
    {
        public IActionResult Index()
        {
            var chart = JsonConvert.DeserializeObject<Chart>(HttpContext.Session.GetString("ActiveChart")!)!;
            chart.TotalPrice = 0;

            foreach (var item in chart.Items)
            {
                chart.TotalPrice += item.Amount * item.ArticlePrice;
            }
            ViewBag.Chart = chart;
            return View();
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
    }
}
