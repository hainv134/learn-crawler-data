using HtmlAgilityPack;

namespace Crawler.Interface
{
    public interface ICrawlerBase<T>
    {
        Task<HtmlNode> GetDocumentNode();
        Task<HtmlNodeCollection> GetAllNodes(string xpath);
        Task<HtmlNodeCollection> GetNodePagination(int pagination, string routeParam, string xpath);
        T GetDetail(HtmlNode node);
        Task<string> Output();
        Task Export();

        string GetInnerText(HtmlNode node, string xpath);
        string GetInnerTexts(HtmlNode node, string xpath, char separator);
    }
}