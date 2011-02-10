using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using LunchCrawler.Common;
using LunchCrawler.Common.Enums;
using LunchCrawler.Common.Interfaces;
using LunchCrawler.Common.Logging;
using LunchCrawler.Common.Model;
using LunchCrawler.Data.Local;


namespace LunchCrawler.Analyzer.Test
{
    public class LunchRestaurantAnalyzer
    {
        public ILogger Logger { get; set; }

        public LunchRestaurantAnalyzer()
        {
            Logger = NullLogger.Instance;
        }

        private readonly IEnumerable<ILunchMenuParsingStrategy> _parsingStrategies;

        public LunchRestaurantAnalyzer(IEnumerable<ILunchMenuParsingStrategy> strategies)
        {
            _parsingStrategies = strategies;
        }

        /// <summary>
        /// Starts the analysis process in a separate thread.
        /// Aborts the mission after specified timeout.
        /// </summary>
        public void Start()
        {
            // timeout for thread to abort
            const double timeoutMinutes = 60;

            // maximum stack size for thread (10MB)
            const int maxStackSize = 10485760;

            try
            {
                var task = new ThreadStart(AnalyzeLunchRestaurants);
                var thread = new Thread(task, maxStackSize);
                thread.Start();
                if (!thread.Join(TimeSpan.FromMinutes(timeoutMinutes)))
                {
                    Thread.Sleep(2000);
                    thread.Abort();
                    Logger.Fatal("Analyze process timed out after {0} minutes.", timeoutMinutes);
                }
            }
            catch (OutOfMemoryException exOutOfMemory)
            {
                Logger.Fatal("Analyze process out of memory.", exOutOfMemory);
            }
            catch (Exception ex)
            {
                Logger.Fatal("Analyze process error: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Starts seeking for new lunch restaurants based on search keywords and ones added manually.
        /// Scores all found URLs.
        /// </summary>
        public void AnalyzeLunchRestaurants()
        {
            if (_parsingStrategies == null || _parsingStrategies.IsEmpty())
            {
                Logger.Fatal("No parsing strategies initialized!");
                return;
            }

            var restaurantsToBeAnalyzed = LunchDA.Instance.GetLunchRestaurantsWithStatus(LunchMenuStatus.OK)
                                                          .Select(rt => rt.AbsoluteURL);

            Logger.InfoFormat("Started analyzing total of {0} restaurants..", restaurantsToBeAnalyzed.Count());
            Parallel.ForEach(restaurantsToBeAnalyzed, AnalyzeLunchRestaurant);
        }

        public void AnalyzeLunchRestaurant(string url)
        {
            var doc = Utils.GetLunchRestaurantDocumentForUrl(url);
            if (ShouldSkipAnalysis(doc))
            {
                return;
            }

            // let's analyze based on parsing strategies' priority
            var strategies = _parsingStrategies.OrderByDescending(st => st.Priority);
            LunchMenu parseResult = null;
            foreach (var strategy in strategies)
            {
                parseResult = strategy.ParseDocument(doc.HtmlDocument);
                if (parseResult.Confidence > 0.90M)
                {
                    break;
                }
            }

            CompleteLunchRestaurantAnalysis(parseResult);
        }

        private static void CompleteLunchRestaurantAnalysis(LunchMenu parseResult)
        {
            // DB update + print result
        }

        private static bool ShouldSkipAnalysis(LunchRestaurantDocument doc)
        {
            var existingDoc = LunchDA.Instance.FindExistingLunchRestaurant(doc.URL);
            return existingDoc != null && existingDoc.SiteHash.Equals(doc.Hash);
        }
    }
}
