using System.Linq;
using System.Collections.Generic;

using LunchCrawler.Common;
using LunchCrawler.Common.Interfaces;
using LunchCrawler.Common.Logging;


namespace LunchCrawler.MenuSeeker.Test
{
    /// <summary>
    /// Acts as the 'main' search engine for the entire application.
    /// Aggregates results from all the ISearchEngines defined within the assembly.
    /// </summary>
    public class LunchRestaurantSearchEngine : ILunchRestaurantSearchEngine
    {
        public ILogger Logger { get; set; }

        public LunchRestaurantSearchEngine()
        {
            Logger = NullLogger.Instance;
        }

        /// <summary>
        /// Collection of found search engines in this solution.
        /// </summary>
        public IEnumerable<ISearchEngine> SearchEngines { get; set; }


        public LunchRestaurantSearchEngine(IEnumerable<ISearchEngine> engines)
        {
            SearchEngines = engines;
        }

        /// <summary>
        /// Searches for lunch menu URLs through all search engines with a given query string.
        /// </summary>
        /// <param name="query">Query string to search with.</param>
        public IList<string> SearchForLunchMenuURLs(string query)
        {
            return SearchEngines.SelectMany(engine =>
            {
                var linksFromEngine = engine.SearchForLinks(query) ?? new List<string>();
                Logger.Info("{0} - found {1} links for '{2}'", engine.Name, linksFromEngine.Count, query);
                return linksFromEngine;
            })
            .Distinct(new UrlComparer())
            .ToList();
        }


        /// <summary>
        /// Searches for lunch menu URLs through all search engines with given query strings.
        /// </summary>
        /// <param name="queries">Query-strings to search with.</param>
        public IList<string> SearchForLunchMenuURLs(IList<string> queries)
        {
            return queries.SelectMany(SearchForLunchMenuURLs)
                          .AsParallel()
                          .Distinct(new UrlComparer())
                          .ToList();
        }
    }
}
