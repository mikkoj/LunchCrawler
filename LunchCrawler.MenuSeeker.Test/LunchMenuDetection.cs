using System.Linq;
using System.Collections.Generic;

using HtmlAgilityPack;

using LunchCrawler.Common;
using LunchCrawler.Common.Enums;
using LunchCrawler.Data.Local;
using LunchCrawler.MenuSeeker.Test.Model;


namespace LunchCrawler.MenuSeeker.Test
{
    public class LunchMenuDetection
    {
        private readonly List<LunchMenuKeyword> _keywords;

        public LunchMenuDetection(List<LunchMenuKeyword> keywords)
        {
            _keywords = keywords;
        }

        /// <summary>
        /// Scores a single HtmlNode as lunch menu content.
        /// </summary>
        /// <param name="node">Node to be scored.</param>
        public LunchMenuScorePoint ScoreNode(HtmlNode node)
        {
            var scorePoint = new LunchMenuScorePoint
            {
                DetectionLocation = LunchMenuDetectionLocation.Unknown,
                DetectionType = StringMatchType.NoMatch
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

            // try exact match
            var exactMatch = _keywords.FirstOrDefault(keyword => keyword.Word.Equals(nodeText));
            if (exactMatch != null)
            {
                scorePoint.DetectionLocation = LunchMenuDetectionLocation.Content;
                scorePoint.DetectionType = StringMatchType.Exact;
                scorePoint.DetectedText = node.InnerText;
                scorePoint.DetectedKeyword = exactMatch.Word;
                scorePoint.PointsGiven = exactMatch.Weight;
                UpdateLunchMenuKeyword(exactMatch);

                // let's also remove the keyword not to match it again
                _keywords.Remove(exactMatch);

                return scorePoint;
            }

            // try partial match
            var partialMatch = _keywords.FirstOrDefault(keyword => nodeText.Contains(keyword.Word));
            if (partialMatch != null)
            {
                scorePoint.DetectionLocation = LunchMenuDetectionLocation.Content;
                scorePoint.DetectionType = StringMatchType.Partial;
                scorePoint.DetectedText = node.InnerText;
                scorePoint.DetectedKeyword = partialMatch.Word;
                scorePoint.PointsGiven = partialMatch.Weight;
                
                // let's also remove the keyword not to match it again
                _keywords.Remove(exactMatch);

                return scorePoint;
            }

            return scorePoint;
        }

        private static void UpdateLunchMenuKeyword(LunchMenuKeyword keyword)
        {
            // let's increase the weight if count..
            var existingWeight = keyword.Weight;
            var existingDetectionCount = keyword.DetectionCount;

            var newDetectionCount = keyword.DetectionCount + 1;
            LunchDA.Instance.UpdateLunchMenuKeywordDetectionCount(keyword.Word, newDetectionCount);
        }
    }
}
