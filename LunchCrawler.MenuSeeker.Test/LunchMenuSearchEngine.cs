using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using LunchCrawler.Common;
using LunchCrawler.Common.Interfaces;


namespace LunchCrawler.MenuSeeker.Test
{
    /// <summary>
    /// Acts as the 'main' search engine for the entire application.
    /// Aggregates results from all the ISearchEngines defined within the assembly.
    /// </summary>
    [Export(typeof(ILunchMenuSearchEngine))]
    public class LunchMenuSearchEngine : ILunchMenuSearchEngine
    {
        /// <summary>
        /// Collection of found search engines in this solution. MEF will compose the collection.
        /// </summary>
        [ImportMany(typeof(ISearchEngine))]
        public IEnumerable<ISearchEngine> SearchEngines { get; set; }


        /// <summary>
        /// Searches for lunch menu URLs through all search engines with a given query string.
        /// </summary>
        /// <param name="query">Query string to search with.</param>
        public List<string> SearchForLunchMenuURLs(string query)
        {
            return SearchEngines.SelectMany(engine => engine.SearchForLinks(query) ?? new List<string>()).Distinct(new UrlComparer()).ToList();
        }


        /// <summary>
        /// Searches for lunch menu URLs through all search engines with given query strings.
        /// </summary>
        /// <param name="queries">Query-strings to search with.</param>
        public List<string> SearchForLunchMenuURLs(IList<string> queries)
        {
            return queries.SelectMany(SearchForLunchMenuURLs).Distinct(new UrlComparer()).ToList();
        }
    }
}
