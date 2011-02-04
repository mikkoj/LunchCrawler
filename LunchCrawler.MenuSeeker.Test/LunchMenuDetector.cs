using System.Linq;
using System.Collections.Generic;

using HtmlAgilityPack;

using LunchCrawler.Common;
using LunchCrawler.Common.Enums;
using LunchCrawler.Data.Local;


namespace LunchCrawler.MenuSeeker.Test
{
    public static class LunchMenuDetector
    {
        private static readonly IList<LunchMenuKeyword> BasicLunchMenuKeywords = LunchDA.Instance.GetAllBasicLunchMenuKeywords();
        
        /// <summary>
        /// Scores a single HtmlNode as lunch menu content.
        /// </summary>
        /// <param name="node">Node to be scored.</param>
        public static LunchMenuScorePoint ScoreNode(HtmlNode node)
        {
            var scorePoint = new LunchMenuScorePoint
            {
                DetectionLocation = LunchMenuDetectionLocation.Unknown
            };

            if (node.InnerText == null)
            {
                return scorePoint;
            }

            var nodeText = Utils.HtmlDecode(node.InnerText.ToLower()).Trim();
            if (string.IsNullOrEmpty(nodeText))
            {
                return scorePoint;
            }

            // ----------------------------------------------------------------------------------------------

            var basicLunchMenuKeywords = BasicLunchMenuKeywords.ToList();

            // try exact match
            var exactMatch = basicLunchMenuKeywords.FirstOrDefault(keyword => keyword.Word.Equals(nodeText));
            if (exactMatch != null)
            {
                scorePoint.DetectionLocation = LunchMenuDetectionLocation.Content;
                scorePoint.DetectionType = StringMatchType.Exact;
                scorePoint.DetectedWord = node.InnerText;
                scorePoint.PointsGiven = exactMatch.Weight;
                UpdateLunchMenuKeyword(exactMatch);
                return scorePoint;
            }

            // try partial match
            var partialMatch = basicLunchMenuKeywords.FirstOrDefault(keyword => nodeText.Contains(keyword.Word));
            if (partialMatch != null)
            {
                scorePoint.DetectionLocation = LunchMenuDetectionLocation.Content;
                scorePoint.DetectionType = StringMatchType.Partial;
                scorePoint.DetectedWord = node.InnerText;
                scorePoint.PointsGiven = partialMatch.Weight;
                return scorePoint;
            }

            return scorePoint;
        }

        private static void UpdateLunchMenuKeyword(LunchMenuKeyword keyword)
        {
            // let's increase the weight if count
            var existingWeight = keyword.Weight;
            var existingDetectionCount = keyword.DetectionCount;

            var newDetectionCount = keyword.DetectionCount + 1;
            LunchDA.Instance.UpdateLunchMenuKeywordDetectionCount(keyword.Word, newDetectionCount);
        }
    }
}
