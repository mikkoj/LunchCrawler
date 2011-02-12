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

        /// <summary>
        /// List of parsing strategies injected by Dependency Injection.
        /// </summary>
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

            var restaurantsToBeAnalyzed = LunchDA.Instance.GetLunchRestaurants((int)LunchMenuStatus.OK, 0.50M);
            Logger.InfoFormat("Started analyzing total of {0} restaurants..\n", restaurantsToBeAnalyzed.Count());
            Parallel.ForEach(restaurantsToBeAnalyzed, AnalyzeLunchRestaurantWithTimeout);
        }


        /// <summary>
        /// Starts a analyzing task for a given URL.
        /// The task will be cancelled if it times out.
        /// </summary>
        private void AnalyzeLunchRestaurantWithTimeout(LunchRestaurant restaurant)
        {
            // let's analyze based on parsing strategies' priority
            var strategies = _parsingStrategies.OrderByDescending(st => st.Priority);

            // we'll cancel the process after timeout in case parsing goes wrong
            // TODO: timeout settings in DB
            const int timeoutForProcessing = 240000;
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            new Thread(() =>
            {
                Thread.Sleep(timeoutForProcessing);
                cts.Cancel();
            }).Start();

            try
            {
                Task.Factory.StartNew(() =>
                {
                    LunchMenu parseResult = null;

                    foreach (var strategy in strategies)
                    {
                        parseResult = strategy.ParseLunchMenu(restaurant);

                        // TODO: confidence settings in DB
                        if (parseResult.Confidence > 0.90M)
                        {
                            break;
                        }
                    }

                    CompleteLunchRestaurantAnalysis(parseResult);
                }).Wait(token);
            }
            catch (OperationCanceledException)
            {
                Logger.ErrorFormat("Analysis timed out for URL: {0}", restaurant.AbsoluteURL);
            }

            Console.WriteLine("---------------------------------------------------------------------------------\n");
        }


        private void CompleteLunchRestaurantAnalysis(LunchMenu parseResult)
        {
            if (parseResult.FoodItems.IsEmpty())
            {
                Logger.InfoFormat("-> {0} - no results.\n", parseResult.RestaurantKey);
            }

            // DB update + print result
        }
    }
}
