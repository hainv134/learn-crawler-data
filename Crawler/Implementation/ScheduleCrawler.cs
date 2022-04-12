using System.Text;
using System.Text.RegularExpressions;
using Crawler.Interface;
using Girl_Infor_Cherimo.Models;
using HtmlAgilityPack;

namespace Crawler.Implementation
{
    public class ScheduleCrawler : CrawlerBase<Schedule>
    {

        private string routeParam;

        public ScheduleCrawler(string url) : base(url)
        {
            this.routeParam = url + "?date=";
        }

        public override Schedule GetDetail(HtmlNode node)
        {
            var schedule = new Schedule();
            var pName = GetInnerText(node, ".//span[contains(@class, 'pName')]/text()");
            schedule.girlName = pName.Trim();
            var pWorkTime = Regex.Split(
                GetInnerText(node, ".//div[contains(@class, 'listedGirlProf')]/div[contains(@class, 'pSpecial')]"),
                @"[^0-9:]"
            );
            schedule.StartTime = pWorkTime[0].Trim();
            schedule.EndTime = pWorkTime[1].Trim();
            return schedule;
        }

        public async Task<List<Schedule>> GetSchedules(DateTime startDate)
        {
            var schedules = new List<Schedule>();
            DateTime endDate = startDate.AddDays(7);
            for (var date = startDate.Date; date.Date < endDate.Date; date = date.AddDays(1))
            {
                var currentDate = date.ToString("yyyy-MM-dd");
                UrlBase = routeParam + currentDate;
                var rootNode = await GetAllNodes("//*[@id='scheListIndex']/div[2]/ul/li");
                foreach (var node in rootNode)
                {
                    var schedule = GetDetail(node);
                    schedule.Date = date.ToString("yyyyMMdd");
                    schedules.Add(schedule);
                }
            }
            return schedules;
        }
        public async Task<Dictionary<string, List<Schedule>>> GetAllGirlSchedule(DateTime startDate)
        {
            var result = new Dictionary<string, List<Schedule>>();
            var schedules = await GetSchedules(startDate);
            foreach (var schedule in schedules.ToList())
            {
                if (result.ContainsKey(schedule.girlName))
                {
                    result[schedule.girlName].Add(schedule);
                }
                else
                {
                    var girlSche = new List<Schedule>();
                    girlSche.Add(schedule);
                    result.Add(schedule.girlName, girlSche);
                }
            }

            return result;
        }
    }
}