using System.Collections.Generic;
using System.Linq;

using LunchCrawler.Common.Enums;
using LunchCrawler.Data.Local;


namespace LunchCrawler.Common.Model
{
    /// <summary>
    /// Scores for an analysis of a potential lunch restaurant or its deep link.
    /// </summary>
    public class LunchMenuScores
    {
        private decimal? _lunchmenuProbability;

        public LunchMenuScores()
        {
            Points = new List<LunchMenuScorePoint>();
        }

        public IList<LunchMenuScorePoint> Points { get; set; }

        /// <summary>
        /// Probability that the analyzed URL is a lunch menu.
        /// </summary>
        public decimal LunchMenuProbability
        {
            get
            {
                if (!_lunchmenuProbability.HasValue)
                {
                    var totalPoints = Points.Sum(point => point.PointsGiven);

                    // bonus points if all weekdays are found
                    var weekDays = new[] { "maanantai", "tiistai", "keskiviikko", "torstai", "perjantai" };
                    if (weekDays.All(day => Points.Any(point => point.DetectedKeyword.ToLowerInvariant().Equals(day))))
                    {
                        Points.Add(new LunchMenuScorePoint { PointsGiven = 20 });
                    }

                    // TODO: more logic for probability
                    _lunchmenuProbability = totalPoints / 100.0M;
                }

                return _lunchmenuProbability.Value;
            }
        }


        /// <summary>
        /// Probability that the analyzed URL is a deep link menu.
        /// TODO: add better logic for probability
        /// </summary>
        public decimal DeepLinkProbability
        {
            get
            {
                // there's usually only one point at this point in Points collection
                // let's find out the type and do some naive calculations
                if (DeepLinkScorePoint != null &&
                    DeepLinkScorePoint.DeepLinkContentType.In(DeepLinkContentType.Monday,
                                                      DeepLinkContentType.Tuesday,
                                                      DeepLinkContentType.Wednesday,
                                                      DeepLinkContentType.Thursday,
                                                      DeepLinkContentType.Friday))
                {
                    // deep links shouldn't have too strict of a probability,
                    // since there's usually items for one day only
                    var totalPoints = Points.Sum(point => point.PointsGiven);
                    return totalPoints / 50M;
                }

                if (DeepLinkScorePoint != null &&
                    DeepLinkScorePoint.DeepLinkContentType.In(DeepLinkContentType.LunchMenu))
                {
                    var totalPoints = Points.Sum(point => point.PointsGiven);

                    // bonus points if all weekdays are found
                    var weekDays = new[] { "maanantai", "tiistai", "keskiviikko", "torstai", "perjantai" };
                    if (weekDays.All(day => Points.Any(point => point.DetectedKeyword.ToLowerInvariant().Equals(day))))
                    {
                        Points.Add(new LunchMenuScorePoint { PointsGiven = 20 });
                    }

                    // bonus points for deep links
                    if (DeepLinks != null)
                    {
                        if (DeepLinks.ToList().Exists(dl => dl.ContentType == (int)DeepLinkContentType.LunchMenu))
                        {
                            Points.Add(new LunchMenuScorePoint { PointsGiven = 80 });
                        }
                        else if (DeepLinks.Any())
                        {
                            Points.Add(new LunchMenuScorePoint { PointsGiven = 30 });
                        }
                    }

                    return totalPoints / 100.0M;
                }

                return 0;
            }
        }
        
        /// <summary>
        /// Deep links for restaurant.
        /// </summary>
        public IList<RestaurantDeepLink> DeepLinks { get; set; }

        /// <summary>
        /// Score point for a deep link.
        /// </summary>
        public LunchMenuScorePoint DeepLinkScorePoint { get; set; }
    }
}

