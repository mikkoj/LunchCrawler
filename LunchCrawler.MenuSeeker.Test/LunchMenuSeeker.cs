using System;
using System.Linq;
using System.Text;

using HtmlAgilityPack;

using LunchCrawler.Common;
using LunchCrawler.Common.Enums;


namespace LunchCrawler.MenuSeeker.Test
{
    public class LunchMenuSeeker
    {
        public void ScoreLunchMenu(string url)
        {
            Console.WriteLine("-> {0}\n", url);
            var htmlDoc = Utils.GetHtmlDocumentForUrl(url);

            // calculate points for the document
            var scorePoints = htmlDoc.DocumentNode.DescendantNodes()
                                     .Where(node => !ShouldSkipNode(node))
                                     .Select(LunchMenuDetector.ScoreNode)
                                     .Where(scored => scored.DetectionLocation != LunchMenuDetectionLocation.Unknown)
                                     .ToList();

            var scores = new LunchMenuScores
            {
                Points = scorePoints
            };

            PrintScores(scores);
        }

        private static bool ShouldSkipNode(HtmlNode node)
        {
            // only nodes without child-nodes, comments, scripts or root DOCUMENT-type
            return node.HasChildNodes ||
                   node.NodeType == HtmlNodeType.Comment ||
                   node.NodeType == HtmlNodeType.Document ||
                   (node.ParentNode != null && node.ParentNode.Name.ToLower().Contains("script"));
        }


        private static void PrintScores(LunchMenuScores scores)
        {
            Console.OutputEncoding = Encoding.Default;

            foreach (var scorePoint in scores.Points
                                             .OrderByDescending(p => p.PointsGiven))
            {
                var inputdata = Encoding.Unicode.GetBytes(Utils.HtmlDecode(scorePoint.DetectedWord));
                var consoledata = Console.OutputEncoding.GetString(Encoding.Convert(Encoding.Unicode, Console.OutputEncoding, inputdata));

                Console.WriteLine("{0}: {1}", scorePoint.PointsGiven, consoledata.Trim());
            }
        }
    }
}
