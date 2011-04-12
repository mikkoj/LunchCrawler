using System;
using System.Linq;
using System.Text;

using LunchCrawler.Common;
using LunchCrawler.Common.Enums;
using LunchCrawler.Common.Model;
using LunchCrawler.MenuSeeker.Test;
using LunchCrawler.Tests.Properties;

using NUnit.Framework;


namespace LunchCrawler.Tests
{
    [TestFixture]
    public class MenuDetectionTests
    {
        [Test]
        public void TestRandomRestaurantDeepLinks()
        {
            var testRestaurants = new[]
            {
                "http://www.spaghetteria.fi/",
                "http://ursula.fi/piritta/"
            };

            testRestaurants.ToList().ForEach(url =>
            {
                var document = Utils.GetLunchRestaurantDocumentForUrl(url, Settings.Default.HTTPTimeoutSeconds);
                var scores = LunchRestaurantSeeker.GetScoresForHtmlDocument(document);
                PrintLunchMenuScores(url, LunchRestaurantStatus.Unknown, scores);
            });
        }


        public void PrintLunchMenuScores(string url, LunchRestaurantStatus status, LunchMenuScores scores)
        {
            //Console.OutputEncoding = Encoding.Default;

            var scoreBuilder = new StringBuilder();

            scoreBuilder.AppendFormat("scores for URL: {0}\n", url);
            scoreBuilder.AppendFormat("- status: {0} - total points: {1} - deep links: {2} - lunch menu probability: {3:P}\n",
                                      status,
                                      scores.Points.Sum(p => p.PointsGiven),
                                      scores.DeepLinks != null ? scores.DeepLinks.Count : 0,
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
            Console.WriteLine(lunchMenuScores);
        }

    }
}
