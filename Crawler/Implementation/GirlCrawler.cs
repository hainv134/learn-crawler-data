using System.Text;
using System.Text.RegularExpressions;
using Girl_Infor_Cherimo.Models;
using HtmlAgilityPack;

namespace Crawler.Implementation
{
    public class GirlCrawler : CrawlerBase<Girl>
    {
        private string? _girlDetailLink;
        private ScheduleCrawler _scheduleCrawler;
        public GirlCrawler(string url) : base(url)
        {
            this._girlDetailLink = url + "/view/";
            this._scheduleCrawler = new ScheduleCrawler("https://www.u-cherimo.com/schedules");
        }
        public override async Task<string> Output()
        {
            var list = await GetGirlList();
            var girlProfiles = new List<object>();
            var schedules = await _scheduleCrawler.GetAllGirlSchedule(DateTime.Now);
            foreach (KeyValuePair<string, string> entry in list)
            {
                var girlProfile = await GetGirl(entry.Value);
                if (schedules.ContainsKey(entry.Key))
                {
                    var schedule = schedules[entry.Key];
                    girlProfiles.Add(new { girlProfile, schedule });
                }
                else
                {
                    girlProfiles.Add(new { girlProfile });
                }
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(girlProfiles, Newtonsoft.Json.Formatting.Indented);
        }
        public async Task<Dictionary<string, string>> GetGirlList()
        {
            var girlList = new Dictionary<string, string>();
            var rootNode = await GetAllNodes("//*[@id='mainContentsInner']/div[2]/div[2]/ul/li");
            foreach (var node in rootNode)
            {
                string[] strHref = node.SelectSingleNode(".//a[contains(@class, 'listedGirlFrame')]").Attributes["href"].Value.Split("/");
                var girlId = strHref[strHref.Length - 1];
                var girlName = GetInnerText(node, ".//span[contains(@class, 'pName')]/text()");

                // If two girls has the same name, pick the first girl
                if (!girlList.ContainsKey(girlName))
                {
                    girlList.Add(girlName, girlId);
                }
            }
            return girlList;
        }
        public async Task<Girl> GetGirl(string girlId)
        {
            UrlBase = _girlDetailLink + girlId;
            var girlProfile = new Girl();
            var rootNode = (await GetAllNodes("//*[@id='mainContents']")).FirstOrDefault();

            Console.WriteLine(UrlBase);

            if (rootNode is not null)
            {
                // Required nodes
                var attributeNodes = rootNode.SelectNodes(".//ul[contains(@class, 'pCategory')]/li");
                var imageNodes = rootNode.SelectNodes(".//ul[contains(@class, 'thumb-item-nav')]/li");
                var age = Regex.Replace(
                    GetInnerText(rootNode, ".//span[contains(@class, 'age')]"),
                    "[()]",
                    String.Empty
                ).Trim();
                var threeSize = Regex.Replace(
                                        GetInnerText(rootNode, ".//span[contains(@class, 'threeSize')]"),
                                        "[a-zA-Z:()]",
                                        String.Empty
                                ).Trim().Split(' ');
                var attributes = new List<string>();
                if (attributeNodes is not null)
                {
                    foreach (var attributeNode in attributeNodes)
                    {
                        attributes.Add(attributeNode.InnerText);
                    }
                }
                var images = new List<string>();
                if (imageNodes is not null)
                {
                    foreach (var imageNode in imageNodes)
                    {
                        images.Add(imageNode.SelectSingleNode(".//img").Attributes["src"].Value);
                    }
                }
                // Assign value to Girl
                girlProfile.Id = girlId;
                girlProfile.Name = GetInnerText(rootNode, ".//h2[contains(@class, 'mainSub')]/text()").Trim();
                girlProfile.Comment = GetInnerText(rootNode, ".//span[contains(@class, 'pComment')]");
                girlProfile.Age = age;
                girlProfile.Tall = threeSize[0];
                girlProfile.Butt = threeSize[1];
                girlProfile.Waist = threeSize[2];
                girlProfile.Hip = threeSize[3];
                girlProfile.Attributes = attributes.ToArray();
                girlProfile.Images = images.ToArray();

                var shopComment = GetInnerText(rootNode, ".//div[contains(@class, 'shopCTxt')]/div[contains(@class, 'comment_text')]");
                girlProfile.ShopComment = shopComment.Trim().Replace("\r\n", "").Replace("\n", "");
            }
            return girlProfile;
        }
        public async Task<object> GetGirlProfile(string girlName)
        {
            var list = await GetGirlList();
            var schedules = await _scheduleCrawler.GetAllGirlSchedule(DateTime.Now);
            var girlProfile = await GetGirl(list[girlName]);
            var schedule = schedules[girlName];
            return new
            {
                girlProfile,
                schedule
            };
        }
    }
}