using HtmlAgilityPack;

namespace Girl_Infor_Cherimo.Services
{
    public interface IHttpClientServices
    {
        public Task<HtmlNode> GetDocumentNode(string url, string method, HttpContent? content);
    }

    public class HttpClientServices : IHttpClientServices
    {
        private HttpClient client;
        public HttpClientServices()
        {
            this.client = new HttpClient();
        }
        public async Task<HtmlNode> GetDocumentNode(string url, string method, HttpContent? content)
        {
            HttpResponseMessage response = (method.Equals("POST"))
                                                ? client.PostAsync(url, content).Result
                                                : client.GetAsync(url).Result;
            var html = await (response.Content.ReadAsStringAsync());
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document.DocumentNode;
        }
    }
}