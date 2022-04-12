using System.Net;
using Crawler.Interface;
using Girl_Infor_Cherimo.Services;
using HtmlAgilityPack;

namespace Crawler.Implementation
{

    public abstract class CrawlerBase<T> : ICrawlerBase<T> where T : class
    {
        protected string UrlBase;
        protected string Method = "GET";
        protected int TotalRecord;
        protected int TaskRecord = 100;
        protected HttpContent? PostContent;
        private HttpClientServices _client;

        public CrawlerBase(string url)
        {
            this.UrlBase = url;
            this._client = new HttpClientServices();
        }

        public async Task Export()
        {
            using (StreamWriter file = File.CreateText($"craw_{typeof(T).Name}.json"))
            {
                await file.WriteLineAsync(await Output());
            }
        }

        public virtual T GetDetail(HtmlNode node) => throw new NotImplementedException();
        public virtual Task<string> Output() => throw new NotImplementedException();

        public async Task<HtmlNode> GetDocumentNode() => await _client.GetDocumentNode(UrlBase, Method, PostContent);
        public async Task<HtmlNodeCollection> GetAllNodes(string xpath) => (await GetDocumentNode()).SelectNodes(xpath);
        public async Task<HtmlNodeCollection> GetNodePagination(int pagination, string routeParam, string xpath)
        {
            var nodeCollection = await GetAllNodes(xpath);
            int currentPage = 1;
            while (nodeCollection.Count <= TaskRecord * pagination)
            {
                // update URL corresponding with current Page
                if (UrlBase.Contains(routeParam))
                {
                    int index = UrlBase.IndexOf(routeParam);
                    UrlBase = UrlBase.Substring(0, index + routeParam.Length) + "=" + ++currentPage;
                }
                else
                {
                    UrlBase = UrlBase + $"&{routeParam}={++currentPage}";
                }
                var rootNode = await GetAllNodes(xpath);
                foreach (var node in rootNode)
                {
                    nodeCollection.Add(node);
                    if (nodeCollection.Count == TaskRecord * pagination)
                        return nodeCollection;
                }
            }
            if (nodeCollection.Count > TaskRecord * pagination)
            {
                nodeCollection.Take(TaskRecord * pagination);
            }
            return nodeCollection;
        }

        public string GetInnerText(HtmlNode node, string xpath)
        {
            HtmlNode? text = node.SelectSingleNode(xpath);
            return text != null ? text.InnerText.Trim() : String.Empty;
        }

        public string GetInnerTexts(HtmlNode node, string xpath, char separator)
        {
            string[] text = node.SelectNodes(xpath)
                            .ToList()
                            .Select(n => n.InnerText)
                            .ToArray();
            return (text.Length > 0) ? String.Join(separator, text) : String.Empty;
        }
    }
}