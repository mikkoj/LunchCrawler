using System.Collections.Generic;


namespace LunchCrawler.Common.Interfaces
{
    /// <summary>
    /// Interface for the central search engine for the entire application.
    /// </summary>
    public interface ILunchMenuSearchEngine
    {
        List<string> SearchForLunchMenuURLs(string query);
        List<string> SearchForLunchMenuURLs(IList<string> queries);
    }
}
