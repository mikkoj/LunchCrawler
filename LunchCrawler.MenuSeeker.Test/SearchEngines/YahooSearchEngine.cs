using System;
using System.Linq;
using System.Collections.Generic;

using LunchCrawler.Common;
using LunchCrawler.Common.Logging;
using LunchCrawler.Common.Interfaces;
using LunchCrawler.MenuSeeker.Test.Properties;

using Newtonsoft.Json.Linq;


namespace LunchCrawler.MenuSeeker.Test.SearchEngines
{
    /// <summary>
    /// Search engine for lunch menus based on Yahoo's BOSS API.
    /// </summary>
    /// <see cref="http://developer.yahoo.com/search/boss/boss_guide/index.html"/>
    public class YahooSearchEngine : ISearchEngine
    {
        public string Name { get { return "Yahoo"; } }
        public ILogger Logger { get; set; }

        public YahooSearchEngine()
        {
            Logger = NullLogger.Instance;
        }

        private const string YahooAppID = "8sFuZtTV34HuPtIi4.L6.ieuHPMQ4JrXAQs4UjomycqXIYsbAaBaP8r4mGHUHWaruBYdag--";
        private readonly int _yahooSearchLimit = Settings.Default.YahooSearchLimit;
        private readonly int _yahooSearchCountForOneSearch = Settings.Default.YahooSearchCountForOneSearch;

        public IList<string> SearchForLinks(string query)
        {
            var allResults = new List<string>();

            var offset = 0;
            while (offset < _yahooSearchLimit)
            {
                var singleSeachResults = SingleYahooSearch(query, _yahooSearchCountForOneSearch, offset);
                allResults.AddRange(singleSeachResults ?? new List<string>());
                offset += _yahooSearchCountForOneSearch;
            }

            return allResults;
        }

        public IList<string> SingleYahooSearch(string query, int count, int offset = 0)
        {
            var queryUrl = "http://boss.yahooapis.com/ysearch/web/v1/{0}?appid={1}&count={2}&start={3}&lang=fi".With(query, YahooAppID, count, offset);
            var result = Utils.GetContentForURL(queryUrl);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            try
            {
                var resultJson = JObject.Parse(result);
                var resultSet = resultJson["ysearchresponse"]["resultset_web"];
                return resultSet.Select(r => (string)r["url"]).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
