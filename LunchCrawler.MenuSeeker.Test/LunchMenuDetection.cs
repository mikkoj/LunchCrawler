using System;
using System.Linq;
using System.Collections.Generic;

using HtmlAgilityPack;

using LunchCrawler.Common;
using LunchCrawler.Common.Enums;
using LunchCrawler.Common.Interfaces;
using LunchCrawler.Common.Logging;
using LunchCrawler.Common.Model;
using LunchCrawler.Data.Local;
using LunchCrawler.MenuSeeker.Test.Properties;


namespace LunchCrawler.MenuSeeker.Test
{
    public class LunchMenuDetection : ILunchMenuDetection
    {
        public ILogger Logger { get; set; }

        private readonly IList<LunchMenuKeyword> _lunchMenuKeywords;
        private Dictionary<string, int> _lunchMenuKeywordCounts;

        private readonly IList<DeepLinkKeyword> _deepLinkKeywords;
        private Dictionary<string, int> _deepLinkKeywordCounts;

        private static readonly IEnumerable<LunchMenuKeyword> BasicLunchMenuKeywords = LunchDA.Instance.GetAllBasicLunchMenuKeywords();
        private static readonly IEnumerable<DeepLinkKeyword> DeepLinkKeywords = LunchDA.Instance.GetAllDeepLinkKeywords();


        public LunchMenuDetection()
        {
            Logger = NullLogger.Instance;

            _lunchMenuKeywords = BasicLunchMenuKeywords.ToList().DeepClone();
            _lunchMenuKeywordCounts = new Dictionary<string, int>();

            _deepLinkKeywords = DeepLinkKeywords.ToList().DeepClone();
            _deepLinkKeywordCounts = new Dictionary<string, int>();
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
            var exactMatch = _lunchMenuKeywords.FirstOrDefault(keyword => keyword.Word.Equals(nodeText));
            if (exactMatch != null)
            {
                scorePoint.DetectionLocation = LunchMenuDetectionLocation.Content;
                scorePoint.DetectionType = StringMatchType.Exact;
                scorePoint.DetectedText = node.InnerText;
                scorePoint.DetectedKeyword = exactMatch.Word;
                scorePoint.PointsGiven = exactMatch.Weight;
                UpdateLunchMenuKeyword(exactMatch);

                // let's also remove the keyword not to match it again
                _lunchMenuKeywords.ToList().RemoveAll(keyword => keyword.Word == exactMatch.Word);

                return scorePoint;
            }

            // try partial match
            var partialMatch = _lunchMenuKeywords.FirstOrDefault(keyword => nodeText.Contains(keyword.Word));
            if (partialMatch != null)
            {
                scorePoint.DetectionLocation = LunchMenuDetectionLocation.Content;
                scorePoint.DetectionType = StringMatchType.Partial;
                scorePoint.DetectedText = node.InnerText;
                scorePoint.DetectedKeyword = partialMatch.Word;
                scorePoint.PointsGiven = partialMatch.Weight;
                UpdateLunchMenuKeyword(partialMatch);
                
                // let's also remove the keyword not to match it again
                _lunchMenuKeywords.ToList().RemoveAll(keyword => keyword.Word == partialMatch.Word);

                return scorePoint;
            }

            return scorePoint;
        }


        private void UpdateLunchMenuKeyword(LunchMenuKeyword keyword)
        {
            if (_lunchMenuKeywordCounts.ContainsKey(keyword.Word))
            {
                _lunchMenuKeywordCounts[keyword.Word]++;
            }
            else
            {
                _lunchMenuKeywordCounts.Add(keyword.Word, 1);
            }
        }


        private void UpdateDeepLinkKeyword(DeepLinkKeyword keyword)
        {
            if (_deepLinkKeywordCounts.ContainsKey(keyword.Word))
            {
                _deepLinkKeywordCounts[keyword.Word]++;
            }
            else
            {
                _deepLinkKeywordCounts.Add(keyword.Word, 1);
            }
        }

        /// <summary>
        /// Updates the detection count for lunch menu keywords inside the DB.
        /// </summary>
        public void UpdateLunchMenuKeywordCountsDB()
        {
            LunchDA.Instance.UpdateLunchMenuKeywordDetectionCounts(_lunchMenuKeywordCounts);
        }

        /// <summary>
        /// Updates the detection count for deep link keywords inside the DB.
        /// </summary>
        public void UpdateDeepLinkKeywordCountsDB()
        {
            LunchDA.Instance.UpdateDeepLinkKeywordDetectionCounts(_deepLinkKeywordCounts);
        }

        /// <summary>
        /// Calculates scorepoints for a given document.
        /// </summary>
        /// <param name="lunchMenuDocument">Document to be scored.</param>
        public IList<LunchMenuScorePoint> GetScorePointsForDocument(LunchRestaurantDocument lunchMenuDocument)
        {
            return lunchMenuDocument.HtmlDocument
                                    .DocumentNode
                                    .DescendantNodes()
                                    .Where(node => !Utils.ShouldSkipNode(node))
                                    .Select(ScoreNode)
                                    .Where(scored => scored.DetectionLocation != LunchMenuDetectionLocation.Unknown)
                                    .ToList();
        }


        /// <summary>
        /// Seeks deep-links recursively.
        /// </summary>
        /// <param name="pageDocument">HTML document to be checked.</param>
        /// <param name="validDeepLinks">Deep links that scored high enough.</param>
        public void FindDeepLinks(LunchRestaurantDocument pageDocument, IList<RestaurantDeepLink> validDeepLinks)
        {
            FindDeepLinks(pageDocument, validDeepLinks, new List<string>(), 1);
        }


        /// <summary>
        /// Seeks deep-links recursively.
        /// </summary>
        /// <param name="pageDocument">HTML document to be checked.</param>
        /// <param name="validDeepLinks">Deep links that scored high enough.</param>
        /// <param name="checkedDeepLinks">Deep links that were already crawled. used to prevent loops.</param>
        /// <param name="level">Level of recursion.</param>
        public void FindDeepLinks(LunchRestaurantDocument pageDocument,
                                  IList<RestaurantDeepLink> validDeepLinks,
                                  IList<string> checkedDeepLinks,
                                  int level)
        {
            var maxLevel = Settings.Default.DeepLinkRecursionLevel;
            if (level >= maxLevel)
            {
                return;
            }

            // let's first collect all valid links for the document
            var links = pageDocument.HtmlDocument
                                    .DocumentNode
                                    .DescendantNodes()
                                    .Where(Utils.IsLink)
                                    .Select(node => node.Attributes["href"].Value)
                                    .ToList();

            foreach (var link in links)
            {
                // first, let's construct a full deep link based on the base url
                var fullDeepLinkUrl = link.StartsWith("http") ? link : ConstructDeepLinkUrl(pageDocument.URL, link);

                // let's score the link itself
                var scoreForLink = ScoreDeepLink(fullDeepLinkUrl);

                // we'll only continue, if this link hasn't been checked yet and it has a high deeplink score
                if (!IsValidDeepLink(fullDeepLinkUrl, scoreForLink, checkedDeepLinks))
                {
                    continue;
                }

                checkedDeepLinks.Add(fullDeepLinkUrl);

                Logger.Info("analyzing a deep link: " + fullDeepLinkUrl);

                // let's score the document that the link points to
                var deepLinkDocument = Utils.GetLunchRestaurantDocumentForUrl(fullDeepLinkUrl, Settings.Default.HTTPTimeoutSeconds);
                if (deepLinkDocument == null)
                {
                    continue;
                }
                var pointsForDeepLinkDocument = GetScorePointsForDocument(deepLinkDocument);
                var deepLinkScores = new LunchMenuScores
                {
                    Points = pointsForDeepLinkDocument,
                    DeepLinkScorePoint = scoreForLink
                };

                // if this link gets a high score, we'll add it as a deep link
                // (deep links have a separate probability since they can be a lot different than full menus)
                if (deepLinkScores.DeepLinkProbability > Settings.Default.LunchMenuProbabilityLimit)
                {
                    validDeepLinks.Add(new RestaurantDeepLink
                    {
                        ContentType = (int)scoreForLink.DeepLinkContentType,
                        DeepLinkURL = link,
                    });
                }
                // ..otherwise we'll go deeper
                else
                {
                    FindDeepLinks(deepLinkDocument, validDeepLinks, checkedDeepLinks, level++);
                }
            }
        }


        /// <summary>
        /// Constructs a deep link URL based on the restaurant url and deep link url.
        /// </summary>
        private static string ConstructDeepLinkUrl(string restaurantLink, string deepLink)
        {
            return Utils.GetBaseUrl(restaurantLink) + deepLink;
        }


        private static bool IsValidDeepLink(string link, LunchMenuScorePoint scoreForLink, ICollection<string> checkedDeepLinks)
        {
            return scoreForLink != null &&
                   scoreForLink.PointsGiven >= 5 &&
                   !checkedDeepLinks.Contains(link) &&
                   link.Length < 200;
        }


        /// <summary>
        /// Scores a single HtmlNode as a deep link.
        /// </summary>
        /// <param name="link">Node to be scored.</param>
        public LunchMenuScorePoint ScoreDeepLink(string link)
        {
            var scorePoint = new LunchMenuScorePoint
            {
                DetectionLocation = LunchMenuDetectionLocation.Unknown,
                DetectionType = StringMatchType.NoMatch
            };

            var linkText = Utils.HtmlDecode(link.ToLower()).Trim();
            if (string.IsNullOrEmpty(linkText))
            {
                return scorePoint;
            }

            // ----------------------------------------------------------------------------------------------

            // try exact match
            var exactMatch = _deepLinkKeywords.FirstOrDefault(keyword => keyword.Word.Equals(linkText));
            if (exactMatch != null)
            {
                scorePoint.DetectionLocation = LunchMenuDetectionLocation.UrlPath;
                scorePoint.DetectionType = StringMatchType.Exact;
                scorePoint.DeepLinkContentType = (DeepLinkContentType)Enum.ToObject(typeof(DeepLinkContentType), exactMatch.ContentType);
                scorePoint.DetectedText = link;
                scorePoint.DetectedKeyword = exactMatch.Word;
                scorePoint.PointsGiven = exactMatch.Weight;
                UpdateDeepLinkKeyword(exactMatch);

                // let's also remove the keyword not to match it again
                _deepLinkKeywords.ToList().RemoveAll(keyword => keyword.Word == exactMatch.Word);

                return scorePoint;
            }

            // try partial match
            var partialMatch = _deepLinkKeywords.FirstOrDefault(keyword => linkText.Contains(keyword.Word));
            if (partialMatch != null)
            {
                scorePoint.DetectionLocation = LunchMenuDetectionLocation.UrlPath;
                scorePoint.DetectionType = StringMatchType.Partial;
                scorePoint.DeepLinkContentType = (DeepLinkContentType)Enum.ToObject(typeof(DeepLinkContentType), partialMatch.ContentType);
                scorePoint.DetectedText = link;
                scorePoint.DetectedKeyword = partialMatch.Word;
                scorePoint.PointsGiven = partialMatch.Weight;
                UpdateDeepLinkKeyword(partialMatch);

                // let's also remove the keyword not to match it again
                _deepLinkKeywords.ToList().RemoveAll(keyword => keyword.Word == partialMatch.Word);

                return scorePoint;
            }

            return scorePoint;
        }

    }
}
