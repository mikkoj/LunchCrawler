using System;
using System.Linq;
using System.Collections.Generic;

using Bing;

using LunchCrawler.Common.Logging;
using LunchCrawler.Common.Interfaces;
using LunchCrawler.MenuSeeker.Test.Properties;


namespace LunchCrawler.MenuSeeker.Test.SearchEngines
{
    /// <summary>
    /// Search engine for lunch menus based on Bing's APIs.
    /// </summary>
    public class BingSearchEngine : ISearchEngine
    {
        public string Name { get { return "Bing"; } }
        public ILogger Logger { get; set; }

        public BingSearchEngine()
        {
            Logger = NullLogger.Instance;
        }

        private const string BingAPIKey = "8F6B6B86691981F718921581C1214CABA0C57B5D";
        private readonly int _bingSearchLimit = Settings.Default.BingSearchLimit;
        private readonly int _bingSearchCountForOneSearch = Settings.Default.BingSearchCountForOneSearch;

        public IList<string> SearchForLinks(string query)
        {
            var allResults = new List<string>();

            var offset = 0;
            while (offset < _bingSearchLimit)
            {
                var singleSearchResults = SingleBingSearch(query, _bingSearchCountForOneSearch, offset);
                allResults.AddRange(singleSearchResults ?? new List<string>());
                offset += _bingSearchCountForOneSearch;
            }

            return allResults;
        }

        public IList<string> SingleBingSearch(string query, int count, int offset = 0)
        {
            WebResponse webResponse;
            try
            {
                var searchRequest = new SearchRequest
                {
                    AppId = BingAPIKey,
                    Query = query,
                    Market = "fi-FI"
                };

                var webRequest = new WebRequest
                {
                    Count = (uint)count,
                    Offset = (uint)offset
                };

                webResponse = API.Web(searchRequest, webRequest);
            }
            catch (Exception)
            {
                return null;
            }

            if (webResponse.Errors.Count > 0)
            {
                var errorMessages = webResponse.Errors.Select(e =>
                {
                    var error = e.Message;
                    if (!string.IsNullOrEmpty(e.Parameter))
                    {
                        error = string.Format("{0} ({1})", error, e.Parameter);
                    }
                    return error;
                });

                var errorMessage = String.Join("\n", errorMessages);
                Logger.Error("There were unexpected API errors: " + errorMessage);
            }

            if (webResponse.Total == 0 ||
                webResponse.Results.Count == 0)
            {
                Logger.Info("Bing search for query {0} returned 0 results.", query);
            }

            return webResponse.Results.Select(result => result.Url).ToList();
        }
    }
}
