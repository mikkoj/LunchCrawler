using System.Collections.Generic;


namespace LunchCrawler.Common.Interfaces
{
    /// <summary>
    /// Interface for the central search engine for the entire application.
    /// </summary>
    public interface ILunchRestaurantSearchEngine
    {
        IList<string> SearchForLunchMenuURLs(string query);
        IList<string> SearchForLunchMenuURLs(IList<string> queries);
    }
}
