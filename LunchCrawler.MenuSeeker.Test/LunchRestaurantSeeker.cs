using System;
using System.Data;
using System.Text;
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
using LunchCrawler.MenuSeeker.Test.Properties;


namespace LunchCrawler.MenuSeeker.Test
{
    public class LunchRestaurantSeeker : ILunchRestaurantSeeker
    {
        public ILogger Logger { get; set; }

        public LunchRestaurantSeeker()
        {
            Logger = NullLogger.Instance;
        }

        private readonly ILunchRestaurantSearchEngine _searchEngine;

        public LunchRestaurantSeeker(ILunchRestaurantSearchEngine searchEngine)
        {
            Logger = NullLogger.Instance;
            _searchEngine = searchEngine;
        }

        private static readonly IEnumerable<LunchMenuKeyword> BasicLunchMenuKeywords = LunchDA.Instance.GetAllBasicLunchMenuKeywords();
        private static readonly IList<string> SearchKeywords = CreateSearchQueries(LunchDA.Instance.GetAllSearchKeywords());


        /// <summary>
        /// Creates the search query combinations based on category levels.
        /// </summary>
        /// <param name="keywords">List of all search keywords</param>
        private static IList<string> CreateSearchQueries(IEnumerable<SearchKeyword> keywords)
        {
            var partition = keywords.GroupBy(keyword => keyword.Category).OrderBy(group => group.Key);
            var accu = new List<string>();
            var merged = partition.Scan(accu,
                                        (cur, group) =>
                                        cur.MergeContents(group.Select(x => x.QueryKeyword),
                                                          (str1, str2) => str1 + " " + str2).ToList());
            
            return merged.Skip(partition.Count() > 1 ? 1 : 0)
                         .SelectMany(x => x)
                         .ToList();
        }


        /// <summary>
        /// Starts the seeking process in a separate thread.
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
                var task = new ThreadStart(SeekLunchRestaurants);
                var thread = new Thread(task, maxStackSize);
                thread.Start();
                if (!thread.Join(TimeSpan.FromMinutes(timeoutMinutes)))
                {
                    Thread.Sleep(2000);
                    thread.Abort();
                    Logger.Fatal("Seek process timed out after {0} minutes.", timeoutMinutes);
                }
            }
            catch (OutOfMemoryException exOutOfMemory)
            {
                Logger.Fatal("Seek process out of memory.", exOutOfMemory);
            }
            catch (Exception ex)
            {
                Logger.Fatal("Seek process error: " + ex.Message, ex);
            }
        }


        /// <summary>
        /// Starts seeking for new lunch restaurants based on search keywords and ones added manually.
        /// Scores all found URLs.
        /// </summary>
        public void SeekLunchRestaurants()
        {
            if (_searchEngine == null)
            {
                Logger.Fatal("Search engine not initialized!");
                return;
            }

            Logger.Info("Searching for links through search engines..");
            var searchedUrls = _searchEngine.SearchForLunchMenuURLs(SearchKeywords);
            var manuallyAddedUrls = LunchDA.Instance.GetLunchRestaurantsWithStatus(LunchMenuStatus.ManuallyAdded);

            var allUrls = searchedUrls.Union(manuallyAddedUrls.Select(m => m.AbsoluteURL))
                                      .Distinct(new UrlComparer());


            Logger.InfoFormat("Started scoring total of {0} links..", allUrls.Count());
            Parallel.ForEach(allUrls, ScoreLunchRestaurant);


            // let's also print the detection counts
            var lunchMenuKeywords = LunchDA.Instance.GetAllBasicLunchMenuKeywords();
            foreach (var keyword in lunchMenuKeywords.OrderByDescending(keyword => keyword.DetectionCount))
            {
                Logger.InfoFormat("- keyword: {0}\tdetection count: {1}", keyword.Word, keyword.DetectionCount);
            }
        }


        /// <summary>
        /// Scores a single URL as a lunch restaurant.
        /// </summary>
        public void ScoreLunchRestaurant(string url)
        {
            var potentialRestaurant = new LunchRestaurant
            {
                URL = Utils.GetBaseUrl(url), // primary key
                AbsoluteURL = url,           // used for creating and parsing the model
                Status = (int)LunchMenuStatus.OK
            };

            try
            {
                // Check if we already have this one
                var existingMenu = LunchDA.Instance.FindExistingLunchRestaurant(potentialRestaurant.URL);

                if (existingMenu == null || existingMenu.Status == (int)LunchMenuStatus.CannotConnect)
                {
                    var lunchMenuDocument = Utils.GetLunchRestaurantDocumentForUrl(url, Settings.Default.HTTPTimeoutSeconds);
                    if (lunchMenuDocument == null)
                    {
                        // no special error handling for now, any HTTP error -> can't connect
                        potentialRestaurant.Status = (int)LunchMenuStatus.CannotConnect;
                        LogLunchMenuScores(url, LunchMenuStatus.CannotConnect, new LunchMenuScores());
                        LunchDA.Instance.UpdateLunchRestaurant(potentialRestaurant);
                        return;
                    }

                    // let's calculate and log scores
                    var scores = GetScoresForHtmlDocument(lunchMenuDocument);
                    LogLunchMenuScores(url, (LunchMenuStatus)potentialRestaurant.Status, scores);

                    // ..and let's finish the potential restaurant instance and update the DB
                    CompletePotentialLunchRestaurant(lunchMenuDocument, potentialRestaurant, scores);
                    LunchDA.Instance.UpdateLunchRestaurant(potentialRestaurant);
                }
            }
            catch (EntityException entityEx)
            {
                var errorMessage = entityEx.ParseInnerError();
                if (errorMessage.ToLowerInvariant().Contains("database is locked"))
                {
                    Logger.Fatal("SQLite database is locked.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("Error scoring document for URL: {0} - {1}".With(url, ex.Message), ex);
            }
        }

        
        /// <summary>
        /// Calculates lunch menu scores for a Html-document.
        /// </summary>
        private static LunchMenuScores GetScoresForHtmlDocument(LunchRestaurantDocument lunchMenuDocument)
        {
            // let's create a new detection based on the basic lunch menu keywords
            var lunchMenuDetection = new LunchMenuDetection(BasicLunchMenuKeywords.ToList());

            // let's calculate and collect individual node-points for the document
            var scorePoints = lunchMenuDocument.HtmlDocument
                                               .DocumentNode
                                               .DescendantNodes()
                                               .Where(node => !Utils.ShouldSkipNode(node))
                                               .Select(lunchMenuDetection.ScoreNode)
                                               .Where(scored => scored.DetectionLocation != LunchMenuDetectionLocation.Unknown)
                                               .ToList();

            // let's update the detection count for found lunch menu keywords
            lunchMenuDetection.UpdateLunchMenuKeywordCountsDB();

            // let's wrap and print scores
            return new LunchMenuScores
            {
                Points = scorePoints
            };
        }


        /// <summary>
        /// Completes a potential lunch restaurant instance by calculating points and probability for it.
        /// </summary>
        private static void CompletePotentialLunchRestaurant(LunchRestaurantDocument lunchMenuDocument,
                                                             LunchRestaurant potentialRestaurant,
                                                             LunchMenuScores scores)
        {
            potentialRestaurant.SiteHash = lunchMenuDocument.Hash;
            potentialRestaurant.TotalPoints = scores.Points.Sum(p => p.PointsGiven);
            potentialRestaurant.LunchMenuProbability = scores.LunchMenuProbability;

            potentialRestaurant.TotalKeywordDetections   = scores.Points.Count(p => p.DetectionType != StringMatchType.NoMatch);
            potentialRestaurant.ExactKeywordDetections   = scores.Points.Count(p => p.DetectionType == StringMatchType.Exact);
            potentialRestaurant.PartialKeywordDetections = scores.Points.Count(p => p.DetectionType == StringMatchType.Partial);
            potentialRestaurant.FuzzyKeywordDetections   = scores.Points.Count(p => p.DetectionType == StringMatchType.Fuzzy);

            if (scores.LunchMenuProbability < Settings.Default.LunchMenuProbabilityLimit)
            {
                potentialRestaurant.Status = (int)LunchMenuStatus.ShouldSkip;
            }
        }


        public void LogLunchMenuScores(string url, LunchMenuStatus status, LunchMenuScores scores)
        {
            Console.OutputEncoding = Encoding.Default;

            var scoreBuilder = new StringBuilder();

            scoreBuilder.AppendFormat("scores for URL: {0}\n", url);
            scoreBuilder.AppendFormat("- status: {0} - total points: {1} - lunch menu probability: {2:P}\n",
                                      status,
                                      scores.Points.Sum(p => p.PointsGiven),
                                      scores.LunchMenuProbability);

            //foreach (var scorePoint in scores.Points.OrderByDescending(p => p.PointsGiven))
            //{
            //    var consoledata = Utils.CleanContentForConsole(scorePoint.DetectedText);
            //    scoreBuilder.AppendFormat("{0,2:00}: {1}\t -> {2}\n",
            //                              scorePoint.PointsGiven,
            //                              scorePoint.DetectedKeyword,
            //                              consoledata);
            //}

            scoreBuilder.AppendLine("\r\n-----------------------------------------------------------------------------");
            var lunchMenuScores = scoreBuilder.ToString();
            Logger.Info(lunchMenuScores);
        }
    }
}
