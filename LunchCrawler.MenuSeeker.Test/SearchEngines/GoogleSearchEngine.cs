using System;
using System.Linq;
using System.Collections.Generic;

using Google.API.Search;

using LunchCrawler.Common;
using LunchCrawler.Common.Interfaces;
using LunchCrawler.Common.Logging;


namespace LunchCrawler.MenuSeeker.Test.SearchEngines
{
    /// <summary>
    /// Search engine for lunch menus based on Google's APIs.
    /// </summary>
    public class GoogleSearchEngine : ISearchEngine
    {
        public string Name { get { return "Google"; } }
        public ILogger Logger { get; set; }

        public GoogleSearchEngine()
        {
            Logger = NullLogger.Instance;
        }

        public IList<string> SearchForLinks(string query)
        {
            try
            {
                return SearchWithGoogleAPI(query);
                //return SearchWithGoogleRaw(query);
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Couldn't search Google with query: {0}", query), ex);
                return null;
            }
        }


        private static IList<string> SearchWithGoogleRaw(string query)
        {
            var queryUrl = string.Format("http://www.google.com/search?as_q={0}&num=100&hl=fi", query);
            var searchResult = Utils.GetLunchRestaurantDocumentForUrl(queryUrl);

            if (searchResult != null)
            {
                return searchResult.HtmlDocument
                                   .DocumentNode
                                   .DescendantNodes()
                                   .Where(Utils.IsLink)
                                   .Select(node => node.Attributes["href"].Value)
                                   .Where(link => link.StartsWith("http://") && !link.ToLower().Contains("google"))
                                   .ToList();
            }

            return null;
        }


        private static IList<string> SearchWithGoogleAPI(string query)
        {
            var gclient = new GwebSearchClient("mysite");
            var searchResult = gclient.Search(query, 1000);

            if (searchResult != null)
            {
                return searchResult.Select(result => result.Url).ToList();
            }

            return null;
        }
    }
}
