using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using HtmlAgilityPack;

using LunchCrawler.Common;
using LunchCrawler.Common.Enums;
using LunchCrawler.Data.Local;
using LunchCrawler.MenuSeeker.Test.Model;


namespace LunchCrawler.MenuSeeker.Test
{
    public static class LunchMenuSeeker
    {
        private static readonly IList<LunchMenuKeyword> BasicLunchMenuKeywords = LunchDA.Instance.GetAllBasicLunchMenuKeywords();

        public static void ScoreLunchMenu(string url)
        {
            Console.WriteLine("-> {0}\n", url);

            var potentialMenu = new PotentialLunchMenu
            {
                URL = url,
                Status = (int)LunchMenuStatus.OK,
            };

            var lunchMenuDocument = Utils.GetLunchMenuDocumentForUrl(url);
            if (lunchMenuDocument == null)
            {
                // no special error handling for now, any HTTP error -> can't connect
                potentialMenu.Status = (int)LunchMenuStatus.CannotConnect;
                PrintScores(LunchMenuStatus.CannotConnect, new LunchMenuScores());
                LunchDA.Instance.UpdateWithPotentialLunchMenu(url, potentialMenu);
                return;
            }

            var scores = GetScoresForHtmlDocument(lunchMenuDocument);
            PrintScores((LunchMenuStatus)potentialMenu.Status, scores);
            
            // ..and let's finish the potential menu object and update the DB
            CompletePotentialLunchMenu(lunchMenuDocument, potentialMenu, scores);
            LunchDA.Instance.UpdateWithPotentialLunchMenu(url, potentialMenu);
        }


        private static LunchMenuScores GetScoresForHtmlDocument(LunchMenuDocument lunchMenuDocument)
        {
            // let's create a new detection based on the basic lunch menu keywords
            var lunchMenuDetection = new LunchMenuDetection(BasicLunchMenuKeywords.ToList());

            // let's calculate and collect individual node-points for the document
            var scorePoints = lunchMenuDocument.HtmlDocument
                                               .DocumentNode
                                               .DescendantNodes()
                                               .Where(node => !ShouldSkipNode(node))
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


        private static bool ShouldSkipNode(HtmlNode node)
        {
            // only nodes without child-nodes, comments, scripts or root DOCUMENT-type
            return node.HasChildNodes ||
                   node.NodeType == HtmlNodeType.Comment ||
                   node.NodeType == HtmlNodeType.Document ||
                   (node.ParentNode != null && node.ParentNode.Name.ToLower().Contains("script"));
        }


        private static void PrintScores(LunchMenuStatus status, LunchMenuScores scores)
        {
            Console.OutputEncoding = Encoding.Default;

            Console.WriteLine("- status: {0} - total points: {1} - lunch menu probability: {2:P}",
                              status,              
                              scores.Points.Sum(p => p.PointsGiven),
                              scores.LunchMenuProbability);

            foreach (var scorePoint in scores.Points.OrderByDescending(p => p.PointsGiven))
            {
                var consoledata = Utils.CleanContentForConsole(scorePoint.DetectedText);
                Console.WriteLine("  {0,2:00}: {1}\t -> {2}",
                                  scorePoint.PointsGiven,
                                  scorePoint.DetectedKeyword,
                                  consoledata);
            }
        }
    }
}
