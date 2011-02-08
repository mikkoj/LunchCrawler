using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using LunchCrawler.Common;
using LunchCrawler.Common.Enums;
using LunchCrawler.Common.Interfaces;
using LunchCrawler.Common.Logging;
using LunchCrawler.Data.Local;
using LunchCrawler.MenuSeeker.Test.Model;


namespace LunchCrawler.MenuSeeker.Test
{
    [Export(typeof(ILunchMenuSeeker))]
    public class LunchMenuSeeker : ILunchMenuSeeker
    {
        public ILogger Logger { get; set; }

        public LunchMenuSeeker()
        {
            Logger = NullLogger.Instance;
        }

        private readonly ILunchMenuSearchEngine _searchEngine;

        /// <summary>
        /// MEF uses this constructor for composing.
        /// </summary>
        /// <param name="searchEngine"></param>
        [ImportingConstructor]
        public LunchMenuSeeker(ILunchMenuSearchEngine searchEngine)
        {
            Logger = NullLogger.Instance;
            _searchEngine = searchEngine;
        }

        private static readonly IList<LunchMenuKeyword> BasicLunchMenuKeywords = LunchDA.Instance.GetAllBasicLunchMenuKeywords();
        private static readonly IEnumerable<string> SearchKeywords = CreateSearchQueries(LunchDA.Instance.GetAllSearchKeywords());
        private static readonly IEnumerable<string> SearchKeywordsRecursive = CreateSearchQueriesRecursivePurkka(LunchDA.Instance.GetAllSearchKeywords());

        private static IEnumerable<string> CreateSearchQueries(IEnumerable<SearchKeyword> keywords)
        {
            var partition = keywords.GroupBy(keyword => keyword.Category).OrderBy(group => group.Key);
            var results = new List<string>(partition.FirstOrDefault().Select(word => word.QueryKeyword).ToList());
            var op = partition.Aggregate(results, (cur, group) => (group.Aggregate(results, (catwords, resword) => catwords.Select(word => word + " " + resword.QueryKeyword).ToList())));
            return op.AsEnumerable();
        }


        private static IEnumerable<string> CreateSearchQueriesRecursivePurkka(IEnumerable<SearchKeyword> keywords,
                                                                              List<string> permutations = null,
                                                                              int currentCategoryDepth = 1)
        {
            if (permutations == null)
            {
                permutations = new List<string>();
            }

            // let's calculate the depth
            var maxCategory = keywords.Max(keyword => keyword.Category);

            if (currentCategoryDepth < maxCategory)
            {
                // let's find the keywords for current category depth - or use recursive ones
                var keywordsForCategory = permutations.Count > 0 ?
                                              permutations :
                                              keywords.Where(keyword => keyword.Category == currentCategoryDepth)
                                                      .Select(k => k.QueryKeyword);

                // let's get the keyword combos for this category depth
                var keywordCombosForCategory = (from currentWord in keywordsForCategory
                                                from nextWord in keywords.Where(keyword => keyword.Category == (currentCategoryDepth + 1))
                                                select string.Format("{0} {1}", currentWord, nextWord.QueryKeyword)).ToList();

                // add rest recursively
                permutations.AddRange(CreateSearchQueriesRecursivePurkka(keywords,
                                                                         keywordCombosForCategory,
                                                                         currentCategoryDepth + 1));
            }

            return permutations;
        }


        public void SeekLunchMenus()
        {
            if (_searchEngine == null)
            {
                Logger.Fatal("Search engine not initialized!");
                return;
            }
            
            // TODO: search queries from a table
            var queries = new[] { "lounaslista turku", "lounaslista helsinki" };
            var lunchMenuUrls = _searchEngine.SearchForLunchMenuURLs(queries);
            //lunchMenuUrls.ForEach(ScoreLunchMenu);
            Parallel.ForEach(lunchMenuUrls, ScoreLunchMenu);
        }

        public void ScoreLunchMenu(string url)
        {
            var potentialMenu = new PotentialLunchMenu
            {
                URL = Utils.GetBaseUrl(url),
                Status = (int)LunchMenuStatus.OK,
            };

            // Check if we already have this one
            var existingMenu = LunchDA.Instance.FindPotentialLunchMenu(potentialMenu);

            if (existingMenu == null || existingMenu.Status == (int)LunchMenuStatus.CannotConnect)
            {
                var lunchMenuDocument = Utils.GetLunchMenuDocumentForUrl(url);
                if (lunchMenuDocument == null)
                {
                    // no special error handling for now, any HTTP error -> can't connect
                    potentialMenu.Status = (int)LunchMenuStatus.CannotConnect;
                    LogLunchMenuScores(url, LunchMenuStatus.CannotConnect, new LunchMenuScores());
                    LunchDA.Instance.UpdateWithPotentialLunchMenu(potentialMenu);
                    return;
                }

                var scores = GetScoresForHtmlDocument(lunchMenuDocument);
                LogLunchMenuScores(url, (LunchMenuStatus)potentialMenu.Status, scores);

                // ..and let's finish the potential menu object and update the DB
                CompletePotentialLunchMenu(lunchMenuDocument, potentialMenu, scores);
                LunchDA.Instance.UpdateWithPotentialLunchMenu(potentialMenu);
            }
        }


        private static LunchMenuScores GetScoresForHtmlDocument(LunchMenuDocument lunchMenuDocument)
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
            // let's wrap and print scores
            return new LunchMenuScores
            {
                Points = scorePoints
            };
        }


        private static void CompletePotentialLunchMenu(LunchMenuDocument lunchMenuDocument, PotentialLunchMenu potentialMenu, LunchMenuScores scores)
        {
            potentialMenu.SiteHash = lunchMenuDocument.Hash;
            potentialMenu.TotalPoints = scores.Points.Sum(p => p.PointsGiven);
            potentialMenu.LunchMenuProbability = scores.LunchMenuProbability;

            potentialMenu.TotalKeywordDetections = scores.Points.Count(p => p.DetectionType != StringMatchType.NoMatch);
            potentialMenu.ExactKeywordDetections = scores.Points.Count(p => p.DetectionType == StringMatchType.Exact);
            potentialMenu.PartialKeywordDetections = scores.Points.Count(p => p.DetectionType == StringMatchType.Partial);
            potentialMenu.FuzzyKeywordDetections = scores.Points.Count(p => p.DetectionType == StringMatchType.Fuzzy);
        }


        public void LogLunchMenuScores(string url, LunchMenuStatus status, LunchMenuScores scores)
        {
            Console.OutputEncoding = Encoding.Default;

            var scoreBuilder = new StringBuilder();

            scoreBuilder.AppendFormat("\n Scores for URL: {0}\n", url);
            scoreBuilder.AppendFormat("- status: {0} - total points: {1} - lunch menu probability: {2:P}\n",
                                         status,
                                         scores.Points.Sum(p => p.PointsGiven),
                                         scores.LunchMenuProbability);

            foreach (var scorePoint in scores.Points.OrderByDescending(p => p.PointsGiven))
            {
                var consoledata = Utils.CleanContentForConsole(scorePoint.DetectedText);
                scoreBuilder.AppendFormat("{0,2:00}: {1}\t -> {2}\n",
                                             scorePoint.PointsGiven,
                                             scorePoint.DetectedKeyword,
                                             consoledata);
            }

            var lunchMenuScores = scoreBuilder.ToString();
            Logger.Info(lunchMenuScores);

            scoreBuilder.AppendLine("\n\n--------------------------------------------------------------\n");
            Console.WriteLine(scoreBuilder.ToString());
        }
    }
}
