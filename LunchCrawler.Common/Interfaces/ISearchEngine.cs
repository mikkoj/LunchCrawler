using System.Collections.Generic;


namespace LunchCrawler.Common.Interfaces
{
    /// <summary>
    /// Interface for an actual search engine (Google, Bing, Yahoo etc.)
    /// </summary>
    public interface ISearchEngine
    {
        string Name { get; }
        IList<string> SearchForLinks(string query);
    }
}
