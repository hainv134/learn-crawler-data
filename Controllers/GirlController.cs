using System.Text;
using Crawler.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Girl_Infor_Cherimo.Controllers
{
    public class GirlController : Controller
    {

        private GirlCrawler girlCrawler;

        public GirlController()
        {
            this.girlCrawler = new GirlCrawler("https://www.u-cherimo.com/girls");
        }
        public async Task<ActionResult> ListGirl()
        {
            ViewBag.jsonstring = Newtonsoft.Json.JsonConvert.SerializeObject(await girlCrawler.GetGirlList(), Newtonsoft.Json.Formatting.Indented);
            return View("List");
        }
        public async Task<ActionResult> GetGirlSchedule()
        {
            ViewBag.jsonstring = await girlCrawler.Output();
            return View("List");
        }
        [HttpPost]
        public async Task<ActionResult> GirlProfile(string girlName)
        {
            ViewBag.typename = $"GirlName: {girlName}";
            ViewBag.jsonstring = Newtonsoft.Json.JsonConvert.SerializeObject(await girlCrawler.GetGirlProfile(girlName), Newtonsoft.Json.Formatting.Indented);
            return View("List");
        }
    }
}
